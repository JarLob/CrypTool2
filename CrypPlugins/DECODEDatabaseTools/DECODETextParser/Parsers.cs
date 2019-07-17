/*
   Copyright 2019 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System;

namespace Cryptool.Plugins.DECODEDatabaseTools
{
    /// <summary>
    /// Abstract class for a parser
    /// </summary>
    public abstract class Parser
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Parser()
        {

        }

        /// <summary>
        /// Text Document to parse
        /// </summary>
        public string DECODETextDocument
        {
            get;
            set;
        }

        public abstract TextDocument GetDocument();

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        protected void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, null, new GuiLogEventArgs(message, null, logLevel));
        }
    }

    /// <summary>
    /// This is a default parser that just creates pages where each line is a single token
    /// </summary>
    public class SimpleSingleTokenParser : Parser
    {
        /// <summary>
        /// Creates a SimpleSingleTokenParser
        /// </summary>
        public SimpleSingleTokenParser()
        {

        }

        /// <summary>
        /// Returns a parsed document
        /// </summary>
        /// <returns></returns>
        public override TextDocument GetDocument()
        {
            if (String.IsNullOrEmpty(DECODETextDocument))
            {
                return null;
            }

            int pagenumber = 1;
            int linenumber = 1;

            TextDocument document = new TextDocument();
            Page currentPage = new Page();
            currentPage.PageNumber = pagenumber;
            document.Pages.Add(currentPage);            

            foreach(string textLine in DECODETextDocument.Split(new[] { '\r', '\n' }))
            {
                string trimmedLine = textLine.Trim();
                //the parser ignores empty lines
                if (string.IsNullOrEmpty(trimmedLine) || string.IsNullOrWhiteSpace(trimmedLine))
                {
                    continue;
                }
                Line line = new Line();
                line.LineNumber = linenumber;
                linenumber++;

                //comments in the DECODE transcription format start with #
                if (trimmedLine.StartsWith("#"))
                {
                    //remove all # from beginning of string
                    int offset = 0;
                    while (trimmedLine[offset] == '#')
                    {
                        offset++;
                    }

                    string comment = trimmedLine.Substring(offset, trimmedLine.Length - offset).ToUpper().TrimStart();

                    line.LineType = LineType.Comment;
                    if(comment.StartsWith("PAGE"))
                    {
                        //at each IMAGE NAME comment, a new page (image) starts
                        line.LineNumber = 1;
                        pagenumber++;
                        linenumber = 2;                                                
                        currentPage = new Page();
                        currentPage.PageNumber = pagenumber;
                        document.Pages.Add(currentPage);
                    }
                    else if(comment.StartsWith("CATALOG NAME"))
                    {
                        var split = trimmedLine.Split(':');
                        if(split.Length != 2)
                        {
                            GuiLogMessage(String.Format("Wrong catalog name definition in document: {0}", trimmedLine), NotificationLevel.Warning);
                        }
                        else
                        {
                            if (!String.IsNullOrEmpty(document.CatalogName))
                            {
                                GuiLogMessage("Catalog name is defined twice in this document. Ignoring second definition", NotificationLevel.Warning);
                            }
                            else
                            {
                                document.CatalogName = split[1].Trim();
                            }
                        }
                    }
                    else if (comment.StartsWith("IMAGE NAME"))
                    {
                        var split = trimmedLine.Split(':');
                        if (split.Length != 2)
                        {
                            GuiLogMessage(String.Format("Wrong image name definition in document: {0}", trimmedLine), NotificationLevel.Warning);
                        }
                        else
                        {
                            if (!String.IsNullOrEmpty(document.ImageName))
                            {
                                GuiLogMessage("Image name is defined twice in this document. Ignoring second definition", NotificationLevel.Warning);
                            }
                            else
                            {
                                document.ImageName = split[1].Trim();
                            }
                        }
                    }
                    else if (comment.StartsWith("TRANSCRIBER NAME"))
                    {
                        var split = trimmedLine.Split(':');
                        if (split.Length != 2)
                        {
                            GuiLogMessage(String.Format("Wrong transcriber name definition in document: {0}", trimmedLine), NotificationLevel.Warning);
                        }
                        else
                        {
                            if (!String.IsNullOrEmpty(document.TranscriberName))
                            {
                                GuiLogMessage("Transcriber name is defined twice in this document. Ignoring second definition", NotificationLevel.Warning);
                            }
                            else
                            {
                                document.TranscriberName = split[1].Trim();
                            }
                        }
                    }
                    else if (comment.StartsWith("DATE OF TRANSCRIPTION"))
                    {
                        var split = trimmedLine.Split(':');
                        if (split.Length != 2)
                        {
                            GuiLogMessage(String.Format("Wrong date of transcription definition in document: {0}", trimmedLine), NotificationLevel.Warning);
                        }
                        else
                        {
                            if (!String.IsNullOrEmpty(document.DateOfTranscription))
                            {
                                GuiLogMessage("Date of transcription name is defined twice in this document. Ignoring second definition", NotificationLevel.Warning);
                            }
                            else
                            {
                                document.DateOfTranscription = split[1].Trim();
                            }
                        }
                    }
                    else if (comment.StartsWith("TRANSCRIPTION TIME"))
                    {
                        var split = trimmedLine.Split(':');
                        if (split.Length != 2)
                        {
                            GuiLogMessage(String.Format("Wrong transcription time definition in document: {0}", trimmedLine), NotificationLevel.Warning);
                        }
                        else
                        {
                            if (!String.IsNullOrEmpty(document.TranscriptionTime))
                            {
                                GuiLogMessage("Transcription time is defined twice in this document. Ignoring second definition", NotificationLevel.Warning);
                            }
                            else
                            {
                                document.TranscriptionTime = split[1].Trim();
                            }
                        }
                    }
                    else if (comment.StartsWith("COMMENTS"))
                    {
                        var split = trimmedLine.Split(':');
                        if (split.Length != 2)
                        {
                            GuiLogMessage(String.Format("Wrong comments definition in document: {0}", trimmedLine), NotificationLevel.Warning);
                        }
                        else
                        {
                            if (!String.IsNullOrEmpty(document.Comments))
                            {
                                GuiLogMessage("Comments is defined twice in this document. Ignoring second definition", NotificationLevel.Warning);
                            }
                            else
                            {
                                document.Comments = split[1].Trim();
                            }
                        }
                    }
                }
                Token token = new Token();
                token.Text = trimmedLine;
                line.Tokens.Add(token);
                currentPage.Lines.Add(line);
            }

            //check, if header fields are set; if not, set these to "undefined"
            if (String.IsNullOrEmpty(document.CatalogName))
            {
                document.CatalogName = "undefined";
            }
            if (String.IsNullOrEmpty(document.ImageName))
            {
                document.ImageName = "undefined";
            }
            if (String.IsNullOrEmpty(document.TranscriberName))
            {
                document.TranscriberName = "undefined";
            }
            if (String.IsNullOrEmpty(document.DateOfTranscription))
            {
                document.DateOfTranscription = "undefined";
            }
            if (String.IsNullOrEmpty(document.TranscriptionTime))
            {
                document.TranscriptionTime = "undefined";
            }
            if (String.IsNullOrEmpty(document.TranscriptionMethod))
            {
                document.TranscriptionMethod = "undefined";
            }
            if (String.IsNullOrEmpty(document.Comments))
            {
                document.Comments = "undefined";
            }
            return document;
        }
    }
}
