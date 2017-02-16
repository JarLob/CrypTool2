using System;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Controls;
using System.Collections;
using System.Text.RegularExpressions;

namespace Cryptool.Plugins.A5
{
    [Author("Kristina Hita", "khita@mail.uni-mannheim.de", "Universität Mannheim", "https://www.uni-mannheim.de/1/english/university/profile/")]
    [PluginInfo("A5.Properties.Resources", "PluginCaption", "PluginTooltip", "A5/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    /**
     * This Class generates the key stream for A5/1. The Stream is generated
     * based on the given key and frame number given to LFSR  
     *
     * @author Kristian Ott
     */
    public class A5 : ICrypComponent
    {

        #region Algorithm variables

        public bool dbMode; //Debug mode

        private int nRegisters = 3; //number of registers
        private int[] mRegLens = new int[] { 19, 22, 23 }; //max register lengths
        private int[] rIndexes = new int[] { 8, 10, 10 }; //register indexes (clocking bits)
        private string[] pArray = new string[] { "x^18+x^17+x^16+x^13+1",
                                                "x^21+x^20+1",
                                                "x^22+x^21+x^20+x^7+1" }; //polynomials
        private int[][] registers; //registers (we are using this to create a table with the registers space for all 3 registers (1st row --> register 1 ; 2nd row --> register 2 etc)
        private int[] iv;
        private int[] key;
        private int[] message;
        private int[][] uExponents;

        private String messageInput;
        private String output;
        private String keyString = null;
        private String initialVector = null;
        private int BUFFERSIZE = 64;
        private bool stop = false;
        private A5Settings settings;

        #endregion




        public A5()
        {
            this.settings = new A5Settings();
        }


        [PropertyInfo(Direction.InputData, "InputKeyString", "InputKeyTooltip", true)]
        public String KeyString
        {
            get { return this.keyString; }
            set
            {
                this.keyString = value;
                OnPropertyChanged("KeyString");
            }
        }

        [PropertyInfo(Direction.InputData, "FrameNumberString", "FrameNumberTooltip", true)]
        public String InitialVector
        {
            get { return this.initialVector; }
            set
            {
                this.initialVector = value;
                OnPropertyChanged("FrameNumber");
            }
        }

        [PropertyInfo(Direction.InputData, "InputStreamCaption", "InputStreamTooltip", true)]
        public String MessageInput
        {
            get { return this.messageInput; }
            set
            {
                this.messageInput = value;
                OnPropertyChanged("MessageInput");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputTooltip", true)]
        public String Output
        {
            get
            {
                return output;
            }
            set
            {
                this.output = value;
                OnPropertyChanged("Output");
            }
        }

        #region A5
        /**
	 * 
	 * @param fileToEncrypt
	 * @param encryptKey
	 * @param encryptFrameNumber
	 * @return The filename of the encrypted file.
	 */
        public int GetMaxRegLensTotal()
        {
            int total = 0;

            foreach (int len in mRegLens)
                total += len;

            return total;
        }

        //A function that shows the result after two values are being XORed with each other
        private int XorValues(int val1, int val2)
        {
            int res = 0;

            if (val1 != val2)
                res = 1;

            return res;
        }

        // Function to XOR registers' values, in order to get later the output values
        private int XorRegValues(int[] vToXor)
        {
            int final = 0;

            for (int i = 0; i < vToXor.Length; i++)
                final = XorValues(final, vToXor[i]);

            return final;
        }

        // We created a table for registers. Here we remove any extra space acquired by register by looking at its index
        private int[][] RemoveIntArrElement(int[][] oArray, int index)
        {
            int[][] nArray = new int[oArray.Length - 1][];

            for (int i = 0; i < oArray.Length; i++)
            {
                if (i != index)
                {
                    nArray[i - 1] = new int[oArray[i].Length];
                    for (int x = 0; x < oArray[i].Length; x++)
                    {
                        nArray[i - 1][x] = oArray[i][x];
                    }
                }
            }

            return nArray;
        }

        // Polymomials are to show the register function/equations
        // We are performing a global search over the whole input string and return all the matches with their corresponding capture data,
        // we are using Regex to get a  MatchCollection which can be iterated over and processed 
        private int[][] ExtractPolyExponents(string[] polynomialsArray)
        {
            int[][] exponents = new int[polynomialsArray.Length][];

            for (int i = 0; i < polynomialsArray.Length; i++)
            {
                Regex expression = new Regex(@"x\^(\d+)");
                MatchCollection polyMatches = expression.Matches(polynomialsArray[i]);

                exponents[i] = new int[polyMatches.Count];

                for (int x = 0; x < polyMatches.Count; x++)
                {
                    //Console.WriteLine(polyMatches[x].Groups[1].ToString());
                    exponents[i][x] = int.Parse(polyMatches[x].Groups[1].ToString());
                }
            }

            return exponents;
        }

        // the largest exponent in the register function/equation is checked in the polynomial selection class, it should not exceed the maximum register length
        private int FindLargestInt(int[] intArray)
        {
            int largest = 0;

            foreach (int num in intArray)
            {
                if (num > largest)
                    largest = num;
            }

            return largest;
        }

        // We are selecting polynomial function for each register
        private int[][] PolySelection()
        {
            int[][] exponents = ExtractPolyExponents(pArray);
            int[][] usingPolynomials = new int[nRegisters][];

            int counter = 0;
            int j = 0; //since i variable is reset
            for (int i = 0; i < exponents.Length; i++)
            {
                if (counter == nRegisters)
                    break;



                int largest = FindLargestInt(exponents[i]);

                if (largest < mRegLens[j])
                {
                    usingPolynomials[counter] = new int[exponents[i].Length];

                    for (int x = 0; x < exponents[i].Length; x++)
                        usingPolynomials[counter][x] = exponents[i][x];

                    exponents = RemoveIntArrElement(exponents, i);

                    i = -1; //reset loop
                    counter++;
                }

                j++;
            }

            return usingPolynomials;
        }

        // The loop has been created to store the index values of the 3 registers in the array. We get index from length of each register
        private int[] GetIndexValues()
        {
            int[] indexValues = new int[registers.Length];

            for (int i = 0; i < registers.Length; i++)
            {
                indexValues[i] = registers[i][rIndexes[i]];
            }

            return indexValues;
        }

        //shows the majority bit funtion
        // the index values of each register indicate the majority bit
        // eg. if the index values of the registers are (1,0,0) this means that the majority bit is 0

        private int[] FindFrequency(int[] indexValues)
        {
            int[] tally = new int[2]; //size of 2 since its just binary

            foreach (int val in indexValues)
            {
                if (val == 0)
                    tally[0]++;
                else if (val == 1)
                    tally[1]++;
            }

            return tally;
        }

        public void CreateRegisters()
        {

            registers = new int[nRegisters][];
            for (int i = 0; i < nRegisters; i++)
            {
                int[] newReg = new int[mRegLens[i]];
                for (int k = 0; k < mRegLens[i]; k++)
                    newReg[k] = 0;
                registers[i] = newReg;
            }

            uExponents = PolySelection();
            MixKey();
            MixIV();

            for (int j = 0; j < 100; j++)
            {
                int[] regTS = RegistersToShift();
                int[] feedbackvalues = GetFeedbackValues(regTS);
                RegisterShiftWithVal(regTS, feedbackvalues);
            }

        }

        // This function returns keystream values after the registers' values are being XORed 
        public int GetOutValue()
        {
            int[] vToXor = new int[registers.Length];
            int outValue = 0;

            for (int i = 0; i < registers.Length; i++)
                vToXor[i] = registers[i][0];

            outValue = XorRegValues(vToXor);

            int[] regTS = RegistersToShift();
            int[] feedbackset = GetFeedbackValues(regTS);
            RegisterShiftWithVal(regTS, feedbackset);

            return outValue;
        }

        // This is for filling registers with data. We would know the length of register from getIndex so the that the function will know when to stop filling
        public int[] RegistersToShift()
        {
            int[] indexValues = GetIndexValues();
            int[] tally = FindFrequency(indexValues);

            int highest = 0;
            int movVal = 0;

            if (dbMode)
            {
                Console.WriteLine("[indexValues]:");
                foreach (int v in indexValues)
                    Console.Write(v.ToString() + " ");
                Console.WriteLine("\n[/indexValues]:");

                Console.WriteLine("[tally]:");
                foreach (int v in tally)
                    Console.Write(v.ToString() + " ");
                Console.WriteLine("\n[/tally]:");
            }

            foreach (int count in tally)
            {
                if (count > highest)
                    highest = count;
            }

            for (int i = 0; i < tally.Length; i++)
            {
                if (tally[i] == highest)
                    movVal = i;
            }

            ArrayList regTS = new ArrayList();

            for (int i = 0; i < indexValues.Length; i++)
            {
                if (indexValues[i] == movVal)
                    regTS.Add(i);
            }

            return (int[])regTS.ToArray(typeof(int));
        }

        private void MixKey()
        {
            int[] regTS = new int[3] { 0, 1, 2 };

            for (int i = 0; i < key.Length; i++)
            {
                int[] feedbackset = GetFeedbackValues(regTS);
                for (int j = 0; j < feedbackset.Length; j++)
                    feedbackset[j] = XorValues(feedbackset[j], key[i]);

                RegisterShiftWithVal(regTS, feedbackset);

            }
        }

        private void MixIV()
        {
            int[] regTS = new int[3] { 0, 1, 2 };

            for (int i = 0; i < iv.Length; i++)
            {
                int[] feedbackset = GetFeedbackValues(regTS);
                for (int j = 0; j < feedbackset.Length; j++)
                    feedbackset[j] = XorValues(feedbackset[j], iv[i]);

                RegisterShiftWithVal(regTS, feedbackset);
            }

        }

        // The feedback in registers are calculated from the bits that are taken out such as for example 13th, 16th, 17th,18th bits in first register
        private int[] GetFeedbackValues(int[] regTS)
        {
            int[] regTSFBV = new int[regTS.Length]; //Reg To Shift Feed Back Values (regTSFBV)

            for (int i = 0; i < regTS.Length; i++)
            {
                int[] feedbackSet = new int[uExponents[regTS[i]].Length];

                for (int x = 0; x < uExponents[regTS[i]].Length; x++)
                {
                    feedbackSet[x] = registers[regTS[i]][uExponents[regTS[i]][x]];
                }

                regTSFBV[i] = XorRegValues(feedbackSet);
            }

            return regTSFBV;
        }

        public void RegisterShiftWithVal(int[] regTS, int[] val)
        {
            for (int i = 0; i < regTS.Length; i++)
            {
                int[] regShifting = registers[regTS[i]]; //Alias the register to shift

                //Creates new register with appropriate max reg length

                int[] nRegister = new int[regShifting.Length]; //Could also use mRegLens[regTS[i]].Length

                //Fill values to length -1

                for (int x = regShifting.Length - 1; x > 0; x--)
                    nRegister[x] = regShifting[x - 1]; //+1 Grabbing everything after position zero



                //Now put feedback values on the end (former RegisterPush method)

                nRegister[0] = val[i];

                registers[regTS[i]] = nRegister; //assign to register (update)
            }
        }

        public int[] encrypt(int[] plaintext, int[] encryptKey, int[] initialvector)
        {

            CreateRegisters();
            int[] encryptedText = new int[228];
            for (int i = 0; i < 228; i++)
            {
                encryptedText[i] = (GetOutValue() + message[i]) % 2;
            }
            return encryptedText;
            //		return outFile;
        }

        /**
         * 
         * @param fileToDecrypt
         * @param decryptKey
         * @param decryptFrameNumber
         * @return The filename of the decrypted file.
         */
        public int[] decrypt(int[] ciphertext, int[] decryptKey, int[] initialvector)
        {
            CreateRegisters();
            int[] decryptedText = new int[228];
            for (int i = 0; i < 228; i++)
            {
                decryptedText[i] = (GetOutValue() + message[i]) % 2;
            }
            return decryptedText;
        }

        /**
         * Adds zeros to the MSB of the given string. If the string is
         * too large, it will be truncated.
         * @param inString
         * @param bytes
         * @return
         */
        private String padZeros(String inString, int bytes)
        {
            char[] initS = new char[bytes];
            char[] inArray = inString.ToCharArray();

            if (inString.Length > bytes)
            {
                int start = inString.Length - bytes;
                Array.Copy(inArray, start, initS, 0, initS.Length);
                //			System.arraycopy(inArray, start, initS, 0, initS.Length);
            }
            else
            {
                int diff = bytes - inString.Length;

                for (int i = 0; i < bytes; i++) initS[i] = '0';
                Array.Copy(inArray, 0, initS, diff, inArray.Length);
                //			System.arraycopy(inArray, 0, initS, diff, inArray.Length);
            }
            return new String(initS);
        }

        /**
         * Entry point to the system.
         * 3 Input arguments:
         * args[0]: /path/to/file.ext
         * args[1]: key
         * args[2]: frame number
         * 
         * @param args
         * @throws IOException
         */
        public void main(String[] args)
        {

            //            String[] abc = {"e:/testImage.jpg","4E2F4D9C1EB88B3A","3AB3CB"};
            //String[] abc = { "3AB3CB4E2F4D9C4E2F4E2F4D9C1EB88B3A4E2F4D9C1EBD9C1EB88B3A1EB88B3A", "4E2F4D9C1EB88B3A", "3AB3CB" };
            //validateInput(abc);

            //		String encryptedFile = encrypt(inputFile, key, frameNumber);
            //                DataFileHandler dfh = new DataFileHandler(encryptedFile);
            //String aa = "3AB3CB4E2F4D9C4E2F4E2F4D9C1EB88B3A4D9C1EB88B3A1EB88B3A";

            //byte[] ttt = Encoding.ASCII.GetBytes(inputFile);
            //byte[] encryptedd = encrypt(ttt, keyString, frameNumber);
            ////byte[] a = aa.getBytes();
            //byte[] decc = decrypt(encryptedd, keyString, frameNumber);


            //for (int i = 0; i < encryptedd.Length; i++)
            //    Console.WriteLine(encryptedd[i]);
            //		String decryptedFile = decrypt(encryptedFile, key, frameNumber);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
            //if (PropertyChanged != null)
            //{
            //  PropertyChanged(this, new PropertyChangedEventArgs(name));
            //}
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (A5Settings)value; }
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation
        {
            get { return null; }
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            stop = false;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);
            try
            {
                switch (settings.Action)
                {
                    case A5Settings.A5Mode.Encrypt:
                        {

                            if (!String.IsNullOrEmpty(messageInput) && !String.IsNullOrEmpty(keyString) && !String.IsNullOrEmpty(initialVector))
                            {
                                String outval = "";
                                key = new int[64];
                                iv = new int[22];
                                message = new int[228];

                                for (int i = 0; i < 64; i++)
                                    key[i] = keyString[i] == '0' ? 0 : 1;
                                for (int j = 0; j < 22; j++)
                                    iv[j] = initialVector[j] == '0' ? 0 : 1;
                                for (int k = 0; k < 228; k++)
                                    message[k] = messageInput[k] == '0' ? 0 : 1;

                                int[] result = encrypt(message, key, iv);

                                for (int i = 0; i < result.Length; i++)
                                    outval += result[i];

                                Output = outval;
                            }
                            break;
                        }
                    case A5Settings.A5Mode.Decrypt:
                        {
                            if (!String.IsNullOrEmpty(messageInput) && !String.IsNullOrEmpty(keyString) && !String.IsNullOrEmpty(initialVector))
                            {
                                String outval = "";
                                key = new int[64];
                                iv = new int[22];

                                for (int i = 0; i < 64; i++)
                                    key[i] = keyString[i] == '0' ? 0 : 1;
                                for (int j = 0; j < 22; j++)
                                    iv[j] = initialVector[j] == '0' ? 0 : 1;
                                for (int k = 0; k < 228; k++)
                                    message[k] = messageInput[k] == '0' ? 0 : 1;

                                int[] result = decrypt(message, key, iv);

                                for (int i = 0; i < result.Length; i++)
                                    outval += result[i];

                                Output = outval;
                            }
                            break;
                        }
                    default:
                        {
                            throw new NotSupportedException("Unkown execution mode!");
                            //break;
                        }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
            ProgressChanged(1, 1);
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {

        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            this.stop = true;
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {

        }

        #endregion


    }
}
