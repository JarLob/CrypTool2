/*
   Copyright 2018 Nils Kopal <Nils.Kopal<AT>Uni-Kassel.de>

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
using CrypToolStoreLib.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrypToolStoreBuildSystem
{
    /// <summary>
    /// This special logger wraps the actual logger
    /// It also creates a complete log using a string builder which will be uploaded
    /// to the database at the end of a build process
    /// </summary>
    public class BuildLogger
    {
        private Logger Logger = Logger.GetLogger();
        private StringBuilder LogStringBuilder = new StringBuilder();

        /// <summary>
        /// Logs a given text
        /// whoLoggs should be set to a reference to the object that wants to log
        /// logtype is LogType (Debug, Info, Warning, Error)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="whoLoggs"></param>
        /// <param name="logtype"></param>
        public void LogText(string message, object whoLoggs, Logtype logtype)
        {
            lock (this)
            {
                string entry;
                switch (logtype)
                {
                    //here, write everything to the LogStringBuilder
                    case Logtype.Debug:
                        entry = String.Format("{0} {1} {2}: {3}", DateTime.Now, "Debug", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null", message);
                        LogStringBuilder.AppendLine(entry);
                        break;
                    case Logtype.Info:
                        entry = String.Format("{0} {1} {2}: {3}", DateTime.Now, "Info", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null", message);
                        LogStringBuilder.AppendLine(entry);
                        break;
                    case Logtype.Warning:
                        entry = String.Format("{0} {1} {2}: {3}", DateTime.Now, "Warning", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null", message);
                        LogStringBuilder.AppendLine(entry);
                        break;
                    case Logtype.Error:
                        entry = String.Format("{0} {1} {2}: {3}", DateTime.Now, "Error", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null", message);
                        LogStringBuilder.AppendLine(entry);
                        break;
                    default:
                        entry = String.Format("{0} {1} {2}: {3}", DateTime.Now, "Unknown", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null", message);
                        LogStringBuilder.AppendLine(entry);
                        break;
                }
                //finally, forward the log to the actual logger
                Logger.LogText(entry, whoLoggs, logtype);
            }
        }

    }
}
