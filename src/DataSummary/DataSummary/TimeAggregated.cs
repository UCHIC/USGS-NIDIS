

namespace DataSummary
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using Logger;
    using System.Data;

    public class TimeAggregated
    {
        /// <summary>
        /// 
        /// </summary>
        private string projectName = Properties.Settings.Default.projectName;


        /// <summary>
        /// 
        /// </summary>       
        List<AggregateSeries> aggregatelist;
        private ClsSummaryDB db;

        public TimeAggregated(string[] siteTypes)
        {
            db = new ClsSummaryDB();            
            aggregatelist = db.getAggregateList(siteTypes);
            CalculatesAggregates();

        }

        public TimeAggregated()
        {
            db = new ClsSummaryDB();
            aggregatelist = db.getAggregateList();
            CalculatesAggregates();
        }

        public void CalculatesAggregates()
        {

            DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "AggregateDataTable" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", "Monthly/BiMonthly Aggregation Has Begun Running");

            foreach (AggregateSeries variable in aggregatelist)
            {
                ClsDBAccessor dba = new ClsDBAccessor(variable, db);
                 try
                {
                    if (variable.DataTimePeriod == "Monthly")
                    {
                        //MonthlyAggregate
                        DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "AggregateDataTable" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", string.Format("{0} Monthly Aggregation Has Begun Running .", variable.SiteType));
                        int rows = dba.SummaryDB.monthlyAggregate(variable);
                        DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "AggregateDataTable" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", string.Format("{1} rows saved. {0} Monthly Aggregation Has Completed Running.", variable.SiteType, rows));

                    }
                    else
                    {
                        //BiMonthlyAggregate
                        DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "AggregateDataTable" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", string.Format("{0} BiMonthly Aggregation Has Begun Running.", variable.SiteType));
                        int rows = dba.SummaryDB.biMonthlyAggregate(variable);
                        DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "AggregateDataTable" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", string.Format("{1} rows saved. {0} BiMonthly Aggregation Has Completed Running.", variable.SiteType, rows));
                    }

                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("See the InnerException"))
                    {
                        DBLogging.WriteLog(this.projectName, "Error", ex.StackTrace, String.Format("{0}. {1}, {2}. ", ex.InnerException.Message, variable.SiteType, variable.DataTimePeriod));
                    }
                    else
                    {
                        try
                        {
                            DBLogging.WriteLog(this.projectName, "Error", ex.StackTrace, String.Format("{1}: {0}.  {2}, {3}. ", ex.Message, ex.InnerException.Message, variable.SiteType, variable.DataTimePeriod));
                        }
                        catch
                        {
                            DBLogging.WriteLog(this.projectName, "Error", ex.StackTrace, String.Format("{0}.  {1}, {2}. ", ex.Message, variable.SiteType, variable.DataTimePeriod));
                        }
                    }
                }
                Thread.Sleep(10);
            }
            DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "AggregateDataTable" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", "Monthly/BiMonthly Aggregation Completed Running");
       }
    }
}

