/*
   Copyright 2019 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cryptool.Plugins.FEAL
{
    /// <summary>
    /// Interaktionslogik für FEALPresentation.xaml
    /// </summary>
    public partial class FEALPresentation : UserControl
    {
        public FEALPresentation()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Shows the visualization of the encryption of the given block using the given key
        /// </summary>
        /// <param name="block"></param>
        /// <param name="key"></param>
        public void VisualizeEncryptBlock(byte[] block, byte[] key)
        {
            if (Feal4Presentation.IsVisible)
            {
                Feal4Presentation.VisualizeEncryptBlock(block, key);
            }
            else if (Feal8Presentation.IsVisible)
            {                
                Feal8Presentation.VisualizeEncryptBlock(block, key);
            }
        }

        /// <summary>
        /// Shows the visualization of the decryption of the given block using the given key
        /// </summary>
        /// <param name="block"></param>
        /// <param name="key"></param>
        public void VisualizeDecryptBlock(byte[] block, byte[] key)
        {
            if (Feal4Presentation.IsVisible)
            {
                Feal4Presentation.VisualizeDecryptBlock(block, key);
            }
            else if (Feal8Presentation.IsVisible)
            {
                Feal8Presentation.VisualizeDecryptBlock(block, key);
            }
        }

        /// <summary>
        /// Show FEAl4 presentation
        /// </summary>
        public void ShowFEAL4Presentation()
        {
            Feal4Presentation.Visibility = Visibility.Visible;
            Feal8Presentation.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Show FEAl8 presentation
        /// </summary>
        public void ShowFEAL8Presentation()
        {
            Feal4Presentation.Visibility = Visibility.Collapsed;
            Feal8Presentation.Visibility = Visibility.Visible;
        }
    }
}
