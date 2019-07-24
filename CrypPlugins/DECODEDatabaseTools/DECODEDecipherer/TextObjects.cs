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

namespace Cryptool.Plugins.DECODEDatabaseTools
{

    /// <summary>
    /// Type of tokens
    /// </summary>
    public enum TokenType
    {
        Unknown = 0,
        Null = 1,
        RegularCode = 2,
        VocabularyElement = 3,
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
            foreach(var page in Pages)
            {
                stringBuilder.Append(page.ToString());
                stringBuilder.Append(Environment.NewLine);
            }
            return stringBuilder.ToString();
        }
      
    }

    /// <summary>
    /// A page contains several lines of text
    /// </summary>
    public class Page
    {
        private TextDocument _textDocument;

        public int PageNumber { get; set; }

        public Page(TextDocument textDocument)
        {
            Lines = new List<Line>();
            _textDocument = textDocument;
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

        public TextDocument GetParentTextDocument()
        {
            return _textDocument;
        }
    }

    /// <summary>
    /// A line is a single line of text consisting of tokens
    /// </summary>
    public class Line
    {
        private Page _page;

        public int LineNumber { get; set; }

        public Line(Page page)
        {
            Tokens = new List<Token>();
            LineType = LineType.Text;
            _page = page;
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
                stringBuilder.Append(token.Text);
                if (token != Tokens[Tokens.Count - 1])
                {
                    stringBuilder.Append(" ");
                }
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

        public Page GetParentPage()
        {
            return _page;
        }
    }

    /// <summary>
    /// A Token is a single element of text
    /// </summary>
    public class Token
    {
        private Line _line = null;

        public Token(Line line)
        {
            TokenType = TokenType.Unknown;
            _line = line;
            DecodedText = null;
        }

        public string Text
        {
            get; set;
        }

        public string DecodedText
        {
            get;set;
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
                switch (_line.LineType)
                {
                    case LineType.Text:
                        switch (TokenType)
                        {
                            case TokenType.Tag:
                                return Brushes.Black;
                            case TokenType.Null:
                                return Brushes.Gray;
                            case TokenType.VocabularyElement:
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

        public Line GetParentLine()
        {
            return _line;
        }
    }

}
