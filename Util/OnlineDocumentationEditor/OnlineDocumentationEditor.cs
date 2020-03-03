/*
   Copyright 2020, Nils Kopal, University of Siegen

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
using Cryptool.Core;
using Cryptool.CrypWin;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Attributes;
using Cryptool.PluginBase.Editor;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using OnlineDocumentationGenerator;
using OnlineDocumentationGenerator.Generators.HtmlGenerator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;

namespace Cryptool.OnlineDocumentationEditor
{
    [TabColor("LightSeaGreen")]
    [EditorInfo("xml", true, false, false, true, false, false)]    
    [Author("Nils Kopal", "kopal@cryptool.org", "University of Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("Cryptool.OnlineDocumentationEditor.Properties.Resources", "PluginCaption", "PluginTooltip", null, "OnlineDocumentationEditor/icon.png")]
    public class OnlineDocumentationEditor : IEditor
    {

        #region members
        private OnlineDocumentationEditorPresentation _presentation = null;
        private List<DocumentationWrapper> _types = null;

        public event SelectedPluginChangedHandler OnSelectedPluginChanged;      
        public event ProjectTitleChangedHandler OnProjectTitleChanged;
        public event OpenProjectFileHandler OnOpenProjectFile;
        public event OpenTabHandler OnOpenTab;
        public event OpenEditorHandler OnOpenEditor;
        public event FileLoadedHandler OnFileLoaded;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public OnlineDocumentationEditor()
        {
            _presentation = new OnlineDocumentationEditorPresentation(this);
        }


        #region properties

        public bool CanUndo
        {
            get
            {
                return false;
            }
        }

        public bool CanRedo
        {
            get
            {
                return false;
            }
        }

        public bool CanCut
        {
            get
            {
                return false;
            }
        }

        public bool CanCopy
        {
            get
            {
                return false;
            }
        }

        public bool CanPaste
        {
            get
            {
                return false;
            }
        }

        public bool CanRemove
        {
            get
            {
                return false;
            }
        }

        public bool CanExecute
        {
            get
            {
                return false;
            }
        }

        public bool CanStop
        {
            get
            {
                return false;
            }
        }

        public bool CanSave
        {
            get
            {
                return true;
            }
        }

        public bool HasChanges
        {
            get
            {
                return false;
            }
        }

        public bool CanPrint
        {
            get
            {
                return false;
            }
        }

        public bool CanSav
        {
            get
            {
                return false;
            }
        }

        public string CurrentFile
        {
            get
            {
                return null;
            }
        }

        public string SamplesDir
        {
            get;
            set;
        }
        public bool ReadOnly
        {
            get;set;            
        }
        public bool HasBeenClosed
        {
            get;
            set;
        }

        public PluginManager PluginManager
        {
            get;
            set;
        }

        public ISettings Settings
        {
            get;
            set;
        }

        public UserControl Presentation
        {
            get
            {
                return _presentation;
            }
        }

        #endregion
       

        public void Add(Type type)
        {            
        }

        public void AddImage()
        {
            
        }

        public void AddText()
        {
            
        }

        public void Copy()
        {
            
        }

        public void Cut()
        {
            
        }

        public void Dispose()
        {
            
        }

        public void Execute()
        {
            
        }

        /// <summary>
        /// Updates xml (left side) and html (right side) of the editor
        /// </summary>
        /// <param name="type"></param>
        /// <param name="xElement"></param>
        private void ChangeXMLAndHTML(Type type, XElement xElement = null)
        {
            try
            {
                string fileName = OnlineHelp.GetPluginDocFilename(type, "en");
                string baseDir = DirectoryHelper.BaseDirectory;
                string helpDir = OnlineHelp.HelpDirectory;
                string fileUrl = baseDir + helpDir + "\\" + fileName;

                if (!File.Exists(fileUrl) || xElement != null)
                {
                    var generatingDocWindow = new GeneratingWindow();
                    string message = string.Format("{0} does not exist. Generating documentation for {1}", fileUrl, type.Name);
                    generatingDocWindow.SetMessage(message);
                    GuiLogMessage(message, NotificationLevel.Info);
                    generatingDocWindow.Title = "Generating Online Documentation";
                    generatingDocWindow.Show();
                    GenerateDoc(type, xElement);
                    generatingDocWindow.Close();
                }
                _presentation.WebBrowser.Source = new Uri(fileUrl);
                var xml = GetOnlineDocumentationXML(type);
                if (xml != null && xElement == null)
                {
                    _presentation.XMLTextBox.Text = xml.ToString();
                }
            }
            catch(Exception ex)
            {
                GuiLogMessage(string.Format("Could not change xml and html. Error occured: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Starts the (re-)generation of the online help of the component identified by its type
        /// If an xml is given, the original is overwritten before generation
        /// </summary>
        /// <param name="type"></param>
        /// <param name="xElement"></param>
        private void GenerateDoc(Type type, XElement xElement = null)
        {
            string baseDir = DirectoryHelper.BaseDirectory;
            var docGenerator = new DocGenerator();
            DocGenerator.XMLReplacement = new XMLReplacement() { XElement = xElement, Type = type };
            var htmlGenerator = new HtmlGenerator(type);            
            docGenerator.Generate(baseDir, htmlGenerator);               
        }

        /// <summary>
        /// Returns the internal stored xml of the defined component (identified by its type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private XElement GetOnlineDocumentationXML(Type type)
        {
            var descriptionUrl = type.GetPluginInfoAttribute().DescriptionUrl;
            if (descriptionUrl == null || Path.GetExtension(descriptionUrl).ToLower() != ".xml")
            {
                return null;
            }

            if (descriptionUrl != string.Empty)
            {
                int sIndex = descriptionUrl.IndexOf('/');
                var xmlUri = new Uri(string.Format("pack://application:,,,/{0};component/{1}",
                                                    descriptionUrl.Substring(0, sIndex), descriptionUrl.Substring(sIndex + 1)));
                var stream = Application.GetResourceStream(xmlUri).Stream;
                return XElement.Load(stream);
            }
            return null;
        }

        /// <summary>
        /// Searches for all types (components, editors) which have a documentation
        /// and puts these into a list of DocumentationWrappers
        /// </summary>
        /// <returns></returns>
        private List<DocumentationWrapper> GetAllDocumentations()
        {
            //search for types with documentations
            var componentTypes = (
                  from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                    from assemblyType in domainAssembly.GetTypes()
                  where typeof(IPlugin).IsAssignableFrom(assemblyType) &&
                    assemblyType.GetPluginInfoAttribute() != null &&
                    assemblyType.GetPluginInfoAttribute().DescriptionUrl != null
                  select assemblyType).ToArray();

            //Create list of documentation wrappers
            List<DocumentationWrapper> list = new List<DocumentationWrapper>();
            foreach(var type in componentTypes)
            {
                list.Add(new DocumentationWrapper() { PluginType = type, Name = type.Name });
            }
            return list;
        }

        /// <summary>
        /// Called by CT2 to initialize this editor
        /// </summary>
        public void Initialize()
        {            
            try
            {
                _types = GetAllDocumentations();
                _presentation.ComboBox.ItemsSource = _types;
                _presentation.ComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception during creation of documentations list: {0}", ex.Message), NotificationLevel.Error);
            }                   

            if (_types != null && _types.Count > 0)
            {
                ChangeXMLAndHTML(_types[0].PluginType);
            }
            
        }

        /// <summary>
        /// User selected a different online help article
        /// </summary>
        /// <param name="e"></param>
        internal void ComboBoxSelectionChanged(SelectionChangedEventArgs e)
        {
            if (_presentation.ComboBox.SelectedItem == null)
            {
                return;
            }
            Type type = ((DocumentationWrapper)_presentation.ComboBox.SelectedItem).PluginType;
            if (type == null)
            {
                return;
            }
            ChangeXMLAndHTML(type);
        }

        /// <summary>
        /// If user presses Ctrl+G, the html is renewed
        /// </summary>
        /// <param name="e"></param>
        internal void XMLTextBox_KeyDown(KeyEventArgs e)
        {
            if(e.Key == Key.G && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                try
                {
                    XElement xElement = XElement.Parse(_presentation.XMLTextBox.Text);
                    ChangeXMLAndHTML(_types[_presentation.ComboBox.SelectedIndex].PluginType, xElement);
                }
                catch(Exception ex)
                {
                    GuiLogMessage(String.Format("Error occured during generation of html: {0}", ex.Message), NotificationLevel.Error);
                }
            }
        }

        public void New()
        {
            
        }

        public void Open(string fileName)
        {
            
        }

        public void Paste()
        {
            
        }

        public void Print()
        {
            
        }

        public void Redo()
        {
           
        }

        public void Remove()
        {
         
        }

        /// <summary>
        /// saves the xml to the defined file
        /// </summary>
        /// <param name="fileName"></param>
        public void Save(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
                using (FileStream stream = new FileStream(fileName, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write(_presentation.XMLTextBox.Text);
                    }
                }
            }
            catch(Exception ex)
            {
                GuiLogMessage(string.Format("Exception occured during saving xml file: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        public void ShowSelectedEntityHelp()
        {
            
        }

        public void Stop()
        {
            
        }

        public void Undo()
        {
            
        }

        /// <summary>
        /// Logs to CT2 gui log
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logLevel"></param>
        internal void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }
    }

    internal class DocumentationWrapper
    {
        public Type PluginType
        {
            get; set;
        }

        public string XMLDocumentation
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public override string ToString()
        {
            return Name;
        }
    }

}
