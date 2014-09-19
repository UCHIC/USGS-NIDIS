using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;

namespace FillSnotel
{
    public class clsConnection
    {


        //Object for storing all nessecary data to access an ODM 1.1 Database

        //#Region " Properties ";
        //ServerAddress property: sets/gets the ServerAddress value for the connection

        string GetServerAddress()
        {
            return m_ServerAddress;
        }
        void SetServerAddress(string value)
        {
            m_ServerAddress = value;
            SetConnectionString();
        }


        //DBName property: sets/gets the Database Name value for the connection

        string GetDBName()
        {
            return m_DBName;
        }

        void SetDBName(string value)
        {
            m_DBName = value;
            SetConnectionString();
        }



        //Trusted property: sets/gets the Trusted Connection value for the connection

        bool GetTrusted()
        {
            return m_Trusted;
        }
        void SetTrusted(bool value)
        {
            m_Trusted = value;
            SetConnectionString();
        }


        //UserID property: sets/gets the User ID value for the connection

        string GetUserID()
        {
            return m_UserID;
        }
        void SetUserID(string value)
        {
            m_UserID = value;
            SetConnectionString();
        }


        //Password property: sets/gets the User Password value for the connection

        string GetPassword()
        {
            return m_Password;
        }
        void SetPassword(string value)
        {
            m_Password = value;
            SetConnectionString();

        }

        //Timeout property: sets/gets the Timeout value for the connection

        int GetTimeOut()
        {
            return m_Timeout;
        }

        void SetTimeout(int value)
        {
            m_Timeout = value;
            SetConnectionString();

        }

        //ConnectionString property: Readonly -> returns the Connection String created for the connection
        string GetConnectionString()
        {
            return m_ConnStr;

        }

        //#End Region

        //#Region " Database Constants "

        //#Region " Database Table Definitions "

        //Category _____ =      Definition
        //------------------------------
        //A _____        =      Automatically provided by the database
        //M _____        =      Mandatory
        //O _____        =      Optional
        //P _____        =      Programatically Derived


        //_ VB data type =      SQL server 2005 data type
        //------------------------------
        //_ Integer      =      int
        //_ Double       =      float
        //_ String: ###  =      nvarchar(###)
        //_ Date         =      datetime
        //_ Boolean      =      bit


        //M Integer: Primary Key ->  Automatically assigned int
        //Definitions from: CUAHSI Comunity Observations Data Model Working Design Specif(ications Document - Version 4

        //#Region " Categories "
        //Categories
        public const string db_tbl_Categories = "Categories";//Table Name
        public const string db_fld_CatVarID = "VariableID";//M Integer: Primary Key -> Unique ID for each Category entry{
        public const string db_fld_CatValue = "Value";//M Double -> Numeric Value
        public const string db_fld_CatDesc = "CategoryDescription";//M String: 255 -> Definition of categorical variable value
        //#End Region

        //#Region " DataValues -> formerly Values "
        //DataValues
        public const string db_tbl_DataValues = "DataValues";//Table Name
        public const string db_fld_ValID = "ValueID";//M Integer: Primary Key ->Unique ID for each Values entry{
        public const string db_fld_ValValue = "DataValue";//M Double -> The numeric value.  Holds the CategoryID for categorical data
        public const string db_fld_ValAccuracyStdDev = "ValueAccuracy";//O Double -> Estimated standard deviation
        public const string db_fld_ValDateTime = "LocalDateTime";//M Local date and time of the measurement
        public const string db_fld_ValUTCOffset = "UTCOffset";//M Offset in hours from UTC time
        public const string db_fld_ValUTCDateTime = "DateTimeUTC";//M UTC date and time of the measurement
        public const string db_fld_ValSiteID = "SiteID";//M Integer -> Linked to Sites.SiteID
        public const string db_fld_ValVarID = "VariableID";//M Integer -> Linked to Variables.VariableID
        public const string db_fld_ValOffsetValue = "OffsetValue";//O Double -> distance from a datum/control point at which the value was observed
        public const string db_fld_ValOffsetTypeID = "OffsetTypeID";//O Integer -> Linked to OffsetTypes.OffsetTypeID
        public const string db_fld_ValCensorCode = "CensorCode";//O String: 50 -> CV.  Whether the data is censored
        public const string db_fld_ValQualifierID = "QualifierID";//O Integer -> Linked to Qualif(iers.Qualif(ierID
        public const string db_fld_ValMethodID = "MethodID";//M Integer -> Linked to Methods.MethodID
        public const string db_fld_ValSourceID = "SourceID";//M Integer -> Linked to Sources.SourceID
        public const string db_fld_ValSampleID = "SampleID";//O Integer -> Linked to Samples.SampleID
        public const string db_fld_ValDerivedFromID = "DerivedFromID";//O Integer -> Linked to DerivedFrom.DerivedFromID
        public const string db_fld_ValQCLevel = "QualityControlLevelID";//O Integer -> Linked to QualityControlLevels.QualityControlLevel

