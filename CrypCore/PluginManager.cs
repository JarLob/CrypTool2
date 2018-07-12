/*
   Copyright 2018 CrypTool team

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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace Cryptool.Core
{
    /// <summary>
    /// PluginManager Class  
    /// </summary>
    public class PluginManager
    {
        private readonly HashSet<string> disabledAssemblies = new HashSet<string>();

        /// <summary>
        /// Counter for the dll files that were found
        /// </summary>
        private int availablePluginsApproximation = 0;

        /// <summary>
        /// Subdirectory of all crypplugins
        /// </summary>
        private const string PluginDirectory = "CrypPlugins";

        /// <summary>
        /// Subdirectory in which plugins of CrypStore are stored and loaded from
        /// </summary>
        private const string CrypStorePluginDirectory = @"CrypTool2\CrypStorePlugins";

        /// <summary>
        /// Fires if an exception occurs
        /// </summary>
        public event CrypCoreExceptionEventHandler OnExceptionOccured;

        /// <summary>
        /// Fires if an info occurs
        /// </summary>
        public event CrypCoreDebugEventHandler OnDebugMessageOccured;

        /// <summary>
        /// Occurs when a plugin was loaded
        /// </summary>
        public event CrypCorePluginLoadedHandler OnPluginLoaded;

        /// <summary>
        /// Folder for plugins that are delivered with CrypTool 2
        /// </summary>
        private readonly string crypPluginsFolder;

        /// <summary>
        /// Folder for plugins of CrypToolStore
        /// </summary>
        private readonly string crypToolStorePluginFolder;

        /// <summary>
        /// Loaded Assemblies
        /// </summary>
        private readonly Dictionary<string, Assembly> loadedAssemblies;

        /// <summary>
        /// Loaded Types
        /// </summary>
        private readonly Dictionary<string, Type> loadedTypes;

        /// <summary>
        /// Found Assemblies
        /// </summary>
        private Dictionary<string, Assembly> foundAssemblies = new Dictionary<string, Assembly>();

        /// <summary>
        /// cTor
        /// </summary>
        public PluginManager(HashSet<string> disabledAssemblies)
        {
            this.disabledAssemblies = disabledAssemblies;
            this.crypPluginsFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), PluginDirectory);
            this.crypToolStorePluginFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), CrypStorePluginDirectory);
            //create folder for CrypToolStore if it does not exsit
            if (!Directory.Exists(crypToolStorePluginFolder))
            {
                Directory.CreateDirectory(crypToolStorePluginFolder);
            }
            this.loadedAssemblies = new Dictionary<string, Assembly>();
            this.loadedTypes = new Dictionary<string, Type>();
        }

        /// <summary>
        /// Returns a type of a loaded assembly
        /// </summary>
        /// <param name="assemblyName">Assembly Name</param>
        /// <param name="typeName">Type Name</param>
        /// <returns>Return the type or null if no type could be found</returns>
        public Type LoadType(string assemblyName, string typeName)
        {
            if (this.loadedAssemblies.ContainsKey(assemblyName))
            {
                return this.loadedAssemblies[assemblyName].GetType(typeName, false);
            }
            return null;
        }

        /// <summary>
        /// Returns all types found in the plugins
        /// </summary>
        /// <param name="state">Load type from all plugins or from signed only</param>
        /// <returns></returns>
        public Dictionary<string, Type> LoadTypes(AssemblySigningRequirement state)
        {
            availablePluginsApproximation = AvailablePluginsApproximation(new DirectoryInfo(crypPluginsFolder));
            availablePluginsApproximation += AvailablePluginsApproximation(new DirectoryInfo(crypToolStorePluginFolder));
            int currentPosition = FindAssemblies(new DirectoryInfo(crypPluginsFolder), state, foundAssemblies, 0);
            FindAssemblies(new DirectoryInfo(crypToolStorePluginFolder), state, foundAssemblies, currentPosition);
            LoadTypes(foundAssemblies);
            return this.loadedTypes;
        }

        /// <summary>
        /// Find all CrypPlugins
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        private int AvailablePluginsApproximation(DirectoryInfo directory)
        {
            int count = 0;
            foreach (DirectoryInfo subDirectory in directory.GetDirectories())
            {
                count = AvailablePluginsApproximation(subDirectory);
            }
            return directory.GetFiles("*.dll").Length;
        }

        /// <summary>
        /// Search for assemblies in given directory
        /// </summary>
        /// <param name="directory">directory</param>
        /// <param name="state">Search for all or only for signed assemblies</param>
        /// <param name="foundAssemblies">list of found assemblies</param>
        private int FindAssemblies(DirectoryInfo directory, AssemblySigningRequirement state, Dictionary<string, Assembly> foundAssemblies, int currentPosition = 0)
        {
            foreach (FileInfo fileInfo in directory.GetFiles("*.dll"))
            {
                if (disabledAssemblies != null && disabledAssemblies.Contains(fileInfo.Name))
                {
                    continue;
                }

                currentPosition++;
                try
                {
                    Assembly asm = Assembly.Load(AssemblyName.GetAssemblyName(fileInfo.FullName));

                    string key = GetAssemblyKey(asm.FullName, state);
                    if (key == null)
                    {
                        throw new UnknownFileFormatException(fileInfo.FullName);
                    }

                    bool sendMessage = false;
                    if (!foundAssemblies.ContainsKey(key))
                    {
                        foundAssemblies.Add(key, asm);
                        sendMessage = true;
                    }
                    else if (GetVersion(asm) > GetVersion(foundAssemblies[key]))
                    {
                        foundAssemblies[key] = asm;
                        sendMessage = true;
                    }

                    if (sendMessage)
                    {
                        SendDebugMessage("Loaded Assembly \"" + asm.FullName + "\" from file: " + fileInfo.FullName);
                        if (OnPluginLoaded != null)
                        {
                            OnPluginLoaded(this, new PluginLoadedEventArgs(currentPosition, this.availablePluginsApproximation, string.Format("{0} Version={1}", asm.GetName().Name, GetVersion(asm))));
                        }
                    }
                }
                catch (BadImageFormatException)
                {
                    SendExceptionMessage(string.Format(Resources.Exceptions.non_plugin_file, fileInfo.FullName));
                }
                catch (Exception ex)
                {
                    SendExceptionMessage(ex);
                }
            }
            return currentPosition;
        }

        /// <summary>
        /// Returns version of the given assembly
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private static Version GetVersion(Assembly assembly)
        {
            var fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;
            if (fileVersion == null)
            {
                return assembly.GetName().Version;
            }

            return new Version(fileVersion);
        }

        /// <summary>
        /// Interate the found assemblies and add well known types
        /// </summary>
        /// <param name="foundAssemblies">list of found assemblies</param>
        private void LoadTypes(Dictionary<string, Assembly> foundAssemblies)
        {
            string interfaceName = "Cryptool.PluginBase.IPlugin";

            foreach (Assembly asm in foundAssemblies.Values)
            {
                AssemblyName assemblyName = new AssemblyName(asm.FullName);
                try
                {
                    foreach (Type type in asm.GetTypes())
                    {
                        if (type.GetInterface(interfaceName) != null && !this.loadedTypes.ContainsKey(type.FullName))
                        {
                            this.loadedTypes.Add(type.FullName, type);
                            if (!this.loadedAssemblies.ContainsKey(assemblyName.Name))
                            {
                                this.loadedAssemblies.Add(assemblyName.Name, asm);
                            }
                        }
                    }
                }
                catch (ReflectionTypeLoadException tle)
                {
                    if (OnExceptionOccured != null)
                    {
                        OnExceptionOccured(this, new PluginManagerEventArgs(new TypeLoadException(asm.FullName + "\n" + tle.LoaderExceptions[0].Message)));
                    }
                }
                catch (Exception exception)
                {
                    if (OnExceptionOccured != null)
                    {
                        OnExceptionOccured(this, new PluginManagerEventArgs(new TypeLoadException(asm.FullName + "\n" + exception.Message)));
                    }
                }
            }
        }

        /// <summary>
        /// Create a unique key for each assembly
        /// </summary>
        /// <param name="assemblyFullName">Full name of the assembly</param>
        /// <param name="state">Signed or unsigned</param>
        /// <returns>Returns the key or null if public key is null and signing is required</returns>
        private string GetAssemblyKey(string assemblyFullName, AssemblySigningRequirement state)
        {
            AssemblyName asmName = new AssemblyName(assemblyFullName);
            if (state == AssemblySigningRequirement.LoadSignedAssemblies)
            {
                if (asmName.KeyPair.PublicKey == null)
                    return null;
                return asmName.Name + "__" + asmName.KeyPair.ToString();
            }
            return asmName.Name;
        }
        /// <summary>
        /// Sends a debug message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void SendDebugMessage(string message)
        {
            if (OnDebugMessageOccured != null)
            {
                OnDebugMessageOccured(this, new PluginManagerEventArgs(message));
            }
        }

        private void SendExceptionMessage(Exception ex)
        {
            if (OnExceptionOccured != null)
            {
                OnExceptionOccured(this, new PluginManagerEventArgs(ex));
            }
        }

        private void SendExceptionMessage(string message)
        {
            if (OnExceptionOccured != null)
            {
                OnExceptionOccured(this, new PluginManagerEventArgs(message));
            }
        }
    }
}
