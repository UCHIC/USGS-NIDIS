 
DECLARE @origVariableID FLOAT = 2
DECLARE @siteType NVARCHAR(50) = 'NCDC-Weather'

--returns DataValues table
Select
--case uses the Aggregate method found below to decide which aggregate value to use in the datavalues table.
	CASE 
		WHEN T.noDVPerc=1 THEN T.NoDataValue
		WHEN T.AggregateMethod='Average' AND AvgDataValue IS NOT NULL THEN AvgDataValue
		WHEN T.AggregateMethod='Total' AND SumDataValue IS NOT NULL THEN SumDataValue
		WHEN T.AggregateMethod='Minimum' AND MinDataValue IS NOT NULL THEN MinDataValue 
		WHEN T.AggregateMethod='Maximum' AND MaxDataValue IS NOT NULL THEN MaxDataValue 
		WHEN T.AggregateMethod='EOI' AND EOIValue IS NOT NULL THEN EOIValue
		WHEN T.AggregateMethod='Delta' AND DeltaValue <> T.NoDataValue THEN NULL -- Fill in below, using seperate Query 
		ELSE T.NoDataValue
	 END AS DataValue,
	 NULL AS ValueAccuracy, 
	 T.LocalDateTime, 
	 T.UTCOffset AS UTCOffset, 
	 DATEADD(HOUR, -T.UTCOffset, T.LocalDateTime) AS DateTimeUTC, --calculate UTC Time
	 T.NewSiteID AS SiteID, 
	 T.NewVariableID AS VariableID, 
	 NULL AS OffsetValue, 
	 NULL AS OffsetTypeID, 
	 'nc' AS CensorCode, 
	 QualifierID,	
	 T.NewMethodID AS MethodID, 
	 T.NewSourceID AS SourceID,
	 NULL AS SampleID, 
	 NULL AS DerivedFromID, 
	 T.NewQCLID AS QualityControlLevelID, 
	 T.AggregateMethod,
	 T.NoDataValue AS NoDV	 