        //#Region " Values/Programmer Defined Variables "
        public const string db_val_ValCensorCode_lt = "'lt'";
        public const string db_outFld_ValDTMonth = "DateMonth";
        public const string db_outFld_ValDTYear = "DateYear";
        public const string db_outFld_ValDTDay = "DateDay";
        //Field names readable -> for Data Editing Tab Table
        public const string db_fldName_ValID = "Value ID";//M Integer: Primary Key ->Unique ID for each Values entry{
        public const string db_fldName_ValValue = "Data Value";//M Double -> The numeric value.  Holds the CategoryID for categorical data
        public const string db_fldName_ValAccuracyStdDev = "Value Accuracy";//O Double -> Estimated standard deviation
        public const string db_fldName_ValDateTime = "Local Date and Time";//M Local date and time of the measurement
        public const string db_fldName_ValUTCOffset = "UTC Offset";//M Offset in hours from UTC time
        public const string db_fldName_ValUTCDateTime = "UTC Date and Time";//M UTC date and time of the measurement
        public const string db_fldName_ValOffsetValue = "Offset Value";//O Double -> distance from a datum/control point at which the value was observed
        public const string db_fldName_ValOffsetTypeID = "Offset Type ID";//O Integer -> Linked to OffsetTypes.OffsetTypeID
        public const string db_fldName_ValCensorCode = "Censor Code";//O String: 50 -> CV.  Whether the data is censored
        public const string db_fldName_ValQualifierID = "Qualifier ID";//O Integer -> Linked to Qualif(iers.Qualif(ierID
        public const string db_fldName_ValSampleID = "Sample ID";//O Integer -> Linked to Samples.SampleID
        public const string db_fldName_ValDerivedFromID = "Derived From ID";//O Integer -> Linked to DerivedFrom.DerivedFromID
        //#End Region

        //#End Region

        //#Region " DerivedFrom "
        //DerivedFrom
        public const string db_tbl_DerivedFrom = "DerivedFrom";//Table Name
        public const string db_fld_DFID = "DerivedFromID";//M Integer -> Unique ID for each group of Derived From entries
        public const string db_fld_DFValueID = "ValueID";//M Integer -> Corresponds to the value id(s) the Derived Value came from

        //#Region " Values/Programmer Defined Variables "
        public const int db_val_DerivedFromID_Removed = -1;
        public const int db_val_DerivedFromID_Invalid = -2;
        //#End Region

        //#End Region

        //#Region " GroupDescriptions "
        //GroupDescriptions
        public const string db_tbl_GroupDesc = "GroupDescriptions";//Table Name
        public const string db_fld_GDGroupID = "GroupID";//M Integer: Primary Key -> Unique ID for each GroupDescriptions entry{
        public const string db_fld_GDDesc = "GroupDescription";//O String: 255 -> Text description of the group
        //#End Region

        //#Region " Groups "
        //Groups 
        public const string db_tbl_Groups = "Groups";//Table Name
        public const string db_fld_GroupID = "GroupID";//M Integer -> Unique ID for each group of Values
        public const string db_fld_GroupValueID = "ValueID";//M Integer -> Corresponds to the value id of each value in the group
        //#End Region

        //#Region " ISOMetaData "
        //ISOMetaData
        public const string db_tbl_ISOMetaData = "ISOMetaData";//Table Name
        public const string db_fld_IMDMetaID = "MetaDataID";//M Integer: Primary Key -> Unique ID for each ISOMetaData entry{
        public const string db_fld_IMDTopicCat = "TopicCategory";//M String: 50 -> Topic category keyword that gives the broad ISO19115 metadata topic category for data from this source.  CV
        public const string db_fld_IMDTitle = "Title";//M String: 255 -> Title of data from a specif(ic data source
        public const string db_fld_IMDAbstract = "Abstract";//M String: 255 -> Abstract of data from a specif(ic data source
        public const string db_fld_IMDProfileVs = "ProfileVersion";//M String: 50 -> Abstract of data from a specif(ic data source
        public const string db_fld_IMDMetaLink = "MetadataLink";//O String: H -> Link to additional metadata reference material
        //#End Region

