﻿/*
   Copyright 2010 Sven Rech

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
using System.Resources;
using System.Text;

namespace Cryptool.PluginBase.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SettingsTabAttribute : Attribute
    {
        public string Caption { get; private set; }
        public string Address { get; private set; }

        public SettingsTabAttribute(string caption, string address)
        {
            Caption = caption;
            Address = address;
        }
    }
}
