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
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace Cryptool.Plugins.DECODEDatabaseTools.Util
{

    /// <summary>
    /// Type of tokens
    /// </summary>
    public enum TokenType
    {
        Unknown = 0,
        Null = 1,
        RegularCode = 2,
        NomenclatureElement = 3,
        Tag = 4
    }

    public enum LineType
    {
        Comment = 0,
        Text = 1
    }

    /// <summary>
    /// A document contains one or more pages
    /// </summary>
    public class TextDocument
    {
        public List<Page> Pages
        {
            get;
            set;
        }

        /// <summary>
        /// Own index of document owner, i.e. file location, e.g. Segr.di Stato Francia 3/1/
        /// </summary>
        public string CatalogName
        {
            get;
            set;
        }

        /// <summary>
        /// The name of the image(s) representing the cipher, e.g. 117r.jpg-117v.jpg
        /// </summary>
        public string ImageName
        {
            get;
            set;
        }

        /// <summary>
        /// Full name or initials of the transcriber, e.g. TimB
        /// </summary>
        public string TranscriberName
        {
            get;
            set;
        }

        /// <summary>
        /// The date the transcription was created, e.g. February 3, 2016
        /// </summary>
        public string DateOfTranscription
        {
            get;
            set;
        }

        /// <summary>
        /// The time it took to transcribe all images of a cipher in hours and minutes without counting breaks and quality checks,
        /// e.g. 30+30+60 minutes = 120 minutes
        /// </summary>
        public string TranscriptionTime
        {
            get;
            set;
        }

        /// <summary>
        /// Method, which was used, to create the transcription
        /// </summary>
        public string TranscriptionMethod
        {
            get;
            set;
        }

        /// <summary>
        /// Description of e.g. difficulties, problems
        /// </summary>
        public string Comments
        {
            get;
            set;
        }

        /// <summary>
        /// Constructor to create a TextDocument
        /// </summary>
        public TextDocument()
        {
            Pages = new List<Page>();
        }


        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var page in Pages)
            {
                stringBuilder.Append(page.ToString());
                stringBuilder.Append(Environment.NewLine);
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Returns the number of tokens of this TextDocument
        /// </summary>
        public int TokenCount
        {
            get
            {
                int count = 0;
                foreach (var page in Pages)
                {                   
                    count += page.TokenCount;
                }
                return count;
            }
        }
    }

    /// <summary>
    /// A page contains several lines of text
    /// </summary>
    public class Page
    {      
        public int PageNumber { get; set; }

        public Page(TextDocument textDocument)
        {
            Lines = new List<Line>();
            ParentTextDocument = textDocument;
        }

        public List<Line> Lines
        {
            get;
            set;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var line in Lines)
            {
                stringBuilder.Append(line.ToString());
                stringBuilder.Append(Environment.NewLine);
            }
            return stringBuilder.ToString();
        }

        public string Visualize
        {
            get
            {
                return ToString();
            }
        }

        public TextDocument ParentTextDocument
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the number of tokens of this Page
        /// </summary>
        public int TokenCount
        {
            get
            {
                int count = 0;
                foreach(var line in Lines)
                {
                    if(line.LineType != LineType.Comment)
                    {
                        count += line.TokenCount;
                    }
                }
                return count;
            }
        }
    }

    /// <summary>
    /// A line is a single line of text consisting of tokens
    /// </summary>
    public class Line
    {
        public int LineNumber { get; set; }

        public Line(Page page)
        {
            Tokens = new List<Token>();
            LineType = LineType.Text;
            ParentPage = page;
        }

        public List<Token> Tokens
        {
            get;
            set;
        }

        public LineType LineType
        {
            get;
            set;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var token in Tokens)
            {
                stringBuilder.Append(token.ToString());               
            }
            return stringBuilder.ToString();
        }

        public string Visualize
        {
            get
            {
                return ToString();
            }
        }

        public Page ParentPage
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the number of tokens of this line
        /// </summary>
        public int TokenCount 
        {
            get
            {
                return Tokens.Count;
            }
        }
    }

    /// <summary>
    /// A Token is a single element of text
    /// </summary>
    public class Token
    {
        private List<Symbol> _symbols = new List<Symbol>();
        private List<Symbol> _decodedSymbols = new List<Symbol>();

        public Token(Line line)
        {
            TokenType = TokenType.Unknown;
            ParentLine = line;
            DecodedSymbols = new List<Symbol>();
            Symbols = new List<Symbol>();
        }

        public Token(Line line, string str) : this(line)
        {
            StringBuilder builder = new StringBuilder();
            foreach(var c in str)
            {
                builder.Append("" + c);
                if(builder.Length == 3)
                {
                    if(builder[1] == '^')
                    {
                        Symbol symbolWithTop = new Symbol(this);
                        symbolWithTop.Top = "" + builder[2];
                        symbolWithTop.Text = "" + builder[0];
                        Symbols.Add(symbolWithTop);
                        builder.Clear();
                        continue;
                    }
                    if (builder[1] == '_')
                    {
                        Symbol symbolWithBottom = new Symbol(this);
                        symbolWithBottom.Bottom = "" + builder[2];
                        symbolWithBottom.Text = "" + builder[0];
                        Symbols.Add(symbolWithBottom);
                        builder.Clear();
                        continue;
                    }
                    Symbol symbol = new Symbol(this);
                    symbol.Text = "" + builder[0];
                    Symbols.Add(symbol);
                    builder.Remove(0, 1);
                    continue;
                }
            }                      
            while(builder.Length > 0)
            {
                Symbol symbol = new Symbol(this);
                symbol.Text = "" + builder[0];
                Symbols.Add(symbol);
                builder.Remove(0, 1);
                continue;
            }
        }

        /// <summary>
        /// Sets the symbols to the given list
        /// also sets parent token at each element to this token
        /// </summary>
        public List<Symbol> Symbols
        {
            get
            {
                return _symbols;
            }
            set
            {
                _symbols = value;
                foreach(var symbol in _symbols)
                {
                    symbol.ParentToken = this;                    
                }
            }
        }

        /// <summary>
        /// Sets the decoded symbols to the given list
        /// also sets parent token at each element to this token
        /// </summary>
        public List<Symbol> DecodedSymbols
        {
            get
            {
                return _decodedSymbols;
            }
            set
            {
                if(value == null)
                {
                    return;
                }
                _decodedSymbols = value;
                foreach (var symbol in _decodedSymbols)
                {
                    symbol.ParentToken = this;
                }
            }
        }

        public TokenType TokenType
        {
            get; set;
        }

        /// <summary>
        /// Returns the ui color of this token
        /// </summary>
        /// <returns></returns>
        public SolidColorBrush TextColor
        {
            get
            {
                if(ParentLine == null)
                {
                    return null;
                }
                switch (ParentLine.LineType)
                {
                    case LineType.Text:
                        switch (TokenType)
                        {
                            case TokenType.Tag:
                                return Brushes.Black;
                            case TokenType.Null:
                                return Brushes.Gray;
                            case TokenType.NomenclatureElement:
                                return Brushes.DarkGreen;
                            case TokenType.RegularCode:
                                return Brushes.DarkBlue;
                            default:
                                return Brushes.Black;
                        }
                    case LineType.Comment:
                        return Brushes.Black;
                    default:
                        return Brushes.Black;
                }
            }
        }

        public Line ParentLine
        {
            get;
            set;
        }

        public override bool Equals(object obj)
        {
            var symbol = obj as Symbol;
            if(symbol != null)
            {
                if(Symbols.Count != 1)
                {
                    return false;
                }
                return symbol.Equals(Symbols[0]);
            }

            var token = obj as Token;
            if (token == null)
            {
                return false;
            }
            else
            {
                return token.GetHashCode() == GetHashCode();
            }
        }

        public override int GetHashCode()
        {
            var hash = 13;
            StringBuilder builder = new StringBuilder();
            var counter = 0;
            foreach (var symbol in Symbols)
            {
                counter++;
                hash = ((counter + hash) * 7) + (symbol != null ? symbol.GetHashCode() : 0);
                builder.Append(hash);
            }          
            return builder.ToString().GetHashCode();
        }

        public static Token operator+ (Token token, Symbol symbol)
        {
            token.Symbols.Add(symbol);
            symbol.ParentToken = token;
            return token;
        }

        public Symbol this[int index]
        {
            get
            {
                return Symbols[index];
            }
            set
            {                
                Symbols[index] = value;
                value.ParentToken = this;
            }
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var symbol in Symbols)
            {
                stringBuilder.Append(symbol.ToString());              
            }
            return stringBuilder.ToString();
        }
    }

    /// <summary>
    /// A symbol is a single symbol of text
    /// examples: A,B,C,...,a,b,c,...,0,1,2,...,0^1,0^2,... etc
    /// </summary>
    public class Symbol : ICloneable, IComparable
    {
        public Symbol(Token token)
        {            
            ParentToken = token;
            Top = string.Empty;
            Text = string.Empty;
            Bottom = string.Empty;
            BottomChangesSymbol = true;
            TopChangesSymbol = true;
        }

        /// <summary>
        /// Top text of symbol
        /// </summary>
        public string Top
        {
            get;
            set;
        }

        /// <summary>
        /// Does the Top text change the meaning of the symbol?
        /// </summary>
        public bool TopChangesSymbol
        {
            get; set;
        }

        /// <summary>
        /// Main text of symbol
        /// </summary>
        public string Text
        {
            get;
            set;
        }

        /// <summary>
        /// Bottom text of the symbol
        /// </summary>
        public string Bottom
        {
            get;
            set;
        }

        /// <summary>
        /// Does the Bottom text change the meaning of the symbol?
        /// </summary>
        public bool BottomChangesSymbol
        {
            get; set;
        }

        /// <summary>
        /// The parent token which this symbol belongs to
        /// </summary>
        public Token ParentToken
        {
            get; set;
        }

        /// <summary>
        /// Compares this symbol with another one
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var str = obj as string;
            if(str != null)
            {
                return str.Equals(Text);
            }

            var symbol = obj as Symbol;
            if(symbol == null)
            {
                return false;
            }
            else
            {
                return symbol.GetHashCode() == GetHashCode();
            }            
        }

        /// <summary>
        /// Returns the hash code of this symbol
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            var hash = 13;
            if (TopChangesSymbol)
            {
                hash = (hash * 3) + (Top != null ? Top.GetHashCode() : 0);
            }
            else
            {
                hash = (hash * 3) + string.Empty.GetHashCode();
            }

            hash = (hash * 5) + (Text != null ? Text.GetHashCode() : 0);

            if (BottomChangesSymbol)
            {
                hash = (hash * 7) + (Bottom != null ? Bottom.GetHashCode() : 0);
            }
            else
            {
                hash = (hash * 7) + string.Empty.GetHashCode();
            }
            return hash;
        }

        /// <summary>
        /// Clones this symbol
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            Symbol symbol = new Symbol(ParentToken);
            symbol.Top = Top;
            symbol.TopChangesSymbol = TopChangesSymbol;
            symbol.Text = Text;
            symbol.Bottom = Bottom;
            symbol.BottomChangesSymbol = BottomChangesSymbol;
            return symbol;
        }

        /// <summary>
        /// Returns the TextColor of this symbol by calling
        /// the property of the parent Token
        /// </summary>
        public SolidColorBrush TextColor
        {
            get
            {
                if (ParentToken != null)
                {
                    return ParentToken.TextColor;
                }
                return null;
            }
        }

        /// <summary>
        /// Returns the string representation of this symbol
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(Text);

            if (!string.IsNullOrEmpty(Top))
            {
                stringBuilder.Append("^" + Top);
            }
            if (!string.IsNullOrEmpty(Bottom))
            {
                stringBuilder.Append("_" + Bottom);
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Compares this Symbol to other objects (Symbols, Tokens, Strings)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            var symobl = obj as Symbol;
            if(symobl != null)
            {
                return symobl.Text.CompareTo(Text);
            }
            var token = obj as Token;
            if (token != null)
            {
                if (token.Symbols.Count > 0)
                {
                    return token.Symbols[0].CompareTo(Text);
                }
                return -1;
            }
            var str = obj as string;
            if (str != null)
            {

                return str.CompareTo(Text);
            }            
            return -1;            
        }

        /// <summary>
        /// Used in the user interface
        /// Returns 0 when there is no top or bottom text
        /// Returns 1 when there is a top text
        /// Returns 2 when there is a bottom text
        /// Returns 3 when there are top AND bottom texts
        /// </summary>
        public int SymbolDisplayType
        {
            get
            {
                int value = 0;
                if (!string.IsNullOrEmpty(Top) && !string.IsNullOrWhiteSpace(Top))
                {
                    value += 1;
                }
                if (!string.IsNullOrEmpty(Bottom) && !string.IsNullOrWhiteSpace(Bottom))
                {
                    value += 2;
                }
                return value;
            }
        }


    } 
}
