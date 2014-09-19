DECLARE @origVariableID FLOAT = 1--12--1--6--
DECLARE @siteType NVARCHAR(50) = 'USGS-Streamflow'--'NCDC-Weather'--'USBR-Reservoir'--
DECLARE @tooConnectionstring VARCHAR(100) ='Data Source=drought.uwrl.usu.edu;Initial Catalog = Summary; User ID=NIDIS; Password=N1d1s!'
DECLARE @tooDBName VARCHAR(35)='Summary'
DECLARE @fromConnectionstring VARCHAR(100)= 'Data Source=drought.uwrl.usu.edu;Initial Catalog = NWIS-L1; User ID=NIDIS; Password=N1d1s!'
DECLARE @fromDBName VARCHAR(35) = '[NWIS-L1]'


CREATE TABLE #temp1
(
    DataValue FLOAT,
    ValueAccuracy FLOAT,
    LocalDateTime DATETIME,
    UTCOffset FLOAT,
    DateTimeUTC DATETIME,
    SiteID INT,
    VariableID INT,
    OffsetValue FLOAT,
    OffsetTypeID INT,
    CensorCode NVARCHAR(50),
    QualifierID INT,
    MethodID INT,
    SourceID INT,
    SampleID INT,
    DerivedFromID INT,
    QualityControlLevelID INT,
    AggregateMethod NVARCHAR(10),
    NoDV FLOAT,
    MonthPart INT
)

DECLARE @getQuery1 NVARCHAR(4000) = ---- Returns DataValues table
'Select '+
-- Case uses the Aggregate method found below to decide which aggregate value to use in the datavalues table.
	'CASE 
		WHEN T.noDVPerc=1 THEN T.NoDataValue
		WHEN T.AggregateMethod=''Average'' AND AvgDataValue IS NOT NULL THEN AvgDataValue
		WHEN T.AggregateMethod=''Total'' AND SumDataValue IS NOT NULL THEN SumDataValue
		WHEN T.AggregateMethod=''Minimum'' AND MinDataValue IS NOT NULL THEN MinDataValue 
		WHEN T.AggregateMethod=''Maximum'' AND MaxDataValue <> T.NoDataValue THEN MaxDataValue 
		WHEN T.AggregateMethod=''EOI'' AND EOIValue IS NOT NULL THEN EOIValue
		WHEN T.AggregateMethod=''Delta'' AND DeltaValue <> T.NoDataValue THEN NULL '+ -- Fill in below, using seperate Query 
		'ELSE T.NoDataValue
	 END AS DataValue,
	 NULL AS ValueAccuracy, 
	 T.LocalDateTime, 
	 T.UTCOffset AS UTCOffset, 
	 DATEADD(HOUR, -T.UTCOffset, T.LocalDateTime) AS DateTimeUTC, '+-- Calculate UTC Time
	 'T.NewSiteID AS SiteID, 
	 T.NewVariableID AS VariableID, 
	 NULL AS OffsetValue, 
	 NULL AS OffsetTypeID, 
	 ''nc'' AS CensorCode, 
	 QualifierID,	
	 T.NewMethodID AS MethodID, 
	 T.NewSourceID AS SourceID,
	 NULL AS SampleID, 
	 NULL AS DerivedFromID, 
	 T.NewQCLID AS QualityControlLevelID, 
	 T.AggregateMethod,
	 T.NoDataValue AS NoDV,	 
	 T.MonthPart
