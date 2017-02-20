using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Cryptool.PluginBase;
using Cryptool.Plugins.A5;
using Cryptool.PluginBase.IO;
using System.ComponentModel;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Controls;
using System.Diagnostics;
using System.Collections;
using System.Text.RegularExpressions;

namespace Cryptool.Plugins.A5
{
    [Author("Kristina Hita", "khita@mail.uni-mannheim.de", "Universität Mannheim", "https://www.uni-mannheim.de/1/english/university/profile/")]
    [PluginInfo("A5.Properties.Resources", "PluginCaption", "PluginTooltip", "A5/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    

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
        private int[] message; //plaintext

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

        //modified
        byte[] keyBytes;
        byte[] ivBytes;
        byte[] messageBytes;
        byte[] outBytes;
        #endregion
        //
        //Converts byte array into string for future using
        private String BytesArrToString(byte[] arr)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var b in arr)
            {
                sb.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
            }
            return sb.ToString();
        }

        public A5()
        {
            this.settings = new A5Settings();
        }


        [PropertyInfo(Direction.InputData, "Key", "Key length = 8 bytes", true)]
        public Byte[] Key
        {
            get
            {
                return keyBytes;
            }
            set
            {
                keyBytes = value;
                KeyString = BytesArrToString(keyBytes);
                OnPropertyChanged("Key");
            }
        }

        public String KeyString
        {
            get { return this.keyString; }
            set
            {
                this.keyString = value;
            }
        }

        [PropertyInfo(Direction.InputData, "IV", "IV length = 22 bits, (last two bits of last byte wont be calculated)", true)]
        public Byte[] IV
        {
            get
            {
                return ivBytes;
            }
            set
            {
                ivBytes = value;
                initialVector = BytesArrToString(ivBytes).Substring(0, 22);

                OnPropertyChanged("IV");
            }
        }

        public String InitialVector
        {
            get { return this.initialVector; }
            set
            {
                this.initialVector = value;
            }
        }

        [PropertyInfo(Direction.InputData, "InputMessage", "Input message bytes", true)]
        public Byte[] Message
        {
            get
            {
                return messageBytes;
            }
            set
            {
                messageBytes = value;
                OnPropertyChanged("InputMessage");
            }
        }

        public String MessageInput
        {
            get { return this.messageInput; }
            set
            {
                this.messageInput = value;
            }
        }

        [PropertyInfo(Direction.OutputData, "Output bytes", "Chipher text", true)]
        public Byte[] Out
        {
            get
            {
                return outBytes;
            }
            set
            {
                outBytes = value;

                OnPropertyChanged("Out");
            }
        }
        public String Output
        {
            get
            {
                return output;
            }
            set
            {
                this.output = value;
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

        // get the maximum length of registers
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
        private int[] GetIndexValues()
        {
            int[] indexValues = new int[registers.Length];

            for (int i = 0; i < registers.Length; i++)
            {
                indexValues[i] = registers[i][rIndexes[i]];
            }

            return indexValues;
        }

        //This function is going to be used later for finding majority bit
        // it stores the frequency of index values of the three registers

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
            // registers are initialized to zero 
            registers = new int[nRegisters][];
            for (int i = 0; i < nRegisters; i++)
            {
                int[] newReg = new int[mRegLens[i]];
                for (int k = 0; k < mRegLens[i]; k++)
                    newReg[k] = 0;
                registers[i] = newReg;
            }
            // after initializing to zero, the key is going to be mixed
            MixKey();
            // after that, the IV is going to be mixed with the feedback of the registers
            MixIV();
            for (int j = 0; j < 100; j++)
            {// registers are being clocked 100 times, using the feedback values
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
                // from each register we are getting values to XOR
                vToXor[i] = registers[i][0];
            // after that we XOR these values
            outValue = XorRegValues(vToXor);

            int[] regTS = RegistersToShift();
            int[] feedbackset = GetFeedbackValues(regTS);
            // registers are being shifted with the feedback set values of registers
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

            // here we find the majority bit
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

            ArrayList regTS = new ArrayList();

            for (int i = 0; i < indexValues.Length; i++)
            { // only registers that are in majority will be clocked
                if (indexValues[i] == movVal)
                    regTS.Add(i);
            }

            return (int[])regTS.ToArray(typeof(int));
        }

        private void MixKey()
        {
            int[] regTS = new int[3] { 0, 1, 2 };

            for (int i = 0; i < key.Length; i++)
            { // we get the feedback value of each register
                int[] feedbackset = GetFeedbackValues(regTS);
                for (int j = 0; j < feedbackset.Length; j++)
                    // the feedback values of registers are going to be XORed with the secret key bits
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
                    // feedback values of registers are going to be XORed with the IV bits
                    feedbackset[j] = XorValues(feedbackset[j], iv[i]);
                // after XORing these values, the registers will be shifted 
                RegisterShiftWithVal(regTS, feedbackset);
            }

        }

        // The feedback in registers are calculated from the bits that are taken out such as for example 13th, 16th, 17th,18th bits in first register
        private int[] GetFeedbackValues(int[] regTS)
        {
            int[] regTSFBV = new int[regTS.Length]; //Reg To Shift Feed Back Values (regTSFBV)

            for (int i = 0; i < regTS.Length; i++)
            {    // the Feedback set of bits in each registers is received from the tapped bits
                int[] feedbackSet = new int[tappedBits[regTS[i]].Length];

                for (int x = 0; x < tappedBits[regTS[i]].Length; x++)
                {
                    feedbackSet[x] = registers[regTS[i]][tappedBits[regTS[i]][x]];
                }

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
            int[] encryptedText = new int[plaintext.Length];
            for (int i = 0; i < plaintext.Length; i++)
            {
                encryptedText[i] = (GetOutValue() + message[i]) % 2;
            }
            return encryptedText;
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
            }
            else
            {
                int diff = bytes - inString.Length;

                for (int i = 0; i < bytes; i++) initS[i] = '0';
                Array.Copy(inArray, 0, initS, diff, inArray.Length);
            }
            return new String(initS);
        }


        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));

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
            {// check the validity of input data
                if (keyBytes.Length != 8)
                {
                    GuiLogMessage("Key Length must be 64 bits, current length " + keyBytes.Length, NotificationLevel.Error);
                    return;
                }
                if (String.IsNullOrEmpty(initialVector) || initialVector.Length != 22)
                {
                    GuiLogMessage("Initial Vector Length must be 22 bits, current length " + initialVector.Length, NotificationLevel.Error);
                    GuiLogMessage(initialVector, NotificationLevel.Error);
                    return;
                }

                String outval = "";
                key = new int[64];
                iv = new int[22];
                message = new int[messageBytes.Length * 8];

                String messagebits = BytesArrToString(messageBytes);

                //Convert text to binary sequence


                for (int i = 0; i < 64; i++)
                    key[i] = keyString[i] == '0' ? 0 : 1;
                for (int j = 0; j < 22; j++)
                    iv[j] = initialVector[j] == '0' ? 0 : 1;

                for (int k = 0; k < messageBytes.Length * 8; k++)
                {
                    message[k] = messagebits[k] == '0' ? 0 : 1;
                }

                int[] result = encrypt(message, key, iv);

                for (int i = 0; i < result.Length; i++)
                    outval += result[i];
                List<Byte> byteList = new List<Byte>();
                for (int i = 0; i < outval.Length; i += 8)
                {
                    byteList.Add(Convert.ToByte(outval.Substring(i, 8), 2));
                }
                Out = byteList.ToArray();
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
