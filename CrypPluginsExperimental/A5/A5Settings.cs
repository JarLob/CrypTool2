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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.A5
{

    // HOWTO: rename class (click name, press F2)
    public class A5Settings : ISettings
    {
        public enum A5Mode {Encrypt = 0 ,  Decrypt = 1};

        private A5Mode selectedAction = A5Mode.Encrypt;
        public A5Settings()
        { }

        public void Initialize()
        {
        }

        #region TaskPane Settings

        [TaskPane("ActionCaption", "ActionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "A5Mode0", "A5Mode1" })]
        public A5Mode Action
        {
            get
            {
                return this.selectedAction;
            }
            set
            {
                if (value != selectedAction)
                {

                    this.selectedAction = value;
                    OnPropertyChanged("Action");
                }
            }
        }

        #endregion

        //static void Main(string[] args)
        //{
        //    //Create registers
        //    A5 A5eng = new A5();

        //    //To expand the algorithm...
        //    //Edit below properties
        //    A5eng.NumRegisters = 3; //Number of registers to use
        //    A5eng.MaxRegLengths = new int[] { 19, 22, 23 }; //Test max reg lengths: { 5,5,5 };
        //    A5eng.RegIndexes = new int[] { 8, 10, 10 }; //Test clocking bits: { 0,0,0 };

        //    //Test polynomials: { "x^1+x^2+x^3+1", "x^0+x^2+x^3+1", "x^3+x^4+1" };
        //    A5eng.PolynomialsArray = new string[] { "x^8+x^17+x^16+x^13+1",
        //                                        "x^21+x^20+1",
        //                                        "x^22+x^21+x^20+x^7+1" };
        //    A5eng.SourceArray = new byte[] { 1, 0, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1,
        //                                    1, 0, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1,
        //                                    1, 0, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1,
        //                                    1, 0, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1,
        //                                    1, 0, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1,
        //                                    0, 1, 0, 1, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 1, 0, 1, 1, 0, 1 };

        //    /* DO NOT EDIT BELOW THIS POINT */
        //    int numRegPushes = 100;
        //    bool testPassed = true; //For cipher check

        //    if (args.Length > 0)
        //    {
        //        if (args[0] == "-d")
        //        {
        //            A5eng.dbMode = true;
        //            if (args.Length > 1)
        //            {
        //                try
        //                {
        //                    numRegPushes = int.Parse(args[1]);
        //                }
        //                catch (Exception ex)
        //                {
        //                    testPassed = false;

        //                    Console.WriteLine("Error: Numeric values only!");
        //                    Console.WriteLine("Exception: " + ex.Message);
        //                }
        //            }
        //        }
        //    }

        //    if (A5eng.SourceArray.Length < A5eng.GetMaxRegLensTotal())
        //    {
        //        testPassed = false;
        //        Console.WriteLine("[-] Not enough source data!");
        //    }

        //    if (A5eng.PolynomialsArray.Length != A5eng.NumRegisters)
        //    {
        //        testPassed = false;
        //        Console.WriteLine("[-] Not enough polynomials");
        //    }

        //    if (testPassed)
        //    {
        //        A5eng.Registers = A5eng.CreateRegisters();

        //        if (A5eng.dbMode)
        //        {
        //            Console.WriteLine("Output (debugging mode): ");
        //            for (int ia = 0; ia < numRegPushes; ia++)
        //            {
        //                Console.WriteLine("[register]");
        //                int c = 0;
        //                foreach (int[] p in A5eng.Registers)
        //                {
        //                    Console.Write("register: {0} ", c);
        //                    foreach (int poly in p)
        //                    {
        //                        Console.Write(poly.ToString());
        //                    }
        //                    Console.WriteLine();

        //                    c++;
        //                }
        //                Console.WriteLine("[/register]");

        //                int[] regTS = A5eng.RegistersToShift();
        //                A5eng.RegisterShift(regTS);

        //                System.Threading.Thread.Sleep(20); //Slow the output
        //            }

        //            Console.WriteLine("\n{0} loops of A5/1 have been completed.", numRegPushes.ToString());
        //        }
        //        else
        //        {
        //            A5eng.Intro();

        //            Console.WriteLine("Output: ");
        //            while (true)
        //            {
        //                Console.Write(A5eng.GetOutValue().ToString());

        //                int[] regTS = A5eng.RegistersToShift();
        //                A5eng.RegisterShift(regTS);

        //                System.Threading.Thread.Sleep(20); //Slow the output
        //            }
        //        }
        //    }
        //}

        
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}