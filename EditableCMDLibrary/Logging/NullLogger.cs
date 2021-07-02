namespace uk.JohnCook.dotnet.EditableCMDLibrary.Logging
{
    /// <summary>
    /// A class for writing log-like data to nowhere.
    /// </summary>
    public class NullLogger : IInputLogger
    {
        /// <summary>
        /// The description of the log, used for printing errors during debugging.
        /// </summary>
        public string LogDescription { get; set; }
        /// <summary>
        /// Whether the log can be written to.
        /// </summary>
        public bool IsWriteable
        {
            get { return true; }
        }

        /// <summary>
        /// An instance of a null logger.
        /// </summary>
        /// <param name="description">A description of the log, used for printing errors during debugging.</param>
        public NullLogger(string description)
        {
            LogDescription = description;
        }

        /// <summary>
        /// Pretends to write a formatted string to the logger using <see cref="string.Format(string, object[])"/>.
        /// </summary>
        /// <param name="format">The format string to pass to <see cref="string.Format(string, object?)"/>.</param>
        /// <param name="args">The arguments to pass to <see cref="string.Format(string, object?)"/>.</param>
        /// <param name="append">True to append to the log. False to truncate the log if it exists and replace its content with the formmated string.</param>
        /// <returns>True on success.</returns>
        public bool FormattedLog(string format, object[] args, bool append = true)
        {
            return true;
        }

        /// <summary>
        /// Pretends to write a string to the logger.
        /// </summary>
        /// <param name="s">The string to write.</param>
        /// <param name="append">True to append to the log. False to truncate the log if it exists and replace its content with the formmated string.</param>
        /// <returns>True on success.</returns>
        public bool Log(string s, bool append = true)
        {
            return true;
        }
    }
}
