using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Text;
using log4net;
using WaterOneFlow.odm.v1_1.ValuesDataSetTableAdapters;
using WaterOneFlow.Schema.v1_1;
using WaterOneFlowImpl;
using WaterOneFlowImpl.v1_1;
using tableSpace = WaterOneFlow.odm.v1_1.ValuesDataSetTableAdapters;
using VariableParam = WaterOneFlowImpl.VariableParam;
/// <summary>
/// Summary description for WebserviceCall
/// </summary>
/// 
namespace WaterOneFlow.odws
{
    using WaterOneFlow.odm.v1_1;
    using DataValuesTableAdapter = tableSpace.DataValuesTableAdapter;
    using UnitsTableAdapter = tableSpace.UnitsTableAdapter;
    using OffsetTypesTableAdapter = tableSpace.OffsetTypesTableAdapter;
    using QualityControlLevelsTableAdapter = tableSpace.QualityControlLevelsTableAdapter;
    using MethodsTableAdapter = tableSpace.MethodsTableAdapter;
    using SamplesTableAdapter = tableSpace.SamplesTableAdapter;
    using SourcesTableAdapter = tableSpace.SourcesTableAdapter;
    using SitesTableAdapter = tableSpace.SitesTableAdapter;
    using SeriesCatalogTableAdapter = WaterOneFlow.odm.v1_1.seriesCatalogDataSetTableAdapters.SeriesCatalogTableAdapter;
    using System.Threading;
    using System.Net;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Reflection;
    public class WebserviceCall
    {
        List<USBRSite> sites = clsUSBRList.createSiteList();
        public WebserviceCall()
        {
            //
            // TODO: Add constructor logic here
            //
        }
        public ValuesDataSet getDataValues(ValuesDataSet ds, DateTime startDate, DateTime endDate)
        {
            SeriesCatalogTableAdapter scAdapater = new SeriesCatalogTableAdapter();
            scAdapater.Connection.ConnectionString = odws.Config.ODDB();
            seriesCatalogDataSet sc = new seriesCatalogDataSet();
            scAdapater.FillBySiteVariable(sc.SeriesCatalog, ds.Sites[0].SiteID, ds.Variables[0].VariableID);

            string requestData = "";
            try
            {
                requestData = generateURL(URLSiteName(ds.Sites), URLVariableCode(ds.Variables, ds.Sites), selectStartDate(startDate, sc.SeriesCatalog[0].SeriesID), selectEndDate(endDate, sc.SeriesCatalog[0].SeriesID));
                return newTable(ds.Variables[0].VariableID.ToString(), sc.SeriesCatalog[0].SeriesID, requestData);
            }
            catch (Exception ex)
            {
                return new ValuesDataSet();
            }
        }

        private string URLSiteName(ValuesDataSet.SitesDataTable site)
        {
            USBRSite siteData = sites.Find(delegate(USBRSite s) { return s.SiteCode.Contains(site[0].SiteCode); });
            if (siteData != null)
                return siteData.URLName;
            else
            {
                string siteName = site[0].SiteName;
                siteName.Replace(' ', '+');
                siteName.Replace(",", "%2C");
                siteName.ToUpper();
                return siteName;
            }
        }

        private string URLVariableCode(ValuesDataSet.VariablesDataTable variable, ValuesDataSet.SitesDataTable site)
        {
            USBRSite siteData = sites.Find(delegate(USBRSite s) { return s.SiteCode.Contains(site[0].SiteCode); });
            USBRVariable V = siteData.variables.Find(delegate(USBRVariable v) { return v.VariableName == variable[0].VariableName; });
            return V.VariableCode;
        }

        private string generateURL(string site, string variable, DateTime startdate, DateTime enddate)
        {
            Match URL;

            try
            {
                string requesturl = string.Format("http://www.usbr.gov/uc/crsp/GetDataSet?l={0}&c={1}&strSDate={2}&strEDate={3}", site, variable, startdate.ToString("d-MMM-yyyy"), enddate.ToString("d-MMM-yyyy"));
                WebRequest request = WebRequest.Create(requesturl);
                WebResponse response = request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                Regex r = new Regex("(?<=<a href=').*(?='>Plain Text)");
                URL = r.Match(responseFromServer);
                if (URL.Length < 1)
                    if (responseFromServer.Contains("Error!"))
                    {
                        return "Error generating URL for webservice call";
                        throw new Exception("Error reading from USBR Website");
                    }
            }
            catch (Exception ex)
            {
                return "Error generating URL for webservice call";
                throw new Exception("Error generating URL for webservice call. See InnerException for more details.", ex);
            }

            int count = 0;
            while (true)
            {
                try
                {
                    WebRequest request = WebRequest.Create(URL.Value);
                    //request.Timeout = Timeout.Infinite;
                    WebResponse response = request.GetResponse();
                    Stream dataStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);
                    string responseFromServer = reader.ReadToEnd();
                    return responseFromServer;
                }
                catch (Exception ex)
                {
                    if (count >= 10)
                    {
                        return "Error generating URL for webservice call";
                        throw new Exception("Error accessing text file in webservice call. See InnerException for more details.", ex);
                    }
                    else
                    {
                        count++;
                        Thread.Sleep(1000);
                    }
                }
            }
        }

