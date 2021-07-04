using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Media;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using uk.JohnCook.dotnet.EditableCMDLibrary.InputProcessing;
using uk.JohnCook.dotnet.EditableCMDLibrary.Interop;
using uk.JohnCook.dotnet.EditableCMDLibrary.Logging;
using uk.JohnCook.dotnet.EditableCMDLibrary.Utils;

namespace uk.JohnCook.dotnet.EditableCMDLibrary.ConsoleSessions
{
    /// <summary>
    /// Class for maintaining the state of the console.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class ConsoleState
    {
        #region Events

        /// <summary>
        /// An event that fires when <see cref="ConsoleInputLine.StartPosition"/> changes.
        /// </summary>
        public event EventHandler<NativeMethods.COORD>? InputStartPositionChanged;
        /// <summary>
        /// An event that fires when <see cref="ConsoleInputLine.EndPosition"/> changes.
        /// </summary>
        public event EventHandler<NativeMethods.COORD>? InputEndPositionChanged;
        /// <summary>
        /// An event that fires when <see cref="ConsoleInputLine.Text"/> changes.
        /// </summary>
        public event EventHandler<ConsoleInputLine>? InputChanged;
        /// <summary>
        /// An event that fires when <see cref="CurrentDirectory"/> changes.
        /// </summary>
        public event EventHandler<string>? CurrentDirectoryChanged;
        /// <summary>
        /// An event that fires when this console session is closing from a call to <see cref="EndSession"/>.
        /// </summary>
        public event EventHandler<bool>? SessionClosing;
        /// <summary>
        /// An event that fires when <see cref="SystemEvents.UserPreferenceChanged"/> fires.
        /// </summary>
        public event EventHandler<UserPreferenceChangedEventArgs>? UserPreferenceChanged;

        #endregion

        /// <summary>
        /// A wrapper around a line of console input - can be reused by calling <see cref="InputClear()"/> or <see cref="InputClear(bool, int)"/>.
        /// </summary>
        public class ConsoleInputLine
        {
            /// <summary>
            /// The position of the first character of the current input.
            /// </summary>
            public NativeMethods.COORD StartPosition;
            /// <summary>
            /// The position of the last character of the current input.
            /// </summary>
            public NativeMethods.COORD EndPosition;
            /// <summary>
            /// The current input text.
            /// </summary>
            public StringBuilder Text;
            /// <summary>
            /// The length of <see cref="Text"/>.
            /// </summary>
            public int Length { get { return Text.Length; } }
            /// <summary>
            /// A variable for holding the position of tab characters in the current command.
            /// </summary>
            public List<int> TabPositions;

            /// <summary>
            /// Start a new console input line.
            /// </summary>
            public ConsoleInputLine()
            {
                StartPosition = new();
                EndPosition = new();
                Text = new();
                TabPositions = new();
            }
        }

        /// <summary>
        /// A wrapper around the console's standard input stream's handle.
        /// </summary>
        public InputHandle StandardInput { get; private set; }
        /// <summary>
        /// A wrapper around the console's standard output stream's handle.
        /// </summary>
        public StandardHandle StandardOutput { get; private set; }
        /// <summary>
        /// Boolean for ending ReadConsoleInput() thread
        /// </summary>
        public volatile bool Closing = false;

        /// <summary>
        /// Application's name
        /// </summary>
        public string ApplicationName { get; private set; }
        /// <summary>
        /// Command line parameters
        /// </summary>
        public CommandPromptParams StartupParams { get; private set; }
        /// <summary>
        /// A variable for storing the current working directory for cmd.exe
        /// </summary>
        public string CurrentDirectory { get; private set; }
        /// <summary>
        /// The drive letter of <see cref="CurrentDirectory"/>.
        /// </summary>
        public char CurrentDriveLetter { get { return CurrentDirectory.ToLower()[0]; } }
        /// <summary>
        /// Current working directory temp file
        /// </summary>
        public FileLogger PathLogger { get; private set; }
        /// <summary>
        /// Environment variable temp file
        /// </summary>
        public FileLogger EnvLogger { get; private set; }
        /// <summary>
        /// Input log file
        /// </summary>
        public IInputLogger InputLogger { get; private set; }

        // Window title
        private string title;
        /// <summary>
        /// Is ECHO ON?
        /// </summary>
        public bool EchoEnabled { get; set; }
        /// <summary>
        /// Last used directory on each drive letter.
        /// </summary>
        public Dictionary<char, string> DrivePaths { get; }

        /// <summary>
        /// Is console in edit-mode?
        /// </summary>
        public volatile bool EditMode = false;
        /// <summary>
        /// Is console in overwrite-mode?
        /// </summary>
        public volatile bool OverwriteMode = false;
        /// <summary>
        /// Is console in mark-mode?
        /// </summary>
        public static bool MarkMode { get { return MarkModeEnabled(); } }
        /// <summary>
        /// The current command
        /// </summary>
        public ConsoleInputLine Input;
        /// <summary>
        /// A variable for holding previously entered commands
        /// </summary>
        public readonly List<string> PreviousCommands = new();
        /// <summary>
        /// AutoComplete
        /// </summary>
        public readonly AutoComplete autoComplete;
        /// <summary>
        /// SoundPlayer
        /// </summary>
        public readonly SoundPlayer soundPlayer = new();

        /// <summary>
        /// A cmd.exe <see cref="Process"/>.
        /// </summary>
        public Process? CmdProcess;
        /// <summary>
        /// The <see cref="ProcessStartInfo"/> for <see cref="CmdProcess"/>,
        /// </summary>
        public ProcessStartInfo CmdProcessStartInfo;
        /// <summary>
        /// A boolean value indicating if <see cref="CmdProcess"/> is currently running.
        /// </summary>
        public volatile bool CmdRunning = false;
        /// <summary>
        /// Input encoding for console.
        /// </summary>
        public Encoding InputEncoding;
        /// <summary>
        /// Output encoding for console.
        /// </summary>
        public Encoding OutputEncoding;


        private readonly string sessionGuid;

        /// <summary>
        /// Holds the state data for the console instance.
        /// </summary>
        /// <param name="startupParams">The parsed command line arguments used to start the application.</param>
        /// <param name="guid">A unique <see cref="Guid"/> for the session ID, or <see cref="Guid.Empty"/> to use a shared (DEBUG) session ID.</param>
        /// <param name="applicationName">Application's name.</param>
        public ConsoleState(CommandPromptParams startupParams, Guid guid, string applicationName)
        {
            ApplicationName = applicationName;
            StartupParams = startupParams;
            InitConsole();
            StandardInput = new InputHandle(NativeMethods.STD_INPUT_HANDLE);
            StandardOutput = new StandardHandle(NativeMethods.STD_OUTPUT_HANDLE);
            if (StandardOutput.CanSetMode)
            {
                StandardOutput.SetMode(StandardOutput.OriginalMode | NativeMethods.ENABLE_VIRTUAL_TERMINAL_PROCESSING);
            }
            sessionGuid = guid == Guid.Empty ? strings.debugSessionGuid : guid.ToString("B");
            InitFileLogging();
            DrivePaths = new();
            EchoEnabled = true;
            Input = new();
            autoComplete = new(this);
            CmdProcessStartInfo = new()
            {
                FileName = StringUtils.GetComSpec()
            };
            ChangeCurrentDirectory(StringUtils.GetDefaultWorkingDirectory());
            CmdProcess = new()
            {
                StartInfo = CmdProcessStartInfo
            };
        }

        /// <summary>
        /// Initialise the files used for storing state and logging input.
        /// </summary>
        [MemberNotNull(nameof(PathLogger))]
        [MemberNotNull(nameof(EnvLogger))]
        [MemberNotNull(nameof(InputLogger))]
        private void InitFileLogging()
        {
            // Path "%Temp%\EditableCMD\guid.txt"
            PathLogger = new FileLogger(directory: string.Join(Path.DirectorySeparatorChar, Path.TrimEndingDirectorySeparator(Path.GetTempPath()), ApplicationName),
                file: string.Concat(sessionGuid, ".txt"),
                description: "session path store");
            // Path "%Temp%\EditableCMD\guid.env.txt"
            EnvLogger = new FileLogger(directory: string.Join(Path.DirectorySeparatorChar, Path.TrimEndingDirectorySeparator(Path.GetTempPath()), ApplicationName),
                file: string.Concat(sessionGuid, ".env.txt"),
                description: "session environment variables store");
            DateTime now = DateTime.UtcNow;
            if (ApplicationName == strings.unsanitisedAppName)
            {
                // Path "%LocalAppData%\EditableCMD\Logs\Year\Month\Logfile.txt"
                InputLogger = new FileLogger(directory: string.Join(Path.DirectorySeparatorChar, Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ApplicationName, "Logs", now.ToString("yyyy"), now.ToString("MM - MMMM yyyy")),
                    file: string.Concat(DateTime.Now.ToString(strings.logDateFormat), ".txt"),
                    description: "session log");
            }
            else
            {
                InputLogger = new NullLogger(description: "session log");
            }
        }

        /// <summary>
        /// Initialise the console
        /// </summary>
        [MemberNotNull(nameof(title))]
        [MemberNotNull(nameof(InputEncoding))]
        [MemberNotNull(nameof(OutputEncoding))]
        private void InitConsole()
        {
            // Add console control event handler - handles Ctrl+C/Ctrl+Break and runs ExitCleanup() on exit
            if (!NativeMethods.SetConsoleCtrlHandler(ConsoleControlHandler, true))
            {
                throw new Exception("Unable to add handler.");
            }
            // Add console cancel event handler
            Console.CancelKeyPress += new ConsoleCancelEventHandler(ConsoleCancelEventHandler);

            // Emulate cmd.exe window title
            NativeMethods.GetStartupInfo(out NativeMethods.STARTUPINFO startupInfo);
            title = ApplicationName != strings.unsanitisedAppName ? ApplicationName : startupInfo.lpTitle.EndsWith(strings.commandPromptFilename) ? StringUtils.GetComSpec() : strings.commandPromptWindowTitle;
            Console.Title = title;

            // Set input and output encoding
            InputEncoding = StartupParams.OutputEncoding;
            OutputEncoding = StartupParams.OutputEncoding;
            Console.InputEncoding = InputEncoding;
            Console.OutputEncoding = OutputEncoding;

            #region Static system event forwarding
            /*
             * "Because this is a static event, you must detach your event handlers
             *  when your application is disposed, or memory leaks will result.
             *  -- https://docs.microsoft.com/en-us/dotnet/api/microsoft.win32.systemevents.userpreferencechanged?view=net-5.0
             *
             *  Attach/detach our event handlers for static system events in this class only.
             *  Have our event handlers forward such events as our own events.
             */

            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
            #endregion
        }

        /// <summary>
        /// Forwards static system event for user preference changes.
        /// </summary>
        /// <inheritdoc cref="UserPreferenceChangedEventHandler.Invoke(object, UserPreferenceChangedEventArgs)" path="param"/>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void OnUserPreferenceChanged(object? sender, UserPreferenceChangedEventArgs e)
        {
            /*
             * "Do not perform time-consuming processing on the thread that raises a system event handler
             *  because it might prevent other applications from functioning."
             *  -- https://docs.microsoft.com/en-us/dotnet/api/microsoft.win32.systemevents?view=net-5.0
             *
             *  Forward system events in a separate thread.
             */
            Thread thread = new(() =>
            {
                // Forward event to static event handlers
                StringUtils.SystemEvents_UserPreferenceChanged(sender, e);
                // Forward event to instance event handlers that are subscribed
                UserPreferenceChanged?.Invoke(sender, e);
            });
            thread.Start();
        }

        /// <summary>
        /// Updates the position where the current line of input begins.
        /// </summary>
        /// <param name="pos">The new position of the first character of input.</param>
        public void UpdateInputStartPosition(NativeMethods.COORD pos)
        {
            Input.StartPosition.X = pos.X;
            Input.StartPosition.Y = pos.Y;
            InputStartPositionChanged?.Invoke(this, Input.StartPosition);
        }

        /// <summary>
        /// Updates the position where the current line of input ends.
        /// </summary>
        /// <param name="pos">The new position of the last character of input.</param>
        public void UpdateInputEndPosition(NativeMethods.COORD pos)
        {
            Input.EndPosition.X = pos.X;
            Input.EndPosition.Y = pos.Y;
            InputEndPositionChanged?.Invoke(this, Input.EndPosition);
        }

        /// <summary>
        /// Prepends text to the start of the input.
        /// </summary>
        /// <param name="s">The text to prepend.</param>
        public void InputPrepend(string s)
        {
            Input.Text.Insert(0, s);
            InputChanged?.Invoke(this, Input);
        }

        /// <summary>
        /// Appends text to the end of the input.
        /// </summary>
        /// <param name="s">The text to append.</param>
        public void InputAppend(string s)
        {
            Input.Text.Append(s);
            InputChanged?.Invoke(this, Input);
        }

        /// <summary>
        /// Inserts a text string at position <paramref name="pos"/> of the input.
        /// </summary>
        /// <param name="pos">The position to start inserting the text.</param>
        /// <param name="s">The text to insert.</param>
        public void InputInsert(int pos, string s)
        {
            Input.Text.Insert(pos, s);
            InputChanged?.Invoke(this, Input);
        }

        /// <summary>
        /// Inserts a character at position <paramref name="pos"/> of the input.
        /// </summary>
        /// <param name="pos">The position to insert the character.</param>
        /// <param name="c">The character to insert.</param>
        public void InputInsert(int pos, char c)
        {
            Input.Text.Insert(pos, c);
            InputChanged?.Invoke(this, Input);
        }

        /// <summary>
        /// Clear variable storing current input.
        /// </summary>
        public void InputClear()
        {
            Input.Text.Clear();
            InputChanged?.Invoke(this, Input);
        }

        #region Clear unentered input
        /// <summary>
        /// Clear current input from display.
        /// </summary>
        /// <param name="clearCurrentCommand">If true, also clear the variable storing current input.</param>
        /// <param name="inputLengthOffset">The number of characters to add/remove from Input.Length.</param>
        public void InputClear(bool clearCurrentCommand, int inputLengthOffset = 0)
        {
            Console.CursorVisible = false;
            MoveCursorToStartOfInput();
            Console.Write(new string(' ', Input.Length + inputLengthOffset));
            if (clearCurrentCommand)
            {
                InputClear();
            }
        }
        #endregion

        /// <summary>
        /// Adds current input to the command history.
        /// </summary>
        public void AddInputToCommandHistory()
        {
            if (PreviousCommands.Contains(Input.Text.ToString()))
            {
                PreviousCommands.Remove(Input.Text.ToString());
            }
            PreviousCommands.Add(Input.Text.ToString());
        }

        /// <summary>
        /// Change the current directory to <paramref name="s"/>.
        /// </summary>
        /// <param name="s">The directory to change to.</param>
        [MemberNotNull(nameof(CurrentDirectory))]
        public void ChangeCurrentDirectory(string s)
        {
            CurrentDirectory = s;
            if (CmdProcessStartInfo == null)
            {
                CmdProcessStartInfo = new ProcessStartInfo(StringUtils.GetComSpec())
                {
                    WorkingDirectory = s
                };
            }
            else
            {
                CmdProcessStartInfo.WorkingDirectory = s;
            }
            CurrentDirectoryChanged?.Invoke(this, CurrentDirectory);
        }

        /// <summary>
        /// Moves cursor to the start of the input line.
        /// </summary>
        public void MoveCursorToStartOfInput()
        {
            Console.SetCursorPosition(Input.StartPosition.X, Input.StartPosition.Y);
        }

        /// <summary>
        /// Moves the cursor to the end of the input line.
        /// </summary>
        public void MoveCursorToEndOfInput()
        {
            Console.SetCursorPosition(Input.EndPosition.X, Input.EndPosition.Y);
        }

        /// <summary>
        /// Checks if Mark Mode is enabled.
        /// </summary>
        /// <returns>True if Mark Mode is enabled.</returns>
        private static bool MarkModeEnabled()
        {
            NativeMethods.CONSOLE_SELECTION_INFO consoleSelectionInfo = new();
            NativeMethods.GetConsoleSelectionInfo(consoleSelectionInfo);
            return (consoleSelectionInfo.Flags & NativeMethods.CONSOLE_SELECTION_INFO.CONSOLE_SELECTION_IN_PROGRESS) != 0;
        }

        /// <summary>
        /// Event handler for enabling/disabling edit mode.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The EditModeChangeEventArgs for the event.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void OnEditModeChanged(object? sender, EditModeChangeEventArgs e)
        {
            bool editModeEnabled = e.EditModeEnabled;
            if (EditMode == editModeEnabled)
            {
                return;
            }
            if (editModeEnabled)
            {
                // Enter edit mode
                StandardInput.SetMode(StandardInput.OriginalMode & ~NativeMethods.ENABLE_QUICK_EDIT_MODE | NativeMethods.ENABLE_WINDOW_INPUT | NativeMethods.ENABLE_MOUSE_INPUT);
            }
            if (!editModeEnabled)
            {
                // Exit edit mode
                MoveCursorToEndOfInput();
                StandardInput.SetMode(StandardInput.OriginalMode);
            }
            EditMode = editModeEnabled;
        }

        /// <summary>
        /// EventArgs for a change in edit-mode state.
        /// </summary>
        public class EditModeChangeEventArgs : EventArgs
        {
            /// <summary>
            /// True if edit-mode is being enabled, False if edit-mode is being disabled.
            /// </summary>
            public bool EditModeEnabled { get; }
            /// <summary>
            /// The <see cref="EventArgs"/> for a change in state of edit-mode.
            /// </summary>
            /// <param name="enableEditMode">True when enabling edit-mode, False when disabling edit-mode.</param>
            public EditModeChangeEventArgs(bool enableEditMode)
            {
                EditModeEnabled = enableEditMode;
            }
        }

        #region CTRL+C, CTRL+Break, Close window, etc.

        /// <summary>
        /// Handle console cancel events (CTRL+C, CTRL+Break)
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">arguments</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void ConsoleCancelEventHandler(object? sender, ConsoleCancelEventArgs e)
        {
            if (e.SpecialKey == ConsoleSpecialKey.ControlBreak)
            {
                e.Cancel = false;
                InputLogger.Log(StringUtils.FormatCancelledInputLogEntry(DateTime.UtcNow, CurrentDirectory, Input.Text.ToString(), e.SpecialKey));
                ConsoleOutput.WriteLine("\n");
                ConsoleOutput.WritePrompt(this, false);
            }
            else if (e.SpecialKey == ConsoleSpecialKey.ControlC)
            {
                e.Cancel = true;
                InputLogger.Log(StringUtils.FormatCancelledInputLogEntry(DateTime.UtcNow, CurrentDirectory, Input.Text.ToString(), e.SpecialKey));
                // Add command to command history if Ctrl+C'd a child process, otherwise don't
                if (CmdRunning)
                {
                    AddInputToCommandHistory();
                }
                else
                {
                    ConsoleOutput.WriteLine("\n");
                    ConsoleOutput.WritePrompt(this, false);
                }
            }
        }

        /// <summary>
        /// End the console session this <see cref="ConsoleState"/> is for is ending.
        /// </summary>
        public void EndSession()
        {
            SessionClosing?.Invoke(this, true);
            ExitCleanup();
        }

        /// <summary>
        /// Cleanup before exiting - endReadConsoleInput=true ends the console input thread, so the end of this method is right before exit
        /// </summary>
        private void ExitCleanup()
        {
            // Stop listening to static system events
            SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;

            // End the console input read loop
            Closing = true;

            // Delete the file storing %CD%
            PathLogger.DeleteFile();
            // Delete the file storing the environment
            EnvLogger.DeleteFile();
            // Dispose the input logger
            if (InputLogger is IDisposable)
            {
                (InputLogger as IDisposable)?.Dispose();
            }
            // Restore original console modes for standard handles
            if (StandardInput.CanSetMode)
            {
                StandardInput.ResetMode();
            }
            if (StandardOutput.CanSetMode)
            {
                StandardOutput.ResetMode();
            }
            soundPlayer?.Dispose();
        }

        /// <summary>
        /// Handle console control events
        /// </summary>
        /// <param name="sig">Received signal</param>
        /// <returns>True if signal should be ignored, False if default behaviour should subsequently occur</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private bool ConsoleControlHandler(NativeMethods.ConsoleControlType sig)
        {
            switch (sig)
            {
                case NativeMethods.ConsoleControlType.CTRL_CLOSE_EVENT:
                case NativeMethods.ConsoleControlType.CTRL_LOGOFF_EVENT:
                case NativeMethods.ConsoleControlType.CTRL_SHUTDOWN_EVENT:
                    EndSession();
                    return true;
                case NativeMethods.ConsoleControlType.CTRL_C_EVENT:
                case NativeMethods.ConsoleControlType.CTRL_BREAK_EVENT:
                    InputClear();
                    autoComplete.AutoCompleteEnd();
                    return true;
                default:
                    return false;
            }
            throw new NotImplementedException();
        }

        #endregion
    }

}
