using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using KeySearcherPresentation.Controls;
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
        private BigInteger localAbortChunks;
        private BigInteger keysPerSecond;
        private String currentOperation;
        private TimeSpan avgTimePerChunk;

        private DateTime startDate;
        private TimeSpan elapsedTime;
        private TimeSpan remainingTime;
        private TimeSpan remainingTimeTotal;
        private DateTime estimatedFinishDate;

        public ObservableCollection<BigInteger> CurrentChunks { get; set; } 
        public ObservableCollection<KeyResultEntry> TopList { get; set; }


        private readonly SpeedStatistics globalSpeedStatistics = new SpeedStatistics();
        private readonly SpeedStatistics localSpeedStatistics = new SpeedStatistics(); 

        public P2PPresentationVM()
        {
            CurrentOperation = "Idle";
            AvgTimePerChunk = new TimeSpan(0);
            TopList = new ObservableCollection<KeyResultEntry>();
            CurrentChunks = new ObservableCollection<BigInteger>();
        }

       
         
        #region local calculation

        public void StartedLocalCalculation(BigInteger blockId)
        {
            CurrentChunks.Add(blockId);
            OnPropertyChanged("CurrentChunks");
        }
      
        public void EndedLocalCalculation(TaskEventArgs taskArgs)
        {
            var itemInList = CurrentChunks.First(it => it == taskArgs.BlockID);
            CurrentChunks.Remove(itemInList);
            OnPropertyChanged("CurrentChunks");

            if (taskArgs.Type == TaskEventArgType.Finished)
            {
                localSpeedStatistics.Tick(KeysPerBlock); 
                AvgTimePerChunk = localSpeedStatistics.LatestAvgTime;
                KeysPerSecond = localSpeedStatistics.LatestKeysPerSecond;

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
            globalSpeedStatistics.Tick(KeysPerBlock);
            AvgTimePerChunkGlobal = globalSpeedStatistics.LatestAvgTime;
            KeysPerSecondGlobal = globalSpeedStatistics.LatestKeysPerSecond;
          
            GlobalProgress = 100 * progress.NumberOfCalculatedBlocks.DivideAndReturnDouble(progress.NumberOfBlocks);

            var numberOfLeftBlocks = progress.NumberOfBlocks - progress.NumberOfCalculatedBlocks;
            var remainingTicks = (long) (AvgTimePerChunkGlobal.Ticks * numberOfLeftBlocks);

            RemainingTimeTotal = new TimeSpan(remainingTicks);
            EstimatedFinishDate = DateTime.Now.Add(RemainingTimeTotal);

            FillTopList(keyResultEntries);
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
