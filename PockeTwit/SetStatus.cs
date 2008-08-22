using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PockeTwit
{
    public partial class SetStatus : Form
    {

		#region Fields (2) 
        private delegate void delUpdateText(string text);
        public string TwitPicFile = null;
        public bool UseTwitPic = false;
        /*
        public GPS.GpsPosition position = null;
        private GPS.GpsDeviceState device = null;
        private GPS.Gps gps = null;
        private Label lblGPS = new Label();
         */
		#endregion Fields 

		#region Constructors (1) 

        public SetStatus()
        {
            InitializeComponent();
            
            lblCharsLeft.Text = "140";
            PopulateAccountList();
            /*
            if (ClientSettings.UseGPS)
            {
                GetGPS();
            }
             */
        }
        /*
        private void GetGPS()
        {
            label1.Visible = false;
            this.Controls.Add(lblGPS);
            lblGPS.ForeColor = Color.LightGray;
            lblGPS.Location = new Point(0, 0);
            lblGPS.Size = new Size(200, 20);
            lblGPS.Visible = true;
            
            gps = new PockeTwit.GPS.Gps();
            gps.DeviceStateChanged += new PockeTwit.GPS.DeviceStateChangedEventHandler(gps_DeviceStateChanged);
            gps.LocationChanged += new PockeTwit.GPS.LocationChangedEventHandler(gps_LocationChanged);

            gps.Open();
        }
        
        private void Updatelbl(string tText)
        {
            if (InvokeRequired)
            {
                delUpdateText d = new delUpdateText(Updatelbl);
                this.Invoke(d, new object[] { tText });
            }
            else
            {
                lblGPS.Text = tText;
            }
        }
        void gps_LocationChanged(object sender, PockeTwit.GPS.LocationChangedEventArgs args)
        {
            
            try
            {
                if (args.Position.LatitudeValid && args.Position.LongitudeValid)
                {
                    position = args.Position;
                    Updatelbl(position.Latitude.ToString() + "," + position.Longitude.ToString());
                }
            }
            catch(DivideByZeroException ex)
            {
            }
        }

        void gps_DeviceStateChanged(object sender, PockeTwit.GPS.DeviceStateChangedEventArgs args)
        {
            device = args.DeviceState;
        }

       */
        private void PopulateAccountList()
        {
            foreach (Yedda.Twitter.Account Account in ClientSettings.AccountsList)
            {
                if (Account.Enabled)
                {
                    cmbAccount.Items.Add(Account);
                }
            }
        }
		#endregion Constructors 

		#region Properties (2) 

        private Yedda.Twitter.Account _AccountToSet = ClientSettings.AccountsList[0];
        public Yedda.Twitter.Account AccountToSet 
        { 
            get
            {
                return _AccountToSet;
            }
            set
            {
                _AccountToSet = value;
                cmbAccount.SelectedItem = _AccountToSet;
                Yedda.Twitter t = new Yedda.Twitter();
                t.AccountInfo = _AccountToSet;
                AllowTwitPic = t.AllowTwitPic;
            }
        }

        public bool AllowTwitPic
        {
            set
            {
                if (DetectDevice.DeviceType == DeviceType.Professional)
                {
                    pictureBox1.Visible = value;
                    pictureBox2.Visible = value;
                }
                else
                {
                    menuCamera.Enabled = value;
                    menuExist.Enabled = value;
                }
            }
        }

        public string StatusText
        {
            get
            {
                return textBox1.Text;
            }
            set
            {
                textBox1.Text = value;
                this.textBox1.SelectionStart = this.textBox1.Text.Length;
            }
        }

		#endregion Properties 

		#region Methods (12) 


		// Private Methods (12) 

        private void InsertPictureFromCamera()
        {
            using (Microsoft.WindowsMobile.Forms.CameraCaptureDialog c = new Microsoft.WindowsMobile.Forms.CameraCaptureDialog())
            {
                try
                {
                    if (c.ShowDialog() == DialogResult.OK)
                    {
                        TwitPicFile = c.FileName;
                        UseTwitPic = true;
                        if (pictureBox1.Visible)
                        {
                            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetStatus));
                            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
                            AddPictureToForm(c.FileName, pictureBox1);
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("The camera is not available.", "PockeTwit");
                }
            }
        }

        private void InsertPictureFromFile()
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                TwitPicFile = openFileDialog1.FileName;
                UseTwitPic = true;
                if (pictureBox1.Visible)
                {
                    System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetStatus));
                    this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
                    AddPictureToForm(openFileDialog1.FileName, pictureBox2);
                }
            }
        }

        private void AddPictureToForm(string ImageFile, PictureBox BoxToUpdate)
        {
            BoxToUpdate.Image = new System.Drawing.Bitmap(ImageFile);
        }

        private void InsertURL()
        {
            URLForm f = new URLForm();
            if (f.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = textBox1.Text + " " + f.URL;
            }
            this.Show();
            f.Close();
        }

        private void menuCancel_Click(object sender, EventArgs e)
        {
            /*
            if (ClientSettings.UseGPS)
            {
                gps.Close();
            }
             */
            this.DialogResult = DialogResult.Cancel;
        }

        void menuExists_Click(object sender, EventArgs e)
        {
            InsertPictureFromFile();
        }
        void menuCamera_Click(object sender, EventArgs e)
        {
            InsertPictureFromCamera();
        }

        private void menuSubmit_Click(object sender, EventArgs e)
        {
            /*
            if (ClientSettings.UseGPS)
            {
                gps.Close();
            }
             */
            this.DialogResult = DialogResult.OK;
        }

        void menuURL_Click(object sender, EventArgs e)
        {
            InsertURL();
        }

        
        private void textBox1_GotFocus(object sender, EventArgs e)
        {
            if (textBox1.Text == "Set Status")
            {
                textBox1.SelectionStart = 0;
                textBox1.SelectionLength = textBox1.Text.Length;
            }
            else
            {
                textBox1.SelectionStart = textBox1.Text.Length;
                textBox1.SelectionLength = 0;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            int lengthLeft = 140-textBox1.Text.Length;
            lblCharsLeft.Text = lengthLeft.ToString();
        }


		#endregion Methods 

        private void cmbAccount_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.AccountToSet = (Yedda.Twitter.Account)cmbAccount.SelectedItem;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            InsertPictureFromCamera();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            InsertPictureFromFile();
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            InsertURL();
        }


    }
}