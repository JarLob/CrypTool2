﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using KeySearcher.KeyPattern;
using KeySearcherPresentation.Controls;

namespace KeySearcher.P2P.Presentation
{
    class StatisticsGenerator
    {
        private readonly StatusContainer status;
        private readonly P2PQuickWatchPresentation quickWatch;
        private readonly KeySearcher keySearcher;
        private readonly DistributedBruteForceManager distributedBruteForceManager;
        private readonly BigInteger totalAmountOfChunks;
        private readonly Stopwatch stopWatch;

        private DateTime lastDateOfGlobalStatistics;
        private BigInteger highestChunkCalculated;
        private BigInteger totalRequestsAtStartOfNodeSearch;

        public StatisticsGenerator(StatusContainer status, P2PQuickWatchPresentation quickWatch, KeySearcher keySearcher, KeySearcherSettings settings, DistributedBruteForceManager distributedBruteForceManager)
        {
            this.status = status;
            this.quickWatch = quickWatch;
            this.keySearcher = keySearcher;
            this.distributedBruteForceManager = distributedBruteForceManager;

            lastDateOfGlobalStatistics = DateTime.Now;
            highestChunkCalculated = -1;
            stopWatch = new Stopwatch();

            var keyPattern = new KeyPattern.KeyPattern(keySearcher.ControlMaster.getKeyPattern())
                                 {WildcardKey = settings.Key};
            var keysPerChunk = Math.Pow(2, settings.ChunkSize);
            var keyPatternPool = new KeyPatternPool(keyPattern, new BigInteger(keysPerChunk));

            totalAmountOfChunks = keyPatternPool.Length;

            status.PropertyChanged += StatusPropertyChanged;
        }

        void StatusPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "DhtOverheadInReadableTime") return;

            if (distributedBruteForceManager.StopWatch.Elapsed.Ticks == 0)
            {
                status.DhtOverheadInPercent = "0 %";
                return;
            }

            var overheadInTicks = (double) status.DhtOverheadInReadableTime.Ticks/
                           distributedBruteForceManager.StopWatch.Elapsed.Ticks;
            var overheadInPercent = Math.Round(overheadInTicks, 2);
            overheadInPercent *= 100;
            status.DhtOverheadInPercent = overheadInPercent + " %";
        }

        public void MarkStartOfNodeSearch()
        {
            totalRequestsAtStartOfNodeSearch = status.TotalDhtRequests;
            stopWatch.Start();
        }

        public void MarkEndOfNodeSearch()
        {
            stopWatch.Stop();
            var elapsedTime = stopWatch.Elapsed.Add(status.DhtOverheadInReadableTime);
            status.DhtOverheadInReadableTime = new TimeSpan(((long) Math.Round((1.0*elapsedTime.Ticks/5))*5));
            stopWatch.Reset();
            
            var requestsForThisNode = status.TotalDhtRequests - totalRequestsAtStartOfNodeSearch;

            if (status.RequestsPerNode == 0)
            {
                status.RequestsPerNode = requestsForThisNode;
                return;
            }

            status.RequestsPerNode = (status.RequestsPerNode + requestsForThisNode)/2;
        }

        public void CalculateGlobalStatistics(BigInteger nextChunk)
        {
            if (highestChunkCalculated == -1) highestChunkCalculated = nextChunk;
            if (nextChunk <= highestChunkCalculated) return;

            var totalAmountOfParticipants = nextChunk - highestChunkCalculated;
            status.TotalAmountOfParticipants = totalAmountOfParticipants;

            var timeUsedForLatestProgress = DateTime.Now.Subtract(lastDateOfGlobalStatistics);
            var secondsForOneChunk = timeUsedForLatestProgress.TotalSeconds/(double) totalAmountOfParticipants;
            var remainingChunks = totalAmountOfChunks - nextChunk;
            var secondsRemaining = (double) remainingChunks*secondsForOneChunk;

            try
            {
                status.EstimatedFinishDate = DateTime.Now.AddSeconds(secondsRemaining).ToString("dd.MM. HH:mm");
            }
            catch (ArgumentOutOfRangeException)
            {
                status.EstimatedFinishDate = "~";
            }

            lastDateOfGlobalStatistics = DateTime.Now;

            highestChunkCalculated = nextChunk;
            var globalProgressValue = (double) highestChunkCalculated/(double) totalAmountOfChunks;
            keySearcher.ProgressChanged(globalProgressValue, 1);
        }

        public void ProcessPatternResults(LinkedList<KeySearcher.ValueKey> result)
        {
            ProcessResultList(result);
        }

        public void ShowProgress(LinkedList<KeySearcher.ValueKey> bestResultList, BigInteger keysInThisChunk, BigInteger keysFinishedInThisChunk, BigInteger keysPerSecond)
        {
            status.ProgressOfCurrentChunk = (double) keysFinishedInThisChunk/(double) keysInThisChunk;
            status.KeysPerSecond = keysPerSecond;

            var time = (Math.Pow(10, BigInteger.Log((keysInThisChunk - keysFinishedInThisChunk), 10) - BigInteger.Log(keysPerSecond, 10)));
            var timeleft = new TimeSpan(-1);

            try
            {
                if (time / (24 * 60 * 60) <= int.MaxValue)
                {
                    int days = (int)(time / (24 * 60 * 60));
                    time = time - (days * 24 * 60 * 60);
                    int hours = (int)(time / (60 * 60));
                    time = time - (hours * 60 * 60);
                    int minutes = (int)(time / 60);
                    time = time - (minutes * 60);
                    int seconds = (int)time;

                    timeleft = new TimeSpan(days, hours, minutes, (int)seconds, 0);
                }
            }
            catch
            {
                //can not calculate time span
            }

            if (timeleft != new TimeSpan(-1))
            {
                status.RemainingTime = timeleft.ToString();
            } 
            else
            {
                status.RemainingTime = "~";
            }

            ProcessResultList(bestResultList);
        }

        private void ProcessResultList(LinkedList<KeySearcher.ValueKey> bestResultList)
        {
            quickWatch.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {

                var enc = Encoding.Default;
                LinkedListNode<KeySearcher.ValueKey> linkedListNode;
                status.TopList.Clear();
                linkedListNode = bestResultList.First;

                int i = 0;
                while (linkedListNode != null)
                {
                    i++;

                    var entry = new ResultEntry();
                    entry.Ranking = i.ToString();
                    entry.Value = Math.Round(linkedListNode.Value.value, 2).ToString();
                    entry.Key = linkedListNode.Value.key;
                    var plainText = enc.GetString(linkedListNode.Value.decryption);

                    const string replaceWith = "";
                    plainText = plainText.Replace("\r\n", replaceWith).Replace("\n", replaceWith).Replace("\r", replaceWith);
                    if (plainText.Length > 30)
                        plainText = plainText.Substring(0, 30) + "...";

                    entry.Text = plainText;

                    status.TopList.Add(entry);
                    linkedListNode = linkedListNode.Next;
                }
            }, null);
        }
    }
}
