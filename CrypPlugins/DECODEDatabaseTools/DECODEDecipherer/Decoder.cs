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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptool.Plugins.DECODEDatabaseTools
{
    public class Decoder
    {
        private string _DECODEKeyDocument;
        private Dictionary<string, string> _keyMapping = new Dictionary<string, string>();

        public Decoder(string DECODEKeyDocument)
        {
            _DECODEKeyDocument = DECODEKeyDocument;
            GenerateKeyMapping();
        }

        private void GenerateKeyMapping()
        {
            foreach (var line in _DECODEKeyDocument.Split('\r', '\n'))
            {
                string trimmedLine = line.Trim();
                if(line.StartsWith("#"))
                {
                    continue;
                }

                var parts = trimmedLine.Split('-');
                if(parts.Length < 2)
                {
                    GuiLogMessage(String.Format("Found a line in the key that does not contain a valid key mapping: {0}", trimmedLine), NotificationLevel.Warning);
                    continue;
                }
                string left = parts[0].Trim();
                string right = string.Empty;
                for(int i = 1; i < parts.Length; i++)
                {                    
                    right += parts[i].Trim();
                    if (i < parts.Length - 1)
                    {
                        right += " - ";
                    }
                }

                if (_keyMapping.ContainsKey(left))
                {
                    GuiLogMessage(String.Format("Found a remapping in the key. Ignoring it: {0}", trimmedLine), NotificationLevel.Warning);
                    continue;
                }
                _keyMapping.Add(left, right);
            }
        }

        /// <summary>
        /// Decodes a line using the stored key
        /// => adds DecodedText to each token
        /// </summary>
        /// <param name="line"></param>
        public void Decode(Line line)
        {
            if(line.LineType == LineType.Comment)
            {
                return;
            }
            foreach(var token in line.Tokens)
            {                
                if(token.TokenType == TokenType.Null)
                {
                    token.DecodedText = " ";
                    continue;
                }
                if (_keyMapping.ContainsKey(token.Text))
                {
                    token.DecodedText = _keyMapping[token.Text];
                }
                else
                {
                    if (token.TokenType == TokenType.RegularCode)
                    {
                        token.DecodedText = "??";
                    }
                    else if(token.TokenType == TokenType.VocabularyElement)
                    {
                        token.DecodedText = "???";
                    }
                    else
                    {
                        token.DecodedText = null;
                    }
                }
            }
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        protected void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, null, new GuiLogEventArgs(message, null, logLevel));
        }

    }
}
