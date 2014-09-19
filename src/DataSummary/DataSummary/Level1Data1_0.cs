// -----------------------------------------------------------------------
// Data Summary
// <copyright file="IndexVariables.cs" company="Utah State University">
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
    using DataSummary.Service1_0;
    using Logger;

    public class Level1Data1_0
    {
        /// <summary>
        /// Connection to the Webservice to gather the Original Agency data
        /// </summary>
        private WaterOneFlow ws;

        /// <summary>
        /// access to the database function
        /// </summary>
        private ClsDBAccessor db;

        // private clsCalculateValues cv = new clsCalculateValues();

        /// <summary>
        /// The value in the database that indicates no value was saved for the time period
        /// </summary>
        private double noDataValue;

        /// <summary>
        /// contains all data to save to database for the Datavalues table
        /// </summary>
        private DataTable values;

         /// <summary>
        /// Initializes a new instance of the OriginalAgencyData1_0 class. Builds a table to hold the DataValue information
        /// </summary>
        public Level1Data1_0(ClsDBAccessor dba)
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

        /// <summary>
        /// gather and save Original Agency data from webservice to database
        /// </summary>
        /// <param name="variable">object from the database that contains all the details of the series we are trying to gather data for</param>
        /// <param name="start">date at the beginning of the interval</param>
        /// <param name="end">date at end of interval</param>1
        public void AgencyDataFill(L1HarvestList variable, string start, string end, int count)
        {
            DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "Level1Data1_0" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", String.Format("{0} {1} {2}-{3} {4}\t{5}-{6} {7}/{8}", "Level1", variable.SiteName, start.Replace('-', '/'), end.Replace('-', '/'), variable.SiteCode, variable.VariableCode, "Level1", count, db.SummaryDB.Count));

            TimeSeriesType dataVals = this.GetData(variable, start, end);
            if (dataVals != null)
            {
                if (Convert.ToInt32(dataVals.values.count) >= 1)
                {

                    int siteID = this.SiteData(variable);
                    if (variable.SiteID == 0)
                        this.db.SummaryDB.UpdateHarvestSite(variable, siteID);
                    this.noDataValue = Convert.ToDouble(dataVals.variable.NoDataValue);
                    TsValuesSingleVariableType vals = dataVals.values;
                    DateTime startDate = (from n in vals.value select Convert.ToDateTime(n.dateTime)).Min();
                    DateTime endDate = (from n in vals.value select Convert.ToDateTime(n.dateTime)).Max();

                    var data = from n in vals.value select n;

                    foreach (ValueSingleVariable member in data)
                    {
                        TimeSpan utcoffset = member.dateTime - member.dateTime.ToUniversalTime();

                        // site, Method, variable, Source 
                        this.values.Rows.Add(SqlInt32.Null, Convert.ToDouble(member.Value), (member.accuracyStdDev == 0 ? SqlDouble.Null : member.accuracyStdDev), member.dateTime, utcoffset.Hours/*utcoffset*/, member.dateTime.ToUniversalTime(), siteID, variable.VariableID, (member.offsetValue == 0 ? SqlDouble.Null : member.offsetValue), (Convert.ToInt32(member.offsetTypeID) == 0 ? SqlInt32.Null : Convert.ToInt32(member.offsetTypeID)), this.CensorCodeToString(member), SqlInt32.Null, variable.MethodID, variable.SourceID, (Convert.ToInt32(member.sampleID) == 0 ? SqlInt32.Null : Convert.ToInt32(member.sampleID)), SqlInt32.Null, variable.QualityControlLevelID);
                    }

                    this.db.TooDB.InsertBulk(this.values);
                    DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "Level1Data1_0" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", values.Rows.Count + " Rows Saved. " + "Level1" + " " + variable.SiteName + " " + variable.SiteCode + "\t" + variable.VariableCode + "-" + "Level1");
                    this.db.TooDB.SaveSeries(siteID, variable);
                }
                else
                {
                    //DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "Level1Data1_0" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", "No Values Found. " + variable.DataTimePeriod + " " + variable.SiteName + " " + variable.SiteCode + "\t" + variable.VariableCode + "-" + variable.DataTimePeriod);

                }
            }
            else
            {
                DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "Level1Data1_0" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", 0 + " Rows Saved. " + "Level1" + " " + variable.SiteName + " " + variable.SiteCode + "\t" + variable.VariableCode + "-" + "Level1");
                   
            }
        }

        private string CensorCodeToString(ValueSingleVariable dataValue)
        {
            if (dataValue.censorCodeSpecified)
            {

                return dataValue.censorCode.ToString();
            }
            else
            {
                return CensorCodeEnum.nc.ToString();
            }
        }

        /// <summary>
        /// Gets the Data from  a webservice for the Original Agency Data
        /// </summary>
        /// <param name="series">object from the database that contains all the details of the series we are trying to gather data for</param>
        /// <param name="start">start of interval to retrieve from webservice</param>
        /// <param name="end">end of the interval to retrieve from webservice</param>
        /// <returns> a class that containsall the data from the webservice call</returns>
        private TimeSeriesType GetData(L1HarvestList series, string start, string end)
        {
            this.ws = new WaterOneFlow();
            this.ws.Url = series.WebServiceURL;
            TimeSeriesResponseType response = this.ws.GetValuesObject(series.SiteCode, series.VariableCode, start, end, string.Empty);
            TimeSeriesType tseries = response.timeSeries;
            return tseries;
        }

        /// <summary>
        /// saves or gathers site information  SiteData
        /// </summary>
        /// <param name="series">object from the database that contains all the details of the series we are trying to gather data for</param>
        /// <returns>the SiteId from the database</returns>
        private int SiteData(L1HarvestList series)
        {
            SiteInfoType site;
            this.ws.Url = series.WebServiceURL;
            try
            {
                // save siteInfo
                SiteInfoResponseType responsesite = this.ws.GetSiteInfoObject(series.SiteCode, string.Empty);
                site = responsesite.site[0].siteInfo;
            }
            catch
            {
                TimeSeriesResponseType response = this.ws.GetValuesObject(series.SiteCode, series.VariableCode, "1900-01-01", "1900-01-01", string.Empty);
                site = (SiteInfoType)response.timeSeries.sourceInfo; // site[0].siteInfo;
            }

            LatLonPointType latlon = (LatLonPointType)site.geoLocation.geogLocation;
            int localx = site.geoLocation.localSiteXY == null ? 0 : (int)site.geoLocation.localSiteXY[0].X; /*(int)DBNull.Value*/
            int localy = site.geoLocation.localSiteXY == null ? 0 : (int)site.geoLocation.localSiteXY[0].Y;
                        
            // note 1 contains county information but not all sites contain a note 1.
            return this.db.TooDB.SaveSite(site.siteCode[0].Value, site.siteName.ToString(), Convert.ToDouble(latlon.longitude), Convert.ToDouble(latlon.latitude), 0/*lat long datum id*/, Convert.ToDouble(site.elevation_m), site.verticalDatum, localx, localy, 0/*local projection id*/, 0/*Position Accuracy*/, site.note[0].Value.Length == 2 ? site.note[0].Value : string.Empty/*"state"*/, string.Empty/*"county"*/, string.Empty/*"comments"*/, series.SiteType);
        }        
    }
}
