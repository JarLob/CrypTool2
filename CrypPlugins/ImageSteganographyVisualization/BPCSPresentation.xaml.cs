﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;

namespace ImageSteganographyVisualization
{
    [Cryptool.PluginBase.Attributes.Localization("ImageSteganographyVisualization.Properties.Resources")]
    public partial class BPCSPresentation : UserControl
    {
        ImageSteganographyVisualization imageStegVis;
        private int pixelX;
        private int pixelY;

        private Bitmap inputBitmap;
        private int introViewCounter;
        Bitmap[] bitplanes;
        ArrayList hiderBlocks;
        ArrayList messageBlocks;
        ArrayList allImageBlocks;
        int totalMessageBlocks;
        int totalHiderBlocks;
        int totalImageBlocks;
        int totalMessageSize;
        int totalCapacitySize;
        int imageIndex = 0;
        int iteration = 0;
        int blueIndex;
        int greenIndex;
        int redIndex;

        public BPCSPresentation(ImageSteganographyVisualization imageStegVis)
        {
            InitializeComponent();
            this.introViewCounter = 0;
            this.imageStegVis = imageStegVis;
            this.inputBitmap = imageStegVis.inputBitmap;
            string orderString = imageStegVis.order.ToString();
            this.blueIndex = orderString.IndexOf('B');
            this.greenIndex = orderString.IndexOf('G');
            this.redIndex = orderString.IndexOf('R');
            
            InitImageSourcesAndLinks();
        }

        #region Main menu methods
        public void EnableButtons()
        {
            SeeBitPlanesButton.IsEnabled = true;
            SeeHidingProcessButton.IsEnabled = true;
            SeePixelConversionButton.IsEnabled = true;
            StartHint.Visibility = Visibility.Hidden;
            BitArray messageBits = new BitArray(Encoding.UTF8.GetBytes(imageStegVis.InputSecretMessage));
            totalMessageSize = messageBits.Count;
        }
        public void DisableButtons()
        {
            SeeBitPlanesButton.IsEnabled = false;
            SeeHidingProcessButton.IsEnabled = false;
            SeePixelConversionButton.IsEnabled = false;
            StartHint.Visibility = Visibility.Visible;
        }
        void BackToMainMenuClick(object sender, RoutedEventArgs e)
        {
            ShowMainMenu();
        }

        public void ShowMainMenu()
        {
            HidingProcessView.Visibility = Visibility.Hidden;
            PixelConversionView.Visibility = Visibility.Hidden;
            BitPlanesView.Visibility = Visibility.Hidden;
            IntroView.Visibility = Visibility.Hidden;
            MainMenu.Visibility = Visibility.Visible;
        }
        #endregion

        #region Intro view methods

