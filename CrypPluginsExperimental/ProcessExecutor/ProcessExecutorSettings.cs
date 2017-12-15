﻿/*
   Copyright 2017 Nils Kopal, Applied Information Security, Uni Kassel
   http://www.uni-kassel.de/eecs/fachgebiete/ais/mitarbeiter/nils-kopal-m-sc.html

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

using Cryptool.PluginBase;
using Cryptool.PluginBase.Attributes;
using System.ComponentModel;


namespace Cryptool.ProcessExecutor
{

    class ProcessExecutorSettings : ISettings
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void Initialize()
        {
            
        }

        
    }
}
