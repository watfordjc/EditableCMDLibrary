using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uk.JohnCook.dotnet.EditableCMDLibrary.ConsoleSessions;
using uk.JohnCook.dotnet.EditableCMDLibrary.Interop;
using static uk.JohnCook.dotnet.EditableCMDLibrary.Utils.KeyUtils;

namespace uk.JohnCook.dotnet.EditableCMDLibrary.Commands
{
    /// <summary>
    /// An interface for handling keyboard input or command input.
    /// </summary>
    public interface ICommandInput
    {
        /// <summary>
        /// Called when adding an implementation of the interface to the list of event handlers. Approximately equivalent to a constructor.
        /// </summary>
        /// <param name="state">The <see cref="ConsoleState"/> for the current console session.</param>
        public abstract void Init(ConsoleState state);

        /// <summary>
        /// Event handler for a <see cref="KeyPress"/>, or a command <c> if (<paramref name="e"/>.Key == <see cref="ConsoleKey.Enter"/>)</c>.
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">The ConsoleKeyEventArgs for the event</param>
        public abstract void ProcessCommand(object sender, NativeMethods.ConsoleKeyEventArgs e);
    }
}
