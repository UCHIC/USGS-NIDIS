using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace LoadNORHSC
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            LoadSNODAS ls = new LoadSNODAS();
            //ls.saveFiles(8);
            ls.saveFiles(10);
            //ls.saveFiles(12);
            ls.saveFiles2011();
            ls.saveFiles2012();
        }
    }
}
