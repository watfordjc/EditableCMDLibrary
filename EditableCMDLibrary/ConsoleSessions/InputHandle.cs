using uk.JohnCook.dotnet.EditableCMDLibrary.Interop;
using static uk.JohnCook.dotnet.EditableCMDLibrary.Interop.NativeMethods;

namespace uk.JohnCook.dotnet.EditableCMDLibrary.ConsoleSessions
{
    /// <summary>
    /// A <see cref="StandardHandle"/> that has methods useful for a standard input stream.
    /// </summary>
    public class InputHandle : StandardHandle
    {
        /// <summary>
        /// Wrapper for a standard input stream.
        /// </summary>
        /// <param name="nStdHandle">A standard stream identifier, such as <see cref="NativeMethods.STD_INPUT_HANDLE"/>.</param>
        public InputHandle(int nStdHandle) : base(nStdHandle)
        {

        }

        /// <summary>
        /// Reads the next input record from the standard input stream.
        /// </summary>
        /// <param name="record">An array of <see cref="INPUT_RECORD"/> to store the read record.</param>
        /// <returns>The number of records read.</returns>
        public uint ReadNextInputRecord(ref INPUT_RECORD[] record)
        {
            record[0] = new INPUT_RECORD();
            ReadConsoleInput(Handle, record, 1, out uint recordsRead);
            return recordsRead;
        }
    }
}
