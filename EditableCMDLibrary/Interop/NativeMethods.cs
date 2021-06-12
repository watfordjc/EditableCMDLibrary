using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using uk.JohnCook.dotnet.EditableCMDLibrary.ConsoleSessions;
using uk.JohnCook.dotnet.EditableCMDLibrary.Utils;

namespace uk.JohnCook.dotnet.EditableCMDLibrary.Interop
{
    /// <summary>
    /// Class containing platform interop function headers and their related data structures/constants/etc.
    /// </summary>
    public class NativeMethods
    {
        #region GetStartupInfo()

        /// <summary>
        /// <para>
        /// Retrieves the contents of the <see cref="STARTUPINFO">STARTUPINFO</see> structure that was specified when the calling process was created.
        /// </para>
        /// <para>
        /// Microsoft Documentation: <seealso href="https://docs.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-getstartupinfow">GetStartupInfoW function</seealso>
        /// </para>
        /// </summary>
        /// <param name="lpStartupInfo">A pointer to a STARTUPINFO structure that receives the startup information.</param>
        [DllImport("kernel32.dll", SetLastError = false, CharSet = CharSet.Ansi, EntryPoint = "GetStartupInfoA")]
        internal static extern void GetStartupInfo(out STARTUPINFO lpStartupInfo);

        /// <summary>
        /// <para>
        /// Specifies the window station, desktop, standard handles, and attributes for a new process. It is used with the CreateProcess and CreateProcessAsUser functions.
        /// </para>
        /// <para>
        /// Microsoft Documentation: <seealso href="https://docs.microsoft.com/en-us/windows/win32/api/winbase/ns-winbase-startupinfoexa">STARTUPINFOEXA structure</seealso>
        /// </para>
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct STARTUPINFOEX
        {
            /// <summary>
            /// A <see cref="STARTUPINFO"/> structure.
            /// </summary>
            public STARTUPINFO StartupInfo;
            /// <summary>
            /// An attribute list. This list is created by the InitializeProcThreadAttributeList function.
            /// </summary>
            public IntPtr lpAttributeList;
        }

        /// <summary>
        /// <para>
        /// Specifies the window station, desktop, standard handles, and appearance of the main window for a process at creation time.
        /// </para>
        /// <para>
        /// Microsoft Documentation: <seealso href="https://docs.microsoft.com/en-us/windows/win32/api/processthreadsapi/ns-processthreadsapi-startupinfoa">STARTUPINFOA structure</seealso>
        /// </para>
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct STARTUPINFO
        {
            /// <summary>
            /// The size of the structure, in bytes.
            /// </summary>
            public Int32 cb;
            /// <summary>
            /// Reserved; must be NULL.
            /// </summary>
            public string lpReserved;
            /// <summary>
            /// The name of the desktop, or the name of both the desktop and window station for this process.
            /// </summary>
            public string lpDesktop;
            /// <summary>
            /// For console processes, this is the title displayed in the title bar if a new console window is created.
            /// </summary>
            public string lpTitle;
            /// <summary>
            /// If dwFlags specifies STARTF_USEPOSITION, this member is the x offset of the upper left corner of a window if a new window is created, in pixels. Otherwise, this member is ignored.
            /// </summary>
            public Int32 dwX;
            /// <summary>
            /// If dwFlags specifies STARTF_USEPOSITION, this member is the y offset of the upper left corner of a window if a new window is created, in pixels. Otherwise, this member is ignored.
            /// </summary>
            public Int32 dwY;
            /// <summary>
            /// If dwFlags specifies STARTF_USESIZE, this member is the width of the window if a new window is created, in pixels. Otherwise, this member is ignored.
            /// </summary>
            public Int32 dwXSize;
            /// <summary>
            /// If dwFlags specifies STARTF_USESIZE, this member is the height of the window if a new window is created, in pixels. Otherwise, this member is ignored.
            /// </summary>
            public Int32 dwYSize;
            /// <summary>
            /// If dwFlags specifies STARTF_USECOUNTCHARS, if a new console window is created in a console process, this member specifies the screen buffer width, in character columns. Otherwise, this member is ignored.
            /// </summary>
            public Int32 dwXCountChars;
            /// <summary>
            /// If dwFlags specifies STARTF_USECOUNTCHARS, if a new console window is created in a console process, this member specifies the screen buffer height, in character rows. Otherwise, this member is ignored.
            /// </summary>
            public Int32 dwYCountChars;
            /// <summary>
            /// If dwFlags specifies STARTF_USEFILLATTRIBUTE, this member is the initial text and background colors if a new console window is created in a console application. Otherwise, this member is ignored.
            /// </summary>
            public Int32 dwFillAttribute;
            /// <summary>
            /// A bitfield that determines whether certain <see cref="STARTUPINFO"/> members are used when the process creates a window.
            /// </summary>
            public Int32 dwFlags;
            /// <summary>
            /// If dwFlags specifies STARTF_USESHOWWINDOW, this member can be any of the values that can be specified in the nCmdShow parameter for the ShowWindow function, except for SW_SHOWDEFAULT. Otherwise, this member is ignored.
            /// </summary>
            public Int16 wShowWindow;
            /// <summary>
            /// Reserved for use by the C Run-time; must be zero.
            /// </summary>
            public Int16 cbReserved2;
            /// <summary>
            /// Reserved for use by the C Run-time; must be NULL.
            /// </summary>
            public IntPtr lpReserved2;
            /// <summary>
            /// If dwFlags specifies STARTF_USESTDHANDLES, this member is the standard input handle for the process. If STARTF_USESTDHANDLES is not specified, the default for standard input is the keyboard buffer.
            /// </summary>
            public IntPtr hStdInput;
            /// <summary>
            /// If dwFlags specifies STARTF_USESTDHANDLES, this member is the standard output handle for the process. Otherwise, this member is ignored and the default for standard output is the console window's buffer.
            /// </summary>
            public IntPtr hStdOutput;
            /// <summary>
            /// If dwFlags specifies STARTF_USESTDHANDLES, this member is the standard error handle for the process. Otherwise, this member is ignored and the default for standard error is the console window's buffer.
            /// </summary>
            public IntPtr hStdError;
        }

