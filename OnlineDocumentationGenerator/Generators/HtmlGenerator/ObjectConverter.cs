﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Cryptool.PluginBase;
using OnlineDocumentationGenerator.DocInformations;
using OnlineDocumentationGenerator.Properties;
using WorkspaceManager.Model;

namespace OnlineDocumentationGenerator.Generators.HtmlGenerator
{
    /// <summary>
    /// Class for converting an object to an html representation.
    /// Also takes care of referenced image resources while converting.
    /// </summary>
    class ObjectConverter
    {
        private readonly List<PluginDocumentationPage> _docPages;
        private readonly string _outputDir;
        private readonly HashSet<string> _createdImages = new HashSet<string>();

        public ObjectConverter(List<PluginDocumentationPage> docPages, string outputDir)
        {
            _docPages = docPages;
            _outputDir = outputDir;
        }

        public string Convert(object theObject, EntityDocumentationPage docPage)
        {
            if (theObject == null)
                return Resources.Not_available;

            if (theObject is XElement)
            {
                var elementString = ConvertXElement((XElement)theObject, docPage);
                if (string.IsNullOrWhiteSpace(elementString))
                {
                    return Convert(null, docPage);
                }
                return elementString;
            }
            if (theObject is BitmapFrame)
            {
                return ConvertImageSource((BitmapFrame)theObject, docPage.Name, docPage);
            }
            if (theObject is ComponentTemplateList)
            {
                return ConvertTemplateList((ComponentTemplateList)theObject, docPage);
            }
            if (theObject is Reference.ReferenceList)
            {
                return ((Reference.ReferenceList)theObject).ToHTML(Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);
            }
            if (theObject is PropertyInfoAttribute[])
            {
                return ConvertConnectorList((PropertyInfoAttribute[])theObject);
            }
            if (theObject is TaskPaneAttribute[])
            {
                return ConvertSettingsList((TaskPaneAttribute[]) theObject);
            }

            return theObject.ToString();
        }

        private string ConvertSettingsList(TaskPaneAttribute[] settings)
        {
            if ((settings != null) && (settings.Length > 0))
            {
                var codeBuilder = new StringBuilder();
                codeBuilder.AppendLine("<table width=\"100%\"  border=\"1\">");
                codeBuilder.AppendLine(string.Format("<tr> <th>{0}</th> <th>{1}</th> <th>{2}</th> </tr>",
                                                     Resources.HtmlGenerator_GenerateConnectorListCode_Name,
                                                     Resources.HtmlGenerator_GenerateConnectorListCode_Description,
                                                     Resources.HtmlGenerator_GenerateSettingsListCode_Type));

                foreach (var setting in settings)
                {
                    codeBuilder.AppendLine(string.Format("<tr> <td>{0}</td> <td>{1}</td> <td>{2}</td> </tr>",
                                                         HttpUtility.HtmlEncode(setting.Caption), HttpUtility.HtmlEncode(setting.ToolTip),
                                                         GetControlTypeString(setting.ControlType)));
                }

                codeBuilder.AppendLine("</table>");
                return codeBuilder.ToString();
            }
            return Resources.NoContent;
        }

        private string GetControlTypeString(ControlType controlType)
        {
            switch (controlType)
            {
                case ControlType.TextBox:
                    return Resources.Text_box;
                case ControlType.ComboBox:
                    return Resources.Combo_box;
                case ControlType.RadioButton:
                    return Resources.Radio_button;
                case ControlType.CheckBox:
                    return Resources.Check_box;
                case ControlType.OpenFileDialog:
                    return Resources.Open_file_dialog;
                case ControlType.SaveFileDialog:
                    return Resources.Save_file_dialog;
                case ControlType.NumericUpDown:
                    return Resources.Numeric_up_down;
                case ControlType.Button:
                    return Resources.Button;
                case ControlType.Slider:
                    return Resources.Slider;
                case ControlType.TextBoxReadOnly:
                    return Resources.Text_box__read_only_;
                case ControlType.DynamicComboBox:
                    return Resources.Dynamic_combo_box;
                case ControlType.TextBoxHidden:
                    return Resources.Text_box__hidden_;
                case ControlType.KeyTextBox:
                    return Resources.Key_text_box;
                default:
                    throw new ArgumentOutOfRangeException("controlType");
            }
        }

