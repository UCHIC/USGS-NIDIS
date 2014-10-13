using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
//using Logger;
using System.Diagnostics;

namespace UpdateSnotel
{
    class clsDatabase : SNOTELEntities
    {
        
        //insert site
        public void insertSite(string siteCode, string siteName, double latitude, double longitude, int latLongDatumID, double Elevation_m, string VerticulDatum, string LocalX, string LocalY, string LocalProjectionID, string PosAccuracy_m, string State, string County, string Comments, double TimeZone, string Status){
            if (siteCode != null)
            {
                try
                {
                    Sites s = new Sites();
                    s.SiteCode = siteCode.Trim();
                    s.SiteName = siteName.Trim();
                    s.Latitude = latitude;
                    s.Longitude = longitude;
                    s.LatLongDatumID = latLongDatumID;
                    s.Elevation_m = Elevation_m;
                    s.VerticalDatum = null;
                    s.LocalX = null;
                    s.LocalY = null;
                    s.LocalProjectionID = null;
                    s.PosAccuracy_m = null;
                    s.State = State;
                    s.County = County;
                    s.Comments = null;
                    s.TimeZone = Convert.ToInt32(TimeZone);
                    s.Status = Status;
                    this.AddToSites(s);
                    this.SaveChanges();
                }
                catch (Exception e)
                {
                    //DBLogging.WriteLog(Properties.Settings.Default.projectName, "Error", "clsDatabase" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name, e.Message + "()" + " \nstate: " + State + ", siteName: " + siteName);
                }
            }
        }
        public void updateSite(int SID, string SiteCode, string SiteName, string State, double lat, double lng/*, double tz*/, double elev_m)
        {
            Sites s = (from r in this.Sites where r.SiteID == SID && r.SiteCode == SiteCode select r).First();
            s.Latitude = lat;
            s.Longitude = lng;
            //s.TimeZone = (int)tz;
            s.Elevation_m = elev_m;
            s.State = State;
            s.SiteName = SiteName;           
            this.SaveChanges();
        }
        //insert series    
        public void insertSeries(int siteID, string siteCode, string siteName, int variableID, string variableCode, string variableName, string speciation, int variableUnitsID, string variableUnitsName,string sampleMedium, string valueType , double  timeSupport, int timeUnitsID ,string timeUnitsName ,string dataType, string generalCategory, int methodID , string methodDescription,int sourceID,string organization,string sourceDescription, string citation,int qualityControlLevelID, string qualityControlLevelCode, DateTime beginDateTime, DateTime endDateTime, DateTime beginDateTimeUTC, DateTime endDateTimeUTC, int valueCount)
        {
            SeriesCatalog sc = new SeriesCatalog();
                       
            sc.SiteID = siteID;
            sc.SiteCode= siteCode;
            sc.SiteName = siteName;
            sc.VariableID = variableID;
            sc.VariableCode = variableCode;
            sc.VariableName= variableName;
            sc.Speciation= speciation;
            sc.VariableUnitsID=variableUnitsID;
            sc.VariableUnitsName= variableUnitsName;
            sc.SampleMedium=sampleMedium;
            sc.ValueType= valueType;
            sc.TimeSupport=timeSupport;
            sc.TimeUnitsID=timeUnitsID;
            sc.TimeUnitsName=timeUnitsName;
            sc.DataType=dataType;
            sc.GeneralCategory=generalCategory; 
            sc.MethodID=methodID;
            sc.MethodDescription=methodDescription;
            sc.SourceID=sourceID;
            sc.Organization=organization;
            sc.SourceDescription=sourceDescription; 
            sc.Citation= citation;
            sc.QualityControlLevelID=qualityControlLevelID;
            sc.QualityControlLevelCode=qualityControlLevelCode; 
            sc.BeginDateTime= beginDateTime;
            sc.EndDateTime=endDateTime;
            sc.BeginDateTimeUTC= beginDateTimeUTC; 
            sc.EndDateTimeUTC=endDateTimeUTC;
            sc.ValueCount = valueCount;
            this.AddToSeriesCatalog(sc);
            this.SaveChanges();
           
        }
        //update series 
        public void updateSeries(int siteID, int varID, DateTime beginDateTime, DateTime endDateTime, DateTime beginDateTimeUTC, DateTime endDateTimeUTC, int valueCount)
        {
            try
            {
                SeriesCatalog sc = (from r in this.SeriesCatalog where r.SiteID == siteID && r.VariableID == varID select r).First();

                Sites s = (from S in this.Sites where S.SiteID == siteID select S).First();
                //sc.SiteID = s.SiteID;
                sc.SiteCode = s.SiteCode;
                sc.SiteName = s.SiteName;
                
                sc.BeginDateTime = beginDateTime;
                sc.EndDateTime = endDateTime;
                sc.BeginDateTimeUTC = beginDateTimeUTC;
                sc.EndDateTimeUTC = endDateTimeUTC;
                sc.ValueCount = valueCount;
                this.SaveChanges();
            }
            catch (Exception e)
            {
                //DBLogging.WriteLog(Properties.Settings.Default.projectName, "Error", "clsDatabase" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", e.Message);

            }
        }
        //check allseries
        public bool isNewSeries(int id, int varID)
        {
            try
            {
                SeriesCatalog sc = (from r in this.SeriesCatalog where r.SiteID == id && r.VariableID == varID select r).First();
                return false;
            }
            catch //(Exception e)
            {
                return true;
            }            
        }        
        public bool isNewSite(string code) {
            try
            {
             Sites s=(from r in this.Sites where r.SiteCode == code select r).First();
             return false;
            }
            catch //(Exception e)
            {
                return true;
            }
        }      
         public int getSiteID(string code){
             Sites s = (from r in this.Sites where r.SiteCode == code select r).First();
             return s.SiteID;
         }
        public string getState(int siteID){
            return (from r in this.Sites where r.SiteID == siteID select r.State).First(); 
        }
        public string getSiteCode(int siteID){
            return (from r in this.Sites where r.SiteID == siteID select r.SiteCode).First();                  
        }
        public int? getTimeZone(int siteID){
            return (from r in this.Sites where r.SiteID == siteID select r.TimeZone).First();
        }
        public int? getVariableID(string varCode){
            return ( from r in this.Variables where r.VariableCode== varCode select r.VariableID).First();           
        }
        public string getSiteName(int siteID){
            return (from r in this.Sites where r.SiteID == siteID select r.SiteName).First();
        }
       
        public Sources getSourceData(int srcID){
            return (from s in this.Sources where s.SourceID == srcID select s).First();
        }
        public Variables getVariableInfo(int variableID){
            return (from v in this.Variables where v.VariableID == variableID select v).First();
        }
        public Units getUnits(int unitID){
            return (from u in this.Units where u.UnitsID == unitID select u).First();
        }        
        public Methods getMethod(int mID){
            return (from m in this.Methods where m.MethodID== mID select m).First();
        }
        public int getNumOfSites(){
            return (from s in this.Sites select s.SiteID).Max();
        }    
    }
}