From(
	SELECT '+
		-- Case Statements to do Unit Conversions for discharge
		-- If streamflow. Multiply value by number of seconds in interval, and divide by number of feet in an acre(43560)
		'CASE 
			WHEN '''+@siteType+''' LIKE ''%USGS%'' AND MonthPart = 1
			THEN Data.AVGDataValue * 1296000/43560
			WHEN '''+@siteType+''' LIKE ''%USGS%'' AND MonthPart = 2
			THEN Data.AVGDataValue * ((DATEDIFF(SECOND, Data.LocalDateTime,(DATEADD(MONTH,1,Data.LocalDateTime))))-1296000)/43560
			WHEN '''+@siteType+''' LIKE ''%NCDC%''
			THEN Data.AVGDataValue * .0039
			ELSE Data.AVGDataValue
		END AS AVGDataValue, 
		CASE
			WHEN '''+@siteType+''' LIKE ''%USGS%'' AND MonthPart = 1
			THEN Data.SumDataValue * 1296000/43560
			WHEN '''+@siteType+''' LIKE ''%USGS%'' AND MonthPart = 2
			THEN Data.SumDataValue * ((DATEDIFF(SECOND, Data.LocalDateTime,(DATEADD(MONTH,1,Data.LocalDateTime))))-1296000)/43560
			WHEN '''+@siteType+''' LIKE ''%NCDC%''
			THEN Data.SumDataValue * .0039
			ELSE Data.SumDataValue
		END AS SumDataValue, 
		CASE 
			WHEN '''+@siteType+''' LIKE ''%USGS%'' AND MonthPart = 1
			THEN Data.MinDataValue * 1296000/43560
			WHEN '''+@siteType+''' LIKE ''%USGS%'' AND MonthPart = 2
			THEN Data.MinDataValue * ((DATEDIFF(SECOND, Data.LocalDateTime,(DATEADD(MONTH,1,Data.LocalDateTime))))-1296000)/43560
			WHEN '''+@siteType+''' LIKE ''%NCDC%''
			THEN Data.MinDataValue * .0039
			ELSE Data.MinDataValue
		END AS MinDataValue, 
		CASE  
			WHEN '''+@siteType+''' LIKE ''%USGS%'' AND MonthPart = 1
			THEN Data.MaxDataValue * 1296000/43560
			WHEN '''+@siteType+''' LIKE ''%USGS%'' AND MonthPart = 2
			THEN Data.MaxDataValue * ((DATEDIFF(SECOND, Data.LocalDateTime,(DATEADD(MONTH,1,Data.LocalDateTime))))-1296000)/43560
			WHEN '''+@siteType+''' LIKE ''%NCDC%''
			THEN Data.MaxDataValue * .0039
			ELSE Data.MaxDataValue
		END AS MaxDataValue, 
		CASE
			WHEN '''+@siteType+''' LIKE ''%USGS%'' AND MonthPart = 1
			THEN DV.DataValue * 1296000/43560
			WHEN '''+@siteType+''' LIKE ''%USGS%'' AND MonthPart = 2
			THEN DV.DataValue * ((DATEDIFF(SECOND, Data.LocalDateTime,(DATEADD(MONTH,1,Data.LocalDateTime))))-1296000)/43560
			WHEN '''+@siteType+''' LIKE ''%NCDC%''
			THEN DV.DataValue * .0039
			ELSE DV.DataValue
		END AS EOIValue,
		Cast(NULL AS FLOAT) AS DeltaValue, '+-- Placeholder for delta Values calculated later
		'Data.EndDate, '+-- Gets the last date with a Value 
		'Data.LocalDateTime, 
		Data.SiteID, 
		Data.SiteCode, 
		Data.VariableID, 
		Data.VariableCode, 
		Data.NumOfValues, 
		Data.noDVPerc, 
		Data.UTCOffset, 
		Data.NoDataValue, 
		Data.EndDateTime,
		Data.AggregateMethod, '+
		-- 'New' indicates the id the new series will be saved with.
	    'Data.NewVariableID,			
		Data.NewSiteID, 
		Data.NewMethodID, 
		Data.NewSourceID,
		Data.NewQCLID,	
		Data.MonthPart,
		CASE 
			WHEN '''+@siteType +'''= ''NWS-SNODAS-HUC10'' THEN 10+Q.QualifierID 
			WHEN Data.noDVPerc =1 THEN NULL'+ -- If all values are missing
			'
			WHEN Data.noDVPerc = 0 THEN NULL' +-- If no values are missing
			'
			ELSE Q.QualifierID
		END AS QualifierID '
 DECLARE @getQuery2 NVARCHAR(4000) =
 '	FROM  
		OPENDATASOURCE( '+--FromDB
		    '''SQLOLEDB'',
			'''+@fromConnectionstring+'''
			).'+@fromDBName+'.dbo.DataValues AS DV 
		    RIGHT JOIN( '+ 
				-- Calculates the aggregate DataValues
				'SELECT AVG(DV.DataValue)AS AVGDataValue, 
					SUM(DV.DataValue)AS SumDataValue,  
					MIN(DV.DataValue) AS MinDataValue, 
					MAX(DV.DataValue) AS MaxDataValue, 
					CASE 
						WHEN MAX(CASE
									WHEN DataValue IS NULL THEN NULL
									ELSE LocalDateTime
							END) IS NULL THEN MAX(LocalDateTime)
					ELSE 
						MAX(CASE
							WHEN DataValue IS NULL THEN NULL
							ELSE LocalDateTime
						END)
					END AS EndDate,
					CASE DV.MonthPart -- sets the local date time to the beginning of the interval
						WHEN 1 THEN CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, LocalDateTime))+''/1/''+CONVERT(VARCHAR,DATEPART(YEAR, LocalDateTime))) 
						WHEN 2 THEN CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, LocalDateTime))+''/16/''+CONVERT(VARCHAR,DATEPART(YEAR, LocalDateTime)))
					END AS  LocalDateTime, 					 
					DV.SiteID, 
					S.SiteCode,
					DV.VariableID, 
					DV.VariableCode,
					COUNT(DV.DataValue) AS NumOfValues, '+ 
					 -- Calculates the number of values needed divided by the number of values gathered
					'CASE DV.MonthPart
					WHEN 1 THEN 1-COUNT(DV.DataValue)/
						CAST(DATEDIFF(DAY,
						CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, LocalDateTime))+''/1/''+CONVERT(VARCHAR,DATEPART(YEAR, LocalDateTime))), 
						CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, LocalDateTime))+''/16/''+CONVERT(VARCHAR,DATEPART(YEAR, LocalDateTime))) 
						) AS FLOAT)
					WHEN 2 THEN 1-COUNT(DV.DataValue)/
						CAST((DATEDIFF(DAY,
						CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, LocalDateTime))+''/16/''+CONVERT(VARCHAR,DATEPART(YEAR, LocalDateTime))), 
						DATEADD(MONTH,1,CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, LocalDateTime))+''/1/''+CONVERT(VARCHAR,DATEPART(YEAR, LocalDateTime)))))
						) AS FLOAT)
					END  AS noDVPerc,
					SC.EndDateTime, '+  
					-- 'New' indicates the id the new series will be saved with.
					'AgS.VariableID AS NewVariableID,			
					NewS.SiteID AS NewSiteID, 
					AgS.MethodID AS NewMethodID, 
					AgS.SourceID AS NewSourceID, 
					AgS.QualityControlLevelID As NewQCLID,				
					AgS.AggregateMethod AS AggregateMethod,	'+-- Specifies wether to select max, min, sum, average or end of interval value			
					'MAX(DV.UTCOffset) AS UTCOffset, 
					MAX(DV.NoDataValue)AS NoDataValue,
					MonthPart '
 DECLARE @getQuery3 NVARCHAR(4000) ='				
					FROM
						(SELECT DV.VariableID, SiteID, V.VariableCode, V.NoDataValue,
							CASE 
								WHEN DV.DataValue=V.NoDataValue THEN NULL
								ELSE DV.DataValue 
							END AS DataValue, LocalDateTime, UTCOffset, CensorCode,
							CASE 
								WHEN DATEPART(DAY, LocalDateTime) BETWEEN 1 AND 15 THEN 1 
								ELSE 2
							END AS MonthPart  
						FROM
							OPENDATASOURCE( '+--FromDB
								'''SQLOLEDB'',
								'''+@fromConnectionstring+'''
								).'+@fromDBName+'.dbo.DataValues AS DV 
							JOIN
							OPENDATASOURCE( '+--FromDB
								'''SQLOLEDB'',
								'''+@fromConnectionstring+'''
								).'+@fromDBName+'.dbo.Variables AS V
							ON DV.VariableID = V.VariableID '+-- gets detailed variable information from current db. needed for NoDataValue
						'WHERE  DV.VariableID = '+CONVERT(VARCHAR, @origVariableID)+' AND DV.CensorCode=''nc'' 
						)AS DV  '+-- select all of the information from the datavalues
					'JOIN 
						OPENDATASOURCE( '+--FromDB
						'''SQLOLEDB'',
						'''+@fromConnectionstring+'''
						).'+@fromDBName+'.dbo.Sites AS S
					ON S.SiteID= DV.SiteID '+-- gets detailed site information from current db. -needed for siteCode
					'JOIN				
						OPENDATASOURCE( '+--TooDB
						'''SQLOLEDB'',
						'''+@tooConnectionstring+'''
						).'+@tooDBName+'.dbo.Sites  AS NewS '+-- select the site information from the database the date is being saved too. needed for "new" siteid
					'ON NewS.SiteCode = S.SiteCode 				
					JOIN	
						OPENDATASOURCE(
						''SQLOLEDB'', '+ --TooDB
						''''+@tooConnectionstring+'''
						).'+@tooDBName+'.dbo.AggregateSeries AS AgS
					ON AgS.DataTimePeriod = ''BiMonthly'' AND AgS.SiteType = '''+@siteType+''' '+-- get the aggregate information for current series, and 'new' id's
				    'LEFT JOIN				
						OPENDATASOURCE(
						''SQLOLEDB'', '+ --TooDB
						''''+@tooConnectionstring+'''
						).'+@tooDBName+'.dbo.SeriesCatalog AS SC '+-- gets the series information to calculate new start date
					'ON SC.VariableID = AgS.VariableID AND SC.SiteID= NewS.SiteID AND SC.QualityControlLevelID = AgS.QualityControlLevelID AND 
					SC.SourceID = AgS.SourceID AND SC.MethodID = AgS.MethodID '+
					-- actually has a value. is of the variable type requested and is non censored data and date time is between enddate and startdate
					'WHERE DV.LocalDateTime Between(  '+-- case statement checks to see if there is a valid enddate from the series catalog entry
						'CASE
							WHEN SC.EndDateTime IS NULL THEN ''1900-01-01''  '+-- if no enddate found set to default 
							-- else if EndDateTime is Valid: 
							'WHEN DATEPART(DAY,SC.EndDateTime) = 1 THEN CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, SC.EndDateTime)) 
								+ ''/16/'' + CONVERT(VARCHAR,DATEPART(YEAR, SC.EndDateTime))) '+	--if it is the first half of the month set equal to 16th day of month
							'WHEN DATEPART(DAY,SC.EndDateTime) = 16 THEN CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, 
							DATEADD(MONTH, 1, SC.EndDateTime))) + ''/1/'' + CONVERT(VARCHAR,DATEPART(YEAR, DATEADD(MONTH, 1, SC.EndDateTime)))) '+--if it is the second half of the month, set equal to first day of the next month
						'END ) '+
					    -- Calculate the endDate by finding the last day of last month
						'AND DATEADD(DAY,-1,CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, GETDATE())) + ''/1/'' + CONVERT(VARCHAR,DATEPART(YEAR, GETDATE()))))			
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
						AgS.QualityControlLevelID,
						MonthPart,
						SC.EndDateTime 		 
			 ) AS Data
			ON DV.SiteID=Data.SiteID AND DV.VariableID=Data.VariableID AND DV.LocalDateTime=Data.EndDate
			LEFT JOIN 
				OPENDATASOURCE(
				''SQLOLEDB'', '+ --TooDB
				''''+@tooConnectionstring+'''
				).'+@tooDBName+'.dbo.Qualifiers AS Q '+ -- finds the qualifier id from the 'new' database associated with the No DataValue Percentage 
		'ON ''2''+CAST( CAST(Data.noDVPerc*10 AS INTEGER)AS NVARCHAR(4)) = QualifierCode  
)AS T'



