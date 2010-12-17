﻿using System;
using System.Collections.Generic;
using System.IO;
using Cryptool.P2P;
using Cryptool.P2P.Internal;
using Cryptool.PluginBase;
using KeySearcher.P2P.Exceptions;
using KeySearcher.P2P.Presentation;
using KeySearcher.P2P.Tree;

namespace KeySearcher.P2P.Storage
{
    class StorageHelper
    {
        private readonly KeySearcher keySearcher;
        private readonly StatisticsGenerator statisticsGenerator;
        private readonly StatusContainer statusContainer;

        //VERSIONNUMBER: Important. Set it +1 manually everytime the length of the MemoryStream Changes
        private const int version = 2;

        public StorageHelper(KeySearcher keySearcher, StatisticsGenerator statisticsGenerator, StatusContainer statusContainer)
        {
            this.keySearcher = keySearcher;
            this.statisticsGenerator = statisticsGenerator;
            this.statusContainer = statusContainer;
        }

        internal RequestResult UpdateInDht(NodeBase nodeToUpdate)
        {
            var memoryStream = new MemoryStream();
            var binaryWriter = new BinaryWriter(memoryStream);

            if (nodeToUpdate is Node)
            {
                UpdateNodeInDht((Node) nodeToUpdate, binaryWriter);
            } else
            {
                UpdateLeafInDht((Leaf) nodeToUpdate, binaryWriter);
            }

            // Append results
            binaryWriter.Write(nodeToUpdate.Result.Count);
            foreach (var valueKey in nodeToUpdate.Result)
            {
                binaryWriter.Write(valueKey.key);
                binaryWriter.Write(valueKey.value);
                binaryWriter.Write(valueKey.decryption.Length);
                binaryWriter.Write(valueKey.decryption);
            }                        
            
 
            //Creating a copy of the activity dictionary
            Dictionary<String, Dictionary<long, int>> copyAct = nodeToUpdate.Activity;

            //Write number of avatarnames
            binaryWriter.Write(copyAct.Keys.Count);
            foreach (string avatar in copyAct.Keys)
            {
                Dictionary<long, int> maschCopy = copyAct[avatar];
                //write avatarname
                binaryWriter.Write(avatar);
                //write the number of maschines for this avatar
                binaryWriter.Write(maschCopy.Keys.Count);

                foreach (long maschID in maschCopy.Keys)
                {
                    //write the maschines and their patterncount
                    binaryWriter.Write(maschID);
                    binaryWriter.Write(maschCopy[maschID]);
                }
            }

            return StoreWithStatistic(KeyInDht(nodeToUpdate), memoryStream.ToArray());
        }

        private static void UpdateNodeInDht(Node nodeToUpdate, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(nodeToUpdate.LeftChildFinished);
            binaryWriter.Write(nodeToUpdate.RightChildFinished);
        }

        private static void UpdateLeafInDht(Leaf nodeToUpdate, BinaryWriter binaryWriter)
        {
            binaryWriter.Write('V');
            binaryWriter.Write(version);
            var buffer = nodeToUpdate.LastReservationDate.ToBinary();
            binaryWriter.Write(buffer);
            binaryWriter.Write(nodeToUpdate.getClientIdentifier());
        }

        internal RequestResult UpdateFromDht(NodeBase nodeToUpdate, bool forceUpdate = false)
        {
            if (!forceUpdate && nodeToUpdate.LastUpdate > DateTime.Now.Subtract(new TimeSpan(0, 0, 5)))
            {
                return new RequestResult { Status = RequestResultType.Success };
            }

            nodeToUpdate.LastUpdate = DateTime.Now;

            var requestResult = RetrieveWithStatistic(KeyInDht(nodeToUpdate));
            var nodeBytes = requestResult.Data;

            if (nodeBytes == null)
            {
                return requestResult;
            }

            var binaryReader = new BinaryReader(new MemoryStream(nodeBytes));

            if (nodeToUpdate is Node)
            {
                UpdateNodeFromDht((Node) nodeToUpdate, binaryReader);
            } else
            {
                UpdateLeafFromDht((Leaf)nodeToUpdate, binaryReader);
            }

            // Load results
            var resultCount = binaryReader.ReadInt32();
            for (var i = 0; i < resultCount; i++)
            {
                var newResult = new KeySearcher.ValueKey
                                    {
                                        key = binaryReader.ReadString(),
                                        value = binaryReader.ReadDouble(),
                                        decryption = binaryReader.ReadBytes(binaryReader.ReadInt32())
                                    };
                nodeToUpdate.Result.AddLast(newResult);
            }
            


            if (binaryReader.BaseStream.Length != binaryReader.BaseStream.Position)
            {  
                //Reading the number of avatarnames
                int avatarcount = binaryReader.ReadInt32();
                for(int i=0; i<avatarcount;i++)
                {
                    //Reading the avatarname and the maschine-count for this name
                    string avatarname = binaryReader.ReadString();
                    int maschcount = binaryReader.ReadInt32();
                    Dictionary<long, int> readMaschcount = new Dictionary<long, int>();

                    for(int j=0;j<maschcount;j++)
                    {
                        //reading the IDs and patterncount
                        long maschID = binaryReader.ReadInt64();
                        int count = binaryReader.ReadInt32();
                        readMaschcount.Add(maschID,count);
                    }
                    if (nodeToUpdate.Activity.ContainsKey(avatarname))
                    {
                        nodeToUpdate.Activity[avatarname] = readMaschcount;
                    }
                    else
                    {
                        nodeToUpdate.Activity.Add(avatarname, readMaschcount);                      
                    }
                }               
            }

//-------------------------------------------------------------------------------------------
//AFTER CHANGING THE FOLLOWING PART INCREASE THE VERSION-NUMBER AT THE TOP OF THIS CLASS!
//-------------------------------------------------------------------------------------------
                       
            if (resultCount > 0)
            {
                keySearcher.IntegrateNewResults(nodeToUpdate.Result,nodeToUpdate.Activity, nodeToUpdate.DistributedJobIdentifier);
                statisticsGenerator.ProcessPatternResults(nodeToUpdate.Result);
            }

            nodeToUpdate.UpdateCache();
            return requestResult;
        }

