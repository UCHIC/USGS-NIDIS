
-- Add the parameters for the stored procedure here
DECLARE @FormulaNo INT =1	
DECLARE @siteType NVARCHAR(50) = 'NCDC-Weather'	
		
		
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
	 SeriesStartDate,
	 CASE	   
		WHEN rowNum <> expectedRows THEN 21 
		WHEN /*DataValue IS NULL AND*/ rowNum =0 THEN 34
		WHEN QualifierID IS NULL THEN NULL
		ELSE(SELECT QualifierID FROM OPENDATASOURCE(
					'SQLOLEDB',
					'Data Source=wasser.uwrl.usu.edu;Initial Catalog = SummaryTest;User ID=NIDIS;Password=N1d1s!'--NewDB
					).SummaryTest.dbo.Qualifiers -- finds the qualifier id from the 'new' database associated with the No DataValue Percentage 
				WHERE QualifierCode =Cast(10+(
					SELECT QualifierCode FROM OPENDATASOURCE(
						'SQLOLEDB',
						'Data Source=wasser.uwrl.usu.edu;Initial Catalog = SummaryTest;User ID=NIDIS;Password=N1d1s!'--NewDB
						).SummaryTest.dbo.Qualifiers AS Q
						Where Q.QualifierID = Data.QualifierID)
					 AS NVARCHAR(4))
				)		
	 END AS QualifierID, 
	 MethodID, 
	 SourceID,
	 SampleID,
	 DerivedFromID, 
	 QualityControlLevelID,
	 expectedRows-rowNum AS MissingRows
