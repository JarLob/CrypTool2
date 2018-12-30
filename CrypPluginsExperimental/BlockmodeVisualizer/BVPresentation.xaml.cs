using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BlockmodeVisualizer;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.BlockmodeVisualizer
{
    /// <summary>
    /// Interaktionslogik für BVPresentation.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("BlockmodeVisualizer.Properties.Resources")]
    public partial class BVPresentation : UserControl
    {
        #region Fields

        private readonly BlockmodeVisualizer bv;

        #endregion

        #region Constructor

        public BVPresentation(BlockmodeVisualizer bv)
        {
            InitializeComponent();
            this.bv = bv;
        }

        #endregion

        #region Presentation Operations

        public void CreatePresentation()
        {
            // Clear previous presentation
            if(root.Children.Count > 0)
                ClearPresentation();

            // Create headline
            string headlineText = bv.ciphername;
            headlineText += Properties.Resources.ResourceManager.GetString("pres_headline_" + bv.settings.Action.ToString().ToLower());
            headlineText += Properties.Resources.pres_headline_conjunction;
            headlineText += Properties.Resources.ResourceManager.GetString("pres_headline_" + bv.settings.Blockmode.ToString().ToLower());
            headlineText += Properties.Resources.pres_headline_mode;
            headline.Content = headlineText;

            // Create grid content
            switch (bv.settings.Blockmode)
            {
                case Blockmodes.ECB:
                    CreateECBPresentation();
                    break;
/*                case Blockmodes.CBC:
                    CreateCBCPresentation();
                    break;
                case Blockmodes.CFB:
                    CreateCFBPresentation();
                    break;
                case Blockmodes.OFB:
                    CreateOFBPresentation();
                    break;
                case Blockmodes.CTR:
                    CreateCTRPresentation();
                    break;
                case Blockmodes.XTS:
                    CreateXTSPresentation();
                    break;
                case Blockmodes.CCM:
                    CreateCCMPresentation();
                    break;
                case Blockmodes.GCM:
                    CreateGCMPresentation();
                    break;
*/                default:
                    string message = Properties.Resources.not_yet_implemented_exception;
                    throw new NotImplementedException(message);
            }
        }

        public void ClearPresentation()
        {
            // Reset headline
            headline.Content = "";

            // Clear grid content
            root.Children.Clear();
        }

        #endregion

        #region Private Functions

        private void CreateECBPresentation()
        {

        }

        private void CreateCBCPresentation()
        {

        }

        private void CreateCFBPresentation()
        {

        }

        private void CreateOFBPresentation()
        {

        }

        private void CreateCTRPresentation()
        {

        }

        private void CreateXTSPresentation()
        {

        }

        private void CreateCCMPresentation()
        {

        }

        private void CreateGCMPresentation()
        {

        }

        private void UpdatePresentation()
        {

        }

        #endregion
    }
}