        #endregion

        #region SetConsoleCtrlHandler()

        // Import SetConsoleCtrlHandler() Win32 method
        /// <summary>
        /// <para>
        /// Adds or removes an application-defined HandlerRoutine function from the list of handler functions for the calling process.
        /// </para>
        /// <para>
        /// <seealso href="https://docs.microsoft.com/en-us/windows/console/setconsolectrlhandler">SetConsoleCtrlHandler function</seealso>
        /// </para>
        /// </summary>
        /// <param name="handlerRoutine">A pointer to the application-defined HandlerRoutine function to be added or removed.</param>
        /// <param name="add">If this parameter is TRUE, the handler is added; if it is FALSE, the handler is removed.</param>
        /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To get extended error information, call <see cref="GetLastError">GetLastError</see>.</returns>
        [DllImport("Kernel32")]
        internal static extern bool SetConsoleCtrlHandler(SetConsoleCtrlEventHandler handlerRoutine, bool add);

        /// <summary>
        /// Delegate for <see cref="SetConsoleCtrlHandler(SetConsoleCtrlEventHandler, bool)"/>.
        /// </summary>
        /// <param name="sig">The <see cref="ConsoleControlType"/> signal.</param>
        /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To get extended error information, call <see cref="GetLastError">GetLastError</see>.</returns>
        public delegate bool SetConsoleCtrlEventHandler(NativeMethods.ConsoleControlType sig);

        /// <summary>
        /// <para>
        /// Constants from HandlerRoutine
        /// </para>
        /// <para>
        /// Microsoft Documentation: <seealso href="https://docs.microsoft.com/en-us/windows/console/handlerroutine">HandlerRoutine callback function</seealso>
        /// </para>
        /// </summary>
        public enum ConsoleControlType
        {
            /// <summary>
            /// Ctrl+C
            /// </summary>
            CTRL_C_EVENT = 0,
            /// <summary>
            /// Ctrl+Break
            /// </summary>
            CTRL_BREAK_EVENT = 1,
            /// <summary>
            /// Application is closing
            /// </summary>
            CTRL_CLOSE_EVENT = 2,
            /// <summary>
            /// User is logging off
            /// </summary>
            CTRL_LOGOFF_EVENT = 5,
            /// <summary>
            /// Computer is shutting down
            /// </summary>
            CTRL_SHUTDOWN_EVENT = 6
        }

        #endregion

        #region GetConsoleMode(), SetConsoleMode(), GetLastError(), FormatMessage(), GetStdHandle()