        private DateTime selectStartDate(DateTime start, int sID)
        {
            SeriesCatalogTableAdapter scAdapater = new SeriesCatalogTableAdapter();
            scAdapater.Connection.ConnectionString = odws.Config.ODDB();
            seriesCatalogDataSet sc = new seriesCatalogDataSet();
            scAdapater.FillBySeriesID(sc.SeriesCatalog, sID);
            DateTime dbStart = sc.SeriesCatalog[0].BeginDateTime;
            if (start > dbStart)
                return start;
            else
                return dbStart;
        }

        private DateTime selectEndDate(DateTime end, int seriesID)
        {
            SeriesCatalogTableAdapter scAdapater = new SeriesCatalogTableAdapter();
            scAdapater.Connection.ConnectionString = odws.Config.ODDB();
            seriesCatalogDataSet sc = new seriesCatalogDataSet();
            scAdapater.FillBySeriesID(sc.SeriesCatalog, seriesID);
            DateTime dbEnd = sc.SeriesCatalog[0].EndDateTime;
            if (end < dbEnd)
                return end;
            else
                return dbEnd;

        }
        /*
         * newTable
         * creates the Data Table required by rest of program to create the XML form
         */
        private ValuesDataSet newTable(string variable, int SID, string Data)
        {
            SeriesCatalogTableAdapter scAdapater = new SeriesCatalogTableAdapter();
            scAdapater.Connection.ConnectionString = odws.Config.ODDB();
            ValuesDataSet ds = basicValuesDataSet();
            DataTable variableTable = ds.Tables[0];
            int ValueID = 0;

            foreach (string entry in Data.Split('\n'))
            {

                string[] row = entry.Split(',');
                if (row[0] != "" && row[0] != null)
                {
                    ValueID++;

                    seriesCatalogDataSet scTable = new seriesCatalogDataSet();
                    scAdapater.FillBySeriesID(scTable.SeriesCatalog, SID);
                    seriesCatalogDataSet.SeriesCatalogRow sc = scTable.SeriesCatalog[0];
                    TimeSpan utcOffset = sc.BeginDateTime.Subtract(sc.BeginDateTimeUTC);
                    variableTable.Rows.Add(ValueID, sc.SiteID, sc.VariableID, 0, 0, "nc", 0, sc.MethodID, sc.SourceID, 0, 0, sc.QualityControlLevelID, (row[0] == string.Empty) || row[1] == string.Empty ? -999999 : Convert.ToDouble(row[1]), 0, Convert.ToDateTime(row[0]), Convert.ToDateTime(row[0]).ToUniversalTime(), utcOffset.Hours);
                    /*"ValueID","SiteID","VariableID",offsetvalue, offset type id, "CensorCode","QualifierID","MethodID","SourceID","SampleID","DerivedFromID","QualityControlLevelID", "Value", "AccuracyStdDev", "DateTime","DateTimeUTC", "UTCOffset"*/
                }
            }

            return ds;
        }
        private ValuesDataSet basicValuesDataSet()
        {
            ValuesDataSet ds = new ValuesDataSet();

            UnitsTableAdapter unitsTableAdapter =
                new UnitsTableAdapter();
            unitsTableAdapter.Connection.ConnectionString = odws.Config.ODDB();

            OffsetTypesTableAdapter offsetTypesTableAdapter =
                new OffsetTypesTableAdapter();
            offsetTypesTableAdapter.Connection.ConnectionString = odws.Config.ODDB();

            QualityControlLevelsTableAdapter qualityControlLevelsTableAdapter =
                new QualityControlLevelsTableAdapter();
            qualityControlLevelsTableAdapter.Connection.ConnectionString = odws.Config.ODDB();

            MethodsTableAdapter methodsTableAdapter =
                new MethodsTableAdapter();
            methodsTableAdapter.Connection.ConnectionString = odws.Config.ODDB();

            SamplesTableAdapter samplesTableAdapter =
                new SamplesTableAdapter();
            samplesTableAdapter.Connection.ConnectionString = odws.Config.ODDB();


            SourcesTableAdapter sourcesTableAdapter =
                new SourcesTableAdapter();
            sourcesTableAdapter.Connection.ConnectionString = odws.Config.ODDB();

            QualifiersTableAdapter qualifiersTableAdapter =
                new QualifiersTableAdapter();
            qualifiersTableAdapter.Connection.ConnectionString = odws.Config.ODDB();

            CensorCodeCVTableAdapter censorCodeCvTableAdapter =
                new CensorCodeCVTableAdapter();
            censorCodeCvTableAdapter.Connection.ConnectionString = odws.Config.ODDB();

            ISOMetadataTableAdapter IsoMetadataTableAdapter =
 new ISOMetadataTableAdapter();
            IsoMetadataTableAdapter.Connection.ConnectionString = odws.Config.ODDB();


            unitsTableAdapter.Fill(ds.Units);
            offsetTypesTableAdapter.Fill(ds.OffsetTypes);
            qualityControlLevelsTableAdapter.Fill(ds.QualityControlLevels);
            methodsTableAdapter.Fill(ds.Methods);
            samplesTableAdapter.Fill(ds.Samples);
            sourcesTableAdapter.Fill(ds.Sources);
            qualifiersTableAdapter.Fill(ds.Qualifiers);
            censorCodeCvTableAdapter.Fill(ds.CensorCodeCV);
            IsoMetadataTableAdapter.Fill(ds.ISOMetadata);

            return ds;
        }
    }
}
