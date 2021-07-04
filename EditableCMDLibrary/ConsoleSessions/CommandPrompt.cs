using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using System.Threading;
using uk.JohnCook.dotnet.EditableCMDLibrary.Interop;
using uk.JohnCook.dotnet.EditableCMDLibrary.Utils;

namespace uk.JohnCook.dotnet.EditableCMDLibrary.ConsoleSessions
{
    /// <summary>
    /// Class for executing a command prompt process in a separate thread, with events and state
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class CommandPrompt : IDisposable
    {
        /// <summary>
        /// An event fired whenever there is a new line of output if <see cref="StoreOutput"/> is true.
        /// </summary>
        public event EventHandler<int>? NewOutput;
        /// <summary>
        /// An event fired just after the process has started.
        /// </summary>
        public event EventHandler<bool>? Started;
        /// <summary>
        /// An event fired just after the process has finished executing.
        /// </summary>
        public event EventHandler<bool>? Completed;

        private readonly ManualResetEventSlim eventSlim = new(false);

        private readonly ConsoleState state;
        private Thread? cmdThread;

        /// <summary>
        /// The command string to execute.
        /// </summary>
        public string Command { get; set; }
        /// <summary>
        /// Whether the current directory set at the end of execution should be persisted.
        /// </summary>
        public bool UpdateCurrentDirectory { get; set; }
        /// <summary>
        /// Whether the environment variables set at the end of execution should be persisted.
        /// </summary>
        public bool UpdateEnvironment { get; set; }
        /// <summary>
        /// Whether the output should be stored in a <see cref="Queue{T}"/> for further parsing.
        /// </summary>
        public bool StoreOutput { get; set; }
        /// <summary>
        /// Whether the output should be printed to console output.
        /// </summary>
        public bool DisplayOutput { get; set; }
        /// <summary>
        /// A queue containing the output from the executable if <see cref="StoreOutput"/> is true.
        /// </summary>
        public Queue<string> Output { get; private set; }
        /// <summary>
        /// The exit code from the executable.
        /// </summary>
        public int ExitCode { get; private set; }
        /// <summary>
        /// Whether the executable has finished executing.
        /// </summary>
        public bool Finished { get; private set; }

        private readonly ProcessStartInfo processStartInfo;
        private bool aborted = false;
        private readonly CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Create a new command prompt process.
        /// </summary>
        /// <param name="state">The <see cref="ConsoleState"/> for the current console session.</param>
        /// <param name="command">The command string to execute. Stored in <see cref="Command"/></param>.
        /// <param name="updateCurrentDirectory">Whether the current directory at the end of execution should be persisted. Stored in <see cref="UpdateCurrentDirectory"/>.</param>
        /// <param name="updateEnvironment">Whether the environment variables set at the end of execution should be persisted. Stored in <see cref="UpdateEnvironment"/>.</param>
        /// <param name="storeOutput">Whether the output should be stored in a <see cref="Queue{T}"/> for further parsing. Stored in <see cref="StoreOutput"/>.</param>
        /// <param name="displayOutput">Whether the output should be printed to console output. Stored in <see cref="DisplayOutput"/>.</param>
        public CommandPrompt(ConsoleState state, string? command, bool updateCurrentDirectory = true, bool updateEnvironment = true, bool storeOutput = false, bool displayOutput = true)
        {
            this.state = state;
            Command = command ?? state.Input.Text.ToString();
            UpdateCurrentDirectory = updateCurrentDirectory;
            UpdateEnvironment = updateEnvironment;
            StoreOutput = storeOutput;
            DisplayOutput = displayOutput;
            ExitCode = 0;
            Finished = false;
            Output = new();
            Completed += OnComplete;
            processStartInfo = new()
            {
                FileName = state.CmdProcessStartInfo.FileName
            };
            aborted = false;
            cancellationTokenSource = new();
            ProcessCommand();
        }

        /// <summary>
        /// Sets up the <see cref="cmdThread"/> <see cref="Process"/>.
        /// </summary>
        private void ProcessCommand()
        {
            eventSlim.Reset();
            state.CmdRunning = true;
            // Start a self-closing cmd process with entered command, then silently store current directory
            string environmentCommand = UpdateEnvironment ? string.Format(" & SET >{0}", state.EnvLogger.LogFile) : string.Empty;
            string directoryCommand = UpdateCurrentDirectory ? string.Format(" & CD >{0}", state.PathLogger.LogFile) : string.Empty;
            processStartInfo.Arguments = string.Format("{0}{1}/D /C {2}{3}{4}", state.StartupParams.CmdParams, state.StartupParams.CmdParams != string.Empty ? " " : "", Command, directoryCommand, environmentCommand);
            processStartInfo.WorkingDirectory = state.CurrentDirectory;
            if (StoreOutput)
            {
                processStartInfo.UseShellExecute = false;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.StandardOutputEncoding = state.OutputEncoding;
                try
                {
                    cmdThread = new Thread(ProcessStoreOutput);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                cmdThread = new Thread(ProcessNoStoreOutput);
            }
        }

        /// <summary>
        /// Start execution of the <see cref="CommandPrompt"/> <see cref="Process"/>.
        /// </summary>
        /// <returns>True if <see cref="Process.Start()"/> was called. False if the <see cref="Process"/> is null.</returns>
        public bool Start()
        {
            if (cmdThread == null)
            {
                return false;
            }
            else
            {
                cmdThread.Start();
                return true;
            }
        }

        /// <summary>
        /// Start execution of the <see cref="CommandPrompt"/> <see cref="Process"/>.
        /// </summary>
        /// <returns>False if the thread was not running. False if the <see cref="Process"/> is null.</returns>
        public bool Stop()
        {
            if (cmdThread == null && cmdThread?.IsAlive != true)
            {
                return false;
            }
            else
            {
                cancellationTokenSource.Cancel();
                aborted = true;
                NativeMethods.SetStdHandle(NativeMethods.STD_OUTPUT_HANDLE, state.StandardOutput.Handle);
                return true;
            }
        }

        /// <summary>
        /// Waits for the <see cref="CommandPrompt"/> <see cref="Process"/> to exit.
        /// </summary>
        public void WaitForExit()
        {
            if (Finished)
            {
                return;
            }
            eventSlim.Wait();
            eventSlim.Reset();
        }

        private void ProcessStartFailed()
        {
            Debug.Fail("Unable to start process - Process.Start returned null.");
            state.InputLogger.Log("Unable to start process - Process.Start returned null.");
            Started?.Invoke(this, false);
            state.CmdProcess = new() { StartInfo = state.CmdProcessStartInfo };
            Finished = false;
            state.CmdRunning = false;
            Completed?.Invoke(this, true);
        }

        /// <summary>
        /// Create a <see cref="CommandPrompt"/> <see cref="Process"/> that will store output in a queue.
        /// </summary>
        private void ProcessStoreOutput()
        {
            state.CmdProcess = Process.Start(processStartInfo);
            if (state.CmdProcess == null)
            {
                ProcessStartFailed();
                return;
            }
            Started?.Invoke(this, true);
            string? currentLine = string.Empty;
            while (!aborted && !state.CmdProcess.StandardOutput.EndOfStream)
            {
                currentLine = state.CmdProcess.StandardOutput.ReadLine();
                if (currentLine == null)
                {
                    continue;
                }
                Output.Enqueue(currentLine);
                NewOutput?.Invoke(this, 1);
                if (DisplayOutput)
                {
                    Console.WriteLine(currentLine);
                }
            }
            if (!aborted)
            {
                state.CmdProcess.WaitForExitAsync(cancellationTokenSource.Token).GetAwaiter().GetResult();
                ExitCode = state.CmdProcess.ExitCode;
            }
            else
            {
                state.CmdProcess = new() { StartInfo = state.CmdProcessStartInfo };
            }
            Finished = true;
            Completed?.Invoke(this, true);
        }

        /// <summary>
        /// Create a <see cref="CommandPrompt"/> <see cref="Process"/> that will not store output in a queue.
        /// </summary>
        private void ProcessNoStoreOutput()
        {
            state.CmdProcess = Process.Start(processStartInfo);
            if (state.CmdProcess == null)
            {
                ProcessStartFailed();
                return;
            }
            Started?.Invoke(this, true);
            state.CmdProcess.WaitForExit();
            if (!aborted)
            {
                ExitCode = state.CmdProcess.ExitCode;
            }
            else
            {
                state.CmdProcess = new() { StartInfo = state.CmdProcessStartInfo };
            }
            Finished = true;
            state.CmdRunning = false;
            Completed?.Invoke(this, true);
        }

        /// <summary>
        /// Event handler for when the <see cref="CommandPrompt"/> <see cref="Process"/> has finished executing.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="done">A boolean indicating the process has finished executing.</param>
        private void OnComplete(object? sender, bool done)
        {
            if (!done)
            {
                return;
            }
            // Copy environment variables to current environment
            if (!aborted && UpdateEnvironment && File.Exists(state.EnvLogger.LogFile))
            {
                FileInfo envFileInfo = new(state.EnvLogger.LogFile);
                bool envFileAccessible = FileUtils.WaitForFile(envFileInfo, 1000);
                if (envFileAccessible)
                {
                    using StreamReader envFile = new(state.EnvLogger.LogFile);
                    string? lineContent;
                    while ((lineContent = envFile.ReadLine()) != null)
                    {
                        string[] variable = lineContent.Split('=', 2);
                        if (!variable[0].StartsWith("EditableCmd_"))
                        {
                            Environment.SetEnvironmentVariable(variable[0], variable[1]);
                        }
                    }
                }
            }
            eventSlim.Set();
        }

        /// <summary>
        /// Disposal of the <see cref="CommandPrompt"/> instance's managed and unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            eventSlim.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
