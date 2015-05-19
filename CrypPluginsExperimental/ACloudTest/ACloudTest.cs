﻿/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Windows.Controls;
using CrypCloud.Core.CloudComponent;
using Cryptool.PluginBase; 

namespace Cryptool.Plugins.ACloudTest
{
    [Author("Anonymous", "coredevs@cryptool.org", "CrypTool 2 Team", "http://cryptool2.vs.uni-due.de")]
    [PluginInfo("Cloud Test Component", "Subtract one number from another", "ACloudTest/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class ACloudTest : ACloudComponent
    {

        #region Private Variables
         
        private readonly ACloudTestSettings settings = new ACloudTestSettings();
        private string someOutput;

        #endregion

        public ACloudTest(): base(1000){}

        public override ISettings Settings
        {
            get { return settings; }
        }

        public override UserControl Presentation
        {
            get { return null; }
        }

        public override void Initialize()
        {
        }

        #region Data Properties

        [PropertyInfo(Direction.OutputData, "Output name", "Output tooltip description")]
        public string SomeOutput
        {
            get { return someOutput; }
            set
            {
                if (value != someOutput)
                {
                    this.someOutput = value;
                    OnPropertyChanged("SomeOutput");
                }
            }
        }

        #endregion

        public override List<byte[]> CalculateBlock(BigInteger blockId, CancellationToken cancelToken)
        {
            var rnd = new Random(123457);
            var results = new List<byte[]> {blockId.ToByteArray()};
            for (var i = 0; i < 4; i++)
            {
                cancelToken.ThrowIfCancellationRequested();
                Thread.Sleep(rnd.Next(1, 3) * 500);
            }

            GuiLogMessage("calculatedBlock: " + blockId, NotificationLevel.Error);
            return results;
        }

        public override List<byte[]> MergeBlockResults(IEnumerable<byte[]> oldResultList, IEnumerable<byte[]> newResultList)
        {
            var newlist = oldResultList.Concat(newResultList).ToList();
            newlist.Sort((bytes, bytes1) => bytes1[0] - bytes[0]);
            var mergeBlockResults = newlist.Take(10).ToList();
            SomeOutput = mergeBlockResults.Aggregate("", (_, it) =>  _ + " " + it[0]);
            
            return mergeBlockResults; 
        }


        public override void StopLocal()
        {
        }

        public override void Execute()
        {
        }

        
        public override void PostExecution()
        {
        }

        public override void Dispose()
        {
        }

        public override void PreExecutionLocal()
        {
            Initialize();
        }

        public override event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        public override event StatusChangedEventHandler OnPluginStatusChanged;
        public override event PluginProgressChangedEventHandler OnPluginProgressChanged; 

      
    }
}
