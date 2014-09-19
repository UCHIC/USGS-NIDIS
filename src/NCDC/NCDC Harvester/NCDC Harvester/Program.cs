using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Logger;
using System.Diagnostics;

namespace NCDC_Harvester
{
    
    static class Program
    {
        
        static void Main()
        {
            string projectName = Properties.Settings.Default.projectName;
            DBLogging.WriteLog(projectName, "Log", "" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", projectName + " has begun Running");
            
            harvestData h = new harvestData();
            DBLogging.WriteLog(projectName, "Log", "" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", projectName + " Completed Running");
            System.Threading.Thread.Sleep(30);
            SendEmail.SendMessage(projectName + " Completed", projectName + " has completed running, view the attached file for details", Properties.Settings.Default.projectName, new TimeSpan(7, 0, 0, 0));

        }
    }
}