        /// <summary>
        /// <para>
        /// Retrieves the current input mode of a console's input buffer or the current output mode of a console screen buffer.
        /// </para>
        /// <para>
        /// Microsoft Documentation: <seealso href="https://docs.microsoft.com/en-us/windows/console/getconsolemode">GetConsoleMode function</seealso>
        /// </para>
        /// </summary>
        /// <param name="hConsoleHandle">A handle to the console input buffer or the console screen buffer. The handle must have the GENERIC_READ access right.</param>
        /// <param name="lpMode">A pointer to a variable that receives the current mode of the specified buffer.</param>
        /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To get extended error information, call <see cref="GetLastError">GetLastError</see>.</returns>
        /// 
        [DllImport("kernel32.dll")]
        internal static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        /// <summary>
        /// <para>
        /// Sets the input mode of a console's input buffer or the output mode of a console screen buffer.
        /// </para>
        /// <para>
        /// Microsoft Documentation: <seealso href="https://docs.microsoft.com/en-us/windows/console/setconsolemode">SetConsoleMode function</seealso>
        /// </para>
        /// </summary>
        /// <param name="hConsoleHandle">A handle to the console input buffer or a console screen buffer. The handle must have the GENERIC_READ access right.</param>
        /// <param name="dwMode">The input or output mode to be set.</param>
        /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero.To get extended error information, call <see cref="GetLastError">GetLastError</see>.</returns>
        [DllImport("kernel32.dll")]
        internal static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        #region Mode constants buffer handles
        /// <summary>
        /// Characters read by the ReadFile or ReadConsole function are written to the active screen buffer as they are read. This mode can be used only if the <see cref="ENABLE_LINE_INPUT"/> mode is also enabled.
        /// </summary>
        public const uint ENABLE_ECHO_INPUT = 0x0004;
        /// <summary>
        /// When enabled, text entered in a console window will be inserted at the current cursor location and all text following that location will not be overwritten. When disabled, all following text will be overwritten.
        /// </summary>
        public const uint ENABLE_INSERT_MODE = 0x0020;
        /// <summary>
        /// The ReadFile or ReadConsole function returns only when a carriage return character is read. If this mode is disabled, the functions return when one or more characters are available.
        /// </summary>
        public const uint ENABLE_LINE_INPUT = 0x0002;
        /// <summary>
        /// If the mouse pointer is within the borders of the console window and the window has the keyboard focus, mouse events generated by mouse movement and button presses are placed in the input buffer.
        /// </summary>
        public const uint ENABLE_MOUSE_INPUT = 0x0010;
        /// <summary>
        /// CTRL+C is processed by the system and is not placed in the input buffer. If the input buffer is being read by ReadFile or ReadConsole, other control keys are processed by the system and are not returned in the ReadFile or ReadConsole buffer. If the <see cref="ENABLE_LINE_INPUT"/> mode is also enabled, backspace, carriage return, and line feed characters are handled by the system.
        /// </summary>
        public const uint ENABLE_PROCESSED_INPUT = 0x0001;
        /// <summary>
        /// This flag enables the user to use the mouse to select and edit text. To enable this mode, use <c><see cref="ENABLE_QUICK_EDIT_MODE"/> | ENABLE_EXTENDED_FLAGS</c>. To disable this mode, use <c>ENABLE_EXTENDED_FLAGS</c> without this flag.
        /// </summary>
        public const uint ENABLE_QUICK_EDIT_MODE = 0x0040;
        /// <summary>
        /// User interactions that change the size of the console screen buffer are reported in the console's input buffer.
        /// </summary>
        public const uint ENABLE_WINDOW_INPUT = 0x0008;
        /// <summary>
        /// Setting this flag directs the Virtual Terminal processing engine to convert user input received by the console window into Console Virtual Terminal Sequences that can be retrieved by a supporting application through WriteFile or WriteConsole functions.
        /// </summary>
        public const uint ENABLE_VIRTUAL_TERMINAL_INPUT = 0x0200;
        #endregion
        #region Mode constants for input handles
        /// <summary>
        /// Characters written by the WriteFile or WriteConsole function or echoed by the ReadFile or ReadConsole function are parsed for ASCII control sequences, and the correct action is performed. Backspace, tab, bell, carriage return, and line feed characters are processed.
        /// </summary>
        public const uint ENABLE_PROCESSED_OUTPUT = 0x0001;
        /// <summary>
        /// When writing with WriteFile or WriteConsole or echoing with ReadFile or ReadConsole, the cursor moves to the beginning of the next row when it reaches the end of the current row.
        /// </summary>
        public const uint ENABLE_WRAP_AT_EOL_OUTPUT = 0x0002;
        /// <summary>
        /// When writing with WriteFile or WriteConsole, characters are parsed for VT100 and similar control character sequences that control cursor movement, color/font mode, and other operations that can also be performed via the existing Console APIs.
        /// </summary>
        public const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
        /// <summary>
        /// When writing with WriteFile or WriteConsole, this adds an additional state to end-of-line wrapping that can delay the cursor move and buffer scroll operations.
        /// </summary>
        public const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;
        /// <summary>
        /// The APIs for writing character attributes including WriteConsoleOutput and WriteConsoleOutputAttribute allow the usage of flags from character attributes to adjust the color of the foreground and background of text.
        /// </summary>
        public const uint ENABLE_LVB_GRID_WORLDWIDE = 0x0010;
        #endregion

        /// <summary>
        /// <para>
        /// Retrieves the calling thread's last-error code value.
        /// </para>
        /// <para>
        /// Microsoft Documentation: <seealso href="https://docs.microsoft.com/en-us/windows/win32/api/errhandlingapi/nf-errhandlingapi-getlasterror">GetLastError function</seealso>
        /// </para>
        /// </summary>
        /// <returns>The return value is the calling thread's last-error code.</returns>
        [DllImport("kernel32.dll")]
        internal static extern uint GetLastError();

