
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

namespace Cryptool.DESVisualisation
{
    /// <summary>
    /// Interaction logic for DESPresentation.xaml
    /// </summary>
    public partial class DESPresentation : UserControl
    {

        //Constructor
        public DESPresentation()
        {
            InitializeComponent();
            playTimer.Tick += PlayTimer_Tick;
            playTimer.Interval = TimeSpan.FromSeconds(1.5);
            playTimer.IsEnabled = false;
            getDiffusionBoxes();
            diffusionActive = false;
            stepCounter = 0;
            screenCounter = 0;
            roundCounter = 0;
            keySchedule = false;
            desRounds = false;
        }

        /////////////////////////////////////////////////////////////
        // Attributes
        int stepCounter;
        int screenCounter;
        int roundCounter;

        bool keySchedule;
        bool desRounds;

        IEnumerable<CheckBox> diffusionBoxes;
        SolidColorBrush greenBrush = new SolidColorBrush(Colors.LightGreen);
        SolidColorBrush yellowBrush = new SolidColorBrush(Colors.Khaki);
        SolidColorBrush buttonBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFBDE0E6"));

        System.Windows.Threading.DispatcherTimer playTimer = new System.Windows.Threading.DispatcherTimer();
        public DESImplementation EncOriginal;
        DESImplementation EncDiffusion;
        bool diffusionActive;




        /////////////////////////////////////////////////////////////
        #region Button-Methods

        //////////////////// Button Row 1

        private void ShiftButton_Click(object sender, RoutedEventArgs e)
        {
            screenCounter = 7;
            stepCounter = 0;
            executeSteps();
        }

        private void PC2Button_Click(object sender, RoutedEventArgs e)
        {
            screenCounter = 8;
            stepCounter = 1;
            executeSteps();
        }

        private void ExpansionButton_Click(object sender, RoutedEventArgs e)
        {
            screenCounter = 13;
            stepCounter = 1;
            executeSteps();
        }

        private void KeyAdditionButton_Click(object sender, RoutedEventArgs e)
        {
            screenCounter = 14;
            stepCounter = 0;
            executeSteps();
        }

        private void SBoxButton_Click(object sender, RoutedEventArgs e)
        {
            screenCounter = 15;
            stepCounter = 0;
            executeSteps();
        }

        private void PermutationButton_Click(object sender, RoutedEventArgs e)
        {
            screenCounter = 16;
            stepCounter = 1;
            executeSteps();
        }

        private void RoundAdditionButton_Click(object sender, RoutedEventArgs e)
        {
            screenCounter = 14;
            stepCounter = 2;
            executeSteps();
        }

        //////////////////// Button Row 2

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            switch (screenCounter)
            {
                case 1:
                    if (stepCounter == 2) showDataScreen();
                    else if (stepCounter == 3)
                    {
                        roundCounter = 16;
                        showKeyScheduleScreen(5);
                    }
                    else if (stepCounter == 4)
                    {
                        roundCounter = 0;
                        showDESRoundScreen(1);
                        roundCounter = 15;
                        showDESRoundScreen(1);
                        showRoundDataScreen(1);
                    }
                    break;
                case 2:
                    if (stepCounter == 2) showExecutionScreen(1);
                    else if (stepCounter == 1) showIntroScreen();

                    break;
                case 3: showExecutionScreen(1); break;
                case 4: showInfoScreen(1); break;
                case 5:
                    if (stepCounter == 1) showStructureScreen();
                    else if (stepCounter == 2) showExecutionScreen(2);

                    break;
                case 6:
                    if (stepCounter == 1 && roundCounter == 0) showExecutionScreen(2);
                    else if (stepCounter == 1 && roundCounter != 0) showKeyScheduleScreen(5);
                    else if (stepCounter == 2 && roundCounter == 1)
                    {
                        showExecutionScreen(3);
                        showPC1Screen(1);
                    }
                    else if (stepCounter == 2 && roundCounter != 1)
                    {
                        roundCounter = roundCounter - 2;
                        showKeyScheduleScreen(1);
                        showRoundKeyDataScreen(1);
                    }
                    else if (stepCounter == 3)
                    {
                        showKeyScheduleScreen(2);
                    }
                    else if (stepCounter == 4)
                    {
                        showKeyScheduleScreen(3);
                    }
                    else if (stepCounter == 5)
                    {
                        showKeyScheduleScreen(4);
                    }
                    break;
                case 7:
                    if (stepCounter == 0)
                    {
                        roundCounter--;
                        showKeyScheduleScreen(1);
                    }
                    else if (stepCounter == 1) showKeyScheduleScreen(2);
                    else if (stepCounter == 2) showShift1Screen(0);
                    else if (stepCounter == 3) showKeyScheduleScreen(3);

                    break;
                case 8:
                    if (stepCounter == 1) showShift1Screen(2);
                    else if (stepCounter == 2) showKeyScheduleScreen(4);

                    break;
                case 9:
                    if (stepCounter == 1) showPC2Screen(1);
                    else if (stepCounter == 2) showFPScreen(1);

                    break;
                case 10:
                    if (stepCounter == 1)
                    {
                        roundCounter = 0;
                        showKeyScheduleScreen(1);
                        roundCounter = 15;
                        showKeyScheduleScreen(1);
                        showRoundKeyDataScreen(1);
                    }
                    else if (stepCounter == 2) showExecutionScreen(3);
                    break;
                case 11:
                    if (stepCounter == 1 && roundCounter == 0) showExecutionScreen(3);
                    else if (stepCounter == 1 && roundCounter != 0) showDESRoundScreen(4);
                    else if (stepCounter == 2 && roundCounter == 1)
                    {
                        showFPScreen(1);
                        showIPScreen(1);
                    }
                    else if (stepCounter == 2 && roundCounter != 1)
                    {
                        roundCounter = roundCounter - 2;
                        showDESRoundScreen(1);
                        showRoundDataScreen(1);
                    }
                    else if (stepCounter == 3)
                    {
                        showPPScreen(1);
                    }
                    else if (stepCounter == 4)
                    {
                        showDESRoundScreen(3);
                    }

                    break;
                case 12:
                    if (stepCounter == 1)
                    {
                        roundCounter--;
                        showDESRoundScreen(1);
                    }
                    else if (stepCounter == 2) showDESRoundScreen(2);
                    else if (stepCounter == 3) showRoundFunctionScreen(2);
                    else if (stepCounter == 4) showRoundFunctionScreen(3);
                    else if (stepCounter == 5) showRoundFunctionScreen(4);
                    else if (stepCounter == 6) showRoundFunctionScreen(5);
                    break;
                case 13:
                    if (stepCounter == 1) showRoundFunctionScreen(1);
                    else if (stepCounter == 2) showRoundFunctionScreen(2);
                    break;
                case 14:
                    if (stepCounter == 0) showExpansionScreen(1);
                    else if (stepCounter == 1) showRoundFunctionScreen(3);
                    else if (stepCounter == 2) showRoundFunctionScreen(6);
                    else if (stepCounter == 3) showDESRoundScreen(3);
                    break;
                case 15:
                    if (stepCounter == 0) showXORScreen(0);
                    else if (stepCounter != 0) showRoundFunctionScreen(4);
                    break;
                case 16:
                    if (stepCounter == 1) showSBoxScreen(0);
                    else if (stepCounter == 2) showRoundFunctionScreen(5);
                    break;
                case 17:
                    if (stepCounter == 1) showXORScreen(2);
                    else if (stepCounter == 2) showExecutionScreen(4);
                    break;
                case 18:
                    if (stepCounter == 1) showDESRoundScreen(4);
                    else if (stepCounter == 2)
                    {
                        roundCounter = 0;
                        showDESRoundScreen(1);
                        roundCounter = 15;
                        showDESRoundScreen(1);
                        showRoundDataScreen(1);
                    }
                    break;
                case 19: showRoundKeyDataScreen(2); break;
                case 20: showRoundDataScreen(2); break;
                default: break;

            }
        }

        private void AutoButton_Click(object sender, RoutedEventArgs e)
        {
            if (AutoButton.IsChecked == true)
            {
                playTimer.Start();
            }
            else
            {
                playTimer.Stop();
            }
        }

        private void AutoSpeedSlider_ValueChanged(object sender, RoutedEventArgs e)
        {
            playTimer.Interval = TimeSpan.FromSeconds(AutoSpeedSlider.Value);
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            executeSteps();
        }

        private void SkipStepButton_Click(object sender, RoutedEventArgs e)
        {

            if (roundCounter > 0 && roundCounter < 16)
            {
                if (desRounds)
                {
                    showDESRoundScreen(1);
                }
                else if (keySchedule)
                {
                    showKeyScheduleScreen(1);
                }
            }
            else if (roundCounter == 16)
            {
                if (desRounds)
                {
                    showFPScreen(1);
                }
                else if (keySchedule)
                {
                    showExecutionScreen(3);
                }
                else
                {
                    executeSteps();
                }
            }
            else
            {
                if (stepCounter != 1)
                {
                    switch (screenCounter)
                    {
                        case 2: executeSteps(); break;
                        case 5: executeSteps(); break;
                        case 10: executeSteps(); break;
                        case 18: executeSteps(); break;
                        default:
                            break;
                    }


                }
                executeSteps();
            }
        }

