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
using System.Collections.Generic;

namespace Cryptool.Plugins.DECODEDatabaseTools.Util
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
            ShowCommentsPlaintextCleartext = false;
        }

        /// <summary>
        /// If this is enabled, plaintext, cleartext, and comments are shown in output
        /// </summary>
        public bool ShowCommentsPlaintextCleartext
        {
            get;
            set;
        }

        /// <summary>
        /// If this is enabled, unknown transcription symbols are shown
        /// </summary>
        public bool ShowUnknownTranscriptionSymbols
        {
            get;
            set;
        }

        /// <summary>
        /// Text Document to parse
        /// </summary>
        public string DECODETextDocument
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the name of this Parser as string
        /// </summary>
        public string ParserName
        {
            get;
            protected set;
        }

        /// <summary>
        /// Returns the parsed TextDocument
        /// </summary>
        /// <returns></returns>
        public abstract TextDocument GetTextDocument();

        /// <summary>
        /// Returns the possible parser parameters for the automatic parser test
        /// </summary>
        /// <returns></returns>
        public abstract PossibleParserParameters GetPossibleParserParameters();
        
        /// <summary>
        /// Event that allows the parsers to log to the CT2 gui
        /// </summary>
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        /// <summary>
        /// Cleans the document before the parsing:
        /// - removes empty lines (only if ShowCommentsPlaintextCleartext is disabled)
        /// - removes empty pages
        /// - recalculates line numbering
        /// - removes plaintext, cleartext, and comments based on the settings (ShowCommentsPlaintextCleartext)
        /// - removes unknownTokens if enabled
        /// </summary>
        /// <param name="document"></param>
        public void CleanupDocument(TextDocument document)
        {
            if (!ShowCommentsPlaintextCleartext)
            {
                //remove all tag tokens
                foreach (var page in document.Pages)
                {
                    List<Line> removeLineList = new List<Line>();
                    foreach (var line in page.Lines)
                    {
                        List<Token> removeTokenList = new List<Token>();
                        foreach (var token in line.Tokens)
                        {
                            if (token.TokenType == TokenType.Tag)
                            {
                                removeTokenList.Add(token);
                            }
                            else if (!ShowUnknownTranscriptionSymbols)
                            {
                                bool removeToken = false;
                                foreach (var symbol in token.Symbols)
                                {
                                    //we remove tokens which contain at least one question mark
                                    if (!string.IsNullOrEmpty(symbol.Text) && symbol.Text.Equals("?"))
                                    {
                                        removeToken = true;
                                        break;
                                    }
                                }
                                if (removeToken)
                                {
                                    removeTokenList.Add(token);
                                }
                            }
                        }
                        foreach (var token in removeTokenList)
                        {
                            line.Tokens.Remove(token);
                        }
                        if (line.Tokens.Count == 0)
                        {
                            removeLineList.Add(line);
                        }
                    }
                    foreach (var line in removeLineList)
                    {
                        page.Lines.Remove(line);
                    }
                    //fix line numbering
                    int lineCounter = 1;
                    foreach (var line in page.Lines)
                    {
                        line.LineNumber = lineCounter;
                        lineCounter++;
                    }
                }
            }

            //here, we remove empty pages
            List<Page> removePageList = new List<Page>();
            foreach (var page in document.Pages)
            {
                bool remove = true;
                foreach (var line in page.Lines)
                {
                    if (line.Tokens.Count > 0)
                    {
                        foreach (var token in line.Tokens)
                        {
                            if (token.TokenType != TokenType.Tag)
                            {
                                remove = false;
                            }
                            if (!remove)
                            {
                                break;
                            }
                        }
                    }
                    if (!remove)
                    {
                        break;
                    }
                }
                if (remove)
                {
                    removePageList.Add(page);
                }
            }
            foreach (var page in removePageList)
            {
                document.Pages.Remove(page);
            }
            //fix page numbering
            for (int i = 0; i < document.Pages.Count; i++)
            {
                document.Pages[i].PageNumber = i + 1;
            }
        }

        /// <summary>
        /// Helper method to invoke a gui log
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logLevel"></param>
        protected void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, null, new GuiLogEventArgs(message, null, logLevel));
        }
    }

    /// <summary>
    /// This class contains all possible prefixes and nulls for a dedicated parser
    /// and can be used to iterate over all combinations
    /// </summary>
    public class PossibleParserParameters
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maximumNumberOfNulls"></param>
        public PossibleParserParameters(int maximumNumberOfNulls)
        {
            MaximumNumberOfNulls = maximumNumberOfNulls;
        }

        /// <summary>
        /// A list of possible null tokens
        /// </summary>
        public readonly List<Token> PossibleNulls = new List<Token>();

        /// <summary>
        /// A list of possible nomenclature prefixes
        /// </summary>
        public readonly List<Token> PossiblePrefixes = new List<Token>();      
        
        /// <summary>
        /// Maximum number of nulls that may occur in the ciphertext
        /// </summary>
        public int MaximumNumberOfNulls
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the number of combinations for all possible settings
        /// </summary>
        /// <returns></returns>
        public int GetNumberOfCombinations()
        {
            int combinations = 1;
            for(int numberOfNulls = MaximumNumberOfNulls; numberOfNulls >= 0; numberOfNulls--)
            {
                combinations += Combinations(PossibleNulls.Count, numberOfNulls) * PossiblePrefixes.Count;
            }
            return combinations;
        }

        /// <summary>
        /// Computes combinations
        /// </summary>
        /// <param name="number"></param>
        /// <param name="selection"></param>
        /// <returns></returns>
        private int Combinations(int number, int selection)
        {
            int result = 1;
            for (int i = 0; i < selection; i++)
            {
                result = result * number;
                number--;
            }
            return result;
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
            ParserName = GetType().Name;
        }

        /// <summary>
        /// Returns a parsed document
        /// </summary>
        /// <returns></returns>
        public override TextDocument GetTextDocument()
        {
            if (string.IsNullOrEmpty(DECODETextDocument))
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
                        if (split.Length < 2)
                        {
                            GuiLogMessage(string.Format("Wrong catalog name definition in document: {0}", trimmedLine), NotificationLevel.Warning);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(document.CatalogName))
                            {
                                GuiLogMessage("Catalog name is defined twice in this document. Ignoring second definition", NotificationLevel.Warning);
                            }
                            else
                            {
                                for (int i = 1; i < split.Length; i++)
                                {
                                    document.CatalogName += split[i].Trim();
                                    if (i < split.Length - 2)
                                    {
                                        document.CatalogName += " ";
                                    }
                                }
                            }
                        }
                    }
                    else if (comment.StartsWith("IMAGE NAME"))
                    {
                        var split = trimmedLine.Split(':');
                        if (split.Length != 2)
                        {
                            GuiLogMessage(string.Format("Wrong image name definition in document: {0}", trimmedLine), NotificationLevel.Warning);
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
                            GuiLogMessage(string.Format("Wrong transcriber name definition in document: {0}", trimmedLine), NotificationLevel.Warning);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(document.TranscriberName))
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
                            GuiLogMessage(string.Format("Wrong date of transcription definition in document: {0}", trimmedLine), NotificationLevel.Warning);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(document.DateOfTranscription))
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
                            GuiLogMessage(string.Format("Wrong transcription time definition in document: {0}", trimmedLine), NotificationLevel.Warning);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(document.TranscriptionTime))
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
                            GuiLogMessage(string.Format("Wrong comments definition in document: {0}", trimmedLine), NotificationLevel.Warning);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(document.Comments))
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
                    if((char.IsWhiteSpace(c) || c == '\r' || c == '\n') && (top || bottom))
                    {
                        //we don't put whitespaces or linebreaks to top or bottom text
                        continue;
                    }
                    switch (c)
                    {
                        case '^':
                            if (bottom == true)
                            {
                                if (string.IsNullOrEmpty(symbol.Bottom))
                                {
                                    symbol.Bottom = "^";
                                }
                                else
                                {
                                    symbol.Bottom += "^";
                                }
                                top = false;
                            }
                            else if (top == true)
                            {
                                if (string.IsNullOrEmpty(symbol.Top))
                                {
                                    symbol.Top = "^";
                                }
                                else
                                {
                                    symbol.Top += "^";
                                }
                                top = false;
                            }
                            else
                            {
                                top = true;
                            }
                            continue;
                        case '_':
                            if (bottom == true)
                            {
                                if (string.IsNullOrEmpty(symbol.Bottom))
                                {
                                    symbol.Bottom = "_";
                                }
                                else
                                {
                                    symbol.Bottom += "_";
                                }
                                bottom = false;
                            }
                            else if (top == true)
                            {
                                if (string.IsNullOrEmpty(symbol.Top))
                                {
                                    symbol.Top = "_";
                                }
                                else
                                {
                                    symbol.Top += "_";
                                }
                                bottom = false;
                            }
                            else
                            {
                                bottom = true;
                            }
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
                if(!ShowCommentsPlaintextCleartext && currentLine.LineType == LineType.Comment)
                {
                    //we don't add comments to the page, thus we remove this line
                    linenumber--;
                    continue;
                }
                currentPage.Lines.Add(currentLine);
            }

            //check, if header fields are set; if not, set these to "undefined"
            if (string.IsNullOrEmpty(document.CatalogName))
            {
                document.CatalogName = "undefined";
            }
            if (string.IsNullOrEmpty(document.ImageName))
            {
                document.ImageName = "undefined";
            }
            if (string.IsNullOrEmpty(document.TranscriberName))
            {
                document.TranscriberName = "undefined";
            }
            if (string.IsNullOrEmpty(document.DateOfTranscription))
            {
                document.DateOfTranscription = "undefined";
            }
            if (string.IsNullOrEmpty(document.TranscriptionTime))
            {
                document.TranscriptionTime = "undefined";
            }
            if (string.IsNullOrEmpty(document.TranscriptionMethod))
            {
                document.TranscriptionMethod = "undefined";
            }
            if (string.IsNullOrEmpty(document.Comments))
            {
                document.Comments = "undefined";
            }

            return document;
        }

        /// <summary>
        /// Returns default empty PossibleParserParameters
        /// </summary>
        /// <returns></returns>
        public override PossibleParserParameters GetPossibleParserParameters()
        {
            return null;
        }
    }

    /// <summary>
    /// Simple parser that just splits the text into regular elements of defined length
    /// Also supports "nulls"
    /// </summary>
    public class NoNomenclatureParser : SimpleSingleTokenParser
    {
        private List<Token> _nulls = new List<Token>();
        private uint _regularElementLength = 0;

        public NoNomenclatureParser(uint regularElementLength, List<Token> nulls = null)
        {
            ParserName = GetType().Name;
            if (nulls != null)
            {
                _nulls = nulls;
            }
            _regularElementLength = regularElementLength;
        }

        /// <summary>
        /// Returns the parsed new document
        /// </summary>
        /// <returns></returns>
        public override TextDocument GetTextDocument()
        {
            TextDocument document = base.GetTextDocument();
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
                                Token regularElementToken = new Token(line);
                                regularElementToken.TokenType = TokenType.RegularElement;
                                regularElementToken.Symbols = tokenBuilder.ToList();
                                line.Tokens.Add(regularElementToken);
                                tokenBuilder.Clear();
                            }
                            Token nullToken = new Token(line);
                            nullToken.TokenType = TokenType.NullElement;
                            nullToken.Symbols.Add(symbol);
                            line.Tokens.Add(nullToken);
                            continue;
                        }
                        //here, we know, we have no null symbol and no whitespace
                        //thus, the symbol belongs to a regular element
                        tokenBuilder.Append(symbol);
                        //finally, check if we reached the regular element length
                        if (_regularElementLength != 0 && tokenBuilder.Length == _regularElementLength)
                        {
                            Token regularElementToken = new Token(line);
                            regularElementToken.TokenType = TokenType.RegularElement;
                            regularElementToken.Symbols = tokenBuilder.ToList();
                            line.Tokens.Add(regularElementToken);
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
                    Token regularElementToken = new Token(lastLine);
                    regularElementToken.TokenType = TokenType.RegularElement;
                    regularElementToken.Symbols = tokenBuilder.ToList();
                    lastLine.Tokens.Add(regularElementToken);
                    tokenBuilder.Clear();
                }
            }

            return document;
        }

        /// <summary>
        /// Returns possible parser parameters for this Parser
        /// </summary>
        /// <returns></returns>
        public override PossibleParserParameters GetPossibleParserParameters()
        {
            PossibleParserParameters possibleParserParameters = new PossibleParserParameters(1);
            //add all digits from 0 to 9 as possible null symbols
            for (int i = 0; i < 10; i++)
            {
                Token nullToken = new Token(null);
                Symbol nullSymbol = new Symbol(nullToken);
                nullSymbol.Text = i.ToString();
                nullToken.Symbols.Add(nullSymbol);
                possibleParserParameters.PossibleNulls.Add(nullToken);
            }
            return possibleParserParameters;
        }
    }

    /// <summary>
    /// Parses the text into regular elements of two
    /// Also supports nomenclature elements of three digits followed by a null digit
    /// </summary>
    public class Nomenclature3DigitsEndingWithNull1DigitsParser : SimpleSingleTokenParser
    {
        private List<Token> _nulls = new List<Token>();

        public Nomenclature3DigitsEndingWithNull1DigitsParser(List<Token> nulls = null)
        {
            ParserName = GetType().Name;
            if (nulls != null)
            {
                _nulls = nulls;
            }
        }

        /// <summary>
        /// Returns the parsed new document
        /// </summary>
        /// <returns></returns>
        public override TextDocument GetTextDocument()
        {
            TextDocument document = base.GetTextDocument();
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
                                //we know, that this is a nomenclature element (length = 3 digits)
                                Token nomenclatureToken = new Token(line);
                                nomenclatureToken.TokenType = TokenType.NomenclatureElement;
                                nomenclatureToken.Symbols = tokenBuilder.ToList();
                                line.Tokens.Add(nomenclatureToken);
                                tokenBuilder.Clear();
                            }
                            else if (tokenBuilder.Length > 0)
                            {
                                Token regularElementToken = new Token(line);
                                regularElementToken.TokenType = TokenType.RegularElement;
                                regularElementToken.Symbols = tokenBuilder.ToList();
                                line.Tokens.Add(regularElementToken);
                                tokenBuilder.Clear();
                            }

                            Token nullToken = new Token(line);
                            nullToken.TokenType = TokenType.NullElement;
                            nullToken.Symbols.Add(symbol);
                            symbol.ParentToken = nullToken;
                            line.Tokens.Add(nullToken);
                            continue;
                        }
                        //here, we know, we have no null symbol and no whitespace
                        //thus, the symbol (maybe) belongs to a regular element or a nomenclature element
                        tokenBuilder.Append(symbol);

                        if (tokenBuilder.Length > 3)
                        {
                            //if we are longer than 3 digits, we know we can not be in a nomenclature, 
                            //thus, we add a regular element
                            Token regularElementToken = tokenBuilder.GetToken(0, 2, line);
                            regularElementToken.TokenType = TokenType.RegularElement;
                            line.Tokens.Add(regularElementToken);
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
                    Token regularElementToken = tokenBuilder.GetToken(0, len, lastLine);
                    regularElementToken.TokenType = TokenType.RegularElement;
                    lastLine.Tokens.Add(regularElementToken);
                    tokenBuilder.Remove(0, len);
                }
            }
            return document;
        }

        /// <summary>
        /// Returns possible parser parameters for this Parser
        /// </summary>
        /// <returns></returns>
        public override PossibleParserParameters GetPossibleParserParameters()
        {
            PossibleParserParameters possibleParserParameters = new PossibleParserParameters(2);
            //add all digits from 0 to 9 as possible null symbols
            for (int i = 0; i < 10; i++)
            {
                Token nullToken = new Token(null);
                Symbol nullSymbol = new Symbol(nullToken);
                nullSymbol.Text = i.ToString();
                nullToken.Symbols.Add(nullSymbol);
                possibleParserParameters.PossibleNulls.Add(nullToken);
            }
            return possibleParserParameters;
        }
    }

    /// <summary>
    /// Parses the text into regular elements of two
    /// Also supports nomenclature elements of three digits followed by two nulls
    /// </summary>
    public class Nomenclature3DigitsEndingWithNull2DigitsParser : SimpleSingleTokenParser
    {
        private List<Token> _nulls = new List<Token>();

        public Nomenclature3DigitsEndingWithNull2DigitsParser(List<Token> nulls = null)
        {
            ParserName = GetType().Name;
            if (nulls != null)
            {
                _nulls = nulls;
            }
        }

        /// <summary>
        /// Returns the parsed new document
        /// </summary>
        /// <returns></returns>
        public override TextDocument GetTextDocument()
        {
            TextDocument document = base.GetTextDocument();
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
                            if (_nulls.Contains(tokenBuilder.GetToken(3, 2, null)))
                            {
                                //we have 2 null symbols => thus, we have a nomenclature element and 2 nulls
                                Token nomenclatureToken = tokenBuilder.GetToken(0, 3, line);
                                nomenclatureToken.TokenType = TokenType.NomenclatureElement;
                                line.Tokens.Add(nomenclatureToken);
                                tokenBuilder.Remove(0, 3);

                                Token nullToken = new Token(line);
                                nullToken.TokenType = TokenType.NullElement;
                                nullToken.Symbols = tokenBuilder.ToList();
                                line.Tokens.Add(nullToken);
                                tokenBuilder.Clear();
                                continue;
                            }
                            else
                            {
                                Token regularElementToken = tokenBuilder.GetToken(0, 2, line);
                                regularElementToken.TokenType = TokenType.RegularElement;
                                line.Tokens.Add(regularElementToken);
                                tokenBuilder.Remove(0, 2);
                                continue;
                            }
                        }
                        if (tokenBuilder.Length == 4)
                        {
                            if (_nulls.Contains(tokenBuilder.GetToken(2, 2, null)))
                            {
                                Token regularElementToken = tokenBuilder.GetToken(0, 2, line);
                                regularElementToken.TokenType = TokenType.RegularElement;
                                line.Tokens.Add(regularElementToken);
                                tokenBuilder.Remove(0, 2);

                                Token nullToken = new Token(line);
                                nullToken.TokenType = TokenType.NullElement;
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
                                nullToken.TokenType = TokenType.NullElement;
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
                    Token regularElementToken = tokenBuilder.GetToken(0, len, lastLine);
                    regularElementToken.TokenType = TokenType.RegularElement;
                    lastLine.Tokens.Add(regularElementToken);
                    tokenBuilder.Remove(0, len);
                }
            }
            return document;
        }

        /// <summary>
        /// Returns possible parser parameters for this Parser
        /// </summary>
        /// <returns></returns>
        public override PossibleParserParameters GetPossibleParserParameters()
        {
            PossibleParserParameters possibleParserParameters = new PossibleParserParameters(4);
            //add all digits from 00 to 99 as possible null symbols
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    Token nullToken = new Token(null);
                    Symbol nullSymbol = new Symbol(nullToken);
                    nullSymbol.Text = i.ToString() + j.ToString();
                    nullToken.Symbols.Add(nullSymbol);
                    possibleParserParameters.PossibleNulls.Add(nullToken);
                }
            }
            return possibleParserParameters;
        }
    }

    /// <summary>
    /// Parses the text into regular elements of two
    /// Also supports nomenclature elements of four digits with a prefix
    /// </summary>
    public class Nomenclature4DigitsWithPrefixParser : SimpleSingleTokenParser
    {
        private List<Token> _nulls = new List<Token>();
        private List<Token> _nomenclaturePrefix = new List<Token>();

        public Nomenclature4DigitsWithPrefixParser(List<Token> nomenclaturePrefix, List<Token> nulls = null)
        {
            ParserName = GetType().Name;
            if (nomenclaturePrefix != null)
            {
                _nomenclaturePrefix = nomenclaturePrefix;
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
        public override TextDocument GetTextDocument()
        {
            TextDocument document = base.GetTextDocument();
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

                            if (_nomenclaturePrefix.Contains(symbol0))
                            {
                                //nomenclature
                                Token nomenclatureToken = tokenBuilder.GetToken(0, 4, line);
                                nomenclatureToken.TokenType = TokenType.NomenclatureElement;
                                line.Tokens.Add(nomenclatureToken);
                                tokenBuilder.Clear();
                                continue;

                            }
                            else if (_nulls.Contains(symbol0) && _nulls.Contains(symbol1))
                            {
                                //null length 2
                                Token nullToken = tokenBuilder.GetToken(0, 2, line);
                                nullToken.TokenType = TokenType.NullElement;
                                line.Tokens.Add(nullToken);
                                tokenBuilder.Remove(0, 2);
                                continue;
                            }
                            else if (_nulls.Contains(symbol0))
                            {
                                //null length 1
                                Token nullToken = tokenBuilder.GetToken(0, 1, line);
                                nullToken.TokenType = TokenType.NullElement;
                                line.Tokens.Add(nullToken);
                                tokenBuilder.Remove(0, 1);
                                continue;
                            }
                            else if (_nulls.Contains(symbol1) || _nomenclaturePrefix.Contains(symbol1))
                            {
                                //code length 1
                                Token regularElementToken = tokenBuilder.GetToken(0, 1, line);
                                regularElementToken.TokenType = TokenType.RegularElement;
                                line.Tokens.Add(regularElementToken);
                                tokenBuilder.Remove(0, 1);
                                continue;
                            }
                            else
                            {
                                //code length 2
                                Token regularElementToken = tokenBuilder.GetToken(0, 2, line);
                                regularElementToken.TokenType = TokenType.RegularElement;
                                line.Tokens.Add(regularElementToken);
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
                        nullToken.TokenType = TokenType.NullElement;
                        lastLine.Tokens.Add(nullToken);
                        tokenBuilder.Remove(0, 1);
                    }
                    else if (tokenBuilder.Length == 1)
                    {
                        //code length 1
                        Token regularElementToken = tokenBuilder.GetToken(0, 1, lastLine);
                        regularElementToken.TokenType = TokenType.RegularElement;
                        lastLine.Tokens.Add(regularElementToken);
                        tokenBuilder.Remove(0, 1);
                    }
                    else
                    {
                        Symbol symbol1 = tokenBuilder[1];
                        if (_nulls.Contains(symbol1))
                        {
                            //code length 1
                            Token regularElementToken = tokenBuilder.GetToken(0, 1, lastLine);
                            regularElementToken.TokenType = TokenType.RegularElement;
                            lastLine.Tokens.Add(regularElementToken);
                            tokenBuilder.Remove(0, 1);
                            //null length 1
                            Token nullToken = tokenBuilder.GetToken(0, 1, lastLine);
                            nullToken.TokenType = TokenType.NullElement;
                            lastLine.Tokens.Add(nullToken);
                            tokenBuilder.Remove(0, 1);
                        }
                        else
                        {
                            //code length 2
                            Token regularElementToken = tokenBuilder.GetToken(0, 2, lastLine);
                            regularElementToken.TokenType = TokenType.RegularElement;
                            lastLine.Tokens.Add(regularElementToken);
                            tokenBuilder.Remove(0, 2);
                        }
                    }
                }
            }
            return document;
        }

        /// <summary>
        /// Returns possible parser parameters for this Parser
        /// </summary>
        /// <returns></returns>
        public override PossibleParserParameters GetPossibleParserParameters()
        {
            PossibleParserParameters possibleParserParameters = new PossibleParserParameters(2);
            //add all digits from 0 to 9 as possible null symbols
            //add all digits from 0 to 9 as possible prefix symbols
            for (int i = 0; i < 10; i++)
            {
                Token nullToken = new Token(null);
                Symbol nullSymbol = new Symbol(nullToken);
                nullSymbol.Text = i.ToString();
                nullToken.Symbols.Add(nullSymbol);
                possibleParserParameters.PossibleNulls.Add(nullToken);

                Token prefixToken = new Token(null);
                Symbol prefixSymbol = new Symbol(prefixToken);
                prefixSymbol.Text = i.ToString();
                prefixToken.Symbols.Add(prefixSymbol);
                possibleParserParameters.PossiblePrefixes.Add(prefixToken);
            }
            return possibleParserParameters;
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
            ParserName = GetType().Name;
            if (nulls != null)
            {
                _nulls = nulls;
            }
        }

        /// <summary>
        /// Returns the parsed new document
        /// </summary>
        /// <returns></returns>
        public override TextDocument GetTextDocument()
        {
            TextDocument document = base.GetTextDocument();
            if (document == null)
            {
                return null;
            }

            //create new tokens based on the "old" tokens
            TokenBuilder tokenBuilder = new TokenBuilder();
            TokenBuilder tagTokenBuilder = new TokenBuilder();
            Line lastLine = null;

            bool is_a_tag = false;
            bool is_nomenclature = false;

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

                        if (!is_nomenclature)
                        {
                            if (_nulls.Contains(symbol))
                            {
                                //null symbol
                                Token nullToken = tokenBuilder.GetToken(0, 1, line);
                                nullToken.TokenType = TokenType.NullElement;
                                line.Tokens.Add(nullToken);
                                tokenBuilder.Clear();
                            }
                            else
                            {
                                //regular element
                                Token regularElementToken = tokenBuilder.GetToken(0, 1, line);
                                regularElementToken.TokenType = TokenType.RegularElement;
                                line.Tokens.Add(regularElementToken);
                                tokenBuilder.Clear();
                            }
                        }

                        if (is_nomenclature && tokenBuilder.Length == 3)
                        {
                            //nomenclature 3
                            if (!_nulls.Contains(tokenBuilder[2]))
                            {
                                Token nomenclatureToken = tokenBuilder.GetToken(0, 3, line);
                                nomenclatureToken.TokenType = TokenType.NomenclatureElement;
                                line.Tokens.Add(nomenclatureToken);
                                tokenBuilder.Clear();
                            }
                            //nomenclature 2 with null
                            else if (_nulls.Contains(tokenBuilder[2]))
                            {
                                Token nomenclatureToken = tokenBuilder.GetToken(0, 2, line);
                                nomenclatureToken.TokenType = TokenType.NomenclatureElement;
                                line.Tokens.Add(nomenclatureToken);
                                tokenBuilder.Remove(0, 2);
                                Token nullToken = tokenBuilder.GetToken(0, 1, line);
                                nullToken.TokenType = TokenType.NullElement;
                                line.Tokens.Add(nullToken);
                                tokenBuilder.Clear();
                            }
                            else
                            {
                                tokenBuilder.Clear();
                            }
                            is_nomenclature = false;
                        }

                        if (symbol.Top.Equals("."))
                        {
                            is_nomenclature = true;
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
                        nullToken.TokenType = TokenType.NullElement;
                        lastLine.Tokens.Add(nullToken);
                        tokenBuilder.Remove(0, 1);
                    }
                    else
                    {
                        //code length 1
                        Token regularElementToken = tokenBuilder.GetToken(0, tokenBuilder.Length - 1, lastLine);
                        regularElementToken.TokenType = TokenType.RegularElement;
                        lastLine.Tokens.Add(regularElementToken);
                        tokenBuilder.Remove(0, 1);
                    }
                }
            }
            return document;
        }

        /// <summary>
        /// Returns possible parser parameters for this Parser
        /// </summary>
        /// <returns></returns>
        public override PossibleParserParameters GetPossibleParserParameters()
        {
            PossibleParserParameters possibleParserParameters = new PossibleParserParameters(2);
            //add all digits from 0 to 9 as possible null symbols
            for (int i = 0; i < 10; i++)
            {
                Token nullToken = new Token(null);
                Symbol nullSymbol = new Symbol(nullToken);
                nullSymbol.Text = i.ToString();
                nullToken.Symbols.Add(nullSymbol);
                possibleParserParameters.PossibleNulls.Add(nullToken);
            }
            return possibleParserParameters;
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
            ParserName = GetType().Name;
            if (nulls != null)
            {
                _nulls = nulls;
            }
        }

        /// <summary>
        /// Returns the parsed new document
        /// </summary>
        /// <returns></returns>
        public override TextDocument GetTextDocument()
        {
            TextDocument document = base.GetTextDocument();
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
                            if (tokenBuilder[0].Top == string.Empty &&
                                tokenBuilder[1].Top == string.Empty &&
                                tokenBuilder[2].Top == string.Empty &&
                                tokenBuilder[3].Top == string.Empty &&
                                _nulls.Contains(tokenBuilder[2]) &&
                                _nulls.Contains(tokenBuilder[3]))
                            {
                                //nomenclature element
                                Token nomenclatureToken = tokenBuilder.GetToken(0, 4, line);
                                nomenclatureToken.TokenType = TokenType.NomenclatureElement;
                                line.Tokens.Add(nomenclatureToken);
                                tokenBuilder.Remove(0, 4);
                                continue;
                            }

                            if (tokenBuilder[1].Text.Equals("2") && tokenBuilder[1].Top != null && tokenBuilder[1].Top.Equals("."))
                            {
                                //code length 2
                                Token regularElementToken = tokenBuilder.GetToken(0, 2, line);
                                regularElementToken.TokenType = TokenType.RegularElement;
                                line.Tokens.Add(regularElementToken);
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
                                    nullToken.TokenType = TokenType.NullElement;
                                    line.Tokens.Add(nullToken);
                                    tokenBuilder.Remove(0, 1);
                                }
                                else
                                {
                                    //regular element length 1
                                    Token regularElementToken = tokenBuilder.GetToken(0, 1, line);
                                    regularElementToken.TokenType = TokenType.RegularElement;
                                    line.Tokens.Add(regularElementToken);
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
                        nullToken.TokenType = TokenType.NullElement;
                        lastLine.Tokens.Add(nullToken);
                        tokenBuilder.Remove(0, 1);
                    }
                    else
                    {
                        //code length 1
                        Token regularElementToken = tokenBuilder.GetToken(0, 1, lastLine);
                        regularElementToken.TokenType = TokenType.RegularElement;
                        lastLine.Tokens.Add(regularElementToken);
                        tokenBuilder.Remove(0, 1);
                    }
                }
            }
            return document;
        }

        /// <summary>
        /// Returns possible parser parameters for this Parser
        /// </summary>
        /// <returns></returns>
        public override PossibleParserParameters GetPossibleParserParameters()
        {
            PossibleParserParameters possibleParserParameters = new PossibleParserParameters(2);
            //add all digits from 0 to 9 as possible null symbols
            for (int i = 0; i < 10; i++)
            {
                Token nullToken = new Token(null);
                Symbol nullSymbol = new Symbol(nullToken);
                nullSymbol.Text = i.ToString();
                nullToken.Symbols.Add(nullSymbol);
                possibleParserParameters.PossibleNulls.Add(nullToken);
            }
            return possibleParserParameters;
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
            ParserName = GetType().Name;
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
        public override TextDocument GetTextDocument()
        {
            TextDocument document = base.GetTextDocument();
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
                                nullToken.TokenType = TokenType.NullElement;
                                line.Tokens.Add(nullToken);
                                tokenBuilder.Remove(0, 1);
                                continue;
                            }

                            // 13^
                            if (_nulls.Contains(symbol0) && !symbol0.Top.Equals(".") && symbol1.Top.Equals("."))
                            {
                                //null length 1
                                Token nullToken = tokenBuilder.GetToken(0, 1, line);
                                nullToken.TokenType = TokenType.NullElement;
                                line.Tokens.Add(nullToken);
                                tokenBuilder.Remove(0, 1);
                                continue;
                            }
                            // 33^
                            if (_oddDigits.Contains(symbol0) && symbol1.Top.Equals("."))
                            {
                                //code length 1
                                Token regularElementToken = tokenBuilder.GetToken(0, 1, line);
                                regularElementToken.TokenType = TokenType.RegularElement;
                                line.Tokens.Add(regularElementToken);
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
                                Token regularElementToken = tokenBuilder.GetToken(0, 2, line);
                                regularElementToken.TokenType = TokenType.RegularElement;
                                line.Tokens.Add(regularElementToken);
                                tokenBuilder.Remove(0, 2);
                                continue;
                            }

                            // 1 *
                            if (_nulls.Contains(symbol0) && !symbol0.Top.Equals("."))
                            {
                                //null length 1
                                Token nullToken = tokenBuilder.GetToken(0, 1, line);
                                nullToken.TokenType = TokenType.NullElement;
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
                                //nomenclature length 4
                                Token nomenclatureToken = tokenBuilder.GetToken(0, 4, line);
                                nomenclatureToken.TokenType = TokenType.NomenclatureElement;
                                line.Tokens.Add(nomenclatureToken);
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
                                //nomenclature length 3
                                Token nomenclatureToken = tokenBuilder.GetToken(0, 3, line);
                                nomenclatureToken.TokenType = TokenType.NomenclatureElement;
                                line.Tokens.Add(nomenclatureToken);
                                tokenBuilder.Remove(0, 3);
                                continue;
                            }

                            // 3^.7 or 4^.1
                            if (symbol0.Top.Contains(".") && _oddDigits.Contains(symbol1))
                            {
                                //code length 2
                                Token regularElementToken = tokenBuilder.GetToken(0, 2, line);
                                regularElementToken.TokenType = TokenType.RegularElement;
                                line.Tokens.Add(regularElementToken);
                                tokenBuilder.Remove(0, 2);
                                continue;
                            }

                            // 3
                            if (_oddDigits.Contains(symbol0))
                            {
                                //code length 1
                                Token regularElementToken = tokenBuilder.GetToken(0, 1, line);
                                regularElementToken.TokenType = TokenType.RegularElement;
                                line.Tokens.Add(regularElementToken);
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
                    if (_nulls.Contains(symbol0) && !symbol0.Top.Equals("."))
                    {
                        //null length 1
                        Token nullToken = tokenBuilder.GetToken(0, 1, lastLine);
                        nullToken.TokenType = TokenType.NullElement;
                        lastLine.Tokens.Add(nullToken);
                        tokenBuilder.Remove(0, 1);
                    }
                    else
                    {
                        if (tokenBuilder[0].Top.Equals(".") && tokenBuilder.Length > 1)
                        {
                            //code length 2
                            Token regularElementToken = tokenBuilder.GetToken(0, 2, lastLine);
                            regularElementToken.TokenType = TokenType.RegularElement;
                            lastLine.Tokens.Add(regularElementToken);
                            tokenBuilder.Remove(0, 2);
                        }
                        else
                        {
                            //code length 1
                            Token regularElementToken = tokenBuilder.GetToken(0, 1, lastLine);
                            regularElementToken.TokenType = TokenType.RegularElement;
                            lastLine.Tokens.Add(regularElementToken);
                            tokenBuilder.Remove(0, 1);
                        }
                    }
                }
            }
            return document;
        }

        /// <summary>
        /// Returns possible parser parameters for this Parser
        /// </summary>
        /// <returns></returns>
        public override PossibleParserParameters GetPossibleParserParameters()
        {
            PossibleParserParameters possibleParserParameters = new PossibleParserParameters(2);
            //add all digits from 0 to 9 as possible null symbols
            for (int i = 0; i < 10; i++)
            {
                Token nullToken = new Token(null);
                Symbol nullSymbol = new Symbol(nullToken);
                nullSymbol.Text = i.ToString();
                nullToken.Symbols.Add(nullSymbol);
                possibleParserParameters.PossibleNulls.Add(nullToken);
            }
            return possibleParserParameters;
        }
    }

    /// <summary>
    /// Parser for Francia 18-1
    /// </summary>
    public class Francia18Parser : SimpleSingleTokenParser
    {
        private List<Token> _nulls = new List<Token>();
        private List<Token> _specialSet = new List<Token>();

        public Francia18Parser(List<Token> nulls)
        {
            ParserName = GetType().Name;
            if (nulls != null)
            {
                _nulls = nulls;
            }

            string specialSetString = "+-,";
            for (int i = 0; i < specialSetString.Length; i++)
            {
                Token specialToken = new Token(null);
                Symbol specialSymbol = new Symbol(specialToken);
                specialSymbol.Text = specialSetString.Substring(i, 1);
                specialToken.Symbols.Add(specialSymbol);
                _specialSet.Add(specialToken);
            }
        }

        /// <summary>
        /// Returns the parsed new document
        /// </summary>
        /// <returns></returns>
        public override TextDocument GetTextDocument()
        {
            TextDocument document = base.GetTextDocument();
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

                        if (tokenBuilder.Length >= 5)
                        {
                            Symbol symbol0 = tokenBuilder[0];
                            Symbol symbol1 = tokenBuilder[1];
                            Symbol symbol2 = tokenBuilder[2];
                            Symbol symbol3 = tokenBuilder[3];
                            Symbol symbol4 = tokenBuilder[4];
                            // 846,8
                            if (_nulls.Contains(symbol0) && _specialSet.Contains(symbol3) && _nulls.Contains(symbol4))
                            {
                                //null length 1
                                Token nullToken = tokenBuilder.GetToken(0, 1, line);
                                nullToken.TokenType = TokenType.NullElement;
                                line.Tokens.Add(nullToken);
                                tokenBuilder.Remove(0, 1);
                                //nomenclature length 3
                                Token nomenclatureToken = tokenBuilder.GetToken(0, 3, line);
                                nomenclatureToken.TokenType = TokenType.NomenclatureElement;
                                line.Tokens.Add(nomenclatureToken);
                                tokenBuilder.Remove(0, 3);
                                continue;
                            }
                            // 0^.47
                            if (symbol1.Top.Equals("."))
                            {
                                //nomenclature length 3
                                Token nomenclatureToken = tokenBuilder.GetToken(0, 3, line);
                                nomenclatureToken.TokenType = TokenType.NomenclatureElement;
                                line.Tokens.Add(nomenclatureToken);
                                tokenBuilder.Remove(0, 3);
                                continue;
                            }
                            // 80^.4
                            if (_nulls.Contains(symbol0) && symbol1.Top.Equals("."))
                            {
                                //null length 1
                                Token nullToken = tokenBuilder.GetToken(0, 1, line);
                                nullToken.TokenType = TokenType.NullElement;
                                line.Tokens.Add(nullToken);
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
                            // 26,8
                            if (!_nulls.Contains(symbol0) && _specialSet.Contains(symbol2) && _nulls.Contains(symbol3))
                            {
                                //nomenclature length 3
                                Token nomenclatureToken = tokenBuilder.GetToken(0, 3, line);
                                nomenclatureToken.TokenType = TokenType.NomenclatureElement;
                                line.Tokens.Add(nomenclatureToken);
                                tokenBuilder.Remove(0, 3);
                                continue;
                            }
                            // 8588
                            if (_nulls.Contains(symbol0) && _nulls.Contains(symbol2) && _nulls.Contains(symbol3))
                            {
                                //null length 1
                                Token nullToken = tokenBuilder.GetToken(0, 1, line);
                                nullToken.TokenType = TokenType.NullElement;
                                line.Tokens.Add(nullToken);
                                tokenBuilder.Remove(0, 1);
                                //nomenclature length 2
                                Token nomenclatureToken = tokenBuilder.GetToken(0, 2, line);
                                nomenclatureToken.TokenType = TokenType.NomenclatureElement;
                                line.Tokens.Add(nomenclatureToken);
                                tokenBuilder.Remove(0, 2);
                                continue;
                            }
                            // 86,2    862+
                            if (_nulls.Contains(symbol0) && !_nulls.Contains(symbol1) && (_specialSet.Contains(symbol2) || _specialSet.Contains(symbol3)))
                            {
                                //null length 1
                                Token nullToken = tokenBuilder.GetToken(0, 1, line);
                                nullToken.TokenType = TokenType.NullElement;
                                line.Tokens.Add(nullToken);
                                tokenBuilder.Remove(0, 1);
                                //nomenclature length 3
                                Token nomenclatureToken = tokenBuilder.GetToken(0, 3, line);
                                nomenclatureToken.TokenType = TokenType.NomenclatureElement;
                                line.Tokens.Add(nomenclatureToken);
                                tokenBuilder.Remove(0, 3);
                                continue;
                            }
                            // 0^.8
                            if (symbol2.Top.Equals(".") && _nulls.Contains(symbol1))
                            {
                                // nomenclature length 2
                                Token nomenclatureToken = tokenBuilder.GetToken(0, 2, line);
                                nomenclatureToken.TokenType = TokenType.NomenclatureElement;
                                line.Tokens.Add(nomenclatureToken);
                                tokenBuilder.Remove(0, 2);
                                continue;
                            }
                        }
                        if (tokenBuilder.Length >= 3)
                        {

                            Symbol symbol0 = tokenBuilder[0];
                            Symbol symbol1 = tokenBuilder[1];
                            Symbol symbol2 = tokenBuilder[2];
                            // 26,
                            if (_specialSet.Contains(symbol2))
                            {
                                // nomenclature length 3
                                Token nomenclatureToken = tokenBuilder.GetToken(0, 3, line);
                                nomenclatureToken.TokenType = TokenType.NomenclatureElement;
                                line.Tokens.Add(nomenclatureToken);
                                tokenBuilder.Remove(0, 3);
                                continue;
                            }
                            //588
                            if (_nulls.Contains(symbol1) && _nulls.Contains(symbol2))
                            {
                                // nomenclature length 2
                                Token nomenclatureToken = tokenBuilder.GetToken(0, 2, line);
                                nomenclatureToken.TokenType = TokenType.NomenclatureElement;
                                line.Tokens.Add(nomenclatureToken);
                                tokenBuilder.Remove(0, 2);
                                continue;
                            }
                        }

                        if (tokenBuilder.Length >= 5)
                        {
                            if (tokenBuilder[0].Text.Equals("."))
                            {
                                //unknown length 1
                                Token unknownToken = tokenBuilder.GetToken(0, 1, line);
                                unknownToken.TokenType = TokenType.Unknown;
                                line.Tokens.Add(unknownToken);
                                tokenBuilder.Remove(0, 1);
                            }
                            if (_nulls.Contains(tokenBuilder[0]))
                            {
                                //null length 1
                                Token nullToken = tokenBuilder.GetToken(0, 1, line);
                                nullToken.TokenType = TokenType.NullElement;
                                line.Tokens.Add(nullToken);
                                tokenBuilder.Remove(0, 1);
                            }
                            else if (_nulls.Contains(tokenBuilder[1]))
                            {
                                //code length 1
                                Token codeToken = tokenBuilder.GetToken(0, 1, line);
                                codeToken.TokenType = TokenType.RegularElement;
                                line.Tokens.Add(codeToken);
                                tokenBuilder.Remove(0, 1);
                            }
                            else
                            {
                                //code length 2
                                Token codeToken = tokenBuilder.GetToken(0, 2, line);
                                codeToken.TokenType = TokenType.RegularElement;
                                line.Tokens.Add(codeToken);
                                tokenBuilder.Remove(0, 2);
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
                    if (tokenBuilder[0].Text.Equals("."))
                    {
                        //unknown length 1
                        Token unknownToken = tokenBuilder.GetToken(0, 1, lastLine);
                        unknownToken.TokenType = TokenType.Unknown;
                        lastLine.Tokens.Add(unknownToken);
                        tokenBuilder.Remove(0, 1);
                    }
                    if (_nulls.Contains(tokenBuilder[0]))
                    {
                        //null length 1
                        Token nullToken = tokenBuilder.GetToken(0, 1, lastLine);
                        nullToken.TokenType = TokenType.NullElement;
                        lastLine.Tokens.Add(nullToken);
                        tokenBuilder.Remove(0, 1);
                    }
                    else if (_nulls.Contains(tokenBuilder[1]))
                    {
                        //code length 1
                        Token codeToken = tokenBuilder.GetToken(0, 1, lastLine);
                        codeToken.TokenType = TokenType.RegularElement;
                        lastLine.Tokens.Add(codeToken);
                        tokenBuilder.Remove(0, 1);
                    }
                    else
                    {
                        //code length 2
                        Token codeToken = tokenBuilder.GetToken(0, 2, lastLine);
                        codeToken.TokenType = TokenType.RegularElement;
                        lastLine.Tokens.Add(codeToken);
                        tokenBuilder.Remove(0, 2);
                    }
                }
            }
            return document;
        }

        /// <summary>
        /// Returns possible parser parameters for this Parser
        /// </summary>
        /// <returns></returns>
        public override PossibleParserParameters GetPossibleParserParameters()
        {
            PossibleParserParameters possibleParserParameters = new PossibleParserParameters(2);
            //add all digits from 0 to 9 as possible null symbols
            for (int i = 0; i < 10; i++)
            {
                Token nullToken = new Token(null);
                Symbol nullSymbol = new Symbol(nullToken);
                nullSymbol.Text = i.ToString();
                nullToken.Symbols.Add(nullSymbol);
                possibleParserParameters.PossibleNulls.Add(nullToken);
            }
            return possibleParserParameters;
        }
    }

    /// <summary>
    /// Parser for variable length homophonic ciphers
    /// </summary>
    public class VariableLengthHomophonicCipher : SimpleSingleTokenParser
    {
        private List<Token> _nulls = new List<Token>();
        private Decoder _decoder;

        public VariableLengthHomophonicCipher(List<Token> nulls, Decoder decoder)
        {
            ParserName = GetType().Name;
            if (nulls != null)
            {
                _nulls = nulls;
            }
            _decoder = decoder;
        }

        /// <summary>
        /// Returns the parsed new document
        /// </summary>
        /// <returns></returns>
        public override TextDocument GetTextDocument()
        {
            TextDocument document = base.GetTextDocument();
            if (document == null)
            {
                return null;
            }
            if (_decoder == null)
            {
                throw new Exception("No decoder (no key) given. Parser is not able to work without a key");
            }

            int maxTokenLength = _decoder.GetMaximumTokenLength();

            foreach (Page page in document.Pages)
            {
                //create new tokens based on the "old" tokens
                TokenBuilder tokenBuilder = new TokenBuilder();
                TokenBuilder tagTokenBuilder = new TokenBuilder();
                Line lastLine = null;

                bool is_a_tag = false;
                List<Token> keyTokens = _decoder.GetKeyTokens();

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

                        if (tokenBuilder.Length >= maxTokenLength)
                        {
                            bool removedOne = false;
                            for (int length = tokenBuilder.Length; length > 0; length--)
                            {
                                Token token = tokenBuilder.GetToken(0, length, line);
                                if (_nulls.Contains(token))
                                {
                                    token.TokenType = TokenType.NullElement;
                                    line.Tokens.Add(token);
                                    tokenBuilder.Remove(0, length);
                                    removedOne = true;
                                    break;
                                }
                                else if (keyTokens.Contains(token))
                                {
                                    token.TokenType = TokenType.RegularElement;
                                    line.Tokens.Add(token);
                                    tokenBuilder.Remove(0, length);
                                    removedOne = true;
                                    break;
                                }
                            }
                            if (!removedOne)
                            {
                                Token token = tokenBuilder.GetToken(0, 1, line);
                                token.TokenType = TokenType.Unknown;
                                line.Tokens.Add(token);
                                tokenBuilder.Remove(0, 1);
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
                    bool removedOne = false;
                    for (int length = tokenBuilder.Length; length > 0; length--)
                    {
                        Token token = tokenBuilder.GetToken(0, length, lastLine);
                        if (_nulls.Contains(token))
                        {
                            token.TokenType = TokenType.NullElement;
                            lastLine.Tokens.Add(token);
                            tokenBuilder.Remove(0, length);
                            removedOne = true;
                            break;
                        }
                        else if (keyTokens.Contains(token))
                        {
                            token.TokenType = TokenType.RegularElement;
                            lastLine.Tokens.Add(token);
                            tokenBuilder.Remove(0, length);
                            removedOne = true;
                            break;
                        }
                    }
                    if (!removedOne)
                    {
                        Token token = tokenBuilder.GetToken(0, 1, lastLine);
                        token.TokenType = TokenType.Unknown;
                        lastLine.Tokens.Add(token);
                        tokenBuilder.Remove(0, 1);
                    }
                }
            }
            return document;
        }

        /// <summary>
        /// Returns possible parser parameters for this Parser
        /// </summary>
        /// <returns></returns>
        public override PossibleParserParameters GetPossibleParserParameters()
        {
            PossibleParserParameters possibleParserParameters = new PossibleParserParameters(4);
            //add all digits from 0 to 9 as possible null symbols
            for (int i = 0; i < 10; i++)
            {
                Token nullToken = new Token(null);
                Symbol nullSymbol = new Symbol(nullToken);
                nullSymbol.Text = i.ToString();
                nullToken.Symbols.Add(nullSymbol);
                possibleParserParameters.PossibleNulls.Add(nullToken);
            }
            //add all digits from 00 to 99 as possible null symbols
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    Token nullToken = new Token(null);
                    Symbol nullSymbol = new Symbol(nullToken);
                    nullSymbol.Text = i.ToString() + j.ToString();
                    nullToken.Symbols.Add(nullSymbol);
                    possibleParserParameters.PossibleNulls.Add(nullToken);
                }
            }
            return possibleParserParameters;
        }
    }

    /// <summary>
    /// Parser for Francia 346-1
    /// </summary>
    public class Francia346Parser : SimpleSingleTokenParser
    {
        private List<Token> _nulls = new List<Token>();

        public Francia346Parser(List<Token> nulls)
        {
            ParserName = GetType().Name;
            if (nulls != null)
            {
                _nulls = nulls;
            }
        }

        /// <summary>
        /// Returns the parsed new document
        /// </summary>
        /// <returns></returns>
        public override TextDocument GetTextDocument()
        {
            TextDocument document = base.GetTextDocument();
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

                        if(tokenBuilder.Length >= 5)
                        {
                            Symbol symbol0 = tokenBuilder[0];
                            Symbol symbol1 = tokenBuilder[1];
                            Symbol symbol2 = tokenBuilder[2];
                            Symbol symbol3 = tokenBuilder[3];

                            if (_nulls.Contains(symbol0))
                            {
                                Token nullToken = tokenBuilder.GetToken(0, 1, line);
                                nullToken.TokenType = TokenType.NullElement;
                                line.Tokens.Add(nullToken);
                                tokenBuilder.Remove(0, 1);
                                continue;
                            }

                            if (_nulls.Contains(tokenBuilder.GetToken(0, 2, null)))
                            {
                                Token nullToken = tokenBuilder.GetToken(0, 2, line);
                                nullToken.TokenType = TokenType.NullElement;
                                line.Tokens.Add(nullToken);
                                tokenBuilder.Remove(0, 2);
                                continue;
                            }                            

                            if (_nulls.Contains(symbol3))
                            {
                                Token codeToken = tokenBuilder.GetToken(0, 3, line);
                                codeToken.TokenType = TokenType.NomenclatureElement;
                                line.Tokens.Add(codeToken);
                                tokenBuilder.Remove(0, 3);

                                Token nullToken = tokenBuilder.GetToken(0, 1, line);
                                nullToken.TokenType = TokenType.NullElement;
                                line.Tokens.Add(nullToken);
                                tokenBuilder.Remove(0, 1);
                                continue;
                            }

                            if (_nulls.Contains(tokenBuilder.GetToken(3, 2, line)))
                            {
                                Token codeToken = tokenBuilder.GetToken(0, 3, line);
                                codeToken.TokenType = TokenType.NomenclatureElement;
                                line.Tokens.Add(codeToken);
                                tokenBuilder.Remove(0, 3);

                                Token nullToken = tokenBuilder.GetToken(0, 2, line);
                                nullToken.TokenType = TokenType.NullElement;
                                line.Tokens.Add(nullToken);
                                tokenBuilder.Remove(0, 2);
                                continue;
                            }

                            Token regularElementToken = tokenBuilder.GetToken(0, 2, line);
                            regularElementToken.TokenType = TokenType.RegularElement;
                            line.Tokens.Add(regularElementToken);
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
                    Symbol symbol0 = tokenBuilder[0];

                    if (_nulls.Contains(symbol0))
                    {
                        Token nullToken = tokenBuilder.GetToken(0, 1, lastLine);
                        nullToken.TokenType = TokenType.NullElement;
                        lastLine.Tokens.Add(nullToken);
                        tokenBuilder.Remove(0, 1);
                        continue;
                    }

                    if (tokenBuilder.Length >= 2 && _nulls.Contains(tokenBuilder.GetToken(0, 2, null)))
                    {
                        Token nullToken = tokenBuilder.GetToken(0, 2, lastLine);
                        nullToken.TokenType = TokenType.NullElement;
                        lastLine.Tokens.Add(nullToken);
                        tokenBuilder.Remove(0, 2);
                        continue;
                    }

                    if (tokenBuilder.Length >= 2)
                    {
                        Token regularElementToken = tokenBuilder.GetToken(0, 2, lastLine);
                        regularElementToken.TokenType = TokenType.RegularElement;
                        lastLine.Tokens.Add(regularElementToken);
                        tokenBuilder.Remove(0, 2);
                    }
                    else
                    {
                        Token unknownToken = tokenBuilder.GetToken(0, 1, lastLine);
                        unknownToken.TokenType = TokenType.Unknown;
                        lastLine.Tokens.Add(unknownToken);
                        tokenBuilder.Remove(0, 1);
                    }
                }
            }
            return document;
        }

        /// <summary>
        /// Returns possible parser parameters for this Parser
        /// </summary>
        /// <returns></returns>
        public override PossibleParserParameters GetPossibleParserParameters()
        {
            PossibleParserParameters possibleParserParameters = new PossibleParserParameters(4);
            //add all digits from 0 to 9 as possible null symbols
            for (int i = 0; i < 10; i++)
            {
                Token nullToken = new Token(null);
                Symbol nullSymbol = new Symbol(nullToken);
                nullSymbol.Text = i.ToString();
                nullToken.Symbols.Add(nullSymbol);
                possibleParserParameters.PossibleNulls.Add(nullToken);
            }
            //add all digits from 00 to 99 as possible null symbols
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    Token nullToken = new Token(null);
                    Symbol nullSymbol = new Symbol(nullToken);
                    nullSymbol.Text = i.ToString() + j.ToString();
                    nullToken.Symbols.Add(nullSymbol);
                    possibleParserParameters.PossibleNulls.Add(nullToken);
                }
            }
            return possibleParserParameters;
        }
    }

    /// <summary>
    /// Parser for Francia 346-1
    /// </summary>
    public class Francia283Parser : SimpleSingleTokenParser
    {      
        public Francia283Parser()
        {
            ParserName = GetType().Name;
        }

        /// <summary>
        /// Returns the parsed new document
        /// </summary>
        /// <returns></returns>
        public override TextDocument GetTextDocument()
        {
            TextDocument document = base.GetTextDocument();
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

                        if (tokenBuilder.Length >= 5)
                        {
                            Symbol symbol0 = tokenBuilder[0];
                            Symbol symbol1 = tokenBuilder[1];
                            Symbol symbol2 = tokenBuilder[2];
                            Symbol symbol3 = tokenBuilder[3];
                            Symbol symbol4 = tokenBuilder[4];

                            if (symbol3.Text.Equals("+"))
                            {
                                Token codeToken = tokenBuilder.GetToken(0, 4, line);
                                codeToken.TokenType = TokenType.NomenclatureElement;
                                line.Tokens.Add(codeToken);
                                tokenBuilder.Remove(0, 4);
                                continue;
                            }
                            if (symbol4.Text.Equals("+"))
                            {

                                Token unknownToken = tokenBuilder.GetToken(0, 1, line);
                                unknownToken.TokenType = TokenType.Unknown;
                                line.Tokens.Add(unknownToken);
                                tokenBuilder.Remove(0, 1);

                                Token codeToken = tokenBuilder.GetToken(0, 4, line);
                                codeToken.TokenType = TokenType.NomenclatureElement;
                                line.Tokens.Add(codeToken);
                                tokenBuilder.Remove(0, 4);
                                continue;
                            }

                            if ("02468".Contains(symbol1.Text))
                            {
                                Token regularElementToken = tokenBuilder.GetToken(0, 2, line);
                                regularElementToken.TokenType = TokenType.RegularElement;
                                line.Tokens.Add(regularElementToken);
                                tokenBuilder.Remove(0, 2);
                            }
                            else
                            {
                                Token unknownToken = tokenBuilder.GetToken(0, 1, line);
                                unknownToken.TokenType = TokenType.Unknown;
                                line.Tokens.Add(unknownToken);
                                tokenBuilder.Remove(0, 1);
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
                    if(tokenBuilder.Length >= 4 && tokenBuilder[3].Text.Equals("+"))
                    {
                        Token codeToken = tokenBuilder.GetToken(0, 4, lastLine);
                        codeToken.TokenType = TokenType.NomenclatureElement;
                        lastLine.Tokens.Add(codeToken);
                        tokenBuilder.Remove(0, 4);
                        continue;
                    }

                    if (tokenBuilder.Length > 1 && "02468".Contains(tokenBuilder[1].Text))
                    {
                        Token regularElementToken = tokenBuilder.GetToken(0, 2, lastLine);
                        regularElementToken.TokenType = TokenType.RegularElement;
                        lastLine.Tokens.Add(regularElementToken);
                        tokenBuilder.Remove(0, 2);
                    }
                    else
                    {
                        Token unknownToken = tokenBuilder.GetToken(0, 1, lastLine);
                        unknownToken.TokenType = TokenType.Unknown;
                        lastLine.Tokens.Add(unknownToken);
                        tokenBuilder.Remove(0, 1);
                    }
                }
            }
            return document;
        }

        /// <summary>
        /// Returns possible parser parameters for this Parser
        /// </summary>
        /// <returns></returns>
        public override PossibleParserParameters GetPossibleParserParameters()
        {
            PossibleParserParameters possibleParserParameters = new PossibleParserParameters(4);
            //add all digits from 0 to 9 as possible null symbols
            for (int i = 0; i < 10; i++)
            {
                Token nullToken = new Token(null);
                Symbol nullSymbol = new Symbol(nullToken);
                nullSymbol.Text = i.ToString();
                nullToken.Symbols.Add(nullSymbol);
                possibleParserParameters.PossibleNulls.Add(nullToken);
            }          
            return possibleParserParameters;
        }
    }

    /// <summary>
    /// This parser is used for generating plaintext for the plaintext-ciphertext mapping
    /// </summary>
    public class KeyAsPlaintextParser : SimpleSingleTokenParser
    {
        public KeyAsPlaintextParser()
        {
            ParserName = GetType().Name;
        }

        /// <summary>
        /// Returns the parsed new document
        /// </summary>
        /// <returns></returns>
        public override TextDocument GetTextDocument()
        {
            TextDocument document = base.GetTextDocument();
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

                        if (symbol.Text.Equals("|"))
                        {
                            Token token = tokenBuilder.GetToken(0, tokenBuilder.Length - 1, line);
                            token.TokenType = TokenType.PlaintextElement;
                            line.Tokens.Add(token);
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
                    Token token = tokenBuilder.GetToken(0, tokenBuilder.Length - 1, lastLine);
                    if(token.Symbols[tokenBuilder.Length - 1].Text.Equals("|"))
                    {
                        token.Symbols.RemoveAt(tokenBuilder.Length - 1);
                    }
                    token.TokenType = TokenType.Unknown;
                    lastLine.Tokens.Add(token);
                    tokenBuilder.Clear();
                }
            }
            return document;
        }
    }
}