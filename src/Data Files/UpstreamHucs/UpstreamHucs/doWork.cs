using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace UpstreamHucs
{
    class doWork
    {
        DataTable HucList;
        DataTable origHucList;
        public void workit()
        {
            int count = 0;
            fillorigHucList();
            setupHucList();
            foreach (DataRow dr in origHucList.Rows)
            {
                count++;
                //if (dr["inUCRB"].ToString() == "1")
                {
                    HucList.Rows.Add(dr["HucID"].ToString(), dr["Area"], dr["HucID"].ToString(), dr["Area"]);
                    getUpstreamSite(dr, dr["HucID"].ToString(), Convert.ToDouble(dr["Area"]));
                }
                //if (count % 100 == 0)
                //{
                //    insertBulk(HucList);
                //    HucList.Clear();
                //}
            }
            insertBulk(HucList);

        }
        private void setupHucList()
        {
            HucList = new DataTable();
            HucList.Columns.Add("HucID", typeof(string));
            HucList.Columns.Add("HucArea", typeof(double));    
            HucList.Columns.Add("UpHucID", typeof(string));            
            HucList.Columns.Add("UpHucArea", typeof(double));         


        }
        private void fillorigHucList()
        {
            origHucList = new DataTable();
            origHucList.Columns.Add("HucID", typeof(string));
            origHucList.Columns.Add("downHucID", typeof(string));
            //origHucList.Columns.Add("inUCRB", typeof(int));
            origHucList.Columns.Add("Area", typeof(double));


            //string sql = "SELECT HUC_10 As HucID, HU_10_DS As downHucID, InUCRB As inUCRB, ACRES As Area FROM HUC10_UCRB";
            string sql = "SELECT HUC_10 As HucID, HUC10_Dcor As downHucID,  Area1 AS Area FROM HUC10_CRB_fixed";
            
            string connectionString = "Data Source=WASSER;Initial Catalog=Tester;User ID= ODM;Password= odm;MultipleActiveResultSets=True";

            using (SqlConnection myConnection = new SqlConnection(connectionString))
            {
                using (SqlCommand myCommand = new SqlCommand(sql, myConnection))
                {
                    myConnection.Open();
                    using (SqlDataReader myReader = myCommand.ExecuteReader())
                    {
                        origHucList.Load(myReader);
                        myConnection.Close();
                    }
                }
            }
        }
        public void getUpstreamSite(DataRow hucCode, string origHucID, double origArea)
        {
            //if ((string)hucCode["HucID"] == (string)hucCode["downHucID"])
            //    return;
            try
            {
                //what do i need to do for this one
                string select = "downHucID= '" + hucCode["HucID"] + "'";
                
                DataRow[] Rows = origHucList.Select(select);
                foreach (DataRow currRow in Rows)
                {
                    HucList.Rows.Add(origHucID, origArea, currRow["HucID"], currRow["Area"]);
                    //call next
                    getUpstreamSite(currRow, origHucID, origArea);
                }
                //getUpstreamSite(currRow, origHucID, origArea);
            }
            catch(Exception ex)
            {
                return;
            }
        }


        public void insertBulk(DataTable data)
        {
            string connectionString = "Data Source=drought.uwrl.usu.edu;Initial Catalog=Summary;User ID= NIDIS;Password= N1d1s!;MultipleActiveResultSets=True";
            SqlBulkCopy bc = new SqlBulkCopy(connectionString);

            bc.BatchSize = 1000;
            bc.DestinationTableName = "UpstreamHucs";
            try
            {
                bc.WriteToServer(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        
    }
}
