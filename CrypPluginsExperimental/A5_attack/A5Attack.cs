/*
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
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.A5_attack
{
    [Author("Kristina Hita", "khita@mail.uni-mannheim.de", "Universität Mannheim", "https://www.uni-mannheim.de/1/english/university/profile/")]
    [PluginInfo("A5_attack.Properties.Resources", "PluginCaption", "PluginTooltip", "A5/DetailedDescription/doc.xml", new[] { "A5/gsm icon.png" })]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class A5_attack : ICrypComponent
    {

        public byte[] CipherText;
        public byte[] PlainText;
        public byte[] IV, outp;
        public byte[] Keystream;
        public byte[] encryptedData;
        public int index;
        private int[][] registers;

        public ISettings Settings
        {
            get { return null; }
        }

        [PropertyInfo(Direction.InputData, " Text input ", " Input a string to be processed by the cipher ", false)]
        public string InputString
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "Text output", "Output a string to be processed by the  cipher ", false)]
        public string OutputString
        {
            get;
            set;
        }

        public void Execute()
        {



            // convert given ciphertext into frames
            // since the frame corresponds to the IV, the size of each frame is 22 bits
            //there is a total of 228 frames

            byte[] frame = new byte[228];
            for (int runs = 0; runs < 228; runs++)
            {
                frame[runs] = CipherText[runs];
            }
        }

        // Encrypt data
        public byte getKeyStreamByte()
        {
            if (index > 1)
            {
                Round();
                index = 0;
            }
            return outp[index++];
        }



        //a function that indicates how 2 values are being XORed to each other

        private int XorValues(int val1, int val2)
        {
            int res = 0;

            if (val1 != val2)
                res = 1;

            return res;
        }


        //using the function mentioned above, here we XOR register values
        private int XorRegValues(int[] vToXor)
        {
            int final = 0;

            for (int i = 0; i < vToXor.Length; i++)
                final = XorValues(final, vToXor[i]);

            return final;
        }





        // In each round the output bits are obtained as a result of XOR of the bits from each register 
        public int Round()
        {
            int[] vToXor = new int[registers.Length];
            int outValue = 0;

            for (int i = 0; i < registers.Length; i++)
                vToXor[i] = registers[i][0];

            outValue = XorRegValues(vToXor);

            return outValue;
        }







        //dst is the value of plaintext that has been calculated using (key and IV)

        //Here we see if the (key and IV(InVector)) produce plaintext equal to original plaintext)

        public byte[] encrypt(byte[] src, byte[] InVector)
        {
            byte[] dst = new byte[src.Length];

            for (int i = 0; i < src.Length; i++)
                dst[i] = (byte)(src[i] ^ getKeyStreamByte()); //role of key comes here

            return dst;
            if (dst == PlainText)
            {
                byte[] Result = PlainText;
                System.Console.Write("Keystream is all zeros");
            }
            else

            {
                foreach (int v in PlainText)
                {
                    Console.WriteLine("plaintext and its complement", PlainText, PlainText);
                }
            }
        }




        public void Dispose()
        {
            CipherText = null;
            PlainText = null;
            Keystream = null;
            IV = null;
        }

        public void Initialize()
        {
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

        public void PostExecution()
        {
            Dispose();
        }

        public void PreExecution()
        {
            Dispose();
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public void Stop()
        {
        }
        #region Event Handling

        public event PropertyChangedEventHandler PropertyChanged;

        private void PluginrogressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}
