using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using ManagedCuda;
using ManagedCuda.BasicTypes;
using ManagedCuda.VectorTypes;
using Cryptool.PluginBase.IO;

namespace Cryptool.Plugins.AnalysisMonoalphabeticSubstitution
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
        //Input
        private string ciphertextString = null;
        private string alphabet = null;
        private int restarts;
        private double[, , ,] _quadgrams;
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

        public string Alphabet
        {
            get { return this.alphabet; }
            set { this.alphabet = value; }
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
        #endregion Output Properties

        public void ExecuteOnGPU()
        {
            //Initialise CUDA
            CudaContext cntxt = new CudaContext();
            InitKernels(alphabet, cntxt);

            #region Variables

            //Local C# Variables
            totalKeys = 0;
            long totalThreads = 0;
            int alphabetlength = alphabet.Length; //Implemented for Performance
            double globalBestCost = double.MinValue;
            int[] ciphertext = MapTextIntoNumberSpace(RemoveInvalidChars(ciphertextString.ToLower(),alphabet), alphabet);
            int[] ciphertextForCuda = CutCiphertext(ciphertext); //if the Ciphertextlength is > 1000, cut the text and ignore everything after the first 1k Symbols. (Performance and Cudaspecific needs).
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
            else if (alphabetlength == 30)
            {
                totalThreads = ((GRIDDIMGER * GRIDDIMGER) * (BLOCKDIMGER * BLOCKDIMGER));
            }

            //Load Costfunction
            Load4Grams();

            //Cuda has no 4dim. Arrays => Break Costfunction down in one dimension
            double[] d_singleDimQuadgrams = new double[alphabetlength * alphabetlength * alphabetlength * alphabetlength];
            d_singleDimQuadgrams = QuadgramsToSingleDim(alphabetlength);

            //Variables for CUDA
            //totalthreads
            //CudaDeviceVariable<long> vector_totalThreads = new CudaDeviceVariable<long>(1);
            //vector_totalThreads.CopyToDevice(totalThreads);

            //Runkey: Copy Data to Device when calling Kernel.
            CudaDeviceVariable<int> vector_runkey = new CudaDeviceVariable<int>(alphabet.Length);

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

                        totalKeys = totalKeys + totalThreads; //Amount of tested keys tested.

                        //check if in _out are better CostValues than there are at the moment & return position else return "-1"
                        int betterKeyPosition = Betterkey(cuda_out, bestkeycost);

                        if (betterKeyPosition != -1)
                        {
                            //adopt new bestcost and new runkey .
                            bestkeycost = cuda_out[betterKeyPosition];
                            foundbetter = true;
                            int i = betterKeyPosition / alphabetlength;
                            int j = betterKeyPosition % alphabetlength;
                            runkey = ModifyKey(runkey, i, j);
                        }

                    } while (foundbetter);

                    if (StopFlag)
                    {
                        return;
                    }

                    restarts--;
                    pluginProgress(totalRestarts - restarts, totalRestarts);

                    //Output
                    if (bestkeycost > globalBestCost)
                    {
                        globalBestCost = bestkeycost;
                        //Add to bestlist-output:
                        KeyCandidate newKeyCan = new KeyCandidate(runkey, globalBestCost,
                            ConverteNumbersToLetters(UseKeyOnCipher(ciphertext, runkey, ciphertext.Length), alphabet), ConverteNumbersToLetters(runkey, alphabet));
                        //Note: in usekeyOnCipher Method i use chiphertext.length instead of textlength! Because textlength = Length Cuda uses != the whole ciphertext textlength
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
            int[] bestkey = new int[alphabet.Length];
            inplaceAmountOfSymbols = new int[alphabet.Length];
            int besti = 0;
            int bestj = 0;
            int alphabetlength = alphabet.Length; //No need for calculating a few million times. (Performance)
            bool foundbetter;
            bool foundInplace = false;
            totalKeys = 0;
            #endregion{Variables}

            //Load Costfunction
            Load4Grams();

            //Take input and prepare
            int[] ciphertext = MapTextIntoNumberSpace(RemoveInvalidChars(ciphertextString.ToLower(),alphabet), alphabet);
            int length = ciphertext.Length;
            int[] plaintext = new int[length];
            inplaceSpots = new int[alphabet.Length, length];

            var totalRestarts = restarts;

            while (restarts > 0)
            {
                //Generate random key:
                Random random = new Random();
                int[] runkey = BuildRandomKey(random);
                double bestkeycost = double.MinValue;

                //Create first plaintext and analyse places of symbols:
                plaintext = UseKeyOnCipher(ciphertext, runkey, length);
                for (int i = 0; i < alphabet.Length; i++)
                {
                    inplaceAmountOfSymbols[i] = 0;
                }
                AnalyseSymbolPlaces(plaintext, length);

                do
                {
                    foundbetter = false;
                    for (int i = 0; i < alphabetlength; i++)
                    {
                        foundInplace = false;
                        for (int j = 0; j < alphabetlength; j++)
                        {
                            if (i == j) { continue; }

                            //create childkey
                            int[] copykey = (int[])runkey.Clone();
                            copykey = ModifyKey(copykey, i, j);

                            totalKeys++; //Count Keys for Performaceoutput

                            int sub1 = copykey[i];
                            int sub2 = copykey[j];

                            //Inplace swap in text
                            for (int m = 0; m < inplaceAmountOfSymbols[sub1]; m++)
                            {
                                plaintext[inplaceSpots[sub1, m]] = sub2;
                            }

                            for (int m = 0; m < inplaceAmountOfSymbols[sub2]; m++)
                            {
                                plaintext[inplaceSpots[sub2, m]] = sub1;
                            }

                            //Calculate the costfunction
                            double costvalue = CalculateQuadgramCost(_quadgrams, plaintext);

                            //Reverte the CopyKeySubstitution
                            //Inplace swap in text
                            for (int m = 0; m < inplaceAmountOfSymbols[sub2]; m++)
                            {
                                plaintext[inplaceSpots[sub2, m]] = sub2;
                            }

                            for (int m = 0; m < inplaceAmountOfSymbols[sub1]; m++)
                            {
                                plaintext[inplaceSpots[sub1, m]] = sub1;
                            }

                            if (costvalue > bestkeycost) //When found a better key adopt it.
                            {
                                bestkeycost = costvalue;
                                bestkey = copykey;
                                foundbetter = true;
                                foundInplace = true;
                                besti = i;
                                bestj = j;
                            }
                        }

                        //Fast converge take over new key + therefore new resulting plaintext
                        if (foundInplace)
                        {
                            runkey = bestkey;
                            plaintext = InplaceSubstitution(plaintext, runkey[besti], runkey[bestj]);
                        }
                    }
                } while (foundbetter);

                if (StopFlag)
                {
                    return;
                }

                restarts--;
                pluginProgress(totalRestarts - restarts, totalRestarts);

                if (bestkeycost > globalbestkeycost)
                {
                    globalbestkeycost = bestkeycost;
                    //Add to bestlist-output:
                    KeyCandidate newKeyCan = new KeyCandidate(bestkey, globalbestkeycost,
                        ConverteNumbersToLetters(plaintext,alphabet), ConverteNumbersToLetters(bestkey,alphabet));
                    newKeyCan.HillAttack = true;
                    this.updateKeyDisplay(newKeyCan);
                }
            }
            pluginProgress(1, 1);
        }

        #region Methods & Functions

        public static string RemoveInvalidChars(string text, string alphabet)
        {
            var builder = new StringBuilder();
            foreach (var c in text)
            {
                if (alphabet.Contains(c))
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }

        public static int[] MapTextIntoNumberSpace(string text, string alphabet)
        {
            var numbers = new int[text.Length];
            var position = 0;
            foreach (var c in text)
            {
                numbers[position] = alphabet.IndexOf(c);
                position++;
            }
            return numbers;
        }

        private int[] BuildRandomKey(Random randomdev)
        {
            List<int> list = new List<int>();
            for (int i = 0; i < alphabet.Length; i++) list.Add(i);

            int[] key = new int[alphabet.Length];

            for (int i = (alphabet.Length - 1); i >= 0; i--)
            {
                int random = randomdev.Next(0, i + 1);
                key[i] = list[random];
                list.RemoveAt(random);
            }
            return key;
        }

        private int[] ModifyKey(int[] parentKey, int i, int j)
        {
            // swap i with j
            int temp = parentKey[i];
            parentKey[i] = parentKey[j];
            parentKey[j] = temp;

            return (parentKey);
        }

        private int[] UseKeyOnCipher(int[] ciphertext, int[] key, int length)
        {
            int[] plaintext = new int[length];
            for (int i = 0; i < length; i++)
            {
                plaintext[i] = key[ciphertext[i]];
            }

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
        public static string ConverteNumbersToLetters(int[] numbers, string alphabet)
        {
            var builder = new StringBuilder();
            foreach (var i in numbers)
            {
                builder.Append(alphabet[i]);
            }
            return builder.ToString();
        }

        private void AnalyseSymbolPlaces(int[] plaintext, int length)
        {
            for (int i = 0; i < length; i++)
            {
                switch (plaintext[i])
                {
                    case 0:
                        inplaceSpots[0, inplaceAmountOfSymbols[0]] = i;
                        inplaceAmountOfSymbols[0]++;
                        break;
                    case 1:
                        inplaceSpots[1, inplaceAmountOfSymbols[1]] = i;
                        inplaceAmountOfSymbols[1]++;
                        break;
                    case 2:
                        inplaceSpots[2, inplaceAmountOfSymbols[2]] = i;
                        inplaceAmountOfSymbols[2]++;
                        break;
                    case 3:
                        inplaceSpots[3, inplaceAmountOfSymbols[3]] = i;
                        inplaceAmountOfSymbols[3]++;
                        break;
                    case 4:
                        inplaceSpots[4, inplaceAmountOfSymbols[4]] = i;
                        inplaceAmountOfSymbols[4]++;
                        break;
                    case 5:
                        inplaceSpots[5, inplaceAmountOfSymbols[5]] = i;
                        inplaceAmountOfSymbols[5]++;
                        break;
                    case 6:
                        inplaceSpots[6, inplaceAmountOfSymbols[6]] = i;
                        inplaceAmountOfSymbols[6]++;
                        break;
                    case 7:
                        inplaceSpots[7, inplaceAmountOfSymbols[7]] = i;
                        inplaceAmountOfSymbols[7]++;
                        break;
                    case 8:
                        inplaceSpots[8, inplaceAmountOfSymbols[8]] = i;
                        inplaceAmountOfSymbols[8]++;
                        break;
                    case 9:
                        inplaceSpots[9, inplaceAmountOfSymbols[9]] = i;
                        inplaceAmountOfSymbols[9]++;
                        break;
                    case 10:
                        inplaceSpots[10, inplaceAmountOfSymbols[10]] = i;
                        inplaceAmountOfSymbols[10]++;
                        break;
                    case 11:
                        inplaceSpots[11, inplaceAmountOfSymbols[11]] = i;
                        inplaceAmountOfSymbols[11]++;
                        break;
                    case 12:
                        inplaceSpots[12, inplaceAmountOfSymbols[12]] = i;
                        inplaceAmountOfSymbols[12]++;
                        break;
                    case 13:
                        inplaceSpots[13, inplaceAmountOfSymbols[13]] = i;
                        inplaceAmountOfSymbols[13]++;
                        break;
                    case 14:
                        inplaceSpots[14, inplaceAmountOfSymbols[14]] = i;
                        inplaceAmountOfSymbols[14]++;
                        break;
                    case 15:
                        inplaceSpots[15, inplaceAmountOfSymbols[15]] = i;
                        inplaceAmountOfSymbols[15]++;
                        break;
                    case 16:
                        inplaceSpots[16, inplaceAmountOfSymbols[16]] = i;
                        inplaceAmountOfSymbols[16]++; ;
                        break;
                    case 17:
                        inplaceSpots[17, inplaceAmountOfSymbols[17]] = i;
                        inplaceAmountOfSymbols[17]++;
                        break;
                    case 18:
                        inplaceSpots[18, inplaceAmountOfSymbols[18]] = i;
                        inplaceAmountOfSymbols[18]++;
                        break;
                    case 19:
                        inplaceSpots[19, inplaceAmountOfSymbols[19]] = i;
                        inplaceAmountOfSymbols[19]++;
                        break;
                    case 20:
                        inplaceSpots[20, inplaceAmountOfSymbols[20]] = i;
                        inplaceAmountOfSymbols[20]++;
                        break;
                    case 21:
                        inplaceSpots[21, inplaceAmountOfSymbols[21]] = i;
                        inplaceAmountOfSymbols[21]++;
                        break;
                    case 22:
                        inplaceSpots[22, inplaceAmountOfSymbols[22]] = i;
                        inplaceAmountOfSymbols[22]++;
                        break;
                    case 23:
                        inplaceSpots[23, inplaceAmountOfSymbols[23]] = i;
                        inplaceAmountOfSymbols[23]++;
                        break;
                    case 24:
                        inplaceSpots[24, inplaceAmountOfSymbols[24]] = i;
                        inplaceAmountOfSymbols[24]++;
                        break;
                    case 25:
                        inplaceSpots[25, inplaceAmountOfSymbols[25]] = i;
                        inplaceAmountOfSymbols[25]++;
                        break;
                    case 26:
                        inplaceSpots[26, inplaceAmountOfSymbols[26]] = i;
                        inplaceAmountOfSymbols[26]++;
                        break;
                    case 27:
                        inplaceSpots[27, inplaceAmountOfSymbols[27]] = i;
                        inplaceAmountOfSymbols[27]++;
                        break;
                    case 28:
                        inplaceSpots[28, inplaceAmountOfSymbols[28]] = i;
                        inplaceAmountOfSymbols[28]++;
                        break;
                    case 29:
                        inplaceSpots[29, inplaceAmountOfSymbols[29]] = i;
                        inplaceAmountOfSymbols[29]++;
                        break;
                    default:
                        Console.WriteLine("Error: analyseSymbolPlaces Switch reached default");
                        break;
                }
            }
        }

        public static double CalculateQuadgramCost(double[, , ,] ngrams4, int[] plaintext)
        {
            double value = 0;
            var end = plaintext.Length - 3;

            for (var i = 0; i < end; i++)
            {
                value += ngrams4[plaintext[i], plaintext[i + 1], plaintext[i + 2], plaintext[i + 3]];
            }
            return value;
        }

        private void Load4Grams()
        {
            string filename = "";
            if (alphabet.Length == 26) filename = "en-4gram-nocs.bin";
            if (alphabet.Length == 27) filename = "es-4gram-nocs.bin";
            if (alphabet.Length == 30) filename = "de-4gram-nocs.bin";

            _quadgrams = new double[alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length];

            using (var fileStream = new FileStream(Path.Combine(DirectoryHelper.DirectoryLanguageStatistics, filename), FileMode.Open, FileAccess.Read))
            {
                using (var reader = new BinaryReader(fileStream))
                {
                    for (int i = 0; i < alphabet.Length; i++)
                    {
                        for (int j = 0; j < alphabet.Length; j++)
                        {
                            for (int k = 0; k < alphabet.Length; k++)
                            {
                                for (int l = 0; l < alphabet.Length; l++)
                                {
                                    var bytes = reader.ReadBytes(8);
                                    _quadgrams[i, j, k, l] = BitConverter.ToDouble(bytes, 0);
                                }
                            }
                        }
                    }
                }
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

        private int Betterkey(double[] costvlaues, double bestcostvalue)
        {
            //This method searches for the best possible key in the costvlauesset that is better then the bestcostvlaue at the moment
            //When there is no better key return -1
            int position = -1;

            for (int i = 0; i < costvlaues.Length; i++)
            {
                if (costvlaues[i] > bestcostvalue)
                {
                    position = i;
                    bestcostvalue = costvlaues[i];
                }
            }
            return position;
        }

        private int[] CutCiphertext(int[] ciphertext)
        {
            //Cuts the Ciphertext to a maximium length of 1000 Symbols. Else it will get too big to handle for Kernel.
            int[] cuttedCipher;

            if (ciphertext.Length < 1000)
            {
                cuttedCipher = new int[ciphertext.Length];
                cuttedCipher = ciphertext;
            }
            else
            {
                cuttedCipher = new int[1000];
                for (int i = 0; i < 1000; i++)
                {
                    cuttedCipher[i] = ciphertext[i];
                }
            }
            return cuttedCipher;
        }

        private double[] QuadgramsToSingleDim(int al)
        {
            // al = alphabetlength
            double[] singleDimQuadgrams = new double[al * al * al * al];

            for (int dim4 = 0; dim4 < al; dim4++)
            {
                for (int dim3 = 0; dim3 < al; dim3++)
                {
                    for (int dim2 = 0; dim2 < al; dim2++)
                    {
                        for (int dim1 = 0; dim1 < al; dim1++)
                        {
                            singleDimQuadgrams[dim1 + (dim2 * al) + (dim3 * (al * al)) + (dim4 * (al * al * al))] = _quadgrams[dim1, dim2, dim3, dim4];
                        }
                    }
                }
            }
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
