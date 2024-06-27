using System;
using System.Collections.Generic;
using System.IO;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.NGHS.Common.Utils
{
    /// <summary>
    /// This class provides a unique file name based on the inputs.
    /// </summary>
    public sealed class UniqueFileNameProvider
    {
        private readonly IDictionary<string, int> cachedNames =
            new Dictionary<string, int>();

        /// <summary>
        /// Adds file names.
        /// </summary>
        /// <param name="fileNames"> The file names. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="fileNames"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <paramref name="fileNames"/> contains duplicate values.
        /// </exception>
        public void AddFiles(IEnumerable<string> fileNames)
        {
            Ensure.NotNull(fileNames, nameof(fileNames));

            foreach (string fileName in fileNames)
            {
                if (cachedNames.ContainsKey(fileName))
                {
                    throw new InvalidOperationException($"Cannot add a file that was already added: {fileName}");
                }

                cachedNames[fileName] = 0;
            }
        }

        /// <summary>
        /// Returns a unique file name based on the specified <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName"> The file name for which to generate a unique file name. </param>
        /// <returns>
        /// The <paramref name="fileName"/> if it is unique, otherwise
        /// the <paramref name="fileName"/> with an appended index.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="fileName"/> is <c>null</c> or empty.
        /// </exception>
        public string GetUniqueFileNameFor(string fileName)
        {
            Ensure.NotNullOrEmpty(fileName, nameof(fileName));

            if (!cachedNames.ContainsKey(fileName))
            {
                cachedNames[fileName] = 0;
                return fileName;
            }

            cachedNames[fileName]++;

            return Path.GetFileNameWithoutExtension(fileName) + $"_{cachedNames[fileName]}" + Path.GetExtension(fileName);
        }
    }
}