        //////////////////// Button Row 3

        private void IntroButton_Click(object sender, RoutedEventArgs e)
        {
            if (desRounds)
            {
                showFPScreen(1);
            }
            else if (keySchedule)
            {
                showExecutionScreen(3);
            }
            screenCounter = 1;
            stepCounter = 1;
            executeSteps();

        }

        private void DataButton_Click(object sender, RoutedEventArgs e)
        {
            if (desRounds)
            {
                showFPScreen(1);
            }
            else if (keySchedule)
            {
                showExecutionScreen(3);
            }
            screenCounter = 3;
            stepCounter = 1;
            executeSteps();
        }

        private void PC1Button_Click(object sender, RoutedEventArgs e)
        {
            if (desRounds)
            {
                showFPScreen(1);
            }
            else if (keySchedule)
            {
                showExecutionScreen(3);
            }
            screenCounter = 5;
            stepCounter = 1;
            executeSteps();
        }

        private void KeyScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            if (desRounds)
            {
                showFPScreen(1);
            }
            roundCounter = 0;
            screenCounter = 6;
            stepCounter = 1;
            executeSteps();
        }

        private void IPButton_Click(object sender, RoutedEventArgs e)
        {
            if (desRounds)
            {
                showFPScreen(1);
            }
            else if (keySchedule)
            {
                showExecutionScreen(3);
            }
            screenCounter = 10;
            stepCounter = 1;
            executeSteps();
        }

        private void DESButton_Click(object sender, RoutedEventArgs e)
        {
            if (keySchedule)
            {
                showExecutionScreen(3);
            }
            roundCounter = 0;
            screenCounter = 11;
            stepCounter = 1;
            executeSteps();
        }

        private void FPButton_Click(object sender, RoutedEventArgs e)
        {
            if (desRounds)
            {
                showFPScreen(1);
            }
            else if (keySchedule)
            {
                showExecutionScreen(3);
            }
            screenCounter = 18;
            stepCounter = 1;
            executeSteps();
        }

        private void SummaryButton_Click(object sender, RoutedEventArgs e)
        {
            if (desRounds)
            {
                showFPScreen(1);
            }
            else if (keySchedule)
            {
                showExecutionScreen(3);
            }
            screenCounter = 1;
            stepCounter = 4;
            executeSteps();
        }

        //////////////////// Button Row 4

        private void roundButton_Click(object sender, RoutedEventArgs e)
        {

            roundCounter = Grid.GetColumn((Button)sender) - 1;
            if (desRounds)
            {
                screenCounter = 11;
            }
            else
            {
                screenCounter = 6;
            }
            stepCounter = 1;
            executeSteps();
        }

        //////////////////// Button in DataScreen

        private void DiffusionTButton_Click(object sender, RoutedEventArgs e)
        {
            if (DiffusionTButton.IsChecked == true)
            {
                showDiffusionBoxes(true);
                DiffusionOKButton.Visibility = Visibility.Visible;
                DiffusionClearButton.Visibility = Visibility.Visible;
                DiffusionTButton.Background = Brushes.Aqua;
            }
            else
            {
                showDiffusionBoxes(false);
                DiffusionOKButton.Visibility = Visibility.Hidden;
                DiffusionClearButton.Visibility = Visibility.Hidden;
                DiffusionTButton.ClearValue(Button.BackgroundProperty);
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox box = (CheckBox)sender;
            int pos = Grid.GetColumn(box) - 1;
            if (box.IsChecked == true)
            {
                if (Grid.GetRow(box) == 9)
                {
                    colorTextSingle(DataKey, (byte)(pos));
                    switchStringBit(DataKey, pos);
                }
                else
                {
                    colorTextSingle(DataMessage, (byte)(pos));
                    switchStringBit(DataMessage, pos);
                }
            }
            else
            {

                if (Grid.GetRow(box) == 9)
                {
                    switchStringBit(DataKey, pos);
                    TextEffect tmp = null;
                    TextEffectCollection.Enumerator enumerator = DataKey.TextEffects.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current.PositionStart == pos)
                            tmp = enumerator.Current;
                    }
                    DataKey.TextEffects.Remove(tmp);

                }
                else
                {
                    switchStringBit(DataMessage, pos);
                    TextEffect tmp = null;
                    TextEffectCollection.Enumerator enumerator = DataMessage.TextEffects.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current.PositionStart == pos)
                            tmp = enumerator.Current;
                    }
                    DataMessage.TextEffects.Remove(tmp);
                }
            }
        }

        private void DiffusionClearButton_Click(object sender, RoutedEventArgs e)
        {
            IEnumerator<CheckBox> enumerator = diffusionBoxes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                enumerator.Current.IsChecked = false;
            }

            clearTexteffects(DataKey);
            clearTexteffects(DataMessage);
            DataKey.Text = EncOriginal.key;
            DataMessage.Text = EncOriginal.message;
        }

        private void DiffusionOKButton_Click(object sender, RoutedEventArgs e)
        {
            if (!DataKey.Text.Equals(EncOriginal.key) || !DataMessage.Text.Equals(EncOriginal.message))
            {
                byte[] key = stringToByteArray(DataKey.Text);
                byte[] msg = stringToByteArray(DataMessage.Text);
                EncDiffusion = new DESImplementation(key, msg);
                EncDiffusion.DES();
                diffusionActive = true;
            }

            //hide Diffusion-Functionality
            showDiffusionBoxes(false);
            DiffusionOKButton.Visibility = Visibility.Hidden;
            DiffusionClearButton.Visibility = Visibility.Hidden;
            DiffusionTButton.ClearValue(Button.BackgroundProperty);
            DiffusionTButton.IsChecked = false;
        }

        #endregion Button-Methods

        /////////////////////////////////////////////////////////////
        #region Screen-Methods

        // Refresh-Methods

        public void showIntroScreen()
        {
            resetAllScreens(true);
            IntroScreen.Visibility = Visibility.Visible;
            screenCounter = 1;
            stepCounter = 1;
        }

        public void showInfoScreen(int step)
        {
            resetAllScreens(true);
            InfoScreen.Visibility = Visibility.Visible;
            title.Visibility = Visibility.Visible;

            if (step == 1)
            {
                title.Content = "Background";
                HistoryText.Visibility = Visibility.Visible;
                screenCounter = 2;
                stepCounter = 2;
            }
            else if (step == 2)
            {
                title.Content = "General Informations";
                InfoText.Visibility = Visibility.Visible;
                screenCounter = 3;
                stepCounter = 1;
            }

        }

        public void showExecutionScreen(int step)
        {
            resetAllScreens(true);
            ExecutionScreen.Visibility = Visibility.Visible;
            switch (step)
            {
                case 1:
                    ExecutionLabel.Content = "Introduction";
                    clearButtonsColor(false);
                    IntroButton.Background = buttonBrush;
                    screenCounter = 2;
                    stepCounter = 1;
                    break;
                case 2:
                    ExecutionLabel.Content = "Key Schedule";
                    screenCounter = 5;
                    stepCounter = 1;
                    break;
                case 3:
                    ExecutionLabel.Content = "DES Encryption";
                    screenCounter = 10;
                    stepCounter = 1;
                    roundCounter = 0;
                    activateRoundButtons(false);
                    keySchedule = false;
                    clearButtonsColor(true);
                    ShiftButton.Visibility = Visibility.Hidden;
                    PC2Button.Visibility = Visibility.Hidden;
                    SkipStepButton.Content = "Skip Step";
                    break;
                case 4:
                    ExecutionLabel.Content = "Summary";
                    clearButtonsColor(false);
                    SummaryButton.Background = buttonBrush;
                    screenCounter = 9;
                    stepCounter = 2;
                    roundCounter = 16;
                    break;
                default:
                    break;
            }

        }

        public void showFinalScreen()
        {
            resetAllScreens(true);
            FinalScreen.Visibility = Visibility.Visible;
            if (diffusionActive)
            {
                FinalCiphertext.Text = EncDiffusion.ciphertext;
                colorText(FinalCiphertext, compareStrings(EncOriginal.ciphertext, EncDiffusion.ciphertext));

                FinalMessage.Text = EncDiffusion.message;
                colorText(FinalMessage, compareStrings(EncOriginal.message, FinalMessage.Text));
                FinalKey.Text = EncDiffusion.key;
                colorText(FinalKey, compareStrings(EncOriginal.key, FinalKey.Text));
            }
            else
            {
                FinalCiphertext.Text = EncOriginal.ciphertext;
                FinalMessage.Text = EncOriginal.message;
                FinalKey.Text = EncOriginal.key;
            }
            screenCounter = 20;
            stepCounter = 1;

        }

        public void showDataScreen()
        {
            resetAllScreens(true);
            DataScreen.Visibility = Visibility.Visible;
            title.Visibility = Visibility.Visible;
            title.Content = "Input Data";


            if (diffusionActive)
            {
                DataMessage.Text = EncDiffusion.message;
                DataKey.Text = EncDiffusion.key;
            }
            else
            {
                DataMessage.Text = EncOriginal.message;
                DataKey.Text = EncOriginal.key;
            }


            clearButtonsColor(false);
            DataButton.Background = buttonBrush;
            screenCounter = 4;
            stepCounter = 1;
        }

        public void showRoundDataScreen(int step)
        {
            resetAllScreens(true);
            RoundDataScreen.Visibility = Visibility.Visible;
            ArrowRounds.Margin = new Thickness(0, 0, 579, 220 - roundCounter * 30);
            IEnumerable<TextBlock> textChilds = RoundDataGrid.Children.OfType<TextBlock>();
            IEnumerator<TextBlock> enumerator = textChilds.GetEnumerator();
            for (int i = 0; i < roundCounter + 1; i++)
            {
                enumerator.MoveNext();
                enumerator.Current.Visibility = Visibility.Visible;
                if (diffusionActive)
                {
                    enumerator.Current.Text = EncDiffusion.LR_Data[i, 0];
                    colorText(enumerator.Current, compareStrings(EncOriginal.LR_Data[i, 0], enumerator.Current.Text));
                }
                else
                {
                    enumerator.Current.Text = EncOriginal.LR_Data[i, 0];
                }
                enumerator.MoveNext();
                enumerator.Current.Visibility = Visibility.Visible;
                if (diffusionActive)
                {
                    enumerator.Current.Text = EncDiffusion.LR_Data[i, 1];
                    colorText(enumerator.Current, compareStrings(EncOriginal.LR_Data[i, 1], enumerator.Current.Text));
                }
                else
                {
                    enumerator.Current.Text = EncOriginal.LR_Data[i, 1];
                }
            }
            if (roundCounter == 16 && step == 1)
            {
                screenCounter = 18;
                stepCounter = 1;
                clearButtonsColor(true);
            }
            else if (roundCounter < 16 && step == 1)
            {
                screenCounter = 11;
                stepCounter = 1;
                clearButtonsColor(true);
            }
            else
            {
                screenCounter = 19;
                stepCounter = 1;
                ArrowRounds.Visibility = Visibility.Hidden;
            }
        }

        public void showRoundKeyDataScreen(int step)
        {
            resetAllScreens(true);
            RoundKeyDataScreen.Visibility = Visibility.Visible;
            ArrowSubKeys.Margin = new Thickness(0, 0, 579, 220 - (roundCounter - 1) * 30);
            IEnumerable<TextBlock> textChilds = RoundKeyDataGrid.Children.OfType<TextBlock>();
            IEnumerator<TextBlock> enumerator = textChilds.GetEnumerator();
            for (int i = 0; i < roundCounter; i++)
            {
                enumerator.MoveNext();
                enumerator.Current.Visibility = Visibility.Visible;
                if (diffusionActive)
                {
                    enumerator.Current.Text = EncDiffusion.RoundKeys[i];
                    colorText(enumerator.Current, compareStrings(EncOriginal.RoundKeys[i], enumerator.Current.Text));
                }
                else
                {
                    enumerator.Current.Text = EncOriginal.RoundKeys[i];
                }
            }

            if (roundCounter == 16 && step == 1)
            {
                screenCounter = 1;
                stepCounter = 3;
                roundCounter = 0;
                clearButtonsColor(true);
            }
            else if (roundCounter < 16 && step == 1)
            {
                screenCounter = 6;
                stepCounter = 1;
                clearButtonsColor(true);
            }
            else
            {
                screenCounter = 17;
                stepCounter = 2;
                ArrowSubKeys.Visibility = Visibility.Hidden;
            }

        }

        public void showSBoxScreen(int fullStep)        //fängt bei 0 an !!!
        {
            int sBoxctr = (int)(fullStep / 5);
            int step = fullStep % 5;
            if (fullStep == 0) resetAllScreens(true);
            else resetAllScreens(false);
            SBoxScreen.Visibility = Visibility.Visible;
            title.Visibility = Visibility.Visible;
            title.Content = "S-Boxes";
            SLabel.Content = "S" + (sBoxctr + 1);
            switch (sBoxctr)
            {
                case 0: S1Box.Visibility = Visibility.Visible; break;
                case 1: S2Box.Visibility = Visibility.Visible; break;
                case 2: S3Box.Visibility = Visibility.Visible; break;
                case 3: S4Box.Visibility = Visibility.Visible; break;
                case 4: S5Box.Visibility = Visibility.Visible; break;
                case 5: S6Box.Visibility = Visibility.Visible; break;
                case 6: S7Box.Visibility = Visibility.Visible; break;
                case 7: S8Box.Visibility = Visibility.Visible; break;
                default: break;

            }

            if (step >= 0)
            {
                SBoxInput.Visibility = Visibility.Visible;
                if (diffusionActive)
                {
                    SBoxInput.Text = EncDiffusion.SBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 0];
                    colorText(SBoxInput, compareStrings(EncDiffusion.SBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 0], EncOriginal.SBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 0]));
                }
                else
                {
                    SBoxInput.Text = EncOriginal.SBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 0];
                }
                if (sBoxctr == 0)
                {
                    clearButtonsColor(true);
                    SBoxButton.Background = buttonBrush;
                }

            }
            if (step >= 1)
            {
                SBoxRow.Visibility = Visibility.Visible;
                if (diffusionActive)
                {
                    SBoxRow.Text = EncDiffusion.SBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 1];
                    colorText(SBoxRow, compareStrings(EncDiffusion.SBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 1], EncOriginal.SBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 1]));
                    SBoxRow.Text += "    --> " + EncDiffusion.SBoxNumberDetails[roundCounter - 1, sBoxctr * 3 + 0];
                }
                else
                {
                    SBoxRow.Text = EncOriginal.SBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 1] + "    --> " + EncOriginal.SBoxNumberDetails[roundCounter - 1, sBoxctr * 3 + 0];
                }

            }
            if (step >= 2)
            {
                SBoxColumn.Visibility = Visibility.Visible;
                if (diffusionActive)
                {
                    SBoxColumn.Text = EncDiffusion.SBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 2];
                    colorText(SBoxColumn, compareStrings(EncDiffusion.SBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 2], EncOriginal.SBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 2]));
                    SBoxColumn.Text += "    --> " + EncDiffusion.SBoxNumberDetails[roundCounter - 1, sBoxctr * 3 + 1];
                }
                else
                {
                    SBoxColumn.Text = EncOriginal.SBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 2] + "  --> " + EncOriginal.SBoxNumberDetails[roundCounter - 1, sBoxctr * 3 + 1];
                }

            }
            if (step >= 3)
            {
                SBoxOutput.Visibility = Visibility.Visible;
                int column, row;
                if (diffusionActive)
                {
                    SBoxOutput.Text = EncDiffusion.SBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 3];
                    colorText(SBoxOutput, compareStrings(EncDiffusion.SBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 3], EncOriginal.SBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 3]));
                    SBoxOutput.Text += "  <-- " + EncDiffusion.SBoxNumberDetails[roundCounter - 1, sBoxctr * 3 + 2];
                    column = EncDiffusion.SBoxNumberDetails[roundCounter - 1, sBoxctr * 3 + 1];
                    row = EncDiffusion.SBoxNumberDetails[roundCounter - 1, sBoxctr * 3 + 0];
                }
                else
                {
                    SBoxOutput.Text = EncOriginal.SBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 3] + "  <-- " + EncOriginal.SBoxNumberDetails[roundCounter - 1, sBoxctr * 3 + 2];
                    column = EncOriginal.SBoxNumberDetails[roundCounter - 1, sBoxctr * 3 + 1];
                    row = EncOriginal.SBoxNumberDetails[roundCounter - 1, sBoxctr * 3 + 0];
                }
                // Set SBoxJumper at the right place                
                SBoxJumper.Visibility = Visibility.Visible;
                if (column < 10) Canvas.SetLeft(SBoxJumper, 77 + column * 21.556);
                else Canvas.SetLeft(SBoxJumper, 297 + (column % 10) * 25.4);
                Canvas.SetTop(SBoxJumper, 63 + row * 20.45);

            }
            if (step >= 4)
            {
                SBoxOut.Visibility = Visibility.Visible;

                if (diffusionActive)
                {
                    SBoxOut.Text += EncDiffusion.SBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 3];
                    colorText(SBoxOut, compareStrings(EncDiffusion.RoundDetails[roundCounter - 1, 2], EncOriginal.RoundDetails[roundCounter - 1, 2]));
                }
                else
                {
                    SBoxOut.Text += EncOriginal.SBoxStringDetails[roundCounter - 1, sBoxctr * 4 + 3];
                }

                if (sBoxctr == 7)
                {
                    screenCounter = 12;
                    stepCounter = 5;
                    return;
                }

            }
            screenCounter = 15;
            stepCounter++;

        }

        public void showShift1Screen(int fullStep)      //fängt bei 0 an !!!
        {
            resetAllScreens(true);
            bool firstShift;
            if (fullStep / 2 == 0) firstShift = true;
            else firstShift = false;
            int step = fullStep % 2;
            Shift1Screen.Visibility = Visibility.Visible;
            title.Visibility = Visibility.Visible;
            title.Content = "Cyclic Shift";
            if (DESImplementation.byteShifts[roundCounter - 1] == 1) singleShift.Visibility = Visibility.Visible;
            else doubleShift.Visibility = Visibility.Visible;

            if (firstShift)
            {
                Shift_topName.Content = "C" + (roundCounter - 1);
                Shift_bottomName.Content = "C" + roundCounter;
                if (diffusionActive)
                {
                    Shift_top.Text = insertSpaces(EncDiffusion.KeySchedule[roundCounter - 1, 0]);
                    colorText(Shift_top, compareStrings(insertSpaces(EncOriginal.KeySchedule[roundCounter - 1, 0]), Shift_top.Text));
                    Shift_bottom.Text = insertSpaces(EncDiffusion.KeySchedule[roundCounter, 0]);
                    colorText(Shift_bottom, compareStrings(insertSpaces(EncOriginal.KeySchedule[roundCounter, 0]), Shift_bottom.Text));
                }
                else
                {
                    Shift_top.Text = insertSpaces(EncOriginal.KeySchedule[roundCounter - 1, 0]);
                    Shift_bottom.Text = insertSpaces(EncOriginal.KeySchedule[roundCounter, 0]);
                }
                if (step == 0)
                {
                    clearButtonsColor(true);
                    ShiftButton.Background = buttonBrush;
                }
            }
            else
            {
                Shift_topName.Content = "D" + (roundCounter - 1);
                Shift_bottomName.Content = "D" + roundCounter;
                if (diffusionActive)
                {
                    Shift_top.Text = insertSpaces(EncDiffusion.KeySchedule[roundCounter - 1, 1]);
                    colorText(Shift_top, compareStrings(insertSpaces(EncOriginal.KeySchedule[roundCounter - 1, 1]), Shift_top.Text));
                    Shift_bottom.Text = insertSpaces(EncDiffusion.KeySchedule[roundCounter, 1]);
                    colorText(Shift_bottom, compareStrings(insertSpaces(EncOriginal.KeySchedule[roundCounter, 1]), Shift_bottom.Text));
                }
                else
                {
                    Shift_top.Text = insertSpaces(EncOriginal.KeySchedule[roundCounter - 1, 1]);
                    Shift_bottom.Text = insertSpaces(EncOriginal.KeySchedule[roundCounter, 1]);
                }
            }

            if (step == 1)
            {
                Shift_bottom.Visibility = Visibility.Visible;
                if (firstShift)
                {
                    screenCounter = 6;
                    stepCounter = 3;
                }
                else
                {
                    screenCounter = 6;
                    stepCounter = 4;
                }
            }
            else
            {
                if (firstShift)
                {
                    screenCounter = 7;
                    stepCounter = 1;
                }
                else
                {
                    screenCounter = 7;
                    stepCounter = 3;
                }
            }

            if (roundCounter < 10) Canvas.SetLeft(RoundTable, 97 + (roundCounter - 1) * 21.625);
            else Canvas.SetLeft(RoundTable, 296 + (roundCounter - 10) * 30.3333);

        }

        public void showStructureScreen()
        {
            resetAllScreens(true);
            StructureScreen.Visibility = Visibility.Visible;
            title.Visibility = Visibility.Visible;
            title.Content = "General Structure";

            screenCounter = 1;
            stepCounter = 2;
        }

        public void showKeyScheduleScreen(int step)
        {
            resetAllScreens(true);
            if (step == 1)
            {
                stepCounter = 2;
                screenCounter = 6;
                roundCounter++;
                if (roundCounter == 1)
                {
                    clearButtonsColor(false);
                    activateRoundButtons(true);
                    keySchedule = true;
                    SkipStepButton.Content = "Skip Round";
                }
                ShiftButton.Visibility = Visibility.Visible;
                PC2Button.Visibility = Visibility.Visible;
                clearButtonsColor(true);
                colorRoundKeys();
                KeyScheduleButton.Background = buttonBrush;
            }
            KeyScheduleScreen.Visibility = Visibility.Visible;
            KeyScheduleRoundKey.Content = "" + roundCounter;
            KeyScheduleLabel.Content = "Round " + roundCounter + "/16";
            KeyScheduleCRoundName.Content = "C" + (roundCounter - 1) + ":";
            KeyScheduleDRoundName.Content = "D" + (roundCounter - 1) + ":";
            if (diffusionActive)
            {
                KeyScheduleCRound.Text = EncDiffusion.KeySchedule[roundCounter - 1, 0];
                colorText(KeyScheduleCRound, compareStrings(EncOriginal.KeySchedule[roundCounter - 1, 0], KeyScheduleCRound.Text));
                KeyScheduleDRound.Text = EncDiffusion.KeySchedule[roundCounter - 1, 1];
                colorText(KeyScheduleDRound, compareStrings(EncOriginal.KeySchedule[roundCounter - 1, 1], KeyScheduleDRound.Text));
            }
            else
            {
                KeyScheduleCRound.Text = EncOriginal.KeySchedule[roundCounter - 1, 0];
                KeyScheduleDRound.Text = EncOriginal.KeySchedule[roundCounter - 1, 1];
            }
            if (roundCounter == 1)
            {
                KeyScheduleTopArrowRound1.Visibility = Visibility.Visible;
                KeySchedulePC1Label.Visibility = Visibility.Visible;
                KeySchedulePC1Box.Visibility = Visibility.Visible;
                KeyScheduleTopLine.Visibility = Visibility.Visible;
                Canvas.SetTop(KeyScheduleTopLine, 149.18);
                KeyScheduleDownArrow1.Visibility = Visibility.Visible;
                KeyScheduleDownArrow2.Visibility = Visibility.Visible;
            }
            else if (roundCounter == 16)
            {
                KeyScheduleTopLine.Visibility = Visibility.Visible;
                Canvas.SetTop(KeyScheduleTopLine, 720);
                KeyScheduleRightArrowRound2.Visibility = Visibility.Visible;
                KeyScheduleLeftArrowRound2.Visibility = Visibility.Visible;
            }
            else
            {
                KeyScheduleRightArrowRound2.Visibility = Visibility.Visible;
                KeyScheduleLeftArrowRound2.Visibility = Visibility.Visible;
                KeyScheduleDownArrow1.Visibility = Visibility.Visible;
                KeyScheduleDownArrow2.Visibility = Visibility.Visible;
            }
            if (step >= 2)
            {
                KeyScheduleShiftBox1.Fill = yellowBrush;
                screenCounter = 7;
                stepCounter = 0;
            }
            if (step >= 3)
            {
                KeyScheduleShiftBox1.Fill = greenBrush;
                KeyScheduleShiftBox2.Fill = yellowBrush;
                screenCounter = 7;
                stepCounter = 2;
            }
            if (step >= 4)
            {
                KeyScheduleShiftBox2.Fill = greenBrush;
                KeySchedulePC2Box.Fill = yellowBrush;
                screenCounter = 8;
                stepCounter = 1;
            }
            if (step >= 5)
            {
                KeySchedulePC2Box.Fill = greenBrush;
                screenCounter = 9;
                stepCounter = 1;
            }


        }

        public void showDESRoundScreen(int step)
        {
            resetAllScreens(true);
            if (step == 1)
            {
                stepCounter = 2;
                screenCounter = 11;
                roundCounter++;
                if (roundCounter == 1)
                {
                    clearButtonsColor(false);
                    activateRoundButtons(true);
                    desRounds = true;
                    SkipStepButton.Content = "Skip Round";
                }
                ExpansionButton.Visibility = Visibility.Visible;
                KeyAdditionButton.Visibility = Visibility.Visible;
                SBoxButton.Visibility = Visibility.Visible;
                PermutationButton.Visibility = Visibility.Visible;
                RoundAdditionButton.Visibility = Visibility.Visible;
                RoundAdditionButton.Content = "L" + (roundCounter - 1) + " ⊕ f (R" + (roundCounter - 1) + ")";
                clearButtonsColor(true);
                colorRoundKeys();
                DESButton.Background = buttonBrush;
            }

            DESRoundScreen.Visibility = Visibility.Visible;
            DESRoundKey.Content = "" + roundCounter;
            DESRoundLabel.Content = "Round " + roundCounter + "/16";
            DESRoundR0Name.Content = "R" + (roundCounter - 1) + ":";
            DESRoundL0Name.Content = "L" + (roundCounter - 1) + ":";
            DESRoundR1Name.Content = "R" + (roundCounter) + ":";
            DESRoundL1Name.Content = "L" + (roundCounter) + ":";
            if (diffusionActive)
            {
                DESRoundL0.Text = EncDiffusion.LR_Data[roundCounter - 1, 0];
                colorText(DESRoundL0, compareStrings(EncOriginal.LR_Data[roundCounter - 1, 0], DESRoundL0.Text));
                DESRoundR0.Text = EncDiffusion.LR_Data[roundCounter - 1, 1];
                colorText(DESRoundR0, compareStrings(EncOriginal.LR_Data[roundCounter - 1, 1], DESRoundR0.Text));

                DESRoundL1.Text = EncDiffusion.LR_Data[roundCounter, 0];
                colorText(DESRoundL1, compareStrings(EncOriginal.LR_Data[roundCounter, 0], DESRoundL1.Text));
                DESRoundR1.Text = EncDiffusion.LR_Data[roundCounter, 1];
                colorText(DESRoundR1, compareStrings(EncOriginal.LR_Data[roundCounter, 1], DESRoundR1.Text));
            }
            else
            {
                DESRoundL0.Text = EncOriginal.LR_Data[roundCounter - 1, 0];
                DESRoundR0.Text = EncOriginal.LR_Data[roundCounter - 1, 1];

                DESRoundL1.Text = EncOriginal.LR_Data[roundCounter, 0];
                DESRoundR1.Text = EncOriginal.LR_Data[roundCounter, 1];
            }
            if (roundCounter == 1)
            {
                DESRoundTopLine.Visibility = Visibility.Visible;
                Canvas.SetTop(DESRoundTopLine, 20.18);
            }
            else if (roundCounter == 16)
            {
                DESRoundTopLine.Visibility = Visibility.Visible;
                Canvas.SetTop(DESRoundTopLine, 685.18);
            }

            if (step >= 2)
            {
                DESRoundFunctionPath.Fill = yellowBrush;
                DESRoundL1Name.Visibility = Visibility.Visible;
                DESRoundL1.Visibility = Visibility.Visible;
                screenCounter = 12;
                stepCounter = 1;
            }
            if (step >= 3)
            {
                DESRoundFunctionPath.Fill = greenBrush;
                RoundAdditionPath.Fill = yellowBrush;
                screenCounter = 14;
                stepCounter = 2;
            }
            if (step >= 4)
            {
                RoundAdditionPath.Fill = greenBrush;
                DESRoundR1Name.Visibility = Visibility.Visible;
                DESRoundR1.Visibility = Visibility.Visible;
                screenCounter = 17;
                stepCounter = 1;
            }
        }

        public void showRoundFunctionScreen(int step)
        {
            resetAllScreens(true);
            RoundFunctionScreen.Visibility = Visibility.Visible;
            FScreenFunctionR.Content = "" + (roundCounter - 1);
            FScreenFunctionKey.Content = "" + roundCounter;
            FScreenFunctionInfoKey.Content = "" + roundCounter;
            FScreenFunctionInfoR.Content = "" + (roundCounter - 1);
            FScreenFunctionInfoRound.Content = "" + roundCounter;

            if (step == 1)
            {
                stepCounter = 2;
                screenCounter = 12;
            }
            if (step >= 2)
            {
                ExpansionPath.Fill = yellowBrush;
                screenCounter = 13;
                stepCounter = 1;
            }
            if (step >= 3)
            {
                ExpansionPath.Fill = greenBrush;
                FScreenXORPath.Fill = yellowBrush;
                screenCounter = 14;
                stepCounter = 0;
            }
            if (step >= 4)
            {
                FScreenXORPath.Fill = greenBrush;
                FScreenSPath1.Fill = yellowBrush;
                FScreenSPath2.Fill = yellowBrush;
                FScreenSPath3.Fill = yellowBrush;
                FScreenSPath4.Fill = yellowBrush;
                FScreenSPath5.Fill = yellowBrush;
                FScreenSPath6.Fill = yellowBrush;
                FScreenSPath7.Fill = yellowBrush;
                FScreenSPath8.Fill = yellowBrush;
                screenCounter = 15;
                stepCounter = 0;
            }
            if (step >= 5)
            {
                FScreenSPath1.Fill = greenBrush;
                FScreenSPath2.Fill = greenBrush;
                FScreenSPath3.Fill = greenBrush;
                FScreenSPath4.Fill = greenBrush;
                FScreenSPath5.Fill = greenBrush;
                FScreenSPath6.Fill = greenBrush;
                FScreenSPath7.Fill = greenBrush;
                FScreenSPath8.Fill = greenBrush;
                FScreenPPermutationBox.Fill = yellowBrush;
                screenCounter = 16;
                stepCounter = 1;
            }
            if (step >= 6)
            {
                FScreenPPermutationBox.Fill = greenBrush;
                screenCounter = 11;
                stepCounter = 3;
            }
        }

        public void showXORScreen(int fullStep)
        {
            resetAllScreens(true);
            bool keyAddition;
            if (fullStep / 2 == 0) keyAddition = true;
            else keyAddition = false;
            int step = fullStep % 2;
            XORScreen.Visibility = Visibility.Visible;
            title.Visibility = Visibility.Visible;
            title.Content = "XOR Operation";
            if (keyAddition)
            {
                XOROperator1Name.Content = "K" + roundCounter + ":";
                XOROperator2Name.Content = "Exp:";
                XORResultName.Content = "SBox:";
                if (diffusionActive)
                {
                    XOROperator1.Text = EncDiffusion.RoundKeys[roundCounter - 1];
                    colorText(XOROperator1, compareStrings(EncOriginal.RoundKeys[roundCounter - 1], XOROperator1.Text));
                    XOROperator2.Text = EncDiffusion.RoundDetails[roundCounter - 1, 0];
                    colorText(XOROperator2, compareStrings(EncOriginal.RoundDetails[roundCounter - 1, 0], XOROperator2.Text));
                    XORResult.Text = EncDiffusion.RoundDetails[roundCounter - 1, 1];
                    colorText(XORResult, compareStrings(EncOriginal.RoundDetails[roundCounter - 1, 1], XORResult.Text));
                }
                else
                {
                    XOROperator1.Text = EncOriginal.RoundKeys[roundCounter - 1];
                    XOROperator2.Text = EncOriginal.RoundDetails[roundCounter - 1, 0];
                    XORResult.Text = EncOriginal.RoundDetails[roundCounter - 1, 1];
                }
                if (step == 0)
                {
                    clearButtonsColor(true);
                    KeyAdditionButton.Background = buttonBrush;
                }
            }
            else
            {
                XOROperator1Name.Content = "L" + (roundCounter - 1) + ":";
                XOROperator2Name.Content = "f(R" + (roundCounter - 1) + "):";
                XORResultName.Content = "R" + roundCounter + ":";
                if (diffusionActive)
                {
                    XOROperator1.Text = EncDiffusion.LR_Data[roundCounter - 1, 0];
                    colorText(XOROperator1, compareStrings(EncOriginal.LR_Data[roundCounter - 1, 0], XOROperator1.Text));
                    XOROperator2.Text = EncDiffusion.RoundDetails[roundCounter - 1, 3];
                    colorText(XOROperator2, compareStrings(EncOriginal.RoundDetails[roundCounter - 1, 3], XOROperator2.Text));
                    XORResult.Text = EncDiffusion.LR_Data[roundCounter, 1];
                    colorText(XORResult, compareStrings(EncOriginal.LR_Data[roundCounter, 1], XORResult.Text));
                }
                else
                {
                    XOROperator1.Text = EncOriginal.LR_Data[roundCounter - 1, 0];
                    XOROperator2.Text = EncOriginal.RoundDetails[roundCounter - 1, 3];
                    XORResult.Text = EncOriginal.LR_Data[roundCounter, 1];
                }
                if (step == 0)
                {
                    clearButtonsColor(true);
                    RoundAdditionButton.Background = buttonBrush;
                }
            }
            FScreenFunctionR.Content = "" + (roundCounter - 1);
            FScreenFunctionKey.Content = "" + roundCounter;
            FScreenFunctionInfoKey.Content = "" + roundCounter;
            FScreenFunctionInfoR.Content = "" + (roundCounter - 1);
            FScreenFunctionInfoRound.Content = "" + roundCounter;

            if (step == 1)
            {
                XORResult.Visibility = Visibility.Visible;
                XORResultName.Visibility = Visibility.Visible;
                if (keyAddition)
                {
                    screenCounter = 12;
                    stepCounter = 4;
                }
                else
                {
                    screenCounter = 11;
                    stepCounter = 4;
                }
            }
            else
            {
                if (keyAddition)
                {
                    screenCounter = 14;
                    stepCounter = 1;
                }
                else
                {
                    screenCounter = 14;
                    stepCounter = 3;
                }
            }
        }       //fängt bei 0 an !!!

        public void showIPScreen(int step)
        {
            resetAllScreens(true);
            IPScreen.Visibility = Visibility.Visible;
            title.Visibility = Visibility.Visible;
            title.Content = "Initial Permutation";
            roundCounter = 0;

            if (diffusionActive)
            {
                IP_top.Text = EncDiffusion.message;
                colorText(IP_top, compareStrings(EncOriginal.message, IP_top.Text));

                String old = EncOriginal.LR_Data[0, 0] + EncOriginal.LR_Data[0, 1];
                String changed = EncDiffusion.LR_Data[0, 0] + EncDiffusion.LR_Data[0, 1];
                IP_bottom.Text = changed;
                colorText(IP_bottom, compareStrings(old, changed));
            }
            else
            {
                IP_top.Text = EncOriginal.message;
                IP_bottom.Text = EncOriginal.LR_Data[0, 0] + EncOriginal.LR_Data[0, 1];
            }
            if (step == 2)
            {
                IP_bottom.Visibility = Visibility.Visible;
                screenCounter = 11;
                stepCounter = 1;
            }
            else
            {
                stepCounter = 2;
                screenCounter = 10;
                clearButtonsColor(false);
                IPButton.Background = buttonBrush;
            }

        }

        public void showPC1Screen(int step)
        {
            resetAllScreens(true);
            PC1Screen.Visibility = Visibility.Visible;
            title.Visibility = Visibility.Visible;
            title.Content = "Permuted Choice 1";
            roundCounter = 0;

            if (diffusionActive)
            {
                PC1_top.Text = EncDiffusion.key;
                colorText(PC1_top, compareStrings(EncOriginal.key, PC1_top.Text));

                String old = EncOriginal.KeySchedule[0, 0] + EncOriginal.KeySchedule[0, 1];
                String changed = EncDiffusion.KeySchedule[0, 0] + EncDiffusion.KeySchedule[0, 1];
                PC1_bottom.Text = changed;
                colorText(PC1_bottom, compareStrings(old, changed));
            }
            else
            {
                PC1_top.Text = EncOriginal.key;
                PC1_bottom.Text = EncOriginal.KeySchedule[0, 0] + EncOriginal.KeySchedule[0, 1];
            }
            if (step == 2)
            {
                PC1_bottom.Visibility = Visibility.Visible;
                screenCounter = 6;
                stepCounter = 1;
            }
            else
            {
                stepCounter = 2;
                screenCounter = 5;
                clearButtonsColor(false);
                PC1Button.Background = buttonBrush;
            }

        }

        public void showPC2Screen(int step)
        {
            resetAllScreens(true);
            PC2Screen.Visibility = Visibility.Visible;
            title.Visibility = Visibility.Visible;
            title.Content = "Permuted Choice 2";

            if (diffusionActive)
            {
                String old = EncOriginal.KeySchedule[roundCounter, 0] + EncOriginal.KeySchedule[roundCounter, 1];
                String changed = EncDiffusion.KeySchedule[roundCounter, 0] + EncDiffusion.KeySchedule[roundCounter, 1];
                PC2_top.Text = changed;
                colorText(PC2_top, compareStrings(old, changed));


                PC2_bottom.Text = EncDiffusion.RoundKeys[roundCounter - 1];
                colorText(PC2_bottom, compareStrings(EncOriginal.RoundKeys[roundCounter - 1], PC2_bottom.Text));
            }
            else
            {
                PC2_top.Text = EncOriginal.KeySchedule[roundCounter, 0] + EncOriginal.KeySchedule[roundCounter, 1];
                PC2_bottom.Text = EncOriginal.RoundKeys[roundCounter - 1];
            }
            if (step == 2)
            {
                PC2_bottom.Visibility = Visibility.Visible;
                screenCounter = 6;
                stepCounter = 5;
            }
            else
            {
                stepCounter = 2;
                screenCounter = 8;
                clearButtonsColor(true);
                PC2Button.Background = buttonBrush;
            }

        }

        public void showExpansionScreen(int step)
        {
            resetAllScreens(true);
            ExpansionScreen.Visibility = Visibility.Visible;
            title.Visibility = Visibility.Visible;
            title.Content = "Expansion";

            if (diffusionActive)
            {
                Expansion_top.Text = EncDiffusion.LR_Data[roundCounter - 1, 1];
                colorText(Expansion_top, compareStrings(EncOriginal.LR_Data[roundCounter - 1, 1], Expansion_top.Text));


                Expansion_bottom.Text = EncDiffusion.RoundDetails[roundCounter - 1, 0];
                colorText(Expansion_bottom, compareStrings(EncOriginal.RoundDetails[roundCounter - 1, 0], Expansion_bottom.Text));
            }
            else
            {
                Expansion_top.Text = EncOriginal.LR_Data[roundCounter - 1, 1];
                Expansion_bottom.Text = EncOriginal.RoundDetails[roundCounter - 1, 0];
            }
            if (step == 2)
            {
                Expansion_bottom.Visibility = Visibility.Visible;
                screenCounter = 12;
                stepCounter = 3;
            }
            else
            {
                stepCounter = 2;
                screenCounter = 13;
                clearButtonsColor(true);
                ExpansionButton.Background = buttonBrush;
            }



        }

        public void showPPScreen(int step)
        {
            resetAllScreens(true);
            PPScreen.Visibility = Visibility.Visible;
            title.Visibility = Visibility.Visible;
            title.Content = "P-Permutation";

            if (diffusionActive)
            {
                PP_top.Text = EncDiffusion.RoundDetails[roundCounter - 1, 2];
                colorText(PP_top, compareStrings(EncOriginal.RoundDetails[roundCounter - 1, 2], PP_top.Text));


                PP_bottom.Text = EncDiffusion.RoundDetails[roundCounter - 1, 3];
                colorText(PP_bottom, compareStrings(EncOriginal.RoundDetails[roundCounter - 1, 3], PP_bottom.Text));
            }
            else
            {
                PP_top.Text = EncOriginal.RoundDetails[roundCounter - 1, 2];
                PP_bottom.Text = EncOriginal.RoundDetails[roundCounter - 1, 3];
            }
            if (step == 2)
            {
                PP_bottom.Visibility = Visibility.Visible;
                screenCounter = 12;
                stepCounter = 6;
            }
            else
            {
                stepCounter = 2;
                screenCounter = 16;
                clearButtonsColor(true);
                PermutationButton.Background = buttonBrush;
            }



        }

        public void showFPScreen(int step)
        {
            resetAllScreens(true);
            FPScreen.Visibility = Visibility.Visible;
            title.Visibility = Visibility.Visible;
            title.Content = "Final Permutation";

            if (diffusionActive)
            {
                String old = EncOriginal.LR_Data[16, 1] + EncOriginal.LR_Data[16, 0];
                String changed = EncDiffusion.LR_Data[16, 1] + EncDiffusion.LR_Data[16, 0];
                FP_top.Text = changed;
                colorText(FP_top, compareStrings(old, changed));


                IP_bottom.Text = EncDiffusion.ciphertext;
                colorText(IP_bottom, compareStrings(EncOriginal.ciphertext, EncDiffusion.ciphertext));
            }
            else
            {
                IP_top.Text = EncOriginal.LR_Data[16, 1] + EncOriginal.LR_Data[16, 0];
                IP_bottom.Text = EncOriginal.ciphertext;
            }
            if (step == 2)
            {
                FP_bottom.Visibility = Visibility.Visible;
                screenCounter = 1;
                stepCounter = 4;
            }
            else
            {
                stepCounter = 2;
                screenCounter = 18;
                clearButtonsColor(false);
                FPButton.Background = buttonBrush;
                activateRoundButtons(false);
                clearButtonsColor(true);
                roundCounter = 0;
                SkipStepButton.Content = "Skip Step";
                ExpansionButton.Visibility = Visibility.Hidden;
                KeyAdditionButton.Visibility = Visibility.Hidden;
                SBoxButton.Visibility = Visibility.Hidden;
                PermutationButton.Visibility = Visibility.Hidden;
                RoundAdditionButton.Visibility = Visibility.Hidden;
                desRounds = false;
            }

        }

        // Reset-Methods

        public void resetIntroScreen()
        {
            IntroScreen.Visibility = Visibility.Hidden;
        }

        public void resetInfoScreen()
        {
            InfoScreen.Visibility = Visibility.Hidden;
            InfoText.Visibility = Visibility.Hidden;
            HistoryText.Visibility = Visibility.Hidden;
            title.Content = "";
            title.Visibility = Visibility.Hidden;
        }

        public void resetExecutionScreen()
        {
            ExecutionScreen.Visibility = Visibility.Hidden;
            ExecutionLabel.Content = "";
        }

        public void resetFinalScreen()
        {
            FinalScreen.Visibility = Visibility.Hidden;
        }

        public void resetDataScreen()
        {
            DataScreen.Visibility = Visibility.Hidden;
            DiffusionTButton.IsChecked = false;
            DiffusionClearButton.Visibility = Visibility.Hidden;
            DiffusionOKButton.Visibility = Visibility.Hidden;
            showDiffusionBoxes(false);                                  //DiffBoxen ausblenden, Werte lassen
            title.Content = "";
            title.Visibility = Visibility.Hidden;
        }

        public void resetRoundDataScreen()
        {
            RoundDataScreen.Visibility = Visibility.Hidden;
            L0.Visibility = Visibility.Hidden;
            L1.Visibility = Visibility.Hidden;
            L2.Visibility = Visibility.Hidden;
            L3.Visibility = Visibility.Hidden;
            L4.Visibility = Visibility.Hidden;
            L5.Visibility = Visibility.Hidden;
            L6.Visibility = Visibility.Hidden;
            L7.Visibility = Visibility.Hidden;
            L8.Visibility = Visibility.Hidden;
            L9.Visibility = Visibility.Hidden;
            L10.Visibility = Visibility.Hidden;
            L11.Visibility = Visibility.Hidden;
            L12.Visibility = Visibility.Hidden;
            L13.Visibility = Visibility.Hidden;
            L14.Visibility = Visibility.Hidden;
            L15.Visibility = Visibility.Hidden;
            L16.Visibility = Visibility.Hidden;
            R0.Visibility = Visibility.Hidden;
            R1.Visibility = Visibility.Hidden;
            R2.Visibility = Visibility.Hidden;
            R3.Visibility = Visibility.Hidden;
            R4.Visibility = Visibility.Hidden;
            R5.Visibility = Visibility.Hidden;
            R6.Visibility = Visibility.Hidden;
            R7.Visibility = Visibility.Hidden;
            R8.Visibility = Visibility.Hidden;
            R9.Visibility = Visibility.Hidden;
            R10.Visibility = Visibility.Hidden;
            R11.Visibility = Visibility.Hidden;
            R12.Visibility = Visibility.Hidden;
            R13.Visibility = Visibility.Hidden;
            R14.Visibility = Visibility.Hidden;
            R15.Visibility = Visibility.Hidden;
            R16.Visibility = Visibility.Hidden;
            ArrowRounds.Margin = new Thickness(0, 0, 579, 220);
            ArrowRounds.Visibility = Visibility.Visible;
        }

        public void resetRoundKeyDataScreen()
        {
            RoundKeyDataScreen.Visibility = Visibility.Hidden;
            K1.Visibility = Visibility.Hidden;
            K2.Visibility = Visibility.Hidden;
            K3.Visibility = Visibility.Hidden;
            K4.Visibility = Visibility.Hidden;
            K5.Visibility = Visibility.Hidden;
            K6.Visibility = Visibility.Hidden;
            K7.Visibility = Visibility.Hidden;
            K8.Visibility = Visibility.Hidden;
            K9.Visibility = Visibility.Hidden;
            K10.Visibility = Visibility.Hidden;
            K11.Visibility = Visibility.Hidden;
            K12.Visibility = Visibility.Hidden;
            K13.Visibility = Visibility.Hidden;
            K14.Visibility = Visibility.Hidden;
            K15.Visibility = Visibility.Hidden;
            K16.Visibility = Visibility.Hidden;
            ArrowSubKeys.Margin = new Thickness(30, 0, 579, 220);
            ArrowSubKeys.Visibility = Visibility.Visible;
        }

        public void resetSBoxScreen(bool full)
        {
            SBoxScreen.Visibility = Visibility.Hidden;
            S1Box.Visibility = Visibility.Hidden;
            S2Box.Visibility = Visibility.Hidden;
            S3Box.Visibility = Visibility.Hidden;
            S4Box.Visibility = Visibility.Hidden;
            S5Box.Visibility = Visibility.Hidden;
            S6Box.Visibility = Visibility.Hidden;
            S7Box.Visibility = Visibility.Hidden;
            S8Box.Visibility = Visibility.Hidden;
            SBoxInput.Visibility = Visibility.Hidden;
            SBoxRow.Visibility = Visibility.Hidden;
            SBoxColumn.Visibility = Visibility.Hidden;
            SBoxOutput.Visibility = Visibility.Hidden;
            SBoxJumper.Visibility = Visibility.Hidden;
            Canvas.SetLeft(SBoxJumper, 53);
            Canvas.SetTop(SBoxJumper, 63);
            title.Content = "";
            title.Visibility = Visibility.Hidden;
            if (full)
            {
                SBoxOut.Visibility = Visibility.Hidden;
                SBoxOut.Text = "";
            }


            //Counter auf 1 setzen vllt (1-8)                
        }

        public void resetShift1Screen()
        {
            Shift1Screen.Visibility = Visibility.Hidden;
            Canvas.SetLeft(RoundTable, 97);
            Canvas.SetTop(RoundTable, 166);
            Shift_bottom.Visibility = Visibility.Hidden;
            singleShift.Visibility = Visibility.Hidden;
            doubleShift.Visibility = Visibility.Hidden;
            Canvas.SetLeft(RoundTable, 97);
            title.Content = "";
            title.Visibility = Visibility.Hidden;
        }

        public void resetStructureScreen()
        {
            StructureScreen.Visibility = Visibility.Hidden;
            title.Content = "";
            title.Visibility = Visibility.Hidden;
            //evtl. Farben wieder auf default setzen
        }

        public void resetKeyScheduleScreen()
        {
            KeyScheduleScreen.Visibility = Visibility.Hidden;
            KeyScheduleTopLine.Visibility = Visibility.Hidden;
            KeyScheduleTopArrowRound1.Visibility = Visibility.Hidden;
            KeyScheduleRightArrowRound2.Visibility = Visibility.Hidden;
            KeyScheduleLeftArrowRound2.Visibility = Visibility.Hidden;
            KeyScheduleDownArrow2.Visibility = Visibility.Hidden;
            KeyScheduleDownArrow1.Visibility = Visibility.Hidden;
            KeySchedulePC1Box.Visibility = Visibility.Hidden;
            KeySchedulePC1Label.Visibility = Visibility.Hidden;
            KeyScheduleShiftBox1.ClearValue(Rectangle.FillProperty);
            KeyScheduleShiftBox2.ClearValue(Rectangle.FillProperty);
            KeySchedulePC2Box.ClearValue(Rectangle.FillProperty);
        }

        public void resetDESRoundScreen()
        {
            DESRoundScreen.Visibility = Visibility.Hidden;
            DESRoundTopLine.Visibility = Visibility.Hidden;

            DESRoundFunctionPath.ClearValue(Path.FillProperty);
            RoundAdditionPath.ClearValue(Path.FillProperty);

            DESRoundL1Name.Visibility = Visibility.Hidden;
            DESRoundR1Name.Visibility = Visibility.Hidden;
            DESRoundL1.Visibility = Visibility.Hidden;
            DESRoundR1.Visibility = Visibility.Hidden;
        }

        public void resetRoundFunctionScreen()
        {
            RoundFunctionScreen.Visibility = Visibility.Hidden;

            ExpansionPath.ClearValue(Path.FillProperty);
            FScreenXORPath.ClearValue(Path.FillProperty);
            FScreenSPath1.ClearValue(Path.FillProperty);
            FScreenSPath2.ClearValue(Path.FillProperty);
            FScreenSPath3.ClearValue(Path.FillProperty);
            FScreenSPath4.ClearValue(Path.FillProperty);
            FScreenSPath5.ClearValue(Path.FillProperty);
            FScreenSPath6.ClearValue(Path.FillProperty);
            FScreenSPath7.ClearValue(Path.FillProperty);
            FScreenSPath8.ClearValue(Path.FillProperty);
            FScreenPPermutationBox.ClearValue(Rectangle.FillProperty);

        }

        public void resetXORScreen()
        {
            XORScreen.Visibility = Visibility.Hidden;
            XORResult.Visibility = Visibility.Hidden;
            XORResultName.Visibility = Visibility.Hidden;
            title.Content = "";
            title.Visibility = Visibility.Hidden;
        }

        public void resetIPScreen()
        {
            IPScreen.Visibility = Visibility.Hidden;
            IP_bottom.Visibility = Visibility.Hidden;

            title.Content = "";
            title.Visibility = Visibility.Hidden;
        }

        public void resetPC1Screen()
        {
            PC1Screen.Visibility = Visibility.Hidden;
            PC1_bottom.Visibility = Visibility.Hidden;

            title.Content = "";
            title.Visibility = Visibility.Hidden;
        }

        public void resetPC2Screen()
        {
            PC2Screen.Visibility = Visibility.Hidden;
            PC2_bottom.Visibility = Visibility.Hidden;

            title.Content = "";
            title.Visibility = Visibility.Hidden;
        }

        public void resetExpansionScreen()
        {
            ExpansionScreen.Visibility = Visibility.Hidden;
            Expansion_bottom.Visibility = Visibility.Hidden;

            title.Content = "";
            title.Visibility = Visibility.Hidden;
        }

        public void resetPPScreen()
        {
            PPScreen.Visibility = Visibility.Hidden;
            PP_bottom.Visibility = Visibility.Hidden;

            title.Content = "";
            title.Visibility = Visibility.Hidden;
        }

        public void resetFPScreen()
        {
            FPScreen.Visibility = Visibility.Hidden;
            FP_bottom.Visibility = Visibility.Hidden;

            title.Content = "";
            title.Visibility = Visibility.Hidden;
        }


        #endregion Screen-Methods

        /////////////////////////////////////////////////////////////
        #region Helper-Methods

        private void showDiffusionBoxes(bool show)
        {
            IEnumerator<CheckBox> enumerator = diffusionBoxes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (show)
                    enumerator.Current.Visibility = Visibility.Visible;
                else
                    enumerator.Current.Visibility = Visibility.Hidden;
            };
        }

        private void getDiffusionBoxes()
        {
            diffusionBoxes = DiffusionGrid.Children.OfType<CheckBox>();
        }

        private List<byte> compareStrings(String old, String changed)
        {
            List<byte> tmp = new List<byte>();
            char[] oldArray = old.ToCharArray();
            char[] changedArray = changed.ToCharArray();

            for (byte i = 0; i < old.Length; i++)
            {
                if (!oldArray[i].Equals(changedArray[i]))
                {
                    tmp.Add(i);
                }
            }
            return tmp;
        }

        private void switchStringBit(TextBlock text, int pos)
        {
            String tmp = text.Text;
            if (tmp.ElementAt(pos).Equals('0'))
            {
                tmp = tmp.Remove(pos, 1);
                tmp = tmp.Insert(pos, "1");
            }
            else
            {
                tmp = tmp.Remove(pos, 1);
                tmp = tmp.Insert(pos, "0");
            }
            text.Text = tmp;

        }

        private void colorText(TextBlock text, List<byte> pos)
        {
            byte[] changePos = pos.ToArray();
            clearTexteffects(text);
            for (byte i = 0; i < changePos.Length; i++)
            {
                colorTextSingle(text, (byte)(changePos[i]));
            }


        }

        private void colorTextSingle(TextBlock text, byte pos)
        {
            TextEffect te = new TextEffect();
            te.PositionStart = pos;
            te.Foreground = Brushes.Red;
            te.PositionCount = 1;
            text.TextEffects.Add(te);
        }

        private void clearTexteffects(TextBlock text)
        {
            text.TextEffects.Clear();
        }

        private byte[] stringToByteArray(String str)
        {
            char[] strArray = str.ToCharArray();
            String[] byteStrings = new String[8];
            byteStrings[0] = "" + strArray[0] + strArray[1] + strArray[2] + strArray[3] + strArray[4] + strArray[5] + strArray[6] + strArray[7];
            byteStrings[1] = "" + strArray[8] + strArray[9] + strArray[10] + strArray[11] + strArray[12] + strArray[13] + strArray[14] + strArray[15];
            byteStrings[2] = "" + strArray[16] + strArray[17] + strArray[18] + strArray[19] + strArray[20] + strArray[21] + strArray[22] + strArray[23];
            byteStrings[3] = "" + strArray[24] + strArray[25] + strArray[26] + strArray[27] + strArray[28] + strArray[29] + strArray[30] + strArray[31];
            byteStrings[4] = "" + strArray[32] + strArray[33] + strArray[34] + strArray[35] + strArray[36] + strArray[37] + strArray[38] + strArray[39];
            byteStrings[5] = "" + strArray[40] + strArray[41] + strArray[42] + strArray[43] + strArray[44] + strArray[45] + strArray[46] + strArray[47];
            byteStrings[6] = "" + strArray[48] + strArray[49] + strArray[50] + strArray[51] + strArray[52] + strArray[53] + strArray[54] + strArray[55];
            byteStrings[7] = "" + strArray[56] + strArray[57] + strArray[58] + strArray[59] + strArray[60] + strArray[61] + strArray[62] + strArray[63];
            byte[] byteBytes = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                byteBytes[i] = Convert.ToByte(byteStrings[i], 2);
            }
            return byteBytes;

        }

        private String insertSpaces(String str)
        {
            String tmp = "";
            char[] strArray = str.ToCharArray();
            for (int i = 0; i < strArray.Length; i++)
            {
                tmp += strArray[i] + "  ";
            }
            return tmp;

        }

        private void PlayTimer_Tick(object sender, EventArgs e)
        {
            executeSteps();
        }

        private void resetAllScreens(bool sBoxfull)
        {
            resetIntroScreen();
            resetInfoScreen();
            resetExecutionScreen();
            resetDataScreen();
            resetStructureScreen();
            resetPC1Screen();
            resetKeyScheduleScreen();
            resetShift1Screen();
            resetPC2Screen();
            resetRoundKeyDataScreen();
            resetIPScreen();
            resetDESRoundScreen();
            resetRoundFunctionScreen();
            resetExpansionScreen();
            resetXORScreen();
            resetSBoxScreen(sBoxfull);
            resetPPScreen();
            resetRoundDataScreen();
            resetFPScreen();
            resetFinalScreen();


        }

        private void executeSteps()
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal,(SendOrPostCallback)delegate
            {           
                switch (screenCounter)
                {
                    case 1: showExecutionScreen(stepCounter); break;
                    case 2: showInfoScreen(stepCounter); break;
                    case 3: showDataScreen(); break;
                    case 4: showStructureScreen(); break;
                    case 5: showPC1Screen(stepCounter); break;
                    case 6: showKeyScheduleScreen(stepCounter); break;
                    case 7: showShift1Screen(stepCounter); break;
                    case 8: showPC2Screen(stepCounter); break;
                    case 9: showRoundKeyDataScreen(stepCounter); break;
                    case 10: showIPScreen(stepCounter); break;
                    case 11: showDESRoundScreen(stepCounter); break;
                    case 12: showRoundFunctionScreen(stepCounter); break;
                    case 13: showExpansionScreen(stepCounter); break;
                    case 14: showXORScreen(stepCounter); break;
                    case 15: showSBoxScreen(stepCounter); break;
                    case 16: showPPScreen(stepCounter); break;
                    case 17: showRoundDataScreen(stepCounter); break;
                    case 18: showFPScreen(stepCounter); break;
                    case 19: showFinalScreen(); break;
                    default: break;
                }
            }, null);
        }

        private void activateRoundButtons(bool active)
        {
            round1Button.IsEnabled = active;
            round2Button.IsEnabled = active;
            round3Button.IsEnabled = active;
            round4Button.IsEnabled = active;
            round5Button.IsEnabled = active;
            round6Button.IsEnabled = active;
            round7Button.IsEnabled = active;
            round8Button.IsEnabled = active;
            round9Button.IsEnabled = active;
            round10Button.IsEnabled = active;
            round11Button.IsEnabled = active;
            round12Button.IsEnabled = active;
            round13Button.IsEnabled = active;
            round14Button.IsEnabled = active;
            round15Button.IsEnabled = active;
            round16Button.IsEnabled = active;

        }

        private void clearButtonsColor(bool top)
        {

            if (top)
            {
                ShiftButton.ClearValue(BackgroundProperty);
                PC2Button.ClearValue(BackgroundProperty);
                ExpansionButton.ClearValue(BackgroundProperty);
                KeyAdditionButton.ClearValue(BackgroundProperty);
                SBoxButton.ClearValue(BackgroundProperty);
                PermutationButton.ClearValue(BackgroundProperty);
                RoundAdditionButton.ClearValue(BackgroundProperty);
            }
            else
            {
                IntroButton.ClearValue(BackgroundProperty);
                DataButton.ClearValue(BackgroundProperty);
                PC1Button.ClearValue(BackgroundProperty);
                KeyScheduleButton.ClearValue(BackgroundProperty);
                IPButton.ClearValue(BackgroundProperty);
                DESButton.ClearValue(BackgroundProperty);
                FPButton.ClearValue(BackgroundProperty);
                SummaryButton.ClearValue(BackgroundProperty);
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
                round11Button.ClearValue(BackgroundProperty);
                round12Button.ClearValue(BackgroundProperty);
                round13Button.ClearValue(BackgroundProperty);
                round14Button.ClearValue(BackgroundProperty);
                round15Button.ClearValue(BackgroundProperty);
                round16Button.ClearValue(BackgroundProperty);
            }


        }

        private void colorRoundKeys()
        {
            clearButtonsColor(false);
            switch (roundCounter)
            {
                case 1: round1Button.Background = buttonBrush; break;
                case 2: round2Button.Background = buttonBrush; break;
                case 3: round3Button.Background = buttonBrush; break;
                case 4: round4Button.Background = buttonBrush; break;
                case 5: round5Button.Background = buttonBrush; break;
                case 6: round6Button.Background = buttonBrush; break;
                case 7: round7Button.Background = buttonBrush; break;
                case 8: round8Button.Background = buttonBrush; break;
                case 9: round9Button.Background = buttonBrush; break;
                case 10: round10Button.Background = buttonBrush; break;
                case 11: round11Button.Background = buttonBrush; break;
                case 12: round12Button.Background = buttonBrush; break;
                case 13: round13Button.Background = buttonBrush; break;
                case 14: round14Button.Background = buttonBrush; break;
                case 15: round15Button.Background = buttonBrush; break;
                case 16: round16Button.Background = buttonBrush; break;
                default: break;
            }
        }




        #endregion Helper-Methods

    }
}

