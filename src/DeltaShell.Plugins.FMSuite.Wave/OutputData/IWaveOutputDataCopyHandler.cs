using System.IO;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.Wave.OutputData
{
    /// <summary>
    /// <see cref="IWaveOutputDataCopyHandler"/> defines the interface used to
    /// copy output data to a new location with.
    /// </summary>
    public interface IWaveOutputDataCopyHandler
    {
        /// <summary>
        /// Copies the run data from the <see cref="sourceDirectoryInfo"/> to
        /// the <see cref="targetDirectoryInfo"/>.
        /// </summary>
        /// <param name="sourceDirectoryInfo">The source directory information.</param>
        /// <param name="targetDirectoryInfo">The target directory information.</param>
        /// <param name="logHandler">An optional log handler.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when the <paramref name="sourceDirectoryInfo"/> or the
        /// <paramref name="targetDirectoryInfo"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// The copy run data assumes a Waves working directory file structure,
        /// as such it will not work when provided with a regular output folder.
        /// </remarks>
        void CopyRunDataTo(DirectoryInfo sourceDirectoryInfo, 
                           DirectoryInfo targetDirectoryInfo,
                           ILogHandler logHandler = null);

        /// <summary>
        /// Copies the output data  from the <see cref="sourceDirectoryInfo"/>
        /// to the <see cref="targetDirectoryInfo"/>.
        /// </summary>
        /// <param name="sourceDirectoryInfo">The source directory information.</param>
        /// <param name="targetDirectoryInfo">The target directory information.</param>
        /// <param name="logHandler">An optional log handler.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="sourceDirectoryInfo"/> or
        /// <paramref name="targetDirectoryInfo"/> is <c>null</c>.
        /// </exception>
        void CopyOutputDataTo(DirectoryInfo sourceDirectoryInfo, 
                              DirectoryInfo targetDirectoryInfo, 
                              ILogHandler logHandler = null);
    }
}