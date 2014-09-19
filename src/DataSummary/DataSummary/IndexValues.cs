

namespace DataSummary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Logger;
    using System.Diagnostics;
        
    public class IndexValues
    {

        List<IndexValuexRef> L4xreflist;
        private ClsSummaryDB db;

        public IndexValues()
        {
            
                db = new ClsSummaryDB();
                DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "IndexValues" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", "Level 4 Calculations Has Begun Running");

                L4xreflist = db.getL4List();
                CalcIndexValues();
                DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "IndexValues" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", "Level 4 Calculations Has Completed Running");
            
        }

        public IndexValues(string[] siteTypes)
        {
           
            db = new ClsSummaryDB();
            DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "IndexValues" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", "Level 4 Calculations Has Begun Running");

            L4xreflist = db.getL4List(siteTypes);
            CalcIndexValues();

        

            DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "IndexValues" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", "Level 4 Calculations Has Completed Running");

        }
        
        public void CalcIndexValues()
        {

            //DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "IndexVariables" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", "Level 3 Spatial Aggregation Has Begun Running");

            foreach (IndexValuexRef siteType in L4xreflist)
            {
                DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "IndexValues" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", string.Format("{0} {1} Level 4 Calculations Has Begun Running. MethodID: {2}", siteType.SiteType, siteType.DataTimePeriod, siteType.MethodID));
                try
                {
                    int rows = this.db.CalcIndexValues(siteType);
                    DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "IndexValues" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", string.Format("{1} rows saved. {0} {2} Level  4 Calculations Aggregating Has Completed Running. MethodID: {3}", siteType.SiteType, rows, siteType.DataTimePeriod, siteType.MethodID));
                }
                catch (Exception ex)
                {
                    DBLogging.WriteLog(Properties.Settings.Default.projectName, "Error", "IndexValues" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", string.Format("{1}: {2}. {0} {3} Level  4 Calculations Aggregating Has Completed Running. MethodID: {4}", siteType.SiteType, ex.Message, ex.InnerException.Message, siteType.DataTimePeriod, siteType.MethodID));
             
                }
            }
            //DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "IndexVariables" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", "Level 3 Spatial Aggregation Has Completed Running");
        }
    }
}
