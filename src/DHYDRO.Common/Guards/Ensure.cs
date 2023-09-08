using System;
using System.ComponentModel;
using DHYDRO.Common.Annotations;

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
        public static void NotNull<T>([NoEnumeration] T obj, string paramName) where T : class
        {
            if (obj == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Verifies that the specified <paramref name="obj"/> is not  <c>null</c>. If it is, then an
        /// <see cref="ArgumentNullException"/>
        /// is thrown.
        /// </summary>
        /// <typeparam name="T"> The type of the object. </typeparam>
        /// <param name="obj"> The object to check. </param>
        /// <param name="paramName"> The name of <paramref name="obj"/>. </param>
        /// <param name="message"> Optional message. </param>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="obj"/> is <c>null</c>. </exception>
        public static void NotNull<T>([NoEnumeration] T obj, string paramName, string message) where T : class
        {
            if (obj == null)
            {
                throw new ArgumentNullException(paramName, message);
            }
        }

        /// <summary>
        /// Verifies that the specified <paramref name="value"/> is not <c>null</c> or empty.
        /// If it is, then an <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <param name="value"> The string value to check. </param>
        /// <param name="paramName"> The name of <paramref name="value"/>. </param>
        /// <param name="message"> The optional message. Defaults to <c>null</c>. </param>
        /// <exception cref="ArgumentException"> Thrown when <paramref name="value"/> is <c>null</c> or empty. </exception>
        public static void NotNullOrEmpty(string value, string paramName, string message = null)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException(message, paramName);
            }
        }

        /// <summary>
        /// Verifies that the specified <paramref name="value"/> is not <c>null</c> or white space.
        /// If it is, then an <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <param name="value"> The string value to check. </param>
        /// <param name="paramName"> The name of <paramref name="value"/>. </param>
        /// <param name="message"> The optional message. Defaults to <c>null</c>. </param>
        /// <exception cref="ArgumentException"> Thrown when <paramref name="value"/> is <c>null</c> or white space. </exception>
        public static void NotNullOrWhiteSpace(string value, string paramName, string message = null)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(message, paramName);
            }
        }

        /// <summary>
        /// Ensure that the <paramref name="value"/> is not <see cref="double.NaN"/>. If it is <see cref="double.NaN"/>
        /// then an <see cref="ArgumentOutOfRangeException"/> is thrown.
        /// </summary>
        /// <param name="value"> The value to check. </param>
        /// <param name="paramName"> Actual name of the parameter. </param>
        /// <param name="message"> Optional message. </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// is thrown if the <paramref name="value"/> is <see cref="double.NaN"/>
        /// </exception>
        public static void NotNaN(double value, string paramName, string message = null)
        {
            if (double.IsNaN(value))
            {
                throw new ArgumentOutOfRangeException(paramName, message);
            }
        }

        /// <summary>
        /// Ensure that the <paramref name="value"/> is not <see cref="double.IsInfinity"/>. If it is
        /// <see cref="double.IsInfinity"/> then an <see cref="ArgumentOutOfRangeException"/> is thrown.
        /// </summary>
        /// <param name="value">the value to check</param>
        /// <param name="paramName">actual name of the parameter</param>
        /// <param name="message">Optional message</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// is thrown if the <paramref name="value"/> is
        /// <see cref="double.IsInfinity"/>
        /// </exception>
        public static void NotInfinity(double value, string paramName, string message = null)
        {
            if (double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(paramName, message);
            }
        }

        /// <summary>
        /// Ensure the specified <paramref name="value"/> is a defined <see cref="Enum"/>. If it is not, then an
        /// <see cref="InvalidEnumArgumentException"/> is thrown.
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="value">The value to check.</param>
        /// <param name="name">The name of <paramref name="value"/>.</param>
        /// <exception cref="InvalidEnumArgumentException">
        /// Thrown when <paramref name="value"/> is not a defined
        /// <see cref="Enum"/>
        /// </exception>
        public static void IsDefined<T>(T value, string name)
        {
            if (!Enum.IsDefined(typeof(T), value))
            {
                throw new InvalidEnumArgumentException(name);
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