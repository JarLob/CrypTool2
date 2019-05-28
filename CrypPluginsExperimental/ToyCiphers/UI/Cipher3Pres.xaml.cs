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

namespace ToyCiphers.UI
{
    /// <summary>
    /// Interaktionslogik für Cipher3Pres.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("ToyCiphers.Properties.Resources")]
    public partial class Cipher3Pres : UserControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Cipher3Pres()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Toggles the view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleViewClicked(object sender, RoutedEventArgs e)
        {
            if (CipherGrid.Visibility == Visibility.Hidden)
            {
                CipherGrid.Visibility = Visibility.Visible;
                OutputBlock.Visibility = Visibility.Visible;
                LabelGrid.Visibility = Visibility.Hidden;
                OuterGrid.Width = 400;
                OuterGrid.Height = 1400;
            }
            else
            {
                CipherGrid.Visibility = Visibility.Hidden;
                OutputBlock.Visibility = Visibility.Hidden;
                LabelGrid.Visibility = Visibility.Visible;
                OuterGrid.Width = 390;
                OuterGrid.Height = 350;
            }
        }
    }
}
