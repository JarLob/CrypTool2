﻿/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

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

#define _DEBUG_

using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System;
using Cryptool.PluginBase.Attributes;
using System.Windows.Threading;
using System.Threading;
using Keccak.Properties;
using System.Windows;


namespace Cryptool.Plugins.Keccak
{
    [Author("Max Brandi", "max.brandi@rub.de", null, null)]
    [PluginInfo("Keccak.Properties.Resources", "PluginCaption", "PluginDescription", "Keccak/Documentation/doc.xml", new[] { "CrypWin/images/default.png" })]
    [ComponentCategory(ComponentCategory.HashFunctions)]
    [ComponentVisualAppearance(ComponentVisualAppearance.VisualAppearanceEnum.Opened)]
    public class Keccak : ICrypComponent
    {
        #region Private Variables

        private KeccakPres pres = new KeccakPres();
        private Encoding encoding = Encoding.UTF8;
        private readonly KeccakSettings settings = new KeccakSettings();
        private bool execute = true;

        #endregion
        
        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputStreamCaption", "InputDataStreamTooltip", true)]
        public ICryptoolStream InputStream
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputStreamCaption", "OutputDataStreamTooltip", true)]
        public ICryptoolStream OutputStream
        {
            get;
            set;
        }

        #if _DEBUG_
        [PropertyInfo(Direction.OutputData, "DebugStreamCaption", "DebugDataStreamTooltip", true)]
        public ICryptoolStream DebugStream
        {
            get;
            set;
        }
        #endif


        #endregion

        public void Execute()
        {
            /* do not execute if checks in PreExecution() failed */
            if (!execute)
                return;

            ProgressChanged(0, 1);

            byte[] input, output;
            int outputLength, rate, capacity;

            /* setup output stream writer */
            CStreamWriter OutputStreamwriter = new CStreamWriter();
            OutputStream = OutputStreamwriter;
            OnPropertyChanged("OutputStream");

            #if _DEBUG_
            /* setup debug stream writer */
            TextWriter consoleOut = Console.Out;    // save the standard output
            CStreamWriter debugStream = new CStreamWriter();
            StreamWriter debugStreamWriter = new StreamWriter(debugStream);
            debugStreamWriter.AutoFlush = true;     // flush stream every time WriteLine is called
            Console.SetOut(debugStreamWriter);
            DebugStream = debugStream;
            OnPropertyChanged("DebugStream");
            #endif

            #region get input

            /* read input */
            using (CStreamReader reader = InputStream.CreateReader())
            {
                int bytesRead;
                byte[] buffer = new byte[128];      // buffer of length 128 byte

                MemoryStream stream = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(stream);
                
                while ((bytesRead = reader.Read(buffer)) > 0)
                {
                    bw.Write(buffer, 0, bytesRead);
                }

                bw.Close();
                input = stream.ToArray();
                OnPropertyChanged("OutputStream");
            }

            #endregion

            outputLength = settings.OutputLength;
            rate = settings.Rate;
            capacity = settings.Capacity;

            /* show presentation intro */
            #region presentation intro

            if (pres.IsVisible)
            {
                /* clean up last round */
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.absorbGrid.Visibility = Visibility.Hidden;
                    pres.imgBlankPage.Visibility = Visibility.Hidden;
                    pres.labelOutput.Visibility = Visibility.Hidden;
                    pres.textBlockStateBeforeAbsorb.Text = "";
                    pres.textBlockBlockToAbsorb.Text = "";
                    pres.textBlockStateAfterAbsorb.Text = "";
                    pres.labelCurrentPhase.Content = "";
                    pres.labelCurrentStep.Content = "";
                    pres.textBlockExplanation.Text = "";
                    pres.textBlockParametersNotSupported.Visibility = Visibility.Hidden;
                    pres.textBlockParametersNotSupported.Text = "";
                    pres.textBlockStepPresentationNotAvailable.Visibility = Visibility.Hidden;
                    pres.textBlockStepPresentationNotAvailable.Text = "";

                    pres.buttonNext.IsEnabled = true;
                    pres.buttonSkip.IsEnabled = false;
                    pres.buttonAutostep.IsEnabled = false;
                    pres.buttonSkipPermutation.IsEnabled = false;
                    pres.autostepSpeedSlider.IsEnabled = false;
                    pres.spButtons.Visibility = Visibility.Visible;
                    pres.buttonSkipIntro.Visibility = Visibility.Visible;
                }, null);

