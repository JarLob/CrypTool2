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
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using FileOutput.Helper;
using System.IO;
using System.Windows.Controls;
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Threading;
using Cryptool.PluginBase.Miscellaneous;

namespace FileOutput
{
  [Author("Thomas Schmid", "thomas.schmid@cryptool.org", "Uni Siegen", "http://www.uni-siegen.de")]
  [PluginInfo(false, "FileOutput", "File based output", "", "FileOutput/icon.png")]
  public class FileOutputClass : IOutput
  {
    #region Private variables    
    public FileOutputSettings settings = null;
    private List<CryptoolStream> listCryptoolStreamsOut = new List<CryptoolStream>();
    #endregion Private variables

    #region IInput Members
    public ISettings Settings
    {
      get { return (ISettings)settings; }
      set { settings = (FileOutputSettings)value; }
    }
    #endregion

    private FileOutputPresentation fileOutputPresentation;    

    public string InputFile { get; set; }

    public FileOutputClass()
    {
      settings = new FileOutputSettings();
      fileOutputPresentation = new FileOutputPresentation(this);

      Presentation = fileOutputPresentation;
      fileOutputPresentation.UscHexBoc.OnExceptionOccured += UscHexBoc_OnExceptionOccured;
      fileOutputPresentation.UscHexBoc.OnInformationOccured += UscHexBoc_OnInformationOccured;
    }

    void UscHexBoc_OnInformationOccured(object sender, Exception e)
    {
      if (OnGuiLogNotificationOccured != null) GuiLogMessage(e.Message, NotificationLevel.Info);
    }

    void UscHexBoc_OnExceptionOccured(object sender, Exception e)
    {
      GuiLogMessage(e.Message, NotificationLevel.Error);
    }

    # region Properties
    private CryptoolStream streamInput;

    [PropertyInfo(Direction.InputData, "Stream Input", "Display the input file in HexEditor.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
    public CryptoolStream StreamInput
    {
      get 
      {
        if (File.Exists(InputFile))
        {
          CryptoolStream cs = new CryptoolStream();
          cs.OpenRead(InputFile);
          listCryptoolStreamsOut.Add(cs);
          return cs;
        }
        return null;
      } 
      set
      {
        Progress(0.5, 1.0);
        if (value != null)
        {          
          listCryptoolStreamsOut.Add(value);
          GuiLogMessage("Received ICryptoolStream: " + value.FileName, NotificationLevel.Debug);
          streamInput = value;
          InputFile = value.FileName;
          OnPropertyChanged("StreamInput");

          // If target file was selected we have to copy the input to target. 
          # region copyToTarget
          if (settings.TargetFilename != null)
          {
            listCryptoolStreamsOut.Add(value);
            InputFile = settings.TargetFilename;
            try
            {
              fileOutputPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
              {
                fileOutputPresentation.ClosePresentationFile();
              }, null);

              value.Seek(0, SeekOrigin.Begin);
              FileStream fs = FileHelper.GetFileStream(settings.TargetFilename, FileMode.Create);
              byte[] byteValues = new byte[1024];
              int byteRead;

              int position = 0;
              GuiLogMessage("Start writing to target file now: " + settings.TargetFilename, NotificationLevel.Debug);
              while ((byteRead = value.Read(byteValues, 0, byteValues.Length)) != 0)
              {
                fs.Write(byteValues, 0, byteRead);
                if (OnPluginProgressChanged != null && value.Length > 0 &&
                    (int)(value.Position * 100 / value.Length) > position)
                {
                  position = (int)(value.Position * 100 / value.Length);
                  Progress(value.Position, value.Length);
                }
              }
              fs.Flush();
              fs.Close();

              GuiLogMessage("Finished writing: " + settings.TargetFilename, NotificationLevel.Debug);
            }
            catch (Exception ex)
            {
              GuiLogMessage(ex.Message, NotificationLevel.Error);
              settings.TargetFilename = null;
            }
          }
          # endregion copyToTarget
          value.Close();

          fileOutputPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
          {
            fileOutputPresentation.OpenPresentationFile();
          }, null);
          Progress(1.0, 1.0);
        }
        else
        {
          GuiLogMessage("Received null value for ICryptoolStream.", NotificationLevel.Warning);
        }        
      }
    }
    #endregion

    #region IPlugin Members
    public event StatusChangedEventHandler OnPluginStatusChanged;
    public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
    public event PluginProgressChangedEventHandler OnPluginProgressChanged;

    public UserControl Presentation { get; private set; }

    public UserControl QuickWatchPresentation
    {
      get { return null; }
    }

    public void Initialize()
    {
      if (settings.SaveAndRestoreState != string.Empty && File.Exists(settings.SaveAndRestoreState))
      {
        this.InputFile = settings.SaveAndRestoreState;
        fileOutputPresentation.OpenPresentationFile();
      }
    }

    /// <summary>
    /// Close open file and save open filename to settings. Will be called when saving
    /// workspace or when deleting an element instance from workspace.
    /// </summary>
    public void Dispose()
    {
      foreach (CryptoolStream cryptoolStream in listCryptoolStreamsOut)
      {
        cryptoolStream.Close();
      }
      listCryptoolStreamsOut.Clear();

      DispatcherHelper.ExecuteMethod(fileOutputPresentation.Dispatcher,
        fileOutputPresentation, "OpenPresentationFile", null);
    }

    public void Stop()
    {

    }

    public void PreExecution()
    {
      InputFile = null;
      DispatcherHelper.ExecuteMethod(fileOutputPresentation.Dispatcher,
        fileOutputPresentation, "ClosePresentationFile", null);
      if (settings.TargetFilename == null || settings.TargetFilename == string.Empty)
      {
        GuiLogMessage("You have to select a target filename before using this plugin as output.", NotificationLevel.Error);
      }
    }

    public void PostExecution()
    {
      Dispose();
    }

    #endregion

    #region INotifyPropertyChanged Members

    public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged(string name)
    {
      EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
    }
    #endregion
    public void UpdateQuickWatch()
    {
      OnPropertyChanged("StreamInput");
    }

    private void Progress(double value, double max)
    {
      EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
    }

    private void GuiLogMessage(string message, NotificationLevel logLevel)
    {
      EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
    }

    #region IPlugin Members

    public void Execute()
    {
      
    }

    public void Pause()
    {
      
    }

    #endregion
  }
}
