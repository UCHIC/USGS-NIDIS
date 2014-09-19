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
/// Summary description for CallSnotel
/// </summary>
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
    using SitesTableAdapter = WaterOneFlow.odm.v1_1.siteInfoDataSetTableAdapters.sitesTableAdapter;
    using SeriesCatalogTableAdapter = WaterOneFlow.odm.v1_1.seriesCatalogDataSetTableAdapters.SeriesCatalogTableAdapter;
    using VariablesTableAdapter = WaterOneFlow.odm.v1_1.VariablesDatasetTableAdapters.VariablesTableAdapter;
    using System.Net;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Data.Common;
    using System.Threading;
    public class CallSnotel
    {

        int timeCollected = 12;
        string dateFormat = "MMddyy";
        public CallSnotel()
        {
            //
            // TODO: Add constructor logic here
            //

        }
        public ValuesDataSet getDataValues(int SiteID, int VariableID, DateTime StartDate, DateTime EndDate)
        {
            seriesCatalogDataSet scDS = new seriesCatalogDataSet();
            SeriesCatalogTableAdapter seriesTableAdapter = new SeriesCatalogTableAdapter();
            seriesTableAdapter.Connection.ConnectionString = odws.Config.ODDB();

            seriesTableAdapter.FillBySiteIDVariableID(scDS.SeriesCatalog, SiteID, VariableID);
            DataSet oDS = new DataSet();
            try
            {
                oDS = getValuesWork(scDS.SeriesCatalog, StartDate, EndDate);
            }
            catch (Exception ex)
            {
                return emptyValues(ex.Message);
            }

            return newTable(scDS.SeriesCatalog, ref oDS);



        }
        private ValuesDataSet emptyValues(string errorMsg)
        {
          
            return basicValuesDataSet();

        }


        private DataSet getValuesWork(seriesCatalogDataSet.SeriesCatalogDataTable series, DateTime startDate, DateTime endDate)
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


            String state = sites.sites[0].State.ToLower().Trim().Replace(' ', '_');
            String ftpAll = "ftp://ftp.wcc.nrcs.usda.gov/data/snow/snotel/cards/{0}/{1}_all.txt";
            String ftpYear = "ftp://ftp.wcc.nrcs.usda.gov/data/snow/snotel/cards/{0}/{1}_{2}.tab";


            //case 1: all dates are in the 'archive' file
            if (startresult >= 0 && endresult > 0)
            {
                WebRequest[] objRequest = new WebRequest[1];
                objRequest[0] = WebRequest.Create(String.Format(ftpAll, state.ToLower(), siteCode.ToLower()));
                return fillTable(objRequest, startdate.ToString(dateFormat), enddate.ToString(dateFormat));
            }
            //case 2: some of the dates are in the 'archiv'e file and the others are in the 'current' file
            else if (startresult >= 0 && endresult < 0)
            {
                WebRequest[] objRequest = new WebRequest[2];
                objRequest[0] = WebRequest.Create(String.Format(ftpAll, state.ToLower(), siteCode.ToLower()));
                objRequest[1] = WebRequest.Create(String.Format(ftpYear, state.ToLower(), siteCode.ToLower(), getCurrWaterYear()));
                return fillTable(objRequest, startdate.ToString(dateFormat), enddate.ToString(dateFormat));
            }

            //case 3: all dates are in the 'current' file
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
        private DataSet fillTable(WebRequest[] request, string startDate, string endDate)
        {
            Boolean noResponse = true;
            int count = 0;
            DataSet oDS = new DataSet();
            oDS.DataSetName = "SNOTEL";
            oDS.Tables.Clear();
            while (noResponse && count < 10)
            {
                count++;
                //try
                //{

                WebResponse[] response = new WebResponse[request.Length];
                Stream[] dataStream = new Stream[request.Length];
                StreamReader[] reader = new StreamReader[request.Length];
                List<string> responseFromServer = new List<string>();//string[request.Length];

                for (int i = 0; i < request.Length; i++)
                {
                    try
                    {
                        response[i] = request[i].GetResponse();
                        dataStream[i] = response[i].GetResponseStream();
                        reader[i] = new StreamReader(dataStream[i]);
                        responseFromServer.Add(reader[i].ReadToEnd());
                    }
                    catch (Exception ex)
                    {

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
                if (responseFromServer.Count > 0)
                {
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
                    for (int i = 0; i < responseFromServer.Count; i++)
                    {
                        //Here we apply our regular expression to our string using the Match object.
                        if (responseFromServer[i] != null)
                        {
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

                }
                //catch (WebException ex)
                //{
                //    ////if (ex.Message.Contains("not found"))
                //    ////    throw new Exception("SNOTEL File not Found");
                //    //// else 
                //    //if (count < 9)
                //    //{
                //    //    Console.WriteLine("cannot connect....." + ex.ToString());
                //    //    Console.ReadLine();
                //    //}
                //    //else
                //    //{
                //    //    if (ex.Message.Contains("not found"))
                //    //        throw new Exception("SNOTEL File not Found", ex);
                //    //    throw new Exception("SNOTEL Website is Offline", ex);
                //    //}
                //}

            }
            return oDS;
        }
        /*
         * getcurrWaterYear: calculates the current water year
         * October 1- September 30 uses Septembers year as water year;
         * so 10/1/2009 is in the 2010 water year
         */
        private string getCurrWaterYear()
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
        private void createColumns(Match oM, ref DataSet oDS)
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
        private void createRows(Match oM, ref DataSet oDS)
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
        private void createRows(string data, DateTime startDate, DateTime endDate, ref DataSet oDS)
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
        private DateTime convertDate(string date, int UTCoffset)
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
        private ValuesDataSet newTable(seriesCatalogDataSet.SeriesCatalogDataTable series, ref DataSet oDS)
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





        // fills dataset with basic tables
        private static ValuesDataSet basicValuesDataSet()
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