/*                              
  Aditya Deshpande, University of Mannheim
  TREYFER Cipher
*/

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Cryptool.PluginBase;

namespace Cryptool.TREYFER
    {
    public class TREYFERSettings : ISettings
    {
        #region Public TREYFER specific interface

        /// <summary>
        /// We use this delegate to send log messages from the settings class to the TREYFER plugin
        /// </summary>
        public delegate void TREYFERLogMessage(string msg, NotificationLevel loglevel);

        public enum TREYFERMode { Encrypt = 0, Decrypt = 1 };

        /// <summary>
        /// An enumaration for the different modes of dealing with unknown characters
        /// </summary>



        /// <summary>
        /// Feuern, wenn ein neuer Text im Statusbar angezeigt werden soll.
        /// </summary>
        public event TREYFERLogMessage LogMessage;

        #endregion

        #region Private variables and public constructor

        private TREYFERMode selectedAction = TREYFERMode.Encrypt;
        

        public TREYFERSettings()
        {
           
        }

        #endregion

        #region Private methods

        private void OnLogMessage(string msg, NotificationLevel level)
        {
            if (LogMessage != null)
                LogMessage(msg, level);
        }       
       

        #endregion

        #region Algorithm settings properties (visible in the Settings pane)

        [PropertySaveOrder(1)]
        [TaskPane("ActionTPCaption", "ActionTPTooltip", null, 1, false, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
        public TREYFERMode Action
        {
            get
            {
                return this.selectedAction;
            }
            set
            {
                if (value != selectedAction)
                {
                    this.selectedAction = value;
                    OnPropertyChanged("Action");
                }
            }
        }

       #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