        private string ConvertConnectorList(PropertyInfoAttribute[] connectors)
        {
            if ((connectors != null) && (connectors.Length > 0))
            {
                var codeBuilder = new StringBuilder();
                codeBuilder.AppendLine("<table width=\"100%\"  border=\"1\">");
                codeBuilder.AppendLine(string.Format("<tr> <th>{0}</th> <th>{1}</th> <th>{2}</th> <th>{3}</th> </tr>",
                                                     Resources.HtmlGenerator_GenerateConnectorListCode_Name,
                                                     Resources.HtmlGenerator_GenerateConnectorListCode_Description,
                                                     Resources.HtmlGenerator_GenerateConnectorListCode_Direction,
                                                     Resources.HtmlGenerator_GenerateConnectorListCode_Type));
                
                foreach (var connector in connectors)
                {
                    var type = connector.PropertyInfo.PropertyType.Name;
                    var color = ColorHelper.GetLineColor(connector.PropertyInfo.PropertyType);
                    codeBuilder.AppendLine(
                        string.Format("<tr> <td bgcolor=\"#{0}{1}{2}\">{3}</td> <td bgcolor=\"#{0}{1}{2}\">{4}</td> <td bgcolor=\"#{0}{1}{2}\" nowrap>{5}</td> <td bgcolor=\"#{0}{1}{2}\">{6}</td> </tr>", color.R.ToString("x"), color.G.ToString("x"), color.B.ToString("x"),
                                      HttpUtility.HtmlEncode(connector.Caption),
                                      HttpUtility.HtmlEncode(connector.ToolTip),
                                      GetDirectionString(connector.Direction),
                                      type));
                }

                codeBuilder.AppendLine("</table>");
                return codeBuilder.ToString();
            }
            return Resources.NoContent;
        }

        private string GetDirectionString(Direction direction)
        {            
            switch (direction)
            {
                case Direction.InputData:
                    return string.Format("◄ {0}", Resources.Input_data);
                case Direction.OutputData:
                    return string.Format("► {0}", Resources.Output_data);
                case Direction.ControlSlave:
                    return string.Format("▲ {0}", Resources.Control_slave);
                case Direction.ControlMaster:
                    return string.Format("▼ {0}", Resources.Control_master);
                default:
                    throw new ArgumentOutOfRangeException("direction");
            }
        }

        private string ConvertTemplateList(ComponentTemplateList componentTemplateList, EntityDocumentationPage entityDocumentationPage)
        {
            if (componentTemplateList.Templates.Count == 0)
                return Resources.NoContent;

            var codeBuilder = new StringBuilder();
            codeBuilder.AppendLine(string.Format("<p>{0}</p>", Resources.Templates_description));
            codeBuilder.AppendLine("<table width=\"100%\"  border=\"1\">");
            codeBuilder.AppendLine(string.Format("<tr> <th>{0}</th> <th>{1}</th> </tr>",
                Resources.File, Resources.Description));

            foreach (var template in componentTemplateList.Templates)
            {
                var link = Path.Combine(Path.Combine("..\\..", DocGenerator.TemplateDirectory), template.Path);
                var file = Path.GetFileName(template.Path);
                codeBuilder.AppendLine(string.Format("<tr> <td><a href=\"{0}\">{1}</a></td> <td>{2}</td> </tr>",
                    link, file, ConvertXElement(template.Description, entityDocumentationPage)));
            }

            codeBuilder.AppendLine("</table>");

            return codeBuilder.ToString();
        }

        /// <summary>
        /// Converts the given imageSource parameter to a file in the doc directory and returns an html string referencing this.
        /// </summary>
        /// <param name="imageSource">The ImageSource containing the image to convert.</param>
        /// <param name="filename">The wished filename (withouth the extension)</param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        private string ConvertImageSource(BitmapFrame imageSource, string filename, EntityDocumentationPage entityDocumentationPage)
        {
            filename = filename + ".png";
            if (!_createdImages.Contains(filename))
            {
                var dir = Path.Combine(Path.Combine(_outputDir, OnlineHelp.HelpDirectory), entityDocumentationPage.DocPath);
                //create image file:
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                var file = Path.Combine(dir, filename);
                using (var fileStream = new FileStream(file, FileMode.Create))
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(imageSource);
                    encoder.Save(fileStream);
                }
            }
            _createdImages.Add(filename);
            return string.Format("<img src=\"{0}\" />", filename);
        }

