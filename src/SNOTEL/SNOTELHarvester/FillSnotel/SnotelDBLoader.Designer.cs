namespace WindowsFormsApplication1
{
    partial class SnotelDBLoader
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SnotelConnection = new System.Data.SqlClient.SqlConnection();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSaveMasterPrefs = new System.Windows.Forms.Button();
            this.btnTestConn = new System.Windows.Forms.Button();
            this.grpSQL = new System.Windows.Forms.GroupBox();
            this.DatabaseNameLabel = new System.Windows.Forms.Label();
            this.txtDatabaseName = new System.Windows.Forms.TextBox();
            this.txtSQLAddress = new System.Windows.Forms.TextBox();
            this.lblServerAddress = new System.Windows.Forms.Label();
            this.txtSQLPWD = new System.Windows.Forms.TextBox();
            this.lblServerPassword = new System.Windows.Forms.Label();
            this.txtSQLUID = new System.Windows.Forms.TextBox();
            this.lblServerUserID = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblNFSchedPeriod = new System.Windows.Forms.Label();
            this.numNFSchedPeriod = new System.Windows.Forms.NumericUpDown();
            this.cboNFSchedPeriod = new System.Windows.Forms.ComboBox();
            this.txtNFSchedDate = new System.Windows.Forms.Label();
            this.dtpNFDate = new System.Windows.Forms.DateTimePicker();
            this.lblNFSchedTime = new System.Windows.Forms.Label();
            this.dtpNFTime = new System.Windows.Forms.DateTimePicker();
            this.grpSQL.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numNFSchedPeriod)).BeginInit();
            this.SuspendLayout();
            // 
            // SnotelConnection
            // 
            this.SnotelConnection.FireInfoMessageEventOnUserErrors = false;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(284, 267);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(120, 24);
            this.btnCancel.TabIndex = 12;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSaveMasterPrefs
            // 
            this.btnSaveMasterPrefs.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSaveMasterPrefs.Location = new System.Drawing.Point(148, 267);
            this.btnSaveMasterPrefs.Name = "btnSaveMasterPrefs";
            this.btnSaveMasterPrefs.Size = new System.Drawing.Size(120, 24);
            this.btnSaveMasterPrefs.TabIndex = 11;
            this.btnSaveMasterPrefs.Text = "&Save Changes";
            this.btnSaveMasterPrefs.Click += new System.EventHandler(this.btnSaveMasterPrefs_Click);
            // 
            // btnTestConn
            // 
            this.btnTestConn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTestConn.Location = new System.Drawing.Point(17, 267);
            this.btnTestConn.Name = "btnTestConn";
            this.btnTestConn.Size = new System.Drawing.Size(120, 24);
            this.btnTestConn.TabIndex = 10;
            this.btnTestConn.Text = "&Test Connection";
            this.btnTestConn.Click += new System.EventHandler(this.btnTestConn_Click);
            // 
            // grpSQL
            // 
            this.grpSQL.Controls.Add(this.DatabaseNameLabel);
            this.grpSQL.Controls.Add(this.txtDatabaseName);
            this.grpSQL.Controls.Add(this.txtSQLAddress);
            this.grpSQL.Controls.Add(this.lblServerAddress);
            this.grpSQL.Controls.Add(this.txtSQLPWD);
            this.grpSQL.Controls.Add(this.lblServerPassword);
            this.grpSQL.Controls.Add(this.txtSQLUID);
            this.grpSQL.Controls.Add(this.lblServerUserID);
            this.grpSQL.Location = new System.Drawing.Point(12, 139);
            this.grpSQL.Name = "grpSQL";
            this.grpSQL.Size = new System.Drawing.Size(392, 120);
            this.grpSQL.TabIndex = 9;
            this.grpSQL.TabStop = false;
            this.grpSQL.Text = "Microsoft SQL Server";
            // 
            // DatabaseNameLabel
            // 
            this.DatabaseNameLabel.Location = new System.Drawing.Point(24, 40);
            this.DatabaseNameLabel.Name = "DatabaseNameLabel";
            this.DatabaseNameLabel.Size = new System.Drawing.Size(104, 20);
            this.DatabaseNameLabel.TabIndex = 2;
            this.DatabaseNameLabel.Text = "Database Name:";
            this.DatabaseNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtDatabaseName
            // 
            this.txtDatabaseName.Location = new System.Drawing.Point(136, 40);
            this.txtDatabaseName.Name = "txtDatabaseName";
            this.txtDatabaseName.Size = new System.Drawing.Size(250, 20);
            this.txtDatabaseName.TabIndex = 3;
            this.txtDatabaseName.TextChanged += new System.EventHandler(this.txtDatabaseName_TextChanged);
            // 
            // txtSQLAddress
            // 
            this.txtSQLAddress.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSQLAddress.Location = new System.Drawing.Point(136, 16);
            this.txtSQLAddress.Name = "txtSQLAddress";
            this.txtSQLAddress.Size = new System.Drawing.Size(250, 20);
            this.txtSQLAddress.TabIndex = 1;
            this.txtSQLAddress.Text = "(local)";
            this.txtSQLAddress.TextChanged += new System.EventHandler(this.txtSQLAddress_TextChanged);
            // 
            // lblServerAddress
            // 
            this.lblServerAddress.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblServerAddress.Location = new System.Drawing.Point(24, 16);
            this.lblServerAddress.Name = "lblServerAddress";
            this.lblServerAddress.Size = new System.Drawing.Size(104, 20);
            this.lblServerAddress.TabIndex = 0;
            this.lblServerAddress.Text = "Server Address:";
            this.lblServerAddress.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtSQLPWD
            // 
            this.txtSQLPWD.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSQLPWD.Location = new System.Drawing.Point(136, 88);
            this.txtSQLPWD.Name = "txtSQLPWD";
            this.txtSQLPWD.Size = new System.Drawing.Size(250, 20);
            this.txtSQLPWD.TabIndex = 8;
            this.txtSQLPWD.UseSystemPasswordChar = true;
            this.txtSQLPWD.TextChanged += new System.EventHandler(this.txtSQLPWD_TextChanged);
            // 
            // lblServerPassword
            // 
            this.lblServerPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblServerPassword.Location = new System.Drawing.Point(24, 88);
            this.lblServerPassword.Name = "lblServerPassword";
            this.lblServerPassword.Size = new System.Drawing.Size(104, 20);
            this.lblServerPassword.TabIndex = 7;
            this.lblServerPassword.Text = "Server Password:";
            this.lblServerPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtSQLUID
            // 
            this.txtSQLUID.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSQLUID.Location = new System.Drawing.Point(136, 64);
            this.txtSQLUID.Name = "txtSQLUID";
            this.txtSQLUID.Size = new System.Drawing.Size(250, 20);
            this.txtSQLUID.TabIndex = 6;
            this.txtSQLUID.TextChanged += new System.EventHandler(this.txtSQLUID_TextChanged);
            // 
            // lblServerUserID
            // 
            this.lblServerUserID.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblServerUserID.Location = new System.Drawing.Point(24, 64);
            this.lblServerUserID.Name = "lblServerUserID";
            this.lblServerUserID.Size = new System.Drawing.Size(104, 20);
            this.lblServerUserID.TabIndex = 5;
            this.lblServerUserID.Text = "Server User ID:";
            this.lblServerUserID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblTitle
            // 
            this.lblTitle.Location = new System.Drawing.Point(12, 115);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(392, 16);
            this.lblTitle.TabIndex = 8;
            this.lblTitle.Text = "Please select a Database:";
            // 
            // lblNFSchedPeriod
            // 
            this.lblNFSchedPeriod.AutoSize = true;
            this.lblNFSchedPeriod.Location = new System.Drawing.Point(36, 28);
            this.lblNFSchedPeriod.Name = "lblNFSchedPeriod";
            this.lblNFSchedPeriod.Size = new System.Drawing.Size(60, 13);
            this.lblNFSchedPeriod.TabIndex = 13;
            this.lblNFSchedPeriod.Text = "Run Every:";
            // 
            // numNFSchedPeriod
            // 
            this.numNFSchedPeriod.Location = new System.Drawing.Point(102, 26);
            this.numNFSchedPeriod.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numNFSchedPeriod.Name = "numNFSchedPeriod";
            this.numNFSchedPeriod.Size = new System.Drawing.Size(93, 20);
            this.numNFSchedPeriod.TabIndex = 14;
            this.numNFSchedPeriod.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // cboNFSchedPeriod
            // 
            this.cboNFSchedPeriod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboNFSchedPeriod.FormattingEnabled = true;
            this.cboNFSchedPeriod.Items.AddRange(new object[] {
            "days",
            "weeks",
            "months",
            "years"});
            this.cboNFSchedPeriod.Location = new System.Drawing.Point(201, 25);
            this.cboNFSchedPeriod.MaxDropDownItems = 5;
            this.cboNFSchedPeriod.Name = "cboNFSchedPeriod";
            this.cboNFSchedPeriod.Size = new System.Drawing.Size(121, 21);
            this.cboNFSchedPeriod.TabIndex = 15;
            // 
            // txtNFSchedDate
            // 
            this.txtNFSchedDate.AutoSize = true;
            this.txtNFSchedDate.Location = new System.Drawing.Point(36, 56);
            this.txtNFSchedDate.Name = "txtNFSchedDate";
            this.txtNFSchedDate.Size = new System.Drawing.Size(32, 13);
            this.txtNFSchedDate.TabIndex = 16;
            this.txtNFSchedDate.Text = "Start:";
            // 
            // dtpNFDate
            // 
            this.dtpNFDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpNFDate.Location = new System.Drawing.Point(102, 52);
            this.dtpNFDate.Name = "dtpNFDate";
            this.dtpNFDate.Size = new System.Drawing.Size(93, 20);
            this.dtpNFDate.TabIndex = 17;
            this.dtpNFDate.Value = new System.DateTime(2007, 1, 1, 0, 0, 0, 0);
            // 
            // lblNFSchedTime
            // 
            this.lblNFSchedTime.AutoSize = true;
            this.lblNFSchedTime.Location = new System.Drawing.Point(201, 56);
            this.lblNFSchedTime.Name = "lblNFSchedTime";
            this.lblNFSchedTime.Size = new System.Drawing.Size(18, 13);
            this.lblNFSchedTime.TabIndex = 18;
            this.lblNFSchedTime.Text = "@";
            // 
            // dtpNFTime
            // 
            this.dtpNFTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dtpNFTime.Location = new System.Drawing.Point(225, 52);
            this.dtpNFTime.MinDate = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);
            this.dtpNFTime.Name = "dtpNFTime";
            this.dtpNFTime.ShowUpDown = true;
            this.dtpNFTime.Size = new System.Drawing.Size(97, 20);
            this.dtpNFTime.TabIndex = 19;
            this.dtpNFTime.Value = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);
            // 
            // SnotelDBLoader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(433, 299);
            this.Controls.Add(this.lblNFSchedPeriod);
            this.Controls.Add(this.numNFSchedPeriod);
            this.Controls.Add(this.cboNFSchedPeriod);
            this.Controls.Add(this.txtNFSchedDate);
            this.Controls.Add(this.dtpNFDate);
            this.Controls.Add(this.lblNFSchedTime);
            this.Controls.Add(this.dtpNFTime);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSaveMasterPrefs);
            this.Controls.Add(this.btnTestConn);
            this.Controls.Add(this.grpSQL);
            this.Controls.Add(this.lblTitle);
            this.Name = "SnotelDBLoader";
            this.Text = "SNOTEL";
            this.Load += new System.EventHandler(this.SnotelDBLoader_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SnotelDBLoader_FormClosing);
            this.grpSQL.ResumeLayout(false);
            this.grpSQL.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numNFSchedPeriod)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Data.SqlClient.SqlConnection SnotelConnection;
        internal System.Windows.Forms.Button btnCancel;
        internal System.Windows.Forms.Button btnSaveMasterPrefs;
        internal System.Windows.Forms.Button btnTestConn;
        internal System.Windows.Forms.GroupBox grpSQL;
        internal System.Windows.Forms.Label DatabaseNameLabel;
        internal System.Windows.Forms.TextBox txtDatabaseName;
        internal System.Windows.Forms.TextBox txtSQLAddress;
        internal System.Windows.Forms.Label lblServerAddress;
        internal System.Windows.Forms.TextBox txtSQLPWD;
        internal System.Windows.Forms.Label lblServerPassword;
        internal System.Windows.Forms.TextBox txtSQLUID;
        internal System.Windows.Forms.Label lblServerUserID;
        internal System.Windows.Forms.Label lblTitle;
        internal System.Windows.Forms.Label lblNFSchedPeriod;
        internal System.Windows.Forms.NumericUpDown numNFSchedPeriod;
        internal System.Windows.Forms.ComboBox cboNFSchedPeriod;
        internal System.Windows.Forms.Label txtNFSchedDate;
        internal System.Windows.Forms.DateTimePicker dtpNFDate;
        internal System.Windows.Forms.Label lblNFSchedTime;
        internal System.Windows.Forms.DateTimePicker dtpNFTime;
    }
}

