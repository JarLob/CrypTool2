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
using System.Text;

namespace Cryptool.Plugins.DECODEDatabaseTools
{
    public static class Extensions
    {
        public static bool Contains(this string[] array, string element)
        {
            foreach (var str in array)
            {
                if (str.Equals(element))
                {
                    return true;
                }
            }
            return false;
        }
    }

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
            Page currentPage = new Page(document);
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
                Line currentLine = new Line(currentPage);
                currentLine.LineNumber = linenumber;
                linenumber++;

                //comments in the DECODE transcription format start with #
                if (trimmedLine.StartsWith("#"))
                {
                    currentLine.LineType = LineType.Comment;

                    //remove all # from beginning of string
                    int offset = 0;
                    while (trimmedLine[offset] == '#')
                    {
                        offset++;
                    }

                    string comment = trimmedLine.Substring(offset, trimmedLine.Length - offset).ToUpper().TrimStart();                

                    if(comment.StartsWith("PAGE"))
                    {
                        //at each IMAGE NAME comment, a new page (image) starts
                        currentLine.LineNumber = 1;
                        pagenumber++;
                        linenumber = 2;                                                
                        currentPage = new Page(document);
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
                Token token = new Token(currentLine);
                token.Text = trimmedLine;
                currentLine.Tokens.Add(token);
                currentPage.Lines.Add(currentLine);
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

    /// <summary>
    /// Simple parser that just splits the text into regular codes of defined length
    /// Also supports "nulls"
    /// </summary>
    public class NoVocabularyParser : SimpleSingleTokenParser
    {
        private string[] _nulls = new string[] { };
        private uint _regularCodeLength = 0;

        public NoVocabularyParser(uint regularCodeLength,  params string[] nulls)
        {
            if (nulls != null)
            {
                _nulls = nulls;
            }
            _regularCodeLength = regularCodeLength;
        }

        /// <summary>
        /// Returns the parsed new document
        /// </summary>
        /// <returns></returns>
        public override TextDocument GetDocument()
        {
            TextDocument document = base.GetDocument();
            if(document == null)
            {
                return null;
            }
            
            foreach(Page page in document.Pages)
            {
                //create new tokens based on the "old" tokens
                StringBuilder tokenStringBuilder = new StringBuilder();
                StringBuilder tagTokenBuilder = new StringBuilder();
                Line lastLine = null;

                bool is_a_tag = false;
                foreach (Line line in page.Lines)
                {
                    if(line.LineType == LineType.Comment)
                    {
                        continue;
                    }                    

                    //We are using the SimpleSingleTokenParser as baseline
                    //Thus, we have a single token for each line
                    string text = line.Tokens[0].Text;
                    line.Tokens.Remove(line.Tokens[0]);
                    
                    for (int position = 0; position < text.Length; position++)
                    {
                        string symbol = text.Substring(position, 1);
                        if (string.IsNullOrWhiteSpace(symbol) && !is_a_tag)
                        {
                            continue;
                        }

                        if (symbol.Equals("<"))
                        {
                            tagTokenBuilder.Append(symbol);
                            is_a_tag = true;
                            continue;
                        }
                        if(is_a_tag == true && symbol.Equals(">"))
                        {
                            tagTokenBuilder.Append(symbol);
                            Token tagToken = new Token(line);
                            tagToken.TokenType = TokenType.Tag;
                            tagToken.Text = tagTokenBuilder.ToString();
                            line.Tokens.Add(tagToken);
                            tagTokenBuilder.Clear();
                            is_a_tag = false;
                            continue;
                        }
                        if (is_a_tag)
                        {
                            tagTokenBuilder.Append(symbol);
                            continue;
                        }
                        if (_nulls.Contains(symbol))
                        {
                            //we found a null, thus, add a new token of previously collected characters
                            if (tokenStringBuilder.Length > 0)
                            {
                                Token regularCodeToken = new Token(line);
                                regularCodeToken.TokenType = TokenType.RegularCode;
                                regularCodeToken.Text = tokenStringBuilder.ToString();
                                line.Tokens.Add(regularCodeToken);
                                tokenStringBuilder.Clear();
                            }
                            Token nullToken = new Token(line);
                            nullToken.TokenType = TokenType.Null;
                            nullToken.Text = symbol;
                            line.Tokens.Add(nullToken);
                            continue;
                        }
                        //here, we know, we have no null symbol and no whitespace
                        //thus, the symbol belongs to a regular code
                        tokenStringBuilder.Append(symbol);                        
                        //finally, check if we reached the regular code length
                        if(_regularCodeLength != 0 && tokenStringBuilder.Length == _regularCodeLength)
                        {
                            Token regularCodeToken = new Token(line);
                            regularCodeToken.TokenType = TokenType.RegularCode;
                            regularCodeToken.Text = tokenStringBuilder.ToString();
                            line.Tokens.Add(regularCodeToken);
                            tokenStringBuilder.Clear();
                        }
                    }
                    lastLine = line;
                }

                if (tagTokenBuilder.Length > 0)
                {
                    Token tagToken = new Token(lastLine);
                    tagToken.TokenType = TokenType.Tag;
                    tagToken.Text = tagTokenBuilder.ToString();
                    lastLine.Tokens.Add(tagToken);
                    tagTokenBuilder.Clear();
                    is_a_tag = false;
                }
                if (tokenStringBuilder.Length > 0)
                {
                    Token regularCodeToken = new Token(lastLine);
                    regularCodeToken.TokenType = TokenType.RegularCode;
                    regularCodeToken.Text = tokenStringBuilder.ToString();
                    lastLine.Tokens.Add(regularCodeToken);
                    tokenStringBuilder.Clear();
                }
            }

            return document;
        }
    }

    /// <summary>
    /// Parses the text into regular codes of two
    /// Also supports vocabulary elements of three digits followed by a null digit
    /// </summary>
    public class Vocabulary3DigitsEndingWithNull1DigitParser : SimpleSingleTokenParser
    {
        private string[] _nulls = new string[] { };

        public Vocabulary3DigitsEndingWithNull1DigitParser(params string[] nulls)
        {
            if (nulls != null)
            {
                _nulls = nulls;
            }
        }

        /// <summary>
        /// Returns the parsed new document
        /// </summary>
        /// <returns></returns>
        public override TextDocument GetDocument()
        {
            TextDocument document = base.GetDocument();
            if (document == null)
            {
                return null;
            }

            foreach (Page page in document.Pages)
            {
                //create new tokens based on the "old" tokens
                StringBuilder tokenStringBuilder = new StringBuilder();
                StringBuilder tagTokenBuilder = new StringBuilder();
                Line lastLine = null;

                bool is_a_tag = false;
                foreach (Line line in page.Lines)
                {
                    if (line.LineType == LineType.Comment)
                    {
                        continue;
                    }

                    //We are using the SimpleSingleTokenParser as baseline
                    //Thus, we have a single token for each line
                    string text = line.Tokens[0].Text;
                    line.Tokens.Remove(line.Tokens[0]);

                    for (int position = 0; position < text.Length; position++)
                    {
                        string symbol = text.Substring(position, 1);
                        if (string.IsNullOrWhiteSpace(symbol) && !is_a_tag)
                        {
                            continue;
                        }

                        if (symbol.Equals("<"))
                        {
                            tagTokenBuilder.Append(symbol);
                            is_a_tag = true;
                            continue;
                        }
                        if (is_a_tag == true && symbol.Equals(">"))
                        {
                            tagTokenBuilder.Append(symbol);
                            Token tagToken = new Token(line);
                            tagToken.TokenType = TokenType.Tag;
                            tagToken.Text = tagTokenBuilder.ToString();
                            line.Tokens.Add(tagToken);
                            tagTokenBuilder.Clear();
                            is_a_tag = false;
                            continue;
                        }
                        if (is_a_tag)
                        {
                            tagTokenBuilder.Append(symbol);
                            continue;
                        }
                        if (_nulls.Contains(symbol))
                        {
                            //we found a null, thus, add a new token of previously collected characters
                            if (tokenStringBuilder.Length == 3)
                            {
                                //we know, that this is a vocabulary element (length = 3 digits)
                                Token vocabularyToken = new Token(line);
                                vocabularyToken.TokenType = TokenType.VocabularyElement;
                                vocabularyToken.Text = tokenStringBuilder.ToString();
                                line.Tokens.Add(vocabularyToken);
                                tokenStringBuilder.Clear();
                            }
                            else if(tokenStringBuilder.Length > 0)
                            {
                                Token regularCodeToken = new Token(line);
                                regularCodeToken.TokenType = TokenType.RegularCode;
                                regularCodeToken.Text = tokenStringBuilder.ToString();
                                line.Tokens.Add(regularCodeToken);
                                tokenStringBuilder.Clear();
                            }

                            Token nullToken = new Token(line);
                            nullToken.TokenType = TokenType.Null;
                            nullToken.Text = symbol;
                            line.Tokens.Add(nullToken);
                            continue;
                        }
                        //here, we know, we have no null symbol and no whitespace
                        //thus, the symbol (maybe) belongs to a regular code or a vocabulary element
                        tokenStringBuilder.Append(symbol);

                        if(tokenStringBuilder.Length > 3)
                        {
                            //if we are longer than 3 digits, we know we can not be in a vocabulary, 
                            //thus, we add a regular code
                            Token regularCodeToken = new Token(line);
                            regularCodeToken.TokenType = TokenType.RegularCode;
                            regularCodeToken.Text = tokenStringBuilder.ToString().Substring(0, 2);
                            line.Tokens.Add(regularCodeToken);
                            tokenStringBuilder.Remove(0, 2);
                        }
                    }
                    lastLine = line;
                }

                if (tagTokenBuilder.Length > 0)
                {
                    Token tagToken = new Token(lastLine);
                    tagToken.TokenType = TokenType.Tag;
                    tagToken.Text = tagTokenBuilder.ToString();
                    lastLine.Tokens.Add(tagToken);
                    tagTokenBuilder.Clear();
                    is_a_tag = false;
                }
                if (tokenStringBuilder.Length > 0)
                {
                    Token regularCodeToken = new Token(lastLine);
                    regularCodeToken.TokenType = TokenType.RegularCode;
                    regularCodeToken.Text = tokenStringBuilder.ToString();
                    lastLine.Tokens.Add(regularCodeToken);
                    tokenStringBuilder.Clear();
                }
            }
            return document;
        }
    }

    /// <summary>
    /// Parses the text into regular codes of two
    /// Also supports vocabulary elements of three digits followed by a null
    /// </summary>
    public class Vocabulary3DigitsEndingWithNull2DigitsParser : SimpleSingleTokenParser
    {
        private string[] _nulls = new string[] { };

        public Vocabulary3DigitsEndingWithNull2DigitsParser(params string[] nulls)
        {
            if (nulls != null)
            {
                _nulls = nulls;
            }
        }

        /// <summary>
        /// Returns the parsed new document
        /// </summary>
        /// <returns></returns>
        public override TextDocument GetDocument()
        {
            TextDocument document = base.GetDocument();
            if (document == null)
            {
                return null;
            }

            foreach (Page page in document.Pages)
            {
                //create new tokens based on the "old" tokens
                StringBuilder tokenStringBuilder = new StringBuilder();
                StringBuilder tagTokenBuilder = new StringBuilder();
                Line lastLine = null;

                bool is_a_tag = false;
                foreach (Line line in page.Lines)
                {
                    if (line.LineType == LineType.Comment)
                    {
                        continue;
                    }

                    //We are using the SimpleSingleTokenParser as baseline
                    //Thus, we have a single token for each line
                    string text = line.Tokens[0].Text;
                    line.Tokens.Remove(line.Tokens[0]);

                    for (int position = 0; position < text.Length; position++)
                    {
                        string symbol = text.Substring(position, 1);
                        if (string.IsNullOrWhiteSpace(symbol) && !is_a_tag)
                        {
                            continue;
                        }

                        if (symbol.Equals("<"))
                        {
                            tagTokenBuilder.Append(symbol);
                            is_a_tag = true;
                            continue;
                        }
                        if (is_a_tag == true && symbol.Equals(">"))
                        {
                            tagTokenBuilder.Append(symbol);
                            Token tagToken = new Token(line);
                            tagToken.TokenType = TokenType.Tag;
                            tagToken.Text = tagTokenBuilder.ToString();
                            line.Tokens.Add(tagToken);
                            tagTokenBuilder.Clear();
                            is_a_tag = false;
                            continue;
                        }
                        if (is_a_tag)
                        {
                            tagTokenBuilder.Append(symbol);
                            continue;
                        }

                        tokenStringBuilder.Append(symbol);
                        
                        if(tokenStringBuilder.Length == 5)
                        {
                            if(_nulls.Contains("" + tokenStringBuilder[tokenStringBuilder.Length - 1]))
                            {
                                //we have 2 null symbols => thus, we have a vocabulary element and 2 nulls
                                Token vocabularyToken = new Token(line);
                                vocabularyToken.TokenType = TokenType.VocabularyElement;
                                vocabularyToken.Text = tokenStringBuilder.ToString().Substring(0, 3);
                                line.Tokens.Add(vocabularyToken);
                                tokenStringBuilder.Remove(0, 3);

                                Token nullToken = new Token(line);
                                nullToken.TokenType = TokenType.Null;
                                nullToken.Text = tokenStringBuilder.ToString();
                                line.Tokens.Add(nullToken);
                                tokenStringBuilder.Clear();
                                continue;
                            }
                            else
                            {
                                Token regularCodeToken = new Token(line);
                                regularCodeToken.TokenType = TokenType.RegularCode;
                                regularCodeToken.Text = tokenStringBuilder.ToString().Substring(0, 2);
                                line.Tokens.Add(regularCodeToken);
                                tokenStringBuilder.Remove(0, 2);
                                continue;
                            }
                        }
                        if (tokenStringBuilder.Length == 4)
                        {
                            if (_nulls.Contains("" + tokenStringBuilder[tokenStringBuilder.Length - 1]))
                            {
                                Token regularCodeToken = new Token(line);
                                regularCodeToken.TokenType = TokenType.RegularCode;
                                regularCodeToken.Text = tokenStringBuilder.ToString().Substring(0, 2);
                                line.Tokens.Add(regularCodeToken);
                                tokenStringBuilder.Remove(0, 2);

                                Token nullToken = new Token(line);
                                nullToken.TokenType = TokenType.Null;
                                nullToken.Text = tokenStringBuilder.ToString();
                                line.Tokens.Add(nullToken);
                                tokenStringBuilder.Clear();
                                continue;
                            }
                        }
                        if (tokenStringBuilder.Length == 2)
                        {
                            if (_nulls.Contains("" + tokenStringBuilder[tokenStringBuilder.Length - 1]))
                            {                    
                                Token nullToken = new Token(line);
                                nullToken.TokenType = TokenType.Null;
                                nullToken.Text = tokenStringBuilder.ToString();
                                line.Tokens.Add(nullToken);
                                tokenStringBuilder.Clear();
                                continue;
                            }
                        }
                    }
                    lastLine = line;
                }

                if (tagTokenBuilder.Length > 0)
                {
                    Token tagToken = new Token(lastLine);
                    tagToken.TokenType = TokenType.Tag;
                    tagToken.Text = tagTokenBuilder.ToString();
                    lastLine.Tokens.Add(tagToken);
                    tagTokenBuilder.Clear();
                    is_a_tag = false;
                }
                if (tokenStringBuilder.Length > 0)
                {
                    Token regularCodeToken = new Token(lastLine);
                    regularCodeToken.TokenType = TokenType.RegularCode;
                    regularCodeToken.Text = tokenStringBuilder.ToString();
                    lastLine.Tokens.Add(regularCodeToken);
                    tokenStringBuilder.Clear();
                }
            }
            return document;
        }
    }

}
