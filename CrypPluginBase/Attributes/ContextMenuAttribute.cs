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
    [AttributeUsage(AttributeTargets.Property)]
    public class ContextMenuAttribute : Attribute
    {
        private readonly string caption;
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

        private readonly string toolTip;
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
        
        public readonly int Order;
        public readonly ContextMenuControlType ControlType;        
        public readonly string[] ControlValues;
        public readonly int[] ArrImagesForControlValues;

        # region translation helpers
        private Type pluginType;

        /// <summary>
        /// Gets or sets the type of the plugin. This value is set by extension method if ResourceFile exists. 
        /// It is used to access the plugins resources to translate the text elements.
        /// </summary>
        /// <value>The type of the plugin.</value>
        public Type PluginType
        {
            get { return pluginType; }
            set { pluginType = value; }
        }

        private bool MultiLanguage
        {
            get { return PluginType != null; }
        }
        # endregion translation helpers

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextMenuAttribute"/> class.
        /// </summary>
        /// <param name="caption">The caption is not used right now and may be removed in future versions.</param>
        /// <param name="toolTip">The tool tip is used for all entries.</param>
        /// <param name="order">The order.</param>
        /// <param name="controlType">Type of the control.</param>
        /// <param name="arrImagesForControlValues">Image indexes for the control values. Can be null if no images should be used.</param>
        /// <param name="controlValues">Strings for the context menu entries.</param>
        public ContextMenuAttribute(string caption, string toolTip, int order, ContextMenuControlType controlType, int[] arrImagesForControlValues, params string[] controlValues)
        {
            this.caption = caption;
            this.toolTip = toolTip;
            this.Order = order;            
            this.ControlType = controlType;
            this.ControlValues = controlValues;
            this.ArrImagesForControlValues = arrImagesForControlValues;
        }
    }
}
