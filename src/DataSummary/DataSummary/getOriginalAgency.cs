using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Logger;

namespace DataSummary
{
    class getOriginalAgency
    {

        /// <summary>
        /// 
        /// </summary>
        private string projectName = Properties.Settings.Default.projectName;
       
        List<L1HarvestList> aggregatelist;
        private ClsSummaryDB db;

        public getOriginalAgency(string[] siteTypes)
        {
            db = new ClsSummaryDB();
            // pull all users into memory, not recommended for large tables.
            aggregatelist = db.getOriginalList(siteTypes);
            downloadOriginal();
        }
       
        public getOriginalAgency()
        {
            db = new ClsSummaryDB();
            aggregatelist = db.getOriginalList();
            downloadOriginal();
        }

        private void downloadOriginal()
        {
            DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "getOriginalAgency" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", "Level 1 Data Harvesting Has Begun Running");

            int count = 0;
            foreach (L1HarvestList variable in aggregatelist)
            {
                {
                    count++;
                    ClsDBAccessor dba = new ClsDBAccessor(variable, db);

                    DateTime start = new DateTime(1900, 01, 01);
                    DateTime end;
                   
                   //test to see if the series exits.  
                    int sID = dba.TooDB.SeriesExists(variable.SiteID, variable.VariableID);

                    DateTime today = DateTime.Today;

                    if (sID > 0)
                    {
                        DateTime s = dba.TooDB.GetLastDateOfSeries(sID);
                        start = s.AddDays(1);
                    }
                    // set end date to yesterday
                    end = new DateTime(today.Year, today.Month, today.Day).AddDays(-1);
                 
                    TimeSpan span = Convert.ToDateTime(end).Subtract(Convert.ToDateTime(start));

                    if (span > new TimeSpan(1, 0, 0, 0))
                    {
                        try
                        {
                            if (variable.WebServiceURL.Contains("1_1"))
                            {
                                Level1Data1_1 m = new Level1Data1_1(dba);
                                m.AgencyDataFill(variable, start.ToString("yyyy-MM-dd"), end.ToString("yyyy-MM-dd"), count);
                            }
                            else
                            {
                                try
                                {
                                    Level1Data1_0 m = new Level1Data1_0(dba);
                                    m.AgencyDataFill(variable, start.ToString("yyyy-MM-dd"), end.ToString("yyyy-MM-dd"), count);
                                }
                                catch (Exception ex)
                                {
                                    if (ex.Message.Contains("Server did not recognize the value of HTTP Header SOAPAction"))
                                    {
                                        Level1Data1_1 m = new Level1Data1_1(dba);
                                        m.AgencyDataFill(variable, start.ToString("yyyy-MM-dd"), end.ToString("yyyy-MM-dd"), count);
                                    }
                                    else
                                    {
                                        throw;
                                    }
                                }

                            }

                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.Contains("See the InnerException"))
                            {
                                DBLogging.WriteLog(this.projectName, "Error", ex.StackTrace, String.Format("{0}.  {3} {1}, {2}-{4}. ", ex.InnerException.Message, variable.SiteCode, variable.VariableCode, variable.SiteName, "Level1"));
                            }
                            else
                            {
                                try
                                {
                                    DBLogging.WriteLog(this.projectName, "Error", ex.StackTrace, String.Format("{1}: {0}.  {4} {2}, {3}-{5}. ", ex.Message, ex.InnerException.Message, variable.SiteCode, variable.VariableCode, variable.SiteName, "Level1"));
                                }
                                catch
                                {
                                    DBLogging.WriteLog(this.projectName, "Error", ex.StackTrace, String.Format("{0}.  {3} {1}, {2}-{4}. ", ex.Message, variable.SiteCode, variable.VariableCode, variable.SiteName, "Level1"));
                                }
                            }
                        }
                    }
                }

                Thread.Sleep(100);
                // variable = this.db.GetNextAggregateEntry(variable.AggregateID);
            }
            db.updateSeriesCatalag();
            DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "getOriginalAgency" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", "Level 1 Data Harvesting Has Completed Running");

        }

    }
}
