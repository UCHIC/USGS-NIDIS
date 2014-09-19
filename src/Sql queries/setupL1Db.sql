
INSERT into [NCDC-L1].dbo.Units
SELECT UnitsName,UnitsType, UnitsAbbreviation 
FROM NCDC.dbo.Units
Where UnitsID = 318

INSERT into [NCDC-L1].dbo.Qualifiers
SELECT QualifierCode, QualifierDescription
FROM NCDC.dbo.Qualifiers

INSERT into [NWIS-L1].dbo.Sites
SELECT SiteCode, SiteName, Latitude, Longitude, LatLongDatumID, Elevation_m, VerticalDatum, LocalX, LocalY, LocalProjectionID, PosAccuracy_m, State,County, Comments
FROM Summary.dbo.Sites
where SiteType Like '%USGS%'

INSERT into [NWIS-L1].dbo.Variables
SELECT VariableCode, VariableName, Speciation, VariableUnitsID, SampleMedium, ValueType, IsRegular, TimeSupport, TimeUnitsID, DataType, GeneralCategory, NoDataValue
FROM Summary.dbo.Variables
Where VariableID = 6
     
INSERT into [USBR-L1].dbo.Methods
SELECT MethodDescription, MethodLink
FROM Summary.dbo.Methods
Where MethodID = 10 


INSERT into [USBR-L1].dbo.Sources
SELECT Organization, SourceDescription, SourceLink, ContactName, Phone, Email, Address, City, State, ZipCode, Citation, MetadataID
FROM Summary.dbo.Sources
Where SourceID = 4


