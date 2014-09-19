using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Data;
using System.Data.SqlTypes;
using System.IO;
using Microsoft.VisualBasic;
using Logger;
using System.Diagnostics;
using System.Threading;

namespace SNODASHarvester
{
    class clsLoadSNODAS
    {
        DataTable values;
        clsDatabase db;
        string projectName = Properties.Settings.Default.projectName;

        public clsLoadSNODAS()
        {
            values = new DataTable("DataValues");
            values.Columns.Add("ValueID", typeof(SqlInt64));
            values.Columns.Add("DataValue", typeof(SqlDouble)); values.Columns.Add("valAccuracy", typeof(SqlDouble)); values.Columns.Add("LocalDT", typeof(SqlDateTime)); values.Columns.Add("UTCOffset", typeof(SqlDouble));
            values.Columns.Add("DateUTC", typeof(SqlDateTime));
            values.Columns.Add("varID", typeof(SqlInt32)); values.Columns.Add("siteID", typeof(SqlInt32)); values.Columns.Add("offsetVal", typeof(SqlDouble)); values.Columns.Add("offsetTypeID", typeof(SqlInt32));
            values.Columns.Add("censorcode", typeof(SqlString)); values.Columns.Add("qualifierID", typeof(SqlInt32)); values.Columns.Add("methodID", typeof(SqlInt32)); values.Columns.Add("sourceID", typeof(SqlInt32));
            values.Columns.Add("sampleID", typeof(SqlInt32)); values.Columns.Add("derivedfromID", typeof(SqlInt32)); values.Columns.Add("qclID", typeof(SqlInt32));

        }
        public void loadData()
        {
            DBLogging.WriteLog(projectName, "Log", "clsLoadSNODAS" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", "SNODAS Harvester Has Begun Running");                    
            List<int> hucs = new List<int> { 8, 0, 2 };

            foreach (int huc in hucs)
            {
                int sweID = 1;
                int coverID = 8;
                int depthID = 2;
                db = new clsDatabase(huc);

                //DateTime Date = db.getLastDate(sweID).Add(new TimeSpan(1,0,0,0));
                DBLogging.WriteLog(projectName, "Log", "clsLoadSNODAS" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", "Gathering Huc "+ huc +" data. ");
                for (DateTime Date = db.getLastDate(sweID).Add(new TimeSpan(1, 0, 0, 0)); Date < DateTime.Now; Date = Date.Add(new TimeSpan(1, 0, 0, 0)))
                {
                    string SWE = "";
                    try
                    {
                        //string SWE = "ftp://ftp.nohrsc.nws.gov/products/collaborators/h8_ssmv11034tS__T0001TTNATS2012021905HP001.txt";//snow water equivalent 
                        SWE = string.Format("ftp://ftp.nohrsc.nws.gov/products/collaborators/h{0}_ssmv11034tS__T0001TTNATS{1}{2}{3}05HP001.txt", huc/*hucNumber*/, Date.Year/*year*/, Date.ToString("MM")/*Month*/, Date.ToString("dd")/*Day*/);//snow water equivalent 
                        //SWE = string.Format("C:/DEV/NIDIS/SNODAS/2012 data/Huc{0}/h{0}_ssmv11034tS__T0001TTNATS{1}{2}{3}05HP001.txt", huc/*8hucNumber*/, Date.Year/*year*/, Date.ToString("MM")/*Month*/, Date.ToString("dd")/*Day*/);//snow water equivalent 
                        DBLogging.WriteLog(projectName, "Log", "clsLoadSNODAS" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", string.Format("Snow Water Equivelant loading file: {0}", SWE));
                        readData(SWE, sweID);
                    }
                    catch (Exception ex)
                    {
                        DBLogging.WriteLog(projectName, "Error", "clsLoadSNODAS" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", string.Format("Snow Water Equivelant file load error . {0}. {1}", SWE, ex.Message));
                    }
                }
                for (DateTime Date = db.getLastDate(coverID).Add(new TimeSpan(1, 0, 0, 0)); Date < DateTime.Now; Date = Date.Add(new TimeSpan(1, 0, 0, 0)))
                {
                    string snowCover = "";
                    try
                    {
                        snowCover = string.Format("ftp://ftp.nohrsc.nws.gov/products/collaborators/h{0}_ssmv11034tS__T0001TTNATS{1}{2}{3}05HP001_s.txt", huc/*8hucNumber*/, Date.Year/*year*/, Date.ToString("MM")/*Month*/, Date.ToString("dd")/*Day*/);//Percent snow Cover 
                        //snowCover = string.Format("C:/DEV/NIDIS/SNODAS/2012 data/Huc{0}/h{0}_ssmv11034tS__T0001TTNATS{1}{2}{3}05HP001_s.txt", huc/*8hucNumber*/, Date.Year/*year*/, Date.ToString("MM")/*Month*/, Date.ToString("dd")/*Day*/);//Percent snow Cover 
                        DBLogging.WriteLog(projectName, "Log", "clsLoadSNODAS" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", string.Format("Snow Cover loading file: {0}", snowCover));
                        readData(snowCover, coverID);
                    }
                    catch (Exception ex)
                    {
                        DBLogging.WriteLog(projectName, "Error", "clsLoadSNODAS" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", string.Format("Snow Cover file load error . {0}. {1}", snowCover, ex.Message));
                    }
                }
                for (DateTime Date = db.getLastDate(coverID).Add(new TimeSpan(1, 0, 0, 0)); Date < DateTime.Now; Date=Date.Add(new TimeSpan(1, 0, 0, 0)))
                {
                    string snowDepth = "";
                    try
                    {   
                        //string snowDepth = "ftp://ftp.nohrsc.nws.gov/products/collaborators/h8_ssmv11036tS__T0001TTNATS2012022805HP001.txt";//snow depth data 
                        snowDepth = string.Format("ftp://ftp.nohrsc.nws.gov/products/collaborators/h{0}_ssmv11036tS__T0001TTNATS{1}{2}{3}05HP001.txt", huc/*8hucNumber*/, Date.Year/*year*/, Date.ToString("MM")/*Month*/, Date.ToString("dd")/*Day*/);//snow depth data 
                        //snowDepth = string.Format("C:/DEV/NIDIS/SNODAS/2012 data/Huc{0}/h{0}_ssmv11036tS__T0001TTNATS{1}{2}{3}05HP001.txt", huc/*8hucNumber*/, Date.Year/*year*/, Date.ToString("MM")/*Month*/, Date.ToString("dd")/*Day*/);//snow depth data 
                        DBLogging.WriteLog(projectName, "Log", "clsLoadSNODAS" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", string.Format("Snow Depth loading file: {0}", snowDepth));                    
            
                        readData(snowDepth, depthID);
                    }
                    catch (Exception ex)
                    {
                        DBLogging.WriteLog(projectName, "Error", "clsLoadSNODAS" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", string.Format("Snow Depth file load error . {0}. {1}", snowDepth, ex.Message));                    
                    }
                }
                db.createSeriesCatalog();
            }
            
            DBLogging.WriteLog(projectName, "Log", "clsLoadSNODAS" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", "SNODAS Harvester Has Completed Running");
            Thread.Sleep(30);
            SendEmail.SendMessage(projectName+ " Completed", projectName + " has completed running, view the attached file for details", projectName, new TimeSpan(7, 0, 0));
            
        }
        private void readData(string filename, int VariableID)    
        {   
            string[] arrSplitLine;
            double DataValue;
            DateTime LocalDateTime;
            int UTCOffset;
            DateTime DateTimeUTC;
            int SiteID; int MethodID = 1; int SourceID = 1; int QualityControlLevelID = 1;

            string Data = ftpFile(filename);
            //string Data = System.IO.File.ReadAllText(filename);
            //Read the file and load each line as a new record into the ODM database                                        
            //while ((strLine = txtStreamReader.ReadLine()) != null)
            foreach (string strLine in Data.Split('\n'))
            {
                //Increment the line number counter
                //i = i + 1;
                //Read a line of the file and split it
                arrSplitLine = strLine.Split('|');

                //Construct the DataValues record to insert into the database
                if (arrSplitLine.Length >= 3)
                {
                    if (Information.IsNumeric(arrSplitLine[3]))
                    {
                        try
                        {
                            //Get the SiteID for the site
                            int siteID = db.getSite(arrSplitLine[0]);

                            if (siteID > 0)
                            {
                                SiteID = siteID;
                                DataValue = Convert.ToDouble(arrSplitLine[3]);
                                LocalDateTime = Convert.ToDateTime(arrSplitLine[2].Split(' ')[0] + " 06:00");
                                UTCOffset = 0;
                                DateTimeUTC = LocalDateTime;
                                values.Rows.Add(SqlInt32.Null, DataValue, SqlDouble.Null, LocalDateTime, UTCOffset/*utcoffset*/, DateTimeUTC, SiteID, VariableID, SqlDouble.Null, SqlInt32.Null, "nc", SqlInt32.Null, MethodID, SourceID, SqlInt32.Null, SqlInt32.Null, QualityControlLevelID);
                                //ValueID += 1;
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception( "Error Saving Data",ex);
                        }
                    }
                }
            }
            db.insertBulk(values);
            values.Clear();  
        }

        public string ftpFile(string fileName)
        {
            try
            {
                System.Net.FtpWebRequest tmpReq = (System.Net.FtpWebRequest)System.Net.FtpWebRequest.Create(fileName);
                tmpReq.Credentials = new System.Net.NetworkCredential("", "");

                //GET THE FTP RESPONSE
                using (System.Net.WebResponse tmpRes = tmpReq.GetResponse())
                {
                    //GET THE STREAM TO READ THE RESPONSE FROM
                    using (System.IO.Stream tmpStream = tmpRes.GetResponseStream())
                    {
                        //CREATE A TXT READER (COULD BE BINARY OR ANY OTHER TYPE YOU NEED)
                        using (System.IO.TextReader tmpReader = new System.IO.StreamReader(tmpStream))
                        {
                            //STORE THE FILE CONTENTS INTO A STRING
                            return tmpReader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error Reading FTP File", ex);
            }
        }        
    }
}
