CREATE TABLE #temp1
(   
	 SiteIDTAD INT,
     SiteCodeTAD NVARCHAR(50),
     VariableIDTAD INT,
     VariableCodeTAD NVARCHAR(50),
     MethodIDTAD INT,
     QualityControlLevelIDTAD INT, 	
	 SourceIDTAD INT, 
     SiteIDIV INT,
     SiteCodeIV NVARCHAR(50), 
     VariableIDIV INT,
     VariableCodeIV NVARCHAR(50),
     MethodIDIV INT,
     QualityControlLevelIDIV INT,
     SourceIDIV INT,
     FormulaNo INT,
	 Weight FLOAT,
	 SiteType NVARCHAR(255)
	 
)




--Precipitation
	
	INsert Into #temp1
	Select
      TadSite.SiteID AS SiteIDTAD,
      TadSite.SiteCode AS SiteCodeTAD,
      28 AS VariableIDTAD,--Month:29 BiMonth:28
      'PRCP-P0.5M' AS VariableCodeTAD,
      6 AS MethodIDTAD,
      2 AS QualityControlLevelIDTAD, 	
	  3 AS SourceIDTAD,      
      
      IVSite.SiteID AS SiteIDIV,
      IVSite.SiteCode AS SiteCodeIV, 
      28 AS VariableIDIV,--Month:29 BiMonth:28
      'PRCP-P0.5M' AS VariableCodeIV,
      9 AS MethodIDIV,
      3 AS QualityControlLevelIDIV,
      3 AS SourceIDIV,
      
      1 AS FormulaNo,
	  Precip_Gage_Weight AS Weight,
	  'NCDC-Weather' AS SiteType
     FROM PrecipWeights as pw
      JOIN	
	OPENDATASOURCE(
     'SQLOLEDB',
	 'Data Source=drought.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
	).Summary.dbo.Sites	AS IVSite
	ON IVSite.SiteCode = pw.Huc_10_SiteCode
	JOIN
	NCDCSiteXRef AS TADSite
	ON TADSite.OldSiteCode= pw.Precip_Gage_SiteCode
	
		
--StreamFlow
INsert Into #temp1		
	Select 	
	--*
	  TadSite.SiteID AS SiteIDTAD,
      TadSite.SiteCode AS SiteCodeTAD,
      CASE 
		WHEN sw.SiteType LIKE '%Reservoir%' THEN 20 --Month:23 BiMonth:20	
		WHEN sw.SiteType LIKE '%Gage%' THEN 4	--Month:5 BiMonth:4	
	  End AS VariableIDTAD,
      CASE 
		WHEN sw.SiteType LIKE '%Reservoir%' THEN 'STORDelta-P0.5M'
		WHEN sw.SiteType LIKE '%Gage%' THEN '00060-P0.5M'		
	  End AS VariableCodeTAD,
      CASE 
		WHEN sw.SiteType LIKE '%Reservoir%' THEN 7
		WHEN sw.SiteType LIKE '%Gage%' THEN 5		
	  End AS MethodIDTAD,
      2 AS QualityControlLevelIDTAD,
      3 AS SourceIDTAD,
      
      IVSite.SiteID AS SiteIDIV,
      IVSite.SiteCode AS SiteCodeIV,     
      30 AS VariableIDIV,--Month:32 BiMonth:30(have to convert to inches)	 
      '00060-P0.5M-UR' AS VariableCodeIV,      
      8 AS MethodIDIV,
      3 AS QualityControlLevelIDIV,
      3 AS SourceIDIV,      
      
      1 AS FormulaNo,
	  sw.Weight AS Weight,	 	
	  CASE 
		WHEN sw.SiteType LIKE '%Reservoir%' THEN 'USBR-Reservoir'
		WHEN sw.SiteType LIKE '%Gage%' THEN 'USGS-Streamflow'		
	  End AS SiteType	
     FROM StreamFlowWeights as sw
     LEFT JOIN	
	OPENDATASOURCE(
     'SQLOLEDB',
	 'Data Source=drought.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
	).Summary.dbo.Sites	AS IVSite
	ON IVSite.SiteCode = sw.Huc_10_SiteCode
	LEFT JOIN
	OPENDATASOURCE(
     'SQLOLEDB',
	 'Data Source=drought.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
	).Summary.dbo.Sites	AS TADSite
	ON TADSite.SiteCode= sw.SiteCode
	
	order by SiteIDIV
	
	
	--Update Tester.dbo.StreamFlowWeights
	--Set SiteCode = 
	--	Case 
	--		When SiteType Like '%Gage%' Then  '0'+SiteCode
	--		else SiteCode 
	--	end
	
	
