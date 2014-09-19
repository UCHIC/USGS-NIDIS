Update DataValues 
 Set DataValue = (Select DV.DataValue*.0039 
 from DataValues AS DV 
where DataValues.SiteID= DV.SiteID and 
	DataValues.SourceID= DV.SourceID and 
	DataValues.VariableID= DV.VariableID and 
	DataValues.MethodID= DV.MethodID and 
	DataValues.QualityControlLevelID=DV.QualityControlLevelID and
	DataValues.LocalDateTime=DV.LocalDateTime
	)	
where VariableID in(28,29) AND ValueID >-9999

--Select DataValue--*.0039 
--As DataValue,VariableID, SiteID, SourceID, MethodID, QualityControlLevelID 
--from Datavalues 
--where VariableID in(28,29) --AND ValueID >-9999
--order by datavalue
