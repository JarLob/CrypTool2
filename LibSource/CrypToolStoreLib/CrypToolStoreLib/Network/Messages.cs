﻿using CrypToolStoreLib.DataObjects;
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CrypToolStoreLib.Tools
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

        RequestDeveloperList = 100,
        ResponseDeveloperList = 101,
        CreateNewDeveloper = 102,
        UpdateDeveloper = 103,
        DeleteDeveloper = 104,
        ResponseDeveloperModification = 105,
        RequestDeveloper = 106,
        ResponseDeveloper = 107,

        //Messages for "plugins"
        RequestPluginList = 200,
        ResponsePluginList = 201,
        CreateNewPlugin = 202,
        UpdatePlugin = 203,
        DeletePlugin = 204,
        ResponsePluginModification = 205,
        RequestPlugin = 206,
        ResponsePlugin = 207,
        
        //Message for "Source"
        RequestSourceList = 300,
        ResponseSourceList = 301,
        CreateNewSource = 302,
        UpdateSource = 303,
        DeleteSource = 304,
        ResponseSourceModification = 305,
        RequestSource = 306,
        ResponseSource = 307,
        
        //Message for "Resources"
        RequestResourceMessage = 400,
        ResponseResourceList = 401,
        CreateNewResource = 402,
        UpdateResource = 403,
        DeleteResource = 404,
        ResponseResourceModification = 405,
        RequestResource = 406,
        ResponseResource = 407,
        
        //Message for "ResourcesData"
        RequestResourceDataList = 500,
        ResponseResourceDataList = 501,
        CreateNewResourceData = 502,
        UpdateResourceData = 503,
        DeleteResourceData = 504,
        ResponseResourceDataModification = 505,
        RequestResourceData = 506,
        ResponseGetResourceData = 507,

        //server error message
        ServerError = 600,
        ClientError = 601,

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
            MessageTypeDictionary.Add(MessageType.RequestDeveloperList, typeof(RequestDeveloperListMessage));
            MessageTypeDictionary.Add(MessageType.ResponseDeveloperList, typeof(ResponseDeveloperListMessage));
            MessageTypeDictionary.Add(MessageType.CreateNewDeveloper, typeof(CreateNewDeveloperMessage));
            MessageTypeDictionary.Add(MessageType.UpdateDeveloper, typeof(UpdateDeveloperMessage));
            MessageTypeDictionary.Add(MessageType.DeleteDeveloper, typeof(DeleteDeveloperMessage));
            MessageTypeDictionary.Add(MessageType.ResponseDeveloperModification, typeof(ResponseDeveloperModificationMessage));
            MessageTypeDictionary.Add(MessageType.RequestDeveloper, typeof(RequestDeveloperMessage));
            MessageTypeDictionary.Add(MessageType.ResponseDeveloper, typeof(ResponseDeveloperMessage));

            //plugins
            MessageTypeDictionary.Add(MessageType.RequestPluginList, typeof(RequestPluginListMessage));
            MessageTypeDictionary.Add(MessageType.ResponsePluginList, typeof(ResponsePluginListMessage));
            MessageTypeDictionary.Add(MessageType.CreateNewPlugin, typeof(CreateNewPluginMessage));
            MessageTypeDictionary.Add(MessageType.UpdatePlugin, typeof(UpdatePluginMessage));
            MessageTypeDictionary.Add(MessageType.DeletePlugin, typeof(DeletePluginMessage));
            MessageTypeDictionary.Add(MessageType.ResponsePluginModification, typeof(ResponsePluginModificationMessage));
            MessageTypeDictionary.Add(MessageType.RequestPlugin, typeof(RequestPluginMessage));
            MessageTypeDictionary.Add(MessageType.ResponsePlugin, typeof(ResponsePluginMessage));

            //source
            MessageTypeDictionary.Add(MessageType.RequestSourceList, typeof(RequestSourceListMessage));
            MessageTypeDictionary.Add(MessageType.ResponseSourceList, typeof(ResponseSourceListMessage));
            MessageTypeDictionary.Add(MessageType.CreateNewSource, typeof(CreateNewSourceMessage));
            MessageTypeDictionary.Add(MessageType.UpdateSource, typeof(UpdateSourceMessage));
            MessageTypeDictionary.Add(MessageType.DeleteSource, typeof(DeleteSourceMessage));
            MessageTypeDictionary.Add(MessageType.ResponseSourceModification, typeof(ResponseSourceModificationMessage));
            MessageTypeDictionary.Add(MessageType.RequestSource, typeof(RequestSourceMessage));
            MessageTypeDictionary.Add(MessageType.ResponseSource, typeof(ResponseSourceMessage));

            //resources
            MessageTypeDictionary.Add(MessageType.RequestResourceMessage, typeof(RequestResourceListMessage));
            MessageTypeDictionary.Add(MessageType.ResponseResourceList, typeof(ResponseResourceListMessage));
            MessageTypeDictionary.Add(MessageType.CreateNewResource, typeof(CreateNewResourceMessage));
            MessageTypeDictionary.Add(MessageType.UpdateResource, typeof(UpdateResourceMessage));
            MessageTypeDictionary.Add(MessageType.DeleteResource, typeof(DeleteResourceMessage));
            MessageTypeDictionary.Add(MessageType.ResponseResourceModification, typeof(ResponseResourceModificationMessage));
            MessageTypeDictionary.Add(MessageType.RequestResource, typeof(RequestResourceMessage));
            MessageTypeDictionary.Add(MessageType.ResponseResource, typeof(ResponseResourceMessage));

            //resource data
            MessageTypeDictionary.Add(MessageType.RequestResourceDataList, typeof(RequestResourceDataListMessage));
            MessageTypeDictionary.Add(MessageType.ResponseResourceDataList, typeof(ResponseResourceDataListMessage));
            MessageTypeDictionary.Add(MessageType.CreateNewResourceData, typeof(CreateNewResourceDataMessage));
            MessageTypeDictionary.Add(MessageType.UpdateResourceData, typeof(UpdateResourceDataMessage));
            MessageTypeDictionary.Add(MessageType.DeleteResourceData, typeof(DeleteResourceDataMessage));
            MessageTypeDictionary.Add(MessageType.ResponseResourceDataModification, typeof(ResponseResourceDataModificationMessage));
            MessageTypeDictionary.Add(MessageType.RequestResourceData, typeof(GetResourceDataMessage));
            MessageTypeDictionary.Add(MessageType.ResponseGetResourceData, typeof(RequestResourceDataMessage));

            //error messages
            MessageTypeDictionary.Add(MessageType.ServerError, typeof(ServerErrorMessage));
            MessageTypeDictionary.Add(MessageType.ClientError, typeof(ClientErrorMessage));
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
        public virtual byte[] Serialize()
        {
            byte[] headerbytes = MessageHeader.Serialize();
            SerializePayload();
            byte[] bytes = new byte[headerbytes.Length + (Payload != null ? Payload.Length : 0)];
            Array.Copy(headerbytes, 0, bytes, 0, headerbytes.Length);
            if (Payload != null && Payload.Length > 0)
            {
                Array.Copy(Payload, 0, bytes, headerbytes.Length, Payload.Length);
            }
            //after serialization, we do not need the payload any more
            Payload = null;
            return bytes;
        }

        /// <summary>
        /// Deserializes a message from the byte array
        /// </summary>
        /// <param name="bytes"></param>
        public virtual void Deserialize(byte[] bytes)
        {
            int offset = MessageHeader.Deserialize(bytes);
            if (offset < bytes.Length - 1)
            {
                Payload = new byte[bytes.Length - offset];
                Array.Copy(bytes, offset, Payload, 0, bytes.Length - offset);
            }
            DeserializePayload();
            //after deserialization, we do not need the payload any more
            Payload = null;
        }

        /// <summary>
        /// Generic method to serialize all members that have a "MessageDataField" attribute
        /// Serialization is independent from ordering of the fields
        /// </summary>
        private void SerializePayload()
        {
            using (MemoryStream stream = new MemoryStream())
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
                        FieldInfo fieldInfo = memberInfo as FieldInfo;
                        PropertyInfo propertyInfo = memberInfo as PropertyInfo;

                        if (fieldInfo != null)
                        {
                            byte[] namebytes = ASCIIEncoding.ASCII.GetBytes(fieldInfo.Name);
                            byte[] namelengthbytes = BitConverter.GetBytes((UInt32)fieldInfo.Name.Length);
                            byte[] valuebytes = new byte[0];
                            switch (fieldInfo.FieldType.Name)
                            {
                                case "String":
                                    valuebytes = UTF8Encoding.UTF8.GetBytes((string)fieldInfo.GetValue(this));
                                    break;
                                case "Int16":
                                    valuebytes = BitConverter.GetBytes((Int16)fieldInfo.GetValue(this));
                                    break;
                                case "Int32":
                                    valuebytes = BitConverter.GetBytes((Int32)fieldInfo.GetValue(this));
                                    break;
                                case "Int64":
                                    valuebytes = BitConverter.GetBytes((Int64)fieldInfo.GetValue(this));
                                    break;
                                case "Double":
                                    valuebytes = BitConverter.GetBytes((Double)fieldInfo.GetValue(this));
                                    break;
                                case "Single":
                                    valuebytes = BitConverter.GetBytes((float)fieldInfo.GetValue(this));
                                    break;
                                case "Byte[]":
                                    valuebytes = (byte[])fieldInfo.GetValue(this);
                                    break;
                                case "Byte":
                                    valuebytes = new byte[] { (byte)fieldInfo.GetValue(this) };
                                    break;
                                case "DateTime":
                                    valuebytes = BitConverter.GetBytes(((DateTime)fieldInfo.GetValue(this)).ToBinary());
                                    break;
                                default:
                                    if (fieldInfo.FieldType.GetInterface("ICrypToolStoreSerializable")  != null)
                                    {
                                        //ICrypToolStoreSerializable implement serialization; thus, we can serialize them and put them into the message
                                        ICrypToolStoreSerializable serializable = (ICrypToolStoreSerializable)fieldInfo.GetValue(this);
                                        valuebytes = serializable.Serialize();
                                    }
                                    else if (fieldInfo.FieldType.IsGenericType &&  //we have a generic type
                                         fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(List<>) && //which is a list
                                         fieldInfo.FieldType.GenericTypeArguments[0].GetInterfaces().Contains(typeof(ICrypToolStoreSerializable))) //that contains an object implementing ICrypToolStoreSerializable
                                    {
                                        //Here, we serialize a generic list that contains data object deriving from ICrypToolStoreSerializable
                                        using (MemoryStream stream2 = new MemoryStream())
                                        {
                                            using (BinaryWriter writer = new BinaryWriter(stream2))
                                            {
                                                dynamic list = (dynamic)fieldInfo.GetValue(this);
                                                int count = list.Count;
                                                writer.Write(count);
                                                foreach (var entry in list)
                                                {
                                                    ICrypToolStoreSerializable serializable = (ICrypToolStoreSerializable)entry;
                                                    byte[] bytes = serializable.Serialize();
                                                    writer.Write(bytes.Length);
                                                    writer.Write(bytes);
                                                }
                                            }
                                            valuebytes = stream2.ToArray();
                                        }
                                    }
                                    else
                                    {
                                        throw new SerializationException(String.Format("Fieldtype \"{0}\" of field \"{1}\" of class \"{2}\" can not be serialized!", fieldInfo.FieldType.Name, fieldInfo.Name, this.GetType().Name));
                                    }
                                    break;
                            }

                            byte[] valuelengthbytes = BitConverter.GetBytes(valuebytes.Length);

                            //namelength        4 byte
                            //name              n byte
                            //valuelength       4 byte
                            //value             n byte
                            stream.Write(namelengthbytes, 0, namelengthbytes.Length);
                            stream.Write(namebytes, 0, namebytes.Length);
                            stream.Write(valuelengthbytes, 0, valuelengthbytes.Length);
                            stream.Write(valuebytes, 0, valuebytes.Length);
                            stream.Flush();
                        }
                        if (propertyInfo != null)
                        {
                            byte[] namebytes = ASCIIEncoding.ASCII.GetBytes(propertyInfo.Name);
                            byte[] namelengthbytes = BitConverter.GetBytes((UInt32)propertyInfo.Name.Length);
                            byte[] valuebytes = new byte[0];
                            switch (propertyInfo.PropertyType.Name)
                            {
                                case "String":
                                    valuebytes = UTF8Encoding.UTF8.GetBytes((string)propertyInfo.GetValue(this));
                                    break;
                                case "Int16":
                                    valuebytes = BitConverter.GetBytes((Int16)propertyInfo.GetValue(this));
                                    break;
                                case "Int32":
                                    valuebytes = BitConverter.GetBytes((Int32)propertyInfo.GetValue(this));
                                    break;
                                case "Int64":
                                    valuebytes = BitConverter.GetBytes((Int64)propertyInfo.GetValue(this));
                                    break;
                                case "Double":
                                    valuebytes = BitConverter.GetBytes((Double)propertyInfo.GetValue(this));
                                    break;
                                case "Single":
                                    valuebytes = BitConverter.GetBytes((float)propertyInfo.GetValue(this));
                                    break;
                                case "Byte[]":
                                    valuebytes = (byte[])propertyInfo.GetValue(this);
                                    break;
                                case "Byte":
                                    valuebytes = new byte[] { (byte)propertyInfo.GetValue(this) };
                                    break;
                                case "DateTime":
                                    valuebytes = BitConverter.GetBytes(((DateTime)propertyInfo.GetValue(this)).ToBinary());
                                    break;
                                default:                                    
                                    if (propertyInfo.PropertyType.GetInterface("ICrypToolStoreSerializable") != null)
                                    {
                                        //ICrypToolStoreSerializable implement serialization; thus, we can serialize them and put them into the message
                                        ICrypToolStoreSerializable serializable = (ICrypToolStoreSerializable)propertyInfo.GetValue(this);
                                        valuebytes = serializable.Serialize();
                                    }
                                    else if (propertyInfo.PropertyType.IsGenericType &&  //we have a generic type
                                        propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(List<>) && //which is a list
                                        propertyInfo.PropertyType.GenericTypeArguments[0].GetInterfaces().Contains(typeof(ICrypToolStoreSerializable))) //that contains an object implementing ICrypToolStoreSerializable
                                    {
                                        //Here, we serialize a generic list that contains data object deriving from ICrypToolStoreSerializable
                                        using (MemoryStream stream2 = new MemoryStream())
                                        {
                                            using (BinaryWriter writer = new BinaryWriter(stream2))
                                            {
                                                dynamic list = (dynamic)propertyInfo.GetValue(this);
                                                int count = list.Count;
                                                writer.Write(count);
                                                foreach (var entry in list)
                                                {
                                                    ICrypToolStoreSerializable serializable = (ICrypToolStoreSerializable)entry;
                                                    byte[] bytes = serializable.Serialize();
                                                    writer.Write(bytes.Length);
                                                    writer.Write(bytes);
                                                }
                                            }
                                            valuebytes = stream2.ToArray();
                                        }
                                    }
                                    else
                                    {
                                        throw new SerializationException(String.Format("Propertytype \"{0}\" of property \"{1}\" of class \"{2}\" can not be serialized!", propertyInfo.PropertyType.Name, propertyInfo.Name, this.GetType().Name));
                                    }
                                    break;                                    
                            }
                            byte[] valuelengthbytes = BitConverter.GetBytes(valuebytes.Length);

                            //namelength        4 byte
                            //name              n byte
                            //valuelength       4 byte
                            //value             n byte
                            stream.Write(namelengthbytes, 0, namelengthbytes.Length);
                            stream.Write(namebytes, 0, namebytes.Length);
                            stream.Write(valuelengthbytes, 0, valuelengthbytes.Length);
                            stream.Write(valuebytes, 0, valuebytes.Length);
                            stream.Flush();
                        }
                    }
                }
                Payload = stream.ToArray();
            }
        }

        /// <summary>
        /// Generic method to deserialize all data received within the payload of the message
        /// Deserialization is independent from ordering of the fields marked with "MessageDataField" attribute
        /// </summary>
        private void DeserializePayload()
        {
            using (MemoryStream stream = new MemoryStream(Payload))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        try
                        {
                            //namelength        4 byte
                            //name              n byte
                            //valuelength       4 byte
                            //value             n byte
                            
                            //get name of field or property
                            byte[] namelengthbytes = reader.ReadBytes(4);
                            int namelength = (int)BitConverter.ToUInt32(namelengthbytes, 0);
                            byte[] namebytes = reader.ReadBytes(namelength);
                            string name = ASCIIEncoding.ASCII.GetString(namebytes);

                            //get value as byte array
                            byte[] valuelengthbytes = reader.ReadBytes(4);
                            int valuelength = (int)BitConverter.ToUInt32(valuelengthbytes, 0);
                            byte[] valuebytes = reader.ReadBytes(valuelength);
                            
                            //get member and set value
                            MemberInfo[] memberInfo = GetType().GetMember(name);
                            if (memberInfo == null || memberInfo.Length == 0)
                            {
                                throw new DeserializationException(String.Format("Can not find any member with name \"{0}\" for deserialization!", name));
                            }

                            FieldInfo fieldInfo = memberInfo[0] as FieldInfo;
                            PropertyInfo propertyInfo = memberInfo[0] as PropertyInfo;

                            if (fieldInfo != null)
                            {
                                switch (fieldInfo.FieldType.Name)
                                {
                                    case "String":
                                        fieldInfo.SetValue(this, UTF8Encoding.UTF8.GetString(valuebytes));
                                        break;
                                    case "Int16":
                                        fieldInfo.SetValue(this, BitConverter.ToInt16(valuebytes, 0));
                                        break;
                                    case "Int32":
                                        fieldInfo.SetValue(this, BitConverter.ToInt32(valuebytes, 0));
                                        break;
                                    case "Int64":
                                        fieldInfo.SetValue(this, BitConverter.ToInt64(valuebytes, 0));
                                        break;
                                    case "Double":
                                        fieldInfo.SetValue(this, BitConverter.ToDouble(valuebytes, 0));
                                        break;
                                    case "Single":
                                        fieldInfo.SetValue(this, BitConverter.ToSingle(valuebytes, 0));
                                        break;
                                    case "Byte[]":
                                        fieldInfo.SetValue(this, valuebytes);
                                        break;
                                    case "Byte":
                                        fieldInfo.SetValue(this, valuebytes[0]);
                                        break;
                                    case "DateTime":
                                        fieldInfo.SetValue(this, DateTime.FromBinary(BitConverter.ToInt64(valuebytes, 0)));
                                        break;
                                    default:
                                        if (fieldInfo.FieldType.GetInterface("ICrypToolStoreSerializable") != null)
                                        {
                                            //ICrypToolStoreSerializable implement serialization; thus, we can deserialize it                                             
                                            ICrypToolStoreSerializable serializable = (ICrypToolStoreSerializable)Activator.CreateInstance(fieldInfo.FieldType);
                                            serializable.Deserialize(valuebytes);
                                            fieldInfo.SetValue(this, serializable);
                                        }
                                        else if (fieldInfo.FieldType.IsGenericType &&  //we have a generic type
                                           fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(List<>) && //which is a list
                                           fieldInfo.FieldType.GenericTypeArguments[0].GetInterfaces().Contains(typeof(ICrypToolStoreSerializable))) //that contains an object implementing ICrypToolStoreSerializable
                                        {
                                            //Here, we deserialize a generic list that contains data object deriving from ICrypToolStoreSerializable
                                            using (MemoryStream stream2 = new MemoryStream(valuebytes))
                                            {
                                                using (BinaryReader reader2 = new BinaryReader(stream2))
                                                {
                                                    dynamic list = Activator.CreateInstance(fieldInfo.FieldType);
                                                    fieldInfo.SetValue(this, list);
                                                    int count = reader2.ReadInt32();
                                                    for (int i = 0; i < count; i++)
                                                    {
                                                        int bytecount = reader2.ReadInt32();
                                                        byte[] bytes = reader2.ReadBytes(bytecount);
                                                        dynamic serializable = (dynamic)Activator.CreateInstance(fieldInfo.FieldType.GenericTypeArguments[0]);
                                                        serializable.Deserialize(bytes);
                                                        list.Add(serializable);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            throw new SerializationException(String.Format("Fieldtype \"{0}\" of field \"{1}\" of class \"{2}\" can not be deserialized!", fieldInfo.FieldType.Name, fieldInfo.Name, this.GetType().Name));
                                        }
                                        break;
                                }
                            }
                            if (propertyInfo != null)
                            {
                                switch (propertyInfo.PropertyType.Name)
                                {
                                    case "String":
                                        propertyInfo.SetValue(this, UTF8Encoding.UTF8.GetString(valuebytes));
                                        break;
                                    case "Int16":
                                        propertyInfo.SetValue(this, BitConverter.ToInt16(valuebytes, 0));
                                        break;
                                    case "Int32":
                                        propertyInfo.SetValue(this, BitConverter.ToInt32(valuebytes, 0));
                                        break;
                                    case "Int64":
                                        fieldInfo.SetValue(this, BitConverter.ToInt64(valuebytes, 0));
                                        break;
                                    case "Double":
                                        propertyInfo.SetValue(this, BitConverter.ToDouble(valuebytes, 0));
                                        break;
                                    case "Single":
                                        propertyInfo.SetValue(this, BitConverter.ToSingle(valuebytes, 0));
                                        break;
                                    case "Byte[]":
                                        propertyInfo.SetValue(this, valuebytes);
                                        break;
                                    case "Byte":
                                        propertyInfo.SetValue(this, valuebytes[0]);
                                        break;
                                    case "DateTime":
                                        propertyInfo.SetValue(this, DateTime.FromBinary(BitConverter.ToInt64(valuebytes, 0)));                                        
                                        break;
                                    default:
                                        if (propertyInfo.PropertyType.GetInterface("ICrypToolStoreSerializable") != null)
                                        {
                                            //ICrypToolStoreSerializable implement serialization; thus, we can deserialize it                                             
                                            ICrypToolStoreSerializable serializable = (ICrypToolStoreSerializable)Activator.CreateInstance(propertyInfo.PropertyType);
                                            serializable.Deserialize(valuebytes);
                                            propertyInfo.SetValue(this, serializable);
                                        }
                                        else if (propertyInfo.PropertyType.IsGenericType &&  //we have a generic type
                                            propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(List<>) && //which is a list
                                            propertyInfo.PropertyType.GenericTypeArguments[0].GetInterfaces().Contains(typeof(ICrypToolStoreSerializable))) //that contains an object implementing ICrypToolStoreSerializable
                                        {
                                            //Here, we deserialize a generic list that contains data object deriving from ICrypToolStoreSerializable
                                            using (MemoryStream stream2 = new MemoryStream(valuebytes))
                                            {
                                                using (BinaryReader reader2 = new BinaryReader(stream2))
                                                {
                                                    dynamic list = Activator.CreateInstance(propertyInfo.PropertyType);
                                                    propertyInfo.SetValue(this, list);
                                                    int count = reader2.ReadInt32();
                                                    for (int i = 0; i < count; i++)
                                                    {
                                                        int bytecount = reader2.ReadInt32();
                                                        byte[] bytes = reader2.ReadBytes(bytecount);
                                                        dynamic serializable = (dynamic)Activator.CreateInstance(propertyInfo.PropertyType.GenericTypeArguments[0]);
                                                        serializable.Deserialize(bytes);
                                                        list.Add(serializable);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            throw new SerializationException(String.Format("Propertytype \"{0}\" of property \"{1}\" of class \"{2}\" can not be deserialized!", propertyInfo.PropertyType.Name, propertyInfo.Name, this.GetType().Name));
                                        }
                                        break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new DeserializationException(String.Format("Exception during deserialization of message \"{0}\": {1}", this.GetType().Name, ex.Message));
                        }
                    }
                }
            }
        }              

        /// <summary>
        /// Only used during serialiazion and deserialization
        /// </summary>
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
                        object value = fieldType.GetValue(this);                        
                        builder.Append(fieldType.Name + "=" + (value.GetType().Name.Equals("Byte[]") ? Tools.ByteArrayToHexString((byte[])value) : value ) + ", ");
                    }
                    if (propertyInfo != null)
                    {
                        object value = propertyInfo.GetValue(this);
                        builder.Append(propertyInfo.Name + "=" + (value.GetType().Name.Equals("Byte[]") ? Tools.ByteArrayToHexString((byte[])value) : value) + ", ");
                    }
                }
            }
            builder.Remove(builder.Length - 2, 2);
            builder.Append("}");

            return builder.ToString();
        }
    }

