using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CrypCloud.Manager.Services;

namespace CrypCloud.Manager.ViewModels
{
    public class ScreenViewModel : INotifyPropertyChanged
    {
        public ScreenNavigator Navigator { get; set; }

        protected TaskFactory UiContext;

        #region viewProperties

        private bool isActive;
        public bool IsActive
        {
            get { return isActive; }
            set
            {
                isActive = value;
                RaisePropertyChanged("IsActive");
            }
        }

        private string errorMessage;
        public string ErrorMessage
        {
            get { return errorMessage; }
            set 
            { 
                errorMessage = value;
                RaisePropertyChanged("ErrorMessage");
            }
        }

        #endregion

        public ScreenViewModel()
        {
            UiContext = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
        }


        protected Action RunInUiContext(Action action)
        {
            return () => UiContext.StartNew(action);
        }

        #region INotifyPropertyChanged

        internal void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}