﻿/*
   Copyright 2008 Martin Saternus, University of Duisburg-Essen

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

namespace Cryptool.PluginBase
{
    /// <summary>
    /// This attribute is optional for the properties of the ISettings class. If
    /// you need an explicit order (see Caesar for example) you can use this
    /// Attribute. If the order doesn't matter, just skip using this attribute
    /// and your properties will be stored in alphabetical order.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertySaveOrderAttribute : Attribute
    {
        public readonly int Order;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertySaveOrderAttribute"/> class.
        /// </summary>
        /// <param name="order">The order in which the property will be saved and restored.</param>
        public PropertySaveOrderAttribute(int order)
        {
            this.Order = order;
        }
    }
}
