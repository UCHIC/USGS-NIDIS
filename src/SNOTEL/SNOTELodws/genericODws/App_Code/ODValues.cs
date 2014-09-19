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

    using WaterOneFlow.odm.v1_1;
    using SitesTableAdapter = WaterOneFlow.odm.v1_1.siteInfoDataSetTableAdapters.sitesTableAdapter;
    using SeriesCatalogTableAdapter = WaterOneFlow.odm.v1_1.seriesCatalogDataSetTableAdapters.SeriesCatalogTableAdapter;
    using VariablesTableAdapter = WaterOneFlow.odm.v1_1.VariablesDatasetTableAdapters.VariablesTableAdapter;
    using System.Net;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Data.Common;
    using System.Threading;
    


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


        #region odm 1 series based
        //public static ValuesDataSet GetValuesDataSet(int siteID)
        //{
        //    ValuesDataSet ds = basicValuesDataSet();

        //    ValuesDataSetTableAdapters.DataValuesTableAdapter valuesTableAdapter = new DataValuesTableAdapter();
        //    valuesTableAdapter.Connection.ConnectionString = Config.ODDB();

        //    try
        //    {
        //        valuesTableAdapter.FillBySiteID(ds.DataValues, siteID);
        //    }
        //    catch (Exception e)
        //    {
        //        log.Fatal("Cannot retrieve information from database: " + e.Message); //+ valuesTableAdapter.Connection.DataSource
        //    }


        //    return ds;

        //}

        //public static ValuesDataSet GetValuesDataSet(int? siteID, int? VariableID)
        //{


        //    ValuesDataSet ds = basicValuesDataSet();
        //    if (!siteID.HasValue || !VariableID.HasValue) return ds;
        //    ValuesDataSetTableAdapters.DataValuesTableAdapter valuesTableAdapter = new DataValuesTableAdapter();
        //    valuesTableAdapter.Connection.ConnectionString = Config.ODDB();

        //    valuesTableAdapter.FillBySiteIDVariableID(ds.DataValues, siteID.Value, VariableID.Value);

        //    return ds;

        //}

        //public static ValuesDataSet GetValuesDataSet(int? siteID, int? VariableID, W3CDateTime BeginDateTime, W3CDateTime EndDateTime)
        //{

        //    ValuesDataSet ds = basicValuesDataSet();
        //    if (!siteID.HasValue || !VariableID.HasValue) return ds;
        //    ValuesDataSetTableAdapters.DataValuesTableAdapter valuesTableAdapter = new DataValuesTableAdapter();
        //    valuesTableAdapter.Connection.ConnectionString = Config.ODDB();

        //    valuesTableAdapter.FillBySiteIdVariableIDBetweenDates(ds.DataValues, siteID.Value, VariableID.Value, BeginDateTime.DateTime, EndDateTime.DateTime);

        //    return ds;

        //}


        static int timeCollected = 12;
        static string dateFormat = "MMddyy";


        public static ValuesDataSet GetValuesDataSet(int siteID)
        {
            ValuesDataSet ds = basicValuesDataSet();
            VariablesTableAdapter variablesTableAdapter = new VariablesTableAdapter();
            variablesTableAdapter.Connection.ConnectionString = odws.Config.ODDB();
            DataTable Variables = variablesTableAdapter.GetDataBySiteID(siteID);

            foreach (DataRow row in Variables.Rows)
            {
                return getDataValues(siteID, Convert.ToInt32(row["VariableID"]), new DateTime(1900, 01, 01), DateTime.Now);
            }

            return ds;
        }

        public static ValuesDataSet GetValuesDataSet(int siteID, W3CDateTime BeginDateTime, W3CDateTime EndDateTime)
        {
            ValuesDataSet ds = basicValuesDataSet();
            VariablesTableAdapter variablesTableAdapter = new VariablesTableAdapter();
            variablesTableAdapter.Connection.ConnectionString = odws.Config.ODDB();
            DataTable Variables = variablesTableAdapter.GetDataBySiteID(siteID);

            foreach (DataRow row in Variables.Rows)
            {
                return getDataValues(siteID, 1, BeginDateTime.DateTime, EndDateTime.DateTime);
            }

            return ds;
        }

        public static ValuesDataSet GetValuesDataSet(int? siteID, int? VariableID)
        {
            ValuesDataSet ds = basicValuesDataSet();
            if (!siteID.HasValue || !VariableID.HasValue) return ds;

            return getDataValues(siteID.Value, VariableID.Value, new DateTime(1900, 01, 01), DateTime.Now);
        }

        public static ValuesDataSet GetValuesDataSet(int? siteID, int? VariableID, W3CDateTime BeginDateTime,
                                                     W3CDateTime EndDateTime)
        {
            ValuesDataSet ds = basicValuesDataSet();
            if (!siteID.HasValue || !VariableID.HasValue) return ds;

            return getDataValues(siteID.Value, VariableID.Value, BeginDateTime.DateTime, EndDateTime.DateTime);
        }

        public static ValuesDataSet getDataValues(int SiteID, int VariableID, DateTime StartDate, DateTime EndDate)
        {
            seriesCatalogDataSet scDS = new seriesCatalogDataSet();
            SeriesCatalogTableAdapter seriesTableAdapter = new SeriesCatalogTableAdapter();
            seriesTableAdapter.Connection.ConnectionString = odws.Config.ODDB();

            seriesTableAdapter.FillBySiteIDVariableID(scDS.SeriesCatalog, SiteID, VariableID);
            DataSet oDS = new DataSet();
            try
            {
                //   oDS = getValuesWork(scDS.SeriesCatalog, StartDate, EndDate);

                Thread thread = new Thread(delegate()
                {
                    oDS = getValuesWork(scDS.SeriesCatalog, StartDate, EndDate);
                });
                thread.Start();
                while (thread.IsAlive)
                {
                    Thread.Sleep(150);
                }
                return newTable(scDS.SeriesCatalog, ref oDS);
            }
            catch (Exception e)
            {
                log.Fatal("Cannot retrieve information from database: " + e.Message); //+ valuesTableAdapter.Connection.DataSource
            }
            return basicValuesDataSet();
        }


        private static DataSet getValuesWork(seriesCatalogDataSet.SeriesCatalogDataTable series, DateTime startDate, DateTime endDate)
        {
            string siteCode = series[0].SiteCode;
            string curryear = ((Convert.ToInt32(getCurrWaterYear()) - 1).ToString()).Substring(2);

            int endresult = endDate.CompareTo(series[0].EndDateTime);
            int startresult = startDate.CompareTo(series[0].BeginDateTime);
            /*Less than zero   This instance is earlier than value. 
              begin: change to values[0].
              end: use enddatetime*/

            /*check to see if the start date requested is before or after the start datd in the 
            database. if after use given date, if before use date in database.*/
            DateTime startdate;
            if (startresult < 0)
                startdate = (series[0].BeginDateTime);
            else
                startdate = startDate;
            /*check to see if the end date requested is before or after the end datd in the 
            database. if it is after after use date in database, if before use given date. */
            DateTime enddate;
            if (endresult < 0)
                enddate = endDate;
            else
                enddate = ((DateTime)series[0].EndDateTime);

            string endOfYear = "0930" + curryear;
            DateTime eoy = convertDate(endOfYear, 0);

            //reset result to get data for which files to open
            endresult = DateTime.Compare(eoy, endDate);
            startresult = DateTime.Compare(eoy, startDate);

            SitesTableAdapter sitesAdapter = new SitesTableAdapter();
            siteInfoDataSet sites = new siteInfoDataSet();
            sitesAdapter.Connection.ConnectionString = odws.Config.ODDB();
            sitesAdapter.FillBySiteID(sites.sites, series[0].SiteID);


            String state = sites.sites[0].State.ToLower().Trim();
            String ftpAll = "ftp://ftp.wcc.nrcs.usda.gov/data/snow/snotel/cards/{0}/{1}_all.txt";
            String ftpYear = "ftp://ftp.wcc.nrcs.usda.gov/data/snow/snotel/cards/{0}/{1}_{2}.tab";

            //case 1: all files are in the archive file
            if (startresult >= 0 && endresult > 0)
            {
                WebRequest[] objRequest = new WebRequest[1];
                objRequest[0] = WebRequest.Create(String.Format(ftpAll, state.ToLower(), siteCode.ToLower()));
                return fillTable(objRequest, startdate.ToString(dateFormat), enddate.ToString(dateFormat));
            }
            //case 2: half the files are in the archive file half are in the current file
            else if (startresult >= 0 && endresult < 0)
            {
                WebRequest[] objRequest = new WebRequest[2];
                objRequest[0] = WebRequest.Create(String.Format(ftpAll, state.ToLower(), siteCode.ToLower()));
                objRequest[1] = WebRequest.Create(String.Format(ftpYear, state.ToLower(), siteCode.ToLower(), getCurrWaterYear()));
                return fillTable(objRequest, startdate.ToString(dateFormat), enddate.ToString(dateFormat));
            }

            //case 3: all files are in the current file
            else//startresult>=0&& endresult>=0
            {
                WebRequest[] objRequest = new WebRequest[1];
                objRequest[0] = WebRequest.Create(String.Format(ftpYear, state.ToLower(), siteCode.ToLower(), getCurrWaterYear()));
                return fillTable(objRequest, startdate.ToString(dateFormat), enddate.ToString(dateFormat));
            }


        }
        /*
         * fill table: takes the URL(s) scrapes the data off the text file 
         * then fills a data table with requested variable for the 
         * requested time period
         */
        static private DataSet fillTable(WebRequest[] request, string startDate, string endDate)
        {
            Boolean noResponse = true;
            int count = 0;
            DataSet oDS = new DataSet();
            oDS.DataSetName = "SNOTEL";
            oDS.Tables.Clear();
            while (noResponse && count < 10)
            {
                count++;
                try
                {
                    WebResponse[] response = new WebResponse[request.Length];
                    Stream[] dataStream = new Stream[request.Length];
                    StreamReader[] reader = new StreamReader[request.Length];
                    string[] responseFromServer = new string[request.Length];

                    for (int i = 0; i < request.Length; i++)
                    {
                        response[i] = request[i].GetResponse();
                        dataStream[i] = response[i].GetResponseStream();
                        reader[i] = new StreamReader(dataStream[i]);
                        responseFromServer[i] = reader[i].ReadToEnd();
                    }
                    noResponse = false;

                    Regex[] regex = new Regex[request.Length + 1];
                    Match[] oM = new Match[request.Length + 1];

                    //get the columns of the Dataset
                    //Regex matches only the first line of the text file these are the headers
                    regex[0] = new Regex("-.*\n");
                    oM[0] = regex[0].Match(responseFromServer[0]);

                    //create columns from the header line
                    createColumns(oM[0], ref oDS);


                    //regex to match from the start date to end date in one file
                    if (request.Length < 2)
                    {
                        regex[1] = new Regex(startDate + "(.|\n)*" + endDate + ".*", RegexOptions.IgnoreCase);
                    }
                    else
                    {
                        //matches from start date to end of file.                             
                        regex[1] = new Regex(startDate + "(.|\n)*", RegexOptions.IgnoreCase);
                        //matches from 2nd line in file(1st line is headers) to the enddate
                        regex[2] = new Regex("\n(.|\n)*" + endDate + ".*\n", RegexOptions.IgnoreCase);
                    }
                    for (int i = 0; i < request.Length; i++)
                    {
                        //Here we apply our regular expression to our string using the Match object.
                        oM[i + 1] = regex[i + 1].Match(responseFromServer[i]);

                        if (oM[i + 1].ToString() != "")
                            //fill the rows of the table with data from the file
                            createRows(oM[i + 1], ref oDS);
                        else
                        {
                            Regex regex2 = new Regex("\n(.|\n)*");
                            createRows((regex2.Match(responseFromServer[i])).ToString(), convertDate(startDate, 0), convertDate(endDate, 0), ref oDS);
                        }
                        reader[i].Close();
                        response[i].Close();
                    }

                }
                catch (WebException ex)
                {
                    //if (ex.Message.Contains("not found"))
                    //    throw new Exception("SNOTEL File not Found");
                    // else 
                    if (count < 9)
                    {
                        Console.WriteLine("cannot connect....." + ex.ToString());
                        Console.ReadLine();
                    }
                    else
                    {
                        if (ex.Message.Contains("not found"))
                            throw new Exception("SNOTEL File not Found", ex);
                        throw new Exception("SNOTEL Website is Offline", ex);
                    }
                }

            }
            return oDS;
        }
        /*
         * getcurrWaterYear: calculates the current water year
         * October 1- September 30 uses Septembers year as water year;
         * so 10/1/2009 is in the 2010 water year
         */
        static private string getCurrWaterYear()
        {
            string currYear = DateTime.Now.Year.ToString();
            int intYear = Convert.ToInt32(currYear);
            DateTime eoy = new DateTime(intYear, 09, 30);
            if (DateTime.Today.CompareTo(eoy) < 0)
                return currYear;
            else
                return (intYear + 1).ToString();

        }
        /*
         * Create Columns
         * takes a REGEX match and usint a tab seperator creates columns for 
         * a data table using the text as its headers.
         */
        private static void createColumns(Match oM, ref DataSet oDS)
        {
            //int numOfCols = 0;
            char strDelimiter = '\t';

            DataTable variableTable = oDS.Tables.Add("Variables");

            foreach (String strFields in oM.ToString().Trim().Split(strDelimiter))
            {
                // numOfCols++;
                variableTable.Columns.Add(strFields, typeof(string));
            }

        }
        /*
         * Create Rows
         * takes a REGEX match and using a tab seperator creates rows for 
         * a data table using the text as its variables
         * used if there is no specific date time data
         */
        private static void createRows(Match oM, ref DataSet oDS)
        {
            int numOfCols = oDS.Tables["Variables"].Columns.Count;
            DataRow oRows;
            char[] strDelimiter = { '\t', '\n' };
            int i = 0;
            //create a new row.
            oRows = oDS.Tables["Variables"].NewRow();

            foreach (string strFields in oM.ToString().Trim().Split(strDelimiter))
            {
                // all fields must have a values so if there is nothing in the field enter an outlier 
                if (strFields.Trim() == "" || strFields == "    ")
                    oRows[oDS.Tables["Variables"].Columns[i]] = Convert.ToString(-9999);
                else
                    oRows[oDS.Tables["Variables"].Columns[i]] = strFields;

                i = i + 1;
                //if the row has as many columns as the Variables datatable does add it to the table;
                if (i == numOfCols)
                {
                    i = 0;
                    oDS.Tables["Variables"].Rows.Add(oRows); oRows = oDS.Tables["Variables"].NewRow();
                }
            }
        }
        /*
         * Create Rows
         * takes a REGEX match and using a tab seperator creates rows for 
         * a data table using the text as its variables
         * tests to make sure the data is in the correct time frame;
         */
        private static void createRows(string data, DateTime startDate, DateTime endDate, ref DataSet oDS)
        {
            int numOfCols = oDS.Tables["Variables"].Columns.Count;
            DataRow oRows;
            char[] strDelimiter = { '\t', '\n' };
            int i = 0;
            oRows = oDS.Tables["Variables"].NewRow();
            bool skip = false;
            foreach (string strFields in data.Trim().Split(strDelimiter))
            {
                if (i == 0)
                {
                    DateTime currDate = convertDate(strFields, 0);
                    int endresult = endDate.CompareTo(currDate);
                    int startresult = startDate.CompareTo(currDate);
                    //Less than zero,   (end/start)date is earlier than curr date
                    //equal zero. same date
                    //greater than zero, (end/start)date is later than curr date

                    //if the currdate is the same or after startdate AND before enddate include it
                    if (startresult < 0 && endresult > 0)
                        oRows[oDS.Tables["Variables"].Columns[i]] = strFields;
                    else//skip over and do not include any data from the row.
                        skip = true;
                    i++;

                }
                else
                {
                    if (!skip)
                    {
                        // all fields must have a values so if there is nothing in the field enter an outlier 
                        if (strFields.Trim() == "" || strFields == "    ")
                            oRows[oDS.Tables["Variables"].Columns[i]] = Convert.ToString(-9999);
                        else
                            oRows[oDS.Tables["Variables"].Columns[i]] = strFields;
                        i++;
                        if (i == numOfCols)
                        {
                            i = 0;
                            //if the row has as many columns as the Variables datatable does add it to the table;
                            oDS.Tables["Variables"].Rows.Add(oRows); oRows = oDS.Tables["Variables"].NewRow();
                        }
                    }
                    else
                        //make sure all the data is read from the row that is not required and then continue reading data
                        if (i == numOfCols - 1)
                        {
                            i = 0;
                            skip = false;
                        }
                        else
                            i++;
                }
            }
        }
        /*
         * convertDate: takes a string and a UTCoffset and converts it to a DateTime format
         * string format: mmddyy ex 100180 = October 10, 1980 the format from the SNOTEL website
         * UTCoffset= int ex -9
         */
        private static DateTime convertDate(string date, int UTCoffset)
        {
            string month, day, year;

            char[] monthChar = new char[2];
            date.CopyTo(0, monthChar, 0, 2);
            month = new string(monthChar);

            char[] dayChar = new char[2];
            date.CopyTo(2, dayChar, 0, 2);
            day = new string(dayChar);

            char[] yearChar = new char[2];
            date.CopyTo(4, yearChar, 0, 2);
            year = new string(yearChar);

            int yearInt = Convert.ToInt32(year);

            DateTime dt;

            if (yearInt < 40)
                dt = new DateTime(Convert.ToInt32("20" + year), Convert.ToInt32(month), Convert.ToInt32(day), timeCollected - UTCoffset, 0, 0);
            else
                dt = new DateTime(Convert.ToInt32("19" + year), Convert.ToInt32(month), Convert.ToInt32(day), timeCollected - UTCoffset, 0, 0);
            return dt;
        }
        /*
         * newTable
         * creates the Data Table required by rest of program to create the XML form
         */
        private static ValuesDataSet newTable(seriesCatalogDataSet.SeriesCatalogDataTable series, ref DataSet oDS)
        {
            ValuesDataSet ds = basicValuesDataSet();
            DataTable variableTable = ds.DataValues;
            SitesTableAdapter sitesAdapter = new SitesTableAdapter();
            siteInfoDataSet sites = new siteInfoDataSet();
            sitesAdapter.Connection.ConnectionString = odws.Config.ODDB();
            sitesAdapter.FillBySiteID(sites.sites, series[0].SiteID);
            int ValueID = 0;

            DataRow[] rows = oDS.Tables["Variables"].Select();
            //DataRow siteTable = sites.sites[0];
            foreach (DataRow row in rows)
            {
                ValueID++;
                int timezone = sites.sites[0].TimeZone;
                variableTable.Rows.Add(ValueID, series[0].SiteID, series[0].VariableID, DBNull.Value, DBNull.Value, "nc", 0, series[0].MethodID, series[0].SourceID, 0, 0, series[0].QualityControlLevelID, (row[0] == "") ? (-9999) : row[series[0].VariableCode.ToLower()], 0, convertDate(row[0].ToString(), 0), convertDate(row[0].ToString(), timezone), timezone);
                //"ValueID","SiteID","VariableID",""OffsetValue", "OffsetTypeID","CensorCode","QualifierID","MethodID","SourceID","SampleID","DerivedFromID","QualityControlLevelID", "Value",                                   "AccuracyStdDev","DateTime",                        "DateTimeUTC",                                               "UTCOffset"*/
            }
            return ds;
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
            IEnumerator dataValuesEnumerator ;

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
                            if (tsTypeValue.qualityControlLevel != QualityControlLevelEnum.Unknown){
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

        public static void addCategoricalInformation( List<ValueSingleVariable> variables, int variableID, VariablesDataset vds)
        {
            
            foreach (ValueSingleVariable variable in variables)
            {
                string selectquery = String.Format("VariableID = {0} AND DataValue = {1}", variableID.ToString(),
                                                   variable.Value);
                DataRow[] rows =  vds.Categories.Select(selectquery);
                if (rows.Length >0 )
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
