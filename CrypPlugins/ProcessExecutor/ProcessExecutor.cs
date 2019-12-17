/*
   Copyright 2018 Nils Kopal, Henner Heck, Applied Information Security, Uni Kassel
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
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Controls;
using System.Threading;
using System.IO.Pipes;
using System.Diagnostics;
using System.Collections.Concurrent;
using Cryptool.Plugins.Ipc.Messages;
using Google.Protobuf;
using System.Windows.Threading;
using Cryptool.CrypAnalysisViewControl;

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

        private Process _Process = null;      

        private string _Output1 = null;
        private string _Output2 = null;
        private string _Output3 = null;

        private ConcurrentQueue<OutgoingData> _SendingQueue;
        private ProcessExecutorSettings _settings = new ProcessExecutorSettings();
        private AssignmentPresentation _presentation = new AssignmentPresentation();
        private DateTime _startTime;
        private bool _alreadyExecuted = false;

        [PropertyInfo(Direction.InputData, "Input1Caption", "Input1Tooltip", false)]
        public string Input1
        {
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    _SendingQueue.Enqueue(new OutgoingData() { outputId = 1, value = value });
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
                    _SendingQueue.Enqueue(new OutgoingData() { outputId = 2, value = value });
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
                    _SendingQueue.Enqueue(new OutgoingData() { outputId = 3, value = value });
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
            _SendingQueue = new ConcurrentQueue<OutgoingData>();
            _Output1 = String.Empty;
            _Output2 = String.Empty;
            _Output3 = String.Empty;
            _alreadyExecuted = false;
        }

        public void PostExecution()
        {
            
        }          

        public ISettings Settings
        {
            get { return _settings; }
        }

        public UserControl Presentation
        {
            get { return _presentation; }
        }

        public void Execute()
        {       
            // some checks:

            if (_alreadyExecuted)
            {
                GuiLogMessage(Properties.Resources.AlreadyExecuted, NotificationLevel.Error);
                return;
            }
            _alreadyExecuted = true;

            if (String.IsNullOrEmpty(_settings.Filename))
            {
                GuiLogMessage(Properties.Resources.NoFilenameOrProgramGiven, NotificationLevel.Error);
                return;
            }

            try
            {
                //Step 0: Set running true :-)
                _Running = true;

                //set start time in presentation; remove elapsedTime and endTime
                _startTime = DateTime.Now;
                _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        _presentation.StartTime.Value = _startTime.ToString();
                        _presentation.EndTime.Value = string.Empty;
                        _presentation.ElapsedTime.Value = string.Empty;
                    }
                    catch (Exception)
                    {
                        //wtf?
                    }
                }, null);

                //Step 1: Create process                
                _Process = new Process();
                _Process.StartInfo.FileName = _settings.Filename;
                _Process.StartInfo.Arguments = _settings.Arguments;
                _Process.StartInfo.CreateNoWindow = true;
                if (_settings.ShowWindow)
                {
                    _Process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                }
                else
                {
                    _Process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                }

                if (!_Process.Start())
                {
                    GuiLogMessage("Could not start process. It returned false.", NotificationLevel.Error);
                    return;
                }                

                //Step 2: Create named pipes with processID of process
                string serverPipeName = "clientToServer" + _Process.Id;
                string clientPipeName = "serverToClient" + _Process.Id;
                _PipeServer = new NamedPipeServerStream(serverPipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                _PipeClient = new NamedPipeServerStream(clientPipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

                //Step 3: Busy wait for external process to connect         
                _PipeServer.BeginWaitForConnection(connectionServerCallback, _PipeServer);
                _PipeClient.BeginWaitForConnection(connectionClientCallback, _PipeClient);

                int time = 0;
                while (!_Process.HasExited && (!_PipeServer.IsConnected || !_PipeClient.IsConnected))
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

                if (_Process.HasExited)
                {
                    return;
                }

                //Step 5:
                //Send settings and
                //create and start sending and receiving thread                
                _SendingQueue.Enqueue(new OutgoingData() { outputId = 100, value = "" + (_settings.Threads + 1)}); //index starts at 0; thus +1
                _SendingQueue.Enqueue(new OutgoingData() { outputId = 200, value = "" + _settings.Cycles });
                _SendingQueue.Enqueue(new OutgoingData() { outputId = 300, value = "" + _settings.ResourceDirectory });
                
                Thread receivingThread = new Thread(ReceivingMethod);
                receivingThread.IsBackground = true;
                receivingThread.Start();

                Thread sendingThread = new Thread(SendingMethod);
                sendingThread.IsBackground = true;
                sendingThread.Start();

                SendCt2HelloMessage();

                DateTime lastUpdateTime = DateTime.Now;
                //Step 6: while both pipes are connected, we busy wait
                while (_PipeServer.IsConnected && _PipeClient.IsConnected)
                {
                    Thread.Sleep(100);
                    if (!_Running)
                    {
                        return;
                    }
                    if (DateTime.Now >= lastUpdateTime.AddSeconds(1))
                    {
                        lastUpdateTime = DateTime.Now;
                        //set elapsed time in presentation
                        _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            try
                            {              
                                var elapsedTime =  (DateTime.Now - _startTime);
                                _presentation.ElapsedTime.Value = new TimeSpan(elapsedTime.Hours, elapsedTime.Minutes, elapsedTime.Seconds).ToString();
                            }
                            catch (Exception)
                            {
                                //wtf?
                            }
                        }, null);
                    }
                }
            }
            finally
            {
                SendCt2ShutdownMessage();
                _Running = false;
                //Step 7: Close pipe and input/output streams
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
                    Thread.Sleep(10);
                    time++;
                    if (time == 200)
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

                if (_Process.HasExited)
                {
                    GuiLogMessage(String.Format("Process exited with exit code: {0}", _Process.ExitCode), _Process.ExitCode != 0 ? NotificationLevel.Error : NotificationLevel.Info);
                }

                //set end time in presentation
                _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        _presentation.EndTime.Value = DateTime.Now.ToString();
                        var elapsedTime = (DateTime.Now - _startTime);
                        _presentation.ElapsedTime.Value = new TimeSpan(elapsedTime.Hours, elapsedTime.Minutes, elapsedTime.Seconds).ToString();
                    }
                    catch (Exception)
                    {
                        //wtf?
                    }
                }, null);

            }            
        }

        /// <summary>
        /// Sends a Ct2HelloMessage
        /// </summary>
        private void SendCt2HelloMessage()
        {
            if (_PipeClient.IsConnected)
            {
                try
                {
                    Ct2Hello ct2Hello = new Ct2Hello();
                    ct2Hello.ProgramName = "CrypTool 2";
                    ct2Hello.ProgramVersion = "2.1.";
                    Ct2IpcMessage message = new Ct2IpcMessage();
                    message.Body = ct2Hello.ToByteString();
                    message.MessageType = 1;
                    message.WriteDelimitedTo(_PipeClient);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(String.Format("Can not send Ct2Hello message: {0}", ex.Message), NotificationLevel.Error);
                }
            }
        }

        /// <summary>
        /// Sends a Ct2ShutdownMessage
        /// </summary>
        private void SendCt2ShutdownMessage()
        {
            if (_PipeClient != null && _PipeClient.IsConnected)
            {
                try
                {
                    Ct2Shutdown ct2Shutdown = new Ct2Shutdown();
                    ct2Shutdown.Reason = "Execute method of ProcessExecutor is terminating";                    
                    Ct2IpcMessage message = new Ct2IpcMessage();
                    message.Body = ct2Shutdown.ToByteString();
                    message.MessageType = 2;
                    message.WriteDelimitedTo(_PipeClient);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(String.Format("Can not send Ct2Shutdown message: {0}", ex.Message), NotificationLevel.Error);
                }
            }
        }

        /// <summary>
        /// Method that sends input data from the queue to the application
        /// </summary>
        private void SendingMethod()
        {
            while (_PipeClient.IsConnected && _Running)
            {
                if (_SendingQueue.Count > 0)
                {
                    try
                    {
                        OutgoingData outgoingData = null;
                        _SendingQueue.TryDequeue(out outgoingData);
                        if (outgoingData != null)
                        {
                            Ct2Values ct2Values = new Ct2Values();
                            ct2Values.PinId.Add(outgoingData.outputId);
                            ct2Values.Value.Add(outgoingData.value);
                            Ct2IpcMessage message = new Ct2IpcMessage();
                            message.Body = ct2Values.ToByteString();
                            message.MessageType = 3;
                            message.WriteDelimitedTo(_PipeClient);
                        }
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage(String.Format("Exception while sending data: {0}", ex.Message), NotificationLevel.Error);
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
                try
                {
                    Ct2IpcMessage message = Ct2IpcMessage.Parser.ParseDelimitedFrom(_PipeServer);
                    switch (message.MessageType)
                    {
                        case 1: // Ct2Hello
                            var ct2Hello = Ct2Hello.Parser.ParseFrom(message.Body.ToByteArray());
                            //HandleCt2HelloMessage(ct2Hello);
                            break;

                        case 2: //Ct2Shutdown
                            var ct2Shutdown = Ct2Shutdown.Parser.ParseFrom(message.Body.ToByteArray());
                            //HandleCt2ShutdownMessage(ct2Shutdown);
                            break;

                        case 3: //Ct2Values
                            var ct2Values = Ct2Values.Parser.ParseFrom(message.Body.ToByteArray());
                            HandleCt2ValuesMessage(ct2Values);
                            break;

                        case 4: //Ct2LogEntry
                            var ct2LogEntry = Ct2LogEntry.Parser.ParseFrom(message.Body.ToByteArray());
                            HandleCt2LogEntryMessage(ct2LogEntry);
                            break;

                        case 5: //Ct2Progress
                            var ct2Progress = Ct2Progress.Parser.ParseFrom(message.Body.ToByteArray());
                            HandleCt2ProgressMessage(ct2Progress);
                            break;

                        case 6: //Ct2Goodbye
                            var ct2Goodbye = Ct2Goodbye.Parser.ParseFrom(message.Body.ToByteArray());
                            HandleCt2GoodbyeMessage(ct2Goodbye);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    if (_Running)
                    {
                        GuiLogMessage(String.Format("Exception occured during receving and handling of message: {0}", ex.Message), NotificationLevel.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Handles an incoming Ct2Goodbye message
        /// </summary>
        /// <param name="ct2Goodbye"></param>
        private void HandleCt2GoodbyeMessage(Ct2Goodbye ct2Goodbye)
        {
            if (ct2Goodbye.ExitCode != 0)
            {
                GuiLogMessage(string.Format("Process terminates now with return code {0}. Reason: {1}", ct2Goodbye.ExitCode, ct2Goodbye.ExitMessage), NotificationLevel.Error);
            }                        
            _Running = false;
        }       

        /// <summary>
        /// Handles an incoming Ct2Values message
        /// </summary>
        /// <param name="ct2Values"></param>
        private void HandleCt2ValuesMessage(Ct2Values ct2Values)
        {
            for (int i = 0; i < ct2Values.PinId.Count; i++)
            {
                int id = ct2Values.PinId[i];
                string value = ct2Values.Value[i];
                switch (id)
                {
                    case 1:
                        Output1 = value;
                        OnPropertyChanged("Output1");
                        break;
                    case 2:
                        Output2 = value;
                        OnPropertyChanged("Output2");
                        break;
                    case 3:
                        Output3 = value;
                        OnPropertyChanged("Output3");
                        break;
                    case 1000:
                        HandleIncomingBestList(value);
                        break;
                    default:
                        GuiLogMessage(String.Format("Received a value for an output that does not exist: {0}", id), NotificationLevel.Warning);
                        break;
                }
            }
        }

        /// <summary>
        /// Handles incoming best list entries
        /// </summary>
        /// <param name="value"></param>
        private void HandleIncomingBestList(string value)
        {
            // it is allowed to send an empty string as well as a -
            // if we receive that, we just ignore it
            if (String.IsNullOrEmpty(value) || value.Equals("-"))
            {
                return;
            }

            try
            {                
                _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        _presentation.BestList.Clear();
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage(String.Format("Error occured while clearing best list: {0}", ex.Message), NotificationLevel.Error);
                    }
                }, null);
                string[] lines = value.Trim().Split('\n', '\r');
                foreach (string line in lines)
                {
                    string[] values = line.Trim().Split(';');
                    if (values.Length != 5)
                    {
                        GuiLogMessage(String.Format("Received invalid best list entry: {0}", line), NotificationLevel.Warning);
                        continue;
                    }
                    ResultEntry resultEntry = new ResultEntry();
                    resultEntry.Ranking = values[0];
                    resultEntry.Value = values[1];
                    resultEntry.Key = values[2];
                    resultEntry.Text = values[3];
                    resultEntry.Info = values[4];
                    _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        try
                        {
                            _presentation.BestList.Add(resultEntry);
                        }
                        catch (Exception ex)
                        {
                            GuiLogMessage(String.Format("Error occured while adding new entry to best list: {0}", ex.Message), NotificationLevel.Error);
                        }
                    }, null);
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(String.Format("Error occured while handling new best list: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Handles a Ct2LogEntry message by showing the appropriate log entry
        /// </summary>
        /// <param name="ct2LogEntry"></param>
        private void HandleCt2LogEntryMessage(Ct2LogEntry ct2LogEntry)
        {
            switch (ct2LogEntry.LogLevel)
            {
                case Ct2LogEntry.Types.LogLevel.Ct2Debug:
                    GuiLogMessage(ct2LogEntry.Entry, NotificationLevel.Debug);
                    break;
                case Ct2LogEntry.Types.LogLevel.Ct2Info:
                    GuiLogMessage(ct2LogEntry.Entry, NotificationLevel.Info);
                    break;
                case Ct2LogEntry.Types.LogLevel.Ct2Warning:
                    GuiLogMessage(ct2LogEntry.Entry, NotificationLevel.Warning);
                    break;
                case Ct2LogEntry.Types.LogLevel.Ct2Error:
                    GuiLogMessage(ct2LogEntry.Entry, NotificationLevel.Error);
                    break;
                case Ct2LogEntry.Types.LogLevel.Ct2Balloon:
                    GuiLogMessage(ct2LogEntry.Entry, NotificationLevel.Balloon);
                    break;
            }
        }

        /// <summary>
        /// Handles a Ct2Progress by showing its progress to the user
        /// </summary>
        /// <param name="ct2Progress"></param>
        private void HandleCt2ProgressMessage(Ct2Progress ct2Progress)
        {
            OnProgressChanged(ct2Progress.CurrentValue, ct2Progress.MaxValue);
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
            }
            catch (Exception ex)
            {
                GuiLogMessage("Exception during establishing of connection to client Pipe: " + ex.Message, NotificationLevel.Error);
                _Running = false;
            }
        }

        public void Stop()
        {
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

        private void OnProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

    }

    /// <summary>
    /// Wrapper class for wrapping output data and output connector id
    /// </summary>
    class OutgoingData
    {
        public int outputId;
        public string value;
    }

    public class ResultEntry : ICrypAnalysisResultListEntry, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string ranking;
        public string Ranking
        {
            get => ranking;
            set
            {
                ranking = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Ranking)));
            }
        }

        public string Value { get; set; }
        public string Key { get; set; }
        public string Text { get; set; }
        public string Info { get; set; }


        public string ClipboardValue => Value.ToString();
        public string ClipboardKey => Key;
        public string ClipboardText => Text;
        public string ClipboardEntry =>
            "Rank: " + Ranking + Environment.NewLine +
            "Value: " + Value + Environment.NewLine +
            "Key: " + Key + Environment.NewLine +
            "Text: " + Text + Environment.NewLine +
            "Info: " + Info;
    }
}
