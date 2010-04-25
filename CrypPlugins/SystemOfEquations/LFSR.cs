﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
namespace Cryptool.SystemOfEquations
{
    class LFSR
    {
        public int length;
        string feedbackpolynom;
        public bool[][] internalstate;
        public char[] outputcells;
        public LFSR(string charpolynom)
        {
            feedbackpolynom = charpolynom;
            length = charpolynom.Length;
            internalstate = new bool[length][];
            for (int i = 0; i < internalstate.Length; i++)
            {
                internalstate[i] = new bool[length];
                for (int j = 0; j < internalstate.Length; j++)
                {
                    if (i == j) internalstate[i][j] = true;
                    else internalstate[i][j] = false;
                }
            }
        }
        public bool[] XorCells(bool[] cell1, bool[] cell2)
        {
            bool[] res = new bool[cell2.Length];
            for (int i = 0; i < cell2.Length; i++)
            {
                res[i] = cell1[i] ^ cell2[i];
            }
            return res;
        }
        public void OneClock()
        {
            char[] c = feedbackpolynom.ToCharArray();
            bool[] res = new bool[length];
            res = internalstate[0];
            for (int k = 1; k < length; k++)
            {
                if (c[k] == '1') res = XorCells(res, internalstate[k]);
            }
            for (int j = 0; j < internalstate.Length - 1; j++)
            {
                internalstate[j] = internalstate[j + 1];
            }
            internalstate[length - 1] = res;
        }
        public void Clock(int time)
        {
            for (int i = 0; i < time; i++)
            {
                OneClock();
            }
        }
        public ArrayList Output()
        {
            ArrayList lfsroutput = new ArrayList();
            for (int i = 0; i < outputcells.Length; i++)
            {
                if (outputcells[i] == '1')
                    lfsroutput.Add(internalstate[i]);
            }
            return lfsroutput;
        }
    }
}
