/*
   Copyright 2008-2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Attributes;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.Numerics;
using System.Windows.Documents;
using DiffMatchPatch;
using System.Windows.Media;

namespace TextOutput
{
    [Author("Thomas Schmid", "thomas.schmid@cryptool.org", "Uni Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("TextOutput.Properties.Resources", "PluginCaption", "PluginTooltip", "TextOutput/DetailedDescription/doc.xml", "TextOutput/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    [ComponentVisualAppearance(ComponentVisualAppearance.VisualAppearanceEnum.Opened)]
    public class TextOutput : DependencyObject, ICrypComponent
    {
        #region Fields and properties

        /// <summary>
        /// This dic is used to store error messages while properties are set in PlayMode. The messages
        /// will be sent in the execute method.
        /// The editor flushes plugin color markers before calling the execute method.
        /// So these messages would still appear in LogWindow, but the color marker of the
        /// plugin (red/yellow) would be lost if sending the messages right on property set.
        /// </summary>
        private Dictionary<string, NotificationLevel> dicWarningsAndErros = new Dictionary<string, NotificationLevel>();
        private TextOutputPresentation textOutputPresentation;

        private TextOutputSettings settings;
        public ISettings Settings
        {
            get { return settings; }
            set { settings = (TextOutputSettings)value; }
        }

        private object input;

        [PropertyInfo(Direction.InputData, "InputCaption", "InputTooltip", true)]
        public object Input
        {
            get
            {
                return input;
            }
            set
            {
                try
                {
                    Progress(0, 1);
                    input = value;
                    if (input != null) ShowInPresentation(input);
                    Progress(1, 1);
                    OnPropertyChanged("Input");
                }
                catch(Exception ex)
                {
                    AddMessage(ex.Message, NotificationLevel.Error);
                }
            }
        }

        private string _currentValue;
        public string CurrentValue
        {
            get { return _currentValue; }
            private set 
            {
                _currentValue = value;
                OnPropertyChanged("CurrentValue");
            }
        }

        #endregion

        #region Event handling

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PropertyChangedEventHandler PropertyChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void settings_OnGuiLogNotificationOccured(IPlugin sender, GuiLogEventArgs args)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(args.Message, this, args.NotificationLevel));
        }

        private void Progress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion events

        #region Constructor and implementation

        public TextOutput()
        {
            settings = new TextOutputSettings(this);
            settings.PropertyChanged += settings_OnPropertyChanged;

            textOutputPresentation = new TextOutputPresentation();
            setStatusBar();
        }

        private void settings_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ShowChars" || e.PropertyName == "ShowLines" || e.PropertyName == "ShowDigits")
            {
                setStatusBar();
                //textOutputPresentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                //{
                //    setStatusBar();
                //}, null);
            }
        }

        Thread statusBarThread = null;
        private void setStatusBar()
        {
            try
            {
                if (statusBarThread != null && statusBarThread.IsAlive)
                    statusBarThread.Abort();
            }
            catch (Exception ex)
            {
            }

            statusBarThread = new Thread(() => setStatusBar_invoke());
            statusBarThread.IsBackground = true;
            statusBarThread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
            statusBarThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
            statusBarThread.Start();
        }

        private void setStatusBar_invoke()
        {
            textOutputPresentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                setStatusBar_orig();
            }, null);
        }

        private byte[] ConvertStreamToByteArray( ICryptoolStream stream )
        {
            CStreamReader reader = stream.CreateReader();
            reader.WaitEof(); // does not support chunked streaming

        	if (reader.Length > settings.MaxLength)
	            AddMessage("WARNING - Stream is too large (" + (reader.Length / 1024).ToString("0.00") + " kB), output will be truncated to " + (settings.MaxLength / 1024).ToString("0.00") + "kB", NotificationLevel.Warning);
	        
            byte[] byteArray = new byte[ Math.Min(settings.MaxLength, reader.Length) ];
	        reader.Seek(0, SeekOrigin.Begin);
	        reader.ReadFully(byteArray, 0, byteArray.Length);
            reader.Close();

            return byteArray;
        }


        private byte[] GetByteArray(byte[] byteArray)
        {
            if (byteArray.Length <= settings.MaxLength)
                return byteArray;

            AddMessage("WARNING - Byte array is too large (" + (byteArray.Length / 1024).ToString("0.00") + " kB), output will be truncated to " + (settings.MaxLength / 1024).ToString("0.00") + "kB", NotificationLevel.Warning);
            
            byte[] truncatedByteArray = new byte[settings.MaxLength];
            Buffer.BlockCopy(byteArray, 0, truncatedByteArray, 0, settings.MaxLength);

            return truncatedByteArray;
        }

        private void ShowInPresentation(object value)
        {
            //textOutputPresentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            //{
            //    if (!settings.Append)
            //        textOutputPresentation.textBox.Text = null;
            //    clearStatusBar();
            //}, null);

            if (value == null) return;

            string fillValue;

	        if (value is string)
            {
                fillValue = (string)value;
            }
            else if (value is byte[])
            {
                byte[] byteArray = GetByteArray((byte[])value);
                fillValue = BitConverter.ToString(byteArray).Replace("-", " ");
            }
            else if (value is ICryptoolStream)
            {
                byte[] byteArray = ConvertStreamToByteArray((ICryptoolStream)value);
                fillValue = BitConverter.ToString(byteArray).Replace("-", " ");
            }
            else if (value is System.Collections.IEnumerable)
            {
                var enumerable = value as System.Collections.IEnumerable;

                List<string> s = new List<string>();
                foreach (var obj in enumerable)
                    s.Add((obj == null ? "null" : obj.ToString()));

                fillValue = String.Join("\r",s);
            }
            else if (value is BigInteger)
            {
                //fillValue = BigIntegerHelper.ToBaseString((BigInteger)value, 10);
                fillValue = value.ToString();   // ~ 2x faster than ToBaseString
            }
            else
            {
                fillValue = value.ToString();
            }

            if (fillValue.Length > settings.MaxLength)
            {
                AddMessage("WARNING - String is too large (" + (fillValue.Length / 1024).ToString("0.00") + " kB), output will be truncated to " + (settings.MaxLength / 1024).ToString("0.00") + "kB", NotificationLevel.Warning);
                fillValue = fillValue.Substring(0, settings.MaxLength);
            }            
            
            Presentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {                
                string oldtext = (CurrentValue == null ? String.Empty : CurrentValue);
                string newtext = String.Empty;
                if (settings.Append)
                {
                    // append line breaks only if not first line
                    if (!string.IsNullOrEmpty(oldtext))
                    {
                        for (int i = 0; i < settings.AppendBreaks; i++)
                            textOutputPresentation.textBox.AppendText("\r");
                    }                    
                    textOutputPresentation.textBox.AppendText(fillValue);
                    textOutputPresentation.textBox.ScrollToEnd();
                    newtext = new TextRange(textOutputPresentation.textBox.Document.ContentStart, textOutputPresentation.textBox.Document.ContentEnd).Text;                    
                }
                else
                {
                    textOutputPresentation.textBox.Document = new FlowDocument();
                    fillValue = fillValue.Replace("\n", "");
                    textOutputPresentation.textBox.AppendText(fillValue);
                    newtext = new TextRange(textOutputPresentation.textBox.Document.ContentStart, textOutputPresentation.textBox.Document.ContentEnd).Text;                    
                }

                if (settings.ShowChanges == 1 || settings.ShowChanges == 2)
                {
                    var diff = new diff_match_patch();
                    var diffs = diff.diff_main(oldtext, newtext, true);
                    diff.diff_cleanupSemanticLossless(diffs);

                    textOutputPresentation.textBox.Document = new FlowDocument();
                    var para = new Paragraph();
                    foreach (var d in diffs)
                    {
                        switch (d.operation)
                        {
                            case Operation.EQUAL:
                                para.Inlines.Add(new Run(d.text));
                                break;
                            case Operation.INSERT:
                                if (settings.ShowChanges == 1)
                                {
                                    var run = new Run(d.text);
                                    run.Background = new SolidColorBrush(Colors.LightBlue);
                                    para.Inlines.Add(run);
                                }
                                else if (settings.ShowChanges == 2)
                                {
                                    var run = new Run(d.text);
                                    run.Background = new SolidColorBrush(Colors.LightGreen);
                                    para.Inlines.Add(run);
                                }
                                break;
                            case Operation.DELETE:
                                if (settings.ShowChanges == 2 && d.text.Trim().Length > 0)
                                {
                                    var run = new Run(d.text);
                                    run.Background = new SolidColorBrush(Color.FromRgb((byte)0xF3, (byte)0x6D, (byte)0x74));
                                    para.Inlines.Add(run);
                                }
                                break;
                        }
                    }
                    textOutputPresentation.textBox.Document.Blocks.Add(para);
                }
                else if(settings.ShowChanges == 3)
                {
                    textOutputPresentation.textBox.Document = new FlowDocument();
                    var para = new Paragraph();
                    var position = 0;
                    while (position < newtext.Length)
                    {
                        var run = new Run("" + newtext[position]);
                        if (oldtext.Length == 0 || position > oldtext.Length || (position < oldtext.Length && oldtext[position] != newtext[position]))
                        {
                            run.Background = new SolidColorBrush(Colors.LightBlue);
                        }
                        para.Inlines.Add(run);
                        position++;
                    }
                    textOutputPresentation.textBox.Document.Blocks.Add(para);
                }
                CurrentValue = newtext;
                setStatusBar();

            }, fillValue);

            //if the presentation is visible we wait some ms to avoid a "hanging" of the application
            if (Presentation.IsVisible)
            {
                try
                {
                    Thread.Sleep(5);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(String.Format("Error during Thread.Sleep of TextOutput: {0}", ex.Message), NotificationLevel.Warning);
                }
            }
        }

        void clearStatusBar()
        {
            textOutputPresentation.labelBytes.Content = "";
        }

        void setStatusBar_orig()
        {
            // create status line string
            textOutputPresentation.labelBytes.Content = "...";

            string currentText = new TextRange(textOutputPresentation.textBox.Document.ContentStart, textOutputPresentation.textBox.Document.ContentEnd).Text;
            string label = "";

            if (settings.ShowDigits)
            {
                if (Input is string)
                {
                    var value = Input as string;
                    if (Regex.IsMatch(value, @"^-?\d+$"))
                    {
                        try
                        {
                            input = BigIntegerHelper.Parse(value, 10);
                        }
                        catch (Exception)
                        {
                            //wtf ?
                        }
                    }
                }

                if (Input is Int16) input = (BigInteger)(int)(Int16)Input;
                else if (Input is Int32) input = (BigInteger)(int)(Int32)Input;
                else if (Input is byte) input = (BigInteger)(byte)Input;

                if (Input is BigInteger)
                {
                    int digits = 0;
                    int bits;
                    try
                    {
                        BigInteger number = (BigInteger)input;
                        double log2 = BigInteger.Log(BigInteger.Abs(number),2.0);
                        if (log2 < 10000000)
                        {
                            bits = number.BitCount();
                        }
                        else
                        {
                            bits = (int)System.Math.Ceiling(log2);
                            //bits = (int)(System.Math.Ceiling(log / System.Math.Log(2, 10)));
                            //digits = (int)(System.Math.Ceiling(log) + 0.5);
                            //digits = BigInteger.Abs(number).ToString().Length;
                        }
                        digits = currentText.Length;
                        if (number < 0) digits--;
                    }
                    catch (Exception)
                    {
                        digits = 0;
                        bits = 0;
                    }
                    string digitText = (digits == 1) ? Properties.Resources.Digit : Properties.Resources.Digits;
                    string bitText = (bits == 1) ? Properties.Resources.Bit : Properties.Resources.Bits;
                    label = string.Format(" {0:#,0} {1}, {2:#,0} {3}", digits, digitText, bits, bitText);
                    textOutputPresentation.labelBytes.Content = label;
                    return;
                }
            }

            if (settings.ShowChars)
            {
                currentText = currentText.Replace("\r", "\r\n");
                int chars = (currentText == null) ? 0 : currentText.Length - 3;
                string entity = (chars == 1) ? Properties.Resources.Char : Properties.Resources.Chars;
                label += string.Format(" {0:#,0} " + entity, chars);
            }

            if (settings.ShowLines)
            {
                int lines = 0;
                if (currentText != null && currentText.Length > 0)
                {
                    lines = new Regex(System.Environment.NewLine, RegexOptions.Multiline).Matches(currentText).Count;
                    if (currentText[currentText.Length - 1] != '\n') lines++;
                }
                string entity = (lines == 1) ? Properties.Resources.Line : Properties.Resources.Lines;
                if (label != "") label += ", ";
                label += string.Format(" {0:#,0} " + entity, lines);
            }

            textOutputPresentation.labelBytes.Content = label;
        }

        private void AddMessage(string message, NotificationLevel level)
        {
            if (!dicWarningsAndErros.ContainsKey(message))
                dicWarningsAndErros.Add(message, level);
        }

        #endregion

        #region IPlugin Members

        public UserControl Presentation
        {
            get { return textOutputPresentation; }
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        public void Stop()
        {
            if (statusBarThread != null && statusBarThread.IsAlive)
                statusBarThread.Abort();
        }

        public void PreExecution()
        {
            textOutputPresentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                textOutputPresentation.textBox.Document = new FlowDocument();
                //input = null;
                clearStatusBar();
            }, null);
        }

        public void PostExecution()
        {
        }

        public void Execute()
        {
            Progress(100, 100);
            foreach (KeyValuePair<string, NotificationLevel> kvp in dicWarningsAndErros)
            {
                GuiLogMessage(kvp.Key, kvp.Value);
            }
            dicWarningsAndErros.Clear();
        }

        #endregion
    }
}