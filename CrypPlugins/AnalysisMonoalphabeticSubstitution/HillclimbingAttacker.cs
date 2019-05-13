using System;
using System.Collections.Generic;
using System.Linq;
using ManagedCuda;
using ManagedCuda.BasicTypes;
using ManagedCuda.VectorTypes;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Utils;

namespace Cryptool.AnalysisMonoalphabeticSubstitution
{
    class HillclimbingAttacker
    {
        #region Constants

        /* CudaVars if Alphabet is English:
         * there are 26 letters to be tested. Testing each pair is 26*26 = 676 = 2*2*13*13
        */
        const int GRIDDIMENG = 2;      // => x*x amount of Blocks ! Important, else there will be an error within the Kernel
        const int BLOCKDIMENG = 13;     // => x*x Threads per Block! Important: maximum amount is 32. Because  32*32 = 1024  is the maximum amount of threads per block on todays GPU's

        /* CudaVars if Alphabet is German:
         * there are 30 letters to be tested. Testing each pair is 30*30 = 900 = 3*3*10*10
        */
        const int GRIDDIMGER = 3;      // => x*x amount of Blocks ! Important, else there will be an error within the Kernel
        const int BLOCKDIMGER = 10;     // => x*x Threads per Block! Important: maximum amount is 32. Because  32*32 = 1024  is the maximum amount of threads per block on todays GPU's

        /* CudaVars if Alphabet is Spanish:
         * there are 27 letters to be tested. Testing each pair is 27*27 = 729 = 3*3*3*3*3*3
        */
        const int GRIDDIMES = 3;      // => x*x amount of Blocks ! Important, else there will be an error within the Kernel
        const int BLOCKDIMES = 9;      // => x*x Threads per Block! Important: maximum amount is 32. Because  32*32 = 1024  is the maximum amount of threads per block on todays GPU's 
        
        #endregion Constants

        #region Variables

        // Delegate
        private bool stopFlag;
        private PluginProgress pluginProgress;
        private UpdateKeyDisplay updateKeyDisplay;
        public CalculateCostDelegate calculateCost;

        //Input
        private string ciphertextString = null;
        private string ciphertextalphabet = null;
        private string plaintextalphabet = null;
        private int restarts;
        public QuadGrams quadgrams; // GPU requires quadgrams

        //InplaceSymbols
        int[,] inplaceSpots;
        int[] inplaceAmountOfSymbols;

        //CudaVars
        static CudaKernel MajorKernel;

        //Output
        private long totalKeys;

        #endregion Variables;

        #region Input Properties

        public long TotalKeys
        {
            get { return this.totalKeys; }
        }

        public string Ciphertext
        {
            get { return this.ciphertextString; }
            set { this.ciphertextString = value; }
        }

        public string CiphertextAlphabet
        {
            get { return this.ciphertextalphabet; }
            set { this.ciphertextalphabet = value; }
        }

        public string PlaintextAlphabet
        {
            get { return this.plaintextalphabet; }
            set { this.plaintextalphabet = value; }
        }

        public int Restarts
        {
            get { return this.restarts; }
            set { this.restarts = value; }
        }

        public Boolean StopFlag
        {
            get { return this.stopFlag; }
            set { this.stopFlag = value; }
        }
        #endregion Input Properties

        #region Output Properties

        public UpdateKeyDisplay UpdateKeyDisplay
        {
            get { return this.updateKeyDisplay; }
            set { this.updateKeyDisplay = value; }
        }

        public PluginProgress PluginProgressCallback
        {
            get { return this.pluginProgress; }
            set { this.pluginProgress = value; }
        }

        public CalculateCostDelegate CalculateCost
        {
            get { return this.calculateCost; }
            set { this.calculateCost = value; }
        }

        #endregion Output Properties

