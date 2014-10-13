using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
//using Logger;


namespace UpdateSnotel
{
    public partial class UpdateDB 
    {        
        int numOfSites=0;
        int beginSite=1;
        int numOfVars;
        int numOfCols = 0;
        //int timeCollected = 12;
        DataSet oDS;       
        Boolean keepgoing=true;
        clsFormatForDB format = new clsFormatForDB();
        clsDatabase db = new clsDatabase();
        int siteID;
        string projectName = Properties.Settings.Default.projectName;
        public  UpdateDB()
        {
            //DBLogging.WriteLog(projectName, "Log", "UpdateDB" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", "SNOTEL Harvester Has Begun Running");                    
            
            try
            {
                fillMeta();
                fillData();
                //DBLogging.WriteLog(projectName, "Log", "UpdateDB" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", "SNOTEL Harvester Has Completed Running");
                System.Threading.Thread.Sleep(20);
                //SendEmail.SendMessage("SNOTELHarvester Completed", "SNOTEL Harvester has completed running, view the attached file for details", projectName, new TimeSpan(12, 0, 0));
            }
            catch (Exception ex)
            {
                //DBLogging.WriteLog(projectName, "Log", "UpdateDB" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", "SNOTEL Harvester Has Completed Running");
                System.Threading.Thread.Sleep(20);
                //SendEmail.SendMessage("SNOTELHarvester error", string.Format("SNOTEL Harvester has encountered an error and is shutting down. The error is {0} and occured in:  \n{1}",ex.Message, ex.StackTrace), projectName, new TimeSpan(12, 0, 0));
        
            }
            
        }

        private void fillMeta()
        {
            //getStates
            clsSnoTelStateList l = new clsSnoTelStateList("http://www.wcc.nrcs.usda.gov/snow/sntllist.html");
            //DBLogging.WriteLog(projectName, "Log", "UpdateDB" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", string.Format("Updating Sites Table using Method 1: {0} states found.", l.stateList.Count));
                               
            //getSites
            for (int i = 0; i < l.stateList.Count; i++)
            {
                clsSnoTelStateTable h = new clsSnoTelStateTable(l.stateList[i].URL);
               
                //Console.WriteLine(l.stateList[i].state);   
                foreach (SnoTelRecord sr in h.Records)
                {
                    //Console.WriteLine("\t" + sr.SiteName);                    
                    char[] splitters = { '\r', '\n' };
                    string siteCode = sr.SiteIds.Split(splitters)[0];
                   
                    if (siteCode != "")
                    {
                        double lat = format.ConvertLatLong(sr.Latitude);
                        double lng = format.ConvertLatLong(sr.Longitude);
                        if (db.isNewSite(siteCode))
                        {
                            //DBLogging.WriteLog(projectName, "Log", "UpdateDB" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", "inserting site: " + sr.SiteName);
                
                            db.insertSite(siteCode, sr.SiteName, lat, lng, 0, format.feetToMeters(Convert.ToDouble(sr.Elevation.Split('\'')[0])), null, null, null, null, null, l.stateList[i].state, sr.County, null, format.getTimeZone(lat, lng), sr.Status);
                            
                        }
                        else
                        {
                            db.updateSite(db.getSiteID(siteCode), siteCode, sr.SiteName, l.stateList[i].state, lat, lng, /*format.getTimeZone(lat, lng),*/ format.feetToMeters(Convert.ToDouble(sr.Elevation.Split('\'')[0])));

                        }
                    }
                }    
            }
            clsSnoTelSiteTable st = new clsSnoTelSiteTable("http://www.wcc.nrcs.usda.gov/nwcc/sitelist.jsp");
            //DBLogging.WriteLog(projectName, "Log", "UpdateDB" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name+"()", "Updating Sites Table using Method 2");
            foreach (clsSnoTelSiteTable.SnoTelSiteRecord ssr in st.Records)
            {
                double lat = format.ConvertLatLong(ssr.Latitude);
                double lng = format.ConvertLatLong(ssr.Longitude);
                if (ssr.SiteCode != "")
                {
                    if (db.isNewSite(ssr.SiteCode))
                    {
                        //DBLogging.WriteLog(projectName, "Log", "UpdateDB" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", "inserting site: " + ssr.SiteName);
                        db.insertSite(ssr.SiteCode, ssr.SiteName, lat, lng, 0, format.feetToMeters(Convert.ToDouble(ssr.Elevation.Split('\'')[0])), null, null, null, null, null, format.getFullState(ssr.State), ssr.County, null, format.getTimeZone(lat, lng), ssr.Status);                        
                    }
                    else
                    {
                        db.updateSite(db.getSiteID(ssr.SiteCode), ssr.SiteCode,ssr.SiteName, format.getFullState(ssr.State), lat, lng,/* format.getTimeZone(lat, lng),*/ format.feetToMeters(Convert.ToDouble(ssr.Elevation.Split('\'')[0])));
                    }
                }
            }
            //DBLogging.WriteLog(projectName, "Log", "UpdateDB" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", "Updating Sites Table Completed");                
        }
        

