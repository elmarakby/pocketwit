﻿using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Web;
using System.Collections.Generic;
using System.Text;

namespace Yedda
{
    public class yFrog : PictureServiceBase
    {
        #region private properties

        private static volatile yFrog _instance;
        private static object syncRoot = new Object();
        
        private const string API_UPLOAD = "http://yFrog.com/api/upload";
        private const string API_UPLOAD_POST = "http://yFrog.com/api/uploadAndPost";
        private const string API_SHOW_THUMB = "http://yFrog.com/";  //The extra / for directly sticking the image-id on.

        private const string API_ERROR_UPLOAD = "Failed to upload picture to yFrog.";
        private const string API_ERROR_NOTREADY = "A request is already running.";
        private const string API_ERROR_DOWNLOAD = "Unable to download picture, try again later.";

        #endregion

        #region private objects

        private System.Threading.Thread workerThread;
        private PicturePostObject workerPPO;

        #endregion

        #region constructor
        /// <summary>
        /// Private constructor for usage in singleton.
        /// </summary>
        private yFrog()
        {
            API_SAVE_TO_PATH = "\\ArtCache\\www.yFrog.com\\";
            API_SERVICE_NAME = "yFrog";
        }

        /// <summary>
        /// Singleton constructor
        /// </summary>
        /// <returns></returns>
        public static yFrog Instance
        {
           get
           {
               if (_instance == null)
               {
                   lock (syncRoot)
                   {
                       if (_instance == null)
                       {
                           _instance = new yFrog();
                           _instance.HasEventHandlersSet = false;
                       }
                   }
               }
               return _instance;
           }
        }

        #endregion

        #region IPictureService Members

        public override void  PostPicture(PicturePostObject postData)
        {
            #region Argument check

            //Check for empty path
            if (string.IsNullOrEmpty(postData.Filename))
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, "Failed to upload picture to yFrog.", ""));
            }

            //Check for empty credentials
            if (string.IsNullOrEmpty(postData.Username) ||
                string.IsNullOrEmpty(postData.Password) )
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, "Failed to upload picture to yFrog.", ""));
            }

            #endregion

            using (System.IO.FileStream file = new FileStream(postData.Filename, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    //Load the picture data
                    byte[] incoming = new byte[file.Length];
                    file.Read(incoming, 0, incoming.Length);

                    if (postData.UseAsync)
                    {
                        workerPPO = (PicturePostObject) postData.Clone();
                        workerPPO.PictureData = incoming;

                        if (workerThread == null)
                        {
                            workerThread = new System.Threading.Thread(new System.Threading.ThreadStart(ProcessUpload));
                            workerThread.Name = "PictureUpload";
                            workerThread.Start();
                        }
                        else
                        {
                            OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.NotReady, "", API_ERROR_NOTREADY));
                        }
                    }
                    else
                    {
                        //use sync.
                        postData.PictureData = incoming;
                        XmlDocument uploadResult = UploadPicture(API_UPLOAD, postData);

                        if (uploadResult.SelectSingleNode("rsp").Attributes["stat"].Value == "fail")
                        {
                            string ErrorText = uploadResult.SelectSingleNode("//err").Attributes["msg"].Value;
                            OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, ErrorText, ""));
                        }
                        else
                        {
                            string URL = uploadResult.SelectSingleNode("//mediaurl").InnerText;
                            OnUploadFinish(new PictureServiceEventArgs(PictureServiceErrorLevel.OK, URL, "", postData.Filename));
                        }
                    }
                }
                catch (Exception e)
                {
                    OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, "", API_ERROR_UPLOAD));
                }
            }
        }

        public override void  FetchPicture(string pictureURL)
        {
            #region Argument check

            //Need a url to read from.
            if (string.IsNullOrEmpty(pictureURL))
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, "", "Failed to download picture from yFrog."));
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
            catch (Exception e)
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, "", "Failed to download picture from yFrog."));
            } 
        }

        public override bool CanFetchUrl(string URL)
        {
            const string siteMarker = "yfrog";
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

                string resultFileName = RetrievePicture(imageID);

                if (string.IsNullOrEmpty(resultFileName))
                {
                    OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, "", "Unable to download picture."));
                }
                else
                {
                    OnDownloadFinish(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, resultFileName, "", pictureURL));
                }
            }
            catch (Exception e)
            {
                //No need to throw, postPicture throws event.
                //OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, "", "Unable to upload picture, try again later."));
            }
            workerThread = null;
        }

        private void ProcessUpload()
        {
            try
            {
                XmlDocument uploadResult = UploadPicture(API_UPLOAD, workerPPO);

                if (uploadResult.SelectSingleNode("rsp").Attributes["stat"].Value == "fail")
                {
                    string ErrorText = uploadResult.SelectSingleNode("//err").Attributes["msg"].Value;
                    OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, ErrorText, ""));
                }
                else
                {
                    string URL = uploadResult.SelectSingleNode("//mediaurl").InnerText;
                    OnUploadFinish(new PictureServiceEventArgs(PictureServiceErrorLevel.OK,URL,"",workerPPO.Filename));
                }
            }
            catch (Exception e)
            {
                //No need to throw, postPicture throws event.
                //OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed,"Unable to upload picture, try again later.",""));
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
        private string RetrievePicture(string imageId)
        {
            //We use the "iphone" optimized images
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(API_SHOW_THUMB + imageId + ".th.jpg");
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

        private XmlDocument UploadPicture(string url, PicturePostObject ppo)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                string boundary = System.Guid.NewGuid().ToString();
                request.Credentials = new NetworkCredential(ppo.Username, ppo.Password);
                request.Headers.Add("Accept-Language", "cs,en-us;q=0.7,en;q=0.3");
                request.PreAuthenticate = true;
                request.ContentType = string.Format("multipart/form-data;boundary={0}", boundary);

                //request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "POST";
                request.Timeout = 20000;
                string header = string.Format("--{0}", boundary);
                string ender = "\r\n" + header + "\r\n";

                StringBuilder contents = new StringBuilder();

                contents.Append(CreateContentPartString(header, "username", ppo.Username));
                contents.Append(CreateContentPartString(header, "password", ppo.Password));
                contents.Append(CreateContentPartString(header, "source", "pocketwit"));

                if (!string.IsNullOrEmpty(ppo.Message))
                {
                    contents.Append(CreateContentPartString(header, "message", ppo.Message));
                }

                contents.Append(CreateContentPartPicture(header));

                //Create the form message to send in bytes
                byte[] message = Encoding.UTF8.GetBytes(contents.ToString());
                byte[] footer = Encoding.UTF8.GetBytes(ender);
                request.ContentLength = message.Length + ppo.PictureData.Length + footer.Length;
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(message, 0, message.Length);
                    requestStream.Write(ppo.PictureData, 0, ppo.PictureData.Length);
                    requestStream.Write(footer, 0, footer.Length);
                    requestStream.Flush();
                    requestStream.Close();

                    using (WebResponse response = request.GetResponse())
                    {
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            XmlDocument responseXML = new XmlDocument();
                            responseXML.LoadXml(reader.ReadToEnd());
                            return responseXML;
                        }
                    }
                }
               
            }
            catch (Exception e)
            {
                //Socket exception 10054 could occur when sending large files.
                //Error is thrown to event in PostPicture
                //OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, "", "Unable to upload picture."));
                return null;
            }
        }

#endregion


        
    }
}