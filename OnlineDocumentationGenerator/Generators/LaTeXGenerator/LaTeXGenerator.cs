﻿using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Attributes;
using Cryptool.PluginBase.Miscellaneous;
using OnlineDocumentationGenerator.DocInformations;
using OnlineDocumentationGenerator.DocInformations.Localization;

namespace OnlineDocumentationGenerator.Generators.LaTeXGenerator
{
    public class LaTeXGenerator : Generator
    {
        public static readonly string HelpDirectory = "LaTeXDoc";

        private ObjectConverter _objectConverter;
        private TemplateDirectory _templatesDir;
        private readonly bool _noIcons;
        private readonly bool _showAuthors;

        public LaTeXGenerator(bool noIcons, bool showAuthors)
        {
            _noIcons = noIcons;
            _showAuthors = showAuthors;
        }

        public override void Generate(TemplateDirectory templatesDir)
        {
            foreach (var lang in AvailableLanguages)
            {
                var cultureInfo = new CultureInfo(lang);
                Thread.CurrentThread.CurrentCulture = cultureInfo;
                Thread.CurrentThread.CurrentUICulture = cultureInfo;

                _templatesDir = templatesDir;
                _objectConverter = new ObjectConverter(DocPages, OutputDir);

                //
                // create LaTeX description of templates
                //
                var tableCode = GenerateTemplateOverviewTableCode(lang);
                var descriptionCode = GenerateTemplateDescriptionCode(lang);
                var versionString = GetVersion();

                // write template description file
                var latexCode = Properties.Resources.LaTeXTmpl_Templates.Replace("$CONTENT$", tableCode + "\n" + descriptionCode);
                latexCode = latexCode.Replace("$VERSION$", versionString);
                StoreLaTeX(latexCode, "templates-" + lang + ".tex");

                // write appendix for CT2 script
                latexCode = Properties.Resources.LaTeXTmpl_Appendix.Replace("$CONTENT$", tableCode);
                StoreLaTeX(latexCode, "appendix-" + lang + ".tex");

                //
                // create LaTeX description of components
                //
                var componentDoc = GenerateComponentIndexPages(lang);
                latexCode = Properties.Resources.LaTeXTmpl_Components.Replace("$CONTENT$", componentDoc);
                latexCode = latexCode.Replace("$VERSION$", versionString);
                StoreLaTeX(latexCode, "components-" + lang + ".tex");
            }
        }

        private string GenerateComponentIndexPages(string lang)
        {
            var components = DocPages.FindAll(x => x is ComponentDocumentationPage).Select(x => (ComponentDocumentationPage)x);
            var componentListCode = GenerateComponentTreeCode(components, lang);
            return componentListCode;
        }

        private static string GenerateComponentListCode(IEnumerable<ComponentDocumentationPage> componentDocumentationPages, string lang)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("<table width=\"100%\" border=\"0\" cellspacing=\"3\" cellpadding=\"3\" class=\"filterable\">");

            var anchorBuilder = new StringBuilder();
            anchorBuilder.Append("<p>");

            var query = from pages in componentDocumentationPages
                        orderby pages.Localizations[pages.Localizations.ContainsKey(lang) ? lang : "en"].Name
                        select pages;

            char actualIndexCharacter = ' ';

            foreach (var page in query)
            {
                var linkedLang = page.Localizations.ContainsKey(lang) ? lang : "en";
                var pp = (LocalizedComponentDocumentationPage)page.Localizations[linkedLang];

                if (actualIndexCharacter != pp.Name[0])
                {
                    //actualIndexCharacter = pp.Name.ToUpper()[0];
                    //stringBuilder.AppendLine(string.Format("<tr><td><h2 id=\"{0}\">{0}</h1></td><td></td></tr>", actualIndexCharacter));
                    //anchorBuilder.AppendLine(string.Format("<a href=\"#{0}\"><b>{0}</b><a>&nbsp;", actualIndexCharacter));
                    anchorBuilder.AppendLine(pp.Name);
                    if (pp.Introduction != null) anchorBuilder.AppendLine(pp.Introduction.Value);
                    //anchorBuilder.AppendLine(pp.Manual.Value);
                }
                //stringBuilder.AppendLine(string.Format("<tr><td><a href=\"{0}\">{1}</a></td><td>{2}</td></tr>",
                //    OnlineHelp.GetPluginDocFilename(pp.PluginType, linkedLang), pp.Name, pp.ToolTip));
            }

