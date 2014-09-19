using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Text;
using log4net;

using WaterOneFlowImpl;

namespace WaterOneFlow.odws
{
    using CuahsiBuilder = WaterOneFlowImpl.v1_0.CuahsiBuilder;
    using WaterOneFlow.Schema.v1;

    using WaterOneFlow.odm.v1_1;
    using ValuesDataSetTableAdapters = WaterOneFlow.odm.v1_1.ValuesDataSetTableAdapters;
    using DataValuesTableAdapter = WaterOneFlow.odm.v1_1.ValuesDataSetTableAdapters.DataValuesTableAdapter;
    using unitsDatasetTableAdapters = WaterOneFlow.odm.v1_1.ValuesDataSetTableAdapters.UnitsTableAdapter;
    using UnitsTableAdapter = WaterOneFlow.odm.v1_1.ValuesDataSetTableAdapters.UnitsTableAdapter;
    using QualifiersTableAdapter = WaterOneFlow.odm.v1_1.ValuesDataSetTableAdapters.QualifiersTableAdapter;
    using VariablesDataset = WaterOneFlow.odm.v1_1.VariablesDataset;
    using QualityControlLevelsTableAdapter = WaterOneFlow.odm.v1_1.ValuesDataSetTableAdapters.QualityControlLevelsTableAdapter;
    using VariablesTableAdapter = WaterOneFlow.odm.v1_1.ValuesDataSetTableAdapters.VariablesTableAdapter;
    using MethodsTableAdapter = WaterOneFlow.odm.v1_1.ValuesDataSetTableAdapters.MethodsTableAdapter;
    using SourcesTableAdapter = WaterOneFlow.odm.v1_1.ValuesDataSetTableAdapters.SourcesTableAdapter;
    using SitesTableAdapter = WaterOneFlow.odm.v1_1.ValuesDataSetTableAdapters.SitesTableAdapter;
    using SeriesCatalogTableAdapter = WaterOneFlow.odm.v1_1.seriesCatalogDataSetTableAdapters.SeriesCatalogTableAdapter;
    using System.Threading;
    using System.IO;
    using System.Net;
    using System.Text.RegularExpressions;
    using WaterOneFlow.odm.v1_1.VariablesDatasetTableAdapters;


    public class reqData
    {
        public reqData(ValuesDataSet ds, DateTime startDate, DateTime endDate)
        {
            this.ds = ds;
            this.startDate = startDate;
            this.endDate = endDate;
        }
        ValuesDataSet ds;

        public ValuesDataSet DS
        {
            get { return DS; }
            set { DS = value; }
        }
        DateTime startDate;

        public DateTime StartDate
        {
            get { return startDate; }
            set { startDate = value; }
        }
        DateTime endDate;

        public DateTime EndDate
        {
            get { return endDate; }
            set { endDate = value; }
        }

    }
    //using ReservoirsModel;

