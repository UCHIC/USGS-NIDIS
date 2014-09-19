// -----------------------------------------------------------------------
// Data Summary
// <copyright file="ClsTooDB.cs" company="Utah State University">
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
    using System.Data.SqlTypes;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using Logger;

    public class ClsTooDB : SummaryEntities
    {
        private string conn;
        public ClsTooDB() : base()
        {
            this.conn = ConfigurationManager.ConnectionStrings["SummaryEntities"].ConnectionString.Split('"')[1];     
        }

        public ClsTooDB(string connectionString)
            : base(connectionString)
        {
            this.conn = connectionString.Split('"')[1];
        }

        public Variable GetVariable(int vID)
        {
            try
            {
                return (from v in this.Variables where v.VariableID == vID select v).First();
            }
            catch (Exception ex)
            {
                throw new Exception("clsTooDatabase.getVariable(" + vID + ")", ex);
            }
        }

        public DateTime GetLastDateOfSeries(int sID)
        {
            try
            {
                return (DateTime)(from SC in this.SeriesCatalogs where SC.SeriesID == sID select SC.EndDateTime).First();
            }
            catch (Exception ex)
            {
                throw new Exception("clsTooDB.GetLastDateOfSeries(" + sID + ")", ex);
            }
        }

        public int SiteExists(string siteCode)
        {
            try
            {
                return (from sID in this.Sites where sID.SiteCode == siteCode select sID.SiteID).First();
            }
            catch
            {
                return -99;
            }
        }

       
        public int SeriesExists(int siteID, int variableID)
        {
            try
            {
                return (from SC in this.SeriesCatalogs where SC.VariableID == variableID && SC.SiteID == siteID select SC.SeriesID).First();
            }
            catch
            {
                return -99;
            }
        }

        public SeriesCatalog GetSeries(string siteCode, int variableID)
        {
            try
            {
                return (from SC in this.SeriesCatalogs where SC.VariableID == variableID && SC.SiteCode == siteCode select SC).First();
            }
            catch (Exception ex)
            {
                throw new Exception("clsTooDB.getSeries(" + siteCode + ", " + variableID + ")", ex);
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
                throw new Exception("clsTooDB.getSeries(" + siteID + ", " + variableID + ")", ex);
            }
        }

        public int GetSiteID(string siteCode)
        {
            try
            {
                return (from s in this.Sites where s.SiteCode == siteCode select s.SiteID).First();
            }
            catch (Exception ex)
            {
                throw new Exception("clsTooDB.getSiteID(" + siteCode + ")", ex);
            }
        }        
    
        public string GetQCL(int qclID)
        {
            try
            {
                return (from Q in this.QualityControlLevels where Q.QualityControlLevelID == qclID select Q.QualityControlLevelCode).First();
            }
            catch (Exception ex)
            {
                throw new Exception("clsTooDB.getQCL(" + qclID + ")", ex);
            }
        }

        public string GetUnits(int unitsID)
        {
            try
            {
                return (from U in this.Units where U.UnitsID == unitsID select U.UnitsName).First();
            }
            catch (Exception ex)
            {
                throw new Exception("clsTooDB.getUnits(" + unitsID + ")", ex);
            }
        }


        public int GetUnitID(string unitName)
        {
            try
            {
                return (from U in this.Units where U.UnitsName == unitName select U.UnitsID).First();
            }
            catch (Exception ex)
            {
                throw new Exception("clsTooDB.getUnitID(" + unitName + ")", ex);
            }
        }
        public void updateSeriesCatalag()
        {           
            SqlConnection con = new SqlConnection(this.conn);
            SqlCommand cmd = new SqlCommand("UpdateSeriesCatalog", con);
            cmd.CommandType = CommandType.StoredProcedure;
            
           
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

        public void InsertBulk(DataTable data)
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
                    throw;
                }
            }
        }

        public int SaveSite(Site site)
        {
            site.SiteID = 0;
            this.AddToSites(site);
            this.SaveChanges();
            return site.SiteID;
        }

        public int SaveSite(
            string siteCode,
            string siteName,
            double lng,
            double lat,
            int lldatumID,
            double elev,
            string vertDatum,
            int localx,
            int localy,
            int lprojid,
            int posAcc,
            string state,
            string county,
            string comments,
            string siteType)
        {
            Site s;
            try
            {
                s = (from S in this.Sites where S.SiteCode == siteCode /*&& S.SiteType == siteType/* && S.SiteName == siteName && S.Longitude == lng*/ select S).First();
            }
            catch
            {
                DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "clsTooDB" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", string.Format("Saving new Site: {0}", siteName));

                s = new Site();
                s.SiteCode = siteCode;
                s.SiteName = siteName;
                s.Longitude = lng;
                s.Latitude = lat;
                s.LatLongDatumID = lldatumID;
                s.Elevation_m = this.TestInput(elev);
                s.VerticalDatum = this.TestInput(vertDatum);

                if (localx != 0)
                {
                    s.LocalX = localx;
                }

                if (localy != 0)
                {
                    s.LocalY = localy;
                }

                if (lprojid != 0)
                {
                    s.LocalProjectionID = lprojid;
                }

                if (posAcc != 0)
                {
                    s.PosAccuracy_m = posAcc;
                }

                s.State = this.TestInput(state);
                s.County = this.TestInput(county);
                s.Comments = this.TestInput(comments);
               // s.SiteType = siteType;
                this.AddToSites(s);
                this.SaveChanges();
            }

            return s.SiteID;
        }

        public int SaveMethod(string mlink, string mdescrip)
        {
            Method m;
            try
            {
                m = (from M in this.Methods where M.MethodDescription == mdescrip select M).First();
            }
            catch
            {
                m = new Method();
                m.MethodDescription = mdescrip;
                m.MethodLink = this.TestInput(mlink);
                this.AddToMethods(m);
                this.SaveChanges();
            }

            return m.MethodID;
        }

        //public int SaveSeries(int[] meta, int qclid)
        //{
        //    SeriesCatalog sc;
        //    int siteID = meta[0];
        //    int methodID = meta[1];
        //    int varID = meta[2];
        //    int sourceID = meta[3];
        //    IQueryable<DataValue> dataList = from DV in this.DataValues where DV.Site.SiteID == siteID && DV.Variable.VariableID == varID && DV.Method.MethodID == methodID && DV.Source.SourceID == sourceID select DV;

        //    try
        //    {
        //        sc = (from S in this.SeriesCatalogs where S.SiteID == siteID && S.VariableID == varID && S.MethodID == methodID && S.SourceID == sourceID select S).First();
        //        sc.ValueCount = dataList.Count();
        //        sc.EndDateTime = (from D in dataList select D.LocalDateTime).Max();
        //        sc.EndDateTimeUTC = (from D in dataList select D.DateTimeUTC).Max();
        //        this.SaveChanges();
        //    }
        //    catch
        //    {
        //        sc = new SeriesCatalog();
        //        DataValue data = dataList.First();
        //        sc = new SeriesCatalog();
        //        sc.SiteID = data.Site.SiteID;
        //        sc.SiteCode = data.Site.SiteCode;
        //        sc.SiteName = data.Site.SiteName;
        //        sc.VariableID = data.Variable.VariableID;
        //        sc.VariableName = data.Variable.VariableName;
        //        sc.VariableCode = data.Variable.VariableCode;
        //        sc.Speciation = data.Variable.Speciation;
        //        sc.VariableUnitsID = data.Variable.Unit.UnitsID;
        //        sc.VariableUnitsName = this.GetUnits(data.Variable.Unit.UnitsID);
        //        sc.SampleMedium = data.Variable.SampleMedium;
        //        sc.ValueType = data.Variable.ValueType;
        //        sc.TimeSupport = data.Variable.TimeSupport;
        //        sc.TimeUnitsID = data.Variable.Unit1.UnitsID;
        //        sc.TimeUnitsName = this.GetUnits(data.Variable.Unit1.UnitsID);
        //        sc.DataType = data.Variable.DataType;
        //        sc.GeneralCategory = data.Variable.GeneralCategory;
        //        sc.MethodID = data.Method.MethodID;
        //        sc.MethodDescription = data.Method.MethodDescription;
        //        sc.SourceID = data.Source.SourceID;
        //        sc.Organization = data.Source.Organization;
        //        sc.SourceDescription = data.Source.SourceDescription;
        //        sc.Citation = data.Source.Citation;
        //        sc.QualityControlLevelID = qclid;
        //        sc.QualityControlLevelCode = this.GetQCL(qclid);
        //        sc.BeginDateTime = (from DL in dataList select DL.LocalDateTime).Min();
        //        sc.BeginDateTimeUTC = (from DL in dataList select DL.DateTimeUTC).Min();
        //        sc.EndDateTime = (from DL in dataList select DL.LocalDateTime).Max();
        //        sc.EndDateTimeUTC = (from DL in dataList select DL.DateTimeUTC).Max();
        //        sc.ValueCount = dataList.Count();
        //        this.AddToSeriesCatalogs(sc);
        //        this.SaveChanges();
        //    }

        //    return sc.SeriesID;
        //}

        public int SaveSeries(int siteID, L1HarvestList variable)
        {
            SeriesCatalog sc;
            int methodID = variable.MethodID;
            int varID = variable.VariableID;
            int sourceID = variable.SourceID;
            IQueryable<DataValue> dataList = from DV in this.DataValues where DV.Site.SiteID == siteID && DV.Variable.VariableID == varID && DV.Method.MethodID == methodID && DV.Source.SourceID == sourceID select DV;

            try
            {
                // site, Method, variable, Source  
                sc = (from S in this.SeriesCatalogs where S.SiteID == siteID && S.VariableID == varID && S.MethodID == methodID && S.SourceID == sourceID select S).First();
                sc.ValueCount = dataList.Count();
                //// sc.BeginDateTime = StartDate;
                //// sc.BeginDateTimeUTC = StartDate.ToUniversalTime();
                sc.EndDateTime = (from DL in dataList select DL.LocalDateTime).Max();
                sc.EndDateTimeUTC = (from DL in dataList select DL.DateTimeUTC).Max();
                this.SaveChanges();
            }
            catch(Exception ex)
            {
                try
                {
                    DataValue data = dataList.First();
                    //// DataValues D = (from DV in this.DataValues where DV.ValueID == DataValID select DV).First();
                    sc = new SeriesCatalog();
                    if (data.Site != null)
                    {
                        sc.SiteID = data.Site.SiteID;
                        sc.SiteCode = data.Site.SiteCode;
                        sc.SiteName = data.Site.SiteName;
                    }
                    else
                    {
                        Site s = (from S in this.Sites where S.SiteID == siteID select S).First();
                        sc.SiteID = s.SiteID;
                        sc.SiteCode = s.SiteCode;
                        sc.SiteName = s.SiteName;
                    }

                    if (data.Variable != null)
                    {
                        sc.VariableID = data.Variable.VariableID;
                        sc.VariableName = data.Variable.VariableName;
                        sc.VariableCode = data.Variable.VariableCode;
                        sc.Speciation = data.Variable.Speciation;
                        //if (data.Variable.Unit != null)
                        //{
                            sc.VariableUnitsID = data.Variable.Unit.UnitsID;
                            sc.VariableUnitsName = data.Variable.Unit.UnitsName;
                        //}
                        //else
                        //{  //Units u = (from U in this.Units where U.UnitsID==1 select U).First();
                            //sc.VariableUnitsID = Convert.ToInt16(data.Variable UnitsReference.EntityKey.EntityKeyValues[0].Value);
                            //sc.VariableUnitsName = this.GetUnits(sc.VariableUnitsID.Value);
                        //}
                        
                        sc.SampleMedium = data.Variable.SampleMedium;
                        sc.ValueType = data.Variable.ValueType;
                        sc.TimeSupport = data.Variable.TimeSupport;
                        //if (data.Variable.Unit != null)
                        //{
                            sc.TimeUnitsID = data.Variable.Unit1.UnitsID;
                            sc.TimeUnitsName = this.GetUnits(data.Variable.Unit1.UnitsID);                            
                        //}
                        //else
                        //{ // Units u = (from U in this.Units where U.UnitsID==1 select U).First();
                        //    sc.TimeUnitsID = Convert.ToInt16(data.Variable.Units1Reference.EntityKey.EntityKeyValues[0].Value);
                        //    sc.TimeUnitsName = this.GetUnits(sc.TimeUnitsID.Value);
                        //}
                        
                        sc.DataType = data.Variable.DataType;
                        sc.GeneralCategory = data.Variable.GeneralCategory;
                    }
                    else
                    {
                        Variable v = (from V in this.Variables where V.VariableID == varID select V).First();

                        sc.VariableID = v.VariableID;
                        sc.VariableName = v.VariableName;
                        sc.VariableCode = v.VariableCode;
                        sc.Speciation = v.Speciation;
                        
                        if (v.Unit != null)
                        {
                            sc.VariableUnitsID = v.Unit.UnitsID;
                            sc.VariableUnitsName = this.GetUnits(v.Unit.UnitsID);
                        }
                        else
                        {   
                        sc.VariableUnitsID = Convert.ToInt16(v.UnitReference.EntityKey.EntityKeyValues[0].Value);
                        sc.VariableUnitsName = this.GetUnits(sc.VariableUnitsID.Value);
                        }

                        sc.SampleMedium = v.SampleMedium;
                        sc.ValueType = v.ValueType;
                        sc.TimeSupport = v.TimeSupport;
                        if (v.Unit1 != null)
                        {
                            sc.TimeUnitsID = v.Unit1.UnitsID;
                            sc.TimeUnitsName = v.Unit1.UnitsName;
                        }
                        else
                        {
                            
                            sc.TimeUnitsID = Convert.ToInt16(v.Unit1Reference.EntityKey.EntityKeyValues[0].Value);
                            sc.TimeUnitsName = this.GetUnits(sc.TimeUnitsID.Value);
                        }

                        sc.DataType = v.DataType;
                        sc.GeneralCategory = v.GeneralCategory;
                    }

                    if (data.Method != null)
                    {
                        sc.MethodID = data.Method.MethodID;
                        sc.MethodDescription = data.Method.MethodDescription;
                    }
                    else
                    {
                        Method m = (from M in this.Methods where M.MethodID == methodID select M).First();
                        sc.MethodID = m.MethodID;
                        sc.MethodDescription = m.MethodDescription;
                    }

                    if (data.Source != null)
                    {
                        sc.SourceID = data.Source.SourceID;
                        sc.Organization = data.Source.Organization;
                        sc.SourceDescription = data.Source.SourceDescription;
                        sc.Citation = data.Source.Citation;
                    }
                    else
                    {
                        Source s = (from S in this.Sources where S.SourceID == sourceID select S).First();

                        sc.SourceID = s.SourceID;
                        sc.Organization = s.Organization;
                        sc.SourceDescription = s.SourceDescription;
                        sc.Citation = s.Citation;
                    }

                    sc.QualityControlLevelID = variable.QualityControlLevelID;
                    sc.QualityControlLevelCode = this.GetQCL(variable.QualityControlLevelID);
                    sc.BeginDateTime = (from DL in dataList select DL.LocalDateTime).Min();
                    sc.BeginDateTimeUTC = (from DL in dataList select DL.DateTimeUTC).Min();
                    sc.EndDateTime = (from DL in dataList select DL.LocalDateTime).Max();
                    sc.EndDateTimeUTC = (from DL in dataList select DL.DateTimeUTC).Max();
                    sc.ValueCount = dataList.Count();
                    this.AddToSeriesCatalogs(sc);
                    this.SaveChanges();
                }
                catch (Exception exception)
                {
                    throw new Exception("SaveSeries(int " + siteID + ", AggregateSeries " + variable.SiteCode + ")", exception);
                }
            }

            return sc.SeriesID;
        }

       

        public void CreateSeriesCatalog()
        {
            using (SqlConnection sqlConnection1 = new SqlConnection(this.conn[1] + ";Connection Timeout=9999;"))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    int rowsAffected;

                    cmd.CommandText = "dbo.spUpdateSeriesCatalog";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = sqlConnection1;
                    Console.WriteLine(sqlConnection1.ConnectionTimeout);
                    cmd.CommandTimeout = sqlConnection1.ConnectionTimeout;
                    sqlConnection1.Open();

                    rowsAffected = cmd.ExecuteNonQuery();
                    sqlConnection1.Close();
                }
            }
        }
       
        private string TestInput(string input)
        {
            return input == string.Empty ? null : input;
        }

        private int TestInput(int input)
        {
            return input == 0 ? Convert.ToInt32(null) : input;
        }

        private double TestInput(double input)
        {
            return input == 0 ? Convert.ToDouble(null) : input;
        }

       
    }
}
