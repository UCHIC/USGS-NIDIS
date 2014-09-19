using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logger
{
    class clsDatabase: MQMEntities2
    {
        public void WriteLogDB(string process, string code, string function, string message)
        {
            try
            {
                ProcessLogs p = new ProcessLogs();
                p.ProcessName = process;
                p.LogCode = code;
                p.LogDate = DateTime.Now;
                p.LogMessage = message;
                p.ProcessPhase = function;
                this.AddToProcessLogs(p);
                this.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        public List<ProcessLogs> printLogs(string process, TimeSpan time)
        {
            DateTime start = DateTime.Now.Subtract(time);
            return (from p in this.ProcessLogs where p.ProcessName == process && (p.LogDate < DateTime.Now) && (p.LogDate > start) select p).ToList<ProcessLogs>();
        }
    }
}