        /// <summary>
        /// <para>
        /// Formats a message string. The function requires a message definition as input.
        /// </para>
        /// <para>
        /// Microsoft Documentation: <seealso href="https://docs.microsoft.com/en-gb/windows/win32/api/winbase/nf-winbase-formatmessage?redirectedfrom=MSDN">FormatMessage function</seealso>
        /// </para>
        /// </summary>
        /// <param name="dwFlags">The formatting options, and how to interpret the lpSource parameter.</param>
        /// <param name="lpSource">The location of the message definition. The type of this parameter depends upon the settings in the dwFlags parameter.</param>
        /// <param name="dwMessageId">The message identifier for the requested message. This parameter is ignored if dwFlags includes FORMAT_MESSAGE_FROM_STRING.</param>
        /// <param name="dwLanguageId">The language identifier for the requested message. This parameter is ignored if dwFlags includes FORMAT_MESSAGE_FROM_STRING.</param>
        /// <param name="lpBuffer">A pointer to a buffer that receives the null-terminated string that specifies the formatted message. If dwFlags includes FORMAT_MESSAGE_ALLOCATE_BUFFER, the function allocates a buffer using the LocalAlloc function, and places the pointer to the buffer at the address specified in lpBuffer.</param>
        /// <param name="nSize">If the FORMAT_MESSAGE_ALLOCATE_BUFFER flag is not set, this parameter specifies the size of the output buffer, in TCHARs. If FORMAT_MESSAGE_ALLOCATE_BUFFER is set, this parameter specifies the minimum number of TCHARs to allocate for an output buffer.</param>
        /// <param name="Arguments">An array of values that are used as insert values in the formatted message. A %1 in the format string indicates the first value in the Arguments array; a %2 indicates the second argument; and so on.</param>
        /// <returns>If the function succeeds, the return value is the number of TCHARs stored in the output buffer, excluding the terminating null character. If the function fails, the return value is zero. To get extended error information, call <see cref="GetLastError">GetLastError</see>.</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal static extern uint FormatMessage(uint dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId, [Out] StringBuilder lpBuffer, uint nSize, IntPtr Arguments);
        /// <summary>
        /// The function allocates a buffer large enough to hold the formatted message, and places a pointer to the allocated buffer at the address specified by lpBuffer.
        /// </summary>
        public const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
        /// <summary>
        /// The Arguments parameter is not a va_list structure, but is a pointer to an array of values that represent the arguments.
        /// </summary>
        public const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000;
        /// <summary>
        /// The lpSource parameter is a module handle containing the message-table resource(s) to search. If this lpSource handle is NULL, the current process's application image file will be searched. This flag cannot be used with <see cref="FORMAT_MESSAGE_FROM_STRING"/>.
        /// </summary>
        public const int FORMAT_MESSAGE_FROM_HMODULE = 0x00000800;
        /// <summary>
        /// The lpSource parameter is a pointer to a null-terminated string that contains a message definition. The message definition may contain insert sequences, just as the message text in a message table resource may. This flag cannot be used with <see cref="FORMAT_MESSAGE_FROM_HMODULE"/> or <see cref="FORMAT_MESSAGE_FROM_SYSTEM"/>.
        /// </summary>
        public const int FORMAT_MESSAGE_FROM_STRING = 0x00000400;
        /// <summary>
        /// The function should search the system message-table resource(s) for the requested message.
        /// </summary>
        public const int FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
        /// <summary>
        /// Insert sequences in the message definition such as %1 are to be ignored and passed through to the output buffer unchanged. This flag is useful for fetching a message for later formatting. If this flag is set, the Arguments parameter is ignored.
        /// </summary>
        public const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
        /// <summary>
        /// The function ignores regular line breaks in the message definition text.
        /// </summary>
        public const int FORMAT_MESSAGE_MAX_WIDTH_MASK = 0x000000FF;

        /// <summary>
        /// <para>
        /// Retrieves a handle to the specified standard device (standard input, standard output, or standard error).
        /// </para>
        /// <para>
        /// Microsoft Documentation: <seealso href="https://docs.microsoft.com/en-us/windows/console/getstdhandle">GetStdHandle function</seealso>
        /// </para>
        /// </summary>
        /// <param name="nStdHandle">The standard device.</param>
        /// <returns>If the function succeeds, the return value is a handle to the specified device. If the function fails, the return value is INVALID_HANDLE_VALUE (-1). To get extended error information, call <see cref="GetLastError">GetLastError</see>.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetStdHandle(int nStdHandle);
        /// <summary>
        /// The standard input device. Initially, this is the console input buffer, <c>CONIN$</c>.
        /// </summary>
        public const int STD_INPUT_HANDLE = -10;
        /// <summary>
        /// The standard output device. Initially, this is the active console screen buffer, <c>CONOUT$</c>.
        /// </summary>
        public const int STD_OUTPUT_HANDLE = -11;
        /// <summary>
        /// The standard error device. Initially, this is the active console screen buffer, <c>CONOUT$</c>.
        /// </summary>
        public const int STD_ERROR_HANDLE = -12;

        #endregion

        #region WriteConsoleInput(), ReadConsoleInput(), PeakConsoleInput()

