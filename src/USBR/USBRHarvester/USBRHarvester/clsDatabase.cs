using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace USBRHarvester
{
    class clsDatabase : ReservoirsCatalogEntities
    {
        public int saveSeries(Variable variable, Site site, DateTime beginDT, DateTime endDT, int valCount, string vunitname)
        {
            SeriesCatalog sc;            
            try
            {
                sc = (from SC in this.SeriesCatalog where SC.VariableName == variable.VariableName && SC.SiteCode == site.SiteCode select SC).First();

                //sc.BeginDateTime = beginDT;
                //sc.BeginDateTimeUTC = beginDT.ToUniversalTime();

                
                sc.EndDateTime = endDT;
                sc.EndDateTimeUTC = endDT.ToUniversalTime();
                TimeSpan val = sc.EndDateTime.Value.Subtract(sc.BeginDateTime.Value);
                sc.ValueCount = val.Days;//sc.ValueCount + valCount;
                this.SaveChanges();
            }
            catch(Exception ex)
            {

                Sites s = (from S in this.Sites where S.SiteCode == site.SiteCode select S).First();
                Variables v = (from V in this.Variables where V.VariableName == variable.VariableName select V).First();
                Methods m = (from M in this.Methods where M.MethodID == 0 select M).First();
                Sources so = (from S in this.Sources where S.SourceID == 1 select S).First();
                QualityControlLevels qcl = (from Q in this.QualityControlLevels where Q.QualityControlLevelID == -9999 select Q).First();
                Units tU = (from U in this.Units where U.UnitsID == 104 select U).First();
                Units vU = (from U in this.Units where U.UnitsName == vunitname select U).First(); ;
                sc = new SeriesCatalog();                
                sc.SiteID = s.SiteID;
                sc.SiteCode = s.SiteCode;
                sc.SiteName = s.SiteName;
                sc.VariableID = v.VariableID;
                sc.VariableName =v.VariableName;
                sc.VariableCode = v.VariableCode;
                sc.Speciation = v.Speciation;
                sc.VariableUnitsID = vU.UnitsID;//v.Units.UnitsID;
                sc.VariableUnitsName = vU.UnitsName;//v.Units.UnitsName;
                sc.SampleMedium = v.SampleMedium;
                sc.ValueType = v.ValueType;
                sc.TimeSupport = v.TimeSupport;
                sc.TimeUnitsID = tU.UnitsID;//v.Units1.UnitsID;
                sc.TimeUnitsName = tU.UnitsName;//v.Units1.UnitsName;
                sc.DataType = v.DataType;
                sc.GeneralCategory = v.GeneralCategory;
                sc.MethodID = m.MethodID;
                sc.MethodDescription = m.MethodDescription;
                sc.SourceID = so.SourceID;
                sc.Organization = so.Organization;
                sc.SourceDescription = so.SourceDescription;
                sc.Citation = so.Citation;
                sc.QualityControlLevelID = qcl.QualityControlLevelID;
                sc.QualityControlLevelCode = qcl.QualityControlLevelCode;
                sc.BeginDateTime = beginDT;
                sc.BeginDateTimeUTC = beginDT.ToUniversalTime() ;
                sc.EndDateTime = endDT;
                sc.EndDateTimeUTC = endDT.ToUniversalTime();
                sc.ValueCount = valCount;
                this.AddToSeriesCatalog(sc);
                this.SaveChanges();
            }
            return sc.SeriesID;
        }
        public int saveSeries(int siteID, string siteCode, string siteName, int varID, string varCode, string varName, string speciation, int varUnitsID,
            string varUnitsName, string sampleMedium, string valType, int timeSupport, int timeUnitsID, string timeUnitsName, string dataType, string generalCategory,
            int methodID, string methodDescription, int sourceID, string organization, string sourceDescription, string citation, int qclID,
            string qclCode, DateTime beginDT, DateTime endDT, DateTime beginDTUTC, DateTime endDTUTC, int valCount)
        {

            SeriesCatalog sc;
            try
            {
                sc = (from S in this.SeriesCatalog where S.SiteCode == siteCode && S.VariableCode == varCode && S.MethodID == methodID select S).First();
                sc.EndDateTime= endDT;
                sc.EndDateTimeUTC = endDTUTC;
                sc.ValueCount = sc.ValueCount + valCount;
                this.SaveChanges();
            }
            catch
            {
                sc = new SeriesCatalog();
                sc.SiteID = siteID;
                sc.SiteCode = siteCode;
                sc.SiteName = siteName;
                sc.VariableID = varID;
                sc.VariableName = varName;
                sc.VariableCode = varCode;
                sc.Speciation = speciation;
                sc.VariableUnitsID = varUnitsID;
                sc.VariableUnitsName = varUnitsName;
                sc.SampleMedium = sampleMedium;
                sc.ValueType = valType;
                sc.TimeSupport = timeSupport;
                sc.TimeUnitsID = timeUnitsID;
                sc.TimeUnitsName = timeUnitsName;
                sc.DataType = dataType;
                sc.GeneralCategory = generalCategory;
                sc.MethodID = methodID;
                sc.MethodDescription = methodDescription;
                sc.SourceID = sourceID;
                sc.Organization = organization;
                sc.SourceDescription = sourceDescription;
                sc.Citation = citation;
                sc.QualityControlLevelID = qclID;
                sc.QualityControlLevelCode = qclCode;
                sc.BeginDateTime = beginDT;
                sc.BeginDateTimeUTC = beginDTUTC;
                sc.EndDateTime = endDT;
                sc.EndDateTimeUTC = endDTUTC;
                sc.ValueCount = valCount;
                this.AddToSeriesCatalog(sc);
                this.SaveChanges();
            }
            return sc.SeriesID;
        }
        public int SeriesExists(string variableName, string siteCode)
        {
            try
            {
                return (int)(from SC in this.SeriesCatalog where SC.VariableName == variableName && SC.SiteCode == siteCode select SC.SiteID).First();
            }
            catch (Exception ex)
            {
                return -99;
            }

        }
        public DateTime getEndDate(int SID)
        {
            return (DateTime)(from SC in this.SeriesCatalog where SC.SeriesID == SID select SC.EndDateTime).First();
        }


    }
}