        //#Region " LabMethods "
        //LabMethods
        public const string db_tbl_LabMethods = "LabMethods";//Table Name
        public const string db_fld_LMID = "LabMethodID";//M Integer: Primary Key -> Unique ID for each LabMethods entry{
        public const string db_fld_LMLabName = "LabName";//M String: 255 -> Name of the laboratory responsible for processing the sample
        public const string db_fld_LMLabOrg = "LabOrganization";//M String: 255 -> Organization responsible for sample analysis
        public const string db_fld_LMName = "LabMethodName";//M String: 255 -> Name of the method and protocols used for sample analysis
        public const string db_fld_LMDesc = "LabMethodDescription";//M String: 255 -> Description of the method and protocols used for sample analysis
        public const string db_fld_LMLink = "LabMethodLink";//O String: H -> Link to additional reference material to the analysis method
        //#End Region

        //#Region " Methods "
        //Methods
        public const string db_tbl_Methods = "Methods";//Table Name
        public const string db_fld_MethID = "MethodID";//M Integer: Primary Key -> Unique ID for each Methods entry{
        public const string db_fld_MethDesc = "MethodDescription";//M String: 255 -> Text descriptionof each method including Quality Assurance and Quality Control procedures
        public const string db_fld_MethLink = "MethodLink";//O String: H -> Link to additional reference material on the method
        //#End Region

        //#Region " OffsetTypes "
        //OffsetTypes
        public const string db_tbl_OffsetTypes = "OffsetTypes";//Table Name
        public const string db_fld_OTID = "OffsetTypeID";//M Integer: Primary Key ->Unique ID for each OffsetTypes entry{ 
        public const string db_fld_OTUnitsID = "OffsetUnitsID";//M Integer -> Linked to Units.UnitsID
        public const string db_fld_OTDesc = "OffsetDescription";//M String: 255 -> Full text description of the offset type
        //#End Region

        //#Region " Qualif(iers "
        //Qualif(iers
        public const string db_tbl_Qualifiers = "Qualifiers";//Table Name
        public const string db_fld_QlfyID = "QualifierID";//M Integer: Primary Key -> Unique ID for each Qualif(iers entry{
        public const string db_fld_QlfyCode = "QualifierCode";//O String: 50 -> Text code used by organization that collects the data
        public const string db_fld_QlfyDesc = "QualifierDescription";//M String: 255 -> Text of the data qualif(ying comment
        //#End Region

        //#Region " QualityControlLevels "
        //QualityControlLevels
        public const string db_tbl_QCLevels = "QualityControlLevels";//'Table Name
        public const string db_fld_QCLQCLevel = "QualityControlLevelID";//M Integer: Primary Key -> Pre-defined ID from 0 to 5
        public const string db_fld_QCLDefinition = "Definition";//M String: 255 -> Definition of Quality Control Level
        public const string db_fld_QCLExplanation = "Explanation";//M String: 500 -> Explanation of Quality Control Level

        //#Region " DB Loaded Constants "
        // public string db_val_QCLDef_Level(,);//Pre-loaded Quality control level definitions for each ID
        //#End Region

        //#End Region

        //#Region " Samples "
        //Samples
        public const string db_tbl_Samples = "Samples";//Table Name
        public const string db_fld_SampleID = "SampleID";//M Integer: Primary Key -> Unique ID for each Samples entry{
        public const string db_fld_SampleType = "SampleType";//M String: 50 -> CV specif(ying the sample type
        public const string db_fld_SampleLabCode = "LabSampleCode";//M String: 50 -> Code or label used to identif(y and track lab sample/sample-container (e.g. bottle) during lab analysis
        public const string db_fld_SampleMethodID = "LabMethodID";//M Integer -> Linked to LabMethods.LabMethodID
        //#End Region

