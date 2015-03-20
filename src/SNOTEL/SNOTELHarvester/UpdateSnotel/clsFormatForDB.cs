using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using System.Diagnostics;
//using Logger;

namespace UpdateSnotel
{
    public class clsFormatForDB
    {                
        public double feetToMeters(double ft)
        {
            return ft / 3.28083989501312;
        }
        public double ConvertLatLong(string l)
        {
            char[] sep = { '\'', ' ' };
            string[] values = l.Split(sep);
            double degree = Convert.ToDouble(values[0].Substring(0, values[0].Length - 1));
            double minutes = Convert.ToDouble(values[1]) / 60;
            if (degree < 0)
                return degree - minutes;
            else
                return degree + minutes;            
        }
        
        public double getTimeZone(double lat, double lng)
        {
            try
            {
                string url = string.Format("http://ws.geonames.org/timezone?lat={0}&lng={1}", Math.Round(lat, 4), Math.Round(lng, 4));

                WebRequest req = WebRequest.Create(url);
                WebResponse response = req.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                Regex tzreg = new Regex(@"(?<=<dstOffset>).*(?=</dstOffset>)");
                string timez = tzreg.Match(responseFromServer).ToString();
                return Convert.ToDouble(timez);
            }
            catch(Exception ex)
            {
                //DBLogging.WriteLog(Properties.Settings.Default.projectName, "Error", "clsFormatForDB" + "." + (new StackTrace(true)).GetFrame(0).GetMethod().Name + "()", ex.Message);
                return 0;
            }
        }
        int timeCollected = 12;
        
        public DateTime toDateTime(string date, int TimeZone)
        {           
            string month, day, year;

            char[] monthChar = new char[2];
            date.CopyTo(0, monthChar, 0, 2);
            month = new string(monthChar);

            char[] dayChar = new char[2];
            date.CopyTo(2, dayChar, 0, 2);
            day = new string(dayChar);

            char[] yearChar = new char[2];
            date.CopyTo(4, yearChar, 0, 2);
            year = new string(yearChar);
           
            int yearInt = Convert.ToInt32(year);

            DateTime dt;

            if (yearInt < 60)
                dt = new DateTime(Convert.ToInt32("20" + year), Convert.ToInt32(month), Convert.ToInt32(day), timeCollected - TimeZone, 0, 0);
            else
                dt = new DateTime(Convert.ToInt32("19" + year), Convert.ToInt32(month), Convert.ToInt32(day), timeCollected - TimeZone, 0, 0);

            return dt;
        }
        public string getFullState(string st)
        {
            switch (st.Trim())
            {
                case "AK": return "Alaska";
                case "AZ": return "Arizona";
                case "CA": return "California";
                case "CO": return "Colorado";
                case "ID": return "Idaho";
                case "MT": return "Montana";
                case "NM": return "New Mexico";
                case "NV": return "Nevada";
                case "OR": return "Oregon";
                case "SD": return "South Dakota";
                case "UT": return "Utah";
                case "WA": return "Washington";
                case "WY": return "Wyoming";
            }
            return st;
        }
    }
}
