using Cryptool.PluginBase.Miscellaneous;
/*
   Copyright 2018 Nils Kopal <Nils.Kopal<at>CrypTool.org

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using Cryptool.Plugins.DECODEDatabaseTools.DataObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Cryptool.Plugins.DECODEDatabaseTools
{
    public class JsonDownloaderAndConverter : IDisposable
    {
        public const string DOWNLOAD_URL = "https://stp.lingfil.uu.se/decode/database/records";

        public const string GETRECORDLIST = "GetRecordsList";
        public const string GETRECORDSTRING = "GetRecordString";
        public const string GETDATA = "GetData";
        public const string USER_AGENT = "CrypTool 2";

        public event DownloadDataCompletedEventHandler DownloadDataCompleted;
        public event DownloadStringCompletedEventHandler DownloadStringCompleted;
        public event DownloadProgressChangedEventHandler DownloadProgressChanged;

        private MyWebClient WebClient = new MyWebClient();
        private bool isDownloading = false;

        /// <summary>
        /// Inherited WebClient to change timeout
        /// </summary>
        private class MyWebClient : WebClient
        {
            private const int TIMEOUT = 5000;

            protected override WebRequest GetWebRequest(Uri uri)
            {
                WebRequest w = base.GetWebRequest(uri);
                w.Timeout = TIMEOUT;
                return w;
            }
        }

        /// <summary>
        /// Creates a new JsonDownloaderAndConverter
        /// </summary>
        public JsonDownloaderAndConverter()
        {
            WebClient.DownloadDataCompleted += WebClient_DownloadDataCompleted;
            WebClient.DownloadStringCompleted += WebClient_DownloadStringCompleted;
            WebClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
        }

        /// <summary>
        /// Called when the download progress of the webclient changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (DownloadProgressChanged != null)
            {
                DownloadProgressChanged.Invoke(this, e);
            }
        }

        /// <summary>
        /// Called when the download of the webclient is completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WebClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            lock (this)
            {              
                isDownloading = false;
            }
            if (DownloadStringCompleted != null)
            {
                DownloadStringCompleted.Invoke(this, e);
            }
        }

        /// <summary>
        /// Called when the download of the webclient is completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WebClient_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            lock (this)
            {
                isDownloading = false;
            }
            if (DownloadDataCompleted != null)
            {
                DownloadDataCompleted.Invoke(this, e);
            }
        }

        /// <summary>
        /// Get the list of records of the DECODE database using the json protocol
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public void GetRecordsList(string url)
        {
            lock (this)
            {
                if (isDownloading)
                {
                    return;
                }
                isDownloading = true;
            }
            try
            {
                WebClient.Headers.Add("Accept", "application/json");
                WebClient.Headers.Add("Accept", "text/plain");
                WebClient.Headers.Add("user-agent", USER_AGENT + ";" + AssemblyHelper.InstallationType.ToString() + ";" + AssemblyHelper.Version);
                WebClient.DownloadDataAsync(new Uri(url), GETRECORDLIST);
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Error while downloading records data from DECODE database: {0}", ex.Message), ex);
            }                
            
        }

        /// <summary>
        /// Get a single record as string from the DECODE database using the json protocol
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public void GetRecordString(RecordsRecord record)
        {
            lock (this)
            {
                if (isDownloading)
                {
                    return;
                }
                isDownloading = true;
            }
            try
            {
                WebClient.Headers.Add("Accept", "application/json");
                WebClient.Headers.Add("Accept", "text/plain");
                WebClient.Headers.Add("user-agent", USER_AGENT + ";" + AssemblyHelper.InstallationType.ToString() + ";" + AssemblyHelper.Version);
                string url = DOWNLOAD_URL + "/" + record.record_id;
                WebClient.DownloadStringAsync(new Uri(url), "GetRecordString");
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Error while downloading record data from DECODE database: {0}", ex.Message), ex);
            }
        }

        /// <summary>
        /// Get a record object from a string containing Record json data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Record GetRecordFromString(string data)
        {
            try
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Record));
                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
                {
                    stream.Position = 0;
                    Record record = (Record)serializer.ReadObject(stream);
                    return record;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Could not deserialize json data: {0}", ex.Message), ex);
            }
        }

        /// <summary>
        /// Downloads data from the specified URL and returns it as byte array
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public void GetData(string url)
        {
            lock (this)
            {
                if (isDownloading)
                {
                    return;
                }
                isDownloading = true;
            }
            try
            {
                WebClient.DownloadDataAsync(new Uri(url), GETDATA);
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Error while downloading data from {0}: {1}", url, ex.Message), ex);
            }
        }

        /// <summary>
        /// Disposes the internal webclient
        /// </summary>
        public void Dispose()
        {
            WebClient.Dispose();
        }
    }
}
