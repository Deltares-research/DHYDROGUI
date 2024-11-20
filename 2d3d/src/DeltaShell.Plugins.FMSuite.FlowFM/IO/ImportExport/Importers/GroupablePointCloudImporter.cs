using System;
using System.Collections.Generic;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers
{
    public class GroupablePointCloudImporter : PointCloudImporter<GroupablePointFeature>
    {
        private const int groupNameProgressTextInterval = 1000;
        private static readonly ILog log = LogManager.GetLogger(typeof(GroupablePointCloudImporter));

        public override bool CanImportOnRootLevel => false;

        public Func<IList<GroupablePointFeature>, string> GetRootDirectory { get; set; }
        public Func<IList<GroupablePointFeature>, string> GetBaseDirectory { get; set; }

        protected override object OnImportItem(string path, object target = null)
        {
            try
            {
                IList<GroupablePointFeature> pointFeatureList = target is IDataItem dataItem
                                                                    ? dataItem.Value as IList<GroupablePointFeature>
                                                                    : target as IList<GroupablePointFeature>;

                if (pointFeatureList == null)
                {
                    return null;
                }

                object onImportItem;
                // If importing from DeltaShell GUI, GetBaseFolder is set. In that case we import with progress
                if (GetBaseDirectory == null || GetBaseDirectory(pointFeatureList) == string.Empty || GetRegion == null)
                {
                    onImportItem = base.OnImportItem(path, pointFeatureList);
                    pointFeatureList.ForEach(f => f.GroupName = path);
                    return onImportItem;
                }

                TotalNumberOfProgressSteps = 3;
                var importedFeatures = new List<GroupablePointFeature>();
                onImportItem = base.OnImportItem(path, importedFeatures);

                AddNewFeaturesToListWithGroupName(pointFeatureList, importedFeatures, path);

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
        /// This is done in batches of the number that is defined by <see cref="groupNameProgressTextInterval"/>, as we execute
        /// this code on
        /// a different thread in method AddFeatures and we can only change the progress text (shown to the user) on the current
        /// thread.
        /// </summary>
        /// <param name="originalFeatures"> The feature list on which the newly imported features will be added. </param>
        /// <param name="newFeatures">
        /// The new features that will be added to originalFeatures list after renaming their group
        /// names.
        /// </param>
        /// <param name="path"> The feature file path. </param>
        private void AddNewFeaturesToListWithGroupName(IList<GroupablePointFeature> originalFeatures, List<GroupablePointFeature> newFeatures, string path)
        {
            GetRegion(originalFeatures)?.BeginEdit("Setting group names");

            int numOfFeaturesToAdd = newFeatures.Count;
            for (var startIndex = 0; startIndex < numOfFeaturesToAdd; startIndex += groupNameProgressTextInterval)
            {
                int numOfFeaturesLeft = numOfFeaturesToAdd - startIndex;
                List<GroupablePointFeature> listToAdd = numOfFeaturesLeft > groupNameProgressTextInterval
                                                            ? newFeatures.GetRange(startIndex, groupNameProgressTextInterval)
                                                            : newFeatures.GetRange(startIndex, numOfFeaturesLeft);
                AddFeatures(listToAdd, originalFeatures, path);
                UpdateProgress(string.Format(Resources.GroupablePointCloudImporter_Setting_group_names__0_____1_, startIndex, newFeatures.Count), 2);
            }

            UpdateProgress(string.Format(SharpMapGis.Properties.Resources.PointCloudImporter_OnImportItem_Finished_importing__0__point_features, numOfFeaturesToAdd), 3);

            GetRegion(originalFeatures)?.EndEdit();
        }

        /// <summary>
        /// Adds the features in <paramref name="newFeatures"/> to <paramref name="originalFeatures"/>, after changing the group
        /// name of the feature.
        /// </summary>
        /// <param name="newFeatures"> A collection of new features. </param>
        /// <param name="originalFeatures"> The list of features to add the new features to. </param>
        /// <param name="path"> The feature file path. </param>
        [InvokeRequired]
        private void AddFeatures(IEnumerable<GroupablePointFeature> newFeatures, IList<GroupablePointFeature> originalFeatures, string path)
        {
            foreach (GroupablePointFeature groupablePointFeature in newFeatures)
            {
                groupablePointFeature.GroupName = path;
                groupablePointFeature.MakeGroupNameRelative(GetRootDirectory(originalFeatures), GetBaseDirectory(originalFeatures));
                originalFeatures.Add(groupablePointFeature);
            }
        }
    }
}