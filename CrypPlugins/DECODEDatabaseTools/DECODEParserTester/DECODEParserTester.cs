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
        private DECODEParserTestPresentation _presentation = new DECODEParserTestPresentation();
        private bool _running = false;

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
            get
            {
                return _presentation;
            }
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
            _running = true;
            _presentation.ClearBestlist();
            List<Type> parserTypes = GetAllParsers();

            //here, we count the number of usable parser:
            int totalParsers = 0;
            foreach (var parserType in parserTypes)
            {
                if (GetVoidConstructorInfo(parserType) == null)
                {
                    continue;
                }
                var parser = (Parser)Activator.CreateInstance(parserType);

                var possibleParserParameters = parser.GetPossibleParserParameters();
                if (possibleParserParameters == null ||
                    (possibleParserParameters.PossiblePrefixes.Count == 0 && possibleParserParameters.PossibleNulls.Count == 0))
                {
                    continue;
                }
                totalParsers++;
            }

            //here, we do the actual testing of each parser
            int parserCounter = 0;
            foreach (var parserType in parserTypes)
            {
                if (!_running)
                {
                    return;
                }
                if(GetVoidConstructorInfo(parserType) == null)
                {
                    continue;
                }
                var parser = (Parser)Activator.CreateInstance(parserType);

                var possibleParserParameters = parser.GetPossibleParserParameters();
                if (possibleParserParameters == null ||
                    (possibleParserParameters.PossiblePrefixes.Count == 0 && possibleParserParameters.PossibleNulls.Count == 0))
                {
                    continue;
                }

                //test all settings of the parser
                GuiLogMessage(string.Format("Testing all {0} setting combinations of {1}", parser.GetPossibleParserParameters().GetNumberOfSettingCombinations(), parser.ParserName), NotificationLevel.Info);
                DateTime startDateTime = DateTime.Now;
                try
                {                    
                    TestParser(parser);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(string.Format("Exception occured during parser test of {0}:", parser.ParserName, ex.Message), NotificationLevel.Error);
                    continue;
                }
                GuiLogMessage(string.Format("Tested all setting combinations of {0} done in {1} ms !", parser.ParserName, (DateTime.Now - startDateTime).TotalMilliseconds), NotificationLevel.Info);
                parserCounter++;
                ProgressChanged(parserCounter, totalParsers);
            }
            ProgressChanged(1, 1);
            _running = false;
        }       

        /// <summary>
        /// Tests all possible settings of the given parser
        /// </summary>
        /// <param name="parser"></param>
        private void TestParser(Parser parser)
        {
            PossibleParserParameters possibleParserParameters = parser.GetPossibleParserParameters();
            int combinations = possibleParserParameters.GetNumberOfSettingCombinations();

            List<BestListEntry> bestList = new List<BestListEntry>();
            for (int i = 0; i < combinations; i++)
            {
                if (!_running)
                {
                    return;
                }
                var settings = possibleParserParameters.GetSettings(i);
                if (settings == null)
                {
                    continue;
                }
                parser.Prefixes = settings.Item1;
                parser.Nulls = settings.Item2;
                parser.DECODETextDocument = Cluster;

                var textDocument = parser.GetTextDocument();

                var entropyValue = TextDocument.CalculateEntropy(textDocument);

                BestListEntry bestListEntry = new BestListEntry();
                bestListEntry.ParserName = parser.ParserName;
                bestListEntry.Nulls = parser.Nulls;
                bestListEntry.Prefixes = parser.Prefixes;
                bestListEntry.EntropyValue = entropyValue;                
                bestList.Add(bestListEntry);
            }
            bestList.Sort();
            if(bestList.Count > 1)
            {
                _presentation.AddNewBestlistEntry(bestList[0]);
                _presentation.AddNewBestlistEntry(bestList[1]);
            }
            else if(bestList.Count == 1)
            {
                _presentation.AddNewBestlistEntry(bestList[0]);
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
            _running = false;
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

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }
    }

    /// <summary>
    /// BestListEntry for best list of parsers
    /// </summary>
    public class BestListEntry : IComparable
    {
        public BestListEntry()
        {
            ParserName = string.Empty;
            EntropyValue = 0;
        }

        public string ParserName { get; set; }
        public double EntropyValue { get; set; }

        public List<Token> Nulls = new List<Token>();
        public List<Token> Prefixes = new List<Token>();

        public string EntropyAsString
        {
            get
            {
                return "" + Math.Round(EntropyValue, 2);
            }
        }

        public string PrefixesAsString
        {
            get
            {
                if (Prefixes.Count == 0)
                {
                    return string.Empty;
                }
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < Prefixes.Count; i++)
                {
                    stringBuilder.Append(Prefixes[i]);
                    if (i < Prefixes.Count - 1)
                    {
                        stringBuilder.Append(", ");
                    }
                }
                return stringBuilder.ToString();
            }
        }

        public string NullsAsString
        {
            get
            {
                if (Nulls.Count == 0)
                {
                    return string.Empty;
                }
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < Nulls.Count; i++)
                {
                    stringBuilder.Append(Nulls[i]);
                    if (i < Nulls.Count - 1)
                    {
                        stringBuilder.Append(", ");
                    }
                }
                return stringBuilder.ToString();
            }
        }
    
        public int CompareTo(object obj)
        {
            if(obj is BestListEntry)
            {
                return EntropyValue.CompareTo(((BestListEntry)obj).EntropyValue);
            }
            else
            {
                return 0;
            }
        }
    }
}
