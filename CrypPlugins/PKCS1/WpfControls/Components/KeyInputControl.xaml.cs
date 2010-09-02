﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;
using System.Text.RegularExpressions;
using PKCS1.WpfResources;
using PKCS1.Resources.lang.Gui;
using PKCS1.Library;
using Org.BouncyCastle.Math;

namespace PKCS1.WpfControls.Components
{
    /// <summary>
    /// Interaction logic for KeyInputControl.xaml
    /// </summary>
    public partial class KeyInputControl : UserControl, IPkcs1UserControl
    {
        private bool m_bPrivKeyValid = false;
        private bool m_bModulusValid = false;
        private int m_radixModulus = 16;
        private int m_radixPrivKey = 16;

        private enum ParameterName
        {
            PrivKey,
            PubKey,
            Modulus
        }

        public KeyInputControl()
        {
            InitializeComponent();
            this.btnValInput.IsEnabled = false;
            this.btnPrivKeyHexadec.IsChecked = true;
            this.btnModHexadec.IsChecked = true;
        }

        private void tbPubKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            /*
            per Binding an RsaKey gebunden; zulässiger Eingabebereich wird per .xaml gesteuert
             */
        }

        private void tbPrivKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.m_bPrivKeyValid = this.checkInputTextBox(tbPrivKey.Text, this.m_radixPrivKey, lblErrorPrivKey, ParameterName.PrivKey);
            this.testAndEnableButton();
        }

        private void tbModulus_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.m_bModulusValid = this.checkInputTextBox(tbModulus.Text, this.m_radixModulus, lblErrorModulus, ParameterName.Modulus);
            this.testAndEnableButton();            
        }

        private bool checkInputTextBox(string inputText, int radix, Label outputLabel, ParameterName paramName)
        {
            if (inputText != string.Empty)
            {
                if (isInputInRightFormat(inputText, radix))
                {
                    BigInteger tmp = new BigInteger(inputText, radix);

                    if (tmp.BitLength > Convert.ToInt32(tbKeyLength.Text))
                    {
                        outputLabel.Content = RsaKeyInputCtrl.errorBitLengthShorter;
                        return false;
                    }
                    else
                    {
                        if (paramName == ParameterName.PrivKey) { RsaKey.Instance.setPrivKey(inputText, radix); }
                        if (paramName == ParameterName.Modulus) { RsaKey.Instance.setModulus(inputText, radix); }
                        outputLabel.Content = string.Empty;
                        return true;
                    }
                }
                else
                {
                    outputLabel.Content = RsaKeyInputCtrl.errorValidSignsOnly;
                    return false;
                }
            }
            else
            {
                outputLabel.Content = RsaKeyInputCtrl.errorInsertNumber;
                return false;
            }
            return false;
        }

        #region IPkcs1UserControl Member

        void IPkcs1UserControl.Dispose()
        {
            //throw new NotImplementedException();
        }

        void IPkcs1UserControl.Init()
        {
            //throw new NotImplementedException();
        }

        void IPkcs1UserControl.SetTab(int i)
        {
            //throw new NotImplementedException();
        }

        #endregion

        private void btnValInput_Click(object sender, RoutedEventArgs e)
        {          
            RsaKey.Instance.setInputParams();
        }

        private bool isInputInRightFormat(string input, int radix)
        {
            if (10 == radix)
            {
                Match invalid_chars = Regex.Match(input, "[^0-9]");
                return !invalid_chars.Success;
            }
            else if (16 == radix)
            {
                Match invalid_chars = Regex.Match(input, "[^0-9a-fA-F]");
                return !invalid_chars.Success;
            }
            return false;
        }

        private void testAndEnableButton()
        {
            if (this.m_bModulusValid &&
                this.m_bPrivKeyValid &&
                tbPubKey.Text != string.Empty)
            {
                this.btnValInput.IsEnabled = true;
                this.lblResult.Content = string.Empty;
            }
            else
            {
                this.btnValInput.IsEnabled = false;
            }
        }

        private void btnDecimal_Click(object sender, RoutedEventArgs e)
        {
            if (sender == btnModDecimal)
            {
                this.m_radixModulus = 10;
                this.m_bModulusValid = this.checkInputTextBox(tbModulus.Text, this.m_radixModulus, lblErrorModulus, ParameterName.Modulus);
            }
            else if (sender == btnPrivKeyDecimal)
            {
                this.m_radixPrivKey = 10;
                this.m_bPrivKeyValid = this.checkInputTextBox(tbPrivKey.Text, this.m_radixPrivKey, lblErrorPrivKey, ParameterName.PrivKey);
            }
            this.testAndEnableButton();
        }

        private void btnHexadec_Click(object sender, RoutedEventArgs e)
        {
            if (sender == btnModHexadec)
            {
                this.m_radixModulus = 16;
                this.m_bModulusValid = this.checkInputTextBox(tbModulus.Text, this.m_radixModulus, lblErrorModulus, ParameterName.Modulus);
            }
            else if (sender == btnPrivKeyHexadec)
            {
                this.m_radixPrivKey = 16;
                this.m_bPrivKeyValid = this.checkInputTextBox(tbPrivKey.Text, this.m_radixPrivKey, lblErrorPrivKey, ParameterName.PrivKey);
            }
            this.testAndEnableButton();
        }

        private void btn_Help_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender == btnHelpPubKey)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(PKCS1.OnlineHelp.OnlineHelpActions.KeyGen_PubExponent);
            }
            else if (sender == btnHelpBitSizeModulus)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(PKCS1.OnlineHelp.OnlineHelpActions.KeyGen_ModulusSize);
            }
            e.Handled = true;
        }
    }
}
