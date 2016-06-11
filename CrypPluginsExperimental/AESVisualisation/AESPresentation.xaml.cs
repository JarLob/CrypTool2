using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AESVisualisation
{
    /// <summary>
    /// Interaction logic for AESPresentation.xaml
    /// </summary>
    public partial class AESPresentation : UserControl
    {
        public TextBlock tempBlock = new TextBlock();
        public int roundNumber = 1;
        public int action = 1;
        public bool autostep;
        public int autostepSpeed;
        public AutoResetEvent buttonNextClickedEvent;
        public byte[][] sBox = new byte[16][];
        public byte[][] states = new byte[40][];
        public byte[][] keyList = new byte[11][];
        public byte[] tempState;
        static Random rnd = new Random();
        public List<List<TextBlock>> textBlockList = new List<List<TextBlock>>();
        public List<List<Border>> borderList = new List<List<Border>>();
        public List<List<TextBlock>> sBoxList = new List<List<TextBlock>>();
        public byte[] key;
        bool first = true;
        bool expansion = true;

        public AESPresentation()
        {
            InitializeComponent();
            buttonNextClickedEvent = new AutoResetEvent(false);
            autostep = false;
            autostepSpeedSlider.IsEnabled = true;
            keyButton.Content = "Skip Expansion";
            buttonVisible();
            hideButton();
            for (int x = 0; x < 13; x++)
            {
                List<TextBlock> temp = new List<TextBlock>();
                temp = createTextBlockList(x);
                textBlockList.Add(temp);
            }
            for (int x = 0; x < 12; x++)
            {
                List<Border> temp = new List<Border>();
                temp = createBorderList(x);
                borderList.Add(temp);
            }
            disableButtons();
            sBoxList = createSBoxList();
        }

        #region Buttons
        private void round1Button_Click(object sender, RoutedEventArgs e)
        {
            if (expansion)
            {                
                roundNumber = 1;
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    setUpExpansion();
                }, null);
                return;
            }       
            removeColors();
            lightRemoveColor();
            round1Button.Background = Brushes.Aqua;
            subByteButton.Background = Brushes.Aqua;
            buttonVisible();
            roundNumber = 1;
            action = 1;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                setUpSubByte(states);
            }, null);          
            mixColButton.IsEnabled = true;
        }

        private void round2Button_Click(object sender, RoutedEventArgs e)
        {
            if (expansion)
            {               
                roundNumber = 2;
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    setUpExpansion();
                }, null);
                return;
            }
            removeColors();
            lightRemoveColor();
            round2Button.Background = Brushes.Aqua;
            buttonVisible();
            roundNumber = 2;
            action = 1;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                setUpSubByte(states);
            }, null);
            mixColButton.IsEnabled = true;
        }

        private void round3Button_Click(object sender, RoutedEventArgs e)
        {
            if (expansion)
            {             
                roundNumber = 3;
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    setUpExpansion();
                }, null);
                return;
            }
            removeColors();
            lightRemoveColor();
            round3Button.Background = Brushes.Aqua;
            buttonVisible();
            roundNumber = 3;
            action = 1;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                setUpSubByte(states);
            }, null);
            mixColButton.IsEnabled = true;
        }

        private void round4Button_Click(object sender, RoutedEventArgs e)
        {
            if (expansion)
            {              
                roundNumber = 4;
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    setUpExpansion();
                }, null);
                return;
            }
            removeColors();
            lightRemoveColor();
            round4Button.Background = Brushes.Aqua;
            buttonVisible();
            roundNumber = 4;
            action = 1;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                setUpSubByte(states);
            }, null);
            mixColButton.IsEnabled = true;
        }

        private void round5Button_Click(object sender, RoutedEventArgs e)
        {
            if (expansion)
            {              
                roundNumber = 5;
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    setUpExpansion();
                }, null);
                return;
            }
            removeColors();
            lightRemoveColor();
            round5Button.Background = Brushes.Aqua;
            buttonVisible();
            roundNumber = 5;
            action = 1;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                setUpSubByte(states);
            }, null);
            mixColButton.IsEnabled = true;
        }

        private void round6Button_Click(object sender, RoutedEventArgs e)
        {
            if (expansion)
            {              
                roundNumber = 6;
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    setUpExpansion();
                }, null);
                return;
            }
            removeColors();
            lightRemoveColor();
            round6Button.Background = Brushes.Aqua;
            buttonVisible();
            roundNumber = 6;
            action = 1;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                setUpSubByte(states);
            }, null);
            mixColButton.IsEnabled = true;
        }

        private void round7Button_Click(object sender, RoutedEventArgs e)
        {
            if (expansion)
            {             
                roundNumber = 7;
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    setUpExpansion();
                }, null);
                return;
            }
            removeColors();
            lightRemoveColor();
            round7Button.Background = Brushes.Aqua;
            buttonVisible();
            roundNumber = 7;
            action = 1;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                setUpSubByte(states);
            }, null);
            mixColButton.IsEnabled = true;
        }

        private void round8Button_Click(object sender, RoutedEventArgs e)
        {
            if (expansion)
            {               
                roundNumber = 8;
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    setUpExpansion();
                }, null);
                return;
            }
            removeColors();
            lightRemoveColor();
            round8Button.Background = Brushes.Aqua;
            buttonVisible();
            roundNumber = 8;
            action = 1;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                setUpSubByte(states);
            }, null);
            mixColButton.IsEnabled = true;
        }

        private void round9Button_Click(object sender, RoutedEventArgs e)
        {
            if (expansion)
            {               
                roundNumber = 9;
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    setUpExpansion();
                }, null);
                return;
            }
            removeColors();
            lightRemoveColor();
            round9Button.Background = Brushes.Aqua;
            buttonVisible();
            roundNumber = 9;
            action = 1;
            setUpSubByte(states);
            mixColButton.IsEnabled = true;
        }

        private void round10Button_Click(object sender, RoutedEventArgs e)
        {
            if (expansion)
            {              
                roundNumber = 10;
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    setUpExpansion();
                }, null);
                return;
            }
            removeColors();
            lightRemoveColor();
            round10Button.Background = Brushes.Aqua;
            buttonVisible();
            roundNumber = 10;
            action = 1;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                setUpSubByte(states);
            }, null);
            mixColButton.IsEnabled = false;
        }

        private void keyButton_Click(object sender, RoutedEventArgs e)
        {
            expansion = !expansion;
            buttonNextClickedEvent.Set();
        }

        private void subByteButton_Click(object sender, RoutedEventArgs e)
        {
            if(roundNumber == 0)
            {
                return;
            }
            lightRemoveColor();
            action = 1;
            setUpSubByte(states);
        }

        private void shiftRowButton_Click(object sender, RoutedEventArgs e)
        {
            if (roundNumber == 0)
            {
                return;
            }
            lightRemoveColor();
            action = 2;
            setUpShiftRows();
        }

        private void mixColButton_Click(object sender, RoutedEventArgs e)
        {
            if (roundNumber == 0)
            {
                return;
            }
            lightRemoveColor();
            action = 3;
            setUpMixColumns();
        }

        private void addKeyButton_Click(object sender, RoutedEventArgs e)
        {
            if (roundNumber == 0)
            {
                return;
            }
            lightRemoveColor();
            action = 4;
            setUpAddKey();
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            disableButtons();
            buttonNextClickedEvent.Set();           
        }

        private void prevStepButton_Click(object sender, RoutedEventArgs e)
        {
            if (expansion)
            {
                if(roundNumber > 0)
                {
                    roundNumber--;
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        setUpExpansion();
                    }, null);
                    return;
                }
                return;
            }
            if (action == 4 && roundNumber == 0)
            {
                return;
            }
            switch (action)
            {
                case 1:                    
                    roundNumber--;
                    action = 4;
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        setUpAddKey();
                        changeRoundButton();
                    }, null);
                    if(roundNumber == 9)
                    {
                        mixColButton.IsEnabled = true;
                    }
                    break;
                case 2:
                    action = 1;
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        removeColors();
                        changeRoundButton();
                        setUpSubByte(states);
                    }, null);
                    break;
                case 3:
                    action = 2;
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        setUpShiftRows();
                    }, null);
                    break;
                case 4:
                    if (roundNumber == 10)
                    {
                        action = 2;
                        Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            setUpShiftRows();
                        }, null);
                    }
                    else
                    {
                        action = 3;
                        Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            setUpMixColumns();
                        }, null);
                    }
                    break;
                default:
                    break;
            }
        }

        private void nextStepButton_Click(object sender, RoutedEventArgs e)
        {
            if (expansion)
            {
                if (roundNumber < 10)
                {
                    roundNumber++;
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        setUpExpansion();
                    }, null);
                    return;
                }
                expansion = !expansion;
                buttonNextClickedEvent.Set();
                return;
            }
            if (action == 4 && roundNumber == 10)
            {
                return;
            }
            switch (action)
            {
                case 1:
                    action = 2;
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        setUpShiftRows();                        
                    }, null);                                        
                    break;
                case 2:
                    if (roundNumber == 10)
                    {
                        action = 4;
                        Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            setUpAddKey();                        
                        }, null);
                    }
                    else
                    {
                        action = 3;
                        Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            setUpMixColumns();                            
                        }, null);
                    }                 
                    break;
                case 3:
                    action = 4;
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        setUpAddKey();
                    }, null);
                    break;
                case 4:
                    action = 1;
                    roundNumber++;
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        removeColors();
                        changeRoundButton();
                        setUpSubByte(states);
                    }, null);
                    if(roundNumber == 10)
                    {
                        mixColButton.IsEnabled = false;
                    }
                    break;
                default:
                    break;
            }
        }

        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {
            disableButtons();
            autostep = !autostep;
            if(autostep)
            {
                buttonNextClickedEvent.Set();
            }               
        }

        private void autostepSpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            autostepSpeed = 50 + 100 * (10 - (int)autostepSpeedSlider.Value);
        }
        #endregion Buttons

        #region Methods
        public void actionMethod()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                enableButtons();
            }, null);
            wait();
            if (expansion)
            {
                return;
            }
            while (roundNumber < 11)
            {
                while (action < 5)
                {
                    switch (action)
                    {
                        case 1:                            
                            subBytes();
                            autostep = false;                            
                            wait();
                            action = 2;
                            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {
                                setUpShiftRows();
                                enableButtons();
                            }, null);                         
                            autostep = false;
                            wait();
                            if (expansion)
                            {
                                return;
                            }
                            break;
                        case 2:                           
                            shiftRow();
                            autostep = false;                            
                            wait();
                            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {
                                shiftRowGrid.Visibility = Visibility.Hidden;
                                lightRemoveColor();
                                resetShiftRow();
                            }, null);
                            if(roundNumber < 10)
                            {
                                action = 3;
                                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                {
                                    setUpMixColumns();
                                    enableButtons();
                                }, null);
                            }
                            else
                            {
                                action = 4;
                                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                {
                                    setUpAddKey();
                                    enableButtons();
                                }, null);
                            }
                            autostep = false;
                            wait();
                            if (expansion)
                            {
                                return;
                            }
                            break;
                        case 3:                        
                            mixColumns();
                            action = 4;
                            autostep = false;
                            wait();
                            List<TextBlock> resultBoxes = textBlockList[8];
                            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {
                                foreach (TextBlock tb in resultBoxes)
                                {
                                    tb.Text = "";
                                }
                                setUpAddKey();
                                enableButtons();
                            }, null);                           
                            autostep = false;
                            wait();
                            if (expansion)
                            {
                                return;
                            }
                            break;
                        case 4:
                            addKey();
                            autostep = false;                                                  
                            wait();
                            if(roundNumber < 10)
                            {
                                action = 1;
                                roundNumber++;  
                                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                {
                                    removeColors();
                                    changeRoundButton();
                                    setUpSubByte(states);
                                    enableButtons();
                                }, null);                             
                                autostep = false;
                                wait();
                                if (expansion)
                                {
                                    return;
                                }
                            }  
                            else
                            {
                                action = 5;
                            }    
                            break;
                        default:
                            action = 5;
                            break;
                    }
                }
                roundNumber++;
            }
        }       

        private void removeColors()
        {
            keyButton.ClearValue(BackgroundProperty);
            round1Button.ClearValue(BackgroundProperty);
            round2Button.ClearValue(BackgroundProperty);
            round3Button.ClearValue(BackgroundProperty);
            round4Button.ClearValue(BackgroundProperty);
            round5Button.ClearValue(BackgroundProperty);
            round6Button.ClearValue(BackgroundProperty);
            round7Button.ClearValue(BackgroundProperty);
            round8Button.ClearValue(BackgroundProperty);
            round9Button.ClearValue(BackgroundProperty);
            round10Button.ClearValue(BackgroundProperty);
            //subByteButton.ClearValue(BackgroundProperty);
            //shiftRowButton.ClearValue(BackgroundProperty);
            //mixColButton.ClearValue(BackgroundProperty);
            //addKeyButton.ClearValue(BackgroundProperty);
        }

        private void lightRemoveColor()
        {
            subByteButton.ClearValue(BackgroundProperty);
            shiftRowButton.ClearValue(BackgroundProperty);
            mixColButton.ClearValue(BackgroundProperty);
            addKeyButton.ClearValue(BackgroundProperty);
        }

        private void changeRoundButton()
        {
            switch (roundNumber)
            {
                case 1:
                    removeColors();
                    round1Button.Background = Brushes.Aqua;
                    break;
                case 2:
                    removeColors();
                    round2Button.Background = Brushes.Aqua;
                    break;
                case 3:
                    removeColors();
                    round3Button.Background = Brushes.Aqua;
                    break;
                case 4:
                    removeColors();
                    round4Button.Background = Brushes.Aqua;
                    break;
                case 5:
                    removeColors();
                    round5Button.Background = Brushes.Aqua;
                    break;
                case 6:
                    removeColors();
                    round6Button.Background = Brushes.Aqua;
                    break;
                case 7:
                    removeColors();
                    round7Button.Background = Brushes.Aqua;
                    break;
                case 8:
                    removeColors();
                    round8Button.Background = Brushes.Aqua;
                    break;
                case 9:
                    removeColors();
                    round9Button.Background = Brushes.Aqua;
                    break;
                case 10:
                    removeColors();
                    round10Button.Background = Brushes.Aqua;
                    break;
                case 0:
                    removeColors();
                    break;
                default:
                    break;
            }
        }

        public List<TextBlock> createTextBlockList(int textBlockList)
        {
            List<TextBlock> list = new List<TextBlock>();
            int x;
            string temp;
            switch (textBlockList)
            {
                case 0:
                    list.Add(keyTextBlock1);
                    list.Add(keyTextBlock2);
                    list.Add(keyTextBlock3);
                    list.Add(keyTextBlock4);
                    list.Add(keyTextBlock5);
                    list.Add(keyTextBlock6);
                    list.Add(keyTextBlock7);
                    list.Add(keyTextBlock8);
                    list.Add(keyTextBlock9);
                    list.Add(keyTextBlock10);
                    list.Add(keyTextBlock11);
                    list.Add(keyTextBlock12);
                    list.Add(keyTextBlock13);
                    list.Add(keyTextBlock14);
                    list.Add(keyTextBlock15);
                    list.Add(keyTextBlock16);
                    break;
                case 1:
                    list.Add(keyTextBlock17);
                    list.Add(keyTextBlock18);
                    list.Add(keyTextBlock19);
                    list.Add(keyTextBlock20);
                    list.Add(keyTextBlock21);
                    list.Add(keyTextBlock22);
                    list.Add(keyTextBlock23);
                    list.Add(keyTextBlock24);
                    list.Add(keyTextBlock25);
                    list.Add(keyTextBlock26);
                    list.Add(keyTextBlock27);
                    list.Add(keyTextBlock28);
                    list.Add(keyTextBlock29);
                    list.Add(keyTextBlock30);
                    list.Add(keyTextBlock31);
                    list.Add(keyTextBlock32);
                    break;
                case 2:
                    list.Add(keyTextBlock33);
                    list.Add(keyTextBlock34);
                    list.Add(keyTextBlock35);
                    list.Add(keyTextBlock36);
                    list.Add(keyTextBlock37);
                    list.Add(keyTextBlock38);
                    list.Add(keyTextBlock39);
                    list.Add(keyTextBlock40);
                    list.Add(keyTextBlock41);
                    list.Add(keyTextBlock42);
                    list.Add(keyTextBlock43);
                    list.Add(keyTextBlock44);
                    list.Add(keyTextBlock45);
                    list.Add(keyTextBlock46);
                    list.Add(keyTextBlock47);
                    list.Add(keyTextBlock48);
                    break;
                case 3:
                    x = 19;
                    temp = "sTextBlock";
                    while (x < 306)
                    {
                        if (x % 18 != 0 && (x + 1) % 18 != 0)
                        {
                            string y = temp + x;
                            list.Add((TextBlock)FindName(y));
                            x++;
                        }
                        else
                        {
                            x++;
                        }
                    }
                    break;
                case 4:
                    x = 1;
                    temp = "sStateTextBlock";
                    while (x < 17)
                    {
                        string y = temp + x;
                        list.Add((TextBlock)this.FindName(y));
                        x++;
                    }
                    break;
                case 5:
                    x = 1;
                    temp = "sResultTextBlock";
                    while (x < 17)
                    {
                        string y = temp + x;
                        list.Add((TextBlock)this.FindName(y));
                        x++;
                    }
                    break;
                case 6:
                    x = 1;
                    temp = "mStateTextBlock";
                    while (x < 17)
                    {
                        string y = temp + x;
                        list.Add((TextBlock)this.FindName(y));
                        x++;
                    }
                    break;
                case 7:
                    x = 1;
                    temp = "mTransitionTextBlock";
                    while (x < 17)
                    {
                        string y = temp + x;
                        list.Add((TextBlock)this.FindName(y));
                        x++;
                    }
                    break;
                case 8:
                    x = 1;
                    temp = "mResultTextBlock";
                    while (x < 17)
                    {
                        string y = temp + x;
                        list.Add((TextBlock)this.FindName(y));
                        x++;
                    }
                    break;
                case 9:
                    x = 1;
                    temp = "expansionKeyBlock";
                    while (x < 17)
                    {
                        string y = temp + x;
                        list.Add((TextBlock)this.FindName(y));
                        x++;
                    }
                    break;
                case 10:
                    x = 1;
                    temp = "expansionTransitionBlock";
                    while (x < 17)
                    {
                        string y = temp + x;
                        list.Add((TextBlock)this.FindName(y));
                        x++;
                    }
                    break;
                case 11:
                    x = 1;
                    temp = "expansionTransition1Block";
                    while (x < 17)
                    {
                        string y = temp + x;
                        list.Add((TextBlock)this.FindName(y));
                        x++;
                    }
                    break;
                case 12:
                    x = 1;
                    temp = "expansionResultBlock";
                    while (x < 17)
                    {
                        string y = temp + x;
                        list.Add((TextBlock)this.FindName(y));
                        x++;
                    }
                    break;
                default:
                    break;
            }

            return list;
        }

        public List<Border> createBorderList(int y)
        {
            List<Border> temp = new List<Border>();
            int x;
            string border;
            switch (y)
            {
                case 0:
                    temp.Add(keyBorder1);
                    temp.Add(keyBorder2);
                    temp.Add(keyBorder3);
                    temp.Add(keyBorder4);
                    temp.Add(keyBorder5);
                    temp.Add(keyBorder6);
                    temp.Add(keyBorder7);
                    temp.Add(keyBorder8);
                    temp.Add(keyBorder9);
                    temp.Add(keyBorder10);
                    temp.Add(keyBorder11);
                    temp.Add(keyBorder12);
                    temp.Add(keyBorder13);
                    temp.Add(keyBorder14);
                    temp.Add(keyBorder15);
                    temp.Add(keyBorder16);
                    break;
                case 1:
                    temp.Add(keyBorder17);
                    temp.Add(keyBorder18);
                    temp.Add(keyBorder19);
                    temp.Add(keyBorder20);
                    temp.Add(keyBorder21);
                    temp.Add(keyBorder22);
                    temp.Add(keyBorder23);
                    temp.Add(keyBorder24);
                    temp.Add(keyBorder25);
                    temp.Add(keyBorder26);
                    temp.Add(keyBorder27);
                    temp.Add(keyBorder28);
                    temp.Add(keyBorder29);
                    temp.Add(keyBorder30);
                    temp.Add(keyBorder31);
                    temp.Add(keyBorder32);
                    break;
                case 2:
                    temp.Add(keyBorder33);
                    temp.Add(keyBorder34);
                    temp.Add(keyBorder35);
                    temp.Add(keyBorder36);
                    temp.Add(keyBorder37);
                    temp.Add(keyBorder38);
                    temp.Add(keyBorder39);
                    temp.Add(keyBorder40);
                    temp.Add(keyBorder41);
                    temp.Add(keyBorder42);
                    temp.Add(keyBorder43);
                    temp.Add(keyBorder44);
                    temp.Add(keyBorder45);
                    temp.Add(keyBorder46);
                    temp.Add(keyBorder47);
                    temp.Add(keyBorder48);
                    break;
                case 3:
                    temp.Add(border1);
                    temp.Add(border2);
                    temp.Add(border3);
                    temp.Add(border4);
                    temp.Add(border5);
                    temp.Add(border6);
                    temp.Add(border7);
                    temp.Add(border8);
                    temp.Add(border9);
                    temp.Add(border10);
                    temp.Add(border11);
                    temp.Add(border12);
                    temp.Add(border13);
                    temp.Add(border14);
                    temp.Add(border15);
                    temp.Add(border16);
                    temp.Add(border17);
                    temp.Add(border18);
                    temp.Add(border19);
                    temp.Add(border20);
                    temp.Add(border21);
                    temp.Add(border22);
                    temp.Add(border23);
                    temp.Add(border24);
                    temp.Add(border25);
                    temp.Add(border26);
                    temp.Add(border27);
                    temp.Add(border28);
                    temp.Add(border29);
                    temp.Add(border30);
                    temp.Add(border31);
                    temp.Add(border32);
                    temp.Add(border33);
                    temp.Add(border34);
                    break;
                case 4:
                    x = 1;
                    border = "mStateBorder";
                    while (x < 17)
                    {
                        string z = border + x;
                        temp.Add((Border)this.FindName(z));
                        x++;
                    }
                    break;
                case 5:
                    x = 1;
                    border = "mMatrixBorder";
                    while (x < 17)
                    {
                        string z = border + x;
                        temp.Add((Border)this.FindName(z));
                        x++;
                    }
                    break;
                case 6:
                    x = 1;
                    border = "mTransitionBorder";
                    while (x < 17)
                    {
                        string z = border + x;
                        temp.Add((Border)this.FindName(z));
                        x++;
                    }
                    break;
                case 7:
                    x = 1;
                    border = "mResultBorder";
                    while (x < 17)
                    {
                        string z = border + x;
                        temp.Add((Border)this.FindName(z));
                        x++;
                    }
                    break;
                case 8:
                    x = 1;
                    border = "expansionKeyBorder";
                    while (x < 17)
                    {
                        string z = border + x;
                        temp.Add((Border)this.FindName(z));
                        x++;
                    }
                    break;
                case 9:
                    x = 1;
                    border = "expansionTransitionBorder";
                    while (x < 17)
                    {
                        string z = border + x;
                        temp.Add((Border)this.FindName(z));
                        x++;
                    }
                    break;
                case 10:
                    x = 1;
                    border = "expansionTransition1Border";
                    while (x < 17)
                    {
                        string z = border + x;
                        temp.Add((Border)this.FindName(z));
                        x++;
                    }
                    break;
                case 11:
                    x = 1;
                    border = "expansionResultBorder";
                    while (x < 17)
                    {
                        string z = border + x;
                        temp.Add((Border)this.FindName(z));
                        x++;
                    }
                    break;
                default:
                    break;
            }
            return temp;
        }      

        private void setMixStateTransition(byte[] temp)
        {
            List<TextBlock> blockList = createTextBlockList(6);
            int x = 0;
            foreach (TextBlock tb in blockList)
            {
                tb.Text = temp[x].ToString("X2");
                x++;
            }
            blockList.Clear();
            blockList = createTextBlockList(7);
            x = 0;
            foreach (TextBlock tb in blockList)
            {
                tb.Text = temp[x].ToString("X2");
                x++;
            }
        }

        private void invisible()
        {
            subByteResultGrid.Visibility = Visibility.Hidden;
            subByteStateGrid.Visibility = Visibility.Hidden;
            subByteTransitionGrid.Visibility = Visibility.Hidden;
            sBoxGrid.Visibility = Visibility.Hidden;
            addKeyStateGrid.Visibility = Visibility.Hidden;
            addKeyKeyGrid.Visibility = Visibility.Hidden;
            addKeyResultGrid.Visibility = Visibility.Hidden;
            shiftRowGrid.Visibility = Visibility.Hidden;
            mixColMatrixGrid.Visibility = Visibility.Hidden;
            mixColStateGrid.Visibility = Visibility.Hidden;
            mixColResultGrid.Visibility = Visibility.Hidden;
            mixColTransitionGrid.Visibility = Visibility.Hidden;
        }

        private void updateUI()
        {

            DispatcherFrame frame = new DispatcherFrame();

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(delegate (object parameter)

            {

                frame.Continue = false;

                return null;

            }), null);

            Dispatcher.PushFrame(frame);

        }

        public byte[][] setSBox()
        {

            int x = 0;
            while (x < 16)
            {
                this.sBox[x] = new byte[16];
                x++;
            }
            x = 0;
            List<int> temp = new List<int>();
            while (x < 256)
            {
                temp.Add(x);
                x++;
            }
            int y = 0;
            x = 0;
            int z;
            while (y < 16)
            {
                while (x < 16)
                {
                    z = rnd.Next(temp.Count);
                    sBox[y][x] = Convert.ToByte(temp[z]);
                    temp.RemoveAt(z);
                    x++;
                }
                y++;
                x = 0;
            }
            x = 0;
            y = 0;
            List<TextBlock> blockList = null;
            this.Dispatcher.Invoke((Action)(() =>
            {
                blockList = createTextBlockList(3);
            }));
            foreach (TextBlock tb in blockList)
            {

                tb.Text = sBox[y][x].ToString("X2");
                x++;
                if (x > 15)
                {
                    x = 0;
                    y++;
                }
                if (y > 15)
                {
                    break;
                }
            }
            return sBox;
        }

        public void subBytes()
        {

            List<TextBlock> sState = textBlockList[4];
            List<TextBlock> sResult = textBlockList[5];
            List<Border> tempBordes = new List<Border>();
            int r;
            int x = 0;
            int y = 0;
            int z = 0;
            foreach (TextBlock tb in sState)
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    tb.Background = Brushes.Green;
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    sTransitionTextBlock3.Text = tb.Text;
                    sTransitionTextBlock3.Background = Brushes.Green;
                    sTransitionBorder3.Visibility = Visibility.Visible;
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    tb.Background = Brushes.Transparent;
                    sTransitionTextBlock3.Background = Brushes.Transparent;
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    sTransitionBorder3.Visibility = Visibility.Hidden;
                    sTransitionTextBlock1.Text = sTransitionTextBlock3.Text.Substring(0, 1);
                    sTransitionTextBlock2.Text = sTransitionTextBlock3.Text.Substring(1, 1);
                    sTransitionTextBlock3.Text = "";
                    sTransitionBorder1.Visibility = Visibility.Visible;
                    sTransitionBorder2.Visibility = Visibility.Visible;
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                   sTransitionTextBlock2.Background = Brushes.Transparent;
                    sTransitionTextBlock1.Background = Brushes.Green;
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    switch (sTransitionTextBlock1.Text)
                    {
                        case "0":
                            x = 0;
                            sBorder18.Background = Brushes.Green;
                            tempBordes.Add(sBorder18);
                            break;
                        case "1":
                            x = 1;
                            sBorder36.Background = Brushes.Green;
                            tempBordes.Add(sBorder36);
                            break;
                        case "2":
                            x = 2;
                            sBorder54.Background = Brushes.Green;
                            tempBordes.Add(sBorder54);
                            break;
                        case "3":
                            x = 3;
                            sBorder72.Background = Brushes.Green;
                            tempBordes.Add(sBorder72);
                            break;
                        case "4":
                            x = 4;
                            sBorder90.Background = Brushes.Green;
                            tempBordes.Add(sBorder90);
                            break;
                        case "5":
                            x = 5;
                            sBorder108.Background = Brushes.Green;
                            tempBordes.Add(sBorder108);
                            break;
                        case "6":
                            x = 6;
                            sBorder126.Background = Brushes.Green;
                            tempBordes.Add(sBorder126);
                            break;
                        case "7":
                            x = 7;
                            sBorder144.Background = Brushes.Green;
                            tempBordes.Add(sBorder144);
                            break;
                        case "8":
                            x = 8;
                            sBorder162.Background = Brushes.Green;
                            tempBordes.Add(sBorder162);
                            break;
                        case "9":
                            x = 9;
                            sBorder180.Background = Brushes.Green;
                            tempBordes.Add(sBorder180);
                            break;
                        case "A":
                            x = 10;
                            sBorder198.Background = Brushes.Green;
                            tempBordes.Add(sBorder198);
                            break;
                        case "B":
                            x = 11;
                            sBorder216.Background = Brushes.Green;
                            tempBordes.Add(sBorder216);
                            break;
                        case "C":
                            x = 12;
                            sBorder234.Background = Brushes.Green;
                            tempBordes.Add(sBorder234);
                            break;
                        case "D":
                            x = 13;
                            sBorder252.Background = Brushes.Green;
                            tempBordes.Add(sBorder252);
                            break;
                        case "E":
                            x = 14;
                            sBorder270.Background = Brushes.Green;
                            tempBordes.Add(sBorder270);
                            break;
                        case "F":
                            x = 15;
                            sBorder288.Background = Brushes.Green;
                            tempBordes.Add(sBorder288);
                            break;
                        default:
                            break;
                    }
                    sTransitionTextBlock1.Background = Brushes.Transparent;
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    sTransitionTextBlock2.Background = Brushes.Green;
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    switch (sTransitionTextBlock2.Text)
                    {
                        case "0":
                            y = 0;
                            sBorder1.Background = Brushes.Green;
                            tempBordes.Add(sBorder1);
                            break;
                        case "1":
                            y = 1;
                            sBorder2.Background = Brushes.Green;
                            tempBordes.Add(sBorder2);
                            break;
                        case "2":
                            y = 2;
                            sBorder3.Background = Brushes.Green;
                            tempBordes.Add(sBorder3);
                            break;
                        case "3":
                            y = 3;
                            sBorder4.Background = Brushes.Green;
                            tempBordes.Add(sBorder4);
                            break;
                        case "4":
                            y = 4;
                            sBorder5.Background = Brushes.Green;
                            tempBordes.Add(sBorder5);
                            break;
                        case "5":
                            y = 5;
                            sBorder6.Background = Brushes.Green;
                            tempBordes.Add(sBorder6);
                            break;
                        case "6":
                            y = 6;
                            sBorder7.Background = Brushes.Green;
                            tempBordes.Add(sBorder7);
                            break;
                        case "7":
                            y = 7;
                            sBorder8.Background = Brushes.Green;
                            tempBordes.Add(sBorder8);
                            break;
                        case "8":
                            y = 8;
                            sBorder9.Background = Brushes.Green;
                            tempBordes.Add(sBorder9);
                            break;
                        case "9":
                            y = 9;
                            sBorder10.Background = Brushes.Green;
                            tempBordes.Add(sBorder10);
                            break;
                        case "A":
                            y = 10;
                            sBorder11.Background = Brushes.Green;
                            tempBordes.Add(sBorder11);
                            break;
                        case "B":
                            y = 11;
                            sBorder12.Background = Brushes.Green;
                            tempBordes.Add(sBorder12);
                            break;
                        case "C":
                            y = 12;
                            sBorder13.Background = Brushes.Green;
                            tempBordes.Add(sBorder13);
                            break;
                        case "D":
                            y = 13;
                            sBorder14.Background = Brushes.Green;
                            tempBordes.Add(sBorder14);
                            break;
                        case "E":
                            y = 14;
                            sBorder15.Background = Brushes.Green;
                            tempBordes.Add(sBorder15);
                            break;
                        case "F":
                            y = 15;
                            sBorder16.Background = Brushes.Green;
                            tempBordes.Add(sBorder16);
                            break;
                        default:
                            break;
                    }
                    sTransitionTextBlock2.Background = Brushes.Transparent;
                }, null);
                wait();
                r = (x + 1) * 18 + y + 1 - 19 - 2 * x;
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    textBlockList[3][r].Background = Brushes.Green;
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    sResult[z].Text = sBox[x][y].ToString("X2");
                    sResult[z].Background = Brushes.Green;
                    sTransitionBorder1.Visibility = Visibility.Hidden;
                    sTransitionBorder2.Visibility = Visibility.Hidden;
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    sResult[z].Background = Brushes.Transparent;
                    foreach (Border br in tempBordes)
                    {
                        br.Background = Brushes.Yellow;
                    }
                    tempBordes.Clear();
                    z++;
                    sTransitionTextBlock1.Text = "";
                    sTransitionTextBlock2.Text = "";
                   textBlockList[3][r].Background = Brushes.Transparent;
                }, null);
                wait();
            }
        }

        public void setUpSubByte(byte[][] states)
        {
            lightRemoveColor();
            invisible();
            List<TextBlock> temp = createTextBlockList(4);
            int x = 0;
            foreach (TextBlock tb in temp)
            {
                tb.Text = states[(roundNumber - 1) * 4 + action - 1][x].ToString("X2");
                x++;
            }
            temp = createTextBlockList(5);
            foreach (TextBlock tb in temp)
            {
                tb.Text = "";
            }
            sBoxGrid.Visibility = Visibility.Visible;
            subByteStateGrid.Visibility = Visibility.Visible;
            subByteResultGrid.Visibility = Visibility.Visible;
            subByteTransitionGrid.Visibility = Visibility.Visible;
            expansionKeyGrid.Visibility = Visibility.Hidden;
            subByteButton.Background = Brushes.Aqua;        
            
        }

        public TextBlock findTextBlock(int r)
        {
            string tempString = "sTextBlock" + r;
            tempBlock = (TextBlock)FindName(tempString);
            return tempBlock;
        }
        
        private List<List<TextBlock>> createSBoxList()
        {
            List<List<TextBlock>> result = new List<List<TextBlock>>();
            List<TextBlock> list = new List<TextBlock>();           
            string temp = "sTextBlock";
            for(int z = 0; z < 10; z++)
            {

                switch (z)
                {
                    case 0:
                        int x = 19;
                        while (x < 49)
                        {
                            if (x % 18 != 0 && (x + 1) % 18 != 0)
                            {
                                string y = temp + x;
                                list.Add((TextBlock)FindName(y));
                                x++;
                            }
                            else
                            {
                                x++;
                            }
                        }
                        result.Add(list);
                        list.Clear();
                        break;
                    case 1:
                        x = 49;
                        while (x < 79)
                        {
                            if (x % 18 != 0 && (x + 1) % 18 != 0)
                            {
                                string y = temp + x;
                                list.Add((TextBlock)FindName(y));
                                x++;
                            }
                            else
                            {
                                x++;
                            }
                        }
                        result.Add(list);
                        list.Clear();
                        break;
                    case 2:
                        x = 79;
                        while (x < 109)
                        {
                            if (x % 18 != 0 && (x + 1) % 18 != 0)
                            {
                                string y = temp + x;
                                list.Add((TextBlock)FindName(y));
                                x++;
                            }
                            else
                            {
                                x++;
                            }
                        }
                        result.Add(list);
                        list.Clear();
                        break;
                    case 3:
                        x = 109;
                        while (x < 139)
                        {
                            if (x % 18 != 0 && (x + 1) % 18 != 0)
                            {
                                string y = temp + x;
                                list.Add((TextBlock)FindName(y));
                                x++;
                            }
                            else
                            {
                                x++;
                            }
                        }
                        result.Add(list);
                        list.Clear();
                        break;
                    case 4:
                        x = 139;
                        while (x < 169)
                        {
                            if (x % 18 != 0 && (x + 1) % 18 != 0)
                            {
                                string y = temp + x;
                                list.Add((TextBlock)FindName(y));
                                x++;
                            }
                            else
                            {
                                x++;
                            }
                        }
                        result.Add(list);
                        list.Clear();
                        break;
                    case 5:
                        x = 169;
                        while (x < 199)
                        {
                            if (x % 18 != 0 && (x + 1) % 18 != 0)
                            {
                                string y = temp + x;
                                list.Add((TextBlock)FindName(y));
                                x++;
                            }
                            else
                            {
                                x++;
                            }
                        }
                        result.Add(list);
                        list.Clear();
                        break;
                    case 6:
                        x = 199;
                        while (x < 229)
                        {
                            if (x % 18 != 0 && (x + 1) % 18 != 0)
                            {
                                string y = temp + x;
                                list.Add((TextBlock)FindName(y));
                                x++;
                            }
                            else
                            {
                                x++;
                            }
                        }
                        result.Add(list);
                        list.Clear();
                        break;
                    case 7:
                        x = 229;
                        while (x < 259)
                        {
                            if (x % 18 != 0 && (x + 1) % 18 != 0)
                            {
                                string y = temp + x;
                                list.Add((TextBlock)FindName(y));
                                x++;
                            }
                            else
                            {
                                x++;
                            }
                        }
                        result.Add(list);
                        list.Clear();
                        break;
                    case 8:
                        x = 259;
                        while (x < 289)
                        {
                            if (x % 18 != 0 && (x + 1) % 18 != 0)
                            {
                                string y = temp + x;
                                list.Add((TextBlock)FindName(y));
                                x++;
                            }
                            else
                            {
                                x++;
                            }
                        }
                        result.Add(list);
                        list.Clear();
                        break;
                    case 9:
                        x = 289;
                        while (x < 306)
                        {
                            if (x % 18 != 0 && (x + 1) % 18 != 0)
                            {
                                string y = temp + x;
                                list.Add((TextBlock)FindName(y));
                                x++;
                            }
                            else
                            {
                                x++;
                            }
                        }
                        result.Add(list);
                        list.Clear();
                        break;
                    default:
                        break;
                }            

            }
            return result;
        }

        private void wait()
        {
            if (!autostep)
            {              
                buttonNextClickedEvent.WaitOne();
            }
            else
            {                
                buttonNextClickedEvent.WaitOne(autostepSpeed);
            }
        }

        public void enableButtons()
        {
            
            
            round1Button.IsEnabled = true;
            round2Button.IsEnabled = true;
            round3Button.IsEnabled = true;
            round4Button.IsEnabled = true;
            round5Button.IsEnabled = true;
            round6Button.IsEnabled = true;
            round7Button.IsEnabled = true;
            round8Button.IsEnabled = true;
            round9Button.IsEnabled = true;
            round10Button.IsEnabled = true;
            keyButton.IsEnabled = true;
            nextStepButton.IsEnabled = true;
            prevStepButton.IsEnabled = true;          
            if (!expansion)
            {
                subByteButton.IsEnabled = true;
                shiftRowButton.IsEnabled = true;
                addKeyButton.IsEnabled = true;
                if (roundNumber < 10)
                {
                    mixColButton.IsEnabled = true;
                }
            }
        }

        public void disableButtons()
        {
            subByteButton.IsEnabled = false;
            shiftRowButton.IsEnabled = false;
            mixColButton.IsEnabled = false;
            addKeyButton.IsEnabled = false;
            round1Button.IsEnabled = false;
            round2Button.IsEnabled = false;
            round3Button.IsEnabled = false;
            round4Button.IsEnabled = false;
            round5Button.IsEnabled = false;
            round6Button.IsEnabled = false;
            round7Button.IsEnabled = false;
            round8Button.IsEnabled = false;
            round9Button.IsEnabled = false;
            round10Button.IsEnabled = false;
            keyButton.IsEnabled = false;
            nextStepButton.IsEnabled = false;
            prevStepButton.IsEnabled = false;
        }

        private void resetShiftRow()
        {
            List<Border> borders = createBorderList(3);
            int temp = 0;
            while (temp < 16)
            {
                borders[temp].Visibility = Visibility.Visible;
                temp++;
            }
            while (temp < 34)
            {
                borders[temp].Visibility = Visibility.Hidden;
                temp++;
            }
        }

        private void setUpShiftRows()
        {
            resetShiftRow();
            invisible();
            lightRemoveColor();
            rowSetBlockText(states[(roundNumber - 1) * 4 + action - 1]);
            shiftRowGrid.Visibility = Visibility.Visible;
            shiftRowButton.Background = Brushes.Aqua;
        }

        private void rowSetBlockText(byte[] block)
        {
            textBlock1.Text = block[0].ToString("X2");
            textBlock2.Text = block[1].ToString("X2");
            textBlock3.Text = block[2].ToString("X2");
            textBlock4.Text = block[3].ToString("X2");
            textBlock5.Text = block[4].ToString("X2");
            textBlock6.Text = block[5].ToString("X2");
            textBlock7.Text = block[6].ToString("X2");
            textBlock8.Text = block[7].ToString("X2");
            textBlock9.Text = block[8].ToString("X2");
            textBlock10.Text = block[9].ToString("X2");
            textBlock11.Text = block[10].ToString("X2");
            textBlock12.Text = block[11].ToString("X2");
            textBlock13.Text = block[12].ToString("X2");
            textBlock14.Text = block[13].ToString("X2");
            textBlock15.Text = block[14].ToString("X2");
            textBlock16.Text = block[15].ToString("X2");
            textBlock17.Text = block[4].ToString("X2");
            textBlock18.Text = block[5].ToString("X2");
            textBlock19.Text = block[6].ToString("X2");
            textBlock20.Text = block[7].ToString("X2");
            textBlock21.Text = block[8].ToString("X2");
            textBlock22.Text = block[9].ToString("X2");
            textBlock23.Text = block[10].ToString("X2");
            textBlock24.Text = block[11].ToString("X2");
            textBlock25.Text = block[12].ToString("X2");
            textBlock26.Text = block[13].ToString("X2");
            textBlock27.Text = block[14].ToString("X2");
            textBlock28.Text = block[15].ToString("X2");
            textBlock29.Text = block[4].ToString("X2");
            textBlock30.Text = block[8].ToString("X2");
            textBlock31.Text = block[9].ToString("X2");
            textBlock32.Text = block[12].ToString("X2");
            textBlock33.Text = block[13].ToString("X2");
            textBlock34.Text = block[14].ToString("X2");
        }

        private void shiftRow()
        {
            List<Border> borders = createBorderList(3);
            int temp = 4;
            wait();
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                while (temp < 8)
                {
                    borders[temp].Visibility = Visibility.Hidden;
                    temp++;
                }
            }, null);            
            temp = 16;
            wait();
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                while (temp < 20)
                {
                    borders[temp].Visibility = Visibility.Visible;
                    temp++;
                }
            }, null);
            temp = 8;
            wait();
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                while (temp < 12)
                {
                    borders[temp].Visibility = Visibility.Hidden;
                    temp++;
                }
            }, null);
            temp = 20;
            wait();
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                while (temp < 24)
                {
                    borders[temp].Visibility = Visibility.Visible;
                    temp++;
                }
            }, null);
            wait();
            temp = 12;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                while (temp < 16)
                {
                    borders[temp].Visibility = Visibility.Hidden;
                    temp++;
                }
            }, null);
            temp = 24;
            wait();
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                while (temp < 28)
                {
                    borders[temp].Visibility = Visibility.Visible;
                    temp++;
                }
            }, null);
            wait();
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                borders[16].Visibility = Visibility.Hidden;
                borders[20].Visibility = Visibility.Hidden;
                borders[21].Visibility = Visibility.Hidden;
                borders[24].Visibility = Visibility.Hidden;
                borders[25].Visibility = Visibility.Hidden;
                borders[26].Visibility = Visibility.Hidden;
            }, null);           
            wait();
            temp = 27;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                while (temp < 34)
                {
                    borders[temp].Visibility = Visibility.Visible;
                    temp++;
                }
            }, null);
            wait();            
        }

        private void setUpMixColumns()
        {
            lightRemoveColor();
            invisible();
            mixColButton.Background = Brushes.Aqua;
            setMixStateTransition(states[(roundNumber - 1) * 4 + action - 1]);
            mixColMatrixGrid.Visibility = Visibility.Visible;
            mixColStateGrid.Visibility = Visibility.Visible;
            mixColResultGrid.Visibility = Visibility.Visible;
            mixColTransitionGrid.Visibility = Visibility.Visible;
        }

        private void mixColumns()
        {
            List<Border> stateList = borderList[4];
            List<Border> matrixList = borderList[5];
            List<Border> transitionList = borderList[6];
            List<Border> resultList = borderList[7];
            mColoring(0, matrixList, resultList, stateList, transitionList);
            mColoring(1, matrixList, resultList, stateList, transitionList);
            mColoring(2, matrixList, resultList, stateList, transitionList);
            mColoring(3, matrixList, resultList, stateList, transitionList);                          
        }

        private void mColoring(int z, List<Border> matrixList, List<Border> resultList, List<Border> stateList, List<Border> transitionList)
        {
            List<TextBlock> resultBoxes = textBlockList[8];
            int y = 0;
            int x = 0;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                while (x < 16)
                {
                    if (x == 0 + z || x == 4 + z || x == 8 + z || x == 12 + z)
                    {
                        stateList[x].Background = Brushes.Green;
                    }
                    x++;
                }
            }, null);           
            x = 0;
            wait();
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                while (x < 16)
                {
                    if (x == 0 + z || x == 4 + z || x == 8 + z || x == 12 + z)
                    {
                        stateList[x].Background = Brushes.Transparent;
                        transitionList[x].Background = Brushes.Green;
                        transitionList[x].Visibility = Visibility.Visible;
                    }
                    x++;
                }
            }, null);           
            x = 0;
            wait();
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                while (x < 16)
                {
                    if (x == 0 + z || x == 4 + z || x == 8 + z || x == 12 + z)
                    {
                        transitionList[x].Background = Brushes.Transparent;
                    }
                    x++;
                }
            }, null);            
            x = 0;
            wait();
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                while (y < 4)
                {
                    matrixList[y].Background = Brushes.Green;
                    y++;
                }
            }, null);
            wait();
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                resultList[0 + z].Background = Brushes.Green;
                resultBoxes[0 + z].Text = states[(roundNumber - 1) * 4 + action][0 + z].ToString("X2");
            }, null);
            wait();
            y = 0;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                while (y < 4)
                {
                    matrixList[y].Background = Brushes.Transparent;
                    y++;
                }
                resultList[0 + z].Background = Brushes.Transparent;
            }, null);
            wait();
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                while (y < 8)
                {
                    matrixList[y].Background = Brushes.Green;
                    y++;
                }
            }, null);       
            y = 4;
            wait();
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                resultList[4 + z].Background = Brushes.Green;
                resultBoxes[4 + z].Text = states[(roundNumber - 1) * 4 + action][4 + z].ToString("X2");
            }, null);
            wait();
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                while (y < 8)
                {
                    matrixList[y].Background = Brushes.Transparent;
                    resultList[4 + z].Background = Brushes.Transparent;
                    y++;
                }
            }, null);           
            wait();
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                while (y < 12)
                {
                    matrixList[y].Background = Brushes.Green;
                    y++;
                }
            }, null);
            y = 8;
            wait();
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                resultList[8 + z].Background = Brushes.Green;
                resultBoxes[8 + z].Text = states[(roundNumber - 1) * 4 + action][8 + z].ToString("X2");
            }, null);
            wait();
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                while (y < 12)
                {
                    resultList[8 + z].Background = Brushes.Transparent;
                    matrixList[y].Background = Brushes.Transparent;
                    y++;
                }
            }, null);
            wait();
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                while (y < 16)
                {
                    matrixList[y].Background = Brushes.Green;
                    y++;
                }
            }, null);          
            y = 12;
            wait();
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                resultList[12 + z].Background = Brushes.Green;
                resultBoxes[12 + z].Text = states[(roundNumber - 1) * 4 + action][12 + z].ToString("X2");
            }, null);
            wait();
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                while (y < 16)
                {
                    matrixList[y].Background = Brushes.Transparent;
                    y++;
                }
                resultList[12 + z].Background = Brushes.Transparent;
            }, null);
            wait();
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                while (x < 16)
                {
                    if (x == 0 + z || x == 4 + z || x == 8 + z || x == 12 + z)
                    {
                        transitionList[x].Visibility = Visibility.Hidden;
                    }
                    x++;
                }
            }, null);          
        }

        private void setUpAddKey()
        {
            lightRemoveColor();
            action = 4;
            resetAddKey();
            invisible();
            if(roundNumber == 0)
            {
                keySetBlockText(tempState, key);
            }
            else if (roundNumber == 10)
            {
                keySetBlockText(states[(roundNumber - 1) * 4 + action - 2], key);
            }
            else
            {
                keySetBlockText(states[(roundNumber - 1) * 4 + action - 1], key);
            }
            addKeyStateGrid.Visibility = Visibility.Visible;
            addKeyKeyGrid.Visibility = Visibility.Visible;
            addKeyResultGrid.Visibility = Visibility.Visible;
            addKeyButton.Background = Brushes.Aqua;
        }

        private void resetAddKey()
        {
            List<TextBlock> resultList = textBlockList[2];
            foreach (TextBlock tb in resultList)
            {
                tb.Text = "";
            }
        }

        private void keySetBlockText(byte[] temp, byte[] key)
        {
            keyTextBlock1.Text = temp[0].ToString("X2");
            keyTextBlock2.Text = temp[1].ToString("X2");
            keyTextBlock3.Text = temp[2].ToString("X2");
            keyTextBlock4.Text = temp[3].ToString("X2");
            keyTextBlock5.Text = temp[4].ToString("X2");
            keyTextBlock6.Text = temp[5].ToString("X2");
            keyTextBlock7.Text = temp[6].ToString("X2");
            keyTextBlock8.Text = temp[7].ToString("X2");
            keyTextBlock9.Text = temp[8].ToString("X2");
            keyTextBlock10.Text = temp[9].ToString("X2");
            keyTextBlock11.Text = temp[10].ToString("X2");
            keyTextBlock12.Text = temp[11].ToString("X2");
            keyTextBlock13.Text = temp[12].ToString("X2");
            keyTextBlock14.Text = temp[13].ToString("X2");
            keyTextBlock15.Text = temp[14].ToString("X2");
            keyTextBlock16.Text = temp[15].ToString("X2");
            keyTextBlock17.Text = keyList[roundNumber][0].ToString("X2");
            keyTextBlock18.Text = keyList[roundNumber][1].ToString("X2");
            keyTextBlock19.Text = keyList[roundNumber][2].ToString("X2");
            keyTextBlock20.Text = keyList[roundNumber][3].ToString("X2");
            keyTextBlock21.Text = keyList[roundNumber][4].ToString("X2");
            keyTextBlock22.Text = keyList[roundNumber][5].ToString("X2");
            keyTextBlock23.Text = keyList[roundNumber][6].ToString("X2");
            keyTextBlock24.Text = keyList[roundNumber][7].ToString("X2");
            keyTextBlock25.Text = keyList[roundNumber][8].ToString("X2");
            keyTextBlock26.Text = keyList[roundNumber][9].ToString("X2");
            keyTextBlock27.Text = keyList[roundNumber][10].ToString("X2");
            keyTextBlock28.Text = keyList[roundNumber][11].ToString("X2");
            keyTextBlock29.Text = keyList[roundNumber][12].ToString("X2");
            keyTextBlock30.Text = keyList[roundNumber][13].ToString("X2");
            keyTextBlock31.Text = keyList[roundNumber][14].ToString("X2");
            keyTextBlock32.Text = keyList[roundNumber][15].ToString("X2");
        }

        private void addKey()
        {
            byte[] result;
            if(roundNumber == 0)
            {
                result = states[0];
            }
            else if (roundNumber == 10)
            {
                result = states[(roundNumber - 1) * 4 + action - 1];
            }
            else
            {
                result = states[(roundNumber - 1) * 4 + action];
            }
            List<TextBlock> resultList = textBlockList[2];
            List<Border> resultBorders = borderList[2];
            List<Border> stateBorders = borderList[0];
            List<Border> keyBorders = borderList[1];
            int y = 0;
            foreach (TextBlock tb in resultList)
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    if (y > 0)
                    {
                        resultBorders[y - 1].Background = Brushes.Transparent;
                        stateBorders[y - 1].Background = Brushes.Transparent;
                        keyBorders[y - 1].Background = Brushes.Transparent;
                    }
                    stateBorders[y].Background = Brushes.Green;
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    keyBorders[y].Background = Brushes.Green;
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    renameTextBlock(tb, result[y]);
                    resultBorders[y].Background = Brushes.Green;
                }, null);
                wait();
                y++;
            }
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                resultBorders[y - 1].Background = Brushes.Transparent;
                stateBorders[y - 1].Background = Brushes.Transparent;
                keyBorders[y - 1].Background = Brushes.Transparent;
            }, null);          
        }

        private void renameTextBlock(TextBlock tb, byte temp)
        {
            tb.Text = temp.ToString("X2");
        }

        public void keyExpansion()
        {
            List<Border> keyBorders = borderList[8];
            List<Border> transitionBorders = borderList[9];
            List<Border> transition1Borders = borderList[10];
            List<Border> resultBorders = borderList[11];
            List<TextBlock> keyBlocks = textBlockList[9];
            List<TextBlock> transitionBlocks = textBlockList[10];
            List<TextBlock> transition1Blocks = textBlockList[11];
            List<TextBlock> resultBlocks = textBlockList[12];
            List<Border> tempBordes = new List<Border>();
            string[] tempString = new string[4];
            int x = 0;
            int y = 0;
            int z;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                setUpExpansion();
                enableButtons();
            }, null);
            wait();
            if (!expansion)
            {
                return;
            }
            while (roundNumber < 11)
            {                       
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    transitionBlocks[1].Text = keyBlocks[3].Text;
                    transitionBlocks[4].Text = keyBlocks[7].Text;
                    transitionBlocks[7].Text = keyBlocks[11].Text;
                    transitionBlocks[10].Text = keyBlocks[15].Text;
                    keyBorders[3].Background = Brushes.Green;
                    keyBorders[7].Background = Brushes.Green;
                    keyBorders[11].Background = Brushes.Green;
                    keyBorders[15].Background = Brushes.Green;
                    expansionExplanation.Visibility = Visibility.Visible;
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    expansionTransitionGrid.Visibility = Visibility.Visible;
                    transitionBorders[1].Background = Brushes.Green;
                    transitionBorders[4].Background = Brushes.Green;
                    transitionBorders[7].Background = Brushes.Green;
                    transitionBorders[10].Background = Brushes.Green;
                    keyBorders[3].Background = Brushes.Transparent;
                    keyBorders[7].Background = Brushes.Transparent;
                    keyBorders[11].Background = Brushes.Transparent;
                    keyBorders[15].Background = Brushes.Transparent;
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    transitionBorders[1].Background = Brushes.Transparent;
                    transitionBorders[4].Background = Brushes.Transparent;
                    transitionBorders[7].Background = Brushes.Transparent;
                    transitionBorders[10].Background = Brushes.Transparent;
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    transitionBorders[10].Background = Brushes.Green;
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    transitionBorders[10].Background = Brushes.Transparent;
                    transitionBlocks[10].Text = "";
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    transitionBlocks[1].Text = "";
                    transitionBlocks[4].Text = keyBlocks[3].Text;
                    transitionBlocks[7].Text = keyBlocks[7].Text;
                    transitionBlocks[10].Text = keyBlocks[11].Text;                   
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    transitionBlocks[1].Text = keyBlocks[15].Text;
                    transitionBorders[1].Background = Brushes.Green;
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                { 
                    transitionBorders[1].Background = Brushes.Transparent;
                    expansionExplanation.Visibility = Visibility.Hidden;
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {                   
                    sBoxGrid.Visibility = Visibility.Visible;
                    expansionTransitionGrid1.Visibility = Visibility.Visible;
                    expansionExplanation1.Visibility = Visibility.Visible;
                }, null);
                wait();
                for (int r = 0; r < 4; r++)
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        transitionBorders[1 + 3 * r].Background = Brushes.Green;
                    }, null);
                    wait();
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        transitionBorders[1 + 3 * r].Visibility = Visibility.Hidden;
                        transitionBorders[3 * r].Visibility = Visibility.Visible;
                        transitionBorders[2 + 3 * r].Visibility = Visibility.Visible;
                        transitionBlocks[3 * r].Text = transitionBlocks[3 * r + 1].Text.Substring(0, 1);
                        transitionBlocks[3 * r + 2].Text = transitionBlocks[3 * r + 1].Text.Substring(1, 1);
                        transitionBorders[1 + 3 * r].Background = Brushes.Transparent;
                    }, null);
                    wait();
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        transitionBorders[3 * r].Background = Brushes.Green;
                    }, null);
                    wait();
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        switch (transitionBlocks[3*r].Text)
                        {
                            case "0":
                                x = 0;
                                sBorder18.Background = Brushes.Green;
                                tempBordes.Add(sBorder18);
                                break;
                            case "1":
                                x = 1;
                                sBorder36.Background = Brushes.Green;
                                tempBordes.Add(sBorder36);
                                break;
                            case "2":
                                x = 2;
                                sBorder54.Background = Brushes.Green;
                                tempBordes.Add(sBorder54);
                                break;
                            case "3":
                                x = 3;
                                sBorder72.Background = Brushes.Green;
                                tempBordes.Add(sBorder72);
                                break;
                            case "4":
                                x = 4;
                                sBorder90.Background = Brushes.Green;
                                tempBordes.Add(sBorder90);
                                break;
                            case "5":
                                x = 5;
                                sBorder108.Background = Brushes.Green;
                                tempBordes.Add(sBorder108);
                                break;
                            case "6":
                                x = 6;
                                sBorder126.Background = Brushes.Green;
                                tempBordes.Add(sBorder126);
                                break;
                            case "7":
                                x = 7;
                                sBorder144.Background = Brushes.Green;
                                tempBordes.Add(sBorder144);
                                break;
                            case "8":
                                x = 8;
                                sBorder162.Background = Brushes.Green;
                                tempBordes.Add(sBorder162);
                                break;
                            case "9":
                                x = 9;
                                sBorder180.Background = Brushes.Green;
                                tempBordes.Add(sBorder180);
                                break;
                            case "A":
                                x = 10;
                                sBorder198.Background = Brushes.Green;
                                tempBordes.Add(sBorder198);
                                break;
                            case "B":
                                x = 11;
                                sBorder216.Background = Brushes.Green;
                                tempBordes.Add(sBorder216);
                                break;
                            case "C":
                                x = 12;
                                sBorder234.Background = Brushes.Green;
                                tempBordes.Add(sBorder234);
                                break;
                            case "D":
                                x = 13;
                                sBorder252.Background = Brushes.Green;
                                tempBordes.Add(sBorder252);
                                break;
                            case "E":
                                x = 14;
                                sBorder270.Background = Brushes.Green;
                                tempBordes.Add(sBorder270);
                                break;
                            case "F":
                                x = 15;
                                sBorder288.Background = Brushes.Green;
                                tempBordes.Add(sBorder288);
                                break;
                            default:
                                break;
                        }
                        transitionBorders[3 * r].Background = Brushes.Transparent;
                    }, null);
                    wait();
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        transitionBorders[3 * r + 2].Background = Brushes.Green;
                    }, null);
                    wait();
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        switch (transitionBlocks[3 * r + 2].Text)
                        {
                            case "0":
                                y = 0;
                                sBorder1.Background = Brushes.Green;
                                tempBordes.Add(sBorder1);
                                break;
                            case "1":
                                y = 1;
                                sBorder2.Background = Brushes.Green;
                                tempBordes.Add(sBorder2);
                                break;
                            case "2":
                                y = 2;
                                sBorder3.Background = Brushes.Green;
                                tempBordes.Add(sBorder3);
                                break;
                            case "3":
                                y = 3;
                                sBorder4.Background = Brushes.Green;
                                tempBordes.Add(sBorder4);
                                break;
                            case "4":
                                y = 4;
                                sBorder5.Background = Brushes.Green;
                                tempBordes.Add(sBorder5);
                                break;
                            case "5":
                                y = 5;
                                sBorder6.Background = Brushes.Green;
                                tempBordes.Add(sBorder6);
                                break;
                            case "6":
                                y = 6;
                                sBorder7.Background = Brushes.Green;
                                tempBordes.Add(sBorder7);
                                break;
                            case "7":
                                y = 7;
                                sBorder8.Background = Brushes.Green;
                                tempBordes.Add(sBorder8);
                                break;
                            case "8":
                                y = 8;
                                sBorder9.Background = Brushes.Green;
                                tempBordes.Add(sBorder9);
                                break;
                            case "9":
                                y = 9;
                                sBorder10.Background = Brushes.Green;
                                tempBordes.Add(sBorder10);
                                break;
                            case "A":
                                y = 10;
                                sBorder11.Background = Brushes.Green;
                                tempBordes.Add(sBorder11);
                                break;
                            case "B":
                                y = 11;
                                sBorder12.Background = Brushes.Green;
                                tempBordes.Add(sBorder12);
                                break;
                            case "C":
                                y = 12;
                                sBorder13.Background = Brushes.Green;
                                tempBordes.Add(sBorder13);
                                break;
                            case "D":
                                y = 13;
                                sBorder14.Background = Brushes.Green;
                                tempBordes.Add(sBorder14);
                                break;
                            case "E":
                                y = 14;
                                sBorder15.Background = Brushes.Green;
                                tempBordes.Add(sBorder15);
                                break;
                            case "F":
                                y = 15;
                                sBorder16.Background = Brushes.Green;
                                tempBordes.Add(sBorder16);
                                break;
                            default:
                                break;
                        }
                        transitionBorders[3 * r + 2].Background = Brushes.Transparent;
                    }, null);
                    wait();
                    z = (x + 1) * 18 + y + 1 - 19 - 2 * x;
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        textBlockList[3][z].Background = Brushes.Green;
                    }, null);
                    wait();
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        transition1Blocks[r * 3 + 2].Text = sBox[x][y].ToString("X2");
                        transition1Blocks[r * 3 + 2].Background = Brushes.Green;
                        transitionBorders[3 * r].Visibility = Visibility.Hidden;
                        transitionBorders[2 + 3 * r].Visibility = Visibility.Hidden;
                    }, null);
                    wait();
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        transition1Blocks[r * 3 + 2].Background = Brushes.Transparent;
                        foreach (Border br in tempBordes)
                        {
                            br.Background = Brushes.Yellow;
                        }
                        tempBordes.Clear();
                        transitionBlocks[3 * r].Text = "";
                        transitionBlocks[3 * r + 2].Text = "";
                        textBlockList[3][z].Background = Brushes.Transparent;
                    }, null);
                    wait();                  
                }
                byte[] constant = { Convert.ToByte(roundNumber), 00, 00, 00 };
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    expansionExplanation1.Visibility = Visibility.Hidden;
                    expansionExplanation2.Visibility = Visibility.Visible;
                    transition1Borders[0].Visibility = Visibility.Visible;
                    transition1Borders[3].Visibility = Visibility.Visible;
                    transition1Borders[6].Visibility = Visibility.Visible;
                    transition1Borders[9].Visibility = Visibility.Visible;
                    transition1Blocks[0].Text = constant[0].ToString("X2");
                    transition1Blocks[3].Text = constant[1].ToString("X2");
                    transition1Blocks[6].Text = constant[2].ToString("X2");
                    transition1Blocks[9].Text = constant[3].ToString("X2");
                    expansionResultGrid.Visibility = Visibility.Visible;
                    sBoxGrid.Visibility = Visibility.Hidden;
                }, null);
                wait(); 
                for(z = 0; z < 4; z++)
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        transition1Borders[z * 3].Background = Brushes.Green;
                    }, null);
                    wait();
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        transition1Borders[z * 3 + 2].Background = Brushes.Green;
                    }, null);
                    wait();
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        transition1Borders[z * 3].Background = Brushes.Transparent;
                        transition1Borders[z * 3 + 2].Background = Brushes.Transparent;
                        resultBorders[z * 4].Background = Brushes.Green;
                        byte[] l = StringToByteArray(transition1Blocks[z * 3].Text);
                        byte m = StringToByteArray(transition1Blocks[z * 3 + 2].Text)[0];
                        byte n = (byte)(l[0] ^ m);
                        resultBlocks[z * 4].Text = n.ToString("X2");
                    }, null);
                    wait();
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {                       
                        resultBorders[z * 4].Background = Brushes.Transparent;                       
                    }, null);
                    wait();                                 
                }
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    transition1Blocks[0].Text = "";
                    transition1Blocks[2].Text = "";
                    transition1Blocks[3].Text = "";
                    transition1Blocks[5].Text = "";
                    transition1Blocks[6].Text = "";
                    transition1Blocks[8].Text = "";
                    transition1Blocks[9].Text = "";
                    transition1Blocks[11].Text = "";
                    expansionExplanation2.Visibility = Visibility.Hidden;
                    expansionExplanation3.Visibility = Visibility.Visible;
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    resultBorders[0].Background = Brushes.Green;
                    resultBorders[4].Background = Brushes.Green;
                    resultBorders[8].Background = Brushes.Green;
                    resultBorders[12].Background = Brushes.Green;
                }, null);
                wait();                  
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    tempString[0] = resultBlocks[0].Text;
                    tempString[1] = resultBlocks[4].Text;
                    tempString[2] = resultBlocks[8].Text;
                    tempString[3] = resultBlocks[12].Text;
                    resultBorders[0].Background = Brushes.Transparent;
                    resultBorders[4].Background = Brushes.Transparent;
                    resultBorders[8].Background = Brushes.Transparent;
                    resultBorders[12].Background = Brushes.Transparent;
                    resultBlocks[0].Text = "";
                    resultBlocks[4].Text = "";
                    resultBlocks[8].Text = "";
                    resultBlocks[12].Text = "";
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    transition1Borders[2].Background = Brushes.Green;
                    transition1Borders[5].Background = Brushes.Green;
                    transition1Borders[8].Background = Brushes.Green;
                    transition1Borders[11].Background = Brushes.Green;
                    transition1Blocks[2].Text = tempString[0];
                    transition1Blocks[5].Text = tempString[1];
                    transition1Blocks[8].Text = tempString[2];
                    transition1Blocks[11].Text = tempString[3];
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    transition1Borders[2].Background = Brushes.Transparent;
                    transition1Borders[5].Background = Brushes.Transparent;
                    transition1Borders[8].Background = Brushes.Transparent;
                    transition1Borders[11].Background = Brushes.Transparent;
                    keyBorders[0].Background = Brushes.Green;
                    keyBorders[4].Background = Brushes.Green;
                    keyBorders[8].Background = Brushes.Green;
                    keyBorders[12].Background = Brushes.Green;                  
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    transition1Borders[0].Background = Brushes.Green;
                    transition1Borders[3].Background = Brushes.Green;
                    transition1Borders[6].Background = Brushes.Green;
                    transition1Borders[9].Background = Brushes.Green;
                    transition1Blocks[0].Text = keyBlocks[0].Text;
                    transition1Blocks[3].Text = keyBlocks[4].Text;
                    transition1Blocks[6].Text = keyBlocks[8].Text;
                    transition1Blocks[9].Text = keyBlocks[12].Text;
                    keyBorders[0].Background = Brushes.Transparent;
                    keyBorders[4].Background = Brushes.Transparent;
                    keyBorders[8].Background = Brushes.Transparent;
                    keyBorders[12].Background = Brushes.Transparent;
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    transition1Borders[0].Background = Brushes.Transparent;
                    transition1Borders[3].Background = Brushes.Transparent;
                    transition1Borders[6].Background = Brushes.Transparent;
                    transition1Borders[9].Background = Brushes.Transparent;                   
                }, null);
                wait();
                x = 0;
                while(x < 4)
                {
                    for (z = 0; z < 4; z++)
                    {
                        Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            transition1Borders[z * 3].Background = Brushes.Green;
                        }, null);
                        wait();
                        Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            transition1Borders[z * 3 + 2].Background = Brushes.Green;
                        }, null);
                        wait();
                        Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            transition1Borders[z * 3].Background = Brushes.Transparent;
                            transition1Borders[z * 3 + 2].Background = Brushes.Transparent;
                            resultBorders[z * 4 + x].Background = Brushes.Green;                           
                            resultBlocks[z * 4 + x].Text = keyList[roundNumber][z * 4 + x].ToString("X2");
                        }, null);
                        wait();
                        Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            resultBorders[z * 4 + x].Background = Brushes.Transparent;
                        }, null);
                        wait();                       
                    }
                    if(x < 3)
                    {
                        Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            expansionExplanation3.Visibility = Visibility.Hidden;
                            expansionExplanation4.Visibility = Visibility.Visible;
                            transition1Blocks[0].Text = "";
                            transition1Blocks[2].Text = "";
                            transition1Blocks[3].Text = "";
                            transition1Blocks[5].Text = "";
                            transition1Blocks[6].Text = "";
                            transition1Blocks[8].Text = "";
                            transition1Blocks[9].Text = "";
                            transition1Blocks[11].Text = "";
                        }, null);
                        wait();
                        Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            resultBorders[0 + x].Background = Brushes.Green;
                            resultBorders[4 + x].Background = Brushes.Green;
                            resultBorders[8 + x].Background = Brushes.Green;
                            resultBorders[12 + x].Background = Brushes.Green;
                        }, null);
                        wait();
                        Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            tempString[0] = resultBlocks[0 + x].Text;
                            tempString[1] = resultBlocks[4 + x].Text;
                            tempString[2] = resultBlocks[8 + x].Text;
                            tempString[3] = resultBlocks[12 + x].Text;
                            resultBorders[0 + x].Background = Brushes.Transparent;
                            resultBorders[4 + x].Background = Brushes.Transparent;
                            resultBorders[8 + x].Background = Brushes.Transparent;
                            resultBorders[12 + x].Background = Brushes.Transparent;
                        }, null);
                        wait();
                        Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            transition1Borders[2].Background = Brushes.Green;
                            transition1Borders[5].Background = Brushes.Green;
                            transition1Borders[8].Background = Brushes.Green;
                            transition1Borders[11].Background = Brushes.Green;
                            transition1Blocks[2].Text = tempString[0];
                            transition1Blocks[5].Text = tempString[1];
                            transition1Blocks[8].Text = tempString[2];
                            transition1Blocks[11].Text = tempString[3];
                        }, null);
                        wait();
                        Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            transition1Borders[2].Background = Brushes.Transparent;
                            transition1Borders[5].Background = Brushes.Transparent;
                            transition1Borders[8].Background = Brushes.Transparent;
                            transition1Borders[11].Background = Brushes.Transparent;
                            keyBorders[1 + x].Background = Brushes.Green;
                            keyBorders[5 + x].Background = Brushes.Green;
                            keyBorders[9 + x].Background = Brushes.Green;
                            keyBorders[13 + x].Background = Brushes.Green;
                        }, null);
                        wait();
                        Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            transition1Borders[0].Background = Brushes.Green;
                            transition1Borders[3].Background = Brushes.Green;
                            transition1Borders[6].Background = Brushes.Green;
                            transition1Borders[9].Background = Brushes.Green;
                            transition1Blocks[0].Text = keyBlocks[1 + x].Text;
                            transition1Blocks[3].Text = keyBlocks[5 + x].Text;
                            transition1Blocks[6].Text = keyBlocks[9 + x].Text;
                            transition1Blocks[9].Text = keyBlocks[13 + x].Text;
                            keyBorders[1 + x].Background = Brushes.Transparent;
                            keyBorders[5 + x].Background = Brushes.Transparent;
                            keyBorders[9 + x].Background = Brushes.Transparent;
                            keyBorders[13 + x].Background = Brushes.Transparent;
                        }, null);
                        wait();
                        Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            transition1Borders[0].Background = Brushes.Transparent;
                            transition1Borders[3].Background = Brushes.Transparent;
                            transition1Borders[6].Background = Brushes.Transparent;
                            transition1Borders[9].Background = Brushes.Transparent;
                        }, null);
                        wait();                    
                    }                    
                    x++;
                }
                autostep = false;
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    expansionExplanation4.Visibility = Visibility.Hidden;
                    transition1Blocks[0].Text = "";
                    transition1Blocks[2].Text = "";
                    transition1Blocks[3].Text = "";
                    transition1Blocks[5].Text = "";
                    transition1Blocks[6].Text = "";
                    transition1Blocks[8].Text = "";
                    transition1Blocks[9].Text = "";
                    transition1Blocks[11].Text = "";
                    foreach(TextBlock tb  in resultBlocks)
                    {
                        tb.Text = "";
                    }
                    expansionResultGrid.Visibility = Visibility.Hidden;
                    transition1Borders[0].Visibility = Visibility.Hidden;                   
                    transition1Borders[3].Visibility = Visibility.Hidden;
                    transition1Borders[6].Visibility = Visibility.Hidden;
                    transition1Borders[9].Visibility = Visibility.Hidden;
                    transitionBorders[1].Visibility = Visibility.Visible;
                    transitionBorders[4].Visibility = Visibility.Visible;
                    transitionBorders[7].Visibility = Visibility.Visible;
                    transitionBorders[10].Visibility = Visibility.Visible;
                    expansionTransitionGrid.Visibility = Visibility.Hidden;
                    expansionTransitionGrid1.Visibility = Visibility.Hidden;
                }, null);
                roundNumber++;                            
                if (roundNumber < 11)
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        setUpExpansion();
                    }, null);
                }
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    enableButtons();
                }, null);
                if(roundNumber < 11)
                {
                    autostep = false;
                    wait();
                }               
                if (!expansion)
                {
                    return;
                }
            }
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                expansionKeyGrid.Visibility = Visibility.Hidden;
            }, null);
            roundNumber = 1;
            expansion = false;
        }
        private void setUpExpansion()
        {
            List<Border> keyBorderList = borderList[8];
            List<TextBlock> keyBlockList = textBlockList[9];
            byte[] prevKey;
            changeRoundButton();
            int x;
            prevKey = keyList[roundNumber - 1];
            x = 0;
            foreach (TextBlock tb in keyBlockList)
            {
                tb.Text = prevKey[x].ToString("X2");
                x++;
            }
            expansionKeyGrid.Visibility = Visibility.Visible;
        }
        public byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
        public void exectue()
        {
            while (expansion)
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    keyButton.Content = "Skip Expansion";
                    invisible();
                    disableButtons();
                    changeRoundButton();
                    buttonVisible();
                }, null);
                keyExpansion();
                roundNumber = 0;
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    autostep = false;
                    expansionKeyGrid.Visibility = Visibility.Hidden;
                    keyButton.Content = "Key Expansion";
                    changeRoundButton();
                    setUpAddKey();
                    buttonVisible();
                }, null);
                actionMethod();
                action = 1;
                roundNumber = 1;
               
            }
        }
        public void buttonVisible()
        {
            if (expansion)
            {
                subByteButton.Visibility = Visibility.Hidden;
                shiftRowButton.Visibility = Visibility.Hidden;
                mixColButton.Visibility = Visibility.Hidden;
                addKeyButton.Visibility = Visibility.Hidden;
            }
            else
            {
                subByteButton.Visibility = Visibility.Visible;
                shiftRowButton.Visibility = Visibility.Visible;
                mixColButton.Visibility = Visibility.Visible;
                addKeyButton.Visibility = Visibility.Visible;
            }
        }
        public void hideButton()
        {
            autostepSpeedSlider.Visibility = Visibility.Hidden;
            nextStepButton.Visibility = Visibility.Hidden;
            playButton.Visibility = Visibility.Hidden;
            prevStepButton.Visibility = Visibility.Hidden;
            pauseButton.Visibility = Visibility.Hidden;
            round1Button.Visibility = Visibility.Hidden;
            round2Button.Visibility = Visibility.Hidden;
            round3Button.Visibility = Visibility.Hidden;
            round4Button.Visibility = Visibility.Hidden;
            round5Button.Visibility = Visibility.Hidden;
            round6Button.Visibility = Visibility.Hidden;
            round7Button.Visibility = Visibility.Hidden;
            round8Button.Visibility = Visibility.Hidden;
            round9Button.Visibility = Visibility.Hidden;
            round10Button.Visibility = Visibility.Hidden;
            keyButton.Visibility = Visibility.Hidden; 
        }
        public void showButton()
        {
            autostepSpeedSlider.Visibility = Visibility.Visible;
            nextStepButton.Visibility = Visibility.Visible;
            playButton.Visibility = Visibility.Visible;
            prevStepButton.Visibility = Visibility.Visible;
            pauseButton.Visibility = Visibility.Visible;
            round1Button.Visibility = Visibility.Visible;
            round2Button.Visibility = Visibility.Visible;
            round3Button.Visibility = Visibility.Visible;
            round4Button.Visibility = Visibility.Visible;
            round5Button.Visibility = Visibility.Visible;
            round6Button.Visibility = Visibility.Visible;
            round7Button.Visibility = Visibility.Visible;
            round8Button.Visibility = Visibility.Visible;
            round9Button.Visibility = Visibility.Visible;
            round10Button.Visibility = Visibility.Visible;
            keyButton.Visibility = Visibility.Visible;
        }
        #endregion
    }
}
