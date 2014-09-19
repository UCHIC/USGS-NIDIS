using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;


namespace NCDC_Harvester
{
    public class sitedata
    {
        public sitedata(string sc, string sn, int si)
        {
            SiteCode = sc;
            SiteName = sn;
            SiteID = si;

        }
        public string SiteCode;
        public string SiteName;
        public int SiteID;
    }

    class clsDatabase : NCDCEntities
    {
        private string conn = ConfigurationManager.ConnectionStrings["NCDCEntities"].ConnectionString.Split('"')[1];
        

        //public List<Sites> getSiteList()
        public List<sitedata> getSiteList()
        {
            //return (from s in Sites select s).ToList();

            var query = from s in Sites
                        join sc in SeriesCatalog on s.SiteCode equals sc.SiteCode
                        where sc.EndDateTime > new DateTime(2012,04,01)
                        select new { s.SiteCode, s.SiteName, s.SiteID };
            List<sitedata> sites = new List<sitedata>();
            foreach (var group in query)
            {
                sites.Add(new sitedata(group.SiteCode, group.SiteName, group.SiteID));
            }
            return sites;     
       
        }
        public DateTime getSeriesLastDate(int seriesID)
        {
            return Convert.ToDateTime((from sc in SeriesCatalog where sc.SeriesID == seriesID select sc.EndDateTime).First());
        }
        public int getSeriesID(int siteID, int variableID)
        {
            try
            {
                int SID = (from sc in SeriesCatalog where sc.VariableID == variableID && sc.SiteID == siteID select sc.SeriesID).First();
                return SID;//Convert.ToInt32();
            }
            catch (Exception ex)
            {
                return -99;
            }
            
        }
        public Variables getVariableID(string variableCode)
        {
            return (from v in Variables where v.VariableCode == variableCode select v).First();
        }
        private DataTable createDVTable(string TableName)
        {
            DataTable values = new DataTable(TableName);
            values.Columns.Add("ValueID", typeof(SqlInt64));
            values.Columns.Add("DataValue", typeof(SqlDouble));
            values.Columns.Add("valAccuracy", typeof(SqlDouble));
            values.Columns.Add("LocalDT", typeof(SqlDateTime));
            values.Columns.Add("UTCOffset", typeof(SqlDouble));
            values.Columns.Add("DateUTC", typeof(SqlDateTime));            
            values.Columns.Add("siteID", typeof(SqlInt32));
            values.Columns.Add("varID", typeof(SqlInt32));
            values.Columns.Add("offsetVal", typeof(SqlDouble));
            values.Columns.Add("offsetTypeID", typeof(SqlInt32));
            values.Columns.Add("censorcode", typeof(SqlString));
            values.Columns.Add("qualifierID", typeof(SqlInt32));
            values.Columns.Add("methodID", typeof(SqlInt32));
            values.Columns.Add("sourceID", typeof(SqlInt32));
            values.Columns.Add("sampleID", typeof(SqlInt32));
            values.Columns.Add("derivedfromID", typeof(SqlInt32));
            values.Columns.Add("qclID", typeof(SqlInt32));
            return values;
        }
        public int saveDataValues(DataRow[] Rows, Variables variable, sitedata site)
        {
            DataTable values = createDVTable("DataValues");
            int count = 0;

            try
            {
                foreach (DataRow row in Rows)
                {
                    
                    DateTime localDateTime;

                    localDateTime = (DateTime)row["Date"];

                    double utcOffset = -7;
                    int qualifierID;
                    try
                    {
                        string qualCode = row["MFlag"].ToString() + row["QFlag"].ToString() + row["SFlag"].ToString();
                        qualCode=qualCode.Replace(' ', '_');
                        qualifierID = GetQualifierID(qualCode);
                    }
                    catch
                    {
                        qualifierID = -99;
                    }
                    DateTime dateTimeUTC = localDateTime.Add(new TimeSpan((int)utcOffset, 0, 0));
                    try
                    {
                        
                        SeriesCatalog sc = (from series in this.SeriesCatalog where series.SiteID == site.SiteID && series.VariableID == variable.VariableID select series).First();
                        values.Rows.Add(SqlInt64.Null, Convert.ToDouble(row["Value"]), SqlDouble.Null, localDateTime, utcOffset, dateTimeUTC, site.SiteID, variable.VariableID, SqlDouble.Null, SqlInt32.Null, "nc", ( qualifierID< 0 ? SqlInt32.Null : qualifierID), sc.MethodID, sc.SourceID, SqlInt32.Null, SqlInt32.Null, sc.QualityControlLevelID);
                        count++;
                    }
                    catch (Exception ex)
                    {
                                              
                        values.Rows.Add(SqlInt64.Null, Convert.ToDouble(row["Value"]), SqlDouble.Null, localDateTime, utcOffset, dateTimeUTC, site.SiteID, variable.VariableID, SqlDouble.Null, SqlInt32.Null, "nc", ( qualifierID< 0 ? SqlInt32.Null : qualifierID), 0, 1, SqlInt32.Null, SqlInt32.Null, 1);
                        count++; 
                    }
                }
                this.InsertBulk(values);
                
            }
            catch (Exception ex)
            {
                throw new Exception("clsDatabase.SaveSeries( DataTable, " + site.SiteID + ", " + variable.VariableID + ") on row "+ count, ex);
            }
            return count;
        }

        private int GetQualifierID(string p)
        {
            return (from q in this.Qualifiers where q.QualifierCode == p select q.QualifierID).First();
            
        }
        private void InsertBulk(DataTable data)
        {
            SqlBulkCopy bc = new SqlBulkCopy(this.conn);

            bc.BatchSize = 1000;
            bc.DestinationTableName = "DataValues";
            try
            {
                bc.WriteToServer(data);
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("Violation of"))
                {
                    throw ex;
                }
            }
        }
        public void updateSeriesCatalag()
        {

            SqlConnection con = new SqlConnection(this.conn);
            SqlCommand cmd = new SqlCommand("UpdateSeriesCatalog", con);
            cmd.CommandType = CommandType.StoredProcedure;


            cmd.CommandTimeout = 30000;
            // Try to open the database and execute the update. 
            int added = 0;
            try
            {
                con.Open();
                added = cmd.ExecuteNonQuery();
            }
            catch (Exception err)
            {
                throw err;
            }

            finally
            {
                con.Close();
            }
        }
        
    }
}
