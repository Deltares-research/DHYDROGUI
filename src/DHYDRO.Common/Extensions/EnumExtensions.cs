using System;
using System.ComponentModel;
using System.Reflection;
using DHYDRO.Common.Guards;

namespace DHYDRO.Common.Extensions
{
    /// <summary>
    /// Provides extension methods for enum types.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Retrieves the description associated with the enum value from the <see cref="DescriptionAttribute"/>.
        /// </summary>
        /// <param name="value">The enum value for which to retrieve the description.</param>
        /// <typeparam name="T">The type of the enum value.</typeparam>
        /// <returns>The description of the enum value, or the enum value as a string if no description attribute is found.</returns>
        /// <exception cref="InvalidEnumArgumentException">Thrown when <paramref name="value"/> is not a valid enum value.</exception>
        public static string GetDescription<T>(this T value) where T : Enum
        {
            Ensure.IsDefined(value, nameof(value));

            return GetEnumAttribute<DescriptionAttribute>(value)?.Description ?? value.ToString();
        }

        private static T GetEnumAttribute<T>(Enum value) where T : Attribute
        {
            return (T)Attribute.GetCustomAttribute(GetEnumField(value), typeof(T));
        }

        private static FieldInfo GetEnumField(Enum value)
        {
            return value.GetType().GetField(value.ToString());
        }
    }
}