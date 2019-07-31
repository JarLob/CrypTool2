using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cryptool.Plugins.DECODEDatabaseTools
{
    public class Preprocessor
    {
        /// <summary>
        /// Text Document to parse
        /// </summary>
        public string DECODETextDocument
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the proprocessed line
        /// </summary>
        /// <returns></returns>
        public string GetProcessedString()
        {
            StringBuilder builder = new StringBuilder();
            string[] lines = DECODETextDocument.Split(new char[] { '\r', '\n' });
            
            foreach(var line in lines)
            {
                string processedLine = line;
                processedLine = Regex.Replace(processedLine, @"0\s*_\s*\.", "A");
                processedLine = Regex.Replace(processedLine, @"1\s*_\s*\.", "B");
                processedLine = Regex.Replace(processedLine, @"2\s*_\s*\.", "C");
                processedLine = Regex.Replace(processedLine, @"3\s*_\s*\.", "D");
                processedLine = Regex.Replace(processedLine, @"4\s*_\s*\.", "E");
                processedLine = Regex.Replace(processedLine, @"5\s*_\s*\.", "F");
                processedLine = Regex.Replace(processedLine, @"6\s*_\s*\.", "G");
                processedLine = Regex.Replace(processedLine, @"7\s*_\s*\.", "H");
                processedLine = Regex.Replace(processedLine, @"8\s*_\s*\.", "I");
                processedLine = Regex.Replace(processedLine, @"9\s*_\s*\.", "J");

                processedLine = Regex.Replace(processedLine, @"0\s*\^\s*\.", "a");
                processedLine = Regex.Replace(processedLine, @"1\s*\^\s*\.", "b");
                processedLine = Regex.Replace(processedLine, @"2\s*\^\s*\.", "c");
                processedLine = Regex.Replace(processedLine, @"3\s*\^\s*\.", "d");
                processedLine = Regex.Replace(processedLine, @"4\s*\^\s*\.", "e");
                processedLine = Regex.Replace(processedLine, @"5\s*\^\s*\.", "f");
                processedLine = Regex.Replace(processedLine, @"6\s*\^\s*\.", "g");
                processedLine = Regex.Replace(processedLine, @"7\s*\^\s*\.", "h");
                processedLine = Regex.Replace(processedLine, @"8\s*\^\s*\.", "i");
                processedLine = Regex.Replace(processedLine, @"9\s*\^\s*\.", "j");

                processedLine = Regex.Replace(processedLine, @"0\s*\^\s*`", "k");
                processedLine = Regex.Replace(processedLine, @"1\s*\^\s*`", "l");
                processedLine = Regex.Replace(processedLine, @"2\s*\^\s*`", "m");
                processedLine = Regex.Replace(processedLine, @"3\s*\^\s*`", "n");
                processedLine = Regex.Replace(processedLine, @"4\s*\^\s*`", "o");
                processedLine = Regex.Replace(processedLine, @"5\s*\^\s*`", "p");
                processedLine = Regex.Replace(processedLine, @"6\s*\^\s*`", "q");
                processedLine = Regex.Replace(processedLine, @"7\s*\^\s*`", "r");
                processedLine = Regex.Replace(processedLine, @"8\s*\^\s*`", "s");
                processedLine = Regex.Replace(processedLine, @"9\s*\^\s*`", "t");

                builder.AppendLine(processedLine);
            }
            return builder.ToString();
        }

    }
}
