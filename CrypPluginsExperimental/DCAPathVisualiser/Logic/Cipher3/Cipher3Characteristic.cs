﻿using System;

namespace DCAPathVisualiser.Logic.Cipher3
{
    public class Cipher3Characteristic : Characteristic
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Cipher3Characteristic()
        {
            InputDifferentials = new UInt16[5];
            OutputDifferentials = new UInt16[4];

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
            Characteristic obj = new Cipher3Characteristic
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
            return "Prob: " + Probability + " InputDiffRound5: " + InputDifferentials[4] + " OutputDiffRound4: " +
                   OutputDifferentials[3] + " InputDiffRound4: " + InputDifferentials[3] + " OutputDiffRound3: " +
                   OutputDifferentials[2] + " InputDiffRound3: " + InputDifferentials[2] + " OutputDiffRound2: " +
                   OutputDifferentials[1] + " InputDiffRound2: " + InputDifferentials[1] + " OutputDiffRound1: " +
                   OutputDifferentials[0] + " InputDiffRound1: " + InputDifferentials[0];
        }
    }
}
