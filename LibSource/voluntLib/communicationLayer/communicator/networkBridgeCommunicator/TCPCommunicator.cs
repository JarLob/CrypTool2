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

        /// <summary>
        ///   Begins to read messages from the stream.
        ///   This method waits busy for messages occurring on the networkStream.
        ///   It will return if:
        ///   - it cannot read from the stream.
        ///   - the connections is closed ( ether by sending an 0-sized message or forcely drop the connection)
        ///   - it times out.
        /// </summary>
        /// <param name="netStream">The net stream.</param>
        /// <param name="remoteIP">The remote IP.</param>
        protected void BeginReadFromStream(NetworkStream netStream, IPAddress remoteIP)
        {
            if (!netStream.CanRead)
            {
                Logger.Debug("Read from TCP stream failed, cant read from stream");
                return;
            }

            try
            {
                // read message length
                var emptyBytes = new byte[2];
                var bufferLength = new byte[2];
                netStream.Read(bufferLength, 0, 2);
                var messageLength = BitConverter.ToUInt16(bufferLength, 0);

                while (messageLength != 0 && netStream.CanRead)
                {
                    //repeat until all messages have been fetched
                    var buffer = new byte[messageLength];
                    netStream.Read(buffer, 0, buffer.Length);

                    //handle data
                    comLayer.HandleIncomingMessages(buffer, remoteIP);

                    // read next 2 bytes to see if another message is available
                    emptyBytes.CopyTo(bufferLength, 0);
                    netStream.Read(bufferLength, 0, 2);
                    messageLength = BitConverter.ToUInt16(bufferLength, 0);
                }
            } catch (IOException e)
            {
                Logger.Debug("Read from TCP stream faild due: " + e.Message + e.StackTrace);
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
                BitConverter.GetBytes((ulong) message.Length).CopyTo(bytes, 0);
                message.CopyTo(bytes, 2);

                //write to stream
                netStream.Write(bytes, 0, bytes.Length);
            } catch (IOException e)
            {
                Logger.Debug("Write to TCP stream faild due: " + e.Message + e.StackTrace);
            }
        }
    }
}