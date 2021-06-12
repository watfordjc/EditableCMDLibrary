using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using uk.JohnCook.dotnet.EditableCMDLibrary.Interop;
using uk.JohnCook.dotnet.EditableCMDLibrary.Utils;

namespace uk.JohnCook.dotnet.EditableCMDLibrary.ConsoleSessions
{
    /// <summary>
    /// Class for display output helper methods
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class ConsoleOutput
    {
        /// <summary>
        /// Prevent instantiation.
        /// </summary>
        private ConsoleOutput()
        {

        }

        /// <summary>
        /// Write a string using <see cref="Console.Write(string?)"/>.
        /// </summary>
        /// <param name="s">The string to write.</param>
        public static void Write(string s)
        {
            Console.Write(s);
        }

        /// <summary>
        /// Write a string using <see cref="Console.WriteLine(string?)"/>.
        /// </summary>
        /// <param name="s">The string to write.</param>
        public static void WriteLine(string s)
        {
            Console.WriteLine(s);
        }

        #region Write prompt to console (e.g. "C:\Windows\System32>")
        /// <summary>
        /// <para>
        /// Writes command prompt to console.
        /// </para>
        /// <para>Example: C:\Windows\System32></para>
        /// </summary>
        /// <param name="state">The <see cref="ConsoleState"/> for the current console session.</param>
        /// <param name="addInputToCommandHistory">Adds the current input to the command history.</param>
        public static void WritePrompt(ConsoleState state, bool addInputToCommandHistory)
        {
            if (!File.Exists(state.PathLogger.LogFile))
            {
                state.PathLogger.Log(state.StartupParams.RunDirectory.Length > 0 ? state.StartupParams.RunDirectory : state.CurrentDirectory);
            }
            if (state.EchoEnabled)
            {
                string newCurrentDirectory = File.ReadLines(state.PathLogger.LogFile).First();
                char newDriveLetter = newCurrentDirectory.ToLower()[0];
                if (!state.DrivePaths.ContainsKey(state.CurrentDriveLetter))
                {
                    state.DrivePaths[state.CurrentDriveLetter] = state.CurrentDirectory;
                }
                state.DrivePaths[newDriveLetter] = newCurrentDirectory;
                state.ChangeCurrentDirectory(newCurrentDirectory);
                Console.Write(string.Concat(state.CurrentDirectory, ">"));
            }
            NativeMethods.COORD cursorPosition = ConsoleCursorUtils.GetCurrentCursorPosition();
            state.UpdateInputStartPosition(cursorPosition);
            state.UpdateInputEndPosition(cursorPosition);
            if (addInputToCommandHistory && state.Input.Length > 0)
            {
                state.AddInputToCommandHistory();
            }
            state.InputClear();
        }
        #endregion

        #region Rewrite unentered input to console and reposition cursor
        /// <summary>
        /// Rewrite the current command to the console window and then move the cursor
        /// </summary>
        /// <param name="state">The <see cref="ConsoleState"/> for the current console session.</param>
        /// <param name="cursorPosition">The current position of the cursor.</param>
        /// <param name="cursorMoveX">Shift the cursor this many places. -1 will emulate the left arrow, +1 the right arrow</param>
        public static void UpdateCurrentCommand(ConsoleState state, NativeMethods.COORD cursorPosition, int cursorMoveX)
        {
            // Total additional on-screen length of state.Input.Text due to tab characters
            int totalTabOffset = state.Input.Text.ToString().Contains('\t') ? ConsoleCursorUtils.GetTabOffset(state, state.Input.Length, false) : 0;
            // On-screen end of input (CursorLeft)
            state.Input.EndPosition.X = (short)((state.Input.StartPosition.X + state.Input.Length + totalTabOffset) % Console.BufferWidth);
            // On-screen end of input (CursorTop)
            state.Input.EndPosition.Y = (short)(state.Input.StartPosition.Y + ((state.Input.StartPosition.X + state.Input.Length + totalTabOffset) / Console.BufferWidth));
            // Disable cursor, rewrite prompt and user input
            Console.CursorVisible = false;
            state.MoveCursorToStartOfInput();
            Console.Write(state.Input.Text.ToString());
            // Cursor needs moving to the right
            if (cursorMoveX > 0 && (cursorPosition.Y < state.Input.EndPosition.Y || cursorPosition.X < state.Input.EndPosition.X))
            {
                if (cursorPosition.Y < state.Input.EndPosition.Y && cursorPosition.X + cursorMoveX == Console.BufferWidth - 1)
                {
                    Console.SetCursorPosition(cursorMoveX - 1, cursorPosition.Y + 1);
                }
                else if (cursorPosition.Y < state.Input.EndPosition.Y || cursorPosition.X < state.Input.EndPosition.X)
                {
                    int positionX = (cursorPosition.X + cursorMoveX) % Console.BufferWidth;
                    int offsetY = (cursorPosition.X + cursorMoveX) / Console.BufferWidth;
                    Console.SetCursorPosition(positionX, cursorPosition.Y + offsetY);
                }
                else
                {
                    Console.SetCursorPosition(cursorPosition.X, cursorPosition.Y);
                }
            }
            // Cursor needs moving to the left
            else if (cursorMoveX < 0 && (cursorPosition.Y > state.Input.StartPosition.Y || cursorPosition.X > state.Input.StartPosition.X))
            {
                if (cursorPosition.Y > state.Input.StartPosition.Y && cursorPosition.X + cursorMoveX < 0)
                {
                    Console.SetCursorPosition(Console.BufferWidth + cursorMoveX, cursorPosition.Y - 1);
                }
                else if (cursorPosition.Y > state.Input.StartPosition.Y || cursorPosition.X > state.Input.StartPosition.X)
                {
                    Console.SetCursorPosition(cursorPosition.X + cursorMoveX, cursorPosition.Y);
                }
                else
                {
                    Console.SetCursorPosition(cursorPosition.X, cursorPosition.Y);
                }
            }
            // Cursor doesn't need moving
            else if (cursorMoveX == 0)
            {
                Console.SetCursorPosition(cursorPosition.X, cursorPosition.Y);
            }
            Console.CursorVisible = true;
        }
        #endregion
    }
}
