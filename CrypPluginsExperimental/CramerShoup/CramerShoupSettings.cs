using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.CramerShoup
{
    public class CramerShoupSettings : ISettings
    {
        #region Private Variables

        private int action = 0;
        private int someParameter = 32;

        #endregion

        public void Initialize()
        {

        }

        #region TaskPane Settings

        /// <summary>
        /// Getter/Setter for the source of the Key Data
        /// </summary>
        [TaskPane("ActionCaption", "ActionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "128", "256", "512" })]
        public int Action
        {
            get { return this.action; }
            set
            {
                if (value != action)
                {
                    this.action = value;

                    OnPropertyChanged("Action");
                }
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}