    // <summary>
    /// Summary description for ODValues
    /// </summary>
    public class ODValues
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ODValues));

        public ODValues()
        {
            //
            // TODO: Add constructor logic here
            // 
        }

        static List<USBRSite> sites = clsUSBRList.createSiteList();
        private static SeriesCatalogTableAdapter scAdapater = new SeriesCatalogTableAdapter();
        private static SitesTableAdapter siteAdapter = new SitesTableAdapter();
        private static VariablesTableAdapter vAdapter = new VariablesTableAdapter();
        private static MethodsTableAdapter mAdapter = new MethodsTableAdapter();
        private static SourcesTableAdapter srcAdapter = new SourcesTableAdapter();
        private static QualityControlLevelsTableAdapter qclAdapter = new QualityControlLevelsTableAdapter();
        private static seriesCatalogDataSet scDS = new seriesCatalogDataSet();
        private static void generateURL(object r)
        {
            reqData rData = (reqData)r;
            seriesCatalogDataSet sc = new seriesCatalogDataSet();
            scAdapater.FillBySiteVariable(sc.SeriesCatalog, rData.DS.Sites[0].SiteID, rData.DS.Variables[0].VariableID);

            requestData = generateURL(URLSiteName(rData.DS.Sites), URLVariableCode(rData.DS.Variables, rData.DS.Sites), selectStartDate(rData.StartDate, sc.SeriesCatalog[0].SeriesID), selectEndDate(rData.EndDate, sc.SeriesCatalog[0].SeriesID));
            //newDataTable = newTable(rData.DS.Variables[0].VariableID.ToString(), sc.SeriesCatalog[0].SeriesID, requestData);


            //int SeriesID = db.seriesID(rData.Site, rData.Variable);
            //string requestData = generateURL(URLSiteName(rData.Site), URLVariableCode(rData.Variable, rData.Site), selectStartDate(rData.StartDate, SeriesID), selectEndDate(rData.EndDate, SeriesID));
            //newDataTable= newTable(rData.Variable, SeriesID, requestData);

        }
        // private static ValuesDataSet newDataTable;
        private static string requestData;
        private static ValuesDataSet getDataValues(ValuesDataSet ds, DateTime startDate, DateTime endDate)
        {

            //requestData = string.Empty;
            seriesCatalogDataSet sc = new seriesCatalogDataSet();
            scAdapater.FillBySiteVariable(sc.SeriesCatalog, ds.Sites[0].SiteID, ds.Variables[0].VariableID);

            Thread newThread = new Thread(ODValues.generateURL);
            newThread.Start(new reqData(ds, startDate, endDate));//URLSiteName(site), URLVariableCode(variable, site), selectStartDate(startDate, SeriesID), selectEndDate(endDate, SeriesID));
            try
            {
                // mutex.WaitOne();
                //int SeriesID = db.seriesID(site, variable);
                ds = newTable(ds.Variables[0].VariableID.ToString(), sc.SeriesCatalog[0].SeriesID, requestData);
                //return newTable(variable, SeriesID, requestData);
                return ds;
            }
            finally
            { //mutex.ReleaseMutex(); 
            }

            //return new DataTable();
            //return newDataTable;
        }
        private static string URLSiteName(ValuesDataSet.SitesDataTable site)
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

        private static string URLVariableCode(ValuesDataSet.VariablesDataTable variable, ValuesDataSet.SitesDataTable site)
        {
            USBRSite siteData = sites.Find(delegate(USBRSite s) { return s.SiteCode.Contains(site[0].SiteCode); });
            USBRVariable V = siteData.variables.Find(delegate(USBRVariable v) { return v.VariableName == variable[0].VariableCode; });
            return V.VariableCode;
        }

        private static string generateURL(string site, string variable, DateTime startdate, DateTime enddate)
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
                        throw new Exception("Error reading from USBR Website");
            }
            catch (Exception ex)
            {
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
        private static DateTime selectStartDate(DateTime start, int sID)
        {

            seriesCatalogDataSet sc = new seriesCatalogDataSet();
            scAdapater.FillBySeriesID(sc.SeriesCatalog, sID);

            DateTime dbStart = sc.SeriesCatalog[0].BeginDateTime;
            if (start > dbStart)
                return start;
            else
                return dbStart;
        }
        private static DateTime selectEndDate(DateTime end, int sID)
        {
            seriesCatalogDataSet sc = new seriesCatalogDataSet();
            scAdapater.FillBySeriesID(sc.SeriesCatalog, sID);

            DateTime dbEnd = sc.SeriesCatalog[0].BeginDateTime;
            if (end < dbEnd)
                return end;
            else
                return dbEnd;

        }
        /*
         * newTable
         * creates the Data Table required by rest of program to create the XML form
         */
        private static ValuesDataSet newTable(string variable, int SID, string Data)
        {

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


        private void SetConnectionString()
        {
            // valuesTableAdapter.Connection.ConnectionString = odws.Config.ODDB();
            scAdapater.Connection.ConnectionString = odws.Config.ODDB();
            siteAdapter.Connection.ConnectionString = odws.Config.ODDB();
            vAdapter.Connection.ConnectionString = odws.Config.ODDB();
            mAdapter.Connection.ConnectionString = odws.Config.ODDB();
            srcAdapter.Connection.ConnectionString = odws.Config.ODDB();
            qclAdapter.Connection.ConnectionString = odws.Config.ODDB();
        }
        /*
         * newTable
         * creates the Data Table required by rest of program to create the XML form
         * use this if no variable or datetime information is provided
         */
        //private static ValuesDataSet newTable(int siteID)
        //{
        //    ValuesDataSet ds = basicValuesDataSet();
        //    DataTable variableTable = ds.Tables[0];
        //    int ValueID = 0;
        //    DataRow[] rows = oDS.Tables[0].Select();

        //    foreach (DataRow row in rows)
        //    {
        //        for (int i = 1; i < oDS.Tables["Variables"].Columns.Count; i++)//(string column in oDS.Tables["Variables"].Columns)
        //        {
        //            //SqlCommand cmd = new SqlCommand("SELECT MethodID, SourceID, QualityControllevelID, TimeZone, VariableID FROM dbo.SeriesCatalog INNER JOIN dbo.Sites ON dbo.SeriesCatalog.SiteID=dbo.Sites.SiteID WHERE dbo.SeriesCatalog.SiteID = '" + siteID + "' AND dbo.SeriesCatalog.VariableCode= '" + oDS.Tables["Variables"].Columns[i].ColumnName + "'", SnotelConnection);
        //            //SqlDataReader objReader = cmd.ExecuteReader();
        //            object[] Values = new object[5];
        //           // objReader.Read();
        //           // objReader.GetValues(Values);
        //           // variableTable.Rows.Add(ValueID, siteID, Values[4], "nc", 0, Values[0], Values[1], 0, 0, Values[2], row[0] == "" ? -9999 : row[Convert.ToInt32(Values[4])], 0, convertDate(row[0].ToString(), 0), convertDate(row[0].ToString(), Convert.ToInt32(Values[3])), Convert.ToInt32(Values[3]));
        //            ValueID++;                    
        //        }
        //    }
        //    return ds;
        //}
        #region odm 1 series based
        public static ValuesDataSet GetValuesDataSet(int siteID)
        {
            ValuesDataSet ds = basicValuesDataSet();

            //ValuesDataSetTableAdapters.DataValuesTableAdapter valuesTableAdapter = new DataValuesTableAdapter();
            //valuesTableAdapter.Connection.ConnectionString = Config.ODDB();

            //try
            //{
            //    valuesTableAdapter.FillBySiteID(ds.DataValues, siteID);
            //}
            //catch (Exception e)
            //{
            //    log.Fatal("Cannot retrieve information from database: " + e.Message); //+ valuesTableAdapter.Connection.DataSource
            //}
            //return ds;

            //style 2
            //List<string> list = db.getVariablesList(siteID);
            //foreach (string var in list)
            //{
            //    ds=getDataValues(db.getSiteCode(siteID), var, new DateTime(1900, 01, 01), DateTime.Now);
            //    return ds;
            //}
            //return ds;
            siteAdapter.FillBySiteID(ds.Sites, siteID);
            vAdapter.FillBySiteID(ds.Variables, siteID);

            foreach (VariableInfoType variable in ds.Variables.Rows)
            {
                ds.DataValues.Rows.Add(getDataValues(ds, new DateTime(1900, 01, 01), DateTime.Now));

            }

            return ds;
        }
        public static ValuesDataSet GetValuesDataSet(int? siteID, int? VariableID)
        {


            ValuesDataSet ds = basicValuesDataSet();
            //if (!siteID.HasValue || !VariableID.HasValue) return ds;
            //ValuesDataSetTableAdapters.DataValuesTableAdapter valuesTableAdapter = new DataValuesTableAdapter();
            //valuesTableAdapter.Connection.ConnectionString = Config.ODDB();

            //valuesTableAdapter.FillBySiteIDVariableID(ds.DataValues, siteID.Value, VariableID.Value);
            //return ds;

            //style 2
            //return getDataValues(db.getSiteCode(siteID.Value), db.getVariableCode(VariableID.Value), new DateTime(1900, 01, 01), DateTime.Now);
            siteAdapter.FillBySiteID(ds.Sites, siteID.Value);
            vAdapter.FillBySiteID(ds.Variables, ds.Sites[0].SiteID);


            return getDataValues(ds, new DateTime(1900, 01, 01), DateTime.Now);

        }

        public static ValuesDataSet GetValuesDataSet(int? siteID, int? VariableID, W3CDateTime BeginDateTime, W3CDateTime EndDateTime)
        {

            ValuesDataSet ds = basicValuesDataSet();
            //if (!siteID.HasValue || !VariableID.HasValue) return ds;
            //ValuesDataSetTableAdapters.DataValuesTableAdapter valuesTableAdapter = new DataValuesTableAdapter();
            //valuesTableAdapter.Connection.ConnectionString = Config.ODDB();

            //valuesTableAdapter.FillBySiteIdVariableIDBetweenDates(ds.DataValues, siteID.Value, VariableID.Value, BeginDateTime.DateTime, EndDateTime.DateTime);

            //return ds;


            //style 2
            // return getDataValues(db.getSiteCode(siteID.Value), db.getVariableCode(VariableID.Value), BeginDateTime.DateTime, EndDateTime.DateTime);


            siteAdapter.FillBySiteID(ds.Sites, siteID.Value);
            vAdapter.FillBySiteID(ds.Variables, ds.Sites[0].SiteID);
            return getDataValues(ds, new DateTime(1900, 01, 01), DateTime.Now);

        }

        // never implemented or used. Now implemented as a filter using a passed variable parameter
        //public static ValuesDataSet GetValueDataSet(int? siteID, int? VariableID, int? MethodID, int? SourceID, int? QualityControlLevelID, W3CDateTime BeginDateTime, W3CDateTime EndDateTime)
        //{
        //    ValuesDataSet ds = basicValuesDataSet();
        //    if (!siteID.HasValue || !VariableID.HasValue) return ds;
        //    ValuesDataSetTableAdapters.DataValuesTableAdapter valuesTableAdapter = new DataValuesTableAdapter();
        //    valuesTableAdapter.Connection.ConnectionString = Config.ODDB();

        //    valuesTableAdapter.FillBySiteIdVariableIDBetweenDates(ds.DataValues, siteID.Value, VariableID.Value, BeginDateTime.DateTime, EndDateTime.DateTime);

        //    return ds;
        //}
        #endregion








































        // fills dataset with basic tables
        private static ValuesDataSet basicValuesDataSet()
        {
            ValuesDataSet ds = new ValuesDataSet();

            ValuesDataSetTableAdapters.UnitsTableAdapter unitsTableAdapter =
                new UnitsTableAdapter();
            unitsTableAdapter.Connection.ConnectionString = Config.ODDB();

            ValuesDataSetTableAdapters.OffsetTypesTableAdapter offsetTypesTableAdapter =
                      new ValuesDataSetTableAdapters.OffsetTypesTableAdapter();
            offsetTypesTableAdapter.Connection.ConnectionString = Config.ODDB();

            ValuesDataSetTableAdapters.QualityControlLevelsTableAdapter qualityControlLevelsTableAdapter =
                     new ValuesDataSetTableAdapters.QualityControlLevelsTableAdapter();
            qualityControlLevelsTableAdapter.Connection.ConnectionString = Config.ODDB();

            ValuesDataSetTableAdapters.MethodsTableAdapter methodsTableAdapter =
                            new ValuesDataSetTableAdapters.MethodsTableAdapter();
            methodsTableAdapter.Connection.ConnectionString = Config.ODDB();

            ValuesDataSetTableAdapters.SamplesTableAdapter samplesTableAdapter =
                          new ValuesDataSetTableAdapters.SamplesTableAdapter();
            samplesTableAdapter.Connection.ConnectionString = Config.ODDB();

            ValuesDataSetTableAdapters.SourcesTableAdapter sourcesTableAdapter =
                         new ValuesDataSetTableAdapters.SourcesTableAdapter();
            sourcesTableAdapter.Connection.ConnectionString = Config.ODDB();

            QualifiersTableAdapter qualifiersTableAdapter =
                new QualifiersTableAdapter();
            qualifiersTableAdapter.Connection.ConnectionString = Config.ODDB();

            unitsTableAdapter.Fill(ds.Units);
            offsetTypesTableAdapter.Fill(ds.OffsetTypes);
            qualityControlLevelsTableAdapter.Fill(ds.QualityControlLevels);
            methodsTableAdapter.Fill(ds.Methods);
            samplesTableAdapter.Fill(ds.Samples);
            sourcesTableAdapter.Fill(ds.Sources);
            qualifiersTableAdapter.Fill(ds.Qualifiers);



            return ds;

        }


        /// <summary>
        /// DataValue creation. Converts a ValuesDataSet to the XML schema ValueSingleVariable
        /// If variable is null, it will return all
        /// If variable has extra options (variable:code/Value=Key/Value=Key)
        /// 
        /// </summary>
        /// <param name="ds">Dataset that you want converted</param>
        /// <param name="variable">Variable that you want to use to place limits on the returned data</param>
        /// <returns></returns>
        public static List<ValueSingleVariable> dataset2ValuesList(ValuesDataSet ds, VariableParam variable)
        {


            List<ValueSingleVariable> tsTypeList = new List<ValueSingleVariable>();


            /* logic
             * if there is a variable that has options, then get a set of datarows
           * using a select clause
             * use an enumerator, since it is generic
             * */
            IEnumerator dataValuesEnumerator;

            ValuesDataSet.DataValuesRow[] dvRows; // if we find options, we need to use this.

            String select = OdValuesCommon.CreateValuesWhereClause(variable, null);

            if (select.Length > 0)
            {
                dvRows = (ValuesDataSet.DataValuesRow[])ds.DataValues.Select(select.ToString());

                dataValuesEnumerator = dvRows.GetEnumerator();

            }
            else
            {
                dataValuesEnumerator = ds.DataValues.GetEnumerator();
            }

            //  foreach (ValuesDataSet.DataValuesRow aRow in ds.DataValues){
            while (dataValuesEnumerator.MoveNext())
            {
                ValuesDataSet.DataValuesRow aRow = (ValuesDataSet.DataValuesRow)dataValuesEnumerator.Current;
                try
                {
                    ValueSingleVariable tsTypeValue = new ValueSingleVariable();

                    #region DateTime Standard
                    tsTypeValue.dateTime = Convert.ToDateTime(aRow.DateTime);
                    //tsTypeValue.dateTimeSpecified = true;
                    DateTime temprealdate;



                    //<add key="returnUndefinedUTCorLocal" value="Undefined"/>
                    if (ConfigurationManager.AppSettings["returnUndefinedUTCorLocal"].Equals("Undefined"))
                    {
                        temprealdate = Convert.ToDateTime(aRow.DateTime); // not time zone shift

                    }
                    else if (ConfigurationManager.AppSettings["returnUndefinedUTCorLocal"].Equals("Local"))
                    {

                        TimeSpan zone = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
                        Double offset = Convert.ToDouble(aRow.UTCOffset);

                        if (zone.TotalHours.Equals(offset))
                        {
                            // zone is the same as server. Shift
                            temprealdate = DateTime.SpecifyKind(Convert.ToDateTime(aRow.DateTime), DateTimeKind.Local);
                        }
                        else
                        {
                            //// zone is not the same. Output in UTC.
                            //temprealdate = DateTime.SpecifyKind(Convert.ToDateTime(aRow.DateTime), DateTimeKind.Utc);
                            //// correct time with shift.
                            //temprealdate = temprealdate.AddHours(offset);

                            // just use the DateTime UTC
                            temprealdate = DateTime.SpecifyKind(Convert.ToDateTime(aRow.DateTimeUTC), DateTimeKind.Utc);

                        }
                    }
                    else if (ConfigurationManager.AppSettings["returnUndefinedUTCorLocal"].Equals("UTC"))
                    {
                        temprealdate = DateTime.SpecifyKind(Convert.ToDateTime(aRow.DateTimeUTC), DateTimeKind.Utc);

                    }
                    else
                    {
                        temprealdate = Convert.ToDateTime(aRow.DateTime); // not time zone shift


                    }

                    temprealdate = Convert.ToDateTime(aRow.DateTime); // not time zone shift
                    #endregion
                    #region DateTimeOffset Failed
                    /// using XML overrides
                    // Attemp to use DateTimeOffset in xml Schema
                    //TimeSpan zone = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
                    //Double offset = Convert.ToDouble(aRow.UTCOffset);

                    //DateTimeOffset temprealdate;
                    //temprealdate = new DateTimeOffset(Convert.ToDateTime(aRow.DateTime), 
                    //    new TimeSpan(Convert.ToInt32(offset),0,0));
                    //DateTimeOffset temprealdate;
                    //temprealdate = new DateTimeOffset(Convert.ToDateTime(aRow.DateTime),
                    //    new TimeSpan(Convert.ToInt32(offset), 0, 0));

                    //tsTypeValue.dateTime = temprealdate;
                    //tsTypeValue.dateTimeSpecified = true;
                    #endregion

                    //tsTypeValue.censored = string.Empty;
                    if (string.IsNullOrEmpty(aRow.Value.ToString()))
                        continue;
                    else
                        tsTypeValue.Value = Convert.ToDecimal(aRow.Value);

                    try
                    {

                        tsTypeValue.censorCode = (CensorCodeEnum)Enum.Parse(typeof(CensorCodeEnum), aRow.CensorCode, true);
                        tsTypeValue.censorCodeSpecified = true;

                        if (!aRow.IsOffsetTypeIDNull())
                        {
                            tsTypeValue.offsetTypeID = aRow.OffsetTypeID;
                            tsTypeValue.offsetTypeIDSpecified = true;

                            // enabled to fix issue with hydroobjects
                            ValuesDataSet.OffsetTypesRow off = ds.OffsetTypes.FindByOffsetTypeID(aRow.OffsetTypeID);


                            ValuesDataSet.UnitsRow offUnit = ds.Units.FindByUnitsID(off.OffsetUnitsID);
                            tsTypeValue.offsetUnitsCode = offUnit.UnitsID.ToString();
                            tsTypeValue.offsetUnitsAbbreviation = offUnit.UnitsAbbreviation;
                        }

                        // offset value may be separate from the units... anticpating changes for USGS
                        if (!aRow.IsOffsetValueNull())
                        {
                            tsTypeValue.offsetValue = aRow.OffsetValue;
                            tsTypeValue.offsetValueSpecified = true;
                        }


                        ValuesDataSet.MethodsRow meth = ds.Methods.FindByMethodID(aRow.MethodID);
                        tsTypeValue.methodID = aRow.MethodID;
                        tsTypeValue.methodIDSpecified = true;


                        if (!aRow.IsQualifierIDNull())
                        {
                            ValuesDataSet.QualifiersRow qual = ds.Qualifiers.FindByQualifierID(aRow.QualifierID);
                            if (qual != null)
                            {
                                tsTypeValue.qualifiers = qual.QualifierCode;

                            }
                        }


                        ValuesDataSet.QualityControlLevelsRow qcl =
                            ds.QualityControlLevels.FindByQualityControlLevelID(aRow.QualityControlLevelID);
                        string qlName = qcl.Definition.Replace(" ", "");

                        if (Enum.IsDefined(typeof(QualityControlLevelEnum), qlName))
                        {
                            tsTypeValue.qualityControlLevel = (QualityControlLevelEnum)
                                                              Enum.Parse(
                                                                  typeof(QualityControlLevelEnum), qlName, true);
                            if (tsTypeValue.qualityControlLevel != QualityControlLevelEnum.Unknown)
                            {
                                tsTypeValue.qualityControlLevelSpecified = true;
                            }
                        }
                        //}
                        tsTypeValue.sourceID = aRow.SourceID;
                        tsTypeValue.sourceIDSpecified = true;

                        if (!aRow.IsSampleIDNull())
                        {
                            tsTypeValue.sampleID = aRow.SampleID;
                            tsTypeValue.sampleIDSpecified = true;
                        }


                    }
                    catch (Exception e)
                    {
                        log.Debug("Error generating a value " + e.Message);
                        // non fatal exceptions
                    }

                    tsTypeList.Add(tsTypeValue);
                }
                catch (Exception e)
                {
                    //  ignore any value errors
                }

            }
            return tsTypeList;

        }

        public static void addCategoricalInformation(List<ValueSingleVariable> variables, int variableID, VariablesDataset vds)
        {

            foreach (ValueSingleVariable variable in variables)
            {
                string selectquery = String.Format("VariableID = {0} AND DataValue = {1}", variableID.ToString(),
                                                   variable.Value);
                DataRow[] rows = vds.Categories.Select(selectquery);
                if (rows.Length > 0)
                {
                    variable.codedVocabulary = true;
                    variable.codedVocabularySpecified = true;
                    variable.codedVocabularyTerm = (string)rows[0]["CategoryDescription"];
                }

            }

        }


        /// <summary>
        /// Method to generate a list of qualifiers in a ValuesDataSet
        /// This is done as a separate method since Values can could contain other VariableValue Types
        ///
        /// </summary>
        /// <param name="ds">ValuesDataSet with the values used in the timeSeries</param>
        /// <returns></returns>
        public static List<qualifier> datasetQualifiers(ValuesDataSet ds)
        {
            /* generate a list
             * create a distinct DataSet
             * - new data view
             * - set filter (no nulls)
             * - use toTable with unique to get unique list
             * foreach to generate qualifiers
             * */

            List<qualifier> qualifiers = new List<qualifier>();
            try
            {
                DataView qview = new DataView(ds.DataValues);
                qview.RowFilter = "QualifierID is not Null";
                DataTable qids = qview.ToTable("qualifiers", true, new string[] { "QualifierID" });

                foreach (DataRow q in qids.Rows)
                {
                    try
                    {
                        Object qid = q["QualifierID"];

                        ValuesDataSet.QualifiersRow qual = ds.Qualifiers.FindByQualifierID((int)qid);
                        if (qual != null)
                        {
                            qualifier qt = new qualifier();
                            qt.qualifierID = qual.QualifierID.ToString();

                            if (!qual.IsQualifierCodeNull()) qt.qualifierCode = qual.QualifierCode;
                            if (!String.IsNullOrEmpty(qual.QualifierDescription)) qt.Value = qual.QualifierDescription;
                            qualifiers.Add(qt);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Error("Error generating a qualifier " + q.ToString() + e.Message);
                    }
                }
                return qualifiers;
            }

            catch (Exception e)
            {
                log.Error("Error generating a qualifiers " + e.Message);
                // non fatal exceptions
                return null;
            }
        }

        /// <summary>
        /// Method to generate a list of qualifiers in a ValuesDataSet
        /// This is done as a separate method since Values can could contain other VariableValue Types
        ///
        /// </summary>
        /// <param name="ds">ValuesDataSet with the values used in the timeSeries</param>
        /// <returns></returns>
        public static List<MethodType> datasetMethods(ValuesDataSet ds)
        {
            /* generate a list
             * create a distinct DataSet
             * - new data view
             * - set filter (no nulls)
             * - use toTable with unique to get unique list
             * foreach to generate qualifiers
             * */
            string COLUMN = "MethodID";
            string TABLENAME = "methods";
            List<MethodType> list = new List<MethodType>();
            try
            {
                DataView view = new DataView(ds.DataValues);
                view.RowFilter = COLUMN + " is not Null";
                DataTable ids = view.ToTable(TABLENAME, true, new string[] { COLUMN });

                foreach (DataRow r in ids.Rows)
                {
                    try
                    {
                        Object aId = r[COLUMN];
                        // edit here
                        ValuesDataSet.MethodsRow method = ds.Methods.FindByMethodID((int)aId);
                        if (method != null)
                        {
                            MethodType t = new MethodType();
                            t.methodID = method.MethodID;
                            t.methodIDSpecified = true;
                            if (!String.IsNullOrEmpty(method.MethodDescription)) t.MethodDescription = method.MethodDescription;
                            if (!method.IsMethodLinkNull()) t.MethodLink = method.MethodLink;
                            list.Add(t);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Error("Error generating a qualifier " + r.ToString() + e.Message);
                    }
                }
                return list;
            }

            catch (Exception e)
            {
                log.Error("Error generating a qualifiers " + e.Message);
                // non fatal exceptions
                return null;
            }
        }

        /// <summary>
        /// Method to generate a list of Sources in a ValuesDataSet
        /// This is done as a separate method since Values can could contain other VariableValue Types
        ///
        /// </summary>
        /// <param name="ds">ValuesDataSet with the values used in the timeSeries</param>
        /// <returns></returns>
        public static List<SourceType> datasetSources(ValuesDataSet ds)
        {
            /* generate a list
             * create a distinct DataSet
             * - new data view
             * - set filter (no nulls)
             * - use toTable with unique to get unique list
             * foreach to generate qualifiers
             * */
            string COLUMN = "SourceID";
            string TABLENAME = "sources";
            List<SourceType> list = new List<SourceType>();
            try
            {
                DataView view = new DataView(ds.DataValues);
                view.RowFilter = COLUMN + " is not Null";
                DataTable ids = view.ToTable(TABLENAME, true, new string[] { COLUMN });

                foreach (DataRow r in ids.Rows)
                {
                    try
                    {
                        Object aId = r[COLUMN];
                        // edit here
                        ValuesDataSet.SourcesRow source = ds.Sources.FindBySourceID((int)aId);
                        if (source != null)
                        {
                            SourceType t = new SourceType();
                            t.sourceID = source.SourceID;
                            t.sourceIDSpecified = true;
                            if (!String.IsNullOrEmpty(source.Organization)) t.Organization = source.Organization;
                            t.SourceDescription = source.SourceDescription;
                            if (!source.IsSourceLinkNull()) t.SourceLink = source.SourceLink;
                            // create a contact
                            // only one for now

                            ContactInformationType contact = new ContactInformationType();
                            contact.TypeOfContact = "main";
                            if (!String.IsNullOrEmpty(source.ContactName)) contact.ContactName = source.ContactName;
                            if (!String.IsNullOrEmpty(source.Email)) contact.Email = source.Email;
                            if (!String.IsNullOrEmpty(source.Phone)) contact.Phone = source.Phone;
                            StringBuilder address = new StringBuilder();

                            if (!String.IsNullOrEmpty(source.Address)) address.Append(source.Address + System.Environment.NewLine);
                            if (!String.IsNullOrEmpty(source.City)
                                && !String.IsNullOrEmpty(source.State)
                                && !String.IsNullOrEmpty(source.ZipCode))
                                address.AppendFormat(",{0}, {1} {2}", source.City, source.State, source.ZipCode);


                            contact.Address = address.ToString();

                            //ContactInformationType[] contacts = new ContactInformationType[1];
                            // contacts[0] = contact;
                            // t.ContactInformation = contacts;
                            t.ContactInformation = contact;
                            list.Add(t);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Error("Error generating a qualifier " + r.ToString() + e.Message);
                    }
                }
                return list;
            }

            catch (Exception e)
            {
                log.Error("Error generating a qualifiers " + e.Message);
                // non fatal exceptions
                return null;
            }
        }

        /// <summary>
        /// Method to generate a list of offset (from OD OffsetTypes table) in a ValuesDataSet
        /// This is done as a separate method since Values can could contain other VariableValue Types
        ///
        /// </summary>
        /// <param name="ds">ValuesDataSet with the values used in the timeSeries</param>
        /// <returns></returns>
        public static List<OffsetType> datasetOffsetTypes(ValuesDataSet ds)
        {
            /* generate a list
             * create a distinct DataSet
             * - new data view
             * - set filter (no nulls)
             * - use toTable with unique to get unique list
             * foreach to generate qualifiers
             * */
            string COLUMN = "OffsetTypeID";
            string TABLENAME = "offsetTypes";
            List<OffsetType> list = new List<OffsetType>();
            try
            {
                DataView view = new DataView(ds.DataValues);
                view.RowFilter = COLUMN + " is not Null";
                DataTable ids = view.ToTable(TABLENAME, true, new string[] { COLUMN });

                foreach (DataRow r in ids.Rows)
                {
                    try
                    {
                        Object aId = r[COLUMN];
                        // edit here
                        ValuesDataSet.OffsetTypesRow offset = ds.OffsetTypes.FindByOffsetTypeID((int)aId);
                        if (offset != null)
                        {
                            OffsetType t = new OffsetType();
                            t.offsetTypeID = offset.OffsetTypeID;
                            t.offsetTypeIDSpecified = true;

                            if (!String.IsNullOrEmpty(offset.OffsetDescription)) t.offsetDescription = offset.OffsetDescription;

                            ValuesDataSet.UnitsRow offUnit = ds.Units.FindByUnitsID(offset.OffsetUnitsID);
                            string offUnitsCode;
                            string offUnitsName = null;
                            string offUnitsAbbreviation = null;
                            if (!String.IsNullOrEmpty(offUnit.UnitsAbbreviation)) offUnitsAbbreviation = offUnit.UnitsAbbreviation;
                            if (!String.IsNullOrEmpty(offUnit.UnitsName)) offUnitsName = offUnit.UnitsName;
                            if (offUnit != null)
                                t.units = CuahsiBuilder.CreateUnitsElement(
                                    null, offUnit.UnitsID.ToString(), offUnitsAbbreviation, offUnitsName);

                            list.Add(t);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Error("Error generating a qualifier " + r.ToString() + e.Message);
                    }
                }
                return list;
            }

            catch (Exception e)
            {
                log.Error("Error generating a qualifiers " + e.Message);
                // non fatal exceptions
                return null;
            }
        }

    }
}
