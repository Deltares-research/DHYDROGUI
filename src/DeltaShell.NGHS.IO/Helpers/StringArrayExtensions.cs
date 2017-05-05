using System;
using System.Linq;

namespace DeltaShell.NGHS.IO.Helpers
{
    public static class StringArrayExtensions
    {
        public static string[] ReplaceSpacesInString(this string[] names, char desiredReplacement = '_')
        {
            for (int i = 0; i < names.Length; ++i)
            {
                if (names[i] == null)
                {
                    continue;
                }

                var newName = names[i].Replace(' ', desiredReplacement);
                if (newName != names[i] && names.Contains(newName))
                {
                    throw new ArgumentException(string.Format("Tried to replace a space in the name with '{1}', but an item with name '{0}' is already present", newName, desiredReplacement));
                }
                names[i] = newName;
            }
            return names;
        }
    }
}