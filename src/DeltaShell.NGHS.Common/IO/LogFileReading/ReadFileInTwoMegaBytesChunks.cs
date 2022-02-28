using System.IO;
using System.Text;
using DelftTools.Utils.Guards;

namespace DeltaShell.NGHS.Common.IO.LogFileReading
{
    /// <summary>
    /// Implements the <see cref="ILogFileReader"/>. It reads the logfile in chunks of 2 MB.
    /// </summary>
    public class ReadFileInTwoMegaBytesChunks : ILogFileReader
    {
        /// <inheritdoc/>
        public string ReadCompleteStream(Stream stream)
        {
            Ensure.NotNull(stream, nameof(stream));

            var stringBuilder = new StringBuilder();

            // Read data from file until read position is not equals to length of file
            while (stream.Position != stream.Length)
            {
                // Read number of remaining bytes to read
                long lRemainingBytes = stream.Length - stream.Position;

                // If bytes to read greater than 2 mega bytes size create array of 2 mega bytes
                // Else create array of remaining bytes
                byte[] arrData = lRemainingBytes > 262144 ? new byte[262144] : new byte[lRemainingBytes];

                // Read data from file
                stream.Read(arrData, 0, arrData.Length);

                stringBuilder.Append(Encoding.UTF8.GetString(arrData, 0, arrData.Length));
            }

            return stringBuilder.ToString();
        }
    }
}