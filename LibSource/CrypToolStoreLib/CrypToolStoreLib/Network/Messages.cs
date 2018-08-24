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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CrypToolStoreLib.Network
{
    /// <summary>
    /// Message types of the network protocol
    /// </summary>
    public enum MessageType
    {
        //Login
        Login = 0,
        ResponseLogin = 1,
        Logout = 2,
        //Messages for "developers"

        ListDevelopers = 100,
        ResponseListDevelopers = 101,
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
        ResponseResourceDataModification = 505,
        GetResourceData = 506,
        ResponseGetResourceData = 507,

        //no type defined
        Undefined = 10000
    }


    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class MessageDataField : Attribute
    {
        /// <summary>
        /// Is the ToString-method allowd to show the corresponding field or property?
        /// </summary>
        public bool ShowInToString { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="showInToString">Indicates whether the ToString-method is allowd to show the attribute/field or not</param>
        public MessageDataField(bool showInToString = true)
        {
            ShowInToString = showInToString;
        }
    }

    /// <summary>
    /// Message header of messages
    /// </summary>
    public class MessageHeader
    {                
        private const string MAGIC = "CrypToolStore";       // 13 byte (string); each message begins with that
        public MessageType MessageType { get; set; }        // 4 byte (uint32)
        private UInt32 PayloadSize { get; set; }            // 4 byte (unint32)
        public String Username { get; set; }                // 4 bytes for length + n bytes for string

        /// <summary>
        /// Constructor
        /// </summary>
        public MessageHeader()
        {
            MessageType = MessageType.Undefined;
            Username = string.Empty;
            PayloadSize = 0;
        }

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
                throw new DeserializationException(String.Format("Message header too small. Got {0} but expect min {1}", bytes.Length, MAGIC.Length));
            }            
            string magicnumber = ASCIIEncoding.ASCII.GetString(bytes, 0, 13);
            if (!magicnumber.Equals(MAGIC))
            {
                throw new DeserializationException(String.Format("Magic number mismatch. Got \"{0}\" but expect \"{1}\"", magicnumber, MAGIC));
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
    public abstract class Message
    {
        private static Dictionary<MessageType, Type> MessageTypeDictionary = new Dictionary<MessageType,Type>();

        /// <summary>
        /// Register all message types for lookup
        /// </summary>
        static Message()
        {
            //login/logout
            MessageTypeDictionary.Add(MessageType.Login, typeof(LoginMessage));
            MessageTypeDictionary.Add(MessageType.ResponseLogin, typeof(ResponseLoginMessage));
            MessageTypeDictionary.Add(MessageType.Logout, typeof(LogoutMessage));

            //developers
            MessageTypeDictionary.Add(MessageType.ListDevelopers, typeof(ListDevelopersMessage));
            MessageTypeDictionary.Add(MessageType.ResponseListDevelopers, typeof(ResponseListDevelopersMessage));
            MessageTypeDictionary.Add(MessageType.CreateNewDeveloper, typeof(CreateNewDeveloperMessage));
            MessageTypeDictionary.Add(MessageType.UpdateDeveloper, typeof(UpdateDeveloperMessage));
            MessageTypeDictionary.Add(MessageType.DeleteDeveloper, typeof(DeleteDeveloperMessage));
            MessageTypeDictionary.Add(MessageType.ResponseDeveloperModification, typeof(ResponseDeveloperModificationMessage));
            MessageTypeDictionary.Add(MessageType.GetDeveloper, typeof(GetDeveloperMessage));
            MessageTypeDictionary.Add(MessageType.ResponseGetDeveloper, typeof(ResponseGetDeveloperMessage));

            //plugins
            MessageTypeDictionary.Add(MessageType.ListPlugins, typeof(ListPluginsMessage));
            MessageTypeDictionary.Add(MessageType.ResponseListPlugins, typeof(ResponseListPluginsMessage));
            MessageTypeDictionary.Add(MessageType.CreateNewPlugin, typeof(CreateNewPluginMessage));
            MessageTypeDictionary.Add(MessageType.UpdatePlugin, typeof(UpdatePluginMessage));
            MessageTypeDictionary.Add(MessageType.DeletePlugin, typeof(DeletePluginMessage));
            MessageTypeDictionary.Add(MessageType.ResponsePluginModification, typeof(ResponsePluginModificationMessage));
            MessageTypeDictionary.Add(MessageType.GetPlugin, typeof(GetPluginMessage));
            MessageTypeDictionary.Add(MessageType.ResponseGetPlugin, typeof(ResponseGetPluginMessage));

            //source
            MessageTypeDictionary.Add(MessageType.ListSources, typeof(ListSourcesMessage));
            MessageTypeDictionary.Add(MessageType.ResponseListSources, typeof(ResponseListSourcesMessage));
            MessageTypeDictionary.Add(MessageType.CreateNewSource, typeof(CreateNewSourceMessage));
            MessageTypeDictionary.Add(MessageType.UpdateSource, typeof(UpdateSourceMessage));
            MessageTypeDictionary.Add(MessageType.DeleteSource, typeof(DeleteSourceMessage));
            MessageTypeDictionary.Add(MessageType.ResponseSourceModification, typeof(ResponseSourceModificationMessage));
            MessageTypeDictionary.Add(MessageType.GetSource, typeof(GetSourceMessage));
            MessageTypeDictionary.Add(MessageType.ResponseGetSource, typeof(ResponseGetSourceMessage));

            //resources
            MessageTypeDictionary.Add(MessageType.ListResources, typeof(ListResourcesMessage));
            MessageTypeDictionary.Add(MessageType.ResponseListResources, typeof(ResponseListResourcesMessage));
            MessageTypeDictionary.Add(MessageType.CreateNewResource, typeof(CreateNewResourceMessage));
            MessageTypeDictionary.Add(MessageType.UpdateResource, typeof(UpdateResourceMessage));
            MessageTypeDictionary.Add(MessageType.DeleteResource, typeof(DeleteResourceMessage));
            MessageTypeDictionary.Add(MessageType.ResponseResourceModification, typeof(ResponseResourceModificationMessage));
            MessageTypeDictionary.Add(MessageType.GetResource, typeof(GetResourceMessage));
            MessageTypeDictionary.Add(MessageType.ResponseGetResource, typeof(ResponseGetResourceMessage));

            //resource data
            MessageTypeDictionary.Add(MessageType.ListResourcesData, typeof(ListResourcesDataMessage));
            MessageTypeDictionary.Add(MessageType.ResponseListResourcesData, typeof(ResponseListResourcesDataMessage));
            MessageTypeDictionary.Add(MessageType.CreateNewResourceData, typeof(CreateNewResourceDataMessage));
            MessageTypeDictionary.Add(MessageType.UpdateResourceData, typeof(UpdateResourceDataMessage));
            MessageTypeDictionary.Add(MessageType.DeleteResourceData, typeof(DeleteResourceDataMessage));
            MessageTypeDictionary.Add(MessageType.ResponseResourceDataModification, typeof(ResponseResourceDataModificationMessage));
            MessageTypeDictionary.Add(MessageType.GetResourceData, typeof(GetResourceDataMessage));
            MessageTypeDictionary.Add(MessageType.ResponseGetResourceData, typeof(ResponseGetResourceDataMessage));
        }

        /// <summary>
        /// Constructor
        /// creates message header
        /// </summary>
        public Message()
        {
            MessageHeader = new MessageHeader();
            //detect message type
            bool typeFound = false;
            foreach(Type type in MessageTypeDictionary.Values)
            {
                if (type.Equals(this.GetType()))
                {
                    MessageType typeId = MessageTypeDictionary.FirstOrDefault(x => x.Value.Equals(type)).Key;
                    MessageHeader.MessageType = typeId;
                    typeFound = true;
                }
            }
            if (!typeFound)
            {
                throw new Exception(string.Format("Message type of class \"{0}\" can not be found! Please check and fix lookup dictionary in Messages.cs!", this.GetType().Name));
            }

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
            SerializePayload();
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
            DeserializePayload();
        }

        /// <summary>
        /// Generic method to serialize all members that have a "MessageDataField" attribute
        /// Serialization is independent from ordering of the fields
        /// </summary>
        private void SerializePayload()
        {
            var memberInfos = GetType().GetMembers();
            foreach (var memberInfo in memberInfos)
            {
                bool serializeMember = false;
                foreach (var attribute in memberInfo.GetCustomAttributes(true))
                {
                    if (attribute.GetType().Name.Equals("MessageDataField")) 
                    {
                        serializeMember = true;
                        break;
                    }
                }
                if (serializeMember)
                {
                    FieldInfo fieldType = memberInfo as FieldInfo;
                    PropertyInfo propertyInfo = memberInfo as PropertyInfo;

                    if (fieldType != null)
                    {
                        //namelength        4byte
                        //name              nbyte
                        //valuelength       4byte
                        //value             nbyte
                        Console.WriteLine("Fieldname: " + fieldType.Name);
                        Console.WriteLine("Fieldtype: " + fieldType.FieldType);
                        Console.WriteLine("Data: " + fieldType.GetValue(this));
                    }
                    if(propertyInfo != null)
                    {
                        Console.WriteLine("Fieldname: " + propertyInfo.Name);
                        Console.WriteLine("Fieldtype: " + propertyInfo.PropertyType);
                        Console.WriteLine("Data: " + propertyInfo.GetValue(this));
                    }                                        
                }
            }

        }


        private void DeserializePayload()
        {
            
        }              

        private byte[] Payload { get; set; }
        
        /// <summary>
        /// Generic method which shows all fields and attributes marked with the "MessageDataField" attribute
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();                        
            builder.Append(MessageTypeDictionary[MessageHeader.MessageType] != null ? MessageTypeDictionary[MessageHeader.MessageType].Name : "undefined");
            builder.Append("{");

            var memberInfos = GetType().GetMembers();
            foreach (var memberInfo in memberInfos)
            {
                

                bool showMember = false;
                foreach (var attribute in memberInfo.GetCustomAttributes(true))
                {
                    if (attribute.GetType().Name.Equals("MessageDataField"))
                    {
                        MessageDataField messageDataField = (MessageDataField)attribute;
                        //we only show data in the ToString method which is allowed to be shown
                        //thus, we can hide password fields in log
                        if (messageDataField.ShowInToString)
                        {
                            showMember = true;
                        }
                        break;
                    }
                }
                if (showMember)
                {
                    FieldInfo fieldType = memberInfo as FieldInfo;
                    PropertyInfo propertyInfo = memberInfo as PropertyInfo;

                    if (fieldType != null)
                    {
                        builder.Append(fieldType.Name + "=" + fieldType.GetValue(this) + ", ");
                    }
                    if (propertyInfo != null)
                    {
                        builder.Append(propertyInfo.Name + "=" + propertyInfo.GetValue(this) + ", ");
                    }
                }
            }
            builder.Remove(builder.Length - 2, 2);
            builder.Append("}");

            return builder.ToString();
        }
    }


#region Login messages

    public class LoginMessage : Message
    {
        [MessageDataField]
        public string Username { get; set; }

        [MessageDataField(false)]
        public string Password { get; set; }

        [MessageDataField]
        public DateTime UTCTime { get; set; }
    }
    
    public class ResponseLoginMessage : Message
    {

    }
        
    public class LogoutMessage : Message
    {

    }

#endregion

#region Developers messages

    class ListDevelopersMessage : Message{

    }

    public class ResponseListDevelopersMessage : Message
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

    public class ResponseListResourcesMessage : Message
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

    public class ResponseGetResourceMessage : Message
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
        
    public class ResponseResourceDataModificationMessage : Message
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
