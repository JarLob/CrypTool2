/*
   Copyright 2015 Robert Rauer

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
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Threading;
using System.Threading;
using System;
using System.Text;

namespace Cryptool.Plugins.FleißnerGrille
{
    [Author("Robert Rauer", "robert_rauer@yahoo.de", "Universität Kassel", "http://cryptool2.vs.uni-due.de")]
    [PluginInfo("Cryptool.Plugins.FleißnerGrille.Properties.Resources", "PluginCaption", "PluginTooltip", "FleißnerGrille/userdoc.xml",
        new[] { "FleißnerGrille/Images/FleißnerGrille.png" })]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class FleißnerGrille : ICrypComponent
    {
        #region Private Variables
        // HOWTO: You need to adapt the settings class as well, see the corresponding file.
        public FleißnerGrilleSettings settings;  //readonly
        private FleißnerGrillePresentation myPresentation; 
        private bool running = false;
        private bool stopped = false;
        private string output;
        private bool isPlayMode = false;

        private struct koord
        {
            public int k_i;
            public int k_j;
        }

        #endregion

        #region Data Properties

        /// <summary>
        /// Constructor
        /// </summary>
        public FleißnerGrille()
        {
            this.settings = new FleißnerGrilleSettings();
            myPresentation = new FleißnerGrillePresentation(this);
            Presentation = myPresentation;
            myPresentation.fireEnd += new EventHandler(presentation_finished);
            myPresentation.updateProgress += new EventHandler(update_progress);
            this.settings.PropertyChanged += settings_OnPropertyChange;
            this.settings.LogMessage += FleißnerStencil_LogMessage;
        }

        private void update_progress(object sender, EventArgs e)
        {
            //TranspositionPresentation myhelp = new TranspositionPresentation();
            //myhelp = (TranspositionPresentation)sender;
            ProgressChanged(myPresentation.progress, 3000);
        }

        private void presentation_finished(object sender, EventArgs e)
        {
            if (!myPresentation.Stop)
                OutputString = this.output;
            ProgressChanged(1, 1);

            running = false;
        }

        private void settings_OnPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            myPresentation.UpdateSpeed(this.settings.PresentationSpeed);
        }

        /// <summary>
        /// Get or set all settings for this algorithm.
        /// </summary>
        public ISettings Settings
        {
            get { return this.settings; }
        }

        /// <summary>
        /// HOWTO: Input interface to read the input data. 
        /// You can add more input properties of other type if needed.
        /// </summary>
        /// 
        private string _inputString;

        [PropertyInfo(Direction.InputData, "InputStringCaption", "InputStringTooltip",true)]
        public string InputString
        {
            get 
            {
                
                if (_inputString!=null &&_inputString.Length > this.settings.StencilString.Length) //grille is smaller than inputString
                {
                    int min = minGrille(_inputString);
                    this.settings.OnLogMessage("FAILURE:\n" +
                            "Grille from user: \n" +
                            "\"" + this.settings.StencilString + "\" (With a " + (int)Math.Sqrt(this.settings.StencilString.Length) + " square grille) is smaller than the length of " + _inputString + "\n" +
                            "there must be at least a " + min + " x " + min + " grille", NotificationLevel.Info);
                }
                return _inputString;            
            }
            set 
            {
                if (value.Length > this.settings.StencilString.Length) //grille is smaller than inputString
                {
                    int min = minGrille(value);
                    this.settings.OnLogMessage("FAILURE:\n" +
                            "Grille from user: \n" +
                            "\"" + this.settings.StencilString + "\" (With a " + (int)Math.Sqrt(this.settings.StencilString.Length) + " square grille) is smaller than the length of " + value + "\n" +
                            "there must be at least a " + min + " x " + min + " grille", NotificationLevel.Info);
                }
                else
                {
                    _inputString = value;
                }
            }
        }

        /// <summary>
        /// HOWTO: Output interface to write the output data.
        /// You can add more output properties ot other type if needed.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip",false)]
        public string OutputString
        {
            get;
            set;
        }

        /// <summary>
        /// HOWTO: Input interface to read the input data. 
        /// You can add more input properties of other type if needed.
        /// </summary>
        /// 
        private string _stencilString;
        /// <summary>
        /// HOWTO: Input interface to read the input data. 
        /// You can add more input properties of other type if needed.
        /// </summary>
        [PropertyInfo(Direction.InputData, "StencilStringCaption", "StencilStringTooltip", false)]
        public string StencilString
        {
            get 
            {
                if (_stencilString != null && _stencilString != settings.StencilString)
                {
                    if (_stencilString.Length < _inputString.Length) //grille is smaller than inputString
                    {
                        int min = minGrille(_inputString);
                        this.settings.OnLogMessage("FAILURE:\n" +
                            "Grille from user: \n" +
                            "\"" + _stencilString + "\" (With a " + (int)Math.Sqrt(_stencilString.Length) + " square grille) is smaller than the length of " + _inputString + "\n" +
                            "there must be at least a " + min + " x " + min + " grille", NotificationLevel.Info);
                    }                                     
                }
                return this.settings.StencilString;            
            }
            set
            {
                if (value != null && value != settings.StencilString)
                {
                    if (value.Length < _inputString.Length) //grille is smaller than inputString
                    {
                        int min = minGrille(_inputString);
                        this.settings.OnLogMessage("FAILURE:\n"+
                            "Grille from user: \n"+
                            "\"" + value + "\" (With a " + (int)Math.Sqrt(value.Length) + " square grille) is smaller than the length of " + _inputString + "\n" +
                            "there must be at least a " + min + " x " + min +" grille", NotificationLevel.Info);
                    }
                    else
                    {

                        this.settings.StencilString = value;
                        OnPropertyChanged("StencilString");
                    }
                }
            }
        }

        private int minGrille(string input) 
        {
            int min;
            if (Math.Sqrt(input.Length) - (int)Math.Sqrt(input.Length) > 0) // sqrt from length is odd
            {
                min = ((int)Math.Sqrt(input.Length)) + 1;
            }
            else // sqrt from lenght is even
            {
                min = (int)Math.Sqrt(input.Length);
            }
            return min;
        }

        #endregion

        #region IPlugin Members

        ///// <summary>
        ///// Provide plugin-related parameters (per instance) or return null.
        ///// </summary>
        //public ISettings Settings
        //{
        //    get { return settings; }
        //}


        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            running = false;
            stopped = false;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            while (running)
            {
                myPresentation.my_Stop(this, EventArgs.Empty);
                if (stopped)
                    return;
            }

            isPlayMode = true;

            ProgressChanged(0, 100);

            if (!string.IsNullOrEmpty(InputString))
            {
                ProcessFleißnerStencil();
                OnPropertyChanged("OutputString"); // push output to editor

                // HOWTO: You can pass error, warning, info or debug messages to the CT2 main window.
                /*if (settings.SomeParameter < 0)
                    GuiLogMessage("SomeParameter is negative", NotificationLevel.Debug);
                */
                // HOWTO: Make sure the progress bar is at maximum when your Execute() finished successfully.
                ProgressChanged(1, 1);

            }
            else 
            {
                FleißnerStencil_LogMessage("InputString ist empty or null", NotificationLevel.Error);
                OutputString = null;
                return;
            }

            if (Presentation.IsVisible && !string.IsNullOrEmpty(InputString) && settings.isCorrectStencil(StencilString))
            {
                FleißnerStencil_LogMessage("Tester 1", NotificationLevel.Debug);
                Presentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    bool encrypt, rotate;
                    if(settings.ActionMode == 0) { encrypt = true; }
                    else { encrypt = false; }
                    if (settings.ActionRotate == 0) { rotate = false; }
                    else { rotate = true; }
                    myPresentation.main(settings.StringToStencil(StencilString), InputString, encrypt, rotate, this);
                }
               , null);
            }
            else 
            {
                OutputString = this.output;
                ProgressChanged(1, 1);
            }
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public System.Windows.Controls.UserControl Presentation
        {
            get;
            private set;
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            stopped = true;

            myPresentation.my_Stop(this, EventArgs.Empty);
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

        #region Private methods

        private void ProcessFleißnerStencil()
        {
            if (settings.ActionMode == FleißnerGrilleSettings.FleißnerMode.Encrypt)
            {
                OutputString = Encrypt(InputString);
            }
            else
            {
                OutputString = Decrypt(InputString);
            }
        }

        /// <summary>
        /// Handles re-execution events from settings class
        /// </summary>
        private void FleißnerStencil_ReExecute()
        {
            if (isPlayMode)
            {
                Execute();
            }
        }
        /// <summary>
        /// fill Matrix with 00 ... nn
        /// </summary>
        private koord[,] generateStecil(int n)
        {
            koord[,] stencil = new koord[n, n];
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < n; ++j)
                {
                    stencil[i, j].k_i = i;
                    stencil[i, j].k_j = j;
                }
            }
            return stencil;
        }
        /// <summary>
        // rotate a 2-dimensional Array
        public bool[,] RotateStencil(bool[,] stencil, bool right)
        {
            int stencilLength = (int) Math.Sqrt(stencil.Length);
            bool[,] ret = null;
            if (settings.ActionRotate == FleißnerGrilleSettings.FleißnerRotate.Right)
            {
                ret = rotate(stencil);
            }
            else 
            {
                for (int i = 0; i < 3; i++) 
                {
                    ret = rotate(stencil);
                    stencil = ret;
                }
            }
            return ret;
        }

        /// <summary>
        // rotate a 2-dimensional Array
        public bool[,] rotate(bool[,] stencil)
        {
            int stencilLength = (int)Math.Sqrt(stencil.Length);
            bool[,] ret = new bool[stencilLength, stencilLength];
            for (int i = 0; i < stencilLength; ++i)
            {
                for (int j = 0; j < stencilLength; ++j)
                {
                    ret[i, j] = stencil[stencilLength - j - 1, i];
                }
            }
            return ret;
        }
        /// <summary>
        /// mark Fields in Matrix with -1-1
        /// </summary>
        private bool[,] markFieldInStencil(bool[,] stencil, koord point)
        {
            for (int i = 0; i < 4; i++ )
            {
                // HOWTO: Set Field unvisible
                if (i == 0) // set hole in Stecil
                {
                    stencil[point.k_i, point.k_j] = true;
                }
                else 
                {
                    stencil[point.k_i, point.k_j] = false;
                }
                if (settings.ActionRotate == FleißnerGrilleSettings.FleißnerRotate.Right) //right
                {
                    stencil = RotateStencil(stencil, true);
                }
                else 
                {
                    stencil = RotateStencil(stencil, false);
                }
            } 
            return stencil;
        }
        /// <summary>
        /// write plaintext with the FleißnerStencil in a Matrix
        /// </summary>        
        public char[,] EncryptedMatrix(bool[,] stencil, string plaintext)
        {
            int x=0;
            int stencilLength = (int) Math.Sqrt(StencilString.Length);
            char[,] encrypted = new char[stencilLength, stencilLength];
            bool right;
            if (settings.ActionRotate == FleißnerGrilleSettings.FleißnerRotate.Right)
            {
                right = true;
            }
            else 
            {
                right = false;
            }
            for (int rotate = 0; rotate < 4; rotate++) // rotate stencil 4 times 
            {
                for (int i = 0; i < stencil.GetLength(0); i++)
                {
                    for (int j = 0; j < stencil.GetLength(1); j++)
                    {
                        if (stencil[i, j] == true) // stencil has on position i,j a hole
                        {

                            int k = plaintext.Length - 1;
                            if (x - 1 < plaintext.Length-1)
                            {
                                encrypted[i, j] = plaintext[x]; // write on this position the current plaintext letter in the encryptMatrix
                            }
                            else 
                            {
                                encrypted[i, j] = '#';
                            }
                            x++;
                        }
                    }
                }
                stencil = RotateStencil(stencil, right);
            }
            return encrypted;
        }
        /// <summary>
        /// fill 2-dimensional char Array with String rowwise
        /// </summary>
        
        /// <summary>
        /// encrypt the plaintext with a FleißnerStencil
        /// </summary>
        private string Encrypt(string plaintext)
        {
            bool[,] myStencil = stencil(StencilString);
            char[,] encryptedMatrix = EncryptedMatrix(myStencil,plaintext);
            return TwoDCharArrayToString(encryptedMatrix);
        }

        private bool[,] stencil(string stencilString) 
        {
            bool[,] stencil = settings.StringToStencil(settings.StencilString);
            return stencil;
        }
        /// <summary>
        /// decrypt the ciphertext with a FleißnerStencil
        /// </summary>
        private string Decrypt(string ciphertext)
        {
            int stencilLength = (int)Math.Sqrt(settings.StencilString.Length);
            bool[,] stencil = settings.StringToStencil(settings.StencilString);
            var plaintext = new StringBuilder();
            char[,] encrypted = StringTo2DCharArray(ciphertext, stencilLength);  
            for (int rotate = 0; rotate < 4; rotate++) // rotate stencil 4 times over encryptMatrix
            {
                for (int i = 0; i < stencil.GetLength(0); i++)
                {
                    for (int j = 0; j < stencil.GetLength(1); j++)
                    {
                        if (stencil[i, j] == true) // stencil has on position i,j a hole
                        {
                            plaintext.Append(encrypted[i, j]); // write this position from encryptMatrix to plaintext
                        }
                    }
                }
                if (settings.ActionRotate == FleißnerGrilleSettings.FleißnerRotate.Right) //rotate right
                {
                    stencil = RotateStencil(stencil, true);
                }
                else 
                {
                    stencil = RotateStencil(stencil, false);
                }
                
            }
            return plaintext.ToString();
        }
        /// <summary>
        /// fill 2-dimensional char Array with String rowwise
        /// </summary>
        private char[,] StringTo2DCharArray(string input, int stencilLength)
        {
            int count=0;
            char[,] matrix = new char[stencilLength, stencilLength];
            for (int i = 0; i < stencilLength; i++) 
            {
                for (int j = 0; j < stencilLength; j++)
                {
                    if (count < input.Length)
                    {
                        matrix[i, j] = input[count];
                        count++;
                    }
                }
            }
            return matrix;
        }
        /// <summary>
        /// read string from a 2-dimensional char Array rowwise
        /// </summary>
        private string TwoDCharArrayToString(char[,] matrix)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < matrix.GetLength(0); i++) 
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    builder.Append(matrix[i,j]);
                }
            }
            return builder.ToString();
        }
        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void FleißnerStencil_LogMessage(string message, NotificationLevel logLevel)
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
