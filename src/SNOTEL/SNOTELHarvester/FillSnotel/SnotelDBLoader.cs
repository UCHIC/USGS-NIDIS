using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Xml;
using FillSnotel;
using System.Reflection;


namespace WindowsFormsApplication1
{
    public partial class SnotelDBLoader : Form
    {
        bool m_ConnectionTested;
        bool m_Saving ;
        bool m_NewConnection;    
        clsConnection m_ConnSettings;
        string serverAddress=("local");
        string DBName;
        string User;
        string Pword;
              
        string g_EXE_Dir;
        
   
        public SnotelDBLoader()
        {
            InitializeComponent();                     
        }

        public void writeXML(int currSite, int totalSites)
        {
            {
                XmlTextWriter writer = new XmlTextWriter(g_EXE_Dir + "\\Config.xml", System.Text.Encoding.UTF8);

                try
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Config"); //Start: Config
                    writer.WriteStartElement("File"); //Start: Config >> File
                    writer.WriteAttributeString("ID", "1");
                    writer.WriteElementString("ServerAddress", serverAddress);
                    writer.WriteElementString("DatabaseName", DBName);
                    writer.WriteElementString("UserName", User);
                    writer.WriteElementString("Pword", Pword);
                    writer.WriteElementString("SchedulePeriod", numNFSchedPeriod.Value.ToString() + " " + cboNFSchedPeriod.SelectedItem);
                    writer.WriteElementString("ScheduleBeginning", dtpNFDate.Value.ToString("MM/dd/yyyy") + " " + dtpNFTime.Value.ToString("HH:mm:ss tt"));
                    writer.WriteElementString("CurrentSiteID", currSite.ToString());
                    writer.WriteElementString("NumberOfSites", totalSites.ToString());

                    writer.WriteEndElement(); //End: Config >> File
                    writer.WriteEndElement(); //End: Config
                    writer.Close();
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message);
                }
            }
        }

        private void SnotelDBLoader_Load(object sender, EventArgs e)
        {    
            g_EXE_Dir = Path.GetDirectoryName(this.GetType().Assembly.Location);            
            if (System.IO.File.Exists(g_EXE_Dir + "..\\Config.xml"))
            {
                System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
                System.Xml.XmlNode root, fileNode, mapNode;
                xmlDoc.Load(g_EXE_Dir + "\\Config.xml");
                root = xmlDoc.DocumentElement;
                for (int x = 0; x <= (root.ChildNodes.Count - 1); x++)
                {
                    fileNode = root.ChildNodes[x];
                    for (int y = 0; y <= fileNode.ChildNodes.Count - 1; y++)
                    {
                        mapNode = fileNode.ChildNodes[y];
                        switch (mapNode.Name)
                        {
                            case "ServerAddress":
                                serverAddress = mapNode.InnerText;
                                txtSQLAddress.Text = serverAddress;
                                break;
                            case "DatabaseName":
                                DBName = mapNode.InnerText;
                                txtDatabaseName.Text = DBName;
                                break;
                            case "UserName":
                                User = mapNode.InnerText;
                                txtSQLUID.Text = User;
                                break;
                            case "Pword":
                                Pword = mapNode.InnerText;
                                txtSQLPWD.Text = Pword;
                                break;
                            case "SchedulePeriod":
                                numNFSchedPeriod.Value = Convert.ToInt32((mapNode.InnerText).Split(' ')[0]);
                                cboNFSchedPeriod.SelectedItem = (mapNode.InnerText).Split(' ')[1];
                                break;
                            case "ScheduleBeginning":
                                //Dim tempDate As DateTime 'Temporary DT value to parse individual dates or times into
                                DateTime tempDate;
                                DateTime.TryParse((mapNode.InnerText).Split(' ')[0], out tempDate);
                                dtpNFDate.Value = tempDate;
                                DateTime.TryParse((mapNode.InnerText).Split(' ')[1] + (mapNode.InnerText).Split(' ')[2], out tempDate);
                                dtpNFTime.Value = tempDate;
                                break;
                        }
                    }
                }
                m_NewConnection = false;
            }   
        }

        private void btnTestConn_Click(object sender, EventArgs e)
        {
            m_ConnectionTested = false;
            try
            {
                //checks for blank fields
                if (txtDatabaseName.Text == "")
                    MessageBox.Show("Please enter a database name. \nNo Database Name");
                else if (txtSQLUID.Text == "")
                    MessageBox.Show("Please enter a User Name.\nNo Username");
                else if (txtSQLPWD.Text == "")
                    MessageBox.Show("Please enter a Password.\nNo Password");
                else
                {
                    m_ConnSettings = new clsConnection(txtSQLAddress.Text, txtDatabaseName.Text, 1, false, txtSQLUID.Text, txtSQLPWD.Text);
                    //string e_ServerAddress,  string e_DBName, int e_Timeout,  bool e_Trusted, string e_UserID , string e_Password)
                    if (m_ConnSettings.TestDBConnection())
                    {
                        //'Conection was completed without exceptions
                        MessageBox.Show("Connection Successful", "ODM Data Loader", MessageBoxButtons.OK);
                        //Me.Cursor = Windows.Forms.Cursors.Default
                        this.Cursor = Cursors.Default;
                        m_ConnectionTested = true;
                        //Check for Known Errors
                    }
                    else
                    {
                        m_ConnectionTested = false;
                        MessageBox.Show("Cannot connect to the specified server.\nPlease change the server name or use a different login.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Testing the Database Connection \n" + ex.Message);
            } 


        }
        
        private void btnSaveMasterPrefs_Click(object sender, EventArgs e)
        {
            //Checks if the connection has been tested, if not then it tests
            //if the test is succesful it saves the current data to the ConnString
            //and exits the program
            //Input:     Default form Input plus Determines whther the connection is valid and whether to save it.
            //Output:    Whether to save the settings or not  

            if (m_ConnectionTested)
            {
                m_Saving = true;
                this.Close();
            }
            else
            {
                btnTestConn.PerformClick();
                if (m_ConnectionTested)
                {
                    m_Saving = true;
                    this.Close();
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            //Cancels the dialog
            //Input:     Default form input
            //Output:    Triggers the Form.close event.

            if (m_NewConnection)
            {
                if (MessageBox.Show("No Database Connection:  Unable to continue without a valid Database Connection.\n Would you like to quit the ODM Data Loader?", "No Database Connection", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                    this.Close();
                }
                else
                    this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                this.Close();
            }
        }

        private void SnotelDBLoader_FormClosing(object sender, FormClosingEventArgs e)
        {
             
        //When the dialog box is closed without saving it sends a dialogresult.cancel  
        //Input:     m_NewConnection, m_Saving, and User Inputs -> Used to decide whether to store the connection or not.
        //Output:    FinalConnsettings, Me.dialogresult ->Stores Successful connection settings to CurrConnSettings, and sends a dialogresult.ok

        //if( saving { save the settings
            if (m_Saving)
            {
                //return an OK dialog result
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            else
            {
                m_ConnSettings = new clsConnection();
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            }
            writeXML(1, 1);
            Process p = new Process();
            //p.StartInfo.FileName = (g_EXE_Dir + "\\UpdateSnotel.exe");
            p.Start();


        }

        private void txtSQLAddress_TextChanged(object sender, EventArgs e)
        {
            m_NewConnection = true;
            serverAddress = txtSQLAddress.Text;
        }

        private void txtDatabaseName_TextChanged(object sender, EventArgs e)
        {
            m_NewConnection = true;
            DBName = txtDatabaseName.Text;
        }

        private void txtSQLUID_TextChanged(object sender, EventArgs e)
        {
            m_NewConnection = true;
            User = txtSQLUID.Text;
        }

        private void txtSQLPWD_TextChanged(object sender, EventArgs e)
        {
            m_NewConnection = true;
            Pword = txtSQLPWD.Text;
        }

      
    }
    
}
