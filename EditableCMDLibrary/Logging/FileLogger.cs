using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace uk.JohnCook.dotnet.EditableCMDLibrary.Logging
{
    /// <summary>
    /// A class for writing log-like data to text files
    /// </summary>
    public class FileLogger : IInputLogger
    {
        /// <summary>
        /// The directory to save the log-like file.
        /// </summary>
        public string LogDirectory { get; }
        /// <summary>
        /// The filename of the log-like file (does not include the path).
        /// </summary>
        public string LogFile { get; }
        /// <inheritdoc cref="IInputLogger.LogDescription"/>
        public string LogDescription { get; set; }
        /// <inheritdoc cref="IInputLogger.IsWriteable"/>
        public bool IsWriteable { get; private set; }

        /// <summary>
        /// An instance of a text file writer for log-like files.
        /// </summary>
        /// <param name="directory">The directory to save the file.</param>
        /// <param name="file">The filename (does not include the path).</param>
        /// <param name="description">A description of the file/log used for printing errors during debugging.</param>
        public FileLogger(string directory, string file, string description)
        {
            LogDirectory = directory;
            LogFile = Path.Combine(LogDirectory, file);
            LogDescription = description;
        }

        /// <summary>
        /// Writes a string to the logger's file.
        /// </summary>
        /// <inheritdoc cref="IInputLogger.Log(string, bool)" path="param|returns"/>
        public bool Log(string s, bool append = true)
        {
            if (LogDirectory is null || LogFile is null)
            {
                return false;
            }
            if (!Directory.Exists(LogDirectory))
            {
                try
                {
                    Directory.CreateDirectory(LogDirectory);
                }
                catch (Exception e)
                {
                    Trace.WriteLine(string.Format("Unable to create {0} directory - {1}", LogDescription, e.Message));
                    return false;
                }
            }
            try
            {
                if (append)
                {
                    File.AppendAllText(LogFile, string.Concat(s, Environment.NewLine), Encoding.UTF8);
                }
                else
                {
                    File.WriteAllText(LogFile, string.Concat(s, Environment.NewLine), Encoding.UTF8);
                }
                IsWriteable = true;
            }
            catch (Exception e)
            {
                IsWriteable = false;
                Trace.WriteLine(string.Format("Unable to write to {0} - {1}", LogDescription, e.Message));
                return false;
            }
            return true;
        }

        /// <summary>
        /// Writes a formmated string to the logger's file, using <see cref="string.Format(string, object[])"/>.
        /// </summary>
        /// <inheritdoc cref="IInputLogger.FormattedLog(string, object[], bool)" path="param|returns"/>
        public bool FormattedLog(string format, object[] args, bool append = true)
        {
            return Log(string.Format(format, args), append);
        }

        /// <summary>
        /// Deletes this logger's file from the disk.
        /// </summary>
        public void DeleteFile()
        {
            if (LogDirectory is null && LogFile is null)
            {
                return;
            }
            if (File.Exists(LogFile) &&
                !Directory.Exists(LogFile) &&
                !LogFile.Equals(Path.GetTempPath()) &&
                !LogFile.Equals(Environment.GetFolderPath(Environment.SpecialFolder.Windows)) &&
                !LogFile.Equals(Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.Windows).ToString())))
            {
                IsWriteable = false;
                File.Delete(LogFile);
            }
        }
    }
}
