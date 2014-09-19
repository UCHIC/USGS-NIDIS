using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SNODASHarvester
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>        
        static void Main()
        {
         clsLoadSNODAS  ls = new clsLoadSNODAS();
         ls.loadData();
        }
    }
}