        //#Region " SeriesCatalog "
        //SeriesCatalog
        public const string db_tbl_SeriesCatalog = "SeriesCatalog";//Table Name
        public const string db_fld_SCSeriesID = "SeriesID";//P Integer: Primary Key -> Unique ID for each SeriesCatalog entry{
        public const string db_fld_SCSiteID = "SiteID";//P Integer -> Linked to Sites.SiteID
        public const string db_fld_SCSiteCode = "SiteCode";//P String: 50 -> Site Identif(ier used by organization that collects the data
        public const string db_fld_SCSiteName = "SiteName";//P String: 255 -> Full text name of sampling location
        public const string db_fld_SCVarID = "VariableID";//P Integer -> Linked to Variables.VariableID
        public const string db_fld_SCVarCode = "VariableCode";//P String: 50 -> Variable identif(ier used by the organization that collects the data
        public const string db_fld_SCVarName = "VariableName";//P String: 255 -> Name of the variable from the variables table
        public const string db_fld_SCVarUnitsID = "VariableUnitsID";//P Integer -> Linked to Units.UnitsID
        public const string db_fld_SCVarUnitsName = "VariableUnitsName";//P String: 255 -> Full text name of the variable units from the UnitsName field in the Units Table
        public const string db_fld_SCSampleMed = "SampleMedium";//P String: 50 -> 
        public const string db_fld_SCValueType = "ValueType";//P String: 50 -> Text value indicating what type of value is being recorded
        public const string db_fld_SCTimeSupport = "TimeSupport";//P Double -> Numerical value that indicates the time support (or temporal footprint). 0 = instantaneous. otherwise = time over which values are averaged. 
        public const string db_fld_SCTimeUnitsID = "TimeUnitsID";//P Integer -> Linked to Units.UnitsID
        public const string db_fld_SCTimeUnitsName = "TimeUnitsName";//P String: 255 -> Full text name of the time support units from Units.UnitsName 
        public const string db_fld_SCDataType = "DataType";//P String: 50 -> CV. Data type that identif(ies the data as one of several types from the DataTypeCV.
        public const string db_fld_SCGenCat = "GeneralCategory";//P String: 50 -> CV. General category of the variable
        public const string db_fld_SCMethodID = "MethodID";//P Integer -> Corresponds to the ID of the Method for the Series
        public const string db_fld_SCMethodDesc = "MethodDescription";//P String: 255 -> Corresponds to the Method Description for the Series
        public const string db_fld_SCQCLevel = "QualityControlLevelID";  //P Integer -> Corresponds to the Quality Control Level of the Series
        public const string db_fld_SCSourceID = "SourceID";//P Integer -> Corresponds to the ID of the Source for the Series
        public const string db_fld_SCOrganization = "Organization";//P String: 50 -> Corresponds to the Organization for the Series
        public const string db_fld_SCSourceDesc = "SourceDescription";//P String: 255 -> Corresponds to the Source Description for the Series
        public const string db_fld_SCBeginDT = "BeginDateTime";//P Date -> Date and time of the first value in the series
        public const string db_fld_SCEndDT = "EndDateTime";//P Date -> Date and time of the first value in the series
        public const string db_fld_SCBeginDTUTC = "BeginDateTimeUTC";//P DateTime -> The First UTC Date
        public const string db_fld_SCEndDTUTC = "EndDateTimeUTC";//P DateTime -> The Last UTC Date
        public const string db_fld_SCValueCount = "ValueCount";//P Integer -> The number of vaues in the series (SiteID + VariableID)
        //#End Region

        //#Region " Sites "
        //   //Sites
        public const string db_tbl_Sites = "Sites";//Table Name
        //    public Const db_fld_SiteID As String = "SiteID"//M Integer: Primary Key -> Unique ID for each Sites entry{
        //    public Const db_fld_SiteCode As String = "SiteCode"//O String: 50 -> Code used by organization that collects the data
        //    public Const db_fld_SiteName As String = "SiteName"//O String: 255 -> Full name of sampling location
        //    public Const db_fld_SiteLat As String = "Latitude"//M Double -> Latitude in degrees w/ Decimals
        //    public Const db_fld_SiteLong As String = "Longitude" //M Double -> Longitude in degrees w/ Decimals
        //    public Const db_fld_SiteLatLongDatumID As String = "LatLongDatumID" //M Integer -> Linked to SpatialReferences.SpatialReferenceID
        //    public Const db_fld_SiteElev_m As String = "Elevation_m" //M Double -> Elevation of sampling location in meters.  
        //    public Const db_fld_SiteVertDatum As String = "VerticalDatum" //M String: 50 -> CV. Vertical Datum 
        //    public Const db_fld_SiteLocX As String = "LocalX" //O Double -> Local Projection X Coordinate
        //    public Const db_fld_SiteLocY As String = "LocalY" //O Double -> Local Projection Y Coordinate
        //    public Const db_fld_SiteLocProjID As String = "LocalProjectionID" //O Integer -> Linked to SpatialReferences.SpatialReferenceID
        //    public Const db_fld_SitePosAccuracy_m As String = "PosAccuracy_m" //O Double -> Value giving the acuracy with which the positional information is specif(ied.  in meters
        //    public Const db_fld_SiteState As String = "State" //O String: 50 -> Name of state in which the sampling station is located
        //    public Const db_fld_SiteCounty As String = "County" //O String: 50 -> Name of County in which the sampling station is located
        //    public Const db_fld_SiteComments As String = "Comments" //O String: 500 -> Comments related to the site
        //#End Region

