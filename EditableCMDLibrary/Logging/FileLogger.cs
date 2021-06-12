using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace uk.JohnCook.dotnet.EditableCMDLibrary.Logging
{
    /// <summary>
    /// A class for writing log-like data to text files
    /// </summary>
    public class FileLogger
    {
        /// <summary>
        /// The directory to save the log-like file.
        /// </summary>
        public string LogDirectory { get; }
        /// <summary>
        /// The filename of the log-like file (does not include the path).
        /// </summary>
        public string LogFile { get; }
        /// <summary>
        /// The description of the file/log used for printing errors during debugging.
        /// </summary>
        public string LogDescription { get; set; }

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
        /// <param name="s">The string to write.</param>
        /// <param name="append">True to append to the file. False to truncate the file if it exists and replace its content with the formmated string.</param>
        /// <returns>True on success.</returns>
        public bool Log(string s, bool append = true)
        {
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
            }
            catch (Exception e)
            {
                Trace.WriteLine(string.Format("Unable to write to {0} - {1}", LogDescription, e.Message));
                return false;
            }
            return true;
        }

        /// <summary>
        /// Writes a formmated string to the logger's file, using <see cref="string.Format(string, object[])"/>.
        /// </summary>
        /// <param name="format">The format string to pass to <see cref="string.Format(string, object?)"/>.</param>
        /// <param name="args">The arguments to pass to <see cref="string.Format(string, object?)"/>.</param>
        /// <param name="append">True to append to the file. False to truncate the file if it exists and replace its content with the formmated string.</param>
        /// <returns>True on success.</returns>
        public bool FormattedLog(string format, object[] args, bool append = true)
        {
            return Log(string.Format(format, args), append);
        }

        /// <summary>
        /// Deletes this logger's file from the disk.
        /// </summary>
        public void DeleteFile()
        {
            if (File.Exists(LogFile) &&
                !Directory.Exists(LogFile) &&
                !LogFile.Equals(Path.GetTempPath()) &&
                !LogFile.Equals(Environment.GetFolderPath(Environment.SpecialFolder.Windows)) &&
                !LogFile.Equals(Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.Windows).ToString())))
            {
                File.Delete(LogFile);
            }
        }

        /// <summary>
        /// Creates a formmated string for an input entry.
        /// </summary>
        /// <param name="timestamp">The timestamp of the input.</param>
        /// <param name="path">The directory path of the input (the string that will appear after the timestamp and before the input).</param>
        /// <param name="input">The input command or text.</param>
        /// <param name="endInputKey">The key pressed to complete the entry, such as <see cref="ConsoleKey.Enter"/>.</param>
        /// <returns>The formatted string.</returns>
        public static string FormatInputLogEntry(DateTime timestamp, string path, string input, ConsoleKey endInputKey)
        {
            string finalKey = endInputKey == ConsoleKey.Enter ? strings.logEnter : string.Format("{0}", endInputKey.ToString());
            return string.Concat(strings.logPrecedingCharacter, timestamp.ToString(strings.logDateFormat), strings.logDateTimezoneZulu, strings.logProceedingCharacter,
                            path, ">", input,
                            strings.logPrecedingCharacter, finalKey, strings.logProceedingCharacter);
        }

        /// <summary>
        /// Creates a formatted string for a cancelled input entry.
        /// </summary>
        /// <param name="timestamp">The timestamp of the input.</param>
        /// <param name="path">The directory path of the input (the string that will appear after the timestamp and before the input).</param>
        /// <param name="input">The input command or text.</param>
        /// <param name="endInputKey">The key pressed to complete the cancelled entry, such as <see cref="ConsoleSpecialKey.ControlC"/>.</param>
        /// <returns>The formatted string.</returns>
        public static string FormatCancelledInputLogEntry(DateTime timestamp, string path, string input, ConsoleSpecialKey endInputKey)
        {
            string finalKey = string.Empty;
            switch (endInputKey)
            {
                case ConsoleSpecialKey.ControlBreak:
                    finalKey = strings.logCtrlBreak;
                    break;
                case ConsoleSpecialKey.ControlC:
                    finalKey = strings.logCtrlC;
                    break;
            }
            return string.Concat(strings.logPrecedingCharacter, timestamp.ToString(strings.logDateFormat), strings.logDateTimezoneZulu, strings.logProceedingCharacter,
                            path, ">", input,
                            strings.logPrecedingCharacter, finalKey, strings.logProceedingCharacter);
        }
    }
}
