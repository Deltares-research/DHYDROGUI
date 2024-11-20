using System.IO;
using Deltares.Infrastructure.API.Logging;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.IniOperations
{
    /// <summary>
    /// <see cref="IIniFileOperator"/> defines the interface to execute
    /// a set of <see cref="IIniPropertyBehaviour"/> on a specified file.
    /// </summary>
    public interface IIniFileOperator
    {
        /// <summary>
        /// Invokes the operations of this <see cref="IIniFileOperator"/>
        /// on the specified <paramref name="sourceFileStream"/>.
        /// </summary>
        /// <param name="sourceFileStream">The source file.</param>
        /// <param name="sourceFilePath">The source file path to write to.</param>
        /// <param name="logHandler">The log handler.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="sourceFileStream"/>,
        /// <paramref name="sourceFilePath"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="sourceFileStream"/> does not support
        /// reading;
        /// </exception>
        void Invoke(Stream sourceFileStream, string sourceFilePath, ILogHandler logHandler);
    }
}