﻿/*                              
   Copyright 2010 Sven Rech, Uni Duisburg-Essen

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
using System.IO.Compression;
using System.IO;
using Cryptool.P2P;
using System.Numerics;
using System.Diagnostics;
using System.Collections;
using System.Threading;
using System.Windows.Threading;
using System.Runtime.Serialization.Formatters.Binary;

namespace Cryptool.Plugins.QuadraticSieve
{
    class PeerToPeer
    {
        private struct PeerPerformanceInformations
        {
            public DateTime lastChecked;
            public int peerID;
            public double performance;
            public int lastAlive;            

            public PeerPerformanceInformations(DateTime lastChecked, int peerID, double performance, int lastAlive)
            {
                this.lastAlive = lastAlive;
                this.lastChecked = lastChecked;
                this.peerID = peerID;
                this.performance = performance;
            }
        }

        private string channel;
        private BigInteger number;
        private BigInteger factor;
        private int head;
        private Queue storequeue;   //yields to store in the DHT
        private Queue loadqueue;    //yields that have been loaded from the DHT
        private Thread loadStoreThread;
        private bool stopLoadStoreThread;
        private QuadraticSievePresentation quadraticSieveQuickWatchPresentation;
        private AutoResetEvent yieldEvent;
        private int ourID;           //Our ID
        private Dictionary<int, string> nameCache;  //associates the ids with the names
        private Queue<PeerPerformanceInformations> peerPerformances;      //A queue of performances from the different peers ordered by the date last checked.
        private HashSet<int> activePeers;                                 //A set of active peers
        private double ourPerformance = 0;
        private int aliveCounter = 0;       //This is stored together with the performance in the DHT
        private string ourName;

        public delegate void P2PWarningHandler(String warning);
        public event P2PWarningHandler P2PWarning;        

        public PeerToPeer(QuadraticSievePresentation presentation, AutoResetEvent yieldEvent)
        {
            quadraticSieveQuickWatchPresentation = presentation;
            this.yieldEvent = yieldEvent;
            SetOurID();
        }

        private void SetOurID()
        {
            Random random = new Random();
            ourID = random.Next(1, Int32.MaxValue);        //TODO: Maybe we should calculate an id based on mac address and username?
            quadraticSieveQuickWatchPresentation.ProgressYields.setOurID(ourID);
            ourName = System.Net.Dns.GetHostName();
        }

        /// <summary>
        /// Reads yield at position "index" from DHT and returns the ownerID and the decompressed packet itself.
        /// </summary>
        private byte[] ReadYield(int index, out int ownerID)
        {
            ownerID = 0;
            byte[] yield = P2PManager.Retrieve(YieldIdentifier(index));
            if (yield == null)
                return null;

            byte[] decompressedYield = DecompressYield(yield);

            byte[] idbytes = new byte[4];
            Array.Copy(decompressedYield, idbytes, 4);
            ownerID = BitConverter.ToInt32(idbytes, 0);
            byte[] y = new byte[decompressedYield.Length - 4];
            Array.Copy(decompressedYield, 4, y, 0, y.Length);

            return y;
        }

        private static byte[] DecompressYield(byte[] yield)
        {
            MemoryStream memStream = new MemoryStream();
            DeflateStream defStream = new DeflateStream(memStream, CompressionMode.Decompress);
            memStream.Write(yield, 0, yield.Length);
            memStream.Position = 0;
            MemoryStream memStream2 = new MemoryStream();
            defStream.CopyTo(memStream2);
            defStream.Close();
            byte[] decompressedYield = memStream2.ToArray();
            return decompressedYield;
        }

        private uint ReadAndEnqueueYield(int loadIndex, uint downloaded, uint uploaded)
        {
            int ownerID;
            byte[] yield = ReadYield(loadIndex, out ownerID);
            if (yield != null)
            {
                downloaded += (uint)yield.Length;
                ShowTransfered(downloaded, uploaded);
                loadqueue.Enqueue(yield);

                //Progress Yield:
                if (!nameCache.ContainsKey(ownerID))
                {
                    byte[] n = P2PManager.Retrieve(NameIdentifier(ownerID));
                    if (n != null)
                        nameCache.Add(ownerID, System.Text.ASCIIEncoding.ASCII.GetString(n));
                }
                string name = null;
                if (nameCache.ContainsKey(ownerID))
                    name = nameCache[ownerID];
                SetProgressYield(loadIndex, ownerID, name);

                //Performance and alive informations:
                if (!activePeers.Contains(ownerID))
                {
                    UpdatePeerPerformance(ownerID);
                    activePeers.Add(ownerID);
                    UpdateActivePeerInformation();
                }

                yieldEvent.Set();
                return downloaded;
            }
            return 0;
        }

        /// <summary>
        /// Tries to read and enqueue yield on position "loadIndex".
        /// If it fails, it stores the index in lostIndices queue.
        /// </summary>
        private uint TryReadAndEnqueueYield(int index, uint downloaded, uint uploaded, Queue<KeyValuePair<int, DateTime>> lostIndices)
        {
            uint res = ReadAndEnqueueYield(index, downloaded, uploaded);
            if (res != 0)
                downloaded = res;
            else
            {
                var e = new KeyValuePair<int, DateTime>(index, DateTime.Now);
                lostIndices.Enqueue(e);
                SetProgressYield(index, -1, null);
            }

            return downloaded;
        }

        private void LoadStoreThreadProc()
        {
            int loadIndex = 0;
            uint downloaded = 0;
            uint uploaded = 0;
            HashSet<int> ourIndices = new HashSet<int>();   //Stores all the indices which belong to our packets
            //Stores all the indices (together with there check date) which belong to lost packets (i.e. packets that can't be load anymore):
            Queue<KeyValuePair<int, DateTime>> lostIndices = new Queue<KeyValuePair<int, DateTime>>();
            double lastPerformance = 0;
            DateTime performanceLastPut = new DateTime();

            try
            {
                while (!stopLoadStoreThread)
                {
                    //Store our performance and our alive counter in the DHT, either if the performance changed or when the last write was more than 1 minute ago:
                    if (ourPerformance != lastPerformance || performanceLastPut.CompareTo(DateTime.Now.Subtract(new TimeSpan(0, 1, 0))) < 0)
                    {
                        P2PManager.Retrieve(PerformanceIdentifier(ourID));      //just to outsmart the versioning system
                        P2PManager.Store(PerformanceIdentifier(ourID), concat(BitConverter.GetBytes(ourPerformance), BitConverter.GetBytes(aliveCounter++)));
                        lastPerformance = ourPerformance;
                        performanceLastPut = DateTime.Now;
                    }

                    //updates all peer performances which have last been checked more than 2 minutes ago and check if they are still alive:
                    while (peerPerformances.Count != 0 && peerPerformances.Peek().lastChecked.CompareTo(DateTime.Now.Subtract(new TimeSpan(0, 2, 0))) < 0)
                    {
                        var e = peerPerformances.Dequeue();

                        byte[] performancebytes = P2PManager.Retrieve(PerformanceIdentifier(e.peerID));
                        if (performancebytes != null)
                        {
                            double performance = BitConverter.ToDouble(performancebytes, 0);
                            int peerAliveCounter = BitConverter.ToInt32(performancebytes, 8);
                            if (peerAliveCounter <= e.lastAlive)
                            {
                                activePeers.Remove(e.peerID);
                                UpdateActivePeerInformation();
                            }
                            else
                                peerPerformances.Enqueue(new PeerPerformanceInformations(DateTime.Now, e.peerID, performance, peerAliveCounter));
                        }
                        else
                        {
                            activePeers.Remove(e.peerID);
                        }                       
                    }

                    SynchronizeHead();

                    if (storequeue.Count != 0)  //store our packages
                    {
                        byte[] yield = (byte[])storequeue.Dequeue();
                        bool success = P2PManager.Store(YieldIdentifier(head), yield);
                        while (!success)
                        {
                            SynchronizeHead();
                            success = P2PManager.Store(YieldIdentifier(head), yield);
                        }
                        SetProgressYield(head, ourID, null);
                        ourIndices.Add(head);
                        
                        head++;

                        //show informations about the uploaded yield:
                        uploaded += (uint)yield.Length;
                        ShowTransfered(downloaded, uploaded);
                    }
                    else                      //if there is nothing to store, we can load the other yields.
                    {
                        //skip all indices which are uploaded by us:
                        while (ourIndices.Contains(loadIndex))
                            loadIndex++;

                        if (loadIndex < head)
                        {
                            downloaded = TryReadAndEnqueueYield(loadIndex, downloaded, uploaded, lostIndices);
                            loadIndex++;
                        }
                        else
                        {
                            int count = 0;
                            //check all lost indices which are last checked longer than 1 minutes ago:
                            //TODO: Maybe we should throw away those indices, which have been checked more than several times.
                            while (lostIndices.Count != 0 && lostIndices.Peek().Value.CompareTo(DateTime.Now.Subtract(new TimeSpan(0, 1, 0))) < 0)
                            {
                                var e = lostIndices.Dequeue();
                                downloaded = TryReadAndEnqueueYield(loadIndex, downloaded, uploaded, lostIndices);
                                count++;
                            }

                            if (count == 0)
                                Thread.Sleep(5000);    //Wait 5 seconds
                        }
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
                return;
            }
        }

        private void UpdateActivePeerInformation()
        {
            quadraticSieveQuickWatchPresentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                quadraticSieveQuickWatchPresentation.amountOfPeers.Content = "" + activePeers.Count + " peers active!";
            }, null);
        }

        private void UpdatePeerPerformance(int peerID)
        {
            byte[] performancebytes = P2PManager.Retrieve(PerformanceIdentifier(peerID));
            if (performancebytes != null)
            {
                double performance = BitConverter.ToDouble(performancebytes, 0);
                int peerAliveCounter = BitConverter.ToInt32(performancebytes, 8);
                peerPerformances.Enqueue(new PeerPerformanceInformations(DateTime.Now, peerID, performance, peerAliveCounter));
            }
        }

        /// <summary>
        /// Loads head from DHT. If ours is greater (or there is no entry yet), we store ours.
        /// </summary>
        private void SynchronizeHead()
        {
            byte[] h = P2PManager.Retrieve(HeadIdentifier());
            if (h != null)
            {
                int dhthead = int.Parse(System.Text.ASCIIEncoding.ASCII.GetString(h));
                if (head > dhthead)
                {
                    bool success = P2PManager.Store(HeadIdentifier(), System.Text.ASCIIEncoding.ASCII.GetBytes(head.ToString()));
                    if (!success)
                        SynchronizeHead();
                }
                else if (head < dhthead)
                {
                    head = dhthead;
                    SetProgressYield(head - 1, 0, null);
                }
            }
            else
            {
                bool success = P2PManager.Store(HeadIdentifier(), System.Text.ASCIIEncoding.ASCII.GetBytes(head.ToString()));
                if (!success)
                    SynchronizeHead();
            }
        }

        private void ShowTransfered(uint downloaded, uint uploaded)
        {
            string s1 = ((downloaded / 1024.0) / 1024).ToString();
            string size1 = s1.Substring(0, (s1.Length < 3) ? s1.Length : 3);
            string s2 = ((uploaded / 1024.0) / 1024).ToString();
            string size2 = s2.Substring(0, (s2.Length < 3) ? s2.Length : 3);
            quadraticSieveQuickWatchPresentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                quadraticSieveQuickWatchPresentation.relationsInfo.Content = "Downloaded " + size1 + " MB! Uploaded " + size2 + " MB!";
            }, null);
        }

        private void SetProgressYield(int index, int id, string name)
        {
            quadraticSieveQuickWatchPresentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                quadraticSieveQuickWatchPresentation.ProgressYields.Set(index, id, name);
            }, null);
        }

        private void ClearProgressYields()
        {
            quadraticSieveQuickWatchPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                quadraticSieveQuickWatchPresentation.ProgressYields.Clear();
            }, null);
        }

        private string HeadIdentifier()
        {
            return channel + "#" + number + "-" + factor + "HEAD";
        }

        private string FactorListIdentifier()
        {
            return channel + "#" + number + "FACTORLIST";
        }

        private string YieldIdentifier(int index)
        {
            return channel + "#" + number + "-" + factor + "!" + index;
        }

        private string NameIdentifier(int ID)
        {
            return channel + "#" + number + "NAME" + ID.ToString();
        }

        private string PerformanceIdentifier(int ID)
        {
            return channel + "#" + number + "-" + factor + "PERFORMANCE" + ID.ToString();
        }   

        private void StartLoadStoreThread()
        {
            storequeue = Queue.Synchronized(new Queue());
            loadqueue = Queue.Synchronized(new Queue());

            stopLoadStoreThread = false;
            loadStoreThread = new Thread(LoadStoreThreadProc);
            loadStoreThread.Start();
        }

        /// <summary>
        /// Concatenates the two byte arrays a1 and a2 an returns the result array.
        /// </summary>
        private byte[] concat(byte[] a1, byte[] a2)
        {
            byte[] res = new byte[a1.Length + a2.Length];
            System.Buffer.BlockCopy(a1, 0, res, 0, a1.Length);
            System.Buffer.BlockCopy(a2, 0, res, a1.Length, a2.Length);
            return res;
        }

        public void StopLoadStoreThread()
        {
            stopLoadStoreThread = true;
            loadStoreThread.Interrupt();
            loadStoreThread.Join();
            loadStoreThread = null;
        }

        public Queue GetLoadedYieldsQueue()
        {
            return loadqueue;
        }

        /// <summary>
        /// Compresses the yield and puts it in the DHT.
        /// </summary>
        public void Put(byte[] serializedYield)
        {
            //Add our ID:
            byte[] idbytes = BitConverter.GetBytes(ourID);
            Debug.Assert(idbytes.Length == 4);
            serializedYield = concat(idbytes, serializedYield); ;

            //Compress:
            MemoryStream memStream = new MemoryStream();
            DeflateStream defStream = new DeflateStream(memStream, CompressionMode.Compress);
            defStream.Write(serializedYield, 0, serializedYield.Length);
            defStream.Close();
            byte[] compressedYield = memStream.ToArray();

            //Debug stuff:
            byte[] decompr = DecompressYield(compressedYield);
            Debug.Assert(decompr.Length == serializedYield.Length);

            //store in queue, so the LoadStoreThread can store it in the DHT later:
            storequeue.Enqueue(compressedYield);            
        }

        /// <summary>
        /// Sets the channel in which we want to sieve
        /// </summary>
        public void SetChannel(string channel)
        {
            this.channel = channel;
        }

        /// <summary>
        /// Sets the number to sieve
        /// </summary>
        public void SetNumber(BigInteger number)
        {
            this.number = number;
        }

        /// <summary>
        /// Sets the factor to sieve next and starts reading informations from the DHT.
        /// </summary>
        public void SetFactor(BigInteger factor)
        {
            this.factor = factor;
            Debug.Assert(this.number % this.factor == 0);

            ClearProgressYields();
            nameCache = new Dictionary<int, string>();
            peerPerformances = new Queue<PeerPerformanceInformations>();
            activePeers = new HashSet<int>();

            //load head:
            byte[] h = P2PManager.Retrieve(HeadIdentifier());
            if (h != null)
            {                
                head = int.Parse(System.Text.ASCIIEncoding.ASCII.GetString(h));
                SetProgressYield(head-1, 0, null);
            }
            else
                head = 0;

            //SetOurID();

            //store our name:
            P2PManager.Retrieve(NameIdentifier(ourID));     //just to outsmart the versioning system
            P2PManager.Store(NameIdentifier(ourID),  System.Text.ASCIIEncoding.ASCII.GetBytes(ourName.ToCharArray())); //TODO: proper name here!

            if (loadStoreThread != null)
                throw new Exception("LoadStoreThread already started");
            StartLoadStoreThread();
        }

        /// <summary>
        /// Synchronizes the factorManager with the DHT.
        /// Return false if this.factor is not a composite factor in the DHT (which means, that another peer already finished sieving this.factor).
        /// </summary>
        public bool SyncFactorManager(FactorManager factorManager)
        {
            FactorManager dhtFactorManager = null;
            //load DHT Factor Manager:
            byte[] dhtFactorManagerBytes = P2PManager.Retrieve(FactorListIdentifier());
            if (dhtFactorManagerBytes != null)
            {
                MemoryStream memstream = new MemoryStream();
                memstream.Write(dhtFactorManagerBytes, 0, dhtFactorManagerBytes.Length);
                memstream.Position = 0;
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
                try
                {
                    dhtFactorManager = (FactorManager)bformatter.Deserialize(memstream);
                }
                catch (System.Runtime.Serialization.SerializationException)
                {
                    P2PWarning("DHT factor list is broken!");
                    P2PManager.Remove(FactorListIdentifier());
                    return SyncFactorManager(factorManager);
                }                
            }

            //Synchronize DHT Factor Manager with our Factor List
            if (dhtFactorManager == null || factorManager.Synchronize(dhtFactorManager))
            {
                //Our Factor Manager has more informations, so let's store it in the DHT:
                MemoryStream memstream = new MemoryStream();
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
                bformatter.Serialize(memstream, factorManager);
                bool success = P2PManager.Store(FactorListIdentifier(), memstream.ToArray());
                if (!success)
                {
                    Thread.Sleep(1000);
                    return SyncFactorManager(factorManager);       //Just try again
                }
            }

            return factorManager.ContainsComposite(this.factor);
        }

        /// <summary>
        /// Sets the performance of this machine, so that this class can write it to the DHT later.
        /// The performance is meassured in relations per ms.
        /// </summary>
        public void SetOurPerformance(double[] relationsPerMS)
        {
            double globalPerformance = 0;
            foreach (double r in relationsPerMS)
                globalPerformance += r;

            ourPerformance = globalPerformance;
        }

        /// <summary>
        /// Returns the performance of all peers (excluding ourselve) in relations per ms.
        /// </summary>
        public double GetP2PPerformance()
        {
            double perf = 0;

            foreach (var p in peerPerformances)
                perf += p.performance;

            return perf;
        }
    }
}
