using System;
using System.Collections.Generic;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Controls;
using System.Collections;

namespace Cryptool.Plugins.A5
{
    [Author("Kristina Hita", "khita@mail.uni-mannheim.de", "Universität Mannheim", "https://www.uni-mannheim.de/1/english/university/profile/")]
    [PluginInfo("A5.Properties.Resources", "PluginCaption", "PluginTooltip", "A5/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    /**
     * This Class generates the keystream for A5/1. The Stream is generated
     * based on the given key and frame number IV given to LFSRs  
     *
     */
    public class A5 : ICrypComponent
    {

        #region Algorithm variables

        public bool dbMode; //Debug mode

        private int nRegisters = 3; //number of registers
        private int[] mRegLens = new int[] { 19, 22, 23 }; //max register lengths
        private int[] rIndexes = new int[] { 8, 10, 10 }; //register indexes (clocking bits)

        private int[][] registers; //registers (we are using this to create a table with the registers space for all 3 registers (1st row --> register 1 ; 2nd row --> register 2 etc)
        private int[] iv;
        private int[] key;
        private int[] message;

        // bits that are tapped in each register (1st row -> Tapped bits of 1st Register and similarly the list follows till third register)
        private int[][] tappedBits = new int[3][]
        {
            new int[] { 18, 17, 16, 13 },
            new int[] { 21, 20 },
            new int[] { 22, 21, 20, 7 }
        };

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

        [PropertyInfo(Direction.InputData, "IV-String", "IV-Tooltip", true)]
        public String InitialVector
        {
            get { return this.initialVector; }
            set
            {
                this.initialVector = value;
                OnPropertyChanged("InitialVector");
            }
        }

        [PropertyInfo(Direction.InputData, "InputMessageString", "InputMessageTooltip", true)]
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

        // The loop has been created to store the index values of the 3 registers in the array. We get index from length of each register
        // We will use indexes later when we will shift registers
        private int[] GetIndexValues()
        {
            int[] indexValues = new int[registers.Length];

            for (int i = 0; i < registers.Length; i++)
            {
                indexValues[i] = registers[i][rIndexes[i]];
            }

            return indexValues;
        }

        
        // creates a vector that counts the frequency of 0s and 1s of the index values
        // this function is going to be used later to indicate the majority bit
        private int[] FindFrequency(int[] indexValues)
        {
            int[] tally = new int[2]; //size of 2 since it's just binary

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
                // define register's length
                int[] newReg = new int[mRegLens[i]];
                for (int k = 0; k < mRegLens[i]; k++)
               // fill them initially with zero
                    newReg[k] = 0;
                registers[i] = newReg;
            }
            // after initializing the registers, we are mixing the secret key and the IV 
            // (using XOR operations between tapped bits and secret key bits)
            MixKey();
            MixIV();
            // registers are then clocked 100 times
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

        // This is for filling registers with data. We would know the length of register from GetIndexValues so the that the function will know when to stop filling
        public int[] RegistersToShift()
        {   // get index values 
            int[] indexValues = GetIndexValues();
            // find the frequencies of the index values
            int[] tally = FindFrequency(indexValues);

            int highest = 0;
            int movVal = 0;

            if (dbMode)
            {
                Console.WriteLine("[indexValues]:");
                foreach (int v in indexValues)
                    // convert index values to their string representation
                    Console.Write(v.ToString() + " ");
                Console.WriteLine("\n[/indexValues]:");

                Console.WriteLine("[tally]:");
                foreach (int v in tally)
                    // convert the majority bit to it's string representation
                    Console.Write(v.ToString() + " ");
                Console.WriteLine("\n[/tally]:");
            }


            //this loop is used to calculate the majority bit 
            // the index values of each register indicate the majority bit
            // eg. if the index values of the registers are (1,0,0) this means that the majority bit is 0
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
            // registers in majority are being shifted
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
                // get the feedback values
                int[] feedbackset = GetFeedbackValues(regTS);
                // XOR secret key bits with feedback values bits
                for (int j = 0; j < feedbackset.Length; j++)
                    feedbackset[j] = XorValues(feedbackset[j], key[i]);
                // shift registers 
                RegisterShiftWithVal(regTS, feedbackset);

            }
        }
        // function to XOR IV values with registers values
        private void MixIV()
        {
            int[] regTS = new int[3] { 0, 1, 2 };

            for (int i = 0; i < iv.Length; i++)
            {    // get feedback values of registers (based on tapped bits)
                int[] feedbackset = GetFeedbackValues(regTS);
                for (int j = 0; j < feedbackset.Length; j++)
                    // XOR feedback values of the registers with the IV bit values
                    feedbackset[j] = XorValues(feedbackset[j], iv[i]);
                // shift registers
                RegisterShiftWithVal(regTS, feedbackset);
            }

        }

        // The feedback in registers are calculated from the tapped bits that are taken out
        // such as for example 13th, 16th, 17th,18th bits in first register
        private int[] GetFeedbackValues(int[] regTS)
        {
            int[] regTSFBV = new int[regTS.Length]; //Reg To Shift Feed Back Values (regTSFBV)

            for (int i = 0; i < regTS.Length; i++)
            {   // get the tapped bits of each register
                int[] feedbackSet = new int[tappedBits[regTS[i]].Length];

                for (int x = 0; x < tappedBits[regTS[i]].Length; x++)
                {
                    feedbackSet[x] = registers[regTS[i]][tappedBits[regTS[i]][x]];
                }
                // XOR values of feedback bits
                regTSFBV[i] = XorRegValues(feedbackSet);
            }

            return regTSFBV;
        }

        public void RegisterShiftWithVal(int[] regTS, int[] val)
        {
            for (int i = 0; i < regTS.Length; i++)
            {
                int[] regShifting = registers[regTS[i]]; //Make a copy of the register to shift

                //Creates new register with appropriate max reg length

                int[] nRegister = new int[regShifting.Length];

                //Shifting values (the last bit is replaced with the second last bit and the rest of the bits are shifted to the right.)

                for (int x = regShifting.Length - 1; x > 0; x--)
                    nRegister[x] = regShifting[x - 1]; //



                //Now put feedback value on the zero index (the feedback value we recieve from xoring of tapped bits.)

                nRegister[0] = val[i];

                registers[regTS[i]] = nRegister; //assign to register (update)
            }
        }

        public int[] encrypt(int[] plaintext, int[] encryptKey, int[] initialvector)
        {

            CreateRegisters();
            int[] encryptedText = new int[228];
            for (int i = 0; i < 228; i++)
            { // the keystream is being XORed with the plaintext to give the ciphertext
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

            // truncate the string if it is too big
            if (inString.Length > bytes)
            {  
                int start = inString.Length - bytes;
                Array.Copy(inArray, start, initS, 0, initS.Length);
                //			System.arraycopy(inArray, start, initS, 0, initS.Length);
            }
            else
            { // else add zeros to the left most bit (MSB)
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
            {  // check validity of input values
                if (String.IsNullOrEmpty(messageInput) || messageInput.Length != 28)
                {  // the plaintext must be 28 characters
                    // because 28*8 bits=224 , 4 bits padded left to make it 228 bits
                    GuiLogMessage("Message Length must be 28 characters! Please stop the template and then Click Play after entering correct length.", NotificationLevel.Error);
                    return;
                }
                if (String.IsNullOrEmpty(keyString) || keyString.Length != 64)
                {
                    GuiLogMessage("Key Length must be 64 bits! Please stop the template and then Click Play after entering correct length.", NotificationLevel.Error);
                    return;
                }
                if (String.IsNullOrEmpty(initialVector) || initialVector.Length != 22)
                {
                    GuiLogMessage("Initial Vector Length must be 22 bits! Please stop the template and then Click Play after entering correct length.", NotificationLevel.Error);
                    return;
                }
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

                                String messagebits = "";

                                //Convert text to binary sequence
                                foreach (char c in messageInput)
                                    messagebits += Convert.ToString((int)c, 2).PadLeft(8, '0');
                                //-------//

                                // Pad zeros to the bit sequence to create frame length 228 bits for A5/1
                                if (messagebits.Length < 228)
                                    messagebits = messagebits.PadLeft(228, '0');
                                //------//

                                for (int i = 0; i < 64; i++)
                                    key[i] = keyString[i] == '0' ? 0 : 1;
                                for (int j = 0; j < 22; j++)
                                    iv[j] = initialVector[j] == '0' ? 0 : 1;
                                for (int k = 0; k < 228; k++)
                                    message[k] = messagebits[k] == '0' ? 0 : 1;

                                int[] result = encrypt(message, key, iv);

                                for (int i = 0; i < result.Length; i++)
                                    outval += result[i];

                                // padding 0 bits on left to create bit length of 232 to convert back to text, 
                                // 228 bits if cipher can all be non-zero and to generate text out of 228 bits we need
                                // the bit length to be multiple of 8 bits (byte)
                                if (outval.Length == 228)
                                    if (outval.Length % 8 != 0)
                                        outval = outval.PadLeft(232, '0');

                                // Convert Binary Sequence back to text
                                List<Byte> byteList = new List<Byte>();
                                for (int i = 0; i < outval.Length; i += 8)
                                {
                                    byteList.Add(Convert.ToByte(outval.Substring(i, 8), 2));
                                }
                                String cipher = Encoding.ASCII.GetString(byteList.ToArray());

                                Output = cipher;
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

                                String messagebits = "";

                                //Convert text to binary sequence
                                foreach (char c in messageInput)
                                    messagebits += Convert.ToString((int)c, 2).PadLeft(8, '0');
                                //-------//

                                // Removing Padded zeros to the bit sequence to create frame length 228 bits for A5/1

                                if (messagebits.Length > 228)
                                {
                                    messagebits = messagebits.Remove(0, 4);
                                }
                                //------//

                                for (int i = 0; i < 64; i++)
                                    key[i] = keyString[i] == '0' ? 0 : 1;
                                for (int j = 0; j < 22; j++)
                                    iv[j] = initialVector[j] == '0' ? 0 : 1;
                                for (int k = 0; k < 228; k++)
                                    message[k] = messagebits[k] == '0' ? 0 : 1;

                                int[] result = decrypt(message, key, iv);

                                for (int i = 0; i < result.Length; i++)
                                    outval += result[i];

                                // removing padded 0 bits on left to create bit length of 224 to convert back to text, 
                                // in the plain text we added four bits to create 228 bits of length
                                // the bit length to be multiple of 8 bits (byte)
                                if (outval.Length == 228)
                                    if (outval.Length % 8 != 0)
                                        outval = outval.Remove(0, 4);

                                // Convert Binary Sequence back to text
                                List<Byte> byteList = new List<Byte>();
                                for (int i = 0; i < outval.Length; i += 8)
                                {
                                    byteList.Add(Convert.ToByte(outval.Substring(i, 8), 2));
                                }
                                String plain = Encoding.ASCII.GetString(byteList.ToArray());

                                Output = plain;
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
