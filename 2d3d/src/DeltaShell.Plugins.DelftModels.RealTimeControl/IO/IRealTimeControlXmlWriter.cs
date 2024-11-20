namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO
{
    /// <summary>
    /// Represents a writer for Real-Time Control (RTC) model XML data.
    /// </summary>
    public interface IRealTimeControlXmlWriter
    {
        /// <summary>
        /// Writes the Real-Time Control (RTC) model XML data to the specified directory.
        /// </summary>
        /// <param name="model">The RTC model to be written.</param>
        /// <param name="directory">The directory path where the XML file will be saved. It must exist.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="model"/> is <c>null</c>.</exception>
        /// <exception cref="System.ArgumentException">Thrown when <paramref name="directory"/> is <c>null</c> or empty.</exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">Thrown when <paramref name="directory"/> does not exist.</exception>
        void WriteToXml(RealTimeControlModel model, string directory);
    }
}