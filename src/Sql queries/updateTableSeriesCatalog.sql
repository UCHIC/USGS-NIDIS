
select * into #temp1 from  Summary.dbo.DataValues where LocalDateTime >'5-1-2012'

DROP TABLE #temp1

  --INSERT INTO [Summary].[dbo].[SeriesCatalog],

SELECT     dv.SiteID, s.SiteCode, s.SiteName, dv.VariableID, v.VariableCode, 
           v.VariableName, v.Speciation, v.VariableUnitsID, u.UnitsName AS VariableUnitsName, v.SampleMedium, 
           v.ValueType, v.TimeSupport, v.TimeUnitsID, u1.UnitsName AS TimeUnitsName, v.DataType, 
           v.GeneralCategory, dv.MethodID, m.MethodDescription, dv.SourceID, so.Organization, 
           so.SourceDescription, so.Citation, dv.QualityControlLevelID, qc.QualityControlLevelCode, dv.BeginDateTime, 
           dv.EndDateTime, dv.BeginDateTimeUTC, dv.EndDateTimeUTC, dv.ValueCount 
INTO #temp2         
FROM  (SELECT SiteID, VariableID, MethodID, QualityControlLevelID, SourceID, MIN(LocalDateTime) AS BeginDateTime, 
           MAX(LocalDateTime) AS EndDateTime, MIN(DateTimeUTC) AS BeginDateTimeUTC, MAX(DateTimeUTC) AS EndDateTimeUTC, 
		   COUNT(DataValue) AS ValueCount
FROM #temp1
GROUP BY SiteID, VariableID, MethodID, QualityControlLevelID, SourceID) dv
           INNER JOIN OPENDATASOURCE(--FromDB
					'SQLOLEDB',
					'Data Source=wasser.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
				).[Summary].[dbo].Sites s ON dv.SiteID = s.SiteID 
		   INNER JOIN OPENDATASOURCE(--FromDB
					'SQLOLEDB',
					'Data Source=wasser.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
				).[Summary].[dbo].Variables v ON dv.VariableID = v.VariableID 
		   INNER JOIN OPENDATASOURCE(--FromDB
					'SQLOLEDB',
					'Data Source=wasser.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
				).[Summary].[dbo].Units u ON v.VariableUnitsID = u.UnitsID 
		   INNER JOIN OPENDATASOURCE(--FromDB
					'SQLOLEDB',
					'Data Source=wasser.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
				).[Summary].[dbo].Methods m ON dv.MethodID = m.MethodID 
		   INNER JOIN OPENDATASOURCE(--FromDB
					'SQLOLEDB',
					'Data Source=wasser.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
				).[Summary].[dbo].Units u1 ON v.TimeUnitsID = u1.UnitsID 
		   INNER JOIN OPENDATASOURCE(--FromDB
					'SQLOLEDB',
					'Data Source=wasser.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
				).[Summary].[dbo].Sources so ON dv.SourceID = so.SourceID 
		   INNER JOIN OPENDATASOURCE(--FromDB
					'SQLOLEDB',
					'Data Source=wasser.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
				).[Summary].[dbo].QualityControlLevels qc ON dv.QualityControlLevelID = qc.QualityControlLevelID
GROUP BY   dv.SiteID, s.SiteCode, s.SiteName, dv.VariableID, v.VariableCode, v.VariableName, v.Speciation,
           v.VariableUnitsID, u.UnitsName, v.SampleMedium, v.ValueType, v.TimeSupport, v.TimeUnitsID, u1.UnitsName, 
		   v.DataType, v.GeneralCategory, dv.MethodID, m.MethodDescription, dv.SourceID, so.Organization, 
		   so.SourceDescription, so.Citation, dv.QualityControlLevelID, qc.QualityControlLevelCode, dv.BeginDateTime,
		   dv.EndDateTime, dv.BeginDateTimeUTC, dv.EndDateTimeUTC, dv.ValueCount

select * from #temp2



/* Step 2 - Insert New Rows from Source into Target */
INSERT INTO OPENDATASOURCE(--FromDB
					'SQLOLEDB',
					'Data Source=wasser.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
				).[Summary].[dbo].SeriesCatalog
		(SiteID, SiteCode,SiteName,VariableID, VariableCode, VariableName, Speciation, VariableUnitsID,VariableUnitsName,
      SampleMedium, ValueType,TimeSupport, TimeUnitsID, TimeUnitsName,DataType,GeneralCategory,MethodID,
      MethodDescription,SourceID, Organization,SourceDescription, Citation, QualityControlLevelID,QualityControlLevelCode, BeginDateTime,
      EndDateTime,BeginDateTimeUTC,EndDateTimeUTC,ValueCount)						
SELECT *
  FROM #temp2 AS SOURCE
WHERE NOT EXISTS
(SELECT *
   FROM OPENDATASOURCE(--FromDB
					'SQLOLEDB',
					'Data Source=wasser.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
				).[Summary].[dbo].SeriesCatalog AS TARGET
  WHERE target.SiteID = Source.SiteID AND target.VariableID = Source.VariableID AND target.MethodID = Source.MethodID AND target.SourceID = Source.SourceID AND target.QualityControlLevelID= Source.QualityControlLevelID
)


