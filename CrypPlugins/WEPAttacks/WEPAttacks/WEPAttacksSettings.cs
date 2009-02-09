﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Cryptool.PluginBase;

namespace Cryptool.WEPAttacks
{
    /// <summary>
    /// Some settings for the <see cref="WEPAttacks"/> plugin.
    /// </summary>
    public class WEPAttacksSettings : ISettings
    {
        #region Private variables

        /// <summary>
        /// Kind of attack. 0 ==> FMS, 1 ==> KoreK, 2 ==> PTW
        /// </summary>
        private int action = 0;
        private bool fileOrNot = false;
        private bool hasChanges = false;
        private string dataSource = string.Empty;

        /// <summary>
        /// Action. 0 => FMS, 1 => KoreK, 2 => PTW
        /// </summary>
        [ContextMenu("Kind of attack",
            "Kind of attack",
            1,
            DisplayLevel.Beginner,
            ContextMenuControlType.ComboBox,
            null,
            new string[] { "\"FMS\"", "\"KoreK\"", "\"PTW\"" })]
        [TaskPane("Kind of attack",
            "Kind of attack",
            "",
            1,
            false,
            DisplayLevel.Beginner,
            ControlType.ComboBox,
            new string[] { "\"FMS\"", "\"KoreK\"", "\"PTW\"" })]
        public int Action
        {
            get { return this.action; }
            set
            {
                if ((int)value != action)
                {
                    hasChanges = true;
                }
                this.action = (int)value;
                OnPropertyChanged("Action");
            }
        }

        /// <summary>
        /// true => source is data, false => source is another plugin (NOT "File Input"!!!)
        /// </summary>
        [TaskPane("Data comes from file",
            "Does the data come from file or not?",
            "",
            2,
            false,
            DisplayLevel.Beginner,
            ControlType.CheckBox,
            new string[] { "Data comes from file" })]
        public bool FileOrNot
        {
            get
            {
                return this.fileOrNot;
            }
            set
            {
                if ((bool)value != fileOrNot)
                {
                    hasChanges = true;
                }
                this.fileOrNot = (bool)value;
                OnPropertyChanged("FileOrNot");
            }
        }

        /// <summary>
        /// Indicates whether data comes from file or from another plugin (most propably the "Internet frame generator" plugin).
        /// Needed to react if attack was not successful.
        /// </summary>
         
        // Radiobuttons are not implemented yet, so I coment them out (11-25-2008)

        /*[TaskPane("Dealing with end of given data",
            "Dealing with end of given data",
            "groupRadiobutton",
            3,
            false,
            DisplayLevel.Beginner,
            ControlType.RadioButton,
            new string[]{"Data comes from file (finish, if attack is not successful)",
                "Data comes from IFG (wait for further package, if attack is not successful yet)"})]
        public string DataSource
        {
            get
            {
                return this.dataSource;
            }
            set
            {
                if ((string)value != dataSource)
                {
                    hasChanges = true;
                }
                this.dataSource = (string)value;
                OnPropertyChanged("DataSource");
            }
        }*/

        #endregion

        #region ISettings Member

        public event StatusChangedEventHandler OnPluginStatusChanged;
        private void ChangePluginIncon(int Icon)
        {
            if (OnPluginStatusChanged != null)
            {
                OnPluginStatusChanged(null, new StatusEventArgs(Icon));
            }
        }

        public bool HasChanges
        {
            get { return this.hasChanges; }
            set { this.hasChanges = value; }
        }

        #endregion

        #region INotifyPropertyChanged Member

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
