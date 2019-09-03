﻿using Cryptool.PluginBase;
using System;
using System.Collections.Generic;

namespace WorkspaceManager.View.Base
{
    /// <summary>
    /// Holds a plugin instance and keeps track of all task pane attribute changes for that plugin.
    /// </summary>
    public class PluginSettingsContainer
    {
        private readonly Dictionary<string, TaskPaneAttribteContainer> currentTaskPaneAttributes;

        public IEnumerable<TaskPaneAttribteContainer> CurrentTaskPaneAttributes => currentTaskPaneAttributes.Values;

        public IPlugin Plugin { get; }

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public PluginSettingsContainer(IPlugin plugin)
        {
            Plugin = plugin;
            currentTaskPaneAttributes = new Dictionary<string, TaskPaneAttribteContainer>();

            var taskPaneAttributeChanged = plugin.Settings?.GetTaskPaneAttributeChanged();
            if (taskPaneAttributeChanged != null)
            {
                taskPaneAttributeChanged.AddEventHandler(plugin.Settings, 
                    new TaskPaneAttributeChangedHandler(HandleTaskPaneAttributeChange));
            }
        }

        private void HandleTaskPaneAttributeChange(ISettings settings, TaskPaneAttributeChangedEventArgs args)
        {
            foreach (TaskPaneAttribteContainer tpac in args.ListTaskPaneAttributeContainer)
            {
                currentTaskPaneAttributes[tpac.Property] = tpac;
            }

            TaskPaneAttributeChanged?.Invoke(settings, args);
        }
    }
}
