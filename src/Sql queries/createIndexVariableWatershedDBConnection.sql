
	DECLARE @origVariableID FLOAT = 2--12--6--
	DECLARE @origMethodID FLOAT = 12
	DECLARE @siteType NVARCHAR(50) = 'NRCS-SNOTEL'	
	DECLARE @tooConnectionstring VARCHAR(100) ='Data Source=wasser.uwrl.usu.edu;Initial Catalog=Summary;User ID=NIDIS;Password=N1d1s!'
	DECLARE @tooDBName VARCHAR(35)='[Summary]'
	DECLARE @fromConnectionstring VARCHAR(100)= 'Data Source=wasser.uwrl.usu.edu;Initial Catalog=Summary;User ID=NIDIS;Password=N1d1s!'
	DECLARE @fromDBName VARCHAR(35) = '[Summary]'


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
    QualityControlLevelID INT
)

DECLARE @getQuery1 NVARCHAR(4000) ='
SELECT
	CASE 
		WHEN DataValue IS NULL THEN NoDataValue
		WHEN QualifierID =21 THEN NoDataValue
		WHEN SiteType = ''USBR-Reservoir'' THEN DataValue
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
	 ''nc'' AS CensorCode, 
	  CASE	   
		WHEN rowNum <> expectedRows THEN 21 
		WHEN  rowNum =0 THEN 34
		WHEN QualifierID IS NULL THEN NULL
		ELSE QualifierID	
	 END AS QualifierID,
	 MethodID, 
	 SourceID,
	 NULL AS SampleID,
	 NULL AS DerivedFromID, 
	 QualityControlLevelID	