        //#Region " Sources "
        //Sources
        public const string db_tbl_Sources = "Sources"; //Table Name
        public const string db_fld_SrcID = "SourceID"; //M Integer: Primary Key -> Unique ID for each Sources entry{
        public const string db_fld_SrcOrg = "Organization"; //M String: 50 -> Name of organization that collected the data itself.  not who held it
        public const string db_fld_SrcDesc = "SourceDescription"; //M String: 255 -> Full text description of the source of the data
        public const string db_fld_SrcLink = "SourceLink"; //M String: H -> Link to original data m_viewtable and associated metadata stored in the digital library or URL of data source
        public const string db_fld_SrcContactName = "ContactName"; //M String: 50 -> Name of Contact Person for data source
        public const string db_fld_SrcPhone = "Phone"; //M String: 50 -> Phone number for contact person
        public const string db_fld_SrcEmail = "Email"; //M String: 50 -> email address for contact person
        public const string db_fld_SrcAddress = "Address"; //M String: 255 -> Address for contact person
        public const string db_fld_SrcCity = "City"; //M String: 50 -> city for contact person
        public const string db_fld_SrcState = "State"; //M String: 50 -> state for contact person. 2 letter abreviations for "state, US", give full name for other countries
        public const string db_fld_SrcZip = "ZipCode"; //M String: 50 -> US zip code or country{ postal code
        public const string db_fld_SrcMetaID = "MetaDataID"; //M Integer -> ISOMetaData.MetaDataID
        //#End Region

        //#Region " SpatialReferences "
        //SpatialReferences
        public const string db_tbl_SpatialRefs = "SpatialReferences"; //Table Name
        public const string db_fld_SRID = "SpatialReferenceID"; //M Integer: Primary Key -> Unique ID for each SpatialReferences entry{
        public const string db_fld_SRSRSID = "SRSID"; //O Integer -> ID for Spatial Reference System @ http://epsg.org/
        public const string db_fld_SRSRSName = "SRSName"; //M String: 255 -> Name of spatial reference system
        public const string db_fld_SRIsGeo = "IsGeographic"; //M Boolean -> Whether it uses geographic coordinates (Lat., Long.)
        public const string db_fld_SRNotes = "Notes"; //O String: 500 -> Descriptive information about reference system
        //#End Region

        //#Region " Units "
        //Units
        public const string db_tbl_Units = "Units";//Table Name
        public const string db_fld_UnitsID = "UnitsID"; //M Integer: Primary Key -> Unique ID for each Units entry{
        public const string db_fld_UnitsName = "UnitsName"; //M String: 255 -> Full name of the units
        public const string db_fld_UnitsType = "UnitsType"; //M String: 50 -> Dimensions of the units
        public const string db_fld_UnitsAbrv = "UnitsAbbreviation"; //M String: 50 -> Abbreviation for the units

        //#Region " Values/Programmer Defined Variables "
        public const string db_val_UnitsTimeSupport_MilliSecond = "millisecond";
        public const string db_val_UnitsTimeSupport_Second = "second";
        public const string db_val_UnitsTimeSupport_Minute = "minute";
        public const string db_val_UnitsTimeSupport_Hour = "hour";
        public const string db_val_UnitsTimeSupport_Day = "day";
        //#End Region

        //#End Region

