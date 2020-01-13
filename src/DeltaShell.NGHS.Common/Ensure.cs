using System;

namespace DeltaShell.NGHS.Common
{
    /// <summary>
    /// <see cref="Ensure"/> provides a collection of methods to verify different
    /// conditions on parameters.
    /// </summary>
    public static class Ensure
    {
        /// <summary>
        /// Verifies that the specified <paramref name="obj"/> is not
        /// <c>null</c>. If it is not, then an <see cref="ArgumentNullException"/>
        /// is thrown.
        /// </summary>
        /// <typeparam name="T"> The type of the object. </typeparam>
        /// <param name="obj"> The object to check. </param>
        /// <param name="paramName"> The name of <paramref name="obj"/>. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="obj"/> is <c>null</c>.
        /// </exception>
        public static void NotNull<T>(T obj, string paramName) where T : class
        {
            if (obj == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}