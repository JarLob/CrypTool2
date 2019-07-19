using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DCAKeyRecovery.Logic
{
    public class RoundResult : INotifyPropertyChanged
    {
        private int _roundNumber;
        private int _remainingKeys;

        /// <summary>
        /// Property for remaining keys count
        /// </summary>
        public int RemainingKeys
        {
            get { return _remainingKeys; }
            set
            {
                _remainingKeys = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for round number
        /// </summary>
        public int RoundNumber
        {
            get { return _roundNumber; }
            set
            {
                _roundNumber = value;
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
