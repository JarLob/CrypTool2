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
        public const string DOWNLOAD_RECORDS_URL = "http://localhost/DECODE/records.json";
        public const string DOWNLOAD_RECORD_URL = "http://localhost/DECODE/record.json";

        /// <summary>
        /// Get the list of records of the DECODE database using the json protocol
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static List<RecordsRecord> GetRecordsList(string url)
        {
            WebClient client = new WebClient();
            byte[] data = client.DownloadData(url);
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Records));
            MemoryStream stream = new MemoryStream(data);
            stream.Position = 0;
            Records records = (Records)serializer.ReadObject(stream);
            return records.records;
        }

        /// <summary>
        /// Get a single record as string from the DECODE database using the json protocol
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public static string GetRecordString(RecordsRecord record)
        {
            WebClient client = new WebClient();
            string url = DOWNLOAD_RECORD_URL + "?record_id=" + record.record_id;
            byte[] data = client.DownloadData(url);           
            return UTF8Encoding.UTF8.GetString(data);
        }

        /// <summary>
        /// Get a record object from a string containing Record json data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Record GetRecordFromString(string data)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Record));
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
            stream.Position = 0;
            Record record = (Record)serializer.ReadObject(stream);
            return record;
        }
    }
}
