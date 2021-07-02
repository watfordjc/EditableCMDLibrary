using System.IO;
using System.Threading;

namespace uk.JohnCook.dotnet.EditableCMDLibrary.Utils
{
    /// <summary>
    /// Helper methods for files
    /// </summary>
    public static class FileUtils
    {
        /// <summary>
        /// Waits for a file to be closed, or no longer locked.
        /// </summary>
        /// <param name="file">The file to wait for.</param>
        /// <param name="timeout">A timeout in milliseconds. If the file is still locked after this long, the method returns.</param>
        /// <returns>True if the file is no longer locked. False if it is still locked.</returns>
        public static bool WaitForFile(FileInfo file, uint timeout)
        {
            bool cancel = false;
            bool done = false;
            FileStream? fs = null;
            System.Timers.Timer timer = new();
            if (timeout > 0)
            {
                timer.Elapsed += OnTimedEvent;
                timer.Start();
            }
            void OnTimedEvent(object sender, System.Timers.ElapsedEventArgs e)
            {
                timer.Elapsed -= OnTimedEvent;
                cancel = true;
            }
            while (!done && !cancel)
            {
                try
                {
                    fs = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    timer.Stop();
                    timer.Elapsed -= OnTimedEvent;
                }
                catch (IOException)
                {
                    Thread.Sleep(50);
                    continue;
                }
                finally
                {
                    if (fs != null)
                    {
                        fs.Close();
                        done = true;
                    }
                }
            }
            return !cancel;
        }
    }
}
