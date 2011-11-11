﻿/* 
   Copyright 2009 Holger Pretzsch, University of Duisburg-Essen

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
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using Cryptool.PluginBase;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows;
using Cryptool.PluginBase.IO;
using System.Runtime.CompilerServices;
using Cryptool.PluginBase.Miscellaneous;
using System.Runtime.Remoting.Contexts;
using Cryptool.MD5.Presentation;
using Cryptool.MD5.Algorithm;
using System.Windows.Threading;

namespace Cryptool.MD5
{
    [Author("Sebastian Przybylski", "sebastian@przybylski.org", "Uni-Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("Cryptool.Plugins.MD5.Properties.Resources", "PluginCaption", "PluginTooltip", "MD5/DetailedDescription/doc.xml", "MD5/MD5.png")]
    [ComponentCategory(ComponentCategory.HashFunctions)]
    public class MD5 : ICrypComponent
    {
        #region Private variables

        private ICryptoolStream inputData;
        private byte[] outputData;
        private PresentableMD5 md5;
        private PresentationContainer presentationContainer;

        #endregion

        #region Public interface
        public MD5()
        {
            md5 = new PresentableMD5();
            md5.AddSkippedState(MD5StateDescription.STARTING_ROUND_STEP);
            md5.AddSkippedState(MD5StateDescription.FINISHING_COMPRESSION);

            presentationContainer = new PresentationContainer(md5);

            md5.StatusChanged += Md5StatusChanged;
        }

        public ISettings Settings
        {
            get { return null; }
        }

        [PropertyInfo(Direction.InputData, "InputDataCaption", "InputDataTooltip", true, QuickWatchFormat.Hex, null)]
        public ICryptoolStream InputData
        {
            get 
            {
                return inputData;
              }
          
            set 
            { 
              this.inputData = value;
              OnPropertyChanged("InputData");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputDataStreamCaption", "OutputDataStreamTooltip", false, QuickWatchFormat.Hex, null)]
        public ICryptoolStream OutputDataStream
        {
          get 
          {            
            if (outputData != null)
            {
              GuiLogMessage("Got request for hash (Stream)...", NotificationLevel.Debug);
                return new CStreamWriter(outputData);
            }
            return null; ;
          }
        }

        [PropertyInfo(Direction.OutputData, "OutputDataCaption", "OutputDataTooltip", false, QuickWatchFormat.Hex, null)]
        public byte[] OutputData 
        {
          
            get 
            {
              GuiLogMessage("Got request for hash (Byte Array)...", NotificationLevel.Debug);
              return this.outputData; 
            }
          
            set 
            { 
              outputData = value;              
              OnPropertyChanged("OutputData");
              OnPropertyChanged("OutputDataStream");             
            }
        }

        void Md5StatusChanged()
        {
            if (md5.IsInFinishedState)
            {
                OutputData = md5.HashValueBytes;
                ProgressChanged(1.0, 1.0);
            }
        }

        public void Execute()
        {
            ProgressChanged(0.5, 1.0);
            if (inputData != null)
            {
                if (Presentation.IsVisible)
                {
                    md5.Initialize(inputData);
                }
                else
                {
                    HashAlgorithm builtinMd5 = System.Security.Cryptography.MD5.Create();

                    using (CStreamReader reader = inputData.CreateReader())
                    {
                        OutputData = builtinMd5.ComputeHash(reader);
                    }

                    ProgressChanged(1.0, 1.0);
                }
            }
            else
            {
                GuiLogMessage("Received null value for CryptoolStream.", NotificationLevel.Warning);
            }
        }
        
        public void Initialize()
        {            
        }
        
        public void Dispose()
        {
        }
        #endregion

        #region IPlugin Members

        public UserControl Presentation
        {
          get { return presentationContainer; }
        }

        public void Stop()
        {

        }
        
        public void PostExecution()
        {
            inputData = null;
        }
        
        public void PreExecution()
        {
            inputData = null;
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
          EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region IPlugin Members

#pragma warning disable 67
				public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
          EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
          EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}
