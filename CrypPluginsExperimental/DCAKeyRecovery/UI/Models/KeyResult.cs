using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DCAKeyRecovery.UI.Models
{
    public class KeyResult : INotifyPropertyChanged
    {
        private int _key;
        private string _binaryKey;
        private double _probability;
        private int _hitCount;

        /// <summary>
        /// Property for _key
        /// </summary>
        public int Key
        {
            get { return _key; }
            set
            {
                _key = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _binaryKey
        /// </summary>
        public string BinaryKey
        {
            get { return _binaryKey; }
            set
            {
                _binaryKey = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _probability
        /// </summary>
        public double Probability
        {
            get { return _probability; }
            set
            {
                _probability = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _hitCount
        /// </summary>
        public int HitCount
        {
            get { return _hitCount; }
            set
            {
                _hitCount = value;
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
            if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
