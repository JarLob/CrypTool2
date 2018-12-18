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
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Cryptool.Core;
using Cryptool.PluginBase.Attributes;
using Cryptool.PluginBase.Miscellaneous;
using CrypToolStoreLib.Client;

namespace Cryptool.CrypToolStore
{
    /// <summary>
    /// This class allows to locate and download resources from CrypToolStore
    /// defined by resourceId and resourceVersion
    /// </summary>
    public class ResourceHelper
    {
        private static readonly object LockObject = new Object();

        /// <summary>
        /// Get the path to a resource's folder path if the resource exists.
        /// If it does not exist, it prompts the user to download it
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="resourceVersion"></param>
        /// <returns>the path if it exists; otherwise returns null</returns>
        public static string GetResourceFolderPath(int resourceId, int resourceVersion)
        {
            lock (LockObject)
            {
                string resourcesFolder = GetResourcesFolder();
                //we create the resources folder if it does not exist
                if (!Directory.Exists(resourcesFolder))
                {
                    Directory.CreateDirectory(resourcesFolder);
                }

                //now, we check, if the requested resource folder exists
                resourcesFolder = Path.Combine(resourcesFolder, String.Format("resource-{0}-{1}", resourceId, resourceVersion));
                if (Directory.Exists(resourcesFolder))
                {
                    return resourcesFolder;
                }

                //the resources folder does not exists; thus, we download the resource from CrypToolStoreServer
                return DownloadResource(resourceId, resourceVersion);
            }
        }

        /// <summary>
        /// This method shows a download dialog for downloading the resource to the resource folder
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="resourceVersion"></param>
        /// <returns></returns>
        private static string DownloadResource(int resourceId, int resourceVersion)
        {
            //check CrypToolStore if resource exists
           


            return null;
        }

        /// <summary>
        /// Returns the absolute path to the resources folder
        /// </summary>
        /// <returns></returns>
        internal static string GetResourcesFolder()
        {
            //Translate the Ct2BuildType to a folder name for CrypToolStore plugins                
            string crypToolStoreSubFolder = "";
            switch (AssemblyHelper.BuildType)
            {
                case Ct2BuildType.Developer:
                    crypToolStoreSubFolder = "Developer";
                    break;
                case Ct2BuildType.Nightly:
                    crypToolStoreSubFolder = "Nightly";
                    break;
                case Ct2BuildType.Beta:
                    crypToolStoreSubFolder = "Beta";
                    break;
                case Ct2BuildType.Stable:
                    crypToolStoreSubFolder = "Release";
                    break;
                default: //if no known version is given, we assume developer
                    crypToolStoreSubFolder = "Developer";
                    break;
            }
            string crypToolStorePluginFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), PluginManager.CrypToolStorePluginDirectory);
            crypToolStorePluginFolder = System.IO.Path.Combine(crypToolStorePluginFolder, crypToolStoreSubFolder);
            crypToolStorePluginFolder = System.IO.Path.Combine(crypToolStorePluginFolder, "resources");
            return crypToolStorePluginFolder;
        }
    }
}
