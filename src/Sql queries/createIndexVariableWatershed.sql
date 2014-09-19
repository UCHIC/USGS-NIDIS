
DECLARE @origVariableID FLOAT = 32--12--6--
	DECLARE @origMethodID FLOAT = 8
	DECLARE @siteType NVARCHAR(50) = 'USGS-Streamflow'


SELECT
	CASE 
		WHEN DataValue IS NULL THEN NoDataValue
		WHEN QualifierID =21 THEN NoDataValue
		WHEN SiteType = 'USBR-Reservoir' THEN DataValue
		WHEN rowNum <> expectedRows THEN NoDataValue		
		ELSE DataValue
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
		WHEN rowNum <> expectedRows THEN 21 
		WHEN /*DataValue IS NULL AND*/ rowNum =0 THEN 34
		WHEN QualifierID IS NULL THEN NULL
		ELSE QualifierID		
	 END AS QualifierID,
	 MethodID, 
	 SourceID,
	 NULL AS SampleID,
	 NULL AS DerivedFromID, 
	 QualityControlLevelID,
	 expectedRows-rowNum AS MissingRows	
INTO #temp1 
FROM(
	SELECT 
		CASE
			WHEN w.SiteType = 'NRCS-SNOTEL' 
				THEN SUM(DV.DataValue * UH.HucArea)/12 --SUM(HUC DV * HUC Area)  convert to acre-feet
			WHEN w.SiteType = 'NCDC-Weather'
				THEN SUM(DV.DataValue*(UH.HucArea/totalArea)) --Weighted Average = 
			WHEN w.SiteType = 'USBR-Reservoir' -- sum of reservoirs
				THEN SUM(DV.DataValue)	
			WHEN w.SiteType = 'USGS-Streamflow'
				THEN SUM(DV.DataValue * UH.HucArea)/12--SUM(HUC dv * HUC Area) Convert to acre-feet	
			ELSE 
				SUM(DV.DataValue)		
		END AS DataValue, 
		LocalDateTime, 
		S.SiteID, 
		DV.DateTimeUTC, 
		DV.VariableID As OldVariableID, 
		w.SiteType, 
		totalArea, 
		w.VariableID, 
		w.MethodID, 
		w.QualityControlLevelID, 
		w.SourceID,
		MAX(DV.NoDataValue) AS NoDataValue,
		MAX(QualifierID) As QualifierID,
		COUNT(*) AS rowNum,
		MAX(U2.expectedRows)AS expectedRows,		
		Max(Sd.BeginDateTime) As SeriesStartDate


--DECLARE @origVariableID FLOAT = 32--12--6--
--	DECLARE @origMethodID FLOAT = 8
--	DECLARE @siteType NVARCHAR(50) = 'USGS-Streamflow'
--select * 
	FROM 
		UpstreamHucs AS UH --Summary
		JOIN 
			(SELECT 
				HucID, 
				Sum(UpstreamHucArea) AS totalArea, -- Sum of the area of all contributing hucs to a watershed
				Count(*) AS expectedRows -- number of hucs in watershed
			FROM UpstreamHucs
			GROUP BY HucID
			)AS U2
		ON UH.HucID = U2.HucID
		JOIN
			OPENDATASOURCE(--FromDB
				'SQLOLEDB',
				'Data Source=wasser.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
			).Summary.dbo.Sites AS upS 
		ON upS.SiteCode = UH.UpstreamHucID 
		JOIN
			OPENDATASOURCE(--FromDB
				'SQLOLEDB',
				'Data Source=wasser.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
			).Summary.dbo.Sites AS S
		ON S.SiteCode = UH.HucID 
		LEFT JOIN
			(SELECT 
				CASE	
					WHEN DataValue = v.NoDataValue THEN NULL
					ELSE DataValue
				END AS DataValue,
				LocalDateTime,UTCOffset, DateTimeUTC, CensorCode, SiteID, dv.VariableID, QualifierID, MethodID, SourceID, QualityControlLevelID, VariableCode, NoDataValue						
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
			WHERE DV.VariableID = @origVariableID AND DV.MethodID = @origMethodID AND CensorCode='nc' AND dv.QualityControlLevelID = 3 AND dv.localDateTime >= '2012-05-01'
		)AS DV --from db
		ON DV.SiteID = upS.SiteID 
		LEFT JOIN--Summary				
		WatershedSeries AS w
		ON DV.VariableID = w.L3VariableID AND DV.MethodID = w.L3MethodID
		LEFT JOIN 
			OPENDATASOURCE(--FromDB
				'SQLOLEDB',
				'Data Source=wasser.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
			).Summary.dbo.SeriesCatalog AS L3SC
		ON DV.SiteID = L3SC.SiteID AND w.L3VariableCode = L3SC.VariableCode AND w.L3MethodID = L3SC.MethodID AND L3SC.QualityControlLevelID=3			
		LEFT JOIN		
		(
			SELECT MAX(SC.BeginDateTime) AS BeginDateTime, UH.HucID FROM
			--select*			From
				UpstreamHucs AS UH
			LEFT JOIN
				OPENDATASOURCE(--FromDB
					'SQLOLEDB',
					'Data Source=wasser.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
				).Summary.dbo.SeriesCatalog AS SC
			ON UH.UpstreamHucID = SC.SiteCode 
			WHERE  SC.VariableID= @origVariableID AND SC.MethodID = @origMethodID
		 GROUP BY UH.HucID
		) AS Sd--series start date
		ON UH.HucID = Sd.HucID 
		
		LEFT JOIN	
			OPENDATASOURCE(--newdb
				'SQLOLEDB',
				'Data Source=wasser.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'--NewDB
			).Summary.dbo.SeriesCatalog AS SC --gets the series information to calculate new start date
		ON SC.VariableID = w.VariableID AND SC.SiteID= S.SiteID AND SC.MethodID = w.MethodID AND SC.QualityControlLevelID=3
		WHERE w.SiteType= @siteType --select only the sitetype 
		AND DV.LocalDateTime BETWEEN 
			CASE-- case statement checks to see if there is a valid enddate from the series catalog entry
				WHEN SC.EndDateTime IS NULL THEN '1900-01-01'-- if no enddate found set to default 
				-- if EndDateTime is Valid: 
				WHEN w.DataTimePeriod= 'Monthly' THEN CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, DATEADD(MONTH, 1, SC.EndDateTime))) + '/1/' + CONVERT(VARCHAR,DATEPART(YEAR, DATEADD(MONTH, 1, SC.EndDateTime))))
				WHEN DATEPART(DAY, SC.EndDateTime) = 1 THEN CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, SC.EndDateTime)) + '/16/' + CONVERT(VARCHAR,DATEPART(YEAR, SC.EndDateTime)))	--if it is the first half of the month set equal to 16th day of month
				WHEN DATEPART(DAY, SC.EndDateTime) = 16 THEN CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, DATEADD(MONTH, 1, SC.EndDateTime))) + '/1/' + CONVERT(VARCHAR,DATEPART(YEAR, DATEADD(MONTH, 1, SC.EndDateTime))))--if it is the second half of the month, set equal to first day of the next month
			END 
		AND   
			CASE-- Calculate the endDate by finding the last day of last month
				-- if EndDateTime is Valid: 
				WHEN w.DataTimePeriod= 'Monthly' OR DATEPART(DAY,GETDATE()) < 16  THEN DATEADD(DAY,-1,CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, GETDATE())) + '/1/' + CONVERT(VARCHAR,DATEPART(YEAR, GETDATE()))))	
				WHEN DATEPART(DAY,GETDATE()) > 16 THEN CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, GETDATE())) + '/15/' + CONVERT(VARCHAR,DATEPART(YEAR, GETDATE())))
			END 
		
			--AND DV.LocalDateTime  BETWEEN(--case statement checks to see if there is a valid enddate from the series catalog entry
			--	CASE		--instead of using 1900 what is the max start date of all of the sites involved in the equationj
			--		WHEN sc.EndDateTime IS NULL THEN '1900-01-01'--if no enddate found set to default  
			--		ELSE  DATEADD(MONTH, 1, sc.EndDateTime)-- if EndDateTime is Valid, add 1 month on for next start date
			--	END )
			--AND DATEADD(DAY,-1,CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, GETDATE())) + '/1/' + CONVERT(VARCHAR,DATEPART(YEAR, GETDATE()))))			
		GROUP BY UH.HucID, LocalDateTime, DateTimeUTC, S.SiteID, DV.VariableID, w.SiteType, U2.totalArea, w.VariableID, w.MethodID, w.QualityControlLevelID, w.SourceID
	)AS Data
	WHERE LocalDateTime>=SeriesStartDate




SELECT * FROM #temp1
--where DataValue IS NULL

--INSERT INTO
----Tester.dbo.DataValuesold 
--SummaryTest.dbo.DataValues
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
SELECT	 
	DataValue,
	 ValueAccuracy,		 
	 LocalDateTime,
     UTCOffset,
     DateTimeUTC,     
	 SiteID, 
	 VariableID,
	 OffsetValue,
	 OffsetTypeID,
	 CensorCode, 
	 QualifierID, 
	 MethodID, 
	 SourceID,
	 SampleID,
	 DerivedFromID, 
	 QualityControlLevelID
	 FROM #temp1

--DROP TABLE #temp1

  
  
  
  



