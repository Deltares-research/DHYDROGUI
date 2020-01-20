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
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    public class GroupablePointCloudImporter : PointCloudImporter<GroupablePointFeature>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(GroupablePointCloudImporter));
        private const int groupNameProgressTextInterval = 1000;
        
        public override bool CanImportOnRootLevel => false;

        public Func<IList<GroupablePointFeature>, string> GetBaseFolder { get; set; }

        protected override object OnImportItem(string path, object target = null)
        {
            try
            {
                IList<GroupablePointFeature> pointFeatureList = target is IDataItem dataItem
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

                string baseFolder = GetBaseFolder(pointFeatureList);
                string relativePathToFile = FileUtils.GetRelativePath(baseFolder, path);
                string groupName = GroupableFeatureExtensions.GetNewGroupName(relativePathToFile, baseFolder, path);

                AddNewFeaturesToListWithGroupName(pointFeatureList, importedFeatures, groupName);

                return onImportItem;
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }

            return null;
        }

        /// <summary>
        /// We rename the group names of all GroupablePointFeatures and add them to the original list of features.
        /// This is done in batches of the number that is defined by <see cref="groupNameProgressTextInterval"/>, as we execute this code on
        /// a different thread in method AddFeatures and we can only change the progress text (shown to the user) on the current thread.
        /// </summary>
        /// <param name="originalFeatures"> The feature list on which the newly imported features will be added. </param>
        /// <param name="newFeatures"> The new features that will be added to originalFeatures list after renaming their group names. </param>
        /// <param name="groupName"> The new group name. </param>
        private void AddNewFeaturesToListWithGroupName(ICollection<GroupablePointFeature> originalFeatures, List<GroupablePointFeature> newFeatures, string groupName)
        {
            GetRegion(originalFeatures)?.BeginEdit("Setting group names");

            int numOfFeaturesToAdd = newFeatures.Count;
            for (var startIndex = 0; startIndex < numOfFeaturesToAdd; startIndex += groupNameProgressTextInterval)
            {
                int numOfFeaturesLeft = numOfFeaturesToAdd - startIndex;
                List<GroupablePointFeature> listToAdd = numOfFeaturesLeft > groupNameProgressTextInterval
                                                            ? newFeatures.GetRange(startIndex, groupNameProgressTextInterval)
                                                            : newFeatures.GetRange(startIndex, numOfFeaturesLeft);
                AddFeatures(listToAdd, originalFeatures, groupName);
                UpdateProgress(string.Format(Properties.Resources.GroupablePointCloudImporter_Setting_group_names__0_____1_, startIndex, newFeatures.Count), 2);
            }

            UpdateProgress(string.Format(Resources.PointCloudImporter_OnImportItem_Finished_importing__0__point_features, numOfFeaturesToAdd), 3);

            GetRegion(originalFeatures)?.EndEdit();
        }

        /// <summary>
        /// Adds the features in <paramref name="newFeatures"/> to <paramref name="originalFeatures"/>, after changing the group name of the feature.
        /// </summary>
        /// <param name="newFeatures"> A collection of new features. </param>
        /// <param name="originalFeatures"> The list of features to add the new features to. </param>
        /// <param name="groupName"> The new group name of the new features. </param>
        [InvokeRequired]
        private static void AddFeatures(IEnumerable<GroupablePointFeature> newFeatures, ICollection<GroupablePointFeature> originalFeatures, string groupName)
        {
            foreach (GroupablePointFeature groupablePointFeature in newFeatures)
            {
                groupablePointFeature.GroupName = groupName;
                originalFeatures.Add(groupablePointFeature);
            }
        }
    }
}