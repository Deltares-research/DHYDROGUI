namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO
{
    /// <summary>
    /// Represents a reader for Real-Time Control (RTC) model XML data.
    /// </summary>
    public interface IRealTimeControlXmlReader
    {
        /// <summary>
        /// Reads the Real-Time Control (RTC) model XML data from the specified directory and populates the provided model.
        /// </summary>
        /// <param name="model">The RTC model object to populate with the data read from XML.</param>
        /// <param name="directory">The directory path from which to read the XML file.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="model"/> is <c>null</c>.</exception>
        /// <exception cref="System.ArgumentException">Thrown when <paramref name="directory"/> is <c>null</c> or empty.</exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">Thrown when <paramref name="directory"/> does not exist.</exception>
        void ReadFromXml(RealTimeControlModel model, string directory);
    }
}