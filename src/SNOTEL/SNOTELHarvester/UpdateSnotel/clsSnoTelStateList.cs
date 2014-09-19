using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;

namespace UpdateSnotel
{
    class clsSnoTelStateList
    {
        public string URL { get; set; }
        public List<SnoTelState> stateList = new List<SnoTelState>();
        public clsSnoTelStateList(string url)
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
                //if(element is mshtml.HTMLTableCellClass && element.innerText.Contains("SNOTEL Site List")) { tableFound=true;}
                if (element is mshtml.HTMLPhraseElement && element.innerText == "SNOTEL Site List") { tableFound = true; }
                //if (element is mshtml.HTMLHeaderElementClass && element.innerText=="SNOTEL Site List") { tableFound = true; }
                else if (element is mshtml.HTMLTableClass && tableFound && element.innerText.Contains("Site Map")) { tableFound = false; }
                else if (element is mshtml.HTMLParaElementClass && tableFound)
                {
                    if (!element.innerText.Contains("Please select"))
                        stateList.Add(new SnoTelState(element.innerText, element.innerHTML));
                }
            }
        }
    }
    public class SnoTelState
    {
        public string state;
        public string URL;
        private Regex getlink;
        public SnoTelState(string s, string u)
        {
            getlink = new Regex("(?<=<A href=\").*?(?=\">)");
            state = s;
            URL = "http://www.wcc.nrcs.usda.gov"+getlink.Match(u).ToString();
        }
        public override string ToString()
        {
            return state;
        }
    }
}

