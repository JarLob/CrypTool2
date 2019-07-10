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

namespace Cryptool.Plugins.DECODEDatabaseTools
{
    /// <summary>
    /// A document contains one or more pages
    /// </summary>
    public class TextDocument
    {
        public TextDocument()
        {
            Pages = new List<Page>();
        }

        public List<Page> Pages
        {
            get;
            set;
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
        public Page()
        {
            Lines = new List<Line>();
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
            set
            {
                //do nothing
            }
        }
    }

    /// <summary>
    /// Type of the line
    /// </summary>
    public enum LineType
    {
        Comment = 0,
        Text = 1
    }

    /// <summary>
    /// A line is a single line of text consisting of tokens
    /// </summary>
    public class Line
    {
        public Line()
        {
            Tokens = new List<Token>();
            LineType = LineType.Text;
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
    }

    /// <summary>
    /// Type of tokens
    /// </summary>
    public enum TokenType
    {
        Unknown = 0,
        Ciphertext = 1,
        Plaintext = 2,
        Cleartext = 3
    }

    /// <summary>
    /// A Token is a single element of text
    /// </summary>
    public class Token
    {
        public Token()
        {
            TokenType = TokenType.Unknown;
        }

        public string Text
        {
            get; set;
        }

        public TokenType TokenType
        {
            get; set;
        }
    }

}
