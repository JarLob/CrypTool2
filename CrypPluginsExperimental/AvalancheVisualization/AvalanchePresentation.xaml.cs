using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Documents;
using System.IO;
using System.Linq;


namespace AvalancheVisualization
{
    /// <summary>
    /// Interaction logic for AvaAESPresentation.xaml
    /// </summary>
    public partial class AvalanchePresentation : UserControl
    {

        #region variables
        public int roundNumber = 1;
        public int action = 1;
        public AutoResetEvent buttonNextClickedEvent;
        public byte[][] sBox = new byte[16][];
        public byte[][] states = new byte[40][];
        public byte[][] statesB = new byte[40][];
        public byte[][] keyList = new byte[11][];
        public string[,] lrData = new string[17, 2];
        public string[,] lrDataB = new string[17, 2];

        public byte[] tempState;
        public byte[] textA;
        public byte[] textB;
        public byte[] keyA;
        public byte[] key;
        public byte[] unchangedCipher;
        public byte[] changedCipher;
        public string[] differentBits;
        public int sequencePosition;
        public double avgNrDiffBit;
        double avalanche;
        static Random rnd = new Random();

        public byte[][] roundConstant = new byte[12][];
        public int keysize;
        public double progress;
        public int shift = 0;
        public int mode;
        // public byte[] keyBytes;

        public bool lostFocus = false;
        public int slideNr = 1;
        public DES desDiffusion;
        public AES aesDiffusion;
        public int roundDES;
        public bool singleBitChange = false;


        public string[] leftHalf = new string[32];
        public string[] rightHalf = new string[32];
        public string[] leftHalfB = new string[32];
        public string[] rightHalfB = new string[32];
        public byte[] seqA;
        public byte[] seqB;

        #endregion

        #region constructor

        public AvalanchePresentation()
        {
            InitializeComponent();
            buttonNextClickedEvent = new AutoResetEvent(false);
            inputInBits.IsVisibleChanged += onVisibleChanged;
            initStateTitle.IsVisibleChanged += onTitleChanged;
            modificationGridDES.IsVisibleChanged += onTitleChanged;
            //ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(Int32.MaxValue));
        }
        #endregion

        #region methods

        //loads initial unchanged text
        public void loadInitialState(byte[] initState, byte[] initKey)
        {
            int i = 1;
            int j = 17;
            int k = 1;

            string binSequence = binaryAsString(initState).Replace(" ", "");
            string keyBinSequence = binaryAsString(initKey).Replace(" ", "");


            if (mode == 0)
            {
                while (i <= 16)
                {
                    ((TextBlock)this.FindName("initStateTxtBlock" + i)).Text = initState[i - 1].ToString("X2");
                    i++;
                }

                if (keysize == 0)
                {
                    int index128 = 0;

                    while (j <= 32 && index128 < 16)
                    {
                        ((TextBlock)this.FindName("initStateTxtBlock" + j)).Text = initKey[index128].ToString("X2");
                        j++;
                        index128++;
                    }


                    for (int a = 1; a <= binSequence.Length; a++)
                        ((TextBlock)this.FindName(string.Format("bit{0}", a))).Text = binSequence[a - 1].ToString();
                    for (int a = 1; a <= keyBinSequence.Length; a++)
                        ((TextBlock)this.FindName(string.Format("keyBit{0}", a))).Text = keyBinSequence[a - 1].ToString();
                }
                else if (keysize == 1)
                {


                    while (k <= 24)
                    {
                        ((TextBlock)this.FindName("initStateKey192_" + k)).Text = initKey[k - 1].ToString("X2");
                        k++;

                    }


                    for (int a = 1; a <= binSequence.Length; a++)
                        ((TextBlock)this.FindName(string.Format("bit{0}", a))).Text = binSequence[a - 1].ToString();
                    for (int a = 1; a <= keyBinSequence.Length; a++)
                        ((TextBlock)this.FindName(string.Format("keyBit192_{0}", a))).Text = keyBinSequence[a - 1].ToString();
                }
                else
                {
                    while (k <= 32)
                    {
                        ((TextBlock)this.FindName("initStateKey256_" + k)).Text = initKey[k - 1].ToString("X2");
                        k++;

                    }

                    for (int a = 1; a <= binSequence.Length; a++)
                        ((TextBlock)this.FindName(string.Format("bit{0}", a))).Text = binSequence[a - 1].ToString();

                    for (int a = 1; a <= keyBinSequence.Length; a++)
                        ((TextBlock)this.FindName(string.Format("keyBit256_{0}", a))).Text = keyBinSequence[a - 1].ToString();
                }

            }
            else
            {
                for (int a = 1; a <= binSequence.Length; a++)
                    ((TextBlock)this.FindName(string.Format("desBit{0}", a))).Text = binSequence[a - 1].ToString();
                for (int a = 1; a <= keyBinSequence.Length; a++)
                    ((TextBlock)this.FindName(string.Format("desKeyBit{0}", a))).Text = keyBinSequence[a - 1].ToString();

                string firstHalf = binSequence.Substring(0, 32);
                string secondHalf = binSequence.Substring(32, 32);
                string firstKeyHalf = keyBinSequence.Substring(0, 32);
                string secondKeyHalf = keyBinSequence.Substring(32, 32);

                origTextDES.Text = string.Format("{0}{1}{2}", firstHalf, Environment.NewLine, secondHalf);
                origKeyDES.Text = string.Format("{0}{1}{2}", firstKeyHalf, Environment.NewLine, secondKeyHalf);
            }
        }

        public void loadChangedMsg(byte[] msg, bool textChanged)
        {
            int k = 33;
            int l = 49;
            int i = 1;

            if (mode == 0)
            {

                while (k <= 48 && l <= 64)
                {
                    if (textChanged)
                        ((TextBlock)this.FindName("initStateTxtBlock" + k)).Text = msg[i - 1].ToString("X2");
                    else
                        ((TextBlock)this.FindName("initStateTxtBlock" + l)).Text = key[i - 1].ToString("X2");
                    i++;
                    k++;
                    l++;
                }

            }
            else
            {
                string binSequence = binaryAsString(msg).Replace(" ", "");
                string firstHalf = binSequence.Substring(0, 32);
                string secondHalf = binSequence.Substring(32, 32);

                modTextDES.Text = string.Format("{0}{1}{2}", firstHalf, Environment.NewLine, secondHalf);
            }
        }

        public void loadChangedKey(byte[] newKey)
        {

            if (mode == 0)
            {
                if (keysize == 1)
                {

                    int i = 1;

                    while (i <= 24)
                    {
                        ((TextBlock)this.FindName("modKey192_" + i)).Text = newKey[i - 1].ToString("X2");
                        i++;

                    }

                }
                else if (keysize == 2)
                {
                    int i = 1;

                    while (i <= 32)
                    {
                        ((TextBlock)this.FindName("modKey256_" + i)).Text = newKey[i - 1].ToString("X2");
                        i++;

                    }
                }
                else
                {

                    int l = 49;
                    int i = 1;

                    while (l <= 64)
                    {
                        ((TextBlock)this.FindName("initStateTxtBlock" + l)).Text = newKey[i - 1].ToString("X2");
                        l++;
                        i++;
                    }
                }
            }
            else
            {

                string keyBinSequence = binaryAsString(newKey).Replace(" ", "");

                string firstKeyHalf = keyBinSequence.Substring(0, 32);
                string secondKeyHalf = keyBinSequence.Substring(32, 32);

                modKeyDES.Text = string.Format("{0}{1}{2}", firstKeyHalf, Environment.NewLine, secondKeyHalf);
            }
        }

        public void setAndLoadButtons()
        {


            if (mode == 0)
            {
                aesCheckBox.Visibility = Visibility.Hidden;
                inputInBits.Visibility = Visibility.Hidden;
                explanationTxt.Visibility = Visibility.Hidden;


                modifiedInitialStateGrid.Visibility = Visibility.Visible;
                radioButtons.Visibility = Visibility.Visible;
                setButtonsScrollViewer();

                buttonsPanel.Visibility = Visibility.Visible;
                buttonsTitle.Visibility = Visibility.Visible;

                if (keysize == 1)
                    modifiedKeyGrid192.Visibility = Visibility.Visible;
                else if (keysize == 2)
                    modifiedKeyGrid256.Visibility = Visibility.Visible;
                else
                    modifiedKeyGrid.Visibility = Visibility.Visible;

            }
            else
            {


                inputGridDES.Visibility = Visibility.Hidden;
                modificationGridDES.Visibility = Visibility.Visible;
                buttonsPanel.Visibility = Visibility.Visible;
                buttonsTitle.Visibility = Visibility.Visible;
                setButtonsScrollViewer();
            }
        }

        //Loads byte information into the respective columns
        public void loadBytePropagationData()
        {
            int a = 0;

            List<TextBox> tmp = createTxtBoxList();
            byte[] state = arrangeColumn(states[0]);


            foreach (TextBox tb in tmp)
            {
                tb.Text = tempState[a].ToString("X2");
                a++;
            }

            int i = 1;

            byte[] subBytes1 = arrangeColumn(states[1]);
            byte[] shiftRows1 = arrangeColumn(states[2]);
            byte[] mixColumns1 = arrangeColumn(states[3]);
            byte[] addKey1 = arrangeColumn(states[4]);
            byte[] subBytes2 = arrangeColumn(states[5]);
            byte[] shiftRows2 = arrangeColumn(states[6]);
            byte[] mixColumns2 = arrangeColumn(states[7]);
            byte[] addKey2 = arrangeColumn(states[8]);
            byte[] subBytes3 = arrangeColumn(states[9]);
            byte[] shiftRows3 = arrangeColumn(states[10]);
            byte[] mixColumns3 = arrangeColumn(states[11]);
            byte[] addKey3 = arrangeColumn(states[12]);

            while (i <= 16)
            {

                ((TextBlock)this.FindName("roundZero" + i)).Text = state[i - 1].ToString("X2");
                ((TextBlock)this.FindName("sBoxRound1_" + i)).Text = subBytes1[i - 1].ToString("X2");
                ((TextBlock)this.FindName("shiftRowRound1_" + i)).Text = shiftRows1[i - 1].ToString("X2");
                ((TextBlock)this.FindName("mixColumns1_" + i)).Text = mixColumns1[i - 1].ToString("X2");
                ((TextBlock)this.FindName("addKey1_" + i)).Text = addKey1[i - 1].ToString("X2");

                ((TextBlock)this.FindName("sBoxRound2_" + i)).Text = subBytes2[i - 1].ToString("X2");
                ((TextBlock)this.FindName("shiftRowRound2_" + i)).Text = shiftRows2[i - 1].ToString("X2");
                ((TextBlock)this.FindName("mixColumns2_" + i)).Text = mixColumns2[i - 1].ToString("X2");
                ((TextBlock)this.FindName("addKey2_" + i)).Text = addKey2[i - 1].ToString("X2");


                ((TextBlock)this.FindName("sBoxRound3_" + i)).Text = subBytes3[i - 1].ToString("X2");
                ((TextBlock)this.FindName("shiftRowRound3_" + i)).Text = shiftRows3[i - 1].ToString("X2");
                ((TextBlock)this.FindName("mixColumns3_" + i)).Text = mixColumns3[i - 1].ToString("X2");
                ((TextBlock)this.FindName("addKey3_" + i)).Text = addKey3[i - 1].ToString("X2");
                i++;
            }
        }

        //applies a PKCS7 padding 
        public void padding()
        {
            if (textB.Length != 16)
            {
                byte[] temp = new byte[16];
                int x = 0;
                int padding = 16 - textB.Length;
                if (textB.Length < 16)
                {
                    foreach (byte b in textB)
                    {
                        temp[x] = b;
                        x++;
                    }
                    while (x < 16)
                    {
                        temp[x] = (byte)padding;
                        x++;
                    }
                }
                else
                {
                    while (x < 16)
                    {
                        temp[x] = textB[x];
                        x++;
                    }
                }
                textB = temp;
            }
        }

        //shows different intermediate states of the AES encryption process
        public void printIntermediateStates(byte[][] states, byte[][] statesB)
        {
            List<TextBlock> tmp = createTxtBlockList(3);
            List<TextBlock> tmpB = createTxtBlockList(4);


            byte[] state = arrangeText(states[(roundNumber - 1) * 4 + action - 1]);
            byte[] stateB = arrangeText(statesB[(roundNumber - 1) * 4 + action - 1]);

            int i = 0;
            int j = 0;

            foreach (TextBlock txtBlock in tmp)
            {
                txtBlock.Text = state[i].ToString("X2");
                i++;
            }

            foreach (TextBlock txtBlock in tmpB)
            {
                txtBlock.Text = stateB[j].ToString("X2");
                j++;
            }
        }


        public string toString(byte[] result)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in result)
            {
                sb.Append(b.ToString() + " ");
            }
            return sb.ToString();
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
        //shows initial state
        public void setUpSubByte(byte[][] states, byte[][] statesB)
        {


            List<TextBlock> tmp = createTxtBlockList(1);
            List<TextBlock> tmpB = createTxtBlockList(2);


            byte[] state = arrangeText(states[(roundNumber - 1) * 4 + action - 1]);
            byte[] stateB = arrangeText(statesB[(roundNumber - 1) * 4 + action - 1]);

            int i = 0;
            int j = 0;

            foreach (TextBlock txtBlock in tmp)
            {
                txtBlock.Text = state[i].ToString("X2");
                i++;
            }

            foreach (TextBlock txtBlock in tmpB)
            {
                txtBlock.Text = stateB[j].ToString("X2");
                j++;
            }


            //encryptionProgress();

        }

        // Value of avalanche effect
        public double calcAvalancheEffect(int flippedBits, Tuple<string, string> strTuple)
        {

            double avalancheEffect = ((double)flippedBits / strTuple.Item1.Length) * 100;
            double roundUp = Math.Round(avalancheEffect, 1, MidpointRounding.AwayFromZero);

            return roundUp;
        }

        //average number of flipped bits per byte
        public double avgNrperByte(int flippedBits)
        {
            if (mode == 0)
                avgNrDiffBit = ((double)flippedBits / 16);
            else
                avgNrDiffBit = ((double)flippedBits / 8);

            avgNrDiffBit = Math.Round(avgNrDiffBit, 1, MidpointRounding.AwayFromZero);

            return avgNrDiffBit;

        }

        //calculates longest identical bit sequence
        public int longestIdenticalSequence(string[] str)
        {
            int lastCount = 0;
            int longestCount = 0;
            int i = 0;

            int offset = 0;

            while (i < str.Length)
            {
                if (str[i] == " ")
                {
                    lastCount++;

                    if (lastCount > longestCount)
                    {
                        longestCount = lastCount;
                        offset = i - lastCount + 1;
                    }
                }
                else
                {
                    lastCount = 0;
                }

                i++;
            }
            sequencePosition = offset;
            return longestCount;
        }

        //set colors of pie chart
        public void setColors()
        {
            SolidColorBrush brushA = new SolidColorBrush();
            SolidColorBrush brushB = new SolidColorBrush();

            brushA.Color = Color.FromRgb(255, 40, 0);
            brushB.Color = Color.FromRgb(76, 187, 23);
            flippedBitsPiece.Fill = brushA;
            unflippedBitsPiece.Fill = brushB;
        }

        //set position of angles of pie chart
        public void setAngles(double angle_A, double angle_B)
        {
            flippedBitsPiece.angle = angle_A;
            unflippedBitsPiece.angle = angle_B;
            flippedBitsPiece.pieceRotation = 0;
            unflippedBitsPiece.pieceRotation = angle_A;
        }

        //prints out current statistical values of cipher
        public void showStatistics(int bitsFlipped, int longestLength, Tuple<string, string> strTuple)
        {
            stats1.Inlines.Add(new Run(" " + bitsFlipped.ToString()) { Foreground = Brushes.Red, FontWeight = FontWeights.DemiBold });
            stats1.Inlines.Add(new Run(string.Format(" bit flipped (out of {0}.)", strTuple.Item1.Length)));
            stats1.Inlines.Add(new LineBreak());
            stats1.Inlines.Add(new Run(string.Format(" Avalanche effect of {0}%", avalanche)));
            stats2.Inlines.Add(new Run(string.Format(" Length of longest identical bit  sequence: {0}. Offset {1}.", longestLength.ToString(), sequencePosition)));
            if (mode != 2)
            {
                stats3.Inlines.Add(new Run(string.Format(" Average nr. of differing bits per   byte: {0} ", avgNrDiffBit)));
            }
        }

