// -----------------------------------------------------------------------
// Data Summary
// <copyright file="IndexVariables.cs" company="Utah State University">
//          Copyright (c) 2011, Utah State University
// All rights reserved.
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//           Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//           Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//           Neither the name of the Utah State University nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// </copyright>
// -----------------------------------------------------------------------
namespace DataSummary
{
    // haven't taken into consideration gathering data for later dates ( not initial run) 
    ////          need to build in a series catalog check into sql query
    // haven't considered if the query returns a null value or something that can't be saved
    ////          to a database. make sure to get rid of all excess data
    
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Logger;
    using System.Diagnostics;

    public class IndexVariables
    {
       
        List<string> xreflist;
         List<WatershedSeries> wxreflist;
        private ClsSummaryDB db;

        public IndexVariables(string[] siteTypes)
        {
            db = new ClsSummaryDB();
            DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "IndexVariables" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", "Level 3 Spatial Aggregation Has Begun Running");

            xreflist = db.getL3List(siteTypes);
            wxreflist = db.getL3wList(siteTypes);
            CalcIndexVariables();
            CalcIndexVariablesWatershed();

            DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "IndexVariables" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", "Level 3 Spatial Aggregation Has Completed Running");
       

        }

        public IndexVariables()
        {
            db = new ClsSummaryDB();
            DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "IndexVariables" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", "Level 3 Spatial Aggregation Has Begun Running");

            xreflist = db.getL3List();
            wxreflist = db.getL3wList();
            CalcIndexVariables();

           // db.updateSeriesCatalag();
            CalcIndexVariablesWatershed();

            DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "IndexVariables" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", "Level 3 Spatial Aggregation Has Completed Running");
       
        }
        
        public void CalcIndexVariables()
        {
            foreach (string siteType in xreflist)
            {
                DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "IndexVariables" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", string.Format("{0} Level 3 Spatial Aggregating Has Begun Running .", siteType));
                try
                {
                    int rows = this.db.CreateIndexVariableTable(1, siteType);
                    DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "IndexVariables" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", string.Format("{1} rows saved. {0} Level 3 Spatial Aggregating Has Completed Running.", siteType, rows));
                }
                catch (Exception ex)
                {
                    DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "IndexVariables" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", string.Format("{1}:{2}. {0} Level 3 Spatial Aggregating Has Completed Running.", siteType, ex.Message, ex.InnerException.Message));
                
                }
               }
        }
        
        public void CalcIndexVariablesWatershed(){

            foreach (WatershedSeries watershed in wxreflist)
            {
                DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "IndexVariables" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", string.Format("{0} Level 3 {1} Watershed Spatial Aggregating Has Begun Running .", watershed.SiteType, watershed.DataTimePeriod));
                try
                {
                    int rows = this.db.CreateIndexVariableWatershed(watershed);
                    DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "IndexVariables" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", string.Format("{1} rows saved. {0} Level 3 {2} Watershed Spatial Aggregating Has Completed Running.", watershed.SiteType, rows, watershed.DataTimePeriod));
                }
                catch (Exception ex)
                {
                    DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "IndexVariables" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", string.Format("{1}:{2} {0} Level 3 {3} Watershed Spatial Aggregating Has Completed Running.", watershed.SiteType, ex.Message, ex.InnerException.Message, watershed.DataTimePeriod));
                
                }
            }
            
          
        }

        //public void CalcIndexVariables()
        //{
        //    int formCount = this.dba.SummaryDB.GetMaxFormulaNum();
            

        //    DataSet tables = new DataSet();
        //    for (int i = 1; i <= formCount; i++)
        //    {
        //        tables.Tables.Add(this.dba.SummaryDB.CreateIndexVariableTable(i));
              
        //    }

        //    DataTable vals = tables.Tables[0];
        //    if (formCount > 1)
        //    {
        //        for (int i = 1; i < tables.Tables.Count; i++)
        //        {
        //            vals = this.MergeUnique("Merged", vals, tables.Tables[i]);                   
        //        }
        //    }

        //    this.dba.TooDB.InsertBulk(vals);
        //}

       private DataTable MergeUnique(string tableName, DataTable sourceTable1, DataTable sourceTable2)
       {
           DataTable merged = sourceTable1;           
           foreach (DataRow dr in sourceTable2.Rows)
           {
               DataRow[] rowlist = merged.Select("SourceID= " + dr["SourceID"] +
                   " AND SiteID= " + dr["SiteID"] +
                   " AND VariableID= " + dr["VariableID"] +
                   " AND MethodID= " + dr["MethodID"] +
                   " AND QualityControlLevelID= " + dr["QualityControlLevelID"] +
                   " And DateTimeUTC= '" + dr["DateTimeUTC"] + "'");
               if (rowlist.Length < 1)
               {
                   merged.ImportRow(dr);
               }
           }

           return merged;
       }

        //private void PrintRows(DataRow[] rows, string label)
        //{
        //    StreamWriter sw = new StreamWriter(label + ".txt");
        //    sw.WriteLine("\n{0}", label);
        //    if (rows.Length <= 0)
        //    {
        //        sw.WriteLine("no rows found");
        //        return;
        //    }

        //    foreach (DataRow r in rows)
        //    {
        //        foreach (DataColumn c in r.Table.Columns)
        //        {
        //            sw.Write("\t {0}", r[c]);
        //        }

        //        sw.WriteLine();
        //    }
        //}
    }
}
