﻿#define _DEBUG_

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;
using System.Windows;
using Keccak.Properties;

namespace Cryptool.Plugins.Keccak
{
    public class Sponge
    {
        #region variables

        private int rate, capacity, laneSize;
        private byte[] state;
        private Keccak_f keccak_f;
        private KeccakPres pres;
        private Keccak plugin;

        private int progressionSteps, progressionStepCounter;

        private int[] widthOfPermutation = { 25, 50, 100, 200, 400, 800, 1600 };

        #endregion

        public Sponge(int rate, int capacity, ref KeccakPres pres, Keccak plugin, int progressionSteps)
        {
            Debug.Assert(rate > 0);
            Debug.Assert(widthOfPermutation.Contains(rate + capacity));

            this.rate = rate;
            this.capacity = capacity;
            laneSize = (rate + capacity) / 25;

            keccak_f = new Keccak_f(capacity + rate, ref state, ref pres, plugin);

            state = new byte[capacity + rate];
            this.pres = pres;
            this.plugin = plugin;
            this.progressionSteps = progressionSteps;
        }

        public void Absorb(byte[] input)
        {
            byte[] paddedInputBits;
            byte[][] inputBlocks;

            /* pad message */
            paddedInputBits = Pad(input);

            /* split padded message into blocks of equal length */
            inputBlocks = SplitBlocks(paddedInputBits);

            #region presentation execution phase information
            if (pres.IsVisible && !pres.skipPresentation)
            {
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.labelCurrentPhase.Content = Resources.PresExecution;
                    pres.labelCurrentStep.Content = Resources.PresOverview;

                    pres.labelExecutionInfoStateSize.Content = string.Format(Resources.PresExecutionInfoStateSize, (rate + capacity));
                    pres.labelExecutionInfoCapacitySize.Content = string.Format(Resources.PresExecutionInfoCapacitySize, capacity);
                    pres.labelExecutionInfoRateSize.Content = string.Format(Resources.PresExecutionInfoBitRateSize, rate);
                    pres.labelExecutionMessageLength.Content = string.Format(Resources.PresExecutionMessageLength, input.Length, input.Length / 8);
                    pres.labelExecutionPaddedMessageLength.Content = string.Format(Resources.PresExecutionPaddedMessageLength, paddedInputBits.Length, paddedInputBits.Length / 8);
                    pres.labelExecutionNumberOfBlocks.Content = string.Format(Resources.PresExecutionNumberOfBlocks, inputBlocks.Length);

                    pres.executionInfoCanvas.Visibility = Visibility.Visible;
                }, null);
                
