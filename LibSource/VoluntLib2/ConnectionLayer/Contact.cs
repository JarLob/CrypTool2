﻿/*
   Copyright 2018 Nils Kopal <Nils.Kopal<AT>Uni-Kassel.de>

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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VoluntLib2.ConnectionLayer
{
    /// <summary>
    /// A contact is a combination of IPAddress and Port referencing a Peer
    /// It also knows the Peer's PeerID (unique random identification number)
    /// Additionally, it knows when the peer was last seen and the last hello was sent to him
    /// </summary>
    internal class Contact
    {
        /// <summary>
        /// Randomly generated 16 byte PeerId
        /// </summary>
        public byte[] PeerId { get; set; }

        /// <summary>
        /// Remote IPAddress of this Peer
        /// </summary>
        public IPAddress IPAddress { get; set; }

        /// <summary>
        /// Remote udp port of this Peer
        /// </summary>
        public ushort Port { get; set; }

        /// <summary>
        /// Receiving time of last packt from this Peer
        /// </summary>
        public DateTime LastSeen { get; set; }

        /// <summary>
        /// Time when we sent the last HelloMessage to this Peer
        /// </summary>
        public DateTime LastHelloSent { get; set; }

        /// <summary>
        /// Flag if this Peer is offline
        /// </summary>
        public bool IsOffline { get; set; }

        /// <summary>
        /// Creates a new contact
        /// </summary>
        public Contact()
        {
            PeerId = new byte[16];
            Port = 0;
            LastSeen = DateTime.Now;
            LastHelloSent = DateTime.Now;
            IsOffline = false;
        }

        /// <summary>
        /// List of Peers that know this Peer
        /// </summary>
        public ConcurrentDictionary<IPEndPoint, Contact> KnownBy = new ConcurrentDictionary<IPEndPoint, Contact>();

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Contact");
            builder.AppendLine("{");
            builder.Append("  PeerId: ");
            builder.AppendLine(BitConverter.ToString(PeerId) + ",");
            builder.Append("  IPAddress: ");
            builder.AppendLine(IPAddress.ToString() + ",");
            builder.Append("  Port: ");
            builder.AppendLine("" + Port + ",");
            builder.Append("  Last seen: ");
            builder.AppendLine("" + LastSeen + ",");
            builder.Append("  Last hello sent: ");
            builder.AppendLine("" + LastHelloSent + ",");
            builder.Append("  Offline: ");
            builder.AppendLine("" + IsOffline + ",");
            builder.AppendLine("}");
            return builder.ToString();
        }

        /// <summary>
        /// Serializes PeerID, IPAddress, and port
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            byte[] data = new byte[16 + 4 + 2];
            Array.Copy(PeerId, data, 16);
            Array.Copy(IPAddress.GetAddressBytes(), 0, data, 16, 4);
            Array.Copy(BitConverter.GetBytes(Port), 0, data, 20, 2);
            return data;
        }

        /// <summary>
        /// Deserializes from a byte array
        /// </summary>
        /// <param name="data"></param>
        public void Deserialize(byte[] data)
        {
            Array.Copy(data, PeerId, 16);
            IPAddress = new IPAddress(new byte[] { data[16], data[17], data[18], data[19] });
            Port = BitConverter.ToUInt16(data, 20);
        }
    }
}
