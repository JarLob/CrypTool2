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
using CrypToolStoreLib.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrypToolStoreBuildSystem
{
    public class BuildWorker
    {
        private Logger Logger = Logger.GetLogger();        

        private Source Source
        {
            get;
            set;
        }

        public bool IsRunning { 
            get; 
            private set; 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        public BuildWorker(Source source)
        {
            Source = source;
        }

        /// <summary>
        /// Starts this BuildWorker
        /// </summary>
        public void Start()
        {
            if (IsRunning)
            {
                return;
            }
            IsRunning = true;
            Task buildWorkerTask = new Task(BuildWorkerTaskMethod);
            buildWorkerTask.Start();

        }

        /// <summary>
        /// Stops this BuildWorker
        /// </summary>
        public void Stop()
        {
            IsRunning = false;
        }

        public void BuildWorkerTaskMethod()
        {
            Logger.LogText(String.Format("Started build of source {0}-{1}", Source.PluginId, Source.PluginVersion), this, Logtype.Info);
            try
            {
                // 1) Process creates folder for plugin (e.g. Build\Plugin-1-1, = Plugin-PluginId-SourceId)
                if (!CreateBuildFolder())
                {
                    return;
                }

                // 2) Process creates folder structure in plugin folder
                // --> \plugin           contains source
                // --> \build_output     contains builded plugins
                // --> build_plugin.xml  contains msbuild script
                
                // note: Also makes references to
                // --> signing certificate
                // --> custom build tasks
                // --> ct2 libraries (CrypCore.dll and CrypPluginBase.dll)
                if (!CreateSubFoldersAndFiles())
                {
                    return;
                }               

                // 3) Process downloads zip file and extracts complete content into "plugin" folder
                if (!DownloadZipFile())
                {
                    return;
                }

                // 4) Process searches for exactly one csproj file in the root folder, i.e. "plugin"
                // --> if it finds 0 or more than 1, the build process fails at this point
                if (!SearchCSProjFile())
                {
                    return;
                }

                // 5) Process modifies csproj file
                // --> changes references to CrypPluginBase to correct path (hint: dont forget <private>false</private>)
                // --> changes output folder of "Release" target to "build_output" folder
                if (!ModifyCSProjFile())
                {
                    return;
                }

                // 6) Process modifies msbuild script
                // --> change name of target project to name of csproj file found in "plugin" folder
                if (!ModifyMsBuildScript())
                {
                    return;
                }

                // 7) Process starts "msbuild.exe" (hint: set correct password for signtool to allow it opening signing certificate)
                // --> msbuild compiles the plugin
                // --> signtool is also started and signs the builded assembly file
                if (!BuildPlugin())
                {
                    return;
                }

                // 8) Process checks, if assembly file exists in "build_output" (if not => ERROR)
                if (!CheckBuild())
                {
                    return;
                }

                // 9) Process checks, if a component is located in the assembly, i.e. a class which inherits from IPlugin (if not => ERROR)
                if (!CheckComponentExists())
                {
                    return;
                }

                // 10)  Process zips everything located in "build_output" -- this also includes "de/ru" etc subfolders of the plugin
                // --> zip name is "Assembly-1-1.zip, = Assembly-PluginId-SourceId")
                if (!CreateAssemblyZip())
                {
                    return;
                }

                // 11) Process uploads assembly zip file to CrypToolStore Server, and also updates source data in database
                if (!UploadAssemblyZip())
                {
                    return;
                }                

                Logger.LogText(String.Format("Finished build of source {0}-{1}", Source.PluginId, Source.PluginVersion), this, Logtype.Info);
            }
            catch (Exception ex)
            {
                Logger.LogText(String.Format("Exception occured during build of source {0}-{1}: {2}", Source.PluginId, Source.PluginVersion, ex.Message), this, Logtype.Error);
            }
            finally
            {
                // 12) Process cleans up by deleting build folder (also in case of an error)
                CleanUp();
            }            
            IsRunning = false;
        }

        private bool CreateBuildFolder()
        {
            return true;
        }

        private bool CreateSubFoldersAndFiles()
        {
            return true;
        }

        private bool DownloadZipFile()
        {
            return true;
        }

        private bool SearchCSProjFile()
        {
            return true;
        }

        private bool ModifyCSProjFile()
        {
            return true;
        }

        private bool ModifyMsBuildScript()
        {
            return true;
        }

        private bool BuildPlugin()
        {
            return true;
        }

        private bool CheckBuild()
        {
            return true;
        }

        private bool CheckComponentExists()
        {
            return true;
        }

        private bool CreateAssemblyZip()
        {
            return true;
        }

        private bool UploadAssemblyZip()
        {
            return true;
        }

        private void CleanUp()
        {
            return true;
        }

       
    }
}
