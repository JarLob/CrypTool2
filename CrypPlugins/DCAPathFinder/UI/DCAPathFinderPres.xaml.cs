using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using DCAPathFinder.Logic;
using DCAPathFinder.Logic.Cipher2;
using DCAPathFinder.UI.Controls;
using DCAPathFinder.UI.Models;
using DCAPathFinder.UI.Tutorial2;
using DCAPathFinder.UI.Tutorial3;

namespace DCAPathFinder.UI
{
    /// <summary>
    /// Interaktionslogik für DCAPathFinderPres.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("DCAPathFinder.Properties.Resources")]
    public partial class DCAPathFinderPres : UserControl, INotifyPropertyChanged
    {
        public AutoResetEvent buttonNextClickedEvent;
        public AutoResetEvent buttonPrevClickedEvent;
        public AutoResetEvent sendDataEvent;
        public AutoResetEvent workDataEvent;
        public event EventHandler<MessageEventArgs> MessageToDisplayOccured;
        public event EventHandler<ProgressEventArgs> ProgressChangedOccured;

        private double _progressIncrement;
        private int _stepCounter;
        private bool _presentationMode;
        private bool _automaticMode;
        private int _tutorialNumber;
        private int _currentTutorialLastSlideNumber;
        private Visibility _slideCounterVisibility;
        private string _selectedTutorial;
        private bool _isNextPossible;
        private bool _isPreviousPossible;
        private bool _isSkipPossible;
        private bool _workspaceRunning;
        private bool[] _SBoxesAlreadyAttacked;
        private bool[] _SBoxesCurrentAttack;
        private DispatcherTimer _dispatcher;
        private SearchPolicy _searchPolicy;
        private int _messageCount;
        public bool UIProgressRefresh = true;

        /// <summary>
        /// Constructor
        /// </summary>
        public DCAPathFinderPres()
        {
            _dispatcher = new System.Windows.Threading.DispatcherTimer();
            _dispatcher.Tick += new EventHandler(highlightComponent);
            _dispatcher.Interval = new TimeSpan(0, 0, 0, 500);

            buttonNextClickedEvent = new AutoResetEvent(false);
            buttonPrevClickedEvent = new AutoResetEvent(false);
            sendDataEvent = new AutoResetEvent(false);
            workDataEvent = new AutoResetEvent(false);

            _SBoxesAlreadyAttacked = new bool[4];
            _SBoxesCurrentAttack = new bool[4];

            _stepCounter = 0;
            _slideCounterVisibility = Visibility.Hidden;
            _workspaceRunning = false;
            _isNextPossible = true;
            _isPreviousPossible = false;

            DataContext = this;
            InitializeComponent();

            SetupView();

            //setup pres content
            ContentViewBox.Child = new Overview();
            IsPreviousPossible = false;
            IsNextPossible = false;
        }

        /// <summary>
        /// handler to highlight the button next
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void highlightComponent(object sender, EventArgs e)
        {
            if (BtnNext.Background == Brushes.LightGray)
            {
                BtnNext.Background = Brushes.LimeGreen;
            }
            else
            {
                BtnNext.Background = Brushes.LightGray;
            }
        }