        void InitImageSourcesAndLinks()
        {
            ModelImage.Source = new BitmapImage(new Uri(Properties.Resources.StegModelUrl, UriKind.Relative));
            ConjugationImage.Source = new BitmapImage(new Uri("images/conjugation.png", UriKind.Relative));
            CoverImage.Source = new BitmapImage(new Uri("images/coverimage.jpg", UriKind.Relative));
            ComplexRegion.Source = new BitmapImage(new Uri("images/complex.PNG", UriKind.Relative));
            InformativeRegion.Source = new BitmapImage(new Uri("images/informative.PNG", UriKind.Relative));
            Intro2TextBlock3.Text = Properties.Resources.Intro2BPCSText3;
            Intro2TextBlock3.Inlines.Add(Slide4HyperLink);
            Intro2TextBlock4.Text = Properties.Resources.Intro2BPCSText4;
            Intro2TextBlock4.Inlines.Add(Slide5HyperLink);
        }
        void ShowIntroView(object sender, RoutedEventArgs e)
        {
            MainMenu.Visibility = Visibility.Hidden;
            IntroView.Visibility = Visibility.Visible;
            Intro0.Visibility = Visibility.Visible;
            Intro1.Visibility = Visibility.Hidden;
            Intro2.Visibility = Visibility.Hidden;
            Intro3.Visibility = Visibility.Hidden;
            Intro4.Visibility = Visibility.Hidden;
            introViewCounter = 0;
            PrevIntro.IsEnabled = false;
        }
        void NextIntroButtonClick(object sender, RoutedEventArgs e)
        {
            introViewCounter++;
            switch (introViewCounter)
            {
                case 1:
                    Intro0.Visibility = Visibility.Hidden;
                    Intro1.Visibility = Visibility.Visible;
                    PrevIntro.IsEnabled = true;
                    break;
                case 2:
                    Intro1.Visibility = Visibility.Hidden;
                    Intro2.Visibility = Visibility.Visible;

                    break;
                case 3:
                    Intro2.Visibility = Visibility.Hidden;
                    Intro3.Visibility = Visibility.Visible;
                    break;
                case 4:
                    Intro3.Visibility = Visibility.Hidden;
                    Intro4.Visibility = Visibility.Visible;
                    NextIntro.IsEnabled = false;
                    break;
            }
        }
        void PrevIntroButtonClick(object sender, RoutedEventArgs e)
        {
            introViewCounter--;
            NextIntro.IsEnabled = true;
            switch (introViewCounter)
            {
                case 0:
                    Intro1.Visibility = Visibility.Hidden;
                    Intro0.Visibility = Visibility.Visible;
                    PrevIntro.IsEnabled = false;
                    break;
                case 1:
                    Intro2.Visibility = Visibility.Hidden;
                    Intro1.Visibility = Visibility.Visible;
                    break;
                case 2:
                    Intro3.Visibility = Visibility.Hidden;
                    Intro2.Visibility = Visibility.Visible;
                    break;
                case 3:
                    Intro4.Visibility = Visibility.Hidden;
                    Intro3.Visibility = Visibility.Visible;
                    break;
            }
        }

        void SeeSlide4(object sender, RoutedEventArgs e)
        {
            introViewCounter = 3;
            Intro2.Visibility = Visibility.Hidden;
            Intro3.Visibility = Visibility.Visible; 
        }
        void SeeSlide5(object sender, RoutedEventArgs e)
        {
            introViewCounter = 4;
            Intro2.Visibility = Visibility.Hidden;
            Intro4.Visibility = Visibility.Visible;
            NextIntro.IsEnabled = false;
        }

        #endregion

        #region Hiding process view methods

        public void InitMessageAndImageBlocks(ArrayList hiderBlocks, ArrayList messageBlocks, ArrayList allImageBlocks)
        {
            this.messageBlocks = new ArrayList(messageBlocks);
            this.allImageBlocks = (ArrayList)allImageBlocks.Clone();
            this.hiderBlocks = (ArrayList)hiderBlocks.Clone();
            totalCapacitySize = hiderBlocks.Count * 63;
            totalHiderBlocks = hiderBlocks.Count;
            totalMessageBlocks = messageBlocks.Count;
            totalImageBlocks = allImageBlocks.Count;
        }

        void ShowHidingProcessView(object sender, RoutedEventArgs e)
        {
            MainMenu.Visibility = Visibility.Hidden;
            HidingProcessView.Visibility = Visibility.Visible;
            MessageInfoText1.Text = Properties.Resources.MessageBlocksInfoText1 + messageBlocks.Count;
            TotalImageBlocksLabel.Text = string.Format(Properties.Resources.ImageBlocksInfoText1 + " {0}", allImageBlocks.Count);
            TotalComplexBlocksLabel.Text = Properties.Resources.ImageBlocksInfoText2 + hiderBlocks.Count;
            HidingCapacityLabel.Text = string.Format(Properties.Resources.HidingCapacityText + " {0:0.###} ", totalCapacitySize);
            double percentageCapacity = ((double)totalHiderBlocks / (double)totalImageBlocks) * 100;
            PercentageCapacityLabel.Text = string.Format(Properties.Resources.PercentageCapacityLabel + " {0:0.##} %", percentageCapacity);
            InvalidIndexPrompt.Text = string.Format(Properties.Resources.InvalidInputPrompt + " 0 =< x < {0}", totalImageBlocks);
            InvalidIterationPrompt.Text = string.Format(Properties.Resources.InvalidInputPrompt + " 0 =< x < {0}", totalHiderBlocks);
            UpdateHidingIterationsView();
        }