            //stringBuilder.AppendLine("</table>");
            //stringBuilder.AppendLine("<script type=\"text/javascript\" src=\"filterTable.js\"></script>");

            //anchorBuilder.Append("</p>");
            //anchorBuilder.Append(stringBuilder);
            return anchorBuilder.ToString();
        }

        private string GenerateComponentTreeCode(IEnumerable<PluginDocumentationPage> componentDocumentationPages, string lang)
        {
            var stringBuilder = new StringBuilder();

            var query = from pages in componentDocumentationPages
                        orderby pages.Category
                        select pages;

            ComponentCategory actualCategory = ComponentCategory.Undefined;

            foreach (var page in query)
            {
                var linkedLang = page.Localizations.ContainsKey(lang) ? lang : "en";
                var pp = (LocalizedComponentDocumentationPage)page.Localizations[linkedLang];

                if (actualCategory != page.Category)
                {
                    actualCategory = page.Category;
                    stringBuilder.AppendLine(@"\chapter{" + Helper.EscapeLaTeX( GetComponentCategory(page.Category) ) + "}");
                }

                stringBuilder.AppendLine(@"\section{" + Helper.EscapeLaTeX(pp.Name) + "}");
                stringBuilder.AppendLine("Tooltip:" + Helper.EscapeLaTeX(pp.ToolTip) + @"\\");

                StringBuilder text = new StringBuilder(); 
                text.AppendLine(@"\subsection*{" + Properties.Resources.ComponentTemplate_Introduction + "}\n" + _objectConverter.Convert(pp.Introduction, page));
                text.AppendLine(@"\subsection*{" + Properties.Resources.ComponentTemplate_Usage + "}\n" + _objectConverter.Convert(pp.Manual, page));
                text.AppendLine(@"\subsection*{" + Properties.Resources.ComponentTemplate_Presentation + "}\n" + _objectConverter.Convert(pp.Presentation, page));

                var connectors = pp.GetType().GetProperty("Connectors").GetValue(pp, null);
                text.AppendLine(@"\subsection*{" + Properties.Resources.ComponentTemplate_Connections + "}\n" + _objectConverter.Convert(connectors, page));

                var settings = pp.GetType().GetProperty("Settings").GetValue(pp, null);
                text.AppendLine(@"\subsection*{" + Properties.Resources.ComponentTemplate_Settings + "}\n" + _objectConverter.Convert(settings, page));

                //stringBuilder.AppendLine("</table>");
                //anchorBuilder.Append("</p>");
                string s = text.ToString();
                //s = Regex.Replace(s, @"\\\\[\r\n\t ]+\\\\", '\\' + "\n" + '\\');
                s = Regex.Replace(s, "\r\n", "\n");
                s = Regex.Replace(s, "\r", "\n");
                //s = Regex.Replace(s, "\n", " ");
                s = Regex.Replace(s, "\n{2,}", "\n\n");
                //s = Regex.Replace(s, "\\\\\\\\", "\n");
                s = Regex.Replace(s, "[\t ]+\n", "\n");
                //s = Regex.Replace(s, "\\\\\\\\", "\n");
                //s = Regex.Replace(s, @"\n[\t ]+", "\n");
                //s = Regex.Replace(s, "\n{2,}", "\n");
                //s = Regex.Replace(s, @"\n[\t ]+\\\\", "\n" + @"\\");
                //s = Regex.Replace(s, @"\n+\\\\", @" \\");

                stringBuilder.AppendLine(s);
            }

            return stringBuilder.ToString();
        }

