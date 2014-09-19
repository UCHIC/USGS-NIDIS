
		
		
	-- Add the parameters for the stored procedure here
	DECLARE @FormulaNo INT =1
	DECLARE @tooConnectionstring VARCHAR(100) ='Data Source=wasser.uwrl.usu.edu;Initial Catalog=Summary;User ID=NIDIS;Password=N1d1s!'
	DECLARE @tooDBName VARCHAR(35)='[Summary]'
	DECLARE @fromConnectionstring VARCHAR(100)= 'Data Source=wasser.uwrl.usu.edu;Initial Catalog=Summary;User ID=NIDIS;Password=N1d1s!'
	DECLARE @fromDBName VARCHAR(35) = '[Summary]'
	DECLARE @siteType NVARCHAR(50) = 'USGS-Streamflow'


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
    --,
    --rowNum INT,
    --expectedRows INT    
)

 
DECLARE @getQuery1 NVARCHAR(4000) = '
    SELECT 
	CASE
		WHEN rowNum <> expectedRows THEN NoDataValue
		WHEN DataValue IS NULL THEN NoDataValue
		ELSE DataValue	
	END AS	 
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
	 CASE	   
		WHEN rowNum <> expectedRows THEN 21 
		WHEN rowNum =0 THEN 34
		WHEN QualifierID IS NULL THEN NULL
		ELSE(SELECT QualifierID FROM 
			OPENDATASOURCE(
				''SQLOLEDB'',
				'''+@tooConnectionstring+'''
			).'+@tooDBName+'.dbo.Qualifiers '+-- finds the qualifier id from the 'new' database associated with the No DataValue Percentage 
				' WHERE QualifierCode =Cast(10+(
					SELECT QualifierCode FROM 
						OPENDATASOURCE(
							''SQLOLEDB'',
							'''+@fromConnectionstring+'''
						).'+@fromDBName+'.dbo.Qualifiers AS Q
						Where Q.QualifierID = Data.QualifierID)
					 AS NVARCHAR(4))
				)		
	 END AS QualifierID, 
	 MethodID, 
	 SourceID,
	 SampleID,
	 DerivedFromID, 
	 QualityControlLevelID
