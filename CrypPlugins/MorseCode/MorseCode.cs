﻿/*
   Copyright 2013 Nils Kopal <Nils.Kopal@Uni-Kassel.de>

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

using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using MorseCode;


namespace Cryptool.Plugins.MorseCode
{
    [Author("Nils Kopal", "Nils.Kopal@Uni-Kassel.de", "Universität Kassel", "http://www.uc.uni-kassel.de/")]
    [PluginInfo("MorseCode.Properties.Resources", "PluginCaption", "PluginTooltip", "MorseCode/userdoc.xml", new[] { "MorseCode/icon.png" })]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class MorseCode : ICrypComponent
    {
        //private SoundPlayer _player = new SoundPlayer();

        /// <summary>
        /// Constructs our mapping and creates our MorseCode object
        /// </summary>
        public MorseCode()
        {
            //Generate our mapping between letters and the morse code
            //Obtained from http://de.wikipedia.org/wiki/Morsecode
            // We use the ITU standard:
            _mapping.Add(' ',"/");
            _mapping.Add('A',".-");
            _mapping.Add('B',"-...");
            _mapping.Add('C',"-.-.");
            _mapping.Add('D',"-..");
            _mapping.Add('E',".");
            _mapping.Add('F',"..-.");
            _mapping.Add('G',"--.");
            _mapping.Add('H',"....");
            _mapping.Add('I',"..");
            _mapping.Add('J',".---");
            _mapping.Add('K',"-.-");
            _mapping.Add('L',".-..");
            _mapping.Add('M',"--");
            _mapping.Add('N',"-.");
            _mapping.Add('O',"---");
            _mapping.Add('P',".--.");
            _mapping.Add('Q',"--.-");
            _mapping.Add('R',".-.");
            _mapping.Add('S',"...");
            _mapping.Add('T',"-");
            _mapping.Add('U',"..-");
            _mapping.Add('V',"...-");
            _mapping.Add('W',".--");
            _mapping.Add('X',"-..-");
            _mapping.Add('Y',"-.--");
            _mapping.Add('Z',"--..");
            _mapping.Add('0',"-----");
            _mapping.Add('1',".----");
            _mapping.Add('2',"..---");
            _mapping.Add('3',"...--");
            _mapping.Add('4',"....-");
            _mapping.Add('5',".....");
            _mapping.Add('6',"-....");
            _mapping.Add('7',"--...");
            _mapping.Add('8',"---..");
            _mapping.Add('9',"----.");
            _mapping.Add('À',".--.-");
            _mapping.Add('Å',".--.-");
            _mapping.Add('Ä',".-.-");
            _mapping.Add('È',".-..-");
            _mapping.Add('É',"..-..");
            _mapping.Add('Ö',"---.");
            _mapping.Add('Ü',"..--");
            _mapping.Add('ß',"...--..");
            _mapping.Add('Ñ',"--.--");
            _mapping.Add('.',".-.-.-");
            _mapping.Add(',',"--..--");
            _mapping.Add(':',"---...");
            _mapping.Add(';',"-.-.-.");
            _mapping.Add('?',"..--..");
            _mapping.Add('-',"-....-");
            _mapping.Add('_',"..--.-");
            _mapping.Add('(',"-.--.");
            _mapping.Add(')',"-.--.-");
            _mapping.Add('\'',".----.");
            _mapping.Add('=',"-...-");
            _mapping.Add('+',".-.-.");
            _mapping.Add('/',"-..-.");
            _mapping.Add('@',".--.-.");            
        }

        #region Private Variables

        private readonly MorseCodeSettings _settings = new MorseCodeSettings();
        private readonly Dictionary<char, string> _mapping = new Dictionary<char, string>();
        private bool _stopped = false;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputTextCaption", "InputTextTooltip")]
        public string InputText
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputTextCaption", "OutputTextTooltip")]
        public string OutputText
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings
        {
            get { return _settings; }
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation
        {
            get { return null; }
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            _stopped = false;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {            
            ProgressChanged(0, 1);
            if (string.IsNullOrEmpty(InputText) || string.IsNullOrWhiteSpace(InputText))
            {
                OutputText = "";
                OnPropertyChanged("OutputText");
            }
            else
            {
                switch (_settings.Action)
                {
                    case 0:
                        Encode();
                        break;
                    case 1:
                        Decode();
                        break;
                    case 2:
                        Play();
                        break;
                }
            }
            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Plays the given Morse Code
        /// </summary>
        private void Play()
        {            
            using(Stream ditStream = new MemoryStream())
            using(Stream daStream = new MemoryStream())
            using (Stream silenceStream = new MemoryStream())
            {
                var ditPlayer = new SoundPlayer(ditStream);
                var daPlayer = new SoundPlayer(daStream);
                var silencePlayer = new SoundPlayer(silenceStream);
                var dit = new Wave();
                dit.GenerateSound(256, 600, 50);
                dit.WriteToStream(ditStream);
                var dah = new Wave();
                dah.GenerateSound(256, 600, 100);
                dah.WriteToStream(daStream);
                var silence = new Wave();
                silence.GenerateSound(256, 0, 100);
                silence.WriteToStream(silenceStream);
                foreach (char c in InputText)
                {
                    if(_stopped)
                    {
                        ditPlayer.Stop();
                        daPlayer.Stop();
                        silencePlayer.Stop();
                        return;
                    }
                    if (c == '.')
                    {
                        ditStream.Position = 0;
                        ditPlayer.PlaySync();        
                    }
                    else if (c == '-')
                    {
                        daStream.Position = 0;
                        daPlayer.PlaySync();  
                    }
                    else
                    {
                        silenceStream.Position = 0;
                        silencePlayer.PlaySync(); 
                    }
                }
                ditPlayer.Stop();
                daPlayer.Stop(); 
                silencePlayer.Stop();
            }            
        }

        /// <summary>
        /// Creates an output containing the morse code corresponding to the input text
        /// </summary>
        private void Encode()
        {
            var builder = new StringBuilder();
            var uppertext = InputText.Trim().ToUpper();
            for (int i = 0; i < uppertext.Length; i++)
            {
                if (_mapping.ContainsKey(uppertext[i]))
                {
                    //we found a correspinding morse code and put it into the output
                    builder.Append(_mapping[uppertext[i]]);
                    builder.Append(" ");
                }
                else if (uppertext[i] == '\r' && uppertext[i + 1] == '\n')
                {
                    //we detect a linebreak and put it directly into the output
                    builder.Append("\r\n");
                    i++;
                }
                else
                {
                    //no corresponding morse code exists for the found character
                    builder.Append("? ");
                }
            }
            //finally we have to remove the last added space
            builder.Remove(builder.Length - 1, 1);
            OutputText = builder.ToString();
            OnPropertyChanged("OutputText");
        }

        /// <summary>
        /// Creates an output containing the text corresponding to the input morse code
        /// </summary>
        private void Decode()
        {
            var builder = new StringBuilder();
            string[] tokens = InputText.Split(' ');
            foreach(string token in tokens)
            {
                
                if(_mapping.ContainsValue(token))
                {
                    //We have no leading or trailing linebreaks because we directly found the token
                    //So we do not need to add line breaks
                    builder.Append(GetKeyFromValue(token));
                }
                else if(token.StartsWith("\r\n") || token.EndsWith("\r\n"))
                {
                    //1. Count leading linebreaks in our token and put them into the output
                    int count = CountStringOccurence(token.TrimEnd(), "\r\n");
                    for (int i = 0; i < count; i++)
                    {
                        builder.Append("\r\n");
                    }
                    //2. Now replace the trimmed token by its corresponding value
                    builder.Append(GetKeyFromValue(token.Trim()));
                    //3. Now count trailing linebreaks and put them into the output
                    count = CountStringOccurence(token.TrimStart(), "\r\n");
                    for (int i = 0; i < count; i++)
                    {
                        builder.Append("\r\n");
                    }
                }
                else
                {
                    //we found a token without linebreaks and we do not have a corresponding
                    //character value
                    builder.Append("?");
                }
            }           
            OutputText = builder.ToString();
            OnPropertyChanged("OutputText");
        }

        /// <summary>
        /// Looks up the mappings and returns a key which belongs to a given value
        /// This is needed to decode a given morse code
        /// If it does not find the given value it returns a '?'
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private char GetKeyFromValue(string token)
        {
            foreach(var s in _mapping)
            {
                if(s.Value.Equals(token))
                {
                    return s.Key;
                }
            }
            return '?';
        }

        /// <summary>
        /// Counts the Occurence of a given stringToSearchFor in a StringToCheck
        /// </summary>
        /// <param name="stringToCheck"></param>
        /// <param name="stringToSearchFor"></param>
        /// <returns></returns>
        public int CountStringOccurence(string stringToCheck, string stringToSearchFor)
        {
            var collection = Regex.Matches(stringToCheck, stringToSearchFor);
            return collection.Count;
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            _stopped = true;
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion
        
        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}
