// -----------------------------------------------------------------------
// Data Summary
// <copyright file="OriginalAgencyData1_1.cs" company="Utah State University">
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
    using System.Data;
    using System.Data.SqlTypes;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using Service1_1;
    using Logger;

    public class Level1Data1_1
    {
        /// <summary>
        /// Connection to the Webservice to gather the Original Agency data
        /// </summary>
        private WaterOneFlow ws;

        /// <summary>
        /// access to the database function
        /// </summary>
        private ClsDBAccessor db; 

        /// <summary>
        /// The value in the database that indicates no value was saved for the time period
        /// </summary>
        private double noDataValue;

        /// <summary>
        /// contains all data to save to database for the Datavalues table
        /// </summary>
        private DataTable values;

         /// <summary>
        /// Initializes a new instance of the OriginalAgencyData1_1 class. Builds a table to hold the DataValue information
        /// </summary>
        public Level1Data1_1(ClsDBAccessor dba)
        {
            this.db = dba;
            this.values = new DataTable("DataValues");
            this.values.Columns.Add("ValueID", typeof(SqlInt64));
            this.values.Columns.Add("DataValue", typeof(SqlDouble));
            this.values.Columns.Add("valAccuracy", typeof(SqlDouble));
            this.values.Columns.Add("LocalDT", typeof(SqlDateTime));
            this.values.Columns.Add("UTCOffset", typeof(SqlDouble));
            this.values.Columns.Add("DateUTC", typeof(SqlDateTime));
            this.values.Columns.Add("varID", typeof(SqlInt32));
            this.values.Columns.Add("siteID", typeof(SqlInt32));
            this.values.Columns.Add("offsetVal", typeof(SqlDouble));
            this.values.Columns.Add("offsetTypeID", typeof(SqlInt32));
            this.values.Columns.Add("censorcode", typeof(SqlString));
            this.values.Columns.Add("qualifierID", typeof(SqlInt32));
            this.values.Columns.Add("methodID", typeof(SqlInt32));
            this.values.Columns.Add("sourceID", typeof(SqlInt32));
            this.values.Columns.Add("sampleID", typeof(SqlInt32));
            this.values.Columns.Add("derivedfromID", typeof(SqlInt32));
            this.values.Columns.Add("qclID", typeof(SqlInt32));
        }

        public void AgencyDataFill(L1HarvestList variable, string start, string end, int count)
        {
            DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "Level1Data1_1" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", String.Format("{0} {1} {2}-{3} {4}\t{5}-{6} {7}/{8}", "Level1", variable.SiteName, start.Replace('-', '/'), end.Replace('-', '/'), variable.SiteCode, variable.VariableCode, "Level1", count, db.SummaryDB.Count));

            TimeSeriesType dataVals = this.GetData(variable, start, end);
            if (dataVals.values.Count() >= 1)
            {
               
                int siteID = this.SiteData(variable);
                if (variable.SiteID == 0 )
                    this.db.SummaryDB.UpdateHarvestSite(variable, siteID);
                this.noDataValue = Convert.ToDouble(dataVals.variable.noDataValue);
                TsValuesSingleVariableType vals = dataVals.values[0];
                if (vals.value != null)
                {
                    DateTime startDate = (from n in vals.value select Convert.ToDateTime(n.dateTime)).Min();
                    DateTime endDate = (from n in vals.value select Convert.ToDateTime(n.dateTime)).Max();

                    var data = from n in vals.value select n;
                    foreach (ValueSingleVariable member in data)
                    {
                        TimeSpan utcoffset = member.dateTime - member.dateTime.ToUniversalTime();
                        this.values.Rows.Add(SqlInt32.Null, Convert.ToDouble(member.Value), (member.accuracyStdDev == 0 ? SqlDouble.Null : member.accuracyStdDev), member.dateTime, utcoffset.Hours, member.dateTime.ToUniversalTime(), siteID, variable.VariableID, (member.offsetValue == 0 ? SqlDouble.Null : member.offsetValue), (Convert.ToInt32(member.offsetTypeID) == 0 ? SqlInt32.Null : Convert.ToInt32(member.offsetTypeID)), member.censorCode.ToString(), SqlInt32.Null, variable.MethodID, variable.SourceID, (Convert.ToInt32(member.sampleID) == 0 ? SqlInt32.Null : Convert.ToInt32(member.sampleID)), SqlInt32.Null, variable.QualityControlLevelID);
                    }
                    
                    this.db.TooDB.InsertBulk(this.values);
                    DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "Level1Data1_1" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", values.Rows.Count + " Rows Saved. " + "Level1" + " " + variable.SiteName + " " + variable.SiteCode + "\t" + variable.VariableCode + "-" + "Level1");
                    this.db.TooDB.SaveSeries(siteID, variable);
                }                
            }
            else
                DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "Level1Data1_1" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()",  "No Values Found. " + "Level" + " " + variable.SiteName + " " + variable.SiteCode + "\t" + variable.VariableCode + "-" + "Level1");
        }
        
        private TimeSeriesType GetData(L1HarvestList series, string start, string end)
        {
            this.ws = new WaterOneFlow();
            this.ws.Url = series.WebServiceURL;
            this.ws.Timeout = Timeout.Infinite;
            TimeSeriesResponseType response = this.ws.GetValuesObject(series.SiteCode, series.VariableCode, start, end, string.Empty);
            TimeSeriesType tseries = response.timeSeries[0];
            return tseries;
        }

        private int SiteData(L1HarvestList series)
        {
            this.ws.Url = series.WebServiceURL;

            // save siteInfo
            SiteInfoResponseType responsesite = this.ws.GetSiteInfoObject(series.SiteCode, string.Empty);
            SiteInfoType site = responsesite.site[0].siteInfo;
            LatLonPointType latlon = (LatLonPointType)site.geoLocation.geogLocation;
            int localx = site.geoLocation.localSiteXY == null ? 0 : (int)site.geoLocation.localSiteXY[0].X; /*(int)DBNull.Value*/
            int localy = site.geoLocation.localSiteXY == null ? 0 : (int)site.geoLocation.localSiteXY[0].Y;

            string state = string.Empty;
            string county = string.Empty;
            if (site.siteProperty != null)
            {
                foreach (PropertyType s in site.siteProperty)
                {
                    if (s.name == "State")
                    {
                        state = s.Value;
                    }
                    else if (s.name == "County")
                    {
                        county = s.Value;
                    }
                }
            }

            return this.db.TooDB.SaveSite(site.siteCode[0].Value, site.siteName.ToString(), Convert.ToDouble(latlon.longitude), Convert.ToDouble(latlon.latitude), 0, Convert.ToDouble(site.elevation_m), site.verticalDatum, localx, localy, 0, 0, state, county/*"county"*/, string.Empty/*"comments"*/, series.SiteType);
        }
    }
}
