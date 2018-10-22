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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrypToolStoreBuildSystem
{
    public class CrypToolStoreBuildServer
    {
        //interval in which the build server connects to the CrypToolStore server
        //and checks, if builds are needed
        private const int BUILD_CHECK_INTERVAL = 60000; //60 sec;     
        private const int MAX_BUILD_WORKERS = 8;

        private Logger Logger = Logger.GetLogger();
        private bool IsRunning { get; set; }
        private ConcurrentBag<BuildWorker> workers = new ConcurrentBag<BuildWorker>();

        /// <summary>
        /// Creates a new instance of the BuildServer
        /// </summary>
        public CrypToolStoreBuildServer()
        {

        }

        /// <summary>
        /// Starts the build server
        /// by starting the BuildServerLoop
        /// </summary>
        public void Start()
        {
            if (IsRunning == true)
            {
                return;
            }
            IsRunning = true;
            Logger.LogText("CrypToolStore BuildServer is starting", this, Logtype.Info);

            Thread thread = new Thread(BuildServerLoop);
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// Stops everything
        /// </summary>
        public void Stop()
        {
            IsRunning = false;
        }

        /// <summary>
        /// Main loop of the buildserver
        /// Feteches sources that need to be build and starts workers for building them
        /// </summary>
        public void BuildServerLoop()
        {
            Logger.LogText("CrypToolStore BuildServer started", this, Logtype.Info);
            DateTime lastCheckTime = DateTime.Now.AddSeconds(-60);
            while (IsRunning)
            {
                try
                {
                    if (DateTime.Now >= lastCheckTime.AddMilliseconds(BUILD_CHECK_INTERVAL))
                    {
                        CheckBuildNeeded();
                        
                    }
                    
                }
                catch (Exception ex)
                {
                    Logger.LogText(String.Format("Exception occured in BuildServerLoop: {0}", ex.Message), this, Logtype.Error);
                }
                finally
                {
                    lastCheckTime = DateTime.Now;
                    Thread.Sleep(1000);
                }


            }
            Logger.LogText("CrypToolStore BuildServer terminated", this, Logtype.Info);
        }

        /// <summary>
        /// Checks, by querying the database which source needs a new build
        /// Then, it starts workers until MAX_BUILD_WORKERS is reached
        /// </summary>
        private void CheckBuildNeeded()
        {
            Logger.LogText("Check sources for building", this, Logtype.Info);

            //remove workers that are not running
            foreach (BuildWorker worker in workers)
            {
                if (!worker.IsRunning)
                {
                    BuildWorker removeworker;
                    workers.TryTake(out removeworker);
                }
            }

            if (workers.Count >= MAX_BUILD_WORKERS)
            {
                return;
            }

            //get all sources from the CrypToolStore
            CrypToolStoreClient client = new CrypToolStoreClient();
            client.ServerAddress = Constants.ServerAddress;
            client.ServerPort = Constants.ServerPort;
            client.Connect();
            client.Login(Constants.Username, Constants.Password);

            List<Source> sources = (List<Source>)client.GetSourceList(-1, BuildState.UPLOADED.ToString()).DataObject;

            client.Disconnect();

            //start new workers until MAX_BUILD_WORKERS is reached
            foreach (Source source in sources)
            {
                if (workers.Count < MAX_BUILD_WORKERS)
                {

                    Logger.LogText(String.Format("Creating and starting worker to build source {0}-{1}", source.PluginId, source.PluginVersion), this, Logtype.Info);
                    BuildWorker worker = new BuildWorker(source);
                    workers.Add(worker);
                    worker.Start();
                    Thread.Sleep(1000);
                }
            }
        }        
    }
}
