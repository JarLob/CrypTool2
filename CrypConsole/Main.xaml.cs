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

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Initialized(object sender, EventArgs e)
        {
            Start(CrypConsole.Args);
        }

        public void Start(string[] args)
        {
            if (ArgsHelper.CheckShowHelp(args))
            {
                Environment.Exit(0);
            }

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

            _verbose = ArgsHelper.CheckVerbose(args);

            try
            {
                var parameters = ArgsHelper.GetInputParameters(args);
                foreach (var param in parameters)
                {
                    Console.WriteLine("Input param: " + param);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception occured while parsing parameters: {0}", ex.Message);
                Environment.Exit(-3);
            }

            try
            {
                UpdateAppDomain();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception occured while parsing parameters: {0}", ex.Message);
                Environment.Exit(-4);
            }

            WorkspaceModel model = null;
            try
            {
                ModelPersistance modelPersistance = new ModelPersistance();
                model = modelPersistance.loadModel(cwm_file, true);

                foreach (var pluginModel in model.GetAllPluginModels())
                {
                    pluginModel.Plugin.OnGuiLogNotificationOccured += OnGuiLogNotificationOccured;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured while loading model from cwm file: {0}", ex.Message);
                Environment.Exit(-5);
            }

            ExecutionEngine engine = null;
            try
            {

                engine = new ExecutionEngine(null);
                engine.OnGuiLogNotificationOccured += OnGuiLogNotificationOccured;
                engine.Execute(model, false);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception occured while executing model: {0}", ex.Message);
                Environment.Exit(-6);
            }

            Thread t = new Thread(() =>
            {
                while (engine.IsRunning())
                {
                    foreach (var p in model.GetAllPluginModels())
                    {
                        if (p.GetName().Equals("Ciphertext"))
                        {
                            foreach (var input in p.GetInputConnectors())
                            {
                                if (input.PropertyName.Equals("Input"))
                                {
                                    if (input.LastData != null)
                                    {
                                        Console.WriteLine("Output data: " + input.LastData);
                                        engine.Stop();                                        
                                    }
                                }
                            }
                        }
                    }
                }
                Environment.Exit(0);
            });
            t.Start();
        }        

        /// <summary>
        /// Logs guilog to console based on error level and verbosity
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnGuiLogNotificationOccured(PluginBase.IPlugin sender, PluginBase.GuiLogEventArgs args)
        {
            if (_verbose || args.NotificationLevel == NotificationLevel.Error)
            {
                Console.WriteLine("GuiLog:{0}:{1}:{2}:{3}", DateTime.Now, args.NotificationLevel, (sender != null ? sender.GetType().Name : "null"), args.Message);
            }
        }

        /// <summary>
        /// Updates app domain with user defined assembly resolber routine
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
