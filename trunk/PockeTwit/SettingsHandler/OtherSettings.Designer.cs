namespace PockeTwit
{
    partial class OtherSettings
    {

		#region Fields (17) 

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem menuAccept;
        private System.Windows.Forms.MenuItem menuCancel;

		#endregion Fields 

		#region Methods (1) 


		// Protected Methods (1) 

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


		#endregion Methods 


        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.menuAccept = new System.Windows.Forms.MenuItem();
            this.menuCancel = new System.Windows.Forms.MenuItem();
            this.chkGPS = new System.Windows.Forms.CheckBox();
            this.chkVersion = new System.Windows.Forms.CheckBox();
            this.lblUpDates = new System.Windows.Forms.Label();
            this.txtUpdate = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.chkTranslate = new System.Windows.Forms.CheckBox();
            this.chkSkweezer = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtCaheDir = new System.Windows.Forms.TextBox();
            this.chkStartup = new System.Windows.Forms.CheckBox();            this.lblPhotoService = new System.Windows.Forms.Label();            this.cmbMediaService = new System.Windows.Forms.ComboBox();            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.Add(this.menuAccept);
            this.mainMenu1.MenuItems.Add(this.menuCancel);
            // 
            // menuAccept
            // 
            this.menuAccept.Text = "Accept";
            this.menuAccept.Click += new System.EventHandler(this.menuAccept_Click);
            // 
            // menuCancel
            // 
            this.menuCancel.Text = "Cancel";
            this.menuCancel.Click += new System.EventHandler(this.menuCancel_Click);
            // 
            // chkGPS
            // 
            this.chkGPS.ForeColor = System.Drawing.Color.LightGray;
            this.chkGPS.Location = new System.Drawing.Point(2, 29);
            this.chkGPS.Name = "chkGPS";
            this.chkGPS.Size = new System.Drawing.Size(235, 20);
            this.chkGPS.TabIndex = 1;
            this.chkGPS.Text = "Use GPS";
            // 
            // chkVersion
            // 
            this.chkVersion.ForeColor = System.Drawing.Color.LightGray;
            this.chkVersion.Location = new System.Drawing.Point(2, 3);
            this.chkVersion.Name = "chkVersion";
            this.chkVersion.Size = new System.Drawing.Size(235, 20);
            this.chkVersion.TabIndex = 0;
            this.chkVersion.Text = "Automatically check for new version";
            // 
            // lblUpDates
            // 
            this.lblUpDates.ForeColor = System.Drawing.Color.LightGray;
            this.lblUpDates.Location = new System.Drawing.Point(3, 52);
            this.lblUpDates.Name = "lblUpDates";
            this.lblUpDates.Size = new System.Drawing.Size(234, 20);
            this.lblUpDates.Text = "Automatic Update (Minutes)";
            // 
            // txtUpdate
            // 
            this.txtUpdate.Location = new System.Drawing.Point(3, 71);
            this.txtUpdate.Name = "txtUpdate";
            this.txtUpdate.Size = new System.Drawing.Size(90, 21);
            this.txtUpdate.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.ForeColor = System.Drawing.Color.LightGray;
            this.label1.Location = new System.Drawing.Point(99, 72);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(124, 20);
            this.label1.Text = "0 to disable";
            // 
            // linkLabel1
            // 
            this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabel1.ForeColor = System.Drawing.Color.LightBlue;
            this.linkLabel1.Location = new System.Drawing.Point(110, 248);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(130, 20);
            this.linkLabel1.TabIndex = 4;
            this.linkLabel1.Text = "Configure Notifications";
            this.linkLabel1.Click += new System.EventHandler(this.linkLabel1_Click);
            // 
            // chkTranslate
            // 
            this.chkTranslate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.chkTranslate.ForeColor = System.Drawing.Color.White;
            this.chkTranslate.Location = new System.Drawing.Point(3, 99);
            this.chkTranslate.Name = "chkTranslate";
            this.chkTranslate.Size = new System.Drawing.Size(220, 20);
            this.chkTranslate.TabIndex = 3;
            this.chkTranslate.Text = "Automatically translate";
            // 
            // chkSkweezer
            // 
            this.chkSkweezer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.chkSkweezer.ForeColor = System.Drawing.Color.LightGray;
            this.chkSkweezer.Location = new System.Drawing.Point(3, 125);
            this.chkSkweezer.Name = "chkSkweezer";
            this.chkSkweezer.Size = new System.Drawing.Size(216, 20);
            this.chkSkweezer.TabIndex = 4;
            this.chkSkweezer.Text = "Load URLs in Skweezer";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(4, 174);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(233, 20);
            this.label2.Text = "Cache Directory:";
            // 
            // txtCaheDir
            // 
            this.txtCaheDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCaheDir.Location = new System.Drawing.Point(4, 197);
            this.txtCaheDir.Name = "txtCaheDir";
            this.txtCaheDir.Size = new System.Drawing.Size(233, 21);
            this.txtCaheDir.TabIndex = 6;
            // 
            // chkStartup            //             this.chkStartup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)                        | System.Windows.Forms.AnchorStyles.Right)));            this.chkStartup.ForeColor = System.Drawing.Color.LightGray;            this.chkStartup.Location = new System.Drawing.Point(3, 151);            this.chkStartup.Name = "chkStartup";            this.chkStartup.Size = new System.Drawing.Size(216, 20);            this.chkStartup.TabIndex = 5;            this.chkStartup.Text = "Launch PockeTwit on startup";            //                         // lblPhotoService            //             this.lblPhotoService.ForeColor = System.Drawing.SystemColors.ControlLight;            this.lblPhotoService.Location = new System.Drawing.Point(4, 205);            this.lblPhotoService.Name = "lblPhotoService";            this.lblPhotoService.Size = new System.Drawing.Size(89, 20);            this.lblPhotoService.Text = "MediaService";            //             // cmbMediaService            //             this.cmbMediaService.Items.Add("TwitPic");            this.cmbMediaService.Items.Add("MobyPicture");            this.cmbMediaService.Location = new System.Drawing.Point(99, 203);            this.cmbMediaService.Name = "cmbMediaService";            this.cmbMediaService.Size = new System.Drawing.Size(138, 22);            this.cmbMediaService.TabIndex = 13;            //            // OtherSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.chkStartup);            this.Controls.Add(this.cmbMediaService);            this.Controls.Add(this.lblPhotoService);            this.Controls.Add(this.txtCaheDir);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.chkSkweezer);
            this.Controls.Add(this.chkTranslate);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtUpdate);
            this.Controls.Add(this.lblUpDates);
            this.Controls.Add(this.chkVersion);
            this.Controls.Add(this.chkGPS);
            this.Menu = this.mainMenu1;
            this.Name = "OtherSettings";
            this.Text = "Other Settings";
            this.Load += new System.EventHandler(this.OtherSettings_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox chkGPS;
        private System.Windows.Forms.CheckBox chkVersion;
        private System.Windows.Forms.Label lblUpDates;
        private System.Windows.Forms.TextBox txtUpdate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.CheckBox chkTranslate;
        private System.Windows.Forms.CheckBox chkSkweezer;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtCaheDir;
        private System.Windows.Forms.CheckBox chkStartup;        private System.Windows.Forms.Label lblPhotoService;        private System.Windows.Forms.ComboBox cmbMediaService;
    }
}