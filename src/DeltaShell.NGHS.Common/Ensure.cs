using System;
using System.ComponentModel;

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

        /// <summary>
        /// Determines whether the specified value is a defined <see cref="Enum"/>;
        /// if it is not, then an <see cref="InvalidEnumArgumentException"/> is thrown.
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="value">The value to check.</param>
        /// <param name="name">The name of <paramref name="value"/>.</param>
        /// <exception cref="InvalidEnumArgumentException">
        /// Thrown when <paramref name="value"/> is not a definedn <see cref="Enum"/>
        /// </exception>
        public static void IsDefined<T>(T value, string name)
        {
            if (!Enum.IsDefined(typeof(T), value))
            {
                throw new InvalidEnumArgumentException(name);
            }
        }
    }
}