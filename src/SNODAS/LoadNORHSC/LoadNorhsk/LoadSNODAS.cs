using System;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Microsoft.VisualBasic;

namespace LoadNORHSC
{
    public class LoadSNODAS
    {
        DataTable values;
        clsDatabase db;
        public LoadSNODAS()
        {
            values = new DataTable("DataValues");
            values.Columns.Add("ValueID", typeof(SqlInt64));
            values.Columns.Add("DataValue", typeof(SqlDouble)); values.Columns.Add("valAccuracy", typeof(SqlDouble)); values.Columns.Add("LocalDT", typeof(SqlDateTime)); values.Columns.Add("UTCOffset", typeof(SqlDouble));
            values.Columns.Add("DateUTC", typeof(SqlDateTime));
            values.Columns.Add("varID", typeof(SqlInt32)); values.Columns.Add("siteID", typeof(SqlInt32)); values.Columns.Add("offsetVal", typeof(SqlDouble)); values.Columns.Add("offsetTypeID", typeof(SqlInt32));
            values.Columns.Add("censorcode", typeof(SqlString)); values.Columns.Add("qualifierID", typeof(SqlInt32)); values.Columns.Add("methodID", typeof(SqlInt32)); values.Columns.Add("sourceID", typeof(SqlInt32));
            values.Columns.Add("sampleID", typeof(SqlInt32)); values.Columns.Add("derivedfromID", typeof(SqlInt32)); values.Columns.Add("qclID", typeof(SqlInt32));
                      
        }
        public void saveFiles2012()
        {
            //db = new clsDatabase(HucNumber);
            string dirpath = Properties.Settings.Default.filePath + "\\2012 data";

            DirectoryInfo di = new DirectoryInfo(dirpath);
            
            //Dim txtFile As System.IO.File
            StreamReader txtStreamReader;
            string strLine;
            string[] arrSplitLine;
            //int i = 0;
            List<int> hucs = new List<int>();

            double DataValue;
            DateTime LocalDateTime;
            int UTCOffset;
            DateTime DateTimeUTC;
            int SiteID;
            int VariableID;
            int MethodID = 1;
            int SourceID = 1;
            int QualityControlLevelID = 1;


            //Get the Sites from the database            
            string strError;

            // Loop through the .gz files in the directory, decompress them and then load them into
            // the SQL Server Database.

            foreach (FileInfo file in di.GetFiles("*.txt"))
            {

                int HucNumber = 0;
                if (file.Name.StartsWith("h8"))
                    HucNumber = 8;
                else if (file.Name.StartsWith("h0"))
                    HucNumber = 10;
                else if (file.Name.StartsWith("h2"))
                    HucNumber = 12;


                //#if DEBUG 
                //                    db = new clsDatabase(8);
                //#else
                if (HucNumber == 10)
                {
                    db = new clsDatabase(HucNumber);
                    //#endif
                    // int ValueID = db.getValueID() + 1;
                    //i = 0;
                    txtStreamReader = File.OpenText(file.FullName);

                    //Figure out what kind of file it is (eg., snow, SWE, snow depth)
                    // and Set the VariableID baseed on variabletype 
                    if (file.Name.Contains("HP001_s"))
                        //The file contains Percent snow Cover
                        VariableID = 8;
                    else if (file.Name.Contains("11036tS_"))
                        //The file contains snow depth data 
                        VariableID = 2;
                    else
                        //The file contains snow water equivalent
                        VariableID = 1;



                    //Read the file and load each line as a new record into the ODM database                                        
                    while ((strLine = txtStreamReader.ReadLine()) != null)
                    {
                        //Increment the line number counter
                        //i = i + 1;
                        //Read a line of the file and split it
                        arrSplitLine = strLine.Split('|');

                        //Construct the DataValues record to insert into the database
                        if (arrSplitLine.Length >= 3)
                        {
                            if (Information.IsNumeric(arrSplitLine[3]))
                            {
                                try
                                {
                                    //Get the SiteID for the site
                                    int siteID = db.getSite(arrSplitLine[0]);

                                    if (siteID > 0)
                                    {
                                        SiteID = siteID;
                                        DataValue = Convert.ToDouble(arrSplitLine[3]);
                                        LocalDateTime = Convert.ToDateTime(arrSplitLine[2].Split(' ')[0] + " 06:00");
                                        UTCOffset = 0;
                                        DateTimeUTC = LocalDateTime;
                                        values.Rows.Add(SqlInt32.Null, DataValue, SqlDouble.Null, LocalDateTime, UTCOffset/*utcoffset*/, DateTimeUTC, SiteID, VariableID, SqlDouble.Null, SqlInt32.Null, "nc", SqlInt32.Null, MethodID, SourceID, SqlInt32.Null, SqlInt32.Null, QualityControlLevelID);

                                        //ValueID += 1;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    strError = ex.ToString();
                                }
                            }
                        }
                    }
#if DEBUG
                    //Console.WriteLine(values.Rows[0][0]);
                    //HucNumber = 8;
                    if (HucNumber == 8)
                        db.insertBulk(values);

#else
                db.insertBulk(values);

#endif
                    values.Clear();
                    txtStreamReader.Close();
                    //Delete the .txt file

                }
                foreach (int huc in hucs)
                {
                    db = new clsDatabase(huc);
                    db.createSeriesCatalog();
                }
            }
        }
                

        public void saveFiles2011() {
            //db = new clsDatabase(HucNumber);
            string dirpath = Properties.Settings.Default.filePath+"\\2011 data";

            DirectoryInfo di = new DirectoryInfo(dirpath);
            string currFile;
            string origName;
            //Dim txtFile As System.IO.File
            StreamReader txtStreamReader;
            string strLine;
            string[] arrSplitLine;
            //int i = 0;
            List<int> hucs = new List<int>();

            double DataValue;
            DateTime LocalDateTime;
            int UTCOffset;
            DateTime DateTimeUTC;
            int SiteID;
            int VariableID;
            int MethodID = 1;
            int SourceID = 1;
            int QualityControlLevelID = 1;


            //Get the Sites from the database            
            string strError;

            // Loop through the .gz files in the directory, decompress them and then load them into
            // the SQL Server Database.
            foreach (FileInfo fi in di.GetFiles("*.gz"))
            {                
                //Get the name of the file and the name of the decompressed file
                currFile = fi.FullName;
                origName = currFile.Remove(currFile.Length - fi.Extension.Length);
                //Reset the file record counter
                //i = 0;

                //Decompress the file
                DecompressGZip(fi);
                foreach (FileInfo fi2 in di.GetFiles("*.tar"))
                {
                    DecompressTar(fi2);
                }
                foreach (FileInfo file in di.GetFiles("*.txt"))
                {

                  
                    int HucNumber=0;
                    if (file.Name.StartsWith("h8"))
                        HucNumber = 8;
                    else if (file.Name.StartsWith("h10"))
                        HucNumber = 10;
                    else if (file.Name.StartsWith("h12"))
                        HucNumber = 12;

                    
//#if DEBUG 
//                    db = new clsDatabase(8);
//#else 
                    if (HucNumber == 10){
                    db = new clsDatabase(HucNumber);
//#endif
                   // int ValueID = db.getValueID() + 1;
                    //i = 0;
                    txtStreamReader = File.OpenText(file.FullName);

                    //Figure out what kind of file it is (eg., snow, SWE, snow depth)
                    // and Set the VariableID baseed on variabletype 
                    if (file.Name.Contains("HP001_s"))                    
                        //The file contains Percent snow Cover
                        VariableID = 8;
                    else if (file.Name.Contains("11036tS_"))
                        //The file contains snow depth data 
                        VariableID = 2;         
                    else
                        //The file contains snow water equivalent
                        VariableID = 1; 
                        
                    

                    //Read the file and load each line as a new record into the ODM database                                        
                    while ((strLine = txtStreamReader.ReadLine()) != null)
                    {
                        //Increment the line number counter
                        //i = i + 1;
                        //Read a line of the file and split it
                        arrSplitLine = strLine.Split('|');

                        //Construct the DataValues record to insert into the database
                        if (arrSplitLine.Length >= 3)
                        {
                            if (Information.IsNumeric(arrSplitLine[3]))
                            {
                                try
                                {
                                    //Get the SiteID for the site
                                    int siteID = db.getSite(arrSplitLine[0]);

                                    if (siteID > 0)
                                    {
                                        SiteID = siteID;
                                        DataValue = Convert.ToDouble(arrSplitLine[3]);
                                        LocalDateTime = Convert.ToDateTime(arrSplitLine[2].Split(' ')[0] + " 06:00");
                                        UTCOffset = 0;
                                        DateTimeUTC = LocalDateTime;
                                        values.Rows.Add(SqlInt32.Null, DataValue, SqlDouble.Null, LocalDateTime, UTCOffset/*utcoffset*/, DateTimeUTC, SiteID, VariableID, SqlDouble.Null, SqlInt32.Null, "nc", SqlInt32.Null, MethodID, SourceID, SqlInt32.Null, SqlInt32.Null, QualityControlLevelID);

                                        //ValueID += 1;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    strError = ex.ToString();
                                }
                            }
                        }
                    }
//#if DEBUG
//                    //Console.WriteLine(values.Rows[0][0]);
//                    //HucNumber = 8;
//                    if (HucNumber ==8)
//                        db.insertBulk(values);
                    
//#else 
                    db.insertBulk(values);
                    
//#endif
                    values.Clear();
                    txtStreamReader.Close();
                    //Delete the .txt file
                    File.Delete(file.FullName);
                }
                //Delete the decompressed file
                File.Delete(origName);
                
            }
            foreach (int huc in hucs)
            {
                db = new clsDatabase(huc);
                db.createSeriesCatalog();
            }
            }
        }

        public void saveFiles(int HucNumber)
        {
            db = new clsDatabase(HucNumber);
            string dirpath = Properties.Settings.Default.filePath + "\\pre2011";

            DirectoryInfo di = new DirectoryInfo(dirpath);
            string currFile;
            string origName;
            //Dim txtFile As System.IO.File
            StreamReader txtStreamReader;
            string strLine;
            string[] arrSplitLine;
            int i = 0;

            double DataValue;
            DateTime LocalDateTime;
            int UTCOffset;
            DateTime DateTimeUTC;
            int SiteID;
            int VariableID;
            int MethodID = 1;
            int SourceID = 1;
            int QualityControlLevelID = 1;

            //Get the Sites from the database            
            string strError;

            // Loop through the .gz files in the directory, decompress them and then load them into
            // the SQL Server Database.
            foreach (FileInfo fi in di.GetFiles("*.gz"))
            {
                string filename = "hucs_" + HucNumber;
                if (fi.Name.ToLower().Contains(filename))
                {
                    //Get the name of the file and the name of the decompressed file
                    currFile = fi.FullName;
                    origName = currFile.Remove(currFile.Length - fi.Extension.Length);
                    //Reset the file record counter
                   // i = 0;

                    //Decompress the file
                    DecompressGZip(fi);
                    txtStreamReader = File.OpenText(origName);

                    //Figure out what kind of file it is (eg., snow, SWE, snow depth)
                    if (fi.Name.Contains("snowdepth_"))
                        //The file contains snow depth data
                        VariableID = 2;
                    else if (fi.Name.Contains("snow_"))
                        //The file contains snow
                        VariableID = 8;
                    else
                        //The file contains snow water equivalent
                        VariableID = 1;
                    //int ValueID = db.getValueID() + 1;

                    //Read the file and load each line as a new record into the ODM database

                    // while (!txtStreamReader.EndOfStream)
                    while ((strLine = txtStreamReader.ReadLine()) != null)
                    {
                        //Increment the line number counter
                        //i = i + 1;
                        i = 0;
                        //Read a line of the file and split it
                        //strLine = txtStreamReader.ReadLine();
                        //txtStreamReader.
                        arrSplitLine = strLine.Split('|');

                        //Construct the DataValues record to insert into the database
                        if (arrSplitLine.Length >= 3)
                        {
                            if (Information.IsNumeric(arrSplitLine[3]))
                            {
                                try
                                {
                                    //Get the SiteID for the site
                                    int siteID = db.getSite(arrSplitLine[0]);

                                    if (siteID > 0)
                                    {

                                        SiteID = siteID;
                                        DataValue = Convert.ToDouble(arrSplitLine[3]);
                                        LocalDateTime = Convert.ToDateTime(arrSplitLine[2].Split(' ')[0] + " 06:00");
                                        UTCOffset = 0;
                                        DateTimeUTC = LocalDateTime;
                                        values.Rows.Add(SqlInt32.Null, DataValue, SqlDouble.Null, LocalDateTime, UTCOffset/*utcoffset*/, DateTimeUTC, SiteID, VariableID, SqlDouble.Null, SqlInt32.Null, "nc", SqlInt32.Null, MethodID, SourceID, SqlInt32.Null, SqlInt32.Null, QualityControlLevelID);
                                        i++;
                                        //ValueID += 1;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    strError = ex.ToString();
                                }
                            }
                        }
                    }
                    db.insertBulk(values);
                    txtStreamReader.Close();
                    values.Clear();
                    //Delete the decompressed file
                    File.Delete(origName);
                }
            }
            addSeriesCatalog();
        }
       
        private void addSeriesCatalog()
        {
            //call stored procedure spUpdateSeriesCatalog
            db.createSeriesCatalog();
        }

        private void DecompressTar(FileInfo fi)
        {
           System.Collections.ObjectModel.ReadOnlyCollection<Tar.TarEntry> result= Tar.Extract(fi.FullName);            
        }
        private void DecompressGZip(FileInfo fi)
        {
            // Get the stream of the source file.
            using (FileStream inFile = fi.OpenRead())
            {
                // Get orignial file extension, for example "doc" from report.doc.gz.
                string curFile = fi.FullName;
                string origName = curFile.Remove(curFile.Length - fi.Extension.Length);

                // Create the decompressed file.
                using (FileStream outFile = File.Create(origName))
                {
                    using (GZipStream Decompress = new GZipStream(inFile, CompressionMode.Decompress))
                    {
                        // Copy the compressed file into the decompression stream.
                        Byte[] buffer = new Byte[4096];
                        int numRead;
                        numRead = Decompress.Read(buffer, 0, buffer.Length);
                        do
                        {
                            outFile.Write(buffer, 0, numRead);
                            numRead = Decompress.Read(buffer, 0, buffer.Length);

                        } while (numRead != 0);
                        //Console.WriteLine("Decompressed: {0}", fi.Name)
                    };
                };
            }
        }
    }
}
