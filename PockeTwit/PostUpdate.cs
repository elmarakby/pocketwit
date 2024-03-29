﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using PockeTwit.FingerUI.SpellingCorrections;
using PockeTwit.OtherServices.TextShrinkers;
using PockeTwit.Themes;
using Yedda;
using PockeTwit.MediaServices;
using PockeTwit.OtherServices.GoogleSpell;
using PockeTwit.Position;
using System.Globalization;

namespace PockeTwit
{
    public partial class PostUpdate : Form
    {
        private System.Windows.Forms.ContextMenu copyPasteMenu;
        
        private System.Windows.Forms.MenuItem PasteItem;
        private System.Windows.Forms.MenuItem CopyItem;
		
        public GeoCoord GPSLocation = null;
        private LocationManager LocationFinder = new LocationManager();
        private bool _StandAlone;
        private delegate void delUpdateText(string text);

        private IPictureService pictureService;
        private bool localPictureEventsSet = false;
        private List<Place> places = null;
        private bool oldInputPanelState = false;

        // this will have more features as time goes on
        private AttachmentUploadManager UploadManager;

        public delegate void delAddPicture(string ImageFile, PictureBox BoxToUpdate);
        public delegate void delUpdatePictureData(string pictureUrl, bool uploadingPicture);

        private Microsoft.WindowsCE.Forms.InputPanel inputPanel = null;

