

namespace DataSummary
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Text;

    public class ClsFromDB:SummaryEntities
    {
        private string conn; 

        public ClsFromDB()
            : base()
        {
            this.conn = ConfigurationManager.ConnectionStrings["SummaryEntities"].ConnectionString.Split('"')[1];
        }

        public ClsFromDB(string connectionString)
            : base(connectionString)
        {
            this.conn = connectionString.Split('"')[1];
        }

        public SeriesCatalog GetSeries(string siteCode, string variableCode)
        {
            try
            {
                return (from SC in this.SeriesCatalogs where SC.VariableCode == variableCode && SC.SiteCode == siteCode select SC).First();
            }
            catch (Exception ex)
            {
                throw new Exception("clsFromDB.getSeries(" + siteCode + ", " + variableCode + ")", ex);
            }
        }
        
        public int GetVariableID(string variableCode)
        {
            try
            {
                return (from V in this.Variables where V.VariableCode == variableCode select V.VariableID).First();
            }
            catch (Exception ex)
            {
                throw new Exception("clsFromDB.GetVariableID(" + variableCode + ")", ex);
            }
        }

        public SeriesCatalog GetSeries(int siteID, int variableID)
        {
            try
            {
                return (from SC in this.SeriesCatalogs where SC.VariableID == variableID && SC.SiteID == siteID select SC).First();
            }
            catch (Exception ex)
            {
                throw new Exception("clsFromDB.getSeries(" + siteID + ", " + variableID + ")", ex);
            }
        }

        public double GetNoDataValue(int variableID)
        {
            try
            {
                return (from v in this.Variables where v.VariableID == variableID select v.NoDataValue).First();
            }
            catch (Exception ex)
            {
                throw new Exception("clsFromDB.getNoDataValue(" + variableID + ")", ex);
            }
        }

        public double GetNoDataValue(string variableCode)
        {
            try
            {
                return (from v in this.Variables where v.VariableCode == variableCode select v.NoDataValue).First();
            }
            catch (Exception ex)
            {
                throw new Exception("clsFromDB.getNoDataValue(" + variableCode + ")", ex);
            }
        }

        public int GetMaxFormulaNum()
        {
            //return (from i in this.IVtoTADxRef select i.FormulaNo).Max();
            return 1;
        }        

        public DataTable CreateIndexVariableTable(int formulaNum)
        {
            DataTable values = new DataTable(formulaNum.ToString());
            string query = "SELECT NULL as ValueID, SUM(weight * DataValue) as DataValue, NULL as " +
                        "ValueAccuracy, MAX(LocalDateTime)as LocalDateTime,  MAX(UTCOffset) as UTCOffset, " +
                        "DateTimeUTC, SiteIDIV as SiteID, VariableIDIV as VariableID,NULL as OffsetValue, NULL as OffsetTypeID, " +
                        "MIN(CensorCode) as CensorCode,NULL as QualifierID,MethodIDIV as MethodID,SourceIDIV as SourceID, " +
                        "NULL as SampleID, NULL as DerivedFromID, QualityControlLevelIDIV as qualityControlLevelID \n" +
            "FROM IVtoTADxRef left join DataValues on " +
                            "SourceIDTAD = SourceID and " +
                            "SiteIDTAD = SiteID and " +
                            "VariableIDTAD = VariableID and " +
                            "MethodIDTAD = MethodID and  " +
                            "QualityControlLevelIDTAD = QualityControlLevelID " +

            "WHERE FormulaNo = " + formulaNum +
            " GROUP BY SourceIDIV, SiteIDIV, VariableIDIV, MethodIDIV, QualityControlLevelIDIV, DateTimeUTC";

            using (SqlConnection sqlConnection1 = new SqlConnection(this.conn))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    int rowsAffected;

                    cmd.CommandText = query;
                    cmd.Connection = sqlConnection1;
                    cmd.CommandTimeout = 9999;
                    sqlConnection1.Open();

                    rowsAffected = cmd.ExecuteNonQuery();
                    SqlDataReader reader = cmd.ExecuteReader();

                    values.Load(reader);
                    sqlConnection1.Close();
                }
            }

            /*       var LeftJoin = from IV in IVtoTADxRef
                                  join dv in DataValues
                                      on
                                  IV.SourceIDTAD
                                      //new { IV.SourceIDTAD, IV.SiteIDTAD, IV.VariableIDTAD, IV.MethodIDTAD, IV.QualityControlLevelIDTAD } 
                                  equals 
                                  dv.Sources.SourceID
                                      //new { dv.Sources.SourceID, dv.Sites.SiteID, dv.Variables.VariableID, dv.Methods.MethodID, dv.QualityControlLevels.QualityControlLevelID }
                                  into JoinedTable

                                  from joined in JoinedTable.DefaultIfEmpty()                           
                                  select new
                                  {                               
                                      SourceID = IV.SourceIDTAD,
                                      DataValueID = (joined != null ? joined.ValueID : int.MinValue)

                                      // Max(ValueID) as ValueID, SUM(weight * DataValue) as DataValue, NULL as ValueAccuracy, MAX(LocalDateTime)as LocalDateTime,  MAX(UTCOffset) as UTCOffset,
            //  DateTimeUTC, SiteIDIV, VariableIDIV,NULL as OffsetValue, NULL as OffsetTypeID,
             // MIN(CensorCode) as CensorCode, NULL as QualifierID, MethodIDIV,SourceIDIV,
             // NULL as SampleID, NULL as DerivedFromID, QualityControlLevelIDIV
                                  };
             where IV.FormulaNo = formulaNum
             group joined by new {IV.SourceIDIV, IV.SiteIDIV, IV.VariableIDIV, IV.MethodIDIV, IV.QualityContorlLevelIDIV, IV.DateTimeUTC }into grouped                            
            */
            /* values.LoadDataRow((createIndexVariables(formulaNum).ToArray()[0].ToString()), LoadOption.OverwriteChanges);//Pass array object to LoadDataRow method
            */
            /* return (from dv in DataValues where dv.Sites.SiteID== formulaNum select dv);*/
            /*return createIndexVariables(formulaNum).AsQueryable();*/
            return values;
        }
        

        public int GetSiteID(string siteCode)
        {
            try
            {
                return (from s in this.Sites where s.SiteCode == siteCode select s.SiteID).First();
            }
            catch (Exception ex)
            {
                throw new Exception("clsFromDB.getSiteID(" + siteCode + ")", ex);
            }
        }

        public Site GetSite(string siteCode)
        {
            try
            {
                return (from s in this.Sites where s.SiteCode == siteCode select s).First();
            }
            catch (Exception ex)
            {
                throw new Exception("clsFromDB.getSite(" + siteCode + ")", ex);
            }
        }

        public DataValue GetPreviousDataValue(AggregateSeries variable, int siteID, DateTime start)
        {
            try
            {
                return (from DV in this.DataValues where DV.Site.SiteID == siteID && DV.Variable.VariableID == variable.VariableID && DV.Method.MethodID == variable.MethodID && DV.LocalDateTime < start orderby DV.LocalDateTime descending select DV).First();
            }
            catch (Exception ex)
            {
                throw new Exception("clsFromDB.GetPreviousDataValue(" + variable + ", " + siteID + ", " + start + ")", ex);
            }
        }

        public DataValue GetBeginningofAccumEntry(DataValue dv)
        {
            int siteID, variableID, methodID;
            try
            {
                siteID = (int)dv.Site.SiteID;//SitesReference.EntityKey.EntityKeyValues[0].Value;
                variableID = (int)dv.Variable.VariableID;//sReference.EntityKey.EntityKeyValues[0].Value;
                methodID = (int)dv.Method.MethodID;// Reference.EntityKey.EntityKeyValues[0].Value;

                // if i am getting the beginning of the accumulated interval I have the date at the end of the interval so the PREVIOUS data, I need all of the datavalue information 
               return (from DV in this.DataValues where DV.Site.SiteID == siteID && DV.Variable.VariableID == variableID && DV.Method.MethodID == methodID && DV.LocalDateTime < dv.LocalDateTime orderby DV.LocalDateTime descending select DV).First();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Sequence"))
                {
                    DataValue newdv = new DataValue();
                    newdv.LocalDateTime = dv.LocalDateTime;
                    return newdv;
                }

                throw new Exception("clsFromDB.getBeginningofAccumEntry(" + dv.Site.SiteID+ ", " + dv.LocalDateTime + ", " + dv.Variable.VariableID + ")", ex);
            }
        }

        public DataValue GetEndOfAccumEntry(DataValue dv)
        {
            int siteID=0, variableID=0, methodID=0;
            try
            {
                siteID = (int)dv.Site.SiteID;//SitesReference.EntityKey.EntityKeyValues[0].Value;
                variableID = (int)dv.Variable.VariableID;//sReference.EntityKey.EntityKeyValues[0].Value;
                methodID = (int)dv.Method.MethodID;// Reference.EntityKey.EntityKeyValues[0].Value;

                // if i am getting the end of accumulated interval I have the date at the beginning of the interval so the NEXT data, I need all of the datavalue information 
                return (from DV in this.DataValues where DV.Site.SiteID == siteID && DV.Variable.VariableID == variableID && DV.Method.MethodID == methodID && DV.LocalDateTime > dv.LocalDateTime orderby DV.LocalDateTime select DV).First();
            }
            catch (Exception ex)
            {
                throw new Exception("clsFromDB.getEndOfAccumEntry(" + siteID + ", " + dv.LocalDateTime + ", " + variableID + ")", ex);
            }
        }

        public System.Linq.IQueryable<DataValue> GetListDataValues(int seriesID, DateTime startDate, DateTime endDate)
        {
            try
            {
                SeriesCatalog s = (from sc in this.SeriesCatalogs where sc.SeriesID == seriesID select sc).First();
                return from d in this.DataValues where d.Site.SiteID == s.SiteID && d.Variable.VariableID == s.VariableID && d.LocalDateTime >= startDate && d.LocalDateTime <= endDate orderby d.LocalDateTime select d;
            }
            catch (Exception ex)
            {
                throw new Exception("clsFromDB.GetListDataValues(" + seriesID + ", " + startDate + ", " + endDate + ")", ex);
            }
        }

        internal int GetVariableID(int p)
        {
            throw new NotImplementedException();
        }
    }
}
