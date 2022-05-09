using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Editing;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using DeltaShell.Plugins.SharpMapGis.Properties;
using GeoAPI.Extensions.Feature;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class GroupablePointCloudImporter : PointCloudImporter<GroupablePointFeature>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GroupablePointCloudImporter));
        private const int GroupNameProgressTextInterval = 1000;

        public override bool CanImportOnRootLevel
        {
            get { return false; }
        }
        
        public Func<IList<GroupablePointFeature>, string> GetBaseFolder { get; set; }

        protected override object OnImportItem(string path, object target = null)
        {
            try
            {
                var pointFeatureList = target is IDataItem dataItem
                    ? dataItem.Value as IList<GroupablePointFeature>
                    : target as IList<GroupablePointFeature>;

                if (pointFeatureList == null) return null;

                object onImportItem;
                // If importing from DeltaShell GUI, GetBaseFolder is set. In that case we import with progress
                if (GetBaseFolder == null || GetBaseFolder(pointFeatureList) == string.Empty || GetRegion == null)
                {
                    onImportItem = base.OnImportItem(path, pointFeatureList);
                    pointFeatureList.ForEach(f => f.GroupName = path);
                    return onImportItem;
                }

                TotalNumberOfProgressSteps = 3;
                var importedFeatures = new List<GroupablePointFeature>();
                onImportItem = base.OnImportItem(path, importedFeatures);

                var region = GetRegion?.Invoke(pointFeatureList);
                var baseFolder = GetBaseFolder(pointFeatureList);
                var relativePathToFile = FileUtils.GetRelativePath(baseFolder, path);
                var groupName = GroupableFeatureExtensions.GetNewGroupName(relativePathToFile, baseFolder, path);

                AddNewFeaturesToListWithGroupName(region, pointFeatureList, importedFeatures, groupName);

                return onImportItem;
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }

            return null;
        }

        /// <summary>
        /// We rename the group names of all GroupablePointFeatures and add them to the original list of features.
        /// This is done in batches of the number that is defined by 'groupNameProgressTextInterval', as we execute this code on
        /// a different thread in method AddFeatures and we can only change the progress text (shown to the user) on the current thread.
        /// </summary>
        /// <param name="region">The region where oiginalfeatures come from. We disable event bubbling while adding features to the originalFeatures list.</param>
        /// <param name="originalFeatures">The feature list on which the newly imported features will be added.</param>
        /// <param name="newFeatures">The new features that will be added to originalFeatures list after renaming their group names.</param>
        /// <param name="groupName">The new group name.</param>
        private void AddNewFeaturesToListWithGroupName(IRegion region, IList<GroupablePointFeature> originalFeatures, List<GroupablePointFeature> newFeatures, string groupName)
        {
            region?.BeginEdit("Setting group names");
            var numOfFeaturesToAdd = newFeatures.Count;
            for (var startIndex = 0; startIndex < numOfFeaturesToAdd; startIndex += GroupNameProgressTextInterval)
            {
                var numOfFeaturesLeft = numOfFeaturesToAdd - startIndex;
                var listToAdd = numOfFeaturesLeft > GroupNameProgressTextInterval
                    ? newFeatures.GetRange(startIndex, GroupNameProgressTextInterval)
                    : newFeatures.GetRange(startIndex, numOfFeaturesLeft);
                AddFeatures(listToAdd, originalFeatures, groupName);
                UpdateProgress($"Setting group names {startIndex} / {newFeatures.Count}", 2);
            }

            UpdateProgress(string.Format(Resources.PointCloudImporter_OnImportItem_Finished_importing__0__point_features, numOfFeaturesToAdd), 3);
            region?.EndEdit();
        }

        /// <summary>
        /// Adds the features in newList to originalList, after changing the group name of the feature.
        /// </summary>
        /// <param name="newList">New list of features.</param>
        /// <param name="originalList">original list of features.</param>
        /// <param name="groupName">The new group name of the new features.</param>
        [InvokeRequired]
        private static void AddFeatures(IList<GroupablePointFeature> newList, IList<GroupablePointFeature> originalList, string groupName)
        {
            foreach (var groupablePointFeature in newList)
            {
                groupablePointFeature.GroupName = groupName;
                originalList.Add(groupablePointFeature);
            }
        }
    }
}