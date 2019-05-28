using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Interaktionslogik für Cipher4Pres.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("ToyCiphers.Properties.Resources")]
    public partial class Cipher4Pres : UserControl, INotifyPropertyChanged
    {
        private ObservableCollection<TableMapping> sboxData;

        /// <summary>
        /// Constructor
        /// </summary>
        public Cipher4Pres()
        {
            sboxData = new ObservableCollection<TableMapping>();
            sboxData.Add(new TableMapping()
            {
                Direction = ToyCiphers.Properties.Resources.Input,
                ZeroOutput = 0,
                OneOutput = 1,
                TwoOutput = 2,
                ThreeOutput = 3,
                FourOutput = 4,
                FiveOutput = 5,
                SixOutput = 6,
                SevenOutput = 7,
                EightOutput = 8,
                NineOutput = 9,
                TenOutput = 10,
                ElevenOutput = 11,
                TwelveOutput = 12,
                ThirteenOutput = 13,
                FourteenOutput = 14,
                FifteenOutput = 15
            });
            sboxData.Add(new TableMapping()
            {
                Direction = ToyCiphers.Properties.Resources.Output,
                ZeroOutput = 10,
                OneOutput = 0,
                TwoOutput = 9,
                ThreeOutput = 14,
                FourOutput = 6,
                FiveOutput = 3,
                SixOutput = 15,
                SevenOutput = 5,
                EightOutput = 1,
                NineOutput = 13,
                TenOutput = 12,
                ElevenOutput = 7,
                TwelveOutput = 11,
                ThirteenOutput = 4,
                FourteenOutput = 2,
                FifteenOutput = 8
            });

            DataContext = this;
            InitializeComponent();
        }

        /// <summary>
        /// Property for binding the sboxData
        /// </summary>
        public ObservableCollection<TableMapping> SBoxData
        {
            get { return sboxData; }
            set
            {
                sboxData = value;
                OnPropertyChanged("SBoxData");
            }
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
                LabelGrid.Visibility = Visibility.Hidden;
            }
            else
            {
                CipherGrid.Visibility = Visibility.Hidden;
                LabelGrid.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Method to call if data changes
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
