using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using OnlineDocumentationGenerator.DocInformations.Localization;
using OnlineDocumentationGenerator.DocInformations.Utils;

namespace OnlineDocumentationGenerator.DocInformations
{
    public class TemplateDocumentationPage : EntityDocumentationPage
    {
        private readonly string _relativeTemplateDirectory;

        public string RelativeTemplateDirectory
        {
            get { return _relativeTemplateDirectory; }
        }

        public string TemplateFile { get; private set; }
        public XElement TemplateXML { get; private set; }
        public List<string> RelevantPlugins { get; private set; }

        public override string Name
        {
            get { return Path.GetFileNameWithoutExtension(Localizations["en"].FilePath); }
        }

        public override string DocDirPath
        {
            get { return DocGenerator.TemplateDirectory; }
        }

        public new LocalizedTemplateDocumentationPage CurrentLocalization
        {
            get { return (LocalizedTemplateDocumentationPage) base.CurrentLocalization; }
        }

        public TemplateDocumentationPage(string templateFile, string relativeTemplateDirectory)
        {
            _relativeTemplateDirectory = relativeTemplateDirectory;
            TemplateFile = templateFile;
            
            string templateXMLFile = Path.Combine(Path.GetDirectoryName(templateFile), Path.GetFileNameWithoutExtension(templateFile) + ".xml");
            if (!File.Exists(templateXMLFile))
                throw new Exception(string.Format("Missing meta infos for template {0}!", templateFile));

            TemplateXML = XElement.Load(templateXMLFile);

            BitmapFrame icon = null;
            if (TemplateXML.Element("icon") != null && TemplateXML.Element("icon").Attribute("file") != null)
            {
                var iconFile = Path.Combine(Path.GetDirectoryName(templateFile), TemplateXML.Element("icon").Attribute("file").Value);
                if (iconFile == null || !File.Exists(iconFile))
                {
                    iconFile = Path.Combine(Path.GetDirectoryName(templateFile), Path.GetFileNameWithoutExtension(templateFile) + ".png");
                }
                if (File.Exists(iconFile))
                {
                    try
                    {
                        icon = BitmapFrame.Create(new BitmapImage(new Uri(iconFile)));
                    }
                    catch (Exception)
                    {
                        icon = null;
                    }
                }
            }

            string author = null;
            var authorElement = XMLHelper.FindLocalizedChildElement(TemplateXML, "author");
            if (authorElement != null)
            {
                author = authorElement.Value;
            }

            var relevantPlugins = TemplateXML.Element("relevantPlugins");
            if (relevantPlugins != null)
            {
                RelevantPlugins = new List<string>();
                foreach (var plugin in relevantPlugins.Elements("plugin"))
                {
                    var name = plugin.Attribute("name");
                    if (name != null)
                    {
                        RelevantPlugins.Add(name.Value);
                    }
                }
            }
                
            foreach (var title in TemplateXML.Elements("title"))
            {
                var langAtt = title.Attribute("lang");
                if (langAtt != null && !AvailableLanguages.Contains(langAtt.Value))
                {
                    Localizations.Add(langAtt.Value, new LocalizedTemplateDocumentationPage(this, langAtt.Value, icon, author));
                }
            }
            if (!Localizations.ContainsKey("en"))
            {
                throw new Exception("Documentation should at least support english language!");
            }
        }
    }
}