namespace uk.JohnCook.dotnet.EditableCMDLibrary.Logging
{
    /// <summary>
    /// A class for writing log-like data to nowhere.
    /// </summary>
    public class NullLogger : IInputLogger
    {
        /// <inheritdoc cref="IInputLogger.LogDescription"/>
        public string LogDescription { get; set; }
        /// <inheritdoc cref="IInputLogger.IsWriteable"/>
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
        /// <inheritdoc cref="IInputLogger.FormattedLog(string, object[], bool)" path="param|returns"/>
        public bool FormattedLog(string format, object[] args, bool append = true)
        {
            return true;
        }

        /// <summary>
        /// Pretends to write a string to the logger.
        /// </summary>
        /// <inheritdoc cref="IInputLogger.Log(string, bool)" path="param|returns"/>
        public bool Log(string s, bool append = true)
        {
            return true;
        }
    }
}
