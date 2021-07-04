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
        /// Name of the plugin.
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        /// Summary of the plugin's functionality.
        /// </summary>
        public abstract string Description { get; }
        /// <summary>
        /// Author's name (can be <see cref="string.Empty"/>)
        /// </summary>
        public abstract string AuthorName { get; }
        /// <summary>
        /// Author's Twitch username (can be <see cref="string.Empty"/>)
        /// </summary>
        public abstract string AuthorTwitchUsername { get; }
        /// <summary>
        /// An array of the keys handled by the plugin. For commands, this should be <see cref="ConsoleKey.Enter"/>.
        /// </summary>
        public abstract ConsoleKey[]? KeysHandled { get; }
        /// <summary>
        /// Whether the plugin handles keys/commands input in normal mode (such as a command entered at the prompt).
        /// </summary>
        public abstract bool NormalModeHandled { get; }
        /// <summary>
        /// Whether the plugin handles keys input in edit mode.
        /// </summary>
        public abstract bool EditModeHandled { get; }
        /// <summary>
        /// Whether the plugin handles keys input in mark mode.
        /// </summary>
        public abstract bool MarkModeHandled { get; }
        /// <summary>
        /// An array of commands handled by the plugin, in lowercase.
        /// </summary>
        public abstract string[]? CommandsHandled { get; }

        /// <summary>
        /// Called when adding an implementation of the interface to the list of event handlers. Approximately equivalent to a constructor.
        /// </summary>
        /// <param name="state">The <see cref="ConsoleState"/> for the current console session.</param>
        public void Init(ConsoleState state)
        {

        }

        /// <summary>
        /// Event handler for a <see cref="KeyPress"/>, or a command <c>if (<paramref name="e"/>.Key == <see cref="ConsoleKey.Enter"/>)</c>.
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">The ConsoleKeyEventArgs for the event</param>
        public void ProcessCommand(object? sender, NativeMethods.ConsoleKeyEventArgs e)
        {
            e.Handled = false;
            return;
        }
    }
}
