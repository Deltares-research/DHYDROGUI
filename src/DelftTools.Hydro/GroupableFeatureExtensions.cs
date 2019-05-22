using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.IO;

namespace DelftTools.Hydro
{
    public static class GroupableFeatureExtensions
    {
        public static void MakeGroupNameRelative(this IGroupableFeature groupableFeature, string mduFilePath)
        {
            if (groupableFeature == null)
            {
                return;
            }

            string directory = Path.GetDirectoryName(mduFilePath);
            string originalGroupName = groupableFeature.GroupName;

            string relativePathToFile = FileUtils.GetRelativePath(directory, originalGroupName);
            if (string.IsNullOrEmpty(relativePathToFile))
            {
                return;
            }

            groupableFeature.GroupName = GetNewGroupName(relativePathToFile, directory, originalGroupName);
        }

        public static string GetNewGroupName(string relativePathToFile, string directory, string originalGroupName)
        {
            bool isInSubDirectory = !relativePathToFile.Contains("..") &&
                                    (!Path.IsPathRooted(originalGroupName) || Path.GetPathRoot(directory) ==
                                     Path.GetPathRoot(originalGroupName));

            return isInSubDirectory ? relativePathToFile : Path.GetFileName(relativePathToFile);
        }
    }

    public static class GroupableFeaturesExtensions
    {
        public static void RemoveUngroupedItems<TFeature>(this IList<TFeature> featureList)
        {
            List<TFeature> itemsToRemove = featureList.OfType<IGroupableFeature>()
                                                      .Where(g => string.IsNullOrWhiteSpace(g.GroupName))
                                                      .OfType<TFeature>()
                                                      .ToList();

            itemsToRemove.ForEach(f => featureList.Remove(f));
        }

        public static void RemoveGroup<TFeature>(this IList<TFeature> eventedList, string group)
        {
            List<TFeature> itemsToRemove = eventedList.OfType<IGroupableFeature>()
                                                      .Where(g => g.GroupName == group)
                                                      .OfType<TFeature>()
                                                      .ToList();

            itemsToRemove.ForEach(f => eventedList.Remove(f));
        }
    }
}