        //#Region " Variables "
        //    //Variables
        public const string db_tbl_Variables = "Variables"; //Table Name
        //    public Const db_fld_VarID As String = "VariableID" //M Integer: Primary Key -> Unique ID for each Variables entry{
        //    public Const db_fld_VarCode As String = "VariableCode" //O String: 50 -> Code used by the organization that collects the data
        //    public Const db_fld_VarName As String = "VariableName" //M String: 255 -> CV. Name of the variable that was measured/observed/modeled
        //    public Const db_fld_VarUnitsID As String = "VariableUnitsID" //O Integer -> Linked to Units.UnitsID
        //    public Const db_fld_VarSampleMed As String = "SampleMedium" //M String: 50 -> CV. The medium of the sample
        //    public Const db_fld_VarValueType As String = "ValueType" //M String: 50 -> CV. Text value indicating what type of value is being recorded
        //    public Const db_fld_VarIsRegular As String = "IsRegular" //M Boolean -> Whether the values are from a regularly sampled time series
        //    public Const db_fld_VarTimeSupport As String = "TimeSupport" //M Double -> Numerical value indicating the temporal footprint over which values are averaged.  0 = instantaneous
        //    public Const db_fld_VarTimeUnitsID As String = "TimeUnitsID" //O Integer -> Linked to Units.UnitsID
        //    public Const db_fld_VarDataType As String = "DataType" //M STring: 50 -> CV. text value that identif(ies the data as one of several types
        //    public Const db_fld_VarGenCat As String = "GeneralCategory" //M STring: 50 -> CV. General category of the values
        //    public Const db_fld_VarNoDataVal As String = "NoDataValue" //M Double -> Numeric value used to encode no data values for this variable
        //    public Const db_fld_VarSpec As String = "Speciation"
        //#End Region

        //#End Region

        //#Region " Controlled Vocabulary Table Definitions "

        //table names
        public const string db_tbl_VariableNameCV = "VariableNameCV";
        public const string db_tbl_ValueTypeCV = "ValueTypeCV";
        public const string db_tbl_CensorCodeCV = "CensorCodeCV";
        public const string db_tbl_SampleMediumCV = "SampleMediumCV";
        public const string db_tbl_GeneralCategoryCV = "GeneralCategoryCV";
        public const string db_tbl_TopicCategoryCV = "TopicCategoryCV";
        public const string db_tbl_DataTypeCV = "DataTypeCV";
        public const string db_tbl_SampleTypeCV = "SampleTypeCV";
        public const string db_tbl_VerticalDatumCV = "VerticalDatumCV";

        //fields
        public const string db_fld_CV_Term = "Term";
        public const string db_fld_CV_Definition = "Definition";

        //#End Region

        //#End Region

        //#Region " Connection Variables "

        private string m_ServerAddress;   //The network address of the server
        private string m_DBName;      //The Name of the Database
        private bool m_Trusted;   //true if( using Windows Au){tication
        private string m_UserID;     //User Name for the connection
        private string m_Password;    //Password of the connection
        private int m_Timeout;    //Timeout of the connection
        private string m_ConnStr; //The actual connection string used by the DB connection

        //#End Region

        //#Region " Functions "

        //#Region " Connection String Functions "

        public clsConnection(string e_ServerAddress, string e_DBName, int e_Timeout, bool e_Trusted, string e_UserID, string e_Password)
        {
            //Create a new set of connection settings with the specif(ied parameters (if( any are specif(ied)
            m_ServerAddress = e_ServerAddress;
            m_DBName = e_DBName;
            m_Trusted = e_Trusted;
            m_Timeout = e_Timeout;
            m_UserID = e_UserID;
            m_Password = e_Password;

            //Regenerate the connection string
            SetConnectionString();
        }
        public clsConnection()
        {
            //Create a new set of connection settings with the specif(ied parameters (if( any are specif(ied)
            m_ServerAddress = "";
            m_DBName = "";
            m_Trusted = false;
            m_Timeout = 1;
            m_UserID = "";
            m_Password = "";

            //Regenerate the connection string
            SetConnectionString();
        }
        public bool IncrementTimeout()
        {
            //Increments the Timeout setting by 1 as long as it is <= 15
            //){ regenerates the conntection string 
            //Output:    returns true if( m_timeout is not too high
            if (m_Timeout <= 30)
            {
                m_Timeout = m_Timeout + 1;
                SetConnectionString();
                return true;
            }
            else
                return false;

        }

        private void SetConnectionString()
        {
            //Generates a connection string for use in accessing a database
            if (m_Trusted)
            {
                if (!(m_ServerAddress == "" | m_DBName == "" | m_Timeout <= 0))
                    m_ConnStr = "Data Source=" + m_ServerAddress + ";Integrated Security=" + "true" + ";Connect Timeout=" + m_Timeout + ";Initial Catalog=" + m_DBName + ";MultipleActiveResultSets=true";
                else
                    m_ConnStr = "";

            }
            else
            {
                if (!(m_ServerAddress == "" | m_DBName == "" | m_Timeout <= 0 | m_UserID == "" | m_Password == ""))
                    m_ConnStr = "Data Source=" + m_ServerAddress + ";User ID=" + m_UserID + ";Password=" + m_Password + ";Connect Timeout=" + m_Timeout + ";Initial Catalog=" + m_DBName + ";MultipleActiveResultSets=true";
                else
                    m_ConnStr = "";

            }
        }



