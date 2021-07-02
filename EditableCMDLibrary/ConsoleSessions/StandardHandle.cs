using System;
using uk.JohnCook.dotnet.EditableCMDLibrary.Interop;

namespace uk.JohnCook.dotnet.EditableCMDLibrary.ConsoleSessions
{
    /// <summary>
    /// A wrapper around a HANDLE/IntPtr for a standard stream (stdin/stdout/stderr).
    /// </summary>
    public class StandardHandle
    {
        /// <summary>
        /// The handle for the standard stream.
        /// </summary>
        public IntPtr Handle { get; }
        /// <summary>
        /// The original console mode of the standard stream.
        /// </summary>
        public uint OriginalMode { get; }
        /// <summary>
        /// The current console mode of the standard stream.
        /// </summary>
        public uint CurrentMode { get; set; }
        /// <summary>
        /// True if getting/changing the stream's console mode has been successful, False if there hasn't been a success since the last failure.
        /// </summary>
        public bool CanSetMode { get; set; }

        /// <summary>
        /// An event fired when a standard stream's mode change has succeeded.
        /// </summary>
        public event EventHandler<uint>? ModeChanged;
        /// <summary>
        /// An event fired when a standard stream's mode change has failed.
        /// </summary>
        public event EventHandler<uint>? ModeSetFailed;

        /// <summary>
        /// Wrapper for a standard stream.
        /// </summary>
        /// <param name="nStdHandle">A standard stream identifier, such as <see cref="NativeMethods.STD_INPUT_HANDLE"/>.</param>
        public StandardHandle(int nStdHandle)
        {
            CanSetMode = false;
            Handle = NativeMethods.GetStdHandle(nStdHandle);
            if (Handle.ToInt32() != -1 && NativeMethods.GetConsoleMode(Handle, out uint mode))
            {
                OriginalMode = mode;
                CurrentMode = mode;
                CanSetMode = true;
            }
        }

        /// <summary>
        /// Change the mode for the standard stream using <see cref="NativeMethods.SetConsoleMode(IntPtr, uint)"/>.
        /// </summary>
        /// <param name="mode">The mode to change to</param>
        /// <returns>True on success. False if mode cannot be set.</returns>
        public bool SetMode(uint mode)
        {
            if (!NativeMethods.SetConsoleMode(Handle, mode))
            {
                CanSetMode = false;
                ModeSetFailed?.Invoke(this, mode);
                return false;
            }
            CurrentMode = mode;
            ModeChanged?.Invoke(this, CurrentMode);
            return true;
        }

        /// <summary>
        /// Reset the mode for the standard stream to what it was when created.
        /// </summary>
        /// <returns>True on success. False if mode cannot be set.</returns>
        public bool ResetMode()
        {
            return SetMode(OriginalMode);
        }
    }
}
