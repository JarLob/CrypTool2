/*
   Copyright 2020 Nils Kopal <kopal<AT>cryptool.org>

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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptool.CrypConsole
{
    public class ArgsHelper
    {        
        /// <summary>
        /// Returns the filename of the first cwm entry in args
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string GetCWMFileName(string[] args)
        {
            var query = from str in args
                        where (str.Length >= 5 && str.ToLower().Substring(0, 5).Equals("-cwm=")) 
                           || (str.Length >= 6 && str.ToLower().Substring(0, 6).Equals("--cwm="))
                        select str;

            if (query.Count() > 0)
            {
                var filename = query.First().Split('=')[1];
                if (filename.StartsWith("\""))
                {
                    filename = filename.Substring(1, filename.Length - 1);
                }
                if (filename.EndsWith("\""))
                {
                    filename = filename.Substring(0, filename.Length - 1);
                }
                return filename;
            }
            return null;
        }

        public static bool CheckVerbose(string[] args)
        {
            var query = from str in args
                        where (str.Length >= 8 && str.ToLower().Substring(0, 8).Equals("-verbose"))
                           || (str.Length >= 9 && str.ToLower().Substring(0, 9).Equals("--verbose"))
                        select str;

            if (query.Count() > 0)
            {
                return true;
            }
            return false;
        }

        public static List<string> GetInputParameters(string[] args)
        {
            var query = from str in args
                        where (str.Length >= 7 && str.ToLower().Substring(0, 7).Equals("-input=")) 
                            || (str.Length >= 8 && str.ToLower().Substring(0, 8).Equals("--input="))
                        select str;

            List<string> parameters = new List<string>();
            foreach(var param in query)
            {
                var p = param.Split('=')[1];
                if (p.StartsWith("\""))
                {
                    p = p.Substring(1, p.Length - 1);
                }
                if (p.EndsWith("\""))
                {
                    p = p.Substring(0, p.Length - 1);
                }
                parameters.Add(p);
            }
            return parameters;
        }


        /// <summary>
        /// This method checks the args, if the user wants to see the help
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool CheckShowHelp(string[] args)
        {
            var query = from str in args
                        where str.ToLower().Equals("--help") || str.ToLower().Equals("-help")
                        select str;

            //we show help, if requested or no parameters were given
            if (args.Length == 0 || query.Count() > 0)
            {
                ShowHelp();
                return true;
            }
            return false;
        }

        /// <summary>
        ///  Shows the help
        /// </summary>
        public static void ShowHelp()
        {
            Console.WriteLine("");
            Console.WriteLine("-= CrypConsole -- a CrypTool 2 Console for executing CrypTool 2 workspaces in the Windows console =- ");
            Console.WriteLine("(C) 2020 by Nils Kopal, kopal@cryptool.org");
            Console.WriteLine("Usage:");
            Console.WriteLine("CrypConsole.exe -cwm=path/to/cwm/file -input=<input param definition> -output=<output param definition>");
            Console.WriteLine("");
            Console.WriteLine(" -help                               -> shows this help page");
            Console.WriteLine(" -verbose                            -> writes logs etc to the console; for debugging");
            Console.WriteLine(" -cwm=path/to/cwm/file               -> specifies a path to a cwm file that should be executed");
            Console.WriteLine(" -input=<input param definition>     -> specifies an input parameter");
            Console.WriteLine(" -output==<output param definition>  -> specifies an output parameter");
        }
    }

    public enum ParameterType
    {
        Number,
        Text,
        Hex,
        File
    }

    public class Parameter
    {
        public ParameterType ParameterType
        {
            get; 
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string Value
        {
            get;
            set;
        }

    }
}