        //#End Region

        //#Region " Database Functions "
        DateTime stTime = new DateTime();
        DateTime endTime = new DateTime();
        public DataTable OpenTable(SqlConnection conn, SqlTransaction trans, string tableName, string sqlQuery)
        {
            //returns a dataTable of the query data.
            //Inputs:  tablename -> name of the table
            //         SqlQuery -> sql Query to retreive the data with
            //         connString -> the connection String for the database to connect to, to retreive the data from
            //Outputs: the dataTable of data retreived from the database using SqlQuery
            //create a flow table
            DataTable table = new DataTable(tableName); //the table of data to return
            SqlDataAdapter dataAdapter;//the dataAdapter to fill the table
            try
            {
                //connect to the Database
                dataAdapter = new SqlDataAdapter(sqlQuery, conn);
                dataAdapter.SelectCommand.Transaction = trans;

                //get the table from the database
                // stTime = DateTime.Now
                dataAdapter.Fill(table);

                //endTime = DateTime.Now
                // writer.WriteLine(tableName + " time taken: " + (endTime - stTime).ToString()) //Testing Only!
                //MsgBox(tableName + " time taken: " + (endTime - stTime).ToString()) //Testing Only!

                dataAdapter = null;
                return table;
            }
            catch (System.Exception ex)
            {

                //endTime = DateTime.Now
                //writer.WriteLine(tableName + " In } catch(-time taken: " + (endTime - stTime).ToString()) //Testing Only!
                //MsgBox(tableName + " In } catch(-time taken: " + (endTime - stTime).ToString()) //Testing Only!

                table = null;
                dataAdapter = null;
                //if( the connection timed out, increment the timeout and resave the settings. ){ try{{ to open the table again.
                if ((ex.Message.ToLower()).Contains("timeout"))
                {
                    if (IncrementTimeout())
                    {
                        //My.Settings.Timeout = settings.Timeout
                        //My.Settings.Save()
                        return OpenTable(conn, trans, tableName, sqlQuery);
                    }
                    else
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("An Error occurred while opening the Table = " + tableName + ex.Message);
                }
            }

            return null;
        }

        public int UpdateTable(SqlConnection conn, SqlTransaction trans, /*byRef*/ DataTable table, string query)
        {
            //this function updates the database after new rows have been added to or existing rows have been edited in the dataTable
            //the datatable is the the dataTable that was used to add/edit the rows, query is the query used to create the original datatable
            //Inputs: dataTable -> the dataTable used to add/edit the row
            //        query -> the query used to create the original dataTable
            //        connectionString -> the connectionString to the database
            //Outputs: none
            int count;
            if (table == null)
                return -1;

            if (table.Rows.Count <= 0)
                return 0;

            if (conn.State != ConnectionState.Open)
                conn.Open();


            SqlDataAdapter updateAdapter; //updateAdapter -> finds out if( anything has been changed and marks the rows that need to be added -> used by the command builder
            SqlCommandBuilder commandBuilder; //CommandBuilder -> creates the insert function for updating the database

            try
            {
                //create the updateAdapter,commandBuilder
                updateAdapter = new SqlDataAdapter(query, conn);
                updateAdapter.SelectCommand.Transaction = trans;
                commandBuilder = new SqlCommandBuilder(updateAdapter);
                if (table.Columns[0].ColumnName.EndsWith("ID") & (table.Columns[0].ColumnName != "GroupID") & (table.Columns[0].ColumnName != "DerivedFromID"))
                {
                    SqlCommand insert = commandBuilder.GetInsertCommand();
                    SqlCommand update = commandBuilder.GetUpdateCommand();
                    string text = query + " WHERE " + table.Columns[0].ColumnName + " = @@Identity";
                    commandBuilder.Dispose();
                    updateAdapter.InsertCommand = insert;
                    updateAdapter.InsertCommand.CommandText += text;
                    updateAdapter.UpdateCommand = update;
                    updateAdapter.UpdateCommand.CommandText += text;
                    updateAdapter.ContinueUpdateOnError = false;
                    updateAdapter.InsertCommand.UpdatedRowSource = UpdateRowSource.FirstReturnedRecord;

                    //update the database
                    count = updateAdapter.Update(table);
                }
                else
                {
                    updateAdapter.ContinueUpdateOnError = false;
                    //update the database
                    count = updateAdapter.Update(table);

                    table.Rows.Clear();

                    updateAdapter.Fill(table);
                }

                //updateAdapter.ContinueUpdateOnError = false
                ////update the database
                //count = updateAdapter.Update(table)

                //table.Rows.Clear()

                //updateAdapter.Fill(table)

                if ((count <= 0))
                {
                    return -1;
                }

            } //catch( ExitError ExEr){
            // throw ExEr;
            catch (Exception ex)
            {
                if (ex.Message.Contains("Violation of UNIQUE KEY constraint"))
                {
                    MessageBox.Show("One (or more) rows in " + table.TableName + " already exist in the database.");
                }
                else
                {
                    MessageBox.Show("Error in UpdateTable()\nMessage = " + ex.Message);
                }
                throw new Exception("clsConnection.UpdateTable(conn, trans, table, query)");
                //return -1;
            }

            //return the number of rows updated
            return count;
        }

