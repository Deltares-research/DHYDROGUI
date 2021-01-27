using System.Text.RegularExpressions;
using DelftTools.Utils;

namespace DelftTools.Hydro.GroupableFeatures
{
    public static class GroupableFeatureHelper
    {
        public static string SetGroupableFeatureGroupName(string value)
        {
            if (value == null)
            {
                return null;
            }

            string groupName = value.Replace(@"\", "/").TrimStart('/');
            Match match = Regex.Match(groupName, @"/{2,}");
            while (match.Success)
            {
                groupName = groupName.ReplaceFirst(match.Value, "/");
                match = Regex.Match(groupName, @"/{2,}");
            }

            return groupName;
        }

        public static T CloneGroupableFeature<T>(this T groupableFeature, T instance) where T : IGroupableFeature
        {
            instance.GroupName = groupableFeature.GroupName;
            instance.IsDefaultGroup = groupableFeature.IsDefaultGroup;
            return instance;
        }
    }
}