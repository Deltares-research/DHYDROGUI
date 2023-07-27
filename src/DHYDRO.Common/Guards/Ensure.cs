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
        
        /// <summary>
        /// Ensure that the <paramref name="value"/> is not negative. If it is,
        /// then an <see cref="ArgumentOutOfRangeException"/> is thrown.
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <param name="paramName">The actual name of the parameter</param>
        /// <param name="message">Optional message</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the <paramref name="value"/> is negative.
        /// </exception>
        public static void NotNegative(double value, string paramName, string message = null)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(paramName, message);
            }
        }
    }
}