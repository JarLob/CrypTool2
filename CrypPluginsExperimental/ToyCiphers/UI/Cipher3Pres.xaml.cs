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
    /// Interaktionslogik für Cipher3Pres.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("ToyCiphers.Properties.Resources")]
    public partial class Cipher3Pres : UserControl, INotifyPropertyChanged
    {
        private ObservableCollection<TableMapping> sboxData;
        private ObservableCollection<TableMapping> permutationData;

        /// <summary>
        /// Constructor
        /// </summary>
        public Cipher3Pres()
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
                ZeroOutput = 6,
                OneOutput = 4,
                TwoOutput = 12,
                ThreeOutput = 5,
                FourOutput = 0,
                FiveOutput = 7,
                SixOutput = 2,
                SevenOutput = 14,
                EightOutput = 1,
                NineOutput = 15,
                TenOutput = 3,
                ElevenOutput = 13,
                TwelveOutput = 8,
                ThirteenOutput = 10,
                FourteenOutput = 9,
                FifteenOutput = 11
            });

            permutationData = new ObservableCollection<TableMapping>();
            permutationData.Add(new TableMapping()
            {
                Direction = ToyCiphers.Properties.Resources.TablePermutationInput,
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
            permutationData.Add(new TableMapping()
            {
                Direction = ToyCiphers.Properties.Resources.TablePermutationOutput,
                ZeroOutput = 0,
                OneOutput = 4,
                TwoOutput = 8,
                ThreeOutput = 12,
                FourOutput = 1,
                FiveOutput = 5,
                SixOutput = 9,
                SevenOutput = 13,
                EightOutput = 2,
                NineOutput = 6,
                TenOutput = 10,
                ElevenOutput = 14,
                TwelveOutput = 3,
                ThirteenOutput = 7,
                FourteenOutput = 11,
                FifteenOutput = 15
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
        /// Property for binding the permutationData
        /// </summary>
        public ObservableCollection<TableMapping> PermutationData
        {
            get { return permutationData; }
            set
            {
                permutationData = value;
                OnPropertyChanged("PermutationData");
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
