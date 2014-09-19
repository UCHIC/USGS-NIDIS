	DECLARE @origVariableID FLOAT = 14
	DECLARE @origMethodID FLOAT = 16
	DECLARE @tooConnectionstring VARCHAR(100) ='Data Source=drought.uwrl.usu.edu;Initial Catalog=Summary;User ID=NIDIS;Password=N1d1s!'
	DECLARE @tooDBName VARCHAR(35)='[Summary]'
	DECLARE @fromConnectionstring VARCHAR(100)= 'Data Source=drought.uwrl.usu.edu;Initial Catalog=Summary;User ID=NIDIS;Password=N1d1s!'
	DECLARE @fromDBName VARCHAR(35) = '[Summary]'
	DECLARE @siteType NVARCHAR(50) = 'USBR-Reservoir'

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

		
DECLARE @getQuery1 NVARCHAR(4000) = '		
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
	 ''nc'' AS CensorCode, 
	 CASE 
		WHEN NumOfValues <= 5 THEN 33
		WHEN MaxDataValue IS NULL THEN 32
		ELSE  QualifierID
	END AS QualifierID, 
	 MethodID, 
	 SourceID,
	 NULL AS SampleID,
	 NULL AS DerivedFromID, 
	 QualityControlLevelID--,	 
	 --LessThanCount,
	 --NumOfValues,
	 --MaxDataValue
	 FROM(
		
		SELECT SiteID,  
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
						
		(SELECT Top 1 QualifierID FROM 
				OPENDATASOURCE(
					''SQLOLEDB'',
					'''+@tooConnectionstring+'''
					).'+@tooDBName+'.dbo.Qualifiers 
			WHERE QualifierCode =Cast(10.0+(
				SELECT QualifierCode FROM OPENDATASOURCE(
					''SQLOLEDB'',
					'''+@tooConnectionstring+'''
					).'+@tooDBName+'.dbo.Qualifiers AS Q
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
				'
DECLARE @getQuery2 NVARCHAR(4000) = 
	    '
				IV.MethodID AS NewMethodID,
				IV.VariableID AS NewVariableID,
				IV.SourceID AS NewSourceID,
				IV.QualityControlLevelID AS NewQCLID
				FROM
				(SELECT * FROM
					OPENDATASOURCE(
					''SQLOLEDB'',
					'''+@fromConnectionstring+'''
					).'+@fromDBName+'.dbo.DataValues 
				WHERE VariableID = '+Cast (@origVariableID AS NVARCHAR)+' AND MethodID = '+ CAST (@origMethodID AS NVARCHAR) +' AND CensorCode=''nc'' AND QualityControlLevelID = 3
				)AS dv
				LEFT JOIN
				OPENDATASOURCE(
				''SQLOLEDB'',
				'''+@fromConnectionstring+'''
				).'+@fromDBName+'.dbo.Variables AS v
				ON dv.VariableID = v.VariableID			
				LEFT JOIN
				OPENDATASOURCE(
				''SQLOLEDB'',
				'''+@fromConnectionstring+'''
				).'+@fromDBName+'.dbo.DataValues AS HISTdv
				ON DATEPART(MONTH, HISTdv.LocalDateTime)= DATEPART(MONTH, dv.LocalDateTime) AND DATEPART(DAY, HISTdv.LocalDateTime)= DATEPART(DAY, dv.LocalDateTime)  AND DATEPART(YEAR, HISTdv.LocalDateTime)<= DATEPART(YEAR, dv.LocalDateTime) AND HISTdv.VariableID = dv.VariableID AND HISTdv.SiteID=dv.SiteID AND dv.MethodID = HISTdv.MethodID
				LEFT JOIN 
				IndexValuexRef AS IV
				ON 
				IV.L3VariableID = DV.VariableID AND IV.L3MethodID = DV.MethodID 
				LEFT JOIN					
				OPENDATASOURCE(
					''SQLOLEDB'',
					'''+@tooConnectionstring+'''
					).'+@tooDBName+'.dbo.SeriesCatalog AS SC --gets the series information to calculate new start date
				ON SC.VariableID = IV.VariableID AND SC.SiteID= DV.SiteID AND SC.MethodID = IV.MethodID And SC.QualityControlLevelID= IV.QualityControlLevelID		
				WHERE dv.LocalDateTime  BETWEEN
					CASE '+-- case statement checks to see if there is a valid enddate from the series catalog entry
						'WHEN SC.EndDateTime IS NULL THEN ''1900-01-01'' '+-- if no enddate found set to default 
						-- if EndDateTime is Valid: 
						'WHEN IV.DataTimePeriod= ''Monthly'' THEN CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, DATEADD(MONTH, 1, SC.EndDateTime))) + ''/1/'' + CONVERT(VARCHAR,DATEPART(YEAR, DATEADD(MONTH, 1, SC.EndDateTime))))
						WHEN DATEPART(DAY, SC.EndDateTime) = 1 THEN CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, SC.EndDateTime)) + ''/16/'' + CONVERT(VARCHAR,DATEPART(YEAR, SC.EndDateTime)))	'+--if it is the first half of the month set equal to 16th day of month
						'WHEN DATEPART(DAY, SC.EndDateTime) = 16 THEN CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, DATEADD(MONTH, 1, SC.EndDateTime))) + ''/1/'' + CONVERT(VARCHAR,DATEPART(YEAR, DATEADD(MONTH, 1, SC.EndDateTime)))) '+--if it is the second half of the month, set equal to first day of the next month
					'END 
				AND   
					CASE'+-- Calculate the endDate by finding the last day of last month
						-- if EndDateTime is Valid: 
						' WHEN IV.DataTimePeriod= ''Monthly'' OR DATEPART(DAY,GETDATE()) < 16  THEN DATEADD(DAY,-1,CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, GETDATE())) + ''/1/'' + CONVERT(VARCHAR,DATEPART(YEAR, GETDATE()))))	
						WHEN DATEPART(DAY,GETDATE()) > 16 THEN CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, GETDATE())) + ''/15/'' + CONVERT(VARCHAR,DATEPART(YEAR, GETDATE())))
					END 
			) AS Data
			GROUP BY  LocalDateTime, VariableID, MethodID, SiteID
		) AS dv '
			


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
FROM #temp1	'


  print @getQuery1
	print @getQuery2
	print @saveQuery
	
	
--INSERT INTO #temp1 EXEC( @getQuery1+ @getQuery2)  
--EXEC SP_EXECUTESQL @saveQuery
--Select  * from #temp1

DROP TABLE #temp1