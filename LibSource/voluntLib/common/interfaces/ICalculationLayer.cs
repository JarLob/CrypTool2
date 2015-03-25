﻿// Copyright 2014 Christopher Konze, University of Kassel
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
using System.Collections.Generic;
using voluntLib.common.eventArgs;
using voluntLib.managementLayer.localStateManagement;
using voluntLib.managementLayer.localStateManagement.states;

#endregion

namespace voluntLib.common.interfaces
{
    public interface ICalculationLayer
    {
        byte[] JobPayload { get; set; }
        int NumberOfRunningWorker { get; }
        bool IsStarted { get; }

        bool Start();
        void Stop();

        List<byte[]> MergeResults(IEnumerable<byte[]> a, IEnumerable<byte[]> b);
        void RegisterLocalStateManager(LocalStateManager<EpochState> stateManager);

        event EventHandler<TaskEventArgs> TaskStarted;
        event EventHandler<TaskEventArgs> TaskProgress;
        event EventHandler<TaskEventArgs> TaskStopped;
    }
}