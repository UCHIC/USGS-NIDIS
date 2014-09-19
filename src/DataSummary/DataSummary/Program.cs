//command line argument
//NCDC-Weather
//USGS-Streamflow
//NRCS-SNOTEL
//NWS-SNODAS-HUC10

namespace DataSummary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using Logger;
    using System.Diagnostics;
using CommandLine;
    using CommandLine.Text;

    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        static void Main(string[] args)
        {

            ClsDBAccessor dba = new ClsDBAccessor();
            DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", "DataSummary Running" + listAgencies(args));


            var options = new Options();
            if (CommandLineParser.Default.ParseArguments(args, options))
            {
                // consume Options type properties
                if (options.Levels == null)
                {
                    options.Levels = new List<string>();
                    options.Levels.Add("1");
                    options.Levels.Add("2");
                    options.Levels.Add("3");
                    options.Levels.Add("4");

                }
                
            }

            foreach (string level in options.Levels)
            {
                switch (level)
                {
                    case "1":
                        if (options.Agency == null)
                            new getOriginalAgency();
                        else
                            new getOriginalAgency(options.Agency.ToArray());
                       // dba.SummaryDB.updateSeriesCatalag();
                        break;
                    case "2":
                        if (options.Agency == null)
                            new TimeAggregated();
                        else
                            new TimeAggregated(options.Agency.ToArray());
                       // dba.SummaryDB.updateSeriesCatalag();
                        break;
                    case "3":
                        if (options.Agency == null)
                            new IndexVariables();
                        else
                            new IndexVariables(options.Agency.ToArray());
                       // dba.SummaryDB.updateSeriesCatalag();
                        break;
                    case "4":
                        if (options.Agency == null)
                            new IndexValues();
                            
                        else
                            new IndexValues(options.Agency.ToArray());
                       // dba.SummaryDB.updateSeriesCatalag();
                        break;
                }
               
            }
            

            DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", "Updating SeriesCatalog of Summary Database");
            dba.SummaryDB.updateSeriesCatalag();

            DBLogging.WriteLog(Properties.Settings.Default.projectName, "Log", "" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", "DataSummary Completed Running" + listAgencies(args));
            SendEmail.SendMessage("DataSummary Completed", "DataSummary has completed running, view the attached file for details", Properties.Settings.Default.projectName, new TimeSpan(7, 0, 0, 0));


        }

        private static string listAgencies(string[] args)
        {
            string where = string.Empty;
            if (args.Length > 0)
            {
                where = " For: ";
                where += args[0];
                for (int i = 1; i < args.Length; i++)
                {
                    where += ", " + args[i];
                }
            }
            else
            {
                where = " For All Agencies";
            }
            return where;
        }
    }


    class Options
    {


        [OptionList("l", "level", Separator = ';', HelpText = "Specify Levels of data you would like to generate, separated by a semi-colon.")]
        public IList<string> Levels { get; set; }
        [OptionList("a", "agency", Separator = ';', HelpText = "Specify Agencies you would like to run, separated by a semi-colon.")]
        public IList<string> Agency { get; set; }


        [ValueList(typeof(List<string>), MaximumElements = 0)]
        public IList<string> Items { get; set; }


    }
}