#region Login messages

    /// <summary>
    /// Message used for login in by developer/user
    /// </summary>
    public class LoginMessage : Message
    {
        [MessageDataField]
        public string Username { get; set; }

        [MessageDataField(false)]
        public string Password { get; set; }

        [MessageDataField]
        public DateTime UTCTime { get; set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public LoginMessage()
        {
            Username = string.Empty;
            Password = string.Empty;
            UTCTime = DateTime.UtcNow;
        }
    }
    
    /// <summary>
    /// Message send in response to the login request of the developer/user
    /// </summary>
    public class ResponseLoginMessage : Message
    {
        [MessageDataField]
        public bool LoginOk;

        [MessageDataField]
        public string Message;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ResponseLoginMessage()
        {
            LoginOk = false;
            Message = String.Empty;
        }
    }
    
    /// <summary>
    /// Send to indicate, that a logout occurs
    /// Can be send by client and server
    /// </summary>
    public class LogoutMessage : Message
    {
        [MessageDataField]
        public string Username { get; set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public LogoutMessage()
        {
            Username = String.Empty;
        }
    }

#endregion

#region Developers messages

    /// <summary>
    /// Message to request the list of developers
    /// </summary>
    public class RequestDeveloperListMessage : Message
    {                

    }

    /// <summary>
    /// Message to response to request message
    /// </summary>
    public class ResponseDeveloperListMessage : Message
    {
        [MessageDataField]
        public List<Developer> DeveloperList
        {
            get;
            set;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ResponseDeveloperListMessage()
        {
            DeveloperList = new List<Developer>();
        }       
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

    public class RequestDeveloperMessage : Message
    {

    }

    public class  ResponseDeveloperMessage : Message
    {

    }

#endregion

#region Plugin messages

    public class RequestPluginListMessage : Message
    {

    }

    public class ResponsePluginListMessage : Message
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

    public class RequestPluginMessage : Message
    {

    }

    public class ResponsePluginMessage : Message
    {

    }

#endregion

#region Sources messages
       
    public class RequestSourceListMessage : Message
    {
    
    }

    public class ResponseSourceListMessage : Message
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

    public class RequestSourceMessage : Message 
    {

    }
    
    public class ResponseSourceMessage : Message
    {

    }
        
#endregion

#region Resources messages

    public class RequestResourceListMessage : Message
    {
    
    }

    public class ResponseResourceListMessage : Message
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

    public class RequestResourceMessage : Message
    {

    }

    public class ResponseResourceMessage : Message
    {

    }
        
#endregion

#region ResourcesData messages
    
    public class RequestResourceDataListMessage : Message
    {

    }
    
    public class ResponseResourceDataListMessage : Message
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

    public class RequestResourceDataMessage : Message
    {

    }

#endregion

#region Error messages

    public class ServerErrorMessage : Message
    {

    }

    public class ClientErrorMessage : Message
    {

    }
#endregion
}