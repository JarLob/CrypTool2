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
using Cryptool.PluginBase.Editor;
using System.Collections.Generic;
using System.Windows;

namespace Cryptool.PluginBase
{

  public class StatusEventArgs : EventArgs
  {
    public readonly int ImageIndex;
    public readonly StatusChangedMode StatusChangedMode;

    public StatusEventArgs(StatusChangedMode statusChangedMode, int imageIndex)
    {
      this.ImageIndex = imageIndex;
      this.StatusChangedMode = statusChangedMode;
    }

    public StatusEventArgs(StatusChangedMode statusChangedMode)
    {
      if (statusChangedMode == StatusChangedMode.ImageUpdate)
      {
        throw new ArgumentException("statusChangedMode");
      }
      this.StatusChangedMode = statusChangedMode;
    }
  }
  
  public class GuiLogEventArgs : EventArgs
  {
    private string title = string.Empty;
    public readonly DateTime DateTime;
    public readonly string Message;
    public readonly IPlugin Plugin;
    public readonly NotificationLevel NotificationLevel;

    /// <summary>
    /// Title needs to be writeable, because the editor will add the current title
    /// before the event is given to the gui.
    /// </summary>
    public string Title 
    {
      get { return title; }
      set { title = value; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GuiLogEventArgs"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="plugin">The plugin.</param>
    /// <param name="notificationLevel">The notification level.</param>    
    public GuiLogEventArgs(string message, IPlugin plugin, NotificationLevel notificationLevel)
    {
      this.DateTime = DateTime.Now;
      this.Message = message;
      this.Plugin = plugin;
      this.NotificationLevel = notificationLevel;      
    }
  }

  public class PluginProgressEventArgs : EventArgs
  {
    public readonly double Value;
    public readonly double Max;

    public PluginProgressEventArgs(double value, double max)
    {
      this.Value = value;
      this.Max = max;
    }
  }

  public class PluginChangedEventArgs : EventArgs
  {
    public readonly IPlugin SelectedPlugin;
    public readonly string Title;
    public readonly DisplayPluginMode DisplayPluginMode;

    public PluginChangedEventArgs(IPlugin selectedPlugin, string title, DisplayPluginMode mode)
    {
      this.SelectedPlugin = selectedPlugin;
      this.Title = title;
      this.DisplayPluginMode = mode;
    }
  }

  public class ProjectTitleChangedEventArgs : EventArgs
  {
    public readonly string Title;

    public ProjectTitleChangedEventArgs(string title)
    {
      this.Title = title;      
    }
  }

  public class TaskPaneAttributeChangedEventArgs : EventArgs
  {
    public readonly List<TaskPaneAttribteContainer> ListTaskPaneAttributeContainer;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskPaneAttributeChangedEventArgs"/> class.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="visible">The visible.</param>
    public TaskPaneAttributeChangedEventArgs(TaskPaneAttribteContainer tpac)
    {
      if (tpac == null)
        throw new ArgumentException("tpac is null");
      ListTaskPaneAttributeContainer = new List<TaskPaneAttribteContainer>();
      ListTaskPaneAttributeContainer.Add(tpac);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskPaneAttributeChangedEventArgs"/> class.
    /// </summary>
    /// <param name="properties">The property list</param>
    public TaskPaneAttributeChangedEventArgs(List<TaskPaneAttribteContainer> listTaskPaneAttributeContainer)
    {
      if (listTaskPaneAttributeContainer == null || listTaskPaneAttributeContainer.Count == 0)
        throw new ArgumentException("listTaskPaneAttributeContainer is null or empty");
      this.ListTaskPaneAttributeContainer = listTaskPaneAttributeContainer;
    }
  }

  public class TaskPaneAttribteContainer
  {
    public readonly string Property;
    public readonly Visibility Visibility;
    public readonly TaskPaneAttribute TaskPaneAttribute;

    public TaskPaneAttribteContainer(string property, Visibility visibility)
      : this(property, visibility, null)
    {
    }

    public TaskPaneAttribteContainer(string property, Visibility visibility, TaskPaneAttribute taskPaneAttribute)
    {
      if (property == null || property == string.Empty)
        throw new ArgumentException("property is null or empty");
      this.Property = property;
      this.Visibility = visibility;
      this.TaskPaneAttribute = taskPaneAttribute;
    }
  }

}
