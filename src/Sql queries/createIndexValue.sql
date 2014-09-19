DECLARE @origVariableID FLOAT = 14
DECLARE @origMethodID FLOAT = 13
DECLARE @siteType NVARCHAR(50) = 'USBR-Reservoir'


SELECT 
	CASE 
		WHEN MaxDataValue IS NULL THEN NoDataValue
		WHEN NumOfValues <=5 THEN NoDataValue
		WHEN MaxDataValue = 0 THEN (Cast(LessThanCount+1 As Float)/2)/Cast(NumOfValues +1 AS Float)*100
		ELSE Cast(LessThanCount As Float)/Cast(NumOfValues +1 AS Float)*100
	END AS DataValue,
	 NULL AS ValueAccuracy,		 
	 LocalDateTime,
     DATEDIFF(HOUR, DateTimeUTC, LocalDateTime) AS UTCOffset,
     DateTimeUTC,     
	 SiteID, 
	 VariableID,
	 NULL AS OffsetValue,
	 NULL AS OffsetTypeID,
	 'nc' AS CensorCode, 
	 CASE 
		WHEN NumOfValues <= 5 THEN 33
		WHEN MaxDataValue IS NULL THEN 32
		ELSE  QualifierID
	END AS QualifierID, 
	 MethodID, 
	 SourceID,
	 NULL AS SampleID,
	 NULL AS DerivedFromID, 
	 QualityControlLevelID,	 
	 LessThanCount,
	 NumOfValues,
	 MaxDataValue
	 INTO #temp1
	 FROM(
		
		SELECT SiteID, --VariableID AS OldVariable, MethodID AS OldMethod, 
			MAX(NoDataValue) AS NoDataValue,
			MAX(NewMethodID) AS MethodID,
			MAX(NewVariableID) AS VariableID,
			MAX(NewSourceID) AS SourceID,
			MAX(NewQCLID) AS QualityControlLevelID,		
			MAX(DataValue)AS MaxDataValue, 
			MAX(LocalDateTime) AS LocalDateTime,
			MAX(DateTimeUTC) AS DateTimeUTC, 
			SUM(CASE WHEN HISTDataValue<=DataValue THEN 1 ELSE 0 END) AS LessThanCount, 
			SUM(CASE WHEN HISTDataValue IS NOT NULL THEN 1 ELSE 0 END) As NumOfValues,
			--CASE 
			--WHEN MAX(DataValue) IS NULL THEN MAX(NoDataValue)
			--WHEN SUM(CASE WHEN HISTDataValue IS NOT NULL THEN 1 ELSE 0 END) <5 THEN MAX(NoDataValue)
			--ELSE
			--(CAST(SUM(CASE WHEN HISTDataValue<=DataValue THEN 1 ELSE 0 END)AS FLOAT)/CAST(SUM(CASE WHEN HISTDataValue IS NOT NULL THEN 1 ELSE 0 END)+1 AS FLOAT)) *100 
			--END AS DataValue,	--AS NonExProb, 
			
		(SELECT Top 1 QualifierID FROM OPENDATASOURCE(
				'SQLOLEDB',--TooDB
				'Data Source=wasser.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
			).Summary.dbo.Qualifiers -- finds the qualifier id from the 'new' database associated with the NoDataValue Percentage 
			WHERE QualifierCode =Cast(10.0+(
				SELECT QualifierCode FROM OPENDATASOURCE(
					'SQLOLEDB',--TooDB
					'Data Source=wasser.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
				).Summary.dbo.Qualifiers AS Q
				Where Q.QualifierID = MAX(Data.QualifierID)
					)								
			 AS NVARCHAR(4))
		)	AS QualifierID
			
		FROM
			(				
			SELECT CASE	
					WHEN dv.DataValue = v.NoDataValue THEN NULL
					ELSE dv.DataValue
				END AS DataValue,
				dv.LocalDateTime,  
				dv.SiteID, 
				dv.VariableID, 
				dv.DateTimeUTC, 
				dv.QualifierID, 
				dv.MethodID,  
				dv.QualityControlLevelID,
				v.NoDataValue,						
				CASE	
					WHEN HISTdv.DataValue = v.NoDataValue THEN NULL
					ELSE HISTdv.DataValue
				END AS HISTDataValue, HISTdv.LocalDateTime AS HISTLocalDateTime,
				IV.MethodID AS NewMethodID,
				IV.VariableID AS NewVariableID,
				IV.SourceID AS NewSourceID,
				IV.QualityControlLevelID AS NewQCLID
				FROM
				OPENDATASOURCE(--FromDB
					'SQLOLEDB',
					'Data Source=wasser.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
				).Summary.dbo.DataValues AS dv
				LEFT JOIN
				OPENDATASOURCE(--FromDB
					'SQLOLEDB',
					'Data Source=wasser.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
				).Summary.dbo.Variables AS v
				ON dv.VariableID = v.VariableID			
				LEFT JOIN
				OPENDATASOURCE(--FromDB
					'SQLOLEDB',
					'Data Source=wasser.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
				).Summary.dbo.DataValues AS HISTdv
				ON DATEPART(MONTH, HISTdv.LocalDateTime)= DATEPART(MONTH, dv.LocalDateTime) AND DATEPART(DAY, HISTdv.LocalDateTime)= DATEPART(DAY, dv.LocalDateTime) AND DATEPART(YEAR, HISTdv.LocalDateTime)<= DATEPART(YEAR, dv.LocalDateTime)AND HISTdv.VariableID = dv.VariableID AND HISTdv.SiteID=dv.SiteID AND dv.MethodID = HISTdv.MethodID
				LEFT JOIN 
				IndexValuexRef AS IV
				ON 
				IV.L3VariableID = DV.VariableID AND IV.L3MethodID = DV.MethodID 
				LEFT JOIN					
				OPENDATASOURCE(
					'SQLOLEDB',--TooDB
					'Data Source=wasser.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
				).Summary.dbo.SeriesCatalog AS SC --gets the series information to calculate new start date
				ON SC.VariableID = IV.VariableID AND SC.SiteID= DV.SiteID AND SC.MethodID = IV.MethodID And SC.QualityControlLevelID= IV.QualityControlLevelID		
				WHERE DV.VariableID = @origVariableID AND DV.MethodID = @origMethodID AND DV.CensorCode='nc' AND DV.QualityControlLevelID = 3
				AND DV.LocalDateTime BETWEEN 
			CASE-- case statement checks to see if there is a valid enddate from the series catalog entry
				WHEN SC.EndDateTime IS NULL THEN '1900-01-01'-- if no enddate found set to default 
				-- if EndDateTime is Valid: 
				WHEN IV.DataTimePeriod= 'Monthly' THEN CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, DATEADD(MONTH, 1, SC.EndDateTime))) + '/1/' + CONVERT(VARCHAR,DATEPART(YEAR, DATEADD(MONTH, 1, SC.EndDateTime))))
				WHEN DATEPART(DAY, SC.EndDateTime) = 1 THEN CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, SC.EndDateTime)) + '/16/' + CONVERT(VARCHAR,DATEPART(YEAR, SC.EndDateTime)))	--if it is the first half of the month set equal to 16th day of month
				WHEN DATEPART(DAY, SC.EndDateTime) = 16 THEN CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, DATEADD(MONTH, 1, SC.EndDateTime))) + '/1/' + CONVERT(VARCHAR,DATEPART(YEAR, DATEADD(MONTH, 1, SC.EndDateTime))))--if it is the second half of the month, set equal to first day of the next month
			END 
		AND   
			CASE-- Calculate the endDate by finding the last day of last month
				-- if EndDateTime is Valid: 
				WHEN IV.DataTimePeriod= 'Monthly' OR DATEPART(DAY,GETDATE()) < 16  THEN DATEADD(DAY,-1,CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, GETDATE())) + '/1/' + CONVERT(VARCHAR,DATEPART(YEAR, GETDATE()))))	
				WHEN DATEPART(DAY,GETDATE()) > 16 THEN CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, GETDATE())) + '/15/' + CONVERT(VARCHAR,DATEPART(YEAR, GETDATE())))
			END 
				--AND dv.LocalDateTime  BETWEEN(
				--	--case statement checks to see if there is a valid enddate from the series catalog entry
				--	CASE				
				--		WHEN sc.EndDateTime IS NULL THEN '1900-01-01'--if no enddate found set to default  
				--		ELSE  DATEADD(MONTH, 1, sc.EndDateTime)-- if EndDateTime is Valid, add 1 month on for next start date
				--	END )
				--AND DATEADD(DAY,-1,CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, GETDATE())) + '/1/' + CONVERT(VARCHAR,DATEPART(YEAR, GETDATE()))))	

			) AS Data
			GROUP BY  LocalDateTime, VariableID, MethodID, SiteID
		) AS dv 

--INSERT INTO
----Summary.dbo.DataValues
--Tester.dbo.DataValues 
--	(DataValue,
--	 ValueAccuracy,		 
--	 LocalDateTime,
--     UTCOffset,
--     DateTimeUTC,     
--	 SiteID, 
--	 VariableID,
--	 OffsetValue,
--	 OffsetTypeID,
--	 CensorCode, 
--	 QualifierID, 
--	 MethodID, 
--	 SourceID,
--	 SampleID,
--	 DerivedFromID, 
--	 QualityControlLevelID)
--SELECT --*		
--	 DataValue,
--	 ValueAccuracy,		 
--	 LocalDateTime,
--     UTCOffset,
--     DateTimeUTC,     
--	 SiteID, 
--	 VariableID,
--	 OffsetValue,
--	 OffsetTypeID,
--	 CensorCode, 
--	 QualifierID, 
--	 MethodID, 
--	 SourceID,
--	 SampleID,
--	 DerivedFromID, 
--	 QualityControlLevelID
--FROM #temp1	

select * from #temp1
order by SiteiD, VariableID, LocalDateTime


--DROP TABLE #temp1