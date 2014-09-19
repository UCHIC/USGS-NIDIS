using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Data;
using System.IO;
using Logger;
using System.Diagnostics;

namespace NCDC_Harvester
{
    class harvestData
    {
        string projectName = Properties.Settings.Default.projectName;
        clsDatabase db = new clsDatabase();
        public harvestData()
        {
            //List<Sites> siteList = db.getSiteList();
            List<sitedata> siteList = db.getSiteList();
            int count = 1;
            db.updateSeriesCatalag(); 
            //foreach (Sites site in siteList)
            foreach(sitedata site in siteList)
            { 
                DBLogging.WriteLog(projectName, "Log", "harvestData" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", string.Format("Requesting data for Site:{1}, {0}. {2}/{3}", site.SiteName, site.SiteCode, count, siteList.Count));                   
                //string data = requestData("ACW00011604");
                try
                {
                    string data = requestData(site.SiteCode);
                    DataTable table = createTable(data);
                    getNeededData(table, site);
                    count++;
                }
                catch (Exception ex)
                {
                    DBLogging.WriteLog(projectName, "Error", "harvestData" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", string.Format("Error on Site:{1}, {0}. {2}", site.SiteName, site.SiteCode, ex.Message));                   
                
                }
            }
            DBLogging.WriteLog(projectName, "Log", "harvestData" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", "Done requesting data from NCDC ftp site");                   
                
            DBLogging.WriteLog(projectName, "Log", "harvestData" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()","Building Series Catalog");                   
            db.updateSeriesCatalag(); 
        }
        //private void getNeededData(DataTable table, Sites site)
        private void getNeededData(DataTable table, sitedata site)
        {//get a list of all the variables in the table
            var x = (from r in table.AsEnumerable()
                     select r["Element"]).Distinct().ToList();
            //loop through all of the variables
            foreach (var variable in x)
            {
                if (variable.ToString() == "PRCP")
                {
                    Variables vari = db.getVariableID(variable.ToString());
                    int seriesID = db.getSeriesID(site.SiteID, vari.VariableID);
                    DateTime startdate;
                    if (seriesID >= 0)
                    {
                        //get the end date for the current variable
                        startdate = db.getSeriesLastDate(seriesID);
                    }
                    else
                    {
                        //if series is not in table set start date
                        startdate = new DateTime(1900, 01, 01);
                    }
                    if (Math.Abs(startdate.Subtract(DateTime.Today).Days) > 3)
                    {

                        string expression = "Date > '" + startdate.ToString() + "' AND Date <= '" + DateTime.Today.Add(new TimeSpan(-1, 0, 0, 0)).ToString() + "' AND Element = '" + variable + "'";
                        DataRow[] selectedrows = table.Select(expression);
                        int rows = db.saveDataValues(selectedrows, vari, site);
                        DBLogging.WriteLog(projectName, "Log", "harvestData" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", string.Format("{0} rows saved. Date > {1}, Site:{2}, Variable:{3} ", rows, startdate, site.SiteCode, variable.ToString()));
                    }
                }
            }            
        } 
        //Parse Data and store it in a datatable
        private void NCDCRows(ref DataTable NCDCTable, string data)
        {
            foreach (string strField in data.Split('\n'))
            {
                if (strField.Length > 0)
                {
                    string sitecode = strField.Substring(0, 11);
                    string year = strField.Substring(11, 4);
                    string month = strField.Substring(15, 2);
                    string element = strField.Substring(17, 4);
                    int day = 1;
                    for (int i = 21; i < strField.Length; i = i + 8)
                    {
                        try
                        {
                            DataRow r = NCDCTable.NewRow();
                            r["ID"] = sitecode;
                            r["Year"] = year;
                            r["Month"] = month;
                            r["Element"] = element;
                            r["Day"] = day;
                            r["Date"] = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), day);
                            r["Value"] = strField.Substring(i, 5);                            
                            r["MFlag"] = strField.Substring(i + 5, 1); 
                            r["QFlag"] = strField.Substring(i + 6, 1);
                            r["SFlag"] = strField.Substring(i + 7, 1); 
                            day++;
                            NCDCTable.Rows.Add(r);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }
            }
        }
        // create a datatable from the data read from the website
        private DataTable createTable(string data)
        {
            DataTable NCDCTable = new DataTable();
            NCDCColumns(ref NCDCTable);
            NCDCRows(ref NCDCTable, data);
            return NCDCTable;
        }
        // add all the columns to the Datatable from the file
        private void NCDCColumns(ref DataTable NCDCTable)
        {
            NCDCTable.Columns.Add("ID");
            NCDCTable.Columns.Add("Year");
            NCDCTable.Columns.Add("Month");
            NCDCTable.Columns.Add("Element");
            NCDCTable.Columns.Add("Day");
            NCDCTable.Columns.Add("Date",typeof(DateTime));
            NCDCTable.Columns.Add("Value");
            NCDCTable.Columns.Add("MFlag");
            NCDCTable.Columns.Add("QFlag");
            NCDCTable.Columns.Add("SFlag");    
        }
        //query the ncdc ftp site to get the files
        private string requestData(string siteCode)
        {
            string url = string.Format("ftp://ftp.ncdc.noaa.gov/pub/data/ghcn/daily/all/{0}.dly", siteCode.Trim());

            int count = 0;
            while (true)
            {
                try
                {
                    WebRequest request = WebRequest.Create(url);
                    request.Timeout = Timeout.Infinite;
                    WebResponse response = request.GetResponse();
                    Stream dataStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);
                    string responseFromServer = reader.ReadToEnd();
                    dataStream.Close();
                    reader.Close();
                    return responseFromServer;
                }
                catch (Exception ex)
                {
                    if (count >= 10)
                    {
                        throw new Exception("Error accessing Data URL: (" + url + ")", ex);
                    }
                    else
                    {
                        count++;
                        Thread.Sleep(100);
                    }
                }
            }
        }
    
    }
}