using uk.JohnCook.dotnet.EditableCMDLibrary.Interop;
using static uk.JohnCook.dotnet.EditableCMDLibrary.Interop.NativeMethods;

namespace uk.JohnCook.dotnet.EditableCMDLibrary.Utils
{
    /// <summary>
    /// Class for Win32 utility/helper methods that abstract <see cref="Interop.NativeMethods"/> platform invoke (P/Invoke) methods.
    /// </summary>
    public class Win32Utils
    {
        /// <summary>
        /// Adds an application-defined HandlerRoutine function from the list of handler functions for the calling process.
        /// </summary>
        /// <param name="handlerRoutine">A pointer to the application-defined HandlerRoutine function to be added.</param>
        /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To get extended error information, call <see cref="GetLastError">GetLastError</see>.</returns>
        public static bool AddConsoleCtrlHandler(SetConsoleCtrlEventHandler handlerRoutine)
        {
            return SetConsoleCtrlHandler(handlerRoutine, true);
        }

        /// <summary>
        /// Removes an application-defined HandlerRoutine function from the list of handler functions for the calling process.
        /// </summary>
        /// <param name="handlerRoutine">A pointer to the application-defined HandlerRoutine function to be removed.</param>
        /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To get extended error information, call <see cref="GetLastError">GetLastError</see>.</returns>
        public static bool RemoveConsoleCtrlHandler(SetConsoleCtrlEventHandler handlerRoutine)
        {
            return SetConsoleCtrlHandler(handlerRoutine, false);
        }

        /// <summary>
        /// <para>
        /// Retrieves the calling thread's last-error code value.
        /// </para>
        /// <para>
        /// Microsoft Documentation: <seealso href="https://docs.microsoft.com/en-us/windows/win32/api/errhandlingapi/nf-errhandlingapi-getlasterror">GetLastError function</seealso>
        /// </para>
        /// </summary>
        /// <returns>The return value is the calling thread's last-error code.</returns>
        public static uint GetLastError()
        {
            return NativeMethods.GetLastError();
        }
    }
}