FROM(

	SELECT 
		CASE
			WHEN w.SiteType = ''NRCS-SNOTEL'' 
			THEN SUM(DV.DataValue * UH.HucArea)/12 '+ --  convert to acre-feet
		'	WHEN w.SiteType = ''NCDC-Weather''
			THEN SUM(DV.DataValue*(UH.HucArea/totalArea)) '+ --Weighted Average = 
		'	WHEN w.SiteType = ''USBR-Reservoir'' 
			THEN SUM(DV.DataValue) '+	-- sum of reservoirs	
		'	WHEN w.SiteType = ''USGS-Streamflow''
			THEN SUM(DV.DataValue * UH.HucArea)/12 '+--SUM(HUC dv * HUC Area) Convert to acre-feet		
		'	ELSE SUM(DV.DataValue)	
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
		MAX(Sd.BeginDateTime) As SeriesStartDate
	FROM 
		UpstreamHucs AS UH '+ --SummaryTest
		'JOIN 
			(SELECT 
				HucID, 
				Sum(UpstreamHucArea) AS totalArea, '+ -- Sum of the area of all contributing hucs to a watershed
			'	Count(*) AS expectedRows '+-- number of hucs in watershed
			' FROM UpstreamHucs
			GROUP BY HucID
			)AS U2
		ON UH.HucID = U2.HucID
		JOIN
			OPENDATASOURCE(
				''SQLOLEDB'',
				'''+@fromConnectionstring+'''
				).'+@fromDBName+'.dbo.Sites AS upS 
		ON upS.SiteCode = UH.UpstreamHucID 
		'
DECLARE @getQuery2 NVARCHAR(4000) = 
	    '
	    JOIN
			OPENDATASOURCE(
				''SQLOLEDB'',
				'''+@fromConnectionstring+'''
				).'+@fromDBName+'.dbo.Sites AS S
		ON S.SiteCode = UH.HucID 
		JOIN
			(SELECT 
				CASE	
					WHEN DataValue = v.NoDataValue THEN NULL
					ELSE DataValue
				END AS DataValue,
				LocalDateTime,UTCOffset, DateTimeUTC, CensorCode, SiteID, dv.VariableID, QualifierID, MethodID, SourceID, QualityControlLevelID, VariableCode, NoDataValue						
			FROM
				OPENDATASOURCE(
				''SQLOLEDB'',
				'''+@fromConnectionstring+'''
				).'+@fromDBName+'.dbo.DataValues AS dv
			LEFT JOIN
				OPENDATASOURCE(
				''SQLOLEDB'',
				'''+@fromConnectionstring+'''
				).'+@fromDBName+'.dbo.Variables AS v
			ON dv.VariableID = v.VariableID
			WHERE DV.VariableID = '+CAST(@origVariableID AS VARCHAR)+' AND DV.MethodID = '+ CAST(@origMethodID AS VARCHAR)+' AND CensorCode=''nc'' AND dv.QualityControlLevelID = 3
		)AS DV 
		ON DV.SiteID = upS.SiteID 
		JOIN				
		WatershedSeries AS w
		ON DV.VariableID = w.L3VariableID AND DV.MethodID = w.L3MethodID
		LEFT JOIN 
			OPENDATASOURCE(
				''SQLOLEDB'',
				'''+@fromConnectionstring+'''
				).'+@fromDBName+'.dbo.SeriesCatalog AS L3SC
		ON DV.SiteID = L3SC.SiteID AND w.L3VariableCode = L3SC.VariableCode AND w.L3MethodID = L3SC.MethodID AND L3SC.QualityControlLevelID=3			
		LEFT JOIN		
		(
			SELECT MAX(SC.BeginDateTime) AS BeginDateTime, UH.HucID FROM
			UpstreamHucs AS UH
			LEFT JOIN
				OPENDATASOURCE(
				''SQLOLEDB'',
				'''+@fromConnectionstring+'''
				).'+@fromDBName+'.dbo.SeriesCatalog AS SC
			ON UH.UpstreamHucID = SC.SiteCode 
			WHERE  SC.VariableID= '+CAST(@origVariableID AS VARCHAR)+' AND SC.MethodID = '+ CAST(@origMethodID AS VARCHAR)+'
		 GROUP BY UH.HucID
		) AS Sd '+--series start date
		'ON UH.HucID = Sd.HucID 
		LEFT JOIN				
			OPENDATASOURCE(
				''SQLOLEDB'',
				'''+@tooConnectionstring+'''
				).'+@tooDBName+'.dbo.SeriesCatalog AS SC '+ --gets the series information to calculate new start date
		'ON SC.VariableID = w.VariableID AND SC.SiteID= S.SiteID AND SC.MethodID = w.MethodID AND SC.QualityControlLevelID=3
		WHERE w.SiteType= '''+@siteType +--select only the sitetype 
			''' AND DV.LocalDateTime  BETWEEN   '+--case statement checks to see if there is a valid enddate from the series catalog entry
				'CASE '+-- case statement checks to see if there is a valid enddate from the series catalog entry
				'WHEN SC.EndDateTime IS NULL THEN ''1900-01-01'' '+-- if no enddate found set to default 
				-- if EndDateTime is Valid: 
				'WHEN w.DataTimePeriod= ''Monthly'' THEN CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, DATEADD(MONTH, 1, SC.EndDateTime))) + ''/1/'' + CONVERT(VARCHAR,DATEPART(YEAR, DATEADD(MONTH, 1, SC.EndDateTime))))
				WHEN DATEPART(DAY, SC.EndDateTime) = 1 THEN CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, SC.EndDateTime)) + ''/16/'' + CONVERT(VARCHAR,DATEPART(YEAR, SC.EndDateTime)))	'+--if it is the first half of the month set equal to 16th day of month
				'WHEN DATEPART(DAY, SC.EndDateTime) = 16 THEN CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, DATEADD(MONTH, 1, SC.EndDateTime))) + ''/1/'' + CONVERT(VARCHAR,DATEPART(YEAR, DATEADD(MONTH, 1, SC.EndDateTime)))) '+--if it is the second half of the month, set equal to first day of the next month
			'END 
		AND   
			CASE'+-- Calculate the endDate by finding the last day of last month
				-- if EndDateTime is Valid: 
				' WHEN w.DataTimePeriod= ''Monthly'' OR DATEPART(DAY,GETDATE()) < 16  THEN DATEADD(DAY,-1,CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, GETDATE())) + ''/1/'' + CONVERT(VARCHAR,DATEPART(YEAR, GETDATE()))))	
				WHEN DATEPART(DAY,GETDATE()) > 16 THEN CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, GETDATE())) + ''/15/'' + CONVERT(VARCHAR,DATEPART(YEAR, GETDATE())))
			END
			GROUP BY UH.HucID, LocalDateTime, DateTimeUTC, S.SiteID, DV.VariableID, w.SiteType, U2.totalArea, w.VariableID, w.MethodID, w.QualityControlLevelID, w.SourceID
	)AS Data
	WHERE LocalDateTime>=SeriesStartDate'





DECLARE @saveQuery NVARCHAR(2000)='
INSERT INTO
	OPENDATASOURCE(
	''SQLOLEDB'',
	'''+@tooConnectionstring+'''
	).'+@tooDBName+'.dbo.DataValues
	(DataValue,
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
	 QualityControlLevelID)
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
	 FROM #temp1'



    print @getQuery1
	print @getQuery2
	print @saveQuery
--INSERT INTO #temp1 EXEC( @getQuery1+ @getQuery2)  
--EXEC SP_EXECUTESQL @saveQuery

--SELECT * FROM #temp1
	 DROP TABLE #temp1