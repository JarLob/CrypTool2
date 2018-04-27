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
    public class JsonDownloaderAndConverter
    {
        public const string DOWNLOAD_RECORDS_URL = "https://stp.lingfil.uu.se/decodedev/records";
        public const string DOWNLOAD_RECORD_URL = "https://stp.lingfil.uu.se/decodedev/records/";

        /// <summary>
        /// Hack to override timeout
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
        /// Get the list of records of the DECODE database using the json protocol
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static List<RecordsRecord> GetRecordsList(string url)
        {
            using (MyWebClient client = new MyWebClient())
            {
                client.Headers.Add("Accept", "application/json");
                client.Headers.Add("Accept", "text/plain");
                
                byte[] data;
                try
                {
                    data = client.DownloadData(url);
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format("Could not download records data from DECODE database: {0}", ex.Message), ex);
                }
                //dirty hack
                string json = "{\"records\":{" + UTF8Encoding.UTF8.GetString(data) + "}";
                data = UTF8Encoding.UTF8.GetBytes(json);
                //end of dirty hack
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Records));
                using (MemoryStream stream = new MemoryStream(data))
                {
                    stream.Position = 0;
                    try
                    {
                        Records records = (Records)serializer.ReadObject(stream);
                        return records.records;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(String.Format("Could not deserialize json data received from DECODE database: {0}", ex.Message), ex);
                    }
                }
            }
        }

        /// <summary>
        /// Get a single record as string from the DECODE database using the json protocol
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public static string GetRecordString(RecordsRecord record)
        {
            try
            {
                using (MyWebClient client = new MyWebClient())
                {
                    client.Headers.Add("Accept", "application/json");
                    client.Headers.Add("Accept", "text/plain");
                    string url = DOWNLOAD_RECORD_URL + "/" + record.id;
                    byte[] data = client.DownloadData(url);
                    return UTF8Encoding.UTF8.GetString(data);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Could not download record data from DECODE database: {0}", ex.Message), ex);
            }
        }

        /// <summary>
        /// Get a record object from a string containing Record json data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Record GetRecordFromString(string data)
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
        public static byte[] GetData(string url)
        {
            try
            {
                using (MyWebClient client = new MyWebClient())
                {
                    return client.DownloadData(url);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Could not download data from {0}: {1}", url, ex.Message), ex);
            }
        }
    }
}
