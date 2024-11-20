using System;
using System.Collections.Generic;
using System.IO;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter
{
    /// <summary>
    /// Interface to retrieve manifest data.
    /// </summary>
    public interface IManifestRetriever
    {
        /// <summary>
        /// Gets all the fixed file resources.
        /// </summary>
        IEnumerable<string> FixedResources { get; }

        /// <summary>
        /// Stream to read the fixed manifest data of <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName">File from the fixed manifest which is to be read.</param>
        /// <returns>
        /// <see cref="Stream"/> of <paramref name="fileName"/> or
        /// null if no resources were specified during compilation or if the resource is not visible to the caller.
        /// </returns>
        /// <exception cref="ArgumentNullException">The name parameter is null.</exception>
        /// <exception cref="ArgumentException">The name parameter is an empty string ("")</exception>
        /// <exception cref="FileLoadException">A file that was found could not be loaded.</exception>
        /// <exception cref="FileNotFoundException">name was not found.</exception>
        /// <exception cref="BadImageFormatException">name is not a valid assembly.</exception>
        /// <exception cref="NotImplementedException">Resource length is greater than MaxValue.</exception>
        Stream GetFixedStream(string fileName);
    }
}