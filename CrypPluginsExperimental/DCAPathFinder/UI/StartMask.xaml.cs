using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace DCAPathFinder.UI
{
    /// <summary>
    /// Interaktionslogik für StartMask.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("DCAPathFinder.Properties.Resources")]
    public partial class StartMask : UserControl, INotifyPropertyChanged
    {
        private string _selectedTutorial;

        /// <summary>
        /// Constructor
        /// </summary>
        public StartMask()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Property for selected tutorial
        /// </summary>
        public string SelectedTutorial
        {
            get { return _selectedTutorial; }
            set
            {
                _selectedTutorial = value;
                OnPropertyChanged();
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