--Reservoir Storage
INsert Into #temp1
	Select 
		--*	
	  TadSite.SiteID AS SiteIDTAD,
      TadSite.SiteCode AS SiteCodeTAD,
      13 AS VariableIDTAD,--Month:14 BiMonth:13
      'STOR-P0.5M' AS VariableCodeTAD,
      11 AS MethodIDTAD,
      2 AS QualityControlLevelIDTAD,
      3 AS SourceIDTAD,      
      
      IVSite.SiteID AS SiteIDIV,
      res.HUC10 AS SiteCodeIV,
      13 AS VariableIDIV,--Month:14 BiMonth:13
      'STOR-P0.5M' AS VariableCodeIV,
      13 AS MethodIDIV,
      3 AS QualityControlLevelIDIV,
      3 AS SourceIDIV,
      
      1 AS FormulaNo,
	  1 AS Weight,
	 'USBR-Reservoir' AS SiteType 
	  
     FROM Reservoirs_UCRB as res
     LEFT JOIN	
	OPENDATASOURCE(
     'SQLOLEDB',
	 'Data Source=drought.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
	).Summary.dbo.Sites	AS IVSite
	ON IVSite.SiteCode = res.HUC10
	LEFT JOIN
	OPENDATASOURCE(
     'SQLOLEDB',
	 'Data Source=drought.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
	).Summary.dbo.Sites	AS TADSite
	ON TADSite.SiteCode= res.SiteCode	
	order by SiteIDIV
	
	
	
	
	----------------------------------------------------Monthly----------------------------------------------------------------------------------
	
	
	--Precipitation
	INsert Into #temp1
	Select
      TadSite.SiteID AS SiteIDTAD,
      TadSite.SiteCode AS SiteCodeTAD,
      29 AS VariableIDTAD,--Month:29 BiMonth:28
      'PRCP-P1M' AS VariableCodeTAD,
      6 AS MethodIDTAD,
      2 AS QualityControlLevelIDTAD, 	
	  3 AS SourceIDTAD,
      
      IVSite.SiteID AS SiteIDIV,
      IVSite.SiteCode AS SiteCodeIV, 
      29 AS VariableIDIV,--Month:29 BiMonth:28
      'PRCP-P1M' AS VariableCodeIV,
      9 AS MethodIDIV,
      3 AS QualityControlLevelIDIV,
      3 AS SourceIDIV,
      
      1 AS FormulaNo,
	  Precip_Gage_Weight AS Weight,
	  'NCDC-Weather' AS SiteType
	  
     FROM PrecipWeights as pw
      JOIN	
	OPENDATASOURCE(
     'SQLOLEDB',
	 'Data Source=drought.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
	).Summary.dbo.Sites	AS IVSite
	ON IVSite.SiteCode = pw.Huc_10_SiteCode
	JOIN
	NCDCSiteXRef AS TADSite
	ON TADSite.OldSiteCode= pw.Precip_Gage_SiteCode
	
	
