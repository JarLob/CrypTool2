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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoluntLib2.Tools
{
    public enum Logtype
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    }

      /// <summary>
    /// Data Class of a log entry
    /// </summary>
    public class LogEntry
    {
        public string LogTime { get; set; }

        public string LogType { get; set; }
        
        public string Class { get; set; }

        public string Message { get; set; }
    }


    public class Logger
    {
        public event EventHandler<LogEventArgs> Logged;

        private static Logger Instance = new Logger();
        private static Logtype Loglevel = Logtype.Info;

        /// <summary>
        /// Singleton, thus private constructor
        /// </summary>
        private Logger()
        {
            //add file handling for logfile
        }

        /// <summary>
        /// Returns the global logger instance
        /// </summary>
        /// <returns></returns>
        public static Logger GetLogger()
        {
            return Instance;
        }

        /// <summary>
        /// Set minimum loglevel (default = info)
        /// </summary>
        /// <param name="loglevel"></param>
        public static void SetLogLevel(Logtype loglevel){
            Loglevel = loglevel;
        }
    
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
            if(logtype < Loglevel){
                return;
            }         
            lock (this)
            {
                switch (logtype)
                {
                    case Logtype.Debug:                        
                        Console.WriteLine("{0} {1} {2}: {3}", DateTime.Now, "Debug", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null", message);                        
                        break;
                    case Logtype.Info:
                        Console.WriteLine("{0} {1} {2}: {3}", DateTime.Now, "Info", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null", message);                        
                        break;
                    case Logtype.Warning:
                        Console.WriteLine("{0} {1} {2}: {3}", DateTime.Now, "Warning", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null", message);                        
                        break;
                    case Logtype.Error:
                        Console.Error.WriteLine("{0} {1} {2}: {3}", DateTime.Now, "Error", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null", message);                        
                        break;
                    default:
                        Console.WriteLine("{0} {1} {2}: {3}", DateTime.Now, "Unknown", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null", message);
                        break;
                }
                LoggOccured(String.Format("{0} {1}", (whoLoggs != null ? whoLoggs.GetType().FullName : "null"), message), logtype);
            }
        }

        /// <summary>
        /// Fires the Logged event
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logtype"></param>
        private void LoggOccured(string message, Logtype logtype)
        {
            if (Logged != null)
            {
                Logged.BeginInvoke(this, new LogEventArgs(logtype, message), null, null);
            }
        }

        /// <summary>
        /// Logs a given exception
        /// whoLoggs should be set to a reference to the object that wants to log
        /// logtype is LogType (Debug, Info, Warning, Error)
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="whoLoggs"></param>
        /// <param name="logtype"></param>
        public void LogException(Exception ex, object whoLoggs, Logtype logtype)
        {
            if(logtype < Loglevel){
                return;
            }
                        
            lock (this)
            {
                switch (logtype)
                {
                    case Logtype.Debug:
                        Console.WriteLine("{0} {1} {2}: Stacktrace:", DateTime.Now, "Debug", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null");
                        Console.WriteLine(ex.StackTrace);                        
                        break;
                    case Logtype.Info:
                        Console.WriteLine("{0} {1} {2}: Stacktrace:", DateTime.Now, "Info", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null");
                        Console.WriteLine(ex.StackTrace);                        
                        break;
                    case Logtype.Warning:
                        Console.WriteLine("{0} {1} {2}: Stacktrace:", DateTime.Now, "Warning", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null");
                        Console.WriteLine(ex.StackTrace);
                        break;
                    case Logtype.Error:
                        Console.WriteLine("{0} {1} {2}: Stacktrace:", DateTime.Now, "Error", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null");
                        Console.Error.WriteLine(ex.StackTrace);
                        break;
                    default:
                        Console.WriteLine("{0} {1} {2}: Stacktrace:", DateTime.Now, "Unknown", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null");
                        Console.WriteLine(ex.StackTrace);
                        break;
                }
                LoggOccured(String.Format("{0} {1}: Stacktrace: {2}", (whoLoggs != null ? whoLoggs.GetType().FullName : "null"), ex.Message, ex.StackTrace), logtype);
            }
        }
    }

    public class LogEventArgs : EventArgs
    {
        public Logtype Logtype { private set; get; }
        public string Message { private set; get; }

        public LogEventArgs(Logtype logtype, string message)
        {
            Logtype = logtype;
            Message = message;
        }
    }
}
