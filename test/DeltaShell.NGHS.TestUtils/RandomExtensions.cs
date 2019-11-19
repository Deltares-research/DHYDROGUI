using System;

namespace DeltaShell.NGHS.TestUtils
{
    /// <summary>
    /// Extension methods for <see cref="Random"/>.
    /// </summary>
    public static class RandomExtensions
    {
        /// <summary>
        /// Returns a random value of <typeparamref name="TEnum"/>.
        /// </summary>
        /// <typeparam name="TEnum">The <see cref="Enum"/> to use.</typeparam>
        /// <param name="random">A pseudo-random number generator.</param>
        /// <returns>A new random value of type <typeparamref name="TEnum"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="random"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <typeparamref name="TEnum"/> is not an <see cref="Enum"/>.</exception>
        public static TEnum NextEnumValue<TEnum>(this Random random)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException($"'{typeof(TEnum).Name}' is not an enum.");
            }

            var enumValues = (TEnum[])Enum.GetValues(typeof(TEnum));
            return enumValues[random.Next(enumValues.Length)];
        }

        /// <summary>
        /// Returns a random boolean value.
        /// </summary>
        /// <param name="random">A pseudo-random number generator.</param>
        /// <returns>A new random boolean value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="random"/> is <c>null</c>.</exception>
        public static bool NextBoolean(this Random random)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            return Convert.ToBoolean(random.Next(0, 2));
        }
    }
}