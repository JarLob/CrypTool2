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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Cryptool.Plugins.DECODEDatabaseTools.DataObjects
{
    /*
     *  classes generated with http://json2csharp.com/
     */

    #region DataRecords - List

    [DataContract]
    public class Records
    {
        [DataMember]
        public List<RecordsRecord> records { get; set; }
    }

    [DataContract]
    public class RecordsRecord
    {
        [DataMember]
        public int record_id { get; set; }

        [DataMember]
        public string name { get; set; }
    }

    #endregion

    #region Record - single item

    [DataContract]
    public class Content
    {
        [DataMember]
        public string type { get; set; }
        [DataMember]
        public string cipher_type { get; set; }
        [DataMember]
        public string symbol_set { get; set; }
        [DataMember]
        public int number_of_pages { get; set; }
        [DataMember]
        public string inline_plaintext { get; set; }
        [DataMember]
        public string inline_cleartext { get; set; }
        [DataMember]
        public string cleartext_language { get; set; }
        [DataMember]
        public string plaintext_language { get; set; }
    }

    [DataContract]
    public class Origin
    {
        [DataMember]
        public string author { get; set; }
        [DataMember]
        public string sender { get; set; }
        [DataMember]
        public string receiver { get; set; }
        [DataMember]
        public string dating { get; set; }
        [DataMember]
        public string region { get; set; }
        [DataMember]
        public string city { get; set; }
    }

    [DataContract]
    public class Format
    {
        [DataMember]
        public string paper { get; set; }
        [DataMember]
        public string ink_type { get; set; }
    }

    [DataContract]
    public class Metadata
    {
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public Content content { get; set; }
        [DataMember]
        public Origin origin { get; set; }
        [DataMember]
        public Format format { get; set; }
        [DataMember]
        public string additional_information { get; set; }
    }

    [DataContract]
    public class Settings
    {
        [DataMember]
        public bool public_images { get; set; }
    }

    [DataContract]
    public class Image
    {
        [DataMember]
        public int image_id { get; set; }
        [DataMember]
        public string full_url { get; set; }
        [DataMember]
        public string thumbnail_url { get; set; }
    }

    [DataContract]
    public class DecipheredText
    {
        [DataMember]
        public int document_id { get; set; }
        [DataMember]
        public string title { get; set; }
        [DataMember]
        public string upload_date { get; set; }
        [DataMember]
        public string size { get; set; }
        [DataMember]
        public string file_type { get; set; }
        [DataMember]
        public string download_url { get; set; }
    }

    [DataContract]
    public class Transcription
    {
        [DataMember]
        public int document_id { get; set; }
        [DataMember]
        public string title { get; set; }
        [DataMember]
        public string upload_date { get; set; }
        [DataMember]
        public string size { get; set; }
        [DataMember]
        public string file_type { get; set; }
        [DataMember]
        public string download_url { get; set; }
    }

    [DataContract]
    public class Translation
    {
        [DataMember]
        public int document_id { get; set; }
        [DataMember]
        public string title { get; set; }
        [DataMember]
        public string upload_date { get; set; }
        [DataMember]
        public string size { get; set; }
        [DataMember]
        public string file_type { get; set; }
        [DataMember]
        public string download_url { get; set; }
    }

    [DataContract]
    public class Documents
    {
        [DataMember]
        public List<DecipheredText> deciphered_text { get; set; }
        [DataMember]
        public List<object> cleartext { get; set; }
        [DataMember]
        public List<object> cryptanalysis_statistics { get; set; }
        [DataMember]
        public List<object> miscellaneous { get; set; }
        [DataMember]
        public List<object> publication { get; set; }
        [DataMember]
        public List<Transcription> transcription { get; set; }
        [DataMember]
        public List<Translation> translation { get; set; }
    }

    [DataContract]
    public class Record
    {
        [DataMember]
        public int record_id { get; set; }
        [DataMember]
        public Metadata metadata { get; set; }
        [DataMember]
        public Settings settings { get; set; }
        [DataMember]
        public List<Image> images { get; set; }
        [DataMember]
        public Documents documents { get; set; }
    }

    #endregion
}
