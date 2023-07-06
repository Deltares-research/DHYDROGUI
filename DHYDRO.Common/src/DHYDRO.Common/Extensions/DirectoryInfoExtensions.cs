using System.IO;

namespace DHYDRO.Common.Extensions
{
    /// <summary>
    /// Contains extensions methods for <see cref="DirectoryInfo"/>.
    /// </summary>
    public static class DirectoryInfoExtensions
    {
        /// <summary>
        /// Determines whether to instances of <see cref="DirectoryInfo"/> point to the same directory.
        /// </summary>
        /// <param name="directory1"> The first directory info. </param>
        /// <param name="directory2"> The second directory info. </param>
        /// <returns>
        /// A boolean indicating whether the two instances of <see cref="DirectoryInfo"/> point to the same directory or are both
        /// <c>null</c>.
        /// </returns>
        /// <remarks>
        /// This method is only guaranteed to work for absolute paths.
        /// </remarks>
        public static bool EqualsDirectory(this DirectoryInfo directory1, DirectoryInfo directory2)
        {
            if (directory1 == null || directory2 == null)
            {
                return directory1 == directory2;
            }

            return directory1.FullName.EqualsCaseInsensitive(directory2.FullName);
        }
    }
}