using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Reflection;
using DHYDRO.Common.Extensions;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers.EnumOperations
{
    /// <summary>
    /// Class that parses a string to an Enum value.
    /// </summary>
    internal sealed class EnumParser
    {
        /// <summary>
        /// Parse the given description to an <see cref="TEnum"/> value with this description (comparison is case-insensitive).
        /// </summary>
        /// <param name="description"> The description to search for. </param>
        /// <param name="defaultValue"> The value to return if parsing fails. </param>
        /// <param name="convertedValue"> When returning, this parameter contains the converted enum value. </param>
        /// <typeparam name="TEnum">
        /// The type of enum. Enum should contain a None and Unsupported value:
        /// <paramref name="defaultValue"/> if <paramref name="description"/> is <c>null</c> or white space.
        /// A <see cref="TEnum"/> with the given description, if it exists; otherwise, <paramref name="defaultValue"/>
        /// </typeparam>
        /// <returns>
        /// <c>false</c> if the given description could not be parsed; otherwise, <c>true</c>.
        /// </returns>
        public bool TryParseByDescription<TEnum>(string description, TEnum defaultValue, out TEnum convertedValue) where TEnum : Enum
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                convertedValue = defaultValue;
                return true;
            }

            IEnumerable<TEnum> enumValues = Enum.GetValues(typeof(TEnum)).OfType<TEnum>();
            IEnumerable<TEnum> enumValuesWithDescription = enumValues.Where(v => HasDescription(v, description)).ToArray();

            if (enumValuesWithDescription.Any())
            {
                convertedValue = enumValuesWithDescription.Single();
                return true;
            }

            convertedValue = defaultValue;
            return false;
        }

        private static bool HasDescription<TEnum>(TEnum enumValue, string description) where TEnum : Enum
        {
            return description.EqualsCaseInsensitive(enumValue.GetDescription());
        }
    }
}