                AutoResetEvent buttonNextClickedEvent = pres.buttonNextClickedEvent;
                buttonNextClickedEvent.WaitOne();

                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.executionInfoCanvas.Visibility = Visibility.Hidden;
                }, null);
            }
            #endregion

            #if _DEBUG_
            Console.WriteLine("#Sponge: the input of length {0} bits is padded to {1} bits\n" +
                "#Sponge: the padded input is splitted into {2} block(s) of size {3} bit\n", input.Length, paddedInputBits.Length, inputBlocks.Length, inputBlocks[0].Length);
            Console.WriteLine("#Sponge: begin absorbing phase");            
            #endif
            int blocksCounter = 1;
            progressionStepCounter = 0;

            if (pres.IsVisible && !pres.skipPresentation)
            {
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.spInfo.Visibility = Visibility.Visible;
                }, null);
            }
             
            /* absorb and permute */
            foreach (byte[] block in inputBlocks)
            {             
                #if _DEBUG_
                Console.WriteLine("#Sponge: XORing input block #{0} on state\n", blocksCounter);                
                #endif                           

                #region presentation absorbing phase
                if (pres.IsVisible && !pres.skipPresentation)
                {
                    string stateStr = KeccakHashFunction.GetByteArrayAsString(state, laneSize);
                    string blockStr = KeccakHashFunction.GetByteArrayAsString(block, laneSize);

                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.labelRound.Content = "";
                        pres.labelBlock.Content = string.Format("Block {0}/{1}", blocksCounter, inputBlocks.Length);

                        pres.textBlockExplanation.Text = string.Format(Resources.PresAbsorbingPhaseExplanation, blocksCounter);

                        pres.imgBlankPage.Visibility = Visibility.Visible;
                        pres.labelCurrentPhase.Content = Resources.PresAbsorbingPhase;
                        pres.labelCurrentStep.Content = "";
                        pres.labelAbsorbGridBlockCounter.Content = string.Format("Block #{0}/{1}", blocksCounter, inputBlocks.Length);
                        pres.labelNewState.Visibility = Visibility.Visible;
                        pres.labelOldState.Content = Resources.PresOldState;

                        pres.textBlockStateBeforeAbsorb.Text = stateStr;
                        pres.textBlockBlockToAbsorb.Text = blockStr;

                        pres.absorbGrid.Visibility = Visibility.Visible;

                        pres.buttonSkipPermutation.IsEnabled = true;
                        pres.buttonSkip.IsEnabled = false;
                        pres.buttonAutostep.IsEnabled = false;
                        pres.autostepSpeedSlider.IsEnabled = false;
                    }, null);

                    //AutoResetEvent buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    //buttonNextClickedEvent.WaitOne();
                }
                #endregion

                XorBlockOnState(block);

                plugin.ProgressChanged((double)progressionStepCounter + (1.0 / 6.0), (double)progressionSteps);

                #region presentation absorbing phase
                if (pres.IsVisible && !pres.skipPresentation)
                {
                    string stateStr = KeccakHashFunction.GetByteArrayAsString(state, laneSize);

                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {                        
                        pres.textBlockStateAfterAbsorb.Text = stateStr;
                    }, null);

                    AutoResetEvent buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }

                if (pres.IsVisible || pres.skipPresentation)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.textBlockExplanation.Text = "";
                        pres.labelAbsorbGridBlockCounter.Content = "";
                        pres.absorbGrid.Visibility = Visibility.Hidden;
                        pres.imgBlankPage.Visibility = Visibility.Hidden;
                        pres.labelNewState.Visibility = Visibility.Hidden;
                        pres.labelOldState.Content = "";
                        pres.textBlockStateBeforeAbsorb.Text = "";
                        pres.textBlockBlockToAbsorb.Text = "";
                        pres.textBlockStateAfterAbsorb.Text = "";
                        pres.labelCurrentPhase.Content = "";
                        pres.labelCurrentStep.Content = "";
                    }, null);
                }
                #endregion

                keccak_f.Permute(ref state, progressionStepCounter, progressionSteps);

                blocksCounter++;
                progressionStepCounter++;
                pres.skipPermutation = false;
            }

            #if _DEBUG_
            Console.WriteLine("\n#Sponge: absorbing done!");
            blocksCounter++;
            #endif
        }

        public byte[] Squeeze(int outputLength)
        {
            byte[] output = new byte[outputLength];

            #if _DEBUG_
            Console.WriteLine("\n#Sponge: begin squeezing phase");
            #endif

            if (outputLength <= rate)
            {
                #if _DEBUG_
                Console.WriteLine("#Sponge: the output length is smaller or equal to the bit rate size ({0} <= {1})", outputLength, rate);
                Console.WriteLine("#Sponge: -> squeeze output from state");
                #endif

                /* append `outputLength` bits of the state to the output */
                output = KeccakHashFunction.SubArray(state, 0, outputLength);

                /* presentation squeezing phase*/
                #region presentation squeezing phase
                if (pres.IsVisible && !pres.skipPresentation)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.labelRound.Content = "";
                        pres.labelBlock.Content = "";
                    }, null);
                }

                if (pres.IsVisible && !pres.skipPresentation)
                {
                    string stateStr = KeccakHashFunction.GetByteArrayAsString(state, laneSize);
                    string outputStr = KeccakHashFunction.GetByteArrayAsString(output, laneSize);

                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.textBlockExplanation.Text = string.Format(Resources.PresSqueezingPhaseExplanation, outputLength); // If the hash value is larger than this part the state is repeatedly permuted by Keccak-f such that sufficient output can be extracted.", outputLength);

                        pres.imgBlankPage.Visibility = Visibility.Visible;
                        pres.labelCurrentPhase.Content = Resources.PresSqueezingPhase;
                        pres.labelOutput.Visibility = Visibility.Visible;
                        pres.textBlockStateBeforeAbsorb.Text = stateStr;
                        pres.absorbGrid.Visibility = Visibility.Visible;
                        pres.labelOldState.Content = Resources.PresState;
                        
                        pres.textBlockStateAfterAbsorb.Text = outputStr;         
                    }, null);
                }
                #endregion

            }
            else
            {
                int remainingOutput = outputLength, i = 0;

                #if _DEBUG_
                int squeezingRounds = remainingOutput % rate == 0 ? (int)(remainingOutput / rate - 1) : (remainingOutput / rate);
                Console.WriteLine("#Sponge: the output length is larger than the bit rate ({0} > {1})", outputLength, rate);
                Console.WriteLine("#Sponge: -> squeeze output from state iteratively ({0} iteration(s) required)\n", squeezingRounds);
                #endif

                /* append size of `rate` bits of the state to the output */
                while (remainingOutput > rate)
                {
                    Array.Copy(state, 0, output, i++ * rate, rate);

                    #if _DEBUG_
                    Console.WriteLine("#Sponge: squeeze iteration #{0}\n", i);
                    #endif
                    
                    remainingOutput -= rate;

                    plugin.ProgressChanged((double)progressionStepCounter + (1.0 / 6.0), (double)progressionSteps);
                    keccak_f.Permute(ref state, progressionStepCounter, progressionSteps);
                    progressionStepCounter++;
                }

                if (remainingOutput > 0)
                {
                    /* append remaining bits of the state to the output to fit the output length */
                    Array.Copy(state, 0, output, i * rate, remainingOutput);
                }
            }

            #if _DEBUG_
            Console.WriteLine("#Sponge: squeezing done!\n");
            #endif

            if (pres.IsVisible)
            {
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.spInfo.Visibility = Visibility.Hidden;
                    //pres.labelCurrentStep.Content = "";
                    //pres.labelCurrentPhase.Content = "";
                }, null);
            }

            return output;
        }

        #region helper methods

        private void XorBlockOnState(byte[] block)
        {
            Debug.Assert(block.Length == rate);
            for (int i = 0; i < block.Length; i++)
            {
                state[i] ^= block[i];
            }
        }

        public byte[][] SplitBlocks(byte[] paddedInputBits)
        {
            Debug.Assert(paddedInputBits.Length % rate == 0);
            int numberOfBlocks = paddedInputBits.Length / rate;

            byte[][] inputBlocks = null;
            byte[] block;

            /* split message into blocks of size `rate` */
            List<byte[]> inputBlockList = new List<byte[]>(numberOfBlocks);

            for (int i = 0; i < numberOfBlocks; i++)
            {
                block = KeccakHashFunction.SubArray(paddedInputBits, i * rate, rate);
                inputBlockList.Add(block);
            }

            inputBlocks = inputBlockList.ToArray();

            return inputBlocks;
        }

        public byte[] Pad(byte[] messageBits)
        {
            byte[] paddedInput = null;


            byte padStart = 0x01;
            byte padEnd = 0x01;
            byte padByte = 0x00;
            byte[] tmpPaddedInput;

            List<byte> padding = new List<byte>();

            /* missing bits to fit block size */
            int missingBits = rate - (messageBits.Length % rate);

            /* if only one bit is missing, pad one bit plus one block */
            if (missingBits == 1)
            {
                missingBits += rate;
            }

            tmpPaddedInput = new byte[messageBits.Length + missingBits];

            if (missingBits == 2)
            {
                padding.Add(padStart);
                padding.Add(padEnd);
            }
            else if (missingBits >= 2)
            {
                padding.Add(padStart);
                for (int i = 0; i < missingBits - 2; i++)
                {
                    padding.Add(padByte);
                }
                padding.Add(padEnd);
            }

            Debug.Assert(padding.Count() == missingBits);
            tmpPaddedInput = messageBits.Concat(padding.ToArray()).ToArray();

            Debug.Assert((tmpPaddedInput.Count() % rate) == 0);
            paddedInput = tmpPaddedInput.ToArray();


            return paddedInput;
        }

        #endregion

    }
}
