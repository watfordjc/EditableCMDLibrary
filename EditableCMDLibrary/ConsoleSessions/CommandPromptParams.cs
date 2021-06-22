using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using uk.JohnCook.dotnet.EditableCMDLibrary.Interop;
using uk.JohnCook.dotnet.EditableCMDLibrary.Utils;

namespace uk.JohnCook.dotnet.EditableCMDLibrary.ConsoleSessions
{
    /// <summary>
    /// Class for parsing and storing the application's startup command line arguments
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class CommandPromptParams
    {
        // Command line parameters
        private readonly bool close = false;
        private readonly bool keep = false;
        private readonly bool stringTreatment = false;
        private readonly bool echoOff = false;
        private readonly bool headerOff = false;
        private readonly bool autorunDisabled = false;
        private readonly bool color = false;
        private readonly string colorColors = string.Empty;
        private readonly bool extensions = false;
        private readonly bool fileCompletion = true;
        private readonly bool variableExpansion = false;
        private readonly bool help = false;
        private readonly string cmdParams = string.Empty;
        private readonly string commandString = string.Empty;
        // Output encoding
        private readonly Encoding outputEncoding = Encoding.UTF8;
        // Passthrough command
        private readonly string runExecutable = string.Empty;
        private readonly string runCommand = string.Empty;
        private readonly string runDirectory = string.Empty;
        private readonly CommandPromptParams runCommandCliParams = null;
        private readonly bool fromAutoRun = false;

        /// <summary>
        /// Create an instance of a command-line argument parser and parse the parameters.
        /// </summary>
        /// <param name="callingExectuable">The executable that called/started the application.</param>
        /// <param name="args">The command-line arguments used when starting the application.</param>
        public CommandPromptParams(string callingExectuable, string[] args)
        {
            if (!string.IsNullOrEmpty(callingExectuable))
            {
                runExecutable = callingExectuable;
                close = true;
                keep = false;
                headerOff = true;
            }
            string[] switchArgs = null;
            // Find first non-switch parameter
            int firstNonSwitchParameter = Array.FindIndex(args, IsNotSwitch);
            if (firstNonSwitchParameter >= 0)
            {
                switchArgs = new string[firstNonSwitchParameter];
                Array.Copy(args, 0, switchArgs, 0, firstNonSwitchParameter);
            }
            else
            {
                switchArgs = args;
            }

            // Determine if the /K or /C switch is used, and if so which one comes first
            int cSwitchParam = Array.FindIndex(switchArgs, IsSwitchC);
            int kSwitchParam = Array.FindIndex(switchArgs, IsSwitchK);
            Switch startString = cSwitchParam >= 0 ? Switch.c : Switch.invalid;
            if (kSwitchParam >= 0 && (cSwitchParam == 0 || kSwitchParam < cSwitchParam))
            {
                startString = Switch.k;
            }

            // Translate the parameters into a command string
            // TODO: Handle double quotes and stuff
            int paramCount = switchArgs.Length;
            if (firstNonSwitchParameter >= 0)
            {
                for (int argn = firstNonSwitchParameter; argn < args.Length; argn++)
                {
                    if (args[argn].Contains(' '))
                    {
                        args[argn] = string.Format("\"{0}\"", args[argn]);
                    }
                }
            }
            switch (startString)
            {
                case Switch.invalid:
                    if (firstNonSwitchParameter >= 0)
                    {
                        commandString = string.Join(' ', args, firstNonSwitchParameter, args.Length - paramCount);
                    }
                    break;
                case Switch.c:
                    paramCount = cSwitchParam;
                    if (cSwitchParam < switchArgs.Length)
                    {
                        commandString = string.Join(' ', args, cSwitchParam + 1, args.Length - paramCount - 1);
                    }
                    break;
                case Switch.k:
                    paramCount = kSwitchParam;
                    if (kSwitchParam < switchArgs.Length)
                    {
                        commandString = string.Join(' ', args, kSwitchParam + 1, args.Length - paramCount - 1);
                    }
                    break;
            }

            // Parse switches
            for (int argn = 0; argn < paramCount; argn++)
            {
                if ((switchArgs[argn][0] != '/' && switchArgs[argn][1] != '"') || switchArgs[argn].Length == 1)
                {
                    continue;
                }
                int valueLength = switchArgs[argn].Length - 2;
                switch (CharToSwitch(switchArgs[argn][1]))
                {
                    case Switch.c:
                        close = true;
                        continue;
                    case Switch.k:
                        if (string.IsNullOrEmpty(callingExectuable) && !fromAutoRun)
                        {
                            keep = true;
                        }
                        else
                        {
                            close = true;
                        }
                        continue;
                    case Switch.s:
                        if (valueLength == 0)
                        {
                            cmdParams += " /S";
                            stringTreatment = true;
                        }
                        continue;
                    case Switch.q:
                        echoOff = true;
                        continue;
                    case Switch.d:
                        if (valueLength == 0)
                        {
                            cmdParams += " /D";
                            autorunDisabled = true;
                        }
                        continue;
                    case Switch.a:
                        if (valueLength == 0)
                        {
                            cmdParams += " /A";
                            outputEncoding = Encoding.ASCII;
                        }
                        continue;
                    case Switch.u:
                        if (valueLength == 0)
                        {
                            cmdParams += " /U";
                            outputEncoding = Encoding.Unicode;
                        }
                        continue;
                    case Switch.t:
                        if (valueLength > 1 && valueLength <= 3 && switchArgs[argn][2] == ':')
                        {
                            if (valueLength == 3)
                            {
                                colorColors += Uri.IsHexDigit(switchArgs[argn][3]) ? switchArgs[argn][3] : "";
                                colorColors += Uri.IsHexDigit(switchArgs[argn][4]) ? switchArgs[argn][4] : "";
                                if (colorColors.Length != 2)
                                {
                                    colorColors = string.Empty;
                                    continue;
                                }
                            }
                            else
                            {
                                colorColors += Uri.IsHexDigit(switchArgs[argn][3]) ? switchArgs[argn][3] : "";
                                if (colorColors.Length != 1)
                                {
                                    colorColors = string.Empty;
                                    continue;
                                }
                            }
                            cmdParams += string.Format(" /T:{0}", colorColors);
                            color = true;
                        }
                        continue;
                    case Switch.e:
                        if (valueLength > 1 && valueLength <= 4 && switchArgs[argn][2] == ':')
                        {
                            string value = switchArgs[argn][3..].ToLower();
                            if (value.Equals("on") || value.Equals("off"))
                            {
                                cmdParams += string.Format(" /E:{0}", value);
                                extensions = value.Equals("on");
                            }
                        }
                        continue;
                    case Switch.x:
                        cmdParams += " /E:on";
                        extensions = true;
                        continue;
                    case Switch.y:
                        cmdParams += " /E:off";
                        extensions = false;
                        continue;
                    case Switch.f:
                        if (valueLength > 1 && valueLength <= 4 && switchArgs[argn][2] == ':')
                        {
                            string value = switchArgs[argn][3..].ToLower();
                            if (value.Equals("on") || value.Equals("off"))
                            {
                                cmdParams += string.Format(" /F:{0}", value);
                                fileCompletion = value.Equals("on");
                            }
                        }
                        continue;
                    case Switch.v:
                        if (valueLength > 1 && valueLength <= 4 && switchArgs[argn][2] == ':')
                        {
                            string value = switchArgs[argn][3..].ToLower();
                            if (value.Equals("on") || value.Equals("off"))
                            {
                                cmdParams += string.Format(" /V:{0}", value);
                                variableExpansion = value.Equals("on");
                            }
                        }
                        continue;
                    case Switch.questionMark:
                        if (valueLength == 0)
                        {
                            help = true;
                            commandString = "/?";
                        }
                        continue;
                    case Switch.r:
                        // The /R switches are custom EditableCMD switches for passing commands from cmd.exe to EditableCMD using AutoRun scripts
                        // /R1:"\"value\"" - value is the value of %COMSPEC%
                        // /R2:"\"value\"" - value is the value of %CmdCmdLine%
                        // /R3:"\"value\"" - value is the value of %CD%
                        headerOff = true;
                        if (valueLength > 2)
                        {
                            switch (switchArgs[argn][2])
                            {
                                case '1':
                                    runExecutable = switchArgs[argn].Trim()[5..^1].ToLower();
                                    if (runCommand.Length > 0)
                                    {
                                        if (string.IsNullOrEmpty(callingExectuable))
                                        {
                                            close = true;
                                            keep = false;
                                            runCommand = runCommand.Remove(0, runExecutable.Length + 2).Replace("/K", "/C").Replace("/k", "/C").Trim();
                                            if (runExecutable.Length == runCommand.Length)
                                            {
                                                runCommand = string.Empty;
                                                continue;
                                            }
                                        }
                                    }
                                    continue;
                                case '2':
                                    runCommand = switchArgs[argn][5..^1].Trim();
                                    string[] argv = null;
                                    IntPtr cmdParamPtr = NativeMethods.CommandLineToArgvW(runCommand, out int argc);
                                    if (cmdParamPtr != IntPtr.Zero)
                                    {
                                        argv = new string[argc];
                                        for (int argn2 = 0; argn2 < argc; argn2++)
                                        {
                                            IntPtr thisParamPtr = Marshal.ReadIntPtr(cmdParamPtr, IntPtr.Size * argn2);
                                            if (thisParamPtr != IntPtr.Zero)
                                            {
                                                argv[argn2] = Marshal.PtrToStringUni(thisParamPtr);
                                                switch (argv[argn2].ToLower())
                                                {
                                                    case "/k":
                                                        keep = true;
                                                        argv[argn2] = "/C";
                                                        break;
                                                    case "/c":
                                                        close = true;
                                                        argv[argn2] = "/C";
                                                        break;
                                                    case "/?":
                                                        help = true;
                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                argv[argn2] = string.Empty;
                                            }
                                        }
                                        NativeMethods.LocalFree(cmdParamPtr);
                                        if (argv[0].ToLower() == StringUtils.GetComSpec().ToLower())
                                        {
                                            fromAutoRun = true;
                                        }
                                        runExecutable = argv[0];
                                        if (argc > 1)
                                        {
                                            string[] argvTemp = new string[argc - 1];
                                            Array.Copy(argv, 1, argvTemp, 0, argc - 1);
                                            runCommandCliParams = new CommandPromptParams(argv[0], argvTemp);
                                        }
                                    }

                                    if (runExecutable.Length > 0)
                                    {
                                        if (string.IsNullOrEmpty(callingExectuable))
                                        {
                                            runCommand = runCommand.Remove(0, runExecutable.Length + 2).Replace("/K", "/C").Replace("/k", "/C").Trim();
                                            if (runCommand.Length == runExecutable.Length)
                                            {
                                                runCommand = string.Empty;
                                                continue;
                                            }
                                        }
                                    }
                                    continue;
                                case '3':
                                    runDirectory = switchArgs[argn].Trim()[5..^1];
                                    continue;
                                default:
                                    continue;
                            }

                        }
                        continue;
                }
            }

            cmdParams = cmdParams.Trim();
            commandString = commandString.Trim();

            if (!help && colorColors != string.Empty)
            {
                switch (colorColors.Length)
                {
                    case 1:
                        Console.ForegroundColor = (ConsoleColor)Convert.ToInt32("0x" + colorColors[0], 16);
                        break;
                    case 2:
                        Console.BackgroundColor = (ConsoleColor)Convert.ToInt32("0x" + colorColors[0], 16);
                        Console.ForegroundColor = (ConsoleColor)Convert.ToInt32("0x" + colorColors[1], 16);
                        break;
                    default:
                        break;
                }
                Console.Clear();
            }

        }


        /// <summary>
        /// List of parameter switches
        /// </summary>
        public enum Switch
        {
            /// <summary>
            /// Command line switch invalid or not recognised.
            /// </summary>
            invalid,
            /// <summary>
            /// /C - carry out command string and stop running.
            /// </summary>
            c,
            /// <summary>
            /// /K - carry out command string and continue running.
            /// </summary>
            k,
            /// <summary>
            /// /S - modified treatment of command string.
            /// </summary>
            s,
            /// <summary>
            /// /Q - turn off echo.
            /// </summary>
            q,
            /// <summary>
            /// /D - disable autorun.
            /// </summary>
            d,
            /// <summary>
            /// /A - use ANSI output.
            /// </summary>
            a,
            /// <summary>
            /// /U - use Unicode (UTF16-LE) output.
            /// </summary>
            u,
            /// <summary>
            /// /T:{ &lt;b&gt;&lt;f&gt; | &lt;f&gt; } - sets background and foreground colours.
            /// </summary>
            t,
            /// <summary>
            /// /E:{ on | off } - enables/disables command extensions.
            /// </summary>
            e,
            /// <summary>
            /// /X - old switch equivalent to /E:on.
            /// </summary>
            x,
            /// <summary>
            /// /Y - old switch equivalent to /E:off.
            /// </summary>
            y,
            /// <summary>
            /// /F:{ on | off } - enables/disables file and directory name completion.
            /// </summary>
            f,
            /// <summary>
            /// /V:{ on | off } - enables/disables delayed environment variable expansion.
            /// </summary>
            v,
            /// <summary>
            /// /? - displays help.
            /// </summary>
            questionMark,
            /// <summary>
            /// /R{1|2|3}:"string with double quotes escaped" - passthrough command prompt command arguments and other variables.
            /// </summary>
            r
        };

        /// <summary>
        /// Convert a parameter switch character to a ParameterSwitch value
        /// </summary>
        /// <param name="c">The parameter switch character</param>
        /// <returns>The ParameterSwitch value for c, ParameterSwitch.invalid if the character is an invalid switch.</returns>
        public static Switch CharToSwitch(char c)
        {
            return char.ToLower(c) switch
            {
                'c' => Switch.c,
                'k' => Switch.k,
                's' => Switch.s,
                'q' => Switch.q,
                'd' => Switch.d,
                'a' => Switch.a,
                'u' => Switch.u,
                't' => Switch.t,
                'e' => Switch.e,
                'x' => Switch.x,
                'y' => Switch.y,
                'f' => Switch.f,
                'v' => Switch.v,
                '?' => Switch.questionMark,
                'r' => Switch.r,
                _ => Switch.invalid,
            };
        }

        /// <summary>
        /// Test a string for looking like a command line switch
        /// </summary>
        /// <param name="s">The string to test.</param>
        /// <returns>True if the string starts with '/', otherwise false.</returns>
        private bool IsNotSwitch(String s)
        {
            return s[0] != '/';
        }

        /// <summary>
        /// Test a string for being a /C switch.
        /// </summary>
        /// <param name="s">The string to test.</param>
        /// <returns>True if the string matches "/c" after being made lowercase.</returns>
        private bool IsSwitchC(String s)
        {
            return s.ToLower() == "/c";
        }

        /// <summary>
        /// Test a string for being a /K switch.
        /// </summary>
        /// <param name="s">The string to test.</param>
        /// <returns>True if the string matches "/k" after being made lowercase.</returns>
        private bool IsSwitchK(String s)
        {
            return s.ToLower() == "/k";
        }

        #region Properties

        /// <summary>
        /// Command-line parameter /C
        /// </summary>
        public bool Close
        {
            get { return close; }
        }

        /// <summary>
        /// Command-line parameter /K
        /// </summary>
        public bool Keep
        {
            get { return keep; }
        }

        /// <summary>
        /// Command-line parameter /S
        /// </summary>
        public bool StringTreatment
        {
            get { return stringTreatment; }
        }

        /// <summary>
        /// Command-line parameter /Q
        /// </summary>
        public bool EchoOff
        {
            get { return echoOff; }
        }

        /// <summary>
        /// Disable displaying the command prompt header
        /// </summary>
        public bool HeaderOff
        {
            get { return headerOff; }
        }

        /// <summary>
        /// Command-line parameter /D
        /// </summary>
        public bool AutorunDisabled
        {
            get { return autorunDisabled; }
        }

        /// <summary>
        /// Command-line parameter /T
        /// </summary>
        public bool Color
        {
            get { return color; }
        }

        /// <summary>
        /// Command-line parameter /T's colours: 1-2 hex digits
        /// </summary>
        public string ColorColors
        {
            get { return colorColors; }
        }

        /// <summary>
        /// Command-line parameter /E: true if on, false if off
        /// </summary>
        public bool Extensions
        {
            get { return extensions; }
        }

        /// <summary>
        /// Command-line parameter /F: true if on, false if off
        /// </summary>
        public bool FileCompletion
        {
            get { return fileCompletion; }
        }

        /// <summary>
        /// Command-line parameter /V
        /// </summary>
        public bool VariableExpansion
        {
            get { return variableExpansion; }
        }

        /// <summary>
        /// Command-line parameter /?
        /// </summary>
        public bool Help
        {
            get { return help; }
        }

        /// <summary>
        /// Command-line parameters
        /// </summary>
        public string CmdParams
        {
            get { return cmdParams; }
        }

        /// <summary>
        /// Command-line parameter string/command
        /// </summary>
        public string CommandString
        {
            get { return commandString; }
        }

        /// <summary>
        /// Command-line parameter /A (ANSI) and /U (UTF-16LE)
        /// </summary>
        public Encoding OutputEncoding
        {
            get { return outputEncoding; }
        }

        /// <summary>
        /// Command-line parameter /R1:"path" - full path of AutoRun executable (e.g. value of %COMSPEC%)
        /// </summary>
        public string RunExecutable
        {
            get { return runExecutable; }
        }

        /// <summary>
        /// Command-line parameter /R2:"args" - command line arguments from AutoRun script
        /// </summary>
        public string RunCommand
        {
            get { return runCommand; }
        }

        /// <summary>
        /// Command-line parameter /R3:"path" - the value of %CD% from the AutoRun script
        /// </summary>
        public string RunDirectory
        {
            get { return runDirectory; }
        }

        /// <summary>
        /// The value of <see cref="RunCommand"/> parsed as <see cref="CommandPromptParams"/>.
        /// </summary>
        public CommandPromptParams RunCommandCliParams
        {
            get { return runCommandCliParams; }
        }

        /// <summary>
        /// Boolean indicating if application was started via AutoRun.
        /// </summary>
        public bool FromAutoRun
        {
            get { return fromAutoRun; }
        }

        #endregion
    }
}