into #temp1	 
From(

	SELECT 
		--Case Statements to do Unit Conversions for discharge
		-- if streamflow. multiply value by number of seconds in interval, and divide by number of feet in an acre(43560)
		CASE 
			WHEN @siteType LIKE '%USGS%'
				THEN Data.AVGDataValue * ABS(DATEDIFF(SECOND, Data.LocalDateTime, DATEADD(MONTH, 1,Data.LocalDateTime)))/43560
			WHEN @siteType LIKE '%NCDC%'
				THEN Data.AVGDataValue *.0039
			ELSE Data.AVGDataValue
		END AS AVGDataValue, 
		CASE
			WHEN @siteType LIKE '%USGS%'
				THEN Data.SumDataValue *ABS(DATEDIFF(SECOND, Data.LocalDateTime, DATEADD(MONTH, 1,Data.LocalDateTime)))/43560
			WHEN @siteType LIKE '%NCDC%'
				THEN Data.SumDataValue *.0039
			ELSE Data.SumDataValue
		END AS SumDataValue, 
		CASE 
			WHEN @siteType LIKE '%USGS%'
				THEN Data.MinDataValue*ABS(DATEDIFF(SECOND, Data.LocalDateTime, DATEADD(MONTH, 1,Data.LocalDateTime)))/43560
			WHEN @siteType LIKE '%NCDC%'
				THEN Data.MinDataValue *.0039
			ELSE Data.MinDataValue
		END AS MinDataValue, 
		CASE  
			WHEN @siteType LIKE '%USGS%'
				THEN Data.MaxDataValue*ABS(DATEDIFF(SECOND, Data.LocalDateTime, DATEADD(MONTH, 1,Data.LocalDateTime)))/43560
			WHEN @siteType LIKE '%NCDC%'
				THEN Data.MaxDataValue *.0039
			ELSE Data.MaxDataValue
		END AS MaxDataValue, 
		CASE
			WHEN @siteType LIKE '%USGS%'
				THEN DV.DataValue*ABS(DATEDIFF(SECOND, Data.LocalDateTime, DATEADD(MONTH, 1,Data.LocalDateTime)))/43560
			WHEN @siteType LIKE '%NCDC%'
				THEN DV.DataValue *.0039
			ELSE DV.DataValue
		END AS EOIValue,
		Cast(NULL AS FLOAT) AS DeltaValue,-- placeholder for delta Values calculated later
		Data.EndDate, -- gets the last date with a Value 
		Data.LocalDateTime, 
		Data.SiteID, 
		Data.SiteCode, 
		Data.VariableID, 
		Data.VariableCode, 
		Data.NumOfValues, 
		Data.noDVPerc, 
		Data.UTCOffset, 
		Data.NoDataValue, 
		Data.EndDateTime,
		Data.AggregateMethod,
		--'New' indicates the id the new series will be saved with.
	    Data.NewVariableID,			
		Data.NewSiteID, 
		Data.NewMethodID, 
		Data.NewSourceID,
		Data.NewQCLID,	
		CASE 
			WHEN @siteType = 'NWS-SNODAS-HUC10' THEN 10+Q.QualifierID 
			WHEN Data.noDVPerc =1 THEN NULL -- If all values are missing
			WHEN Data.noDVPerc = 0 THEN NULL -- If no values are missing
			ELSE Q.QualifierID
		END AS QualifierID 
	FROM  
		OPENDATASOURCE(
		     'SQLOLEDB',
			 'Data Source=wasser.uwrl.usu.edu;Initial Catalog = SummaryTest;User ID=NIDIS;Password=N1d1s!'--FromDB
			).SummaryTest.dbo.DataValues AS DV 
		    RIGHT JOIN( 
				-- Calculates the aggregate DataValues
				SELECT AVG(DV.DataValue)AS AVGDataValue, 
					SUM(DV.DataValue)AS SumDataValue, 
					MIN(DV.DataValue) AS MinDataValue, 
					MAX(DV.DataValue) AS MaxDataValue,					
					CASE 
						WHEN MAX(CASE
									WHEN DataValue IS NULL THEN NULL
									ELSE LocalDateTime
								END) IS NULL THEN MAX(LocalDateTime)
						ELSE MAX(CASE
								WHEN DataValue IS NULL THEN NULL
								ELSE LocalDateTime
							END)
					END AS EndDate,
					--MAX(localdatetime) AS EndDate,
					CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, LocalDateTime)) + '/1/' + CONVERT(VARCHAR,DATEPART(YEAR, LocalDateTime))) AS LocalDateTime, --sets the local date time to the first day of the month
					DV.SiteID, 
					S.SiteCode,
					DV.VariableID, 
					DV.VariableCode, 
					COUNT(DV.DataValue) AS NumOfValues,  
					 -- Calculates the number of values needed divided by the number of values gathered
					1-COUNT(DV.DataValue)/CAST((DATEDIFF(DAY,CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, LocalDateTime)) + '/1/' +
					CONVERT(VARCHAR,DATEPART(YEAR, LocalDateTime))), DATEADD(MONTH,1,CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, LocalDateTime)) + '/1/'
					+CONVERT(VARCHAR,DATEPART(YEAR, LocalDateTime)))))) AS FLOAT) AS noDVPerc,
					SC.EndDateTime, 
					--'New' indicates the id the new series will be saved with.
					AgS.VariableID AS NewVariableID,			
					NewS.SiteID AS NewSiteID, 
					AgS.MethodID AS NewMethodID, 
					AgS.SourceID AS NewSourceID,
					AgS.QualityControlLevelID As NewQCLID,				
					AgS.AggregateMethod AS AggregateMethod,	--specifies wether to select max, min, sum, average or end of interval value			
					MAX(DV.UTCOffset) AS UTCOffset, 
					MAX(DV.NoDataValue)AS NoDataValue
					FROM
						(SELECT DV.VariableID, SiteID, 
							CASE 
								WHEN DV.DataValue=V.NoDataValue THEN NULL
								ELSE DV.DataValue 
							END AS DataValue, LocalDateTime, UTCOffset, CensorCode, V.VariableCode, V.NoDataValue 							 
						FROM
							OPENDATASOURCE(
								'SQLOLEDB',
								'Data Source=wasser.uwrl.usu.edu;Initial Catalog = NCDC;User ID=NIDIS;Password=N1d1s!'--FromDB
								).NCDC.dbo.DataValues AS DV
							JOIN
							OPENDATASOURCE(
								'SQLOLEDB',
								'Data Source=wasser.uwrl.usu.edu;Initial Catalog = NCDC;User ID=NIDIS;Password=N1d1s!'--FromDB
								).NCDC.dbo.Variables AS V
							ON DV.VariableID = V.VariableID -- gets detailed variable information from current db. needed for NoDataValue
							WHERE DV.VariableID = @origVariableID AND CensorCode='nc'  
						) AS DV-- gets detailed variable information from current db. needed for NoDataValue
					JOIN 
						OPENDATASOURCE(
						'SQLOLEDB',
						'Data Source=wasser.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'--FromDB
						).Summary.dbo.Sites AS S
					ON S.SiteID= DV.SiteID --gets detailed site information from current db. -needed for siteCode
					JOIN				
						OPENDATASOURCE(
						'SQLOLEDB',
						'Data Source=wasser.uwrl.usu.edu;Initial Catalog = SummaryTest; User ID=NIDIS;Password=N1d1s!'--NewDB
						).SummaryTest.dbo.Sites  AS NewS --select the site information from the database the date is being saved too. needed for "new" siteid
					ON NewS.SiteCode = S.SiteCode 				
					JOIN	
						OPENDATASOURCE(
						'SQLOLEDB',
						'Data Source=wasser.uwrl.usu.edu;Initial Catalog = SummaryTest;User ID=NIDIS;Password=N1d1s!'--NewDB
						).SummaryTest.dbo.AggregateSeries AS AgS
					ON AgS.DataTimePeriod = 'Monthly' AND AgS.SiteType = @siteType --get the aggregate information for current series, and 'new' id's
				    LEFT JOIN				
						OPENDATASOURCE(
						'SQLOLEDB',
						'Data Source=wasser.uwrl.usu.edu;Initial Catalog = SummaryTest;User ID=NIDIS;Password=N1d1s!'--NewDB
						).SummaryTest.dbo.SeriesCatalog AS SC --gets the series information to calculate new start date
					ON SC.VariableID = AgS.VariableID AND SC.SiteID= NewS.SiteID AND SC.QualityControlLevelID = AgS.QualityControlLevelID AND SC.SourceID = AgS.SourceID AND SC.MethodID = AgS.MethodID 
					-- actually has a value. is of the variable type requested and is non censored data and date time is between enddate and startdate
					WHERE DV.VariableID = @origVariableID AND DV.CensorCode='nc'  -- DV.DataValue <> V.NoDataValue AND
					AND DV.LocalDateTime Between(
					--case statement checks to see if there is a valid enddate from the series catalog entry
						CASE
							WHEN SC.EndDateTime IS NULL THEN '1900-01-01'--if no enddate found set to default  
							ELSE  DATEADD(MONTH, 1, SC.EndDateTime)-- if EndDateTime is Valid, add 1 month on for next start date
						END )
					--Calculate the endDate by finding the last day of last month
					AND DATEADD(DAY,-1,CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, GETDATE())) + '/1/' + CONVERT(VARCHAR,DATEPART(YEAR, GETDATE()))))			
					GROUP BY DATEPART(YEAR, LocalDateTime), DATEPART(MONTH, LocalDateTime), 
						DV.SiteID, 
						S.SiteCode, 
						DV.VariableID,  
						DV.VariableCode, 
						SC.EndDateTime, 
						AgS.AggregateMethod,
						NewS.SiteID, 
						AgS.VariableID, 
						AgS.MethodID, 
						AgS.SourceID, 
						AgS.QualityControlLevelID 		 
			 ) AS Data
		ON DV.SiteID=Data.SiteID AND DV.VariableID=Data.VariableID AND DV.LocalDateTime=Data.EndDate
		LEFT JOIN 
		OPENDATASOURCE(
			'SQLOLEDB',
			'Data Source=wasser.uwrl.usu.edu;Initial Catalog = SummaryTest;User ID=NIDIS;Password=N1d1s!'--NewDB
			).SummaryTest.dbo.Qualifiers AS Q --finds the qualifier id from the 'new' database associated with the No DataValue Percentage 
		ON '2'+CAST( CAST(Data.noDVPerc*10 AS INTEGER)AS NVARCHAR(4)) = QualifierCode   
)AS T