        /// <summary>
        /// Handles the different views
        /// </summary>
        public void SetupView()
        {
            //introduction slides
            if (PresentationMode && !AutomaticMode)
            {
                //set last slide num
                switch (TutorialNumber)
                {
                    case 1:
                    {
                        _progressIncrement = 1.0 / (TutorialConfiguration.TUTORIAL1STATESWITHPRES - 1);
                        CurrentTutorialLastSlideNumber = TutorialConfiguration.TUTORIAL1STATESWITHPRES;
                        OnPropertyChanged("SlideCounter");
                    }
                        break;
                    case 2:
                    {
                        _progressIncrement = 0.5 / (TutorialConfiguration.TUTORIAL2STATESWITHPRES - 8);
                        CurrentTutorialLastSlideNumber = TutorialConfiguration.TUTORIAL2STATESWITHPRES;
                        OnPropertyChanged("SlideCounter");
                    }
                        break;
                    case 3:
                    {
                    }
                        break;
                }

                if (StepCounter == 0)
                {
                    if (WorkspaceRunning)
                    {
                        //setup possible button actions
                        IsSkipPossible = true;
                        IsPreviousPossible = false;
                        IsNextPossible = true;
                    }

                    //setup pres content
                    ContentViewBox.Child = new Overview();
                }
                else if (StepCounter == 1)
                {
                    //setup possible button actions
                    IsSkipPossible = true;
                    IsPreviousPossible = true;
                    IsNextPossible = true;

                    //setup pres content
                    ContentViewBox.Child = new TutorialDescriptions();
                }
                //StepCounter > 1
                else
                {
                    //check the selected tutorial number
                    switch (TutorialNumber)
                    {
                        //this is tutorial number 1
                        case 1:
                        {
                            _currentTutorialLastSlideNumber = TutorialConfiguration.TUTORIAL1STATESWITHPRES;
                            OnPropertyChanged("SlideCounter");

                            //check the current step
                            switch (StepCounter)
                            {
                                case 2:
                                {
                                    //setup possible button actions
                                    IsPreviousPossible = true;
                                    IsSkipPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial1.Title();
                                }
                                    break;
                                case 3:
                                {
                                    //setup possible button actions
                                    IsPreviousPossible = true;
                                    IsSkipPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial1.IntroductionHeader();
                                }
                                    break;
                                case 4:
                                {
                                    //setup possible button actions
                                    IsPreviousPossible = true;
                                    IsSkipPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial1.IntroductionSlide1();
                                }
                                    break;
                                case 5:
                                {
                                    //setup possible button actions
                                    IsPreviousPossible = true;
                                    IsSkipPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial1.IntroductionSlide2();
                                }
                                    break;
                                case 6:
                                {
                                    //setup possible button actions
                                    IsPreviousPossible = true;
                                    IsSkipPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial1.IntroductionSlide3();
                                }
                                    break;
                                case 7:
                                {
                                    //setup possible button actions
                                    IsPreviousPossible = true;
                                    IsSkipPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial1.DifferentialCryptanalysisHeader();
                                }
                                    break;
                                case 8:
                                {
                                    //setup possible button actions
                                    IsPreviousPossible = true;
                                    IsSkipPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial1.DifferentialCryptanalysisSlide1();
                                }
                                    break;
                                case 9:
                                {
                                    //setup possible button actions
                                    IsPreviousPossible = true;
                                    IsSkipPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial1.DifferentialCryptanalysisSlide2();
                                }
                                    break;
                                case 10:
                                {
                                    //setup possible button actions
                                    IsPreviousPossible = true;
                                    IsSkipPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial1.DifferentialCryptanalysisSlide3();
                                }
                                    break;
                                case 11:
                                {
                                    //setup possible button actions
                                    IsPreviousPossible = true;
                                    IsSkipPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial1.DifferentialCryptanalysisSlide4();
                                }
                                    break;
                                case 12:
                                {
                                    //setup possible button actions
                                    IsPreviousPossible = true;
                                    IsSkipPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial1.DifferentialCryptanalysisSlide5();
                                }
                                    break;
                                case 13:
                                {
                                    //setup possible button actions
                                    IsPreviousPossible = true;
                                    IsSkipPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial1.DifferentialCryptanalysisSlide6();
                                }
                                    break;
                                case 14:
                                {
                                    //setup possible button actions
                                    IsPreviousPossible = true;
                                    IsSkipPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial1.DifferentialCryptanalysisSlide7();
                                }
                                    break;
                                case 15:
                                {
                                    //setup possible button actions
                                    IsPreviousPossible = true;
                                    IsSkipPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial1.DifferentialCryptanalysisSlide8();
                                }
                                    break;
                                case 16:
                                {
                                    //setup possible button actions
                                    IsPreviousPossible = true;
                                    IsSkipPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial1.DifferentialCryptanalysisSlide9();
                                }
                                    break;
                                case 17:
                                {
                                    //setup possible button actions
                                    IsPreviousPossible = true;
                                    IsSkipPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial1.DifferentialCryptanalysisSlide10();
                                }
                                    break;
                                case 18:
                                {
                                    //setup possible button actions
                                    IsPreviousPossible = true;
                                    IsSkipPossible = false;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial1.PracticalDifferentialCryptanalysisHeader();
                                }
                                    break;
                                case 19:
                                {
                                    //setup possible button actions
                                    IsPreviousPossible = true;
                                    IsSkipPossible = false;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial1.PracticalDifferentialCryptanalysisSlide1();
                                }
                                    break;
                                case 20:
                                {
                                    //setup possible button actions
                                    IsPreviousPossible = false;
                                    IsSkipPossible = false;
                                    IsNextPossible = false;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial1.LastSlide();
                                    sendDataEvent.Set();
                                }
                                    break;
                            }
                        }
                            break;

                        //Tutorial 2
                        case 2:
                        {
                            _currentTutorialLastSlideNumber = TutorialConfiguration.TUTORIAL2STATESWITHPRES;
                            OnPropertyChanged("SlideCounter");

                            //check the current step
                            switch (StepCounter)
                            {
                                case 2:
                                {
                                    //setup possible button actions
                                    IsSkipPossible = true;
                                    IsPreviousPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial2.Title();
                                }
                                    break;
                                case 3:
                                {
                                    //setup possible button actions
                                    IsSkipPossible = true;
                                    IsPreviousPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial2.IntroductionHeader();
                                }
                                    break;
                                case 4:
                                {
                                    //setup possible button actions
                                    IsSkipPossible = true;
                                    IsPreviousPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial2.IntroductionSlide1();
                                }
                                    break;
                                case 5:
                                {
                                    //setup possible button actions
                                    IsSkipPossible = true;
                                    IsPreviousPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial2.AnalysisHeader();
                                }
                                    break;
                                case 6:
                                {
                                    //setup possible button actions
                                    IsSkipPossible = true;
                                    IsPreviousPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial2.AnalysisSlide1();
                                }
                                    break;
                                case 7:
                                {
                                    //setup possible button actions
                                    IsSkipPossible = true;
                                    IsPreviousPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial2.AnalysisSlide2();
                                }
                                    break;
                                case 8:
                                {
                                    //setup possible button actions
                                    IsSkipPossible = true;
                                    IsPreviousPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial2.AnalysisSlide3();
                                }
                                    break;
                                case 9:
                                {
                                    //setup possible button actions
                                    IsSkipPossible = true;
                                    IsPreviousPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial2.AnalysisOfSBoxHeader();
                                }
                                    break;
                                case 10:
                                {
                                    //setup possible button actions
                                    IsSkipPossible = true;
                                    IsPreviousPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial2.AnalysisOfSBoxSlide1();
                                }
                                    break;
                                case 11:
                                {
                                    //setup possible button actions
                                    IsSkipPossible = true;
                                    IsPreviousPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial2.AnalysisOfSBoxSlide2();
                                }
                                    break;
                                case 12:
                                {
                                    //setup possible button actions
                                    IsSkipPossible = true;
                                    IsPreviousPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial2.AnalysisOfSBoxSlide3();
                                }
                                    break;
                                case 13:
                                {
                                    //setup possible button actions
                                    IsSkipPossible = true;
                                    IsPreviousPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial2.CharacteristicHeader();
                                }
                                    break;
                                case 14:
                                {
                                    //setup possible button actions
                                    IsSkipPossible = true;
                                    IsPreviousPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial2.CharacteristicSlide1();
                                }
                                    break;
                                case 15:
                                {
                                    //setup possible button actions
                                    IsSkipPossible = true;
                                    IsPreviousPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial2.CharacteristicSlide2();
                                }
                                    break;
                                case 16:
                                {
                                    //setup possible button actions
                                    IsSkipPossible = true;
                                    IsPreviousPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial2.CharacteristicSlide3();
                                }
                                    break;
                                case 17:
                                {
                                    //setup possible button actions
                                    IsSkipPossible = true;
                                    IsPreviousPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial2.DifferentialHeader();
                                }
                                    break;
                                case 18:
                                {
                                    //setup possible button actions
                                    IsSkipPossible = true;
                                    IsPreviousPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial2.DifferentialSlide1();
                                }
                                    break;
                                case 19:
                                {
                                    //setup possible button actions
                                    IsSkipPossible = true;
                                    IsPreviousPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial2.RecoverKeyInformationHeader();
                                }
                                    break;
                                case 20:
                                {
                                    //setup possible button actions
                                    IsSkipPossible = true;
                                    IsPreviousPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial2.RecoverKeyInformationSlide1();
                                }
                                    break;
                                case 21:
                                {
                                    //setup possible button actions
                                    IsSkipPossible = false;
                                    IsPreviousPossible = true;
                                    IsNextPossible = true;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial2.AttackHeader();
                                }
                                    break;
                                //attack on round 3
                                case 22:
                                {
                                    //setup possible button actions
                                    IsSkipPossible = false;
                                    IsPreviousPossible = false;
                                    IsNextPossible = true;

                                    var view = new Tutorial2.AttackKeyRound3();
                                    //Prepare view
                                    if (SBoxesAlreadyAttacked[3])
                                    {
                                        view.SBox4Round3.AlreadyAttacked = true;
                                        view.SBox4Round3.IsClickable = false;
                                    }
                                    else
                                    {
                                        view.SBox4Round3.IsClickable = true;
                                    }

                                    if (SBoxesAlreadyAttacked[2])
                                    {
                                        view.SBox3Round3.AlreadyAttacked = true;
                                        view.SBox3Round3.IsClickable = false;
                                    }
                                    else
                                    {
                                        view.SBox3Round3.IsClickable = true;
                                    }

                                    if (SBoxesAlreadyAttacked[1])
                                    {
                                        view.SBox2Round3.AlreadyAttacked = true;
                                        view.SBox2Round3.IsClickable = false;
                                    }
                                    else
                                    {
                                        view.SBox2Round3.IsClickable = true;
                                    }

                                    if (SBoxesAlreadyAttacked[0])
                                    {
                                        view.SBox1Round3.AlreadyAttacked = true;
                                        view.SBox1Round3.IsClickable = false;
                                    }
                                    else
                                    {
                                        view.SBox1Round3.IsClickable = true;
                                    }

                                    //pepare viewbox
                                    //ContentViewBox.Stretch = Stretch.None;

                                    //setup pres content
                                    SlideCounterVisibility = Visibility.Visible;
                                    ContentViewBox.Child = view;
                                    view.SelectionChanged += SBoxSelectionChanged;
                                }
                                    break;

                                case 23:
                                {
                                    //setup possible button actions
                                    IsPreviousPossible = false;
                                    IsNextPossible = false;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial2.AttackSearchResult();
                                    workDataEvent.Set();
                                }
                                    break;
                                //show waiting slide for next components
                                case 24:
                                {
                                    //setup possible button actions
                                    IsPreviousPossible = false;
                                    IsNextPossible = false;

                                    //setup pres content
                                    ContentViewBox.Child = new Tutorial2.WaitingSlide();
                                    SlideCounterVisibility = Visibility.Hidden;
                                    sendDataEvent.Set();
                                    _dispatcher.Stop();
                                }
                                    break;

                                //attack on round 2
                                case 25:
                                {
                                    //setup possible button actions
                                    IsPreviousPossible = false;
                                    IsNextPossible = true;

                                    //setup pres content
                                    var view = new Tutorial2.AttackKeyRound2();
                                    //Prepare view
                                    if (SBoxesAlreadyAttacked[3])
                                    {
                                        view.SBox4Round2.AlreadyAttacked = true;
                                        view.SBox4Round2.IsClickable = false;
                                    }

                                    if (SBoxesAlreadyAttacked[2])
                                    {
                                        view.SBox3Round2.AlreadyAttacked = true;
                                        view.SBox3Round2.IsClickable = false;
                                    }

                                    if (SBoxesAlreadyAttacked[1])
                                    {
                                        view.SBox2Round2.AlreadyAttacked = true;
                                        view.SBox2Round2.IsClickable = false;
                                    }

                                    if (SBoxesAlreadyAttacked[0])
                                    {
                                        view.SBox1Round2.AlreadyAttacked = true;
                                        view.SBox1Round2.IsClickable = false;
                                    }

                                    //setup pres content
                                    SlideCounterVisibility = Visibility.Visible;
                                    ContentViewBox.Child = view;
                                    view.SelectionChanged += SBoxSelectionChanged;
                                }
                                    break;
                                case 26:
                                {
                                    //setup possible button actions
                                    IsPreviousPossible = false;
                                    IsNextPossible = false;

                                    //setup pres content
                                    var view = new Tutorial2.AttackSearchResult();

                                    ContentViewBox.Child = view;
                                    view.DataGridCharacteristicsOutputDiffR2Col.Visibility = Visibility.Hidden;
                                    view.DataGridCharacteristicsInputDiffR3Col.Visibility = Visibility.Hidden;
                                    workDataEvent.Set();
                                }
                                    break;

                                //show waiting slide for next components
                                case 27:
                                {
                                    //setup possible button actions
                                    IsPreviousPossible = false;
                                    IsNextPossible = false;

                                    //setup pres content
                                    SlideCounterVisibility = Visibility.Hidden;
                                    ContentViewBox.Child = new Tutorial2.WaitingSlide();
                                    sendDataEvent.Set();
                                    _dispatcher.Stop();
                                }
                                    break;

                                //attack on last round
                                case 28:
                                {
                                    //setup possible button actions
                                    IsPreviousPossible = false;
                                    IsNextPossible = true;

                                    //setup pres content
                                    SlideCounterVisibility = Visibility.Visible;
                                    ContentViewBox.Child = new Tutorial2.AttackFirstRound();
                                }
                                    break;
                                case 29:
                                {
                                    //setup possible button actions
                                    IsPreviousPossible = false;
                                    IsNextPossible = false;

                                    //setup pres content
                                    SlideCounterVisibility = Visibility.Hidden;
                                    ContentViewBox.Child = new Tutorial2.AttackFinished();
                                    sendDataEvent.Set();
                                    _dispatcher.Stop();
                                }
                                    break;
                            }
                        }
                            break;

                        //Tutorial 3
                        case 3:
                        {
                        }
                            break;
                    }
                }
            }
            else if (!PresentationMode && AutomaticMode)
            {
                //setup possible button actions
                IsSkipPossible = false;
                IsPreviousPossible = false;
                IsNextPossible = false;
                SlideCounterVisibility = Visibility.Hidden;

                //setup pres content
                ContentViewBox.Child = new AutomaticMode();
            }
            //no presentation, no automatic
            else
            {
                if (StepCounter == 0)
                {
                    if (WorkspaceRunning)
                    {
                        //setup possible button actions
                        IsSkipPossible = false;
                        IsPreviousPossible = false;
                        IsNextPossible = true;
                    }
                    else
                    {
                        //setup pres content
                        ContentViewBox.Child = new Overview();
                    }
                }

                //check active tutorial number
                switch (TutorialNumber)
                {
                    //tutorial 1 without pres
                    case 1:
                    {
                        _currentTutorialLastSlideNumber = TutorialConfiguration.TUTORIAL1STATESWITHOUTPRES;
                        OnPropertyChanged("SlideCounter");

                        //presentation for tutorial 1
                        switch (StepCounter)
                        {
                            case 0:
                            {
                                //setup pres content
                                ContentViewBox.Child = new Overview();
                            }
                                break;
                            case 1:
                            {
                                //setup possible button actions
                                IsPreviousPossible = true;
                                IsSkipPossible = false;
                                IsNextPossible = true;

                                //setup pres content
                                ContentViewBox.Child = new Tutorial1.PracticalDifferentialCryptanalysisSlide1();
                            }
                                break;
                            case 2:
                            {
                                //setup possible button actions
                                IsPreviousPossible = false;
                                IsSkipPossible = false;
                                IsNextPossible = false;

                                //setup pres content
                                ContentViewBox.Child = new Tutorial1.LastSlide();
                                sendDataEvent.Set();
                            }
                                break;
                        }
                    }
                        break;

                    //Tutorial 2 without pres
                    case 2:
                    {
                        _currentTutorialLastSlideNumber = TutorialConfiguration.TUTORIAL2STATESWITHOUTPRES;
                        OnPropertyChanged("SlideCounter");

                        switch (StepCounter)
                        {
                            case 0:
                            {
                                //setup pres content
                                ContentViewBox.Child = new Overview();
                            }
                                break;

                            //attack on round 3
                            case 1:
                            {
                                //setup possible button actions
                                IsSkipPossible = false;
                                IsPreviousPossible = false;
                                IsNextPossible = true;

                                var view = new Tutorial2.AttackKeyRound3();
                                //Prepare view
                                if (SBoxesAlreadyAttacked[3])
                                {
                                    view.SBox4Round3.AlreadyAttacked = true;
                                    view.SBox4Round3.IsClickable = false;
                                }
                                else
                                {
                                    view.SBox4Round3.IsClickable = true;
                                }

                                if (SBoxesAlreadyAttacked[2])
                                {
                                    view.SBox3Round3.AlreadyAttacked = true;
                                    view.SBox3Round3.IsClickable = false;
                                }
                                else
                                {
                                    view.SBox3Round3.IsClickable = true;
                                }

                                if (SBoxesAlreadyAttacked[1])
                                {
                                    view.SBox2Round3.AlreadyAttacked = true;
                                    view.SBox2Round3.IsClickable = false;
                                }
                                else
                                {
                                    view.SBox2Round3.IsClickable = true;
                                }

                                if (SBoxesAlreadyAttacked[0])
                                {
                                    view.SBox1Round3.AlreadyAttacked = true;
                                    view.SBox1Round3.IsClickable = false;
                                }
                                else
                                {
                                    view.SBox1Round3.IsClickable = true;
                                }

                                //setup pres content
                                SlideCounterVisibility = Visibility.Visible;
                                ContentViewBox.Child = view;
                                view.SelectionChanged += SBoxSelectionChanged;
                            }
                                break;
                            case 2:
                            {
                                //setup possible button actions
                                IsPreviousPossible = false;
                                IsNextPossible = false;

                                //setup pres content
                                ContentViewBox.Child = new Tutorial2.AttackSearchResult();
                                workDataEvent.Set();
                            }
                                break;
                            //show waiting slide for next components
                            case 3:
                            {
                                //setup possible button actions
                                IsPreviousPossible = false;
                                IsNextPossible = false;

                                //setup pres content
                                ContentViewBox.Child = new Tutorial2.WaitingSlide();

                                SlideCounterVisibility = Visibility.Hidden;
                                sendDataEvent.Set();
                                _dispatcher.Stop();
                            }
                                break;

                            //attack on round 2
                            case 4:
                            {
                                //setup possible button actions
                                IsPreviousPossible = false;
                                IsNextPossible = true;

                                //setup pres content
                                var view = new Tutorial2.AttackKeyRound2();
                                //Prepare view
                                if (SBoxesAlreadyAttacked[3])
                                {
                                    view.SBox4Round2.AlreadyAttacked = true;
                                    view.SBox4Round2.IsClickable = false;
                                }

                                if (SBoxesAlreadyAttacked[2])
                                {
                                    view.SBox3Round2.AlreadyAttacked = true;
                                    view.SBox3Round2.IsClickable = false;
                                }

                                if (SBoxesAlreadyAttacked[1])
                                {
                                    view.SBox2Round2.AlreadyAttacked = true;
                                    view.SBox2Round2.IsClickable = false;
                                }

                                if (SBoxesAlreadyAttacked[0])
                                {
                                    view.SBox1Round2.AlreadyAttacked = true;
                                    view.SBox1Round2.IsClickable = false;
                                }

                                //setup pres content
                                SlideCounterVisibility = Visibility.Visible;
                                ContentViewBox.Child = view;
                                view.SelectionChanged += SBoxSelectionChanged;
                            }
                                break;
                            case 5:
                            {
                                //setup possible button actions
                                IsPreviousPossible = false;
                                IsNextPossible = false;

                                //setup pres content
                                var view = new Tutorial2.AttackSearchResult();
                                view.DataGridCharacteristicsOutputDiffR2Col.Visibility = Visibility.Hidden;
                                view.DataGridCharacteristicsInputDiffR3Col.Visibility = Visibility.Hidden;
                                ContentViewBox.Child = view;
                                workDataEvent.Set();
                            }
                                break;
                            //show waiting slide for next components
                            case 6:
                            {
                                //setup possible button actions
                                IsPreviousPossible = false;
                                IsNextPossible = false;

                                //setup pres content
                                SlideCounterVisibility = Visibility.Hidden;
                                ContentViewBox.Child = new Tutorial2.WaitingSlide();
                                sendDataEvent.Set();
                                _dispatcher.Stop();
                            }
                                break;

                            //attack on last round
                            case 7:
                            {
                                //setup possible button actions
                                IsPreviousPossible = false;
                                IsNextPossible = true;

                                //setup pres content
                                SlideCounterVisibility = Visibility.Visible;
                                ContentViewBox.Child = new Tutorial2.AttackFirstRound();
                            }
                                break;
                            case 8:
                            {
                                //setup possible button actions
                                IsPreviousPossible = false;
                                IsNextPossible = false;

                                //setup pres content
                                SlideCounterVisibility = Visibility.Hidden;
                                ContentViewBox.Child = new Tutorial2.AttackFinished();
                                sendDataEvent.Set();
                                _dispatcher.Stop();
                            }
                                break;
                        }
                    }
                        break;

                    //Tutorial 3 without pres
                    case 3:
                    {
                        _currentTutorialLastSlideNumber = TutorialConfiguration.TUTORIAL3STATESWITHOUTPRES;
                        OnPropertyChanged("SlideCounter");

                        switch (StepCounter)
                        {
                            case 0:
                            {
                                //setup pres content
                                ContentViewBox.Child = new Overview();
                            }
                                break;

                            //attack on round 5
                            case 1:
                            {
                                //setup possible button actions
                                IsSkipPossible = false;
                                IsPreviousPossible = false;
                                IsNextPossible = true;

                                var view = new Tutorial3.AttackKeyRound5();

                                //Prepare view
                                if (SBoxesAlreadyAttacked[3])
                                {
                                    view.SBox4Round5.AlreadyAttacked = true;
                                    view.SBox4Round5.IsClickable = false;
                                }
                                else
                                {
                                    view.SBox4Round5.IsClickable = true;
                                }

                                if (SBoxesAlreadyAttacked[2])
                                {
                                    view.SBox3Round5.AlreadyAttacked = true;
                                    view.SBox3Round5.IsClickable = false;
                                }
                                else
                                {
                                    view.SBox3Round5.IsClickable = true;
                                }

                                if (SBoxesAlreadyAttacked[1])
                                {
                                    view.SBox2Round5.AlreadyAttacked = true;
                                    view.SBox2Round5.IsClickable = false;
                                }
                                else
                                {
                                    view.SBox2Round5.IsClickable = true;
                                }

                                if (SBoxesAlreadyAttacked[0])
                                {
                                    view.SBox1Round5.AlreadyAttacked = true;
                                    view.SBox1Round5.IsClickable = false;
                                }
                                else
                                {
                                    view.SBox1Round5.IsClickable = true;
                                }


                                //setup pres content
                                SlideCounterVisibility = Visibility.Visible;
                                ContentViewBox.Child = view;
                                view.SelectionChanged += SBoxSelectionChanged;
                            }
                                break;
                            case 2:
                            {
                                //setup possible button actions
                                IsPreviousPossible = false;
                                IsNextPossible = false;

                                //setup pres content
                                ContentViewBox.Child = new Tutorial3.AttackSearchResult();
                                workDataEvent.Set();
                            }
                                break;
                            case 3:
                            {
                                //setup possible button actions
                                IsPreviousPossible = false;
                                IsSkipPossible = false;
                                IsNextPossible = false;

                                //setup pres content
                                ContentViewBox.Child = new Tutorial3.WaitingSlide();

                                SlideCounterVisibility = Visibility.Hidden;
                                sendDataEvent.Set();
                                _dispatcher.Stop();
                            }
                                break;
                            case 4:
                            {
                                //setup possible button actions
                                IsPreviousPossible = false;
                                IsSkipPossible = false;
                                IsNextPossible = true;

                                var view = new Tutorial3.AttackKeyRound4();

                                //Prepare view
                                if (SBoxesAlreadyAttacked[3])
                                {
                                    view.SBox4Round4.AlreadyAttacked = true;
                                    view.SBox4Round4.IsClickable = false;
                                }
                                else
                                {
                                    view.SBox4Round4.IsClickable = true;
                                }

                                if (SBoxesAlreadyAttacked[2])
                                {
                                    view.SBox3Round4.AlreadyAttacked = true;
                                    view.SBox3Round4.IsClickable = false;
                                }
                                else
                                {
                                    view.SBox3Round4.IsClickable = true;
                                }

                                if (SBoxesAlreadyAttacked[1])
                                {
                                    view.SBox2Round4.AlreadyAttacked = true;
                                    view.SBox2Round4.IsClickable = false;
                                }
                                else
                                {
                                    view.SBox2Round4.IsClickable = true;
                                }

                                if (SBoxesAlreadyAttacked[0])
                                {
                                    view.SBox1Round4.AlreadyAttacked = true;
                                    view.SBox1Round4.IsClickable = false;
                                }
                                else
                                {
                                    view.SBox1Round4.IsClickable = true;
                                }

                                //setup pres content
                                SlideCounterVisibility = Visibility.Visible;
                                ContentViewBox.Child = view;
                                view.SelectionChanged += SBoxSelectionChanged;
                            }
                                break;
                            case 5:
                            {
                                //setup possible button actions
                                IsPreviousPossible = false;
                                IsNextPossible = false;

                                var view = new Tutorial3.AttackSearchResult();
                                view.DataGridCharacteristicsInputDiffR5Col.Visibility = Visibility.Hidden;
                                view.DataGridCharacteristicsOutputDiffR4Col.Visibility = Visibility.Hidden;

                                //setup pres content
                                ContentViewBox.Child = view;
                                workDataEvent.Set();
                            }
                                break;
                            case 6:
                            {
                                //setup possible button actions
                                IsPreviousPossible = false;
                                IsSkipPossible = false;
                                IsNextPossible = false;

                                //setup pres content
                                ContentViewBox.Child = new Tutorial3.WaitingSlide();

                                SlideCounterVisibility = Visibility.Hidden;
                                sendDataEvent.Set();
                                _dispatcher.Stop();
                            }
                                break;
                            case 7:
                            {
                                //setup possible button actions
                                IsPreviousPossible = false;
                                IsSkipPossible = false;
                                IsNextPossible = true;

                                var view = new Tutorial3.AttackKeyRound3();

                                //Prepare view
                                if (SBoxesAlreadyAttacked[3])
                                {
                                    view.SBox4Round3.AlreadyAttacked = true;
                                    view.SBox4Round3.IsClickable = false;
                                }
                                else
                                {
                                    view.SBox4Round3.IsClickable = true;
                                }

                                if (SBoxesAlreadyAttacked[2])
                                {
                                    view.SBox3Round3.AlreadyAttacked = true;
                                    view.SBox3Round3.IsClickable = false;
                                }
                                else
                                {
                                    view.SBox3Round3.IsClickable = true;
                                }

                                if (SBoxesAlreadyAttacked[1])
                                {
                                    view.SBox2Round3.AlreadyAttacked = true;
                                    view.SBox2Round3.IsClickable = false;
                                }
                                else
                                {
                                    view.SBox2Round3.IsClickable = true;
                                }

                                if (SBoxesAlreadyAttacked[0])
                                {
                                    view.SBox1Round3.AlreadyAttacked = true;
                                    view.SBox1Round3.IsClickable = false;
                                }
                                else
                                {
                                    view.SBox1Round3.IsClickable = true;
                                }

                                //setup pres content
                                SlideCounterVisibility = Visibility.Visible;
                                ContentViewBox.Child = view;
                                view.SelectionChanged += SBoxSelectionChanged;
                            }
                                break;
                            case 8:
                            {
                                //setup possible button actions
                                IsPreviousPossible = false;
                                IsSkipPossible = false;
                                IsNextPossible = false;

                                var view = new Tutorial3.AttackSearchResult();
                                view.DataGridCharacteristicsInputDiffR5Col.Visibility = Visibility.Hidden;
                                view.DataGridCharacteristicsOutputDiffR4Col.Visibility = Visibility.Hidden;
                                view.DataGridCharacteristicsInputDiffR4Col.Visibility = Visibility.Hidden;
                                view.DataGridCharacteristicsOutputDiffR3Col.Visibility = Visibility.Hidden;

                                //setup pres content
                                ContentViewBox.Child = view;
                                workDataEvent.Set();
                            }
                                break;
                            case 9:
                            {
                                //setup possible button actions
                                IsPreviousPossible = false;
                                IsSkipPossible = false;
                                IsNextPossible = false;

                                //setup pres content
                                ContentViewBox.Child = new Tutorial3.WaitingSlide();

                                SlideCounterVisibility = Visibility.Hidden;
                                sendDataEvent.Set();
                                _dispatcher.Stop();
                            }
                                break;
                            case 10:
                            {
                                //setup possible button actions
                                IsPreviousPossible = false;
                                IsSkipPossible = false;
                                IsNextPossible = true;

                                var view = new Tutorial3.AttackKeyRound2();

                                //Prepare view
                                if (SBoxesAlreadyAttacked[3])
                                {
                                    view.SBox4Round2.AlreadyAttacked = true;
                                    view.SBox4Round2.IsClickable = false;
                                }
                                else
                                {
                                    view.SBox4Round2.IsClickable = true;
                                }

                                if (SBoxesAlreadyAttacked[2])
                                {
                                    view.SBox3Round2.AlreadyAttacked = true;
                                    view.SBox3Round2.IsClickable = false;
                                }
                                else
                                {
                                    view.SBox3Round2.IsClickable = true;
                                }

                                if (SBoxesAlreadyAttacked[1])
                                {
                                    view.SBox2Round2.AlreadyAttacked = true;
                                    view.SBox2Round2.IsClickable = false;
                                }
                                else
                                {
                                    view.SBox2Round2.IsClickable = true;
                                }

                                if (SBoxesAlreadyAttacked[0])
                                {
                                    view.SBox1Round2.AlreadyAttacked = true;
                                    view.SBox1Round2.IsClickable = false;
                                }
                                else
                                {
                                    view.SBox1Round2.IsClickable = true;
                                }

                                //setup pres content
                                SlideCounterVisibility = Visibility.Visible;
                                ContentViewBox.Child = view;
                                view.SelectionChanged += SBoxSelectionChanged;
                            }
                                break;
                            case 11:
                            {
                                //setup possible button actions
                                IsPreviousPossible = false;
                                IsSkipPossible = false;
                                IsNextPossible = false;

                                var view = new Tutorial3.AttackSearchResult();
                                view.DataGridCharacteristicsInputDiffR5Col.Visibility = Visibility.Hidden;
                                view.DataGridCharacteristicsOutputDiffR4Col.Visibility = Visibility.Hidden;
                                view.DataGridCharacteristicsInputDiffR4Col.Visibility = Visibility.Hidden;
                                view.DataGridCharacteristicsOutputDiffR3Col.Visibility = Visibility.Hidden;
                                view.DataGridCharacteristicsInputDiffR3Col.Visibility = Visibility.Hidden;
                                view.DataGridCharacteristicsOutputDiffR2Col.Visibility = Visibility.Hidden;

                                //setup pres content
                                ContentViewBox.Child = view;
                                workDataEvent.Set();
                            }
                                break;
                            case 12:
                            {
                                //setup possible button actions
                                IsPreviousPossible = false;
                                IsSkipPossible = false;
                                IsNextPossible = false;

                                //setup pres content
                                ContentViewBox.Child = new Tutorial3.WaitingSlide();

                                SlideCounterVisibility = Visibility.Hidden;
                                sendDataEvent.Set();
                                _dispatcher.Stop();
                            }
                                break;
                            //attack on last round
                            case 13:
                            {
                                //setup possible button actions
                                IsPreviousPossible = false;
                                IsSkipPossible = false;
                                IsNextPossible = true;

                                //setup pres content
                                SlideCounterVisibility = Visibility.Visible;
                                ContentViewBox.Child = new Tutorial3.AttackFirstRound();
                            }
                                break;
                            case 14:
                            {
                                //setup possible button actions
                                IsPreviousPossible = false;
                                IsSkipPossible = false;
                                IsNextPossible = false;

                                //setup pres content
                                SlideCounterVisibility = Visibility.Hidden;
                                ContentViewBox.Child = new Tutorial3.AttackFinished();
                                sendDataEvent.Set();
                                _dispatcher.Stop();
                            }
                                break;
                        }

                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Handler for getting new attack data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SBoxSelectionChanged(object sender, EventArgs e)
        {
            _4BitSBox box = sender as _4BitSBox;
            switch (box.Name)
            {
                case "SBox4Round5":
                {
                    _SBoxesCurrentAttack[3] = !_SBoxesCurrentAttack[3];
                }
                    break;
                case "SBox3Round5":
                {
                    _SBoxesCurrentAttack[2] = !_SBoxesCurrentAttack[2];
                }
                    break;
                case "SBox2Round5":
                {
                    _SBoxesCurrentAttack[1] = !_SBoxesCurrentAttack[1];
                }
                    break;
                case "SBox1Round5":
                {
                    _SBoxesCurrentAttack[0] = !_SBoxesCurrentAttack[0];
                }
                    break;

                case "SBox4Round4":
                {
                    _SBoxesCurrentAttack[3] = !_SBoxesCurrentAttack[3];
                }
                    break;
                case "SBox3Round4":
                {
                    _SBoxesCurrentAttack[2] = !_SBoxesCurrentAttack[2];
                }
                    break;
                case "SBox2Round4":
                {
                    _SBoxesCurrentAttack[1] = !_SBoxesCurrentAttack[1];
                }
                    break;
                case "SBox1Round4":
                {
                    _SBoxesCurrentAttack[0] = !_SBoxesCurrentAttack[0];
                }
                    break;
                case "SBox4Round3":
                {
                    _SBoxesCurrentAttack[3] = !_SBoxesCurrentAttack[3];
                }
                    break;
                case "SBox3Round3":
                {
                    _SBoxesCurrentAttack[2] = !_SBoxesCurrentAttack[2];
                }
                    break;
                case "SBox2Round3":
                {
                    _SBoxesCurrentAttack[1] = !_SBoxesCurrentAttack[1];
                }
                    break;
                case "SBox1Round3":
                {
                    _SBoxesCurrentAttack[0] = !_SBoxesCurrentAttack[0];
                }
                    break;
                case "SBox4Round2":
                {
                    _SBoxesCurrentAttack[3] = !_SBoxesCurrentAttack[3];
                }
                    break;
                case "SBox3Round2":
                {
                    _SBoxesCurrentAttack[2] = !_SBoxesCurrentAttack[2];
                }
                    break;
                case "SBox2Round2":
                {
                    _SBoxesCurrentAttack[1] = !_SBoxesCurrentAttack[1];
                }
                    break;
                case "SBox1Round2":
                {
                    _SBoxesCurrentAttack[0] = !_SBoxesCurrentAttack[0];
                }
                    break;
            }
        }

        /// <summary>
        /// Property for _isSkipPossible
        /// </summary>
        public bool IsSkipPossible
        {
            get { return _isSkipPossible; }
            set
            {
                _isSkipPossible = value;
                OnPropertyChanged();
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
            ProgressEventArgs ev = new ProgressEventArgs();

            switch (TutorialNumber)
            {
                //Tutorial 2
                case 2:
                {
                    //check if next is possible
                    if (!_SBoxesCurrentAttack[0] && !_SBoxesCurrentAttack[1] && !_SBoxesCurrentAttack[2] &&
                        !_SBoxesCurrentAttack[3] && PresentationMode && (StepCounter == 22 || StepCounter == 25))
                    {
                        if (MessageToDisplayOccured != null)
                            MessageToDisplayOccured.Invoke(this, new MessageEventArgs()
                            {
                                message = Properties.Resources.SBoxWarning
                            });
                        return;
                    }
                    else if (!_SBoxesCurrentAttack[0] && !_SBoxesCurrentAttack[1] && !_SBoxesCurrentAttack[2] &&
                             !_SBoxesCurrentAttack[3] && !PresentationMode &&
                             (StepCounter == 1 || StepCounter == 4))
                    {
                        if (MessageToDisplayOccured != null)
                            MessageToDisplayOccured.Invoke(this, new MessageEventArgs()
                            {
                                message = Properties.Resources.SBoxWarning
                            });
                        return;
                    }
                }
                    break;
                //Tutorial 3
                case 3:
                {
                    //check if next is possible
                    if (!_SBoxesCurrentAttack[0] && !_SBoxesCurrentAttack[1] && !_SBoxesCurrentAttack[2] &&
                        !_SBoxesCurrentAttack[3] && PresentationMode && (StepCounter == 22 || StepCounter == 25))
                    {
                        if (MessageToDisplayOccured != null)
                            MessageToDisplayOccured.Invoke(this, new MessageEventArgs()
                            {
                                message = Properties.Resources.SBoxWarning
                            });
                        return;
                    }
                    else if (!_SBoxesCurrentAttack[0] && !_SBoxesCurrentAttack[1] && !_SBoxesCurrentAttack[2] &&
                             !_SBoxesCurrentAttack[3] && !PresentationMode &&
                             (StepCounter == 1 || StepCounter == 4 || StepCounter == 7 || StepCounter == 10))
                    {
                        if (MessageToDisplayOccured != null)
                            MessageToDisplayOccured.Invoke(this, new MessageEventArgs()
                            {
                                message = Properties.Resources.SBoxWarning
                            });
                        return;
                    }
                }
                    break;
            }

            //increment to go to the next step
            StepCounter++;

            if (UIProgressRefresh)
            {
                if (PresentationMode && !AutomaticMode)
                {
                    ev.Increment = _progressIncrement * StepCounter;
                    ProgressChangedOccured.Invoke(this, ev);
                }
                else if (!PresentationMode && !AutomaticMode)
                {
                }
            }

            SetupView();
            buttonNextClickedEvent.Set();
        }

        /// <summary>
        /// Handles a previous step
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPreviousClicked(object sender, RoutedEventArgs e)
        {
            ProgressEventArgs ev = new ProgressEventArgs();

            //decrement to go to the previous step
            StepCounter--;

            if (UIProgressRefresh)
            {
                if (PresentationMode && !AutomaticMode)
                {
                    ev.Increment = _progressIncrement * StepCounter;
                    ProgressChangedOccured.Invoke(this, ev);
                }
                else if (!PresentationMode && !AutomaticMode)
                {
                }
            }

            SetupView();
            buttonPrevClickedEvent.Set();
        }

        /// <summary>
        /// Handles a skip chapter operation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSkipChapterClicked(object sender, RoutedEventArgs e)
        {
            ProgressEventArgs ev = new ProgressEventArgs();

            switch (TutorialNumber)
            {
                case 1:
                {
                    if (StepCounter < TutorialConfiguration.TUTORIAL1INTRODUCTIONHEADER)
                    {
                        StepCounter = TutorialConfiguration.TUTORIAL1INTRODUCTIONHEADER;
                        ev.Increment = _progressIncrement * StepCounter;
                        ProgressChangedOccured.Invoke(this, ev);
                        SetupView();
                    }
                    else if (StepCounter < TutorialConfiguration.TUTORIAL1DIFFERENTIALCRYPTANALYSISHEADER)
                    {
                        StepCounter = TutorialConfiguration.TUTORIAL1DIFFERENTIALCRYPTANALYSISHEADER;
                        ev.Increment = _progressIncrement * StepCounter;
                        ProgressChangedOccured.Invoke(this, ev);
                        SetupView();
                    }
                    else if (StepCounter < TutorialConfiguration.TUTORIAL1PRACTICALDIFFERENTIALCRYPTANALYSISHEADER)
                    {
                        StepCounter = TutorialConfiguration.TUTORIAL1PRACTICALDIFFERENTIALCRYPTANALYSISHEADER;
                        ev.Increment = _progressIncrement * StepCounter;
                        ProgressChangedOccured.Invoke(this, ev);
                        SetupView();
                    }
                }
                    break;
                case 2:
                {
                    if (StepCounter < TutorialConfiguration.TUTORIAL2INTRODUCTIONHEADER)
                    {
                        StepCounter = TutorialConfiguration.TUTORIAL2INTRODUCTIONHEADER;
                        ev.Increment = _progressIncrement * StepCounter;
                        ProgressChangedOccured.Invoke(this, ev);
                        SetupView();
                    }
                    else if (StepCounter < TutorialConfiguration.TUTORIAL2ANALYSISHEADER)
                    {
                        StepCounter = TutorialConfiguration.TUTORIAL2ANALYSISHEADER;
                        ev.Increment = _progressIncrement * StepCounter;
                        ProgressChangedOccured.Invoke(this, ev);
                        SetupView();
                    }
                    else if (StepCounter < TutorialConfiguration.TUTORIAL2SBOXHEADER)
                    {
                        StepCounter = TutorialConfiguration.TUTORIAL2SBOXHEADER;
                        ev.Increment = _progressIncrement * StepCounter;
                        ProgressChangedOccured.Invoke(this, ev);
                        SetupView();
                    }
                    else if (StepCounter < TutorialConfiguration.TUTORIAL2CHARACTERISTICHEADER)
                    {
                        StepCounter = TutorialConfiguration.TUTORIAL2CHARACTERISTICHEADER;
                        ev.Increment = _progressIncrement * StepCounter;
                        ProgressChangedOccured.Invoke(this, ev);
                        SetupView();
                    }
                    else if (StepCounter < TutorialConfiguration.TUTORIAL2DIFFERENTIALHEADER)
                    {
                        StepCounter = TutorialConfiguration.TUTORIAL2DIFFERENTIALHEADER;
                        ev.Increment = _progressIncrement * StepCounter;
                        ProgressChangedOccured.Invoke(this, ev);
                        SetupView();
                    }
                    else if (StepCounter < TutorialConfiguration.TUTORIAL2RECOVERKEYHEADER)
                    {
                        StepCounter = TutorialConfiguration.TUTORIAL2RECOVERKEYHEADER;
                        ev.Increment = _progressIncrement * StepCounter;
                        ProgressChangedOccured.Invoke(this, ev);
                        SetupView();
                    }
                    else
                    {
                        StepCounter = TutorialConfiguration.TUTORIAL2ATTACKHEADER;
                        ev.Increment = _progressIncrement * StepCounter;
                        ProgressChangedOccured.Invoke(this, ev);
                        SetupView();
                    }
                }
                    break;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Property for _messageCount
        /// </summary>
        public int MessageCount
        {
            get { return _messageCount; }
            set
            {
                _messageCount = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _searchPolicy
        /// </summary>
        public SearchPolicy SearchPolicy
        {
            get { return _searchPolicy; }
            set
            {
                _searchPolicy = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _dispatcher
        /// </summary>
        public DispatcherTimer HighlightDispatcher
        {
            get { return _dispatcher; }
            set
            {
                _dispatcher = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for attackable SBoxes
        /// </summary>
        public bool[] SBoxesAlreadyAttacked
        {
            get { return _SBoxesAlreadyAttacked; }
            set
            {
                _SBoxesAlreadyAttacked = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for current SBoxes to attack
        /// </summary>
        public bool[] SBoxesCurrentAttack
        {
            get { return _SBoxesCurrentAttack; }
            set
            {
                _SBoxesCurrentAttack = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for last Slide Num
        /// </summary>
        public int CurrentTutorialLastSlideNumber
        {
            get { return _currentTutorialLastSlideNumber; }
            set
            {
                _currentTutorialLastSlideNumber = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for slide counter visibility
        /// </summary>
        public Visibility SlideCounterVisibility
        {
            get { return _slideCounterVisibility; }
            set
            {
                _slideCounterVisibility = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for indicating that the workspace is running
        /// </summary>
        public bool WorkspaceRunning
        {
            get { return _workspaceRunning; }
            set
            {
                _workspaceRunning = value;
                if (_workspaceRunning)
                {
                    IsNextPossible = true;
                    IsPreviousPossible = false;
                    IsSkipPossible = true;
                }
                else
                {
                    IsNextPossible = false;
                    IsPreviousPossible = false;
                    IsSkipPossible = false;
                }

                OnPropertyChanged();

                StepCounter = 0;
                SetupView();
            }
        }

        /// <summary>
        /// Property for automatic mode
        /// </summary>
        public bool AutomaticMode
        {
            get { return _automaticMode; }
            set
            {
                _automaticMode = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for presentation mode
        /// </summary>
        public bool PresentationMode
        {
            get { return _presentationMode; }
            set
            {
                _presentationMode = value;
                OnPropertyChanged();
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
            get { return ((StepCounter + 1) + "/" + _currentTutorialLastSlideNumber); }
        }

        #endregion

        /// <summary>
        /// adds a new search result to the UI
        /// </summary>
        /// <param name="res"></param>
        public void addSearchResultToUI(SearchResult res)
        {
            switch (TutorialNumber)
            {
                //Tutorial 1
                case 1:
                {
                }
                    break;
                //Tutorial 2
                case 2:
                {
                    if (ContentViewBox.Child is Tutorial2.AttackSearchResult)
                    {
                        Tutorial2.AttackSearchResult view = ContentViewBox.Child as Tutorial2.AttackSearchResult;

                        if (res.startTime != DateTime.MinValue)
                        {
                            view.StartTime = res.startTime;
                        }

                        view.EndTime = res.endTime;
                        string activeSBoxes = "";
                        for (int i = res.activeSBoxes.Length - 1; i >= 0; i--)
                        {
                            if (res.activeSBoxes[i])
                            {
                                activeSBoxes += "1";
                            }
                            else
                            {
                                activeSBoxes += "0";
                            }
                        }

                        view.SBoxes = activeSBoxes;
                        view.Round = res.round;

                        if (SearchPolicy == SearchPolicy.FirstAllCharacteristicsDepthSearch)
                        {
                            view.SearchPolicy = Properties.Resources.SearchPolicy3;
                        }
                        else if (SearchPolicy == SearchPolicy.FirstBestCharacteristicDepthSearch)
                        {
                            view.SearchPolicy = Properties.Resources.SearchPolicy2;
                        }
                        else if (SearchPolicy == SearchPolicy.FirstBestCharacteristicHeuristic)
                        {
                            view.SearchPolicy = Properties.Resources.SearchPolicy1;
                        }

                        if (res.result != null)
                        {
                            foreach (CharacteristicUI item in res.result)
                            {
                                Cipher2CharacteristicUI el = item as Cipher2CharacteristicUI;
                                view.Characteristics.Add(el);
                            }


                            view.CharacteristicCount = view.Characteristics.Count;
                        }
                    }
                }
                    break;
                //Tutorial 3
                case 3:
                {
                    if (ContentViewBox.Child is Tutorial3.AttackSearchResult)
                    {
                        Tutorial3.AttackSearchResult view = ContentViewBox.Child as Tutorial3.AttackSearchResult;

                        if (res.startTime != DateTime.MinValue)
                        {
                            view.StartTime = res.startTime;
                        }

                        view.EndTime = res.endTime;
                        string activeSBoxes = "";
                        for (int i = res.activeSBoxes.Length - 1; i >= 0; i--)
                        {
                            if (res.activeSBoxes[i])
                            {
                                activeSBoxes += "1";
                            }
                            else
                            {
                                activeSBoxes += "0";
                            }
                        }

                        view.SBoxes = activeSBoxes;
                        view.Round = res.round;

                        if (SearchPolicy == SearchPolicy.FirstAllCharacteristicsDepthSearch)
                        {
                            view.SearchPolicy = Properties.Resources.SearchPolicy3;
                        }
                        else if (SearchPolicy == SearchPolicy.FirstBestCharacteristicDepthSearch)
                        {
                            view.SearchPolicy = Properties.Resources.SearchPolicy2;
                        }
                        else if (SearchPolicy == SearchPolicy.FirstBestCharacteristicHeuristic)
                        {
                            view.SearchPolicy = Properties.Resources.SearchPolicy1;
                        }

                        if (res.result != null)
                        {
                            foreach (CharacteristicUI item in res.result)
                            {
                                Cipher3CharacteristicUI el = item as Cipher3CharacteristicUI;
                                view.Characteristics.Add(el);
                            }

                            view.CharacteristicCount = view.Characteristics.Count;
                        }
                    }
                }
                    break;
            }
        }

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
            if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}