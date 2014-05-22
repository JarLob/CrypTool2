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
using System.Windows;
using System.Windows.Navigation;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.Plugins.NetworkSender;

namespace Cryptool.Plugins.NetworkSender {
    public class NetworkSenderSettings : ISettings {
        public static int udpProtocol = 0;
        public static int tcpProtocol = 1;

        #region Private Variables

        private int port;
        private string deviceIP;
        private bool byteAsciiSwitch;
        private bool tryConnect = false;
        private int connectIntervall;
        private int protocol;

        #endregion

        #region TaskPane Settings

        [TaskPane("DeviceIpCaption", "DeviceIpCaptionTooltip", "NetworkConditions", 0, false, ControlType.TextBox)]
        public string DeviceIP {
            get { return deviceIP; }
            set {
                deviceIP = value;
                OnPropertyChanged("DeviceIp");
            }
        }

        [TaskPane("Port", "PortToolTip", "NetworkConditions", 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 65535)]
        public int Port {
            get {
                return port;
            }
            set {
                if (port != value) {
                    port = value;
                    OnPropertyChanged("Port");
                }
            }
        }

        [TaskPane("ByteAsciiSwitchCaption", "ByteAsciiSwitchCaptionTooltip", "PresentationSettings", 3, false, ControlType.CheckBox)]
        public bool ByteAsciiSwitch {
            get { return byteAsciiSwitch; }
            set {
                if (value != byteAsciiSwitch) {
                    byteAsciiSwitch = value;
                    OnPropertyChanged("ByteAsciiSwitch");
                }
            }
        }

        [TaskPane("Protocol", "ProtocolToolTip", "NetworkConditions", 2, false, ControlType.ComboBox, new[] {"UDP", "TCP"})]
        public int Protocol {
            get {
                return protocol;
            }
            set {
                if (protocol != value) {
                    protocol = value;
                    OnPropertyChanged("Protocol");
                    UpdateTaskPaneVisibility();
                }
            }
        }
        /*
        [TaskPane("TryConnectCaption", "TryConnectCaptionToolTip", "TCPIPSettings", 4, false, ControlType.CheckBox)]
        public bool TryConnect {
            get { return tryConnect; }
            set {
                if (value != tryConnect) {
                    tryConnect = value;
                    OnPropertyChanged("TryConnect");
                    UpdateTaskPaneVisibility();
                }
            }
        }

        [TaskPane("ConnectIntervallCaption", "ConnectIntervallCaptionToolTip", "TCPIPSettings", 5, false, ControlType.NumericUpDown,
            ValidationType.RangeInteger, 100, 10000)]
        public int ConnectIntervall {
            get {
                return connectIntervall;
            }
            set {
                if (connectIntervall != value) {
                    connectIntervall = value;
                    OnPropertyChanged("ConnectIntervall");
                }
            }
        }*/

        #endregion

        public void Initialize() {}

        internal void UpdateTaskPaneVisibility() {
            if (TaskPaneAttributeChanged == null) {
                return;
            }

            if (Protocol == udpProtocol) {
                var tba = new TaskPaneAttribteContainer("TryConnect", Visibility.Collapsed);
                var tbb = new TaskPaneAttribteContainer("ConnectIntervall", Visibility.Collapsed);
                var tbac = new TaskPaneAttributeChangedEventArgs(tba);
                var tbbc = new TaskPaneAttributeChangedEventArgs(tbb);
                TaskPaneAttributeChanged(this, tbac);
                TaskPaneAttributeChanged(this, tbbc);
                return;
            }
            /*
            //Protocol == tcp
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("TryConnect", Visibility.Visible)));
            if (tryConnect) {
                TaskPaneAttributeChanged(this,
                    new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("ConnectIntervall", Visibility.Visible)));
            } else {
                var tbaa = new TaskPaneAttribteContainer("ConnectIntervall", Visibility.Collapsed);
                var tbbb = new TaskPaneAttributeChangedEventArgs(tbaa);
                TaskPaneAttributeChanged(this, tbbb);
            }*/
        }

        #region Events


        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;
        public event PropertyChangedEventHandler PropertyChanged;


        private void OnPropertyChanged(string propertyName) {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}

