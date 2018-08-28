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
using CrypToolStoreLib.DataObjects;

namespace CrypToolStoreLib.Client
{
    public class Client
    {
        public Client()
        {

        }

        public void Connect()
        {

        }

        public void Disconnect()
        {

        }

        #region Methods for working with developers

        public string CreateNewDeveloper()
        {

            return string.Empty;
        }

        public string UpdateDeveloper()
        {
            return string.Empty;
        }

        public string DeleteDeveloper()
        {
            return string.Empty;
        }

        public List<Developer> GetDeveloperList()
        {
            return null;
        }

        public Developer GetDeveloper()
        {
            return null;
        }

        #endregion

        #region Methods for working with plugins

        public string CreatePlugin()
        {
            return string.Empty;
        }

        public string UpdatePlugin()
        {
            return string.Empty;
        }

        public string DeletePlugin()
        {
            return string.Empty;
        }

        public Plugin GetPlugin()
        {
            return null;
        }

        public List<Plugin> GetPluginList()
        {
            return null;
        }

        #endregion

        #region Methods for working with sources

        public string CreateSource()
        {
            return string.Empty;
        }

        public string UpdateSource()
        {
            return string.Empty;
        }

        public string DeleteSource()
        {
            return string.Empty;
        }

        public Source GetSource()
        {
            return null;
        }

        public List<Source> GetSourceList()
        {
            return null;
        }

        #endregion

        #region Methods for working with Resources

        public string CreateResource()
        {
            return string.Empty;
        }

        public string UpdateResource()
        {
            return string.Empty;
        }

        public string DeleteResource()
        {
            return string.Empty;
        }

        public Resource GetResource()
        {
            return null;
        }

        public List<Resource> GetResourceList()
        {
            return null;
        }

        #endregion

        #region Methods for working with ResourceDatas

        public string CreateResourceData()
        {
            return string.Empty;
        }

        public string UpdateResourceData()
        {
            return string.Empty;
        }

        public string DeleteResourceData()
        {
            return string.Empty;
        }

        public ResourceData GetResourceData()
        {
            return null;
        }

        public List<ResourceData> GetResourceDataList()
        {
            return null;
        }

        #endregion

    }
}
