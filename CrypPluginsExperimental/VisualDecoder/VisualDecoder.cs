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

using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.Plugins.VisualDecoder.Decoders;
using Cryptool.Plugins.VisualDecoder.Model;
using ZXing;

namespace Cryptool.Plugins.VisualDecoder
{
    [Author("Christopher Konze", "Christopher.Konze@cryptool.org", "University of Kassel", "http://www.uni-kassel.de/eecs/")]
    [PluginInfo("VisualDecoder.Properties.Resources", "VisualDecoderCaption", "VisualDecoderCaptionTooltip", "VisualDecoder/userdoc.xml", new[] { "VisualDecoder/Images/icon.png" })]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class VisualDecoder : ICrypComponent
    {

        #region Private Variables

       private readonly VisualDecoderPresentation presentation = new VisualDecoderPresentation();
        private readonly VisualDecoderSettings settings = new VisualDecoderSettings();
        private Thread decodingThread = null;
        private readonly ParameterizedThreadStart threadStart;
        private bool codeFound = false;
        
        // decoder chain
        private readonly Dictionary<VisualDecoderSettings.DimCodeType, DimCodeDecoder> codeTypeHandler = 
                                                        new Dictionary<VisualDecoderSettings.DimCodeType, DimCodeDecoder>();
        
       
        #endregion

        public VisualDecoder()
        {

            threadStart = new ParameterizedThreadStart(ProcessImage);

            //init chain
            codeTypeHandler.Add(VisualDecoderSettings.DimCodeType.EAN8, new ZXingDecoder(this, BarcodeFormat.EAN_8));
            codeTypeHandler.Add(VisualDecoderSettings.DimCodeType.EAN13, new ZXingDecoder(this, BarcodeFormat.EAN_13));
            codeTypeHandler.Add(VisualDecoderSettings.DimCodeType.Code39, new ZXingDecoder(this, BarcodeFormat.CODE_39));
            codeTypeHandler.Add(VisualDecoderSettings.DimCodeType.Code128, new ZXingDecoder(this, BarcodeFormat.CODE_128));
            codeTypeHandler.Add(VisualDecoderSettings.DimCodeType.QRCode, new ZXingDecoder(this, BarcodeFormat.QR_CODE));
            codeTypeHandler.Add(VisualDecoderSettings.DimCodeType.PDF417, new ZXingDecoder(this, BarcodeFormat.PDF_417));
            codeTypeHandler.Add(VisualDecoderSettings.DimCodeType.DataMatrix, new ZXingDecoder(this, BarcodeFormat.DATA_MATRIX));
        }


        #region Data Properties

        [PropertyInfo(Direction.InputData, "PictureInput", "PictureInputTooltip")]
        public byte[] PictureInput
        {
            get;
            set;
        }


        [PropertyInfo(Direction.OutputData, "OutputData", "OutputTooltip")]
        public byte[] OutputData
        {
            get;
            set;
        }

        #endregion

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
            get { return presentation; }
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            presentation.ClearPresentation();
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0.1, 1);
            if (decodingThread == null || !decodingThread.IsAlive   // decoding thread is idle
                && (!codeFound || !settings.StopOnSuccess)) // stop if setting is selected and we decoded  something  
            {
                ProgressChanged(0.5, 1);
                decodingThread = new Thread(threadStart); // unfortunately we cant resart a thread and a threadpool with just one thread
                                                          // would produce more overhead, hence we have to create a new thread
                decodingThread.Start(PictureInput);
            }
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            codeFound = false;
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {

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
        

        /// <summary>
        /// This Methode decodes the given Image and updates the outputs and presentation. 
        /// Its meant to be called by the DecodingThread, hence the image has be declared as an object
        /// </summary>
        /// <param name="image">has to be a bytearray representation of an image</param>
        public void ProcessImage(object image)
        {
            var curImage = image as byte[]; 

            DimCodeDecoderItem dimCode = null;
            if (settings.DecodingType != VisualDecoderSettings.DimCodeType.AUTO)
            {
                dimCode = codeTypeHandler[settings.DecodingType].Decode(curImage);
            }
            else // automatic mode (try all decoder)
            {
                foreach (var decoder in codeTypeHandler)
                {
                    dimCode = decoder.Value.Decode(curImage);
                    if (dimCode != null)
                        break;
                }
            }
            
            if (dimCode != null) //input is valid and has been decoded
            {
                //update Presentation
                presentation.SetImages(dimCode.BitmapWithMarkedCode);
                presentation.SetData(System.Text.Encoding.ASCII.GetString(dimCode.CodePayload), dimCode.CodeType);

                //update output
                OutputData = dimCode.CodePayload;
                OnPropertyChanged("OutputData");

                //update Progress
                ProgressChanged(1, 1);

                codeFound = true;
            }
            else
            {
                presentation.ClearPresentation();
                presentation.SetImages(PictureInput);
            }
        }


        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public void GuiLogMessage(string message, NotificationLevel logLevel)
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
