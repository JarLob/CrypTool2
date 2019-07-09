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

namespace Cryptool.Plugins.DECODEDatabaseTools
{
    /// <summary>
    /// Abstract class for a parser
    /// </summary>
    public abstract class Parser
    {
        public Parser()
        {

        }

        public string DECODETextDocument
        {
            get;
            set;
        }

        public abstract TextDocument GetDocument();
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

            TextDocument document = new TextDocument();

            Page currentPage = new Page();
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
                //comments in the DECODE transcription format start with #
                if (trimmedLine.StartsWith("#"))
                {
                    string comment = trimmedLine.Substring(1, trimmedLine.Length - 1).ToUpper();

                    line.LineType = LineType.Comment;
                    if(comment.StartsWith("IMAGE NAME") || comment.StartsWith("PAGE"))
                    {
                        //at each IMAGE NAME comment, a new page (image) starts
                        currentPage = new Page();
                        document.Pages.Add(currentPage);
                    }
                }
                Token token = new Token();
                token.Text = trimmedLine;
                line.Tokens.Add(token);
                currentPage.Lines.Add(line);
            }
            return document;
        }
    }
}