INTO #temp1
FROM(
  SELECT   
	CASE
		WHEN (MAX(xref.SiteType) LIKE ''%USGS%'' OR MIN(xref.SiteType) LIKE ''%USGS%'') Then (SUM(weight * DataValue)/Max(huc.hucArea))*12
		ELSE (SUM(weight * DataValue))
	END	AS DataValue, 		 
		 NULL AS ValueAccuracy, 
		 MAX(LocalDateTime)AS LocalDateTime,  
		 MAX(UTCOffset) AS UTCOffset, 
         DateTimeUTC, 
         xref.SiteIDIV AS SiteID, 
         xref.VariableIDIV AS VariableID,
         NULL AS OffsetValue, 
         NULL AS OffsetTypeID, 
         MIN(CensorCode) AS CensorCode,
         MAX(QualifierID) AS QualifierID,
         xref.MethodIDIV AS MethodID,
         SourceIDIV AS SourceID, 
         NULL AS SampleID, 
         NULL AS DerivedFromID, 
         QualityControlLevelIDIV AS QualityControlLevelID,
         Max(xref.SiteType) As SiteType,
         COUNT(*) AS rowNum,
         MAX(xrefcnt.rowCnt) AS expectedRows,
         MAX(NoDataValue) AS NoDataValue,
         MAX(xrefcnt.BeginDateTime) AS SeriesStartDate          
	FROM 				
		('+--select the rows for the formula and get its corresponding TAD Series Catalog entry so we can later select the Max Start date and see where the series is supposed to begin
			'SELECT * FROM
				IVtoTADxRef AS xref
			LEFT JOIN
				OPENDATASOURCE(
					''SQLOLEDB'',
					'''+@fromConnectionstring+'''
					).'+@fromDBName+'.dbo.SeriesCatalog AS SC
			ON xref.SiteCodeTAD = SC.SiteCode AND xref.VariableCodeTAD = SC.VariableCode AND xref.MethodIDTAD = SC.MethodID AND xref.QualityControlLevelIDTAD = SC.QualityControlLevelID
			WHERE xref.FormulaNo = '+Cast(@FormulaNo AS NVARCHAR)+' AND xref.SiteType= '''+@siteType+''''+ --select only the sitetype and formula we are currently trying to calculate
		' ) AS xref 
		LEFT JOIN ( ' +
		--calculates the number of datavalues that should be used in the calculation
				'SELECT SiteCodeIV, VariableCodeIV, COUNT(*) AS rowCnt ,MethodIDIV, MAX(SC.BeginDateTime) AS BeginDateTime
				FROM IVtoTADxRef AS xref
			LEFT JOIN
				OPENDATASOURCE(
					''SQLOLEDB'',
					'''+@fromConnectionstring+'''
					).'+@fromDBName+'.dbo.SeriesCatalog AS SC
			ON xref.SiteCodeTAD = SC.SiteCode AND xref.VariableCodeTAD = SC.VariableCode AND xref.MethodIDTAD = SC.MethodID AND xref.QualityControlLevelIDTAD = SC.QualityControlLevelID
			WHERE  xref.FormulaNo = '+Cast(@FormulaNo AS NVARCHAR)+' AND xref.SiteType= '''+@siteType+''''+--select only the sitetype and formula we are currently trying to calculate
		   ' GROUP BY SiteCodeIV, VariableCodeIV, MethodIDIV, QualityControlLevelIDIV
		)  AS xrefcnt
			ON xref.SiteCodeIV = xrefcnt.SiteCodeIV AND xref.VariableCodeIV = xrefcnt.VariableCodeIV and xrefcnt.MethodIDIV = xref.MethodIDIV
			'
DECLARE @getQuery2 NVARCHAR(4000) = 
	    '	
		LEFT JOIN 
		(	SELECT CASE	
					WHEN DataValue = v.NoDataValue THEN NULL
					ELSE DataValue
				END AS DataValue,LocalDateTime,UTCOffset, DateTimeUTC, CensorCode, SiteID, dv.VariableID, QualifierID, MethodID, SourceID, QualityControlLevelID, VariableCode, NoDataValue						
			FROM
			OPENDATASOURCE(
				''SQLOLEDB'',
				'''+@fromConnectionstring+'''
				).'+@fromDBName+'.dbo.DataValues AS dv
			LEFT JOIN		
			OPENDATASOURCE(
				''SQLOLEDB'',
				'''+@tooConnectionstring+'''
				).'+@tooDBName+'.dbo.Variables AS v
			ON dv.VariableID = v.VariableID
			WHERE CensorCode=''nc''
		)AS dv
		ON 	
			xref.SourceIDTAD = dv.SourceID AND
			xref.SiteIDTAD = dv.SiteID AND
			xref.VariableIDTAD = dv.VariableID AND
			xref.MethodIDTAD = dv.MethodID AND
			xref.QualityControlLevelIDTAD = dv.QualityControlLevelID			
		LEFT JOIN 
		OPENDATASOURCE(
			''SQLOLEDB'',
			'''+@fromConnectionstring+'''
			).'+@fromDBName+'.dbo.SeriesCatalog AS sc '+--gets the series information to calculate new start date
		'ON 
			xref.SourceIDIV = sc.SourceID AND
			xref.SiteIDIV = sc.SiteID AND
			xref.VariableIDIV = sc.VariableID AND
			xref.MethodIDIV = sc.MethodID AND 
			xref.QualityControlLevelIDIV = sc.QualityControlLevelID
		LEFT JOIN 
			( SELECT DISTINCT s.SiteID, s.SiteCode, uh.HucID, uh.HucArea FROM 
				OPENDATASOURCE(
					''SQLOLEDB'',
					'''+@fromConnectionstring+'''
				).'+@fromDBName+'.dbo.Sites AS s '+--gets the series information to calculate new start date
				'JOIN 
				UpstreamHucs AS uh 
				ON uh.HucID = s.SiteCode
			) AS huc
		ON huc.SiteCode = xref.SiteCodeIV
	WHERE dv.LocalDateTime  BETWEEN
					'+--case statement checks to see if there is a valid enddate from the series catalog entry
				'CASE '+-- case statement checks to see if there is a valid enddate from the series catalog entry
				'WHEN SC.EndDateTime IS NULL THEN ''1900-01-01'' '+-- if no enddate found set to default 
				-- if EndDateTime is Valid: 
				'WHEN xref.DataTimePeriod= ''Monthly'' THEN CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, DATEADD(MONTH, 1, SC.EndDateTime))) + ''/1/'' + CONVERT(VARCHAR,DATEPART(YEAR, DATEADD(MONTH, 1, SC.EndDateTime))))
				WHEN DATEPART(DAY, SC.EndDateTime) = 1 THEN CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, SC.EndDateTime)) + ''/16/'' + CONVERT(VARCHAR,DATEPART(YEAR, SC.EndDateTime)))	'+--if it is the first half of the month set equal to 16th day of month
				'WHEN DATEPART(DAY, SC.EndDateTime) = 16 THEN CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, DATEADD(MONTH, 1, SC.EndDateTime))) + ''/1/'' + CONVERT(VARCHAR,DATEPART(YEAR, DATEADD(MONTH, 1, SC.EndDateTime)))) '+--if it is the second half of the month, set equal to first day of the next month
			'END 
		AND   
			CASE '+-- Calculate the endDate by finding the last day of last month
				-- if EndDateTime is Valid: 
				'WHEN xref.DataTimePeriod= ''Monthly'' OR DATEPART(DAY,GETDATE()) < 16  THEN DATEADD(DAY,-1,CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, GETDATE())) + ''/1/'' + CONVERT(VARCHAR,DATEPART(YEAR, GETDATE()))))	
				WHEN DATEPART(DAY,GETDATE()) > 16 THEN CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, GETDATE())) + ''/15/'' + CONVERT(VARCHAR,DATEPART(YEAR, GETDATE())))
			END
	GROUP BY SourceIDIV, xref.SiteIDIV, xref.VariableIDIV, xref.MethodIDIV, QualityControlLevelIDIV, DateTimeUTC
) AS Data
WHERE LocalDateTime >=  SeriesStartDate'




DECLARE @saveQuery NVARCHAR(2000)='
INSERT INTO
	OPENDATASOURCE(
	''SQLOLEDB'',
	'''+@tooConnectionstring+'''
	).'+@tooDBName+'.dbo.DataValues
( DataValue,
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
	 )
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

--Select  * from #temp1
--where VariableID = 13 OR VariableID= 14
--order by SiteType

--SELECT @returnval = COUNT(*) FROM #temp1
	--Drop Table #temp1