        void HiderBlocksButtonClick(object sender, RoutedEventArgs e)
        {
            HiderBlocksButton.IsEnabled = false;
            AllBlocksButton.IsEnabled = true;
            AllImageBlocksPanel.Visibility = Visibility.Hidden;
            HidingIterationsPanel.Visibility = Visibility.Visible;
            HiderBlocksLabel1.Visibility = Visibility.Visible;
        }

        void AllBlocksButtonClick(object sender, RoutedEventArgs e)
        {
            AllBlocksButton.IsEnabled = false;
            HiderBlocksButton.IsEnabled = true;
            HidingIterationsPanel.Visibility = Visibility.Hidden;
            AllImageBlocksPanel.Visibility = Visibility.Visible;
            HiderBlocksLabel1.Visibility = Visibility.Hidden;
            UpdateAllBlocksView();
        }

        private void CBLoaded(object sender, RoutedEventArgs e)
        {
            List<string> units = new List<string>();
            units.Add("bits");
            units.Add("bytes");
            units.Add("kilobits");
            units.Add("megabits");

            var comboBox = sender as ComboBox;
            comboBox.ItemsSource = units;
            comboBox.SelectedIndex = 0;
        }

        void CBUnitChanged(object sender, RoutedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            string value = comboBox.SelectedItem as string;
            double sizeConverted = comboBox.Name == "UnitComboBoxMessage" ? totalMessageSize : totalCapacitySize;

            switch (value)
            {
                case "bits":
                    break;
                case "bytes":
                    sizeConverted /= 8;
                    break;
                case "kilobits":
                    sizeConverted /= 1000;
                    break;
                case "megabits":
                    sizeConverted /= 1000000;
                    break;
            }
            if (comboBox.Name == "UnitComboBoxMessage")
            {
                MessageInfoText2.Text = string.Format(Properties.Resources.MessageBlocksInfoText2 + " {0:0.###} ", sizeConverted);
            }
            else if (comboBox.Name == "UnitComboBoxCapacity")
            {
                HidingCapacityLabel.Text = string.Format(Properties.Resources.HidingCapacityText + " {0:0.###} ", sizeConverted);
            }
        }

        void UpdateHidingIterationsView()
        {
            ImageBlock im = (ImageBlock)hiderBlocks[iteration]; 
            CurrentComplexBlockLabel.Text = Properties.Resources.CurrentComplexBlockNrLabel + iteration;
            ComplexImageBlockIm.Source = BitmapConversion.BitmapToBitmapSource(GetBlockBitmap(im.GetBlockArray(), GetColorFromLayer(im.GetLayer())));
            PlaneLayerLabelComplex.Text = Properties.Resources.PlaneLabel + im.GetLayer();
            ImageComplexityLabelComplex.Text = Properties.Resources.ComplexityLabel + im.GetComplexity().ToString("0.00");
            PointXYLabelComplex.Text = string.Format(Properties.Resources.PointLabel + " (x,y): ({0} , {1})", im.GetRow(), im.GetCol());

            if (iteration < messageBlocks.Count && iteration >= 0)
            {
                NoMessageBlock.Visibility = Visibility.Hidden;
                MessagePanel.Visibility = Visibility.Visible;
                MessageBlockImage.Visibility = Visibility.Visible;
                MessageBlock msg = (MessageBlock)messageBlocks[iteration];
                CurrentMessageBlockLabel.Text = Properties.Resources.CurrentMessageBlockNrLabel + iteration;
                ConjugatedLabel.Text = Properties.Resources.ConjugatedLabel + msg.IsConjugated();
                MessageComplexityLabel.Text = Properties.Resources.ComplexityLabel + msg.GetComplexity().ToString("0.00");
                MessageBlockImage.Source = BitmapConversion.BitmapToBitmapSource(GetBlockBitmap(msg.GetBlock(), Color.Black));
            }
            else
            {
                MessagePanel.Visibility = Visibility.Hidden;
                MessageBlockImage.Visibility = Visibility.Hidden;
                NoMessageBlock.Visibility = Visibility.Visible;
            }
            InvalidIterationPrompt.Visibility = Visibility.Hidden;
        }

