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
        public bool expansion = true;
        public byte[][] roundConstant;
        public int keysize;
        public double progress;
        public int shift = 0;
        public byte[] keyBytes;
        List<int> markedList = new List<int>();
        List<int[]> markedPositions = new List<int[]>();
        public Brush brush = Brushes.Green;
        public bool skip = false;
        private bool finish = false;
        public int language;
        Thread expansionThread;
        Thread encryptionThread;

        public AESPresentation()
        {
            InitializeComponent();
            buttonNextClickedEvent = new AutoResetEvent(false);
            autostep = false;
            autostepSpeedSlider.IsEnabled = true;
            keyButton.Content = "Skip Expansion";
            buttonVisible();
            hideButton();

            for (int x = 0; x < 18; x++)
            {
                List<TextBlock> temp = new List<TextBlock>();
                temp = createTextBlockList(x);
                textBlockList.Add(temp);
            }
            for (int x = 0; x < 17; x++)
            {
                List<Border> temp = new List<Border>();
                temp = createBorderList(x);
                borderList.Add(temp);
            }
            sBoxList = createSBoxList();
            
        }

        #region Buttons
        private void round1Button_Click(object sender, RoutedEventArgs e)
        {
            autostep = false;
            if (expansion)
            {
                roundNumber = 1;
                cleanUp();
                skip = true;
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    setUpExpansion();
                }, null);
                return;
            }
            skip = true;
            cleanUp();
            removeColors();
            lightRemoveColor();
            round1Button.Background = Brushes.Aqua;
            subByteButton.Background = Brushes.Aqua;
            buttonVisible();
            roundNumber = 1 + shift * 2 * keysize;
            action = 1;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                setUpSubByte(states);
            }, null);
            if (keysize != 0)
            {
                shiftButtons(0);
            }
            mixColButton.IsEnabled = true;
        }

        private void round2Button_Click(object sender, RoutedEventArgs e)
        {
            autostep = false;
            if (expansion)
            {
                roundNumber = 2;
                skip = true;
                cleanUp();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    setUpExpansion();
                }, null);
                return;
            }
            skip = true;
            cleanUp();
            removeColors();
            lightRemoveColor();
            round2Button.Background = Brushes.Aqua;
            buttonVisible();
            roundNumber = 2 + shift * 2 * keysize;
            action = 1;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                setUpSubByte(states);
            }, null);
            if (keysize != 0)
            {
                shiftButtons(0);
            }
            mixColButton.IsEnabled = true;
        }

        private void round3Button_Click(object sender, RoutedEventArgs e)
        {
            autostep = false;
            if (expansion)
            {             
                roundNumber = 3;
                skip = true;
                cleanUp();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    setUpExpansion();
                }, null);
                return;
            }
            skip = true;
            cleanUp();
            removeColors();
            lightRemoveColor();
            round3Button.Background = Brushes.Aqua;
            buttonVisible();
            roundNumber = 3 + shift * 2 * keysize;
            action = 1;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                setUpSubByte(states);
            }, null);
            if (keysize != 0)
            {
                shiftButtons(0);
            }
            mixColButton.IsEnabled = true;
        }

        private void round4Button_Click(object sender, RoutedEventArgs e)
        {
            autostep = false;
            if (expansion)
            {              
                roundNumber = 4;
                skip = true;
                cleanUp();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    setUpExpansion();
                }, null);
                return;
            }
            skip = true;
            cleanUp();
            removeColors();
            lightRemoveColor();
            round4Button.Background = Brushes.Aqua;
            buttonVisible();
            roundNumber = 4 + shift * 2 * keysize;
            action = 1;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                setUpSubByte(states);
            }, null);
            if (keysize != 0)
            {
                shiftButtons(0);
            }
            mixColButton.IsEnabled = true;
        }

        private void round5Button_Click(object sender, RoutedEventArgs e)
        {
            autostep = false;
            if (expansion)
            {              
                roundNumber = 5;
                skip = true;
                cleanUp();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    setUpExpansion();
                }, null);
                return;
            }
            skip = true;
            cleanUp();
            removeColors();
            lightRemoveColor();
            round5Button.Background = Brushes.Aqua;
            buttonVisible();
            roundNumber = 5 + shift * 2 * keysize;
            action = 1;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                setUpSubByte(states);
            }, null);
            //if (keysize == 1 && shift == 0)
            //{
            //    shiftButtons(0);
            //}
            mixColButton.IsEnabled = true;
        }

        private void round6Button_Click(object sender, RoutedEventArgs e)
        {
            autostep = false;
            if (expansion)
            {              
                roundNumber = 6;
                skip = true;
                cleanUp();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    setUpExpansion();
                }, null);
                return;
            }
            skip = true;
            cleanUp();
            removeColors();
            lightRemoveColor();
            round6Button.Background = Brushes.Aqua;
            buttonVisible();
            roundNumber = 6 + shift * 2 * keysize;
            action = 1;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                setUpSubByte(states);
            }, null);
            //if (keysize == 1)
            //{
            //    shiftButtons(0);
            //}
            mixColButton.IsEnabled = true;
        }

        private void round7Button_Click(object sender, RoutedEventArgs e)
        {
            autostep = false;
            if (expansion)
            {             
                roundNumber = 7;
                skip = true;
                cleanUp();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    setUpExpansion();
                }, null);
                return;
            }
            skip = true;
            cleanUp();
            removeColors();
            lightRemoveColor();
            round7Button.Background = Brushes.Aqua;
            buttonVisible();
            roundNumber = 7 + shift * 2 * keysize;
            action = 1;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                setUpSubByte(states);
            }, null);
            if (keysize != 0)
            { 
                shiftButtons(1);
            }
            mixColButton.IsEnabled = true;
        }

        private void round8Button_Click(object sender, RoutedEventArgs e)
        {
            autostep = false;
            if (expansion)
            {               
                roundNumber = 8;
                skip = true;
                cleanUp();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    setUpExpansion();
                }, null);
                return;
            }
            skip = true;
            cleanUp();
            removeColors();
            lightRemoveColor();
            round8Button.Background = Brushes.Aqua;
            buttonVisible();
            roundNumber = 8 + shift * 2 * keysize;
            action = 1;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                setUpSubByte(states);
            }, null);
            if (keysize != 0)
            {
                shiftButtons(1);
            }
            mixColButton.IsEnabled = true;
        }

        private void round9Button_Click(object sender, RoutedEventArgs e)
        {
            autostep = false;
            if (expansion)
            {               
                roundNumber = 9;
                skip = true;
                cleanUp();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    setUpExpansion();
                }, null);
                return;
            }
            skip = true;
            cleanUp();
            removeColors();
            lightRemoveColor();
            round9Button.Background = Brushes.Aqua;
            buttonVisible();
            roundNumber = 9 + shift * 2 * keysize;
            action = 1;
            setUpSubByte(states);
            if (keysize != 0)
            {
                shiftButtons(1);
            }
            mixColButton.IsEnabled = true;
        }

        private void round10Button_Click(object sender, RoutedEventArgs e)
        {
            autostep = false;
            if (expansion)
            {              
                roundNumber = 10;
                skip = true;
                cleanUp();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    setUpExpansion();
                }, null);
                return;
            }
            skip = true;
            cleanUp();
            removeColors();
            lightRemoveColor();
            round10Button.Background = Brushes.Aqua;
            buttonVisible();
            roundNumber = 10 + shift * 2 * keysize;
            action = 1;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                setUpSubByte(states);
            }, null);
            if (keysize != 0)
            {
                shiftButtons(1);
            }
            mixColButton.IsEnabled = false;
        }

        private void keyButton_Click(object sender, RoutedEventArgs e)
        {
            autostep = false;
            cleanUp();
            expansion = !expansion;
            buttonNextClickedEvent.Set();
            skip = true;

        }

        private void subByteButton_Click(object sender, RoutedEventArgs e)
        {
            autostep = false;
            if (roundNumber == 0)
            {
                return;
            }
            skip = true;
            cleanUp();
            lightRemoveColor();
            action = 1;
            setUpSubByte(states);
        }

        private void shiftRowButton_Click(object sender, RoutedEventArgs e)
        {
            autostep = false;
            if (roundNumber == 0)
            {
                return;
            }
            skip = true;
            cleanUp();
            lightRemoveColor();
            action = 2;
            setUpShiftRows();
        }

        private void mixColButton_Click(object sender, RoutedEventArgs e)
        {
            autostep = false;
            if (roundNumber == 0)
            {
                return;
            }
            skip = true;
            cleanUp();
            lightRemoveColor();
            action = 3;
            setUpMixColumns();
        }

        private void addKeyButton_Click(object sender, RoutedEventArgs e)
        {
            autostep = false;
            if (roundNumber == 0)
            {
                return;
            }
            skip = true;
            cleanUp();
            lightRemoveColor();
            action = 4;
            setUpAddKey();
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            disableButtons();
            buttonNextClickedEvent.Set();      
            if(keysize == 2 && expansion)
            {
                enableButtons();
            }
            enableButtons();
        }

        private void prevStepButton_Click(object sender, RoutedEventArgs e)
        {
            autostep = false;
            if (expansion)
            {
                if(roundNumber > 1)
                {
                    roundNumber--;
                    skip = true;
                    cleanUp();
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
            skip = true;
            cleanUp();
            switch (action)
            {
                case 1:                    
                    roundNumber--;
                    if(roundNumber < 7 && keysize == 1)
                    {
                        if (keysize == 1)
                        {
                            shiftButtons(0);
                        }
                    }
                    action = 4;
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        setUpAddKey();
                        changeRoundButton();
                    }, null);
                    if(roundNumber == 9+ 2 * keysize)
                    {
                        mixColButton.IsEnabled = true;
                    }
                    encryptionProgress();
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
            autostep = false;
            if (expansion)
            {
                if (roundNumber < 10 && keysize == 0)
                {
                    roundNumber++;
                    skip = true;
                    cleanUp();
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        setUpExpansion();
                    }, null);
                    return;
                }
                else if(roundNumber < 8 && keysize == 1)
                {
                    roundNumber++;
                    skip = true;
                    cleanUp();
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        setUpExpansion();
                    }, null);
                    return;
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        setUpExpansion();
                    }, null);
                    return;
                }
                else if(roundNumber < 7 && keysize == 2)
                {
                    roundNumber++;
                    if (keysize == 2)
                    {
                        skip = true;
                        cleanUp();
                        Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            setUpExpansion();
                        }, null);
                        return;
                    }
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
            skip = true;
            cleanUp();
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
                    if(roundNumber > 6 && keysize == 1)
                    {
                        if (keysize == 1)
                        {
                            shiftButtons(1);
                        }
                    }
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
            if(keysize == 2 && expansion)
            {
                enableButtons();
            }
            enableButtons();
            autostep = !autostep;
            if(autostep)
            {
                buttonNextClickedEvent.Set();
            }               
        }

        private void shiftLeftButton_Click(object sender, RoutedEventArgs e)
        {
            shiftButtons(0);
        }

        private void shiftRightButton_Click(object sender, RoutedEventArgs e)
        {
            shiftButtons(1);
        }

        private void autostepSpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            autostepSpeed = 50 + 100 * (10 - (int)autostepSpeedSlider.Value);
        }

        private void endButton_Click(object sender, RoutedEventArgs e)
        {
            autostep = false;
            cleanUp();
            expansion = !expansion;
            buttonNextClickedEvent.Set();
            //roundNumber = 10 + keysize * 2;
            //action = 4;
            //Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            //{
            //    setUpAddKey();
            //}, null);
            skip = true;
            finish = true;
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (!expansion)
            {
                autostep = false;
                cleanUp();
                expansion = !expansion;
                buttonNextClickedEvent.Set();
                skip = true;
                roundNumber = 1;
            }
            else
            {
                autostep = false;
                roundNumber = 1;
                cleanUp();
                skip = true;
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    setUpExpansion();
                }, null);
            }
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
            while (roundNumber < 11 + 2 * keysize)
            {
                while (action < 5)
                {
                    switch (action)
                    {
                        case 1:
                            encryptionThread = new Thread(subBytes);
                            skip = false;
                            encryptionThread.Start();
                            while (encryptionThread.IsAlive)
                            {
                                if (skip)
                                {
                                    encryptionThread.Abort();                                  
                                }
                            }
                            //subBytes();
                            if (!skip)
                            {
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
                            }
                            skip = false;
                            if (expansion)
                            {
                                return;
                            }
                            break;
                        case 2:
                            encryptionThread = new Thread(shiftRow);
                            encryptionThread.Start();
                            while (encryptionThread.IsAlive)
                            {
                                if (skip)
                                {
                                    encryptionThread.Abort();
                                }
                            }
                            //shiftRow();
                            if (!skip)
                            {
                                autostep = false;
                                wait();
                                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                {
                                    shiftRowGrid.Visibility = Visibility.Hidden;
                                    lightRemoveColor();
                                    resetShiftRow();
                                }, null);
                                if (roundNumber < 10 + keysize * 2)
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
                            }
                            skip = false;                       
                            if (expansion)
                            {
                                return;
                            }
                            break;
                        case 3:
                            encryptionThread = new Thread(mixColumns);
                            encryptionThread.Start();
                            while (encryptionThread.IsAlive)
                            {
                                if (skip)
                                {
                                    encryptionThread.Abort();
                                }
                            }
                            //mixColumns();
                            if (!skip)
                            {
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
                            }
                            skip = false;
                            if (expansion)
                            {
                                return;
                            }
                            break;
                        case 4:
                            encryptionThread = new Thread(addKey);
                            encryptionThread.Start();
                            while (encryptionThread.IsAlive)
                            {
                                if (skip)
                                {
                                    encryptionThread.Abort();
                                }
                            }
                            //addKey();
                            if (!skip)
                            {
                                autostep = false;
                                wait();
                                if (roundNumber < 10 + keysize * 2)
                                {
                                    action = 1;
                                    roundNumber++;
                                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                    {
                                        if (roundNumber > 6 && keysize == 1)
                                        {
                                            if (keysize == 1)
                                            {
                                                shiftButtons(1);
                                            }
                                        }
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
                            }
                            skip = false;
                            if (expansion)
                            {
                                return;
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
                    if (shift == 0)
                    {
                        round1Button.Background = Brushes.Aqua;
                    }
                    break;
                case 2:
                    removeColors();
                    if (shift == 0)
                    {
                        round2Button.Background = Brushes.Aqua;
                    }
                    break;
                case 3:
                    removeColors();
                    if(shift == 1 && keysize == 1)
                    {
                        round1Button.Background = Brushes.Aqua;
                        break;
                    }
                    round3Button.Background = Brushes.Aqua;
                    break;
                case 4:
                    removeColors();
                    if (shift == 1 && keysize == 1)
                    {
                        round2Button.Background = Brushes.Aqua;
                        break;
                    }
                    round4Button.Background = Brushes.Aqua;
                    break;
                case 5:
                    removeColors();
                    if (shift == 1 && keysize == 1)
                    {
                        round3Button.Background = Brushes.Aqua;
                        break;
                    }
                    else if (shift == 1 && keysize == 2)
                    {
                        round1Button.Background = Brushes.Aqua;
                        break;
                    }
                    round5Button.Background = Brushes.Aqua;
                    break;
                case 6:
                    removeColors();
                    if (shift == 1 && keysize == 1)
                    {
                        round4Button.Background = Brushes.Aqua;
                        break;
                    }
                    else if (shift == 1 && keysize == 2)
                    {
                        round2Button.Background = Brushes.Aqua;
                        break;
                    }
                    round6Button.Background = Brushes.Aqua;
                    break;
                case 7:
                    removeColors();
                    if (shift == 1 && keysize == 1)
                    {
                        round5Button.Background = Brushes.Aqua;
                        break;
                    }
                    else if (shift == 1 && keysize == 2)
                    {
                        round3Button.Background = Brushes.Aqua;
                        break;
                    }
                    round7Button.Background = Brushes.Aqua;
                    break;
                case 8:
                    removeColors();
                    if (shift == 1 && keysize == 1)
                    {
                        round6Button.Background = Brushes.Aqua;
                        break;
                    }
                    else if (shift == 1 && keysize == 2)
                    {
                        round4Button.Background = Brushes.Aqua;
                        break;
                    }
                    round8Button.Background = Brushes.Aqua;
                    break;
                case 9:
                    removeColors();
                    if (shift == 1 && keysize == 1)
                    {
                        round7Button.Background = Brushes.Aqua;
                        break;
                    }
                    else if (shift == 1 && keysize == 2)
                    {
                        round5Button.Background = Brushes.Aqua;
                        break;
                    }
                    round9Button.Background = Brushes.Aqua;
                    break;
                case 10:
                    removeColors();
                    if (shift == 1 && keysize == 1)
                    {
                        round8Button.Background = Brushes.Aqua;
                        break;
                    }
                    else if (shift == 1 && keysize == 2)
                    {
                        round6Button.Background = Brushes.Aqua;
                        break;
                    }
                    round10Button.Background = Brushes.Aqua;
                    break;
                case 11:
                    removeColors();
                    if (shift == 1 && keysize == 1)
                    {
                        round9Button.Background = Brushes.Aqua;
                        break;
                    }
                    else if (shift == 1 && keysize == 2)
                    {
                        round7Button.Background = Brushes.Aqua;
                        break;
                    }
                    break;
                case 12:
                    removeColors();
                    if (shift == 1 && keysize == 1)
                    {
                        round10Button.Background = Brushes.Aqua;
                        break;
                    }
                    else if (shift == 1 && keysize == 2)
                    {
                        round8Button.Background = Brushes.Aqua;
                        break;
                    }
                    break;
                case 13:
                    removeColors();
                    if (shift == 1 && keysize == 2)
                    {
                        round9Button.Background = Brushes.Aqua;
                        break;
                    }
                    break;
                case 14:
                    removeColors();
                    if (shift == 1 && keysize == 2)
                    {
                        round10Button.Background = Brushes.Aqua;
                        break;
                    }
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
                case 13:
                    x = 17;
                    temp = "expansionKeyBlock";
                    while(x < 41)
                    {
                        string y = temp + x;
                        list.Add((TextBlock)this.FindName(y));
                        x++;
                    }
                    break;
                case 14:
                    x = 17;
                    temp = "expansionResultBlock";
                    while (x < 41)
                    {
                        string y = temp + x;
                        list.Add((TextBlock)this.FindName(y));
                        x++;
                    }
                    break;
                case 15:
                    x = 41;
                    temp = "expansionKeyBlock";
                    while (x < 73)
                    {
                        string y = temp + x;
                        list.Add((TextBlock)this.FindName(y));
                        x++;
                    }
                    break;
                case 16:
                    x = 41;
                    temp = "expansionResultBlock";
                    while (x < 73)
                    {
                        string y = temp + x;
                        list.Add((TextBlock)this.FindName(y));
                        x++;
                    }
                    break;
                case 17:
                    x = 1;
                    temp = "expansionTransition2Block";
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
                case 12:
                    x = 17;
                    border = "expansionKeyBorder";
                    while (x < 41)
                    {
                        string z = border + x;
                        temp.Add((Border)this.FindName(z));
                        x++;
                    }
                    break;
                case 13:
                    x = 17;
                    border = "expansionResultBorder";
                    while (x < 41)
                    {
                        string z = border + x;
                        temp.Add((Border)this.FindName(z));
                        x++;
                    }
                    break;
                case 14:
                    x = 41;
                    border = "expansionKeyBorder";
                    while (x < 73)
                    {
                        string z = border + x;
                        temp.Add((Border)this.FindName(z));
                        x++;
                    }
                    break;
                case 15:
                    x = 41;
                    border = "expansionResultBorder";
                    while (x < 73)
                    {
                        string z = border + x;
                        temp.Add((Border)this.FindName(z));
                        x++;
                    }
                    break;
                case 16:
                    x = 1;
                    border = "expansionTransition2Border";
                    while (x < 13)
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

        public void invisible()
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
            shiftRowExplanation.Visibility = Visibility.Hidden;
            addKeyExplanation.Visibility = Visibility.Hidden;
            subByteExplanation.Visibility = Visibility.Hidden;
            subByteExplanation1.Visibility = Visibility.Hidden;
            mixColExplanation.Visibility = Visibility.Hidden;
            mixColExplanation1.Visibility = Visibility.Hidden;
            expansionKeyGrid.Visibility = Visibility.Hidden;
            expansionKeyGrid192.Visibility = Visibility.Hidden;
            expansionKeyGrid256.Visibility = Visibility.Hidden;
            expansionTransitionGrid.Visibility = Visibility.Hidden;
            expansionTransitionGrid1.Visibility = Visibility.Hidden;
            expansionTransitionGrid2.Visibility = Visibility.Hidden;
            expansionResultGrid256.Visibility = Visibility.Hidden;
            expansionResultGrid192.Visibility = Visibility.Hidden;
            expansionResultGrid.Visibility = Visibility.Hidden;
        }

        public void updateUI()
        {

            DispatcherFrame frame = new DispatcherFrame();

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(delegate (object parameter)

            {

                frame.Continue = false;

                return null;

            }), null);

            Dispatcher.PushFrame(frame);

        }

        public void createSBox()
        {
            int x = 0;
            while (x < 16)
            {
                this.sBox[x] = new byte[16];
                x++;
            }
            x = 0;
            byte[] temp = {99, 124, 119, 123, 242, 107, 111, 197, 48, 1, 103, 43, 254, 215, 171, 118, 202, 130, 201, 125, 250, 89, 71, 240, 173, 212, 162, 175, 156, 164, 114, 192, 183, 253, 147, 38, 54, 63, 247, 204, 52, 165, 229, 241, 113, 216, 49, 21, 4, 199, 35, 195, 24, 150, 5, 154, 7, 18, 128, 226, 235, 39, 178, 117, 9, 131, 44, 26, 27, 110, 90, 160, 82, 59, 214, 179, 41, 227, 47, 132, 83, 209, 0, 237, 32, 252, 177, 91, 106, 203, 190, 57, 74, 76, 88, 207, 208, 239, 170, 251, 67, 77, 51, 133, 69, 249, 2, 127, 80, 60, 159, 168, 81, 163, 64, 143, 146, 157, 56, 245, 188, 182, 218, 33, 16, 255, 243, 210, 205, 12, 19, 236, 95, 151, 68, 23, 196, 167, 126, 61, 100, 93, 25, 115, 96, 129, 79, 220, 34, 42, 144, 136, 70, 238, 184, 20, 222, 94, 11, 219, 224, 50, 58, 10, 73, 6, 36, 92, 194, 211, 172, 98, 145, 149, 228, 121, 231, 200, 55, 109, 141, 213, 78, 169, 108, 86, 244, 234, 101, 122, 174, 8, 186, 120, 37, 46, 28, 166, 180, 198, 232, 221, 116, 31, 75, 189, 139, 138, 112, 62, 181, 102, 72, 3, 246, 14, 97, 53, 87, 185, 134, 193, 29, 158, 225, 248, 152, 17, 105, 217, 142, 148, 155, 30, 135, 233, 206, 85, 40, 223, 140, 161, 137, 13, 191, 230, 66, 104, 65, 153, 45, 15, 176, 84, 187, 22};
            x = 0;
            int y = 0;
            foreach(byte b in temp)
            {
                sBox[y][x] = b;
                x++;
                if(x == 16)
                {
                    y++;
                    x = 0;
                }
            }
            List<TextBlock> blockList = null;
            this.Dispatcher.Invoke((Action)(() =>
            {
                blockList = createTextBlockList(3);
            }));
            x = 0;
            y = 0;
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
                    subByteExplanation1.Visibility = Visibility.Hidden;
                    subByteExplanation.Visibility = Visibility.Visible;
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
                    subByteExplanation.Visibility = Visibility.Hidden;
                    subByteExplanation1.Visibility = Visibility.Visible;
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
            subByteExplanation.Visibility = Visibility.Visible;
            List<TextBlock> temp = createTextBlockList(4);
            byte[] state = arrangeText(states[(roundNumber - 1) * 4 + action - 1]);
            int x = 0;
            foreach (TextBlock tb in temp)
            {
                tb.Text =state[x].ToString("X2");
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
            encryptionProgress();     
            
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
                if (roundNumber < 10 + keysize * 2)
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
            rowSetBlockText(arrangeText(states[(roundNumber - 1) * 4 + action - 1]));
            shiftRowGrid.Visibility = Visibility.Visible;
            shiftRowButton.Background = Brushes.Aqua;
            shiftRowExplanation.Visibility = Visibility.Visible;
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
            mixColExplanation.Visibility = Visibility.Visible;
            mixColButton.Background = Brushes.Aqua;
            setMixStateTransition(arrangeText(states[(roundNumber - 1) * 4 + action - 1]));
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
                mixColExplanation.Visibility = Visibility.Visible;
                mixColExplanation1.Visibility = Visibility.Hidden;
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
                mixColExplanation.Visibility = Visibility.Hidden;
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
                mixColExplanation1.Visibility = Visibility.Visible;
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
            addKeyExplanation.Visibility = Visibility.Visible;
            if(roundNumber == 0)
            {
                keySetBlockText(arrangeText(tempState), key);
            }
            else if (roundNumber == 10 + keysize * 2)
            {
                keySetBlockText(arrangeText(states[(roundNumber - 1) * 4 + action - 2]), key);
            }
            else
            {
                keySetBlockText(arrangeText(states[(roundNumber - 1) * 4 + action - 1]), key);
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
            keyTextBlock18.Text = keyList[roundNumber][4].ToString("X2");
            keyTextBlock19.Text = keyList[roundNumber][8].ToString("X2");
            keyTextBlock20.Text = keyList[roundNumber][12].ToString("X2");
            keyTextBlock21.Text = keyList[roundNumber][1].ToString("X2");
            keyTextBlock22.Text = keyList[roundNumber][5].ToString("X2");
            keyTextBlock23.Text = keyList[roundNumber][9].ToString("X2");
            keyTextBlock24.Text = keyList[roundNumber][13].ToString("X2");
            keyTextBlock25.Text = keyList[roundNumber][2].ToString("X2");
            keyTextBlock26.Text = keyList[roundNumber][6].ToString("X2");
            keyTextBlock27.Text = keyList[roundNumber][10].ToString("X2");
            keyTextBlock28.Text = keyList[roundNumber][14].ToString("X2");
            keyTextBlock29.Text = keyList[roundNumber][3].ToString("X2");
            keyTextBlock30.Text = keyList[roundNumber][7].ToString("X2");
            keyTextBlock31.Text = keyList[roundNumber][11].ToString("X2");
            keyTextBlock32.Text = keyList[roundNumber][15].ToString("X2");
        }

        private void addKey()
        {
            byte[] result;
            if(roundNumber == 0)
            {
                result = arrangeText(states[0]);
            }
            else if (roundNumber == 10 + keysize * 2)
            {
                result = arrangeText(states[(roundNumber - 1) * 4 + action - 1]);
            }
            else
            {
                result = arrangeText(states[(roundNumber - 1) * 4 + action]);
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

        public void keyExpansion256()
        {
            List<Border> keyBorders = borderList[14];
            List<Border> transitionBorders = borderList[9];
            List<Border> transition1Borders = borderList[10];
            List<Border> resultBorders = borderList[15];
            List<Border> transition2Borders = borderList[16];
            List<TextBlock> keyBlocks = textBlockList[15];
            List<TextBlock> transitionBlocks = textBlockList[10];
            List<TextBlock> transition1Blocks = textBlockList[11];
            List<TextBlock> resultBlocks = textBlockList[16];
            List<TextBlock> transition2Blocks = textBlockList[17];
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
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                transition2Blocks[1].Text = keyBlocks[28].Text;
                transition2Blocks[4].Text = keyBlocks[29].Text;
                transition2Blocks[7].Text = keyBlocks[30].Text;
                transition2Blocks[10].Text = keyBlocks[31].Text;
            }, null);
            markBorders2(14, new int[] { 28, 29, 30, 31 });
            wait();
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                expansionTransitionGrid2.Visibility = Visibility.Visible;
            }, null);
            unmarkBorders2(14, new int[] { 28, 29, 30, 31 });
            markBorders2(16, new int[] { 1, 4, 7, 10 });
            wait();
            unmarkBorders2(16, new int[] { 1, 4, 7, 10 });
            wait();
            markBorders2(16, new int[] { 1 });
            wait();
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                transition2Blocks[1].Text = "";
            }, null);
            unmarkBorders2(16, new int[] { 1 });
            wait();
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                transition2Blocks[1].Text = keyBlocks[29].Text;
                transition2Blocks[4].Text = keyBlocks[30].Text;
                transition2Blocks[7].Text = keyBlocks[31].Text;
                transition2Blocks[10].Text = "";
            }, null);
            wait();
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                transition2Blocks[10].Text = keyBlocks[28].Text;
            }, null);
            markBorders2(16, new int[] { 10 });
            wait();
            unmarkBorders2(16, new int[] { 10 });
            wait();        
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                sBoxGrid.Visibility = Visibility.Visible;
                expansionTransitionGrid1.Visibility = Visibility.Visible;
            }, null);
            wait();
            for (int r = 0; r < 4; r++)
            {
                markBorders2(16, new int[] { 1 + 3 * r });
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    transition2Borders[1 + 3 * r].Visibility = Visibility.Hidden;
                    transition2Borders[3 * r].Visibility = Visibility.Visible;
                    transition2Borders[2 + 3 * r].Visibility = Visibility.Visible;
                    transition2Blocks[3 * r].Text = transition2Blocks[3 * r + 1].Text.Substring(0, 1);
                    transition2Blocks[3 * r + 2].Text = transition2Blocks[3 * r + 1].Text.Substring(1, 1);
                }, null);
                unmarkBorders2(16, new int[] { 1 + 3 * r });
                wait();
                markBorders2(16, new int[] { 3 * r });
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    switch (transition2Blocks[3 * r].Text)
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
                    unmarkBorders2(16, new int[] { 3 * r });
                }, null);
                wait();
                markBorders2(16, new int[] { 2 + 3 * r });
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    switch (transition2Blocks[3 * r + 2].Text)
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
                    unmarkBorders2(16, new int[] { 2 + 3 * r });
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
                    transition2Borders[3 * r].Visibility = Visibility.Hidden;
                    transition2Borders[2 + 3 * r].Visibility = Visibility.Hidden;
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
                    transition2Blocks[3 * r].Text = "";
                    transition2Blocks[3 * r + 2].Text = "";
                    textBlockList[3][z].Background = Brushes.Transparent;
                }, null);
                wait();
            }
            byte[] constant = roundConstant[roundNumber - 1];
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                transition1Borders[0].Visibility = Visibility.Visible;
                transition1Borders[3].Visibility = Visibility.Visible;
                transition1Borders[6].Visibility = Visibility.Visible;
                transition1Borders[9].Visibility = Visibility.Visible;
                transition1Blocks[0].Text = constant[0].ToString("X2");
                transition1Blocks[3].Text = constant[1].ToString("X2");
                transition1Blocks[6].Text = constant[2].ToString("X2");
                transition1Blocks[9].Text = constant[3].ToString("X2");
                expansionResultGrid256.Visibility = Visibility.Visible;
                sBoxGrid.Visibility = Visibility.Hidden;
            }, null);
            wait();
            for (z = 0; z < 4; z++)
            {
                markBorders2(10, new int[] { z * 3 });
                wait();
                markBorders2(10, new int[] { z * 3 + 2 });
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    unmarkBorders2(10, new int[] { z * 3, z * 3 + 2 });
                    markBorders2(15, new int[] { z });
                    byte[] l = StringToByteArray(transition1Blocks[z * 3].Text);
                    byte m = StringToByteArray(transition1Blocks[z * 3 + 2].Text)[0];
                    byte n = (byte)(l[0] ^ m);
                    resultBlocks[z].Text = n.ToString("X2");
                }, null);
                wait();
                unmarkBorders2(15, new int[] { z });
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
            }, null);
            wait();
            markBorders2(15, new int[] { 0, 1, 2, 3 });
            wait();
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                tempString[0] = resultBlocks[0].Text;
                tempString[1] = resultBlocks[1].Text;
                tempString[2] = resultBlocks[2].Text;
                tempString[3] = resultBlocks[3].Text;
                resultBlocks[0].Text = "";
                resultBlocks[1].Text = "";
                resultBlocks[2].Text = "";
                resultBlocks[3].Text = "";
            }, null);
            unmarkBorders2(15, new int[] { 0, 1, 2, 3 });
            wait();
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                transition1Blocks[2].Text = tempString[0];
                transition1Blocks[5].Text = tempString[1];
                transition1Blocks[8].Text = tempString[2];
                transition1Blocks[11].Text = tempString[3];
            }, null);
            markBorders2(10, new int[] { 2, 5, 8, 11 });
            wait();
            unmarkBorders2(10, new int[] { 2, 5, 8, 11 });
            markBorders2(14, new int[] { 0, 1, 2, 3 });
            wait();
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                transition1Blocks[0].Text = keyBlocks[0].Text;
                transition1Blocks[3].Text = keyBlocks[1].Text;
                transition1Blocks[6].Text = keyBlocks[2].Text;
                transition1Blocks[9].Text = keyBlocks[3].Text;
            }, null);
            markBorders2(10, new int[] { 0, 3, 6, 9 });
            unmarkBorders2(14, new int[] { 0, 1, 2, 3 });
            wait();
            unmarkBorders2(10, new int[] { 0, 3, 6, 9 });
            wait();
            x = 0;
            while (x < 4)
            {
                for (z = 0; z < 4; z++)
                {
                    markBorders2(10, new int[] { z * 3 });
                    wait();
                    markBorders2(10, new int[] { z * 3 + 2 });
                    wait();
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        unmarkBorders2(10, new int[] { z * 3, z * 3 + 2 });
                        markBorders2(15, new int[] { z + 4 * x });
                        if (x == 4)
                        {
                            resultBlocks[z + x * 4].Text = ((Byte)(keyBytes[(roundNumber) * 32 + (x - 1) * 4 + z] ^ keyBytes[(roundNumber) * 32 - 16 + z])).ToString("X2");
                        }
                        else
                        {
                            resultBlocks[z + x * 4].Text = keyBytes[(roundNumber) * 32 + x * 4 + z].ToString("X2");
                        }
                    }, null);
                    wait();
                    unmarkBorders2(15, new int[] { z + 4 * x });
                    wait();
                }
                if (x < 3)
                {
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
                    }, null);
                    wait();
                    markBorders2(15, new int[] { 0 + 4 * x, 1 + 4 * x, 2 + 4 * x, 3 + 4 * x });
                    wait();
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        tempString[0] = resultBlocks[0 + 4 * x].Text;
                        tempString[1] = resultBlocks[1 + 4 * x].Text;
                        tempString[2] = resultBlocks[2 + 4 * x].Text;
                        tempString[3] = resultBlocks[3 + 4 * x].Text;
                    }, null);
                    unmarkBorders2(15, new int[] { 0 + 4 * x, 1 + 4 * x, 2 + 4 * x, 3 + 4 * x });
                    wait();
                    markBorders2(10, new int[] { 2, 5, 8, 11 });
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        transition1Blocks[2].Text = tempString[0];
                        transition1Blocks[5].Text = tempString[1];
                        transition1Blocks[8].Text = tempString[2];
                        transition1Blocks[11].Text = tempString[3];
                    }, null);
                    wait();
                    unmarkBorders2(10, new int[] { 2, 5, 8, 11 });
                    markBorders2(14, new int[] { 4 + 4 * x, 5 + 4 * x, 6 + 4 * x, 7 + 4 * x });
                    wait();
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        transition1Blocks[0].Text = keyBlocks[4 + 4 * x].Text;
                        transition1Blocks[3].Text = keyBlocks[5 + 4 * x].Text;
                        transition1Blocks[6].Text = keyBlocks[6 + 4 * x].Text;
                        transition1Blocks[9].Text = keyBlocks[7 + 4 * x].Text;
                    }, null);
                    markBorders2(10, new int[] { 0, 3, 6, 9 });
                    unmarkBorders2(14, new int[] { 4 + 4 * x, 5 + 4 * x, 6 + 4 * x, 7 + 4 * x });
                    wait();
                    unmarkBorders2(10, new int[] { 0, 3, 6, 9 });
                    wait();
                }
                x++;
            }
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                transitionBlocks[1].Text = resultBlocks[12].Text;
                transitionBlocks[4].Text = resultBlocks[13].Text;
                transitionBlocks[7].Text = resultBlocks[14].Text;
                transitionBlocks[10].Text = resultBlocks[15].Text;
                markBorders(new List<Border> { resultBorders[12], resultBorders[13], resultBorders[14], resultBorders[15] });
            }, null);
            wait();
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                expansionTransitionGrid.Visibility = Visibility.Visible;
                expansionKeyGrid256.Visibility = Visibility.Hidden;
                unmarkBorders(new List<Border> { resultBorders[12], resultBorders[13], resultBorders[14], resultBorders[15] });
                markBorders(new List<Border> { transitionBorders[1], transitionBorders[4], transitionBorders[7], transitionBorders[10] });
            }, null);
            wait();
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                unmarkBorders(new List<Border> { transitionBorders[1], transitionBorders[4], transitionBorders[7], transitionBorders[10] });
                sBoxGrid.Visibility = Visibility.Visible;
                expansionResultGrid256.SetValue(Grid.ColumnProperty, 2);
                expansionTransitionGrid1.Visibility = Visibility.Visible;
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
                    switch (transitionBlocks[3 * r].Text)
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
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                expansionTransitionGrid.Visibility = Visibility.Hidden;
                expansionKeyGrid256.Visibility = Visibility.Visible;
                expansionResultGrid256.SetValue(Grid.ColumnProperty, 3);
                expansionTransitionGrid1.Visibility = Visibility.Visible;
                sBoxGrid.Visibility = Visibility.Hidden;
            }, null);
            wait();
            x = 3;
            while (x < 8)
            {
                if (x == 3)
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        keyBorders[4 + 4 * x].Background = Brushes.Green;
                        keyBorders[5 + 4 * x].Background = Brushes.Green;
                        keyBorders[6 + 4 * x].Background = Brushes.Green;
                        keyBorders[7 + 4 * x].Background = Brushes.Green;
                    }, null);
                    wait();
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        transition1Borders[0].Background = Brushes.Green;
                        transition1Borders[3].Background = Brushes.Green;
                        transition1Borders[6].Background = Brushes.Green;
                        transition1Borders[9].Background = Brushes.Green;
                        transition1Blocks[0].Text = keyBlocks[4 + 4 * x].Text;
                        transition1Blocks[3].Text = keyBlocks[5 + 4 * x].Text;
                        transition1Blocks[6].Text = keyBlocks[6 + 4 * x].Text;
                        transition1Blocks[9].Text = keyBlocks[7 + 4 * x].Text;
                        keyBorders[4 + 4 * x].Background = Brushes.Transparent;
                        keyBorders[5 + 4 * x].Background = Brushes.Transparent;
                        keyBorders[6 + 4 * x].Background = Brushes.Transparent;
                        keyBorders[7 + 4 * x].Background = Brushes.Transparent;
                    }, null);
                    wait();
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        transition1Borders[0].Background = Brushes.Transparent;
                        transition1Borders[3].Background = Brushes.Transparent;
                        transition1Borders[6].Background = Brushes.Transparent;
                        transition1Borders[9].Background = Brushes.Transparent;
                    }, null);
                    x = 4;
                    wait();
                }
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
                        resultBorders[z + 4 * x].Background = Brushes.Green;
                        resultBlocks[z + x * 4].Text = keyBytes[(roundNumber) * 32 + x * 4 + z].ToString("X2");
                    }, null);
                    wait();
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        resultBorders[z + 4 * x].Background = Brushes.Transparent;
                    }, null);
                    wait();
                }
                if (x < 7)
                {
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
                    }, null);
                    wait();
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        resultBorders[0 + 4 * x].Background = Brushes.Green;
                        resultBorders[1 + 4 * x].Background = Brushes.Green;
                        resultBorders[2 + 4 * x].Background = Brushes.Green;
                        resultBorders[3 + 4 * x].Background = Brushes.Green;
                    }, null);
                    wait();
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        tempString[0] = resultBlocks[0 + 4 * x].Text;
                        tempString[1] = resultBlocks[1 + 4 * x].Text;
                        tempString[2] = resultBlocks[2 + 4 * x].Text;
                        tempString[3] = resultBlocks[3 + 4 * x].Text;
                        resultBorders[0 + 4 * x].Background = Brushes.Transparent;
                        resultBorders[1 + 4 * x].Background = Brushes.Transparent;
                        resultBorders[2 + 4 * x].Background = Brushes.Transparent;
                        resultBorders[3 + 4 * x].Background = Brushes.Transparent;
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
                        keyBorders[4 + 4 * x].Background = Brushes.Green;
                        keyBorders[5 + 4 * x].Background = Brushes.Green;
                        keyBorders[6 + 4 * x].Background = Brushes.Green;
                        keyBorders[7 + 4 * x].Background = Brushes.Green;
                    }, null);
                    wait();
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        transition1Borders[0].Background = Brushes.Green;
                        transition1Borders[3].Background = Brushes.Green;
                        transition1Borders[6].Background = Brushes.Green;
                        transition1Borders[9].Background = Brushes.Green;
                        transition1Blocks[0].Text = keyBlocks[4 + 4 * x].Text;
                        transition1Blocks[3].Text = keyBlocks[5 + 4 * x].Text;
                        transition1Blocks[6].Text = keyBlocks[6 + 4 * x].Text;
                        transition1Blocks[9].Text = keyBlocks[7 + 4 * x].Text;
                        keyBorders[4 + 4 * x].Background = Brushes.Transparent;
                        keyBorders[5 + 4 * x].Background = Brushes.Transparent;
                        keyBorders[6 + 4 * x].Background = Brushes.Transparent;
                        keyBorders[7 + 4 * x].Background = Brushes.Transparent;
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
                foreach (TextBlock tb in resultBlocks)
                {
                    tb.Text = "";
                }
                expansionResultGrid256.Visibility = Visibility.Hidden;
                transition1Borders[0].Visibility = Visibility.Hidden;
                transition1Borders[3].Visibility = Visibility.Hidden;
                transition1Borders[6].Visibility = Visibility.Hidden;
                transition1Borders[9].Visibility = Visibility.Hidden;
                transition2Borders[1].Visibility = Visibility.Visible;
                transition2Borders[4].Visibility = Visibility.Visible;
                transition2Borders[7].Visibility = Visibility.Visible;
                transition2Borders[10].Visibility = Visibility.Visible;
                expansionTransitionGrid2.Visibility = Visibility.Hidden;
                expansionTransitionGrid1.Visibility = Visibility.Hidden;
            }, null);
            x = 0;
            roundNumber++;
        }

        public void keyExpansion192()
        {
            List<Border> keyBorders = borderList[12];
            List<Border> transitionBorders = borderList[9];
            List<Border> transition1Borders = borderList[10];
            List<Border> resultBorders = borderList[13];
            List<TextBlock> keyBlocks = textBlockList[13];
            List<TextBlock> transitionBlocks = textBlockList[10];
            List<TextBlock> transition1Blocks = textBlockList[11];
            List<TextBlock> resultBlocks = textBlockList[14];
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
           Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                transitionBlocks[1].Text = keyBlocks[20].Text;
                transitionBlocks[4].Text = keyBlocks[21].Text;
                transitionBlocks[7].Text = keyBlocks[22].Text;
                transitionBlocks[10].Text = keyBlocks[23].Text;               
                markBorders(new List<Border> { keyBorders[20], keyBorders[21], keyBorders[22], keyBorders[23] });
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    expansionTransitionGrid.Visibility = Visibility.Visible;
                    unmarkBorders(new List<Border> { keyBorders[20], keyBorders[21], keyBorders[22], keyBorders[23] });
                    markBorders(new List<Border> { transitionBorders[1], transitionBorders[4], transitionBorders[7], transitionBorders[10] });
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    unmarkBorders(new List<Border> { transitionBorders[1], transitionBorders[4], transitionBorders[7], transitionBorders[10] });
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    markBorders(new List<Border> { transitionBorders[1] });
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    unmarkBorders(new List<Border> { transitionBorders[1] });
                    transitionBlocks[1].Text = "";
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    transitionBlocks[1].Text = keyBlocks[21].Text;
                    transitionBlocks[4].Text = keyBlocks[22].Text;
                    transitionBlocks[7].Text = keyBlocks[23].Text;
                    transitionBlocks[10].Text = "";
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    transitionBlocks[10].Text = keyBlocks[20].Text;
                    markBorders(new List<Border> { transitionBorders[10]});
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    unmarkBorders(new List<Border> { transitionBorders[10] });
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    sBoxGrid.Visibility = Visibility.Visible;
                    expansionTransitionGrid1.Visibility = Visibility.Visible;
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
                        switch (transitionBlocks[3 * r].Text)
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
                byte[] constant = roundConstant[roundNumber - 1];
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    transition1Borders[0].Visibility = Visibility.Visible;
                    transition1Borders[3].Visibility = Visibility.Visible;
                    transition1Borders[6].Visibility = Visibility.Visible;
                    transition1Borders[9].Visibility = Visibility.Visible;
                    transition1Blocks[0].Text = constant[0].ToString("X2");
                    transition1Blocks[3].Text = constant[1].ToString("X2");
                    transition1Blocks[6].Text = constant[2].ToString("X2");
                    transition1Blocks[9].Text = constant[3].ToString("X2");
                    expansionResultGrid192.Visibility = Visibility.Visible;
                    sBoxGrid.Visibility = Visibility.Hidden;
                }, null);
                wait();
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
                        resultBorders[z].Background = Brushes.Green;
                        byte[] l = StringToByteArray(transition1Blocks[z * 3].Text);
                        byte m = StringToByteArray(transition1Blocks[z * 3 + 2].Text)[0];
                        byte n = (byte)(l[0] ^ m);
                        resultBlocks[z].Text = n.ToString("X2");
                    }, null);
                    wait();
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        resultBorders[z].Background = Brushes.Transparent;
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
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    resultBorders[0].Background = Brushes.Green;
                    resultBorders[1].Background = Brushes.Green;
                    resultBorders[2].Background = Brushes.Green;
                    resultBorders[3].Background = Brushes.Green;
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    tempString[0] = resultBlocks[0].Text;
                    tempString[1] = resultBlocks[1].Text;
                    tempString[2] = resultBlocks[2].Text;
                    tempString[3] = resultBlocks[3].Text;
                    resultBorders[0].Background = Brushes.Transparent;
                    resultBorders[1].Background = Brushes.Transparent;
                    resultBorders[2].Background = Brushes.Transparent;
                    resultBorders[3].Background = Brushes.Transparent;
                    resultBlocks[0].Text = "";
                    resultBlocks[1].Text = "";
                    resultBlocks[2].Text = "";
                    resultBlocks[3].Text = "";
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
                    keyBorders[1].Background = Brushes.Green;
                    keyBorders[2].Background = Brushes.Green;
                    keyBorders[3].Background = Brushes.Green;
                }, null);
                wait();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    transition1Borders[0].Background = Brushes.Green;
                    transition1Borders[3].Background = Brushes.Green;
                    transition1Borders[6].Background = Brushes.Green;
                    transition1Borders[9].Background = Brushes.Green;
                    transition1Blocks[0].Text = keyBlocks[0].Text;
                    transition1Blocks[3].Text = keyBlocks[1].Text;
                    transition1Blocks[6].Text = keyBlocks[2].Text;
                    transition1Blocks[9].Text = keyBlocks[3].Text;
                    keyBorders[0].Background = Brushes.Transparent;
                    keyBorders[1].Background = Brushes.Transparent;
                    keyBorders[2].Background = Brushes.Transparent;
                    keyBorders[3].Background = Brushes.Transparent;
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
                while (x < 6)
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
                            resultBorders[z + 4 * x].Background = Brushes.Green;
                            resultBlocks[z + x * 4].Text = keyBytes[(roundNumber) * 24 + x * 4 + z].ToString("X2");
                        }, null);
                        wait();
                        Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            resultBorders[z + 4 * x].Background = Brushes.Transparent;
                        }, null);
                        wait();
                    }
                    if (x < 5)
                    {
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
                        }, null);
                        wait();
                        Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            resultBorders[0 + 4 * x].Background = Brushes.Green;
                            resultBorders[1 + 4 * x].Background = Brushes.Green;
                            resultBorders[2 + 4 * x].Background = Brushes.Green;
                            resultBorders[3 + 4 * x].Background = Brushes.Green;
                        }, null);
                        wait();
                        Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            tempString[0] = resultBlocks[0 + 4 * x].Text;
                            tempString[1] = resultBlocks[1 + 4 * x].Text;
                            tempString[2] = resultBlocks[2 + 4 * x].Text;
                            tempString[3] = resultBlocks[3 + 4 * x].Text;
                            resultBorders[0 + 4 * x].Background = Brushes.Transparent;
                            resultBorders[1 + 4 * x].Background = Brushes.Transparent;
                            resultBorders[2 + 4 * x].Background = Brushes.Transparent;
                            resultBorders[3 + 4 * x].Background = Brushes.Transparent;
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
                            keyBorders[4 + 4 * x].Background = Brushes.Green;
                            keyBorders[5 + 4 * x].Background = Brushes.Green;
                            keyBorders[6 + 4 * x].Background = Brushes.Green;
                            keyBorders[7 + 4 * x].Background = Brushes.Green;
                        }, null);
                        wait();
                        Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            transition1Borders[0].Background = Brushes.Green;
                            transition1Borders[3].Background = Brushes.Green;
                            transition1Borders[6].Background = Brushes.Green;
                            transition1Borders[9].Background = Brushes.Green;
                            transition1Blocks[0].Text = keyBlocks[4 + 4 * x].Text;
                            transition1Blocks[3].Text = keyBlocks[5 + 4 * x].Text;
                            transition1Blocks[6].Text = keyBlocks[6 + 4 * x].Text;
                            transition1Blocks[9].Text = keyBlocks[7 + 4 * x].Text;
                            keyBorders[4 + 4 * x].Background = Brushes.Transparent;
                            keyBorders[5 + 4 * x].Background = Brushes.Transparent;
                            keyBorders[6 + 4 * x].Background = Brushes.Transparent;
                            keyBorders[7 + 4 * x].Background = Brushes.Transparent;
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
                    foreach (TextBlock tb in resultBlocks)
                    {
                        tb.Text = "";
                    }
                    expansionResultGrid192.Visibility = Visibility.Hidden;
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
                progress = roundNumber * 0.5 / 8;             
                if (!expansion)
                {
                    return;
                }
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                expansionKeyGrid192.Visibility = Visibility.Hidden;
            }, null);
            roundNumber = 1;
            expansion = false;       
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
            //Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            //{
            //    setUpExpansion();
            //    enableButtons();
            //}, null);
            wait();
            if (!expansion)
            {
                return;
            }
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
                    switch (transitionBlocks[3 * r].Text)
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
            byte[] constant = roundConstant[roundNumber - 1];
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
            while (x < 4)
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
                if (x < 3)
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
                foreach (TextBlock tb in resultBlocks)
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
        }
        private void setUpExpansion()
        {
            int c = 0;
            if(keysize == 2)
            {
                c = 2;
            }
            List<Border> keyBorderList = borderList[8 + keysize * 4 - c];
            List<TextBlock> keyBlockList = textBlockList[9 + keysize * 4 - c];
            byte[] prevKey;
            changeRoundButton();
            int x;
            if(keysize == 0)
            {
                prevKey = keyList[roundNumber - 1];
                x = 0;
                foreach (TextBlock tb in keyBlockList)
                {
                    tb.Text = prevKey[x].ToString("X2");
                    x++;
                }
                expansionKeyGrid.Visibility = Visibility.Visible;
            }
           else if(keysize == 1)
            {
                int y = (roundNumber - 1) * 24;
                x = 0;    
                foreach(TextBlock tb in keyBlockList)
                {
                    tb.Text = keyBytes[x + y].ToString("X2");
                    x++;
                }
                expansionKeyGrid192.Visibility = Visibility.Visible;    
            }
            else
            {
                int y = (roundNumber - 1) * 32;
                x = 0;
                foreach (TextBlock tb in keyBlockList)
                {
                    tb.Text = keyBytes[x + y].ToString("X2");
                    x++;
                }
                expansionKeyGrid256.Visibility = Visibility.Visible;
            }
            expansionProgress();
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
            int saveRoundNumber = 11 - keysize * 2;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                hideButton();
                StartingImage.Visibility = Visibility.Hidden;
                playButton.Visibility = Visibility.Visible;
                playButton.SetValue(Grid.RowProperty, 4);
            }, null);
            switch (language)
            {
                case 0:
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        Intro1ENImage.Visibility = Visibility.Visible;
                    }, null);
                    wait();
                    //Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    //{
                    //    Intro1DEImage.Visibility = Visibility.Hidden;
                    //    Intro2DEImage.Visibility = Visibility.Visible;
                    //}, null);
                    //wait();
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        Intro1ENImage.Visibility = Visibility.Hidden;
                        Intro3ENImage.Visibility = Visibility.Visible;
                    }, null);
                    wait();
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        Intro3ENImage.Visibility = Visibility.Hidden;
                        KeyExpansionImage.Visibility = Visibility.Visible;
                    }, null);
                    wait();
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        playButton.SetValue(Grid.RowProperty, 3);
                        KeyExpansionImage.Visibility = Visibility.Hidden;
                    }, null);
                    break;
                case 1:
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        Intro1DEImage.Visibility = Visibility.Visible;
                    }, null);
                    wait();
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        Intro1DEImage.Visibility = Visibility.Hidden;
                        Intro2DEImage.Visibility = Visibility.Visible;
                    }, null);
                    wait();
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        Intro2DEImage.Visibility = Visibility.Hidden;
                        Intro3DEImage.Visibility = Visibility.Visible;
                    }, null);  
                    wait();
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        Intro3DEImage.Visibility = Visibility.Hidden;
                        KeyExpansionImage.Visibility = Visibility.Visible;
                    }, null);
                    wait();
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        playButton.SetValue(Grid.RowProperty, 3);
                        KeyExpansionImage.Visibility = Visibility.Hidden;
                    }, null);
                    break;
            }
            while (expansion && !finish)
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    switch (language)
                    {
                        case 0:
                            keyButton.Content = "Skip Expansion";
                            expansionEncryptionTextBlock.Text = "Key Expansion";
                            break;
                        case 1:
                            keyButton.Content = "Erweiterung Überspringen";
                            expansionEncryptionTextBlock.Text = "Schlüsselerweiterung";
                            break;
                    }
                    
                    showButton();
                    invisible();
                    changeRoundButton();
                    buttonVisible();
                    if (expansion && shift == 1)
                    {
                        shiftButtons(0);
                    }
                }, null);
                if (keysize == 0)
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        setUpExpansion();
                    }, null);
                    //keyExpansion192();
                    while (roundNumber < 11 && expansion)
                    {
                        skip = false;
                        expansionThread = new Thread(keyExpansion);
                        expansionThread.Start();
                        while (expansionThread.IsAlive)
                        {
                            if (skip)
                            {
                                expansionThread.Abort();
                                skip = false;
                                if (!expansion)
                                {
                                    saveRoundNumber = roundNumber;
                                    roundNumber = 11;
                                }
                            }
                        }
                        progress = roundNumber * 0.5 / 10;
                        if (roundNumber < 11)
                        {
                            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {
                                setUpExpansion();
                            }, null);
                        }
                        if (roundNumber < 11)
                        {
                            autostep = false;
                            wait();
                        }       
                    }
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        expansionKeyGrid.Visibility = Visibility.Hidden;
                    }, null);
                    expansion = false;
                    //roundNumber = 1;
                    roundNumber = saveRoundNumber;
                }
                else if (keysize == 1)
                {                   
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        setUpExpansion();
                    }, null);
                    //keyExpansion192();
                    while (roundNumber < 9 && expansion)
                    {
                        skip = false;
                        expansionThread = new Thread(keyExpansion192);
                        expansionThread.Start();
                        while (expansionThread.IsAlive)
                        {
                            if (skip)
                            {
                                expansionThread.Abort();
                                skip = false;
                                if (!expansion)
                                {
                                    saveRoundNumber = roundNumber;
                                    roundNumber = 9;
                                }
                            }
                        }
                        progress = roundNumber * 0.5 / 8;
                        if (roundNumber < 9)
                        {
                            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {
                                setUpExpansion();
                            }, null);
                        }
                        if (roundNumber < 9)
                        {
                            autostep = false;
                            wait();
                        }
                    }
                    roundNumber = saveRoundNumber;
                }
                else
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        setUpExpansion();
                    }, null);
                    while (roundNumber < 8 && expansion)
                    {
                        skip = false;
                        expansionThread = new Thread(keyExpansion256);
                        expansionThread.Start();
                        while (expansionThread.IsAlive)
                        {
                            if (skip)
                            {
                                expansionThread.Abort();
                                skip = false;
                                if (!expansion)
                                {
                                    saveRoundNumber = roundNumber;
                                    roundNumber = 8;
                                }
                            }
                        }
                        progress = roundNumber * 0.5 / 7;
                        if (roundNumber < 8)
                        {
                            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {
                                setUpExpansion();
                            }, null);
                        }
                        if (roundNumber < 8)
                        {
                            autostep = false;
                            wait();
                        }
                    }
                    roundNumber = saveRoundNumber;
                }
                expansion = false;
                if (first && !finish)
                {
                    roundNumber = 0;
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        setUpAddKey();
                    }, null);
                    first = false;
                }
                else if(!finish)
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        setUpSubByte(states);
                    }, null);
                }
                else if (finish)
                {
                    action = 4;
                    roundNumber = 10 + keysize * 2;
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        setUpAddKey();
                    }, null);
                    byte[] result;
                    result = arrangeText(states[(roundNumber - 1) * 4 + action - 1]);
                    List<TextBlock> resultList = textBlockList[2];
                    int y = 0;
                    foreach (TextBlock tb in resultList)
                    {                        
                        Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            renameTextBlock(tb, result[y]);
                        }, null);
                        y++;
                    }
                }
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    expansionKeyGrid.Visibility = Visibility.Hidden;
                    autostep = false;
                    expansionKeyGrid.Visibility = Visibility.Hidden;
                    showButton();
                    keyButton.Content = "Key Expansion";
                    expansionEncryptionTextBlock.Text = "Encryption";
                    changeRoundButton();
                    buttonVisible();
                }, null);
                progress = 0.5;
                if (!finish)
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        EncryptionImage.Visibility = Visibility.Visible;
                    }, null);
                    wait();
                    autostep = false;
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        EncryptionImage.Visibility = Visibility.Hidden;
                    }, null);
                    actionMethod();
                }
                action = 1;
                if (keysize == 1 && roundNumber > 8)
                {
                    roundNumber = 8;
                }
                if (keysize == 2 && roundNumber > 7)
                {
                    roundNumber = 7;
                }
                if (roundNumber == 0)
                {
                    roundNumber++;
                }
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
                shiftRightButton.Visibility = Visibility.Hidden;
                shiftLeftButton.Visibility = Visibility.Hidden;
                startButton.SetValue(Grid.ColumnProperty, 9);
                endButton.SetValue(Grid.ColumnProperty, 10);
            }
            else
            {
                subByteButton.Visibility = Visibility.Visible;
                shiftRowButton.Visibility = Visibility.Visible;
                mixColButton.Visibility = Visibility.Visible;
                addKeyButton.Visibility = Visibility.Visible;
                if(keysize != 0)
                {
                    shiftRightButton.Visibility = Visibility.Visible;
                    shiftLeftButton.Visibility = Visibility.Visible;
                    startButton.SetValue(Grid.ColumnProperty, 7);
                    endButton.SetValue(Grid.ColumnProperty, 8);
                }
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
            startButton.Visibility = Visibility.Hidden;
            endButton.Visibility = Visibility.Hidden;
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
            startButton.Visibility = Visibility.Visible;
            endButton.Visibility = Visibility.Visible;
            if(!expansion || keysize == 0)
            {
                round9Button.Visibility = Visibility.Visible;
                round10Button.Visibility = Visibility.Visible;
                prevStepButton.SetValue(Grid.RowProperty, 3);
                prevStepButton.SetValue(Grid.ColumnProperty, 2);
                nextStepButton.SetValue(Grid.ColumnProperty, 3);
                nextStepButton.SetValue(Grid.RowProperty, 3);
                autostepSpeedSlider.SetValue(Grid.ColumnProperty, 4);

            }
            else if(keysize == 1)
            {
                round10Button.Visibility = Visibility.Hidden;
                round9Button.Visibility = Visibility.Hidden;
                prevStepButton.SetValue(Grid.RowProperty, 4);
                prevStepButton.SetValue(Grid.ColumnProperty, 9);
                nextStepButton.SetValue(Grid.ColumnProperty, 10);
                nextStepButton.SetValue(Grid.RowProperty, 4);
                autostepSpeedSlider.SetValue(Grid.ColumnProperty, 2);
            }
            else
            {
                round10Button.Visibility = Visibility.Hidden;
                round9Button.Visibility = Visibility.Hidden;
                round8Button.Visibility = Visibility.Hidden;
                prevStepButton.SetValue(Grid.RowProperty, 4);
                prevStepButton.SetValue(Grid.ColumnProperty, 8);
                nextStepButton.SetValue(Grid.ColumnProperty, 9);
                nextStepButton.SetValue(Grid.RowProperty, 4);
                autostepSpeedSlider.SetValue(Grid.ColumnProperty, 2);
            }
            keyButton.Visibility = Visibility.Visible;
        }
        private byte[] arrangeText(byte[] input)
        {
            byte[] result = new byte[16];
            result[0] = input[0];
            result[4] = input[1];
            result[8] = input[2];
            result[12] = input[3];
            result[1] = input[4];
            result[5] = input[5];
            result[9] = input[6];
            result[13] = input[7];
            result[2] = input[8];
            result[6] = input[9];
            result[10] = input[10];
            result[14] = input[11];
            result[3] = input[12];
            result[7] = input[13];
            result[11] = input[14];
            result[15] = input[15];
            return result;
        }

        private byte[] rearrangeText(byte[] input)
        {
            byte[] result = new byte[16];
            result[0] = input[0];
            result[1] = input[4];
            result[2] = input[8];
            result[3] = input[12];
            result[4] = input[1];
            result[5] = input[5];
            result[6] = input[9];
            result[7] = input[13];
            result[8] = input[2];
            result[9] = input[6];
            result[10] = input[10];
            result[11] = input[14];
            result[12] = input[3];
            result[13] = input[7];
            result[14] = input[11];
            result[15] = input[15];
            return result;
        }

        private void shiftButtons(int shifting)
        {
            shift = shifting;
            if(shift == 0)
            {
                round1Button.Content = "Round 1";
                round2Button.Content = "Round 2";
                round3Button.Content = "Round 3";
                round4Button.Content = "Round 4";
                round5Button.Content = "Round 5";
                round6Button.Content = "Round 6";
                round7Button.Content = "Round 7";
                round8Button.Content = "Round 8";
                round9Button.Content = "Round 9";
                round10Button.Content = "Round 10";
            }
            else if(shift == 1)
            {
                if (keysize == 1)
                {
                    round1Button.Content = "Round 3";
                    round2Button.Content = "Round 4";
                    round3Button.Content = "Round 5";
                    round4Button.Content = "Round 6";
                    round5Button.Content = "Round 7";
                    round6Button.Content = "Round 8";
                    round7Button.Content = "Round 9";
                    round8Button.Content = "Round 10";
                    round9Button.Content = "Round 11";
                    round10Button.Content = "Round 12";
                }
                else
                {
                    round1Button.Content = "Round 5";
                    round2Button.Content = "Round 6";
                    round3Button.Content = "Round 7";
                    round4Button.Content = "Round 8";
                    round5Button.Content = "Round 9";
                    round6Button.Content = "Round 10";
                    round7Button.Content = "Round 11";
                    round8Button.Content = "Round 13";
                    round9Button.Content = "Round 12";
                    round10Button.Content = "Round 14";
                }
            }
            changeRoundButton();
        }

        private void markBorders(List<Border> marking)
        {
            foreach(Border b in marking)
            {
                //markedBorders.Add(b);
                b.Background = brush;
            }
           

        }
        private void markBorders2(int listNumber, int[] position)
        {
            foreach(int x in position)
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    borderList[listNumber][x].Background = Brushes.Green;
                }, null);
            }
            markedList.Add(listNumber);
            markedPositions.Add(position);
        }
        private void unmarkBorders(List<Border> unmark)
        {
            foreach(Border b in unmark)
            {               
                //markedBorders.Remove(b);
                b.Background = Brushes.Transparent;
            }
        }

        private void unmarkBorders2(int listNumber, int[] position)
        {
            foreach (int x in position)
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    borderList[listNumber][x].Background = Brushes.Transparent;
                }, null);
            }
            markedList.Remove(listNumber);
            markedPositions.Remove(position);
        }

        private void expansionProgress()
        {
            switch (keysize)
            {
                case 0:
                    progress = (roundNumber-1) * 0.05;
                    break;
                case 1:
                    progress = (roundNumber-1) * 0.5 / 8;
                    break;
                case 2:
                    progress = (roundNumber-1) * 0.5 / 7;
                    break;
                default:
                    break;
            }
        }

        private void encryptionProgress()
        {
            switch (keysize)
            {
                case 0:
                    progress = (roundNumber - 1) * 0.05 + 0.5;
                    break;
                case 1:
                    progress = (roundNumber - 1) * 0.5 / 12 + 0.5;
                    break;
                case 2:
                    progress = (roundNumber - 1) * 0.5 / 14 + 0.5;
                    break;
                default:
                    break;
            }
        }

        public void cleanUp()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                invisible();
            }, null);
            if (expansion)
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    foreach(Border b in borderList[11])
                    {
                        b.Background = Brushes.Transparent;
                    }
                    foreach(TextBlock tb in textBlockList[12])
                    {
                        tb.Text = "";
                    }
                    expansionExplanation.Visibility = Visibility.Hidden;
                    expansionExplanation1.Visibility = Visibility.Hidden;
                    expansionExplanation2.Visibility = Visibility.Hidden;
                    expansionExplanation3.Visibility = Visibility.Hidden;
                    expansionExplanation4.Visibility = Visibility.Hidden;
                    foreach (Border b in borderList[13])
                    {
                        b.Background = Brushes.Transparent;
                    }
                    foreach (Border b in borderList[12])
                    {
                        b.Background = Brushes.Transparent;
                    }
                    int x = 0;
                    foreach(Border b in borderList[9])
                    {
                        if(b != null)
                        {
                            b.Background = Brushes.Transparent;
                            if (x == 1 || x == 4 || x == 7 || x == 10)
                            {
                                b.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                b.Visibility = Visibility.Hidden;
                            }
                        }
                        x++;
                    }
                    foreach(TextBlock tb in textBlockList[14])
                    {
                        tb.Text = "";
                    }
                    foreach (TextBlock tb in textBlockList[10])
                    {
                        if(tb != null)
                        {
                            tb.Text = "";
                        }
                    }
                    x = 9;
                    while(x < 17)
                    {
                        if(x != 11 && x != 12 && x != 13)
                        {
                            foreach (Border b in borderList[x])
                            {
                                if(b != null)
                                {
                                    b.Background = Brushes.Transparent;
                                }
                            }
                        }
                        x++;
                    }
                    x = 10;
                    while(x < 18)
                    {
                        if(x !=12 && x != 13 && x != 14)
                        {
                            foreach (TextBlock tb in textBlockList[x]) 
                            {
                                if (tb != null)
                                {
                                    tb.Text = "";
                                }
                            }
                        }
                        x++;
                    }
                    x = 0;
                    while(x < 12)
                    {
                        if (x == 1 || x == 4 || x == 7 || x == 10)
                        {
                            borderList[16][x].Visibility = Visibility.Visible;
                        }
                        else {
                            borderList[16][x].Visibility = Visibility.Hidden;
                        }
                        x++;
                    }                   
                    List<Border> tempList = new List<Border>();
                    string y = "sBorder";
                    x = 1;
                    while (x < 17)
                    {
                        tempList.Add((Border)FindName(y + x));
                        x++;
                    }
                    x = 18;
                    while (x < 289)
                    {
                        tempList.Add((Border)FindName(y + x));
                        x += 18;
                    }
                    foreach (Border b in tempList)
                    {
                        b.Background = Brushes.Yellow;
                    }
                    foreach (TextBlock tb in textBlockList[3])
                    {
                        tb.Background = Brushes.Transparent;
                    }
                }, null);
                return;
            }
            switch (action)
            { 
                case 1:
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        List<TextBlock> sResult = textBlockList[5];
                        sTransitionTextBlock1.Text = "";
                        sTransitionTextBlock2.Text = "";
                        sTransitionTextBlock3.Text = "";
                        sTransitionTextBlock1.Background = Brushes.Transparent;
                        sTransitionTextBlock2.Background = Brushes.Transparent;
                        sTransitionTextBlock3.Background = Brushes.Transparent;
                        foreach (TextBlock tb in sResult)
                        {
                            tb.Text = "";
                            tb.Background = Brushes.Transparent;
                        }
                        foreach (TextBlock tb in textBlockList[3])
                        {
                            tb.Background = Brushes.Transparent;
                        }
                        List<Border> tempList = new List<Border>();
                        string y = "sBorder";
                        int x = 1;
                        while (x < 17)
                        {
                            tempList.Add((Border)FindName(y + x));
                            x++;
                        }
                        x = 18;
                        while (x < 289)
                        {
                            tempList.Add((Border)FindName(y + x));
                            x += 18;
                        }
                        foreach (Border b in tempList)
                        {
                            b.Background = Brushes.Yellow;
                        }
                        sTransitionBorder1.Visibility = Visibility.Hidden;
                        sTransitionBorder2.Visibility = Visibility.Hidden;
                    }, null);              
                    break;
                case 2:
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {

                    }, null);
                    break;
                case 3:
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        int x = 4;
                        while(x < 8)
                        {
                            foreach (Border b in borderList[x])
                            {
                                b.Background = Brushes.Transparent;
                                if(x == 6)
                                {
                                    b.Visibility = Visibility.Hidden;
                                }
                            }
                            x++;
                        }
                        foreach (TextBlock tb in textBlockList[8])
                        {
                            tb.Text = "";
                        }
                    }, null);
                    break;
                case 4:
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        int x = 0;
                        while(x < 3)
                        {
                            foreach(Border b in borderList[x])
                            {
                                b.Background = Brushes.Transparent;
                            }
                            x++;
                        }
                        foreach(TextBlock tb in textBlockList[2])
                        {
                            tb.Text = "";
                        }
                    }, null);
                    break;
                default:
                    break;
            }
        }

        public void reset()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                StartCanvas.Visibility = Visibility.Visible;
            }, null);
            roundNumber = 1;
            action = 1;
            first = true;
            shift = 0;
            expansion = true;
            skip = false;
            sBox = new byte[16][];
            states = new byte[40][];
            keyList = new byte[11][];
    }

        public void setLanguage()
        {
            switch (language)
            {
                case 0:
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        round10Button.Content = "Round 10";
                        round9Button.Content = "Round 9";
                        round8Button.Content = "Round 8";
                        round7Button.Content = "Round 7";
                        round6Button.Content = "Round 6";
                        round5Button.Content = "Round 5";
                        round4Button.Content = "Round 4";
                        round3Button.Content = "Round 3";
                        round2Button.Content = "Round 2";
                        round1Button.Content = "Round 1";
                        endButton.Content = "End";
                        playButton.Content = "Next";
                        nextStepButton.Content = "Skip Round";
                        prevStepButton.Content = "Prev. Round";

                    }, null);
                    break;
                case 1:
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        round10Button.Content = "Runde 10";
                        round9Button.Content = "Runde 9";
                        round8Button.Content = "Runde 8";
                        round7Button.Content = "Runde 7";
                        round6Button.Content = "Runde 6";
                        round5Button.Content = "Runde 5";
                        round4Button.Content = "Runde 4";
                        round3Button.Content = "Runde 3";
                        round2Button.Content = "Runde 2";
                        round1Button.Content = "Runde 1";
                        endButton.Content = "Ende";
                        playButton.Content = "Weiter";
                        keyButton.Content = "Schlüsselerweiterung";
                        nextStepButton.Content = "Runde überspringen";
                        prevStepTextBlock.Text = "Vorherige Runde";
                    }, null);
                    break;
            }   
            
            
        }

        #endregion

       
    }
}
