using System;

namespace DHYDRO.Common.Guards
{
    /// <summary>
    /// Static class that holds various methods for input parameter validation
    /// </summary>
    internal static class Ensure
    {
        /// <summary>
        /// Verifies that the specified <paramref name="obj"/> is not <c>null</c>. If it is, then an
        /// <see cref="ArgumentNullException"/>
        /// is thrown.
        /// </summary>
        /// <typeparam name="T"> The type of the object. </typeparam>
        /// <param name="obj"> The object to check. </param>
        /// <param name="paramName"> The name of <paramref name="obj"/>. </param>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="obj"/> is <c>null</c>. </exception>
        public static void NotNull<T>(T obj, string paramName) where T : class
        {
            if (obj == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}