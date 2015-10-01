 
using System;
using System.Configuration;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CrypCloud.Core;
using Cryptool.PluginBase.Attributes; 
namespace CrypCloud.Manager
{
    /// <summary>
    /// </summary>
    [Localization("CrypCloud.Manager.Properties.Resources")]
    [SettingsTab("CrypCloudSettings", "/MainSettings/")]
    public partial class CrypCloudSettingsTab : UserControl
    {
        private int minvalue = 1;
        private readonly int maxvalue = Environment.ProcessorCount - 1;
        private readonly int startvalue;
        public CrypCloudSettingsTab(Style settingsStyle)
        {
            startvalue = Settings.Default.amountOfWorker;
            CrypCloudCore.Instance.AmountOfWorker = startvalue; 
            Resources.Add("settingsStyle", settingsStyle);
            InitializeComponent(); 
            NUDTextBox.Text = startvalue.ToString(); 
        }

        private void NUDButtonUP_Click(object sender, RoutedEventArgs e)
        {
            var number = NUDTextBox.Text != "" ? Convert.ToInt32(NUDTextBox.Text) : 0;
            if (number < maxvalue)
                NUDTextBox.Text = Convert.ToString(number + 1);
        }

        private void NUDButtonDown_Click(object sender, RoutedEventArgs e)
        {
            int number;
            if (NUDTextBox.Text != "") number = Convert.ToInt32(NUDTextBox.Text);
            else number = 0;
            if (number > minvalue)
                NUDTextBox.Text = Convert.ToString(number - 1);
        }

        private void NUDTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Up)
            {
                NUDButtonUP.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NUDButtonUP, new object[] { true });
            }


            if (e.Key == Key.Down)
            {
                NUDButtonDown.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NUDButtonDown, new object[] { true });
            }
        }

        private void NUDTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NUDButtonUP, new object[] { false });

            if (e.Key == Key.Down)
                typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NUDButtonDown, new object[] { false });
        }

        private void NUDTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int number = 0;
            if (NUDTextBox.Text != "")
                if (!int.TryParse(NUDTextBox.Text, out number)) NUDTextBox.Text = startvalue.ToString();
            if (number > maxvalue) NUDTextBox.Text = maxvalue.ToString();
            if (number < minvalue) NUDTextBox.Text = minvalue.ToString();
            NUDTextBox.SelectionStart = NUDTextBox.Text.Length;

            Settings.Default.amountOfWorker = number;
            Settings.Default.Save();
            CrypCloudCore.Instance.AmountOfWorker = number; 
        }

    }
}
