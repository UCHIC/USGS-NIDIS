// -----------------------------------------------------------------------
// Data Summary
// <copyright file="ClsSummaryDB.cs" company="Utah State University">
//          Copyright (c) 2011, Utah State University
// All rights reserved.
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//           Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//           Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//           Neither the name of the Utah State University nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// </copyright>
// -----------------------------------------------------------------------

namespace DataSummary
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Text;
    using System.Threading;

    public class ClsSummaryDB : SummaryEntities
    {
        private string conn;   
        private int count;

        public int Count
        {
            get { return count; }
            set { count = value; }
        }

        public ClsSummaryDB(string connectionString)
            : base(connectionString)
        {
            count = 0;
            this.conn = connectionString;
        }

        public ClsSummaryDB()
        {
            this.count = 0;
            this.conn = ConfigurationManager.ConnectionStrings["SummaryEntities"].ConnectionString.Split('"')[1];
        }
        
        public void UpdateHarvestSite(L1HarvestList variable, int id)
        {
            variable.SiteID = id;
            this.SaveChanges();
        }

        public L1HarvestList GetDataSeriesAggregate(string siteCode, string variableCode)
        {
            try
            {
                return (from A in this.L1HarvestList where A.SiteCode == siteCode && A.VariableCode == variableCode select A).First();
            }
            catch (Exception ex)
            {
                throw new Exception("clsSummaryDB.GetDataAggregate(" + siteCode + ", " + variableCode + ")", ex);
            }
        }

        public List<L1HarvestList> getOriginalList()
        {
            List<L1HarvestList> aggSeriesListLevel1 = (from A in this.L1HarvestList select A).ToList();// orderby A.SiteType
            this.count = aggSeriesListLevel1.Count;
            return aggSeriesListLevel1;
        }

        public List<L1HarvestList> getOriginalList(string[] whereClause)
        {

            List<L1HarvestList> aggSeriesListLevel1 = new List<L1HarvestList>();            
            
            foreach (string type in whereClause)
            {
                aggSeriesListLevel1.AddRange((from A in this.L1HarvestList where A.SiteType == type select A).ToList());
            }

            this.count = aggSeriesListLevel1.Count();     
            return aggSeriesListLevel1;

        }

        public List<AggregateSeries> getAggregateList()
        {
            return (from A in this.AggregateSeries1 where A.AggregateMethod != "Delta" orderby A.SiteType descending select A).ToList();            
        }
       
        public List<AggregateSeries> getAggregateList(string[] whereClause)
        {
            List<AggregateSeries> aggSeriesList = new List<AggregateSeries>();
           
            foreach (string type in whereClause)
            {
                aggSeriesList.AddRange((from A in this.AggregateSeries1 where A.AggregateMethod != "Delta" && A.SiteType == type orderby A.SiteType descending select A).ToList()); 
            }

            this.count = aggSeriesList.Count();
            return aggSeriesList;
        }

        public List<String> getL3List()
        {
            return (from I in this.IVtoTADxRefs group I by I.SiteType into myGroup select myGroup.Key).ToList();
        }

        public List<string> getL3List(string[] whereClause)
        {
            List<string> xrefList = new List<string>();

            foreach (string type in whereClause)
            {
                xrefList.AddRange((from I in this.IVtoTADxRefs group I by I.SiteType into myGroup where myGroup.Key == type  select myGroup.Key).ToList());
            }

            this.count = xrefList.Count();
            return xrefList;
        }       
                
        public List<WatershedSeries> getL3wList()
        {
            return (from W in this.WatershedSeries orderby W.SiteType descending select W).ToList();

        }

        public List<WatershedSeries> getL3wList(string[] whereClause)
        {
            List<WatershedSeries> xrefList = new List<WatershedSeries>();

            foreach (string type in whereClause)
            {
                xrefList.AddRange((from W in this.WatershedSeries where W.SiteType == type orderby W.SiteType descending select W ).ToList());
            }

            this.count = xrefList.Count();
            return xrefList;
                
        }

        public List<IndexValuexRef> getL4List()
        {
            return (from I in this.IndexValuexRef orderby I.SiteType descending select I).ToList();

        }

        public List<IndexValuexRef> getL4List(string[] whereClause)
        {
            List<IndexValuexRef> xrefList = new List<IndexValuexRef>();

            foreach (string type in whereClause)
            {
                xrefList.AddRange((from I in this.IndexValuexRef where I.SiteType == type orderby I.SiteType descending select I).ToList());
            }

            this.count = xrefList.Count();
            return xrefList;

        }

        private string whereClause(string[] args)
        {
            string where = string.Empty;
            if (args.Length > 0)
            {
                where = "SiteType = ";
                where += "'" + args[0] + "'";
                for (int i = 1; i < args.Length; i++)
                {
                    where += " or SiteType='" + args[i] + "'";
                }
            }
            return where;
        }
        
        //takes a DataSource, Initial Catalog, Userid and Password and returns a SQL Connection STring
        private string createConnString(string DS, string IC, string User, string Pass)
        {
            return string.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3}", DS, IC, User, Pass);
        }
                
        public int monthlyAggregate(AggregateSeries series)
        {
            SqlConnection con = new SqlConnection(this.conn);
            SqlCommand cmd = new SqlCommand("MonthlyAggregator", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.CommandTimeout = 30000;
            // Add the parameters.            
            cmd.Parameters.AddWithValue("@origVariableID", series.OriginalVariableID);
            cmd.Parameters.AddWithValue("@siteType", series.SiteType);

            if (series.DBInitialCatalog != null)
            {
                cmd.Parameters.AddWithValue("@tooConnectionstring", createConnString(series.DBDataSource, series.DBInitialCatalog, series.DBUsername, series.DBPassword));
                cmd.Parameters.AddWithValue("@tooDBName", "[" + series.DBInitialCatalog + "]");
            }
            else
            {
                cmd.Parameters.AddWithValue("@tooConnectionstring", this.conn);
                cmd.Parameters.AddWithValue("@tooDBName", "[" + this.conn.Substring(this.conn.IndexOf("Catalog=") + 8, this.conn.IndexOf(";User") - this.conn.IndexOf("Catalog=") - 8) + "]");
                //string dbname = this.conn.Substring(this.conn.IndexOf("Catalog=")+8, this.conn.IndexOf(";User") - this.conn.IndexOf("Catalog=")-8);
            }
            if (series.L1DBInitialCatalog != null)
            {
                cmd.Parameters.AddWithValue("@fromConnectionstring", createConnString(series.L1DBDataSource, series.L1DBInitialCatalog, series.L1DBUsername, series.L1DBPassword));
                cmd.Parameters.AddWithValue("@fromDBName", "[" + series.L1DBInitialCatalog + "]");
            }
            else
            {
                cmd.Parameters.AddWithValue("@fromConnectionstring", this.conn);
                cmd.Parameters.AddWithValue("@fromDBName", "[" + this.conn.Substring(this.conn.IndexOf("Catalog=") + 8, this.conn.IndexOf(";User") - this.conn.IndexOf("Catalog=") - 8) + "]");
              
            }
            //cmd.Parameters.Add("@returnValue");
            cmd.Parameters.Add(new SqlParameter("@returnVal", SqlDbType.Int, 6));
            //Set the direction for the parameter. This parameter returns the Rows that are returned.
            cmd.Parameters["@returnVal"].Direction = ParameterDirection.Output;


            // Try to open the database and execute the update. 
            int added = 0;
            try
            {
                con.Open();
                cmd.ExecuteNonQuery();
                added = (int)cmd.Parameters["@returnVal"].Value;
                return added;
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
        
        public int biMonthlyAggregate(AggregateSeries series)
        {

            SqlConnection con = new SqlConnection(this.conn);
            SqlCommand cmd = new SqlCommand("BiMonthlyAggregator", con);
            cmd.CommandType = CommandType.StoredProcedure;

            // Add the parameters. 
            cmd.CommandTimeout = 90000;
            cmd.Parameters.AddWithValue("@origVariableID", series.OriginalVariableID);
            cmd.Parameters.AddWithValue("@siteType", series.SiteType);
            
            if (series.DBInitialCatalog != null)
            {
                cmd.Parameters.AddWithValue("@tooConnectionstring", createConnString(series.DBDataSource, series.DBInitialCatalog, series.DBUsername, series.DBPassword));
                cmd.Parameters.AddWithValue("@tooDBName", "[" + series.DBInitialCatalog + "]");
            }
            else
            {
                cmd.Parameters.AddWithValue("@tooConnectionstring", this.conn);
                cmd.Parameters.AddWithValue("@tooDBName", "[" + this.conn.Substring(this.conn.IndexOf("Catalog=") + 8, this.conn.IndexOf(";User") - this.conn.IndexOf("Catalog=") - 8) + "]");
            }
            if (series.L1DBInitialCatalog != null)
            {
                cmd.Parameters.AddWithValue("@fromConnectionstring", createConnString(series.L1DBDataSource, series.L1DBInitialCatalog, series.L1DBUsername, series.L1DBPassword));
                cmd.Parameters.AddWithValue("@fromDBName", "[" + series.L1DBInitialCatalog + "]");
            }
            else
            {
                cmd.Parameters.AddWithValue("@fromConnectionstring", this.conn);
                cmd.Parameters.AddWithValue("@fromDBName", "[" + this.conn.Substring(this.conn.IndexOf("Catalog=") + 8, this.conn.IndexOf(";User") - this.conn.IndexOf("Catalog=") - 8) + "]");
            }
           // cmd.Parameters.Add("@returnVal");

            cmd.Parameters.Add(new SqlParameter("@returnVal", SqlDbType.Int, 6));
            //Set the direction for the parameter. This parameter returns the Rows that are returned.
            cmd.Parameters["@returnVal"].Direction = ParameterDirection.Output;



            // Try to open the database and execute the update. 
            int added = 0;
            try
            {
                con.Open();
                cmd.ExecuteNonQuery();
                added = (int)cmd.Parameters["@returnVal"].Value;
                return added;
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
                
        public int CreateIndexVariableTable(int formulaNum, string siteType)
        {
            SqlConnection con = new SqlConnection(this.conn);
            SqlCommand cmd = new SqlCommand("CalcIndexVariables", con);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FormulaNo", formulaNum);
            cmd.Parameters.AddWithValue("@siteType", siteType);
            

            // Add the parameters. 
            cmd.CommandTimeout = 90000;


            IVtoTADxRef xref = (from I in this.IVtoTADxRefs where I.SiteType == siteType select I).First();

            if (xref.IVDBInitialCatalog != null)
            {
                cmd.Parameters.AddWithValue("@tooConnectionstring", createConnString(xref.IVDBDataSource, xref.IVDBInitialCatalog, xref.IVDBUsername, xref.IVDBPassword));
                cmd.Parameters.AddWithValue("@tooDBName", "[" + xref.IVDBInitialCatalog + "]");
            }
            else
            {
                cmd.Parameters.AddWithValue("@tooConnectionstring", this.conn);
                cmd.Parameters.AddWithValue("@tooDBName", "[" + this.conn.Substring(this.conn.IndexOf("Catalog=") + 8, this.conn.IndexOf(";User") - this.conn.IndexOf("Catalog=") - 8) + "]");
            }
            if (xref.TADDBInitialCatalog != null)
            {
                cmd.Parameters.AddWithValue("@fromConnectionstring", createConnString(xref.TADDBDataSource, xref.TADDBInitialCatalog, xref.TADDBUsername, xref.TADDBPassword));
                cmd.Parameters.AddWithValue("@fromDBName", "[" + xref.TADDBInitialCatalog + "]");
            }
            else
            {
                cmd.Parameters.AddWithValue("@fromConnectionstring", this.conn);
                cmd.Parameters.AddWithValue("@fromDBName", "[" + this.conn.Substring(this.conn.IndexOf("Catalog=") + 8, this.conn.IndexOf(";User") - this.conn.IndexOf("Catalog=") - 8) + "]");
            }
            // cmd.Parameters.Add("@returnVal");

            cmd.Parameters.Add(new SqlParameter("@returnVal", SqlDbType.Int, 6));
            //Set the direction for the parameter. This parameter returns the Rows that are returned.
            cmd.Parameters["@returnVal"].Direction = ParameterDirection.Output;
            //try
            //{
            //con.Open();
            //reader = cmd.ExecuteReader();
            //values.Load(reader);
            //reader.Close();
            //}
            int added = 0;

            try
            {
                con.Open();
                cmd.ExecuteNonQuery();
                added = (int)cmd.Parameters["@returnVal"].Value;
                return added;
            }
            catch (Exception ex)
            {
                //return -1;
               throw new Exception("clsSummaryDB.CalcIndexVariable(" + siteType + ")", ex);

            }
            finally
            {
                con.Close();
            }

            //return values;

        }

        public int CreateIndexVariableWatershed( WatershedSeries watershed)
        {
            //DataTable values = new DataTable(formulaNum.ToString());

            SqlConnection con = new SqlConnection(this.conn);
            SqlCommand cmd = new SqlCommand("IndexVariableWatershed", con);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@siteType", watershed.SiteType);
            cmd.Parameters.AddWithValue("@origVariableID", watershed.L3VariableID);
            cmd.Parameters.AddWithValue("@origMethodID", watershed.L3MethodID);
            //SqlDataReader reader;


            // Add the parameters. 
            cmd.CommandTimeout = 90000;



            if (watershed.DBInitialCatalog != null)
            {
                cmd.Parameters.AddWithValue("@tooConnectionstring", createConnString(watershed.DBDataSource, watershed.DBInitialCatalog ,watershed.DBUsername, watershed.DBPassword));
                cmd.Parameters.AddWithValue("@tooDBName", "[" + watershed.DBInitialCatalog + "]");
            }
            else
            {
                cmd.Parameters.AddWithValue("@tooConnectionstring", this.conn);
                cmd.Parameters.AddWithValue("@tooDBName", "[" + this.conn.Substring(this.conn.IndexOf("Catalog=") + 8, this.conn.IndexOf(";User") - this.conn.IndexOf("Catalog=") - 8) + "]");
            }
            if (watershed.L3DBInitialCatalog != null)
            {
                cmd.Parameters.AddWithValue("@fromConnectionstring", createConnString(watershed.L3DBDataSource, watershed.L3DBInitialCatalog, watershed.L3DBUsername, watershed.L3DBPassword));
                cmd.Parameters.AddWithValue("@fromDBName", "[" + watershed.L3DBInitialCatalog + "]");
            }
            else
            {
                cmd.Parameters.AddWithValue("@fromConnectionstring", this.conn);
                cmd.Parameters.AddWithValue("@fromDBName", "[" + this.conn.Substring(this.conn.IndexOf("Catalog=") + 8, this.conn.IndexOf(";User") - this.conn.IndexOf("Catalog=") - 8) + "]");
            }
            // cmd.Parameters.Add("@returnVal");

            cmd.Parameters.Add(new SqlParameter("@returnVal", SqlDbType.Int, 6));
            //Set the direction for the parameter. This parameter returns the Rows that are returned.
            cmd.Parameters["@returnVal"].Direction = ParameterDirection.Output;
           
            int added = 0;

            try
            {
                con.Open();
                cmd.ExecuteNonQuery();
                added = (int)cmd.Parameters["@returnVal"].Value;
                return added;
            }
            catch (Exception ex)
            {

                throw new Exception("clsSummaryDB.CalcIndexVariableWatershed(" + watershed.SiteType + ")", ex);

            }
            finally
            {
                con.Close();
            }

            //return values;

        }
        
        public int CalcIndexValues(IndexValuexRef indexval)
        {
            //DataTable values = new DataTable(formulaNum.ToString());

            SqlConnection con = new SqlConnection(this.conn);
            SqlCommand cmd = new SqlCommand("CalcIndexValue", con);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@siteType", indexval.SiteType);
            cmd.Parameters.AddWithValue("@origVariableID", indexval.L3VariableID);
            cmd.Parameters.AddWithValue("@origMethodID", indexval.L3MethodID);
            //SqlDataReader reader;


            // Add the parameters. 
            cmd.CommandTimeout = 90000;



            if (indexval.DBInitialCatalog != null)
            {
                cmd.Parameters.AddWithValue("@tooConnectionstring", createConnString(indexval.DBDataSource, indexval.DBInitialCatalog, indexval.DBUsername, indexval.DBPassword));
                cmd.Parameters.AddWithValue("@tooDBName", "[" + indexval.DBInitialCatalog + "]");
            }
            else
            {
                cmd.Parameters.AddWithValue("@tooConnectionstring", this.conn);
                cmd.Parameters.AddWithValue("@tooDBName", "[" + this.conn.Substring(this.conn.IndexOf("Catalog=") + 8, this.conn.IndexOf(";User") - this.conn.IndexOf("Catalog=") - 8) + "]");
            }
            if (indexval.L3DBInitialCatalog != null)
            {
                cmd.Parameters.AddWithValue("@fromConnectionstring", createConnString(indexval.L3DBDataSource, indexval.L3DBInitialCatalog, indexval.L3DBUsername, indexval.L3DBPassword));
                cmd.Parameters.AddWithValue("@fromDBName", "[" + indexval.L3DBInitialCatalog + "]");
            }
            else
            {
                cmd.Parameters.AddWithValue("@fromConnectionstring", this.conn);
                cmd.Parameters.AddWithValue("@fromDBName", "[" + this.conn.Substring(this.conn.IndexOf("Catalog=") + 8, this.conn.IndexOf(";User") - this.conn.IndexOf("Catalog=") - 8) + "]");
            }
            // cmd.Parameters.Add("@returnVal");

            cmd.Parameters.Add(new SqlParameter("@returnVal", SqlDbType.Int, 6));
            //Set the direction for the parameter. This parameter returns the Rows that are returned.
            cmd.Parameters["@returnVal"].Direction = ParameterDirection.Output;

            int added = 0;

            try
            {
                con.Open();
                cmd.ExecuteNonQuery();
                added = (int)cmd.Parameters["@returnVal"].Value;
                return added;
            }
            catch (Exception ex)
            {
                //return -1;
                throw new Exception("clsSummaryDB.CalcIndexValue(" + indexval.SiteType + ")", ex);

            }
            finally
            {
                con.Close();
            }

            //return values;

        }
                
        public void updateSeriesCatalag()
        {
            SqlConnection con = new SqlConnection(this.conn);
            SqlCommand cmd = new SqlCommand("spUpdateSeriesCatalog", con);
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
