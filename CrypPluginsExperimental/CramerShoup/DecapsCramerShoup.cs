using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using Org.BouncyCastle.Security;
using System.Windows.Controls;
using Cryptool.Plugins.CramerShoup.lib;
using Cryptool.PluginBase.Miscellaneous;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto;

namespace Cryptool.Plugins.CramerShoup
{
    [Author("Jan Jansen", "jan.jansen-n22@rub.de", "Ruhr Uni-Bochum", "http://cits.rub.de/")]
    [PluginInfo("CramerShoup.Properties.Resources", "PluginDecapsCaption", "PluginDecapsTooltip", "CramerShoup/DetailedDescription/doc.xml", new [] { "CramerShoup/Images/CSDecaps.png" })]
    [ComponentCategory(ComponentCategory.CiphersModernAsymmetric)]
    public class DecapsCramerShoup : ICrypComponent
    {
        #region Private Variables

        // HOWTO: You need to adapt the settings class as well, see the corresponding file.
        private readonly CramerShoupSettings settings = new CramerShoupSettings();

        #endregion

        #region Data Properties

        /// <summary>
        /// HOWTO: Input interface to read the input data.
        /// You can add more input properties of other type if needed.
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputPrivCaption", "InputPrivTooltip")]
        public ECCramerShoupPrivateParameter Parameter
        {
            get;
            set;
        }

        /// <summary>
        /// HOWTO: Output interface to write the output data.
        /// You can add more output properties ot other type if needed.
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputChiffreTextCaption", "InputChiffreTextTooltip")]
        public ECCramerShoupCipherText InputChiffreText
        {
            get;
            set;
        }

        /// <summary>
        /// HOWTO: Output interface to write the output data.
        /// You can add more output properties ot other type if needed.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputKeyCaption", "OutputKeyTooltip")]
        public byte[] Key
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
            SecureRandom random = new SecureRandom();
            var engine = new ECCramerShoupEngine();
            // HOWTO: Use this to show the progress of a plugin algorithm execution in the editor.
            ProgressChanged(0, 1);

            if (Parameter != null && InputChiffreText != null)
            {
                IDigest digest = null;
                switch (settings.Action)
                {
                    case 0:
                        digest = new RipeMD128Digest();
                        break;
                    case 1:
                        digest = new Sha256Digest();
                        break;
                    case 2:
                        digest = new Sha512Digest();
                        break;

                }
                ProgressChanged(0.33, 1);
                Key = engine.Decaps(Parameter, InputChiffreText,digest);

                ProgressChanged(0.66, 1);
                OnPropertyChanged("Key");
                // HOWTO: Make sure the progress bar is at maximum when your Execute() finished successfully.
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
