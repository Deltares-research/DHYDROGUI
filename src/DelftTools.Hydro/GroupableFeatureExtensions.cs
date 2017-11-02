using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.IO;

namespace DelftTools.Hydro
{
    public static class GroupableFeatureExtensions
    {
        public static void MakeGroupNameRelative(this IGroupableFeature groupableFeature, string relativePath)
        {
            if (groupableFeature == null) return;

            var directory = Path.GetDirectoryName(relativePath);
            var groupName = groupableFeature.GroupName;

            var relativePathToFile = FileUtils.GetRelativePath(directory, groupName);
            if (string.IsNullOrEmpty(relativePathToFile)) return;

            var isRelativeToPath = !relativePathToFile.Contains("..") &&
                                   (!Path.IsPathRooted(groupName) ||
                                    Path.GetPathRoot(directory) == Path.GetPathRoot(groupName));

            groupableFeature.GroupName = !isRelativeToPath
                    ? Path.GetFileName(relativePathToFile)
                    : relativePathToFile;
        }
    }

    public static class GroupableFeaturesExtensions
    {
        public static void RemoveUngroupedItems<TFeature>(this IList<TFeature> featureList)
        {
            var itemsToRemove = featureList.OfType<IGroupableFeature>()
                .Where(g => string.IsNullOrWhiteSpace(g.GroupName))
                .OfType<TFeature>()
                .ToList();

            itemsToRemove.ForEach(f => featureList.Remove(f));
        }

        public static void RemoveGroup<TFeature>(this IList<TFeature> eventedList, string group)
        {
            var itemsToRemove = eventedList.OfType<IGroupableFeature>()
                .Where(g => g.GroupName == group)
                .OfType<TFeature>()
                .ToList();

            itemsToRemove.ForEach(f => eventedList.Remove(f));
        }
    }
}