        /// <summary>
        /// <para>
        /// Writes data directly to the console input buffer.
        /// </para>
        /// <para>
        /// Microsoft Documentation: <seealso href="https://docs.microsoft.com/en-us/windows/console/writeconsoleinput">WriteConsoleInput function</seealso>
        /// </para>
        /// </summary>
        /// <param name="hConsoleInput">A handle to the console input buffer. The handle must have the GENERIC_WRITE access right.</param>
        /// <param name="lpBuffer">A pointer to an array of INPUT_RECORD structures that contain data to be written to the input buffer.</param>
        /// <param name="nLength">The number of input records to be written.</param>
        /// <param name="lpNumberOfEventsWritten">A pointer to a variable that receives the number of input records actually written.</param>
        /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To get extended error information, call <see cref="GetLastError">GetLastError</see>.</returns>
        [DllImport("kernel32.dll", EntryPoint = "WriteConsoleInputW", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool WriteConsoleInput(IntPtr hConsoleInput, INPUT_RECORD[] lpBuffer, uint nLength, out uint lpNumberOfEventsWritten);

        /// <summary>
        /// <para>
        /// Reads data from a console input buffer and removes it from the buffer.
        /// </para>
        /// <para>
        /// Microsoft Documentation: <seealso href="https://docs.microsoft.com/en-us/windows/console/readconsoleinput">ReadConsoleInput function</seealso>
        /// </para>
        /// </summary>
        /// <param name="hConsoleInput">A handle to the console input buffer. The handle must have the GENERIC_READ access right.</param>
        /// <param name="lpBuffer">A pointer to an array of INPUT_RECORD structures that receives the input buffer data.</param>
        /// <param name="nLength">The size of the array pointed to by the lpBuffer parameter, in array elements.</param>
        /// <param name="lpNumberOfEventsRead">A pointer to a variable that receives the number of input records read.</param>
        /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero.To get extended error information, call <see cref="GetLastError">GetLastError</see>.</returns>
        [DllImport("kernel32.dll", EntryPoint = "ReadConsoleInputW", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool ReadConsoleInput(IntPtr hConsoleInput, [MarshalAs(UnmanagedType.LPArray), Out] INPUT_RECORD[] lpBuffer, uint nLength, out uint lpNumberOfEventsRead);

        /// <summary>
        /// <para>
        /// Reads data from the specified console input buffer without removing it from the buffer.
        /// </para>
        /// <para>
        /// Microsoft Documentation: <seealso href="https://docs.microsoft.com/en-us/windows/console/peekconsoleinput">PeekConsoleInput function</seealso>
        /// </para>
        /// </summary>
        /// <param name="hConsoleInput">A handle to the console input buffer. The handle must have the GENERIC_READ access right.</param>
        /// <param name="lpBuffer">A pointer to an array of INPUT_RECORD structures that receives the input buffer data.</param>
        /// <param name="nLength">The size of the array pointed to by the lpBuffer parameter, in array elements.</param>
        /// <param name="lpNumberOfEventsRead">A pointer to a variable that receives the number of input records read.</param>
        /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero.To get extended error information, call <see cref="GetLastError">GetLastError</see>.</returns>
        [DllImport("kernel32.dll", EntryPoint = "PeekConsoleInputW", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool PeakConsoleInput(IntPtr hConsoleInput, [MarshalAs(UnmanagedType.LPArray), Out] INPUT_RECORD[] lpBuffer, uint nLength, out uint lpNumberOfEventsRead);

        /// <summary>
        /// <para>
        /// Describes an input event in the console input buffer.
        /// </para>
        /// <para>
        /// Microsoft Documentation: <seealso href="https://docs.microsoft.com/en-us/windows/console/input-record-str">INPUT_RECORD structure</seealso>
        /// </para>
        /// </summary>
        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
        public struct INPUT_RECORD
        {
            /// <summary>
            /// The Event member contains a FOCUS_EVENT_RECORD structure. These events are used publicly and should be ignored.
            /// </summary>
            public const ushort FOCUS_EVENT = 0x0010;
            /// <summary>
            /// The Event member contains a <see cref="KEY_EVENT_RECORD"/> structure with information about a keyboard event.
            /// </summary>
            public const ushort KEY_EVENT = 0x0001;
            /// <summary>
            /// The Event member contains a MENU_EVENT_RECORD structure. These events are used publicly and should be ignored.
            /// </summary>
            public const ushort MENU_EVENT = 0x0008;
            /// <summary>
            /// The Event member contains a <see cref="MOUSE_EVENT_RECORD"/> structure with information about a mouse movement or button press event.
            /// </summary>
            public const ushort MOUSE_EVENT = 0x0002;
            /// <summary>
            /// The Event member contains a <see cref="WINDOW_BUFFER_SIZE_RECORD"/> structure with information about the new size of the console screen buffer.
            /// </summary>
            public const ushort WINDOW_BUFFER_SIZE_EVENT = 0x0004;
            /// <summary>
            /// A handle to the type of input event and the event record stored in the Event member.
            /// </summary>
            [FieldOffset(0)]
            public ushort EventType;

            // These are a union
            /// <summary>
            /// The event information. The format of this member depends on the event type specified by the EventType member.
            /// </summary>
            [FieldOffset(4)]
            public KEY_EVENT_RECORD KeyEvent;

            /// <summary>
            /// The event information. The format of this member depends on the event type specified by the EventType member.
            /// </summary>
            [FieldOffset(4)]
            public MOUSE_EVENT_RECORD MouseEvent;

            /// <summary>
            /// The event information. The format of this member depends on the event type specified by the EventType member.
            /// </summary>
            [FieldOffset(4)]
            public WINDOW_BUFFER_SIZE_RECORD WindowBufferSizeEvent;

            /*
            MENU_EVENT_RECORD MenuEvent;
            FOCUS_EVENT_RECORD FocusEvent; */
            // MSDN claims that these are used publicly and shouldn't be used

        }

        /// <summary>
        /// <para>
        /// Defines the coordinates of a character cell in a console screen buffer.
        /// </para>
        /// <para>
        /// Microsoft Documentation: <seealso href="https://docs.microsoft.com/en-us/windows/console/coord-str">COORD structure</seealso>
        /// </para>
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct COORD
        {
            /// <summary>
            /// The horizontal coordinate or column value. The units depend on the function call.
            /// </summary>
            public short X;
            /// <summary>
            /// The vertical coordinate or row value. The units depend on the function call.
            /// </summary>
            public short Y;

            /// <summary>
            /// Defines the coordinates of a character cell in a console screen buffer. The origin of the coordinate system (0,0) is at the top, left cell of the buffer.
            /// </summary>
            /// <param name="x">The horizontal coordinate or column value. The units depend on the function call.</param>
            /// <param name="y">The vertical coordinate or row value. The units depend on the function call.</param>
            public COORD(short x, short y)
            {
                X = x;
                Y = y;
            }
        }

        /// <summary>
        /// <para>
        /// Describes a mouse input event in a console <see cref="INPUT_RECORD">INPUT_RECORD</see> structure.
        /// </para>
        /// <para>
        /// Microsoft Documentation: <seealso href="https://docs.microsoft.com/en-us/windows/console/mouse-event-record-str">MOUSE_EVENT_RECORD structure</seealso>
        /// </para>
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public struct MOUSE_EVENT_RECORD
        {
            /// <summary>
            /// A <see cref="COORD"/> structure that contains the location of the cursor, in terms of the console screen buffer's character-cell coordinates.
            /// </summary>
            [FieldOffset(0)]
            public COORD dwMousePosition;

            /// <summary>
            /// The leftmost mouse button.
            /// </summary>
            public const uint FROM_LEFT_1ST_BUTTON_PRESSED = 0x0001;
            /// <summary>
            /// The second button fom the left.
            /// </summary>
            public const uint FROM_LEFT_2ND_BUTTON_PRESSED = 0x0004;
            /// <summary>
            /// The third button from the left.
            /// </summary>
            public const uint FROM_LEFT_3RD_BUTTON_PRESSED = 0x0008;
            /// <summary>
            /// The fourth button from the left.
            /// </summary>
            public const uint FROM_LEFT_4TH_BUTTON_PRESSED = 0x0010;
            /// <summary>
            /// The rightmost mouse button.
            /// </summary>
            public const uint RIGHTMOST_BUTTON_PRESSED = 0x0002;
            /// <summary>
            /// The status of the mouse buttons. The least significant bit corresponds to the leftmost mouse button. The next least significant bit corresponds to the rightmost mouse button. The next bit indicates the next-to-leftmost mouse button. The bits then correspond left to right to the mouse buttons. A bit is 1 if the button was pressed.
            /// </summary>
            [FieldOffset(4)]
            public uint dwButtonState;

            /// <summary>
            /// The CAPS LOCK light is on.
            /// </summary>
            public const int CAPSLOCK_ON = 0x0080;
            /// <summary>
            /// The key is enhanced.
            /// </summary>
            public const int ENHANCED_KEY = 0x0100;
            /// <summary>
            /// The left ALT key is pressed.
            /// </summary>
            public const int LEFT_ALT_PRESSED = 0x0002;
            /// <summary>
            /// The left CTRL key is pressed.
            /// </summary>
            public const int LEFT_CTRL_PRESSED = 0x0008;
            /// <summary>
            /// The NUM LOCK light is on.
            /// </summary>
            public const int NUMLOCK_ON = 0x0020;
            /// <summary>
            /// The right ALT key is pressed.
            /// </summary>
            public const int RIGHT_ALT_PRESSED = 0x0001;
            /// <summary>
            /// The right CTRL key is pressed.
            /// </summary>
            public const int RIGHT_CTRL_PRESSED = 0x0004;
            /// <summary>
            /// The SCROLL LOCK light is on.
            /// </summary>
            public const int SCROLLLOCK_ON = 0x0040;
            /// <summary>
            /// The SHIFT key is pressed.
            /// </summary>
            public const int SHIFT_PRESSED = 0x0010;
            /// <summary>
            /// The state of the control keys.
            /// </summary>
            [FieldOffset(8)]
            public uint dwControlKeyState;

            /// <summary>
            /// The second click (button press) of a double-click occurred. The first click is returned as a regular button-press event.
            /// </summary>
            public const int DOUBLE_CLICK = 0x0002;
            /// <summary>
            /// The horizontal mouse wheel was moved. If the high word of the dwButtonState member contains a positive value, the wheel was rotated to the right.Otherwise, the wheel was rotated to the left.
            /// </summary>
            public const int MOUSE_HWHEELED = 0x0008;
            /// <summary>
            /// A change in mouse position occurred.
            /// </summary>
            public const int MOUSE_MOVED = 0x0001;
            /// <summary>
            /// The vertical mouse wheel was moved. If the high word of the dwButtonState member contains a positive value, the wheel was rotated forward, away from the user. Otherwise, the wheel was rotated backward, toward the user.
            /// </summary>
            public const int MOUSE_WHEELED = 0x0004;
            /// <summary>
            /// The type of mouse event. If this value is zero, it indicates a mouse button being pressed or released.
            /// </summary>
            [FieldOffset(12)]
            public uint dwEventFlags;
        }

        /// <summary>
        /// <para>
        /// Describes a keyboard input event in a console <see cref="INPUT_RECORD">INPUT_RECORD</see> structure.
        /// </para>
        /// <para>
        /// Microsoft Documentation: <seealso href="https://docs.microsoft.com/en-us/windows/console/key-event-record-str">KEY_EVENT_RECORD structure</seealso>
        /// </para>
        /// </summary>
        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
        public struct KEY_EVENT_RECORD
        {
            /// <summary>
            /// If the key is pressed, this member is TRUE. Otherwise, this member is FALSE (the key is released).
            /// </summary>
            [FieldOffset(0)]
            public bool bKeyDown;
            /// <summary>
            /// The repeat count, which indicates that a key is being held down.
            /// </summary>
            [FieldOffset(4)]
            public ushort wRepeatCount;
            /// <summary>
            /// A virtual-key code that identifies the given key in a device-independent manner.
            /// </summary>
            [FieldOffset(6)]
            public ushort wVirtualKeyCode;
            /// <summary>
            /// The virtual scan code of the given key that represents the device-dependent value generated by the keyboard hardware.
            /// </summary>
            [FieldOffset(8)]
            public ushort wVirtualScanCode;
            /// <summary>
            /// Translated Unicode character.
            /// </summary>
            [FieldOffset(10)]
            public char UnicodeChar;
            /// <summary>
            /// Translated ASCII character.
            /// </summary>
            [FieldOffset(10)]
            public byte AsciiChar;

            /// <summary>
            /// The CAPS LOCK light is on.
            /// </summary>
            public const int CAPSLOCK_ON = 0x0080;
            /// <summary>
            /// The key is enhanced.
            /// </summary>
            public const int ENHANCED_KEY = 0x0100;
            /// <summary>
            /// The left ALT key is pressed.
            /// </summary>
            public const int LEFT_ALT_PRESSED = 0x0002;
            /// <summary>
            /// The left CTRL key is pressed.
            /// </summary>
            public const int LEFT_CTRL_PRESSED = 0x0008;
            /// <summary>
            /// The NUM LOCK light is on.
            /// </summary>
            public const int NUMLOCK_ON = 0x0020;
            /// <summary>
            /// The right ALT key is pressed.
            /// </summary>
            public const int RIGHT_ALT_PRESSED = 0x0001;
            /// <summary>
            /// The right CTRL key is pressed.
            /// </summary>
            public const int RIGHT_CTRL_PRESSED = 0x0004;
            /// <summary>
            /// The SCROLL LOCK light is on.
            /// </summary>
            public const int SCROLLLOCK_ON = 0x0040;
            /// <summary>
            /// The SHIFT key is pressed.
            /// </summary>
            public const int SHIFT_PRESSED = 0x0010;
            /// <summary>
            /// The state of the control keys.
            /// </summary>
            [FieldOffset(12)]
            public uint dwControlKeyState;
        }

        /// <summary>
        /// <para>
        /// Describes a change in the size of the console screen buffer.
        /// </para>
        /// <para>
        /// Microsoft Documentation: <seealso href="https://docs.microsoft.com/en-us/windows/console/window-buffer-size-record-str">WINDOW_BUFFER_SIZE_RECORD structure</seealso>
        /// </para>
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public struct WINDOW_BUFFER_SIZE_RECORD
        {
            /// <summary>
            /// A <see cref="COORD"/> structure that contains the size of the console screen buffer, in character cell columns and rows.
            /// </summary>
            [FieldOffset(0)]
            public COORD dwSize;
        }

        /// <summary>
        /// EventArgs for a console key event.
        /// </summary>
        public class ConsoleKeyEventArgs : HandledEventArgs
        {
            /// <summary>
            /// The <see cref="NativeMethods.KEY_EVENT_RECORD"/> received in the event.
            /// </summary>
            public KEY_EVENT_RECORD KeyEventRecord { get; }
            /// <summary>
            /// The time the <see cref="KeyEventRecord"/> was received.
            /// </summary>
            public DateTime Timestamp { get; }
            /// <summary>
            /// The <see cref="ConsoleState"/> for the current console session.
            /// </summary>
            public ConsoleState State { get; }
            /// <summary>
            /// The <see cref="KeyEventRecord"/> parsed for easier use.
            /// </summary>
            public KeyUtils.KeyPress Key { get; }

            /// <summary>
            /// The EventArgs used when a console key event is received.
            /// </summary>
            /// <param name="record">The <see cref="NativeMethods.KEY_EVENT_RECORD"/> received in the event.</param>
            /// <param name="timestamp">The time the <see cref="KeyEventRecord"/> was received.</param>
            /// <param name="state">The <see cref="ConsoleState"/> for the current console session.</param>
            public ConsoleKeyEventArgs(KEY_EVENT_RECORD record, DateTime timestamp, ConsoleState state)
            {
                KeyEventRecord = record;
                Timestamp = timestamp;
                State = state;
                Key = new(record);
            }
        }

        /// <summary>
        /// EventArgs for a console mouse event.
        /// </summary>
        public class ConsoleMouseEventArgs : HandledEventArgs
        {
            /// <summary>
            /// The <see cref="NativeMethods.MOUSE_EVENT_RECORD"/> received in the event.
            /// </summary>
            public MOUSE_EVENT_RECORD MouseEventRecord { get; }

            /// <summary>
            /// The EventArgs used when a console mouse event is received.
            /// </summary>
            /// <param name="record">The <see cref="NativeMethods.MOUSE_EVENT_RECORD"/> received in the event.</param>
            public ConsoleMouseEventArgs(MOUSE_EVENT_RECORD record)
            {
                MouseEventRecord = record;
            }
        }

        /// <summary>
        /// EventArgs for a console window buffer resize event.
        /// </summary>
        public class ConsoleWindowBufferSizeEventArgs : HandledEventArgs
        {
            /// <summary>
            /// The <see cref="NativeMethods.WINDOW_BUFFER_SIZE_RECORD"/> received in the event.
            /// </summary>
            public WINDOW_BUFFER_SIZE_RECORD WindowBufferSizeRecord { get; }

            /// <summary>
            /// The EventArgs used when a console window buffer size change event is received.
            /// </summary>
            /// <param name="record">The <see cref="NativeMethods.WINDOW_BUFFER_SIZE_RECORD"/> received in the event.</param>
            public ConsoleWindowBufferSizeEventArgs(WINDOW_BUFFER_SIZE_RECORD record)
            {
                WindowBufferSizeRecord = record;
            }
        }

        #endregion

        #region GetConsoleSelectionInfo()

        /// <summary>
        /// <para>
        /// Retrieves information about the current console selection.
        /// </para>
        /// <para>
        /// Microsoft Documentation: <seealso href="https://docs.microsoft.com/en-us/windows/console/getconsoleselectioninfo">GetConsoleSelectionInfo function</seealso>
        /// </para>
        /// </summary>
        /// <param name="lpConsoleSelectionInfo">A pointer to a CONSOLE_SELECTION_INFO structure that receives the selection information.</param>
        /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero.To get extended error information, call <see cref="GetLastError">GetLastError</see>.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool GetConsoleSelectionInfo([Out] CONSOLE_SELECTION_INFO lpConsoleSelectionInfo);

        /// <summary>
        /// <para>
        /// Contains information for a console selection.
        /// </para>
        /// <para>
        /// Microsoft Documentation: <seealso href="https://docs.microsoft.com/en-us/windows/console/console-selection-info-str">CONSOLE_SELECTION_INFO structure</seealso>
        /// </para>
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct CONSOLE_SELECTION_INFO
        {
            /// <summary>
            /// The selection indicator.
            /// </summary>
            public uint Flags;
            /// <summary>
            /// A <see cref="COORD"/> structure that specifies the selection anchor, in characters.
            /// </summary>
            public COORD SelectionAnchor;
            /// <summary>
            /// A <see cref="SMALL_RECT"/> structure that specifies the selection rectangle.
            /// </summary>
            public SMALL_RECT Selection;

            /// <summary>
            /// Mouse is down. The user is actively adjusting the selection rectangle with a mouse.
            /// </summary>
            public const uint CONSOLE_MOUSE_DOWN = 0x0008;
            /// <summary>
            /// Selecting with the mouse. If off, the user is operating conhost.exe mark mode selection with the keyboard.
            /// </summary>
            public const uint CONSOLE_MOUSE_SELECTION = 0x0004;
            /// <summary>
            /// No selection.
            /// </summary>
            public const uint CONSOLE_NO_SELECTION = 0x0000;
            /// <summary>
            /// Selection has begun. If a mouse selection, this will typically not occur without the <see cref="CONSOLE_SELECTION_NOT_EMPTY"/> flag. If a keyboard selection, this may occur when mark mode has been entered but the user is still navigating to the initial position.
            /// </summary>
            public const uint CONSOLE_SELECTION_IN_PROGRESS = 0x0001;
            /// <summary>
            /// Selection rectangle not empty. The payload of <see cref="SelectionAnchor"/> and <see cref="Selection"/> are valid.
            /// </summary>
            public const uint CONSOLE_SELECTION_NOT_EMPTY = 0x0002;
        }

        /// <summary>
        /// <para>
        /// Defines the coordinates of the upper left and lower right corners of a rectangle.
        /// </para>
        /// <para>
        /// Microsoft Documentation: <seealso href="https://docs.microsoft.com/en-us/windows/console/small-rect-str">SMALL_RECT structure</seealso>
        /// </para>
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct SMALL_RECT
        {
            /// <summary>
            /// The x-coordinate of the upper left corner of the rectangle.
            /// </summary>
            public short left;
            /// <summary>
            /// The y-coordinate of the upper left corner of the rectangle.
            /// </summary>
            public short top;
            /// <summary>
            /// The x-coordinate of the lower right corner of the rectangle.
            /// </summary>
            public short right;
            /// <summary>
            /// The y-coordinate of the lower right corner of the rectangle.
            /// </summary>
            public short botton;
        }

        #endregion

        #region Command Prompt handling

        /// <summary>
        /// <para>
        /// Parses a Unicode command line string and returns an array of pointers to the command line arguments, along with a count of such arguments, in a way that is similar to the standard C run-time argv and argc values.
        /// </para>
        /// <para>
        /// Microsoft Documentation: <seealso href="https://docs.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-commandlinetoargvw">CommandLineToArgvW function</seealso>
        /// </para>
        /// </summary>
        /// <param name="lpCmdLine">Pointer to a null-terminated Unicode string that contains the full command line. If this parameter is an empty string the function returns the path to the current executable file.</param>
        /// <param name="pNumArgs">Pointer to an int that receives the number of array elements returned, similar to argc.</param>
        /// <returns>A pointer to an array of LPWSTR values, similar to argv. If the function fails, the return value is NULL.To get extended error information, call <see cref="GetLastError">GetLastError</see>.</returns>
        [DllImport("shell32.dll", SetLastError = true)]
        internal static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

        /// <summary>
        /// <para>
        /// Frees the specified local memory object and invalidates its handle.
        /// </para>
        /// <para>
        /// Microsoft Documentation: <seealso href="https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-localfree">LocalFree function</seealso>
        /// </para>
        /// </summary>
        /// <param name="hMem">A handle to the local memory object. This handle is returned by either the LocalAlloc or LocalReAlloc function. It is not safe to free memory allocated with GlobalAlloc.</param>
        /// <returns>If the function succeeds, the return value is NULL. If the function fails, the return value is equal to a handle to the local memory object. To get extended error information, call <see cref="GetLastError">GetLastError</see>.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr LocalFree(IntPtr hMem);

        #endregion

    }
}
