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
            Thread buildWorkerThread = new Thread(BuildWorkerThreadMethod);
            buildWorkerThread.IsBackground = true;
            buildWorkerThread.Start();

        }

        /// <summary>
        /// Stops this BuildWorker
        /// </summary>
        public void Stop()
        {
            IsRunning = false;
        }

        public void BuildWorkerThreadMethod()
        {
            Logger.LogText(String.Format("Started build of {0}-{1}", Source.PluginId, Source.PluginVersion), this, Logtype.Info);


























            Logger.LogText(String.Format("Terminated build of {0}-{1}", Source.PluginId, Source.PluginVersion), this, Logtype.Info);
            IsRunning = false;
        }

    }
}
