using System.Collections.Generic;
using System.IO;
using System.Linq;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Utils.Extensions;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    /// <summary>
    /// Class that contains file paths and provides methods to filter through them.
    /// </summary>
    public class FilePaths
    {
        private readonly HashSet<FileInfo> fileInfos = new HashSet<FileInfo>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePaths"/> class.
        /// </summary>
        /// <param name="filePaths"> The file paths. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="filePaths"/> is <c>null</c>.
        /// </exception>
        public FilePaths(IEnumerable<string> filePaths)
        {
            Ensure.NotNull(filePaths, nameof(filePaths));

            foreach (string filePath in filePaths.Distinct())
            {
                var fileInfo = new FileInfo(filePath);
                fileInfos.Add(fileInfo);
            }
        }

        /// <summary>
        /// Gets the file info with one of the specified <paramref name="extensions"/>.
        /// </summary>
        /// <param name="extensions"> The extensions to search for. </param>
        /// <returns>
        /// If found, the file info with one of the specified <paramref name="extensions"/>; otherwise <c>null</c>.
        /// </returns>
        /// <remarks>
        /// The string comparison is case insensitive.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="extensions"/> or one of its values is <c>null</c>.
        /// </exception>
        public FileInfo GetByExtensions(params string[] extensions)
        {
            Ensure.NotNull(extensions, nameof(extensions));
            return extensions.Select(GetByExtension).FirstOrDefault(fileInfo => fileInfo != null);
        }

        /// <summary>
        /// Gets the file info with the specified <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName"> The file name to search for.  </param>
        /// <returns>
        /// If found, the file info with the specified <paramref name="fileName"/>; otherwise <c>null</c>.
        /// </returns>
        /// <remarks>
        /// The string comparison is case insensitive.
        /// </remarks>
        public FileInfo GetByName(string fileName)
        {
            Ensure.NotNull(fileName, nameof(fileName));
            return fileInfos.FirstOrDefault(f => f.Name.EqualsCaseInsensitive(fileName));
        }

        /// <summary>
        /// Gets the file info with the specified <paramref name="fileName"/> without extension.
        /// </summary>
        /// <param name="fileName"> The file name without extension to search for.  </param>
        /// <returns>
        /// If found, the file info with the specified <paramref name="fileName"/>; otherwise <c>null</c>.
        /// </returns>
        /// <remarks>
        /// The string comparison is case insensitive.
        /// </remarks>
        public FileInfo GetByNameWithoutExtension(string fileName)
        {
            Ensure.NotNull(fileName, nameof(fileName));
            return fileInfos.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f.Name).EqualsCaseInsensitive(fileName));
        }

        /// <summary>
        /// Gets the file info with the specified <paramref name="extension"/>.
        /// </summary>
        /// <param name="extension"> The extension to search for. </param>
        /// <returns>
        /// If found, the file info with the specified <paramref name="extension"/>; otherwise <c>null</c>.
        /// </returns>
        /// <remarks>
        /// The string comparison is case insensitive.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="extension"/> is <c>null</c>.
        /// </exception>
        private FileInfo GetByExtension(string extension)
        {
            Ensure.NotNull(extension, nameof(extension));
            return fileInfos.FirstOrDefault(f => f.Extension.EqualsCaseInsensitive(extension));
        }
    }
}