--When the aggregate method from the table above is DeltaValue replace its current datavalue with the following 
UPDATE #temp1 
SET DataValue= Value  
FROM (
	    SELECT a.LocalDateTime, 
	       --case makes sure that the a value and b value are only 1 month apart. 
			CASE
				WHEN a.DataValue= a.NoDV OR b.DataValue =b.NoDV THEN a.NoDV	
				 WHEN ABS(DATEDIFF(Month, a.LocalDateTime, b.LocalDateTime)) = 1 
					THEN a.DataValue-b.DataValue
				 ELSE
					a.NoDV  
			END AS Value, 
			--ABS(DATEDIFF(Month, a.LocalDateTime, b.LocalDateTime))AS diff,
			--a.DataValue AS aval,
			--a.RowNum AS anum,
			--b.DataValue AS bval,
			--b.RowNum AS bnum,
			a.SiteID, 
			(Case
			 WHEN a.VariableID = (SELECT MAX(VariableID) FROM #temp1 WHERE AggregateMethod ='Delta') THEN 0
			 ELSE (SELECT MAX(VariableID) FROM #temp1 WHERE AggregateMethod ='Delta')
			 END
			) AS VariableID
			--a.VariableID
			--matches current value with previous value
		FROM		
				--takes the temp table calculated above and assigns a row number to each row.
				--from 1 to number of values in the series(siteid, variableid)  
				--values are in order by time
			(SELECT SiteID,
				VariableID,				
				ROW_NUMBER() OVER(PARTITION BY SiteID, VariableID ORDER BY LocalDateTime) AS RowNum,
				LocalDateTime,
				DataValue,				
				NoDV
			FROM #temp1) AS a 
			LEFT JOIN 
			-- join on same table as above. 
			(SELECT SiteID,
						VariableID,
						ROW_NUMBER() OVER(PARTITION BY SiteID, VariableID ORDER BY LocalDateTime) AS RowNum,
						LocalDateTime,
						DataValue,
						NoDV  
					FROM #temp1 AS T
				UNION
				--needed for an update on the DV table. To calculate the first value in the sequence we need the previous value 
				--i.e. to calculate the value on 10-01-2011 i need to get the value from 09-01-2011. 
				--Union with new Datavalues table, and return the very last datavalue for the series(siteid, variableid). 
				SELECT 
						SiteID, 
						VariableID, 
						0 AS RowNum, --assign row value 0 so it is the very first in the sequence
						LocalDateTime, 
						DataValue,
						NULL AS NoDV 
					FROM  
					OPENDATASOURCE(
					'SQLOLEDB',
					'Data Source=wasser.uwrl.usu.edu;Initial Catalog = SummaryTest;User ID=NIDIS;Password=N1d1s!'--NewDB
					).SummaryTest.dbo.DataValues 
					--VariableID is the value from #temp1 table
					WHERE  VariableID = (SELECT MAX(VariableID) FROM #temp1 WHERE AggregateMethod <> 'Delta') AND LocalDateTime = (SELECT MAX(LocalDateTime) FROM OPENDATASOURCE('SQLOLEDB', 'Data Source=wasser.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!').Summary.dbo.DataValues WHERE VariableID= (SELECT MAX(VariableID) FROM #temp1 WHERE AggregateMethod <> 'Delta'))
				--order by SiteID
				) AS b
		-- join two tables by series(siteid, variableid) using the next number in sequence. so 2 is matched with 1.
		ON a.SiteID = b.SiteID  AND a.VariableID= b.VariableID AND a.RowNum = b.RowNum+1
		--ORDER BY VariableID, SiteID
		) AS Data
--how do i know which values from #temp1 i should replace?
WHERE #temp1.LocalDateTime = Data.LocalDateTime AND #temp1.SiteID= Data.SiteID AND #temp1.VariableID = Data.VariableID AND #temp1.AggregateMethod = 'Delta'--#temp1.VariableID= 23--VariableId is from #temp1

	

	
--INSERT INTO OPENDATASOURCE(
--		'SQLOLEDB',
--		'Data Source=wasser.uwrl.usu.edu;Initial Catalog = SummaryTest;User ID=NIDIS;Password=N1d1s!'--NewDB
--		).SummaryTest.dbo.DataValues
--		(DataValue,
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
--SELECT DataValue,
--	 ValueAccuracy, 
--	 LocalDateTime, 
--	 UTCOffset, 
--	 DateTimeUTC,
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
--	 FROM #temp1
--ORDER BY VariableID, SiteID, LocalDateTime


SELECT * FROM #temp1
ORDER BY VariableID, SiteID, LocalDateTime
--DROP TABLE #temp1
