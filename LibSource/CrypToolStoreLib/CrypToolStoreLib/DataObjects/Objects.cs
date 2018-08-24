using CrypToolStoreLib.Tools;
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
using System.Text;
using System.Threading.Tasks;

namespace CrypToolStoreLib.DataObjects
{
    /// <summary>
    /// Simple object to store developer data
    /// </summary>
    public class Developer
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Developer()
        {
            Username = String.Empty;
            Password = String.Empty;
            Firstname = String.Empty;
            Lastname = String.Empty;
            Email = String.Empty;
            IsAdmin = false;
        }

        public override string ToString()
        {
            return String.Format("Developer{{username={0}, firstname={1}, lastname={2}, email={3}, isadmin={4}}}", Username, Firstname, Lastname, Email, (IsAdmin ? "true" : "false"));
        }

        /// <summary>
        /// Serializes this developer into a byte array
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        writer.Write(Username);
                        writer.Write(Password);
                        writer.Write(Firstname);
                        writer.Write(Lastname);
                        writer.Write(Email);
                        writer.Write(IsAdmin);
                        return stream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SerializationException(String.Format("Exception during serialization of developer: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Deserializes a developer from the byte array
        /// </summary>
        /// <param name="bytes"></param>
        public void Deserialize(byte[] bytes)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(bytes))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        Username = reader.ReadString();
                        Password = reader.ReadString();
                        Firstname = reader.ReadString();
                        Lastname = reader.ReadString();
                        Email = reader.ReadString();
                        IsAdmin = reader.ReadBoolean();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DeserializationException(String.Format("Exception during deserialization of developer: {0}", ex.Message));
            }
        }
    }

    /// <summary>
    /// Simple object to store plugin data
    /// </summary>
    public class Plugin
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }
        public string Authornames { get; set; }
        public string Authorinstitutes { get; set; }
        public string Authoremails { get; set; }
        public byte[] Icon { get; set; }
        public int ActiveVersion { get; set; }
        public bool Publish { get; set; }

        public override string ToString()
        {
            return String.Format("Plugin{{id={0}, username={1}, name={2}, shortdescription={3}, longdescription={4}, authornames={5}, authoremails={6}, authorinstitutes={7}, icon={8}, activeversion={9}, publish={10}}}",
                Id, Username, Name, ShortDescription, LongDescription, Authornames, Authoremails, Authorinstitutes, Icon != null ? Icon.Length.ToString() : "null", ActiveVersion, Publish == true ? "true" : "false");
        }
    }

    /// <summary>
    /// Simple object to store source data
    /// </summary>
    public class Source
    {
        public int PluginId { get; set; }
        public int PluginVersion { get; set; }
        public int BuildVersion { get; set; }
        public byte[] ZipFile { get; set; }
        public string BuildState { get; set; }
        public string BuildLog { get; set; }
        public byte[] Assembly { get; set; }
        public DateTime UploadDate { get; set; }
        public DateTime BuildDate { get; set; }

        public override string ToString()
        {
            return String.Format("Source{{pluginid={0}, pluginversion={1}, buildversion={2}, zipfile={3},buildstate={4}, buildlog={5}, assembly={6}, uploaddate={7}, builddate={8}}}",
                PluginId, PluginVersion, BuildVersion, ZipFile != null ? ZipFile.Length.ToString() : "null", BuildState, BuildLog, Assembly != null ? Assembly.Length.ToString() : "null", UploadDate, BuildDate);
        }        
    }

    /// <summary>
    /// Simple object to store resource data
    /// </summary>
    public class Resource
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ActiveVersion { get; set; }
        public bool Publish { get; set; }

        public override string ToString()
        {
            return String.Format("Resource{{id={0}, username={1}, name={2}, description={3}, activeversion={4}, publish={5}}}",
                Id, Username, Name, Description, ActiveVersion, Publish == true ? "true" : "false");
        }        
    }

    /// <summary>
    /// Simple object to store resourceData data
    /// </summary>
    public class ResourceData
    {
        public int ResourceId { get; set; }
        public int Version { get; set; }
        public byte[] Data { get; set; }
        public DateTime UploadDate { get; set; }

        public override string ToString()
        {
            return String.Format("ResourceData{{resourceid={0}, version={1}, data={2}, uploadtime={3}}}",
                ResourceId, Version, Data != null ? Data.Length.ToString() : "null", UploadDate);
        }
    }
}