        //Signalizes flipped bits and highlight the differences 
        public void showBitSequence(Tuple<string, string> strTuple)
        {

            string[] diffBits = new string[strTuple.Item1.Length];


            for (int i = 0; i < strTuple.Item1.Length; i++)
            {
                if (strTuple.Item1[i] != strTuple.Item2[i])
                {
                    diffBits[i] = "X";
                }
                else
                    diffBits[i] = " ";

            }

            differentBits = diffBits;



            if (mode == 0)
            {
                int a = 0;
                int b = 256;

                while (a < 128 && b < 384)
                {
                    ((TextBlock)this.FindName("txt" + b)).Text = diffBits[a].ToString();
                    a++;
                    b++;
                }

                int j = 0;
                int k = 128;

                while (j < 128 && k < 256)
                {
                    if (diffBits[j] == "X")
                    {
                        ((TextBlock)this.FindName("txt" + j)).Background = (Brush)new BrushConverter().ConvertFromString("#faebd7");
                        ((TextBlock)this.FindName("txt" + k)).Background = (Brush)new BrushConverter().ConvertFromString("#fe6f5e");

                    }
                    j++;
                    k++;
                }
            }
            else if (mode == 1)
            {
                int a = 0;
                int b = 129;

                while (a < 64 && b < 193)
                {
                    ((TextBlock)this.FindName("desTxt" + b)).Text = diffBits[a].ToString();
                    ((TextBlock)this.FindName("desTxt" + b)).Foreground = Brushes.Red;
                    a++;
                    b++;
                }

                int j = 1;
                int k = 65;

                while (j < 64 && k < 128)
                {
                    if (diffBits[j - 1] == "X")
                    {
                        ((TextBlock)this.FindName("desTxt" + j)).Background = (Brush)new BrushConverter().ConvertFromString("#faebd7");
                        ((TextBlock)this.FindName("desTxt" + k)).Background = (Brush)new BrushConverter().ConvertFromString("#fe6f5e");

                    }
                    j++;
                    k++;
                }
            }
            else
            {
                StringBuilder sb = new StringBuilder();

                foreach (string str in diffBits)
                {
                    sb.Append(str);
                }
                string differentBits2 = Regex.Replace(sb.ToString(), ".{8}", "$0 ");
                TB3.Text = differentBits2;
            }

        }

        //Shows binary values of each cipher byte
        public void displayBinaryValues(byte[] cipherStateA, byte[] cipherStateB)
        {

            byte[] textToArrange = arrangeText(cipherStateA);
            byte[] textToArrangeB = arrangeText(cipherStateB);


            string encryptionStateA = binaryAsString(textToArrange).Replace(" ", "");
            string encryptionStateB = binaryAsString(textToArrangeB).Replace(" ", "");


            int a = 0;
            int b = 128;
            while (a < 128 && b < 256)
            {
                ((TextBlock)this.FindName("txt" + a)).Text = encryptionStateA[a].ToString();
                ((TextBlock)this.FindName("txt" + b)).Text = encryptionStateB[a].ToString();
                a++;
                b++;
            }

        }
        public void toStringArray(int round)
        {

            string A = lrData[round, 0];
            string B = lrData[round, 1];
            string C = lrDataB[round, 0];
            string D = lrDataB[round, 1];


            for (int i = 0; i < 32; i++)
            {
                leftHalf[i] = A.Substring(i, 1);
                rightHalf[i] = B.Substring(i, 1);
                leftHalfB[i] = C.Substring(i, 1);
                rightHalfB[i] = D.Substring(i, 1);

            }

            string bitSeqA = string.Concat(A, B);
            string bitseqB = string.Concat(C, D);

            seqA = getByteArray(bitSeqA);
            seqB = getByteArray(bitseqB);
        }



        public byte[] getByteArray(string str)
        {
            byte[] byteArray = new byte[8];

            for (int i = 0; i < 8; i++)
                byteArray[i] = Convert.ToByte(str.Substring(8 * i, 8), 2);

            return byteArray;
        }

        public void displayBinaryValuesDES()
        {
            /*int a = 0;
            int b = 64;
            while (a < 64 && b < 128)
            {
                ((TextBlock)this.FindName("txt" + a)).Text = msgA[a];
                ((TextBlock)this.FindName("txt" + b)).Text = msgB[a];
                a++;
                b++;
            }*/

            bitGridDES.Visibility = Visibility.Visible;

            int a = 0;
            int b = 32;
            int c = 64;
            int d = 96;
            while (a < 32 && b < 64 && c < 96 && d < 128)
            {

                ((TextBlock)this.FindName("txt" + a)).Text = leftHalf[a];
                ((TextBlock)this.FindName("txt" + b)).Text = rightHalf[a];
                ((TextBlock)this.FindName("txt" + c)).Text = leftHalfB[a];
                ((TextBlock)this.FindName("txt" + d)).Text = rightHalfB[a];

                a++;
                b++;
                c++;
                d++;
            }


            int i = 1;
            int j = 33;
            int k = 65;
            int l = 97;
            while (i < 33 && j < 65 && k < 97 && l < 129)
            {
                ((TextBlock)this.FindName("desTxt" + i)).Text = leftHalf[i - 1];
                ((TextBlock)this.FindName("desTxt" + j)).Text = rightHalf[i - 1];
                ((TextBlock)this.FindName("desTxt" + k)).Text = leftHalfB[i - 1];
                ((TextBlock)this.FindName("desTxt" + l)).Text = rightHalfB[i - 1];

                i++;
                j++;
                k++;
                l++;
            }

            //txt0.Text = lrData[1, 0];
            //txt0.Text = des.lrData[round, 0];


        }
        //transforms to string of binary values
        public string binaryAsString(byte[] byteSequence)
        {
            StringBuilder sb = new StringBuilder();

            sb = new StringBuilder();

            var encoding = Encoding.GetEncoding(437);

            for (int i = 0; i < byteSequence.Length; i++)
            {
                if (byteSequence[i] <= 127)
                {
                    sb.Append(Convert.ToString(byteSequence[i], 2).PadLeft(8, '0') + " ");
                }
                else { sb.Append(Convert.ToString(byteSequence[i], 2) + " "); }
            }

            return sb.ToString();
        }

        //string of decimal values
        public string decimalAsString(byte[] byteSequence)
        {
            StringBuilder sb = new StringBuilder();
            sb = new StringBuilder();

            foreach (byte b in byteSequence)
                sb.AppendFormat("{0:D3}{1}", b, " ");

            return sb.ToString(); ;

        }


