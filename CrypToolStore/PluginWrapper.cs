/*
   Copyright 2018 Nils Kopal <Nils.Kopal<AT>Uni-Kassel.de>

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
using CrypToolStoreLib.DataObjects;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Cryptool.CrypToolStore
{
    /// <summary>
    /// This class wraps a PluginAndData object, offering comfort functions, thus, it can be easier shown in the UI
    /// </summary>
    public class PluginWrapper
    {
        public int PluginId { get; set; }
        public int PluginVersion { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }
        public string Authornames { get; set; }
        public string Authoremails { get; set; }
        public string Authorinstitutes { get; set; }

        public bool IsInstalled { get; set; }

        private byte[] iconData { get; set; }

        public PublishState PublishState { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pluginAndSource"></param>
        public PluginWrapper(PluginAndSource pluginAndSource)
        {
            Plugin plugin = pluginAndSource.Plugin;
            Source source = pluginAndSource.Source;

            PluginId = plugin.Id;
            PluginVersion = source.PluginVersion;
            Name = plugin.Name;
            ShortDescription = plugin.ShortDescription;
            LongDescription = plugin.LongDescription;
            Authornames = plugin.Authornames;
            Authoremails = plugin.Authoremails;
            Authorinstitutes = plugin.Authorinstitutes;
            iconData = plugin.Icon;
        }

        /// <summary>
        /// Get icon of the plugin as BitmapFrame for displaying it in the UI
        /// </summary>
        /// <returns></returns>
        public BitmapFrame Icon
        {
            get
            {
                byte[] data;
                if (iconData == null || iconData.Length == 0)
                {
                    //we have no icon, thus, we display the default icon
                    MemoryStream stream = new MemoryStream();
                    Properties.Resources._default.Save(stream, ImageFormat.Png);
                    stream.Position = 0;
                    data = stream.ToArray();
                    stream.Close();
                }
                else
                {
                    data = iconData;
                }
                BitmapDecoder decoder = PngBitmapDecoder.Create(new MemoryStream(data),
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
        /// Get installation icon of the plugin as BitmapFrame for displaying it in the UI
        /// </summary>
        /// <returns></returns>
        public BitmapFrame InstallationIcon
        {
            get
            {
                byte[] data;
                MemoryStream stream = new MemoryStream();
                if (IsInstalled)
                {
                    Properties.Resources.downloaded.Save(stream, ImageFormat.Png);
                    
                }
                else
                {

                    Properties.Resources.download.Save(stream, ImageFormat.Png);
                }
                stream.Position = 0;
                data = stream.ToArray();
                stream.Close();
                BitmapDecoder decoder = PngBitmapDecoder.Create(new MemoryStream(data),
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
    }
}
