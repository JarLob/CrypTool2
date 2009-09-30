﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Analysis;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.PluginBase.IO;
// Reference to the BFPController interface
using Cryptool.BooleanFunctionParserController;
// Reference to the TriviumController interface (own dll)
using Cryptool.TriviumController;

namespace Cryptool.CubeAttack
{
    [Author("David Oruba", 
        "dav083@web.de", 
        "Uni-Bochum", 
        "http://www.ruhr-uni-bochum.de/")]
    [PluginInfo(true, 
        "Cube Attack", 
        "Cube Attack", 
        "CubeAttack/DetailedDescription/Description.xaml",
        "CubeAttack/Images/ca_color.png")]
    public class CubeAttack : IAnalysisMisc
    {
        #region Private variables

        private CubeAttackSettings settings;
        private string outputMaxterm;
        private string outputKeyBits;
        private enum CubeAttackMode { preprocessing, setPublicBits };
        private List<CryptoolStream> listCryptoolStreamsOut = new List<CryptoolStream>();
        private bool stop = false;

        #endregion


        #region Properties (Inputs/Outputs)
        
        [PropertyInfo(Direction.OutputData, 
            "Maxterm output", 
            "Outputs the located maxterms.", 
            "", 
            false, 
            false, 
            DisplayLevel.Beginner, 
            QuickWatchFormat.Text, 
            null)]
        public CryptoolStream OutputMaxterm
        {
            get
            {
                if (outputMaxterm != null)
                {
                    CryptoolStream cs = new CryptoolStream();
                    listCryptoolStreamsOut.Add(cs);
                    cs.OpenRead(Encoding.Default.GetBytes(outputMaxterm.ToCharArray()));
                    return cs;
                }
                else
                {
                    return null;
                }
            }
            set { }
        }

        [PropertyInfo(Direction.OutputData, 
            "Key bits", 
            "This output provides the result of the secret key", 
            "", 
            false, 
            false, 
            DisplayLevel.Beginner, 
            QuickWatchFormat.Text, 
            null)]
        public CryptoolStream OutputKeyBits
        {
            get
            {
                if (outputKeyBits != null)
                {
                    CryptoolStream cs = new CryptoolStream();
                    listCryptoolStreamsOut.Add(cs);
                    cs.OpenRead(Encoding.Default.GetBytes(outputKeyBits.ToCharArray()));
                    return cs;
                }
                else
                {
                    return null;
                }
            }
            set { }
        }

        #endregion


        #region Public interface

        /// <summary>
        /// Contructor
        /// </summary>
        public CubeAttack()
        {
            this.settings = new CubeAttackSettings();
            ((CubeAttackSettings)(this.settings)).LogMessage += CubeAttack_LogMessage;
        }

        /// <summary>
        /// Get or set all settings for this algorithm
        /// </summary>
        public ISettings Settings
        {
            get { return (ISettings)this.settings; }
            set { this.settings = (CubeAttackSettings)value; }
        }      

        /// <summary>
        /// Complete CubeAttack 
        /// </summary>
        public void Preprocessing()
        {
            ProcessCubeAttack(CubeAttackMode.preprocessing);
        }

        /// <summary>
        /// Manual input of public bits
        /// </summary>
        public void SetPublicBits()
        {
            ProcessCubeAttack(CubeAttackMode.setPublicBits);
        }

        #endregion


        #region IPlugin members

        public void Initialize()
        {
        }

        public void Dispose()
        {
            stop = false;
            //this.stop = false;
            foreach (CryptoolStream stream in listCryptoolStreamsOut)
            {
                stream.Close();
            }
            listCryptoolStreamsOut.Clear();
        }

        public bool HasChanges
        {
            get { return settings.HasChanges; }
            set { settings.HasChanges = value; }
        }

        /// <summary>
        /// Fire, if progress bar has to be updated
        /// </summary>
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        /// <summary>
        /// Fire, if new message has to be shown in the status bar
        /// </summary>
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public UserControl Presentation
        {
            get { return null; }
        }

        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void Stop()
        {
            this.stop = true;
        }

        public void PostExecution()
        {
            Dispose();
        }

        public void PreExecution()
        {
            Dispose();
        }

