﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Linq;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Media.Imaging;
using System.Windows.Controls;

namespace Cryptool.PluginBase
{
    public class ImageHelper
    {
        public static ImageSource LoadImage(Uri file)
        {
            Image i = new Image();
            i.Source = new BitmapImage(file);
            return i.Source;
        }
    }


    [Serializable()]
    [Cryptool.PluginBase.Attributes.Localization("Cryptool.PluginBase.Properties.Resources")]
    public class TabInfo : IDeserializationCallback
    {
        [NonSerialized]
        private Span tooltip;
        public Span Tooltip { get { return tooltip; }  set { tooltip = value; } }

        [NonSerialized]
        private ImageSource icon;
        public ImageSource Icon { get { return icon; } set { icon = value; } }

        private string title;
        public string Title { get { return title; } set { title = value; } }

        private FileInfo filename;
        public FileInfo Filename
        {
            get { return filename; }
            set
            {
                filename = value;
                var info = GenerateTabInfo(value);
                this.Title = info.Title;
                this.Icon = info.Icon;
                this.Tooltip = info.Tooltip;
            }
        }

        public TabInfo GenerateTabInfo(FileInfo file)
        {
            bool component = (file.Extension.ToLower() == ".component");
            string title = null;
            Span summary = new Span();
            string iconFile = null;
            string xmlFile = Path.Combine(file.Directory.FullName, Path.GetFileNameWithoutExtension(file.Name) + ".xml");
            if (File.Exists(xmlFile))
            {
                try
                {
                    XElement xml = XElement.Load(xmlFile);
                    var titleElement = XMLHelper.GetGlobalizedElementFromXML(xml, "title");
                    if (titleElement != null)
                        title = titleElement.Value;

                    var summaryElement = XMLHelper.GetGlobalizedElementFromXML(xml, "summary");
                    var descriptionElement = XMLHelper.GetGlobalizedElementFromXML(xml, "description");
                    if (summaryElement != null)
                    {
                        summary.Inlines.Add(new Bold(XMLHelper.ConvertFormattedXElement(summaryElement)));
                    }
                    if (descriptionElement != null && descriptionElement.Value.Length > 1)
                    {
                        summary.Inlines.Add(new LineBreak());
                        summary.Inlines.Add(new LineBreak());
                        summary.Inlines.Add(XMLHelper.ConvertFormattedXElement(descriptionElement));
                    }

                    if (xml.Element("icon") != null && xml.Element("icon").Attribute("file") != null)
                    {
                        iconFile = Path.Combine(file.Directory.FullName, xml.Element("icon").Attribute("file").Value);
                    }
                }
                catch (Exception)
                {
                    //we do nothing if the loading of an description xml fails => this is not a hard error
                }
            }

            if ((title == null) || (title.Trim() == ""))
            {
                title = component ? file.Name : Path.GetFileNameWithoutExtension(file.Name).Replace("-", " ").Replace("_", " ");
            }

            if (summary.Inlines.Count == 0)
            {
                string desc = component ? Properties.Resources.This_is_a_standalone_component_ : Properties.Resources.This_is_a_WorkspaceManager_file_;
                summary.Inlines.Add(new Run(desc));
            }

            if (iconFile == null || !File.Exists(iconFile))
            {
                iconFile = Path.Combine(file.Directory.FullName, Path.GetFileNameWithoutExtension(file.Name) + ".png");
            }
            ImageSource image = null;
            if (File.Exists(iconFile))
            {
                try
                {
                    image = ImageHelper.LoadImage(new Uri(iconFile));
                }
                catch (Exception)
                {
                    image = null;
                }
            }
            else
            {
                var ext = file.Extension.Remove(0, 1);
                if (!component && ComponentInformations.EditorExtension.ContainsKey(ext))
                {
                    Type editorType = ComponentInformations.EditorExtension[ext];
                    image = editorType.GetImage(0).Source;
                }
            }
            return new TabInfo() { Tooltip = summary, Title = title, Icon = image, filename = file };
        }

        public void OnDeserialization(object sender)
        {
            if (filename == null)
                return;
            var info = GenerateTabInfo(filename);
            this.Icon = info.Icon;
            this.Title = info.Title;
            this.Tooltip = Tooltip;
        }
    }
}
