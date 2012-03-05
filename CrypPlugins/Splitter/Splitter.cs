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
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase;
using System.Windows.Controls;
using System.ComponentModel;
using Cryptool.PluginBase.Miscellaneous;

namespace Splitter
{
  [Author("Thomas Schmid", "thomas.schmid@cryptool.org", "Uni Siegen", "http://www.uni-siegen.de")]
  [PluginInfo("Splitter.Properties.Resources", "PluginCaption", "PluginTooltip", "Splitter/DetailedDescription/doc.xml", "Splitter/icon.png")]
  [ComponentCategory(ComponentCategory.ToolsDataflow)]
  public class Splitter : ICrypComponent
  {
    # region private variables
    private SplitterSettings settings = new SplitterSettings();
    private string dictionaryInputString;
    private List<string> listWords = new List<string>();
    # endregion 

    # region public interfacde

    [PropertyInfo(Direction.InputData, "DictionaryInputStringCaption", "DictionaryInputStringTooltip", true)]
    public string DictionaryInputString
    {
      get { return this.dictionaryInputString; }
      set
      {
        // no unequal check here, because new dic input should create new word list.
        dictionaryInputString = value;
        listWords.Clear();
        if (value != null)
          listWords.AddRange(value.Split(settings.DelimiterDictionary[0]));          
        OnPropertyChanged("DictionaryInputString");
      }
    }

    private bool fireNext;
    [PropertyInfo(Direction.InputData, "FireNextCaption", "FireNextTooltip", true)]
    public bool FireNext
    {
      get { return fireNext; }
      set
      {
        fireNext = value;
        if (listWords.Count > 0 && ((value && settings.FireOnValue == 0) || (!value && settings.FireOnValue == 1)))
        {
          OutputString = listWords[0];
          listWords.RemoveAt(0);
        }
        OnPropertyChanged("FireNext");
      }
    }


    private string outputString;
    [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", false)]
    public string OutputString
    {
      get { return this.outputString; }
      set
      {
        outputString = value;
        OnPropertyChanged("OutputString");
      }
    }

    # endregion 


    #region IPlugin Members

#pragma warning disable 67
		public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore

    public event PluginProgressChangedEventHandler OnPluginProgressChanged;
    private void Progress(double value, double max)
    {
        EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
    }
    
    public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
    private void GuiLogMessage(string message, NotificationLevel logLevel)
    {
      EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
    }


    public ISettings Settings
    {
      get { return settings; }
    }

    public UserControl Presentation
    {
      get { return null; }
    }

      public void PreExecution()
    {
    }

    public void Execute()
    {
        Progress(0.0,1.0);

        if (DictionaryInputString == null)
            GuiLogMessage("Got null value for dictionary.", NotificationLevel.Warning);

        Progress(1.0, 1.0);
    }

    public void PostExecution()
    {
    }

      public void Stop()
    {
    }

    public void Initialize()
    {
    }

    public void Dispose()
    {
    }

    #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string name)
    {
      EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
    }

    #endregion
  }
}
