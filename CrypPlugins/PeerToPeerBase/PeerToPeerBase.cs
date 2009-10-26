﻿/* Copyright 2009 Team CrypTool (Christian Arnold), Uni Duisburg-Essen

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
using PeersAtPlay.P2PStorage.DHT;
using PeersAtPlay.P2PStorage.FullMeshDHT;
using PeersAtPlay.P2PLink.SimpleSnalNG;
using PeersAtPlay.P2POverlay.Bootstrapper;
using PeersAtPlay.P2POverlay;
using PeersAtPlay.P2POverlay.Bootstrapper.LocalMachineBootstrapper;
using PeersAtPlay.P2POverlay.FullMeshOverlay;
using PeersAtPlay.P2PLink;
using PeersAtPlay.P2POverlay.Bootstrapper.IrcBootstrapper;
using System.Threading;
using Cryptool.PluginBase.Control;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;
using Cryptool.PluginBase.IO;

/*
 * Synchronous functions successfully tested (store, retrieve)
 * Standard connection for test issues is LocalMachineBootstrapper. 
 * IrcBootstrapper also works, but it's to lame for testing issues!
 * 
 * TODO:
 * Testing asynchronous methods incl. EventHandlers
 * 
 * TO DO in Related Projects:
 * - Change stand-alone P2P-Apps to IControl-Slaves from one P2P-Master (store, load, (remove))
 */
namespace Cryptool.Plugins.PeerToPeer
{
    /// <summary>
    /// Wrapper class to integrate peer@play environment into CrypTool. 
    /// This class synchronizes asynchronous methods for easier usage in CT2. For future
    /// </summary>
    public class P2PBase
    {
        #region Delegates and Events for asynchronous p2p functions

        public delegate void SystemJoined();
        public event SystemJoined OnSystemJoined;

        public delegate void SystemLeft();
        public event SystemLeft OnSystemLeft;

        public delegate void P2PMessageReceived(string sMsg);
        public event P2PMessageReceived OnP2PMessageReceived;

        /// <summary>
        /// returns true if key-value-pair was successfully stored in the DHT
        /// </summary>
        /// <param name="result"></param>
        public delegate void DHTStoreCompleted(bool result);
        public event DHTStoreCompleted OnDhtStore_Completed;

        public delegate void DHTLoadCompleted(byte[] loadedData);
        public event DHTLoadCompleted OnDhtLoad_Completed;

        /// <summary>
        /// returns true if key was found and removed successfully from the DHT
        /// </summary>
        /// <param name="result"></param>
        public delegate void DHTRemoveCompleted(bool result);
        public event DHTRemoveCompleted OnDhtRemove_Completed;

        #endregion


        #region Variables

        private IDHT dht;
        private IP2PLinkManager linkmanager;
        private IBootstrapper bootstrapper;
        private P2POverlay overlay;
        private AutoResetEvent systemJoined;
        private AutoResetEvent systemLeft;

        /// <summary>
        /// Dictionary for synchronizing asynchronous DHT retrieves.
        /// Cryptool doesn't offers an asynchronous environment, so this workaround is necessary
        /// </summary>
        private Dictionary<Guid, ResponseWait> waitDict;

        #endregion

        public P2PBase()
        {
            this.waitDict = new Dictionary<Guid, ResponseWait>();
            this.systemJoined = new AutoResetEvent(false);
            this.systemLeft = new AutoResetEvent(false);
        }

        #region Basic P2P Methods (Init, Start, Stop) - synch and asynch

        /// <summary>
        /// Initializing is the first step to build a new or access an existing p2p network
        /// </summary>
        /// <param name="sUserName">Choose an individual name for the user</param>
        /// <param name="sWorldName">fundamental: two peers are only in the SAME P2P system, when they initialized the SAME WORLD!</param>
        public void Initialize(string sUserName, string sWorldName)
        {
            //snal = secure network abstraction layer
            this.linkmanager = new Snal();
            //LocalMachineBootstrapper = only local connection (runs only on one machine)
            //for more machines use this line:
            
            //this.bootstrapper = new IrcBootstrapper();
            this.bootstrapper = new LocalMachineBootstrapper();

            // changing overlay example: this.overlay = new ChordOverlay();
            this.overlay = new FullMeshOverlay();
            //changing overlay example: this.overlay = new ExampleDHT();
            this.dht = new FullMeshDHT();

            this.dht.Initialize(sUserName, sWorldName, this.overlay, this.bootstrapper, this.linkmanager, null);

            this.dht.MessageReceived += new EventHandler<MessageReceived>(OnDHT_MessageReceived);
            this.dht.SystemJoined += new EventHandler(OnDHT_SystemJoined);
            this.dht.SystemLeft += new EventHandler(OnDHT_SystemLeft);
        }

