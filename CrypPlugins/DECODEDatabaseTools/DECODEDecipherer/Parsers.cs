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
using System.Collections.Generic;

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
                token.TokenType = TokenType.Unknown;
                bool top = false;
                bool bottom = false;                
                Symbol symbol = null;

                foreach (var c in trimmedLine)
                {
                    switch (c)
                    {
                        case '^':
                            top = true;
                            continue;
                        case '_':
                            bottom = true;
                            continue;
                        default:
                            if (top && symbol != null)
                            {
                                top = false;
                                symbol.Top = c + "";
                                continue;
                            }
                            if (bottom && symbol != null)
                            {
                                bottom = false;
                                symbol.Bottom = c + "";
                                continue;
                            }
                            symbol = new Symbol(token);
                            symbol.Text = c + "";
                            token.Symbols.Add(symbol);
                            break;
                    }
                }
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
        private List<Token> _nulls = new List<Token>();
        private uint _regularCodeLength = 0;

        public NoVocabularyParser(uint regularCodeLength, List<Token> nulls = null)
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
                TokenBuilder tokenBuilder = new TokenBuilder();
                TokenBuilder tagTokenBuilder = new TokenBuilder();
                Line lastLine = null;

                bool is_a_tag = false;
                foreach (Line line in page.Lines)
                {
                    if (line.LineType == LineType.Comment)
                    {
                        continue;
                    }

                    //We are using the SimpleSingleTokenParser as baseline
                    //Thus, we have a single token with symbols for each line
                    List<Symbol> text = line.Tokens[0].Symbols;
                    line.Tokens.Remove(line.Tokens[0]);

                    for (int position = 0; position < text.Count; position++)
                    {
                        Symbol symbol = text[position];
                        if (string.IsNullOrWhiteSpace(symbol.Text) && !is_a_tag)
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
                            tagToken.Symbols = tagTokenBuilder.ToList();
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
                            if (tokenBuilder.Length > 0)
                            {
                                Token regularCodeToken = new Token(line);
                                regularCodeToken.TokenType = TokenType.RegularCode;
                                regularCodeToken.Symbols = tokenBuilder.ToList();
                                line.Tokens.Add(regularCodeToken);
                                tokenBuilder.Clear();
                            }
                            Token nullToken = new Token(line);
                            nullToken.TokenType = TokenType.Null;
                            nullToken.Symbols.Add(symbol);
                            line.Tokens.Add(nullToken);
                            continue;
                        }
                        //here, we know, we have no null symbol and no whitespace
                        //thus, the symbol belongs to a regular code
                        tokenBuilder.Append(symbol);
                        //finally, check if we reached the regular code length
                        if (_regularCodeLength != 0 && tokenBuilder.Length == _regularCodeLength)
                        {
                            Token regularCodeToken = new Token(line);
                            regularCodeToken.TokenType = TokenType.RegularCode;
                            regularCodeToken.Symbols = tokenBuilder.ToList();
                            line.Tokens.Add(regularCodeToken);
                            tokenBuilder.Clear();
                        }
                    }
                    lastLine = line;
                }

                if (tagTokenBuilder.Length > 0)
                {
                    Token tagToken = new Token(lastLine);
                    tagToken.TokenType = TokenType.Tag;
                    tagToken.Symbols = tagTokenBuilder.ToList();
                    lastLine.Tokens.Add(tagToken);
                    tagTokenBuilder.Clear();
                    is_a_tag = false;
                }
                if (tokenBuilder.Length > 0)
                {
                    Token regularCodeToken = new Token(lastLine);
                    regularCodeToken.TokenType = TokenType.RegularCode;
                    regularCodeToken.Symbols = tokenBuilder.ToList();
                    lastLine.Tokens.Add(regularCodeToken);
                    tokenBuilder.Clear();
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
        private List<Token> _nulls = new List<Token>();

        public Vocabulary3DigitsEndingWithNull1DigitsParser(List<Token> nulls = null)
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
                TokenBuilder tokenBuilder = new TokenBuilder();
                TokenBuilder tagTokenBuilder = new TokenBuilder();
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
                    List<Symbol> text = line.Tokens[0].Symbols;
                    line.Tokens.Remove(line.Tokens[0]);

                    for (int position = 0; position < text.Count; position++)
                    {
                        Symbol symbol = text[position];
                        if (string.IsNullOrWhiteSpace(symbol.Text) && !is_a_tag)
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
                            tagToken.Symbols = tagTokenBuilder.ToList();
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
                            if (tokenBuilder.Length == 3)
                            {
                                //we know, that this is a vocabulary element (length = 3 digits)
                                Token vocabularyToken = new Token(line);
                                vocabularyToken.TokenType = TokenType.VocabularyElement;
                                vocabularyToken.Symbols = tokenBuilder.ToList();
                                line.Tokens.Add(vocabularyToken);
                                tokenBuilder.Clear();
                            }
                            else if (tokenBuilder.Length > 0)
                            {
                                Token regularCodeToken = new Token(line);
                                regularCodeToken.TokenType = TokenType.RegularCode;
                                regularCodeToken.Symbols = tokenBuilder.ToList();
                                line.Tokens.Add(regularCodeToken);
                                tokenBuilder.Clear();
                            }

                            Token nullToken = new Token(line);
                            nullToken.TokenType = TokenType.Null;
                            nullToken.Symbols.Add(symbol);
                            symbol.ParentToken = nullToken;
                            line.Tokens.Add(nullToken);
                            continue;
                        }
                        //here, we know, we have no null symbol and no whitespace
                        //thus, the symbol (maybe) belongs to a regular code or a vocabulary element
                        tokenBuilder.Append(symbol);

                        if (tokenBuilder.Length > 3)
                        {
                            //if we are longer than 3 digits, we know we can not be in a vocabulary, 
                            //thus, we add a regular code
                            Token regularCodeToken = tokenBuilder.GetToken(0, 2, line);
                            regularCodeToken.TokenType = TokenType.RegularCode;                            
                            line.Tokens.Add(regularCodeToken);
                            tokenBuilder.Remove(0, 2);
                        }
                    }
                    lastLine = line;
                }

                if (tagTokenBuilder.Length > 0)
                {
                    Token tagToken = new Token(lastLine);
                    tagToken.TokenType = TokenType.Tag;
                    tagToken.Symbols = tagTokenBuilder.ToList();
                    lastLine.Tokens.Add(tagToken);
                    tagTokenBuilder.Clear();
                    is_a_tag = false;
                }

                while (tokenBuilder.Length > 0)
                {
                    int len = Math.Min(tokenBuilder.Length, 2);
                    Token regularCodeToken = tokenBuilder.GetToken(0, len, lastLine);
                    regularCodeToken.TokenType = TokenType.RegularCode;                    
                    lastLine.Tokens.Add(regularCodeToken);
                    tokenBuilder.Remove(0, len);
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
        private List<Token> _nulls = new List<Token>();

        public Vocabulary3DigitsEndingWithNull2DigitsParser(List<Token> nulls = null)
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
                TokenBuilder tokenBuilder = new TokenBuilder();
                TokenBuilder tagTokenBuilder = new TokenBuilder();
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
                    List<Symbol> text = line.Tokens[0].Symbols;
                    line.Tokens.Remove(line.Tokens[0]);

                    for (int position = 0; position < text.Count; position++)
                    {
                        Symbol symbol = text[position];
                        if (string.IsNullOrWhiteSpace(symbol.Text) && !is_a_tag)
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
                            tagToken.Symbols = tagTokenBuilder.ToList();
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

                        tokenBuilder.Append(symbol);

                        if (tokenBuilder.Length == 5)
                        {
                            if (_nulls.Contains(tokenBuilder.GetToken(3,2,null)))
                            {
                                //we have 2 null symbols => thus, we have a vocabulary element and 2 nulls
                                Token vocabularyToken = tokenBuilder.GetToken(0, 3, line);
                                vocabularyToken.TokenType = TokenType.VocabularyElement;
                                line.Tokens.Add(vocabularyToken);
                                tokenBuilder.Remove(0, 3);

                                Token nullToken = new Token(line);
                                nullToken.TokenType = TokenType.Null;
                                nullToken.Symbols = tokenBuilder.ToList();
                                line.Tokens.Add(nullToken);
                                tokenBuilder.Clear();
                                continue;
                            }
                            else
                            {
                                Token regularCodeToken = tokenBuilder.GetToken(0, 2, line);
                                regularCodeToken.TokenType = TokenType.RegularCode;
                                line.Tokens.Add(regularCodeToken);
                                tokenBuilder.Remove(0, 2);
                                continue;
                            }
                        }
                        if (tokenBuilder.Length == 4)
                        {
                            if (_nulls.Contains(tokenBuilder.GetToken(2, 2, null)))
                            {
                                Token regularCodeToken = tokenBuilder.GetToken(0, 2, line);
                                regularCodeToken.TokenType = TokenType.RegularCode;
                                line.Tokens.Add(regularCodeToken);
                                tokenBuilder.Remove(0, 2);

                                Token nullToken = new Token(line);
                                nullToken.TokenType = TokenType.Null;
                                nullToken.Symbols = tokenBuilder.ToList();
                                line.Tokens.Add(nullToken);
                                tokenBuilder.Clear();
                                continue;
                            }
                        }
                        if (tokenBuilder.Length == 2)
                        {
                            if (_nulls.Contains(tokenBuilder.GetToken(0, 2, null)))
                            {
                                Token nullToken = new Token(line);
                                nullToken.TokenType = TokenType.Null;
                                nullToken.Symbols = tokenBuilder.ToList();
                                line.Tokens.Add(nullToken);
                                tokenBuilder.Clear();
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
                    tagToken.Symbols = tagTokenBuilder.ToList();
                    lastLine.Tokens.Add(tagToken);
                    tagTokenBuilder.Clear();
                    is_a_tag = false;
                }
                while (tokenBuilder.Length > 0)
                {
                    int len = Math.Min(tokenBuilder.Length, 2);
                    Token regularCodeToken = tokenBuilder.GetToken(0, len, lastLine);
                    regularCodeToken.TokenType = TokenType.RegularCode;
                    lastLine.Tokens.Add(regularCodeToken);
                    tokenBuilder.Remove(0, len);
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
        private List<Token> _nulls = new List<Token>();
        private List<Token> _vocabularyPrefix = new List<Token>();

        public Vocabulary4DigitsWithPrefixParser(List<Token> vocabularyPrefix, List<Token> nulls = null)
        {
            if (vocabularyPrefix != null)
            {
                _vocabularyPrefix = vocabularyPrefix;
            }
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
                TokenBuilder tokenBuilder = new TokenBuilder();
                TokenBuilder tagTokenBuilder = new TokenBuilder();
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
                    List<Symbol> text = line.Tokens[0].Symbols;
                    line.Tokens.Remove(line.Tokens[0]);

                    for (int position = 0; position < text.Count; position++)
                    {
                        Symbol symbol = text[position];                        

                        if (string.IsNullOrWhiteSpace(symbol.Text) && !is_a_tag)
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
                            tagToken.Symbols = tagTokenBuilder.ToList();
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

                        tokenBuilder.Append(symbol);

                        if (tokenBuilder.Length == 4)
                        {
                            Symbol symbol0 = tokenBuilder[0];
                            Symbol symbol1 = tokenBuilder[1];

                            if (_vocabularyPrefix.Contains(symbol0))
                            {
                                //vocabulary
                                Token vocabularyToken = tokenBuilder.GetToken(0, 4, line);
                                vocabularyToken.TokenType = TokenType.VocabularyElement;                                
                                line.Tokens.Add(vocabularyToken);
                                tokenBuilder.Clear();
                                continue;

                            }
                            else if (_nulls.Contains(symbol0) && _nulls.Contains(symbol1))
                            {
                                //null length 2
                                Token nullToken = tokenBuilder.GetToken(0, 2, line);
                                nullToken.TokenType = TokenType.Null;
                                line.Tokens.Add(nullToken);
                                tokenBuilder.Remove(0, 2);
                                continue;
                            }
                            else if (_nulls.Contains(symbol0))
                            {
                                //null length 1
                                Token nullToken = tokenBuilder.GetToken(0, 1, line);
                                nullToken.TokenType = TokenType.Null;
                                line.Tokens.Add(nullToken);
                                tokenBuilder.Remove(0, 1);
                                continue;
                            }
                            else if (_nulls.Contains(symbol1) || _vocabularyPrefix.Contains(symbol1))
                            {
                                //code length 1
                                Token regularCodeToken = tokenBuilder.GetToken(0, 1, line);
                                regularCodeToken.TokenType = TokenType.RegularCode;
                                line.Tokens.Add(regularCodeToken);
                                tokenBuilder.Remove(0, 1);
                                continue;
                            }
                            else
                            {
                                //code length 2
                                Token regularCodeToken = tokenBuilder.GetToken(0, 2, line);
                                regularCodeToken.TokenType = TokenType.RegularCode;
                                line.Tokens.Add(regularCodeToken);
                                tokenBuilder.Remove(0, 2);
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
                    tagToken.Symbols = tagTokenBuilder.ToList();
                    lastLine.Tokens.Add(tagToken);
                    tagTokenBuilder.Clear();
                    is_a_tag = false;
                }

                while (tokenBuilder.Length > 0)
                {
                    Symbol symbol0 = tokenBuilder[0];
                    if (_nulls.Contains(symbol0))
                    {
                        //null length 1
                        Token nullToken = tokenBuilder.GetToken(0, 1, lastLine);
                        nullToken.TokenType = TokenType.Null;
                        lastLine.Tokens.Add(nullToken);
                        tokenBuilder.Remove(0, 1);
                    }
                    else if (tokenBuilder.Length == 1)
                    {
                        //code length 1
                        Token regularCodeToken = tokenBuilder.GetToken(0, 1, lastLine);
                        regularCodeToken.TokenType = TokenType.RegularCode;
                        lastLine.Tokens.Add(regularCodeToken);
                        tokenBuilder.Remove(0, 1);
                    }
                    else
                    {
                        Symbol symbol1 = tokenBuilder[1];
                        if (_nulls.Contains(symbol1))
                        {
                            //code length 1
                            Token regularCodeToken =tokenBuilder.GetToken(0, 1, lastLine);
                            regularCodeToken.TokenType = TokenType.RegularCode;
                            lastLine.Tokens.Add(regularCodeToken);
                            tokenBuilder.Remove(0, 1);
                            //null length 1
                            Token nullToken = tokenBuilder.GetToken(0, 1, lastLine);
                            nullToken.TokenType = TokenType.Null;
                            lastLine.Tokens.Add(nullToken);
                            tokenBuilder.Remove(0, 1);
                        }
                        else
                        {
                            //code length 2
                            Token regularCodeToken = tokenBuilder.GetToken(0, 2, lastLine);
                            regularCodeToken.TokenType = TokenType.RegularCode;
                            lastLine.Tokens.Add(regularCodeToken);
                            tokenBuilder.Remove(0, 2);
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
        private List<Token> _nulls = new List<Token>();

        public Francia4Parser(List<Token> nulls)
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

            //create new tokens based on the "old" tokens
            TokenBuilder tokenBuilder = new TokenBuilder();
            TokenBuilder tagTokenBuilder = new TokenBuilder();
            Line lastLine = null;

            bool is_a_tag = false;
            bool is_vocabulary = false;

            foreach (Page page in document.Pages)
            {
                foreach (Line line in page.Lines)
                {
                    if (line.LineType == LineType.Comment)
                    {
                        continue;
                    }

                    //We are using the SimpleSingleTokenParser as baseline
                    //Thus, we have a single token for each line
                    List<Symbol> text = line.Tokens[0].Symbols;
                    line.Tokens.Remove(line.Tokens[0]);

                    for (int position = 0; position < text.Count; position++)
                    {
                        Symbol symbol = text[position];
                        symbol.TopChangesSymbol = false;

                        if (string.IsNullOrWhiteSpace(symbol.Text) && !is_a_tag)
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
                            tagToken.Symbols = tagTokenBuilder.ToList();
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

                        tokenBuilder.Append(symbol);

                        if (!is_vocabulary)
                        {
                            if (_nulls.Contains(symbol))
                            {
                                //null symbol
                                Token nullToken = tokenBuilder.GetToken(0, 1, line);
                                nullToken.TokenType = TokenType.Null;
                                line.Tokens.Add(nullToken);
                                tokenBuilder.Clear();
                            }
                            else
                            {
                                //regular code
                                Token regularCodeToken = tokenBuilder.GetToken(0, 1, line);
                                regularCodeToken.TokenType = TokenType.RegularCode;
                                line.Tokens.Add(regularCodeToken);
                                tokenBuilder.Clear();
                            }
                        }

                        if (is_vocabulary && tokenBuilder.Length == 3)
                        {
                            //vocabulary 3
                            if (!_nulls.Contains(tokenBuilder[2]))
                            {
                                Token vocabularyToken = tokenBuilder.GetToken(0, 3, line);
                                vocabularyToken.TokenType = TokenType.VocabularyElement;
                                line.Tokens.Add(vocabularyToken);
                                tokenBuilder.Clear();                                
                            }
                            //vocabulary 2 with null
                            else if (_nulls.Contains(tokenBuilder[2]))
                            {
                                Token vocabularyToken = tokenBuilder.GetToken(0, 2, line);
                                vocabularyToken.TokenType = TokenType.VocabularyElement;
                                line.Tokens.Add(vocabularyToken);
                                tokenBuilder.Remove(0, 2);
                                Token nullToken = tokenBuilder.GetToken(0, 1, line);
                                nullToken.TokenType = TokenType.Null;
                                line.Tokens.Add(nullToken);
                                tokenBuilder.Clear();
                            }
                            else
                            {
                                tokenBuilder.Clear();
                            }
                            is_vocabulary = false;
                        }

                        if (symbol.Top.Equals("."))
                        {
                            is_vocabulary = true;
                        }

                    }
                    lastLine = line;
                }

                if (tagTokenBuilder.Length > 0)
                {
                    Token tagToken = new Token(lastLine);
                    tagToken.TokenType = TokenType.Tag;
                    tagToken.Symbols = tagTokenBuilder.ToList();
                    lastLine.Tokens.Add(tagToken);
                    tagTokenBuilder.Clear();
                    is_a_tag = false;
                }

                while (tokenBuilder.Length > 0)
                {
                    Symbol symbol0 = tokenBuilder[0];
                    if (_nulls.Contains(symbol0))
                    {
                        //null length 1
                        Token nullToken = tokenBuilder.GetToken(0, tokenBuilder.Length - 1, lastLine);
                        nullToken.TokenType = TokenType.Null;
                        lastLine.Tokens.Add(nullToken);
                        tokenBuilder.Remove(0, 1);
                    }
                    else
                    {
                        //code length 1
                        Token regularCodeToken = tokenBuilder.GetToken(0, tokenBuilder.Length - 1, lastLine);
                        regularCodeToken.TokenType = TokenType.RegularCode;
                        lastLine.Tokens.Add(regularCodeToken);
                        tokenBuilder.Remove(0, 1);
                    }
                }
            }
            return document;
        }
    }

    /// <summary>
    /// Parser for Francia 6-1
    /// </summary>
    public class Francia6Parser : SimpleSingleTokenParser
    {
        private List<Token> _nulls = new List<Token>();

        public Francia6Parser(List<Token> nulls)
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
                TokenBuilder tokenBuilder = new TokenBuilder();
                TokenBuilder tagTokenBuilder = new TokenBuilder();
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
                    List<Symbol> text = line.Tokens[0].Symbols;
                    line.Tokens.Remove(line.Tokens[0]);

                    for (int position = 0; position < text.Count; position++)
                    {
                        Symbol symbol = text[position];
                        if (string.IsNullOrWhiteSpace(symbol.Text) && !is_a_tag)
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
                            tagToken.Symbols = tagTokenBuilder.ToList();
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

                        tokenBuilder.Append(symbol);                        

                        if (tokenBuilder.Length == 4)
                        {
                            if(tokenBuilder[0].Top == string.Empty &&
                                tokenBuilder[1].Top == string.Empty &&
                                tokenBuilder[2].Top == string.Empty &&
                                tokenBuilder[3].Top == string.Empty &&
                                _nulls.Contains(tokenBuilder[2]) &&
                                _nulls.Contains(tokenBuilder[3]))
                            {
                                //vocabulary element
                                Token vocabularyToken = tokenBuilder.GetToken(0, 4, line);
                                vocabularyToken.TokenType = TokenType.VocabularyElement;
                                line.Tokens.Add(vocabularyToken);
                                tokenBuilder.Remove(0, 4);
                                continue;
                            }                                                                                 

                            if (tokenBuilder[1].Text.Equals("2") && tokenBuilder[1].Top != null && tokenBuilder[1].Top.Equals("."))
                            {
                                //code length 2
                                Token regularCodeToken = tokenBuilder.GetToken(0, 2, line);
                                regularCodeToken.TokenType = TokenType.RegularCode;
                                line.Tokens.Add(regularCodeToken);
                                tokenBuilder.Remove(0, 2);
                                continue;
                            }
                            else
                            {                              
                                Symbol symbol0 = tokenBuilder[0];
                                if (_nulls.Contains(symbol0))
                                {
                                    //null length 1
                                    Token nullToken = tokenBuilder.GetToken(0, 1, line);
                                    nullToken.TokenType = TokenType.Null;
                                    line.Tokens.Add(nullToken);
                                    tokenBuilder.Remove(0, 1);
                                }
                                else
                                {
                                    //regular code length 1
                                    Token regularCodeToken = tokenBuilder.GetToken(0, 1, line);
                                    regularCodeToken.TokenType = TokenType.RegularCode;
                                    line.Tokens.Add(regularCodeToken);
                                    tokenBuilder.Remove(0, 1);
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
                    tagToken.Symbols = tagTokenBuilder.ToList();
                    lastLine.Tokens.Add(tagToken);
                    tagTokenBuilder.Clear();
                    is_a_tag = false;
                }

                while (tokenBuilder.Length > 0)
                {
                    Symbol symbol0 = tokenBuilder[0];
                    if (_nulls.Contains(symbol0))
                    {
                        //null length 1
                        Token nullToken = tokenBuilder.GetToken(0, 1, lastLine);
                        nullToken.TokenType = TokenType.Null;
                        lastLine.Tokens.Add(nullToken);
                        tokenBuilder.Remove(0, 1);
                    }
                    else
                    {
                        //code length 1
                        Token regularCodeToken = tokenBuilder.GetToken(0, 1, lastLine);
                        regularCodeToken.TokenType = TokenType.RegularCode;
                        lastLine.Tokens.Add(regularCodeToken);
                        tokenBuilder.Remove(0, 1);
                    }
                }
            }
            return document;
        }
    }

    /// <summary>
    /// Parser for Francia 17-1
    /// </summary>
    public class Francia17Parser : SimpleSingleTokenParser
    {
        private List<Token> _nulls = new List<Token>();

        private List<Token> _evenDigits = new List<Token>();
        private List<Token> _oddDigits = new List<Token>();

        private List<Token> _specialSet = new List<Token>();

        public Francia17Parser(List<Token> nulls)
        {
            if (nulls != null)
            {
                _nulls = nulls;
            }

            for (int i = 0; i < 10; i++)
            {

                Token token = new Token(null);
                Symbol symbol = new Symbol(token);
                token.Symbols.Add(symbol);
                symbol.Text = "" + i;
                if (i % 2 == 0)
                {
                    _evenDigits.Add(token);
                }
                else
                {
                    _oddDigits.Add(token);
                }
            }

            Token plusToken = new Token(null);
            Symbol plusSymbol = new Symbol(plusToken);
            plusSymbol.Text = "+";
            plusToken.Symbols.Add(plusSymbol);
            _specialSet.Add(plusToken);

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
                TokenBuilder tokenBuilder = new TokenBuilder();
                TokenBuilder tagTokenBuilder = new TokenBuilder();
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
                    List<Symbol> text = line.Tokens[0].Symbols;
                    line.Tokens.Remove(line.Tokens[0]);

                    for (int position = 0; position < text.Count; position++)
                    {
                        Symbol symbol = text[position];
                        symbol.TopChangesSymbol = false;

                        if (string.IsNullOrWhiteSpace(symbol.Text) && !is_a_tag)
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
                            tagToken.Symbols = tagTokenBuilder.ToList();
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

                        tokenBuilder.Append(symbol);

                        if (tokenBuilder.Length >= 2)
                        {

                            Symbol symbol0 = tokenBuilder[0];
                            Symbol symbol1 = tokenBuilder[1];

                            // 11
                            if (_nulls.Contains(symbol0) && !symbol0.Top.Equals(".") && _nulls.Contains(symbol1))
                            {
                                //null length 1
                                Token nullToken = tokenBuilder.GetToken(0, 1, line);
                                nullToken.TokenType = TokenType.Null;
                                line.Tokens.Add(nullToken);
                                tokenBuilder.Remove(0, 1);
                                continue;
                            }

                            // 13^
                            if (_nulls.Contains(symbol0) && !symbol0.Top.Equals(".") && symbol1.Top.Equals("."))
                            {
                                //null length 1
                                Token nullToken = tokenBuilder.GetToken(0, 1, line);
                                nullToken.TokenType = TokenType.Null;
                                line.Tokens.Add(nullToken);
                                tokenBuilder.Remove(0, 1);
                                continue;
                            }
                            // 33^
                            if (_oddDigits.Contains(symbol0) && symbol1.Top.Equals("."))
                            {
                                //code length 1
                                Token regularCodeToken = tokenBuilder.GetToken(0, 1, line);
                                regularCodeToken.TokenType = TokenType.RegularCode;
                                line.Tokens.Add(regularCodeToken);
                                tokenBuilder.Remove(0, 1);
                                continue;
                            }
                        }
                        
                        if (tokenBuilder.Length >= 4)
                        {

                            Symbol symbol0 = tokenBuilder[0];
                            Symbol symbol1 = tokenBuilder[1];
                            Symbol symbol2 = tokenBuilder[2];
                            Symbol symbol3 = tokenBuilder[3];

                            // 1^.7
                            if (_nulls.Contains(symbol0) && symbol0.Top.Equals(".") && _oddDigits.Contains(symbol1))                                
                            {
                                //code length 2
                                Token regularCodeToken = tokenBuilder.GetToken(0, 2, line);
                                regularCodeToken.TokenType = TokenType.RegularCode;
                                line.Tokens.Add(regularCodeToken);
                                tokenBuilder.Remove(0, 2);
                                continue;
                            }
                                
                            // 1 *
                            if (_nulls.Contains(symbol0) && !symbol0.Top.Equals("."))
                            {
                                //null length 1
                                Token nullToken = tokenBuilder.GetToken(0, 1, line);
                                nullToken.TokenType = TokenType.Null;
                                line.Tokens.Add(nullToken);
                                tokenBuilder.Remove(0, 1);
                                continue;
                            }

                            // 266+
                            if ((_evenDigits.Contains(symbol1) || _evenDigits.Contains(symbol2)) && _specialSet.Contains(symbol3)
                                && !symbol0.Top.Equals(".")
                                && !symbol1.Top.Equals(".")
                                && !symbol2.Top.Equals(".")
                                && !symbol3.Top.Equals("."))
                            {
                                //vocabulary length 4
                                Token vocabularyToken = tokenBuilder.GetToken(0, 4, line);
                                vocabularyToken.TokenType = TokenType.VocabularyElement;
                                line.Tokens.Add(vocabularyToken);
                                tokenBuilder.Remove(0, 4);
                                continue;
                            }

                            // 266
                            if ((_evenDigits.Contains(symbol1) || _evenDigits.Contains(symbol2)) && !_specialSet.Contains(symbol3)
                                && !symbol0.Top.Equals(".")
                                && !symbol1.Top.Equals(".")
                                && !symbol2.Top.Equals(".")
                                && !symbol3.Top.Equals("."))
                            {
                                //vocabulary length 3
                                Token vocabularyToken = tokenBuilder.GetToken(0, 3, line);
                                vocabularyToken.TokenType = TokenType.VocabularyElement;
                                line.Tokens.Add(vocabularyToken);
                                tokenBuilder.Remove(0, 3);
                                continue;
                            }

                            // 3^.7 or 4^.1
                            if (symbol0.Top.Contains(".") && _oddDigits.Contains(symbol1))
                            {                            
                                //code length 2
                                Token regularCodeToken = tokenBuilder.GetToken(0, 2, line);
                                regularCodeToken.TokenType = TokenType.RegularCode;
                                line.Tokens.Add(regularCodeToken);
                                tokenBuilder.Remove(0, 2);
                                continue;
                            }

                            // 3
                            if (_oddDigits.Contains(symbol0))
                            {
                                //code length 1
                                Token regularCodeToken = tokenBuilder.GetToken(0, 1, line);
                                regularCodeToken.TokenType = TokenType.RegularCode;
                                line.Tokens.Add(regularCodeToken);
                                tokenBuilder.Remove(0, 1);
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
                    tagToken.Symbols = tagTokenBuilder.ToList();
                    lastLine.Tokens.Add(tagToken);
                    tagTokenBuilder.Clear();
                    is_a_tag = false;
                }

                while (tokenBuilder.Length > 0)
                {
                    Symbol symbol0 = tokenBuilder[0];
                    if (_nulls.Contains(symbol0))
                    {
                        //null length 1
                        Token nullToken = tokenBuilder.GetToken(0, 1, lastLine);
                        nullToken.TokenType = TokenType.Null;
                        lastLine.Tokens.Add(nullToken);
                        tokenBuilder.Remove(0, 1);
                    }
                    else
                    {
                        if (tokenBuilder[0].Top.Equals("."))
                        {
                            //code length 2
                            Token regularCodeToken = tokenBuilder.GetToken(0, 2, lastLine);
                            regularCodeToken.TokenType = TokenType.RegularCode;
                            lastLine.Tokens.Add(regularCodeToken);
                            tokenBuilder.Remove(0, 2);
                        }
                        else
                        {
                            //code length 1
                            Token regularCodeToken = tokenBuilder.GetToken(0, 1, lastLine);
                            regularCodeToken.TokenType = TokenType.RegularCode;
                            lastLine.Tokens.Add(regularCodeToken);
                            tokenBuilder.Remove(0, 1);
                        }
                    }
                }
            }
            return document;
        }
    }
}
