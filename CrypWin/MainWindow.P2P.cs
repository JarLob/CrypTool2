/*
   Copyright 2008-2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Cryptool.CrypWin
{
    public partial class MainWindow
    {
        private bool _reconnect = false;
        private bool _reconnecting = false;  

        public static readonly DependencyProperty P2PButtonVisibilityProperty = 
            DependencyProperty.Register("P2PButtonVisibility",typeof(Visibility),typeof(MainWindow),
                new FrameworkPropertyMetadata(Visibility.Visible, FrameworkPropertyMetadataOptions.AffectsRender, null));

        [TypeConverter(typeof(Visibility))]
        public Visibility P2PButtonVisibility
        {
            get
            {
                return (Visibility)GetValue(P2PButtonVisibilityProperty);
            }
            set
            {
                SetValue(P2PButtonVisibilityProperty, value);
            }
        }

        //TODO @ckonze remove
        public bool P2PIconImageRotating{get;set;}

        private bool p2PIconImageGray = false;
        public bool P2PIconImageGray
        {
            get
            {
                return p2PIconImageGray;
            }
            set
            {
                p2PIconImageGray = value;
                UpdateIcons();
            }
        }

        private void UpdateIcons()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback) delegate
            {
                if (p2PIconImageGray)
                {
                    SetP2PImages("p2pconnectIcon", Properties.Resources.cryptool);
                }
                else
                {
                    SetP2PImages("p2pdisconnectIcon", Properties.Resources.cryptoolP2P);
                }
            }, null);
        }

        private void SetP2PImages(string iconSource, Icon notificationIcon)
        {
            P2PIconImageBig.Source = (ImageSource) FindResource(iconSource);
            P2PIconImageSmall.Source = (ImageSource) FindResource(iconSource);

            try
            {
                notifyIcon.Icon = notificationIcon;
            } catch {}
        }
    }
}
