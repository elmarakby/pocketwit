﻿using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Web;
using System.Collections.Generic;
using System.Text;
using OAuth;
using Yedda;

namespace PockeTwit.MediaServices
{
    public class SimplifiedTwitPic : PictureServiceBase
    {
        #region private properties
        private static string TwitPicKey = "5b976ad6e50575acef064fe98ae67bcc";

        private static volatile SimplifiedTwitPic _instance;
        private static object syncRoot = new Object();

        private const string API_UPLOAD = "http://api.twitpic.com/2/upload.xml";
        private const string API_UPLOAD_POST = "http://api.twitpic.com/2/upload.xml";
        private const string API_SHOW_THUMB = "http://twitpic.com/show/large/";  //The extra / for directly sticking the image-id on.

        private const string API_ERROR_UPLOAD = "Unable to upload to TwitPic";
        private const string API_ERROR_DOWNLOAD = "Unable to download from TwitPic";

        private Twitter.Account account = null;
        #endregion

        #region private objects

        private System.Threading.Thread workerThread;
        private PicturePostObject workerPPO;

        #endregion

        #region constructor
        /// <summary>
        /// Private constructor for usage in singleton.
        /// </summary>
        private SimplifiedTwitPic()
        {
            API_SAVE_TO_PATH = "\\ArtCache\\www.twitpic.com\\";
            API_SERVICE_NAME = "TwitPic (New Upload Method)";
            API_CAN_UPLOAD_GPS = false;
            API_CAN_UPLOAD_MESSAGE = false;
            API_URLLENGTH = 25;

            API_FILETYPES.Add(new MediaType("jpg", "image/jpeg", MediaTypeGroup.PICTURE));
            API_FILETYPES.Add(new MediaType("jpeg", "image/jpeg", MediaTypeGroup.PICTURE));
            API_FILETYPES.Add(new MediaType("gif", "image/gif", MediaTypeGroup.PICTURE));
            API_FILETYPES.Add(new MediaType("png", "image/png", MediaTypeGroup.PICTURE));
        }

        /// <summary>
        /// Singleton constructor
        /// </summary>
        /// <returns></returns>
        public static SimplifiedTwitPic Instance
        {
           get
           {
               if (_instance == null)
               {
                   lock (syncRoot)
                   {
                       if (_instance == null)
                       {
                           _instance = new SimplifiedTwitPic();
                           _instance.HasEventHandlersSet = false;
                       }
                   }
               }
               return _instance;
           }
        }


        #endregion

        #region IPictureService Members


