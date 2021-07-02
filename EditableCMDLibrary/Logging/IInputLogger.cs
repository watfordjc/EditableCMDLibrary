namespace uk.JohnCook.dotnet.EditableCMDLibrary.Logging
{
    /// <summary>
    /// An interface for writing log-like data.
    /// </summary>
    public interface IInputLogger
    {
        /// <summary>
        /// The description of the log.
        /// </summary>
        public string LogDescription { get; }
        /// <summary>
        /// Whether the log can be written to.
        /// </summary>
        public bool IsWriteable { get; }

        /// <summary>
        /// Writes a string to the logger.
        /// </summary>
        /// <param name="s">The string to write.</param>
        /// <param name="append">True to append to the logger. False to truncate the logger if it exists and replace its content with the string.</param>
        /// <returns>True on success.</returns>
        public abstract bool Log(string s, bool append = true);

        /// <summary>
        /// Writes a formatted string to the logger, using <see cref="string.Format(string, object[])"/>.
        /// </summary>
        /// <param name="format">The format string to pass to <see cref="string.Format(string, object?)"/>.</param>
        /// <param name="args">The arguments to pass to <see cref="string.Format(string, object?)"/>.</param>
        /// <param name="append">True to append to the logger. False to truncate the logger if it exists and replace its content with the formmated string.</param>
        /// <returns>True on success.</returns>
        public abstract bool FormattedLog(string format, object[] args, bool append = true);
    }
}
