using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.IO;
using GeoAPI.Extensions.Feature;

namespace DelftTools.Hydro
{
    /// <summary>
    /// Provides <see cref="IGroupableFeature"/> extension methods.
    /// </summary>
    public static class GroupableFeatureExtensions
    {
        /// <summary>
        /// Makes the group name of the <see cref="IGroupableFeature"/> relative to the specified base directory.
        /// If the path goes beyond the root directory, the group name is changed to just the file name.
        /// </summary>
        /// <param name="groupableFeature">The <see cref="IGroupableFeature"/> object whose group name is to be made relative.</param>
        /// <param name="rootDirectory">The root directory to which the group name should be constrained.</param>
        /// <param name="baseDirectory">The base directory used to calculate the relative path if the group name is absolute.</param>
        public static void MakeGroupNameRelative(this IGroupableFeature groupableFeature, string rootDirectory, string baseDirectory)
        {
            string groupName = groupableFeature.GroupName;

            if (string.IsNullOrEmpty(groupName))
            {
                return;
            }
            
            if (!FileUtils.PathIsRelative(groupName))
            {
                groupName = FileUtils.GetRelativePath(baseDirectory, groupName);
            }

            string normalizedRoot = Path.GetFullPath(rootDirectory);
            string fullGroupPath = Path.GetFullPath(Path.Combine(baseDirectory, groupName));

            groupableFeature.GroupName = fullGroupPath.StartsWith(normalizedRoot) ? groupName : Path.GetFileName(groupName);
        }
        
        public static void TrySetGroupName(this IFeature feature, string filePath)
        {
            if (!(feature is IGroupableFeature groupableFeature))
            {
                return;
            }

            groupableFeature.GroupName = filePath;
        }
        
        public static bool HasDefaultGroupName(this IGroupableFeature feature, string featureExtension, string defaultGroupName)
        {
            var featureGroupName = feature.GroupName;
            return string.IsNullOrEmpty(featureGroupName) || featureGroupName.Replace(featureExtension, string.Empty).Equals(defaultGroupName);
        }
    }

    /// <summary>
    /// Provides <see cref="IGroupableFeature"/> collection extension methods.
    /// </summary>
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