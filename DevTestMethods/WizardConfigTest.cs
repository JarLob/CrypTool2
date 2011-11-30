﻿using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Cryptool.PluginBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wizard;
using WorkspaceManager.Model;
using ValidationType = System.Xml.ValidationType;

namespace Tests
{
    [TestClass]
    public class WizardConfigTest
    {
        private DirectoryInfo _templateDirectory;
        private const string configXMLPath = "Wizard.Config.wizard.config.start.xml";

        [TestMethod]
        public void TemplateConsistencyTest()
        {
            XElement xml = GenerateXML(GetXml(configXMLPath));

            var dir = Directory.GetParent(System.Environment.CurrentDirectory);
            while (dir != null && dir.GetDirectories("Templates").Length == 0)
            {
                dir = dir.Parent;
            }
            if (dir == null)
            {
                Assert.Fail("Template directory not found!");
            }
            _templateDirectory = dir.GetDirectories("Templates")[0];

            foreach (var template in GetTemplatesFromXML(xml, new Dictionary<string, List<KeyValuePair<string, object>>>()))
            {
                LoadAndCheckTemplate(template.Key, template.Value);
            }
        }

        private void LoadAndCheckTemplate(string file, Dictionary<string, List<KeyValuePair<string, object>>> pluginProperties)
        {
            var model = ModelPersistance.loadModel(Path.Combine(_templateDirectory.FullName, file));
            model.OnGuiLogNotificationOccured += delegate(IPlugin sender, GuiLogEventArgs args)
            {
                if (args.NotificationLevel == NotificationLevel.Error)
                {
                    Assert.Fail(string.Format("Error guilogmessage in template {0}!", file));
                }
            };

            /*
            foreach (PluginModel pluginModel in model.GetAllPluginModels())
            {
                pluginModel.Plugin.Initialize();
            }
             */

            //iterate over all plugins which properties should be set and check them:
            foreach (var pluginSetting in pluginProperties)
            {
                KeyValuePair<string, List<KeyValuePair<string, object>>> setting = pluginSetting;
                int count = 0;
                foreach (var plugin in model.GetAllPluginModels().Where(x => x.GetName() == setting.Key))
                {
                    count++;
                    
                    foreach (var property in pluginSetting.Value)
                    {
                        if (!WizardControl.SetPluginProperty(new PluginPropertyValue() {PluginName = setting.Key, PropertyName = property.Key, Value = property.Value}, plugin))
                        {
                            Assert.Fail(string.Format("Property {0} in plugin {1} does not exist in template {2}!", property.Key, pluginSetting.Key, file));
                        }
                    }
                }

                if (count == 0)
                {
                    Assert.Fail(string.Format("Plugin with name {0} does not exist in template {1}!", setting.Key, file));
                }
            }
        }

        private IEnumerable<KeyValuePair<string, Dictionary<string, List<KeyValuePair<string, object>>>>> GetTemplatesFromXML(XElement xml, Dictionary<string, List<KeyValuePair<string, object>>> pluginProperties)
        {
            Dictionary<string, List<KeyValuePair<string, object>>> p;
            switch (xml.Name.ToString())
            {
                case "sampleViewer":
                    if (xml.Attribute("file") != null)
                    {
                        p = GetElementspluginProperties(xml, pluginProperties);
                        yield return new KeyValuePair<string, Dictionary<string, List<KeyValuePair<string, object>>>>(xml.Attribute("file").Value, p);
                    }
                    break;
                case "loadSample":
                    if (xml.Attribute("file") != null)
                    {
                        yield return new KeyValuePair<string, Dictionary<string, List<KeyValuePair<string, object>>>>(xml.Attribute("file").Value, pluginProperties);
                    }
                    break;
                case "input":
                    p = GetElementspluginProperties(xml, pluginProperties);
                    foreach (var sub in GetSubEntries(xml))
                    {
                        foreach (var el in GetTemplatesFromXML(sub, p))
                        {
                            yield return el;
                        }
                    }
                    break;
                case "category":
                    foreach (var sub in GetSubEntries(xml))
                    {
                        foreach (var el in GetTemplatesFromXML(sub, pluginProperties))
                        {
                            yield return el;
                        }
                    }
                    break;
            }
        }

        private IEnumerable<XElement> GetSubEntries(XElement xml)
        {
            foreach (var el in xml.Elements())
            {
                switch (el.Name.ToString())
                {
                    case "category":
                    case "input":
                    case "sampleViewer":
                    case "loadSample":
                    case "file":
                        yield return el;
                        break;
                }
            }
        }

        private Dictionary<string, List<KeyValuePair<string, object>>> GetElementspluginProperties(XElement xml, Dictionary<string, List<KeyValuePair<string, object>>> pluginProperties)
        {
            var result = new Dictionary<string, List<KeyValuePair<string, object>>>(pluginProperties);
            foreach (var el in xml.Elements())
            {
                object val = null;
                switch (el.Name.ToString())
                {
                    case "inputBox":
                        val = "";
                        break;
                    case "comboBox":
                    case "checkBox":
                    case "outputBox":
                        val = 0;
                        break;
                }

                switch (el.Name.ToString())
                {
                    case "inputBox":
                    case "comboBox":
                    case "checkBox":
                    case "outputBox":
                        if (el.Attribute("plugin") != null && el.Attribute("property") != null)
                        {
                            var plugin = el.Attribute("plugin").Value;
                            var prop = new KeyValuePair<string, object>(el.Attribute("property").Value, val);
                            if (result.ContainsKey(plugin))
                            {
                                if (!result[plugin].Contains(prop))
                                    result[plugin].Add(prop);
                            }
                            else
                            {
                                result.Add(plugin, new List<KeyValuePair<string, object>>() { prop });
                            }
                        }
                        break;

                    case "presentation":
                        if (el.Attribute("plugin") != null)
                        {
                            var plugin = el.Attribute("plugin").Value;
                            if (!result.ContainsKey(plugin))
                            {
                                result.Add(plugin, new List<KeyValuePair<string, object>>());
                            }
                        }
                        break;
                }
            }
            return result;
        }

        private static XElement GetXml(string file)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse;
            settings.ValidationType = ValidationType.DTD;
            settings.ValidationEventHandler +=
                delegate(object sender, ValidationEventArgs e) { Assert.Fail("DTD check failed!"); };
            settings.XmlResolver = new WizardControl.ResourceDTDResolver();

            return WizardControl.LoadXMLFromAssembly(file, settings);
        }

        // generate the full XML tree for the wizard (recursive)
        private XElement GenerateXML(XElement xml)
        {
            try
            {
                IEnumerable<XElement> allFiles = xml.Elements("file");
                foreach (var ele in allFiles)
                {
                    XAttribute att = ele.Attribute("resource");
                    if (att != null)
                    {
                        string path = att.Value;
                        XElement sub = GetXml(path);
                        ele.AddAfterSelf(sub);
                    }
                }

                IEnumerable<XElement> allElements = xml.Elements();
                if (allElements.Any())
                {
                    foreach (XElement ele in allElements)
                    {
                        if (ele.Name != "file")
                        {
                            GenerateXML(ele);
                        }
                    }
                }

                return xml;
            }
            catch (Exception ex)
            {
                Assert.Fail("Error trying to generate XML!");
            }
            return null;
        }
    }
}
