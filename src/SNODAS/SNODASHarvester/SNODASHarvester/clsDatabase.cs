using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SNODASHarvester
{
    class clsDatabase : SNODASEntities
    {
        List<Sites> list;
        private int huc;

        public int HucNumber
        {
            get { return huc; }
            set { huc = value; }
        }


        private string conn;
        public clsDatabase() : base()
        {
            this.conn = ConfigurationManager.ConnectionStrings["SNODASEntities"].ConnectionString.Split('"')[1];
            list = (from S in Sites select S).ToList();
            huc = 8;
        }

        public clsDatabase(int hucNumber)
            : base(ConfigurationManager.ConnectionStrings["SNODASEntities"+hucNumber].ConnectionString)
        {
            this.conn = ConfigurationManager.ConnectionStrings["SNODASEntities" + hucNumber].ConnectionString.Split('"')[1];
            list = (from S in Sites where S.SiteCode == "1405000103" select S).ToList();
            huc = hucNumber;
        }

        public DateTime getLastDate(int variableID)
        {
            return (DateTime)(from SC in this.SeriesCatalog where SC.VariableID == variableID select SC.EndDateTime).Max();
        }

        public void insertBulk(DataTable data)
        {            
            SqlBulkCopy bc = new SqlBulkCopy(conn);

            bc.BatchSize = 1000;
            bc.DestinationTableName = "DataValues";
            try
            {
                bc.WriteToServer(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public int getSite(string sitecode){
            if (sitecode == "1405000103")
            {
                return 634;
            }
            else return -99;
            
            //try
            //{ 
            //    Sites siteData =list.Find(delegate(Sites s) { return s.SiteCode == sitecode; });
            //    //if (list.Contains(sitecode)) 
            //        //return 0;
            //    //else
            //    return siteData.SiteID;
            //    //return (from S in Sites where S.SiteCode == sitecode select S.SiteID).First();
            //}
            //catch (Exception ex)
            //{
            //    return -99;
            //}

        }
       public int getValueID(){
           try
           {
               return (from D in this.DataValues orderby D.ValueID descending select D.ValueID).First();
           }
           catch
           {
               return 0;
           }
       }
       public void createSeriesCatalog() {
           createSeriesCatalog(8);
       }
       public void createSeriesCatalog(int huc)
       {
           using (SqlConnection sqlConnection1 = new SqlConnection(conn))
           {
               using (SqlCommand cmd = new SqlCommand())
               {
                   Int32 rowsAffected;

                   cmd.CommandText = "dbo.spUpdateSeriesCatalog";
                   cmd.CommandType = CommandType.StoredProcedure;
                   cmd.Connection = sqlConnection1;

                   sqlConnection1.Open();

                   rowsAffected = cmd.ExecuteNonQuery();

               }
           }
       }



    }
}
