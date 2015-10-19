// Copyright 2014 Christopher Konze, University of Kassel
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#region

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using voluntLib.common.utils;

#endregion

namespace voluntLib.managementLayer.delayedTasks
{
    /// <summary>
    ///   A Simple Task, that's waits a random time and then executes its action.
    /// </summary>
    public abstract class ARandomDelayedTask
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly string timerTypeForLogger;
        private readonly IPAddress toAddress;
        private CancellationTokenSource cancellationToken;
        private Task currentTask;

        protected ARandomDelayedTask(string timerTypeForLogger) : this(timerTypeForLogger, IPAddress.Broadcast) {}

        protected ARandomDelayedTask(string timerTypeForLogger, IPAddress toAddress)
        {
            this.timerTypeForLogger = timerTypeForLogger;
            this.toAddress = toAddress;
        }

        public bool IsRunning()
        {
            return currentTask != null && !currentTask.IsCompleted;
        }

        public void StartTimer(int max)
        {
            if (currentTask != null)
            {
                cancellationToken.Cancel();
                if (currentTask.IsCanceled || currentTask.IsFaulted || currentTask.IsCompleted)
                {
                    currentTask.Dispose();
                }
            }

            if (max <= 0)
            {
                // do action now
                TimerEvent();
                return;
            }

            var rndTime = new Random().Next(1, max);
            Logger.Info("(" + timerTypeForLogger + ") Timer will be elapsed in " + rndTime + "ms");

            cancellationToken = new CancellationTokenSource();
            var token = cancellationToken.Token;
            currentTask = TaskHelper.Delay(rndTime).ContinueWith(_ => TimerEvent(), token);
        }

        public void StopTimer()
        {
            if (IsRunning())
            {
                Logger.Info("(" + timerTypeForLogger + ") Timer has been stoped");
                cancellationToken.Cancel();
                currentTask.Dispose();
            }
        }

        private void TimerEvent()
        {
            Logger.Info("(" + timerTypeForLogger + ") Timer elapsed");
            TimerAction(toAddress);
        }

        protected abstract void TimerAction(IPAddress to);
    }
}