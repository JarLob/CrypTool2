using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using CrypCloud.Core;
using Cryptool.PluginBase;
using KeySearcher.CrypCloud.statistics;
using KeySearcher.KeyPattern;
using KeySearcherPresentation.Controls;
using voluntLib.common;
using voluntLib.common.eventArgs;
using Timer = System.Timers.Timer;

namespace KeySearcher.CrypCloud
{
    public class P2PPresentationVM : INotifyPropertyChanged
    {
        private String jobName;
        private String jobDesc;
        private BigInteger jobID;
        private BigInteger totalAmountOfChunks;
        private BigInteger keysPerBlock;

        private double globalProgress;
        private BigInteger keysPerSecondGlobal;
        private TimeSpan avgTimePerChunkGlobal;

        private BigInteger localFinishedChunks;
        private BigInteger finishedNumberOfBlocks;
        private BigInteger localAbortChunks;
        private BigInteger keysPerSecond;
        private String currentOperation = "idle";
        private TimeSpan avgTimePerChunk;

        private DateTime startDate;
        private TimeSpan elapsedTime;
        private TimeSpan remainingTime;
        private TimeSpan remainingTimeTotal;
        private DateTime estimatedFinishDate;
        private BigInteger numberOfLeftBlocks;

        public SpeedStatistics GlobalSpeedStatistics { get; set; }
        public SpeedStatistics LocalSpeedStatistics { get; set; }

        public TaskFactory UiContext { get; set; }

        public ObservableCollection<BigInteger> CurrentChunks { get; set; } 
        public ObservableCollection<KeyResultEntry> TopList { get; set; }


        public P2PPresentationVM()
        {
            CurrentOperation = "Idle";
            AvgTimePerChunk = new TimeSpan(0);
            TopList = new ObservableCollection<KeyResultEntry>();
            CurrentChunks = new ObservableCollection<BigInteger>(); 
           
        }
        public void UpdateStaticView(BigInteger jobId, KeySearcher keySearcher, KeySearcherSettings keySearcherSettings)
        { 
            if (!CrypCloudCore.Instance.IsRunning) return;
             
            var job = CrypCloudCore.Instance.GetJobsById(jobId);
            if(job == null) return;

            JobName = job.JobName;

            if (job.StateConfig.NumberOfBlocks != 0)
            {
                FinishedNumberOfBlocks = CrypCloudCore.Instance.GetCalculatedBlocksOfJob(jobId);
                var progress = 100 * finishedNumberOfBlocks.DivideAndReturnDouble(job.StateConfig.NumberOfBlocks);
                GlobalProgress = progress;
                OnPropertyChanged("GlobalProgressString"); 
            }

        }

        public void UpdateSettings(KeySearcher keySearcher, KeySearcherSettings keySearcherSettings)
        {
            if (CannotUpdateView(keySearcher, keySearcherSettings))
            {
                return;
            }

            var keyPattern = new KeyPattern.KeyPattern(keySearcher.ControlMaster.GetKeyPattern()) { WildcardKey = keySearcherSettings.Key };
            var keysPerChunk = keyPattern.size() / BigInteger.Pow(2, keySearcherSettings.NumberOfBlocks);
            if (keysPerChunk < 1)
            {
                keySearcherSettings.NumberOfBlocks = (int)BigInteger.Log(keyPattern.size(), 2);
            }

            var keyPatternPool = new KeyPatternPool(keyPattern, keysPerChunk);

            TotalAmountOfChunks = keyPatternPool.Length;
            KeysPerBlock = keysPerChunk;
            JobID = keySearcher.JobID;
            
        }
        
        private static bool CannotUpdateView(KeySearcher keySearcher, KeySearcherSettings keySearcherSettings)
        {
            return keySearcher.Pattern == null || !keySearcher.Pattern.testWildcardKey(keySearcherSettings.Key) || keySearcherSettings.NumberOfBlocks == 0;
        }

        #region local calculation

        public void StartedLocalCalculation(BigInteger blockId)
        {
            if (CurrentChunks.Contains(blockId)) return;

            CurrentChunks.Add(blockId);
            OnPropertyChanged("CurrentChunks");
        }
      
        public void EndedLocalCalculation(TaskEventArgs taskArgs)
        {
            var itemInList = CurrentChunks.FirstOrDefault(it => it == taskArgs.BlockID);
            if (itemInList != default(BigInteger))
            {
                CurrentChunks.Remove(itemInList);
                OnPropertyChanged("CurrentChunks");
            }

            if (itemInList == 0 && CurrentChunks.Contains(0))
            {
                CurrentChunks.Remove(0);
                OnPropertyChanged("CurrentChunks");
            }
            
           if (taskArgs.Type == TaskEventArgType.Finished)
            {
                LocalFinishedChunks++;
            }
            else 
            {
                LocalAbortChunks++;
            }  
        } 
     
        #endregion 
        
        public void BlockHasBeenFinished(JobProgressEventArgs progress, List<KeyResultEntry> keyResultEntries)
        {
            FinishedNumberOfBlocks = CrypCloudCore.Instance.GetCalculatedBlocksOfJob(JobID);
            GlobalSpeedStatistics.AddEntry(KeysPerBlock); 
            GlobalProgress = 100 * progress.NumberOfCalculatedBlocks.DivideAndReturnDouble(progress.NumberOfBlocks);
            numberOfLeftBlocks = progress.NumberOfBlocks - progress.NumberOfCalculatedBlocks;
            FillTopList(keyResultEntries);

            OnPropertyChanged("GlobalProgressString"); 
        }

