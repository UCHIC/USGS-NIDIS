using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using Logger;

namespace USBRHarvester
{
    public class FillUSBR 
    {
        clsDatabase db = new clsDatabase();
        string URLstring;
        DataSet oDS;
        string projectName = Properties.Settings.Default.projectName;
        public FillUSBR()
        {
            DBLogging.WriteLog(projectName, "Log", "FillUSBR" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", "USBRHarvester Has Begun Running");                    
            fillDatabase();
            DBLogging.WriteLog(projectName, "Log", "FillUSBR" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", "USBRHarvester Has Completed Running");
            Thread.Sleep(30);
            SendEmail.SendMessage("USBR Reservoir Harvester Completed", "USBR Reservoir Harvester has completed running, view the attached file for details", projectName, new TimeSpan(12, 0, 0));
            
        }

        private void fillDatabase()
        {
            //DBLogging.WriteLog(projectName, "Log", "FillUSBR" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", string.Format(""));
           List<Site> sites = clsUSBRList.createSiteList();
            int count = 0;
            foreach (Site s in sites)
            {
                count++;
                foreach (Variable v in s.variables)
                {
                    try
                    {
                        DateTime startDate;
                        int SID = db.SeriesExists(v.VariableName, s.SiteCode);
                        //if (SID > 0)
                        //    startDate = db.getEndDate(SID).Add(new TimeSpan(1, 0, 0, 0));
                        //else 
                            startDate = new DateTime(1938, 01, 01);
                        DateTime endDate = DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0));
                        if (endDate >= startDate)
                        {
                            DBLogging.WriteLog(projectName, "Log", "FillUSBR" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", string.Format("Gathering data for {1} {0}, {4}/{5} {2}-{3}", v.VariableName, s.SiteName, startDate, endDate, count, sites.Count));

                            string data = generateURL(s, v, startDate, endDate);
                            //MessageBox.Show(data);
                            //DBLogging.WriteLog(projectName, "Log", "FillUSBR" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", string.Format("\tdata recieved for {0} {1}", s.SiteName, v.VariableName));
                            createTable(v.VariableName, data);
                            saveSeries(s, v);
                        }
                    }
                    catch (Exception ex)
                    {
                        DBLogging.WriteLog(projectName, "Error", "FillUSBR" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", s.SiteName + " " + v.VariableName + " " + ex.Message);
                    }
                }

            }
        }

        private void saveSeries(Site s, Variable v)
        {
            int startind = 0;
            int endind = 0;
            //throw new NotImplementedException();
            //MessageBox.Show(URLstring);
            DateTime Start=FindStart(ref startind);
            DateTime End = FindEnd(ref endind);
            int numVals = calcValues(startind, endind);
            if (numVals > 0)
            {
                db.saveSeries(v, s, Start, End, numVals, getVUnitName());
            }
        }

        private string getVUnitName()
        {
            if (URLstring.Contains("acre"))
                return "acre foot";
            if(URLstring.Contains("cubic"))
                return "cubic feet per second"; 
            if (URLstring.Contains("megawatt"))
                return "megawatt hours";
            return "international foot";
        }

        DateTime FindStart( ref int startIndex)
        {
            for (int curRow = 0; curRow < oDS.Tables[0].Rows.Count; curRow++)
            {
                if (oDS.Tables[0].Rows[curRow][1].ToString().Trim() != "")
                {
                    startIndex = curRow;
                    return Convert.ToDateTime(oDS.Tables[0].Rows[curRow][0]);

                }
            }
            return DateTime.Now;
        }

        DateTime FindEnd(ref int endIndex)
        {
            for (int curRow = oDS.Tables[0].Rows.Count - 1; curRow >= 0; curRow--)
            {
                if (oDS.Tables[0].Rows[curRow][1].ToString().Trim() != "")
                {
                    endIndex = curRow;
                    DateTime endDate = Convert.ToDateTime(oDS.Tables[0].Rows[curRow][0]);
                    return endDate;
                }
            }
            return DateTime.Now;
        }

        int calcValues(int startIndex, int endIndex)
        {            
            return endIndex - startIndex;            
        }

        private string generateURL(Site s, Variable v, DateTime startDate, DateTime endDate)
        {
            Match URL;
            string url=string.Format("http://www.usbr.gov/uc/crsp/GetDataSet?l={0}&c={1}&strSDate={2}&strEDate={3}", s.URLName, v.VariableCode, startDate.ToString("d-MMM-yyyy"), endDate.ToString("d-MMM-yyyy"));
               
            try
            {                
                WebRequest request = WebRequest.Create(url);
                //WebRequest request= WebRequest.Create(@"http://www.usbr.gov/uc/crsp/GetDataSet?l=JOES+VALLEY+RESERVOIR&c=1881&strSDate=22-SEP-2010&strEDate=1-NOV-2010");

                request.Timeout = Timeout.Infinite;
                WebResponse response = request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                Regex r = new Regex("(?<=<a href=').*(?='>Plain Text)");
                URL = r.Match(responseFromServer);
            }                
            catch(Exception ex){
                DBLogging.WriteLog(projectName, "Error", "FillUSBR" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", "Error generating URL: (" + url + ")"); 
                  
                throw ex;
            }
            URLstring = URL.Value;
            int count = 0;
            while (true)
            {
                try
                {
                    WebRequest request = WebRequest.Create(URL.Value);
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
                       DBLogging.WriteLog(projectName, "Error", "FillUSBR" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", "Error accessing Data URL: (" + URL.Value + ")"); 
                       throw ex;
                    }
                    else
                    {
                        //DBLogging.WriteLog(projectName, "Error", "FillUSBR" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", " Attempt to access URL(" + URL.Value + ")"); 
                        count++;
                        Thread.Sleep(1000);
                    }
                }
            }            
        }

        public void createTable(string vName, string data)
        {
            createColumns(vName);
            createRows(data);
        }
       
        private void createColumns(string variable)
        {
            oDS = new DataSet();
            oDS.DataSetName = "USBR";
            DataTable variableTable = oDS.Tables.Add("Variables");

            variableTable.Columns.Add("DateTime", typeof(string));
            variableTable.Columns.Add(variable, typeof(string));            
        }

        private void createRows(string data)
        {
            DataRow oRows;
            char[] delimiters = new char[] { ',', '\n' };

            int i = 0;
            oRows = oDS.Tables["Variables"].NewRow();

            string[] datalist=data.Split(delimiters);
            foreach (string strFields in datalist)
            {
                oRows.SetField(i, strFields);
                
                i = i + 1;
                if (i == 2)
                {
                    i = 0;
                    oDS.Tables["Variables"].Rows.Add(oRows);
                    oRows = oDS.Tables["Variables"].NewRow();
                }
            }
        }      
    }
}