        public bool TestDBConnection()
        {
            //Used to test a databse connection
            //Inputs:  Settings -> A ConnectionSettings instance used to create a connection to a database
            //Outputs: TestDBConnection -> returns true if( the test was successful, otherwise returns false

            //Create a new connection
            SqlConnection testConn = new SqlConnection(m_ConnStr); //Temporary connection settings to test
            string sql1; //SQL command to test DB with
            //dim SQL2 as string
            if (m_DBName == "" | m_ServerAddress == "")
                return false;
            else
            {
                try
                {

                    testConn.Open();

                    //Create an sql command that accesses all tables and a field within the series catalog table
                    sql1 = "SELECT MAX(VersionNumber) as CurrentVersion FROM ODMVersion";
                    //SQL2 = "SELECT MAX(value) as CurrentVersion FROM ::fn_listextendedproperty('ODM_version', NULL, NULL, NULL, NULL, NULL, NULL)"

                    //Test the connection
                    SqlDataAdapter VersTable = new SqlDataAdapter(sql1, testConn);
                    //Dim VersExPrp As New SqlClient.SqlDataAdapter(SQL2, TestConn)
                    DataTable Table= new DataTable();
                    //Dim ExPrp As New DataTable

                    VersTable.Fill(Table);
                    //VersExPrp.Fill(ExPrp)

                    testConn.Close();
                    testConn.Dispose();

                    if (Table.Rows.Count == 1)
                    {
                        if (Table.Rows[0]["CurrentVersion"].ToString().StartsWith("1.1"))
                        {
                            testConn.Close();
                            testConn.Dispose();
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("This program is only compatible with ODM 1.1 Databases.  You have an ODM " + Table.Rows[0]["VersionNumber"].ToString() + " Database");
                            testConn.Close();
                            testConn.Dispose();
                            return false;
                        }

                    }

                    else
                    {
                        MessageBox.Show("There was an error retrieving the Version Number of the ODM Database");
                        testConn.Close();
                        testConn.Dispose(); 
                        return false;
                    }

                    
                }
                catch (Exception ex)
                {
                    //if( the connection timed out, increment the timeout setting, return the results of another test, else{ return false
                    if (ex.Message.Contains("Timeout expired"))
                    {
                        if (IncrementTimeout())
                            return TestDBConnection();
                    }

                    else if (ex.Message.Contains("SQL Server does not exist"))
                        MessageBox.Show("Server Address Incorrect." + ex.Message);
                    else if (ex.Message.Contains("Cannot open database"))
                        MessageBox.Show("Database Name Incorrect." + ex.Message);
                    else if (ex.Message.Contains("Login failed for user"))
                        MessageBox.Show("Username or Password Incorrect." + ex.Message);
                    else if (ex.Message.Contains("Cannot open database requested in login '"))
                        MessageBox.Show("The requested database does not exist on that server.\nPlease enter the full server path.");
                    else if (ex.Message.Contains("Invalid object name '"))
                        MessageBox.Show("The requested database does not contain the correct tables.\nPlease enter a dif(ferent database name.");
                    else
                    {
                        MessageBox.Show("Unable to connect to Database");
                    }
                    return false;//after logging error
                }
                //return true;
            }
            //No Errors
           
        }

        public string FormatDateForQuery(DateTime d)
        {
            //Formats the date for a query string
            // Sql puts quotes and oledb puts # around the date
            //Inputs:  d -> the date to format
            //Outputs: the correctly formatted date

            //Format the Date for an SQL Database
            return "'" + d.ToString("MM-dd-yyy") + "'";

        }

        public string FormatStringForQuery(string value)
        {
            string formatted;
            formatted = "'" + value.Replace("'", "''") + "'";

            return formatted;
        }

        //#End Region

        //#End Region



    }
}

