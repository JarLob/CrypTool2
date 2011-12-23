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
using System.Reflection;
using System.Windows;

namespace Cryptool.PluginBase
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]    
    public class TaskPaneAttribute : Attribute
    {
        # region multi language properties        
        private readonly string caption;
        public string Caption
        {          
          get
          {
            if (IsMultiLanguage && caption != null)
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
            if (IsMultiLanguage && toolTip != null)
              return PluginType.GetPluginStringResource(toolTip);
            else
              return toolTip;
          }
        }

        public readonly string groupName;
        public string GroupName
        {
          get
          {
            if (IsMultiLanguage && HasGroupName)
              return PluginType.GetPluginStringResource(groupName);
            else
              return groupName;
          }
        }

        public bool HasGroupName
        {
          get { return !string.IsNullOrEmpty(groupName); }
        }
        # endregion multi language properties

        public readonly int Order;
        public readonly ControlType ControlType;
        
        public readonly string[] controlValues;
        private string[] translatedControlValues;
        public string[] ControlValues
        {
            get
            {
                if (controlValues == null || !IsMultiLanguage)
                    return controlValues;

                if (translatedControlValues != null)
                    return translatedControlValues;

                translatedControlValues = new string[controlValues.Length];
                for (int i = 0; i < controlValues.Length; i++)
                {
                    translatedControlValues[i] = (controlValues[i] != null)
                        ? PluginType.GetPluginStringResource(controlValues[i])
                        : null;
                }

                return this.translatedControlValues;
            }
        }

        private string fileExtension;
        public string FileExtension
        {
            get { return fileExtension; }
            set
            {
                if (fileExtension == null)
                    fileExtension = value;
                else
                    throw new ArgumentException("This setter should only be accessed once.");
            }
        }
              
        public readonly ValidationType ValidationType;
        public readonly string RegularExpression;
        public readonly int IntegerMinValue;
        public readonly int IntegerMaxValue;
        public readonly double DoubleMinValue;
        public readonly double DoubleMaxValue;

        public bool ChangeableWhileExecuting;

        private MethodInfo method;
        public MethodInfo Method
        {
          get { return method; }
          set 
          {
            if (method == null)
              method = value; 
            else
              throw new ArgumentException("This setter should only be accessed once.");
          }
        }

        private string propertyName;
        public string PropertyName
        {
          get { return propertyName; }
          set 
          { 
            // This value should be readonly but for user convenience we set it in extension method. 
            // This setter should only be accessed once.
            if (propertyName == null)
              propertyName = value;
            else
              throw new ArgumentException("This setter should only be accessed once.");
          }
        }

        # region translation helpers

        /// <summary>
        /// Gets or sets the type of the plugin. This value is set by extension method if ResourceFile exists. 
        /// It is used to access the plugins resources to translate the text elements.
        /// </summary>
        /// <value>The type of the plugin.</value>
        public Type PluginType { get; set; }

        private bool IsMultiLanguage
        {
          get { return PluginType != null; }
        }
        # endregion translation helpers

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskPaneAttribute"/> class.
        /// </summary>
        /// <param name="caption">General name</param>
        /// <param name="toolTip">The tool tip.</param>
        /// <param name="order">The order.</param>        
        /// <param name="controlType">Type of the control.</param>
        /// <param name="controlValues">The control values used to display in ComboBox.</param>              
        /// helpAnchor
        public TaskPaneAttribute(string caption, string toolTip, string groupName, int order, bool changeableWhileExecuting, ControlType controlType, params string[] controlValues)
        {
            this.caption = caption;
            this.toolTip = toolTip;
            this.groupName = groupName;
            this.Order = order;              
            this.ControlType = controlType;            
            this.controlValues = controlValues;
            this.ChangeableWhileExecuting = changeableWhileExecuting;
        }

        /// <summary>
        /// Regex validation and ControlType.TextBox
        /// </summary>
        public TaskPaneAttribute(string caption, string toolTip, string groupName, int order, bool changeableWhileExecuting, ControlType controlType, ValidationType validationType, string regularExpression)
        {
          this.caption = caption;
          this.toolTip = toolTip;
          this.groupName = groupName;
          this.Order = order;
          this.ControlType = controlType;          
          this.ValidationType = validationType;
          this.RegularExpression = regularExpression;
          this.ChangeableWhileExecuting = changeableWhileExecuting;
        }
        
        /// <summary>
        /// NumericUpDown int.
        /// </summary>
        public TaskPaneAttribute(string caption, string toolTip, string groupName, int order, bool changeableWhileExecuting, ControlType controlType, ValidationType validationType, int integerMinValue, int integerMaxValue)
        {
          this.caption = caption;
          this.toolTip = toolTip;
          this.groupName = groupName;
          this.Order = order;
          this.ControlType = controlType;
          this.ValidationType = validationType;
          this.IntegerMinValue = integerMinValue;
          this.IntegerMaxValue = integerMaxValue;
          this.ChangeableWhileExecuting = changeableWhileExecuting;
        }

        /// <summary>
        /// NumericUpDown double.
        /// </summary>
        public TaskPaneAttribute(string caption, string toolTip, string groupName, int order, bool changeableWhileExecuting,  ControlType controlType, ValidationType validationType, double doubleMinValue, double doubleMaxValue)
        {
          this.caption = caption;
          this.toolTip = toolTip;
          this.groupName = groupName;
          this.Order = order;
          this.ControlType = controlType;
          this.ValidationType = validationType;
          this.DoubleMinValue = doubleMinValue;
          this.DoubleMaxValue = doubleMaxValue;
          this.ChangeableWhileExecuting = changeableWhileExecuting;
        }

        /// <summary>
        /// Slider
        /// </summary>
        public TaskPaneAttribute(string caption, string toolTip, string groupName, int order, bool changeableWhileExecuting, ControlType controlType, double doubleMinValue, double doubleMaxValue)
        {
          this.caption = caption;
          this.toolTip = toolTip;
          this.groupName = groupName;
          this.Order = order;
          this.ControlType = controlType;          
          this.DoubleMinValue = doubleMinValue;
          this.DoubleMaxValue = doubleMaxValue;
          this.ChangeableWhileExecuting = changeableWhileExecuting;
        }

        /// <summary>
        /// This constructor is used to mark methods in combination with ControlType.Button and Textblock
        /// </summary>
        /// <param name="caption">The caption.</param>
        /// <param name="toolTip">The tool tip.</param>
        /// <param name="order">The order.</param>
        /// <param name="controlType">Type of the control should be button in this construcor.</param>
        public TaskPaneAttribute(string caption, string toolTip, string groupName, int order, bool changeableWhileExecuting, ControlType controlType)
        {
          this.caption = caption;
          this.toolTip = toolTip;
          this.groupName = groupName;
          this.Order = order;
          this.ControlType = controlType;
          this.ChangeableWhileExecuting = changeableWhileExecuting;
        }

        public override string ToString()
        {
            return string.Format("TaskPaneAttribute[caption={0}]", caption);
        }
    }
}
