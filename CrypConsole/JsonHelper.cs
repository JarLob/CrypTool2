﻿/*
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
using Cryptool.PluginBase;

namespace Cryptool.CrypConsole
{
    public class JsonHelper
    {
        /// <summary>
        /// Returns the output string as json string
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        public static string GetOutputJsonString(string output, string name)
        {
            return string.Format("{{\"output\":{{\"name\":\"{0}\",\"value\":\"{1}\"}}}}", name, EscapeString(output));
        }
       
        /// <summary>
        /// Returns the log as json string
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string GetLogJsonString(IPlugin sender, GuiLogEventArgs args)
        {
            return string.Format("{{\"log\":{{\"logtime\":\"{0}\",\"logtype\":\"{1}\",\"sender\":\"{2}\",\"message\":\"{3}\"}}}}",
                DateTime.Now,
                args.NotificationLevel,
                sender == null ? "null" : sender.GetPluginInfoAttribute().Caption,
                EscapeString(args.Message == null ? "null" : args.Message));
        }

        /// <summary>
        /// returns the global progress as json string
        /// </summary>
        /// <param name="globalProgress"></param>
        /// <returns></returns>
        public static string GetProgressJson(int globalProgress)
        {
            return string.Format("{{\"progress\":{{\"value\":\"{0}\"}}}}", globalProgress);
        }

        /// <summary>
        /// Escapes the string by replacing \ with \\
        /// and '\r' with \r and '\n' with \n
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        private static string EscapeString(string output)
        {
            string newoutput = output.Replace("\\", "\\\\");
            newoutput = newoutput.Replace("\n", "\\n");
            newoutput = newoutput.Replace("\r", "\\r");
            return newoutput;
        }

    }
}