--StreamFlow
INsert Into #temp1		
	Select 	
	--*
	  TadSite.SiteID AS SiteIDTAD,
      TadSite.SiteCode AS SiteCodeTAD,
      CASE 
		WHEN sw.SiteType LIKE '%Reservoir%' THEN 23 --Month:23 BiMonth:20	
		WHEN sw.SiteType LIKE '%Gage%' THEN 5	--Month:5 BiMonth:4	
	  End AS VariableIDTAD,
      CASE 
		WHEN sw.SiteType LIKE '%Reservoir%' THEN 'STORDelta-P1M'
		WHEN sw.SiteType LIKE '%Gage%' THEN '00060-P1M'		
	  End
       AS VariableCodeTAD,
      CASE 
		WHEN sw.SiteType LIKE '%Reservoir%' THEN 7
		WHEN sw.SiteType LIKE '%Gage%' THEN 5		
	  End
       AS MethodIDTAD,
      2 AS QualityControlLevelIDTAD,
      3 AS SourceIDTAD,
      
      
      IVSite.SiteID AS SiteIDIV,
      IVSite.SiteCode AS SiteCodeIV,     
      32 AS VariableIDIV,--Month:32 BiMonth:30(have to convert to inches)	 
      '00060-P1M-UR' AS VariableCodeIV,      
      8 AS MethodIDIV,
      3 AS QualityControlLevelIDIV,
      3 AS SourceIDIV,
      
      1 AS FormulaNo,
	  sw.Weight AS Weight,
	 	
	  CASE 
		WHEN sw.SiteType LIKE '%Reservoir%' THEN 'USBR-Reservoir'
		WHEN sw.SiteType LIKE '%Gage%' THEN 'USGS-Streamflow'		
	  End AS SiteType		  
	
     FROM StreamFlowWeights as sw
     LEFT JOIN	
	OPENDATASOURCE(
     'SQLOLEDB',
	 'Data Source=drought.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
	).Summary.dbo.Sites	AS IVSite
	ON IVSite.SiteCode = sw.Huc_10_SiteCode
	LEFT JOIN
	OPENDATASOURCE(
     'SQLOLEDB',
	 'Data Source=drought.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
	).Summary.dbo.Sites	AS TADSite
	ON TADSite.SiteCode= sw.SiteCode
	
	order by SiteIDIV
	
	--Monthlu
	
	
--Reservoir Storage
INsert Into #temp1
	Select 
		--*	
	  TadSite.SiteID AS SiteIDTAD,
      TadSite.SiteCode AS SiteCodeTAD,
      14 AS VariableIDTAD,--Month:14 BiMonth:13
      'STOR-P1M' AS VariableCodeTAD,
      11 AS MethodIDTAD,
      2 AS QualityControlLevelIDTAD,
      3 AS SourceIDTAD,      
      
      IVSite.SiteID AS SiteIDIV,
      res.HUC10 AS SiteCodeIV,
      14 AS VariableIDIV,--Month:14 BiMonth:13
      'STOR-P1M' AS VariableCodeIV,
      13 AS MethodIDIV,
      3 AS QualityControlLevelIDIV,
      3 AS SourceIDIV,
      
      1 AS FormulaNo,
	  1 AS Weight,
	 'USBR-Reservoir' AS SiteType 
	 	
     FROM Reservoirs_UCRB as res
     LEFT JOIN	
	OPENDATASOURCE(
     'SQLOLEDB',
	 'Data Source=drought.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
	).Summary.dbo.Sites	AS IVSite
	ON IVSite.SiteCode = res.HUC10
	LEFT JOIN
	OPENDATASOURCE(
     'SQLOLEDB',
	 'Data Source=drought.uwrl.usu.edu;Initial Catalog = Summary;User ID=NIDIS;Password=N1d1s!'
	).Summary.dbo.Sites	AS TADSite
	ON TADSite.SiteCode= res.SiteCode	
	order by SiteIDIV
	
	SELECT * FROM #temp1
	where SiteIDIV=9749 ANd VariableIDIV =30
	
	
	--drop table #temp1