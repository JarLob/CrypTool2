using System;
using System.Collections;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace TextSteganography
{
    [Cryptool.PluginBase.Attributes.Localization("TextSteganography.Properties.Resources")]
    public partial class TextSteganographyPresentation : UserControl
    {
        private TextSteganography textSteg; 

        public TextSteganographyPresentation(TextSteganography textSteg)
        {
            InitializeComponent();
            this.textSteg = textSteg;
        }

        public void ShowZeroWidthSpaceEncoding()
        {
            StegoTextBlock.Text = "";
            if(textSteg.GetAction() == ActionType.Hide)
            {
                BitArray messagebits = new BitArray(Encoding.UTF8.GetBytes(textSteg.InputSecretMessage));
                StegoTextBlock.Inlines.Add(textSteg.CoverText.Substring(0, textSteg.offset));
                for (int i = 0; i < messagebits.Length; i++)
                {
                    if (messagebits[i])
                    {
                        StegoTextBlock.Inlines.Add(new Run(" ") { Background = Brushes.Aquamarine });
                    }
                    else
                    {
                        StegoTextBlock.Inlines.Add(new Run(" ") { Background = Brushes.LightYellow });
                    }
                }
                StegoTextBlock.Inlines.Add(textSteg.CoverText.Substring(textSteg.offset));
            } else
            {
                for(int i = 0; i < textSteg.CoverText.Length; i++)
                {
                    if(textSteg.CoverText[i] == '\u200b')
                    {
                        StegoTextBlock.Inlines.Add(new Run(" ") { Background = Brushes.Aquamarine });
                    } else if(textSteg.CoverText[i] == '\u200c')
                    {
                        StegoTextBlock.Inlines.Add(new Run(" ") { Background = Brushes.LightYellow });
                    } else
                    {
                        StegoTextBlock.Inlines.Add((textSteg.CoverText[i]).ToString());
                    }
                }
            }
            
            CBShowBits.IsChecked = false;
        }

        public void ShowBitsCheckbox()
        {
            CBPanel.Visibility = Visibility.Visible; 
        }

        public void HideBitsCheckBox()
        {
            CBPanel.Visibility = Visibility.Hidden;
        }

        public void ClearPres()
        {
            StegoTextBlock.Text = ""; 
        }

        public void HideMessageBits(object sender, RoutedEventArgs e)
        {
            ShowZeroWidthSpaceEncoding();
        }

        public void ShowMessageBits(object sender, RoutedEventArgs e)
        {
            StegoTextBlock.Text = "";
            
            if(textSteg.GetAction() == ActionType.Hide)
            {
                BitArray messagebits = new BitArray(Encoding.UTF8.GetBytes(textSteg.InputSecretMessage));
                StegoTextBlock.Inlines.Add(textSteg.CoverText.Substring(0, textSteg.offset));
                for (int i = 0; i < messagebits.Length; i++)
                {
                    if (messagebits[i])
                    {
                        StegoTextBlock.Inlines.Add(new Run("1") { Background = Brushes.Aquamarine });
                    }
                    else
                    {
                        StegoTextBlock.Inlines.Add(new Run("0") { Background = Brushes.LightYellow });
                    }
                }
                StegoTextBlock.Inlines.Add(textSteg.CoverText.Substring(textSteg.offset));
            } else
            {
                for (int i = 0; i < textSteg.CoverText.Length; i++)
                {
                    if (textSteg.CoverText[i] == '\u200b')
                    {
                        StegoTextBlock.Inlines.Add(new Run("1") { Background = Brushes.Aquamarine });
                    }
                    else if (textSteg.CoverText[i] == '\u200c')
                    {
                        StegoTextBlock.Inlines.Add(new Run("0") { Background = Brushes.LightYellow });
                    }
                    else
                    {
                        StegoTextBlock.Inlines.Add((textSteg.CoverText[i]).ToString());
                    }
                }
            }
            
        }

        public void ShowCapitalLetterEncoding(string stegoText)
        {
            StegoTextBlock.Text = "";
            for (int i = 0; i < stegoText.Length; i++)
            {
                char c = stegoText[i];
                if (Char.IsUpper(stegoText[i]))
                {
                    StegoTextBlock.Inlines.Add(new Run(Char.ToString(stegoText[i])) { FontWeight = FontWeights.Bold, Background = Brushes.Aquamarine });
                }
                else if (stegoText[i] == '\n')
                {
                    StegoTextBlock.Inlines.Add(new Run(Char.ToString(stegoText[i])) { Background = Brushes.Aquamarine });
                }
                else
                {
                    StegoTextBlock.Inlines.Add(new Run(Char.ToString(stegoText[i])) { });
                }
            }
        }

        public void ShowLettersMarkingEncoding(string stegoText)
        {
            StegoTextBlock.Text = "";
            for (int i = 0; i < stegoText.Length - 1; i++)
            {
                char c = stegoText[i];
                char d = stegoText[i + 1];

                if (d == '\u0323')
                {
                    string s = "";
                    s += c;
                    s += d;

                    StegoTextBlock.Inlines.Add(new Run(s) { FontWeight = FontWeights.Bold, Background = Brushes.Aquamarine });
                    i++;
                }
                else
                {
                    StegoTextBlock.Inlines.Add(new Run(Char.ToString(c)));
                }
            }
        }
    }
}
