/*
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

using System.Collections.Generic;
using System.ComponentModel;
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

        #endregion

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
            NumberOfBlocks = 1000;
        }

        #region Data Properties

        [PropertyInfo(Direction.InputData, "Input name", "Input tooltip description")]
        public int SomeInput {get; set;} 
        
        [PropertyInfo(Direction.OutputData, "Output name", "Output tooltip description")]
        public int SomeOutput {get; set;}

        #endregion

        public override List<byte[]> CalculateBlock(BigInteger blockId, CancellationToken cancelToken)
        {
            GuiLogMessage("ERROR: " + blockId, NotificationLevel.Error);
            return new List<byte[]>();
        }

        public override List<byte[]> MergeBlockResults(IEnumerable<byte[]> oldResultList, IEnumerable<byte[]> newResultList)
        {
            return new List<byte[]>();
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
         
        public override event StatusChangedEventHandler OnPluginStatusChanged;
        public override event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public override event PropertyChangedEventHandler PropertyChanged;
    }
}
