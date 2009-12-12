﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.Control;
using Cryptool.PluginBase;
using System.Threading;
using KeySearcher;

/*
 * TODO:
 * - Serializing ResultList is buggy when converting decryption byte[] 
 *   to String and following deserializing in P2PManager
 * - Error gets thrown after ending bruteforcing once. "set-Property not found" (oder so)
 *   I think it's an error in KeySearcher_IControl --> method keySearcher_OnAllMasterControlsInitialized
 *   where I set the KeyPattern and additional the WildcardKey manual...
 */

namespace Cryptool.Plugins.PeerToPeer
{
    public enum EncryptionPatternLength
    {
        AES = 16,
        //SDES = x,
        DES = 8
    }

    public enum KeyPatternLength
    {
        AES = 271,
        //SDES = x,
        DES = 135
    }

    public class P2PWorkerBase : P2PSubscriberBase
    {
        #region events and variables

        // Events
        public delegate void KeyPatternReceived(KeyPattern pattern);
        /// <summary>
        /// Every time when this worker receives a new KeyPattern from the Manager, this event will be fired.
        /// </summary>
        public event KeyPatternReceived OnKeyPatternReceived;
        public delegate void FinishedBruteforcingThePattern(KeyPattern pattern);
        /// <summary>
        /// fired when bruteforcing the actual pattern - assigned from the Manager - was finisher. 
        /// Attention: No indication that the worker doesn't have to bruteforce any other assigned Patterns!
        /// </summary>
        public event FinishedBruteforcingThePattern OnFinishedBruteforcingThePattern;

        // Variables
        /// <summary>
        /// only goal: validate incoming KeyPatterns
        /// </summary>
        private KeyPattern patternForValidateIncomingPatterns;
        private IControlKeySearcher keySearcherControl;
        private IControlCost costControl;
        private IControlEncryption encryptControl;
        /// <summary>
        /// Flag to check if this worker is currently working on a Pattern.
        /// So you must wait till current job is finished.
        /// </summary>
        private bool currentlyWorking = false;
        /// <summary>
        /// waiting job Queue, if jobs received, but the worker is still working on another job
        /// </summary>
        private Queue<KeyPattern> waitingJobList;

        #endregion

        public P2PWorkerBase(IP2PControl p2pControl, IControlKeySearcher keySearcherControl)
            : base(p2pControl)
        {
            this.keySearcherControl = keySearcherControl;
            this.keySearcherControl.OnEndedBruteforcing += new KeySearcher.KeySearcher.BruteforcingEnded(keySearcherControl_OnEndedBruteforcing);
            this.encryptControl = this.keySearcherControl.GetEncyptionControl();
            this.costControl = this.keySearcherControl.GetCostControl();
            if (!InitializeTestKeyPattern(encryptControl))
                throw (new Exception("P2PWorkerBase: Encryption Type not supported"));
            waitingJobList = new Queue<KeyPattern>();
        }

        #region Only for testing reasons (Build a TestPattern to check the validity of the incoming KeyPattern)

        private bool InitializeTestKeyPattern(IControlEncryption encryptControl)
        {
            bool result = false;
            string sPattern = encryptControl.getKeyPattern();
            patternForValidateIncomingPatterns = new KeyPattern(sPattern);
            int len = sPattern.ToString().Length;

            if (len == (int)KeyPatternLength.AES)
            {
                // "30-30-30-30-30-30-30-30-30-30-30-30-30-**-**-**"
                string sKeyPattern = GetKeyForInit(EncryptionPatternLength.AES, 3);
                this.patternForValidateIncomingPatterns.WildcardKey = sKeyPattern;
                result = true;
            }
            else if (len == (int)KeyPatternLength.DES)
            {
                // "30-30-30-30-30-**-**-**"
                string sKeyPattern = GetKeyForInit(EncryptionPatternLength.DES, 3);
                this.patternForValidateIncomingPatterns.WildcardKey = sKeyPattern;
                result = true;
            }
            return result;
        }

        private string GetKeyForInit(EncryptionPatternLength encryptLen, int wildCardAmount)
        {
            int iEncryptLen = (int)encryptLen;

            if (iEncryptLen < wildCardAmount)
                return null;

            StringBuilder sbRet = new StringBuilder("30");
            for (int i = 0; i < iEncryptLen - wildCardAmount - 1; i++)
            {
                sbRet.Append("-30");
            }
            for (int i = 0; i < wildCardAmount; i++)
            {
                sbRet.Append("-**");
            }
            return sbRet.ToString();
        }

