﻿/*
   Copyright 2008-2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.Reflection;

namespace Cryptool.PluginBase
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyInfoAttribute : Attribute
    {
        # region multi language properties
        public readonly string caption;
        public string Caption
        {
          get
          {
            if (MultiLanguage && caption != null)
              return PluginType.GetPluginStringResource(caption);
            else
              return caption;
          }
        }
        
        public readonly string toolTip;
        public string ToolTip
        {
          get
          {
            if (MultiLanguage && toolTip != null)
              return PluginType.GetPluginStringResource(toolTip);
            else
              return toolTip;
          }
        }
        # endregion multi language properties

        # region normal properties
        public readonly Direction Direction;
        public readonly bool Mandatory;
        public QuickWatchFormat QuickWatchFormat;
        public string PropertyName; // will be set in extension-method
        public PropertyInfo PropertyInfo { get; set; } // will be set in extension-method
        public readonly string QuickWatchConversionMethod;
        #endregion normal properties

        # region translation helpers

        /// <summary>
        /// Gets or sets the type of the plugin. This value is set by extension method if ResourceFile exists. 
        /// It is used to access the plugins resources to translate the text elements.
        /// </summary>
        /// <value>The type of the plugin.</value>
        public Type PluginType { get; set; }

        private bool MultiLanguage
        {
          get { return PluginType != null && PluginType.GetPluginInfoAttribute().ResourceFile != null; }
        }
        # endregion translation helpers     

        #region constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyInfoAttribute"/> class.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="toolTip">The tool tip.</param>
        /// <param name="mandatory">if set to <c>true</c> [mandatory].</param>
        /// <param name="quickWatchFormat">The quick watch format.</param>
        /// <param name="quickWatchConversionMethod">Methodname of converstion method.</param>
        public PropertyInfoAttribute(Direction direction, string caption, string toolTip, bool mandatory, QuickWatchFormat quickWatchFormat, string quickWatchConversionMethod)
        {
            this.caption = caption ?? "";
            this.toolTip = toolTip ?? "";
            this.Direction = direction;
            this.Mandatory = mandatory;
            this.QuickWatchFormat = quickWatchFormat;
            this.QuickWatchConversionMethod = quickWatchConversionMethod;
        }

        [Obsolete("descriptionUrl and hasDefaultValue are never used")]
        public PropertyInfoAttribute(Direction direction, string caption, string toolTip, string descriptionUrl, bool mandatory, bool hasDefaultValue, QuickWatchFormat quickWatchFormat, string quickWatchConversionMethod)
            : this(direction, caption, toolTip, mandatory, quickWatchFormat, quickWatchConversionMethod)
        {
        }

        public PropertyInfoAttribute(Direction direction, string caption, string toolTip)
            : this(direction, caption, toolTip, false, QuickWatchFormat.None, null)
        {
        }

        [Obsolete("descriptionUrl is never used")]
        public PropertyInfoAttribute(Direction direction, string caption, string toolTip, string descriptionUrl)
            : this(direction, caption, toolTip, false, QuickWatchFormat.None, null)
        {
        }

        #endregion constructor
    }
  }
