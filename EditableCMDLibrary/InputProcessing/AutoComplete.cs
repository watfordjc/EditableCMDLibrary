using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using uk.JohnCook.dotnet.EditableCMDLibrary.ConsoleSessions;
using uk.JohnCook.dotnet.EditableCMDLibrary.Interop;
using uk.JohnCook.dotnet.EditableCMDLibrary.Utils;

namespace uk.JohnCook.dotnet.EditableCMDLibrary.InputProcessing
{
    /// <summary>
    /// Class for autocomplete functions
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class AutoComplete
    {
        private AutoCompleteMode autoCompleteMode = AutoCompleteMode.None;
        private string[]? autoCompleteList;
        private int autoCompleteListPosition = -1;
        private string autoCompletePrecedingText = string.Empty;
        private bool autoCompleteRetainDoubleQuotes = false;
        private readonly ConsoleState state;

        /// <summary>
        /// Creates an autocomplete session.
        /// </summary>
        /// <param name="state">The <see cref="ConsoleState"/> the session is linked to.</param>
        public AutoComplete(ConsoleState state)
        {
            this.state = state;
        }

        /// <summary>
        /// <para>
        /// Resets an autocomplete session if it is in the wrong mode.
        /// </para>
        /// <para>
        /// Call before calling <see cref="Complete(ConsoleState, bool)"/>.
        /// </para>
        /// </summary>
        public void Reset()
        {
            if (autoCompleteMode != AutoCompleteMode.Tab)
            {
                autoCompleteMode = AutoCompleteMode.Tab;
                autoCompleteList = null;
                autoCompleteListPosition = -1;
            }
        }

        /// <summary>
        /// Complete the next autocomplete operation if possible, creating a new autocompletion list first if either <see cref="Reset"/> or <see cref="AutoCompleteEnd"/> have been called.
        /// </summary>
        /// <param name="state">The <see cref="ConsoleState"/> for this autocomplete session.</param>
        /// <param name="reverseDirection">Whether autocomplete should operate in the reverse direction (i.e. Shift+Tab)</param>
        public void Complete(ConsoleState state, bool reverseDirection)
        {
            NativeMethods.COORD currentPosition = ConsoleCursorUtils.GetCurrentCursorPosition();
            int positionInCommand = ConsoleCursorUtils.CursorPositionToCharPosition(state, currentPosition);
            int stringStartPosition = 0;
            string currentString = string.Empty;
            List<int> doubleQuotePositions = new();
            bool evenPrecedingDoubleQuotes = true;
            // The rules only need following if the cursor isn't at position 0
            if (!currentPosition.Equals(state.Input.StartPosition))
            {
                // Index the position of all non-escaped double quotes
                for (int pos = state.Input.Text.ToString().IndexOf('"'); pos > -1; pos = state.Input.Text.ToString().IndexOf('"', pos + 1))
                {
                    // Rule 1: If a double quote is preceded by a slash, ignore it.
                    if (pos > 1 && state.Input.Text[pos - 1] != '\\')
                    {
                        doubleQuotePositions.Add(pos);
                    }
                    else
                    {
                        doubleQuotePositions.Add(pos);
                    }
                }
                // If there are double quotes, use double quote and space rules
                if (doubleQuotePositions.Count > 0)
                {
                    // Count the number of double quotes before the cursor
                    int precedingDoubleQuoteCount = doubleQuotePositions.Where(x => x < positionInCommand).Count();
                    // Determine if there are an even number of quotes before the cursor
                    evenPrecedingDoubleQuotes = precedingDoubleQuoteCount % 2 == 0;
                    // Rule 2: If there are 2n+1 double quotes before the cursor, the 2n+1 double quote starts the string
                    stringStartPosition = evenPrecedingDoubleQuotes ? doubleQuotePositions[^2] : doubleQuotePositions[^1];
                    // Rule 3: If there are 2n double quotes before cursor, the 2n-1 double quote might start the string
                    if (evenPrecedingDoubleQuotes)
                    {
                        // Create a string that starts at the relevant double quote and ends at the cursor
                        string toSearch = state.Input.Text.ToString()[stringStartPosition..positionInCommand];
                        // Rule 3b: If there is a space after the 2n-1 double quote, the last space before the cursor starts the string
                        stringStartPosition = toSearch.Contains(' ') ? toSearch.LastIndexOf(' ') + 1 : stringStartPosition;
                    }
                }
                // If there are no double quotes, use space rules
                else
                {
                    string toSearch = state.Input.Text.ToString().Substring(0, positionInCommand);
                    // Rule 4: If there are no double quotes but there are spaces, the string starts at the last space before the cursor
                    // Rule 5: If there are no double quotes or spaces, the string starts at the start of input (position 0)
                    stringStartPosition = toSearch.Contains(' ') ? toSearch.LastIndexOf(' ') + 1 : 0;
                }
                // Create the search string - everything after the cursor is discarded
                currentString = state.Input.Text.ToString()[stringStartPosition..positionInCommand];
                // If Rule 3 applied on the first autocompletion, the double quotes get retained.
                if (autoCompleteList == null)
                {
                    autoCompleteRetainDoubleQuotes = currentString.Length > 0 && currentString[0] == '"';
                }
            }
            // Remove any double quotes from the search string
            currentString = currentString.Replace("\"", string.Empty);
            // If on the first autocompletion, cycle to the first (Tab) or last (Shift+Tab) result
            if (autoCompleteList == null)
            {
                // TODO: Look at preceding word, if any, to switch between file/directory/both completion
                string autoCompleteEnvVar = string.Empty;
                string? autoCompleteEnvVarString = string.Empty;
                string autoCompleteDir = state.CurrentDirectory;
                string autoCompleteFile = currentString;
                // If there is a %, try to parse as an environment variable
                if (currentString.Contains("%"))
                {
                    string[] dirPercentSplit = currentString.Split('%', 3);
                    if (dirPercentSplit.Length == 3)
                    {
                        autoCompleteEnvVarString = Environment.GetEnvironmentVariable(dirPercentSplit[1]);
                        if (autoCompleteEnvVarString != null)
                        {
                            autoCompleteEnvVar = dirPercentSplit[1];
                            autoCompleteDir = autoCompleteEnvVarString;
                            autoCompleteFile = dirPercentSplit[2];
                            if (autoCompleteFile.Contains(Path.DirectorySeparatorChar))
                            {
                                int lastSeparatorCharPosition = autoCompleteFile.LastIndexOf(Path.DirectorySeparatorChar);
                                autoCompleteDir += autoCompleteFile[0..lastSeparatorCharPosition];
                                autoCompleteFile = autoCompleteFile[(lastSeparatorCharPosition + 1)..];
                            }
                        }
                        else
                        {
                            autoCompleteEnvVarString = string.Empty;
                        }
                    }
                }
                // If there is a /, try to split the path into a diretory and file
                if (autoCompleteFile.Contains(Path.DirectorySeparatorChar))
                {
                    int lastSeparatorCharPosition = currentString.LastIndexOf(Path.DirectorySeparatorChar);
                    autoCompleteDir = currentString[0..lastSeparatorCharPosition];
                    if (string.IsNullOrEmpty(autoCompleteDir))
                    {
                        if (StringUtils.TryGetPathRoot(state.CurrentDirectory, out string? pathRoot))
                        {
                            autoCompleteDir = pathRoot;
                        }
                    }
                    autoCompleteFile = currentString[(lastSeparatorCharPosition + 1)..];
                }
                // If there is a :, try to parse a drive letter
                if ((autoCompleteDir == state.CurrentDirectory && autoCompleteFile.Contains(":")) || autoCompleteDir.Contains(":"))
                {
                    string[]? dirColonSplit = null;
                    if (currentString.Length > 0)
                    {
                        dirColonSplit = currentString.Split(':', 2);
                    }
                    else if (Path.GetPathRoot(autoCompleteDir)?.Equals(autoCompleteDir) == true)
                    {
                        dirColonSplit = autoCompleteDir.Split(':', 2);
                    }
                    if (dirColonSplit?.Length == 2)
                    {
                        string constructedPathRoot = string.Concat(dirColonSplit[0], ":\\");
                        if (StringUtils.TryGetPathRoot(constructedPathRoot, out string? pathRoot))
                        {
                            autoCompleteDir = pathRoot;
                        }
                        autoCompleteFile = dirColonSplit[1];
                        if (autoCompleteFile.Contains(Path.DirectorySeparatorChar))
                        {
                            int lastSeparatorCharPosition = autoCompleteFile.LastIndexOf(Path.DirectorySeparatorChar);
                            autoCompleteDir += autoCompleteFile[0..lastSeparatorCharPosition];
                            autoCompleteFile = autoCompleteFile[(lastSeparatorCharPosition + 1)..];
                        }
                    }
                }
                // If the directory doesn't contain a slash or starts with relative path, it should be relative
                if (!autoCompleteDir.Contains(':') || !autoCompleteDir.Contains('\\') || autoCompleteDir.StartsWith("..\\") || autoCompleteDir.StartsWith(".\\"))
                {
                    autoCompleteDir = Path.Combine(state.CurrentDirectory, autoCompleteDir);
                }
                // Reserved characters in Windows not valid in filenames: https://docs.microsoft.com/en-us/windows/win32/fileio/naming-a-file
                string[] invalidFileChars = new string[] { "<", ">", ":", "\"", "/", "\\", "|", "?", "*", "\0" };
                if (Directory.Exists(autoCompleteDir) && !invalidFileChars.Any(autoCompleteFile.Contains))
                {
                    // Generate auto-complete dictionary
                    autoCompleteList = Directory.GetFileSystemEntries(autoCompleteDir, string.Concat(autoCompleteFile, "*"), SearchOption.TopDirectoryOnly);
                    // The search path starts with an environment variable
                    if (!string.IsNullOrEmpty(autoCompleteEnvVarString) && !string.IsNullOrEmpty(autoCompleteEnvVar))
                    {
                        // Replace environment variable value with %name%
                        autoCompleteList = autoCompleteList.Select(path => path.Replace(autoCompleteEnvVarString, string.Concat("%", autoCompleteEnvVar, "%"))).ToArray();
                    }
                    // The search path is the current directory
                    else if (autoCompleteDir == state.CurrentDirectory)
                    {
                        // Remove current directory if input is not an absolute path or relative from root of the drive
                        if (!currentString.Contains(':') && !currentString.StartsWith('\\') && !autoCompleteDir.Equals(Path.GetPathRoot(autoCompleteDir)))
                        {
                            autoCompleteList = autoCompleteList.Select(path => path.Replace(state.CurrentDirectory + "\\", "")).ToArray();
                        }
                        // The search path is also the root of the drive but input is not an absolute path 
                        else if (!currentString.Contains(':') && autoCompleteDir.Equals(Path.GetPathRoot(autoCompleteDir)))
                        {
                            // Input uses a leading slash, replace search directory with leading slash
                            if (currentString.StartsWith('\\'))
                            {
                                autoCompleteList = autoCompleteList.Select(path => path.Replace(autoCompleteDir, "\\")).ToArray();
                            }
                            // Input does not use a leading slash, remove search directory
                            else
                            {
                                autoCompleteList = autoCompleteList.Select(path => path.Replace(autoCompleteDir, "")).ToArray();
                            }
                        }
                    }
                    // The search path is relative to the root of the drive, fix double slash after colon - TODO: Make earlier parsing do this
                    else if (currentString.StartsWith('\\'))
                    {
                        if (StringUtils.TryGetPathRoot(autoCompleteDir, out string? pathRoot))
                        {
                            autoCompleteList = autoCompleteList.Select(path => path.Replace(pathRoot, "\\")).ToArray();
                        }
                    }
                    // The search path is relative to the current directory
                    else if (!currentString.Contains(':'))
                    {
                        autoCompleteList = autoCompleteList.Select(path => path.Replace(state.CurrentDirectory + "\\", "")).ToArray();
                    }
                    // The search path is absolute
                    else if (currentString.Contains(':'))
                    {
                        autoCompleteList = autoCompleteList.Select(path => path.Replace(autoCompleteDir, Path.GetFullPath(autoCompleteDir))).ToArray();
                    }
                }
                else
                {
                    SoundUtils.Beep(state.soundPlayer);
                    AutoCompleteEnd();
                    return;
                }
                if (autoCompleteList.Length > 0)
                {
                    int doubleQuoteOffset = evenPrecedingDoubleQuotes ? 0 : 1;
                    if (autoCompletePrecedingText == string.Empty && state.Input.Length > 0 + doubleQuoteOffset)
                    {
                        autoCompletePrecedingText = state.Input.Text.ToString().Substring(0, stringStartPosition);
                    }
                    state.InputClear(true, autoCompletePrecedingText.Length + 1);
                    autoCompleteListPosition = !reverseDirection ? 0 : autoCompleteList.Length - 1;
                    bool addDoubleQuotes = autoCompleteRetainDoubleQuotes || autoCompleteList[autoCompleteListPosition].Contains(' ');
                    state.Input.Text.Append(string.Format("{0}{1}{2}{1}", autoCompletePrecedingText, addDoubleQuotes ? "\"" : "", autoCompleteList[autoCompleteListPosition]));
                    ConsoleOutput.UpdateCurrentCommand(state, state.Input.StartPosition, addDoubleQuotes ? autoCompletePrecedingText.Length + autoCompleteList[autoCompleteListPosition].Length + 2 : autoCompletePrecedingText.Length + autoCompleteList[autoCompleteListPosition].Length);
                }
                else
                {
                    SoundUtils.Beep(state.soundPlayer);
                    AutoCompleteEnd();
                    return;
                }
            }
            // If on a subsequent autocompletion, cycle to the next/previous result
            else if (autoCompleteList != null && autoCompleteListPosition >= 0)
            {
                state.InputClear(true, autoCompletePrecedingText.Length + 1);
                if (!reverseDirection)
                {
                    autoCompleteListPosition += 1;
                    if (autoCompleteListPosition >= autoCompleteList.Length)
                    {
                        autoCompleteListPosition = 0;
                    }
                }
                else
                {
                    autoCompleteListPosition -= 1;
                    if (autoCompleteListPosition < 0)
                    {
                        autoCompleteListPosition = autoCompleteList.Length - 1;
                    }
                }
                bool addDoubleQuotes = autoCompleteRetainDoubleQuotes || autoCompleteList[autoCompleteListPosition].Contains(' ');
                state.Input.Text.Append(string.Format("{0}{1}{2}{1}", autoCompletePrecedingText, addDoubleQuotes ? "\"" : "", autoCompleteList[autoCompleteListPosition]));
                ConsoleOutput.UpdateCurrentCommand(state, state.Input.StartPosition, addDoubleQuotes ? autoCompletePrecedingText.Length + autoCompleteList[autoCompleteListPosition].Length + 2 : autoCompletePrecedingText.Length + autoCompleteList[autoCompleteListPosition].Length);
            }
        }

        /// <summary>
        /// Ends an autocomplete session, clearing all internal values.
        /// </summary>
        public void AutoCompleteEnd()
        {
            if (autoCompleteMode != AutoCompleteMode.None)
            {
                autoCompleteMode = AutoCompleteMode.None;
                autoCompleteListPosition = -1;
                autoCompleteList = null;
                autoCompletePrecedingText = string.Empty;
                autoCompleteRetainDoubleQuotes = false;
            }
        }

        /// <summary>
        /// The AutoComplete types.
        /// </summary>
        public enum AutoCompleteMode
        {
            /// <summary>
            /// No autocompletion mode - unset, default, disabled, etc.
            /// </summary>
            None,
            /// <summary>
            /// Tab key autocompletion.
            /// </summary>
            Tab,
            /// <summary>
            /// Directory and Ctrl+D autocompletion.
            /// </summary>
            Directory,
            /// <summary>
            /// File and Ctrl+F autocompletion.
            /// </summary>
            File
        }
    }
}
