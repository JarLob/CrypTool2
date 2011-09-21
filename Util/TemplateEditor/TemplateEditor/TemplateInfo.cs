﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace TemplateEditor
{
    public class TemplateInfo
    {
        public string FilePath { get; set; }
        public string XMLPath { get; set; }
        public string IconFile
        {
            get { return _iconFile; }
            set
            {
                _iconFile = value;
                var iconFullPath = Path.Combine(Path.GetDirectoryName(XMLPath), IconFile);
                if (File.Exists(iconFullPath))
                {
                    Icon = new BitmapImage(new Uri(iconFullPath));
                }
                else
                {
                    Icon = null;
                }
            }
        }
        public BitmapImage Icon { get; set; }
        public List<string> RelevantPlugins { get; set; }
        public Dictionary<string, LocalizedTemplateData> LocalizedTemplateData = new Dictionary<string, LocalizedTemplateData>();

        private string _iconFile;

        //Helper properties for the overview:
        
        public string Title
        {
            get
            {
                if (LocalizedTemplateData.ContainsKey("en"))
                    return LocalizedTemplateData["en"].Title;
                else
                    return null;
            }
        }

        public bool HasMetadata { get; private set; }

        public string AvailableTranslations
        {
            get
            {
                var res = "";
                if (LocalizedTemplateData == null)
                    return res;

                foreach (var lang in LocalizedTemplateData.Keys)
                {
                    res += lang + ", ";
                }

                if (res.Length > 0)
                {
                    return res.Substring(0, res.Length - 2);
                }
                else
                {
                    return "";
                }
            }
        }

        public string AllKeywords
        {
            get
            {
                var res = "";
                if (LocalizedTemplateData == null)
                    return res;

                foreach (var localizedTemplateData in LocalizedTemplateData)
                {
                    if (localizedTemplateData.Value.Keywords != null)
                    {
                        foreach (var kw in localizedTemplateData.Value.Keywords)
                        {
                            res += kw + ", ";
                        }
                    }
                }
                if (res.Length > 0)
                {
                    return res.Substring(0, res.Length - 2);
                }
                else
                {
                    return "";
                }
            }
        }

        public string AllRelevantPlugins
        {
            get 
            { 
                var res = "";
                if (RelevantPlugins == null)
                    return res;

                foreach (var relevantPlugin in RelevantPlugins)
                {
                    res += relevantPlugin + ", ";
                }

                if (res.Length > 0)
                {
                    return res.Substring(0, res.Length - 2);
                }
                else
                {
                    return "";
                }
            }
        }


        public TemplateInfo(string templateDir, string templateFilePath)
        {
            FilePath = templateFilePath;
            var cwmFile = Path.Combine(templateDir, templateFilePath);
            if ((Path.GetExtension(cwmFile).ToLower() != ".cwm") && (Path.GetExtension(cwmFile).ToLower() != ".component"))
            {
                throw new Exception(string.Format("{0} not a valid template!", templateFilePath));
            }
            if (!File.Exists(cwmFile))
            {
                throw new Exception(string.Format("Template {0} does not exist!", cwmFile));
            }

            //var xmlFile = cwmFile.Substring(0, cwmFile.Length - 3) + "xml";
            var xmlFile = Path.Combine(Path.GetDirectoryName(cwmFile), Path.GetFileNameWithoutExtension(cwmFile)) + ".xml";
            XMLPath = xmlFile;
            HasMetadata = false;
            if (File.Exists(xmlFile))
            {
                HasMetadata = true;
                XElement xml = XElement.Load(xmlFile);

                foreach (var element in xml.Elements())
                {
                    var lang = "en";
                    if (element.Attribute("lang") != null)
                    {
                        lang = element.Attribute("lang").Value;
                    }

                    switch (element.Name.ToString())
                    {
                        case "title":
                            CreateTemplateDataEntryForLang(lang);
                            LocalizedTemplateData[lang].Title = element.Value;
                            break;
                        case "description":
                            CreateTemplateDataEntryForLang(lang);
                            var desc = element.ToString(SaveOptions.None);
                            desc = desc.Substring(desc.IndexOf('>')+1);
                            desc = desc.Substring(0, desc.LastIndexOf('<'));
                            LocalizedTemplateData[lang].Description = desc;
                            break;
                        case "icon":
                            if (element.Attribute("file") != null)
                            {
                                IconFile = element.Attribute("file").Value;
                            }
                            break;
                        case "relevantPlugins":
                            foreach (var plugin in element.Elements("plugin"))
                            {
                                if (plugin.Attribute("name") != null)
                                {
                                    if (RelevantPlugins == null)
                                    {
                                        RelevantPlugins = new List<string>();
                                    }
                                    RelevantPlugins.Add(plugin.Attribute("name").Value.ToString());
                                }
                            }
                            break;
                        case "keywords":
                            CreateTemplateDataEntryForLang(lang);
                            var keywords = element.Value.ToString().Split(',');
                            if (LocalizedTemplateData[lang].Keywords == null)
                            {
                                LocalizedTemplateData[lang].Keywords = new List<string>();
                            }
                            foreach (var keyword in keywords)
                            {
                                LocalizedTemplateData[lang].Keywords.Add(keyword.Trim());
                            }
                            break;
                    }
                }
            }
        }

        private void CreateTemplateDataEntryForLang(string lang)
        {
            if (!LocalizedTemplateData.ContainsKey(lang))
            {
                LocalizedTemplateData.Add(lang, new LocalizedTemplateData());
                LocalizedTemplateData[lang].Lang = lang;
            }
        }

        public void Save()
        {
            XElement xml = new XElement("sample");
            foreach (var data in LocalizedTemplateData.Values)
            {
                if (data.Lang != null && data.Title != null)
                {
                    var title = new XElement("title");
                    title.SetAttributeValue("lang", data.Lang);
                    title.SetValue(data.Title);
                    xml.Add(title);
                }

                if (data.Description != null)
                {
                    xml.Add(XElement.Parse(string.Format("<description lang=\"{0}\">{1}</description>", data.Lang, data.Description)));
                }

                if ((data.Keywords != null) && (data.Keywords.Count() > 0))
                {
                    var keywords = new XElement("keywords");
                    if (data.Lang != "en")
                    {
                        keywords.SetAttributeValue("lang", data.Lang);
                    }
                    keywords.SetValue(data.AllKeywords);
                    xml.Add(keywords);
                }
            }

            if (IconFile != null)
            {
                var icon = new XElement("icon");
                icon.SetAttributeValue("file", IconFile);
                xml.Add(icon);
            }

            if ((RelevantPlugins != null) && (RelevantPlugins.Count > 0))
            {
                var relevantPlugins = new XElement("relevantPlugins");
                foreach (var relevantPlugin in RelevantPlugins)
                {
                    var plugin = new XElement("plugin");
                    plugin.SetAttributeValue("name", relevantPlugin);
                    relevantPlugins.Add(plugin);
                }
                xml.Add(relevantPlugins);
            }

            xml.Save(XMLPath);
            HasMetadata = true;
        }
    }
}
