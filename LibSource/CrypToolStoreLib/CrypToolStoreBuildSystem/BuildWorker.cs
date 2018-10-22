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
using CrypToolStoreLib.Client;
using CrypToolStoreLib.DataObjects;
using CrypToolStoreLib.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrypToolStoreBuildSystem
{
    /// <summary>
    /// A build worker builds sources and uploads the created assembly to the CrypToolStoreServer
    /// </summary>
    public class BuildWorker
    {
        private const string BUILD_FOLDER = "build";
        private const string SOURCE_FILE_NAME = "Source";

        private BuildLogger Logger = new BuildLogger();

        /// <summary>
        /// Reference to source to build
        /// </summary>
        private Source Source
        {
            get;
            set;
        }

        /// <summary>
        /// Is the build process currently running?
        /// </summary>
        public bool IsRunning
        {
            get;
            private set;
        }

        /// <summary>
        /// Name of the csproj file of the plugin
        /// </summary>
        private string CSProjFileName
        {
            get;
            set;
        }

        /// <summary>
        /// Constructor
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
                // 0) Set source to building state
                SetToBuildingState();

                // 1) Process creates folder for plugin (e.g. Build\Plugin-1-1, = Plugin-PluginId-SourceId)
                if (!CreateBuildFolder())
                {
                    return;
                }

                //check, if stop has been called
                if (!IsRunning)
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
                if (!CreateBuildSubFoldersAndFiles())
                {
                    return;
                }

                //check, if stop has been called
                if (!IsRunning)
                {
                    return;
                }

                // 3) Process downloads zip file
                if (!DownloadZipFile())
                {
                    return;
                }

                //check, if stop has been called
                if (!IsRunning)
                {
                    return;
                }

                // 4) Process extracts zip file
                if (!ExtractZipFile())
                {
                    return;
                }

                //check, if stop has been called
                if (!IsRunning)
                {
                    return;
                }

                // 5) Process searches for exactly one csproj file in the root folder, i.e. "plugin"
                // --> if it finds 0 or more than 1, the build process fails at this point
                if (!SearchCSProjFile())
                {
                    return;
                }

                //check, if stop has been called
                if (!IsRunning)
                {
                    return;
                }

                // 6) Process modifies csproj file
                // --> changes references to CrypPluginBase to correct path (hint: dont forget <private>false</private>)
                // --> changes output folder of "Release" target to "build_output" folder
                if (!ModifyCSProjFile())
                {
                    return;
                }

                //check, if stop has been called
                if (!IsRunning)
                {
                    return;
                }

                // 7) Process modifies msbuild script
                // --> change name of target project to name of csproj file found in "plugin" folder
                if (!ModifyMsBuildScript())
                {
                    return;
                }

                //check, if stop has been called
                if (!IsRunning)
                {
                    return;
                }

                // 8) Process starts "msbuild.exe" (hint: set correct password for signtool to allow it opening signing certificate)
                // --> msbuild compiles the plugin
                // --> signtool is also started and signs the builded assembly file
                if (!BuildPlugin())
                {
                    return;
                }

                //check, if stop has been called
                if (!IsRunning)
                {
                    return;
                }

                // 9) Process checks, if assembly file exists in "build_output" (if not => ERROR)
                if (!CheckBuild())
                {
                    return;
                }

                //check, if stop has been called
                if (!IsRunning)
                {
                    return;
                }

                // 10) Process checks, if a component is located in the assembly, i.e. a class which inherits from IPlugin (if not => ERROR)
                if (!CheckComponentExists())
                {
                    return;
                }

                //check, if stop has been called
                if (!IsRunning)
                {
                    return;
                }

                // 11)  Process zips everything located in "build_output" -- this also includes "de/ru" etc subfolders of the plugin
                // --> zip name is "Assembly-1-1.zip, = Assembly-PluginId-SourceId")
                if (!CreateAssemblyZip())
                {
                    return;
                }

                //check, if stop has been called
                if (!IsRunning)
                {
                    return;
                }

                // 12) Process uploads assembly zip file to CrypToolStore Server, and also updates source data in database
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
                // 13) Process cleans up by deleting build folder (also in case of an error)
                try
                {
                    //TODO: remove comment, thus, at the end everything is deleted
                    //CleanUp();
                }
                catch (Exception ex)
                {
                    Logger.LogText(String.Format("Exception occured during cleanup of source {0}-{1}: {2}", Source.PluginId, Source.PluginVersion, ex.Message), this, Logtype.Error);
                }

                IsRunning = false;
            }
        }

        /// <summary>
        ///  0) Set source to building state
        /// </summary>
        private bool SetToBuildingState()
        {
            Logger.LogText(String.Format("Set source {0}-{1} to state: {2}", Source.PluginId, Source.PluginVersion, BuildState.BUILDING.ToString()), this, Logtype.Info);

            CrypToolStoreClient client = new CrypToolStoreClient();
            client.ServerAddress = Constants.ServerAddress;
            client.ServerPort = Constants.ServerPort;
            client.Connect();
            client.Login(Constants.Username, Constants.Password);

            try
            {
                //get source for update
                DataModificationOrRequestResult result = client.GetSource(Source.PluginId, Source.PluginVersion);
                if (!result.Success)
                {
                    Logger.LogText(String.Format("Could not get source-{0}-{1}: {2}", Source.PluginId, Source.PluginVersion, result.Message), this, Logtype.Error);
                    client.Disconnect();
                    return false;
                }
                Source source = (Source)result.DataObject;
                //update that source to building state
                source.BuildState = BuildState.BUILDING.ToString();
                source.BuildLog = String.Format("Buildserver started build process at {0}", DateTime.Now);
                result = client.UpdateSource(source);
                if (!result.Success)
                {
                    Logger.LogText(String.Format("Could not set source-{0}-{1} to state {2}: {3}", Source.PluginId, Source.PluginVersion, BuildState.BUILDING, result.Message), this, Logtype.Error);
                    return false;
                }

                Logger.LogText(String.Format("Source-{0}-{1} is now in state: {2}", Source.PluginId, Source.PluginVersion, BuildState.BUILDING.ToString()), this, Logtype.Info);
                return true;
            }
            finally
            {
                client.Disconnect();
            }
        }

        /// <summary>
        /// 1) Checks, if the BUILD_FOLDER exists, if not it creats it
        /// Also creates SOURCE_FILE_NAME-PluginId-PluginVersion folder for the actual build
        /// </summary>
        /// <returns></returns>
        private bool CreateBuildFolder()
        {
            lock (BUILD_FOLDER)
            {
                if (!Directory.Exists(BUILD_FOLDER))
                {
                    Directory.CreateDirectory(BUILD_FOLDER);
                    Logger.LogText(String.Format("Created build folder: {0}", BUILD_FOLDER), this, Logtype.Info);                    
                }
            }

            string buildfoldername = BUILD_FOLDER + @"\" + SOURCE_FILE_NAME + "-" + Source.PluginId + "-" + Source.PluginVersion;

            if (!Directory.Exists(buildfoldername))
            {                
                Directory.CreateDirectory(buildfoldername);
                Logger.LogText(String.Format("Created build folder for source {0}-{1}: {2}", Source.PluginId, Source.PluginVersion, buildfoldername), this, Logtype.Info);
                return true;
            }
            else
            {
                Logger.LogText(String.Format("Folder for source {0}-{1} already exists. Maybe because of faulty previous build. Abort now", Source.PluginId, Source.PluginVersion), this, Logtype.Error);
                return false;
            }
        }

        /// <summary>
        ///  2) Process creates folder structure in plugin folder
        ///  --> \plugin          contains source
        ///  --> \build_output     contains builded plugins
        ///  --> build_plugin.xml  contains msbuild script
        ///  
        ///      note: Also makes references to
        ///  --> signing certificate
        ///  --> custom build tasks
        ///  --> ct2 libraries (CrypCore.dll and CrypPluginBase.dll)
        /// </summary>
        /// <returns></returns>
        private bool CreateBuildSubFoldersAndFiles()
        {
            string buildfoldername = BUILD_FOLDER + @"\" + SOURCE_FILE_NAME + "-" + Source.PluginId + "-" + Source.PluginVersion;

            //1. create plugin folder
            Directory.CreateDirectory(buildfoldername + @"\plugin");
            Logger.LogText(String.Format("Created plugin folder for source {0}-{1}",Source.PluginId, Source.PluginVersion),this,Logtype.Info);

            //2. create build_output folder
            Directory.CreateDirectory(buildfoldername + @"\build_output");
            Logger.LogText(String.Format("Created build_output folder for source {0}-{1}", Source.PluginId, Source.PluginVersion), this, Logtype.Info);

            //3. create build_plugin.xml
            using (Stream stream = new FileStream(buildfoldername + @"\build_plugin.xml", FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    //todo: variable anteile setzen...
                    writer.WriteLine("<Project DefaultTargets=\"BuildAndSign\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");
                    writer.WriteLine("  <Import Project=\"..\\CustomBuildTasks\\CustomBuildTasks.Targets\"/>");
                    writer.WriteLine("  <PropertyGroup>");
                    writer.WriteLine("    <QM>\"</QM>");
                    writer.WriteLine("  </PropertyGroup>");
                    writer.WriteLine("  <Target Name=\"BuildCrypPlugin\">");
                    writer.WriteLine("    <MSBuild Projects=\"$(ProjectName)\" Targets=\"Build\" />");
                    writer.WriteLine("  </Target>");
                    writer.WriteLine("  <Target Name=\"SignCrypPlugin\">");
                    writer.WriteLine("    <ItemGroup>");
                    writer.WriteLine("      <SignFiles Include=\"build_output\\Release\\*.dll\"/>");
                    writer.WriteLine("    </ItemGroup>");
                    writer.WriteLine("    <SilentExec Command=\"signtool.exe\" Arguments=\"sign /f $(CertificatePfxFile) /p $(CertificatePassword) /t http://timestamp.verisign.com/scripts/timstamp.dll $(QM)%(SignFiles.Identity)$(QM)\" />");
                    writer.WriteLine("  </Target>");
                    writer.WriteLine("  <Target Name=\"BuildAndSign\" DependsOnTargets=\"BuildCrypPlugin;SignCrypPlugin\" />");
                    writer.WriteLine("</Project>");
                }
            }
            Logger.LogText(String.Format("Created build_plugin.xml for source {0}-{1}", Source.PluginId, Source.PluginVersion), this, Logtype.Info);

            return true;
        }

        /// <summary>
        /// 3) Process downloads zip file and extracts complete content into "plugin" folder
        /// </summary>
        /// <returns></returns>
        private bool DownloadZipFile()
        {
            Logger.LogText(String.Format("Start downloading source-{0}-{1}.zip", Source.PluginId, Source.PluginVersion), this, Logtype.Info);
            CrypToolStoreClient client = new CrypToolStoreClient();
            client.ServerAddress = Constants.ServerAddress;
            client.ServerPort = Constants.ServerPort;
            client.Connect();
            client.Login(Constants.Username, Constants.Password);

            string buildfoldername = BUILD_FOLDER + @"\" + SOURCE_FILE_NAME + "-" + Source.PluginId + "-" + Source.PluginVersion;

            DateTime startTime = DateTime.Now;
            bool stop = false;
            DataModificationOrRequestResult result = client.DownloadZipFile(Source, String.Format(buildfoldername + @"\plugin\source-{0}-{1}.zip", Source.PluginId, Source.PluginVersion), ref stop);
            client.Disconnect();

            if (result.Success)
            {
                Logger.LogText(String.Format("Downloaded source-{0}-{1}.zip in {2}", Source.PluginId, Source.PluginVersion, DateTime.Now.Subtract(startTime)), this, Logtype.Info);
                return true;
            }
            else
            {
                Logger.LogText(String.Format("Download of source-{0}-{1}.zip failed. Message was: {2}", Source.PluginId, Source.PluginVersion, result.Message), this, Logtype.Error);
                return false;
            }            
        }

        /// <summary>
        ///  4) Process extracts zip file
        /// </summary>
        /// <returns></returns>
        private bool ExtractZipFile()
        {
            Logger.LogText(String.Format("Start extracting source-{0}-{1}.zip", Source.PluginId, Source.PluginVersion), this, Logtype.Info);
            string buildfoldername = BUILD_FOLDER + @"\" + SOURCE_FILE_NAME + "-" + Source.PluginId + "-" + Source.PluginVersion;
            ZipFile.ExtractToDirectory(buildfoldername + String.Format(@"\plugin\source-{0}-{1}.zip", Source.PluginId, Source.PluginVersion), buildfoldername + @"\plugin\");
            File.Delete(buildfoldername + String.Format(@"\plugin\source-{0}-{1}.zip", Source.PluginId, Source.PluginVersion));
            Logger.LogText(String.Format("Finished extracting source-{0}-{1}.zip", Source.PluginId, Source.PluginVersion), this, Logtype.Info);
            return true;
        }

        /// <summary>
        ///  5) Process searches for exactly one csproj file in the root folder, i.e. "plugin"        
        ///  --> if it finds 0 or more than 1, the build process fails at this point
        /// </summary>
        /// <returns></returns>
        private bool SearchCSProjFile()
        {
            int counter = 0;
            string buildfoldername = BUILD_FOLDER + @"\" + SOURCE_FILE_NAME + "-" + Source.PluginId + "-" + Source.PluginVersion;

            //Search for the csproj file in folder structure
            SearchDir(buildfoldername,ref counter);

            //We only allow exactly one csproj file per Source
            if (counter == 0)
            {
                Logger.LogText(String.Format("source-{0}-{1} does not contain any csproj file", Source.PluginId, Source.PluginVersion), this, Logtype.Info);
                return false;
            }
            if (counter > 1)
            {
                Logger.LogText(String.Format("source-{0}-{1} contains more than one csproj file", Source.PluginId, Source.PluginVersion), this, Logtype.Info);
            }

            Logger.LogText(String.Format("Found csproj file in source-{0}-{1}: {2}", Source.PluginId, Source.PluginVersion, CSProjFileName), this, Logtype.Info);
            return true;
        }

        /// <summary>
        /// This method walks through the dedicated dir and its subdirs and searches for csproj files
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="counter"></param>
        private void SearchDir(string dir, ref int counter)
        {            
            string[] files = Directory.GetFiles(dir);
            foreach (string name in files)
            {
                if (name.ToLower().EndsWith("csproj"))
                {
                    CSProjFileName = name;
                    counter++;
                }
            }
            string[] dirs = Directory.GetDirectories(dir);
            foreach (string dir2 in dirs)
            {
                SearchDir(dir2, ref counter);
            }
        }

        /// <summary>
        ///  6) Process modifies csproj file
        ///  --> changes references to CrypPluginBase to correct path (hint: dont forget <private>false</private>)
        ///  --> changes output folder of "Release" target to "build_output" folder
        /// </summary>
        /// <returns></returns>
        private bool ModifyCSProjFile()
        {
            return true;
        }

        /// <summary>
        ///  7) Process modifies msbuild script
        ///  --> change name of target project to name of csproj file found in "plugin" folder
        /// </summary>
        /// <returns></returns>
        private bool ModifyMsBuildScript()
        {
            return true;
        }

        /// <summary>
        ///  8) Process starts "msbuild.exe" (hint: set correct password for signtool to allow it opening signing certificate)
        ///  --> msbuild compiles the plugin
        ///  --> signtool is also started and signs the builded assembly file
        /// </summary>
        /// <returns></returns>
        private bool BuildPlugin()
        {
            return true;
        }

        /// <summary>
        /// 9) Process checks, if assembly file exists in "build_output" (if not => ERROR)
        /// </summary>
        /// <returns></returns>
        private bool CheckBuild()
        {
            return true;
        }

        /// <summary>
        /// 10) Process checks, if a component is located in the assembly, i.e. a class which inherits from IPlugin (if not => ERROR)
        /// </summary>
        /// <returns></returns>
        private bool CheckComponentExists()
        {
            return true;
        }

        /// <summary>
        ///  11)  Process zips everything located in "build_output" -- this also includes "de/ru" etc subfolders of the plugin
        ///  --> zip name is "Assembly-1-1.zip, = Assembly-PluginId-SourceId")
        /// </summary>
        /// <returns></returns>
        private bool CreateAssemblyZip()
        {
            return true;
        }

        /// <summary>
        /// 12) Process uploads assembly zip file to CrypToolStore Server, and also updates source data in database
        /// </summary>
        /// <returns></returns>
        private bool UploadAssemblyZip()
        {
            return true;
        }

        /// <summary>
        /// 13) Process cleans up by deleting build folder (also in case of an error)
        /// </summary>
        private void CleanUp()
        {
            string buildfoldername = BUILD_FOLDER + @"\" + SOURCE_FILE_NAME + "-" + Source.PluginId + "-" + Source.PluginVersion;
            if (Directory.Exists(buildfoldername))
            {
                Directory.Delete(buildfoldername, true);
                Logger.LogText(String.Format("Deleted build folder for source {0}-{1}: {2}", Source.PluginId, Source.PluginVersion, BUILD_FOLDER), this, Logtype.Info);                
            }
            
        }
    }
}