        private static void UpdateNodeFromDht(Node nodeToUpdate, BinaryReader binaryReader)
        {
            nodeToUpdate.LeftChildFinished = binaryReader.ReadBoolean() || nodeToUpdate.LeftChildFinished;
            nodeToUpdate.RightChildFinished = binaryReader.ReadBoolean() || nodeToUpdate.RightChildFinished;
        }

        private static void UpdateLeafFromDht(Leaf nodeToUpdate, BinaryReader binaryReader)
        {
            CheckVersion(binaryReader);
               
            var date = DateTime.FromBinary(binaryReader.ReadInt64());
            if (date > nodeToUpdate.LastReservationDate)
            {
                nodeToUpdate.LastReservationDate = date;
            }
            
            try
            {
                if (binaryReader.BaseStream.Length - binaryReader.BaseStream.Position >= 8)
                {
                    nodeToUpdate.setClientIdentifier(binaryReader.ReadInt64());
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                // client id not available, use default
                nodeToUpdate.setClientIdentifier(-1);
            }
            
        }

        internal static string KeyInDht(NodeBase node)
        {
            return string.Format("{0}_node_{1}_{2}", node.DistributedJobIdentifier, node.From, node.To);
        }

        private static void CheckVersion(BinaryReader binaryReader)
        {           
            try
            {
                //Checking if there's a version in the stream
                int vers = binaryReader.PeekChar();
                if (vers == 86)
                {
                    //Reading the char and the versionnumber
                    char magic = binaryReader.ReadChar();
                    int versionInUse = binaryReader.ReadInt32();
                    //Check if a newer Version is in use
                    if (versionInUse > version)
                    {
                        throw new KeySearcherStopException();
                    }
                }
            }
            catch(KeySearcherStopException)
            {
                throw new KeySearcherStopException();
            }
            
        }

        public DateTime StartDate(String ofJobIdentifier)
        {
            var key = ofJobIdentifier + "_startdate";
            var requestResult = RetrieveWithStatistic(key);

            if (requestResult.IsSuccessful() && requestResult.Data != null)
            {
                var startTimeUtc = DateTime.SpecifyKind(
                    DateTime.FromBinary(BitConverter.ToInt64(requestResult.Data, 0)), DateTimeKind.Utc);
                return startTimeUtc.ToLocalTime();
            }

            StoreWithStatistic(key, BitConverter.GetBytes((DateTime.UtcNow.ToBinary())));
            return DateTime.Now;
        }

        public long SubmitterID(String ofJobIdentifier)
        {
            var key = ofJobIdentifier + "_submitterid";
            var requestResult = RetrieveWithStatistic(key);

            if (requestResult.IsSuccessful() && requestResult.Data != null)
            {
                var submitterid = BitConverter.ToInt64(requestResult.Data, 0);
                return submitterid;
            }

            StoreWithStatistic(key, BitConverter.GetBytes(Cryptool.PluginBase.Miscellaneous.UniqueIdentifier.GetID()));
            return Cryptool.PluginBase.Miscellaneous.UniqueIdentifier.GetID();
        }

        public RequestResult RetrieveWithStatistic(string key)
        {
            statusContainer.RetrieveRequests++;
            statusContainer.TotalDhtRequests++;
            var requestResult = P2PManager.Retrieve(key);

            if (requestResult.Data != null)
            {
                statusContainer.RetrievedBytes += requestResult.Data.Length;
                statusContainer.TotalBytes += requestResult.Data.Length;
            }

            return requestResult;
        }

        public RequestResult RemoveWithStatistic(string key)
        {
            statusContainer.RemoveRequests++;
            statusContainer.TotalDhtRequests++;
            return P2PManager.Remove(key);
        }

        public RequestResult StoreWithStatistic(string key, byte[] data)
        {
            statusContainer.StoreRequests++;
            statusContainer.TotalDhtRequests++;
            var requestResult = P2PManager.Store(key, data);

            if (requestResult.Data != null)
            {
                statusContainer.StoredBytes += requestResult.Data.Length;
                statusContainer.TotalBytes += requestResult.Data.Length;
            }

            return requestResult;
        }
    }
}
