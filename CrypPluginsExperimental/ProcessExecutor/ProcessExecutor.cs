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
using System.Collections.Concurrent;

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
        //CT2 IPC uses 2 pipes: one for sending and one for receiving messages
        private NamedPipeServerStream _PipeServer = null;
        private NamedPipeServerStream _PipeClient = null;

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

        private ConcurrentQueue<string> _SendingQueue;

        [PropertyInfo(Direction.InputData, "Input1Caption", "Input1Tooltip", false)]
        public string Input1
        {
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    _SendingQueue.Enqueue(value);
                }
            }
        }

        [PropertyInfo(Direction.InputData, "Input2Caption", "Input2Tooltip", false)]
        public string Input2
        {           
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    _SendingQueue.Enqueue(value);
                }
            }
        }

        [PropertyInfo(Direction.InputData, "Input3Caption", "Input3Tooltip", false)]
        public string Input3
        {
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    _SendingQueue.Enqueue(value);
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
            //reset inputs, outputs, and sending queue
            _SendingQueue = new ConcurrentQueue<string>();
            _Input1 = String.Empty;
            _Input2 = String.Empty;
            _Input3 = String.Empty;
            _Output1 = String.Empty;
            _Output2 = String.Empty;
            _Output3 = String.Empty;
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
                GuiLogMessage("Starting", NotificationLevel.Info);
                //Step 0: Set running true :-)
                _Running = true;

                //Step 1: Create process
                _Process = new Process();
                _Process.StartInfo.FileName = @"java";
                _Process.StartInfo.Arguments = @"-jar C:\Users\nilsk\Desktop\ct2ipc_test.jar";
                _Process.StartInfo.CreateNoWindow = true;
                _Process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                //_Process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                _Process.Start();

                //Step 2: Create named pipes with processID of process
                string serverPipeName = "clientToServer" + _Process.Id;
                string clientPipeName = "serverToClient" + _Process.Id;
                _PipeServer = new NamedPipeServerStream(serverPipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                _PipeClient = new NamedPipeServerStream(clientPipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

                //Step 3: Busy wait for external process to connect         
                _PipeServer.BeginWaitForConnection(connectionServerCallback, _PipeServer);
                _PipeClient.BeginWaitForConnection(connectionClientCallback, _PipeClient);

                int time = 0;
                while (!_PipeServer.IsConnected || !_PipeClient.IsConnected)
                {
                    Thread.Sleep(100);
                    time++;
                    if (time == 50)
                    {
                        GuiLogMessage("Process did not connect to both pipes. Stop now.", NotificationLevel.Error);                    
                        return;
                    }                    
                    if (!_Running)
                    {
                        return;
                    }
                }

                //Step 5:
                //create and start sending and receiving thread
                Thread sendingThread = new Thread(SendingMethod);
                Thread receivingThread = new Thread(ReceivingMethod);
                sendingThread.IsBackground = true;
                receivingThread.IsBackground = true;
                sendingThread.Start();
                receivingThread.Start();

                //Step 6: while both pipes are connected, we busy wait
                while (_PipeServer.IsConnected && _PipeClient.IsConnected)
                {
                    Thread.Sleep(100);
                    if (!_Running)
                    {
                        return;
                    }                                     
                }
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
                        GuiLogMessage("Could not close pipe reader for server: " + ex.Message, NotificationLevel.Error);
                    }
                }
                if (_PipeWriter != null)
                {
                    try
                    {
                        _PipeWriter.Close();
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage("Could not close pipe writer for client: " + ex.Message, NotificationLevel.Error);
                    }
                }
                if (_PipeServer != null && _PipeServer.IsConnected)
                {
                    try
                    {
                        _PipeServer.Close();
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage("Could not close named pipe for server: " + ex.Message, NotificationLevel.Error);
                    }
                }
                if (_PipeClient != null && _PipeClient.IsConnected)
                {
                    try
                    {
                        _PipeClient.Close();
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage("Could not close named pipe for client: " + ex.Message, NotificationLevel.Error);
                    }
                }
                //Step 8: wait for process to terminate
                //        If process does not terminate in time, we kill it                
                int time = 0;
                while (!_Process.HasExited)
                {
                    Thread.Sleep(100);
                    time++;
                    if (time == 100)
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
            }            
        }

        /// <summary>
        /// Method for receiving messages from server pipe
        /// </summary>
        /// <param name="obj"></param>
        private void ReceivingMethod(object obj)
        {
            while (_PipeServer.IsConnected && _Running)
            {
                var line = _PipeReader.Read().ToString();
                Output1 += line + " ";
                OnPropertyChanged("Output1");
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Method for sending messages to client pipe
        /// </summary>
        /// <param name="obj"></param>
        private void SendingMethod(object obj)
        {
            while (_PipeClient.IsConnected && _Running)
            {
                if (_SendingQueue.Count > 0)
                {
                    string message = String.Empty;
                    _SendingQueue.TryDequeue(out message);
                    if (!message.Equals(String.Empty))
                    {
                        _PipeWriter.WriteLine(message);
                    }
                }
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Called when client connects to server pipe
        /// </summary>
        /// <param name="asyncResult"></param>
        private void connectionServerCallback(IAsyncResult asyncResult)
        {
            try
            {
                NamedPipeServerStream pipeServer = (NamedPipeServerStream)asyncResult.AsyncState;
                pipeServer.EndWaitForConnection(asyncResult);
                _PipeReader = new StreamReader(pipeServer);                
            }
            catch (Exception ex)
            {
                GuiLogMessage("Exception during establishing of connection to server pipe: " + ex.Message,NotificationLevel.Error);
                _Running = false;
            }
        }

        /// <summary>
        /// Called when client connects to client pipe
        /// </summary>
        /// <param name="asyncResult"></param>
        private void connectionClientCallback(IAsyncResult asyncResult)
        {
            try
            {
                NamedPipeServerStream pipeClient = (NamedPipeServerStream)asyncResult.AsyncState;
                pipeClient.EndWaitForConnection(asyncResult);
                _PipeWriter = new StreamWriter(pipeClient);
            }
            catch (Exception ex)
            {
                GuiLogMessage("Exception during establishing of connection to client Pipe: " + ex.Message, NotificationLevel.Error);
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
