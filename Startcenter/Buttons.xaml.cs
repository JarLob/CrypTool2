 using System.IO;
using System.Windows;
using System.Windows.Controls; 
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using OnlineDocumentationGenerator.Generators.HtmlGenerator;
using System.Windows.Documents;
using System;

namespace Startcenter
{
    /// <summary>
    /// Interaction logic for Buttons.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("Startcenter.Properties.Resources")]
    public partial class Buttons : UserControl
    {
        public event OpenEditorHandler OnOpenEditor;

        public Buttons()
        {
            InitializeComponent();
        }

        private void WizardButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OnOpenEditor(typeof(Wizard.Wizard), null);
            }
            catch (Exception)
            {
                //do nothing
            }
        }
            
        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OnlineHelp.InvokeShowDocPage(null);
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private void WorkspaceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OnOpenEditor(typeof(WorkspaceManager.WorkspaceManagerClass), null);
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private void WebpageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://www.cryptool.org/cryptool2");
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private void YouTubeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://www.youtube.com/channel/UC8_FqvQWJfZYxcSoEJ5ob-Q");
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private void FacebookButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://www.facebook.de/cryptool2");
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private void BookButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(Path.Combine(DirectoryHelper.BaseDirectory, Properties.Resources.CTBookFilename));
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private void CrypToolStoreButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OnOpenEditor(typeof(Cryptool.CrypToolStore.CrypToolStoreEditor), new TabInfo()
                {
                    Title = Properties.Resources.CrypToolStore,
                    Tooltip = new Span(new Run(Properties.Resources.CrypToolStore))
                });
            }
            catch (Exception)
            {
                //do nothing
            }
        }
    }
}
