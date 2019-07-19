using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DCAPathFinder.UI.Tutorial1
{
    public class XorTableMapping : INotifyPropertyChanged
    {
        private string _col1;
        private string _col2;
        private string _colResult;

        /// <summary>
        /// Property for column1
        /// </summary>
        public string Col1
        {
            get { return _col1; }
            set
            {
                _col1 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for column2
        /// </summary>
        public string Col2
        {
            get { return _col2; }
            set
            {
                _col2 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for column3
        /// </summary>
        public string ColResult
        {
            get { return _colResult; }
            set
            {
                _colResult = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// OnPropertyChanged-method for INotifyPropertyChanged
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
