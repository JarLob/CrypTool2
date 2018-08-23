/*
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrypToolStoreLib.Network
{
    public enum MessageType
    {
        //Login
        Login = 0,
        ResponseLogin = 1,
        Logout = 2,
        //Messages for "developers"

        ListDevelopers = 100,
        ResponseDevelopersList = 101,
        CreateNewDeveloper = 102,
        UpdateDeveloper = 103,
        DeleteDeveloper = 104,
        ResponseDeveloperModification = 105,
        GetDeveloper = 106,
        ResponseGetDeveloper = 107,

        //Messages for "plugins"
        ListPlugins = 200,
        ResponseListPlugins = 201,
        CreateNewPlugin = 202,
        UpdatePlugin = 203,
        DeletePlugin = 204,
        ResponsePluginModification = 205,
        GetPlugin = 206,
        ResponseGetPlugin = 207,
        
        //Message for "Source"
        ListSources = 300,
        ResponseListSources = 301,
        CreateNewSource = 302,
        UpdateSource = 303,
        DeleteSource = 304,
        ResponseSourceModification = 305,
        GetSource = 306,
        ResponseGetSource = 307,
        
        //Message for "Resources"
        ListResources = 400,
        ResponseListResources = 401,
        CreateNewResource = 402,
        UpdateResource = 403,
        DeleteResource = 404,
        ResponseResourceModification = 405,
        GetResource = 406,
        ResponseGetResource = 407,
        
        //Message for "ResourcesData"
        ListResourcesData = 500,
        ResponseListResourcesData = 501,
        CreateNewResourceData = 502,
        UpdateResourceData = 503,
        DeleteResourceData = 504,
        ResponseResourceModificationData = 505,
        GetResourceData = 506,
        ResponseGetResourceData = 507,
    }

    /// <summary>
    /// Message header of messages
    /// </summary>
    public class MessageHeader
    {                
        private const string MAGIC = "CrypToolStore";       // 13 byte (string); each message begins with that
        public MessageType MessageType { get; set; }        // 4 byte (uint32)
        public UInt32 PayloadSize { get; set; }             // 4 byte (unint32)
        public String Username { get; set; }                // 4 bytes for length + n bytes for string
        
        /// <summary>
        /// Serializes the header into a byte array
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {            
            //convert everything to byte arrays
            byte[] magicBytes = ASCIIEncoding.ASCII.GetBytes(MAGIC);
            byte[] messageTypeBytes = BitConverter.GetBytes((UInt32)MessageType);
            byte[] payloadSizeBytes = BitConverter.GetBytes(PayloadSize);
            byte[] usernameLengthBytes = BitConverter.GetBytes((UInt32)Username.Length);
            byte[] usernameBytes = ASCIIEncoding.ASCII.GetBytes(Username);

            //create one byte array and return it
            byte[] bytes = new byte[13 + 4 + 4 + 4 + Username.Length];
            int offset = 0;

            Array.Copy(magicBytes, 0, bytes, 0, magicBytes.Length);
            offset += magicBytes.Length;
            
            Array.Copy(messageTypeBytes, 0, bytes, offset, messageTypeBytes.Length);
            offset += messageTypeBytes.Length;

            Array.Copy(payloadSizeBytes, 0, bytes, offset, payloadSizeBytes.Length);
            offset += payloadSizeBytes.Length;

            Array.Copy(usernameLengthBytes, 0, bytes, offset, usernameLengthBytes.Length);
            offset += usernameLengthBytes.Length;

            Array.Copy(usernameBytes, 0, bytes, offset, usernameBytes.Length);
            offset += usernameBytes.Length;

            return bytes;
        }

        /// <summary>
        /// Deserializes a header from the byte array
        /// returns the offset of the payload in the byte array
        /// </summary>
        /// <param name="bytes"></param>
        public int Deserialize(byte[] bytes)
        {
            if (bytes.Length < 25)
            {
                throw new DeserializationException(String.Format("Message header too small. Got {0} but expect min {1}", bytes.Length, 25));
            }            
            string magicnumber = ASCIIEncoding.ASCII.GetString(bytes, 0, 13);
            if (!magicnumber.Equals(MAGIC))
            {
                throw new DeserializationException(String.Format("Magic number mistmatch. Got \"{0}\" but expect \"{1}\"", magicnumber, MAGIC));
            }
            try
            {
                int offset = magicnumber.Length;
                MessageType = (MessageType)BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                PayloadSize = BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                int usernameLength = (int)BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                Username = ASCIIEncoding.ASCII.GetString(bytes, offset, usernameLength);
                offset += usernameLength;
                return offset;
            }
            catch (Exception ex)
            {
                throw new DeserializationException(String.Format("Exception during Deserialization: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Returns infos about the MessageHeader as string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("MessageHeader{{MessageType={0}, PayloadSize={1}, Username={2}}}", MessageType.ToString(), PayloadSize, Username);
        }
    }

    /// <summary>
    /// Superclass for all messages
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Constructor
        /// creates message header
        /// </summary>
        public Message()
        {
            MessageHeader = new MessageHeader();
        }

        /// <summary>
        /// Header of this message
        /// </summary>
        public MessageHeader MessageHeader
        {
            get;
            set;
        }

        /// <summary>
        /// Serializes the message into a byte array
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            byte[] headerbytes = MessageHeader.Serialize();
            byte[] bytes = new byte[headerbytes.Length + (Payload != null ? Payload.Length : 0)];
            Array.Copy(headerbytes, 0, bytes, 0, headerbytes.Length);
            if (Payload != null && Payload.Length > 0)
            {
                Array.Copy(Payload, 0, bytes, headerbytes.Length, Payload.Length);
            }
            return bytes;
        }

        /// <summary>
        /// Deserializes a message from the byte array
        /// </summary>
        /// <param name="bytes"></param>
        public void Deserialize(byte[] bytes)
        {
            int offset = MessageHeader.Deserialize(bytes);
            if (offset < bytes.Length - 1)
            {
                Array.Copy(bytes, offset, Payload, 0, bytes.Length - offset);
            }
        }

        public byte[] Payload { get; set; }
    }


#region Login messages

    public class LoginMessage : Message
    {
        
    }
    
    public class ResponseLoginMessage : Message
    {

    }
        
    public class LogoutMessage : Message
    {

    }

#endregion

#region Developers messages

    class ListDevelopers : Message{

    }

    public class ResponseDevelopersListMessage : Message
    {
    
    }

    public class CreateNewDeveloperMessage : Message
    {

    }

    public class UpdateDeveloperMessage : Message
    {

    }
    
    public class DeleteDeveloperMessage : Message
    {

    }

    public class ResponseDeveloperModificationMessage : Message
    {

    }

    public class GetDeveloperMessage : Message
    {

    }

    public class  ResponseGetDeveloperMessage : Message
    {

    }

#endregion

#region Plugin messages

    public class ListPluginsMessage : Message
    {

    }

    public class ResponseListPluginsMessage : Message
    {
    
    }
    
    
    public class CreateNewPluginMessage : Message
    {

    }

    public class UpdatePluginMessage : Message
    {

    }

    public class DeletePluginMessage : Message
    {

    }

    public class ResponsePluginModificationMessage : Message
    {

    }

    public class GetPluginMessage : Message
    {

    }

    public class ResponseGetPluginMessage : Message
    {

    }

#endregion

#region Sources messages
       
    public class ListSourcesMessage : Message
    {
    
    }

    public class ResponseListSourcesMessage : Message
    {

    }

    public class CreateNewSourceMessage : Message
    {

    }

    public class UpdateSourceMessage : Message
    {

    }

    public class DeleteSourceMessage : Message
    {

    }

    public class ResponseSourceModificationMessage : Message
    {

    }

    public class GetSourceMessage : Message 
    {

    }
    
    public class ResponseGetSourceMessage : Message
    {

    }
        
#endregion

#region Resources messages

    public class ListResourcesMessage : Message
    {
    
    }

    public class ResponseListResources : Message
    {

    }

    public class CreateNewResourceMessage : Message
    {

    }

    public class UpdateResourceMessage : Message
    {

    }

    public class DeleteResourceMessage : Message
    {

    }

    public class ResponseResourceModificationMessage : Message
    {

    }

    public class GetResourceMessage : Message
    {

    }

    public class ResponseGetResource : Message
    {

    }
        
#endregion

#region ResourcesData messages
    
    public class ListResourcesDataMessage : Message
    {

    }
    
    public class ResponseListResourcesDataMessage : Message
    {

    }
    
    public class CreateNewResourceDataMessage : Message
    {

    }


    public class UpdateResourceDataMessage : Message
    {

    }

    public class DeleteResourceDataMessage : Message
    {

    }
        
    public class ResponseResourceModificationData : Message
    {
    
    }

    public class GetResourceDataMessage: Message
    {

    }

    public class ResponseGetResourceDataMessage : Message
    {

    }

#endregion

}
