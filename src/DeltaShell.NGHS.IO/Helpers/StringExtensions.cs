using System;
using System.Linq;
using DelftTools.Utils.Reflection;

namespace DeltaShell.NGHS.IO.Helpers
{
    public static class StringExtensions
    {
        /// <summary>
        ///     <remarks> This method is copied over from the framework where it was removed from. see issue D3DFMIQ-722 </remarks>
        /// </summary>
        /// <param name="displayNameString"> </param>
        /// <param name="enumType"> </param>
        /// <returns>
        /// Null if no match was found in <paramref name="enumType" /> with <paramref name="displayNameString" />; Value
        /// otherwise.
        /// </returns>
        public static object GetEnumValueFromDisplayName(this string displayNameString, Type enumType)
        {
            return Enum.GetValues(enumType)
                .Cast<Enum>()
                .FirstOrDefault(value => value.GetDisplayName() == displayNameString);
        }
    }
}