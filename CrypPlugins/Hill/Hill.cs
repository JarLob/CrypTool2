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

using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Numerics;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.Hill
{
    [Author("Armin Krauß", "coredevs@cryptool.org", "CrypTool 2 Team", "http://cryptool2.vs.uni-due.de")]
    [PluginInfo("Cryptool.Plugins.Hill.Properties.Resources", "PluginCaption", "PluginTooltip", "Hill/DetailedDescription/doc.xml", new[] { "Hill/Hill.png" })]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class Hill : ICrypComponent
    {
        #region Private Variables

        private readonly HillSettings settings = new HillSettings();
        private ModMatrix mat,inv;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputCaption", "InputTooltip")]
        public string Input
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "MatrixCaption", "MatrixTooltip")]
        public BigInteger[] Matrix
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputTooltip")]
        public string Output
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings
        {
            get { return settings; }
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
        }

        private bool CheckParameters()
        {
            int dim;
            string[] s = null;
            BigInteger[] matelements;

            if (Matrix == null)
            {
                // get matrix from settings
                s = settings.MatrixString.Split(',');
                dim = (int)Math.Sqrt(s.Length);
                matelements = new BigInteger[dim];
            }
            else
            {
                // get matrix from matrix input
                matelements = Matrix;
                dim = (int)Math.Sqrt(Matrix.Length);
            }

            if (dim < 1)
            {
                GuiLogMessage("The dimension of the matrix must be greater than 0!", NotificationLevel.Error);
                return false;
            }

            if (dim * dim != Matrix.Length)
            {
                GuiLogMessage("The number of elements in the matrix definition must be a square number!", NotificationLevel.Error);
                return false;
            }

            if (settings.Modulus < 2)
            {
                GuiLogMessage(String.Format("The input alphabet must contain at least 2 different characters!"), NotificationLevel.Error);
                return false;
            }

            if (Input.Length % dim !=0)
            {
                GuiLogMessage(String.Format("The input was padded so that its length is a multiple of the matrix dimension ({0})!", dim), NotificationLevel.Warning);
                char paddingChar = settings.Alphabet.Contains("X") ? 'X' : (settings.Alphabet.Contains("x") ? 'x' : settings.Alphabet[settings.Modulus-1]);
                Input += new String(paddingChar, dim - (Input.Length % dim));
            }

            for (int j = 0; j < Input.Length; j++)
            {
                if (settings.Alphabet.IndexOf(Input[j]) < 0)
                {
                    GuiLogMessage(String.Format("The input contains the illegal character '{0}' at position {1}!", Input[j], j), NotificationLevel.Error);
                    return false;
                }
            }

            if (Matrix == null)
            {
                // read the matrix from the settings string

                int i=0;

                try
                {
                    for (i = 0; i < dim; i++)
                        matelements[i] = BigInteger.Parse(s[i]);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(string.Format("Error while parsing matrix element {0}: \"{1}\"!", i, s[i]), NotificationLevel.Error);
                    return false;
                }
            }

            mat = new ModMatrix(dim, settings.Modulus);

            int k = -1;
            for (int y = 0; y < mat.Dimension; y++)
                for (int x = 0; x < mat.Dimension; x++)
                    mat[x, y] = matelements[++k];

            inv = mat.invert();
            
            if (inv == null)
            {
                GuiLogMessage("The matrix "+mat+" is not invertible.", NotificationLevel.Error);
                return false;
            }
        
            return true;
        }

        public void Execute()
        {
            ProgressChanged(0, 1);

            if (!CheckParameters()) return;

            if (!settings.Action) mat = inv;    // decrypt

            GuiLogMessage("The matrix is " + mat, NotificationLevel.Debug);

            Output = "";

            BigInteger[] result, vector = new BigInteger[mat.Dimension];

            for (int j = 0; j < Input.Length; j += mat.Dimension)
            {
                for (int k = 0; k < mat.Dimension; k++)
                    vector[k] = settings.Alphabet.IndexOf(Input[j + k]);
                result = mat * vector;
                for (int k = 0; k < mat.Dimension; k++)
                    Output += settings.Alphabet[(int)result[k]];
            }

            OnPropertyChanged("Output");

            ProgressChanged(1, 1);
        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
        }

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