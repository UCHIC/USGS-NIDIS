using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace UpdateSnotel
{
    public class clsSnoTelStateTable
    {
        public string URL { get; set; }
        public List<SnoTelRecord> Records = new List<SnoTelRecord>();

        public clsSnoTelStateTable(string url)
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
                if (element is mshtml.HTMLPhraseElement && element.innerText == "Status") { tableFound = true; }
                else if (element is mshtml.HTMLTableClass && tableFound && element.innerText.Contains("Site Map")) { tableFound = false; }
                else if (element is mshtml.HTMLTableRowClass && tableFound)
                {
                    Records.Add(new SnoTelRecord((mshtml.HTMLTableRowClass)element, Records.Count));
                }
            }
        }
    }
    public abstract class SnoTelBaseRecord
    {
        public string SiteName { get; set; } //101
        public string SiteIds { get; set; } //103
        public string Elevation { get; set; } //105
        public string Latitude { get; set; } //106
        public string Longitude { get; set; } //107
        public string HydroUnitArea { get; set; } //108
        public string County { get; set; } //110
        public string Status { get; set; } //111

        public SnoTelBaseRecord(mshtml.HTMLTableRowClass row, int recordNumber)
        {
            
            foreach (mshtml.IHTMLElement element in row.cells)
            {
                int offset = row.innerText.Contains("DISCONTINUED") && (element.sourceIndex - (recordNumber * 12) > 101) ? -1 : 0;
                
                switch (element.sourceIndex - (recordNumber * 12 + offset))
                {
                    case 101: SiteName = element.innerText; break;
                    case 103: SiteIds = element.innerText; break;
                    case 105: Elevation = element.innerText; break;
                    case 106: Latitude = element.innerText; break;
                    case 107: Longitude = element.innerText; break;
                    case 108: HydroUnitArea = element.innerText; break;
                    case 110: County = element.innerText; break;
                    case 111: Status = element.innerText; break;
                }
            }
        }
        public override string ToString()
        {
            return SiteName;
        }
    }
    public class SnoTelRecord : SnoTelBaseRecord
    {
        public SnoTelRecord(mshtml.HTMLTableRowClass row, int recordNumber)
            : base(row, recordNumber)
        {
        }

    }


}