        /// <summary>
        /// Starts the P2P System. When the given P2P world doesn't exist yet, 
        /// inclusive creating the and bootstrapping to the P2P network.
        /// In either case joining the P2P world.
        /// This synchronized method returns true not before the peer has 
        /// successfully joined the network (this may take one or two minutes).
        /// </summary>
        /// <returns>True, if the peer has completely joined the p2p network</returns>
        public bool SynchStart()
        {
            //Start != system joined
            //Only starts the system asynchronous, the possible callback is useless, 
            //because it's invoked before the peer completly joined the P2P system
            this.dht.BeginStart(null);
            //Wait for event SystemJoined. When it's invoked, the peer completly joined the P2P system
            this.systemJoined.WaitOne();
            return true;
        }

        /// <summary>
        /// Disjoins the peer from the system. The P2P system survive while one peer is still in the network.
        /// </summary>
        /// <returns>True, if the peer has completely disjoined the p2p network</returns>
        public bool SynchStop()
        {
            if (this.dht != null)
            {
                this.dht.BeginStop(null);
                //wait till systemLeft Event is invoked
                this.systemLeft.WaitOne();
            }
            return true;
        }


        /// <summary>
        /// Asynchronously starting the peer. When the given P2P world doesn't 
        /// exist yet, inclusive creating the and bootstrapping to the P2P network.
        /// In either case joining the P2P world. To ensure that peer has successfully
        /// joined the p2p world, catch the event OnSystemJoined.
        /// </summary>
        public void AsynchStart()
        {
            // no callback usefull, because starting and joining isn't the same
            // everything else is done by the EventHandler OnDHT_SystemJoined
            this.dht.BeginStart(null);
        }

        /// <summary>
        /// Asynchronously disjoining the actual peer of the p2p system. To ensure
        /// disjoining, catch the event OnDHT_SystemLeft.
        /// </summary>
        public void AsynchStop()
        {
            if (this.dht != null)
            {
                // no callback usefull.
                // Everything else is done by the EventHandler OnDHT_SystemLeft
                this.dht.BeginStop(null);
            }
        }

        #endregion

        #region Event Handling (System Joined, Left and Message Received)

        private void OnDHT_SystemJoined(object sender, EventArgs e)
        {
            if (OnSystemJoined != null)
                OnSystemJoined();
            this.systemJoined.Set();
        }

        private void OnDHT_SystemLeft(object sender, EventArgs e)
        {
            if (OnSystemLeft != null)
                OnSystemLeft();
            this.systemLeft.Set();
        }

        private void OnDHT_MessageReceived(object sender, MessageReceived e)
        {
            if (OnP2PMessageReceived != null)
                OnP2PMessageReceived("Source: " + e.Source + ", Data:" + e.Data);
        }

        #endregion

        /*
         * Attention: The asynchronous methods are not tested at the moment
         */
        #region Asynchronous Methods incl. Callbacks

        /// <summary>
        /// Asynchronously retrieving a key from the DHT. To get value, catch 
        /// event OnDhtLoad_Completed.
        /// </summary>
        /// <param name="sKey">Existing key in DHT</param>
        public void AsynchRetrieve(string sKey)
        {
            Guid g = this.dht.Retrieve(OnAsynchRetrieve_Completed, sKey);
        }
        private void OnAsynchRetrieve_Completed(RetrieveResult rr)
        {
            if (OnDhtLoad_Completed != null)
            {
                OnDhtLoad_Completed(rr.Data);
            }
        }

        /// <summary>
        /// Asynchronously storing a Key-Value-Pair in the DHT. To ensure that 
        /// storing is completed, catch event OnDhtStore_Completed.
        /// </summary>
        /// <param name="sKey"></param>
        /// <param name="sValue"></param>
        public void AsynchStore(string sKey, string sValue)
        {
            this.dht.Store(OnAsynchStore_Completed, sKey, UTF8Encoding.UTF8.GetBytes(sValue));
        }

        private void OnAsynchStore_Completed(StoreResult sr)
        {
            if (OnDhtStore_Completed != null)
            {
                if (sr.Status == OperationStatus.Success)
                    OnDhtStore_Completed(true);
                else
                    OnDhtStore_Completed(false);
            }
                
        }

