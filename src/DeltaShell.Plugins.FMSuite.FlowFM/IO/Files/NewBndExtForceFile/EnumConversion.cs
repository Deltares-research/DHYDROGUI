using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Reflection;
using Deltares.Infrastructure.Extensions;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile
{
    /// <summary>
    /// Class to help with enum conversions.
    /// </summary>
    public static class EnumConversion
    {
        /// <summary>
        /// Gets the enum value with the specified description from the enum type.
        /// </summary>
        /// <param name="description"> The enum value description. </param>
        /// <param name="enumValue">
        /// When this method returns, the enum value associated with the specified description, if found;
        /// otherwise, the default value for <typeparamref name="TEnum"/>
        /// </param>
        /// <typeparam name="TEnum"> The enum type. </typeparam>
        /// <returns>If successful, <c>true</c>; otherwise, <c>false</c>. </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when more than one enum values have the specified description.
        /// </exception>
        public static bool TryGetFromDescription<TEnum>(string description, out TEnum enumValue) where TEnum : Enum
        {
            IEnumerable<TEnum> enumValues = Enum.GetValues(typeof(TEnum)).OfType<TEnum>();
            IEnumerable<TEnum> enumValuesWithDescription = enumValues.Where(v => HasDescription(v, description)).ToArray();

            if (enumValuesWithDescription.Any())
            {
                enumValue = enumValuesWithDescription.Single();
                return true;
            }

            enumValue = default;
            return false;
        }

        private static bool HasDescription<T>(T enumValue, string description)
        {
            return description.EqualsCaseInsensitive((enumValue as Enum).GetDescription());
        }
    }
}