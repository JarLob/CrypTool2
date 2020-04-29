/*
   Copyright 2020 Nils Kopal <kopal<AT>cryptool.org>

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
using System.Reflection;
using WorkspaceManager.Execution;
using WorkspaceManager.Model;
using System.IO;
using Path = System.IO.Path;
using System.Windows;
using System.Threading;
using Cryptool.PluginBase;
using System.Collections.Generic;
using System.Globalization;

namespace Cryptool.CrypConsole
{  
    public partial class Main : Window
    {
        private static string[] subfolders = new string[]
        {
            "",
            "CrypPlugins",
            "Lib",
        };

        private bool _verbose = false;
        private int _timeout = int.MaxValue;
        private TerminationType _terminationType = TerminationType.GlobalProgress;
        private Dictionary<IPlugin, double> _pluginProgressValues = new Dictionary<IPlugin, double>();
        private WorkspaceModel _workspaceModel = null;
        private ExecutionEngine _engine = null;
        private int _globalProgress;
        private DateTime _startTime;
        private object _progressLockObject = new object();

        /// <summary>
        /// Constructor
        /// </summary>
        public Main()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called, after "ui" is initialized. From this point, we should have a running ui thread
        /// Thus, we start the execution of the CrypConsole
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Main_Initialized(object sender, EventArgs e)
        {
            Start(CrypConsole.Args);
        }

        /// <summary>
        /// Starts the execution of the defined workspace
        /// 1) Parses the commandline parameters
        /// 2) Creates CT2 model and execution engine
        /// 3) Starts execution
        /// 4) Gives data as defined by user to the model
        /// 5) Retrieves results for output and outputs these
        /// 6) [terminates]
        /// </summary>
        /// <param name="args"></param>
        public void Start(string[] args)
        {
            _startTime = DateTime.Now;

            //Step 0: Set locale to English
            var cultureInfo = new CultureInfo("en-us", false);
            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;

            //Step 1: Check, if Help needed
            if (ArgsHelper.GetShowHelp(args))
            {
                Environment.Exit(0);
            }            

            //Step 2: Get cwm_file to open
            string cwm_file = ArgsHelper.GetCWMFileName(args);
            if (cwm_file == null)
            {
                Console.WriteLine("Please specify a cwm file using -cwm=filename");
                Environment.Exit(-1);
            }
            if (!File.Exists(cwm_file))
            {
                Console.WriteLine("Specified cwm file \"{0}\" does not exist", cwm_file);
                Environment.Exit(-2);
            }

            //Step 3: Get additional parameters
            _verbose = ArgsHelper.GetVerbose(args);
            try
            {
                _timeout = ArgsHelper.GetTimeout(args);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(-2);
            }
            try
            {
                _terminationType = ArgsHelper.GetTerminationType(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(-2);
            }

            //Step 4: Get input parameters
            List<Parameter> inputParameters = null;
            try
            {
                inputParameters = ArgsHelper.GetInputParameters(args);
                if (_verbose)
                {
                    foreach (var param in inputParameters)
                    {
                        Console.WriteLine("Input parameter given: " + param);
                    }
                }
            }
            catch(InvalidParameterException ipex)
            {
                Console.WriteLine(ipex.Message);
                Environment.Exit(-3);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception occured while parsing parameters: {0}", ex.Message);
                Environment.Exit(-3);
            }

            //Step 5: Get output parameters
            List<Parameter> outputParameters = null;
            try
            {
                outputParameters = ArgsHelper.GetOutputParameters(args);
                if (_verbose)
                {
                    foreach (var param in inputParameters)
                    {
                        Console.WriteLine("Output parameter given: " + param);
                    }
                }
            }
            catch (InvalidParameterException ipex)
            {
                Console.WriteLine(ipex.Message);
                Environment.Exit(-3);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured while parsing parameters: {0}", ex.Message);
                Environment.Exit(-3);
            }

            //Step 6: Update application domain. This allows loading additional .net assemblies
            try
            {
                UpdateAppDomain();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception occured while updating AppDomain: {0}", ex.Message);
                Environment.Exit(-4);
            }

            //Step 7: Load cwm file and create model            
            try
            {
                ModelPersistance modelPersistance = new ModelPersistance();
                _workspaceModel = modelPersistance.loadModel(cwm_file, true);

                foreach (var pluginModel in _workspaceModel.GetAllPluginModels())
                {
                    pluginModel.Plugin.OnGuiLogNotificationOccured += OnGuiLogNotificationOccured;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured while loading model from cwm file: {0}", ex.Message);
                Environment.Exit(-5);
            }

            //Step 8: Set input parameters
            foreach (var param in inputParameters)
            {
                string name = param.Name;
                bool found = false;
                foreach (var component in _workspaceModel.GetAllPluginModels())
                {
                    if (component.GetName().ToLower().Equals(param.Name.ToLower()))
                    {
                        if (component.PluginType.FullName.Equals("Cryptool.TextInput.TextInput"))
                        {
                            var settings = component.Plugin.Settings;
                            var textProperty = settings.GetType().GetProperty("Text");

                            if (param.ParameterType == ParameterType.Text || param.ParameterType == ParameterType.Number)
                            {
                                textProperty.SetValue(settings, param.Value);
                            }
                            else if(param.ParameterType == ParameterType.File)
                            {
                                try
                                {
                                    if (!File.Exists(param.Value))
                                    {
                                        Console.WriteLine("Input file does not exist: {0}", param.Value);
                                        Environment.Exit(-7);
                                    }
                                    var value = File.ReadAllText(param.Value);
                                    textProperty.SetValue(settings, value);
                                }
                                catch(Exception ex)
                                {
                                    Console.WriteLine("Exception occured while reading file {0}: {0}", param.Value, ex.Message);
                                    Environment.Exit(-7);
                                }
                            }
                            //we need to call initialize to get the new text to the ui of the TextInput component
                            //otherwise, it will output the value retrieved by deserialization
                            component.Plugin.Initialize();
                            found = true;
                        }                       
                    }
                }
                if (!found)
                {
                    Console.WriteLine("Component for setting input parameter not found: {0}", param);
                    Environment.Exit(-7);
                }
            }

            //Step 9: Set output parameters
            foreach (var param in outputParameters)
            {
                string name = param.Name;
                bool found = false;
                foreach (var component in _workspaceModel.GetAllPluginModels())
                {
                    if (component.GetName().ToLower().Equals(param.Name.ToLower()))
                    {
                        if (component.PluginType.FullName.Equals("TextOutput.TextOutput"))
                        {
                            component.Plugin.PropertyChanged += Plugin_PropertyChanged;                            
                            found = true;
                        }
                    }
                }
                if (!found)
                {
                    Console.WriteLine("TextOutput for setting output parameter not found: {0}", param);
                    Environment.Exit(-7);
                }
            }

            //Step 10: add OnPluginProgressChanged handlers
            foreach(var plugin in _workspaceModel.GetAllPluginModels())
            {
                plugin.Plugin.OnPluginProgressChanged += OnPluginProgressChanged;
            }

            //Step 11: Create execution engine            
            try
            {
                _engine = new ExecutionEngine(null);
                _engine.OnGuiLogNotificationOccured += OnGuiLogNotificationOccured;
                _engine.Execute(_workspaceModel, false);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception occured while executing model: {0}", ex.Message);
                Environment.Exit(-7);
            }

            //Step 12: Start execution in a dedicated thread
            DateTime endTime = DateTime.Now.AddSeconds(_timeout);
            Thread t = new Thread(() =>
            {
                CultureInfo.CurrentCulture = new CultureInfo("en-Us", false);
                while (_engine.IsRunning())
                {
                    Thread.Sleep(100);
                    if(_engine.IsRunning() && _timeout < int.MaxValue && DateTime.Now >= endTime)
                    {
                        Console.WriteLine("Timeout ({0} seconds) reached. Kill process hard now", _timeout);
                        Environment.Exit(-8);
                    }
                }
                if (_verbose) 
                {
                    Console.WriteLine("Execution engine stopped. Terminate now");
                    Console.WriteLine("Total execution took: {0}", DateTime.Now - _startTime);
                }
                Environment.Exit(0);
            });
            t.Start();
        }

        /// <summary>
        /// Called, when progress on a single plugin changed
        /// Handles the global progress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnPluginProgressChanged(IPlugin sender, PluginProgressEventArgs args)
        {
            if(_terminationType == TerminationType.GlobalProgress)
            {
                lock (_progressLockObject)
                {
                    if (!_pluginProgressValues.ContainsKey(sender))
                    {
                        _pluginProgressValues.Add(sender, args.Value / args.Max);
                    }
                    else
                    {
                        _pluginProgressValues[sender] = args.Value / args.Max;
                    }
                    double numberOfPlugins = _workspaceModel.GetAllPluginModels().Count;
                    double totalProgress = 0;
                    foreach (var value in _pluginProgressValues.Values)
                    {
                        totalProgress += value;
                    }
                    if (totalProgress == numberOfPlugins && _engine.IsRunning())
                    {
                        if (_verbose)
                        {
                            Console.WriteLine("Global progress reached 100%, stop execution engine now");
                        }
                        _engine.Stop();
                    }
                    int newProgress = (int)((totalProgress / numberOfPlugins) * 100);
                    if (_verbose && _globalProgress != newProgress)
                    {
                        _globalProgress = newProgress;
                        Console.WriteLine("Global progress change: {0}%", _globalProgress);
                    }
                }
            }
        }

        /// <summary>
        /// Property changed on plugin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Plugin_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var plugin = (IPlugin)sender;
            var property = sender.GetType().GetProperty(e.PropertyName);
            if (property.Name.ToLower().Equals("input"))
            {
                Console.WriteLine(property.GetValue(plugin).ToString());
                //_engine.Stop();
            }
        }

        /// <summary>
        /// Logs guilog to console based on error level and verbosity
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnGuiLogNotificationOccured(IPlugin sender, GuiLogEventArgs args)
        {
            if (_verbose || args.NotificationLevel == NotificationLevel.Error)
            {
                Console.WriteLine("GuiLog:{0}:{1}:{2}:{3}", DateTime.Now, args.NotificationLevel, (sender != null ? sender.GetType().Name : "null"), args.Message);
            }
        }

        /// <summary>
        /// Updates app domain with user defined assembly resolver routine
        /// </summary>
        private void UpdateAppDomain()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(LoadAssembly);
        }

        /// <summary>
        /// Loads assemblies defined by subfolders definition
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private Assembly LoadAssembly(object sender, ResolveEventArgs args)
        {
            string folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            foreach (var subfolder in subfolders)
            {
                string assemblyPath = Path.Combine(folderPath, (Path.Combine(subfolder, new AssemblyName(args.Name).Name + ".dll")));
                if (File.Exists(assemblyPath))
                {
                    Assembly assembly = Assembly.LoadFrom(assemblyPath);
                    if (_verbose)
                    {
                        Console.WriteLine("Loaded assembly: " + assemblyPath);
                    }
                    return assembly;
                }
                assemblyPath = Path.Combine(folderPath, (Path.Combine(subfolder, new AssemblyName(args.Name).Name + ".exe")));
                if (File.Exists(assemblyPath))
                {
                    Assembly assembly = Assembly.LoadFrom(assemblyPath);
                    if (_verbose)
                    {
                        Console.WriteLine("Loaded assembly: " + assemblyPath);
                    }
                    return assembly;
                }
            }
            return null;
        }
    }       
}
