/*
   Copyright 2019 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Cryptool.Plugins.DECODEDatabaseTools
{
    public class JsonDownloaderAndConverter
    {
        private const string LoginUrl = "https://cl.lingfil.uu.se/decode/database/api/login";
        private const string DownloadRecordsUrl = "https://cl.lingfil.uu.se/decode/database/api/records";
        private const string DownloadRecordUrl = "https://cl.lingfil.uu.se/decode/database/api/records/{0}";

        private const string UserAgent = "CrypTool 2/DECODE JsonDownloaderAndConverter";
        private static CookieContainer _cookieContainer = new CookieContainer();

        /// <summary>
        /// Login into DECODE database using username and password,
        /// also creates a new static CookieContainer, which it uses for storing and using the cookie
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool Login(string username, string password)
        {
            try
            {
                _cookieContainer = new CookieContainer();
                var handler = new HttpClientHandler { CookieContainer = _cookieContainer };
                using (var client = new HttpClient(handler))
                {
                    var usernamePasswordJson = new StringContent(String.Format("{{\"username\": \"{0}\", \"password\": \"{1}\"}}", username, password));
                    client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var response = client.PostAsync(LoginUrl, usernamePasswordJson).Result;
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            return true;
                        case HttpStatusCode.Forbidden:
                            return false;
                        default:
                            throw new Exception(String.Format("Error: Status code was {0}", response.StatusCode));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Error while loggin into DECODE database: {0}", ex.Message), ex);
            }
        }

        /// <summary>
        /// Get the list of records of the DECODE database using the json protocol
        /// </summary>
        public static string GetRecords()
        {
            try
            {
                if (IsLoggedIn() == false)
                {
                    throw new Exception("Not logged in!");
                }

                var handler = new HttpClientHandler { CookieContainer = _cookieContainer };
                using (var client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = client.GetAsync(DownloadRecordsUrl).Result;

                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            return response.Content.ReadAsStringAsync().Result;
                        default:
                            throw new Exception(String.Format("Error: Status code was {0}", response.StatusCode));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Error while downloading records from DECODE database: {0}", ex.Message), ex);
            }
        }

        /// <summary>
        /// Get a single record as string from the DECODE database using the json protocol and http
        /// </summary>
        public static string GetRecord(int id)
        {
            try
            {
                if (IsLoggedIn() == false)
                {
                    throw new Exception("Not logged in!");
                }

                var handler = new HttpClientHandler { CookieContainer = _cookieContainer };
                using (var client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = client.GetAsync(String.Format(DownloadRecordUrl, id)).Result;

                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            return response.Content.ReadAsStringAsync().Result;
                        default:
                            throw new Exception(String.Format("Error: Status code was {0}", response.StatusCode));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Error while downloading record from DECODE database: {0}", ex.Message), ex);
            }
        }

        /// <summary>
        /// Get a records object from a string containing Record json data
        /// </summary>
        /// <param name="data"></param>
        public static Records ConvertStringToRecords(string data)
        {
            try
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Records));
                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
                {
                    stream.Position = 0;
                    Records records = (Records)serializer.ReadObject(stream);
                    return records;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Could not deserialize json data: {0}", ex.Message), ex);
            }
        }

        /// <summary>
        /// Get a record object from a string containing Record json data
        /// </summary>
        /// <param name="data"></param>
        public static Record ConvertStringToRecord(string data)
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
        public static byte[] GetData(string url)
        {
            try
            {
                if (IsLoggedIn() == false)
                {
                    throw new Exception("Not logged in!");
                }
                var handler = new HttpClientHandler { CookieContainer = _cookieContainer };
                using (var client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = client.GetAsync(url).Result;

                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            return response.Content.ReadAsByteArrayAsync().Result;
                        default:
                            throw new Exception(String.Format("Error: Status code was {0}", response.StatusCode));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Error while downloading data from {0}: {1}", url, ex.Message), ex);
            }
        }

        /// <summary>
        /// Checks, if there is a valid cookie
        /// </summary>
        /// <returns></returns>
        public static bool IsLoggedIn()
        {
            return _cookieContainer.Count == 1;
        }

        /// <summary>
        /// Removes the cookie to log out
        /// </summary>
        public static void LogOut()
        {
            _cookieContainer = new CookieContainer();
        }
    }
}
