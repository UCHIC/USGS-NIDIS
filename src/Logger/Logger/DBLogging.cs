using System;
using System.Configuration;

namespace Logger
{
    public class DBLogging 
    {
        public static void WriteLog(string process, string code, string function, string message)
        {
            clsDatabase l = new clsDatabase();
            l.WriteLogDB(process, code, function, message);   
        }
        
    }
}
