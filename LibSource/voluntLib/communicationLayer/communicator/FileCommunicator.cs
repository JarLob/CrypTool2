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
using System.Numerics;
using System.Xml;
using System.Xml.Linq;
using voluntLib.common.interfaces;
using voluntLib.communicationLayer.messages.messageWithCertificate;

#endregion

namespace voluntLib.communicationLayer.communicator
{
    public class FileCommunicator : ICommunicator
    {
        #region private members

        private const string RootElement = "voluntLibStore";
        private const string JobElement = "job";
        private readonly bool clearOnStart;
        private readonly bool enablePersistence;

        private readonly string filePath;
        private readonly bool loadOnStart;

        private ICommunicationLayer comLayer;

        #endregion

        public FileCommunicator(string filePath, bool loadOnStart, bool enablePersistence, bool clearOnStart)
        {
            this.filePath = filePath;
            this.loadOnStart = loadOnStart;
            this.enablePersistence = enablePersistence;
            this.clearOnStart = clearOnStart;
        }

        public void RegisterCommunicationLayer(ICommunicationLayer communicationLayer)
        {
            comLayer = communicationLayer;
        }

        public void ProcessMessage(AMessage data, IPAddress to)
        {
            if (!enablePersistence)
                return;

            var doc = new XmlDocument();
            doc.Load(filePath);
            var jobNode = FindOrCreateJobNode(doc, data.Header.JobID);
            var selectSingleNode = CreateOrSelectSingleNode(((MessageType) data.Header.MessageType).ToString(), jobNode, doc);
            selectSingleNode.InnerText = Convert.ToBase64String(data.Serialize());
            doc.Save(filePath);
        }

        #region helper

        private static XmlNode CreateOrSelectSingleNode(string qualifiedName, XmlNode jobNode, XmlDocument doc)
        {
            var selectSingleNode = jobNode.SelectSingleNode(qualifiedName);
            if (selectSingleNode == null)
            {
                selectSingleNode = doc.CreateElement(qualifiedName);
                jobNode.AppendChild(selectSingleNode);
            }

            return selectSingleNode;
        }

        private static XmlNode FindOrCreateJobNode(XmlDocument doc, BigInteger jobID)
        {
            var node = doc.SelectSingleNode("descendant::" + JobElement + "[@id='" + jobID + "']");
            if (node != null)
                return node;

            //create job root
            node = doc.CreateElement(JobElement);
            var attribute = doc.CreateAttribute("id");
            attribute.Value = jobID.ToString();

            if (node.Attributes != null)
                node.Attributes.Append(attribute);

            if (doc.DocumentElement != null)
                doc.DocumentElement.AppendChild(node);

            return node;
        }

        #endregion

        #region start stop

        public void Start()
        {
            CreateFileIfNecessary();

            if (!loadOnStart)
                return;

            var doc = new XmlDocument();
            doc.Load(filePath);
            var nodes = doc.SelectNodes("//" + JobElement);
            if (nodes == null) 
                return;

            //push messages to comLayer
            foreach (XmlNode jobNode in nodes)
            {
                var createNode = jobNode.SelectSingleNode(MessageType.CreateNetworkJob.ToString());
                if (createNode != null)
                    comLayer.HandleIncomingMessages(Convert.FromBase64String(createNode.InnerText), IPAddress.None); 
                
                var deleteNode = jobNode.SelectSingleNode(MessageType.DeleteNetworkJob.ToString());
                if (deleteNode != null)
                    comLayer.HandleIncomingMessages(Convert.FromBase64String(deleteNode.InnerText), IPAddress.None);

                var stateNode = jobNode.SelectSingleNode(MessageType.PropagateState.ToString());
                if (stateNode != null)
                    comLayer.HandleIncomingMessages(Convert.FromBase64String(stateNode.InnerText), IPAddress.None);
                
            }
        }

        public void Stop()
        { 
            /* since we are stateless, we dont have to do anything on stop */
        }

        private void CreateFileIfNecessary()
        {
            var lastIndexOf = filePath.LastIndexOf('\\');
            if (lastIndexOf != -1)
            {
                var directoryPath = filePath.Substring(0, lastIndexOf);
                if ( ! Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);
                
            }

            if (!File.Exists(filePath) || clearOnStart)
                new XDocument(new XElement(RootElement)).Save(filePath);
        }

        #endregion
    }
}