        public void ExecuteOnGPU()
        {
            //Initialise CUDA
            CudaContext cntxt = new CudaContext();
            InitKernels(plaintextalphabet, cntxt);

            #region Variables

            //Local C# Variables
            totalKeys = 0;
            long totalThreads = 0;
            int alphabetlength = plaintextalphabet.Length; //Implemented for Performance
            double globalBestCost = double.MinValue;
            int[] ciphertext = MapTextIntoNumberSpace(RemoveInvalidChars(ciphertextString.ToLower(), ciphertextalphabet), ciphertextalphabet);
            int[] ciphertextForCuda = ciphertext.Take(1000).ToArray(); //if the Ciphertext length is > 1000, cut the text and ignore everything after the first 1k Symbols. (Performance and Cuda specific needs).
            int textLength = ciphertextForCuda.Length;

            //Compute amount of threads used
            if (alphabetlength == 26)
            {
                totalThreads = ((GRIDDIMENG * GRIDDIMENG) * (BLOCKDIMENG * BLOCKDIMENG));
            }
            if (alphabetlength == 27)
            {
                totalThreads = ((GRIDDIMES * GRIDDIMES) * (BLOCKDIMES * BLOCKDIMES));
            }
            else
            {
                totalThreads = ((GRIDDIMGER * GRIDDIMGER) * (BLOCKDIMGER * BLOCKDIMGER));
            }

            //Load Costfunction
            //Load4Grams();

            //Cuda has no 4dim. Arrays => Break Costfunction down in one dimension
            double[] d_singleDimQuadgrams = d_singleDimQuadgrams = QuadgramsToSingleDim(alphabetlength);

            //Variables for CUDA
            //totalthreads
            //CudaDeviceVariable<long> vector_totalThreads = new CudaDeviceVariable<long>(1);
            //vector_totalThreads.CopyToDevice(totalThreads);

            //Runkey: Copy Data to Device when calling Kernel.
            CudaDeviceVariable<int> vector_runkey = new CudaDeviceVariable<int>(plaintextalphabet.Length);

            //Ciphertext (Already prepared for Kernel).
            CudaDeviceVariable<int> vector_ciphertext = new CudaDeviceVariable<int>(textLength);
            vector_ciphertext.CopyToDevice(ciphertextForCuda);

            //Textlength
            //CudaDeviceVariable<int> vector_textLength = new CudaDeviceVariable<int>(1);
            //vector_textLength.CopyToDevice(textLength);

            //Costfunction
            CudaDeviceVariable<double> vector_quadgrams = new CudaDeviceVariable<double>(d_singleDimQuadgrams.Length);
            vector_quadgrams.CopyToDevice(d_singleDimQuadgrams);

            //Cudavariable for output. Nothing to CopyToDevice
            CudaDeviceVariable<double> vector_cudaout = new CudaDeviceVariable<double>(totalThreads);
            #endregion Variables

            var totalRestarts = restarts;
            try
            {
                //HILLCLIMBING:
                while (restarts > 0)
                {
                    //generate random key:
                    Random random = new Random();
                    int[] runkey = new int[alphabetlength];
                    runkey = BuildRandomKey(random);
                    double bestkeycost = double.MinValue;
                    bool foundbetter;

                    do
                    {
                        foundbetter = false;
                        //Check all Transformations of i,j in CUDA.
                        double[] cuda_out = CudaHillClimb(totalThreads, vector_ciphertext, textLength,
                        runkey, vector_runkey, vector_quadgrams, vector_cudaout);
                        //cntxt.Synchronize();

                        totalKeys += totalThreads; //Amount of tested keys tested.

                        //check if in _out are better CostValues than there are at the moment & return position else return "-1"
                        int betterKeyPosition = Betterkey(cuda_out, bestkeycost);

                        if (betterKeyPosition != -1)
                        {
                            //adopt new bestcost and new runkey .
                            bestkeycost = cuda_out[betterKeyPosition];
                            foundbetter = true;
                            int i = betterKeyPosition / alphabetlength;
                            int j = betterKeyPosition % alphabetlength;
                            Swap(runkey, i, j);
                        }

                    } while (foundbetter);

                    if (StopFlag) return;

                    restarts--;
                    pluginProgress(totalRestarts - restarts, totalRestarts);

                    //Output
                    //if (bestkeycost > globalBestCost)
                    {
                        globalBestCost = bestkeycost;
                        //Add to bestlist-output:
                        KeyCandidate newKeyCan = new KeyCandidate(runkey, globalBestCost,
                            ConvertNumbersToLetters(UseKeyOnCipher(ciphertext, runkey), plaintextalphabet), ConvertNumbersToLetters(runkey, plaintextalphabet));
                        //Note: in usekeyOnCipher Method i use ciphertext.length instead of textlength! Because textlength = Length Cuda uses != the whole ciphertext textlength
                        newKeyCan.HillAttack = true;
                        this.updateKeyDisplay(newKeyCan);
                    }
                }
            }
            finally
            {
                //Free CudaMemory
                //vector_totalThreads.Dispose();
                vector_ciphertext.Dispose();
                //vector_textLength.Dispose();
                vector_quadgrams.Dispose();
                vector_runkey.Dispose();
                vector_cudaout.Dispose();
                cntxt.Dispose();

                pluginProgress(1, 1);
            }
        }

