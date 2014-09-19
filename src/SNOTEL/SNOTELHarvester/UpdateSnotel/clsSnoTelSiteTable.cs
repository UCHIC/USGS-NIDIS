using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace UpdateSnotel
{
    class clsSnoTelSiteTable
    {

        public string URL { get; set; }
        public List<SnoTelSiteRecord> Records = new List<SnoTelSiteRecord>();

        public clsSnoTelSiteTable(string url)
        {
            URL = url;

            WebClient client = new WebClient();
            byte[] data = client.DownloadData(URL);
            mshtml.HTMLDocumentClass Doc = new mshtml.HTMLDocumentClass();
            string sHtml = System.Text.Encoding.ASCII.GetString(data);
            mshtml.IHTMLDocument2 oDoc = (mshtml.IHTMLDocument2)Doc;
            oDoc.write(sHtml);

            bool tableFound = false;
            foreach (mshtml.IHTMLElement element in (mshtml.IHTMLElementCollection)oDoc.body.all)
            {
                if (element is mshtml.HTMLPhraseElement && element.innerText == "Station ID") { tableFound = true; }
                else if (element is mshtml.HTMLTableClass && tableFound && element.innerText.Contains("Site Map")) { tableFound = false; }
                else if (element is mshtml.HTMLTableRowClass && tableFound)
                {
                    Records.Add(new SnoTelSiteRecord((mshtml.HTMLTableRowClass)element, Records.Count));
                }
            }
        }

        public abstract class SnoTelSiteBaseRecord
        {
            public string SiteName { get; set; } //101
            public string SiteCode { get; set; } //103
            public string Elevation { get; set; } //105
            public string Latitude { get; set; } //106
            public string Longitude { get; set; } //107            
            public string County { get; set; } //110
            public string Status { get; set; } //111
            public string State { get; set; }

            public SnoTelSiteBaseRecord(mshtml.HTMLTableRowClass row, int recordNumber)
            {

                foreach (mshtml.IHTMLElement element in row.cells)
                {                    

                    switch (element.sourceIndex - (recordNumber * 9))
                    {
                        case 90: State = element.innerText; break;
                        case 91: SiteName = element.innerText; break;
                        case 93: SiteCode = element.innerText; break;                        
                        case 95: Latitude = element.innerText; break;
                        case 96: Longitude = element.innerText; break;
                        case 97: Elevation = element.innerText; break;
                    }
                    
                }
                Status = "Active";
            }
            public override string ToString()
            {
                return SiteName;
            }
        }
        public class SnoTelSiteRecord : SnoTelSiteBaseRecord
        {
            public SnoTelSiteRecord(mshtml.HTMLTableRowClass row, int recordNumber)
                : base(row, recordNumber)
            {
            }

        }


    }

}