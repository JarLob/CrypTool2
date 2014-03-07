﻿/*                              
   Copyright 2010 Nils Kopal

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
using System.Windows.Threading;
using Cryptool.PluginBase;
using System.Threading;
using System.Windows.Controls;
using Cryptool.PluginBase.IO;
using WorkspaceManager.Execution;
using System.Reflection;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Text;
using WorkspaceManagerModel.Properties;

namespace WorkspaceManager.Model
{

    /// <summary>
    /// Class to represent and wrap a IPlugin in our model graph
    /// </summary>
    [Serializable]
    public class PluginModel : VisualElementModel
    {
        internal const int MaxStrStreamConversionLength = 1048576; //1 MB

        internal PluginModel()
        {
            ViewState = PluginViewState.Default;
            this.InputConnectors = new List<ConnectorModel>();
            this.OutputConnectors = new List<ConnectorModel>();

        }

        #region private members

        [NonSerialized]
        private ICrypComponent plugin;
        private int imageIndex = 0;
        [NonSerialized]
        private PluginModelState state = PluginModelState.Normal;
        private string PluginTypeName = null;
        private string PluginTypeAssemblyName = null;
        [NonSerialized] 
        internal bool SettingesHaveChanges = false;
        
        internal void OnConnectorPlugstateChanged(ConnectorModel connector, PlugState state)
        {
            if(ConnectorPlugstateChanged != null)
            {
                ConnectorPlugstateChanged.Invoke(this,new ConnectorPlugstateChangedEventArgs(state,connector));
            }
        }
        
        #endregion

        #region public members

        [field: NonSerialized]
        public event EventHandler<ConnectorPlugstateChangedEventArgs> ConnectorPlugstateChanged;

        /// <summary>
        /// State of the Plugin
        /// </summary>
        public PluginModelState State
        {
            get { return state; }
            set { state = value; }
        }

        /// <summary>
        /// All ingoing connectors of this PluginModel
        /// </summary>
        internal List<ConnectorModel> InputConnectors = null;

        /// <summary>
        /// Get all ingoing connectors of this PluginModel
        /// </summary>
        public ReadOnlyCollection<ConnectorModel> GetInputConnectors()
        {
            return InputConnectors.AsReadOnly();
        }

        /// <summary>
        /// All outgoing connectors of this PluginModel
        /// </summary>
        internal List<ConnectorModel> OutputConnectors = null;

        /// <summary>
        /// Get all outgoing connectors of this PluginModel
        /// </summary>
        public ReadOnlyCollection<ConnectorModel> GetOutputConnectors()
        {
            return OutputConnectors.AsReadOnly();
        }

        /// <summary>
        /// The wrapped IPlugin of this PluginModel
        /// if there is currently no plugin instance it
        /// will automatically create one. Otherwise
        /// this acts as singleton and returns the created
        /// instance
        /// </summary>        
        public ICrypComponent Plugin
        {
            get
            {
                if (plugin == null && PluginType != null)
                {
                    plugin = PluginType.CreateComponentInstance();
                }
                return plugin;
            }

            private set
            {
                plugin = value;
            }
        }

        /// <summary>
        /// The Type of the Wrapped IPlugin of this PluginModel
        /// Depending on this the Plugin of this PluginModel will be instanciated
        /// </summary>
        public Type PluginType
        {
            get
            {
                if (this.PluginTypeName != null)
                {
                    Assembly assembly = Assembly.Load(PluginTypeAssemblyName);
                    Type t = assembly.GetType(PluginTypeName);
                    return t;
                }
                else
                {
                    return null;
                }
            }
            internal set
            {
                this.PluginTypeName = value.FullName;
                this.PluginTypeAssemblyName = value.Assembly.GetName().Name;
            }
        }

        /// <summary>
        /// Should this plugin may be startet again when it
        /// is startable?
        /// </summary>
        public bool RepeatStart;

        /// <summary>
        /// Not used at all anymore
        /// </summary>
        [Obsolete("Startable flag is not used anymore")]
        public bool Startable;

        /// <summary>
        /// Is the Plugin actually minimized?
        /// </summary>
        public bool Minimized { get; internal set; }

        /// <summary>
        /// The execution state of the progress of the wrapped plugin 
        /// </summary>
        public double PercentageFinished { get; internal set; }

        /// <summary>
        /// The WorkspaceModel of this PluginModel
        /// </summary>
        public WorkspaceModel WorkspaceModel { get; internal set; }

        /// <summary>
        /// Current View state
        /// </summary>
        public PluginViewState ViewState { get; set; }

        /// <summary>
        /// Generates all Connectors of this Plugin.
        /// </summary>
        internal void generateConnectors()
        {
            InputConnectors.Clear();
            OutputConnectors.Clear();

            if (Plugin != null)
            {
                foreach (PropertyInfoAttribute propertyInfoAttribute in Plugin.GetProperties())
                {
                    generateConnector(propertyInfoAttribute);
                }                
            }
        }

        /// <summary>
        /// Generate a single Connector of this Plugin.
        /// </summary>
        /// <param name="propertyInfoAttribute"></param>
        internal void generateConnector(PropertyInfoAttribute propertyInfoAttribute)
        {
            if (propertyInfoAttribute.Direction.Equals(Direction.InputData))
            {
                ConnectorModel connectorModel = new ConnectorModel();

                connectorModel.Caption = propertyInfoAttribute.Caption;
                connectorModel.ConnectorType = propertyInfoAttribute.PropertyInfo.PropertyType;
                connectorModel.WorkspaceModel = WorkspaceModel;
                connectorModel.PluginModel = this;
                connectorModel.IsMandatory = propertyInfoAttribute.Mandatory;
                connectorModel.PropertyName = propertyInfoAttribute.PropertyName;
                connectorModel.Name = propertyInfoAttribute.PropertyName;
                connectorModel.ToolTip = propertyInfoAttribute.ToolTip;
                connectorModel.IControl = false;
                connectorModel.PluginModel.Plugin.PropertyChanged += connectorModel.PropertyChangedOnPlugin;
                InputConnectors.Add(connectorModel);
                WorkspaceModel.AllConnectorModels.Add(connectorModel);
            }
            else if (propertyInfoAttribute.Direction.Equals(Direction.ControlSlave))
            {
                ConnectorModel connectorModel = new ConnectorModel();
                connectorModel.Caption = propertyInfoAttribute.Caption;
                connectorModel.ConnectorType = propertyInfoAttribute.PropertyInfo.PropertyType;
                connectorModel.WorkspaceModel = WorkspaceModel;
                connectorModel.PluginModel = this;
                connectorModel.IsMandatory = propertyInfoAttribute.Mandatory;
                connectorModel.PropertyName = propertyInfoAttribute.PropertyName;
                connectorModel.Name = propertyInfoAttribute.PropertyName;
                connectorModel.ToolTip = propertyInfoAttribute.ToolTip;
                connectorModel.IControl = true;
                connectorModel.PluginModel.Plugin.PropertyChanged += connectorModel.PropertyChangedOnPlugin;
                InputConnectors.Add(connectorModel);
                WorkspaceModel.AllConnectorModels.Add(connectorModel);
            }
            else if (propertyInfoAttribute.Direction.Equals(Direction.OutputData))
            {
                ConnectorModel connectorModel = new ConnectorModel();
                connectorModel.Caption = propertyInfoAttribute.Caption;
                connectorModel.ConnectorType = propertyInfoAttribute.PropertyInfo.PropertyType;
                connectorModel.WorkspaceModel = WorkspaceModel;
                connectorModel.PluginModel = this;
                connectorModel.IsMandatory = propertyInfoAttribute.Mandatory;
                connectorModel.PropertyName = propertyInfoAttribute.PropertyName;
                connectorModel.Name = propertyInfoAttribute.PropertyName;
                connectorModel.ToolTip = propertyInfoAttribute.ToolTip;
                connectorModel.Outgoing = true;
                connectorModel.IControl = false;
                connectorModel.PluginModel.Plugin.PropertyChanged += connectorModel.PropertyChangedOnPlugin;
                OutputConnectors.Add(connectorModel);
                WorkspaceModel.AllConnectorModels.Add(connectorModel);
            }
            else if (propertyInfoAttribute.Direction.Equals(Direction.ControlMaster))
            {
                ConnectorModel connectorModel = new ConnectorModel();
                connectorModel.Caption = propertyInfoAttribute.Caption;
                connectorModel.ConnectorType = propertyInfoAttribute.PropertyInfo.PropertyType;
                connectorModel.WorkspaceModel = WorkspaceModel;
                connectorModel.PluginModel = this;
                connectorModel.IsMandatory = propertyInfoAttribute.Mandatory;
                connectorModel.PropertyName = propertyInfoAttribute.PropertyName;
                connectorModel.Name = propertyInfoAttribute.PropertyName;
                connectorModel.ToolTip = propertyInfoAttribute.ToolTip;
                connectorModel.Outgoing = true;
                connectorModel.IControl = true;
                connectorModel.PluginModel.Plugin.PropertyChanged += connectorModel.PropertyChangedOnPlugin;
                OutputConnectors.Add(connectorModel);
                WorkspaceModel.AllConnectorModels.Add(connectorModel);
            }
        }

        /// <summary>
        /// Get the Image of the Plugin
        /// </summary>
        /// <returns></returns>
        public Image getImage()
        {
            return Plugin.GetImage(imageIndex);
        }

        /// <summary>
        /// Returns the Presentation of the wrapped IPlugin
        /// </summary>
        public UserControl PluginPresentation
        {
            get
            {
                return this.Plugin.Presentation;
            }
        }

        /// <summary>
        /// Progress of the plugin changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void PluginProgressChanged(IPlugin sender, PluginProgressEventArgs args)
        {
            //Calculate % of the plugins process
            this.PercentageFinished = args.Value / args.Max;
            //Tell the ExecutionEngine that this plugin needs a gui update
            this.GuiNeedsUpdate = true;
        }

        /// <summary>
        /// Status of the plugin changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void PluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if (args.StatusChangedMode == StatusChangedMode.ImageUpdate)
            {
                imageIndex = args.ImageIndex;
                if (WorkspaceModel.ExecutionEngine == null || !WorkspaceModel.ExecutionEngine.IsRunning())
                {
                    if (WorkspaceModel.MyEditor != null && WorkspaceModel.MyEditor.Presentation != null && UpdateableView != null)
                    {
                        WorkspaceModel.MyEditor.Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            UpdateableView.update();
                        }, null);
                    }
                }
            }
        }        

        /// <summary>
        /// Called if a Setting of a Plugin is changed 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="propertyChangedEventArgs"></param>
        public void SettingsPropertyChanged(Object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if(sender == plugin.Settings)
            {
                SettingesHaveChanges = true;
            }
        }

        /// <summary>
        /// Returns true if one of this PluginModel inputs is an IControl
        /// </summary>
        /// <returns></returns>
        public bool HasIControlInputs()
        {
            foreach (ConnectorModel connectorModel in OutputConnectors)
            {
                if (connectorModel.IControl)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        [NonSerialized]
        private bool stopped = false;
        internal bool Stop { get { return stopped; } set { stopped = value; } }

        public string CopyID
        {
            get;
            set;
        }

        [NonSerialized]
        internal ManualResetEvent resetEvent = new ManualResetEvent(true);

        /// <summary>
        /// Called by the execution engine threads to execute the internal plugin
        /// </summary>
        /// <param name="o"></param>
        internal void Execute(Object o)
        {
            var executionEngine = (ExecutionEngine)o;
            try
            {
                Stop = false;

                plugin.PreExecution();
                bool firstrun = true;

                while (true)
                {
                    resetEvent.WaitOne(10);
                    resetEvent.Reset();

                    //Check if we want to stop
                    if (Stop)
                    {
                        break;
                    }

                    // ################
                    // 0. If this is our first run and we are startable we start
                    // ################

                    //we are startable if we have NO input connectors
                    //Startable flag is deprecated now
                    if (firstrun && (InputConnectors.Count == 0 || HasOnlyOptionalUnconnectedInputs()))
                    {
                        firstrun = false;
                        try
                        {
                            PercentageFinished = 0;
                            GuiNeedsUpdate = true;

                            Plugin.Execute();
                            executionEngine.ExecutionCounter++;

                            PercentageFinished = 1;
                            GuiNeedsUpdate = true;
                        }
                        catch (Exception ex)
                        {
                            executionEngine.GuiLogMessage(
                                String.Format(Resources.PluginModel_Execute_An_error_occured_while_executing___0______1_, Name, ex.Message),
                                NotificationLevel.Error);
                            State = PluginModelState.Error;
                            GuiNeedsUpdate = true;
                        }
                        continue;
                    }

                    var breakit = false;
                    var atLeastOneNew = false;

                    // ################
                    // 1. Check if we may execute
                    // ################
                    
                    //Check if all necessary inputs are set                
                    foreach (ConnectorModel connectorModel in InputConnectors)
                    {                        
                        if (!connectorModel.IControl &&
                            (connectorModel.IsMandatory || connectorModel.InputConnections.Count > 0))
                        {
                            if(connectorModel.DataQueue.Count == 0 && connectorModel.LastData == null)
                            {
                                breakit = true;
                                continue;
                            }
                            if(connectorModel.DataQueue.Count > 0)
                            {
                                atLeastOneNew = true;
                            }
                        }                        
                    }

                    //Check if all outputs are free         
                    foreach (ConnectorModel connectorModel in OutputConnectors)
                    {
                        if (!connectorModel.IControl)
                        {
                            foreach(ConnectionModel connectionModel in connectorModel.OutputConnections)
                            {
                                if(connectionModel.To.DataQueue.Count>0)
                                {
                                    breakit = true;
                                }
                            }
                        }
                    }

                    //Gate is a special case: here we need all new data
                    if (PluginType.FullName.Equals("Gate.Gate"))
                    {
                        foreach (ConnectorModel connectorModel in InputConnectors)
                        {
                            if (connectorModel.InputConnections.Count > 0 && connectorModel.DataQueue.Count == 0)
                            {
                                breakit = true;
                            }
                        }
                    }

                    if (breakit || !atLeastOneNew)
                    {
                        continue;
                    }                                     

                    // ################
                    //2. Fill all Inputs of the plugin, if this fails break the loop run
                    // ################
                    foreach (ConnectorModel connectorModel in InputConnectors)
                    {
                        try
                        {
                            if((connectorModel.DataQueue.Count == 0 && connectorModel.LastData == null) || connectorModel.InputConnections.Count == 0)
                            {
                                continue;
                            }

                            object data;
                            
                            if (connectorModel.DataQueue.Count > 0)
                            {
                                data = connectorModel.DataQueue.Dequeue();                                
                            }
                            else
                            {
                                continue;
                            }

                            if (data == null)
                            {
                                continue;
                            }

                            //Implicit conversions:

                            //Cast from BigInteger -> Integer
                            if ((connectorModel.ConnectorType.FullName == "System.Int32" ||
                                 connectorModel.ConnectorType.FullName == "System.Int64") &&
                                data.GetType().FullName == "System.Numerics.BigInteger")
                            {
                                try
                                {
                                    data = (int)((BigInteger)data);                                    
                                }
                                catch (OverflowException)
                                {
                                    State = PluginModelState.Error;
                                    WorkspaceModel.ExecutionEngine.GuiLogMessage(String.Format(Resources.PluginModel_Execute_Number_of__0__too_big_for__1____2_, connectorModel.Name, Name, data), NotificationLevel.Error);
                                }
                            }
                            //Cast from Integer -> BigInteger
                            else if (connectorModel.ConnectorType.FullName == "System.Numerics.BigInteger" &&
                               (data.GetType().FullName == "System.Int32" || data.GetType().FullName == "System.Int64"))
                            {
                                data = new BigInteger((int)data);
                            }
                            //Cast from System.Byte[] -> System.String (UTF8)
                            else if (connectorModel.ConnectorType.FullName == "System.String" && data.GetType().FullName == "System.Byte[]")
                            {
                                var encoding = new UTF8Encoding();
                                data = encoding.GetString((byte[])data);
                            }
                            //Cast from System.String (UTF8) -> System.Byte[]
                            else if (connectorModel.ConnectorType.FullName == "System.Byte[]" && data.GetType().FullName == "System.String")
                            {
                                var encoding = new UTF8Encoding();
                                data = encoding.GetBytes((string)data);
                            }
                            //Cast from System.String (UTF8) -> ICryptoolStream
                            else if (connectorModel.ConnectorType.FullName == "Cryptool.PluginBase.IO.ICryptoolStream" && data.GetType().FullName == "System.String")
                            {
                                var writer = new CStreamWriter();
                                var str = (string)data;
                                if (str.Length > MaxStrStreamConversionLength)
                                {
                                    str = str.Substring(0, MaxStrStreamConversionLength);
                                }
                                var encoding = new UTF8Encoding();
                                writer.Write(encoding.GetBytes(str));
                                writer.Close();
                                data = writer;

                            }
                            //Cast from ICryptoolStream -> System.String (UTF8)
                            else if (connectorModel.ConnectorType.FullName == "System.String" && data.GetType().FullName == "Cryptool.PluginBase.IO.CStreamWriter")
                            {
                                var writer = (CStreamWriter)data;
                                using (var reader = writer.CreateReader())
                                {
                                    var buffer = new byte[MaxStrStreamConversionLength];
                                    var readamount = reader.Read(buffer, 0, MaxStrStreamConversionLength);
                                    if (readamount > 0)
                                    {
                                        var encoding = new UTF8Encoding();
                                        var str = encoding.GetString(buffer, 0, readamount);
                                        data = str;
                                    }
                                    else
                                    {
                                        data = String.Empty;
                                    }
                                }
                            }
                            //Cast to System.String
                            else if (connectorModel.ConnectorType.FullName == "System.String")
                            {
                                //Cast from System.Boolean -> System.String
                                if (data.GetType().FullName == "System.Boolean") data = ((Boolean)data).ToString();
                                //Cast from System.Int32 -> System.String
                                else if (data.GetType().FullName == "System.Int32") data = ((Int32)data).ToString();
                                //Cast from System.Int64 -> System.String
                                else if (data.GetType().FullName == "System.Int64") data = ((Int64)data).ToString();
                                //Cast from System.Numerics.BigInteger -> System.String
                                else if (data.GetType().FullName == "System.Numerics.BigInteger") data = ((BigInteger)data).ToString();
                            }

                            //now set the data                           
                            if (connectorModel.property == null)
                            {
                                connectorModel.property =
                                    Plugin.GetType().GetProperty(connectorModel.PropertyName);
                            }
                            connectorModel.property.SetValue(Plugin, data, null);                                                     
                        }
                        catch (Exception ex)
                        {
                            executionEngine.GuiLogMessage(
                                String.Format(Resources.PluginModel_Execute_An_error_occured_while_setting_value_of_connector___0___of___1_____2_, connectorModel.Name, Name, ex.Message), NotificationLevel.Error);
                            State = PluginModelState.Error;
                            GuiNeedsUpdate = true;
                        }

                    }

                    // ################
                    //3. Execute
                    // ################
                    try
                    {
                        if (executionEngine.SleepTime > 0)
                        {
                            Thread.Sleep(executionEngine.SleepTime);
                        }
                        
                        PercentageFinished = 0;
                        GuiNeedsUpdate = true;

                        Plugin.Execute();
                        executionEngine.ExecutionCounter++;

                        if (plugin.GetAutoAssumeFullEndProgressAttribute())
                        {
                            PercentageFinished = 1;
                            GuiNeedsUpdate = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        executionEngine.GuiLogMessage(
                            String.Format(Resources.PluginModel_Execute_An_error_occured_while_executing____0______1__, Name, ex.Message), NotificationLevel.Error);
                        State = PluginModelState.Error;
                        GuiNeedsUpdate = true;
                    }                    
                    
                    // ################
                    // 4. Set all connectorModels belonging to this pluginModel to inactive
                    // ################
                    foreach (ConnectorModel connectorModel in InputConnectors)
                    {
                        foreach (var connectionModel in connectorModel.InputConnections)
                        {
                            connectionModel.Active = false;
                            connectionModel.GuiNeedsUpdate = true;
                        }
                    }

                    // ################
                    // 5. let all plugins before this check if it may execute
                    // ################
                    foreach (ConnectorModel connectorModel in InputConnectors)
                    {
                        foreach (ConnectionModel connectionModel in connectorModel.InputConnections)
                        {
                            connectionModel.From.PluginModel.resetEvent.Set();
                        }
                    }
                }
                plugin.PostExecution();
            }
            catch (Exception ex)
            {
                executionEngine.GuiLogMessage(
                               String.Format(Resources.PluginModel_Execute_An_error_occured_while_executing___0______1_, Name, ex.Message),
                               NotificationLevel.Error);
                State = PluginModelState.Error;
            }
        }

        //public int ZIndex { get; set; }

        public bool HasOnlyOptionalUnconnectedInputs()
        {
            foreach (ConnectorModel cm in GetInputConnectors())
            {
                if (cm.IsMandatory || cm.GetInputConnections().Count > 0)
                {
                    return false;
                } 
            }
            return true;
        }    
    }    

    /// <summary>
    /// The internal state of a Plugin Model
    /// </summary>
    public enum PluginModelState{
        Normal,
        Warning,
        Error
    };

    public enum BinComponentState
    {        
        Min,
        Presentation,
        Data,
        Log,
        Setting,
        Description,
        Default
    };

    public enum PluginViewState
    {        
        Min,
        Presentation,
        Data,
        Log,
        Setting,
        Description,
        Fullscreen,
        Default
    };

    public enum PlugState
    {
        Plugged,
        Unplugged
    }

    public class ConnectorPlugstateChangedEventArgs : EventArgs
    {
        public PlugState PlugState { get; private set; }        
        public ConnectorModel ConnectorModel { get; private set; }
        public int Connections { get; private set; }

        public ConnectorPlugstateChangedEventArgs(PlugState plugstate, ConnectorModel connectorModel)
        {
            PlugState = plugstate;
            ConnectorModel = connectorModel;
            //Count how many connections are connected to this ConnectorModel
            Connections += connectorModel.InputConnections.Count;
            Connections += connectorModel.OutputConnections.Count;
        }
    }
}