        public void UpdateGlobalSpeed(BigInteger keysPerSecond)
        {
            KeysPerSecondGlobal = keysPerSecond;
            if (keysPerSecond != 0)
            {
                var timePerBlock = (KeysPerBlock.DivideAndReturnDouble(KeysPerSecondGlobal));
                AvgTimePerChunkGlobal = TimeSpan.FromSeconds(timePerBlock);
            }

            if(numberOfLeftBlocks == -1) return;

            var remainingTicks = (long)(AvgTimePerChunkGlobal.Ticks * numberOfLeftBlocks);

            RemainingTimeTotal = new TimeSpan(remainingTicks);
            EstimatedFinishDate = DateTime.Now.Add(RemainingTimeTotal);
        }


        public void UpdateLocalSpeed(BigInteger localApproximateKeysPerSecond)
        {
            KeysPerSecond = localApproximateKeysPerSecond;
            if (keysPerSecond != 0)
            {
                var timePerBlock = (KeysPerBlock.DivideAndReturnDouble(localApproximateKeysPerSecond));
                AvgTimePerChunk = TimeSpan.FromSeconds(timePerBlock);
            }
        }


        private void FillTopList(List<KeyResultEntry> keyResultEntries)
        {
            TopList.Clear();
            keyResultEntries.ForEach(it => TopList.Add(it));
        }

        #region properties with propChange handler

        public String JobName
        {
            get { return jobName; }
            set
            {
                jobName = value;
                OnPropertyChanged("JobName");
            }
        }


        public String JobDesc
        {
            get { return jobDesc; }
            set
            {
                jobDesc = value;
                OnPropertyChanged("JobDesc");
            }
        }
        public String CurrentOperation
        {
            get { return currentOperation; }
            set
            {
                currentOperation = value;
                OnPropertyChanged("CurrentOperation");
            }
        }
        
        public BigInteger LocalFinishedChunks
        {
            get{return localFinishedChunks; }
            set
            {
                localFinishedChunks = value;
                OnPropertyChanged("LocalFinishedChunks");
            }
        }

        public BigInteger LocalAbortChunks
        {
            get { return localAbortChunks; }
            set
            {
                localAbortChunks = value;
                OnPropertyChanged("LocalAbortChunks");
            }
        }

        public BigInteger JobID
        {
            get { return jobID; }
            set
            {
                jobID = value;
                OnPropertyChanged("JobID");
            }
        }
     
     
        public double GlobalProgress
        {
            get { return globalProgress; }
            set
            {
                globalProgress = value;
                OnPropertyChanged("GlobalProgress");
            }
        }
      
        public BigInteger KeysPerSecond
        {
            get { return keysPerSecond; }
            set
            {
                keysPerSecond = value;
                OnPropertyChanged("KeysPerSecond");
            }
        }

        public DateTime StartDate
        {
            get { return startDate; }
            set
            {
                startDate = value;
                OnPropertyChanged("StartDate");
            }
        }

        public TimeSpan ElapsedTime
        {
            get { return elapsedTime; }
            set
            {
                elapsedTime = value;
                OnPropertyChanged("ElapsedTime");
            }
        }
        
        public TimeSpan AvgTimePerChunk
        {
            get { return avgTimePerChunk; }
            set
            {
                avgTimePerChunk = value;
                OnPropertyChanged("AvgTimePerChunk");
            }
        }  
        
        public TimeSpan AvgTimePerChunkGlobal
        {
            get { return avgTimePerChunkGlobal; }
            set
            {
                avgTimePerChunkGlobal = value;
                OnPropertyChanged("AvgTimePerChunkGlobal");
            }
        }

        public TimeSpan RemainingTime
        {
            get { return remainingTime; }
            set
            {
                remainingTime = value;
                OnPropertyChanged("RemainingTime");
            }
        }

        public TimeSpan RemainingTimeTotal
        {
            get { return remainingTimeTotal; }
            set
            {
                remainingTimeTotal = value;
                OnPropertyChanged("RemainingTimeTotal");
            }
        }

        public DateTime EstimatedFinishDate
        {
            get { return estimatedFinishDate; }
            set
            {
                estimatedFinishDate = value;
                OnPropertyChanged("EstimatedFinishDate");
            }
        }

        public BigInteger TotalAmountOfChunks
        {
            get { return totalAmountOfChunks; }
            set
            {
                totalAmountOfChunks = value;
                OnPropertyChanged("TotalAmountOfChunks");
            }
        }

        public BigInteger KeysPerBlock
        {
            get { return keysPerBlock; }
            set
            {
                keysPerBlock = value;
                OnPropertyChanged("KeysPerBlock");
            }
        }
        public BigInteger FinishedNumberOfBlocks
        {
            get { return finishedNumberOfBlocks; }
            set
            {
                finishedNumberOfBlocks = value;
                OnPropertyChanged("FinishedNumberOfBlocks");
            }
        }

        public string GlobalProgressString
        {
            get
            {
                if (TotalAmountOfChunks == 0) return "~";

                var doneBlocks = FinishedNumberOfBlocks.ToString("N0", new CultureInfo("de-DE"));
                var totalBlocks = TotalAmountOfChunks.ToString("N0", new CultureInfo("de-DE"));
                var logBlocks = BigInteger.Log(TotalAmountOfChunks, 2);

                return string.Format("{0} / {1} ({2} bits)", doneBlocks, totalBlocks, logBlocks);
            }
            set { } //for binding only
        }
        
        public BigInteger KeysPerSecondGlobal
        {
            get { return keysPerSecondGlobal; }
            set
            {
                keysPerSecondGlobal = value;
                OnPropertyChanged("KeysPerSecondGlobal");
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

      
    }
}
