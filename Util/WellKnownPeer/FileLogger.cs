using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VoluntLib2.Tools;

namespace WellKnownPeer
{
    public class FileLogger : IDisposable
    {
        private DateTime _LogFileOpenedTime;
        private FileStream _FileStream;
        private StreamWriter _StreamWriter;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public FileLogger()
        {
            if (!Directory.Exists("Logs"))
            {
                Directory.CreateDirectory("Logs");
            }

        }

        /// <summary>
        /// Writes a log entry to the log file
        /// Opens a new log file if necessary
        /// </summary>
        /// <param name="whoLoggs"></param>
        /// <param name="logeventargs"></param>
        public void OnLoggOccured(object whoLoggs, LogEventArgs logeventargs)
        {
            lock (this)
            {
                try
                {
                    if (_FileStream == null)
                    {
                        CreateAndOpenLogFile();
                    }
                    if (DateTime.Now > _LogFileOpenedTime.AddDays(1))
                    {
                        _StreamWriter.Flush();
                        _StreamWriter.Close();
                        CreateAndOpenLogFile();
                        DeleteOldLogfiles();
                    }

                    switch (logeventargs.Logtype)
                    {
                        case Logtype.Debug:
                            _StreamWriter.WriteLine("{0} {1} {2}: {3}", DateTime.Now, "Debug", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null", logeventargs.Message);
                            break;
                        case Logtype.Info:
                            _StreamWriter.WriteLine("{0} {1} {2}: {3}", DateTime.Now, "Info", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null", logeventargs.Message);
                            break;
                        case Logtype.Warning:
                            _StreamWriter.WriteLine("{0} {1} {2}: {3}", DateTime.Now, "Warning", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null", logeventargs.Message);
                            break;
                        case Logtype.Error:
                            _StreamWriter.WriteLine("{0} {1} {2}: {3}", DateTime.Now, "Error", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null", logeventargs.Message);
                            break;
                        default:
                            _StreamWriter.WriteLine("{0} {1} {2}: {3}", DateTime.Now, "Unknown", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null", logeventargs.Message);
                            break;
                    }
                    _StreamWriter.Flush();
                    _FileStream.Flush();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(String.Format("Exception in FileLogger: {0}", ex.Message));
                    Console.Error.WriteLine(ex.StackTrace);
                }
            }
        }

        private void DeleteOldLogfiles()
        {
            foreach (var filename in Directory.GetFiles("Logs"))
            {
                DateTime creationdate = File.GetCreationTime(filename);
                if ( DateTime.Now > creationdate.AddDays(31))
                {
                    File.Delete(filename);
                }
            }
        }

        /// <summary>
        /// Creates a new logfile and opens it
        /// </summary>
        private void CreateAndOpenLogFile()
        {
            string logfilename = "logs\\WellKnownPeer_" + DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "_" + DateTime.Now.Minute + ".log";
            _FileStream = new FileStream(logfilename, FileMode.Create, FileAccess.Write);
            _StreamWriter = new StreamWriter(_FileStream);
            _LogFileOpenedTime = DateTime.Now;
        }

        /// <summary>
        /// Closes the current open logfile
        /// </summary>
        public void Dispose()
        {
            _StreamWriter.Flush();
            _StreamWriter.Close();
        }
    }
}