        public void ExecuteOnCPU()
        {
            #region{Variables}

            double globalbestkeycost = double.MinValue;
            int[] bestkey = new int[plaintextalphabet.Length];
            inplaceAmountOfSymbols = new int[plaintextalphabet.Length];
            int alphabetlength = plaintextalphabet.Length; //No need for calculating a few million times. (Performance)
            bool foundbetter;
            bool foundInplace = false;
            totalKeys = 0;
            Random random = new Random();

            #endregion{Variables}

            //Take input and prepare
            //int[] ciphertext = MapTextIntoNumberSpace(RemoveInvalidChars(ciphertextString.ToLower(), ciphertextalphabet), ciphertextalphabet);
            PluginBase.Utils.Alphabet ciphertextAlphabet = new PluginBase.Utils.Alphabet(ciphertextalphabet);
            PluginBase.Utils.Alphabet plaintextAlphabet = new PluginBase.Utils.Alphabet(plaintextalphabet);
            PluginBase.Utils.Text cipherText = new PluginBase.Utils.Text(ciphertextString, ciphertextAlphabet);
            int[] ciphertext = cipherText.ValidLetterArray;
            
            int length = ciphertext.Length;
            int[] plaintext = new int[length];
            inplaceSpots = new int[plaintextalphabet.Length, length];
            
            for (int restart = 0; restart < restarts; restart++)
            {
                pluginProgress(restart, restarts);

                //Generate random key:
                int[] runkey = BuildRandomKey(random);
                double bestkeycost = double.MinValue;

                //Create first plaintext and analyze places of symbols:
                plaintext = UseKeyOnCipher(ciphertext, runkey);
                AnalyzeSymbolPlaces(plaintext, length);

                do
                {
                    foundbetter = false;
                    for (int i = 0; i < alphabetlength; i++)
                    {
                        foundInplace = false;
                        int[] copykey = (int[])runkey.Clone();
                        for (int j = 0; j < alphabetlength; j++)
                        {
                            if (i == j) continue;

                            //create child key
                            Swap(copykey, i, j);

                            int sub1 = copykey[i];
                            int sub2 = copykey[j];

                            //Inplace swap in text
                            for (int m = 0; m < inplaceAmountOfSymbols[sub1]; m++)
                                plaintext[inplaceSpots[sub1, m]] = sub2;

                            for (int m = 0; m < inplaceAmountOfSymbols[sub2]; m++)
                                plaintext[inplaceSpots[sub2, m]] = sub1;

                            //Calculate the costfunction
                            double costvalue = calculateCost(plaintext);

                            if (bestkeycost < costvalue) //When found a better key adopt it.
                            {
                                bestkeycost = costvalue;
                                bestkey = (int[])copykey.Clone();
                                foundbetter = true;
                                foundInplace = true;
                            }

                            //Revert the CopyKey substitution
                            Swap(copykey, i, j);

                            for (int m = 0; m < inplaceAmountOfSymbols[sub2]; m++)
                                plaintext[inplaceSpots[sub2, m]] = sub2;

                            for (int m = 0; m < inplaceAmountOfSymbols[sub1]; m++)
                                plaintext[inplaceSpots[sub1, m]] = sub1;

                            totalKeys++; //Count Keys for Performance output
                        }

                        //Fast converge take over new key + therefore new resulting plaintext
                        if (foundInplace)
                        {
                            runkey = bestkey;
                            plaintext = UseKeyOnCipher(ciphertext, runkey);
                            AnalyzeSymbolPlaces(plaintext, length);
                        }
                    }
                } while (foundbetter);

                if (StopFlag) return;

                if (globalbestkeycost < bestkeycost)
                {
                    globalbestkeycost = bestkeycost;
                    //Add to bestlist-output:
                    string sss = cipherText.ToString(plaintextAlphabet, true);
                    string keystring = CreateKeyOutput(bestkey);
                    KeyCandidate newKeyCan = new KeyCandidate(bestkey, bestkeycost, ConvertNumbersToLetters(UseKeyOnCipher(ciphertext, bestkey), plaintextalphabet), keystring);
                    //KeyCandidate newKeyCan = new KeyCandidate(bestkey, bestkeycost, ConvertNumbersToLetters(UseKeyOnCipher(ciphertext, bestkey), plaintextalphabet), ConvertNumbersToLetters(bestkey, plaintextalphabet));
                    newKeyCan.HillAttack = true;
                    this.updateKeyDisplay(newKeyCan);
                }
            }
        }