        void NextIterationClick(object sender, RoutedEventArgs e)
        {
            PrevIterationButton.IsEnabled = true;
            iteration++;
            UpdateHidingIterationsView();
            if (iteration + 1 == totalHiderBlocks)
            {
                NextIterationButton.IsEnabled = false;
            }
        }

        void PreviousIterationClick(object sender, RoutedEventArgs e)
        {
            NextIterationButton.IsEnabled = true;
            iteration--;
            UpdateHidingIterationsView();
            if (iteration == 0)
            {
                PrevIterationButton.IsEnabled = false;
            }
        }
        
        void NextIndexClick(object sender, RoutedEventArgs e)
        {
            PrevIndexButton.IsEnabled = true;
            imageIndex++;
            UpdateAllBlocksView();
            if (imageIndex + 1 == allImageBlocks.Count)
            {
                NextIndexButton.IsEnabled = false;
            }
        }
        
        void PreviousIndexClick(object sender, RoutedEventArgs e)
        {
            NextIndexButton.IsEnabled = true;
            imageIndex--;
            UpdateAllBlocksView();
            if (imageIndex == 0)
            {
                PrevIndexButton.IsEnabled = false;
            }
        }

        void UpdateAllBlocksView()
        {
            ImageBlock im = (ImageBlock)allImageBlocks[imageIndex];
            CurrentImageBlockLabel.Text = Properties.Resources.CurrentImageBlockNrLabel + imageIndex;
            PlaneLayerLabel.Text = Properties.Resources.PlaneLabel + im.GetLayer();
            ImageComplexityLabel.Text = string.Format(Properties.Resources.ComplexityLabel + "{0:0.##}", im.GetComplexity());
            PointXYLabel.Text = string.Format(Properties.Resources.PointLabel + " (x,y): ({0} , {1})", im.GetRow(), im.GetCol());
            ImageBlockIm.Source = BitmapConversion.BitmapToBitmapSource(GetBlockBitmap(im.GetBlockArray(), GetColorFromLayer(im.GetLayer())));
            InvalidIndexPrompt.Visibility = Visibility.Hidden;
        }

        void ManualIterationEntered(object sender, RoutedEventArgs e)
        {
            try
            {
                int enteredIteration = Int32.Parse(IterationInput.Text);
                if (enteredIteration < 0 || enteredIteration >= hiderBlocks.Count)
                {
                    InvalidIterationPrompt.Visibility = Visibility.Visible;
                }
                else
                {
                    iteration = enteredIteration;
                    UpdateHidingIterationsView();
                    if (iteration == 0)
                    {
                        PrevIterationButton.IsEnabled = false;
                    }
                    else
                    {
                        PrevIterationButton.IsEnabled = true;
                    }
                    if (iteration + 1 >= hiderBlocks.Count)
                    {
                        NextIterationButton.IsEnabled = false;
                    }
                    else
                    {
                        NextIterationButton.IsEnabled = true;
                    }
                }
            }
            catch (FormatException)
            {
                InvalidIterationPrompt.Visibility = Visibility.Visible;
            }

        }

