using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace ImageSteganographyVisualization
{
    [Cryptool.PluginBase.Attributes.Localization("ImageSteganographyVisualization.Properties.Resources")]

    public partial class ImageSteganographyPresentation : UserControl
    {
        public ModeType mode;
        public LSBPresentation lsb;
        public BPCSPresentation bpcs;
        public ImageSteganographyVisualization imageStegVis;

        public ImageSteganographyPresentation(ImageSteganographyVisualization imageStegVis)
        {
            InitializeComponent();
            this.imageStegVis = imageStegVis;
            this.mode = imageStegVis.GetSettings().GetMode();
            Prompt.Text = Properties.Resources.ShowPresentationPrompt;
        }

        public void DisplayHidingProcessPresentation(ModeType mode)
        {
            this.mode = mode;
            this.MinHeight = 350;
            this.MinWidth = 550;
            this.MainFrame.Children.Remove(MainPanel);

            if (mode == ModeType.BPCS)
            {
                this.MainFrame.Children.Add(bpcs);
            }
            else
            {
                this.MainFrame.Children.Add(lsb);
            }
        }

        public void HidePresentation()
        {
            if (mode == ModeType.LSB)
            {
                this.MainFrame.Children.Remove(lsb);
            }
            else if (mode == ModeType.BPCS)
            {
                this.MainFrame.Children.Remove(bpcs);
            }
            this.MinHeight = 100;
            this.MinWidth = 150;
            this.MainFrame.Children.Clear();
            this.MainFrame.Children.Add(MainPanel);
            Prompt.Text = Properties.Resources.ShowPresentationPrompt;
        }

        public void DisplayExtractionPresentation(int messageLength, BitArray redBitMask, BitArray greenBitMask, BitArray blueBitMask)
        {
            this.MinHeight = 100;
            this.MinWidth = 150;
            MainPanel.Visibility = Visibility.Hidden;

            ExtractedHeader.Visibility = Visibility.Visible;
            MessageLengthTB.Text = string.Format(Properties.Resources.MessageLengthExtractLabel + "{0} bits = {1} characters.", messageLength * 8, messageLength);
            RedBitMask.Text = Properties.Resources.RedBitMaskLabel + ": " + BitArrayToString(redBitMask);
            GreenBitMask.Text = Properties.Resources.GreenBitMaskLabel + ": " + BitArrayToString(greenBitMask);
            BlueBitMask.Text = Properties.Resources.BlueBitMaskLabel + ": " + BitArrayToString(blueBitMask);
        }

        public void DisplayNoPresentation()
        {
            this.MinHeight = 100;
            this.MinWidth = 150;
            MainPanel.Visibility = Visibility.Visible;
            ExtractedHeader.Visibility = Visibility.Hidden;

            Prompt.Text = Properties.Resources.NoPresentationPrompt;

        }

        private string BitArrayToString(BitArray bits)
        {
            string bitString = "";
            for (int i = 7; i >= 0; i--)
            {
                if (bits[i])
                {
                    bitString += "1";
                }
                else
                {
                    bitString += "0";
                }
            }
            return bitString;
        }
    }

}