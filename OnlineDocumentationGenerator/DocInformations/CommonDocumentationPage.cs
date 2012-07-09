using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Cryptool.PluginBase;
using OnlineDocumentationGenerator.DocInformations.Localization;
using OnlineDocumentationGenerator.DocInformations.Utils;

namespace OnlineDocumentationGenerator.DocInformations
{
    public class CommonDocumentationPage : EntityDocumentationPage
    {
        private readonly XElement _xml;

        public override string Name
        {
            get { return Localizations["en"].Name; }
        }

        public override string DocDirPath
        {
            get { return DocGenerator.CommonDirectory; }
        }

        public CommonDocumentationPage(XElement xml)
        {
            _xml = xml;
            Localizations = new Dictionary<string, LocalizedEntityDocumentationPage>();

            if (_xml != null && _xml.Name == "documentation")
            {
                foreach (var lang in XMLHelper.GetAvailableLanguagesFromXML(_xml.Elements("language").Select(langElement => langElement.Attribute("culture").Value)))
                {
                    Localizations.Add(lang, new LocalizedCommonDocumentationPage(this, _xml, lang));
                }

                if (!Localizations.ContainsKey("en"))
                {
                    throw new Exception("Documentation should at least support english language!");
                }

                References = XMLHelper.ReadReferences(_xml);
            }
            else
            {
                throw new Exception("Error while trying to read common documentation page.");
            }
        }
    }
}