        /// <summary>
        /// Asynchronously removing an existing key out of the DHT. To ensure
        /// that removing is completed, catch event OnDhtRemove_Completed.
        /// </summary>
        /// <param name="sKey"></param>
        public void AsynchRemove(string sKey)
        {
            this.dht.Remove(OnAsynchRemove_Completed, sKey);
        }
        private void OnAsynchRemove_Completed(RemoveResult rr)
        {
            if (OnDhtRemove_Completed != null)
            {
                if(rr.Status == OperationStatus.Success)
                    OnDhtRemove_Completed(true);
                else
                    OnDhtRemove_Completed(false);
            }
        }

        #endregion

        #region Synchronous Methods incl. Callbacks

        /// <summary>
        /// Stores a value in the DHT at the given key
        /// </summary>
        /// <param name="sKey">Key of DHT Entry</param>
        /// <param name="sValue">Value of DHT Entry</param>
        /// <returns>True, when storing is completed!</returns>
        public bool SynchStore(string sKey, string sValue)
        {
            AutoResetEvent are = new AutoResetEvent(false);
            // this method returns always a GUID to distinguish between asynchronous actions
            Guid g = this.dht.Store(OnSynchStoreCompleted, sKey, UTF8Encoding.UTF8.GetBytes(sValue));

            ResponseWait rw = new ResponseWait() { WaitHandle = are };

            waitDict.Add(g, rw);
            //blocking till response
            are.WaitOne();
            return true;
        }

        /// <summary>
        /// Get the value of the given DHT Key or null, if it doesn't exist.
        /// For synchronous environments use the Synch* methods.
        /// </summary>
        /// <param name="sKey">Key of DHT Entry</param>
        /// <returns>Value of DHT Entry</returns>
        public byte[] SynchRetrieve(string sKey)
        {
            AutoResetEvent are = new AutoResetEvent(false);
            // this method returns always a GUID to distinguish between asynchronous actions
            Guid g = this.dht.Retrieve(OnSynchRetrieveCompleted, sKey);
            
            ResponseWait rw = new ResponseWait() {WaitHandle = are };
            
            waitDict.Add(g,rw  );
            // blocking till response
            are.WaitOne();
            //Rückgabe der Daten
            return rw.Message;
        }

        /// <summary>
        /// Removes a key/value pair out of the DHT
        /// </summary>
        /// <param name="sKey">Key of the DHT Entry</param>
        /// <returns>True, when removing is completed!</returns>
        public bool SynchRemove(string sKey)
        {
            AutoResetEvent are = new AutoResetEvent(false);
            // this method returns always a GUID to distinguish between asynchronous actions
            Guid g = this.dht.Remove(OnSynchRemoveCompleted, sKey);

            ResponseWait rw = new ResponseWait() { WaitHandle = are };

            waitDict.Add(g, rw);
            // blocking till response
            are.WaitOne();
            return true;
        }

        /// <summary>
        /// Callback for a the synchronized store method
        /// </summary>
        /// <param name="rr"></param>
        private void OnSynchStoreCompleted(StoreResult sr)
        {
            ResponseWait rw;
            if (this.waitDict.TryGetValue(sr.Guid, out rw))
            {
                rw.Message = UTF8Encoding.UTF8.GetBytes(sr.Status.ToString());

                //unblock WaitHandle in the synchronous method
                rw.WaitHandle.Set();
                // don't know if this accelerates the system...
                this.waitDict.Remove(sr.Guid);
            }
        }

        /// <summary>
        /// Callback for a the synchronized retrieval method
        /// </summary>
        /// <param name="rr"></param>
        private void OnSynchRetrieveCompleted(RetrieveResult rr)
        {
            ResponseWait rw;

            if (this.waitDict.TryGetValue(rr.Guid, out rw))
            {
                rw.Message = rr.Data;

                //unblock WaitHandle in the synchronous method
                rw.WaitHandle.Set(); 
                // don't know if this accelerates the system...
                this.waitDict.Remove(rr.Guid);
            }
        }

        /// <summary>
        /// Callback for a the synchronized remove method
        /// </summary>
        /// <param name="rr"></param>
        private void OnSynchRemoveCompleted(RemoveResult rr)
        {
            ResponseWait rw;
            if (this.waitDict.TryGetValue(rr.Guid, out rw))
            {
                rw.Message = UTF8Encoding.UTF8.GetBytes(rr.Status.ToString());

                //unblock WaitHandle in the synchronous method
                rw.WaitHandle.Set();
                // don't know if this accelerates the system...
                this.waitDict.Remove(rr.Guid);
            }
        }

        #endregion

        /// <summary>
        /// To log the internal state in the Monitoring Software of P@play
        /// </summary>
        public void LogInternalState()
        {
            if (this.dht != null)
            {
                this.dht.LogInternalState();
            }
        }
    }
}