DECLARE @updateQuery NVARCHAR(4000)=
-- When the aggregate method from the table above is DeltaValue replace its current datavalue with the following 
'UPDATE #temp1 
SET DataValue= Value  
FROM (
	    SELECT a.LocalDateTime, '+
	       -- Case makes sure that the a value and b value are only 1 month apart. 
			'CASE
				 WHEN a.DataValue= a.NoDV OR b.DataValue =b.NoDV THEN a.NoDV
				 WHEN ABS(DATEDIFF(DAY, a.LocalDateTime, b.LocalDateTime)) <=17 
					THEN a.DataValue-b.DataValue
				 ELSE
					a.NoDV  
			END AS Value, 			
			a.SiteID,
			a.MonthPart, 
			(CASE
				WHEN a.VariableID = (SELECT MAX(VariableID) FROM #temp1 WHERE AggregateMethod =''Delta'') THEN 0
				ELSE (SELECT MAX(VariableID) FROM #temp1 WHERE AggregateMethod =''Delta'')
			 END) AS VariableID '+
			-- Matches current value with previous value
		'FROM	'+ 	
			-- Takes the temp table calculated above and assigns a row number to each row.
			-- From 1 to number of values in the series(siteid, variableid)  
			-- Values are in order by time
			'(SELECT SiteID,
				VariableID,				
				ROW_NUMBER() OVER(PARTITION BY SiteID, VariableID ORDER BY LocalDateTime) AS RowNum,
				LocalDateTime,
				DataValue,
				NoDV,
				CASE 
					WHEN DATEPART(DAY, LocalDateTime) BETWEEN 1 AND 15 THEN 1 
					ELSE 2
				END AS MonthPart
			FROM #temp1) AS a 
			LEFT JOIN '+
			-- Join on same table as above. 
			'(SELECT SiteID,
						VariableID,
						ROW_NUMBER() OVER(PARTITION BY SiteID, VariableID ORDER BY LocalDateTime) AS RowNum,
						LocalDateTime,
						DataValue,
						NoDV,
						CASE 
							WHEN DATEPART(DAY, LocalDateTime) BETWEEN 1 AND 15 THEN 1 
							ELSE 2
						END AS MonthPart  
					FROM #temp1 AS T
				UNION '+
				-- Needed for an update on the DV table. To calculate the first value in the sequence we need the previous value 
				-- i.e. to calculate the value on 10-01-2011 i need to get the value from 09-01-2011. 
				-- Union with new Datavalues table, and return the very last datavalue for the series(siteid, variableid). 
				'SELECT 
						SiteID, 
						VariableID, 
						0 AS RowNum,  '+-- Assign row value 0 so it is the very first in the sequence
						'LocalDateTime, 
						DataValue,
						NULL AS NoDV,
						CASE 
							WHEN DATEPART(DAY, LocalDateTime) BETWEEN 1 AND 15 THEN 1 
							ELSE 2
						END AS MonthPart 
					FROM  
					OPENDATASOURCE( '+--TooDB
						'''SQLOLEDB'',
						'''+@tooConnectionstring+'''
						).'+@tooDBName+'.dbo.DataValues 
					-- VariableID is the value from #temp1 table
					WHERE  VariableID = (SELECT MAX(VariableID) FROM #temp1 WHERE AggregateMethod <> ''Delta'') AND 
						LocalDateTime = (SELECT MAX(LocalDateTime) 
											FROM OPENDATASOURCE( '+--TooDB
											'''SQLOLEDB'', '''+@tooConnectionstring+'''
											).'+@tooDBName+'.dbo.DataValues 
											WHERE VariableID= (SELECT MAX(VariableID) FROM #temp1 WHERE AggregateMethod <> ''Delta''))
			) AS b '+
		-- Join two tables by series(siteid, variableid) using the next number in sequence. so 2 is matched with 1.
		'ON a.SiteID = b.SiteID  AND a.VariableID= b.VariableID AND a.RowNum = b.RowNum+1
		) AS Data '+
-- How do i know which values from #temp1 i should replace?
'WHERE #temp1.LocalDateTime = Data.LocalDateTime 
		AND #temp1.SiteID= Data.SiteID 
		AND #temp1.MonthPart= Data.MonthPart
		AND #temp1.VariableID = Data.VariableID 
		AND #temp1.AggregateMethod = ''Delta'''


DECLARE @saveQuery NVARCHAR(1000)= 
--'INSERT INTO 
--	OPENDATASOURCE(
--	''SQLOLEDB'',
--	'''+@tooConnectionstring+'''
--	).'+@tooDBName+'.dbo.DataValues 
--( DataValue,
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
--	 QualityControlLevelID) '+
'SELECT 	 
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
	FROM #temp1'


PRINT @getQuery1
PRINT @getQuery2
PRINT @getQuery3
PRINT @updateQuery
PRINT @saveQuery

INSERT INTO #temp1 EXEC( @getQuery1+ @getQuery2 +@getQuery3)  
EXEC SP_EXECUTESQL @updateQuery
EXEC SP_EXECUTESQL @saveQuery



		
----SELECT * FROM #temp1
----ORDER BY VariableID, SiteID, LocalDateTime
----SELECT COUNT(*) FROM #temp1

DROP TABLE #temp1
