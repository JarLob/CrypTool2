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
   
 * Instructions:
 * numRegisters     = The number of registers to use
 * maxRegLens       = The max register length for each register
 * regIndexes       = The clocking bits for each register
 * polynomialsArray = The polynomials to use for each register 
 * sourceArray      = Random binary bits (random bits are used for both key and IV)


*/

using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.A5
{

    [Author("Kristina Hita", "khita@mail.uni-mannheim.de", "Universität Mannheim", "https://www.uni-mannheim.de/1/english/university/profile/")]
    [PluginInfo("A5.Properties.Resources", "PluginCaption", "PluginTooltip", "A5/DetailedDescription/doc.xml", new[] { "A5/gsm icon.png" })]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]

    public class A5
    {
        public bool dbMode; //Debug mode

        private int nRegisters; //number of registers
        private int[] mRegLens; //max register lengths
        private int[] sArray; //source array
        private int[] rIndexes; //register indexes (clocking bits)
        private string[] pArray; //polynomials
        private int[][] registers; //registers (we are using this to create a table with the registers space for all 3 registers (1st row --> register 1 ; 2nd row --> register 2 etc)

        private int[][] uExponents; //exponents being used


        public void Dispose()
        {
            nRegisters = 0;
            mRegLens = null;
            sArray = null;
            pArray = null;
            rIndexes = null;

        }


        ///* Main method for launching the cipher */
        //public void Execute()
        //{
        //    ProgressChanged(0, 1);

        //    if (!checkParameters()) return;

        //    init();

        //    OutputData = encrypt(message);

        //    ProgressChanged(1, 1);
        //}


        /* --- Setting properties --- */
        public int numRegisters
        {
            get
            { return nRegisters; }
            set
            { nRegisters = value; }
        }

        public int[] maxRegLens
        {
            get
            { return mRegLens; }
            set
            { mRegLens = value; }
        }

        public int[] sourceArray
        {
            get
            { return sArray; }
            set
            { sArray = value; }
        }

        public int[] regIndexes
        {
            get
            { return rIndexes; }
            set
            { rIndexes = value; }
        }

        public string[] polynomialsArray
        {
            get
            { return pArray; }
            set
            { pArray = value; }
        }

        public int[][] Registers
        {
            get
            { return registers; }
            set
            { registers = value; }
        }

        /* --- Begin methods ---*/
        private void slowText(string message)
        {
            foreach (char c in message.ToCharArray())
            {
                Console.Write(c);
                System.Threading.Thread.Sleep(60);
            }

            Console.WriteLine();
        }

        public void Intro()
        // this was just added to show gui on console, so it can omitted if we are showing result on cryptool

        {
            string message = "#################################################################\n";
            message += "#                      A5/1 Implementation                      #\n";
            message += "#                                                               #\n";
            message += "# Information: http://en.wikipedia.org/wiki/A5/1                #\n";
            message += "# Released:    24th October 2008                                #\n";
            message += "# App Options: -d [NumOfLoops] (Debugging mode)                 #\n";
            message += "#                                                               #\n";
            message += "# Written By: Brett Gervasoni (brett.gervasoni [at] gmail.com)  #\n";
            message += "#################################################################\n";

            Console.WriteLine(message);

            string matrix = "Now you will see how deep the rabit hole really goes...";
            slowText(matrix);

            System.Threading.Thread.Sleep(2500);
        }


        // This function calculates the total length of all registers
        // We are going to use this value to find whether the source array have enough data to fill
        // All the registers should be filled completely with the source array values

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



        // Fill register with values until you reach the maximum count

        private int[] RegisterFill(int offset, int regNum)
        {
            int[] outArray = new int[regNum];

            for (int i = 0; i < regNum; i++)
            {
                outArray[i] = sArray[offset + i];
            }

            return outArray;
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





        //We are using source array(sArray) for both key and IV to fill the registers (nRegisters indicates the number of registers).

        public int[][] CreateRegisters()
        {
            int[][] filledRegisters = new int[nRegisters][];
            int offset = 0;

            //Does source array have enough data to fill? The register should be filled completely with the source array values.

            if (GetMaxRegLensTotal() <= sArray.Length)
            {
                for (int i = 0; i < nRegisters; i++)
                {
                    filledRegisters[i] = RegisterFill(offset, mRegLens[i]);
                    offset += mRegLens[i];
                }
            }


            //This is just an additional class called "debug mode". If debug mode is selected it will show step by step processes.

            uExponents = PolySelection();

            if (dbMode)
            {
                //Exponents in use
                int counter = 0;

                Console.WriteLine("[exponents]");
                foreach (int[] set in uExponents)
                {
                    Console.WriteLine("set: {0}", counter.ToString());

                    foreach (int exp in set)
                        Console.Write(exp.ToString() + " ");

                    Console.WriteLine();
                    counter++;
                }

                Console.WriteLine("[/exponents]");
            }

            return filledRegisters;
        }


        // This function returns keystream values after the registers' values are being XORed 

        public int GetOutValue()
        {
            int[] vToXor = new int[registers.Length];
            int outValue = 0;

            for (int i = 0; i < registers.Length; i++)
                vToXor[i] = registers[i][0];

            outValue = XorRegValues(vToXor);

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






        // feedback value-- the value which is xor of key/iv and 13th , 16th , 17th and 18th bit in case of first register of A5/1
        // regts--register to shift, the one that will play its role i.e the one with majority bit 1

        public void RegisterShift(int[] regTS)
        {
            int[] shiftedElements = new int[regTS.Length];
            int[] regTSFBV = GetFeedbackValues(regTS);

            if (dbMode)
            {
                Console.WriteLine("[regTS]:");
                foreach (int v in regTS)
                    Console.Write(v.ToString() + " ");
                Console.WriteLine("\n[/regTS]:");

                Console.WriteLine("[regTSFBV]:");
                foreach (int v in regTSFBV)
                    Console.Write(v.ToString() + " ");
                Console.WriteLine("\n[/regTSFBV]:");
            }

            for (int i = 0; i < regTS.Length; i++)
            {
                int[] regShifting = registers[regTS[i]]; //Alias the register to shift

                shiftedElements[i] = registers[regTS[i]][0]; //Copy position zero value in registers to shift

                //Creates new register with appropriate max reg length

                int[] nRegister = new int[regShifting.Length]; //Could also use mRegLens[regTS[i]].Length

                //Fill values to length -1

                for (int x = 0; x < (regShifting.Length - 1); x++)
                    nRegister[x] = regShifting[x + 1]; //+1 Grabbing everything after position zero



                //Now put feedback values on the end (former RegisterPush method)

                nRegister[nRegister.Length - 1] = regTSFBV[i];

                registers[regTS[i]] = nRegister; //assign to register (update)
            }
        }
    }
}

