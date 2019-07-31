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

        public static string[] Concat(this string[] arrayA, string[] arrayB)
        {
            if (arrayB == null || arrayB.Length == 0)
            {
                return arrayA;
            }
            string[] arrayC = new string[arrayA.Length + arrayB.Length];

            Array.Copy(arrayA, arrayC, arrayA.Length);
            Array.Copy(arrayB, 0, arrayC, arrayA.Length, arrayB.Length);
            return arrayC;
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

            foreach (string textLine in DECODETextDocument.Split(new[] { '\r', '\n' }))
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

                    if (comment.StartsWith("PAGE"))
                    {
                        //at each PAGE comment, a new page (image) starts
                        currentLine.LineNumber = 1;
                        pagenumber++;
                        linenumber = 2;
                        currentPage = new Page(document);
                        currentPage.PageNumber = pagenumber;
                        document.Pages.Add(currentPage);
                    }
                    else if (comment.StartsWith("CATALOG NAME"))
                    {
                        var split = trimmedLine.Split(':');
                        if (split.Length != 2)
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
                            if (string.IsNullOrEmpty(document.ImageName))                               
                            {
                                document.ImageName = split[1].Trim();
                            }
                            else
                            {
                                //at each IMAGE comment, a new page (image) starts
                                //only at first one we don't do that
                                currentLine.LineNumber = 1;
                                pagenumber++;
                                linenumber = 2;
                                currentPage = new Page(document);
                                currentPage.PageNumber = pagenumber;
                                document.Pages.Add(currentPage);
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

        public NoVocabularyParser(uint regularCodeLength, params string[] nulls)
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
                        if (_regularCodeLength != 0 && tokenStringBuilder.Length == _regularCodeLength)
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
    public class Vocabulary3DigitsEndingWithNull1DigitsParser : SimpleSingleTokenParser
    {
        private string[] _nulls = new string[] { };

        public Vocabulary3DigitsEndingWithNull1DigitsParser(params string[] nulls)
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
                            else if (tokenStringBuilder.Length > 0)
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

                        if (tokenStringBuilder.Length > 3)
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

                while (tokenStringBuilder.Length > 0)
                {
                    int len = Math.Min(tokenStringBuilder.Length, 2);
                    Token regularCodeToken = new Token(lastLine);
                    regularCodeToken.TokenType = TokenType.RegularCode;
                    regularCodeToken.Text = tokenStringBuilder.ToString().Substring(0, len);
                    lastLine.Tokens.Add(regularCodeToken);
                    tokenStringBuilder.Remove(0, len);
                }
            }
            return document;
        }
    }

    /// <summary>
    /// Parses the text into regular codes of two
    /// Also supports vocabulary elements of three digits followed by two nulls
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

                        if (tokenStringBuilder.Length == 5)
                        {
                            if (_nulls.Contains("" + tokenStringBuilder[tokenStringBuilder.Length - 1]))
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
                while (tokenStringBuilder.Length > 0)
                {
                    int len = Math.Min(tokenStringBuilder.Length, 2);
                    Token regularCodeToken = new Token(lastLine);
                    regularCodeToken.TokenType = TokenType.RegularCode;
                    regularCodeToken.Text = tokenStringBuilder.ToString().Substring(0, len);
                    lastLine.Tokens.Add(regularCodeToken);
                    tokenStringBuilder.Remove(0, len);
                }
            }
            return document;
        }
    }

    /// <summary>
    /// Parses the text into regular codes of two
    /// Also supports vocabulary elements of four digits with a prefix
    /// </summary>
    public class Vocabulary4DigitsWithPrefixParser : SimpleSingleTokenParser
    {
        private string[] _nulls = new string[] { };
        private string _vocabularyPrefix = null;

        public Vocabulary4DigitsWithPrefixParser(string vocabularyPrefix, params string[] nulls)
        {
            if (nulls != null)
            {
                _nulls = nulls;
            }
            _vocabularyPrefix = vocabularyPrefix;
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

                        if (tokenStringBuilder.Length == 4)
                        {
                            string symbol0 = "" + tokenStringBuilder[0];
                            string symbol1 = "" + tokenStringBuilder[1];

                            if (symbol0.Equals(_vocabularyPrefix))
                            {
                                //vocabulary
                                Token vocabularyToken = new Token(line);
                                vocabularyToken.TokenType = TokenType.VocabularyElement;
                                vocabularyToken.Text = tokenStringBuilder.ToString().Substring(0, 4);
                                line.Tokens.Add(vocabularyToken);
                                tokenStringBuilder.Clear();
                                continue;

                            }
                            else if (_nulls.Contains(symbol0) && _nulls.Contains(symbol1))
                            {
                                //null length 2
                                Token nullToken = new Token(line);
                                nullToken.TokenType = TokenType.Null;
                                nullToken.Text = tokenStringBuilder.ToString().Substring(0, 2);
                                line.Tokens.Add(nullToken);
                                tokenStringBuilder.Remove(0, 2);
                                continue;
                            }
                            else if (_nulls.Contains(symbol0))
                            {
                                //null length 1
                                Token nullToken = new Token(line);
                                nullToken.TokenType = TokenType.Null;
                                nullToken.Text = "" + tokenStringBuilder[0];
                                line.Tokens.Add(nullToken);
                                tokenStringBuilder.Remove(0, 1);
                                continue;
                            }
                            else if (_nulls.Contains(symbol1))
                            {
                                //code length 1
                                Token regularCodeToken = new Token(line);
                                regularCodeToken.TokenType = TokenType.RegularCode;
                                regularCodeToken.Text = tokenStringBuilder.ToString().Substring(0, 1);
                                line.Tokens.Add(regularCodeToken);
                                tokenStringBuilder.Remove(0, 1);
                                continue;
                            }
                            else
                            {
                                //code length 2
                                Token regularCodeToken = new Token(line);
                                regularCodeToken.TokenType = TokenType.RegularCode;
                                regularCodeToken.Text = tokenStringBuilder.ToString().Substring(0, 2);
                                line.Tokens.Add(regularCodeToken);
                                tokenStringBuilder.Remove(0, 2);
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

                while (tokenStringBuilder.Length > 0)
                {
                    string symbol0 = "" + tokenStringBuilder[0];
                    if (_nulls.Contains(symbol0))
                    {
                        //null length 1
                        Token nullToken = new Token(lastLine);
                        nullToken.TokenType = TokenType.Null;
                        nullToken.Text = "" + tokenStringBuilder[0];
                        lastLine.Tokens.Add(nullToken);
                        tokenStringBuilder.Remove(0, 1);
                    }
                    else if (tokenStringBuilder.Length == 1)
                    {
                        //code length 1
                        Token regularCodeToken = new Token(lastLine);
                        regularCodeToken.TokenType = TokenType.RegularCode;
                        regularCodeToken.Text = tokenStringBuilder.ToString().Substring(0, 1);
                        lastLine.Tokens.Add(regularCodeToken);
                        tokenStringBuilder.Remove(0, 1);
                    }
                    else
                    {
                        string symbol1 = "" + tokenStringBuilder[1];
                        if (_nulls.Contains(symbol1))
                        {
                            //code length 1
                            Token regularCodeToken = new Token(lastLine);
                            regularCodeToken.TokenType = TokenType.RegularCode;
                            regularCodeToken.Text = tokenStringBuilder.ToString().Substring(0, 1);
                            lastLine.Tokens.Add(regularCodeToken);
                            tokenStringBuilder.Remove(0, 1);
                            //null length 1
                            Token nullToken = new Token(lastLine);
                            nullToken.TokenType = TokenType.Null;
                            nullToken.Text = "" + tokenStringBuilder[0];
                            lastLine.Tokens.Add(nullToken);
                            tokenStringBuilder.Remove(0, 1);
                        }
                        else
                        {
                            //code length 2
                            Token regularCodeToken = new Token(lastLine);
                            regularCodeToken.TokenType = TokenType.RegularCode;
                            regularCodeToken.Text = tokenStringBuilder.ToString().Substring(0, 2);
                            lastLine.Tokens.Add(regularCodeToken);
                            tokenStringBuilder.Remove(0, 2);
                        }
                    }
                }
            }
            return document;
        }
    }

    /// <summary>
    /// Parser for Francia 4-1
    /// </summary>
    public class Francia4Parser : SimpleSingleTokenParser
    {
        private string[] _nulls = new string[] { };

        public Francia4Parser(params string[] nulls)
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
                bool is_vocabulary = false;

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

                        if ("abcdefghij".Contains(symbol))
                        {
                            is_vocabulary = true;
                            continue;
                        }

                        if (tokenStringBuilder.Length == 5)
                        {
                            //vocabulary
                            if ("abcdefghij".Contains("" + tokenStringBuilder[3]))
                            {
                                Token vocabularyToken = new Token(line);
                                vocabularyToken.TokenType = TokenType.VocabularyElement;
                                vocabularyToken.Text = tokenStringBuilder.ToString().Substring(0, tokenStringBuilder.Length - 2);
                                line.Tokens.Add(vocabularyToken);
                                tokenStringBuilder.Remove(0, 3);
                                continue;
                            }
                            else
                            {
                                Token vocabularyToken = new Token(line);
                                vocabularyToken.TokenType = TokenType.VocabularyElement;
                                vocabularyToken.Text = tokenStringBuilder.ToString().Substring(0, tokenStringBuilder.Length - 1);
                                line.Tokens.Add(vocabularyToken);
                                tokenStringBuilder.Remove(0, 4);
                            }
                            //remaining token
                            if (_nulls.Contains(symbol))
                            {
                                Token nullToken = new Token(line);
                                nullToken.TokenType = TokenType.Null;
                                nullToken.Text = "" + symbol;
                                line.Tokens.Add(nullToken);
                                tokenStringBuilder.Clear();
                            }
                            else if ("abcdefghij".Contains(symbol))
                            {
                                continue;
                            }
                            else
                            {
                                Token regularToken = new Token(line);
                                regularToken.TokenType = TokenType.RegularCode;
                                regularToken.Text = "" + symbol;
                                line.Tokens.Add(regularToken);
                                tokenStringBuilder.Clear();
                            }
                            is_vocabulary = false;
                            continue;
                        }

                        if (_nulls.Contains(symbol))
                        {
                            if (tokenStringBuilder.Length == 4 || tokenStringBuilder.Length == 5 && is_vocabulary)
                            {
                                //vocabulary
                                Token vocabularyToken = new Token(line);
                                vocabularyToken.TokenType = TokenType.VocabularyElement;
                                vocabularyToken.Text = tokenStringBuilder.ToString().Substring(0, tokenStringBuilder.Length - 1);
                                line.Tokens.Add(vocabularyToken);

                                //null length 1
                                Token nullToken = new Token(line);
                                nullToken.TokenType = TokenType.Null;
                                nullToken.Text = "" + symbol;
                                line.Tokens.Add(nullToken);

                                tokenStringBuilder.Clear();
                            }
                            else
                            {
                                for (int i = 0; i < tokenStringBuilder.Length - 2; i++)
                                {
                                    //code length 1
                                    Token regularCodeToken = new Token(line);
                                    regularCodeToken.TokenType = TokenType.RegularCode;
                                    regularCodeToken.Text = tokenStringBuilder.ToString().Substring(0, 1);
                                    line.Tokens.Add(regularCodeToken);
                                    tokenStringBuilder.Remove(0, 1);
                                }
                                Token nullToken = new Token(line);
                                nullToken.TokenType = TokenType.Null;
                                nullToken.Text = "" + symbol;
                                line.Tokens.Add(nullToken);

                                tokenStringBuilder.Clear();
                            }
                            is_vocabulary = false;
                            continue;
                        }

                        if (!is_vocabulary)
                        {
                            //code length 1
                            Token regularCodeToken = new Token(line);
                            regularCodeToken.TokenType = TokenType.RegularCode;
                            regularCodeToken.Text = tokenStringBuilder.ToString().Substring(0, 1);
                            line.Tokens.Add(regularCodeToken);
                            tokenStringBuilder.Remove(0, 1);
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

                while (tokenStringBuilder.Length > 0)
                {
                    string symbol0 = "" + tokenStringBuilder[0];
                    if (_nulls.Contains(symbol0))
                    {
                        //null length 1
                        Token nullToken = new Token(lastLine);
                        nullToken.TokenType = TokenType.Null;
                        nullToken.Text = "" + tokenStringBuilder[0];
                        lastLine.Tokens.Add(nullToken);
                        tokenStringBuilder.Remove(0, 1);
                    }
                    else
                    {
                        //code length 1
                        Token regularCodeToken = new Token(lastLine);
                        regularCodeToken.TokenType = TokenType.RegularCode;
                        regularCodeToken.Text = tokenStringBuilder.ToString().Substring(0, 1);
                        lastLine.Tokens.Add(regularCodeToken);
                        tokenStringBuilder.Remove(0, 1);
                    }
                }
            }
            return document;
        }
    }

    /// <summary>
    /// Parser for Francia 6-1
    /// (vocabulary elements not implemented yet!)
    /// </summary>
    public class Francia6Parser : SimpleSingleTokenParser
    {
        private string[] _nulls = new string[] { };

        public Francia6Parser(params string[] nulls)
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

                        if (tokenStringBuilder.Length == 2)
                        {
                            //vocabulary
                            if (tokenStringBuilder[1] == 'c')
                            {
                                //code length 2
                                Token regularCodeToken = new Token(line);
                                regularCodeToken.TokenType = TokenType.RegularCode;
                                regularCodeToken.Text = tokenStringBuilder.ToString().Substring(0, 2);
                                line.Tokens.Add(regularCodeToken);
                                tokenStringBuilder.Remove(0, 2);
                                continue;
                            }
                            else
                            {                              
                                string symbol0 = "" + tokenStringBuilder[0];
                                if (_nulls.Contains(symbol0))
                                {
                                    //null length 1
                                    Token nullToken = new Token(line);
                                    nullToken.TokenType = TokenType.Null;
                                    nullToken.Text = symbol0;
                                    line.Tokens.Add(nullToken);
                                    tokenStringBuilder.Remove(0, 1);
                                }
                                else
                                {
                                    //regular code length 1
                                    Token regularCodeToken = new Token(line);
                                    regularCodeToken.TokenType = TokenType.RegularCode;
                                    regularCodeToken.Text = symbol0;
                                    line.Tokens.Add(regularCodeToken);
                                    tokenStringBuilder.Remove(0, 1);
                                }                                
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

                while (tokenStringBuilder.Length > 0)
                {
                    string symbol0 = "" + tokenStringBuilder[0];
                    if (_nulls.Contains(symbol0))
                    {
                        //null length 1
                        Token nullToken = new Token(lastLine);
                        nullToken.TokenType = TokenType.Null;
                        nullToken.Text = "" + tokenStringBuilder[0];
                        lastLine.Tokens.Add(nullToken);
                        tokenStringBuilder.Remove(0, 1);
                    }
                    else
                    {
                        //code length 1
                        Token regularCodeToken = new Token(lastLine);
                        regularCodeToken.TokenType = TokenType.RegularCode;
                        regularCodeToken.Text = tokenStringBuilder.ToString().Substring(0, 1);
                        lastLine.Tokens.Add(regularCodeToken);
                        tokenStringBuilder.Remove(0, 1);
                    }
                }
            }
            return document;
        }
    }
}