        private void fillData()
        {
            numOfSites = db.getNumOfSites();

            //DBLogging.WriteLog(projectName, "Log", "UpdateDB" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", string.Format("Updating Series Catalog Table: {0} sites", numOfSites));
             
            //loop through all of the sites and pull down the information from the website
            for (siteID = beginSite; siteID <= numOfSites && keepgoing; siteID++)
            {
                string siteName = db.getSiteName(siteID);
                string state = db.getState(siteID);
                string siteCode = db.getSiteCode(siteID);
                int timeZone = Convert.ToInt16(db.getTimeZone(siteID));

                if (getVariables(siteCode, state))
                {                    
                    //attach variables to columns 
                    //for every variable
                    numOfVars = numOfCols - 1;
                    //check to see if there is information in the database about the variable if true continue, if false skip
                    for (int i = 1; i <= numOfVars; i++)
                    {
                        //int i = 1;
                        int variableID;
                        try
                        {
                            variableID = (int)db.getVariableID(oDS.Tables[0].Columns[i].ColumnName.Trim().ToUpper());
                        }
                        catch { variableID = -9999; }

                        
                        if (variableID > 0)
                        {
                            int startIndex = 0;
                            int endIndex = 0;

                            //calculate begin and end time
                            String endDT = FindEnd(oDS, i, ref endIndex);
                            String beginDT = FindStart(oDS, i, ref startIndex);


                            DateTime beginDateTimeUTC = format.toDateTime(beginDT, timeZone);
                            DateTime endDateTimeUTC = format.toDateTime(endDT, timeZone);
                            DateTime endDateTime = format.toDateTime(endDT, 0);
                            DateTime beginDateTime = format.toDateTime(beginDT, 0);
                            //endDateTimeUTC = endDateTime.ToUniversalTime();

                            int valueCount = calcValues(startIndex, endIndex, i);

                            if (db.isNewSeries(siteID, variableID))
                            {
                                //DBLogging.WriteLog(projectName, "Log", "UpdateDB" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", string.Format("Adding New Series. Site:{0}/{1} SiteName: {2}  Variable ID:{3}, {4} {5}/{6}", siteID + 1, numOfSites, siteName,state ,variableID, i, numOfVars));

                                Variables v = db.getVariableInfo(variableID);
                                Units vu = db.getUnits(Convert.ToInt32(v.UnitsReference.EntityKey.EntityKeyValues[0].Value));
                                Units tu = db.getUnits(Convert.ToInt32(v.Units1Reference.EntityKey.EntityKeyValues[0].Value));
                                Methods m = db.getMethod(variableID);
                                Sources s = db.getSourceData(1);

                                int qualityControlLevelID = 1;
                                string qualityControlLevelCode = qualityControlLevelID.ToString();

                                if (siteName.Contains('\''))
                                {
                                    string[] list = siteName.Split('\'');
                                    siteName = list[0] + "\'\'" + list[1];
                                }
                                db.insertSeries(siteID, siteCode, siteName, v.VariableID, v.VariableCode, v.VariableName, v.Speciation, vu.UnitsID, vu.UnitsName, v.SampleMedium, v.ValueType, v.TimeSupport, tu.UnitsID, tu.UnitsName, v.DataType, v.GeneralCategory, m.MethodID, m.MethodDescription, s.SourceID, s.Organization, s.SourceDescription, s.Citation, qualityControlLevelID, qualityControlLevelCode, beginDateTime, endDateTime, beginDateTimeUTC, endDateTimeUTC, valueCount);
                            }
                            else
                            {
                                //DBLogging.WriteLog(projectName, "Log", "UpdateDB" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", string.Format("Updating Series. Site:{0}/{1} SiteName: {2}  Variable ID:{3}, {4}  {5}/{6}", siteID, numOfSites, siteName,state, variableID, i, numOfVars));
                                //DBLogging.WriteLog(projectName, "Log", "UpdateDB" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", string.Format("Updating Series. Site:{0}/{1} SiteName: {2}  Variable ID:{3}, {4}/{5}", siteID, numOfSites, siteName,, variableID, i, numOfVars));
                                db.updateSeries(siteID, variableID, beginDateTime, endDateTime, beginDateTimeUTC, endDateTimeUTC, valueCount);

                            }
                        }
                    }
                }
                else
                {
                    //DBLogging.WriteLog(projectName, "Error", "UpdateDB" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", string.Format("No Data Found. Site:{0}/{1} SiteName: {2}, {3}  ", siteID , numOfSites, siteName, state ));
                    
                }
                numOfCols = 0;
            }           
        }

        

