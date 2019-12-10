﻿/*
   Copyright 2019 Christian Bender christian1.bender@student.uni-siegen.de

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
using System.Threading.Tasks;

namespace DCAPathVisualiser.Logic.Cipher2
{
    public class Cipher2Characteristic : Characteristic
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Cipher2Characteristic()
        {
            InputDifferentials = new UInt16[3];
            OutputDifferentials = new UInt16[2];

            for (int i = 0; i < InputDifferentials.Length; i++)
            {
                InputDifferentials[i] = 0;
            }

            for (int i = 0; i < OutputDifferentials.Length; i++)
            {
                OutputDifferentials[i] = 0;
            }
        }

        /// <summary>
        /// override of Clone()
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            Characteristic obj = new Cipher2Characteristic
            {
                InputDifferentials = (UInt16[])this.InputDifferentials.Clone(),
                OutputDifferentials = (UInt16[])this.OutputDifferentials.Clone(),
                Probability = this.Probability
            };

            return obj;
        }

        /// <summary>
        /// override of ToString()
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Prob: " + Probability + " InputDiffRound3: " + InputDifferentials[2] + " OutputDiffRound2: " +
                   OutputDifferentials[1] + " InputDiffRound2: " + InputDifferentials[1] + " OutputDiffRound1: " +
                   OutputDifferentials[0] + " InputDiffRound1: " + InputDifferentials[0];
        }
    }
}