        /// <summary>
        /// Converts the given xelement, which is from the xml doc file, into an html formated representation.
        /// </summary>
        /// <param name="xelement"></param>
        /// <param name="entityDocumentationPage"></param>
        /// <returns></returns>
        private string ConvertXElement(XElement xelement, EntityDocumentationPage entityDocumentationPage)
        {
            var result = new StringBuilder();
            foreach (var node in xelement.Nodes())
            {
                if (node is XText)
                {
                    result.Append(HttpUtility.HtmlEncode(((XText)node).Value));
                }
                else if (node is XElement)
                {
                    var nodeName = ((XElement) node).Name.ToString();
                    switch (nodeName)
                    {
                        case "b":
                        case "i":
                        case "u":
                            var nodeRep = ConvertXElement((XElement)node, entityDocumentationPage);
                            result.Append(string.Format("<{0}>{1}</{0}>", nodeName, nodeRep));
                            break;
                        case "ref":
                            var idAtt = ((XElement)node).Attribute("id");
                            if (idAtt != null)
                            {
                                if (entityDocumentationPage is PluginDocumentationPage)
                                {
                                    var htmlLinkToRef = ((PluginDocumentationPage)entityDocumentationPage).References.GetHTMLinkToRef(idAtt.Value);
                                    if (htmlLinkToRef != null)
                                    {
                                        result.Append(htmlLinkToRef);
                                    }
                                }
                            }
                            break;
                        case "img":
                            var srcAtt = ((XElement) node).Attribute("src");
                            if (srcAtt != null)
                            {
                                int sIndex = srcAtt.Value.IndexOf('/');
                                var image = BitmapFrame.Create(new Uri(string.Format("pack://application:,,,/{0};component/{1}", 
                                    srcAtt.Value.Substring(0, sIndex), srcAtt.Value.Substring(sIndex + 1))));
                                var filename = string.Format("{0}_{1}", entityDocumentationPage.Name, Path.GetFileNameWithoutExtension(srcAtt.Value));
                                result.Append(ConvertImageSource(image, filename, entityDocumentationPage));
                            }
                            break;
                        case "newline":
                            result.Append("<br/>");
                            break;
                        case "section":
                            var headline = ((XElement) node).Attribute("headline");
                            if (headline != null)
                            {
                                result.AppendLine(string.Format("<h3>{0}</h3>", headline.Value));
                                result.AppendLine(ConvertXElement((XElement) node, entityDocumentationPage));
                            }
                            break;
                        case "enum":
                        case "list":
                            var t = (nodeName == "enum") ? "ol" : "ul";
                            result.AppendLine(string.Format("<{0}>", t));
                            foreach (var item in ((XElement)node).Elements("item"))
                            {
                                result.AppendLine(string.Format("<li>{0}</li>", ConvertXElement(item, entityDocumentationPage)));
                            }
                            result.AppendLine(string.Format("</{0}>", t));
                            break;
                        case "external":
                            var reference = ((XElement) node).Attribute("ref");
                            if (reference != null)
                            {
                                var linkText = ConvertXElement((XElement) node, entityDocumentationPage);
                                if (string.IsNullOrEmpty(linkText))
                                {
                                    linkText = reference.Value;
                                }
                                result.Append(string.Format("<a href=\"{0}?external\"><img src=\"../external_link.png\" border=\"0\">{1}</a>", reference.Value, linkText));
                            }
                            break;
                        case "docRef":
                            var itemAttribute = ((XElement)node).Attribute("item");
                            if (itemAttribute != null)
                            {
                                var linkText = ConvertXElement((XElement)node, entityDocumentationPage);
                                if (string.IsNullOrEmpty(linkText))
                                {
                                    linkText = itemAttribute.Value;
                                }
                                
                                int dirLevel = entityDocumentationPage.DocPath.Split(Path.PathSeparator).Length - 1;
                                var d = "";
                                for (int i = 0; i < dirLevel; i++)
                                {
                                    d += Path.Combine(d, "..");
                                }
                                var entityLink = GetEntityLink(itemAttribute.Value);
                                if (entityLink != null)
                                {
                                    result.Append(string.Format("<a href=\"{0}\">{1}</a>", Path.Combine(d, entityLink), linkText));
                                }
                                else
                                {
                                    result.Append(string.Format("<i>{0}</i>", linkText));
                                }
                            }
                            break;
                        default:
                            continue;
                    }
                }
            }

            return result.ToString();
        }

        private string GetEntityLink(string entity)
        {
            foreach(var p in _docPages)
            {
                if (p.Localizations["en"].Name == entity)
                {
                    var lang = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
                    if (p.AvailableLanguages.Contains(lang))
                    {
                        return OnlineHelp.GetDocFilename(p.PluginType, lang);
                    }
                    else
                    {
                        return OnlineHelp.GetDocFilename(p.PluginType, "en");
                    }
                }
            }
            return null;
        }
    }
}