                #region check if selected parameters are supported
                if (rate + capacity < 200)
                {
                    pres.skipPresentation = true;

                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.textBlockParametersNotSupported.Text = Resources.PresStateSizeError;
                        pres.textBlockParametersNotSupported.Visibility = Visibility.Visible;
                    }, null);
                }
                else if (outputLength > rate)
                {
                    pres.skipPresentation = true;

                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.textBlockParametersNotSupported.Text = Resources.PresOutputLengthError;
                        pres.textBlockParametersNotSupported.Visibility = Visibility.Visible;
                    }, null);
                }
                #endregion

                AutoResetEvent buttonNextClickedEvent = pres.buttonNextClickedEvent;

                if (!pres.skipPresentation && !pres.skipIntro)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.imgIntroFirstPage.Visibility = Visibility.Visible;
                    }, null);

                    buttonNextClickedEvent.WaitOne();
                }

                if (!pres.skipPresentation && !pres.skipIntro)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.imgIntroFirstPage.Visibility = Visibility.Hidden;
                        pres.labelIntroIntroduction.Visibility = Visibility.Visible;
                    }, null);

                    buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }

                if (!pres.skipPresentation && !pres.skipIntro)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.labelCurrentPhase.Content = Resources.PresIntroduction;
                        pres.labelCurrentStep.Content = Resources.PresInitialization;

                        pres.textBlockIntroduction.Text = string.Format(Resources.PresInitializationText, (rate + capacity), capacity, rate);

                        pres.labelIntroIntroduction.Visibility = Visibility.Hidden;
                        pres.imgIntroSpongeInit.Visibility = Visibility.Visible;
                    }, null);

                    buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }

                if (!pres.skipPresentation && !pres.skipIntro)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.labelCurrentStep.Content = Resources.PresAbsorbingPhase;

                        pres.textBlockIntroduction.Text =
                            string.Format(Resources.PresAbsorbingPhaseText, rate);

                        pres.imgIntroSpongeInit.Visibility = Visibility.Hidden;
                        pres.imgIntroSpongeAbsorb.Visibility = Visibility.Visible;
                    }, null);

                    buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }

                if (!pres.skipPresentation && !pres.skipIntro)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.labelCurrentStep.Content = Resources.PresSqueezingPhase;
                        pres.textBlockIntroduction.Text = Resources.PresSqueezingPhaseText;

                        pres.imgIntroSpongeAbsorb.Visibility = Visibility.Hidden;
                        pres.imgIntroSpongeSqueeze.Visibility = Visibility.Visible;
                    }, null);

                    buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }

                if (!pres.skipPresentation && !pres.skipIntro)
                {
                    /* calculate l parameter */
                    int l = (int)Math.Log((double)((rate + capacity) / 25), 2);

                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.labelCurrentStep.Content = Resources.PresKeccakFPhase;
                        pres.textBlockIntroduction.Text = string.Format(Resources.PresKeccakFPhaseText, l, (12 + 2 * l));
                        pres.labelIntroIterationAmount.Content = string.Format(Resources.PresKeccakFIterations, (12 + 2 * l));

                        pres.imgIntroSpongeSqueeze.Visibility = Visibility.Hidden;
                        pres.imgIntroSpongeKeccakf2.Visibility = Visibility.Visible;
                    }, null);

                    buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }

                if (!pres.skipPresentation && !pres.skipIntro)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.labelIntroIterationAmount.Content = "";
                        pres.imgIntroSpongeKeccakf2.Visibility = Visibility.Hidden;
                        pres.imgIntroStateMapping.Visibility = Visibility.Visible;
                        pres.textBlockIntroduction.Text = string.Format(Resources.PresKeccakFStateMapping, (capacity + rate), (capacity + rate) / 25);
                    }, null);

                    buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }

                if (!pres.skipPresentation && !pres.skipIntro)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.buttonSkipIntro.Visibility = Visibility.Hidden;
                        pres.imgIntroStateMapping.Visibility = Visibility.Hidden;
                        pres.labelIntroExecution.Visibility = Visibility.Visible;
                        pres.textBlockIntroduction.Text = "";
                        pres.labelCurrentPhase.Content = "";
                        pres.labelCurrentStep.Content = "";
                    }, null);

                    buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }

                if (!pres.skipPresentation && !pres.skipIntro)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.labelIntroExecution.Visibility = Visibility.Hidden;
                    }, null);
                }

                if (pres.skipPresentation || pres.skipIntro)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.labelIntroIterationAmount.Content = "";
                        pres.imgIntroFirstPage.Visibility = Visibility.Hidden;
                        pres.imgIntroIntroduction.Visibility = Visibility.Hidden;
                        pres.imgIntroSpongeInit.Visibility = Visibility.Hidden;
                        pres.imgIntroSpongeAbsorb.Visibility = Visibility.Hidden;
                        pres.imgIntroSpongeSqueeze.Visibility = Visibility.Hidden;
                        pres.imgIntroSpongeKeccakf2.Visibility = Visibility.Hidden;
                        pres.imgIntroStateMapping.Visibility = Visibility.Hidden;
                        pres.imgIntroExecution.Visibility = Visibility.Hidden;
                        pres.buttonSkipIntro.Visibility = Visibility.Hidden;
                        pres.labelIntroIntroduction.Visibility = Visibility.Hidden;
                        pres.labelIntroExecution.Visibility = Visibility.Hidden;
                        pres.textBlockIntroduction.Text = "";
                    }, null);
                }
            }
            
            #endregion

            /* hash input */
            output = KeccakHashFunction.Hash(input, outputLength, rate, capacity, ref pres, this);

            /* write output */
            OutputStreamwriter.Write(output);
            OutputStreamwriter.Close();

            #if _DEBUG_
            /* close debug stream and reset standard output */
            debugStreamWriter.Close();
            Console.SetOut(consoleOut);
            #endif
            
            /* hide button panel */
            if (pres.IsVisible)
            {
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.spButtons.Visibility = Visibility.Hidden;
                }, null);
            }

            ProgressChanged(1, 1);
        }

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation
        {
            get { return pres; }
        }
        
        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            bool stateSizeOk = (settings.GetStateSize() == settings.Rate + settings.Capacity);
            bool outputLengthOk = settings.OutputLength % 8 == 0;
            bool outputLengthTruncated = settings.OutputLengthTruncated();
            

            if (stateSizeOk && outputLengthOk)
            {
                if (outputLengthTruncated)
                {
                    GuiLogMessage(Resources.OutputTooLongWarning, NotificationLevel.Warning);
                }

                return;
            }
            else
            {
                if (!stateSizeOk)
                {
                    GuiLogMessage(Resources.StateSizeMatchError, NotificationLevel.Error);
                }
                if (!outputLengthOk)
                {
                    GuiLogMessage(Resources.OutputMatchError, NotificationLevel.Error);
                }
                execute = false;
            }
        }

        // public void Execute()
        // {
        // }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            execute = true;
            pres.autostep = false;
            pres.skipStep = false;
            pres.skipPresentation = false;
            pres.skipIntro = false;
            pres.buttonNextClickedEvent.Reset();
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            pres.buttonNextClickedEvent.Set();
            pres.skipPresentation = true;
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
            settings.UpdateTaskPaneVisibility();
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

        public void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}