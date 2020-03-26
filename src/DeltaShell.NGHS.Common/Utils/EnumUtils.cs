using System;
using System.Linq;
using DelftTools.Utils.Reflection;

namespace DeltaShell.NGHS.Common.Utils
{
    /// <summary>
    /// Provides helper methods for enums.
    /// </summary>
    public static class EnumUtils
    {
        /// <summary>
        /// Gets the enum value with the specified <paramref name="description" />.
        /// </summary>
        /// <typeparam name="T"> The enum type. </typeparam>
        /// <param name="description"> The description. </param>
        /// <returns>
        /// If found, the enum value with the specified <paramref name="description" />;
        /// otherwise, the <c> default </c> of <typeparam name="T" />
        /// </returns>
        public static T GetEnumValueByDescription<T>(string description)
        {
            return Enum.GetValues(typeof(T)).OfType<T>()
                       .FirstOrDefault(v => (v as Enum).GetDescription() == description);
        }
    }
}