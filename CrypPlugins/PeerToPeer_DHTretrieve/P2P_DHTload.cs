﻿/* Copyright 2009 Team CrypTool (Christian Arnold), Uni Duisburg-Essen

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
using Cryptool;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;
using Cryptool.PluginBase.Tool;
using Cryptool.PluginBase.Control;
using Cryptool.Plugins.PeerToPeer.Internal;

namespace Cryptool.Plugins.PeerToPeer
{
    /// <summary>
    /// This PlugIn only works, when its connected with a P2P_Peer object.
    /// </summary>
    [Author("Christian Arnold", "arnold@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "P2P_DHTload", "Loading data out of the DHT", "", "PeerToPeer_DHTretrieve/ct2_dht_load_icon_medium.png")]
    public class P2P_DHTload : IThroughput
    {
        private P2PDHTloadSettings settings;

        #region In and Output

        private IP2PControl p2pMaster;
        /// <summary>
        /// Catches the completely configurated, initialized and joined P2P object from the P2PPeer-Slave-PlugIn.
        /// </summary>
        [PropertyInfo(Direction.ControlMaster, "P2P Slave", "Input the P2P-Peer-PlugIn", "", true, false, QuickWatchFormat.Text, null)]
        public IP2PControl P2PMaster
        {
            get
            {
                return this.p2pMaster;
            }
            set
            {
                if (p2pMaster != null)
                {
                    p2pMaster.OnStatusChanged -= P2PMaster_OnStatusChanged;
                }
                if (value != null)
                {
                    value.OnStatusChanged += new IControlStatusChangedEventHandler(P2PMaster_OnStatusChanged);
                    p2pMaster = (P2PPeerMaster)value;
                    OnPropertyChanged("P2PMaster");
                }
                else
                {
                    p2pMaster = null;
                }
            }
        }

        private void P2PMaster_OnStatusChanged(IControl sender, bool readyForExecution)
        {
            //throw new NotImplementedException();
        }

        private string sDhtKey;
        [PropertyInfo(Direction.InputData, "Key Name", "Key Name of DHT Entry in the P2P-System", "", true, false, QuickWatchFormat.Text, null)]
        public string DhtKey
        {
            get
            {
                return this.sDhtKey;
            }
            set
            {
                this.sDhtKey = value;
                OnPropertyChanged("DhtKey");
            }
        }

        private byte[] bDhtValue;
        [PropertyInfo(Direction.OutputData, "Value of the Key", "Value of the DHT Entry in the P2P-System", "", true, false, QuickWatchFormat.Text, null)]
        public byte[] DhtValue
        {
            get
            {
                return this.bDhtValue;
            }
            set
            {
                this.bDhtValue = value;
                OnPropertyChanged("DhtValue");
            }
        }

        #endregion

        #region Events

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        #endregion

        public P2P_DHTload()
        {
            this.settings = new P2PDHTloadSettings(this);
        }

        public ISettings Settings
        {
            set { this.settings = (P2PDHTloadSettings)value; }
            get { return this.settings; }
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public System.Windows.Controls.UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
        }

        public void Execute()
        {
            // if no P2P Slave PlugIn is connected with this PlugIn --> No execution!
            if (P2PMaster == null)
            {
                GuiLogMessage("No P2P_Peer connected with this PlugIn!", NotificationLevel.Error);
                return;
            }
            if (DhtKey != null)
            {
                DhtValue = P2PMaster.DHTload(DhtKey);
                if (DhtValue != null)
                    GuiLogMessage("KeyValue of DHT Entry '" + DhtKey + "' retrieved: " + DhtValue.ToString(), NotificationLevel.Info);
                else
                    GuiLogMessage("KeyValue of DHT Entry '" + DhtKey + "' is null. Possible cause: Key doesn't exist", NotificationLevel.Info);
            }
            else
            {
                GuiLogMessage("No key in input. Loading isn't possible.", NotificationLevel.Error);
            }
        }

        public void PostExecution()
        {
            //throw new NotImplementedException();
        }

        public void Pause()
        {
            //throw new NotImplementedException();
        }

        public void Stop()
        {
            //throw new NotImplementedException();
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public event PluginProgressChangedEventHandler OnPluginProcessChanged;

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        #endregion
    }
}