        #pragma warning disable 67
		public event StatusChangedEventHandler OnPluginStatusChanged;
        #pragma warning restore

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        public void Execute()
        {
            if (settings.MaxCube > settings.PublicVar)
                CubeAttack_LogMessage("Error: Max cube size cannot be greater than Public bit size.", NotificationLevel.Error);
            else
            {
                try
                {
                    switch (settings.Action)
                    {
                        case 0:
                            Preprocessing();
                            break;
                        case 1:
                            SetPublicBits();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    CubeAttack_LogMessage("Error: " + ex, NotificationLevel.Error);
                }
                finally
                {
                    ProgressChanged(1.0, 1.0);
                }
            }
        }

        /// <summary>
        /// Returns the output bit of the master polynomial p
        /// </summary>
        /// <param name="v">Public bits.</param>
        /// <param name="x">Secret bits.</param>
        /// <returns>Returns the output bit of the master polynomial, either 0 or 1.</returns>
        public int Blackbox(int[] v, int[] x)
        {
            int result = 0;
            
            try
            {
                switch (settings.BlackBox)
                {
                    // Parser as black box
                    case 0:
                        bool[] vBool = new bool[v.Length];
                        bool[] xBool = new bool[x.Length];
                        for (int i = 0; i < v.Length; i++)
                            vBool[i] = Convert.ToBoolean(v[i]);
                        for (int i = 0; i < x.Length; i++)
                            xBool[i] = Convert.ToBoolean(x[i]);
                        bool[] vx = new bool[v.Length + x.Length];
                        System.Buffer.BlockCopy(vBool, 0, vx, 0, vBool.Length);
                        System.Buffer.BlockCopy(xBool, 0, vx, vBool.Length, xBool.Length);
                        result = ParserOutput.SolveFunction(null, vx, 1);
                        break;
                    // Trivium as black box
                    case 1:
                        if (settings.PublicVar != 80 || settings.SecretVar != 80)
                        {
                            CubeAttack_LogMessage("Public bit size and Secret bit size must be 80", NotificationLevel.Error);
                            stop = true;
                            break;
                        }
                        else
                        {
                            result = TriviumOutput.GenerateTriviumKeystream(v, x, settings.TriviumOutputBit, settings.TriviumRounds, false);
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                stop = true;
                CubeAttack_LogMessage("Error: " + ex, NotificationLevel.Error);
            }
            return result;
        }

        /// <summary>
        /// The function derives the algebraic structure of the superpoly from the maxterm.
        /// The structure is derived by computing the free term and the coefficients in the superpoly.
        /// </summary>
        /// <param name="cube">The summation cube I.</param>
        /// <returns>Returns the superpoly of I in p.</returns>
        public List<int> ComputePSI(List<int> cube)
        {
            int constant = 0;
            int coeff = 0;
            List<int> superpoly = new List<int>();
            int[] pubVarElement = new int[settings.PublicVar];
            int[] secVarElement = new int[settings.SecretVar];

            CubeAttack_LogMessage("Start deriving the algebraic structure of the superpoly, compute " + settings.SecretVar + " coefficients and the value of the constant term", NotificationLevel.Info);

            // Compute the free term
            for (ulong i = 0; i < Math.Pow(2, cube.Count); i++)
            {
                if (stop)
                    return superpoly;

                for (int j = 0; j < cube.Count; j++)
                    pubVarElement[cube[j]] = (i & ((ulong)1 << j)) > 0 ? 1 : 0;
                constant ^= Blackbox((int[])pubVarElement.Clone(), (int[])secVarElement.Clone());
            }
            superpoly.Add(constant);
            CubeAttack_LogMessage("Constant term = " + (constant).ToString(), NotificationLevel.Info);

            // Compute coefficients
            for (int k = 0; k < settings.SecretVar; k++)
            {
                for (ulong i = 0; i < Math.Pow(2, cube.Count); i++)
                {
                    if (stop)
                        return superpoly;

                    secVarElement[k] = 1;
                    for (int j = 0; j < cube.Count; j++)
                        pubVarElement[cube[j]] = (i & ((ulong)1 << j)) > 0 ? 1 : 0;
                    coeff ^= Blackbox((int[])pubVarElement.Clone(), (int[])secVarElement.Clone());
                }
                superpoly.Add(constant ^ coeff);
                CubeAttack_LogMessage("Coefficient of x" + k + " = " + (constant ^ coeff), NotificationLevel.Info);
                coeff = 0;
                secVarElement[k] = 0;
            }
            return superpoly;
        }

        /// <summary>
        /// The function outputs a superpoly and its corresponding maxterm.
        /// </summary>
        /// <param name="cube">The summation Cube I.</param>
        /// <param name="superpoly">The superpoly of I in p.</param>
        public void OutputMaxterms(List<int> cube, List<int> superpoly)
        {
            StringBuilder output = new StringBuilder(string.Empty);
            bool superpolyIsEmpty = true;
            bool flag = false;
            output.Append("Maxterm Equation: ");
            if (superpoly[0] == 1)
            {
                output.Append("1");
                superpolyIsEmpty = false;
                flag = true;
            }
            for (int i = 1; i < superpoly.Count; i++)
                if (superpoly[i] == 1)
                {
                    if (flag)
                        output.Append("+x" + Convert.ToString(i - 1));
                    else
                        output.Append("x" + Convert.ToString(i - 1));
                    superpolyIsEmpty = false;
                    flag = true;
                }
            if (superpolyIsEmpty)
                output.Append("0");
            output.Append("   Cube Indexes: {");
            if (cube.Count > 0)
            {
                cube.Sort();
                for (int i = 0; i < cube.Count - 1; i++)
                    output.Append(cube[i] + ",");
                output.AppendLine(cube[cube.Count - 1] + "}");
            }
            else
                output.Append(" }\n");
            outputMaxterm += output.ToString();
            OnPropertyChanged("OutputMaxterm");
        }

        /// <summary>
        /// The function outputs the key bits.
        /// </summary>
        /// <param name="res">Result Vector</param>
        public void OutputKey(Vector res)
        {
            StringBuilder output = new StringBuilder(string.Empty);
            for (int i=0; i<res.Length; i++)
                output.AppendLine("x" + i + " = " + res[i]);
            outputKeyBits = output.ToString();
        }

        /// <summary>
        /// Test if superpoly is already in matrix.
        /// </summary>
        /// <param name="superpoly">The superpoly of I in p.</param>
        /// <param name="matrix">An n x n matrix whose rows contain their corresponding superpolys.</param>
        /// <returns>A boolean value indicating if the superpoly is in the matrix or not.</returns>
        public bool InMatrix(List<int> superpoly, Matrix matrix)
        {
            bool isEqual = true;
            for (int i = 0; i < matrix.Rows; i++)
            {
                isEqual = true;
                for (int j = 0; j < superpoly.Count; j++)
                    if (matrix[i, j] != superpoly[j])
                        isEqual = false;
                if (isEqual)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Test if cube is already known.
        /// </summary>
        /// <param name="mCube">A list of cubes.</param>
        /// <param name="cube">The summation cube I.</param>
        /// <returns>A boolean value indicating if the cube is in the list of cubes or not.</returns>
        public bool CubeKnown(List<List<int>> mCube, List<int> cube)
        {
            bool isEqual = true;
            for (int i = 0; i < mCube.Count; i++)
            {
                isEqual = true;
                if (mCube[i].Count == cube.Count)
                {
                    for (int j = 0; j < cube.Count; j++)
                        if (!mCube[i].Contains(cube[j]))
                            isEqual = false;
                    if (isEqual)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Test if superpoly is linear (BLR linearity test).
        /// </summary>
        /// <param name="cube">The summation Cube I.</param>
        /// <returns>A boolean value indicating if the superpoly is probably linear or not.</returns>
        public bool IsLinear(List<int> cube)
        {
            Random rnd = new Random();
            int psLeft = 0;
            int psRight = 0;
            int[] pubVarElement = new int[settings.PublicVar];
            int[] vectorX = new int[settings.SecretVar];
            int[] vectorY = new int[settings.SecretVar];
            int[] vecXY = new int[settings.SecretVar];

            for (int k = 0; k < settings.LinTest; k++)
            {
                CubeAttack_LogMessage("Linearity test " + (k + 1) + " of " + settings.LinTest, NotificationLevel.Info);
                psLeft = 0;
                psRight = 0;

                // Choose vectors x and y at random
                for (int i = 0; i < settings.SecretVar; i++)
                {
                    vectorX[i] = rnd.Next(0, 2);
                    vectorY[i] = rnd.Next(0, 2);
                }

                pubVarElement = new int[settings.PublicVar];
                for (int i = 0; i < settings.SecretVar; i++)
                    vecXY[i] = (vectorX[i] ^ vectorY[i]);

                for (ulong i = 0; i < Math.Pow(2, cube.Count); i++)
                {
                    if (stop)
                        return false;

                    for (int j = 0; j < cube.Count; j++)
                        pubVarElement[cube[j]] = (i & ((ulong)1 << j)) > 0 ? 1 : 0;
                    psLeft ^= Blackbox((int[])pubVarElement.Clone(), new int[settings.SecretVar]) 
                            ^ Blackbox((int[])pubVarElement.Clone(), (int[])vectorX.Clone()) 
                            ^ Blackbox((int[])pubVarElement.Clone(), (int[])vectorY.Clone());
                    psRight ^= Blackbox((int[])pubVarElement.Clone(), (int[])vecXY.Clone());
                }
                if (psLeft != psRight)
                {
                    CubeAttack_LogMessage("Linearity test " + (k + 1) + " failed", NotificationLevel.Info);
                    return false;
                }

                if (stop)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Test if superpoly is a constant value.
        /// </summary>
        /// <param name="cube">The summation Cube I.</param>
        /// <returns>A boolean value indicating if the superpoly is constant or not.</returns>
        public bool IsSuperpolyConstant(List<int> cube)
        {
            Random rnd = new Random();
            int[] vectorX = new int[settings.SecretVar];
            int flag = 0;
            int output = 0;
            int[] secVarElement = new int[settings.SecretVar];
            int[] pubVarElement = new int[settings.PublicVar];

            string outputCube = string.Empty;
            foreach (int element in cube)
                outputCube += "v" + element + " ";
            CubeAttack_LogMessage("Test if superpoly of subset " + outputCube + " is constant", NotificationLevel.Info);

            for (int i = 0; i < settings.ConstTest; i++)
            {
                for (int j = 0; j < settings.SecretVar; j++)
                    vectorX[j] = rnd.Next(0, 2);
                for (ulong j = 0; j < Math.Pow(2, cube.Count); j++)
                {
                    if (stop)
                        return false;

                    for (int k = 0; k < cube.Count; k++)
                        pubVarElement[cube[k]] = (j & ((ulong)1 << k)) > 0 ? 1 : 0;
                    output ^= Blackbox(pubVarElement, vectorX);
                }
                if (i == 0)
                    flag = output;
                if (flag != output)
                {
                    CubeAttack_LogMessage("Superpoly of subset " + outputCube + " is not constant", NotificationLevel.Info);
                    return false;
                }
                output = 0;

                if (stop)
                    return false;
            }
            return true; 
        }

        /// <summary>
        /// Generates a random permutation of a finite set—in plain terms, for randomly shuffling the set.
        /// </summary>
        /// <param name="ilist">A List of values.</param>
        public static void Shuffle<T>(List<int> ilist)
        {
            Random rand = new Random();
            int iIndex;
            int tTmp;
            for (int i = 1; i < ilist.Count; ++i)
            {
                iIndex = rand.Next(i + 1);
                tTmp = ilist[i];
                ilist[i] = ilist[iIndex];
                ilist[iIndex] = tTmp;
            }
        }

        /// <summary>
        /// Test if an n x m matrix contains n linearly independent vectors.
        /// </summary>
        /// <param name="A">n x m matrix.</param>
        /// <returns>A boolean value indicating if the matrix is regular or not.</returns>
        public bool IsLinearIndependent(Matrix A)
        {
            double maxval;
            int maxind;
            double temp;
            int Rang = 0;
            double[,] a = new double[A.Cols, A.Rows];

            for (int i = 0; i < A.Cols; i++)
                for (int j = 0; j < A.Rows; j++)
                    a[i, j] = A[j, i];

            for (int j = 0; j < A.Rows; j++)
            {
                // Find maximum
                maxval = a[j, j];
                maxind = j;
                for (int k = j; k < A.Cols; k++)
                {
                    if (a[k, j] > maxval)
                    {
                        maxval = a[k, j];
                        maxind = k;
                    }
                    if (-a[k, j] > maxval)
                    {
                        maxval = -a[k, j];
                        maxind = k;
                    }
                }

                if (maxval != 0)
                {
                    Rang++;
                    // Swap_Rows(j, maxind)
                    for (int k = j; k < A.Rows; k++)
                    {
                        temp = a[j, k];
                        a[j, k] = a[maxind, k];
                        a[maxind, k] = temp;
                    }

                    // Gauss elimination 
                    for (int i = j + 1; i < A.Cols; i++)
                        for (int k = j + 1; k < A.Rows; k++)
                            a[i, k] = a[i, k] - (a[i, j] / a[j, j] * a[j, k]);
                }
            }
            if (A.Rows == Rang)
                return true;
            else
                return false;
        }  

        /// <summary>
        /// Preprocessing Phase of the cube attack. 
        /// Implementation of the algorithm Random-Walk to find maxterms.
        /// </summary>
        public void PreprocessingPhase()
        {
            CubeAttack_LogMessage("Start preprocessing", NotificationLevel.Info);

            outputMaxterm = string.Empty;
            int countMaxterms = 0;
            Random rnd = new Random();
            int numberOfVariables = 0;
            List<int> chooseIndexI = new List<int>();
            Matrix mSuperpoly = new Matrix(settings.SecretVar, settings.SecretVar + 1);
            Matrix mCheckLinearity = new Matrix(0, settings.SecretVar);
            List<int> superpoly = new List<int>();
            List<int> cube = new List<int>();
            Vector freeTerms = new Vector(settings.SecretVar);
            List<List<int>> cubeIndex = new List<List<int>>();
            List<List<int>> mCube = new List<List<int>>();

            // Save all public variables indexes in a list 
            for (int i = 0; i < settings.PublicVar; i++)
                chooseIndexI.Add(i);

            // Find n maxterms and save their in the matrix
            while (countMaxterms < settings.SecretVar)
            {
                if (stop)
                    return;
                else
                {
                    cube = new List<int>();
                    superpoly.Clear();

                    // Generate random size k between 1 and the number of public variables
                    numberOfVariables = rnd.Next(1, settings.MaxCube + 1);

                    // Permutation of the public variables
                    Shuffle<int>(chooseIndexI);

                    // Construct cube of size k. Add k public variables to the cube
                    for (int i = 0; i < numberOfVariables; i++)
                        cube.Add(chooseIndexI[i]);

                    string outputCube = string.Empty;
                    foreach (int element in cube)
                        outputCube += "v" + element + " ";
                    CubeAttack_LogMessage("Start search for maxterm with subterm: " + outputCube, NotificationLevel.Info);
                    while (superpoly.Count == 0)
                    {
                        if (cube.Count == 0)
                        {
                            if (numberOfVariables < chooseIndexI.Count)
                            {
                                CubeAttack_LogMessage("Subset is empty, add variable v" + chooseIndexI[numberOfVariables], NotificationLevel.Info);
                                cube.Add(chooseIndexI[numberOfVariables]);
                                numberOfVariables++;
                            }
                            else
                                break;
                        }
                        if (CubeKnown(mCube, cube))
                        {
                            // Maxterm is already known, break and restart with new subset
                            CubeAttack_LogMessage("Maxterm is already known, restart with new subset", NotificationLevel.Info);
                            break;
                        }

                        if (IsSuperpolyConstant(cube))
                        {
                            if (stop)
                                return;
                            else
                            {
                                CubeAttack_LogMessage("Superpoly is likely constant, drop variable v" + cube[0], NotificationLevel.Info);
                                cube.RemoveAt(0);
                            }
                        }
                        else if (!IsLinear(cube))
                        {
                            if (stop)
                                return;
                            else
                            {
                                CubeAttack_LogMessage("Superpoly is not linear", NotificationLevel.Info);
                                if (numberOfVariables < chooseIndexI.Count)
                                {
                                    if (cube.Count <= settings.MaxCube)
                                    {
                                        CubeAttack_LogMessage("Add variable v" + chooseIndexI[numberOfVariables], NotificationLevel.Info);
                                        cube.Add(chooseIndexI[numberOfVariables]);
                                        numberOfVariables++;
                                    }
                                }
                                else
                                    break;
                            }
                        }
                        else
                        {
                            if (stop)
                                return;
                            else
                            {
                                CubeAttack_LogMessage("Superpoly is likely linear", NotificationLevel.Info);
                                mCube.Add(cube);
                                outputCube = string.Empty;
                                foreach (int element in cube)
                                    outputCube += "v" + element + " ";
                                CubeAttack_LogMessage(outputCube + " is new maxterm", NotificationLevel.Info);
                                superpoly = ComputePSI(cube);
                                break;
                            }
                        }
                    }//End while (superpoly.Count == 0)

                    if (!InMatrix(superpoly, mSuperpoly))
                    {
                        List<int> superpolyWithoutConstant = new List<int>();
                        for (int i = 1; i < superpoly.Count; i++)
                            superpolyWithoutConstant.Add(superpoly[i]);

                        mCheckLinearity = mCheckLinearity.AddRow(superpolyWithoutConstant);
                        if (IsLinearIndependent(mCheckLinearity))
                        {
                            for (int j = 0; j < superpoly.Count; j++)
                                mSuperpoly[countMaxterms, j] = superpoly[j];

                            cubeIndex.Add(cube);
                            countMaxterms++;
                            OutputMaxterms(cube, superpoly);
                            ProgressChanged((double)countMaxterms / (double)settings.SecretVar, 1.0);
                        }
                        else
                            mCheckLinearity = mCheckLinearity.DeleteLastRow();
                    }

                    if (countMaxterms == settings.SecretVar)
                    {
                        // Save the free terms of the superpolys in a vector
                        for (int i = 0; i < settings.SecretVar; i++)
                            freeTerms[i] = mSuperpoly[i, 0];
                        mSuperpoly = mSuperpoly.DeleteFirstColumn();

                        try
                        {
                            if (settings.OnlinePhase)
                            {
                                CubeAttack_LogMessage("Start online phase", NotificationLevel.Info);
                                CubeAttack_LogMessage("Invert matrix", NotificationLevel.Info);
                                mSuperpoly = mSuperpoly.Inverse();
                                OnlinePhase(mSuperpoly, freeTerms, cubeIndex);
                            }
                        }
                        catch (Exception exception)
                        {
                            GuiLogMessage(exception.Message, NotificationLevel.Error);
                        }
                    }
                }
            }//End while (countMaxterms < settings.SecretVar)
        }//End PreprocessingPhase

        /// <summary>
        /// Online Phase of the cube attack.
        /// </summary>
        /// <param name="mSuperpoly">An n x n matrix which contains the superpolys (without the free terms).</param>
        /// <param name="b">Vector which contains the corresponding free terms of the superpolys.</param>
        /// <param name="cubeIndex">A list of lists of cube indices.</param>
        public void OnlinePhase(Matrix mSuperpoly, Vector b, List<List<int>> cubeIndex)
        {
            int[] pubVarElement = new int[settings.PublicVar];
            bool superpolyHasConstantTerm = false;

            for (int i = 0; i < settings.SecretVar; i++)
            {
                if (b[i] == 1)
                    superpolyHasConstantTerm = true;
                for (int j = 0; j < settings.PublicVar; j++)
                    pubVarElement[j] = 0;
                for (ulong k = 0; k < Math.Pow(2, cubeIndex[i].Count); k++)
                {
                    if (stop)
                        return;

                    for (int l = 0; l < cubeIndex[i].Count; l++)
                        pubVarElement[cubeIndex[i][l]] = (k & ((ulong)1 << l)) > 0 ? 1 : 0;
                    try
                    {
                        bool[] vBool = new bool[pubVarElement.Length];

                        for (int l = 0; l < pubVarElement.Length; l++)
                            vBool[l] = Convert.ToBoolean(pubVarElement[l]);
                        b[i] ^= ParserOutput.SolveFunction(null, vBool, 2);
                    }
                    catch (Exception ex)
                    {
                        CubeAttack_LogMessage("Error: " + ex, NotificationLevel.Error);
                    }
                }
                if (superpolyHasConstantTerm)
                    CubeAttack_LogMessage("Value of maxterm equation " + (i + 1) + " of " + settings.SecretVar + " = " + (b[i] ^ 1), NotificationLevel.Info);
                else
                    CubeAttack_LogMessage("Value of maxterm equation " + (i + 1) + " of " + settings.SecretVar + " = " + b[i], NotificationLevel.Info);
                // Reset variable
                superpolyHasConstantTerm = false;
            }
            OutputKey(mSuperpoly * b);
            OnPropertyChanged("OutputKeyBits");
        }

        /// <summary>
        /// In this mode the user is allowed to set the summation cube manually.
        /// </summary>
        public void SetPublicBitsPhase()
        {
            outputMaxterm = string.Empty;
            outputKeyBits = string.Empty;
            int[] secVari = new int[settings.SecretVar];
            int[] pubVari = new int[settings.PublicVar];
            List<int> cube = new List<int>();
            bool fault = false;

            if (settings.SetPublicBits.Length != settings.PublicVar)
                CubeAttack_LogMessage("Input public bits must have size " + settings.PublicVar + " (Currently: " + settings.SetPublicBits.Length + " )", NotificationLevel.Error);
            else
            {
                for (int i = 0; i < settings.SetPublicBits.Length; i++)
                {
                    switch (settings.SetPublicBits[i])
                    {
                        case '0':
                            pubVari[i] = 0;
                            break;
                        case '1':
                            pubVari[i] = 1;
                            break;
                        case '*':
                            cube.Add(i);
                            break;
                        default:
                            fault = true;
                            break;
                    }
                }
                if (fault)
                    CubeAttack_LogMessage("The input public bits does not consists only of characters : \'0\',\'1\',\'*\' !", NotificationLevel.Error);
                else
                {
                    if (cube.Count > 0)
                    {
                        if (IsLinear(cube))
                        {
                            List<int> superpoly = ComputePSI(cube);
                            if (!stop)
                                OutputMaxterms(cube, superpoly);
                        }
                        else
                        {
                            if(!stop)
                                CubeAttack_LogMessage("The corresponding superpoly is not a linear polynomial !", NotificationLevel.Info);
                        }
                    }
                    else
                    {
                        StringBuilder output = new StringBuilder(string.Empty);
                        output.Append("Output bit: " + Blackbox(pubVari, secVari));
                        outputMaxterm += output.ToString();
                        OnPropertyChanged("OutputMaxterm");
                        CubeAttack_LogMessage("Output bit: " + Blackbox(pubVari, secVari), NotificationLevel.Info);
                    }
                }
            }
        }

        public void Pause()
        {
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Does the actual CubeAttack processing
        /// </summary>
        private void ProcessCubeAttack(CubeAttackMode mode)
        {
            switch (mode)
            {
                case CubeAttackMode.preprocessing:
                    PreprocessingPhase();
                    break;
                case CubeAttackMode.setPublicBits:
                    SetPublicBitsPhase();
                    break;
            }
        }

        /// <summary>
        /// Handles log messages
        /// </summary>
        private void CubeAttack_LogMessage(string msg, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(msg, this, logLevel));
            }
        }

        #region IControlEncryption Members

        private IControlSolveFunction parserOutput;
        [PropertyInfo(Direction.ControlMaster, "Master for BFP", "Master for BFP (SolveFunction)", "", DisplayLevel.Beginner)]
        public IControlSolveFunction ParserOutput
        {
            get { return parserOutput; }
            set
            {
                if (value != null)
                    parserOutput = value;
            }
        }

        private IControlTrivium triviumOutput;
        [PropertyInfo(Direction.ControlMaster, "Master for Trivium", "Master for Trivium", "", DisplayLevel.Beginner)]
        public IControlTrivium TriviumOutput
        {
            get { return triviumOutput; }
            set
            {
                if (value != null)
                    triviumOutput = value;
            }
        }

        #endregion

        #endregion


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
