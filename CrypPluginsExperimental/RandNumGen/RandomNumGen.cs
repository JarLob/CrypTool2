/*
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
using System.ComponentModel;
using System.Numerics;
using System.Threading;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using RandNumGen.Properties;

namespace Cryptool.Plugins.RandNumGen
{
    [Author("Christian Bender", "christian1.bender@student.uni-siegen.de", null, "http://www.uni-siegen.de")]
    [PluginInfo("RandNumGen.Properties.Resources", "RandNumGenPluginCaption", "RandNumGenTooltip", "RandNumGen/userdoc.xml", new[] { "RandNumGen/images/icon.png" })]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class RandNumGen : ICrypComponent
    {
        #region Private Variables

        private readonly RandomNumSettings settings = new RandomNumSettings();
        private BigInteger _outputlength;
        private BigInteger _seed;
        private BigInteger _modulus;
        private BigInteger _a;
        private BigInteger _b;
        private byte[] _result;
        private Thread workerThread;

        private BigInteger stdPrime = BigInteger.Parse("3");

        #endregion

        #region Data Properties

        /// <summary>
        /// Input of the outputlength
        /// </summary>
        [PropertyInfo(Direction.InputData, "presOutputLength", "presOutputLengthCaption")]
        public BigInteger OutputLength
        {
            get
            {
                return _outputlength;
            }
            set
            {
                _outputlength = value;
            }
        }

        /// <summary>
        /// Input with the seed
        /// </summary>
        [PropertyInfo(Direction.InputData, "presSeed", "presSeedCaption")]
        public BigInteger Seed
        {
            get
            {
                return _seed;
            }
            set
            {
                _seed = value;
            }
        }

        /// <summary>
        /// Input with the seed
        /// </summary>
        [PropertyInfo(Direction.InputData, "presMod", "presModCaption")]
        public BigInteger Modulus
        {
            get
            {
                return _modulus;
            }
            set
            {
                _modulus = value;
            }
        }

        /// <summary>
        /// Input of a
        /// </summary>
        [PropertyInfo(Direction.InputData, "presA", "presACaption")]
        public BigInteger A
        {
            get
            {
                return _a;
            }
            set
            {
                _a = value;
            }
        }

        /// <summary>
        /// Input of b
        /// </summary>
        [PropertyInfo(Direction.InputData, "presB", "presBCaption")]
        public BigInteger B
        {
            get
            {
                return _b;
            }
            set
            {
                _b = value;
            }
        }

        /// <summary>
        /// Output with result
        /// </summary>
        [PropertyInfo(Direction.OutputData, "presResult", "presResultCaption")]
        public byte[] Result
        {
            get
            {
                return _result;
            }
            set
            {
                _result = value;
            }
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
            get { return null; }
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            //Implementation with threads: this approach handles an inputchange in a better way
            if (workerThread == null)
            {
                workerThread = new Thread(new ThreadStart(tExecute));
                workerThread.IsBackground = true;
                workerThread.Start();
            }
            else
            {
                if (workerThread.IsAlive)
                {
                    workerThread.Abort();
                    workerThread = new Thread(new ThreadStart(tExecute));
                    workerThread.IsBackground = true;
                    workerThread.Start();
                }
                else
                {
                    workerThread = new Thread(new ThreadStart(tExecute));
                    workerThread.IsBackground = true;
                    workerThread.Start();
                }
            }
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
            if (workerThread.IsAlive)
            {
                workerThread.Abort();
            }
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

        #region Helpermethods

        /// <summary>
        /// simple test for prime numbers
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        private bool isPrime(BigInteger num)
        {
            for (BigInteger i = 2; i < num; i++)
                if (num % i == 0)
                    return false;
            return true;
        }

        /// <summary>
        ///  the method to be called in the workerthread
        /// </summary>
        private void tExecute()
        {
            ProgressChanged(0, 1);

            switch (settings.RndAlg)
            {
                case 0:
                    X2 x2Gen = new X2(_seed, _modulus, _outputlength);
                    _result = x2Gen.generateRNDNums();
                    OnPropertyChanged("Result");
                    break;
                case 1:
                    LCG lcgGen = new LCG(_seed, _modulus, _a, _b, _outputlength);
                    _result = lcgGen.generateRNDNums();
                    OnPropertyChanged("Result");
                    break;
                case 2:
                    if (!isPrime(Modulus))
                    {
                        GuiLogMessage(Resources.presErrorPrime.Replace("{0}", Modulus.ToString()).Replace("{1}", stdPrime.ToString()), NotificationLevel.Warning);
                        Modulus = stdPrime;
                    }
                    ICG icgGen = new ICG(_seed, Modulus, _a, _b, _outputlength);
                    _result = icgGen.generateRNDNums();
                    OnPropertyChanged("Result");
                    break;
                default:
                    break;
            }

            ProgressChanged(1, 1);
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
