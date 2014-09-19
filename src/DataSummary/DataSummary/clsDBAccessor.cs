// -----------------------------------------------------------------------
// Data Summary
// <copyright file="ClsDBAccessor.cs" company="Utah State University">
//          Copyright (c) 2011, Utah State University
// All rights reserved.
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//           Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//           Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//           Neither the name of the Utah State University nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// </copyright>
// -----------------------------------------------------------------------
namespace DataSummary
{
    using System;
    using System.Collections.Generic;
    using System.Data.EntityClient;
    using System.Data.SqlClient;
    using System.Data.SqlTypes;
    using System.Linq;
    using System.Text;
    using System.Data;

    public class ClsDBAccessor
    {
        private ClsSummaryDB summaryDB;
        private ClsFromDB fromDB;
        private ClsTooDB tooDB;       


        public ClsSummaryDB SummaryDB
        {
            get { return this.summaryDB; }           
        }

        public ClsFromDB FromDB
        {
            get { return this.fromDB; }
            set { this.fromDB = value; }
        }

        public ClsTooDB TooDB
        {
            get { return this.tooDB; }
            set { this.tooDB = value; }
        }
        public ClsDBAccessor(L1HarvestList series, ClsSummaryDB summDB)
        {
            this.summaryDB = summDB;
            if (series.DBInitialCatalog != null && series.DBInitialCatalog != string.Empty)
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = series.DBDataSource; // drought.usu.edu
                builder.InitialCatalog = series.DBInitialCatalog; // Summary
                builder.PersistSecurityInfo = true;
                builder.UserID = series.DBUsername;
                builder.Password = series.DBPassword;
                builder.MultipleActiveResultSets = true;
                //// builder.IntegratedSecurity = true;
                string providerString = builder.ToString();

                EntityConnectionStringBuilder tooConnection = new EntityConnectionStringBuilder();
                tooConnection.Metadata = "res://*/SummaryModel.csdl|res://*/SummaryModel.ssdl|res://*/SummaryModel.msl";//"res://*/ODMModel.csdl|res://*/ODMModel.ssdl|res://*/ODMModel.msl";//"res://*/SummaryModel.csdl|res://*/SummaryModel.ssdl|res://*/SummaryModel.msl";
                tooConnection.Provider = "System.Data.SqlClient";
                tooConnection.ProviderConnectionString = providerString;
                this.tooDB = new ClsTooDB(tooConnection.ConnectionString);
            }
            else
            {
                this.tooDB = new ClsTooDB();
            }
        }

        public ClsDBAccessor(AggregateSeries series, ClsSummaryDB summDB)
        {
            this.summaryDB = summDB;
            if (series.L1DBInitialCatalog != null && series.L1DBInitialCatalog != string.Empty)
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = series.L1DBDataSource; // drought.usu.edu
                builder.InitialCatalog = series.L1DBInitialCatalog; // Summary
                builder.PersistSecurityInfo = true;
                builder.UserID = series.L1DBUsername;
                builder.Password = series.L1DBPassword;
                builder.MultipleActiveResultSets = true;
                //// builder.IntegratedSecurity = true;
                string providerString = builder.ToString();

                EntityConnectionStringBuilder fromConnection = new EntityConnectionStringBuilder();
                fromConnection.Metadata = "res://*/SummaryModel.csdl|res://*/SummaryModel.ssdl|res://*/SummaryModel.msl";//"res://*/ODMModel.csdl|res://*/ODMModel.ssdl|res://*/ODMModel.msl";//"res://*/SummaryModel.csdl|res://*/SummaryModel.ssdl|res://*/SummaryModel.msl";
                fromConnection.Provider = "System.Data.SqlClient";
                fromConnection.ProviderConnectionString = providerString;

                this.fromDB = new ClsFromDB(fromConnection.ConnectionString);
            }
            else
            {
                this.fromDB = new ClsFromDB();
            }

            if (series.DBInitialCatalog != null && series.DBInitialCatalog != string.Empty)
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = series.DBDataSource; // drought.usu.edu
                builder.InitialCatalog = series.DBInitialCatalog; // Summary
                builder.PersistSecurityInfo = true;
                builder.UserID = series.DBUsername;
                builder.Password = series.DBPassword;
                builder.MultipleActiveResultSets = true;
                //// builder.IntegratedSecurity = true;
                string providerString = builder.ToString();

                EntityConnectionStringBuilder tooConnection = new EntityConnectionStringBuilder();
                tooConnection.Metadata = "res://*/SummaryModel.csdl|res://*/SummaryModel.ssdl|res://*/SummaryModel.msl";//"res://*/ODMModel.csdl|res://*/ODMModel.ssdl|res://*/ODMModel.msl";//
                tooConnection.Provider = "System.Data.SqlClient";
                tooConnection.ProviderConnectionString = providerString;
                this.tooDB = new ClsTooDB(tooConnection.ConnectionString);
            }
            else
            {
                this.tooDB = new ClsTooDB();
            }
        } 
       

        public ClsDBAccessor()
        {
                this.summaryDB = new ClsSummaryDB();
                this.fromDB = new ClsFromDB();       
                this.tooDB = new ClsTooDB();
            
        }
    }
}