        #region Methods & Functions

        private String CreateKeyOutput(int[] key)
        {
            char[] k = new char[plaintextalphabet.Length];
            for (int i = 0; i < k.Length; i++) k[i] = ' ';

            for (int i = 0; i < ciphertextalphabet.Length; i++)
                k[key[i]] = ciphertextalphabet[i];

            return new string(k);
        }

        public static string RemoveInvalidChars(string text, string alphabet)
        {
            return new string((text.Where(c => alphabet.Contains(c))).ToArray());
        }

        public static int[] MapTextIntoNumberSpace(string text, string alphabet)
        {
            return text.Select(c => alphabet.IndexOf(c)).ToArray();
        }

        private int[] BuildRandomKey(Random randomdev)
        {
            List<int> list = new List<int>();
            for (int i = 0; i < plaintextalphabet.Length; i++) list.Add(i);

            int[] key = new int[plaintextalphabet.Length];

            for (int i = (plaintextalphabet.Length - 1); i >= 0; i--)
            {
                int random = randomdev.Next(0, i + 1);
                key[i] = list[random];
                list.RemoveAt(random);
            }

            return key;
        }

        private void Swap(int[] key, int i, int j)
        {
            int tmp = key[i];
            key[i] = key[j];
            key[j] = tmp;
        }

        private int[] UseKeyOnCipher(int[] ciphertext, int[] key)
        {
            int[] plaintext = new int[ciphertext.Length];
            
            for (int i = 0; i < ciphertext.Length; i++)
                plaintext[i] = key[ciphertext[i]];

            return plaintext;
        }

        private int[] InplaceSubstitution(int[] plaintext, int symbol1, int symbol2)
        {
            //Swap in Text
            for (int i = 0; i < inplaceAmountOfSymbols[symbol1]; i++)
            {
                plaintext[inplaceSpots[symbol1, i]] = symbol2;
            }

            for (int i = 0; i < inplaceAmountOfSymbols[symbol2]; i++)
            {
                plaintext[inplaceSpots[symbol2, i]] = symbol1;
            }

            //In PlacesArray
            int[] temparray = new int[inplaceAmountOfSymbols[symbol1]];

            for (int t = 0; t < inplaceAmountOfSymbols[symbol1]; t++)
            {
                temparray[t] = inplaceSpots[symbol1, t];
            }
            //Spots of sub1 = Spots of sub2
            for (int t = 0; t < inplaceAmountOfSymbols[symbol2]; t++)
            {
                inplaceSpots[symbol1, t] = inplaceSpots[symbol2, t];
            }

            //Spots of sub2 = Spots of sub1
            for (int t = 0; t < temparray.Length; t++)
            {
                inplaceSpots[symbol2, t] = temparray[t];
            }

            //In AmountArray
            int temp = inplaceAmountOfSymbols[symbol1];
            inplaceAmountOfSymbols[symbol1] = inplaceAmountOfSymbols[symbol2];
            inplaceAmountOfSymbols[symbol2] = temp;

            return plaintext;
        }

        /// <summary>
        /// Maps a given array of numbers into the "textspace" defined by the alphabet
        /// </summary>
        /// <param name="numbers"></param>
        /// <param name="alphabet"></param>
        /// <returns></returns>
        public static string ConvertNumbersToLetters(int[] numbers, string alphabet)
        {
            return String.Join("", numbers.Select(i => alphabet[i]));
        }

        private void AnalyzeSymbolPlaces(int[] text, int length)
        {
            for (int i = 0; i < plaintextalphabet.Length; i++)
                inplaceAmountOfSymbols[i] = 0;

            for (int i = 0; i < length; i++)
            {
                int p = text[i];
                if (p < 0 || p > length)
                {
                    Console.WriteLine("Error: illegal symbol found");
                    break;
                }
                inplaceSpots[p, inplaceAmountOfSymbols[p]++] = i;
            }
        }

        #endregion Methods & Functions

        #region CUDA - Methods & Functions

