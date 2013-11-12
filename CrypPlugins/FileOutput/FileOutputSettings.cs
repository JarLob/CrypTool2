/*
   Copyright 2008 Thomas Schmid, University of Siegen

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;

namespace FileOutput
{
  public class FileOutputSettings : ISettings
  {
      public string SaveAndRestoreState { get; set; }

      private string targetFilename;
    [TaskPane( "TargetFilenameCaption", "TargetFilenameTooltip", null, 1, false, ControlType.SaveFileDialog, "All Files (*.*)|*.*")]
    public string TargetFilename
    {
      get { return targetFilename; }
      set 
      { 
        targetFilename = value;
          OnPropertyChanged("TargetFilename");
      }
    }

    [TaskPane( "ClearFileNameCaption", "ClearFileNameTooltip", null, 2, false, ControlType.Button)]
    public void ClearFileName()
    {
        TargetFilename = null;
    }

    private bool append = false;
    [ContextMenu("AppendCaption", "AppendTooltip", 0, ContextMenuControlType.CheckBox, null, new string[] { "AppendList1" })]
    [TaskPane("AppendCaption", "AppendTooltip", "AppendGroup", 0, false, ControlType.CheckBox, "", null)]
    public bool Append
    {
        get { return append; }
        set
        {
            if (value != append)
            {
                append = value;
                OnPropertyChanged("Append");
            }
        }
    }

    private int appendBreaks = 1;
    [TaskPane("AppendBreaksCaption", "AppendBreaksTooltip", "AppendGroup", 0, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
    public int AppendBreaks
    {
        get { return this.appendBreaks; }
        set
        {
            if (value != this.appendBreaks)
            {
                this.appendBreaks = value;
                OnPropertyChanged("AppendBreaks");
            }
        }
    }

    #region INotifyPropertyChanged Members

    public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
      public void Initialize()
      {
          
      }

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
