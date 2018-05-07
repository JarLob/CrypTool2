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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

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
    public class Image : INotifyPropertyChanged
    {
        private JsonDownloaderAndConverter fullImageDownloader;
        private JsonDownloaderAndConverter thumbnailDownloader;

        public event PropertyChangedEventHandler PropertyChanged;
        public event DownloadDataCompletedEventHandler DownloadDataCompleted;
        public event DownloadProgressChangedEventHandler DownloadProgressChanged;


        [DataMember]
        public int image_id { get; set; }
        [DataMember]
        public string full_url { get; set; }
        [DataMember]
        public string thumbnail_url { get; set; }

        private byte[] thumbnail_data;


        public BitmapFrame GetFullImage
        {
            get
            {
                return null;
            }
        }

        public BitmapFrame GetThumbnail
        {
            get
            {
                if (thumbnailDownloader == null)
                {
                    thumbnailDownloader = new JsonDownloaderAndConverter();
                    thumbnailDownloader.DownloadDataCompleted += thumbnailDownloader_DownloadDataCompleted;
                    thumbnailDownloader.GetData(thumbnail_url);
                    return null;
                }
                if (thumbnail_data == null)
                {
                    return null;
                }
                var decoder = BitmapDecoder.Create(new MemoryStream(thumbnail_data),
                              BitmapCreateOptions.PreservePixelFormat,
                              BitmapCacheOption.None);
                if (decoder.Frames.Count > 0)
                {
                    return decoder.Frames[0];
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Invoked when thumbnail download finished
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void thumbnailDownloader_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs args)
        {
            if (args.Error == null)
            {
                thumbnail_data = args.Result;
            }
            OnPropertyChanged("GetThumbnail");
        }

      
        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        /// <summary>
        /// download the full image
        /// </summary>
        internal void DownloadImage()
        {
            if (fullImageDownloader == null)
            {
                fullImageDownloader = new JsonDownloaderAndConverter();
                fullImageDownloader.DownloadDataCompleted += fullImageDownloader_DownloadDataCompleted;
                fullImageDownloader.DownloadProgressChanged += fullImageDownloader_DownloadProgressChanged;
                fullImageDownloader.GetData(full_url);
            }          
        }

        /// <summary>
        /// Called, when the download progress changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void fullImageDownloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs args)
        {
            if (DownloadProgressChanged != null)
            {
                DownloadProgressChanged.Invoke(this, args);
            }
        }

        /// <summary>
        /// called when download is finished
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void fullImageDownloader_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs args)
        {
            try
            {
                if (DownloadDataCompleted != null)
                {
                    DownloadDataCompleted.Invoke(this, args);
                }
                fullImageDownloader.DownloadDataCompleted -= fullImageDownloader_DownloadDataCompleted;
                fullImageDownloader.DownloadProgressChanged -= fullImageDownloader_DownloadProgressChanged;
                fullImageDownloader.Dispose();
                fullImageDownloader = null;
            }
            catch (Exception ex)
            {
                //wtf?
            }
        }
    }

    [DataContract]
    public class Document
    {
        private JsonDownloaderAndConverter jsonDownloaderAndConverter;

        public event DownloadDataCompletedEventHandler DownloadDataCompleted;
        public event DownloadProgressChangedEventHandler DownloadProgressChanged;

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


        /// <summary>
        /// Tries to download the document; if it fails, it returns null
        /// </summary>
        public void DownloadDocument()
        {                   
            if (jsonDownloaderAndConverter == null)
            {
                jsonDownloaderAndConverter = new JsonDownloaderAndConverter();
                jsonDownloaderAndConverter.DownloadDataCompleted += jsonDownloaderAndConverter_DownloadDataCompleted;
                jsonDownloaderAndConverter.DownloadProgressChanged += jsonDownloaderAndConverter_DownloadProgressChanged;
                jsonDownloaderAndConverter.GetData(download_url);
            }          
        }

        /// <summary>
        /// Called when download progress changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void jsonDownloaderAndConverter_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs args)
        {
            if (DownloadProgressChanged != null)
            {
                DownloadProgressChanged.Invoke(this, args);
            }
        }

        /// <summary>
        /// Called, when download is finished
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void jsonDownloaderAndConverter_DownloadDataCompleted(object sender, System.Net.DownloadDataCompletedEventArgs args)
        {
            try
            {
                if (DownloadDataCompleted != null)
                {
                    DownloadDataCompleted.Invoke(this, args);
                }
                jsonDownloaderAndConverter.DownloadDataCompleted -= jsonDownloaderAndConverter_DownloadDataCompleted;
                jsonDownloaderAndConverter.DownloadProgressChanged -= jsonDownloaderAndConverter_DownloadProgressChanged;
                jsonDownloaderAndConverter.Dispose();
                jsonDownloaderAndConverter = null;
            }
            catch (Exception ex)
            {
                //wtf?
            }
        }
    }


    [DataContract]
    public class Documents
    {
        [DataMember]
        public List<Document> deciphered_text { get; set; }
        [DataMember]
        public List<Document> cleartext { get; set; }
        [DataMember]
        public List<Document> cryptanalysis_statistics { get; set; }
        [DataMember]
        public List<Document> publication { get; set; }
        [DataMember]
        public List<Document> transcription { get; set; }
        [DataMember]
        public List<Document> translation { get; set; }
        [DataMember]
        public List<Document> miscellaneous { get; set; }

        /// <summary>
        /// All documents merged in one list
        /// </summary>
        public List<Document> AllDocuments
        {
            get
            {
                List<Document> documents = new List<Document>();
                documents.AddRange(deciphered_text);
                documents.AddRange(cleartext);
                documents.AddRange(cryptanalysis_statistics);
                documents.AddRange(publication);
                documents.AddRange(transcription);
                documents.AddRange(transcription);
                documents.AddRange(translation);
                documents.AddRange(miscellaneous);
                return documents;
            }
        }
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
