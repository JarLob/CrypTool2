﻿/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.Collections;
using System.IO;
using System.ComponentModel;

namespace Cryptool.Achterbahn
{
    [Author("Armin Krauss","krauss@cryptool.org","CrypTool","http://www.vs.uni-due.de")]
    [PluginInfo("Cryptool.Achterbahn.Properties.Resources", "PluginCaption", "PluginTooltip", "Achterbahn/DetailedDescription/doc.xml", "Achterbahn/Images/icon.png")]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class Achterbahn : ICrypComponent
    {
        #region Private Variables

        private UInt64[] A = new UInt64[13];    // 13 Feedback registers
        private int[] FSRlengths = new int[13] { 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33 };
        private UInt64[] mask = new UInt64[13] { 0x1FFFFFL, 0x3FFFFFL, 0x7FFFFFL, 0xFFFFFFL, 0x1FFFFFFL, 0x3FFFFFFL, 0x7FFFFFFL, 0xFFFFFFFL, 0x1FFFFFFFL, 0x3FFFFFFFL, 0x7FFFFFFFL, 0xFFFFFFFFL, 0x1FFFFFFFFL };
        private UInt64 key33;
        private UInt64[] Key_bits = new UInt64[128];
        private UInt64[] IV_bits = new UInt64[128];

        private byte[] inputData;
        private byte[] inputKey;
        private byte[] inputIV;
        private byte[] outputData;

        private AchterbahnSettings settings = new AchterbahnSettings();

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputDataCaption", "InputDataTooltip", true)]
        public byte[] InputData
        {
            get { return this.inputData; }
            set
            {
                this.inputData = value;
                OnPropertyChanged("InputData");
            }
        }

        [PropertyInfo(Direction.InputData, "InputKeyCaption", "InputKeyTooltip", true)]
        public byte[] InputKey
        {
            get { return this.inputKey; }
            set
            {
                this.inputKey = value;
                OnPropertyChanged("InputKey");
            }
        }

        [PropertyInfo(Direction.InputData, "InputIVCaption", "InputIVTooltip", true)]
        public byte[] InputIV
        {
            get { return this.inputIV; }
            set
            {
                this.inputIV = value;
                OnPropertyChanged("InputIV");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputDataCaption", "OutputDataTooltip", true)]
        public byte[] OutputData
        {
            get { return this.outputData; }
            set
            {
                this.outputData = value;
                OnPropertyChanged("OutputData");
            }
        }

        #endregion

        #region IPlugin Members

        private bool checkParameters()
        {
            if (inputData == null)
            {
                GuiLogMessage("No input given. Aborting.", NotificationLevel.Error);
                return false;
            }

            if (inputKey == null)
            {
                GuiLogMessage("No key given. Aborting.", NotificationLevel.Error);
                return false;
            }

            if (inputIV == null)
            {
                GuiLogMessage("No IV given. Aborting.", NotificationLevel.Error);
                return false;
            }

            int upperlimit = (settings.Mode == 0) ? 10 : 16;

            if ((inputKey.Length < 5) || (inputKey.Length > upperlimit))
            {
                GuiLogMessage("Wrong key length " + inputKey.Length + " bytes. Key length must be >= 5 and <= " + upperlimit + " bytes for Achterbahn-" + (upperlimit * 8) + ".", NotificationLevel.Error);
                return false;
            }

            if ((inputIV.Length < 0) || (inputIV.Length > upperlimit))
            {
                GuiLogMessage("Wrong IV length " + inputIV.Length + " bytes. IV length must be <= " + upperlimit + " bytes for Achterbahn-" + (upperlimit * 8) + ".", NotificationLevel.Error);
                return false;
            }

            return true;
        }

        public void Execute()
        {
            ProgressChanged(0, 1);

            if (!checkParameters()) return;

            setup();

            OutputData = encrypt(inputData);

            ProgressChanged(1, 1);
        }

        // Helper functions
        UInt64 AND3(UInt64 a, UInt64 b, UInt64 c) { return (a & b & c); }
        UInt64 AND2(UInt64 a, UInt64 b) { return (a & b); }
        UInt64 XOR3(UInt64 a, UInt64 b, UInt64 c) { return (a ^ b ^ c); }
        UInt64 XOR2(UInt64 a, UInt64 b) { return (a ^ b); }
        UInt64 MUX3(UInt64 a, UInt64 b, UInt64 c) { return ((c & (a ^ b)) ^ a); }
        UInt64 MAJ3(UInt64 a, UInt64 b, UInt64 c) { return ((a & (b ^ c)) ^ (b & c)); }

        private void A0_cycle(UInt64 feedin)
        {
            UInt64 x = A[0];
            A[0] = (x >> 1) | ((1 & (XOR3(XOR3(feedin, x >> 15, XOR3(x >> 3, x >> 2, x)), XOR3(AND2(x >> 4, x >> 7), XOR3(x >> 5, x >> 6, x >> 8), MUX3(x >> 4, x >> 5, x >> 6)), MUX3(MUX3(x >> 11, x >> 12, x >> 2), AND3(x >> 6, x >> 2, x >> 13), MUX3(x >> 1, x >> 10, x >> 9))))) << 20);
        }

        private void A1_cycle(UInt64 feedin)
        {
            UInt64 x = A[1];
            A[1] = (x >> 1) | ((1 & (XOR3(XOR3(feedin, x >> 15, XOR3(x >> 8, x >> 5, x)), XOR3(AND2(x >> 5, x >> 11), MUX3(x >> 13, x >> 3, x >> 1), MUX3(x >> 6, x >> 4, x >> 12)), MUX3(MUX3(x >> 1, x >> 9, x >> 7), MUX3(x >> 4, x >> 12, x >> 10), AND3(x >> 1, x >> 11, x >> 14))))) << 21);
        }

        private void A2_cycle(UInt64 feedin)
        {
            UInt64 x = A[2];
            A[2] = (x >> 1) | ((1 & (XOR3(XOR3(feedin, x >> 16, XOR3(x >> 13, x >> 4, x)), XOR3(AND2(x >> 12, x >> 14), MUX3(x >> 1, x >> 9, x >> 7), MUX3(x >> 1, x >> 4, x >> 6)), MUX3(MUX3(x >> 5, x >> 8, x >> 11), MUX3(x >> 10, x >> 3, x >> 11), AND3(x >> 1, x >> 9, x >> 15))))) << 22);
        }

        private void A3_cycle(UInt64 feedin)
        {
            UInt64 x = A[3];
            A[3] = (x >> 1) | ((1 & (XOR3(XOR3(feedin, x >> 18, XOR3(x >> 8, x >> 3, x)), XOR3(AND2(x >> 1, x >> 11), MUX3(x >> 2, x >> 14, x >> 13), MUX3(x >> 12, x >> 4, x >> 13)), MUX3(MUX3(x >> 6, x >> 1, x >> 15), MUX3(x >> 14, x >> 16, x >> 9), MAJ3(x >> 2, x >> 5, x >> 7))))) << 23);
        }

        private void A4_cycle(UInt64 feedin)
        {
            UInt64 x = A[4];
            A[4] = (x >> 1) | ((1 & (XOR3(XOR3(feedin, x >> 20, XOR3(x >> 11, x >> 1, x)), XOR3(AND2(x >> 4, x >> 12), MUX3(x >> 1, x >> 3, x >> 5), MUX3(x >> 6, x >> 7, x >> 16)), MUX3(MAJ3(x >> 8, x >> 15, x >> 17), MUX3(x >> 14, x >> 13, x >> 12), MUX3(x >> 5, x >> 3, x >> 2))))) << 24);
        }

        private void A5_cycle(UInt64 feedin)
        {
            UInt64 x = A[5];
            A[5] = (x >> 1) | ((1 & (XOR3(XOR3(feedin, x >> 21, XOR3(x >> 17, x >> 16, x >> 15)), XOR3(XOR3(x >> 5, x >> 4, x), AND2(x >> 3, x >> 6), MUX3(x >> 4, x >> 18, x >> 2)), MUX3(MUX3(x >> 4, x >> 12, x >> 13), MUX3(x >> 14, x >> 11, x >> 7), MAJ3(x >> 3, x >> 10, x >> 15))))) << 25);
        }

        private void A6_cycle(UInt64 feedin)
        {
            UInt64 x = A[6];
            A[6] = (x >> 1) | ((1 & (XOR3(XOR3(feedin, x >> 25, XOR3(x >> 15, x >> 4, x)), XOR3(AND2(x >> 1, x >> 12), MUX3(x >> 10, x >> 6, x >> 17), MUX3(x >> 3, x >> 8, x >> 1)), MUX3(MUX3(x >> 10, x >> 14, x >> 13), MAJ3(x >> 17, x >> 2, x >> 16), AND3(x >> 18, x >> 11, x >> 5))))) << 26);
        }

        private void A7_cycle(UInt64 feedin)
        {
            UInt64 x = A[7];
            A[7] = (x >> 1) | ((1 & (XOR3(XOR3(feedin, x >> 25, XOR3(x >> 18, x >> 5, x)), XOR3(AND2(x >> 4, x >> 12), MUX3(x >> 1, x >> 17, x >> 2), MUX3(x >> 20, x >> 14, x >> 16)), MUX3(MUX3(x >> 18, x >> 15, x >> 10), AND3(x >> 1, x >> 2, x >> 13), AND3(x >> 7, x >> 9, x >> 19))))) << 27);
        }

        private void A8_cycle(UInt64 feedin)
        {
            UInt64 x = A[8];
            A[8] = (x >> 1) | ((1 & (XOR3(XOR3(feedin, x >> 24, XOR3(x >> 21, x >> 18, x >> 17)), XOR3(AND2(x >> 1, x >> 4), XOR3(x >> 11, x >> 2, x), MUX3(x >> 10, x >> 8, x >> 21)), MUX3(AND3(x >> 8, x >> 18, x >> 9), MUX3(x >> 13, x >> 6, x >> 15), MUX3(x >> 19, x >> 16, x >> 14))))) << 28);
        }

        private void A9_cycle(UInt64 feedin)
        {
            UInt64 x = A[9];
            A[9] = (x >> 1) | ((1 & (XOR3(XOR3(feedin, x >> 28, XOR3(x >> 18, x >> 1, x)), XOR3(AND2(x >> 2, x >> 8), MUX3(x >> 12, x >> 19, x >> 10), MUX3(x >> 10, x >> 14, x >> 22)), MUX3(MUX3(x >> 7, x >> 18, x >> 4), MAJ3(x >> 21, x >> 9, x >> 1), MAJ3(x >> 8, x >> 5, x >> 3))))) << 29);
        }

        private void A10_cycle(UInt64 feedin)
        {
            UInt64 x = A[10];
            A[10] = (x >> 1) | ((1 & (XOR3(XOR3(feedin, x >> 25, XOR3(x >> 18, x >> 15, x >> 6)), XOR3(XOR3(x >> 5, x >> 2, x), AND2(x >> 19, x >> 14), MUX3(x >> 17, x >> 12, x >> 21)), MUX3(MUX3(x >> 20, x >> 18, x >> 8), MAJ3(x >> 4, x >> 12, x >> 19), MUX3(x >> 22, x >> 7, x >> 21))))) << 30);
        }

        private void A11_cycle(UInt64 feedin)
        {
            UInt64 x = A[11];
            A[11] = (x >> 1) | ((1 & (XOR3(XOR3(feedin, x >> 28, XOR3(x >> 22, x >> 17, x >> 8)), XOR3(AND2(x >> 13, x >> 15), XOR3(x >> 5, x >> 3, x), MUX3(x >> 5, x >> 7, x >> 19)), MUX3(MUX3(x >> 8, x >> 2, x >> 13), AND3(x >> 4, x >> 11, x >> 24), MUX3(x >> 12, x >> 14, x >> 7))))) << 31);
        }

        private void A12_cycle(UInt64 feedin)
        {
            UInt64 x = A[12];
            A[12] = (x >> 1) | ((1 & (XOR3(XOR3(feedin, x >> 30, XOR3(x >> 23, x >> 10, x >> 9)), XOR3(XOR3(x >> 7, x >> 2, x), AND2(x >> 15, x >> 16), MUX3(x >> 25, x >> 15, x >> 13)), MUX3(MUX3(x >> 15, x >> 12, x >> 16), MAJ3(x >> 14, x >> 1, x >> 18), MUX3(x >> 8, x >> 24, x >> 17))))) << 32);
        }

        //Boolean combining function 
        private UInt64 F(UInt64 x0, UInt64 x1, UInt64 x2, UInt64 x3, UInt64 x4, UInt64 x5, UInt64 x6, UInt64 x7, UInt64 x8, UInt64 x9, UInt64 x10, UInt64 x11, UInt64 x12)
        {
            UInt64 A = x1 ^ x2;
            UInt64 C = x2 ^ x9;
            UInt64 H = x3 ^ x7;
            UInt64 T = x4 ^ x9;
            UInt64 E = ((x0 ^ x6) & x5) ^ x6;
            UInt64 R = ((x1 ^ x4) & C) ^ T;
            UInt64 b = (R ^ (A & x5) ^ x2) & H;
            UInt64 a = ((x10 ^ x11) & (C ^ (A & T) ^ E)) ^ E;
            UInt64 h = (x8 ^ x12) & (b ^ a ^ R ^ x7 ^ x10);
            UInt64 n = H ^ A ^ T ^ a ^ h ^ x0 ^ x5 ^ x6 ^ x11 ^ x12;
            return n;
        }

        private UInt64 KEYSTREAM_BITS()
        {
            return F(
                A[0] >> 5, A[1] >> 6, A[2] >> 7, A[3] >> 8,
                A[4] >> 9, A[5] >> 10, A[6] >> 11, A[7] >> 12, A[8] >> 13,
                A[9] >> 14, A[10] >> 15, A[11] >> 16, A[12] >> 17
                );
        }

        private void NLFSR_CYCLE(UInt64 feedin)
        {
            if (settings.Mode == 1) A0_cycle(feedin);
            A1_cycle(feedin);
            A2_cycle(feedin);
            A3_cycle(feedin);
            A4_cycle(feedin);
            A5_cycle(feedin);
            A6_cycle(feedin);
            A7_cycle(feedin);
            A8_cycle(feedin);
            A9_cycle(feedin);
            A10_cycle(feedin);
            A11_cycle(feedin);
            if (settings.Mode == 1) A12_cycle(feedin);
        }

        private void setup()
        {
            /* save key bits 0 to 33 in one context variable - for faster parallel loading */
            key33 = ((UInt64)InputKey[4] << 32)
                | ((UInt64)InputKey[3] << 24)
                | ((UInt64)InputKey[2] << 16)
                | ((UInt64)InputKey[1] << 8)
                | ((UInt64)InputKey[0]);

            /* save key bits in array, 1 bit/word for faster lookup */
            for (int i = 0; i < InputKey.Length * 8; ++i)
                Key_bits[i] = 1 & ((UInt64)InputKey[i / 8] >> (i % 8));

            /* save IV bits in array, 1 bit/word for faster lookup */
            for (int i = 0; i < InputIV.Length * 8; ++i)
                IV_bits[i] = 1 & ((UInt64)InputIV[i / 8] >> (i % 8));

            /* call setup function and set the initialization flag */
            ACHTERBAHN_setup();
        }

        private void ACHTERBAHN_setup()
        {
            /* ----------------------------------------------------------------------- *
             * step 1:
             * load all driving NLFSRs with the first key bits in parallel.
             * ----------------------------------------------------------------------- */
            if (settings.Mode == 1)
            {
                A[0] = key33 & mask[0];
                A[12] = key33 & mask[12];
            }
            else
            {
                A[0] = 0;
                A[12] = 0;
            }

            for (int i = 1; i < 12; i++)
                A[i] = key33 & mask[i];

            /* ----------------------------------------------------------------------- *
             * step 2:
             * for each driving NLFSR, feed-in the key bits, not already loaded into the
             * register in step 1, into the NLFSR. 
             * ----------------------------------------------------------------------- */
            if (settings.Mode == 1)
            {
                for (int i = FSRlengths[0]; i < InputKey.Length * 8; ++i) A0_cycle(Key_bits[i]);
                for (int i = FSRlengths[12]; i < InputKey.Length * 8; ++i) A12_cycle(Key_bits[i]);
            }

            for (int i = FSRlengths[1]; i < InputKey.Length * 8; ++i) A1_cycle(Key_bits[i]);
            for (int i = FSRlengths[2]; i < InputKey.Length * 8; ++i) A2_cycle(Key_bits[i]);
            for (int i = FSRlengths[3]; i < InputKey.Length * 8; ++i) A3_cycle(Key_bits[i]);
            for (int i = FSRlengths[4]; i < InputKey.Length * 8; ++i) A4_cycle(Key_bits[i]);
            for (int i = FSRlengths[5]; i < InputKey.Length * 8; ++i) A5_cycle(Key_bits[i]);
            for (int i = FSRlengths[6]; i < InputKey.Length * 8; ++i) A6_cycle(Key_bits[i]);
            for (int i = FSRlengths[7]; i < InputKey.Length * 8; ++i) A7_cycle(Key_bits[i]);
            for (int i = FSRlengths[8]; i < InputKey.Length * 8; ++i) A8_cycle(Key_bits[i]);
            for (int i = FSRlengths[9]; i < InputKey.Length * 8; ++i) A9_cycle(Key_bits[i]);
            for (int i = FSRlengths[10]; i < InputKey.Length * 8; ++i) A10_cycle(Key_bits[i]);
            for (int i = FSRlengths[11]; i < InputKey.Length * 8; ++i) A11_cycle(Key_bits[i]);

            /* ----------------------------------------------------------------------- *
             * step 3:
             * for each driving NLFSR, feed-in all IV bits, into the NLFSR.
             * ----------------------------------------------------------------------- */
            for (int i = 0; i < InputIV.Length * 8; ++i)
                NLFSR_CYCLE(IV_bits[i]);

            /* ----------------------------------------------------------------------- *
             * step 4:
             * Feed-in the keystream output into each NLFSR
             * ----------------------------------------------------------------------- */
            for (int i = 0; i < 32; ++i)
            {
                UInt64 z = KEYSTREAM_BITS();
                NLFSR_CYCLE(z);
            }

            /* ----------------------------------------------------------------------- *
             * step 5:
             * set the least significant bit (LSB) of each NLFSR to 1.
             * ----------------------------------------------------------------------- */
            if (settings.Mode == 1)
            {
                A[0] |= 1;
                A[12] |= 1;
            }

            for (int i = 1; i < 12; i++)
                A[i] |= 1;

            /* ----------------------------------------------------------------------- *
             * step 6:
             * warming up.
             * ----------------------------------------------------------------------- */
            for (int i = 0; i < 64; ++i)
                NLFSR_CYCLE(0);
        }

        #endregion

        // get a byte of the keystream
        private byte getStreamByte()
        {
            byte res = (byte)KEYSTREAM_BITS();

            /* cycle eight times to prepare a new byte */
            for (int i = 0; i < 8; i++) NLFSR_CYCLE(0);

            return res;
        }

        // Encrypt or decrypt data 
        public byte[] encrypt(byte[] src)
        {
            byte[] dst = new byte[src.Length];

            for (int i = 0; i < src.Length; i++)
                dst[i] = (byte)(src[i] ^ getStreamByte());

            return dst;
        }

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler
            OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler
            OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;


        private void GuiLogMessage(string message, NotificationLevel
            logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured,
                this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this,
                new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged,
                this, new PluginProgressEventArgs(value, max));
        }

        #endregion

        public ISettings Settings
        {
            get { return settings; }
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
            Dispose();
        }

        public void PostExecution()
        {
            Dispose();
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
            inputData = null;
            inputKey = null;
            inputIV = null;
            outputData = null;
        }
    }
}