        void ManualIndexEntered(object sender, RoutedEventArgs e)
        {
            try
            {
                int enteredIndex = Int32.Parse(IndexInput.Text);
                if (enteredIndex < 0 || enteredIndex >= allImageBlocks.Count)
                {
                    InvalidIndexPrompt.Visibility = Visibility.Visible;
                }
                else
                {
                    imageIndex = enteredIndex;
                    UpdateAllBlocksView();
                    if (imageIndex == 0)
                    {
                        PrevIndexButton.IsEnabled = false;
                    }
                    else
                    {
                        PrevIndexButton.IsEnabled = true;
                    }
                    if (imageIndex + 1 >= allImageBlocks.Count)
                    {
                        NextIndexButton.IsEnabled = false;
                    }
                    else
                    {
                        NextIndexButton.IsEnabled = true;
                    }
                }
            }
            catch (FormatException)
            {
                InvalidIndexPrompt.Visibility = Visibility.Visible;
            }
        }

        public Bitmap GetBlockBitmap(bool[,] block, Color color)
        {
            Bitmap img = new Bitmap(80, 80);
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Color c = block[i, j] ? color : Color.FromArgb(255, 255, 255);
                    for (int k = i * 8; k < i * 8 + 10; k++)
                    {
                        for (int l = j * 8; l < j * 8 + 10; l++)
                        {
                            img.SetPixel(k, l, c);
                        }
                    }
                }
            }
            return img;
        }

        Color GetColorFromLayer(int layer)
        {
            Color c;

            if (layer % 3 == blueIndex)
            {
                c = Color.Blue;
            }
            else if (layer % 3 == greenIndex)
            {
                c = Color.Green;
            }
            else
            {
                c = Color.Red;
            }
            return c;
        }

        #endregion

        #region Pixel conversion view methods

        public void SetPixelConversionView(int x, int y)
        {
            PixelConversionView.Visibility = Visibility.Visible;
            Color pixelBefore = imageStegVis.inputBitmap.GetPixel(x, y);
            Color pixelAfter = imageStegVis.outputBitmap.GetPixel(x, y);
            SolidColorBrush scbBefore = new SolidColorBrush();
            SolidColorBrush scbAfter = new SolidColorBrush();
            scbBefore.Color = System.Windows.Media.Color.FromRgb(pixelBefore.R, pixelBefore.G, pixelBefore.B);
            scbAfter.Color = System.Windows.Media.Color.FromRgb(pixelAfter.R, pixelAfter.G, pixelAfter.B);
            PixelBeforeSample.Fill = scbBefore;
            PixelAfterSample.Fill = scbAfter;
            SetColorValueBitsString(RValueBitsBefore, pixelBefore, 0);
            SetColorValueBitsString(GValueBitsBefore, pixelBefore, 1);
            SetColorValueBitsString(BValueBitsBefore, pixelBefore, 2);
            SetColorValueBitsString(RValueBitsAfter, pixelAfter, 0);
            SetColorValueBitsString(GValueBitsAfter, pixelAfter, 1);
            SetColorValueBitsString(BValueBitsAfter, pixelAfter, 2);
            CurrentPixelText.Text = string.Format(Properties.Resources.CurrentPixelLabel + " (x , y) : ( {0} , {1} )", pixelX, pixelY);
            if ((pixelX == imageStegVis.inputBitmap.Width - 1) && (pixelY == imageStegVis.inputBitmap.Height - 1))
            {
                NextPixelButton.IsEnabled = false;
            } else { 
                NextPixelButton.IsEnabled = true;  
            }
            if ((pixelX == 0) && (pixelY == 0))
            {
                PrevPixelButton.IsEnabled = false;
            } else
            {
                PrevPixelButton.IsEnabled = true; 
            }
        }

        private void SetColorValueBitsString(TextBlock tb, Color pixel, int color)
        {
            tb.Text = "";
            byte colorByte;
            if (color == 0) { colorByte = pixel.R; }
            else if (color == 1) { colorByte = pixel.G; }
            else { colorByte = pixel.B; }
            BitArray bitsArray = new BitArray(new byte[] { colorByte });
            for (int i = 7; i >= 0; i--)
            {
                if (bitsArray[i])
                {
                    tb.Inlines.Add("1");
                }
                else
                {
                    tb.Inlines.Add("0");
                }
            }
        }

        void UpdatePixelConversionViewClick(object sender, RoutedEventArgs e)
        {
            try
            {
                int x = Int32.Parse(PixelX.Text);
                int y = Int32.Parse(PixelY.Text);

                if ((x < 0) || (x >= imageStegVis.inputBitmap.Width) || (y < 0) || (y >= imageStegVis.inputBitmap.Height))
                {
                    InvalidXYMessage.Visibility = Visibility.Visible;
                    ValidXY.Text = GetValidCoordinates(imageStegVis.inputBitmap);
                }
                else
                {
                    InvalidXYMessage.Visibility = Visibility.Hidden;
                    pixelX = x;
                    pixelY = y;
                    SetPixelConversionView(x, y);
                }
            }
            catch (System.FormatException)
            {
                InvalidXYMessage.Visibility = Visibility.Visible;
                ValidXY.Text = GetValidCoordinates(imageStegVis.inputBitmap);
            }
        }

        void NextPixelClick(object sender, RoutedEventArgs e)
        {
            PrevPixelButton.IsEnabled = true;

            if (pixelX == imageStegVis.inputBitmap.Width - 1 && pixelY < imageStegVis.inputBitmap.Height - 1)
            {
                pixelY++;
                pixelX = 0;
            } else
            {
                pixelX++;
            }

            if ((pixelX == imageStegVis.inputBitmap.Width - 1) && (pixelY == imageStegVis.inputBitmap.Height - 1))
            {
                NextPixelButton.IsEnabled = false;
            }

            SetPixelConversionView(pixelX, pixelY);

            PixelX.Text = Properties.Resources.XCoTextArea;
            PixelY.Text = Properties.Resources.YCoTextArea;
            PixelX.GotFocus += TextBoxClicked;
            PixelY.GotFocus += TextBoxClicked;
            InvalidXYMessage.Visibility = Visibility.Hidden;
        }

        void PrevPixelClick(object sender, RoutedEventArgs e)
        {
            NextPixelButton.IsEnabled = true;      

            if (pixelX == 0 && pixelY > 0)
            {
                pixelY--;
                pixelX = imageStegVis.inputBitmap.Width - 1;
            }
            else
            {
                pixelX--;
            }

            SetPixelConversionView(pixelX, pixelY);

            if ((pixelX == 0) && (pixelY == 0))
            {
                PrevPixelButton.IsEnabled = false;
            }

            
            PixelX.Text = Properties.Resources.XCoTextArea;
            PixelY.Text = Properties.Resources.YCoTextArea;
            PixelX.GotFocus += TextBoxClicked;
            PixelY.GotFocus += TextBoxClicked;
            InvalidXYMessage.Visibility = Visibility.Hidden;
        }

        private string GetValidCoordinates(Bitmap inputImage)
        {
            string validXYstring = string.Format("0 <= x < {0} and 0 <= y < {1}", imageStegVis.inputBitmap.Width, imageStegVis.inputBitmap.Height);
            return validXYstring;
        }

        void PixelConversionViewClick(object sender, RoutedEventArgs e)
        {
            PixelConversionView.Visibility = Visibility.Visible;
            SetPixelConversionView(pixelX, pixelY);
        }

        void TextBoxClicked(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.Text = string.Empty;
            tb.FontStyle = FontStyles.Normal;
            tb.GotFocus -= TextBoxClicked;
        }


        #endregion

        #region Bit planes view methods
        void ShowBitPlanesView(object sender, RoutedEventArgs e)
        {
            MainMenu.Visibility = Visibility.Hidden;
            BitPlanesView.Visibility = Visibility.Visible;
            DisplayRedBitPlanesClick(sender, e);
        }

        public void InitBitPlanes(Plane[] rgbPlanes)
        {
            bitplanes = new Bitmap[24];
            Parallel.For(0, 24, i =>
            {
                Color layerColor;
                if (i % 3 == 0)
                {
                    layerColor = Color.Blue;
                }
                else if (i % 3 == 1)
                {
                    layerColor = Color.Green;
                }
                else
                {
                    layerColor = Color.Red;
                }
                Plane currentPlane = rgbPlanes[i];
                bool[,] plane = currentPlane.GetPlane();
                bitplanes[i] = new Bitmap(currentPlane.GetHeight(), currentPlane.GetWidth());
                for (int j = 0; j < bitplanes[i].Height; j++)
                {
                    for (int k = 0; k < bitplanes[i].Width; k++)
                    {
                        if (plane[j, k])
                        {
                            bitplanes[i].SetPixel(k, j, Color.White);
                        }
                        else
                        {
                            bitplanes[i].SetPixel(k, j, layerColor);
                        }
                    }
                }

            });
        }


        void DisplayRedBitPlanesClick(object sender, RoutedEventArgs e)
        {
            GreenBitPlanesButton.IsEnabled = true;
            BlueBitPlanesButton.IsEnabled = true;
            RedBitPlanesButton.IsEnabled = false;
            BitPlane0.Source = BitmapConversion.BitmapToBitmapSource(bitplanes[2]);
            BitPlane1.Source = BitmapConversion.BitmapToBitmapSource(bitplanes[5]);
            BitPlane2.Source = BitmapConversion.BitmapToBitmapSource(bitplanes[8]);
            BitPlane3.Source = BitmapConversion.BitmapToBitmapSource(bitplanes[11]);
            BitPlane4.Source = BitmapConversion.BitmapToBitmapSource(bitplanes[14]);
            BitPlane5.Source = BitmapConversion.BitmapToBitmapSource(bitplanes[17]);
            BitPlane6.Source = BitmapConversion.BitmapToBitmapSource(bitplanes[20]);
            BitPlane7.Source = BitmapConversion.BitmapToBitmapSource(bitplanes[23]);
        }

        void DisplayGreenBitPlanesClick(object sender, RoutedEventArgs e)
        {
            BlueBitPlanesButton.IsEnabled = true;
            RedBitPlanesButton.IsEnabled = true;
            GreenBitPlanesButton.IsEnabled = false;
            BitPlane0.Source = BitmapConversion.BitmapToBitmapSource(bitplanes[1]);
            BitPlane1.Source = BitmapConversion.BitmapToBitmapSource(bitplanes[4]);
            BitPlane2.Source = BitmapConversion.BitmapToBitmapSource(bitplanes[7]);
            BitPlane3.Source = BitmapConversion.BitmapToBitmapSource(bitplanes[10]);
            BitPlane4.Source = BitmapConversion.BitmapToBitmapSource(bitplanes[13]);
            BitPlane5.Source = BitmapConversion.BitmapToBitmapSource(bitplanes[16]);
            BitPlane6.Source = BitmapConversion.BitmapToBitmapSource(bitplanes[19]);
            BitPlane7.Source = BitmapConversion.BitmapToBitmapSource(bitplanes[22]);
        }

        void DisplayBlueBitPlanesClick(object sender, RoutedEventArgs e)
        {
            RedBitPlanesButton.IsEnabled = true;
            GreenBitPlanesButton.IsEnabled = true;
            BlueBitPlanesButton.IsEnabled = false;
            BitPlane0.Source = BitmapConversion.BitmapToBitmapSource(bitplanes[0]);
            BitPlane1.Source = BitmapConversion.BitmapToBitmapSource(bitplanes[3]);
            BitPlane2.Source = BitmapConversion.BitmapToBitmapSource(bitplanes[6]);
            BitPlane3.Source = BitmapConversion.BitmapToBitmapSource(bitplanes[9]);
            BitPlane4.Source = BitmapConversion.BitmapToBitmapSource(bitplanes[12]);
            BitPlane5.Source = BitmapConversion.BitmapToBitmapSource(bitplanes[15]);
            BitPlane6.Source = BitmapConversion.BitmapToBitmapSource(bitplanes[18]);
            BitPlane7.Source = BitmapConversion.BitmapToBitmapSource(bitplanes[21]);
        }

        #endregion

    }
}