        static void InitKernels(string alphabet, CudaContext cntxt)
        {
            //Had to initialize to prevent code Error: 4 and 169 are the values used for the English alphabet. But it will always be new computed with one of the if-statements
            dim3 gridsize = 4;
            dim3 blocksize = 169;
            if (alphabet.Length == 26)
            {
                gridsize = GRIDDIMENG * GRIDDIMENG;
                blocksize = BLOCKDIMENG * BLOCKDIMENG;
            }
            if (alphabet.Length == 27)
            {
                gridsize = GRIDDIMES * GRIDDIMES;
                blocksize = BLOCKDIMES * BLOCKDIMES;
            }
            else if (alphabet.Length == 30)
            {
                gridsize = GRIDDIMGER * GRIDDIMGER;
                blocksize = BLOCKDIMGER * BLOCKDIMGER;
            }

            //Load KERNEL.PTX file.
            string pathKernel_PTX;
            pathKernel_PTX = DirectoryHelper.DirectoryCrypPlugins;
            CUmodule cumodule = cntxt.LoadModule(@pathKernel_PTX + "\\AnalyseMonoalphabeticSubstitution_kernel.ptx");

            //Load the CudaFunction
            if (alphabet.Length == 26)//Algorithm for english version
            {
                MajorKernel = new CudaKernel("_Z9kernelENGlPiiS_PdS0_", cumodule, cntxt);
            }
            else if (alphabet.Length == 27)//Algorithm for spanish version
            {
                MajorKernel = new CudaKernel("_Z8kernelESlPiiS_PdS0_", cumodule, cntxt);
            }
            else if (alphabet.Length == 30)//Algorithm for german version
            {
                MajorKernel = new CudaKernel("_Z9kernelGERlPiiS_PdS0_", cumodule, cntxt);
            }
            MajorKernel.BlockDimensions = blocksize;
            MajorKernel.GridDimensions = gridsize;
        }

        private int Betterkey(double[] costvalues, double bestcostvalue)
        {
            //This method searches for the best possible key in the costvaluesset that is better then the bestcostvalue at the moment
            //When there is no better key return -1
            int position = -1;

            for (int i = 0; i < costvalues.Length; i++)
            {
                if (costvalues[i] > bestcostvalue)
                {
                    position = i;
                    bestcostvalue = costvalues[i];
                }
            }

            return position;
        }

        private int[] CutCiphertext(int[] ciphertext)
        {
            //Cuts the Ciphertext to a maximum length of 1000 Symbols. Else it will get too big to handle for Kernel.
            const int max = 1000;

            if (ciphertext.Length < max)
                return ciphertext;
            
            int[] cuttedCipher = new int[max];
            Array.Copy(ciphertext, cuttedCipher, max);
            return cuttedCipher;
        }

        private double[] QuadgramsToSingleDim(int al)
        {
            // al = alphabetlength
            double[] singleDimQuadgrams = new double[al * al * al * al];

            int i = 0;

            for (int dim4 = 0; dim4 < al; dim4++)
                for (int dim3 = 0; dim3 < al; dim3++)
                    for (int dim2 = 0; dim2 < al; dim2++)
                        for (int dim1 = 0; dim1 < al; dim1++)
                            singleDimQuadgrams[i++] = quadgrams.Frequencies[dim1, dim2, dim3, dim4];

            return singleDimQuadgrams;
        }

        /*CUDAHILLCLIMBING CALL KERNEL:
         * 
         *                  TotlThreads                Ciphertext         Textlength            RunKey         CudaVar-Runkey       SingleDim.Quadgrams           CudaOutput            returntype*/
        static Func<long, CudaDeviceVariable<int>, int, int[], CudaDeviceVariable<int>, CudaDeviceVariable<double>, CudaDeviceVariable<double>, double[]>

            CudaHillClimb = (totalThreads, vector_ciphertext, textlength, runkey, vector_runkey, vector_quadgrams, vector_cudaout) =>
            {
                //Dynamic Input
                vector_runkey.CopyToDevice(runkey);

                //Run cuda method
                MajorKernel.Run(totalThreads, vector_ciphertext.DevicePointer, textlength,
                    vector_runkey.DevicePointer, vector_quadgrams.DevicePointer, vector_cudaout.DevicePointer);

                double[] _out = new double[totalThreads];

                // copy return to host
                vector_cudaout.CopyToHost(_out);

                return _out;
            };

        #endregion CUDA - Methods & Functions
    }
}