        //string of hexadecimal values
        public string hexaAsString(byte[] byteSequence)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var v in byteSequence)
                sb.AppendFormat("{0:X2}{1}", v, " ");

            return sb.ToString();
        }

        //counts how many bits are flipped after comparison
        public int nrOfBitsFlipped(byte[] nr1, byte[] nr2)
        {
            int shift = 7;
            int count = 0;

            byte[] comparison = this.exclusiveOr(nr1, nr2);

            for (int j = 0; j < comparison.Length; j++)
            {

                for (int i = 0; i <= shift; i++)
                {

                    if ((comparison[j] & (1 << i)) != 0)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        // XOR operation
        public byte[] exclusiveOr(byte[] input1, byte[] input2)
        {
            byte[] result = new byte[input1.Length];



            for (int i = 0; i < input1.Length; i++)
            {
                result[i] = (byte)(input1[i] ^ input2[i]);
            }

            return result;
        }

        //returns a tuple of strings
        public Tuple<string, string> binaryStrings(byte[] cipherStateA, byte[] cipherStateB)
        {
            byte[] textToArrange;
            byte[] textToArrangeB;
            string encryptionStateA = "";
            string encryptionStateB = "";

            if (mode == 0)
            {
                textToArrange = arrangeText(cipherStateA);
                textToArrangeB = arrangeText(cipherStateB);
                encryptionStateA = binaryAsString(textToArrange).Replace(" ", "");
                encryptionStateB = binaryAsString(textToArrangeB).Replace(" ", "");

            }
            else if (mode == 2 || mode == 3 || mode == 4)
            {
                encryptionStateA = binaryAsString(cipherStateA).Replace(" ", "");
                encryptionStateB = binaryAsString(cipherStateB).Replace(" ", "");
            }
            else
            {

                encryptionStateA = lrData[roundDES, 0] + lrData[roundDES, 1];
                encryptionStateB = lrDataB[roundDES, 0] + lrDataB[roundDES, 1];

            }

            var tuple = new Tuple<string, string>(encryptionStateA, encryptionStateB);

            return tuple;

        }

        //Set content of toolTips
        public void setToolTips()
        {
            tp1.Content = avalanche + " %";
            tp2.Content = (100 - avalanche) + " %";
        }

        //clear background colors
        private void removeColors()
        {

            if (mode == 0)
            {
                int a = 0;
                int b = 128;
                while (a < 128 && b < 256)
                {
                    ((TextBlock)this.FindName("txt" + a)).Background = Brushes.Transparent;
                    ((TextBlock)this.FindName("txt" + b)).Background = Brushes.Transparent;
                    a++;
                    b++;
                }
            }
            else
            {
                int j = 1;
                int k = 65;
                while (j < 65 && k < 129)
                {
                    ((TextBlock)this.FindName("desTxt" + j)).Background = Brushes.Transparent;
                    ((TextBlock)this.FindName("desTxt" + k)).Background = Brushes.Transparent;
                    j++;
                    k++;
                }
                modificationGridDES.Visibility = Visibility.Hidden;

            }

        }

        //updates roundnr displayed on GUI
        public void changeRoundNr(int number)
        {

            afterRoundsTitle.Text = afterRoundsTitle.Text.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
            afterRoundsSubtitle.Text = afterRoundsSubtitle.Text.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
            if (mode == 0)
            {
                if (keysize == 1)
                    afterRoundsTitle.Inlines.Add(new Run("Avalanche effect AES-192"));
                else if (keysize == 2)
                    afterRoundsTitle.Inlines.Add(new Run("Avalanche effect AES-256"));
                else
                    afterRoundsTitle.Inlines.Add(new Run("Avalanche effect AES-128"));
            }
            else
                if (mode == 1)
                afterRoundsTitle.Inlines.Add(new Run("Avalanche effect DES"));


            afterRoundsSubtitle.Inlines.Add(new Run(string.Format("{0}", number)));

        }

        public void changeTitle()
        {
            if (mode == 0)
            {
                initStateTitle.Text = initStateTitle.Text.TrimEnd('1', '2', '5', '6', '8', '9');
                if (keysize == 1)
                    initStateTitle.Inlines.Add(new Run("192") { Foreground = Brushes.White });
                else if (keysize == 2)
                    initStateTitle.Inlines.Add(new Run("256") { Foreground = Brushes.White });
                else
                    initStateTitle.Inlines.Add(new Run("128") { Foreground = Brushes.White });
            }
            else
            {
                if (mode == 2)
                {
                    othersOrigTitle.Text = "Original hash function";
                    othersModTitle.Text = "Modified hash function";
                    lbl.Text = "Original hash function";
                    lbl2.Text = "Modified hash function";
                    othersSubtitle.Text = "(Hash functions)";
                }
                if (mode == 4)
                    othersSubtitle.Text = ("Modern ciphers");
            }
        }

        //new position after shiftrows operation
        public TextBlock afterShifting(int position, int round)
        {

            int newPosition = 0;

            if (position == 1 || position == 5 || position == 9 || position == 13)
            {
                newPosition = position % 16;
            }
            if (position == 2 || position == 6 || position == 10 || position == 14)
            {
                newPosition = ((position - 4) % 16 + 16) % 16;
            }
            if (position == 3 || position == 7 || position == 11 || position == 15)
            {
                newPosition = ((position - 8) % 16 + 16) % 16;
            }
            if (position == 4 || position == 8 || position == 12 || position == 16)
            {
                newPosition = ((position - 12) % 16 + 16) % 16;

                if (position == 12)
                    newPosition = 16;
            }


            TextBlock txtBlock = ((TextBlock)this.FindName("shiftRowRound" + round + "_" + newPosition));

            return txtBlock;
        }

        //highlight columns
        public void brushColumns(int position, int round)
        {
            Brush greenBrush = (Brush)new BrushConverter().ConvertFromString("#059033");

            int posA = 0;
            int posB = 0;
            int posC = 0;
            int posD = 0;


            switch (position)
            {
                case 1:
                    posA = 1; posB = 2; posC = 3; posD = 4;
                    break;
                case 13:
                    posA = 13; posB = 14; posC = 15; posD = 16;
                    break;
                case 9:
                    posA = 9; posB = 10; posC = 11; posD = 12;
                    break;
                case 5:
                    posA = 5; posB = 6; posC = 7; posD = 8;
                    break;
                default:
                    break;
            }



           ((TextBlock)this.FindName("mixColumns" + round + "_" + posA)).Background = greenBrush;
            ((TextBlock)this.FindName("mixColumns" + round + "_" + posB)).Background = greenBrush;
            ((TextBlock)this.FindName("mixColumns" + round + "_" + posC)).Background = greenBrush;
            ((TextBlock)this.FindName("mixColumns" + round + "_" + posD)).Background = greenBrush;

            ((TextBlock)this.FindName("addKey" + round + "_" + posA)).Background = greenBrush;
            ((TextBlock)this.FindName("addKey" + round + "_" + posB)).Background = greenBrush;
            ((TextBlock)this.FindName("addKey" + round + "_" + posC)).Background = greenBrush;
            ((TextBlock)this.FindName("addKey" + round + "_" + posD)).Background = greenBrush;


        }

        //shows connecting lines according to selected byte
        public void connectingLines(int bytePos, bool clear)
        {
            List<Border> tmp1 = createBorderList(2);
            List<Border> tmp2 = createBorderList(6);
            List<Border> tmp3 = createBorderList(10);
            List<Border> tmp4 = createBorderList(13);
            List<Border> tmp5 = createBorderList(20);
            List<Border> tmp6 = createBorderList(23);
            List<Border> tmp7 = createBorderList(1);
            List<Border> tmp8 = createBorderList(9);
            List<Border> tmp9 = createBorderList(16);
            List<Border> tmp10 = createBorderList(19);
            List<Border> tmp11 = createBorderList(22);
            List<Border> tmp12 = createBorderList(5);
            List<Border> tmp13 = createBorderList(4);
            List<Border> tmp14 = createBorderList(8);
            List<Border> tmp15 = createBorderList(12);
            List<Border> tmp16 = createBorderList(21);
            List<Border> tmp17 = createBorderList(18);
            List<Border> tmp18 = createBorderList(15);
            List<Border> tmp19 = createBorderList(3);
            List<Border> tmp20 = createBorderList(7);
            List<Border> tmp21 = createBorderList(11);
            List<Border> tmp22 = createBorderList(14);
            List<Border> tmp23 = createBorderList(17);
            List<Border> tmp24 = createBorderList(24);

            switch (bytePos)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                    for (int i = 1; i <= 32; i++)
                    {
                        if (clear)
                            ((Border)this.FindName("byte" + bytePos + "_" + i)).Visibility = Visibility.Hidden;
                        else
                            ((Border)this.FindName("byte" + bytePos + "_" + i)).Visibility = Visibility.Visible;
                    }
                    break;

                case 5:
                case 10:
                case 15:
                    for (int i = 1; i <= 7; i++)
                    {
                        if (clear)
                        {
                            ((Border)this.FindName("byte" + bytePos + "_" + i)).Visibility = Visibility.Hidden;
                            byte4_8.Visibility = Visibility.Hidden;

                            foreach (Border br in tmp1)
                                br.Visibility = Visibility.Hidden;
                            foreach (Border br in tmp2)
                                br.Visibility = Visibility.Hidden;
                            foreach (Border br in tmp3)
                                br.Visibility = Visibility.Hidden;
                            foreach (Border br in tmp4)
                                br.Visibility = Visibility.Hidden;
                            foreach (Border br in tmp5)
                                br.Visibility = Visibility.Hidden;
                            foreach (Border br in tmp6)
                                br.Visibility = Visibility.Hidden;

                        }
                        else
                        {
                            ((Border)this.FindName("byte" + bytePos + "_" + i)).Visibility = Visibility.Visible;
                            byte4_8.Visibility = Visibility.Visible;

                            foreach (Border br in tmp1)
                                br.Visibility = Visibility.Visible;
                            foreach (Border br in tmp2)
                                br.Visibility = Visibility.Visible;
                            foreach (Border br in tmp3)
                                br.Visibility = Visibility.Visible;
                            foreach (Border br in tmp4)
                                br.Visibility = Visibility.Visible;
                            foreach (Border br in tmp5)
                                br.Visibility = Visibility.Visible;
                            foreach (Border br in tmp6)
                                br.Visibility = Visibility.Visible;
                        }
                    }
                    break;
                case 6:
                case 11:
                case 16:
                    for (int i = 1; i <= 7; i++)
                    {

                        if (clear)
                        {
                            ((Border)this.FindName("byte" + bytePos + "_" + i)).Visibility = Visibility.Hidden;
                            byte1_8.Visibility = Visibility.Hidden;

                            foreach (Border br in tmp7)
                                br.Visibility = Visibility.Hidden;
                            foreach (Border br in tmp8)
                                br.Visibility = Visibility.Hidden;
                            foreach (Border br in tmp9)
                                br.Visibility = Visibility.Hidden;
                            foreach (Border br in tmp10)
                                br.Visibility = Visibility.Hidden;
                            foreach (Border br in tmp11)
                                br.Visibility = Visibility.Hidden;
                            foreach (Border br in tmp12)
                                br.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            ((Border)this.FindName("byte" + bytePos + "_" + i)).Visibility = Visibility.Visible;
                            byte1_8.Visibility = Visibility.Visible;

                            foreach (Border br in tmp7)
                                br.Visibility = Visibility.Visible;
                            foreach (Border br in tmp8)
                                br.Visibility = Visibility.Visible;
                            foreach (Border br in tmp9)
                                br.Visibility = Visibility.Visible;
                            foreach (Border br in tmp10)
                                br.Visibility = Visibility.Visible;
                            foreach (Border br in tmp11)
                                br.Visibility = Visibility.Visible;
                            foreach (Border br in tmp12)
                                br.Visibility = Visibility.Visible;
                        }
                    }
                    break;
                case 7:
                case 12:
                case 13:
                    for (int i = 1; i <= 7; i++)
                    {

                        if (clear)
                        {
                            ((Border)this.FindName("byte" + bytePos + "_" + i)).Visibility = Visibility.Hidden;
                            byte2_8.Visibility = Visibility.Hidden;

                            foreach (Border br in tmp13)
                                br.Visibility = Visibility.Hidden;
                            foreach (Border br in tmp14)
                                br.Visibility = Visibility.Hidden;
                            foreach (Border br in tmp15)
                                br.Visibility = Visibility.Hidden;
                            foreach (Border br in tmp16)
                                br.Visibility = Visibility.Hidden;
                            foreach (Border br in tmp17)
                                br.Visibility = Visibility.Hidden;
                            foreach (Border br in tmp18)
                                br.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            ((Border)this.FindName("byte" + bytePos + "_" + i)).Visibility = Visibility.Visible;
                            byte2_8.Visibility = Visibility.Visible;

                            foreach (Border br in tmp13)
                                br.Visibility = Visibility.Visible;
                            foreach (Border br in tmp14)
                                br.Visibility = Visibility.Visible;
                            foreach (Border br in tmp15)
                                br.Visibility = Visibility.Visible;
                            foreach (Border br in tmp16)
                                br.Visibility = Visibility.Visible;
                            foreach (Border br in tmp17)
                                br.Visibility = Visibility.Visible;
                            foreach (Border br in tmp18)
                                br.Visibility = Visibility.Visible;
                        }
                    }
                    break;
                case 8:
                case 9:
                case 14:
                    for (int i = 1; i <= 7; i++)
                    {

                        if (clear)
                        {
                            ((Border)this.FindName("byte" + bytePos + "_" + i)).Visibility = Visibility.Hidden;
                            byte3_8.Visibility = Visibility.Hidden;

                            foreach (Border br in tmp19)
                                br.Visibility = Visibility.Hidden;
                            foreach (Border br in tmp20)
                                br.Visibility = Visibility.Hidden;
                            foreach (Border br in tmp21)
                                br.Visibility = Visibility.Hidden;
                            foreach (Border br in tmp22)
                                br.Visibility = Visibility.Hidden;
                            foreach (Border br in tmp23)
                                br.Visibility = Visibility.Hidden;
                            foreach (Border br in tmp24)
                                br.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            ((Border)this.FindName("byte" + bytePos + "_" + i)).Visibility = Visibility.Visible;
                            byte3_8.Visibility = Visibility.Visible;

                            foreach (Border br in tmp19)
                                br.Visibility = Visibility.Visible;
                            foreach (Border br in tmp20)
                                br.Visibility = Visibility.Visible;
                            foreach (Border br in tmp21)
                                br.Visibility = Visibility.Visible;
                            foreach (Border br in tmp22)
                                br.Visibility = Visibility.Visible;
                            foreach (Border br in tmp23)
                                br.Visibility = Visibility.Visible;
                            foreach (Border br in tmp24)
                                br.Visibility = Visibility.Visible;
                        }
                    }
                    break;
                default:
                    break;

            }
        }


        public void brushRemainingColumns()
        {
            Brush greenBrush = (Brush)new BrushConverter().ConvertFromString("#059033");
            int a = 1;

            while (a <= 16)
            {
                ((TextBlock)this.FindName("mixColumns2_" + a)).Background = greenBrush;
                ((TextBlock)this.FindName("addKey2_" + a)).Background = greenBrush;
                ((TextBlock)this.FindName("sBoxRound3_" + a)).Background = greenBrush;
                ((TextBlock)this.FindName("shiftRowRound3_" + a)).Background = greenBrush;
                ((TextBlock)this.FindName("mixColumns3_" + a)).Background = greenBrush;
                ((TextBlock)this.FindName("addKey3_" + a)).Background = greenBrush;

                a++;
            }
        }

        public void clearElements()
        {
            if (mode == 0)
            {
                OrigInitialStateGrid.Visibility = Visibility.Hidden;
                modifiedInitialStateGrid.Visibility = Visibility.Hidden;
                afterInitialRoundGrid.Visibility = Visibility.Hidden;
                afterInitRoundButton.Visibility = Visibility.Hidden;
                initStateTitle.Visibility = Visibility.Hidden;
                radioButtons.Visibility = Visibility.Hidden;
            }
            else
                modificationGridDES.Visibility = Visibility.Hidden;


            afterRoundsTitle.Text = string.Empty;


        }

        public void showElements()
        {
            if (mode == 0)
            {
                afterRoundsGrid.Visibility = Visibility.Visible;
                bitRepresentationGrid.Visibility = Visibility.Visible;
                curvedLinesCanvas.Visibility = Visibility.Visible;
            }

            bitsData.Visibility = Visibility.Visible;
            Cb1.Visibility = Visibility.Visible;
            Cb2.Visibility = Visibility.Visible;
            flippedBitsPiece.Visibility = Visibility.Visible;
            unflippedBitsPiece.Visibility = Visibility.Visible;
            afterRoundsTitle.Visibility = Visibility.Visible;
            afterRoundsSubtitle.Visibility = Visibility.Visible;

        }


        public List<TextBox> createTxtBoxList()
        {
            List<TextBox> txtBoxList = new List<TextBox>();

            for (int i = 1; i <= 16; i++)
                txtBoxList.Add((TextBox)this.FindName("txtBox" + i));

            return txtBoxList;
        }

        public List<Border> createBorderList(int borderListNr)
        {
            List<Border> borderList = new List<Border>();

            switch (borderListNr)
            {
                case 1:
                    for (int i = 9; i <= 12; i++)
                        borderList.Add((Border)this.FindName("byte1_" + i));
                    break;
                case 2:
                    for (int i = 9; i <= 12; i++)
                        borderList.Add((Border)this.FindName("byte4_" + i));
                    break;
                case 3:
                    for (int i = 9; i <= 12; i++)
                        borderList.Add((Border)this.FindName("byte3_" + i));
                    break;
                case 4:
                    for (int i = 9; i <= 12; i++)
                        borderList.Add((Border)this.FindName("byte2_" + i));
                    break;
                case 5:
                    for (int i = 13; i <= 16; i++)
                        borderList.Add((Border)this.FindName("byte1_" + i));
                    break;
                case 6:
                    for (int i = 13; i <= 16; i++)
                        borderList.Add((Border)this.FindName("byte4_" + i));
                    break;
                case 7:
                    for (int i = 13; i <= 16; i++)
                        borderList.Add((Border)this.FindName("byte3_" + i));
                    break;
                case 8:
                    for (int i = 13; i <= 16; i++)
                        borderList.Add((Border)this.FindName("byte2_" + i));
                    break;
                case 9:
                    for (int i = 17; i <= 20; i++)
                        borderList.Add((Border)this.FindName("byte1_" + i));
                    break;
                case 10:
                    for (int i = 17; i <= 20; i++)
                        borderList.Add((Border)this.FindName("byte4_" + i));
                    break;
                case 11:
                    for (int i = 17; i <= 20; i++)
                        borderList.Add((Border)this.FindName("byte3_" + i));
                    break;
                case 12:
                    for (int i = 17; i <= 20; i++)
                        borderList.Add((Border)this.FindName("byte2_" + i));
                    break;
                case 13:
                    for (int i = 21; i <= 24; i++)
                        borderList.Add((Border)this.FindName("byte4_" + i));
                    break;
                case 14:
                    for (int i = 21; i <= 24; i++)
                        borderList.Add((Border)this.FindName("byte3_" + i));
                    break;
                case 15:
                    for (int i = 21; i <= 24; i++)
                        borderList.Add((Border)this.FindName("byte2_" + i));
                    break;
                case 16:
                    for (int i = 21; i <= 24; i++)
                        borderList.Add((Border)this.FindName("byte1_" + i));
                    break;
                case 17:
                    for (int i = 25; i <= 28; i++)
                        borderList.Add((Border)this.FindName("byte3_" + i));
                    break;
                case 18:
                    for (int i = 25; i <= 28; i++)
                        borderList.Add((Border)this.FindName("byte2_" + i));
                    break;
                case 19:
                    for (int i = 25; i <= 28; i++)
                        borderList.Add((Border)this.FindName("byte1_" + i));
                    break;
                case 20:
                    for (int i = 25; i <= 28; i++)
                        borderList.Add((Border)this.FindName("byte4_" + i));
                    break;
                case 21:
                    for (int i = 29; i <= 32; i++)
                        borderList.Add((Border)this.FindName("byte2_" + i));
                    break;
                case 22:
                    for (int i = 29; i <= 32; i++)
                        borderList.Add((Border)this.FindName("byte1_" + i));
                    break;
                case 23:
                    for (int i = 29; i <= 32; i++)
                        borderList.Add((Border)this.FindName("byte4_" + i));
                    break;
                case 24:
                    for (int i = 29; i <= 32; i++)
                        borderList.Add((Border)this.FindName("byte3_" + i));
                    break;
                default:
                    break;

            }

            return borderList;

        }

        public List<TextBlock> createTxtBlockList(int txtBlockType)
        {
            List<TextBlock> txtBlockList = new List<TextBlock>();

            switch (txtBlockType)
            {
                case 0:

                    for (int i = 1; i <= 16; i++)
                    {
                        txtBlockList.Add((TextBlock)this.FindName("initStateTxtBlock" + i));
                    }
                    break;
                case 1:
                    for (int i = 1; i <= 16; i++)
                    {
                        txtBlockList.Add((TextBlock)this.FindName("afterAddKey" + i));
                    }
                    break;
                case 2:
                    for (int i = 17; i <= 32; i++)
                    {
                        txtBlockList.Add((TextBlock)this.FindName("afterAddKey" + i));
                    }
                    break;
                case 3:
                    for (int i = 1; i <= 16; i++)
                    {
                        txtBlockList.Add((TextBlock)this.FindName("afterRoundTxt" + i));
                    }
                    break;
                case 4:
                    for (int i = 17; i <= 32; i++)
                    {
                        txtBlockList.Add((TextBlock)this.FindName("afterRoundTxt" + i));
                    }
                    break;
                case 5:
                    for (int i = 256; i < 384; i++)
                    {
                        txtBlockList.Add((TextBlock)this.FindName("txt" + i));
                    }
                    break;
                case 6:
                    for (int i = 1; i < 129; i++)
                    {
                        txtBlockList.Add((TextBlock)this.FindName("bit" + i));
                        txtBlockList.Add((TextBlock)this.FindName("keyBit" + i));

                    }
                    break;
                case 7:
                    for (int i = 1; i < 65; i++)
                    {
                        txtBlockList.Add((TextBlock)this.FindName("desBit" + i));
                        txtBlockList.Add((TextBlock)this.FindName("desKeyBit" + i));
                    }
                    break;
                default:
                    break;

            }
            return txtBlockList;
        }


        public void setButtonsScrollViewer()
        {
            if (mode == 0)
            {
                if (keysize == 1)
                {
                    afterRound11Button.Visibility = Visibility.Visible;
                    afterRound12Button.Visibility = Visibility.Visible;

                }
                else if (keysize == 2)
                {
                    afterRound11Button.Visibility = Visibility.Visible;
                    afterRound12Button.Visibility = Visibility.Visible;
                    afterRound13Button.Visibility = Visibility.Visible;
                    afterRound14Button.Visibility = Visibility.Visible;

                }
            }
            else
            {
                afterRound11Button.Visibility = Visibility.Visible;
                afterRound12Button.Visibility = Visibility.Visible;
                afterRound13Button.Visibility = Visibility.Visible;
                afterRound14Button.Visibility = Visibility.Visible;
                afterRound15Button.Visibility = Visibility.Visible;
                afterRound16Button.Visibility = Visibility.Visible;
            }
        }


        private byte[] arrangeColumn(byte[] input)
        {
            byte[] result = new byte[16];
            result[0] = input[0];
            result[1] = input[1];
            result[2] = input[2];
            result[3] = input[3];
            result[4] = input[4];
            result[5] = input[5];
            result[6] = input[6];
            result[7] = input[7];
            result[8] = input[8];
            result[9] = input[9];
            result[10] = input[10];
            result[11] = input[11];
            result[12] = input[12];
            result[13] = input[13];
            result[14] = input[14];
            result[15] = input[15];
            return result;
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


        #endregion

        // #region AES methods
        public void createSBox()
        {
            int x = 0;
            while (x < 16)
            {
                this.sBox[x] = new byte[16];
                x++;
            }
            x = 0;
            byte[] temp = { 99, 124, 119, 123, 242, 107, 111, 197, 48, 1, 103, 43, 254, 215, 171, 118, 202, 130, 201, 125, 250, 89, 71, 240, 173, 212, 162, 175, 156, 164, 114, 192, 183, 253, 147, 38, 54, 63, 247, 204, 52, 165, 229, 241, 113, 216, 49, 21, 4, 199, 35, 195, 24, 150, 5, 154, 7, 18, 128, 226, 235, 39, 178, 117, 9, 131, 44, 26, 27, 110, 90, 160, 82, 59, 214, 179, 41, 227, 47, 132, 83, 209, 0, 237, 32, 252, 177, 91, 106, 203, 190, 57, 74, 76, 88, 207, 208, 239, 170, 251, 67, 77, 51, 133, 69, 249, 2, 127, 80, 60, 159, 168, 81, 163, 64, 143, 146, 157, 56, 245, 188, 182, 218, 33, 16, 255, 243, 210, 205, 12, 19, 236, 95, 151, 68, 23, 196, 167, 126, 61, 100, 93, 25, 115, 96, 129, 79, 220, 34, 42, 144, 136, 70, 238, 184, 20, 222, 94, 11, 219, 224, 50, 58, 10, 73, 6, 36, 92, 194, 211, 172, 98, 145, 149, 228, 121, 231, 200, 55, 109, 141, 213, 78, 169, 108, 86, 244, 234, 101, 122, 174, 8, 186, 120, 37, 46, 28, 166, 180, 198, 232, 221, 116, 31, 75, 189, 139, 138, 112, 62, 181, 102, 72, 3, 246, 14, 97, 53, 87, 185, 134, 193, 29, 158, 225, 248, 152, 17, 105, 217, 142, 148, 155, 30, 135, 233, 206, 85, 40, 223, 140, 161, 137, 13, 191, 230, 66, 104, 65, 153, 45, 15, 176, 84, 187, 22 };
            x = 0;
            int y = 0;
            foreach (byte b in temp)
            {
                sBox[y][x] = b;
                x++;
                if (x == 16)
                {
                    y++;
                    x = 0;
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

            return sBox;
        }


        #region Event handlers 



        private void doneButton_Click(object sender, RoutedEventArgs e)
        {

            instructionsTxtBlock2.Visibility = Visibility.Hidden;
            doneButton.Visibility = Visibility.Hidden;
            clearButton.Visibility = Visibility.Hidden;

            if (mode == 0)
            {
                inputInBits.Visibility = Visibility.Hidden;

                string[] strSequence = new string[128];
                string[] result = new string[16];
                string[][] textBits = new string[16][];

                int l = 0;

                for (int i = 1; i < 129; i++)
                    strSequence[i - 1] = ((TextBlock)this.FindName(string.Format("bit{0}", i))).Text;

                string bitSequence = string.Join("", strSequence);

                /*for (int j = 0; j < 16; j++)
                    textBits[j] = new string[8];*/

                for (int k = 0; k < 16; k++)
                {
                    result[k] = bitSequence.Substring(l, 8);
                    l += 8;
                }

                /*for (int i = 0; i < 16; i++)
                    textBits[i] = result;*/

                byte[] newText = result.Select(s => Convert.ToByte(s, 2)).ToArray();

                textB = newText;
                string keyBitSequence = "";
                string[] keyResult = new string[key.Length];
                string[][] keyBits = new string[key.Length][];

                string bitName = "keyBit{0}";
                int bits = 129;

                int m = 0;

                if (keysize == 1)
                {
                    string[] strKeySequence2 = new string[192];

                    for (int i = 1; i < 193; i++)
                        strKeySequence2[i - 1] = ((TextBlock)this.FindName(string.Format("keyBit192_{0}", i))).Text;

                    keyBitSequence = string.Join("", strKeySequence2);
                }
                else if (keysize == 2)
                {

                    string[] strKeySequence3 = new string[256];

                    for (int i = 1; i < 257; i++)
                        strKeySequence3[i - 1] = ((TextBlock)this.FindName(string.Format("keyBit256_{0}", i))).Text;

                    keyBitSequence = string.Join("", strKeySequence3);
                }
                else
                {
                    string[] strKeySequence = new string[128];

                    for (int i = 1; i < bits; i++)
                        strKeySequence[i - 1] = ((TextBlock)this.FindName(string.Format(bitName, i))).Text;

                    keyBitSequence = string.Join("", strKeySequence);
                }

                for (int j = 0; j < 16; j++)
                    keyBits[j] = new string[8];


                for (int k = 0; k < key.Length; k++)
                {
                    keyResult[k] = keyBitSequence.Substring(m, 8);
                    m += 8;
                }


                byte[] newKey = keyResult.Select(s => Convert.ToByte(s, 2)).ToArray();
                key = newKey;
                aesDiffusion = new AES(newKey, newText);


                aesDiffusion.checkKeysize();
                byte[] temporaryB = aesDiffusion.checkTextLength();
                aesDiffusion.executeAES(false);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    loadChangedMsg(temporaryB, true);
                    loadChangedKey(newKey);
                    setAndLoadButtons();

                }, null);

                statesB = aesDiffusion.statesB;
                int z = 0;


            }
            else
            {

                string[] strSequence = new string[64];
                string[] result = new string[8];
                string keyBitSequence = "";
                string[] keyResult = new string[key.Length];

                int l = 0;
                int m = 0;
                for (int i = 1; i < 65; i++)
                    strSequence[i - 1] = ((TextBlock)this.FindName(string.Format("desBit{0}", i))).Text;

                string bitSequence = string.Join("", strSequence);

                /*for (int j = 0; j < 16; j++)
                    textBits[j] = new string[8];*/


                for (int k = 0; k < 8; k++)
                {
                    result[k] = bitSequence.Substring(l, 8);
                    l += 8;
                }

                /*for (int i = 0; i < 16; i++)
                    textBits[i] = result;*/

                byte[] newText = result.Select(s => Convert.ToByte(s, 2)).ToArray();

                textB = newText;

                string[] strKeySequence = new string[64];

                for (int i = 1; i < 65; i++)
                    strKeySequence[i - 1] = ((TextBlock)this.FindName(string.Format("desKeyBit{0}", i))).Text;

                keyBitSequence = string.Join("", strKeySequence);


                for (int k = 0; k < key.Length; k++)
                {
                    keyResult[k] = keyBitSequence.Substring(m, 8);
                    m += 8;
                }

                byte[] newKey = keyResult.Select(s => Convert.ToByte(s, 2)).ToArray();

                desDiffusion = new DES(newText, newKey);
                desDiffusion.textChanged = true;
                desDiffusion.DESProcess();


                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {

                    loadChangedMsg(newText, true);
                    loadChangedKey(newKey);
                    setAndLoadButtons();

                }, null);

                lrDataB = desDiffusion.lrDataB;

            }
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void inputDataButton_Click(object sender, RoutedEventArgs e)
        {
            afterRoundsTitle.Visibility = Visibility.Hidden;
            afterRoundsSubtitle.Visibility = Visibility.Hidden;
            flippedBitsPiece.Visibility = Visibility.Hidden;
            unflippedBitsPiece.Visibility = Visibility.Hidden;
            bitsData.Visibility = Visibility.Hidden;
            Cb1.Visibility = Visibility.Hidden;
            Cb2.Visibility = Visibility.Hidden;
            buttonsTitle.Visibility = Visibility.Hidden;
            buttonsPanel.Visibility = Visibility.Hidden;

            if (mode == 0)
            {
                afterRoundsGrid.Visibility = Visibility.Hidden;
                bitRepresentationGrid.Visibility = Visibility.Hidden;
                curvedLinesCanvas.Visibility = Visibility.Hidden;
                OrigInitialStateGrid.Visibility = Visibility.Visible;

            }
            else
                bitGridDES.Visibility = Visibility.Hidden;


            comparisonPane();

            if ((bool)aesCheckBox.IsChecked || (bool)desCheckBox.IsChecked)
            {
                instructionsTxtBlock2.Visibility = Visibility.Visible;
                doneButton.Visibility = Visibility.Visible;
                clearButton.Visibility = Visibility.Visible;
            }


        }

        private void afterInitRoundButton_Click(object sender, RoutedEventArgs e)
        {

            OrigInitialStateGrid.Visibility = Visibility.Visible;
            setButtonsScrollViewer();
            buttonsPanel.Visibility = Visibility.Visible;

            roundNumber = 1 + shift * 2 * keysize;

            action = 1;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                setUpSubByte(states, statesB);

            }, null);
        }



        private void afterRound0Button_Click(object sender, RoutedEventArgs e)
        {
            //afterInitialRoundGrid.Visibility = Visibility.Visible;
        }

        public void emptyInformation()
        {
            stats1.Text = string.Empty;
            stats2.Text = string.Empty;
            stats3.Text = string.Empty;
        }

        private void afterRound1Button_Click(object sender, RoutedEventArgs e)
        {
            var strings = binaryStrings(states[4], statesB[4]);

            clearElements();
            changeRoundNr(1);
            showElements();
            removeColors();

            if (mode == 0)
            {

                roundNumber = 2 + shift * 2 * keysize;
                action = 1;


                int nrDiffBits = nrOfBitsFlipped(states[4], statesB[4]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequence;
                avgNrDiffBit = avgNrperByte(nrDiffBits);
                emptyInformation();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {

                    printIntermediateStates(states, statesB);
                    displayBinaryValues(states[4], statesB[4]);
                    showBitSequence(strings);
                    lengthIdentSequence = longestIdenticalSequence(differentBits);
                    showStatistics(nrDiffBits, lengthIdentSequence, strings);
                    setColors();
                    setAngles(angle_1, angle_2);
                    setToolTips();

                }, null);

                slideNr = 5;

            }
            else
            {
                roundDES = 1;
                strings = binaryStrings(states[4], statesB[4]);

                toStringArray(roundDES);

                int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                avgNrDiffBit = avgNrperByte(nrDiffBits);
                emptyInformation();
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {

                    displayBinaryValuesDES();
                    showBitSequence(strings);
                    lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
                    showStatistics(nrDiffBits, lengthIdentSequenceDes, strings);
                    setColors();
                    setAngles(angle_1, angle_2);
                    setToolTips();

                }, null);
            }
        }

        private void afterRound2Button_Click(object sender, RoutedEventArgs e)
        {

            var strings = binaryStrings(states[8], statesB[8]);
            clearElements();
            changeRoundNr(2);
            showElements();
            removeColors();

            if (mode == 0)
            {

                roundNumber = 3 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits = nrOfBitsFlipped(states[8], statesB[8]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                emptyInformation();
                int lengthIdentSequence;
                avgNrDiffBit = avgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    printIntermediateStates(states, statesB);
                    displayBinaryValues(states[8], statesB[8]);
                    showBitSequence(strings);
                    lengthIdentSequence = longestIdenticalSequence(differentBits);
                    showStatistics(nrDiffBits, lengthIdentSequence, strings);
                    setColors();
                    setAngles(angle_1, angle_2);
                    setToolTips();

                }, null);
                slideNr = 5;
            }
            else
            {
                roundDES = 2;
                strings = binaryStrings(states[4], statesB[4]);

                toStringArray(roundDES);

                int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                avgNrDiffBit = avgNrperByte(nrDiffBits);
                emptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                 {
                     displayBinaryValuesDES();
                     showBitSequence(strings);
                     lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
                     showStatistics(nrDiffBits, lengthIdentSequenceDes, strings);
                     setColors();
                     setAngles(angle_1, angle_2);
                     setToolTips();
                 }, null);

            }
        }

        private void afterRound3Button_Click(object sender, RoutedEventArgs e)
        {
            var strings = binaryStrings(states[12], statesB[12]);
            clearElements();
            changeRoundNr(3);
            showElements();
            removeColors();

            if (mode == 0)
            {

                roundNumber = 4 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits = nrOfBitsFlipped(states[12], statesB[12]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                emptyInformation();
                int lengthIdentSequence;
                avgNrDiffBit = avgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    printIntermediateStates(states, statesB);
                    displayBinaryValues(states[12], statesB[12]);
                    showBitSequence(strings);
                    lengthIdentSequence = longestIdenticalSequence(differentBits);
                    showStatistics(nrDiffBits, lengthIdentSequence, strings);
                    setColors();
                    setAngles(angle_1, angle_2);
                    setToolTips();

                }, null);
                slideNr = 5;
            }
            else
            {
                roundDES = 3;
                strings = binaryStrings(states[4], statesB[4]);
                toStringArray(roundDES);


                int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                avgNrDiffBit = avgNrperByte(nrDiffBits);
                emptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    displayBinaryValuesDES();
                    showBitSequence(strings);
                    lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
                    showStatistics(nrDiffBits, lengthIdentSequenceDes, strings);
                    setColors();
                    setAngles(angle_1, angle_2);
                    setToolTips();
                }, null);

            }
        }

        private void afterRound4Button_Click(object sender, RoutedEventArgs e)
        {
            var strings = binaryStrings(states[12], statesB[12]);
            clearElements();
            changeRoundNr(4);
            showElements();
            removeColors();

            if (mode == 0)
            {
                roundNumber = 5 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits = nrOfBitsFlipped(states[16], statesB[16]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                emptyInformation();
                int lengthIdentSequence;
                avgNrDiffBit = avgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    printIntermediateStates(states, statesB);
                    displayBinaryValues(states[16], statesB[16]);
                    showBitSequence(strings);
                    lengthIdentSequence = longestIdenticalSequence(differentBits);
                    showStatistics(nrDiffBits, lengthIdentSequence, strings);
                    setColors();
                    setAngles(angle_1, angle_2);
                    setToolTips();

                }, null);

                slideNr = 5;
            }
            else
            {
                roundDES = 4;
                strings = binaryStrings(states[4], statesB[4]);
                toStringArray(roundDES);


                int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                avgNrDiffBit = avgNrperByte(nrDiffBits);
                emptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    displayBinaryValuesDES();
                    showBitSequence(strings);
                    lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
                    showStatistics(nrDiffBits, lengthIdentSequenceDes, strings);
                    setColors();
                    setAngles(angle_1, angle_2);
                    setToolTips();
                }, null);

            }
        }

        private void afterRound5Button_Click(object sender, RoutedEventArgs e)
        {
            var strings = binaryStrings(states[20], statesB[20]);
            clearElements();
            changeRoundNr(5);
            showElements();
            removeColors();

            if (mode == 0)
            {
                roundNumber = 6 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits = nrOfBitsFlipped(states[20], statesB[20]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                emptyInformation();
                int lengthIdentSequence;
                avgNrDiffBit = avgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    printIntermediateStates(states, statesB);
                    displayBinaryValues(states[20], statesB[20]);
                    showBitSequence(strings);
                    lengthIdentSequence = longestIdenticalSequence(differentBits);
                    showStatistics(nrDiffBits, lengthIdentSequence, strings);
                    setColors();
                    setAngles(angle_1, angle_2);
                    setToolTips();

                }, null);

                slideNr = 5;
            }
            else
            {
                roundDES = 5;
                strings = binaryStrings(states[4], statesB[4]);
                toStringArray(roundDES);


                int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                avgNrDiffBit = avgNrperByte(nrDiffBits);
                emptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    displayBinaryValuesDES();
                    showBitSequence(strings);
                    lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
                    showStatistics(nrDiffBits, lengthIdentSequenceDes, strings);
                    setColors();
                    setAngles(angle_1, angle_2);
                    setToolTips();
                }, null);

            }
        }

        private void afterRound6Button_Click(object sender, RoutedEventArgs e)
        {
            var strings = binaryStrings(states[24], statesB[24]);
            clearElements();
            changeRoundNr(6);
            showElements();
            removeColors();

            if (mode == 0)
            {
                roundNumber = 7 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits = nrOfBitsFlipped(states[24], statesB[24]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                emptyInformation();
                int lengthIdentSequence;
                avgNrDiffBit = avgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    printIntermediateStates(states, statesB);
                    displayBinaryValues(states[24], statesB[24]);
                    showBitSequence(strings);
                    lengthIdentSequence = longestIdenticalSequence(differentBits);
                    showStatistics(nrDiffBits, lengthIdentSequence, strings);
                    setColors();
                    setAngles(angle_1, angle_2);
                    setToolTips();

                }, null);

                slideNr = 5;
            }
            else
            {
                roundDES = 6;
                strings = binaryStrings(states[4], statesB[4]);
                toStringArray(roundDES);

                int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                avgNrDiffBit = avgNrperByte(nrDiffBits);
                emptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {

                    displayBinaryValuesDES();
                    showBitSequence(strings);
                    lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
                    showStatistics(nrDiffBits, lengthIdentSequenceDes, strings);
                    setColors();
                    setAngles(angle_1, angle_2);
                    setToolTips();

                }, null);

            }
        }

        private void afterRound7Button_Click(object sender, RoutedEventArgs e)
        {
            var strings = binaryStrings(states[28], statesB[28]);
            clearElements();
            changeRoundNr(7);
            showElements();
            removeColors();

            if (mode == 0)
            {

                roundNumber = 8 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits = nrOfBitsFlipped(states[28], statesB[28]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                emptyInformation();
                int lengthIdentSequence;
                avgNrDiffBit = avgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    printIntermediateStates(states, statesB);
                    displayBinaryValues(states[28], statesB[28]);
                    showBitSequence(strings);
                    lengthIdentSequence = longestIdenticalSequence(differentBits);
                    showStatistics(nrDiffBits, lengthIdentSequence, strings);
                    setColors();
                    setAngles(angle_1, angle_2);
                    setToolTips();

                }, null);

                slideNr = 5;

            }
            else
            {
                roundDES = 7;
                strings = binaryStrings(states[4], statesB[4]);
                toStringArray(roundDES);

                int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                avgNrDiffBit = avgNrperByte(nrDiffBits);
                emptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    displayBinaryValuesDES();
                    showBitSequence(strings);
                    lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
                    showStatistics(nrDiffBits, lengthIdentSequenceDes, strings);
                    setColors();
                    setAngles(angle_1, angle_2);
                    setToolTips();
                }, null);

            }
        }

        private void afterRound8Button_Click(object sender, RoutedEventArgs e)
        {
            var strings = binaryStrings(states[32], statesB[32]);
            clearElements();
            changeRoundNr(8);
            showElements();
            removeColors();

            if (mode == 0)
            {
                roundNumber = 9 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits = nrOfBitsFlipped(states[32], statesB[32]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                emptyInformation();
                int lengthIdentSequence;
                avgNrDiffBit = avgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    printIntermediateStates(states, statesB);
                    displayBinaryValues(states[32], statesB[32]);
                    showBitSequence(strings);
                    lengthIdentSequence = longestIdenticalSequence(differentBits);
                    showStatistics(nrDiffBits, lengthIdentSequence, strings);
                    setColors();
                    setAngles(angle_1, angle_2);
                    setToolTips();

                }, null);

                slideNr = 5;
            }
            else
            {
                roundDES = 8;
                strings = binaryStrings(states[4], statesB[4]);
                toStringArray(roundDES);

                int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                avgNrDiffBit = avgNrperByte(nrDiffBits);
                emptyInformation();


                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    displayBinaryValuesDES();
                    showBitSequence(strings);
                    lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
                    showStatistics(nrDiffBits, lengthIdentSequenceDes, strings);
                    setColors();
                    setAngles(angle_1, angle_2);
                    setToolTips();
                }, null);

            }
        }

        private void afterRound9Button_Click(object sender, RoutedEventArgs e)
        {
            var strings = binaryStrings(states[36], statesB[36]);
            clearElements();
            changeRoundNr(9);
            showElements();
            removeColors();

            if (mode == 0)
            {
                roundNumber = 10 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits = nrOfBitsFlipped(states[36], statesB[36]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                emptyInformation();
                int lengthIdentSequence;
                avgNrDiffBit = avgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    printIntermediateStates(states, statesB);
                    displayBinaryValues(states[36], statesB[36]);
                    showBitSequence(strings);
                    lengthIdentSequence = longestIdenticalSequence(differentBits);
                    showStatistics(nrDiffBits, lengthIdentSequence, strings);
                    setColors();
                    setAngles(angle_1, angle_2);
                    setToolTips();

                }, null);

                slideNr = 5;
            }
            else
            {
                roundDES = 9;
                strings = binaryStrings(states[4], statesB[4]);
                toStringArray(roundDES);

                int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                avgNrDiffBit = avgNrperByte(nrDiffBits);
                emptyInformation();


                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {

                    displayBinaryValuesDES();
                    showBitSequence(strings);
                    lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
                    showStatistics(nrDiffBits, lengthIdentSequenceDes, strings);
                    setColors();
                    setAngles(angle_1, angle_2);
                    setToolTips();

                }, null);

            }
        }

        //after 10
        private void afterRound10Button_Click(object sender, RoutedEventArgs e)
        {
            Tuple<string, string> strings;
            clearElements();
            changeRoundNr(10);
            showElements();
            removeColors();

            if (mode == 0)
            {
                roundNumber = 11 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits;
                double angle_1;
                double angle_2;
                int lengthIdentSequence;
                emptyInformation();

                if (keysize == 0)
                {

                    strings = binaryStrings(states[39], statesB[39]);
                    nrDiffBits = nrOfBitsFlipped(states[39], statesB[39]);
                    angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                    angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                    avalanche = calcAvalancheEffect(nrDiffBits, strings);
                    avgNrDiffBit = avgNrperByte(nrDiffBits);

                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {

                        final();
                        displayBinaryValues(states[39], statesB[39]);
                        showBitSequence(strings);
                        lengthIdentSequence = longestIdenticalSequence(differentBits);
                        showStatistics(nrDiffBits, lengthIdentSequence, strings);
                        setColors();
                        setAngles(angle_1, angle_2);
                        setToolTips();



                    }, null);
                }
                else
                {
                    strings = binaryStrings(states[40], statesB[40]);
                    nrDiffBits = nrOfBitsFlipped(states[40], statesB[40]);
                    angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                    angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                    avalanche = calcAvalancheEffect(nrDiffBits, strings);
                    avgNrDiffBit = avgNrperByte(nrDiffBits);

                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {

                        printIntermediateStates(states, statesB);
                        displayBinaryValues(states[40], statesB[40]);
                        showBitSequence(strings);
                        lengthIdentSequence = longestIdenticalSequence(differentBits);
                        showStatistics(nrDiffBits, lengthIdentSequence, strings);
                        setColors();
                        setAngles(angle_1, angle_2);
                        setToolTips();



                    }, null);
                }
                slideNr = 5;
            }
            else
            {
                roundDES = 10;
                strings = binaryStrings(states[4], statesB[4]);
                toStringArray(roundDES);

                int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                avgNrDiffBit = avgNrperByte(nrDiffBits);
                emptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {

                    displayBinaryValuesDES();
                    showBitSequence(strings);
                    lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
                    showStatistics(nrDiffBits, lengthIdentSequenceDes, strings);
                    setColors();
                    setAngles(angle_1, angle_2);
                    setToolTips();

                }, null);

            }
        }

        private void afterRound11Button_Click(object sender, RoutedEventArgs e)
        {
            var strings = binaryStrings(states[4], statesB[4]);
            clearElements();
            changeRoundNr(11);
            showElements();
            removeColors();

            if (mode == 0)
            {
                roundNumber = 12 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits = nrOfBitsFlipped(states[44], statesB[44]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                emptyInformation();
                int lengthIdentSequence;
                avgNrDiffBit = avgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    printIntermediateStates(states, statesB);
                    displayBinaryValues(states[44], statesB[44]);
                    showBitSequence(strings);
                    lengthIdentSequence = longestIdenticalSequence(differentBits);
                    showStatistics(nrDiffBits, lengthIdentSequence, strings);
                    setColors();
                    setAngles(angle_1, angle_2);
                    setToolTips();

                }, null);

                slideNr = 5;
            }
            else
            {

                roundDES = 11;
                strings = binaryStrings(states[4], statesB[4]);
                toStringArray(roundDES);

                int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                avgNrDiffBit = avgNrperByte(nrDiffBits);
                emptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    displayBinaryValuesDES();
                    showBitSequence(strings);
                    lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
                    showStatistics(nrDiffBits, lengthIdentSequenceDes, strings);
                    setColors();
                    setAngles(angle_1, angle_2);
                    setToolTips();

                }, null);

            }
        }

        private void afterRound12Button_Click(object sender, RoutedEventArgs e)
        {
            Tuple<string, string> strings;
            clearElements();
            changeRoundNr(12);
            showElements();
            removeColors();

            if (mode == 0)
            {
                roundNumber = 13 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits;
                double angle_1;
                double angle_2;
                int lengthIdentSequence;
                emptyInformation();


                if (keysize == 1)
                {
                    strings = binaryStrings(states[47], statesB[47]);
                    nrDiffBits = nrOfBitsFlipped(states[47], statesB[47]);
                    angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                    angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                    avalanche = calcAvalancheEffect(nrDiffBits, strings);
                    avgNrDiffBit = avgNrperByte(nrDiffBits);

                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {

                        final();
                        displayBinaryValues(states[47], statesB[47]);
                        showBitSequence(strings);
                        lengthIdentSequence = longestIdenticalSequence(differentBits);
                        showStatistics(nrDiffBits, lengthIdentSequence, strings);
                        setColors();
                        setAngles(angle_1, angle_2);
                        setToolTips();



                    }, null);
                }
                else
                {
                    strings = binaryStrings(states[48], statesB[48]);
                    nrDiffBits = nrOfBitsFlipped(states[48], statesB[48]);
                    angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                    angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                    avalanche = calcAvalancheEffect(nrDiffBits, strings);
                    avgNrDiffBit = avgNrperByte(nrDiffBits);

                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {

                        printIntermediateStates(states, statesB);
                        displayBinaryValues(states[48], statesB[48]);
                        showBitSequence(strings);
                        lengthIdentSequence = longestIdenticalSequence(differentBits);
                        showStatistics(nrDiffBits, lengthIdentSequence, strings);
                        setColors();
                        setAngles(angle_1, angle_2);
                        setToolTips();



                    }, null);
                }

                slideNr = 5;

            }
            else
            {
                roundDES = 12;
                strings = binaryStrings(states[4], statesB[4]);
                toStringArray(roundDES);

                int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                avgNrDiffBit = avgNrperByte(nrDiffBits);
                emptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    displayBinaryValuesDES();
                    showBitSequence(strings);
                    lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
                    showStatistics(nrDiffBits, lengthIdentSequenceDes, strings);
                    setColors();
                    setAngles(angle_1, angle_2);
                    setToolTips();

                }, null);

            }
        }



        private void afterRound13Button_Click(object sender, RoutedEventArgs e)
        {
            clearElements();
            changeRoundNr(13);
            showElements();
            removeColors();

            if (mode == 0)
            {
                var strings = binaryStrings(states[52], statesB[52]);


                roundNumber = 14 + shift * 2 * keysize;
                action = 1;

                int nrDiffBits = nrOfBitsFlipped(states[52], statesB[52]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                emptyInformation();
                int lengthIdentSequence;
                avgNrDiffBit = avgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    printIntermediateStates(states, statesB);
                    displayBinaryValues(states[52], statesB[52]);
                    showBitSequence(strings);
                    lengthIdentSequence = longestIdenticalSequence(differentBits);
                    showStatistics(nrDiffBits, lengthIdentSequence, strings);
                    setColors();
                    setAngles(angle_1, angle_2);
                    setToolTips();

                }, null);

                slideNr = 5;
            }
            else
            {
                roundDES = 13;
                var strings = binaryStrings(states[4], statesB[4]);
                toStringArray(roundDES);

                int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                avgNrDiffBit = avgNrperByte(nrDiffBits);
                emptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {

                    displayBinaryValuesDES();
                    showBitSequence(strings);
                    lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
                    showStatistics(nrDiffBits, lengthIdentSequenceDes, strings);
                    setColors();
                    setAngles(angle_1, angle_2);
                    setToolTips();

                }, null);

            }
        }

        private void afterRound14Button_Click(object sender, RoutedEventArgs e)
        {
            clearElements();
            changeRoundNr(14);
            showElements();
            removeColors();

            if (mode == 0)
            {
                var strings = binaryStrings(states[55], statesB[55]);

                action = 1;

                int nrDiffBits = nrOfBitsFlipped(states[55], statesB[55]);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                emptyInformation();
                int lengthIdentSequence;
                avgNrDiffBit = avgNrperByte(nrDiffBits);

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    final();
                    displayBinaryValues(states[55], statesB[55]);
                    showBitSequence(strings);
                    lengthIdentSequence = longestIdenticalSequence(differentBits);
                    showStatistics(nrDiffBits, lengthIdentSequence, strings);
                    setColors();
                    setAngles(angle_1, angle_2);
                    setToolTips();

                }, null);

                slideNr = 5;
            }
            else
            {

                roundDES = 14;
                var strings = binaryStrings(states[4], statesB[4]);
                toStringArray(roundDES);

                int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
                double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
                double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
                avalanche = calcAvalancheEffect(nrDiffBits, strings);
                int lengthIdentSequenceDes;
                avgNrDiffBit = avgNrperByte(nrDiffBits);
                emptyInformation();

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    displayBinaryValuesDES();
                    showBitSequence(strings);
                    lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
                    showStatistics(nrDiffBits, lengthIdentSequenceDes, strings);
                    setColors();
                    setAngles(angle_1, angle_2);
                    setToolTips();
                }, null);

            }
        }

        private void afterRound15Button_Click(object sender, RoutedEventArgs e)
        {
            clearElements();
            changeRoundNr(15);
            showElements();
            removeColors();

            roundDES = 15;
            var strings = binaryStrings(states[4], statesB[4]);
            toStringArray(roundDES);

            int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
            double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
            double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
            avalanche = calcAvalancheEffect(nrDiffBits, strings);
            int lengthIdentSequenceDes;
            avgNrDiffBit = avgNrperByte(nrDiffBits);
            emptyInformation();

            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                displayBinaryValuesDES();
                showBitSequence(strings);
                lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
                showStatistics(nrDiffBits, lengthIdentSequenceDes, strings);
                setColors();
                setAngles(angle_1, angle_2);
                setToolTips();
            }, null);
        }

        private void afterRound16Button_Click(object sender, RoutedEventArgs e)
        {

            clearElements();
            changeRoundNr(16);
            showElements();
            removeColors();

            roundDES = 16;
            var strings = binaryStrings(states[4], statesB[4]);
            toStringArray(roundDES);

            int nrDiffBits = nrOfBitsFlipped(seqA, seqB);
            double angle_1 = flippedBitsPiece.calculateAngle(nrDiffBits, strings);
            double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - nrDiffBits, strings);
            avalanche = calcAvalancheEffect(nrDiffBits, strings);
            int lengthIdentSequenceDes;
            avgNrDiffBit = avgNrperByte(nrDiffBits);
            emptyInformation();


            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                displayBinaryValuesDES();
                showBitSequence(strings);
                lengthIdentSequenceDes = longestIdenticalSequence(differentBits);
                showStatistics(nrDiffBits, lengthIdentSequenceDes, strings);
                setColors();
                setAngles(angle_1, angle_2);
                setToolTips();
            }, null);
        }

        public void final()
        {

            List<TextBlock> tmp = createTxtBlockList(3);
            List<TextBlock> tmpB = createTxtBlockList(4);

            byte[] state = { 0 };
            byte[] stateB = { 0 };

            switch (keysize)
            {
                case 0:
                    state = arrangeText(states[39]);
                    stateB = arrangeText(statesB[39]);
                    break;
                case 1:
                    state = arrangeText(states[47]);
                    stateB = arrangeText(statesB[47]);
                    break;
                case 2:
                    state = arrangeText(states[55]);
                    stateB = arrangeText(statesB[55]);
                    break;
                default:
                    break;
            }

            int i = 0;
            int j = 0;


            foreach (TextBlock txtBlock in tmp)
            {
                txtBlock.Text = state[i].ToString("X2");
                i++;
            }

            foreach (TextBlock txtBlock in tmpB)
            {
                txtBlock.Text = stateB[j].ToString("X2");
                j++;
            }

        }

        private void radioButton1Checked(object sender, RoutedEventArgs e)
        {
            string strA = binaryAsString(textA).Replace(" ", "");
            string strB = binaryAsString(textB).Replace(" ", "");

            string firstHalf = strA.Substring(0, 32);
            string secondHalf = strA.Substring(32, 32);
            string firstHalfB = strB.Substring(0, 32);
            string secondHalfB = strB.Substring(32, 32);

            origTextDES.Text = string.Format("{0}{1}{2}", firstHalf, Environment.NewLine, secondHalf);
            modTextDES.Text = string.Format("{0}{1}{2}", firstHalfB, Environment.NewLine, secondHalfB);


            string keyStrA = binaryAsString(keyA).Replace(" ", "");
            string keyStrB = binaryAsString(key).Replace(" ", "");

            string firstKeyHalf = strA.Substring(0, 32);
            string secondKeyHalf = strA.Substring(32, 32);
            string firstKeyHalfB = strB.Substring(0, 32);
            string secondKeyHalfB = strB.Substring(32, 32);

            origKeyDES.Text = string.Format("{0}{1}{2}", firstKeyHalf, Environment.NewLine, secondKeyHalf);
            modKeyDES.Text = string.Format("{0}{1}{2}", firstKeyHalfB, Environment.NewLine, secondKeyHalfB);

        }



        private void radioButton2Checked(object sender, RoutedEventArgs e)
        {
            if (mode == 0)
            {
                var encoding = Encoding.GetEncoding(437);

                int i = 1;
                int j = 33;
                while (i <= 16 && j <= 48)
                {
                    ((TextBlock)this.FindName("initStateTxtBlock" + i)).Text = textA[i - 1].ToString();
                    ((TextBlock)this.FindName("initStateTxtBlock" + j)).Text = textB[i - 1].ToString();

                    i++;
                    j++;
                }

                if (keysize == 0)
                {
                    int k = 1;
                    int l = 17;
                    int m = 49;
                    while (k <= 16 && l <= 32 && m <= 64)
                    {
                        ((TextBlock)this.FindName("initStateTxtBlock" + l)).Text = keyA[k - 1].ToString();
                        ((TextBlock)this.FindName("initStateTxtBlock" + m)).Text = key[k - 1].ToString();
                        k++;
                        l++;
                        m++;
                    }
                }
                else if (keysize == 1)
                {
                    int k = 1;


                    while (k <= 24)
                    {
                        ((TextBlock)this.FindName("initStateKey192_" + k)).Text = keyA[k - 1].ToString();
                        ((TextBlock)this.FindName("modKey192_" + k)).Text = key[k - 1].ToString();
                        k++;

                    }
                }
                else
                {
                    int k = 1;


                    while (k <= 32)
                    {
                        ((TextBlock)this.FindName("initStateKey256_" + k)).Text = keyA[k - 1].ToString();
                        ((TextBlock)this.FindName("modKey256_" + k)).Text = key[k - 1].ToString();
                        k++;

                    }
                }
            }
            else if (mode == 1)
            {

                string strA = decimalAsString(textA);
                string strB = decimalAsString(textB);

                origTextDES.Text = strA;
                modTextDES.Text = strB;

                string keyStrA = decimalAsString(keyA);
                string keyStrB = decimalAsString(key);

                origKeyDES.Text = keyStrA;
                modKeyDES.Text = keyStrB;
            }
            else
            {
                originalMsg.Text = decimalAsString(unchangedCipher);
                modifiedMsg.Text = decimalAsString(changedCipher);
            }
        }


        private void radioButton3Checked(object sender, RoutedEventArgs e)
        {
            if (mode == 0)
            {

                int i = 1;
                int j = 33;
                while (i <= 16 && j <= 48)
                {
                    ((TextBlock)this.FindName("initStateTxtBlock" + i)).Text = textA[i - 1].ToString("X2");
                    ((TextBlock)this.FindName("initStateTxtBlock" + j)).Text = textB[i - 1].ToString("X2");
                    i++;
                    j++;
                }

                if (keysize == 0)
                {
                    int k = 1;
                    int l = 17;
                    int m = 49;
                    while (k <= 16 && l <= 32 && m <= 64)
                    {
                        ((TextBlock)this.FindName("initStateTxtBlock" + l)).Text = keyA[k - 1].ToString("X2");
                        ((TextBlock)this.FindName("initStateTxtBlock" + m)).Text = key[k - 1].ToString("X2");
                        k++;
                        l++;
                        m++;
                    }
                }
                else if (keysize == 1)
                {
                    int k = 1;


                    while (k <= 24)
                    {
                        ((TextBlock)this.FindName("initStateKey192_" + k)).Text = keyA[k - 1].ToString("X2");
                        ((TextBlock)this.FindName("modKey192_" + k)).Text = key[k - 1].ToString("X2");
                        k++;

                    }
                }
                else
                {
                    int k = 1;


                    while (k <= 32)
                    {
                        ((TextBlock)this.FindName("initStateKey256_" + k)).Text = keyA[k - 1].ToString("X2");
                        ((TextBlock)this.FindName("modKey256_" + k)).Text = key[k - 1].ToString("X2");
                        k++;

                    }
                }

            }
            else if (mode == 1)
            {
                string strA = hexaAsString(textA);
                string strB = hexaAsString(textB);

                origTextDES.Text = strA;
                modTextDES.Text = strB;

                string keyStrA = hexaAsString(keyA);
                string keyStrB = hexaAsString(key);

                origKeyDES.Text = keyStrA;
                modKeyDES.Text = keyStrB;
            }
            else
            {
                originalMsg.Text = hexaAsString(unchangedCipher);
                modifiedMsg.Text = hexaAsString(changedCipher);
            }

        }


        private void txtBox1_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox source = e.Source as TextBox;

            string txtBoxName = source.Name;
            Brush greenBrush = (Brush)new BrushConverter().ConvertFromString("#059033");

            switch (txtBoxName)
            {
                case "txtBox1":
                    source.Background = greenBrush;
                    roundZero1.Background = greenBrush;
                    sBoxRound1_1.Background = greenBrush;
                    afterShifting(1, 1).Background = greenBrush;

                    brushColumns(1, 1);

                    sBoxRound2_1.Background = greenBrush;
                    sBoxRound2_2.Background = greenBrush;
                    sBoxRound2_3.Background = greenBrush;
                    sBoxRound2_4.Background = greenBrush;

                    afterShifting(1, 2).Background = greenBrush;
                    afterShifting(2, 2).Background = greenBrush;
                    afterShifting(3, 2).Background = greenBrush;
                    afterShifting(4, 2).Background = greenBrush;

                    brushRemainingColumns();
                    connectingLines(1, false);
                    break;
                case "txtBox2":
                    source.Background = greenBrush;
                    roundZero2.Background = greenBrush;
                    sBoxRound1_2.Background = greenBrush;
                    afterShifting(2, 1).Background = greenBrush;

                    brushColumns(13, 1);

                    sBoxRound2_13.Background = greenBrush;
                    sBoxRound2_14.Background = greenBrush;
                    sBoxRound2_15.Background = greenBrush;
                    sBoxRound2_16.Background = greenBrush;

                    afterShifting(13, 2).Background = greenBrush;
                    afterShifting(14, 2).Background = greenBrush;
                    afterShifting(15, 2).Background = greenBrush;
                    afterShifting(16, 2).Background = greenBrush;

                    brushRemainingColumns();
                    connectingLines(2, false);
                    break;
                case "txtBox3":
                    source.Background = greenBrush;
                    roundZero3.Background = greenBrush;
                    sBoxRound1_3.Background = greenBrush;
                    afterShifting(3, 1).Background = greenBrush;


                    brushColumns(9, 1);

                    sBoxRound2_9.Background = greenBrush;
                    sBoxRound2_10.Background = greenBrush;
                    sBoxRound2_11.Background = greenBrush;
                    sBoxRound2_12.Background = greenBrush;

                    afterShifting(9, 2).Background = greenBrush;
                    afterShifting(10, 2).Background = greenBrush;
                    afterShifting(11, 2).Background = greenBrush;
                    afterShifting(12, 2).Background = greenBrush;

                    brushRemainingColumns();
                    connectingLines(3, false);
                    break;
                case "txtBox4":
                    source.Background = greenBrush;
                    roundZero4.Background = greenBrush;
                    sBoxRound1_4.Background = greenBrush;
                    afterShifting(4, 1).Background = greenBrush;

                    brushColumns(5, 1);

                    sBoxRound2_5.Background = greenBrush;
                    sBoxRound2_6.Background = greenBrush;
                    sBoxRound2_7.Background = greenBrush;
                    sBoxRound2_8.Background = greenBrush;

                    afterShifting(5, 2).Background = greenBrush;
                    afterShifting(6, 2).Background = greenBrush;
                    afterShifting(7, 2).Background = greenBrush;
                    afterShifting(8, 2).Background = greenBrush;

                    brushRemainingColumns();
                    connectingLines(4, false);

                    break;
                case "txtBox5":
                    source.Background = greenBrush;
                    roundZero5.Background = greenBrush;
                    sBoxRound1_5.Background = greenBrush;
                    afterShifting(5, 1).Background = greenBrush;

                    brushColumns(5, 1);

                    sBoxRound2_5.Background = greenBrush;
                    sBoxRound2_6.Background = greenBrush;
                    sBoxRound2_7.Background = greenBrush;
                    sBoxRound2_8.Background = greenBrush;

                    afterShifting(5, 2).Background = greenBrush;
                    afterShifting(6, 2).Background = greenBrush;
                    afterShifting(7, 2).Background = greenBrush;
                    afterShifting(8, 2).Background = greenBrush;

                    brushRemainingColumns();
                    connectingLines(5, false);
                    break;
                case "txtBox6":
                    source.Background = greenBrush;
                    roundZero6.Background = greenBrush;
                    sBoxRound1_6.Background = greenBrush;
                    afterShifting(6, 1).Background = greenBrush;

                    brushColumns(1, 1);

                    sBoxRound2_1.Background = greenBrush;
                    sBoxRound2_2.Background = greenBrush;
                    sBoxRound2_3.Background = greenBrush;
                    sBoxRound2_4.Background = greenBrush;

                    afterShifting(1, 2).Background = greenBrush;
                    afterShifting(2, 2).Background = greenBrush;
                    afterShifting(3, 2).Background = greenBrush;
                    afterShifting(4, 2).Background = greenBrush;

                    brushRemainingColumns();
                    connectingLines(6, false);
                    break;
                case "txtBox7":
                    source.Background = greenBrush;
                    roundZero7.Background = greenBrush;
                    sBoxRound1_7.Background = greenBrush;
                    afterShifting(7, 1).Background = greenBrush;

                    brushColumns(13, 1);

                    sBoxRound2_13.Background = greenBrush;
                    sBoxRound2_14.Background = greenBrush;
                    sBoxRound2_15.Background = greenBrush;
                    sBoxRound2_16.Background = greenBrush;


                    afterShifting(13, 2).Background = greenBrush;
                    afterShifting(14, 2).Background = greenBrush;
                    afterShifting(15, 2).Background = greenBrush;
                    afterShifting(16, 2).Background = greenBrush;


                    brushRemainingColumns();
                    connectingLines(7, false);
                    break;
                case "txtBox8":
                    source.Background = greenBrush;
                    roundZero8.Background = greenBrush;
                    sBoxRound1_8.Background = greenBrush;
                    afterShifting(8, 1).Background = greenBrush;

                    brushColumns(9, 1);

                    sBoxRound2_9.Background = greenBrush;
                    sBoxRound2_10.Background = greenBrush;
                    sBoxRound2_11.Background = greenBrush;
                    sBoxRound2_12.Background = greenBrush;


                    afterShifting(9, 2).Background = greenBrush;
                    afterShifting(10, 2).Background = greenBrush;
                    afterShifting(11, 2).Background = greenBrush;
                    afterShifting(12, 2).Background = greenBrush;

                    brushRemainingColumns();
                    connectingLines(8, false);
                    break;
                case "txtBox9":
                    source.Background = greenBrush;
                    roundZero9.Background = greenBrush;
                    sBoxRound1_9.Background = greenBrush;
                    afterShifting(9, 1).Background = greenBrush;

                    brushColumns(9, 1);

                    sBoxRound2_9.Background = greenBrush;
                    sBoxRound2_10.Background = greenBrush;
                    sBoxRound2_11.Background = greenBrush;
                    sBoxRound2_12.Background = greenBrush;


                    afterShifting(9, 2).Background = greenBrush;
                    afterShifting(10, 2).Background = greenBrush;
                    afterShifting(11, 2).Background = greenBrush;
                    afterShifting(12, 2).Background = greenBrush;

                    brushRemainingColumns();
                    connectingLines(9, false);
                    break;
                case "txtBox10":
                    source.Background = greenBrush;
                    roundZero10.Background = greenBrush;
                    sBoxRound1_10.Background = greenBrush;
                    afterShifting(10, 1).Background = greenBrush;

                    brushColumns(5, 1);

                    sBoxRound2_5.Background = greenBrush;
                    sBoxRound2_6.Background = greenBrush;
                    sBoxRound2_7.Background = greenBrush;
                    sBoxRound2_8.Background = greenBrush;

                    afterShifting(5, 2).Background = greenBrush;
                    afterShifting(6, 2).Background = greenBrush;
                    afterShifting(7, 2).Background = greenBrush;
                    afterShifting(8, 2).Background = greenBrush;

                    brushRemainingColumns();
                    connectingLines(10, false);
                    break;
                case "txtBox11":
                    source.Background = greenBrush;
                    roundZero11.Background = greenBrush;
                    sBoxRound1_11.Background = greenBrush;
                    afterShifting(11, 1).Background = greenBrush;

                    brushColumns(1, 1);

                    sBoxRound2_1.Background = greenBrush;
                    sBoxRound2_2.Background = greenBrush;
                    sBoxRound2_3.Background = greenBrush;
                    sBoxRound2_4.Background = greenBrush;

                    afterShifting(1, 2).Background = greenBrush;
                    afterShifting(2, 2).Background = greenBrush;
                    afterShifting(3, 2).Background = greenBrush;
                    afterShifting(4, 2).Background = greenBrush;

                    brushRemainingColumns();
                    connectingLines(11, false);
                    break;
                case "txtBox12":
                    source.Background = greenBrush;
                    roundZero12.Background = greenBrush;
                    sBoxRound1_12.Background = greenBrush;
                    afterShifting(12, 1).Background = greenBrush;

                    brushColumns(13, 1);

                    sBoxRound2_13.Background = greenBrush;
                    sBoxRound2_14.Background = greenBrush;
                    sBoxRound2_15.Background = greenBrush;
                    sBoxRound2_16.Background = greenBrush;


                    afterShifting(13, 2).Background = greenBrush;
                    afterShifting(14, 2).Background = greenBrush;
                    afterShifting(15, 2).Background = greenBrush;
                    afterShifting(16, 2).Background = greenBrush;
                    brushRemainingColumns();
                    connectingLines(12, false);
                    break;
                case "txtBox13":
                    source.Background = greenBrush;
                    roundZero13.Background = greenBrush;
                    sBoxRound1_13.Background = greenBrush;
                    afterShifting(13, 1).Background = greenBrush;

                    brushColumns(13, 1);

                    sBoxRound2_13.Background = greenBrush;
                    sBoxRound2_14.Background = greenBrush;
                    sBoxRound2_15.Background = greenBrush;
                    sBoxRound2_16.Background = greenBrush;

                    afterShifting(13, 2).Background = greenBrush;
                    afterShifting(14, 2).Background = greenBrush;
                    afterShifting(15, 2).Background = greenBrush;
                    afterShifting(16, 2).Background = greenBrush;

                    brushRemainingColumns();
                    connectingLines(13, false);
                    break;
                case "txtBox14":
                    source.Background = greenBrush;
                    roundZero14.Background = greenBrush;
                    sBoxRound1_14.Background = greenBrush;
                    afterShifting(14, 1).Background = greenBrush;


                    brushColumns(9, 1);

                    sBoxRound2_9.Background = greenBrush;
                    sBoxRound2_10.Background = greenBrush;
                    sBoxRound2_11.Background = greenBrush;
                    sBoxRound2_12.Background = greenBrush;

                    afterShifting(9, 2).Background = greenBrush;
                    afterShifting(10, 2).Background = greenBrush;
                    afterShifting(11, 2).Background = greenBrush;
                    afterShifting(12, 2).Background = greenBrush;

                    brushRemainingColumns();
                    connectingLines(14, false);
                    break;
                case "txtBox15":
                    source.Background = greenBrush;
                    roundZero15.Background = greenBrush;
                    sBoxRound1_15.Background = greenBrush;
                    afterShifting(15, 1).Background = greenBrush;

                    brushColumns(5, 1);

                    sBoxRound2_5.Background = greenBrush;
                    sBoxRound2_6.Background = greenBrush;
                    sBoxRound2_7.Background = greenBrush;
                    sBoxRound2_8.Background = greenBrush;

                    afterShifting(5, 2).Background = greenBrush;
                    afterShifting(6, 2).Background = greenBrush;
                    afterShifting(7, 2).Background = greenBrush;
                    afterShifting(8, 2).Background = greenBrush;


                    brushRemainingColumns();
                    connectingLines(15, false);
                    break;
                case "txtBox16":
                    source.Background = greenBrush;
                    roundZero16.Background = greenBrush;
                    sBoxRound1_16.Background = greenBrush;
                    afterShifting(16, 1).Background = greenBrush;

                    brushColumns(1, 1);

                    sBoxRound2_1.Background = greenBrush;
                    sBoxRound2_2.Background = greenBrush;
                    sBoxRound2_3.Background = greenBrush;
                    sBoxRound2_4.Background = greenBrush;

                    afterShifting(1, 2).Background = greenBrush;
                    afterShifting(2, 2).Background = greenBrush;
                    afterShifting(3, 2).Background = greenBrush;
                    afterShifting(4, 2).Background = greenBrush;

                    brushRemainingColumns();
                    connectingLines(16, false);
                    break;

                default:
                    break;
            }


        }

        private void txtBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox source = e.Source as TextBox;
            string txtBoxName = source.Name;

            switch (txtBoxName)
            {
                case "txtBox1":
                    source.Background = Brushes.Transparent;
                    roundZero1.Background = Brushes.Transparent;
                    sBoxRound1_1.Background = Brushes.Transparent;
                    shiftRowRound1_1.Background = Brushes.Transparent;

                    mixColumns1_1.Background = Brushes.Transparent;
                    mixColumns1_2.Background = Brushes.Transparent;
                    mixColumns1_3.Background = Brushes.Transparent;
                    mixColumns1_4.Background = Brushes.Transparent;


                    addKey1_1.Background = Brushes.Transparent;
                    addKey1_2.Background = Brushes.Transparent;
                    addKey1_3.Background = Brushes.Transparent;
                    addKey1_4.Background = Brushes.Transparent;

                    sBoxRound2_1.Background = Brushes.Transparent;
                    sBoxRound2_2.Background = Brushes.Transparent;
                    sBoxRound2_3.Background = Brushes.Transparent;
                    sBoxRound2_4.Background = Brushes.Transparent;


                    shiftRowRound2_1.Background = Brushes.Transparent;
                    shiftRowRound2_8.Background = Brushes.Transparent;
                    shiftRowRound2_11.Background = Brushes.Transparent;
                    shiftRowRound2_14.Background = Brushes.Transparent;
                    connectingLines(1, true);
                    break;
                case "txtBox2":
                    source.Background = Brushes.Transparent;
                    roundZero2.Background = Brushes.Transparent;
                    sBoxRound1_2.Background = Brushes.Transparent;
                    shiftRowRound1_14.Background = Brushes.Transparent;

                    mixColumns1_13.Background = Brushes.Transparent;
                    mixColumns1_14.Background = Brushes.Transparent;
                    mixColumns1_15.Background = Brushes.Transparent;
                    mixColumns1_16.Background = Brushes.Transparent;


                    addKey1_13.Background = Brushes.Transparent;
                    addKey1_14.Background = Brushes.Transparent;
                    addKey1_15.Background = Brushes.Transparent;
                    addKey1_16.Background = Brushes.Transparent;

                    sBoxRound2_13.Background = Brushes.Transparent;
                    sBoxRound2_14.Background = Brushes.Transparent;
                    sBoxRound2_15.Background = Brushes.Transparent;
                    sBoxRound2_16.Background = Brushes.Transparent;

                    shiftRowRound2_13.Background = Brushes.Transparent;
                    shiftRowRound2_10.Background = Brushes.Transparent;
                    shiftRowRound2_7.Background = Brushes.Transparent;
                    shiftRowRound2_4.Background = Brushes.Transparent;

                    connectingLines(2, true);
                    break;
                case "txtBox3":
                    source.Background = Brushes.Transparent;
                    roundZero3.Background = Brushes.Transparent;
                    sBoxRound1_3.Background = Brushes.Transparent;
                    shiftRowRound1_11.Background = Brushes.Transparent;

                    mixColumns1_9.Background = Brushes.Transparent;
                    mixColumns1_10.Background = Brushes.Transparent;
                    mixColumns1_11.Background = Brushes.Transparent;
                    mixColumns1_12.Background = Brushes.Transparent;

                    addKey1_9.Background = Brushes.Transparent;
                    addKey1_10.Background = Brushes.Transparent;
                    addKey1_11.Background = Brushes.Transparent;
                    addKey1_12.Background = Brushes.Transparent;

                    sBoxRound2_9.Background = Brushes.Transparent;
                    sBoxRound2_10.Background = Brushes.Transparent;
                    sBoxRound2_11.Background = Brushes.Transparent;
                    sBoxRound2_12.Background = Brushes.Transparent;


                    shiftRowRound2_9.Background = Brushes.Transparent;
                    shiftRowRound2_6.Background = Brushes.Transparent;
                    shiftRowRound2_3.Background = Brushes.Transparent;
                    shiftRowRound2_16.Background = Brushes.Transparent;
                    connectingLines(3, true);
                    break;
                case "txtBox4":
                    source.Background = Brushes.Transparent;
                    roundZero4.Background = Brushes.Transparent;
                    sBoxRound1_4.Background = Brushes.Transparent;
                    shiftRowRound1_8.Background = Brushes.Transparent;

                    mixColumns1_5.Background = Brushes.Transparent;
                    mixColumns1_6.Background = Brushes.Transparent;
                    mixColumns1_7.Background = Brushes.Transparent;
                    mixColumns1_8.Background = Brushes.Transparent;

                    addKey1_5.Background = Brushes.Transparent;
                    addKey1_6.Background = Brushes.Transparent;
                    addKey1_7.Background = Brushes.Transparent;
                    addKey1_8.Background = Brushes.Transparent;

                    sBoxRound2_5.Background = Brushes.Transparent;
                    sBoxRound2_6.Background = Brushes.Transparent;
                    sBoxRound2_7.Background = Brushes.Transparent;
                    sBoxRound2_8.Background = Brushes.Transparent;

                    shiftRowRound2_5.Background = Brushes.Transparent;
                    shiftRowRound2_2.Background = Brushes.Transparent;
                    shiftRowRound2_15.Background = Brushes.Transparent;
                    shiftRowRound2_12.Background = Brushes.Transparent;
                    connectingLines(4, true);
                    break;
                case "txtBox5":
                    source.Background = Brushes.Transparent;
                    roundZero5.Background = Brushes.Transparent;
                    sBoxRound1_5.Background = Brushes.Transparent;
                    shiftRowRound1_5.Background = Brushes.Transparent;

                    mixColumns1_5.Background = Brushes.Transparent;
                    mixColumns1_6.Background = Brushes.Transparent;
                    mixColumns1_7.Background = Brushes.Transparent;
                    mixColumns1_8.Background = Brushes.Transparent;

                    addKey1_5.Background = Brushes.Transparent;
                    addKey1_6.Background = Brushes.Transparent;
                    addKey1_7.Background = Brushes.Transparent;
                    addKey1_8.Background = Brushes.Transparent;

                    sBoxRound2_5.Background = Brushes.Transparent;
                    sBoxRound2_6.Background = Brushes.Transparent;
                    sBoxRound2_7.Background = Brushes.Transparent;
                    sBoxRound2_8.Background = Brushes.Transparent;

                    shiftRowRound2_5.Background = Brushes.Transparent;
                    shiftRowRound2_2.Background = Brushes.Transparent;
                    shiftRowRound2_15.Background = Brushes.Transparent;
                    shiftRowRound2_12.Background = Brushes.Transparent;
                    connectingLines(5, true);
                    break;
                case "txtBox6":
                    source.Background = Brushes.Transparent;
                    roundZero6.Background = Brushes.Transparent;
                    sBoxRound1_6.Background = Brushes.Transparent;
                    shiftRowRound1_2.Background = Brushes.Transparent;

                    mixColumns1_1.Background = Brushes.Transparent;
                    mixColumns1_2.Background = Brushes.Transparent;
                    mixColumns1_3.Background = Brushes.Transparent;
                    mixColumns1_4.Background = Brushes.Transparent;

                    addKey1_1.Background = Brushes.Transparent;
                    addKey1_2.Background = Brushes.Transparent;
                    addKey1_3.Background = Brushes.Transparent;
                    addKey1_4.Background = Brushes.Transparent;

                    sBoxRound2_1.Background = Brushes.Transparent;
                    sBoxRound2_2.Background = Brushes.Transparent;
                    sBoxRound2_3.Background = Brushes.Transparent;
                    sBoxRound2_4.Background = Brushes.Transparent;

                    shiftRowRound2_1.Background = Brushes.Transparent;
                    shiftRowRound2_8.Background = Brushes.Transparent;
                    shiftRowRound2_11.Background = Brushes.Transparent;
                    shiftRowRound2_14.Background = Brushes.Transparent;
                    connectingLines(6, true);
                    break;
                case "txtBox7":
                    source.Background = Brushes.Transparent;
                    roundZero7.Background = Brushes.Transparent;
                    sBoxRound1_7.Background = Brushes.Transparent;
                    shiftRowRound1_15.Background = Brushes.Transparent;

                    mixColumns1_13.Background = Brushes.Transparent;
                    mixColumns1_14.Background = Brushes.Transparent;
                    mixColumns1_15.Background = Brushes.Transparent;
                    mixColumns1_16.Background = Brushes.Transparent;

                    addKey1_13.Background = Brushes.Transparent;
                    addKey1_14.Background = Brushes.Transparent;
                    addKey1_15.Background = Brushes.Transparent;
                    addKey1_16.Background = Brushes.Transparent;

                    sBoxRound2_13.Background = Brushes.Transparent;
                    sBoxRound2_14.Background = Brushes.Transparent;
                    sBoxRound2_15.Background = Brushes.Transparent;
                    sBoxRound2_16.Background = Brushes.Transparent;

                    shiftRowRound2_13.Background = Brushes.Transparent;
                    shiftRowRound2_10.Background = Brushes.Transparent;
                    shiftRowRound2_7.Background = Brushes.Transparent;
                    shiftRowRound2_4.Background = Brushes.Transparent;
                    connectingLines(7, true);
                    break;
                case "txtBox8":
                    source.Background = Brushes.Transparent;
                    roundZero8.Background = Brushes.Transparent;
                    sBoxRound1_8.Background = Brushes.Transparent;
                    shiftRowRound1_12.Background = Brushes.Transparent;


                    mixColumns1_9.Background = Brushes.Transparent;
                    mixColumns1_10.Background = Brushes.Transparent;
                    mixColumns1_11.Background = Brushes.Transparent;
                    mixColumns1_12.Background = Brushes.Transparent;

                    addKey1_9.Background = Brushes.Transparent;
                    addKey1_10.Background = Brushes.Transparent;
                    addKey1_11.Background = Brushes.Transparent;
                    addKey1_12.Background = Brushes.Transparent;

                    sBoxRound2_9.Background = Brushes.Transparent;
                    sBoxRound2_10.Background = Brushes.Transparent;
                    sBoxRound2_11.Background = Brushes.Transparent;
                    sBoxRound2_12.Background = Brushes.Transparent;


                    shiftRowRound2_9.Background = Brushes.Transparent;
                    shiftRowRound2_6.Background = Brushes.Transparent;
                    shiftRowRound2_3.Background = Brushes.Transparent;
                    shiftRowRound2_16.Background = Brushes.Transparent;
                    connectingLines(8, true);
                    break;
                case "txtBox9":
                    source.Background = Brushes.Transparent;
                    roundZero9.Background = Brushes.Transparent;
                    sBoxRound1_9.Background = Brushes.Transparent;
                    shiftRowRound1_9.Background = Brushes.Transparent;


                    mixColumns1_9.Background = Brushes.Transparent;
                    mixColumns1_10.Background = Brushes.Transparent;
                    mixColumns1_11.Background = Brushes.Transparent;
                    mixColumns1_12.Background = Brushes.Transparent;

                    addKey1_9.Background = Brushes.Transparent;
                    addKey1_10.Background = Brushes.Transparent;
                    addKey1_11.Background = Brushes.Transparent;
                    addKey1_12.Background = Brushes.Transparent;

                    sBoxRound2_9.Background = Brushes.Transparent;
                    sBoxRound2_10.Background = Brushes.Transparent;
                    sBoxRound2_11.Background = Brushes.Transparent;
                    sBoxRound2_12.Background = Brushes.Transparent;


                    shiftRowRound2_9.Background = Brushes.Transparent;
                    shiftRowRound2_6.Background = Brushes.Transparent;
                    shiftRowRound2_3.Background = Brushes.Transparent;
                    shiftRowRound2_16.Background = Brushes.Transparent;
                    connectingLines(9, true);
                    break;
                case "txtBox10":
                    source.Background = Brushes.Transparent;
                    roundZero10.Background = Brushes.Transparent;
                    sBoxRound1_10.Background = Brushes.Transparent;
                    shiftRowRound1_6.Background = Brushes.Transparent;

                    mixColumns1_5.Background = Brushes.Transparent;
                    mixColumns1_6.Background = Brushes.Transparent;
                    mixColumns1_7.Background = Brushes.Transparent;
                    mixColumns1_8.Background = Brushes.Transparent;

                    addKey1_5.Background = Brushes.Transparent;
                    addKey1_6.Background = Brushes.Transparent;
                    addKey1_7.Background = Brushes.Transparent;
                    addKey1_8.Background = Brushes.Transparent;

                    sBoxRound2_5.Background = Brushes.Transparent;
                    sBoxRound2_6.Background = Brushes.Transparent;
                    sBoxRound2_7.Background = Brushes.Transparent;
                    sBoxRound2_8.Background = Brushes.Transparent;

                    shiftRowRound2_5.Background = Brushes.Transparent;
                    shiftRowRound2_2.Background = Brushes.Transparent;
                    shiftRowRound2_15.Background = Brushes.Transparent;
                    shiftRowRound2_12.Background = Brushes.Transparent;

                    connectingLines(10, true);
                    break;
                case "txtBox11":
                    source.Background = Brushes.Transparent;
                    roundZero11.Background = Brushes.Transparent;
                    sBoxRound1_11.Background = Brushes.Transparent;
                    shiftRowRound1_3.Background = Brushes.Transparent;

                    mixColumns1_1.Background = Brushes.Transparent;
                    mixColumns1_2.Background = Brushes.Transparent;
                    mixColumns1_3.Background = Brushes.Transparent;
                    mixColumns1_4.Background = Brushes.Transparent;

                    addKey1_1.Background = Brushes.Transparent;
                    addKey1_2.Background = Brushes.Transparent;
                    addKey1_3.Background = Brushes.Transparent;
                    addKey1_4.Background = Brushes.Transparent;

                    sBoxRound2_1.Background = Brushes.Transparent;
                    sBoxRound2_2.Background = Brushes.Transparent;
                    sBoxRound2_3.Background = Brushes.Transparent;
                    sBoxRound2_4.Background = Brushes.Transparent;

                    shiftRowRound2_1.Background = Brushes.Transparent;
                    shiftRowRound2_8.Background = Brushes.Transparent;
                    shiftRowRound2_11.Background = Brushes.Transparent;
                    shiftRowRound2_14.Background = Brushes.Transparent;
                    connectingLines(11, true);
                    break;
                case "txtBox12":
                    source.Background = Brushes.Transparent;
                    roundZero12.Background = Brushes.Transparent;
                    sBoxRound1_12.Background = Brushes.Transparent;
                    shiftRowRound1_16.Background = Brushes.Transparent;

                    mixColumns1_13.Background = Brushes.Transparent;
                    mixColumns1_14.Background = Brushes.Transparent;
                    mixColumns1_15.Background = Brushes.Transparent;
                    mixColumns1_16.Background = Brushes.Transparent;

                    addKey1_13.Background = Brushes.Transparent;
                    addKey1_14.Background = Brushes.Transparent;
                    addKey1_15.Background = Brushes.Transparent;
                    addKey1_16.Background = Brushes.Transparent;

                    sBoxRound2_13.Background = Brushes.Transparent;
                    sBoxRound2_14.Background = Brushes.Transparent;
                    sBoxRound2_15.Background = Brushes.Transparent;
                    sBoxRound2_16.Background = Brushes.Transparent;

                    shiftRowRound2_13.Background = Brushes.Transparent;
                    shiftRowRound2_10.Background = Brushes.Transparent;
                    shiftRowRound2_7.Background = Brushes.Transparent;
                    shiftRowRound2_4.Background = Brushes.Transparent;
                    connectingLines(12, true);
                    break;
                case "txtBox13":
                    source.Background = Brushes.Transparent;
                    roundZero13.Background = Brushes.Transparent;
                    sBoxRound1_13.Background = Brushes.Transparent;
                    shiftRowRound1_13.Background = Brushes.Transparent;

                    mixColumns1_13.Background = Brushes.Transparent;
                    mixColumns1_14.Background = Brushes.Transparent;
                    mixColumns1_15.Background = Brushes.Transparent;
                    mixColumns1_16.Background = Brushes.Transparent;

                    addKey1_13.Background = Brushes.Transparent;
                    addKey1_14.Background = Brushes.Transparent;
                    addKey1_15.Background = Brushes.Transparent;
                    addKey1_16.Background = Brushes.Transparent;

                    sBoxRound2_13.Background = Brushes.Transparent;
                    sBoxRound2_14.Background = Brushes.Transparent;
                    sBoxRound2_15.Background = Brushes.Transparent;
                    sBoxRound2_16.Background = Brushes.Transparent;

                    shiftRowRound2_13.Background = Brushes.Transparent;
                    shiftRowRound2_10.Background = Brushes.Transparent;
                    shiftRowRound2_7.Background = Brushes.Transparent;
                    shiftRowRound2_4.Background = Brushes.Transparent;
                    connectingLines(13, true);
                    break;
                case "txtBox14":
                    source.Background = Brushes.Transparent;
                    roundZero14.Background = Brushes.Transparent;
                    sBoxRound1_14.Background = Brushes.Transparent;
                    shiftRowRound1_10.Background = Brushes.Transparent;


                    mixColumns1_9.Background = Brushes.Transparent;
                    mixColumns1_10.Background = Brushes.Transparent;
                    mixColumns1_11.Background = Brushes.Transparent;
                    mixColumns1_12.Background = Brushes.Transparent;

                    addKey1_9.Background = Brushes.Transparent;
                    addKey1_10.Background = Brushes.Transparent;
                    addKey1_11.Background = Brushes.Transparent;
                    addKey1_12.Background = Brushes.Transparent;

                    sBoxRound2_9.Background = Brushes.Transparent;
                    sBoxRound2_10.Background = Brushes.Transparent;
                    sBoxRound2_11.Background = Brushes.Transparent;
                    sBoxRound2_12.Background = Brushes.Transparent;


                    shiftRowRound2_9.Background = Brushes.Transparent;
                    shiftRowRound2_6.Background = Brushes.Transparent;
                    shiftRowRound2_3.Background = Brushes.Transparent;
                    shiftRowRound2_16.Background = Brushes.Transparent;
                    connectingLines(14, true);
                    break;
                case "txtBox15":
                    source.Background = Brushes.Transparent;
                    roundZero15.Background = Brushes.Transparent;
                    sBoxRound1_15.Background = Brushes.Transparent;
                    shiftRowRound1_7.Background = Brushes.Transparent;

                    mixColumns1_5.Background = Brushes.Transparent;
                    mixColumns1_6.Background = Brushes.Transparent;
                    mixColumns1_7.Background = Brushes.Transparent;
                    mixColumns1_8.Background = Brushes.Transparent;

                    addKey1_5.Background = Brushes.Transparent;
                    addKey1_6.Background = Brushes.Transparent;
                    addKey1_7.Background = Brushes.Transparent;
                    addKey1_8.Background = Brushes.Transparent;

                    sBoxRound2_5.Background = Brushes.Transparent;
                    sBoxRound2_6.Background = Brushes.Transparent;
                    sBoxRound2_7.Background = Brushes.Transparent;
                    sBoxRound2_8.Background = Brushes.Transparent;

                    shiftRowRound2_5.Background = Brushes.Transparent;
                    shiftRowRound2_2.Background = Brushes.Transparent;
                    shiftRowRound2_15.Background = Brushes.Transparent;
                    shiftRowRound2_12.Background = Brushes.Transparent;
                    connectingLines(15, true);
                    break;
                case "txtBox16":
                    source.Background = Brushes.Transparent;
                    roundZero16.Background = Brushes.Transparent;
                    sBoxRound1_16.Background = Brushes.Transparent;
                    shiftRowRound1_4.Background = Brushes.Transparent;

                    mixColumns1_1.Background = Brushes.Transparent;
                    mixColumns1_2.Background = Brushes.Transparent;
                    mixColumns1_3.Background = Brushes.Transparent;
                    mixColumns1_4.Background = Brushes.Transparent;

                    addKey1_1.Background = Brushes.Transparent;
                    addKey1_2.Background = Brushes.Transparent;
                    addKey1_3.Background = Brushes.Transparent;
                    addKey1_4.Background = Brushes.Transparent;

                    sBoxRound2_1.Background = Brushes.Transparent;
                    sBoxRound2_2.Background = Brushes.Transparent;
                    sBoxRound2_3.Background = Brushes.Transparent;
                    sBoxRound2_4.Background = Brushes.Transparent;

                    shiftRowRound2_1.Background = Brushes.Transparent;
                    shiftRowRound2_8.Background = Brushes.Transparent;
                    shiftRowRound2_11.Background = Brushes.Transparent;
                    shiftRowRound2_14.Background = Brushes.Transparent;
                    connectingLines(16, true);
                    break;

                default:
                    break;
            }
        }
  

      
        public void comparisonPane()
        {

            StartCanvas.Visibility = Visibility.Hidden;

            switch (mode)
            {
                //AES
                case 0:
                    OrigInitialStateGrid.Visibility = Visibility.Visible;
                    aesCheckBox.Visibility = Visibility.Visible;
                    inputInBits.Visibility = Visibility.Visible;
                    explanationTxt.Visibility = Visibility.Visible;

                
                    if (keysize == 1)
                    {
                        originalKeyGrid192.Visibility = Visibility.Visible;
                        BitKeyGrid192.Visibility = Visibility.Visible;
                    }
                    else if (keysize == 2)
                    {
                        originalKeyGrid256.Visibility = Visibility.Visible;
                        BitKeyGrid256.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        originalKeyGrid.Visibility = Visibility.Visible;
                        BitKeyGrid.Visibility = Visibility.Visible;
                    }


                    initStateTitle.Visibility = Visibility.Visible;

                    changeTitle();

                    int a = 0;
                    int b = 128;
                    while (a < 64 && b < 192)
                    {

                        ((TextBlock)this.FindName("txt" + b)).Foreground = Brushes.Black;
                        a++;
                        b++;
                    }
                  
                    break;
                case 1:

                    inputGridDES.Visibility = Visibility.Visible;
                    
                    break;
                case 2:
                case 3:
                case 4:
                    
                    othersGrid.Visibility = Visibility.Visible;
                    changeTitle();

                    break;
                default:
                    break;

            }

        }

  
        public void removeElements()
        {
            TB1.Text = string.Empty;
            TB2.Text = string.Empty;
            TB3.Text = string.Empty;

            modifiedMsg.Text = string.Empty;
            originalMsg.Text = string.Empty;
            bitsData.Visibility = Visibility.Hidden;
            flippedBitsPiece.Visibility = Visibility.Hidden;
            unflippedBitsPiece.Visibility = Visibility.Hidden;
            avalancheEffectAESText.Visibility = Visibility.Hidden;
            showByteDependencyText.Visibility = Visibility.Hidden;
            divideLine.Visibility = Visibility.Hidden;
            bitRepresentationGrid.Visibility = Visibility.Hidden;
            OrigInitialStateGrid.Visibility = Visibility.Hidden;
            afterInitialRoundGrid.Visibility = Visibility.Hidden;
            afterRoundsGrid.Visibility = Visibility.Hidden;
            //MessagesStackPanel.Visibility = Visibility.Hidden;
            Cb1.Visibility = Visibility.Hidden;
            Cb2.Visibility = Visibility.Hidden;
            afterInitRoundButton.Visibility = Visibility.Hidden;
            
          
            changePropagationGrid.Visibility = Visibility.Hidden;
            informationGrid.Visibility = Visibility.Hidden;
            comparisonTxtBlock.Visibility = Visibility.Hidden;
            afterRound11Button.Visibility = Visibility.Collapsed;
            afterRound12Button.Visibility = Visibility.Collapsed;
            afterRound13Button.Visibility = Visibility.Collapsed;
            afterRound14Button.Visibility = Visibility.Collapsed;
            afterRound15Button.Visibility = Visibility.Collapsed;
            afterRound16Button.Visibility = Visibility.Collapsed;
            OrigInitialStateGrid.Visibility = Visibility.Hidden;

            initStateTitle.Visibility = Visibility.Hidden;
            modifiedInitialStateGrid.Visibility = Visibility.Hidden;
            buttonsPanel.Visibility = Visibility.Hidden;
            radioButtons.Visibility = Visibility.Hidden;
            buttonsTitle.Visibility = Visibility.Hidden;
            curvedLinesCanvas.Visibility = Visibility.Hidden;
            afterRoundsTitle.Visibility = Visibility.Hidden;
            afterRoundsSubtitle.Visibility = Visibility.Hidden;
            inputInBits.Visibility = Visibility.Hidden;
            instructionsTxtBlock2.Visibility = Visibility.Hidden;
            doneButton.Visibility = Visibility.Hidden;
            clearButton.Visibility = Visibility.Hidden;
            originalKeyGrid.Visibility = Visibility.Collapsed;
            originalKeyGrid192.Visibility = Visibility.Collapsed;
            originalKeyGrid256.Visibility = Visibility.Collapsed;
            BitKeyGrid.Visibility = Visibility.Collapsed;
            BitKeyGrid192.Visibility = Visibility.Collapsed;
            BitKeyGrid256.Visibility = Visibility.Collapsed;
            modifiedKeyGrid.Visibility = Visibility.Collapsed;
            modifiedKeyGrid192.Visibility = Visibility.Collapsed;
            modifiedKeyGrid256.Visibility = Visibility.Collapsed;
            radioDecimal.IsChecked = false;
            radioHexa.IsChecked = false;
            singleBitChange = false;
            aesCheckBox.Visibility = Visibility.Hidden;
            explanationTxt.Visibility = Visibility.Hidden;
            aesCheckBox.IsChecked = false;
            // buttonsSV.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            //buttonsSV.Width = 535.0;
            //buttonsPanel.Width = 530.0;
            // buttonsSV.ScrollToHorizontalOffset(0.0);
            bitRepresentationSV.ScrollToHorizontalOffset(0.0);
            List<TextBlock> tmp = createTxtBlockList(6);

            foreach (TextBlock txtB in tmp)
                txtB.Foreground = Brushes.Black;


            int k = 33;
            int l = 49;
            int i = 1;


            while (k <= 48 && l <= 64)
            {

                ((TextBlock)this.FindName("initStateTxtBlock" + k)).Text = string.Empty;

                ((TextBlock)this.FindName("initStateTxtBlock" + l)).Text = string.Empty;
                i++;
                k++;
                l++;
            }

            int j = 1;

            while (j <= 24)
            {
                ((TextBlock)this.FindName("modKey192_" + j)).Text = string.Empty;
                j++;

            }

            int m = 1;

            while (m <= 32)
            {
                ((TextBlock)this.FindName("modKey256_" + m)).Text = string.Empty;
                m++;

            }

            UGrid.Columns = 128;

            //DES


            inputGridDES.Visibility = Visibility.Hidden;
            doneButtonDES.Visibility = Visibility.Hidden;
            clearButtonDES.Visibility = Visibility.Hidden;
            desCheckBox.IsChecked = false;
            modificationGridDES.Visibility = Visibility.Hidden;
            bitGridDES.Visibility = Visibility.Hidden;

            othersGrid.Visibility = Visibility.Hidden;
            readjustStats();
            mode = 0;

            StartCanvas.Visibility = Visibility.Visible;


            List<TextBlock> tmpDES = createTxtBlockList(7);

            foreach (TextBlock txtB in tmpDES)
                txtB.Foreground = Brushes.Black;

        }

        public void adjustStats()
        {

            Grid.SetColumn(bitsData, 0);
            Grid.SetColumnSpan(bitsData, 2);
            bitsData.HorizontalAlignment = HorizontalAlignment.Center;
            bitsData.VerticalAlignment = VerticalAlignment.Center;

            Thickness margin = bitsData.Margin;
            margin.Top = 40;
            bitsData.Margin = margin;

            stats3Bullet.Visibility = Visibility.Collapsed;

            flippedBitsPiece.VerticalAlignment = VerticalAlignment.Bottom;
            unflippedBitsPiece.VerticalAlignment = VerticalAlignment.Bottom;

            flippedBitsPiece.Margin = new Thickness(80, -14, 80, 0);
            unflippedBitsPiece.Margin = new Thickness(80, -14, 80, 0);

            Cb1.VerticalAlignment = VerticalAlignment.Bottom;
            Cb2.VerticalAlignment = VerticalAlignment.Bottom;
            Cb1.Margin = new Thickness(5, 0, 10, 100);
            Cb2.Margin = new Thickness(5, 0, 10, 80);
        }

        public void readjustStats()
        {


            Grid.SetColumn(bitsData, 1);
            Grid.SetColumnSpan(bitsData, 1);
            bitsData.HorizontalAlignment = HorizontalAlignment.Stretch;
            bitsData.VerticalAlignment = VerticalAlignment.Stretch;

            Thickness margin = bitsData.Margin;
            margin.Top = 25;
            bitsData.Margin = margin;

            stats3Bullet.Visibility = Visibility.Visible;

            flippedBitsPiece.VerticalAlignment = VerticalAlignment.Top;
            unflippedBitsPiece.VerticalAlignment = VerticalAlignment.Top;

            flippedBitsPiece.Margin = new Thickness(80, 15, 80, 0);
            unflippedBitsPiece.Margin = new Thickness(80, 15, 80, 0);

            Cb1.VerticalAlignment = VerticalAlignment.Center;
            Cb2.VerticalAlignment = VerticalAlignment.Center;
            Cb1.Margin = new Thickness(5, 0, 10, 80);
            Cb2.Margin = new Thickness(5, 0, 10, 40);
        }

        public void comparison()
        {
            emptyInformation();

            adjustStats();
            bitsData.Visibility = Visibility.Visible;
            Cb1.Visibility = Visibility.Visible;
            Cb2.Visibility = Visibility.Visible;
            flippedBitsPiece.Visibility = Visibility.Visible;
            unflippedBitsPiece.Visibility = Visibility.Visible;
            var strings = binaryStrings(unchangedCipher, changedCipher);
            int bitsFlipped = nrOfBitsFlipped(unchangedCipher, changedCipher);
            int lengthIdentSequence;
            avalanche = calcAvalancheEffect(bitsFlipped, strings);
            double angle_1 = flippedBitsPiece.calculateAngle(bitsFlipped, strings);
            double angle_2 = unflippedBitsPiece.calculateAngle(strings.Item1.Length - bitsFlipped, strings);
            showBitSequence(strings);
            lengthIdentSequence = longestIdenticalSequence(differentBits);
            showStatistics(bitsFlipped, lengthIdentSequence, strings);
            setColors();
            setAngles(angle_1, angle_2);
            setToolTips();
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer sync = (sender == bar3) ? bar2 : bar3;
            ScrollViewer sync2 = (sender == bar3) ? bar1 : bar3;

            sync.ScrollToVerticalOffset(e.VerticalOffset);
            sync.ScrollToHorizontalOffset(e.HorizontalOffset);

            sync2.ScrollToVerticalOffset(e.VerticalOffset);
            sync2.ScrollToHorizontalOffset(e.HorizontalOffset);
        }



        private void TextBlock_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TextBlock txtBlock = sender as TextBlock;


            if ((bool)desCheckBox.IsChecked || (bool)aesCheckBox.IsChecked)
            {

                if (txtBlock.Text == "0")
                {
                    txtBlock.Text = "1";

                    if (txtBlock.Foreground != Brushes.Red)
                        txtBlock.Foreground = Brushes.Red;
                    else
                        txtBlock.Foreground = Brushes.Black;
                }
                else
                {
                    txtBlock.Text = "0";

                    if (txtBlock.Foreground != Brushes.Red)
                        txtBlock.Foreground = Brushes.Red;
                    else
                        txtBlock.Foreground = Brushes.Black;
                }

            }

        }



        private void onVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (inputInBits.IsVisible)
            {
                
                arrow1.Visibility = Visibility.Visible;
                arrow2.Visibility = Visibility.Visible;
            }
            else
            {
                
                arrow1.Visibility = Visibility.Hidden;
                arrow2.Visibility = Visibility.Hidden;
            }




        }

        private void onTitleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (initStateTitle.IsVisible || modificationGridDES.IsVisible)
                inputDataButton.IsEnabled = false;
            else
                inputDataButton.IsEnabled = true;

            if (!modificationGridDES.IsVisible && mode == 1)
                radioBinaryDes.IsChecked = true;
        }
    

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            singleBitChange = true;

            if (keysize == 0 || keysize == 1 || keysize == 2)
            {
                doneButton.Visibility = Visibility.Visible;
                clearButton.Visibility = Visibility.Visible;
                instructionsTxtBlock2.Visibility = Visibility.Visible;
            }
            else
            {
                doneButtonDES.Visibility = Visibility.Visible;
                clearButtonDES.Visibility = Visibility.Visible;
            }

        }

        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            singleBitChange = false;

            if (keysize == 0 || keysize == 1 || keysize == 2)
            {
                doneButton.Visibility = Visibility.Hidden;
                clearButton.Visibility = Visibility.Hidden;
                instructionsTxtBlock2.Visibility = Visibility.Hidden;
            }
            else
            {
                doneButtonDES.Visibility = Visibility.Hidden;
                clearButtonDES.Visibility = Visibility.Hidden;
            }
        }

        
    }
}
#endregion
