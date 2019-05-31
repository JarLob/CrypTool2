using System.Windows;
using System.Windows.Controls;

namespace DCAPathFinder.UI
{
    /// <summary>
    /// Interaktionslogik für DCAPathFinderPres.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("DCAPathFinder.Properties.Resources")]
    public partial class DCAPathFinderPres : UserControl
    {
        private int _stepCounter;

        /// <summary>
        /// Constructor
        /// </summary>
        public DCAPathFinderPres()
        {
            _stepCounter = 0;

            InitializeComponent();

            SetupView();
        }

        /// <summary>
        /// Handles the different views
        /// </summary>
        private void SetupView()
        {
            switch (_stepCounter)
            {
                case 0:
                {
                    ContentViewBox.Child = new Overview();
                }
                    break;
                case 1:
                {
                    ContentViewBox.Child = new TutorialDescriptions();
                }
                    break;
            }
        }

        /// <summary>
        /// Handles a next step
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnNextClicked(object sender, RoutedEventArgs e)
        {
            //increment to go to the next step
            _stepCounter++;
            SetupView();
        }

        /// <summary>
        /// Handles a previous step
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPreviousClicked(object sender, RoutedEventArgs e)
        {
            //decrement to go to the previous step
            _stepCounter--;
            SetupView();
        }

        /// <summary>
        /// Handles a skip chapter operation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSkipChapterClicked(object sender, RoutedEventArgs e)
        {

        }
    }
}
