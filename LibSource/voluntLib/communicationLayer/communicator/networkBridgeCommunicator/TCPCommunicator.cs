// Copyright 2014 Christopher Konze, University of Kassel
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#region

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using NLog;
using voluntLib.common.interfaces;
using voluntLib.communicationLayer.messages.messageWithCertificate;
using System.Security.Cryptography;


#endregion

namespace voluntLib.communicationLayer.communicator.networkBridgeCommunicator
{
    /// <summary>
    /// Abstract class for bidirectional TCP communications. 
    /// </summary>
    public abstract class TCPCommunicator : ICommunicator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        protected ICommunicationLayer comLayer;

        protected TCPCommunicator()
        {
            WaitForRemoteAnswerMS = 5000;
        }

        /// <summary>
        ///   Gets or sets the time that the socket will wait for the remote to answer in ms.
        ///   Default: 5000 ms
        /// </summary>
        public int WaitForRemoteAnswerMS { get; set; }

        public void RegisterCommunicationLayer(ICommunicationLayer communicationLayer)
        {
            comLayer = communicationLayer;
        }

        public abstract void Start();
        public abstract void Stop();
        public abstract void ProcessMessage(AMessage data, IPAddress to);

        /// <summary>
        ///   Closes the connection in to steps:
        ///   First: sending a UInt16 0 to indicate, that no message will follow
        ///   Second: shutdown the socket.
        /// </summary>
        /// <param name="client">The client.</param>
        protected void CloseConnection(TcpClient client)
        {
            if (!client.Connected)
                return;
            try
            {
                using (var stream = client.GetStream())
                {
                    if (stream.CanRead)    
                    {
                        var buffer = new byte[client.ReceiveBufferSize];
                        stream.Read(buffer, 0, buffer.Length);
                    }
               
                    stream.Write(new byte[2], 0, 2); // indicating that no message'll follow   
                }
                client.Close();
            } 
            catch
            {
                Logger.Info("Connection already has been closed");
            }
        }

        /// <summary>
        ///   Writes data to stream.
        ///   It also adds the size ot the data at the beginning.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="netStream">The net stream.</param>
        protected void WriteToStream(byte[] message, NetworkStream netStream)
        {
            if (!netStream.CanWrite)
            {
                Logger.Debug("Write to TCP stream failed, can't write to stream");
                return;
            }

            try
            {
                //add length at the front of the message
                var bytes = new byte[message.Length + 2];
                BitConverter.GetBytes((ushort)message.Length).CopyTo(bytes, 0);
                message.CopyTo(bytes, 2);
                //write to stream
                netStream.Write(bytes, 0, bytes.Length);
            } catch (IOException e)
            {
                Logger.Debug("Write to TCP stream faild due: " + e.Message + e.StackTrace);
            }
        }

        protected byte[] readFromNetworkStream(NetworkStream netStream, int messageLength)
        {
            var buffer = new byte[messageLength];
            return readFromNetworkStream(netStream, buffer, messageLength);
        }

        protected byte[] readFromNetworkStream(NetworkStream netStream, byte[] buffer, int messageLength)
        {
            int readBytes = 0;
            while (readBytes < messageLength && netStream.CanRead)
            {
                readBytes += netStream.Read(buffer, readBytes, messageLength - readBytes);
            };
            return buffer;
        }

    }
}