/* Step 1 - Update Target Rows from Source Rows */
UPDATE  OPENDATASOURCE(--FromDB
					'SQLOLEDB',
					'Data Source=wasser.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
				).[Summary].[dbo].SeriesCatalog-- AS targ
 SET  EndDateTime= Source.EndDateTime,  EndDateTimeUTC= Source.EndDateTimeUTC,  ValueCount= target.ValueCount+Source.ValueCount
 
 --Select*
 FROM
 OPENDATASOURCE(--FromDB
					'SQLOLEDB',
					'Data Source=wasser.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
				).[Summary].[dbo].SeriesCatalog AS target
INNER JOIN 
#temp2 As Source
ON target.SiteID = Source.SiteID AND target.VariableID = Source.VariableID AND target.MethodID = Source.MethodID AND target.SourceID = Source.SourceID AND target.QualityControlLevelID= Source.QualityControlLevelID
 
UPDATE t SET t.EndDateTime= s.EndDateTime, t.EndDateTimeUTC=s.EndDateTimeUTC,t.ValueCount=t.ValueCount+s.ValueCount
FROM OPENDATASOURCE(--FromDB
					'SQLOLEDB',
					'Data Source=wasser.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
				).[Summary].[dbo].SeriesCatalog as t
INNER JOIN #temp2 AS s ON t.SiteID = s.SiteID AND t.VariableID = s.VariableID AND t.MethodID = s.MethodID AND t.SourceID = s.SourceID AND t.QualityControlLevelID= s.QualityControlLevelID

 
 
 
 
















----this method may only be used when the target db is local


--BEGIN
--    SET NOCOUNT ON;

--    MERGE INTO [SummaryTest].[dbo].[SeriesCatalog] AS target
--    USING #temp2 AS source
--    ON (target.SiteID = source.SiteID AND target.VariableID = source.VariableID AND target.MethodID = source.MethodID AND target.SourceID = source.SourceID AND target.QualityControlLevelID= source.QualityControlLevelID)
--    WHEN MATCHED THEN 
--        UPDATE SET  EndDateTime= source.EndDateTime,
--           EndDateTimeUTC= source.EndDateTimeUTC, 
--		    ValueCount= target.ValueCount+source.ValueCount
--	WHEN NOT MATCHED THEN	
--	    INSERT (SiteID,
--      SiteCode,
--      SiteName,
--      VariableID,
--      VariableCode,
--      VariableName,
--      Speciation,
--      VariableUnitsID,
--      VariableUnitsName,
--      SampleMedium,
--      ValueType,
--      TimeSupport,
--      TimeUnitsID,
--      TimeUnitsName,
--      DataType,
--      GeneralCategory,
--      MethodID,
--      MethodDescription,
--      SourceID,
--      Organization,
--      SourceDescription,
--      Citation,
--      QualityControlLevelID,
--      QualityControlLevelCode,
--      BeginDateTime,
--      EndDateTime,
--      BeginDateTimeUTC,
--      EndDateTimeUTC,
--      ValueCount)
--	    VALUES (source.SiteID,
--      source.SiteCode,
--      source.SiteName,
--      source.VariableID,
--      source.VariableCode,
--      source.VariableName,
--      source.Speciation,
--      source.VariableUnitsID,
--      source.VariableUnitsName,
--      source.SampleMedium,
--      source.ValueType,
--      source.TimeSupport,
--      source.TimeUnitsID,
--      source.TimeUnitsName,
--      source.DataType,
--      source.GeneralCategory,
--      source.MethodID,
--      source.MethodDescription,
--      source.SourceID,
--      source.Organization,
--      source.SourceDescription,
--      source.Citation,
--      source.QualityControlLevelID,
--      source.QualityControlLevelCode,
--      source.BeginDateTime,
--      source.EndDateTime,
--      source.BeginDateTimeUTC,
--      source.EndDateTimeUTC,
--      source.ValueCount);
--END;




----run this in every database with the updateTable SP
--CREATE TYPE DataValueTable AS TABLE
--(
--    DataValue FLOAT,
--    ValueAccuracy FLOAT,
--    LocalDateTime DATETIME,
--    UTCOffset FLOAT,
--    DateTimeUTC DATETIME,
--    SiteID INT,
--    VariableID INT,
--    OffsetValue FLOAT,
--    OffsetTypeID INT,
--    CensorCode NVARCHAR(50),
--    QualifierID INT,
--    MethodID INT,
--    SourceID INT,
--    SampleID INT,
--    DerivedFromID INT,
--    QualityControlLevelID INT
--)


---- put this in every stored procedure that calls the updateTableSP
--DECLARE @TempTablebma DataValueTable
--Insert INTO @TempTablebma
--SELECT 	 
--	  DataValue, ValueAccuracy, LocalDateTime,UTCOffset,DateTimeUTC, SiteID, VariableID, OffsetValue,OffsetTypeID, CensorCode,  QualifierID, MethodID, SourceID,SampleID,DerivedFromID, QualityControlLevelID
--	FROM #temp1   
	
--DECLARE	@return_value int
--EXEC	@return_value = 
----OPENDATASOURCE(	'SQLOLEDB',	'Data Source=wasser.uwrl.usu.edu;Initial Catalog = SummaryTest;User ID=NIDIS;Password=N1d1s!').
--SummaryTest.[dbo].[UpdateTableSeriesCatalog]	
--		@TempTable = @TempTablebma
		
