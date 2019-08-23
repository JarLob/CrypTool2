/*
   Copyright 2019 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.Plugins.DECODEDatabaseTools.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Cryptool.Plugins.DECODEDatabaseTools
{
    [Author("Nils Kopal", "nils.kopal@cryptool.org", "CrypTool 2 Team", "https://www.cryptool.org")]
    [PluginInfo("Cryptool.Plugins.DECODEDatabaseTools.Properties.Resources", "DecodeParserTesterCaption", "DecodeParserTesterTooltip", "DECODEDatabaseTools/userdoc.xml", "DECODEDatabaseTools/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class DECODEParserTester : ICrypComponent
    {
        [PropertyInfo(Direction.InputData, "ClusterCaption", "ClusterTooltip", true)]
        public string Cluster
        {
            get;
            set;
        }

        public ISettings Settings
        {
            get;
            set;
        }

        public UserControl Presentation
        {
            get;
            set;
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PropertyChangedEventHandler PropertyChanged;

        public void Dispose()
        {
            
        }

        public void Execute()
        {
            List<Type> parserTypes = GetAllParsers();
            foreach(var parserType in parserTypes)
            {
                if(GetVoidConstructorInfo(parserType) == null)
                {
                    continue;
                }
                var parser = (Parser)Activator.CreateInstance(parserType);
                GuiLogMessage(string.Format("Created a parser object with type name: {0}", parser.ParserName), NotificationLevel.Info);
            }
        }

        public void Initialize()
        {
            
        }

        public void PostExecution()
        {
            
        }

        public void PreExecution()
        {
            
        }

        public void Stop()
        {
            
        }

        /// <summary>
        /// Returns types of all parsers implemented in Cryptool.Plugins.DECODEDatabaseTools.Util
        /// which derive from SimpleSingleTokenParser and are not the KeyAsPlaintextParser
        /// </summary>
        /// <returns></returns>
        public List<Type> GetAllParsers()
        {
            List<Parser> parsers = new List<Parser>();
            var query = from t in Assembly.GetExecutingAssembly().GetTypes()
                    where t.IsClass && 
                    t.Namespace.Equals("Cryptool.Plugins.DECODEDatabaseTools.Util") &&
                    t.BaseType.Name.Equals("SimpleSingleTokenParser") && 
                    !t.Name.Equals("KeyAsPlaintextParser")
                    select t;           
            return query.ToList();
        }

        /// <summary>
        /// Returns the ConstructorInfo of the void constructor of the given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ConstructorInfo GetVoidConstructorInfo(Type type)
        {
            foreach(var constructorInfo in type.GetConstructors())
            {
                ParameterInfo[] parameters = constructorInfo.GetParameters();
                if(parameters.Length == 0)
                {
                    return constructorInfo;
                }
            }
            return null;
        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }
    }
}
