/*
   Copyright 2017 Nils Kopal, Applied Information Security, Uni Kassel
   https://www.uni-kassel.de/eecs/fachgebiete/ais/mitarbeiter/nils-kopal-m-sc.html

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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Threading;
using System.Collections.Generic;
using Cryptool.PluginBase.Attributes;
using System.IO.Pipes;
using System.Diagnostics;

namespace Cryptool.ProcessExecutor
{
    public delegate void PluginProgress(double current, double maximum);
    public delegate void UpdateOutput(String keyString, String plaintextString);

    [Author("Nils Kopal", "Nils.Kopal@Uni-Kassel.de", "Uni Kassel", "https://www.ais.uni-kassel.de")]
    [PluginInfo("Cryptool.ProcessExecutor.Properties.Resources",
    "PluginCaption", "PluginTooltip", "", "ProcessExecutor/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class ProcessExecutor : ICrypComponent
    {

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PropertyChangedEventHandler PropertyChanged;
        
        private bool _Running = false;
        private NamedPipeServerStream _PipeServer = null;
        private StreamReader _PipeReader = null;
        private StreamWriter _PipeWriter = null;
        private Process _Process = null;
        private int _ProcessID = -1;

        private string _Input1 = null;
        private string _Input2 = null;
        private string _Input3 = null;

        private string _Output1 = null;
        private string _Output2 = null;
        private string _Output3 = null;

        [PropertyInfo(Direction.InputData, "Input1Caption", "Input1Tooltip", false)]
        public string Input1
        {
            get { return _Input1; }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    _Input1 = value;
                }
            }
        }

        [PropertyInfo(Direction.InputData, "Input2Caption", "Input2Tooltip", false)]
        public string Input2
        {
            get { return _Input2; }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    _Input2 = value;
                }
            }
        }

        [PropertyInfo(Direction.InputData, "Input3Caption", "Input3Tooltip", false)]
        public string Input3
        {
            get { return _Input3; }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    _Input3 = value;
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "Output1Caption", "Output1Tooltip", false)]
        public string Output1
        {
            get { return _Output1; }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    _Output1 = value;
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "Output2Caption", "Output2Tooltip", false)]
        public string Output2
        {
            get { return _Output2; }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    _Output2 = value;
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "Output3Caption", "Output3Tooltip", false)]
        public string Output3
        {
            get { return _Output3; }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    _Output3 = value;
                }
            }
        }

        public void PreExecution()
        {
            
        }

        public void PostExecution()
        {
            
        }          

        public ISettings Settings
        {
            get { return null; }
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public void Execute()
        {            
            try
            {
                //Step 0: Set running true :-)
                _Running = true;

                //Step 1: Create process and get processID
                _Process = new Process();
                _Process.StartInfo.FileName = "cmd.exe";
                _Process.StartInfo.CreateNoWindow = true;
                _Process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;                                
                _Process.Start();
                string pipeName = "CrypTool2_Pipe_" + _Process.Id;

                //Step 2: Create named pipe with processID
                _PipeServer = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

                //Step 3: Wait for external process to connect            
                _PipeServer.BeginWaitForConnection(connectionCallback, _PipeServer);

                int timer = 0;
                while (!_PipeServer.IsConnected)
                {
                    Thread.Sleep(100);
                    timer++;
                    if (timer == 10)
                    {
                        GuiLogMessage("Process did not connect to pipe. Stop now.", NotificationLevel.Warning);                        
                        return;
                    }
                    
                    if (!_Running)
                    {
                        return;
                    }
                }                
                //Step 4: Send inputs


                //Step 5: Receive outputs
                while (!_PipeServer.IsConnected)
                {
                    Thread.Sleep(100);
                    if (!_Running)
                    {
                        return;
                    }
                }

                //Step 6: Send stop message                 
            }
            finally
            {
                _Running = false;

                //Step 7: Close pipe and input/output streams
                if (_PipeReader != null)
                {
                    try
                    {
                        _PipeReader.Close();
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage("Could not close pipe reader: " + ex.Message, NotificationLevel.Error);
                    }
                    _PipeReader = null;
                }
                if (_PipeWriter != null)
                {
                    try
                    {
                        _PipeWriter.Close();
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage("Could not close pipe writer: " + ex.Message, NotificationLevel.Error);
                    }
                    _PipeWriter = null;
                }
                if (_PipeServer != null && _PipeServer.IsConnected)
                {
                    try
                    {
                        _PipeServer.Close();
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage("Could not close named pipe: " + ex.Message, NotificationLevel.Error);
                    }
                    _PipeServer = null;
                }
                //Step 8: wait for process to terminate
                //        If process does not terminate in time, we kill it                
                int time = 0;
                while (!_Process.HasExited)
                {
                    Thread.Sleep(100);
                    time++;
                    if (time == 10)
                    {
                        try
                        {
                            GuiLogMessage("Process did not terminate in time. Kill it now.", NotificationLevel.Warning);
                            _Process.Kill();
                            break;
                        }
                        catch (Exception ex)
                        {
                            GuiLogMessage("Could not kill process: " + ex.Message, NotificationLevel.Error);
                        }
                    }                    
                }
                _Process = null;
            }            
        }

        private void connectionCallback(IAsyncResult asyncResult)
        {
            try
            {
                NamedPipeServerStream pipeServer = (NamedPipeServerStream)asyncResult.AsyncState;
                pipeServer.EndWaitForConnection(asyncResult);
                _PipeReader = new StreamReader(pipeServer);
                _PipeWriter = new StreamWriter(pipeServer);
            }
            catch (Exception ex)
            {
                GuiLogMessage("Exception during establishing of connection: " + ex.Message,NotificationLevel.Error);
                _Running = false;
            }
        }

        public void Stop()
        {
            //send stop message                        
            _Running = false;
        }

        public void Initialize()
        {
            
        }        

        public void Dispose()
        {
            
        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

    }
}
