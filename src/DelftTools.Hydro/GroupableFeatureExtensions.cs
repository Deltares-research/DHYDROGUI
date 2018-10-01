using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.IO;
using GeoAPI.Extensions.Feature;

namespace DelftTools.Hydro
{
    public static class GroupableFeatureExtensions
    {
        public static void MakeGroupNameRelative(this IGroupableFeature groupableFeature, string mduFilePath)
        {
            if (groupableFeature == null) return;

            var directory = Path.GetDirectoryName(mduFilePath);
            var originalGroupName = groupableFeature.GroupName;

            var relativePathToFile = FileUtils.GetRelativePath(directory, originalGroupName);
            if (String.IsNullOrEmpty(relativePathToFile)) return;
            groupableFeature.GroupName = GetNewGroupName(relativePathToFile, directory, originalGroupName);
        }

        public static string GetNewGroupName(string relativePathToFile, string directory, string originalGroupName)
        {
            var isInSubDirectory = !relativePathToFile.Contains("..") &&
                                   (!Path.IsPathRooted(originalGroupName) || Path.GetPathRoot(directory) == Path.GetPathRoot(originalGroupName));

            return isInSubDirectory ? relativePathToFile : Path.GetFileName(relativePathToFile);
        }

        public static void TrySetGroupName(this IFeature feature, string filePath)
        {
            var groupableFeature = feature as IGroupableFeature;
            if (groupableFeature == null) return;

            groupableFeature.GroupName = filePath;
        }

        public static bool HasDefaultGroupName(this IGroupableFeature feature, string featureExtension, string defaultGroupName)
        {
            var featureGroupName = feature.GroupName;
            return string.IsNullOrEmpty(featureGroupName) || featureGroupName.Replace(featureExtension, string.Empty).Equals(defaultGroupName);
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