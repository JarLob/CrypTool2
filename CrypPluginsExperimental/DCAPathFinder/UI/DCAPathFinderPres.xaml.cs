using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DCAPathFinder.UI
{
    /// <summary>
    /// Interaktionslogik für DCAPathFinderPres.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("DCAPathFinder.Properties.Resources")]
    public partial class DCAPathFinderPres : UserControl, INotifyPropertyChanged
    {
        private int _stepCounter;
        private int _tutorialNumber;
        private int _currentTutorialLastSlideNumber;
        private string _selectedTutorial;
        private bool _isNextPossible;
        private bool _isPreviousPossible;
        private bool _workspaceRunning;

        /// <summary>
        /// Constructor
        /// </summary>
        public DCAPathFinderPres()
        {
            _stepCounter = 0;
            _currentTutorialLastSlideNumber = 3;
            _workspaceRunning = false;
            _isNextPossible = true;
            _isPreviousPossible = false;

            DataContext = this;
            InitializeComponent();

            SetupView();
        }

        /// <summary>
        /// Handles the different views
        /// </summary>
        public void SetupView()
        {
            //introduction slides
            if (!WorkspaceRunning)
            {
                _currentTutorialLastSlideNumber = 3;
                switch (StepCounter)
                {
                    case 0:
                    {
                        //setup possible button actions
                        IsPreviousPossible = false;
                        IsNextPossible = true;

                        //setup pres content
                        ContentViewBox.Child = new Overview();
                    }
                        break;
                    case 1:
                    {
                        //setup possible button actions
                        IsPreviousPossible = true;
                        IsNextPossible = true;

                        //setup pres content
                        ContentViewBox.Child = new TutorialDescriptions();
                    }
                        break;
                    case 2:
                    {
                        //setup possible button actions
                        IsPreviousPossible = true;
                        IsNextPossible = false;

                        //setup pres content
                        var startMask = new StartMask();
                        Binding myBinding = new Binding();
                        myBinding.Source = this;
                        myBinding.Path = new PropertyPath("SelectedTutorial");
                        myBinding.Mode = BindingMode.OneWay;
                        BindingOperations.SetBinding(startMask.TutorialTextBlock, TextBlock.TextProperty, myBinding);
                        ContentViewBox.Child = startMask;
                    }
                        break;
                }
            }
            //tutorial slides
            else
            {
                //check the selected tutorial number
                switch (TutorialNumber)
                {
                    //this is tutorial number 1
                    case 1:
                    {
                        _currentTutorialLastSlideNumber = 10;
                        //check the current step
                        switch (StepCounter)
                        {
                            case 0:
                            {
                                //setup possible button actions
                                IsPreviousPossible = false;
                                IsNextPossible = true;

                                //setup pres content
                                ContentViewBox.Child = new Tutorial1.Title();
                            }
                                break;
                            case 1:
                            {
                                //setup possible button actions
                                IsPreviousPossible = true;
                                IsNextPossible = true;

                                //setup pres content
                                ContentViewBox.Child = new Tutorial1.IntroductionHeader();
                            }
                                break;
                            case 2:
                            {
                                //setup possible button actions
                                IsPreviousPossible = true;
                                IsNextPossible = true;

                                //setup pres content
                                ContentViewBox.Child = new Tutorial1.IntroductionSlide1();
                            }
                                break;
                            case 3:
                            {
                                //setup possible button actions
                                IsPreviousPossible = true;
                                IsNextPossible = true;

                                //setup pres content
                                ContentViewBox.Child = new Tutorial1.IntroductionSlide2();
                            }
                                break;
                            case 4:
                            {
                                //setup possible button actions
                                IsPreviousPossible = true;
                                IsNextPossible = true;

                                //setup pres content
                                ContentViewBox.Child = new Tutorial1.IntroductionSlide3();
                            }
                                break;
                            case 5:
                            {
                                //setup possible button actions
                                IsPreviousPossible = true;
                                IsNextPossible = true;

                                //setup pres content
                                ContentViewBox.Child = new Tutorial1.DifferentialCryptanalysisHeader();
                            }
                                break;
                            case 6:
                            {
                                //setup possible button actions
                                IsPreviousPossible = true;
                                IsNextPossible = true;

                                //setup pres content
                                ContentViewBox.Child = new Tutorial1.DifferentialCryptanalysisSlide1();
                            }
                                break;
                            case 7:
                            {
                                //setup possible button actions
                                IsPreviousPossible = true;
                                IsNextPossible = true;

                                //setup pres content
                                ContentViewBox.Child = new Tutorial1.DifferentialCryptanalysisSlide2();
                            }
                                break;
                            case 8:
                            {
                                //setup possible button actions
                                IsPreviousPossible = true;
                                IsNextPossible = true;

                                //setup pres content
                                ContentViewBox.Child = new Tutorial1.DifferentialCryptanalysisSlide3();
                            }
                                break;
                            case 9:
                            {
                                //setup possible button actions
                                IsPreviousPossible = true;
                                IsNextPossible = true;

                                //setup pres content
                                ContentViewBox.Child = new Tutorial1.DifferentialCryptanalysisSlide4();
                            }
                                break;
                            case 10:
                            {
                                //setup possible button actions
                                IsPreviousPossible = true;
                                IsNextPossible = true;

                                //setup pres content
                                ContentViewBox.Child = new Tutorial1.DifferentialCryptanalysisSlide5();
                            }
                                break;
                            case 11:
                            {
                                //setup possible button actions
                                IsPreviousPossible = true;
                                IsNextPossible = true;

                                //setup pres content
                                ContentViewBox.Child = new Tutorial1.DifferentialCryptanalysisSlide6();
                            }
                                break;
                            case 12:
                            {
                                //setup possible button actions
                                IsPreviousPossible = true;
                                IsNextPossible = true;

                                //setup pres content
                                ContentViewBox.Child = new Tutorial1.DifferentialCryptanalysisSlide7();
                            }
                                break;
                            case 13:
                            {
                                //setup possible button actions
                                IsPreviousPossible = true;
                                IsNextPossible = true;

                                //setup pres content
                                ContentViewBox.Child = new Tutorial1.DifferentialCryptanalysisSlide8();
                            }
                                break;
                            case 14:
                            {
                                //setup possible button actions
                                IsPreviousPossible = true;
                                IsNextPossible = true;

                                //setup pres content
                                ContentViewBox.Child = new Tutorial1.DifferentialCryptanalysisSlide9();
                            }
                                break;
                        }
                    }
                        break;
                }
            }
        }

        #region ButtonHandler

        /// <summary>
        /// Handles a next step
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnNextClicked(object sender, RoutedEventArgs e)
        {
            //increment to go to the next step
            StepCounter++;
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
            StepCounter--;
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

        #endregion

        #region Properties

        /// <summary>
        /// Property for indicating that the workspace is running
        /// </summary>
        public bool WorkspaceRunning
        {
            get { return _workspaceRunning; }
            set
            {
                _workspaceRunning = value;
                OnPropertyChanged();

                StepCounter = 0;
                SetupView();
            }
        }

        /// <summary>
        /// Property for button previous
        /// </summary>
        public bool IsPreviousPossible
        {
            get { return _isPreviousPossible; }
            set
            {
                _isPreviousPossible = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for button next
        /// </summary>
        public bool IsNextPossible
        {
            get { return _isNextPossible; }
            set
            {
                _isNextPossible = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for selected Tutorial
        /// </summary>
        public string SelectedTutorial
        {
            get { return _selectedTutorial; }
            set
            {
                _selectedTutorial = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for the tutorial number
        /// </summary>
        public int TutorialNumber
        {
            get { return _tutorialNumber; }
            set
            {
                _tutorialNumber = value;
                SelectedTutorial = Properties.Resources.StartMaskContent2.Replace("{0}", TutorialNumber.ToString());
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for step counter
        /// </summary>
        public int StepCounter
        {
            get { return _stepCounter; }
            set
            {
                _stepCounter = value;
                OnPropertyChanged();
                OnPropertyChanged("SlideCounter");
            }
        }

        /// <summary>
        /// Property for slide counter in the UI
        /// </summary>
        public string SlideCounter
        {
            get
            {
                return ((StepCounter + 1) + "/" + _currentTutorialLastSlideNumber);
            }
        }

        #endregion

        /// <summary>
        /// Event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Method to call if data changes
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