        #region Properties
        private Yedda.Twitter.Account _AccountToSet = ClientSettings.DefaultAccount;
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
                cmbPlaces.Items.Clear();
                if(GPSLocation != null) {
                    cmbPlaces.Items.Add(GPSLocation);
                    cmbPlaces.SelectedIndex = 0;
                }
                Yedda.Twitter t = Servers.CreateConnection(_AccountToSet);
                AllowTwitPic = t.AllowTwitPic;
                UploadManager.Account = _AccountToSet;
            }
        }



        public bool AllowTwitPic
        {
            set
            {
                pictureFromCamers.Visible = UploadManager.CanUpload(MediaTypeGroup.PICTURE); 
                pictureFromStorage.Visible = UploadManager.CanUpload(MediaTypeGroup.PICTURE); 
                picInsertVideo.Visible = value && UploadManager.CanUpload(MediaTypeGroup.VIDEO);
                picAttachments.Visible = value && (UploadManager.CanUpload(MediaTypeGroup.DOCUMENT) || UploadManager.CanUpload(MediaTypeGroup.VIDEO) || UploadManager.CanUpload(MediaTypeGroup.AUDIO));
            }
        }

        public string StatusText
        {
            get
            {
                return txtStatusUpdate.Text;
            }
            set
            {
                txtStatusUpdate.Text = value;
                txtStatusUpdate.SelectionStart = txtStatusUpdate.Text.Length;
            }
        }

        public string in_reply_to_status_id { get; set; }

        #endregion


        public PostUpdate(bool Standalone)
        {
            _StandAlone = Standalone;
            
            InitializeComponent();
            SetImages();

            pictureService = GetMediaService();
            UploadManager = new AttachmentUploadManager(pictureService);
            
            if (ClientSettings.AutoCompleteAddressBook)
            {
                userListControl1.HookTextBoxKeyPress(txtStatusUpdate);
            }
            FormColors.SetColors(this);
            PockeTwit.Localization.XmlBasedResourceManager.LocalizeForm(this);
            if (ClientSettings.IsMaximized)
            {
                WindowState = FormWindowState.Maximized;
            }

            lblCharsLeft.Text = "140";
            PopulateAccountList();
            if (DetectDevice.DeviceType == DeviceType.Standard)
            {
                SmartPhoneMenu();
            }
            else
            {
                SetupTouchScreen();
            }
            mainMenu1.MenuItems.Add(menuCancel);
            PockeTwit.Localization.XmlBasedResourceManager.LocalizeMenu(this);
            ResumeLayout(false);

            LocationFinder.LocationReady += new LocationManager.delLocationReady(l_LocationReady);

            txtStatusUpdate.Focus();
            userListControl1.ItemChosen += new userListControl.delItemChose(userListControl1_ItemChosen);
        }
  
        private void SetupTouchScreen()
        {
            mainMenu1.MenuItems.Add(menuSubmit);
            
            pictureFromCamers.Click += new EventHandler(pictureFromCamers_Click);
            pictureFromStorage.Click += new EventHandler(pictureFromStorage_Click);
            picInsertVideo.Click += new EventHandler(picInsertVideo_Click);
            picAttachments.Click += new EventHandler(picAttachments_Click);
            pictureURL.Click += new EventHandler(pictureURL_Click);
            pictureLocation.Click += new EventHandler(pictureLocation_Click);
            picInsertGPSLink.Click += new EventHandler(llGPS_Click);

            picAddressBook.Click += new EventHandler(picAddressBook_Click);

            copyPasteMenu = new System.Windows.Forms.ContextMenu();

            PasteItem = new System.Windows.Forms.MenuItem();
            PasteItem.Text = PockeTwit.Localization.XmlBasedResourceManager.GetString("Paste");

            CopyItem = new MenuItem();
            CopyItem.Text = PockeTwit.Localization.XmlBasedResourceManager.GetString("Copy");

            copyPasteMenu.MenuItems.Add(CopyItem);
            copyPasteMenu.MenuItems.Add(PasteItem);
            txtStatusUpdate.ContextMenu = copyPasteMenu;

            CopyItem.Click += new EventHandler(CopyItem_Click);
            PasteItem.Click += new EventHandler(PasteItem_Click);

            inputPanel = new Microsoft.WindowsCE.Forms.InputPanel();
            oldInputPanelState = inputPanel.Enabled;

            inputPanel.EnabledChanged += new EventHandler(inputPanel_EnabledChanged);

            pnlToolbar.Visible = true;
        }

        void picAddressBook_Click(object sender, EventArgs e)
        {
            //txtStatusUpdate.Text = txtStatusUpdate.Text + "@";
            InsertTextAtCursor("@");
            userListControl1.Visible = true;
            userListControl1.Focus();
        }

        void userListControl1_ItemChosen(string itemText)
        {
            //txtStatusUpdate.Text = txtStatusUpdate.Text + itemText;
            //txtStatusUpdate.SelectionStart = txtStatusUpdate.Text.Length;
            InsertTextAtCursor(itemText);
        }

        void PasteItem_Click(object sender, EventArgs e)
        {
            IDataObject iData = Clipboard.GetDataObject();
            if (iData.GetDataPresent(DataFormats.Text))
            {
                txtStatusUpdate.SelectedText = (string)iData.GetData(DataFormats.Text);
            }
        }
        void CopyItem_Click(object sender, EventArgs e)
        {
            string selText = txtStatusUpdate.SelectedText;
            if (!string.IsNullOrEmpty(selText))
            {
                Clipboard.SetDataObject(selText);
            }
        }

        
        void l_LocationReady(GeoCoord Location, LocationManager.LocationSource Source)
        {
            // We're in a separate thread, probably - grab the places from Twitter
            if (Location != null)
            {
                if (GPSLocation != null && Source == LocationManager.LocationSource.RIL)
                    return; // if we've got a location and RIL gives another one, ignore it
                Twitter Conn = Servers.CreateConnection(AccountToSet);
                if (Conn is PlaceAPI)
                {
                    PlaceAPI PlaceSearch = Conn as PlaceAPI;
                    places = PlaceSearch.GetNearbyPlaces(Location);
                }
                DoLocationReady(Location, Source);
            }            
        }

        void DoLocationReady(GeoCoord Location, LocationManager.LocationSource Source)
        {
            try
            {
                if (InvokeRequired)
                {
                    LocationManager.delLocationReady d = new LocationManager.delLocationReady(DoLocationReady);
                    BeginInvoke(d, Location, Source);
                }
                else
                {
                    GPSLocation = Location;
                    lblGPS.Text = PockeTwit.Localization.XmlBasedResourceManager.GetString("Location Found");
                    lblGPS.Text += " (" + Source.ToString() + ")";
                    if (DetectDevice.DeviceType == DeviceType.Standard)
                    {
                        // just enable the menuItem
                        if (null != menuGPSInsert)
                        {
                            menuGPSInsert.Enabled = true;
                        }
                    }
                    else
                    {
                        picInsertGPSLink.Visible = true;
                    }
                    Place selP = null; // currently selected place, if any
                    if (cmbPlaces.SelectedItem is Place)
                    {
                        selP = cmbPlaces.SelectedItem as Place;
                    }
                    cmbPlaces.Items.Clear();
                    cmbPlaces.Visible = true;
                    cmbPlaces.Items.Add(GPSLocation);
                    cmbPlaces.SelectedIndex = 0;
                    if (places != null)
                    {
                        foreach (Place p in places)
                            cmbPlaces.Items.Add(p);
                    }
                    foreach (object item in cmbPlaces.Items)
                    {
                        if (item is Place)
                        {
                            Place p = item as Place;
                            if (p.Equals(selP))
                            {
                                cmbPlaces.SelectedItem = p;
                                break;
                            }
                        }
                    }
                    if (cmbPlaces.SelectedItem is GeoCoord && selP != null) // not selected a place
                    {
                        cmbPlaces.Items.Add(selP);
                        cmbPlaces.SelectedItem = selP;
                    }
                }
            }
            catch (ObjectDisposedException)
            {
            }

        }

        void llGPS_Click(object sender, EventArgs e)
        {
            InsertGpsLocation();
        }

        private void StartLocating()
        {   
            //StartAnimation
            cmbPlaces.Visible = false;
            LocationFinder.StopGPS(); // in case it's running
            LocationFinder.StartGPS();
            lblGPS.Text = PockeTwit.Localization.XmlBasedResourceManager.GetString("Seeking GPS");
            GPSLocation = null;
            //pictureLocation.Visible = false;
            //lblGPS.Visible = true;
        }

        

        private System.Windows.Forms.MenuItem menuExist;
        private System.Windows.Forms.MenuItem menuCamera;
        private System.Windows.Forms.MenuItem menuURL;
        private System.Windows.Forms.MenuItem menuGPS;
        private System.Windows.Forms.MenuItem menuGPSInsert;
        private System.Windows.Forms.MenuItem menuAddressBook;
        private System.Windows.Forms.MenuItem menuItem1;
        private void SmartPhoneMenu()
        {
            pnlToolbar.Visible = false;

            lblGPS.Width = lblGPS.Width + (lblGPS.Left - 5);
            lblGPS.Left = 5;
            pictureLocation.Visible = false;


            pictureFromCamers.Visible = false;
            pictureFromStorage.Visible = false;
            picAttachments.Visible = false;
            //pictureLocation.Visible = false;
            pictureURL.Visible = false;
            picAddressBook.Visible = false;
            menuExist = new MenuItem();
            menuExist.Text = PockeTwit.Localization.XmlBasedResourceManager.GetString("Existing Picture");
            menuExist.Click += new EventHandler(menuExist_Click);

            menuCamera = new MenuItem();
            menuCamera.Text = PockeTwit.Localization.XmlBasedResourceManager.GetString("Take Picture");
            menuCamera.Click += new EventHandler(menuCamera_Click);
            
            menuURL = new MenuItem();
            menuURL.Text = PockeTwit.Localization.XmlBasedResourceManager.GetString("URL...");
            menuURL.Click += new EventHandler(menuURL_Click);

            menuGPS = new MenuItem();
            menuGPS.Text = PockeTwit.Localization.XmlBasedResourceManager.GetString("Update Location");
            menuGPS.Click += new EventHandler(menuGPS_Click);

            menuGPSInsert = new MenuItem();
            menuGPSInsert.Text = PockeTwit.Localization.XmlBasedResourceManager.GetString("Insert GPS Location");
            menuGPSInsert.Click += new EventHandler(menuGPSInsert_Click);
            menuGPSInsert.Enabled = false;

            menuAddressBook = new MenuItem();
            menuAddressBook.Text = PockeTwit.Localization.XmlBasedResourceManager.GetString("Address Book");
            menuAddressBook.Click += new EventHandler(menuAddressBook_Click);
            menuAddressBook.Enabled = true;

            PasteItem = new MenuItem();
            PasteItem.Text = PockeTwit.Localization.XmlBasedResourceManager.GetString("Paste");
            PasteItem.Click += new EventHandler(PasteItem_Click);

            menuItem1 = new System.Windows.Forms.MenuItem();
            menuItem1.Text = PockeTwit.Localization.XmlBasedResourceManager.GetString("Action");

            menuItem1.MenuItems.Add(menuSubmit);
            menuItem1.MenuItems.Add(menuAddressBook);
            menuItem1.MenuItems.Add(PasteItem);
            menuItem1.MenuItems.Add(menuURL);
            menuItem1.MenuItems.Add(menuExist);
            menuItem1.MenuItems.Add(menuCamera);
            menuItem1.MenuItems.Add(menuGPS);
            menuItem1.MenuItems.Add(menuGPSInsert);
            mainMenu1.MenuItems.Add(menuItem1);
        }

        void menuAddressBook_Click(object sender, EventArgs e)
        {
            txtStatusUpdate.Text = txtStatusUpdate.Text + "@";
            userListControl1.Visible = true;
            userListControl1.Focus();
        }

        void menuGPSInsert_Click(object sender, EventArgs e)
        {
            InsertGpsLocation();
        }

        private void InsertGpsLocation()
        {
            // google maps url format
            // http://maps.google.com/maps?f=q&source=s_q&hl=en&geocode=&q=41.4043197631836,-1.28760504722595
            Cursor.Current = Cursors.WaitCursor;
            string sUrl = string.Format(@"http://maps.google.com/maps?q={0}", GPSLocation);
            string gpsUrl = isgd.ShortenURL(sUrl);
            if (string.IsNullOrEmpty(gpsUrl))
            {
                Cursor.Current = Cursors.Default;
                PockeTwit.Localization.LocalizedMessageBox.Show("A communication error occured shortening the URL. Please try again later.");
                return;
            }
            txtStatusUpdate.Text = txtStatusUpdate.Text + " " + gpsUrl;
            Cursor.Current = Cursors.Default;
        }

        void menuGPS_Click(object sender, EventArgs e)
        {
            StartLocating();
        }

        void menuURL_Click(object sender, EventArgs e)
        {
            InsertURL();
        }

        void menuCamera_Click(object sender, EventArgs e)
        {
            InsertPictureFromCamera();
        }

        void menuExist_Click(object sender, EventArgs e)
        {
            InsertPictureFromFile();
        }

        private void SetImages()
        {
            pictureFromCamers.Image = PockeTwit.Themes.FormColors.GetThemeIcon("takepicture", pictureFromCamers.Height);
            pictureFromStorage.Image = PockeTwit.Themes.FormColors.GetThemeIcon("existingimage", pictureFromStorage.Height);
            picInsertVideo.Image = PockeTwit.Themes.FormColors.GetThemeIcon("video", pictureFromStorage.Height);
            picAttachments.Image = PockeTwit.Themes.FormColors.GetThemeIcon("attachment", picAttachments.Height);
            pictureURL.Image = PockeTwit.Themes.FormColors.GetThemeIcon("url", pictureURL.Height);
            pictureLocation.Image = PockeTwit.Themes.FormColors.GetThemeIcon("map", pictureLocation.Height);
            picAddressBook.Image = PockeTwit.Themes.FormColors.GetThemeIcon("address", picAddressBook.Height);
            picInsertGPSLink.Image = PockeTwit.Themes.FormColors.GetThemeIcon("insertgpslink", picInsertGPSLink.Height);
        }

        private void PopulateAccountList()
        {
            foreach (Yedda.Twitter.Account Account in ClientSettings.AccountsList)
            {
                cmbAccount.Items.Add(Account);
            }
            pnlAccounts.Visible = cmbAccount.Items.Count > 1;
        }

        #region Methods
        private void InsertURL()
        {
            using (URLForm f = new URLForm())
            {
                f.Owner = this;
                if (f.ShowDialog() == DialogResult.OK)
                {
                    txtStatusUpdate.Text = txtStatusUpdate.Text + " " + f.URL;
                    txtStatusUpdate.SelectionStart = txtStatusUpdate.Text.Length;
                    txtStatusUpdate.SelectionLength = 0;
                }
                f.Dispose();
            }
        }

        private void InsertPictureFromCamera()
        {
            InsertPictureFromCamera(false);
        }

        private void InsertPictureFromCamera(bool video)
        {
            String pictureUrl = string.Empty;
            String filename = string.Empty;
            try
            {
                using (Microsoft.WindowsMobile.Forms.CameraCaptureDialog c = new Microsoft.WindowsMobile.Forms.CameraCaptureDialog())
                {
                    c.Owner = this;
                    if (video)
                    {
                        c.Mode = Microsoft.WindowsMobile.Forms.CameraCaptureMode.VideoWithAudio;
                        c.VideoTypes = Microsoft.WindowsMobile.Forms.CameraCaptureVideoTypes.Standard;
                    }
                    else
                        c.Mode = Microsoft.WindowsMobile.Forms.CameraCaptureMode.Still;
                    this.WindowState = FormWindowState.Normal;
                    if (c.ShowDialog() == DialogResult.OK)
                    {
                        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PostUpdate));
                        pictureFromStorage.Image = PockeTwit.Themes.FormColors.GetThemeIcon("existingimage", pictureFromStorage.Height);
                        if (DetectDevice.DeviceType == DeviceType.Standard)
                        {
                            pictureFromStorage.Visible = false;
                        }
                        filename = c.FileName;
                    }
                    else
                        return;
                    if(ClientSettings.IsMaximized)
                        this.WindowState = FormWindowState.Maximized;
                    c.Dispose();
                }
            }
            catch
            {
                PockeTwit.Localization.LocalizedMessageBox.Show("The camera is not available.", "PockeTwit");
                return;
            }
            if (string.IsNullOrEmpty(filename))
            {
                return; //no file selected, so don't upload
            }
            try
            {
                pictureService = GetMediaService();
                
                StartUpload(pictureService, filename, _AccountToSet);
            }
            catch
            {
                PockeTwit.Localization.LocalizedMessageBox.Show("Unable to upload picture.", "PockeTwit");
            }
        }

        private void InsertFile()
        {
            String filename = string.Empty;
            pictureService = GetMediaService();

            try
            {
                filename = SelectFileNormal(UploadManager.GetFileFilter(MediaTypeGroup.ALL));
            }
            catch
            {
                PockeTwit.Localization.LocalizedMessageBox.Show("Unable to select picture.", "PockeTwit");
            }
            if (string.IsNullOrEmpty(filename))
                return;

            try
            {
                StartUpload(pictureService, filename, _AccountToSet);
            }
            catch
            {
                PockeTwit.Localization.LocalizedMessageBox.Show("Unable to upload picture.", "PockeTwit");
            }
        }


        private void InsertPictureFromFile()
        {
            String pictureUrl = string.Empty;
            String filename = string.Empty;
            //using (Microsoft.WindowsMobile.Forms.SelectPictureDialog s = new Microsoft.WindowsMobile.Forms.SelectPictureDialog())
            try
            {
                pictureService = GetMediaService();
                filename = SelectFileVisual(UploadManager.GetFileFilter(MediaTypeGroup.PICTURE));

                pictureFromCamers.Image = FormColors.GetThemeIcon("takepicture", pictureFromCamers.Height);
                if (DetectDevice.DeviceType == DeviceType.Standard)
                {
                    pictureFromCamers.Visible = false;
                }
            }
            catch
            {
                PockeTwit.Localization.LocalizedMessageBox.Show("Unable to select picture.", "PockeTwit");
            }
            if  (string.IsNullOrEmpty(filename))
                return;

            try
            {
                StartUpload(pictureService, filename, _AccountToSet);
            }
            catch
            {
                PockeTwit.Localization.LocalizedMessageBox.Show("Unable to upload picture.", "PockeTwit");
            } 
        }

        private string SelectFileVisual(String fileFilter)
        {
            string filename = string.Empty;
            using (Microsoft.WindowsMobile.Forms.SelectPictureDialog fileDialog = new Microsoft.WindowsMobile.Forms.SelectPictureDialog())
            {
                fileDialog.Filter = fileFilter;
                fileDialog.Owner = this;
                this.WindowState = FormWindowState.Normal;
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    filename = fileDialog.FileName;
                    fileDialog.Dispose();
                }
                if (ClientSettings.IsMaximized)
                    this.WindowState = FormWindowState.Maximized;
            }
            return filename;
        }

        private string SelectFileNormal(String fileFilter)
        {
            string filename = string.Empty;
            using (System.Windows.Forms.OpenFileDialog fileDialog = new OpenFileDialog())
            {
                this.WindowState = FormWindowState.Normal;
                fileDialog.Filter = fileFilter;
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    
                    filename = fileDialog.FileName;
                }
                if (ClientSettings.IsMaximized)
                    this.WindowState = FormWindowState.Maximized;
            }
            return filename;
        }

        private void StartUpload(IPictureService mediaService, String fileName, Twitter.Account account)
        {
            UploadAttachment Attachment = new UploadAttachment(fileName, txtStatusUpdate.Text, GPSLocation);
            UploadManager.AddAttachment(Attachment);

            Cursor.Current = Cursors.WaitCursor;
            UploadManager.Upload(Attachment);

            if (Attachment.UploadedUri != null)
            {
                picAttachments.Image = Themes.FormColors.GetThemeIcon("insertlink", picAttachments.Height);
                UpdatePictureData(Attachment.UploadedUri.OriginalString, false);
            }
            Cursor.Current = Cursors.Default;
        }

        /// <summary>
        /// Set all the event handlers for the chosen picture service.
        /// Aparently after posting, event set are lost so these have to be set again.
        /// </summary>
        /// <param name="pictureService">Picture service on which the event handlers should be set.</param>
        private void SetPictureEventHandlers(IPictureService pictureService, bool addEvents)
        {
            if (!localPictureEventsSet && addEvents)
            {
                //No need to set finish upload event when posting to it
                if (!pictureService.CanUploadMessage || !ClientSettings.SendMessageToMediaService)
                {
                    pictureService.UploadFinish += new UploadFinishEventHandler(pictureService_UploadFinish);
                }
                pictureService.MessageReady += new MessageReadyEventHandler(pictureService_MessageReady);
                pictureService.ErrorOccured += new ErrorOccuredEventHandler(pictureService_ErrorOccured);
                pictureService.UploadPart += new UploadPartEventHandler(pictureService_UploadPart);
                localPictureEventsSet = true;
            }
            else if (localPictureEventsSet && !addEvents)
            {
                //No need to remove finish upload event when posting to it 
                if (!pictureService.CanUploadMessage || !ClientSettings.SendMessageToMediaService)
                {
                    pictureService.UploadFinish -= new UploadFinishEventHandler(pictureService_UploadFinish);
                }   
                pictureService.MessageReady -= new MessageReadyEventHandler(pictureService_MessageReady);
                pictureService.ErrorOccured -= new ErrorOccuredEventHandler(pictureService_ErrorOccured);
                pictureService.UploadPart -= new UploadPartEventHandler(pictureService_UploadPart);
                localPictureEventsSet = false;
            }
        }

        private void pictureService_ErrorOccured(object sender, PictureServiceEventArgs eventArgs)
        {
            //Show the error message
            UpdatePictureData(string.Empty, false);
            
            AddPictureToForm(FormColors.GetThemeIconPath("existingimage", pictureFromStorage.Height), pictureFromStorage);
            AddPictureToForm(FormColors.GetThemeIconPath("takepicture", pictureFromCamers.Height), pictureFromCamers);
            picAttachments.Image = Themes.FormColors.GetThemeIcon("insertlink", picAttachments.Height);
            MessageBox.Show(eventArgs.ErrorMessage);
        }

        private void pictureService_MessageReady(object sender, PictureServiceEventArgs eventArgs)
        {
            //Show the message
            MessageBox.Show(eventArgs.ReturnMessage);

            AddPictureToForm(FormColors.GetThemeIconPath("existingimage", pictureFromStorage.Height), pictureFromStorage);
            AddPictureToForm(FormColors.GetThemeIconPath("takepicture", pictureFromCamers.Height), pictureFromCamers);
            picAttachments.Image = Themes.FormColors.GetThemeIcon("insertlink", picAttachments.Height);
            UpdatePictureData(string.Empty, false);

        }

        /// <summary>
        /// Event handling for when the upload is finished
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void pictureService_UploadFinish(object sender, PictureServiceEventArgs eventArgs)
        {
            //AddPictureToForm(eventArgs.PictureFileName, picAttachments);
//            picAttachments.Image = Themes.FormColors.GetThemeIcon("insertlink", picAttachments.Height);
//            UpdatePictureData(eventArgs.ReturnMessage, false);
        }

        /// <summary>
        /// Event handling for when an upload is in progress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void pictureService_UploadPart(object sender, PictureServiceEventArgs eventArgs)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("Sent: {0} / {1} bytes", eventArgs.BytesDownloaded, eventArgs.TotalBytesToDownload));
        }
        
        private IPictureService GetMediaService()
        {
            IPictureService service;
            
            service = PictureServiceFactory.Instance.GetServiceByName(ClientSettings.SelectedMediaService);

            SetPictureEventHandlers(service, true);

            return service;
        }

        /// <summary>
        /// Put the picture in the form.
        /// </summary>
        /// <param name="ImageFile"></param>
        /// <param name="BoxToUpdate"></param>
        private void AddPictureToForm(string ImageFile, PictureBox BoxToUpdate)
        {
            if (InvokeRequired)
            {
                delAddPicture d = new delAddPicture(AddPictureToForm);
                BeginInvoke(d, ImageFile, BoxToUpdate);
            }
            else
            {
                try
                {
                    BoxToUpdate.Image = new System.Drawing.Bitmap(ImageFile);
                    if (DetectDevice.DeviceType == DeviceType.Standard && lblGPS.Visible)
                    {
                        BoxToUpdate.Left = lblGPS.Right + 5;
                    }
                    BoxToUpdate.Visible = true;
                    txtStatusUpdate_TextChanged(null, new EventArgs());
                }
                catch (OutOfMemoryException)
                {
                    BoxToUpdate.Image = PockeTwit.Themes.FormColors.GetThemeIcon("insertlink", BoxToUpdate.Height);
                }
                catch (Exception) // if it wasn't an image
                {
                }
            }
        }

        private void UpdatePictureData(string pictureURL, bool uploadingPicture)
        {
            if (InvokeRequired)
            {
                delUpdatePictureData d = new delUpdatePictureData(UpdatePictureData);
                BeginInvoke(d, pictureURL, uploadingPicture);
            }
            else
            {
                try
                {
                    Cursor.Current = Cursors.Default;
                    if (!string.IsNullOrEmpty(pictureURL))
                    {
                        if (UploadManager.Count > 0) // it should be, otherwise something is up
                            UploadManager[0].UploadedUri = new Uri(pictureURL);
                        if (txtStatusUpdate.Text.Length > 0)
                            InsertTextAtCursor(' ' + pictureURL);
                        else
                            InsertTextAtCursor(pictureURL);
                    }
                }
                catch (OutOfMemoryException)
                {
                }
            }
        }

        private string TrimTo140(string Original, int charMax)
        {
            try
            {
                if (Original.Length > charMax)
                {
                    Original = TryToShrinkWith140It(Original, charMax);
                    txtStatusUpdate.Text = Original;
                }
            }
            catch{}
            try
            {
                if (Original.Length > charMax)
                {
                    Original = TryToUseShortText(Original);
                    txtStatusUpdate.Text = Original;
                }
            }
            catch{}
            return Original;
        }

        private static string TryToShrinkWith140It(string original, int charMax)
        {
            if(PockeTwit.Localization.LocalizedMessageBox.Show("The text is too long.  Would you like to use abbreviations to shorten it?", "Long Text", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1)== System.Windows.Forms.DialogResult.Yes)
            {
                var shrinker = new _140it();
                return shrinker.GetShortenedText(original, charMax);
            }
            return original;
        }

        private string TryToUseShortText(string original)
        {
            if (PockeTwit.Localization.LocalizedMessageBox.Show("The text is too long.  Would you like to add a link to a site with the full text?", "Long Text", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
            {
                var shrinker = new ShortText();

                return shrinker.GetShortenedText(original);
            }
            return original;

        }

        private bool PostTheUpdate()
        {
            LocationFinder.StopGPS();
            string Message = StatusText;

            if (UploadManager.GetUriSpaceRequired(Message) > 0) // need to append links
            {
                if (PockeTwit.Localization.LocalizedMessageBox.Show("Uploaded file not used, add automatically?", "PockeTwit", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                {
                    StatusText = UploadManager.AppendLinks(Message);
                    //return false;
                }
            }
            if (!string.IsNullOrEmpty(Message))
            {
                Cursor.Current = Cursors.WaitCursor;
                var updateText = TrimTo140(StatusText, 140);

                if(updateText.Length>140)
                {
                    if (PockeTwit.Localization.LocalizedMessageBox.Show("The text is still too long.  If you post it twitter will cut off the end.  Post anyway?", "Long Text", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.No)
                    {
                        return false;
                    }
                }
                
                {


                    Yedda.Twitter TwitterConn = Yedda.Servers.CreateConnection(AccountToSet) ;

                    Place p = null;
                    object o = cmbPlaces.SelectedItem;

                    if(o is Place)
                        p = o as Place;

                    string retValue = TwitterConn.Update(
                            updateText, 
                            in_reply_to_status_id, 
                            new PockeTwit.Position.UserLocation {
                                Position = GPSLocation, Location = p },
                             Yedda.Twitter.OutputFormatType.XML);

                    if (string.IsNullOrEmpty(retValue))
                    {
                        PockeTwit.Localization.LocalizedMessageBox.Show("Error posting status -- empty response.  You may want to try again later.");
                        return false;
                    }
                    try
                    {
                        Library.status.DeserializeSingle(retValue, AccountToSet);
                    }
                    catch
                    {
                        PockeTwit.Localization.LocalizedMessageBox.Show("Error posting status -- bad response.  You may want to try again later.");
                        return false;
                    }

                    return true;
                }
            }
            return true;
        }


        #endregion

        #region Events
        private void txtStatusUpdate_TextChanged(object sender, EventArgs e)
        {
            int charsAvail = 140;
            int lengthLeft = charsAvail - txtStatusUpdate.Text.Length;
            
            // Get the upload manager to check length left
            lengthLeft -= UploadManager.GetUriSpaceRequired(txtStatusUpdate.Text);
            lblCharsLeft.Text = lengthLeft.ToString();
            if (lengthLeft < 0)
                lblCharsLeft.ForeColor = ClientSettings.ErrorColor;
            else
                lblCharsLeft.ForeColor = ClientSettings.ForeColor;
        }
        private void menuCancel_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtStatusUpdate.Text))
            {
                if (PockeTwit.Localization.LocalizedMessageBox.Show("Are you sure you want to cancel the update?", "Cancel", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.No)
                {
                    return;
                }
            }

            SetPictureEventHandlers(pictureService, false);
            UpdatePictureData(string.Empty, false);

            //Making sure gps is stopped when a location is not found yet.
            LocationFinder.StopGPS();

            if (_StandAlone)
            {
                Close();
            }
            DialogResult = DialogResult.Cancel;
        }
        void pictureLocation_Click(object sender, EventArgs e)
        {
            StartLocating();
        }
        void pictureURL_Click(object sender, EventArgs e)
        {
            InsertURL();
        }

        void InsertTextAtCursor(string Text)
        {
            int startPos = txtStatusUpdate.SelectionStart;
            txtStatusUpdate.Text =
                txtStatusUpdate.Text.Substring(0, startPos)
                + Text
                + txtStatusUpdate.Text.Substring(startPos);
            txtStatusUpdate.SelectionStart = startPos + Text.Length;
            txtStatusUpdate.Focus();
        }

        void pictureFromStorage_Click(object sender, EventArgs e)
        {
            if (UploadManager.Count == 0 || UploadManager[0].UploadedUri == null /*|| ClientSettings.SendMessageToMediaService*/)
            {
                UploadManager.Clear();
                InsertPictureFromFile();
            }
            else
            {
                
                if (PockeTwit.Localization.LocalizedMessageBox.Show("Paste URL in message?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    InsertTextAtCursor(UploadManager[0].UploadedUri.OriginalString);
                }
                else
                {
                    if (PockeTwit.Localization.LocalizedMessageBox.Show("Load a new picture?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                    {
                        UploadManager.Clear(); 
                        InsertPictureFromFile();
                    }
                }
            }
        }
        void pictureFromCamers_Click(object sender, EventArgs e)
        {
            if (UploadManager.Count == 0 || UploadManager[0].UploadedUri == null /*|| ClientSettings.SendMessageToMediaService*/)
            {
                UploadManager.Clear();
                InsertPictureFromCamera();
            }
            else
            {

                if (PockeTwit.Localization.LocalizedMessageBox.Show("Paste URL in message?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    InsertTextAtCursor(UploadManager[0].UploadedUri.OriginalString);
                }
                else
                {
                    if (PockeTwit.Localization.LocalizedMessageBox.Show("Take a new picture?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                    {
                        UploadManager.Clear();
                        InsertPictureFromCamera();
                    }
                }
            }
        }
        
        private void menuSubmit_Click(object sender, EventArgs e)
        {
            ContinuePost();
        }

        private void ContinuePost()
        {
            bool Success = PostTheUpdate();

            Cursor.Current = Cursors.Default;
            if (Success)
            {
                UpdatePictureData(string.Empty, false);
                SetPictureEventHandlers(pictureService, false);
                if (_StandAlone)
                {
                    Close();
                }
                //Making sure gps is stopped when a location is not found yet.
                LocationFinder.StopGPS();
                DialogResult = DialogResult.OK;
            }
        }

        private void cmbAccount_SelectedIndexChanged(object sender, EventArgs e)
        {
            AccountToSet = (Yedda.Twitter.Account)cmbAccount.SelectedItem;
        }

        #endregion

        protected override void OnClosed(EventArgs e)
        {
            if (ClientSettings.AutoCompleteAddressBook)
            {
                userListControl1.UnHookTextBoxKeyPress();
            }
            base.OnClosed(e);
        }

        private void inputPanel_EnabledChanged(object sender, EventArgs e)
        {
            Panel[] panels = {
                                pnlStatus,
                                pnlAccounts,
                                pnlToolbar
                            };
            bool[] conditions = {
                                        true,
                                        cmbAccount.Items.Count > 1,
                                        DetectDevice.DeviceType == DeviceType.Professional
                }; 
            // on touchscreen phones, the panel has been shown/hidden
            if (DetectDevice.DeviceType == DeviceType.Professional)
            {
                if (inputPanel != null && inputPanel.Enabled)
                {
                    int availHeight = inputPanel.VisibleDesktop.Height + inputPanel.VisibleDesktop.Y - this.Top;
                    int safeHeight = (pnlStatus.Height * 3) / 2; // recommended height of text box

                    for (int i = 0; i < panels.Length; i++)
                    {
                        if (conditions[0])
                        {
                            if (safeHeight + panels[i].Height > availHeight)
                                panels[i].Visible = false;
                            else
                                safeHeight += panels[i].Height;
                        }
                    }

                    pnlSipSize.Height = availHeight;
                }
                else
                {
                    pnlSipSize.Height = this.Height;
                    for (int i = 0; i < panels.Length; i++)
                        panels[i].Visible = conditions[i];
                }
            }
        }

        private void txtStatusUpdate_GotFocus(object sender, EventArgs e)
        {
            if (inputPanel != null && ClientSettings.PopUpKeyboard)
            {
                inputPanel.Enabled = true;
            }
        }

        private void PostUpdate_Closed(object sender, EventArgs e)
        {
            if (inputPanel != null)
            {
                inputPanel.EnabledChanged -= inputPanel_EnabledChanged;
                inputPanel.Enabled = oldInputPanelState; // probably going back to the main screen
            }
        }

        private void PostUpdate_Resize(object sender, EventArgs e)
        {
/*            if (ClientSettings.IsMaximized)
                this.WindowState = FormWindowState.Maximized;
            else
                this.WindowState = FormWindowState.Normal;*/
//            this.BringToFront();

        }

        private void picAttachments_Click(object sender, EventArgs e)
        {
            if (UploadManager.Count == 0 || UploadManager[0].UploadedUri == null /*|| ClientSettings.SendMessageToMediaService*/)
            {
                UploadManager.Clear();
                InsertFile();
            }
            else
            {

                if (PockeTwit.Localization.LocalizedMessageBox.Show("Paste URL in message?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    InsertTextAtCursor(UploadManager[0].UploadedUri.OriginalString);
                }
                else
                {
                    if (PockeTwit.Localization.LocalizedMessageBox.Show("Select a new file?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                    {
                        UploadManager.Clear();
                        InsertFile();
                    }
                }
            }
       }

        private void picInsertVideo_Click(object sender, EventArgs e)
        {
            if (UploadManager.Count == 0 || UploadManager[0].UploadedUri == null /*|| ClientSettings.SendMessageToMediaService*/)
            {
                UploadManager.Clear();
                InsertPictureFromCamera(true);
            }
            else
            {

                if (PockeTwit.Localization.LocalizedMessageBox.Show("Paste URL in message?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    InsertTextAtCursor(UploadManager[0].UploadedUri.OriginalString);
                }
                else
                {
                    if (PockeTwit.Localization.LocalizedMessageBox.Show("Take a new video?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                    {
                        UploadManager.Clear();
                        InsertPictureFromCamera(true);
                    }
                }
            }

        }
    }
}