        int calcValues(int startIndex, int endIndex, int Col)
        {
            int count = 0;

            for (int curRow = startIndex; curRow < endIndex; curRow++)
            {
                count++;
            }
            return count;
        }

        string FindStart(DataSet ds, int Col, ref int startIndex)
        {
            for (int curRow = 0; curRow < ds.Tables[0].Rows.Count; curRow++)
            {
                if (ds.Tables[0].Rows[curRow][Col].ToString().Trim() != "")
                {
                    startIndex = curRow;
                    return ds.Tables[0].Rows[curRow][0].ToString().Trim();                    
                }
            }            
            return DateTime.Now.ToString("MMddyy");
        }

        string FindEnd(DataSet ds, int Col, ref int endIndex)
        {
            for (int curRow = ds.Tables[0].Rows.Count - 1; curRow >= 0; curRow--)
            {
                if (ds.Tables[0].Rows[curRow][Col].ToString().Trim() != "")
                {
                    endIndex = curRow;
                    return ds.Tables[0].Rows[curRow][0].ToString().Trim();
                }
            }            
            return DateTime.Now.ToString("MMddyy");
        }


        private bool ftpFileExist(string fileName)//(WebRequest objRequest)
        {
            //try
            //{
            //    FtpWebRequest fr = (FtpWebRequest) FtpWebRequest.Create(fileName);
            //    FtpWebResponse f = (FtpWebResponse) fr.GetResponse();
            //    //WebRequest wr = WebRequest.Create(fileName);
            //    //WebResponse r = wr.GetResponse();
                
            //    return true;
            //}
            //catch(Exception ex)
            //{
            //    return false;
            //}
            
            WebClient wc = new WebClient();
            try
            {
                //wc.Credentials = new NetworkCredential("FTP_USERNAME", "FTP_PASSWORD");
                byte[] fData = wc.DownloadData(fileName);
                if (fData.Length > -1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch //(Exception e)
            {           
                return false;
            }
            
        }

        private bool getVariables(String siteCode, String state)
        {
            //ensure that the codes are in the correct case for the URL
            siteCode = siteCode.ToLower();
            state = state.Trim().ToLower().Replace(' ', '_');
            //string hisURL = "http://www.wcc.nrcs.usda.gov/ftpref/data/snow/snotel/cards/" +state + "/" + siteCode + "_all.txt";
            //string currURL = "http://www.wcc.nrcs.usda.gov/ftpref/data/snow/snotel/cards/" + state + "/" + siteCode + "_" + getCurrWaterYear() + ".tab";
            string hisURL = "ftp://ftp.wcc.nrcs.usda.gov/data/snow/snotel/cards/" +state + "/" + siteCode + "_all.txt";
            string currURL = "ftp://ftp.wcc.nrcs.usda.gov/data/snow/snotel/cards/" + state + "/" + siteCode + "_" + getCurrWaterYear() + ".tab";
                                    

            //Check the URL           
             bool hasCurr= ftpFileExist(currURL);
             bool hasHis = ftpFileExist(hisURL);
            if (hasCurr && hasHis)
            {//do both
                WebRequest[] objRequest = new WebRequest[2];
                objRequest[0] = WebRequest.Create(hisURL);
                objRequest[1] = WebRequest.Create(currURL);
                return fillTable(objRequest);
            }
            else if (hasCurr)
            {
                WebRequest[] objRequest = new WebRequest[1];
                objRequest[0] = WebRequest.Create(currURL);
                return fillTable(objRequest);
            }
            else if (hasHis)
            {
                WebRequest[] objRequest = new WebRequest[1];
                objRequest[0] = WebRequest.Create(hisURL);
                return fillTable(objRequest);
            }
            else
                return false;
        }

        private bool fillTable(WebRequest[] request)
        {

            int count = 0;
            while (count < 5 && keepgoing)
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

                    Regex[] regex = new Regex[request.Length + 1];
                    Match[] oM = new Match[request.Length + 1];

                    //get the columns of the Dataset
                    regex[0] = new Regex("-.*\n");
                    oM[0] = regex[0].Match(responseFromServer[0]);

                    createColumns(oM[0]);

                    if (request.Length < 2)
                    {

                        regex[1] = new Regex("\n(.|\n)*", RegexOptions.IgnoreCase);
                    }
                    else
                    {
                        regex[1] = new Regex("\n(.|\n)*", RegexOptions.IgnoreCase);
                        regex[2] = new Regex("\n(.|\n)*" + DateTime.Now.ToString("MMddyy"), RegexOptions.IgnoreCase);
                    }
                    for (int i = 0; i < request.Length; i++)
                    {
                        //Here we apply our regular expression to our string using the Match object. 
                        oM[i + 1] = regex[i + 1].Match(responseFromServer[i]);

                        reader[i].Close();
                        response[i].Close();
                        createRows(oM[i + 1]);

                    }
                    return true;
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Unable to read data"))
                    {
                        keepgoing = false;

                    }
                    if (count ==5)
                    {
                        //DBLogging.WriteLog(projectName, "Error", "UpdateDB" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", ex.Message + ". "+ request[0].RequestUri.AbsoluteUri+ " StackTrace:" + ex.StackTrace);
                        return false;
                        //throw new Exception("SNOTEL Website is Offline");                       
                    }
                }
            }
            return false;
        }              

