﻿/*
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

namespace Cryptool.Plugins.DECODEDatabaseTools
{
    public class Decoder
    {
        private string _DECODEKeyDocument;
        private Dictionary<Token, Token> _keyMapping = new Dictionary<Token, Token>();
        private List<Token> _nulls = new List<Token>();

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
                Token left = new Token(null, parts[0].Trim());

                string rightString = "";
                for (int i = 1; i < parts.Length; i++)
                {
                    rightString += parts[i];
                    if (i < parts.Length - 1)
                    {
                        rightString += " - ";
                    }
                }
                rightString = rightString.Trim();

                if (rightString.ToLower().Equals("<null>"))
                {
                    if (!_nulls.Contains(left))
                    {
                        _nulls.Add(left);
                    }
                    else
                    {
                        GuiLogMessage(String.Format("Found a remapping in the key. Ignoring it: {0}", trimmedLine), NotificationLevel.Warning);
                    }
                    continue;
                }

                Token right = new Token(null, rightString);               
                if (_keyMapping.ContainsKey(left))
                {
                    GuiLogMessage(String.Format("Found a remapping in the key. Ignoring it: {0}", trimmedLine), NotificationLevel.Warning);
                    continue;
                }

                                
                _keyMapping.Add(left, right);
            }
        }

        public List<Token> GetNulls()
        {
            return _nulls;
        }

        /// <summary>
        /// Returns the internal key/value dictionary of this decoder
        /// </summary>
        /// <returns></returns>
        public Dictionary<Token, Token> GetKeyDictionary()
        {
            return _keyMapping;
        }

        /// <summary>
        /// Returns all key tokens of this decoder
        /// </summary>
        /// <returns></returns>
        public List<Token> GetKeyTokens()
        {
            return new List<Token>(_keyMapping.Keys);
        }

        /// <summary>
        /// Returns all value tokens of this decoder
        /// </summary>
        /// <returns></returns>
        public List<Token> GetValueTokens()
        {
            return new List<Token>(_keyMapping.Values);
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
                    Symbol nullSymbol = new Symbol(token);
                    nullSymbol.Text = " ";
                    token.DecodedSymbols.Add(nullSymbol);
                    continue;
                }
                if (_keyMapping.ContainsKey(token))
                {
                    token.DecodedSymbols = _keyMapping[token].Symbols;
                }
                else
                {
                    if (token.TokenType == TokenType.RegularCode)
                    {
                        Symbol unknownSymbol = new Symbol(token);
                        unknownSymbol.Text = "??";
                        token.DecodedSymbols.Add(unknownSymbol);
                    }
                    else if(token.TokenType == TokenType.VocabularyElement)
                    {
                        Symbol unknownSymbol = new Symbol(token);
                        unknownSymbol.Text = "???";
                        token.DecodedSymbols.Add(unknownSymbol);
                    }
                    else
                    {
                        token.DecodedSymbols = null;
                    }
                }
            }
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        protected void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, null, new GuiLogEventArgs(message, null, logLevel));
        }

        /// <summary>
        /// Returns the maximum token length of the key tokens
        /// </summary>
        /// <returns></returns>
        public int GetMaximumTokenLength()
        {
            int maxTokenLength = 0;
            foreach(var key in _keyMapping)
            {
                if(key.Key.Symbols.Count > maxTokenLength)
                {
                    maxTokenLength = key.Key.Symbols.Count;
                }
            }
            return maxTokenLength;
        }
    }
}
