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
using System.Text;
using System.Collections.Generic;

namespace Cryptool.Plugins.A5_attack
{
    [Author("Kristina Hita", "khita@mail.uni-mannheim.de", "Universität Mannheim", "https://www.uni-mannheim.de/1/english/university/profile/")]
    [PluginInfo("A5_attack.Properties.Resources", "PluginCaption", "PluginTooltip", "A5/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class A5_attack : ICrypComponent
    {

        private String outputKey = null;
        private String guessKeyBits = null;
        private String keyString = null;
        private String initialVector = null;

        private byte[] keyBytes = null;
        private byte[] ivBytes = null;
        private byte[] guessBytes = null;
        private byte[] outBytes = null;
        private string guessCase = null;

        A5AttackSettings settings = new A5AttackSettings();
        private String BytesArrToString(byte[] arr)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var b in arr)
            {
                sb.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
            }
            return sb.ToString();
        }
        public ISettings Settings
        {
            get { return settings; }
        }

        [PropertyInfo(Direction.InputData, "InputKeyBytes", "Key bytes", true)]
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
                OnPropertyChanged("InputKeyBytes");
            }
        }

        public String KeyString
        {
            get { return this.keyString; }
            set
            {
                this.keyString = value;
                OnPropertyChanged("KeyString");
            }
        }

        [PropertyInfo(Direction.InputData, "IVBytes", "IV-Tooltip", true)]
        public Byte[] IV
        {
            get
            {
                return ivBytes;
            }
            set
            {
                ivBytes = value;
                InitialVector = BytesArrToString(ivBytes).Substring(0, 22);

                OnPropertyChanged("IVBytes");
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

        [PropertyInfo(Direction.OutputData, "OutputKeyBytes", "", true)]
        public Byte[] OutputKeyBytes
        {
            get
            {
                return outBytes;
            }
            set
            {
                outBytes = value;
                OnPropertyChanged("OutputKeyBytes");
            }
        }

        public String OutputKey
        {
            get
            {
                return outputKey;
            }
            set
            {
                this.outputKey = value;
            }
        }

        [PropertyInfo(Direction.OutputData, "GuessKeyBytes", "GuessKeyBitsTooltip", true)]
        public Byte[] GuessKeyBytes
        {
            get
            {
                return guessBytes;
            }
            set
            {
                guessBytes = value;
                OnPropertyChanged("GuessKeyBytes");
            }
        }

        public String GuessKeyBits
        {
            get
            {
                return guessKeyBits;
            }
            set
            {
                this.guessKeyBits = value;
            }
        }

        [PropertyInfo(Direction.OutputData, "GuessCase", "Case used", true)]
        public String GuessCase
        {
            get
            {
                return guessCase;
            }
            set
            {
                guessCase = value;
                OnPropertyChanged("GuessCase");
            }
        }

        public void Execute()
        {
            ProgressChanged(0, 1);
            // check the validity of input data
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

            if (!String.IsNullOrEmpty(keyString) && !String.IsNullOrEmpty(initialVector))
            {
                int[] key = new int[64]; // 64 bit key provided by user, used only to check if the generated key is equal to this key
                int[] iv = new int[22]; // 22 bit Initial Vector
                int[] outKey; // to store the generated key bits by different Scenarios
                string caseUsed = ""; // to identify the case where the scenario falls into (according to the paper where the attack is introduced)
                string finalGuessbits = ""; // to store the guess bits which will be responsible for generation of the key


                for (int i = 0; i < 64; i++)
                    key[i] = keyString[i] == '0' ? 0 : 1;
                for (int j = 0; j < 22; j++)
                    iv[j] = initialVector[j] == '0' ? 0 : 1;

                bool keyEquals = true; // To check if the key created by any scenario is equal to the original key

                // Scenario 0 (represented as case 1) , where all registers are zero, we only need the IV to derive 64 bits of key
                outKey = Scenario0(iv);

                for (int i = 0; i < 64; i++)
                {
                    if (!(outKey[i] == key[i]))
                    {
                        keyEquals = false; // If a single bit is not equal to key bit then this loop will break and keyEqual bool will be set to false
                        break;
                    }
                }

                if (!keyEquals) // If Scenario 0 is not met, then we start checking the Scenario 1, 2 and 3 (a.k.a case 2a, 2b, 2c)
                {

                    // Creating a table of possible 23 bit sequence (guess bits/ free bits) which will be used for attack.
                    // Note that we need 23 bits for Case 2a (Scenario 1), 22 bits for Case 2b (Scenario 2), 19 bits for Case 2c (Scenario 3).
                    // This table of guess bits is created only once and can be used for all three cases by skipping the initial bits when not required depending on register length.
                    String[] guess = new String[(int)Math.Pow(2, 23)];

                    for (int i = 0; i < Math.Pow(2, 23); i++)
                    {
                        guess[i] = Convert.ToString((int)i, 2).PadLeft(23, '0');
                    }
                    //------//

                    for (int j = 0; j < guess.Length; j++) // Loop through all possible guess bits generated
                    {
                        int[] k1;
                        // for case 2c (scenario 3), where register A is non-zero, we only need 19 guess bits, 
                        // which means we can skip the cases where the bit at index 3 becomes 1 in guess bits
                        if (guess[j][3] != '1')
                        {
                            k1 = new int[19];

                            // we start the loop at position 4 because we created above a table for 23 possible bit sequence of free bits
                            // and we only need 19 bits for case 2c 
                            for (int k = 4; k < 23; k++)
                                k1[k - 4] = guess[j][k] == '0' ? 0 : 1;
                            // calculate the secret key bits using the IV and the guessed bits (and the formulas for scenario 3) 
                            int[] kk2 = Scenario3(k1, iv);

                            keyEquals = true;
                            for (int i = 0; i < 64; i++)
                            {
                                if (!(kk2[i] == key[i]))
                                {
                                    keyEquals = false; // If a single bit is not equal to actual key bit, the loop breaks
                                    break;
                                }
                            }
                            if (keyEquals) // If the key created equals the original key, then keyEquals will still be true after the loop
                            {
                                outKey = kk2; // the key generated is equal to actual key so we store it in the outKey variable and break the loop of guess bits.
                                finalGuessbits = guess[j]; // the guess bits which played part in the generation of actual key
                                caseUsed = "Case = 2c";
                                break;
                            }
                        }

                        if (guess[j][0] != '1') // for case 2, where B register is non-zero, we need 22 guess bits, which means we can skip the cases after the bit at index 0 becomes 1 in guess bits
                        {
                            k1 = new int[22];
                            // we start the loop at position 1 because we created above a table for 23 possible bit sequence of free bits
                            // and we only need 22 bits for case 2
                            for (int k = 1; k < 23; k++)
                                k1[k - 1] = guess[j][k] == '0' ? 0 : 1;
                            // calculate the secret key bits using the IV and the guessed bits (and the formulas for scenario 2) 
                            int[] kk3 = Scenario2(k1, iv);
                            keyEquals = true;
                            for (int i = 0; i < 64; i++)
                            {
                                if (!(kk3[i] == key[i]))
                                {
                                    keyEquals = false; // If a single bit is not equal to actual key bit, the loop breaks
                                    break;
                                }
                            }
                            if (keyEquals) // If the key created equals the original key, then keyEquals will still be true after the loop
                            {
                                outKey = kk3; // the key generated is equal to actual key so we store it in the outKey variable and break the loop of guess bits.
                                finalGuessbits = guess[j]; // the guess bits which played part in the generation of actual key
                                caseUsed = "Case = 2b";
                                break;
                            }
                        }

                        // for case 1, where C register is non-zero, we need 23 guess bits
                        k1 = new int[23];
                        // here the loop starts normally at position 0 because we created above a table for 23 possible bit sequence of free bits
                        // and we need all of them for case 1
                        for (int k = 0; k < 23; k++)
                            k1[k] = guess[j][k] == '0' ? 0 : 1;
                        // calculate the secret key bits using the IV and the guessed bits (and the formulas for scenario 1) 
                        int[] kk4 = Scenario1(k1, iv);
                        keyEquals = true;
                        for (int i = 0; i < 64; i++)
                        {
                            if (!(kk4[i] == key[i]))
                            {
                                keyEquals = false; // If a single bit is not equal to actual key bit, the loop breaks
                                break;
                            }
                        }
                        if (keyEquals) // If the key created equals the original key, then keyEquals will still be true after the loop
                        {
                            outKey = kk4; // the key generated is equal to actual key so we store it in the outKey variable and break the loop of guess bits.
                            finalGuessbits = guess[j]; // the guess bits which played part in the generation of actual key
                            caseUsed = "Case = 2a";
                            break;
                        }
                    }
                }
                else
                    // if none of the above criterias meet,
                    // we don't guess free bits to generate the key, we are in the scenario 0--case 1 (only IV is used)
                    caseUsed = "Case 1";

                // if key generated is not equal to the original key
                if (keyEquals == false)
                {
                    GuiLogMessage("Key not found", NotificationLevel.Info);
                    OutputKey = "";
                    GuessKeyBits = "";
                }
                else
                { // if the generated key is equal to the original key,
                  //  the case and the final guess bits are appearing in the screen
                    string resultKey = "";
                    for (int i = 0; i < 64; i++)
                        resultKey += outKey[i];

                    List<Byte> byteList = new List<Byte>();
                    for (int i = 0; i < resultKey.Length; i += 8)
                    {
                        byteList.Add(Convert.ToByte(resultKey.Substring(i, 8), 2));
                    }
                    OutputKeyBytes = byteList.ToArray();

                    if (caseUsed.Contains("1"))
                        GuiLogMessage("Only IV used to generate Key", NotificationLevel.Info);
                    else
                    {
                        GuessKeyBits = finalGuessbits;
                        List<Byte> guessbyteList = new List<Byte>();
                        while (finalGuessbits.Length >= 8)
                        {
                            guessbyteList.Add(Convert.ToByte(finalGuessbits.Substring(0, 8), 2));
                            finalGuessbits = finalGuessbits.Remove(0, 8);
                        }
                        if (finalGuessbits.Length != 0)
                        {// in the final guessed bits we add padding bits on the right
                            guessbyteList.Add(Convert.ToByte(finalGuessbits.PadRight(8, '0'), 2));
                            GuiLogMessage("Last byte in guess bytes has " + finalGuessbits.Length + " important bits. Others are padding", NotificationLevel.Info);
                        }
                        GuessKeyBytes = guessbyteList.ToArray();
                    }
                    GuessCase = caseUsed;
                }

            }
            ProgressChanged(1, 1);
        }

        // Case 1 (scenario 0), Where Register A, B & C are all zeros
        // we only need the values of the IV to generate the key
        public int[] Scenario0(int[] v)
        {
            int[] x = new int[64];

            x[0] = (v[0] + v[1] + v[5] + v[6] + v[8] + v[11] + v[18]) % 2;
            x[1] = (v[1] + v[2] + v[6] + v[7] + v[9] + v[12] + v[19]) % 2;
            x[2] = (v[2] + v[3] + v[7] + v[8] + v[10] + v[13] + v[20]) % 2;
            x[3] = (v[3] + v[4] + v[8] + v[9] + v[11] + v[14] + v[21]) % 2;
            x[4] = (v[4] + v[5] + v[9] + v[10] + v[12] + v[15]) % 2;
            x[5] = (v[5] + v[6] + v[10] + v[11] + v[13] + v[16]) % 2;
            x[6] = (v[6] + v[7] + v[11] + v[12] + v[14] + v[17]) % 2;
            x[7] = (v[7] + v[8] + v[12] + v[13] + v[15] + v[18]) % 2;

            x[8] = (v[0] + v[1] + v[5] + v[6] + v[9] + v[11] + v[13] + v[14] + v[16] + v[18] + v[19]) % 2;
            x[9] = (v[1] + v[2] + v[6] + v[7] + v[10] + v[12] + v[14] + v[15] + v[17] + v[19] + v[20]) % 2;
            x[10] = (v[2] + v[3] + v[7] + v[8] + v[11] + v[13] + v[15] + v[16] + v[18] + v[20] + v[21]) % 2;
            x[11] = (v[3] + v[4] + v[8] + v[9] + v[12] + v[14] + v[16] + v[17] + v[19] + v[21]) % 2;
            x[12] = (v[4] + v[5] + v[9] + v[10] + v[13] + v[15] + v[17] + v[18] + v[20]) % 2;
            x[13] = (v[5] + v[6] + v[10] + v[11] + v[14] + v[16] + v[18] + v[19] + v[21]) % 2;

            x[14] = (v[0] + v[1] + v[5] + v[7] + v[8] + v[12] + v[15] + v[17] + v[18] + v[19] + v[20]) % 2;
            x[15] = (v[1] + v[2] + v[6] + v[8] + v[9] + v[13] + v[16] + v[18] + v[19] + v[20] + v[21]) % 2;
            x[16] = (v[2] + v[3] + v[7] + v[9] + v[10] + v[14] + v[17] + v[19] + v[20] + v[21]) % 2;

            x[17] = (v[0] + v[1] + v[3] + v[4] + v[5] + v[6] + v[10] + v[15] + v[20] + v[21]) % 2;
            x[18] = (v[0] + v[2] + v[4] + v[7] + v[8] + v[16] + v[18] + v[21]) % 2;

            x[19] = (v[0] + v[3] + v[6] + v[9] + v[11] + v[17] + v[18] + v[19]) % 2;
            x[20] = (v[1] + v[4] + v[7] + v[10] + v[12] + v[18] + v[19] + v[20]) % 2;
            x[21] = (v[2] + v[5] + v[8] + v[11] + v[13] + v[19] + v[20] + v[21]) % 2;

            x[22] = (v[0] + v[1] + v[3] + v[5] + v[8] + v[9] + v[11] + v[12] + v[14] + v[18] + v[20] + v[21]) % 2;

            x[23] = (v[0] + v[2] + v[4] + v[5] + v[8] + v[9] + v[10] + v[11] + v[12] + v[13] + v[15] + v[18] + v[19] + v[21]) % 2;
            x[24] = (v[1] + v[3] + v[5] + v[6] + v[9] + v[10] + v[11] + v[12] + v[13] + v[14] + v[16] + v[17] + v[20]) % 2;

            x[25] = (v[0] + v[1] + v[2] + v[4] + v[5] + v[7] + v[8] + v[10] + v[12] + v[13] + v[14] + v[15] + v[17] + v[18] + v[20] + v[21]) % 2;
            x[26] = (v[0] + v[2] + v[3] + v[9] + v[13] + v[14] + v[15] + v[16] + v[19] + v[21]) % 2;

            x[27] = (v[0] + v[3] + v[4] + v[5] + v[6] + v[8] + v[10] + v[11] + v[14] + v[15] + v[16] + v[17] + v[18] + v[20]) % 2;
            x[28] = (v[1] + v[4] + v[5] + v[6] + v[7] + v[9] + v[11] + v[12] + v[15] + v[16] + v[17] + v[18] + v[19] + v[21]) % 2;

            x[29] = (v[0] + v[1] + v[2] + v[7] + v[10] + v[11] + v[12] + v[13] + v[16] + v[17] + v[19] + v[20]) % 2;

            x[30] = (v[0] + v[2] + v[3] + v[5] + v[6] + v[12] + v[13] + v[14] + v[17] + v[20] + v[21]) % 2;
            x[31] = (v[1] + v[3] + v[4] + v[6] + v[7] + v[13] + v[14] + v[15] + v[18] + v[21]) % 2;
            x[32] = (v[2] + v[4] + v[5] + v[7] + v[8] + v[14] + v[15] + v[16] + v[19]) % 2;
            x[33] = (v[3] + v[5] + v[6] + v[8] + v[9] + v[15] + v[16] + v[17] + v[20]) % 2;
            x[34] = (v[4] + v[6] + v[7] + v[9] + v[10] + v[16] + v[17] + v[18] + v[21]) % 2;
            x[35] = (v[5] + v[7] + v[8] + v[10] + v[11] + v[17] + v[18] + v[19]) % 2;
            x[36] = (v[6] + v[8] + v[9] + v[11] + v[12] + v[18] + v[19] + v[20]) % 2;

            x[37] = (v[0] + v[1] + v[5] + v[6] + v[7] + v[8] + v[9] + v[10] + v[11] + v[12] + v[13] + v[18] + v[19] + v[20] + v[21]) % 2;
            x[38] = (v[1] + v[2] + v[6] + v[7] + v[8] + v[9] + v[10] + v[11] + v[12] + v[13] + v[14] + v[19] + v[20] + v[21]) % 2;
            x[39] = (v[2] + v[3] + v[7] + v[8] + v[9] + v[10] + v[11] + v[12] + v[13] + v[14] + v[15] + v[20] + v[21]) % 2;

            x[40] = (v[0] + v[1] + v[3] + v[4] + v[5] + v[6] + v[9] + v[10] + v[12] + v[13] + v[14] + v[15] + v[16] + v[18] + v[21]) % 2;
            x[41] = (v[0] + v[2] + v[4] + v[7] + v[8] + v[10] + v[13] + v[14] + v[15] + v[16] + v[17] + v[18] + v[19]) % 2;
            x[42] = (v[1] + v[3] + v[5] + v[8] + v[9] + v[11] + v[14] + v[15] + v[16] + v[17] + v[18] + v[19] + v[20]) % 2;

            x[43] = (v[0] + v[1] + v[2] + v[4] + v[5] + v[8] + v[9] + v[10] + v[11] + v[12] + v[15] + v[16] + v[17] + v[19] + v[20] + v[21]) % 2;
            x[44] = (v[0] + v[2] + v[3] + v[8] + v[9] + v[10] + v[12] + v[13] + v[16] + v[17] + v[20] + v[21]) % 2;
            x[45] = (v[0] + v[3] + v[4] + v[5] + v[6] + v[8] + v[9] + v[10] + v[13] + v[14] + v[17] + v[21]) % 2;

            x[46] = (v[0] + v[4] + v[7] + v[9] + v[10] + v[14] + v[15]) % 2;
            x[47] = (v[1] + v[5] + v[8] + v[10] + v[11] + v[15] + v[16]) % 2;
            x[48] = (v[2] + v[6] + v[9] + v[11] + v[12] + v[16] + v[17]) % 2;

            x[49] = (v[0] + v[1] + v[3] + v[5] + v[6] + v[7] + v[8] + v[10] + v[12] + v[13] + v[17]) % 2;
            x[50] = (v[1] + v[2] + v[4] + v[6] + v[7] + v[8] + v[9] + v[11] + v[13] + v[14] + v[18]) % 2;
            x[51] = (v[2] + v[3] + v[5] + v[7] + v[8] + v[9] + v[10] + v[12] + v[14] + v[15] + v[19]) % 2;
            x[52] = (v[3] + v[4] + v[6] + v[8] + v[9] + v[10] + v[11] + v[13] + v[15] + v[16] + v[20]) % 2;
            x[53] = (v[4] + v[5] + v[7] + v[9] + v[10] + v[11] + v[12] + v[14] + v[16] + v[17] + v[21]) % 2;
            x[54] = (v[5] + v[6] + v[8] + v[10] + v[11] + v[12] + v[13] + v[15] + v[17] + v[18]) % 2;
            x[55] = (v[6] + v[7] + v[9] + v[11] + v[12] + v[13] + v[14] + v[16] + v[18] + v[19]) % 2;

            x[56] = (v[0] + v[1] + v[5] + v[6] + v[7] + v[10] + v[11] + v[12] + v[13] + v[14] + v[15] + v[17] + v[18] + v[19] + v[20]) % 2;
            x[57] = (v[1] + v[2] + v[6] + v[7] + v[8] + v[11] + v[12] + v[13] + v[14] + v[15] + v[16] + v[18] + v[19] + v[20] + v[21]) % 2;
            x[58] = (v[2] + v[3] + v[7] + v[8] + v[9] + v[12] + v[13] + v[14] + v[15] + v[16] + v[17] + v[19] + v[20] + v[21]) % 2;
            x[59] = (v[3] + v[4] + v[8] + v[9] + v[10] + v[13] + v[14] + v[15] + v[16] + v[17] + v[18] + v[20] + v[21]) % 2;

            x[60] = (v[0] + v[1] + v[4] + v[6] + v[8] + v[9] + v[10] + v[14] + v[15] + v[16] + v[17] + v[19] + v[21]) % 2;
            x[61] = (v[0] + v[2] + v[6] + v[7] + v[8] + v[9] + v[10] + v[15] + v[16] + v[17] + v[20]) % 2;
            x[62] = (v[0] + v[3] + v[5] + v[6] + v[7] + v[9] + v[10] + v[16] + v[17] + v[21]) % 2;
            x[63] = (v[0] + v[4] + v[5] + v[7] + v[10] + v[17]) % 2;

            return x;
        }

        // Case 2a (scenario 1), Where Register A & B are all zeros and Register C contains non-zero values
        public int[] Scenario1(int[] k, int[] v)
        {

            int[] x = new int[64];
            Array.Copy(k, 0, x, 41, k.Length);

            x[0] = (x[41] + x[44] + x[46] + x[50] + x[51] + x[52] + x[53] + x[54] + x[56] + x[57] + x[59] + x[60] + x[62] + v[0] + v[1] + v[2] + v[3] + v[4] + v[8] + v[9] + v[10] + v[11] + v[14] + v[15] + v[18] + v[19] + v[20]) % 2;
            x[1] = (x[42] + x[45] + x[47] + x[51] + x[52] + x[53] + x[54] + x[55] + x[57] + x[58] + x[60] + x[61] + x[63] + v[1] + v[2] + v[3] + v[4] + v[5] + v[9] + v[10] + v[11] + v[12] + v[15] + v[16] + v[19] + v[20] + v[21]) % 2;
            x[2] = (x[43] + x[46] + x[48] + x[52] + x[53] + x[54] + x[55] + x[56] + x[58] + x[59] + x[61] + x[62] + v[0] + v[2] + v[3] + v[4] + v[5] + v[6] + v[10] + v[11] + v[12] + v[13] + v[16] + v[17] + v[20] + v[21]) % 2;
            x[3] = (x[44] + x[47] + x[49] + x[53] + x[54] + x[55] + x[56] + x[57] + x[59] + x[60] + x[62] + x[63] + v[1] + v[3] + v[4] + v[5] + v[6] + v[7] + v[11] + v[12] + v[13] + v[14] + v[17] + v[18] + v[21]) % 2;
            x[4] = (x[45] + x[48] + x[50] + x[54] + x[55] + x[56] + x[57] + x[58] + x[60] + x[61] + x[63] + v[0] + v[2] + v[4] + v[5] + v[6] + v[7] + v[8] + v[12] + v[13] + v[14] + v[15] + v[18] + v[19]) % 2;
            x[5] = (x[46] + x[49] + x[51] + x[55] + x[56] + x[57] + x[58] + x[59] + x[61] + x[62] + v[0] + v[1] + v[3] + v[5] + v[6] + v[7] + v[8] + v[9] + v[13] + v[14] + v[15] + v[16] + v[19] + v[20]) % 2;
            x[6] = (x[47] + x[50] + x[52] + x[56] + x[57] + x[58] + x[59] + x[60] + x[62] + x[63] + v[1] + v[2] + v[4] + v[6] + v[7] + v[8] + v[9] + v[10] + v[14] + v[15] + v[16] + v[17] + v[20] + v[21]) % 2;
            x[7] = (x[48] + x[51] + x[53] + x[57] + x[58] + x[59] + x[60] + x[61] + x[63] + v[0] + v[2] + v[3] + v[5] + v[7] + v[8] + v[9] + v[10] + v[11] + v[15] + v[16] + v[17] + v[18] + v[21]) % 2;
            x[8] = (x[49] + x[52] + x[54] + x[58] + x[59] + x[60] + x[61] + x[62] + v[0] + v[1] + v[3] + v[4] + v[6] + v[8] + v[9] + v[10] + v[11] + v[12] + v[16] + v[17] + v[18] + v[19]) % 2;
            x[9] = (x[50] + x[53] + x[55] + x[59] + x[60] + x[61] + x[62] + x[63] + v[1] + v[2] + v[4] + v[5] + v[7] + v[9] + v[10] + v[11] + v[12] + v[13] + v[17] + v[18] + v[19] + v[20]) % 2;
            x[10] = (x[51] + x[54] + x[56] + x[60] + x[61] + x[62] + x[63] + v[0] + v[2] + v[3] + v[5] + v[6] + v[8] + v[10] + v[11] + v[12] + v[13] + v[14] + v[18] + v[19] + v[20] + v[21]) % 2;
            x[11] = (x[52] + x[55] + x[57] + x[61] + x[62] + x[63] + v[0] + v[1] + v[3] + v[4] + v[6] + v[7] + v[9] + v[11] + v[12] + v[13] + v[14] + v[15] + v[19] + v[20] + v[21]) % 2;
            x[12] = (x[53] + x[56] + x[58] + x[62] + x[63] + v[0] + v[1] + v[2] + v[4] + v[5] + v[7] + v[8] + v[10] + v[12] + v[13] + v[14] + v[15] + v[16] + v[20] + v[21]) % 2;
            x[13] = (x[54] + x[57] + x[59] + x[63] + v[0] + v[1] + v[2] + v[3] + v[5] + v[6] + v[8] + v[9] + v[11] + v[13] + v[14] + v[15] + v[16] + v[17] + v[21]) % 2;

            x[14] = (x[41] + x[44] + x[46] + x[50] + x[51] + x[52] + x[53] + x[54] + x[55] + x[56] + x[57] + x[58] + x[59] + x[62] + v[6] + v[7] + v[8] + v[11] + v[12] + v[16] + v[17] + v[19] + v[20]) % 2;
            x[15] = (x[42] + x[45] + x[47] + x[51] + x[52] + x[53] + x[54] + x[55] + x[56] + x[57] + x[58] + x[59] + x[60] + x[63] + v[7] + v[8] + v[9] + v[12] + v[13] + v[17] + v[18] + v[20] + v[21]) % 2;
            x[16] = (x[43] + x[46] + x[48] + x[52] + x[53] + x[54] + x[55] + x[56] + x[57] + x[58] + x[59] + x[60] + x[61] + v[0] + v[8] + v[9] + v[10] + v[13] + v[14] + v[18] + v[19] + v[21]) % 2;

            x[17] = (x[41] + x[46] + x[47] + x[49] + x[50] + x[51] + x[52] + x[55] + x[58] + x[61] + v[0] + v[2] + v[3] + v[4] + v[8] + v[18]) % 2;
            x[18] = (x[41] + x[42] + x[44] + x[46] + x[47] + x[48] + x[54] + x[57] + x[60] + v[0] + v[2] + v[5] + v[8] + v[10] + v[11] + v[14] + v[15] + v[18] + v[20]) % 2;

            x[19] = (x[41] + x[42] + x[43] + x[44] + x[45] + x[46] + x[47] + x[48] + x[49] + x[50] + x[51] + x[52] + x[53] + x[54] + x[55] + x[56] + x[57] + x[58] + x[59] + x[60] + x[61] + x[62] + v[0] + v[2] + v[4] + v[6] + v[8] + v[10] + v[12] + v[14] + v[16] + v[18] + v[20] + v[21]) % 2;
            x[20] = (x[42] + x[43] + x[44] + x[45] + x[46] + x[47] + x[48] + x[49] + x[50] + x[51] + x[52] + x[53] + x[54] + x[55] + x[56] + x[57] + x[58] + x[59] + x[60] + x[61] + x[62] + x[63] + v[1] + v[3] + v[5] + v[7] + v[9] + v[11] + v[13] + v[15] + v[17] + v[19] + v[21]) % 2;

            x[21] = (x[41] + x[43] + x[45] + x[47] + x[48] + x[49] + x[55] + x[58] + x[61] + x[63] + v[1] + v[3] + v[6] + v[9] + v[11] + v[12] + v[15] + v[16] + v[19]) % 2;

            x[22] = (x[41] + x[42] + x[48] + x[49] + x[51] + x[52] + x[53] + x[54] + x[57] + x[60] + v[1] + v[3] + v[7] + v[8] + v[9] + v[11] + v[12] + v[13] + v[14] + v[15] + v[16] + v[17] + v[18] + v[19]) % 2;
            x[23] = (x[42] + x[43] + x[49] + x[50] + x[52] + x[53] + x[54] + x[55] + x[58] + x[61] + v[2] + v[4] + v[8] + v[9] + v[10] + v[12] + v[13] + v[14] + v[15] + v[16] + v[17] + v[18] + v[19] + v[20]) % 2;
            x[24] = (x[43] + x[44] + x[50] + x[51] + x[53] + x[54] + x[55] + x[56] + x[59] + x[62] + v[3] + v[5] + v[9] + v[10] + v[11] + v[13] + v[14] + v[15] + v[16] + v[17] + v[18] + v[19] + v[20] + v[21]) % 2;
            x[25] = (x[44] + x[45] + x[51] + x[52] + x[54] + x[55] + x[56] + x[57] + x[60] + x[63] + v[4] + v[6] + v[10] + v[11] + v[12] + v[14] + v[15] + v[16] + v[17] + v[18] + v[19] + v[20] + v[21]) % 2;
            x[26] = (x[45] + x[46] + x[52] + x[53] + x[55] + x[56] + x[57] + x[58] + x[61] + v[0] + v[5] + v[7] + v[11] + v[12] + v[13] + v[15] + v[16] + v[17] + v[18] + v[19] + v[20] + v[21]) % 2;
            x[27] = (x[46] + x[47] + x[53] + x[54] + x[56] + x[57] + x[58] + x[59] + x[62] + v[1] + v[6] + v[8] + v[12] + v[13] + v[14] + v[16] + v[17] + v[18] + v[19] + v[20] + v[21]) % 2;
            x[28] = (x[47] + x[48] + x[54] + x[55] + x[57] + x[58] + x[59] + x[60] + x[63] + v[2] + v[7] + v[9] + v[13] + v[14] + v[15] + v[17] + v[18] + v[19] + v[20] + v[21]) % 2;
            x[29] = (x[48] + x[49] + x[55] + x[56] + x[58] + x[59] + x[60] + x[61] + v[0] + v[3] + v[8] + v[10] + v[14] + v[15] + v[16] + v[18] + v[19] + v[20] + v[21]) % 2;
            x[30] = (x[49] + x[50] + x[56] + x[57] + x[59] + x[60] + x[61] + x[62] + v[1] + v[4] + v[9] + v[11] + v[15] + v[16] + v[17] + v[19] + v[20] + v[21]) % 2;
            x[31] = (x[50] + x[51] + x[57] + x[58] + x[60] + x[61] + x[62] + x[63] + v[2] + v[5] + v[10] + v[12] + v[16] + v[17] + v[18] + v[20] + v[21]) % 2;
            x[32] = (x[51] + x[52] + x[58] + x[59] + x[61] + x[62] + x[63] + v[0] + v[3] + v[6] + v[11] + v[13] + v[17] + v[18] + v[19] + v[21]) % 2;
            x[33] = (x[52] + x[53] + x[59] + x[60] + x[62] + x[63] + v[0] + v[1] + v[4] + v[7] + v[12] + v[14] + v[18] + v[19] + v[20]) % 2;
            x[34] = (x[53] + x[54] + x[60] + x[61] + x[63] + v[0] + v[1] + v[2] + v[5] + v[8] + v[13] + v[15] + v[19] + v[20] + v[21]) % 2;

            x[35] = (x[41] + x[44] + x[46] + x[50] + x[51] + x[52] + x[53] + x[55] + x[56] + x[57] + x[59] + x[60] + x[61] + v[4] + v[6] + v[8] + v[10] + v[11] + v[15] + v[16] + v[18] + v[19] + v[21]) % 2;

            x[36] = (x[41] + x[42] + x[44] + x[45] + x[46] + x[47] + x[50] + x[58] + x[59] + x[61] + v[0] + v[1] + v[2] + v[3] + v[4] + v[5] + v[7] + v[8] + v[10] + v[12] + v[14] + v[15] + v[16] + v[17] + v[18]) % 2;
            x[37] = (x[42] + x[43] + x[45] + x[46] + x[47] + x[48] + x[51] + x[59] + x[60] + x[62] + v[1] + v[2] + v[3] + v[4] + v[5] + v[6] + v[8] + v[9] + v[11] + v[13] + v[15] + v[16] + v[17] + v[18] + v[19]) % 2;

            x[38] = (x[41] + x[43] + x[47] + x[48] + x[49] + x[50] + x[51] + x[53] + x[54] + x[56] + x[57] + x[59] + x[61] + x[62] + x[63] + v[0] + v[1] + v[5] + v[6] + v[7] + v[8] + v[11] + v[12] + v[15] + v[16] + v[17]) % 2;
            x[39] = (x[42] + x[44] + x[48] + x[49] + x[50] + x[51] + x[52] + x[54] + x[55] + x[57] + x[58] + x[60] + x[62] + x[63] + v[0] + v[1] + v[2] + v[6] + v[7] + v[8] + v[9] + v[12] + v[13] + v[16] + v[17] + v[18]) % 2;
            x[40] = (x[43] + x[45] + x[49] + x[50] + x[51] + x[52] + x[53] + x[55] + x[56] + x[58] + x[59] + x[61] + x[63] + v[0] + v[1] + v[2] + v[3] + v[7] + v[8] + v[9] + v[10] + v[13] + v[14] + v[17] + v[18] + v[19]) % 2;

            return x;
        }
        // Case 2b-scenario 2, Where Register A & C are all zeros and Register B contains non-zero values
        public int[] Scenario2(int[] k, int[] v)
        {
            int[] x = new int[64];
            Array.Copy(k, 0, x, 42, k.Length);


            x[0] = (x[42] + x[44] + x[47] + x[49] + x[50] + x[51] + x[53] + x[54] + x[60] + x[61] + v[0] + v[4] + v[5] + v[7] + v[12] + v[14] + v[17] + v[19] + v[20] + v[21]) % 2;
            x[1] = (x[43] + x[45] + x[48] + x[50] + x[51] + x[52] + x[54] + x[55] + x[61] + x[62] + v[1] + v[5] + v[6] + v[8] + v[13] + v[15] + v[18] + v[20] + v[21]) % 2;
            x[2] = (x[44] + x[46] + x[49] + x[51] + x[52] + x[53] + x[55] + x[56] + x[62] + x[63] + v[2] + v[6] + v[7] + v[9] + v[14] + v[16] + v[19] + v[21]) % 2;
            x[3] = (x[45] + x[47] + x[50] + x[52] + x[53] + x[54] + x[56] + x[57] + x[63] + v[0] + v[3] + v[7] + v[8] + v[10] + v[15] + v[17] + v[20]) % 2;
            x[4] = (x[46] + x[48] + x[51] + x[53] + x[54] + x[55] + x[57] + x[58] + v[0] + v[1] + v[4] + v[8] + v[9] + v[11] + v[16] + v[18] + v[21]) % 2;
            x[5] = (x[47] + x[49] + x[52] + x[54] + x[55] + x[56] + x[58] + x[59] + v[1] + v[2] + v[5] + v[9] + v[10] + v[12] + v[17] + v[19]) % 2;
            x[6] = (x[48] + x[50] + x[53] + x[55] + x[56] + x[57] + x[59] + x[60] + v[2] + v[3] + v[6] + v[10] + v[11] + v[13] + v[18] + v[20]) % 2;
            x[7] = (x[49] + x[51] + x[54] + x[56] + x[57] + x[58] + x[60] + x[61] + v[3] + v[4] + v[7] + v[11] + v[12] + v[14] + v[19] + v[21]) % 2;

            x[8] = (x[42] + x[44] + x[47] + x[49] + x[51] + x[52] + x[53] + x[54] + x[55] + x[57] + x[58] + x[59] + x[60] + x[62] + v[0] + v[7] + v[8] + v[13] + v[14] + v[15] + v[17] + v[19] + v[21]) % 2;
            x[9] = (x[43] + x[45] + x[48] + x[50] + x[52] + x[53] + x[54] + x[55] + x[56] + x[58] + x[59] + x[60] + x[61] + x[63] + v[1] + v[8] + v[9] + v[14] + v[15] + v[16] + v[18] + v[20]) % 2;
            x[10] = (x[44] + x[46] + x[49] + x[51] + x[53] + x[54] + x[55] + x[56] + x[57] + x[59] + x[60] + x[61] + x[62] + v[0] + v[2] + v[9] + v[10] + v[15] + v[16] + v[17] + v[19] + v[21]) % 2;
            x[11] = (x[45] + x[47] + x[50] + x[52] + x[54] + x[55] + x[56] + x[57] + x[58] + x[60] + x[61] + x[62] + x[63] + v[1] + v[3] + v[10] + v[11] + v[16] + v[17] + v[18] + v[20]) % 2;
            x[12] = (x[46] + x[48] + x[51] + x[53] + x[55] + x[56] + x[57] + x[58] + x[59] + x[61] + x[62] + x[63] + v[0] + v[2] + v[4] + v[11] + v[12] + v[17] + v[18] + v[19] + v[21]) % 2;
            x[13] = (x[47] + x[49] + x[52] + x[54] + x[56] + x[57] + x[58] + x[59] + x[60] + x[62] + x[63] + v[0] + v[1] + v[3] + v[5] + v[12] + v[13] + v[18] + v[19] + v[20]) % 2;

            x[14] = (x[42] + x[44] + x[47] + x[48] + x[49] + x[51] + x[54] + x[55] + x[57] + x[58] + x[59] + x[63] + v[1] + v[2] + v[5] + v[6] + v[7] + v[12] + v[13] + v[17]) % 2;
            x[15] = (x[43] + x[45] + x[48] + x[49] + x[50] + x[52] + x[55] + x[56] + x[58] + x[59] + x[60] + v[0] + v[2] + v[3] + v[6] + v[7] + v[8] + v[13] + v[14] + v[18]) % 2;
            x[16] = (x[44] + x[46] + x[49] + x[50] + x[51] + x[53] + x[56] + x[57] + x[59] + x[60] + x[61] + v[1] + v[3] + v[4] + v[7] + v[8] + v[9] + v[14] + v[15] + v[19]) % 2;

            x[17] = (x[42] + x[44] + x[45] + x[49] + x[52] + x[53] + x[57] + x[58] + x[62] + v[0] + v[2] + v[7] + v[8] + v[9] + v[10] + v[12] + v[14] + v[15] + v[16] + v[17] + v[19] + v[21]) % 2;

            x[18] = (x[42] + x[43] + x[44] + x[45] + x[46] + x[47] + x[49] + x[51] + x[58] + x[59] + x[60] + x[61] + x[63] + v[0] + v[1] + v[3] + v[4] + v[5] + v[7] + v[8] + v[9] + v[10] + v[11] + v[12] + v[13] + v[14] + v[15] + v[16] + v[18] + v[19] + v[21]) % 2;

            x[19] = (x[42] + x[43] + x[45] + x[46] + x[48] + x[49] + x[51] + x[52] + x[53] + x[54] + x[59] + x[62] + v[1] + v[2] + v[6] + v[7] + v[8] + v[9] + v[10] + v[11] + v[13] + v[15] + v[16] + v[21]) % 2;
            x[20] = (x[43] + x[44] + x[46] + x[47] + x[49] + x[50] + x[52] + x[53] + x[54] + x[55] + x[60] + x[63] + v[2] + v[3] + v[7] + v[8] + v[9] + v[10] + v[11] + v[12] + v[14] + v[16] + v[17]) % 2;

            x[21] = (x[42] + x[45] + x[48] + x[49] + x[55] + x[56] + x[60] + v[3] + v[5] + v[7] + v[8] + v[9] + v[10] + v[11] + v[13] + v[14] + v[15] + v[18] + v[19] + v[20] + v[21]) % 2;
            x[22] = (x[43] + x[46] + x[49] + x[50] + x[56] + x[57] + x[61] + v[4] + v[6] + v[8] + v[9] + v[10] + v[11] + v[12] + v[14] + v[15] + v[16] + v[19] + v[20] + v[21]) % 2;

            x[23] = (x[42] + x[49] + x[53] + x[54] + x[57] + x[58] + x[60] + x[61] + x[62] + v[0] + v[4] + v[9] + v[10] + v[11] + v[13] + v[14] + v[15] + v[16] + v[19]) % 2;
            x[24] = (x[43] + x[50] + x[54] + x[55] + x[58] + x[59] + x[61] + x[62] + x[63] + v[1] + v[5] + v[10] + v[11] + v[12] + v[14] + v[15] + v[16] + v[17] + v[20]) % 2;

            x[25] = (x[42] + x[47] + x[49] + x[50] + x[53] + x[54] + x[55] + x[56] + x[59] + x[61] + x[62] + x[63] + v[2] + v[4] + v[5] + v[6] + v[7] + v[11] + v[13] + v[14] + v[15] + v[16] + v[18] + v[19] + v[20]) % 2;
            x[26] = (x[42] + x[43] + x[44] + x[47] + x[48] + x[49] + x[53] + x[55] + x[56] + x[57] + x[61] + x[62] + x[63] + v[3] + v[4] + v[6] + v[8] + v[15] + v[16]) % 2;

            x[27] = (x[42] + x[43] + x[45] + x[47] + x[48] + x[51] + x[53] + x[56] + x[57] + x[58] + x[60] + x[61] + x[62] + x[63] + v[9] + v[12] + v[14] + v[16] + v[19] + v[20] + v[21]) % 2;
            x[28] = (x[43] + x[44] + x[46] + x[48] + x[49] + x[52] + x[54] + x[57] + x[58] + x[59] + x[61] + x[62] + x[63] + v[0] + v[10] + v[13] + v[15] + v[17] + v[20] + v[21]) % 2;
            x[29] = (x[44] + x[45] + x[47] + x[49] + x[50] + x[53] + x[55] + x[58] + x[59] + x[60] + x[62] + x[63] + v[0] + v[1] + v[11] + v[14] + v[16] + v[18] + v[21]) % 2;
            x[30] = (x[45] + x[46] + x[48] + x[50] + x[51] + x[54] + x[56] + x[59] + x[60] + x[61] + x[63] + v[0] + v[1] + v[2] + v[12] + v[15] + v[17] + v[19]) % 2;
            x[31] = (x[46] + x[47] + x[49] + x[51] + x[52] + x[55] + x[57] + x[60] + x[61] + x[62] + v[0] + v[1] + v[2] + v[3] + v[13] + v[16] + v[18] + v[20]) % 2;
            x[32] = (x[47] + x[48] + x[50] + x[52] + x[53] + x[56] + x[58] + x[61] + x[62] + x[63] + v[1] + v[2] + v[3] + v[4] + v[14] + v[17] + v[19] + v[21]) % 2;
            x[33] = (x[48] + x[49] + x[51] + x[53] + x[54] + x[57] + x[59] + x[62] + x[63] + v[0] + v[2] + v[3] + v[4] + v[5] + v[15] + v[18] + v[20]) % 2;
            x[34] = (x[49] + x[50] + x[52] + x[54] + x[55] + x[58] + x[60] + x[63] + v[0] + v[1] + v[3] + v[4] + v[5] + v[6] + v[16] + v[19] + v[21]) % 2;

            x[35] = (x[42] + x[44] + x[47] + x[49] + x[54] + x[55] + x[56] + x[59] + x[60] + v[1] + v[2] + v[6] + v[12] + v[14] + v[19] + v[21]) % 2;
            x[36] = (x[42] + x[43] + x[44] + x[45] + x[47] + x[48] + x[49] + x[51] + x[53] + x[54] + x[55] + x[56] + x[57] + v[0] + v[2] + v[3] + v[4] + v[5] + v[12] + v[13] + v[14] + v[15] + v[17] + v[19] + v[21]) % 2;
            x[37] = (x[42] + x[43] + x[45] + x[46] + x[47] + x[48] + x[51] + x[52] + x[53] + x[55] + x[56] + x[57] + x[58] + x[60] + x[61] + v[0] + v[1] + v[3] + v[6] + v[7] + v[12] + v[13] + v[15] + v[16] + v[17] + v[18] + v[19] + v[21]) % 2;

            x[38] = (x[42] + x[43] + x[46] + x[48] + x[50] + x[51] + x[52] + x[56] + x[57] + x[58] + x[59] + x[60] + x[62] + v[0] + v[1] + v[2] + v[5] + v[8] + v[12] + v[13] + v[16] + v[18] + v[21]) % 2;
            x[39] = (x[43] + x[44] + x[47] + x[49] + x[51] + x[52] + x[53] + x[57] + x[58] + x[59] + x[60] + x[61] + x[63] + v[1] + v[2] + v[3] + v[6] + v[9] + v[13] + v[14] + v[17] + v[19]) % 2;

            x[40] = (x[42] + x[45] + x[47] + x[48] + x[49] + x[51] + x[52] + x[58] + x[59] + x[62] + v[2] + v[3] + v[5] + v[10] + v[12] + v[15] + v[17] + v[18] + v[19] + v[21]) % 2;
            x[41] = (x[43] + x[46] + x[48] + x[49] + x[50] + x[52] + x[53] + x[59] + x[60] + x[63] + v[3] + v[4] + v[6] + v[11] + v[13] + v[16] + v[18] + v[19] + v[20]) % 2;

            return x;
        }

        // Case 2c--scenario 3, Where Register B & C are all zeros and Register A contains non-zero values
        public int[] Scenario3(int[] k, int[] v)
        {

            int[] x = new int[64];
            Array.Copy(k, 0, x, 45, k.Length);


            x[0] = (x[45] + x[48] + x[51] + x[54] + x[57] + x[61] + x[63] + v[5] + v[13] + v[14] + v[16] + v[17] + v[18]) % 2;
            x[1] = (x[46] + x[49] + x[52] + x[55] + x[58] + x[62] + v[0] + v[6] + v[14] + v[15] + v[17] + v[18] + v[19]) % 2;
            x[2] = (x[47] + x[50] + x[53] + x[56] + x[59] + x[63] + v[1] + v[7] + v[15] + v[16] + v[18] + v[19] + v[20]) % 2;
            x[3] = (x[48] + x[51] + x[54] + x[57] + x[60] + v[0] + v[2] + v[8] + v[16] + v[17] + v[19] + v[20] + v[21]) % 2;
            x[4] = (x[49] + x[52] + x[55] + x[58] + x[61] + v[1] + v[3] + v[9] + v[17] + v[18] + v[20] + v[21]) % 2;
            x[5] = (x[50] + x[53] + x[56] + x[59] + x[62] + v[2] + v[4] + v[10] + v[18] + v[19] + v[21]) % 2;
            x[6] = (x[51] + x[54] + x[57] + x[60] + x[63] + v[3] + v[5] + v[11] + v[19] + v[20]) % 2;
            x[7] = (x[52] + x[55] + x[58] + x[61] + v[0] + v[4] + v[6] + v[12] + v[20] + v[21]) % 2;

            x[8] = (x[45] + x[48] + x[51] + x[53] + x[54] + x[56] + x[57] + x[59] + x[61] + x[62] + x[63] + v[1] + v[7] + v[14] + v[16] + v[17] + v[18] + v[21]) % 2;
            x[9] = (x[46] + x[49] + x[52] + x[54] + x[55] + x[57] + x[58] + x[60] + x[62] + x[63] + v[0] + v[2] + v[8] + v[15] + v[17] + v[18] + v[19]) % 2;
            x[10] = (x[47] + x[50] + x[53] + x[55] + x[56] + x[58] + x[59] + x[61] + x[63] + v[0] + v[1] + v[3] + v[9] + v[16] + v[18] + v[19] + v[20]) % 2;
            x[11] = (x[48] + x[51] + x[54] + x[56] + x[57] + x[59] + x[60] + x[62] + v[0] + v[1] + v[2] + v[4] + v[10] + v[17] + v[19] + v[20] + v[21]) % 2;
            x[12] = (x[49] + x[52] + x[55] + x[57] + x[58] + x[60] + x[61] + x[63] + v[1] + v[2] + v[3] + v[5] + v[11] + v[18] + v[20] + v[21]) % 2;
            x[13] = (x[50] + x[53] + x[56] + x[58] + x[59] + x[61] + x[62] + v[0] + v[2] + v[3] + v[4] + v[6] + v[12] + v[19] + v[21]) % 2;
            x[14] = (x[51] + x[54] + x[57] + x[59] + x[60] + x[62] + x[63] + v[1] + v[3] + v[4] + v[5] + v[7] + v[13] + v[20]) % 2;
            x[15] = (x[52] + x[55] + x[58] + x[60] + x[61] + x[63] + v[0] + v[2] + v[4] + v[5] + v[6] + v[8] + v[14] + v[21]) % 2;
            x[16] = (x[53] + x[56] + x[59] + x[61] + x[62] + v[0] + v[1] + v[3] + v[5] + v[6] + v[7] + v[9] + v[15]) % 2;
            x[17] = (x[54] + x[57] + x[60] + x[62] + x[63] + v[1] + v[2] + v[4] + v[6] + v[7] + v[8] + v[10] + v[16]) % 2;
            x[18] = (x[55] + x[58] + x[61] + x[63] + v[0] + v[2] + v[3] + v[5] + v[7] + v[8] + v[9] + v[11] + v[17]) % 2;
            x[19] = (x[56] + x[59] + x[62] + v[0] + v[1] + v[3] + v[4] + v[6] + v[8] + v[9] + v[10] + v[12] + v[18]) % 2;
            x[20] = (x[57] + x[60] + x[63] + v[1] + v[2] + v[4] + v[5] + v[7] + v[9] + v[10] + v[11] + v[13] + v[19]) % 2;
            x[21] = (x[58] + x[61] + v[0] + v[2] + v[3] + v[5] + v[6] + v[8] + v[10] + v[11] + v[12] + v[14] + v[20]) % 2;
            x[22] = (x[59] + x[62] + v[1] + v[3] + v[4] + v[6] + v[7] + v[9] + v[11] + v[12] + v[13] + v[15] + v[21]) % 2;

            x[23] = (x[45] + x[48] + x[51] + x[54] + x[57] + x[60] + x[61] + v[2] + v[4] + v[7] + v[8] + v[10] + v[12] + v[17] + v[18]) % 2;
            x[24] = (x[46] + x[49] + x[52] + x[55] + x[58] + x[61] + x[62] + v[3] + v[5] + v[8] + v[9] + v[11] + v[13] + v[18] + v[19]) % 2;
            x[25] = (x[47] + x[50] + x[53] + x[56] + x[59] + x[62] + x[63] + v[4] + v[6] + v[9] + v[10] + v[12] + v[14] + v[19] + v[20]) % 2;
            x[26] = (x[48] + x[51] + x[54] + x[57] + x[60] + x[63] + v[0] + v[5] + v[7] + v[10] + v[11] + v[13] + v[15] + v[20] + v[21]) % 2;
            x[27] = (x[49] + x[52] + x[55] + x[58] + x[61] + v[0] + v[1] + v[6] + v[8] + v[11] + v[12] + v[14] + v[16] + v[21]) % 2;
            x[28] = (x[50] + x[53] + x[56] + x[59] + x[62] + v[1] + v[2] + v[7] + v[9] + v[12] + v[13] + v[15] + v[17]) % 2;

            x[29] = (x[45] + x[48] + x[60] + x[61] + v[2] + v[3] + v[5] + v[8] + v[10] + v[17]) % 2;

            x[30] = (x[45] + x[46] + x[48] + x[49] + x[51] + x[54] + x[57] + x[62] + x[63] + v[3] + v[4] + v[5] + v[6] + v[9] + v[11] + v[13] + v[14] + v[16] + v[17]) % 2;
            x[31] = (x[46] + x[47] + x[49] + x[50] + x[52] + x[55] + x[58] + x[63] + v[0] + v[4] + v[5] + v[6] + v[7] + v[10] + v[12] + v[14] + v[15] + v[17] + v[18]) % 2;
            x[32] = (x[47] + x[48] + x[50] + x[51] + x[53] + x[56] + x[59] + v[0] + v[1] + v[5] + v[6] + v[7] + v[8] + v[11] + v[13] + v[15] + v[16] + v[18] + v[19]) % 2;
            x[33] = (x[48] + x[49] + x[51] + x[52] + x[54] + x[57] + x[60] + v[1] + v[2] + v[6] + v[7] + v[8] + v[9] + v[12] + v[14] + v[16] + v[17] + v[19] + v[20]) % 2;
            x[34] = (x[49] + x[50] + x[52] + x[53] + x[55] + x[58] + x[61] + v[2] + v[3] + v[7] + v[8] + v[9] + v[10] + v[13] + v[15] + v[17] + v[18] + v[20] + v[21]) % 2;
            x[35] = (x[50] + x[51] + x[53] + x[54] + x[56] + x[59] + x[62] + v[3] + v[4] + v[8] + v[9] + v[10] + v[11] + v[14] + v[16] + v[18] + v[19] + v[21]) % 2;
            x[36] = (x[51] + x[52] + x[54] + x[55] + x[57] + x[60] + x[63] + v[4] + v[5] + v[9] + v[10] + v[11] + v[12] + v[15] + v[17] + v[19] + v[20]) % 2;
            x[37] = (x[52] + x[53] + x[55] + x[56] + x[58] + x[61] + v[0] + v[5] + v[6] + v[10] + v[11] + v[12] + v[13] + v[16] + v[18] + v[20] + v[21]) % 2;
            x[38] = (x[53] + x[54] + x[56] + x[57] + x[59] + x[62] + v[1] + v[6] + v[7] + v[11] + v[12] + v[13] + v[14] + v[17] + v[19] + v[21]) % 2;
            x[39] = (x[54] + x[55] + x[57] + x[58] + x[60] + x[63] + v[2] + v[7] + v[8] + v[12] + v[13] + v[14] + v[15] + v[18] + v[20]) % 2;
            x[40] = (x[55] + x[56] + x[58] + x[59] + x[61] + v[0] + v[3] + v[8] + v[9] + v[13] + v[14] + v[15] + v[16] + v[19] + v[21]) % 2;
            x[41] = (x[56] + x[57] + x[59] + x[60] + x[62] + v[1] + v[4] + v[9] + v[10] + v[14] + v[15] + v[16] + v[17] + v[20]) % 2;

            x[42] = (x[45] + x[48] + x[51] + x[54] + x[58] + x[60] + v[2] + v[10] + v[11] + v[13] + v[14] + v[15] + v[21]) % 2;
            x[43] = (x[46] + x[49] + x[52] + x[55] + x[59] + x[61] + v[3] + v[11] + v[12] + v[14] + v[15] + v[16]) % 2;
            x[44] = (x[47] + x[50] + x[53] + x[56] + x[60] + x[62] + v[4] + v[12] + v[13] + v[15] + v[16] + v[17]) % 2;

            return x;
        }

        public void Dispose()
        {
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
    }
}
