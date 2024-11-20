using System.IO;

namespace DeltaShell.NGHS.Common.IO.LogFileReading
{
    /// <summary>
    /// Contract defining method to read <see cref="Stream"/> into a <see cref="string"/>.
    /// </summary>
    public interface ILogFileReader
    {
        /// <summary>
        /// Read the data from the stream and return it as a <see cref="string"/>. 
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> from which to read the data. </param>
        /// <returns>A <see cref="string"/> representation if the data read from the <paramref name="stream"/>. </returns>
        string ReadCompleteStream(Stream stream);
    }
}