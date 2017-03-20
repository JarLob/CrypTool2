﻿using System;
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
    [PluginInfo("A5.Properties.Resources", "A5", "Encrypt the plaintext", "A5/userdoc.xml", new[] { "A5/Images/gsm.png" })]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]

    public class A5 : ICrypComponent
    {

        public bool stop;
        public ISettings settings;
        private byte[] keyBytes;
        private byte[] IVbytes;
        private byte[] output;
        // Converts bit representation to byte array
        private byte[] FromInt(int[] arr)
        {//initialize result array : each byte = 8 bits, so we divide
            byte[] result = new byte[arr.Length / 8];
            int temporary = 0;
            //cycle goes through all bytes
            for (int i = 0; i < result.Length; i++)
            {//cycle goes through all bits in current byte
                for (int j = 7; j >= 0; j--)
                {//first we shift the bit to its positon in byte
                    //binary OR operation sets the bit to temporary value
                    temporary |= arr[i * 8 + (7 - j)] << j;
                }//sets temporary value to array cell
                result[i] = (byte)temporary;
                temporary = 0;
            }
            return result;
        }
        //method which converts byte array to bit representation
        private int[] ToInt(byte[] arr, int size = -1)
        { //if size was not defined as the argument
            if (size == -1)
                //use default size (each byte = 8 bits)
                size = arr.Length * 8;
            //initializing resulting array
            int[] res = new int[size];
            //cycle goes through all bytes in the array
            for (int i = 0; i < arr.Length; i++)
            {//cycle goes through all bits in current byte
                for (int j = 7; j >= 0; j--)
                {//shifts the number to get current bit value 
                    res[res.Length - size] = (((int)arr[i]) >> j) & 1;
                    size--;
                    //if size was defined and we get all bits
                    if (size < 1)
                        //stop the cycle and end the method
                        return res;
                }
            }
            return res;
        }

        // method increments bit array by one
        // We use different IV`s for different frames. If for the 1st frame IV was 1100....00, for second it will be 0010.....00, for third 1010.....00 
        // for fourth 0110....00 and so on.
        private void Increment(int[] b, int curbit)
        {  //to stop, recursion checks if current bit value isn`t greater then array length
            if (curbit > b.Length)
                return;
            //if current bit is 1, set it to 0 and increment next (by recursive call of this method)
            if (b[curbit] == 1)
            {
                b[curbit] = 0;
                Increment(b, ++curbit);
            }
            else
            { //otherwise set 0 to 1 and end the recursion
                b[curbit]++;
            }
        }

        [PropertyInfo(Direction.InputData, "Plain text", "", true)]
        public Byte[] PlainText
        {
            get
            {
                return FromInt(plainText);
            }
            set
            {
                plainText = ToInt(value);
                OnPropertyChanged("PlainText");
            }
        }
        [PropertyInfo(Direction.InputData, "Key", "Size = 8 bytes", true)]
        public Byte[] Key
        {
            get
            {
                return keyBytes;
            }
            set
            {
                keyBytes = value;
                OnPropertyChanged("Key");
            }
        }
        [PropertyInfo(Direction.InputData, "Initial vector", "Size= 3 bytes", true)]
        public Byte[] InitialVector
        {
            get
            {
                return IVbytes;
            }
            set
            {
                IVbytes = value;
                OnPropertyChanged("InitialVector");
            }
        }

        [PropertyInfo(Direction.OutputData, "Cipher text", "", true)]
        public Byte[] CipherText
        {
            get
            {
                return output;
            }
            set
            {
                output = value;
                OnPropertyChanged("CipherText");
            }
        }


        int[] plainText;
        int[] cipherText;
        private int NumberOfFrames;//Number of different frames to generate
        public A5()
        {
            this.settings = new A5Settings();
        }

        private int[] working_key;
        private int[] working_IV;

        LFSR[] registers;
        //method init all registers for 1 frame
        public void InitFrame(int[] key, int[] iv)
        {
            //init arrays for key and iv
            working_key = new int[64];
            working_IV = new int[22];
            //copying values to those arrays 
            Array.Copy(key, working_key, 64);
            Array.Copy(iv, working_IV, 22);
            //initialize LFSR array
            registers = new LFSR[3];
            //creating LFSRs for A5/1
            // define the register's length, tapped bits and clocking bits
            registers[0] = new LFSR(19, new int[] { 13, 16, 17, 18 }, 8);
            registers[1] = new LFSR(22, new int[] { 20, 21 }, 10);
            registers[2] = new LFSR(23, new int[] { 7, 20, 21, 22 }, 10);
            //initialize LFSRs values
            InitPhase();
        }
        //method for initializing LFSRs values
        //inputs key, IV and then 100 rounds
        private void InitPhase()
        {
            //64 rounds for inputting the key
            for (int i = 0; i < 64; i++)
            {
                registers[0].Shift(working_key[i]);
                registers[1].Shift(working_key[i]);
                registers[2].Shift(working_key[i]);
            }
            //22 rounds for inputting the IV
            for (int i = 0; i < 22; i++)
            {
                registers[0].Shift(working_IV[i]);
                registers[1].Shift(working_IV[i]);
                registers[2].Shift(working_IV[i]);
            }
            //100 rounds, following the majority rule
            for (int i = 0; i < 100; i++)
            {
                Majority();
            }
        }
        //method gets major bit value and returns it
        private int GetMajor()
        {      // if the clocking bit of the first register is the same as the clocking bit of one of the other registers,
               // then this clocking bit is considered as majority bit

            if (registers[0].ClockingTap() == registers[1].ClockingTap() || registers[0].ClockingTap() == registers[2].ClockingTap())
            {
                return registers[0].ClockingTap();
            }
            else
            {//otherwise the clocking bit of the second register is in majority
                return registers[1].ClockingTap();
            }
        }
        private void Majority()
        {
            //get value of major bit
            int major = GetMajor();
            //LINQ expression, select registers that have the clocking bit in majority and then shifts them
            var shiftingRegisters = registers.Where(x => x.ClockingTap() == major);
            foreach (var reg in shiftingRegisters)
            {
                reg.Shift();
            }

        }
        //output function, XOR of last values in each LFSR
        private int Output()
        {
            return (registers[0].GetLast() + registers[1].GetLast() + registers[2].GetLast()) % 2;
        }
        //encrypts single bit
        private int Cipher(int infoBit)
        {
            //majority function, and shifting registers
            Majority();
            //result is XORed with plaintext
            int res = (infoBit + Output()) % 2;

            return res;
        }
        //encrypts array of bits
        public int[] Encrypt(int[] frame)
        {
            int[] encrypted = new int[frame.Length];
            for (int i = 0; i < frame.Length; i++)
            {
                encrypted[i] = Cipher(frame[i]);
            }
            return encrypted;
        }

        public void Execute()
        {
            ProgressChanged(0, 1);
            //check the validity of the input parameters
            if (InitialVector.Length != 3)
            {
                GuiLogMessage("Not valid IV length!", NotificationLevel.Error);
                return;
            }
            if (Key.Length != 8)
            {
                GuiLogMessage("Not valid key length!", NotificationLevel.Error);
                return;
            }
            //initialize the bit array for cipher text
            cipherText = new int[plainText.Length];
            //init bit array for IV
            int[] temporary_IV = new int[22];
            //converting the IV from byte to bit representation and copying them to temporary IV array 
            Array.Copy(ToInt(IVbytes), temporary_IV, 22);
            //gets the number of frames from settings 
            NumberOfFrames = ((A5Settings)settings).FramesCount;
            //count frame size
            int frameSize = plainText.Length / NumberOfFrames;

            //initialize bit array for one (current) frame
            int[] framePlain = new int[frameSize];

            int[] frameCipher;
            for (int i = 0; i < NumberOfFrames; i++)
            {
                //init registers with key and temporary IV
                InitFrame(ToInt(keyBytes), temporary_IV);
                //this output registers values after initial state (before keystream generation)
                GuiLogMessage(this.ToString(), NotificationLevel.Info);
                //copying current frame to array
                Array.Copy(plainText, i * frameSize, framePlain, 0, frameSize);
                //encrypting frame and writing it to the temporary cipher frame array
                frameCipher = Encrypt(framePlain);
                //copying current frame ciphertext to the general ciphertext array
                Array.Copy(frameCipher, 0, cipherText, i * frameSize, frameSize);
                //incrementing the IV for the next frame
                Increment(temporary_IV, 0);
            }
            //converting ciphertext from bit representation to byte array
            CipherText = FromInt(cipherText);
            ProgressChanged(1, 1);
        }

        public override string ToString()
        {
            return registers[0].ToString() + "\n" + registers[1].ToString() + "\n" + registers[2].ToString() + "\n";
        }

        #region IPlugin Members
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));

        }

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
    class LFSR
    {
        int[] cells;
        int[] tapBits;
        int clockingTap;
        public LFSR(int size, int[] tapped, int clocking)
        {
            cells = new int[size];
            tapBits = tapped;
            clockingTap = clocking;
        }
        public int Shift(int input = 0)
        {
            int result = cells[cells.Length - 1];
            int next = input;
            foreach (var tap in tapBits)
            {
                next += cells[tap];
            }
            next = next % 2;
            for (int i = cells.Length - 1; i > 0; i--)
            {
                cells[i] = cells[i - 1];
            }
            cells[0] = next;
            return result;
        }
        public int this[int indexer]
        {
            get { return cells[indexer]; }
        }
        public int ClockingTap()
        {
            return this[clockingTap];
        }
        public int GetLast()
        {
            return cells[cells.Length - 1];
        }
        public override string ToString()
        {
            string s = "";
            foreach (var i in cells)
            {
                s += i.ToString();
            }
            return s;
        }
    }
}