        #endregion

        /// <summary>
        /// Checks whether the incoming data are a valid KeyPattern object. Then it checks
        /// if the worker is still working currently - than enqueue new Pattern to waitingJobList.
        /// </summary>
        /// <param name="senderId"></param>
        /// <param name="sData"></param>
        protected override void HandleIncomingData(PeerId senderId, string sData)
        {
            // returns null if the data aren't a valid KeyPattern.
            KeyPattern receivedKeyPattern = patternForValidateIncomingPatterns.DeserializeFromString(sData);
            if (receivedKeyPattern != null)
            {
                // only one Pattern can be bruteforced concurrently, 
                // so other incoming Patterns have to wait
                if (this.currentlyWorking)
                {
                    this.waitingJobList.Enqueue(receivedKeyPattern);
                }
                else
                {
                    // TODO: to display the incoming KeyPattern in the OutputText, not the perfect way...
                    // base.HandleIncomingData(senderId, sData);

                    Thread processingThread = new Thread(new ParameterizedThreadStart(this.StartProcessing));
                    processingThread.Start(receivedKeyPattern);
                }
            }
            else
            {
                base.HandleIncomingData(senderId, sData);
            }
        }

        // necessary, because Thread-starting doesn't allow other parameters than object
        private void StartProcessing(object receivedKeyPattern)
        {
            if (receivedKeyPattern is KeyPattern)
                StartProcessing(receivedKeyPattern as KeyPattern);
        }

        /// <summary>
        /// Main method for processing a new job. Started in own thread!
        /// </summary>
        /// <param name="receivedKeyPattern">a valid and full initialized KeyPattern</param>
        private void StartProcessing(KeyPattern receivedKeyPattern)
        {
            GuiLogging("Starting Bruteforcing the incoming pattern: '" + receivedKeyPattern.ToString() + "'", NotificationLevel.Info);

            if (OnKeyPatternReceived != null)
                OnKeyPatternReceived(receivedKeyPattern);
            this.currentlyWorking = true;
            // Commit pattern to the KeySearcherControl and wait for result(s)
            this.keySearcherControl.StartBruteforcing(receivedKeyPattern);
        }

        private void keySearcherControl_OnEndedBruteforcing(LinkedList<KeySearcher.KeySearcher.ValueKey> top10List)
        {
            if (top10List != null)
            {
                KeySearcher.KeySearcher.ValueKey bestResult = top10List.First<KeySearcher.KeySearcher.ValueKey>();
                GuiLogging("Bruteforcing Ended. Best key result: '" + bestResult.key + "'. Coefficient value: "
                    + bestResult.value.ToString() + ". Decrypted result: '" + UTF8Encoding.UTF8.GetString(bestResult.decryption)
                    + "'", NotificationLevel.Info);
                SolutionFound(SerializeKeySearcherResult(top10List));
                GuiLogging("Serialized result list sended to Managing-Peer.", NotificationLevel.Debug);
            }
            // if there are any Jobs in the waiting list, process them now successively
            if (this.waitingJobList.Count > 0)
            {
                StartProcessing(this.waitingJobList.Dequeue());
            }
            else
                // set flag to false, because bruteforcing has been finished
                this.currentlyWorking = false;
        }

        // only the first byte of the decryption byte[] are serialized - maybe avoiding the splitting error in P2PManager.Deserialize...
        /*
         * serialization information: 3 fields per data set in the following order: 
         * 1) value (double) 
         * 2) key (string) 
         * 3) decryption (byte[])
         */
        private string seperator = "#;#";
        private string dataSetSeperator = "|**|";
        private string SerializeKeySearcherResult(LinkedList<KeySearcher.KeySearcher.ValueKey> top10List)
        {
            StringBuilder sbRet = new StringBuilder();
            foreach (KeySearcher.KeySearcher.ValueKey valKey in top10List)
            {
                //sbRet.Append(valKey.value.ToString() + seperator + valKey.key + seperator + UTF8Encoding.UTF8.GetString(valKey.decryption) + dataSetSeperator);
                sbRet.Append(valKey.value.ToString() + seperator + valKey.key + seperator + "replaced" + dataSetSeperator);
            }
            string sRet = sbRet.ToString();
            // cut off last dataSetSeperator
            sRet = sRet.Substring(0, sRet.Length - dataSetSeperator.Length);
            return sRet;
        }
    }
}