        /// <summary>
        /// Fetch a picture
        /// </summary>
        /// <param name="pictureURL"></param>
        public override void FetchPicture(string pictureURL, Twitter.Account account)
        {
            #region Argument check

            this.account = account;

            //Need a url to read from.
            if (string.IsNullOrEmpty(pictureURL))
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, "", API_ERROR_DOWNLOAD));
            }

            #endregion

            try
            {
                workerPPO = new PicturePostObject();
                workerPPO.Message = pictureURL;

                if (workerThread == null)
                {
                    workerThread = new System.Threading.Thread(new System.Threading.ThreadStart(ProcessDownload));
                    workerThread.Name = "PictureUpload";
                    workerThread.Start();
                }
                else
                {
                    OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.NotReady, "", "A request is already running."));
                }
            }
            catch (Exception)
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, "", API_ERROR_DOWNLOAD));
            }
        }

        
        /// <summary>
        /// Post a picture
        /// </summary>
        /// <param name="postData"></param>
        public override void PostPicture(PicturePostObject postData, Twitter.Account account)
        {
            DoPost(postData, account, true);
        }

        /// <summary>
        /// Post a picture including a message to the media service.
        /// </summary>
        /// <param name="postData"></param>
        /// <returns></returns>
        public override bool PostPictureMessage(PicturePostObject postData, Twitter.Account account)
        {
            return DoPost(postData, account, false);
        }

        private bool DoPost(PicturePostObject postData, Twitter.Account account, bool successEvent)
        {
            this.account = account;
            #region Argument check

            this.account = account;

            //Check for empty path
            if (string.IsNullOrEmpty(postData.Filename))
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
            }

            //Check for empty credentials
            if (string.IsNullOrEmpty(account.OAuth_token) ||
                string.IsNullOrEmpty(account.OAuth_token_secret))
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
            }

            #endregion

            using (System.IO.FileStream file = new FileStream(postData.Filename, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    #region async
                    //if (postData.UseAsync)
                    //{
                    //    workerPPO = (PicturePostObject) postData.Clone();
                    //    workerPPO.PictureData = incoming;

                    //    if (workerThread == null)
                    //    {
                    //        workerThread = new System.Threading.Thread(new System.Threading.ThreadStart(ProcessUpload));
                    //        workerThread.Name = "PictureUpload";
                    //        workerThread.Start();
                    //    }
                    //    else
                    //    {
                    //        OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.NotReady, string.Empty, "A request is already running."));
                    //    }
                    //}
                    //else
                    //{
                    #endregion
                    //use sync.

                    postData.PictureStream = file;
                    XmlDocument uploadResult = UploadPicture(API_UPLOAD, postData, account);

                    if (uploadResult == null) // occurs in the event of an error
                        return false;
                    if(successEvent)
                    {
                        string URL = uploadResult.SelectSingleNode("//url").InnerText;
                        OnUploadFinish(new PictureServiceEventArgs(PictureServiceErrorLevel.OK, URL, string.Empty, postData.Filename));
                    }
                }
                catch (Exception ex)
                {
                    OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Test whether this service can fetch a picture.
        /// </summary>
        /// <param name="URL"></param>
        /// <returns></returns>
        public override bool CanFetchUrl(string URL)
        {
            const string siteMarker = "twitpic";
            string url = URL.ToLower();

            return (url.IndexOf(siteMarker) >= 0);
        }

        #endregion

        #region thread implementation

        private void ProcessDownload()
        {
            try
            {
                string pictureURL = workerPPO.Message;
                int imageIdStartIndex = pictureURL.LastIndexOf('/') + 1;
                string imageID = pictureURL.Substring(imageIdStartIndex, pictureURL.Length - imageIdStartIndex);

                string resultFileName = RetrievePicture(imageID, account);

                if (!string.IsNullOrEmpty(resultFileName))
                {
                    OnDownloadFinish(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, resultFileName, string.Empty, pictureURL));
                }
            }
            catch (Exception)
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_DOWNLOAD));
            }
            workerThread = null;
        }

        private void ProcessUpload()
        {
            try
            {
                XmlDocument uploadResult = UploadPicture(API_UPLOAD, workerPPO, account);

                if (uploadResult.SelectSingleNode("rsp").Attributes["stat"].Value == "fail")
                {
                    string ErrorText = uploadResult.SelectSingleNode("//err").Attributes["msg"].Value;
                    OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, ErrorText));
                }
                else
                {
                    string URL = uploadResult.SelectSingleNode("//mediaurl").InnerText;
                    OnUploadFinish(new PictureServiceEventArgs(PictureServiceErrorLevel.OK, URL, string.Empty, workerPPO.Filename));
                }
            }
            catch (Exception)
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
            }
            workerThread = null;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Use a imageId to retrieve and save a thumbnail to the device.
        /// </summary>
        /// <param name="imageId">Id for the image</param>
        /// <returns></returns>
        private string RetrievePicture(string imageId, Twitter.Account account)
        {
            try
            {
                HttpWebRequest myRequest = WebRequestFactory.CreateHttpRequest(API_SHOW_THUMB + imageId);
                OAuthAuthorizer.AuthorizeEcho(myRequest, account.OAuth_token, account.OAuth_token_secret);

                myRequest.Method = "GET";
                String pictureFileName = String.Empty;

                using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
                {
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        int totalSize = 0;
                        int totalResponseSize = (int)response.ContentLength;
                        byte[] readBuffer = new byte[PT_READ_BUFFER_SIZE];
                        pictureFileName = GetPicturePath(imageId);

                        int responseSize = dataStream.Read(readBuffer, 0, PT_READ_BUFFER_SIZE);
                        totalSize = responseSize;
                        OnDownloadPart(new PictureServiceEventArgs(responseSize, totalSize, totalResponseSize));
                        while (responseSize > 0)
                        {
                            SavePicture(pictureFileName, readBuffer, responseSize);
                            try
                            {
                                totalSize += responseSize;
                                responseSize = dataStream.Read(readBuffer, 0, PT_READ_BUFFER_SIZE);
                                OnDownloadPart(new PictureServiceEventArgs(responseSize, totalSize, totalResponseSize));
                            }
                            catch
                            {
                                responseSize = 0;
                            }
                            System.Threading.Thread.Sleep(100);
                        }
                        dataStream.Close();
                    }
                    response.Close();
                }

                return pictureFileName;
            }
            catch (Exception)
            {
                 OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_DOWNLOAD));
                 return string.Empty;
            }
        }


        /// <summary>
        /// Upload the picture
        /// </summary>
        /// <param name="url">URL to upload picture to</param>
        /// <param name="ppo">Postdata</param>
        /// <returns></returns>
        private XmlDocument UploadPicture(string url, PicturePostObject ppo, Twitter.Account account)
        {
            try
            {
                HttpWebRequest request = WebRequestFactory.CreateHttpRequest(url);
                request.Timeout = 20000;
                request.AllowWriteStreamBuffering = false; // don't want to buffer 3MB files, thanks
                request.AllowAutoRedirect = false;

                Multipart contents = new Multipart();
                contents.Add("key", TwitPicKey);

                if (!string.IsNullOrEmpty(ppo.Message))
                    contents.Add("message", ppo.Message);
                else
                    contents.Add("message", "");

                contents.Add("media", ppo.PictureStream, Path.GetFileName(ppo.Filename));

                OAuthAuthorizer.AuthorizeEcho(request, account.OAuth_token, account.OAuth_token_secret);

                return contents.UploadXML(request);
            }
            catch (Exception e)
            {
                //Socket exception 10054 could occur when sending large files.

                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                return null;
            }
        }



        /// <summary>
        /// Upload the picture
        /// </summary>
        /// <param name="url">URL to upload picture to</param>
        /// <param name="ppo">Postdata</param>
        /// <returns></returns>
        private XmlDocument UploadPictureMessage(string url, PicturePostObject ppo, Twitter.Account account)
        {
            // they both did the same thing
            return UploadPicture(url, ppo, account);
        }
        #endregion
    }
}