        private string GenerateTemplateOverviewTableCode(string lang)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("\\chapter*{" + OnlineDocumentationGenerator.Properties.Resources.LatexGenerator_ChapterTitle + "}");
            stringBuilder.AppendLine("\\addcontentsline{toc}{chapter}{" + OnlineDocumentationGenerator.Properties.Resources.LatexGenerator_ChapterTitle + "}");
            stringBuilder.AppendLine("\\renewcommand{\\arraystretch}{2}"); 
            stringBuilder.AppendLine("\\begin{longtable}{lp{0.6\\textwidth}}");
            
            foreach (var dir in _templatesDir.SubDirectories)
            {
                GenerateTemplateOverviewTableSection(dir, stringBuilder, 0, lang);
            }

            stringBuilder.AppendLine("\\end{longtable}");
            return stringBuilder.ToString();
        }

        private void GenerateTemplateOverviewTableSection(TemplateDirectory templatesDir, StringBuilder stringBuilder, int depth, string lang)
        {
            const string hspace = "\\hspace{2mm} ";
            var spaces = (depth>0) ? String.Format("\\hspace{{{0}mm}} ", depth*4) : "";

            stringBuilder.AppendLine("\\multicolumn{2}{l}{" + spaces + " \\textbf{" + Helper.EscapeLaTeX(templatesDir.GetName(lang)) + "}} \\\\");

            Boolean itemadded = false;

            foreach (var templateDocumentationPage in templatesDir.ContainingTemplateDocPages)
            {
                var locTemplate = templateDocumentationPage.CurrentLocalization;

                // get icon
                String includeIcon = (locTemplate.Icon != null)
                    ? "\\includegraphics[width=16pt, height=16pt]{" + _objectConverter.GetImagePath(locTemplate.Icon, templateDocumentationPage.Name) + "}"
                    : "\\hspace{16pt}";
                includeIcon = "\\begin{minipage}[c]{16pt}" + includeIcon + "\\end{minipage}";

                // get templateName
                String templateName = Helper.EscapeLaTeX(templateDocumentationPage.CurrentLocalization.Name);
                templateName = "\\begin{flushleft}" + templateName + "\\end{flushleft}";
                templateName = "\\begin{minipage}[t]{0.4\\textwidth}" + templateName + "\\end{minipage}";

                // get summary
                String summary = _objectConverter.Convert(locTemplate.Summary, templateDocumentationPage);
                summary = "\\begin{flushleft}" + summary + "\\end{flushleft}";
                summary = "\\begin{minipage}[t]{0.6\\textwidth}" + summary + "\\end{minipage}";

                stringBuilder.AppendLine(String.Format("{0} {1} {2} & {3} \\\\", spaces + hspace, includeIcon, templateName, summary));

                itemadded = true;
            }

            if( itemadded ) stringBuilder.AppendLine("\\\\");

            foreach (var dir in templatesDir.SubDirectories)
            {
                GenerateTemplateOverviewTableSection(dir, stringBuilder, depth + 1, lang);
            }
        }

        private string SectionFromDepth(int depth)
        {
            if (depth == 0) 
                return @"\chapter";

            if (depth < 4)
                return @"\" + string.Join("", Enumerable.Repeat("sub", depth-1)) + "section";

            return @"\paragraph";
        }

        private void GenerateTemplateDescriptionSection(TemplateDirectory templatesDir, StringBuilder stringBuilder, int depth, string lang)
        {
            stringBuilder.AppendLine(string.Format("{0}{{{1}}}", SectionFromDepth(depth), Helper.EscapeLaTeX(templatesDir.GetName(lang))));

            foreach (var templateDocumentationPage in templatesDir.ContainingTemplateDocPages)
            {
                var locTemplate = templateDocumentationPage.CurrentLocalization;

                var description = _objectConverter.Convert(locTemplate.Description, templateDocumentationPage);
                description = Regex.Replace(description, "[\r\n]+", "\n");

                var templateName = Helper.EscapeLaTeX(templateDocumentationPage.CurrentLocalization.Name);
                stringBuilder.AppendLine(string.Format("{0}{{{1}}}", SectionFromDepth(depth+1), templateName));
                stringBuilder.AppendLine(description);

                if (_showAuthors)
                {
                    var author = _objectConverter.Convert(locTemplate.AuthorName, templateDocumentationPage);
                    stringBuilder.AppendLine("");
                    stringBuilder.AppendLine("Author: " + author);
                    stringBuilder.AppendLine("");
                }

                if (!_noIcons && locTemplate.Icon != null)
                {
                    var icon = _objectConverter.Convert(locTemplate.Icon, templateDocumentationPage);
                    stringBuilder.AppendLine(icon);
                }
            }

            foreach (var dir in templatesDir.SubDirectories)
            {
                GenerateTemplateDescriptionSection(dir, stringBuilder, depth + 1,lang);
            }
        }

        private string GenerateTemplateDescriptionCode(string lang)
        {
            var stringBuilder = new StringBuilder();
            //stringBuilder.AppendLine("\\chapter{" + OnlineDocumentationGenerator.Properties.Resources.LatexGenerator_ChapterSubTitle + "}");
            bool first = true;
            foreach (var dir in _templatesDir.SubDirectories)
            {
                stringBuilder.AppendLine("\\newpage");
                if (first) stringBuilder.AppendLine("\\label{part2}");
                first = false;
                GenerateTemplateDescriptionSection(dir, stringBuilder, 0, lang);
            }
            return stringBuilder.ToString();
        }

        private string GetVersion()
        {
            switch (AssemblyHelper.BuildType)
            {
                case Ct2BuildType.Developer:
                    return "Developer " + AssemblyHelper.Version;
                case Ct2BuildType.Nightly:
                    return "Nightly Build " + AssemblyHelper.Version;
                case Ct2BuildType.Beta:
                    return "Beta " + AssemblyHelper.Version;
                case Ct2BuildType.Stable:
                    return "Stable " + AssemblyHelper.Version;
            }
            return AssemblyHelper.Version.ToString();
        }

        private static string GetComponentCategory(ComponentCategory category)
        {
            switch (category)
            {
                case ComponentCategory.CiphersClassic:
                    return Properties.Resources.Category_Classic_Ciphers;
                case ComponentCategory.CiphersModernSymmetric:
                    return Properties.Resources.Category_CiphersModernSymmetric;
                case ComponentCategory.CiphersModernAsymmetric:
                    return Properties.Resources.Category_CiphersModernAsymmetric;
                case ComponentCategory.Steganography:
                    return Properties.Resources.Category_Steganography;
                case ComponentCategory.HashFunctions:
                    return Properties.Resources.Category_HashFunctions;
                case ComponentCategory.CryptanalysisSpecific:
                    return Properties.Resources.Category_CryptanalysisSpecific;
                case ComponentCategory.CryptanalysisGeneric:
                    return Properties.Resources.Category_CryptanalysisGeneric;
                case ComponentCategory.Protocols:
                    return Properties.Resources.Category_Protocols;
                case ComponentCategory.ToolsBoolean:
                    return Properties.Resources.Category_ToolsBoolean;
                case ComponentCategory.ToolsDataflow:
                    return Properties.Resources.Category_ToolsDataflow;
                case ComponentCategory.ToolsDataInputOutput:
                    return Properties.Resources.Category_ToolsDataInputOutput;
                case ComponentCategory.ToolsMisc:
                    return Properties.Resources.Category_ToolsMisc;
                case ComponentCategory.ToolsP2P:
                    return Properties.Resources.Category_ToolsP2P;
                default:
                    return Properties.Resources.Category_Unknown;
            }
        }

        private void StoreLaTeX(string content, string filename)
        {
            var outDir = Path.Combine(OutputDir, HelpDirectory);
            var filePath = Path.Combine(outDir, filename);

            try
            {
                if (!Directory.Exists(outDir))
                    Directory.CreateDirectory(outDir);

                var utf8WithoutBom = new System.Text.UTF8Encoding(false);   // Don't prepend LaTeX code with Byte Order Mark (BOM), as it confuses some LaTeX compilers.
                var streamWriter = new StreamWriter(filePath, false, utf8WithoutBom);
                streamWriter.Write(content);
                streamWriter.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error trying to write file {0}! Message: {1}", filePath, ex.Message));
            }
        }
    }
}