        private string getCurrWaterYear()
        {
            string currYear = DateTime.Now.ToString("yyyy");
            int intYear = Convert.ToInt32(currYear);
            DateTime eoy = new DateTime(intYear, 09, 30);
            if (DateTime.Today.CompareTo(eoy) < 0)
                return currYear;
            else
                return (intYear + 1).ToString();
        }

        private void createColumns(Match oM)
        {
            oDS = new DataSet();
            oDS.DataSetName = "SNOTEL";
            
            char[] delimiters = new char[] { '\t', '\n' };

            DataTable variableTable = oDS.Tables.Add("Variables");

            foreach (String strFields in oM.ToString().Trim().Split(delimiters))
            {
                numOfCols++;
                variableTable.Columns.Add(strFields, typeof(string));
            }
        }

        private void createRows(Match oM)
        {
            DataRow oRows;
            char[] delimiters = new char[] { '\t', '\n' };

            int i = 0;
            oRows = oDS.Tables["Variables"].NewRow();

            foreach (string strFields in oM.ToString().Trim().Split(delimiters))
            {
                oRows.SetField(i, strFields);
                i = i + 1;
                if (i == numOfCols)
                {
                    i = 0;
                    oDS.Tables["Variables"].Rows.Add(oRows);
                    oRows = oDS.Tables["Variables"].NewRow();
                }
            }
        }
        
    }
}