INTO #temp1
FROM(
  SELECT   
	CASE		
		WHEN (MAX(xref.SiteType) LIKE '%USGS%' OR MIN(xref.SiteType) LIKE '%USGS%')Then (SUM(weight * DataValue)/Max(huc.hucArea))*12
		--WHEN SUM(DataValue) IS NULL THEN MAX(NoDataValue)
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
         MAX(xref.SiteType) As SiteType,
         COUNT(*) AS rowNum,
         MAX(xrefcnt.rowCnt) AS expectedRows,
         MAX(NoDataValue) AS NoDataValue,
         MAX(xrefcnt.BeginDateTime) AS SeriesStartDate         
         --xref.SiteIDTAD
       --SELECT * 
	FROM 				
		(--select the rows for the formula and get its corresponding TAD Series Catalog entry so we can later select the Max Start date and see where the series is supposed to begin
			SELECT * FROM
				IVtoTADxRef AS xref
			LEFT JOIN
				OPENDATASOURCE(--FromDB
					'SQLOLEDB',
					'Data Source=wasser.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
				).Summary.dbo.SeriesCatalog AS SC
			ON xref.SiteCodeTAD = SC.SiteCode AND xref.VariableCodeTAD = SC.VariableCode AND xref.MethodIDTAD = SC.MethodID AND xref.QualityControlLevelIDTAD = SC.QualityControlLevelID
			WHERE xref.SiteCodeIV= '1407000502' AND xref.FormulaNo = /*@FormulaNo*/1 AND xref.SiteType= 'NCDC-Weather'--@siteType --select only the sitetype and formula we are currently trying to calculate
		
		) AS xref		
		----find StartDate
		--LEFT JOIN		
		--(
		--	SELECT MAX(SC.BeginDateTime) AS BeginDateTime, SiteIDIV, VariableIDIV, MethodIDIV FROM
		--		IVtoTADxRef AS xref
		--	LEFT JOIN
		--		OPENDATASOURCE(--FromDB
		--			'SQLOLEDB',
		--			'Data Source=wasser.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
		--		).Summary.dbo.SeriesCatalog AS SC
		--	ON xref.SiteCodeTAD = SC.SiteCode AND xref.VariableCodeTAD = SC.VariableCode AND xref.MethodIDTAD = SC.MethodID AND xref.QualityControlLevelIDTAD = SC.QualityControlLevelID
		--	WHERE xref.SiteCodeIV= '1407000502' AND xref.FormulaNo = /*@FormulaNo*/1 AND xref.SiteType= 'NCDC-Weather'--@siteType --select only the sitetype and formula we are currently trying to calculate
		-- GROUP BY SourceIDIV, xref.SiteIDIV, xref.VariableIDIV, MethodIDIV, QualityControlLevelIDIV
		--) AS Sd--sereis start date
		--ON xref.SiteIDIV = Sd.SiteIDIV AND xref.VariableIDIV = Sd.VariableIDIV and xref.MethodIDIV = Sd.MethodIDIV
		
		--LEFT JOIN (
		----calculates the number of datavalues that should be used in the calculation
		--		SELECT SiteCodeIV, VariableCodeIV, COUNT(*) AS rowCnt 
		--		FROM IVtoTADxRef 
		--		GROUP BY SiteCodeIV, VariableCodeIV
		--	)  AS xrefcnt
		--	ON xref.SiteCodeIV = xrefcnt.SiteCodeIV AND xref.VariableCodeIV = xrefcnt.VariableCodeIV
		--find StartDate		
		LEFT JOIN (
		--calculates the number of datavalues that should be used in the calculation
				SELECT SiteCodeIV, VariableCodeIV, COUNT(*) AS rowCnt ,MethodIDIV, MAX(SC.BeginDateTime) AS BeginDateTime
				FROM IVtoTADxRef AS xref
			LEFT JOIN
				OPENDATASOURCE(--FromDB
					'SQLOLEDB',
					'Data Source=wasser.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
				).Summary.dbo.SeriesCatalog AS SC
			ON xref.SiteCodeTAD = SC.SiteCode AND xref.VariableCodeTAD = SC.VariableCode AND xref.MethodIDTAD = SC.MethodID AND xref.QualityControlLevelIDTAD = SC.QualityControlLevelID
			WHERE  xref.SiteCodeIV= '1407000502' AND xref.FormulaNo = /*@FormulaNo*/1 AND xref.SiteType= 'NCDC-Weather' --select only the sitetype and formula we are currently trying to calculate
		    GROUP BY SiteCodeIV, VariableCodeIV, MethodIDIV, QualityControlLevelIDIV
		)  AS xrefcnt
			ON xref.SiteCodeIV = xrefcnt.SiteCodeIV AND xref.VariableCodeIV = xrefcnt.VariableCodeIV and xrefcnt.MethodIDIV = xref.MethodIDIV
			LEFT JOIN
			(	SELECT CASE	
					WHEN DataValue = v.NoDataValue THEN NULL --this removes the NoDV from the calculation but keeps a row as a place holder
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
			WHERE CensorCode='nc' -- AND LocalDateTime Between '1900-01-01' AND '1920-01-01' --DV.VariableID = @origVariableID AND DV.MethodID = @origMethodID AND 
		)AS dv
		ON
			xref.SourceIDTAD = dv.SourceID AND
			xref.SiteIDTAD = dv.SiteID AND
			xref.VariableIDTAD = dv.VariableID AND
			xref.MethodIDTAD = dv.MethodID AND
			xref.QualityControlLevelIDTAD = dv.QualityControlLevelID			
		LEFT JOIN 
		OPENDATASOURCE(--TooDB
			'SQLOLEDB',
			'Data Source=wasser.uwrl.usu.edu;Initial Catalog = SummaryTest;User ID=NIDIS;Password=N1d1s!'--NewDB
		).SummaryTest.dbo.SeriesCatalog AS sc --gets the series information to calculate new start date
		ON 
			xref.SourceIDIV = sc.SourceID AND
			xref.SiteIDIV = sc.SiteID AND
			xref.VariableIDIV = sc.VariableID AND
			xref.MethodIDIV = sc.MethodID AND 
			xref.QualityControlLevelIDIV = sc.QualityControlLevelID
		LEFT JOIN 
			(--select the area for each huc area 
			SELECT DISTINCT s.SiteID, s.SiteCode, uh.HucID, uh.HucArea 
				FROM 
					OPENDATASOURCE(--TooDB
						'SQLOLEDB',
						'Data Source=wasser.uwrl.usu.edu;Initial Catalog = SummaryTest;User ID=NIDIS;Password=N1d1s!'--NewDB
						).SummaryTest.dbo.Sites AS s
					JOIN UpstreamHucs AS uh --Joins the Site and UPstreamhucs to get the siteid for the hucid. where the HucId is the Sitecode
					ON uh.HucID = s.SiteCode 
			) AS huc
		ON huc.SiteCode = xref.SiteCodeIV
		WHERE DV.LocalDateTime BETWEEN 
			CASE-- case statement checks to see if there is a valid enddate from the series catalog entry
				WHEN SC.EndDateTime IS NULL THEN '1900-01-01'-- if no enddate found set to default 
				-- if EndDateTime is Valid: 
				WHEN xref.DataTimePeriod= 'Monthly' THEN CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, DATEADD(MONTH, 1, SC.EndDateTime))) + '/1/' + CONVERT(VARCHAR,DATEPART(YEAR, DATEADD(MONTH, 1, SC.EndDateTime))))
				WHEN DATEPART(DAY, SC.EndDateTime) = 1 THEN CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, SC.EndDateTime)) + '/16/' + CONVERT(VARCHAR,DATEPART(YEAR, SC.EndDateTime)))	--if it is the first half of the month set equal to 16th day of month
				WHEN DATEPART(DAY, SC.EndDateTime) = 16 THEN CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, DATEADD(MONTH, 1, SC.EndDateTime))) + '/1/' + CONVERT(VARCHAR,DATEPART(YEAR, DATEADD(MONTH, 1, SC.EndDateTime))))--if it is the second half of the month, set equal to first day of the next month
			END 
		AND   
			CASE-- Calculate the endDate by finding the last day of last month
				-- if EndDateTime is Valid: 
				WHEN xref.DataTimePeriod= 'Monthly' OR DATEPART(DAY,GETDATE()) < 16  THEN DATEADD(DAY,-1,CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, GETDATE())) + '/1/' + CONVERT(VARCHAR,DATEPART(YEAR, GETDATE()))))	
				WHEN DATEPART(DAY,GETDATE()) > 16 THEN CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, GETDATE())) + '/15/' + CONVERT(VARCHAR,DATEPART(YEAR, GETDATE())))
			END 
		
	--WHERE dv.LocalDateTime  BETWEEN(
	--				--case statement checks to see if there is a valid enddate from the series catalog entry
	--			CASE
	--			--instead of using 1900 what is the max start date of all of the sites involved in the equation
	--				WHEN sc.EndDateTime IS NULL THEN '1900-01-01'--if no enddate found set to default  
	--				ELSE  DATEADD(MONTH, 1, sc.EndDateTime)-- if EndDateTime is Valid, add 1 month on for next start date
	--			END )
	--			AND DATEADD(DAY,-1,CONVERT(DATETIME,CONVERT(VARCHAR,DATEPART(MONTH, GETDATE())) + '/1/' + CONVERT(VARCHAR,DATEPART(YEAR, GETDATE()))))			
	GROUP BY SourceIDIV, xref.SiteIDIV, xref.VariableIDIV, xref.MethodIDIV, xref.QualityControlLevelIDIV, DateTimeUTC
)AS Data 
WHERE LocalDateTime >=  SeriesStartDate
--ORDER BY SiteID, VariableID, LocalDateTime

Select  * from #temp1
order by VariableID, SiteID, LocalDateTime




--INSERT INTO
--Tester.dbo.DataValues 
----DataValues
--( 
--	DataValue,
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
--	 QualityControlLevelID,
--	 MissingRows
--	 )
--SELECT 	
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
--	 QualityControlLevelID,
--	 MissingRows
--	FROM #temp1
	
	--Drop Table #temp1