using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Editing;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using DeltaShell.Plugins.SharpMapGis.Properties;
using GeoAPI.Extensions.Feature;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    public class GroupablePointCloudImporter : PointCloudImporter<GroupablePointFeature>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GroupablePointCloudImporter));
        private ImportProgressChangedDelegate progressChanged;
        private int groupNameProgressTextInterval = 1000;

        public override bool CanImportOnRootLevel
        {
            get { return false; }
        }

        public override ImportProgressChangedDelegate ProgressChanged
        {
            get { return base.ProgressChanged; }
            set
            {
                progressChanged = value;
                base.ProgressChanged = (name, current, total) =>
                {
                    if(current == 2) return;
                    progressChanged?.Invoke(name, current, total + 1);
                };
            }
        }

        public Func<IList<GroupablePointFeature>, string> GetBaseFolder { get; set; }

        protected override object OnImportItem(string path, object target = null)
        {
            try
            {
                var dataItem = target as IDataItem;
                var pointFeatureList = dataItem != null
                    ? dataItem.Value as IList<GroupablePointFeature>
                    : target as IList<GroupablePointFeature>;

                if (pointFeatureList == null) return null;

                object onImportItem;
                if (GetBaseFolder != null && GetRegion != null)
                {
                    var importedFeatures = new List<GroupablePointFeature>();
                    onImportItem = base.OnImportItem(path, importedFeatures);

                    var region = GetRegion?.Invoke(pointFeatureList);
                    var baseFolder = GetBaseFolder(pointFeatureList);
                    var relativePathToFile = FileUtils.GetRelativePath(baseFolder, path);
                    var groupName = GroupableFeatureExtensions.GetNewGroupName(relativePathToFile, baseFolder, path);

                    AddNewFeaturesToListWithGroupName(region, pointFeatureList, importedFeatures, groupName);

                    return onImportItem;
                }
                else
                {
                    onImportItem = base.OnImportItem(path, pointFeatureList);
                    pointFeatureList.ForEach(f => f.GroupName = path);
                }

                return onImportItem;
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }

            return null;
        }

        private void AddNewFeaturesToListWithGroupName(IRegion region, IList<GroupablePointFeature> originalFeatures, List<GroupablePointFeature> newFeatures, string groupName)
        {
            region?.BeginEdit("Setting group names");
            var numOfFeaturesToAdd = newFeatures.Count;
            for (var startIndex = 0; startIndex < numOfFeaturesToAdd; startIndex += groupNameProgressTextInterval)
            {
                var numOfFeaturesLeft = numOfFeaturesToAdd - startIndex;
                var listToAdd = numOfFeaturesLeft > groupNameProgressTextInterval
                    ? newFeatures.GetRange(startIndex, groupNameProgressTextInterval)
                    : newFeatures.GetRange(startIndex, numOfFeaturesLeft);
                AddFeatures(listToAdd, originalFeatures, groupName);
                progressChanged?.Invoke($"Setting group names {startIndex} / {newFeatures.Count}", 2, 3);
            }

            progressChanged?.Invoke(string.Format(Resources.PointCloudImporter_OnImportItem_Finished_importing__0__point_features, numOfFeaturesToAdd), 3, 3);
            region?.EndEdit();
        }

        [InvokeRequired]
        private static void AddFeatures(IList<GroupablePointFeature> newList, IList<GroupablePointFeature> pointFeatureList, string groupName)
        {
            foreach (var groupablePointFeature in newList)
            {
                groupablePointFeature.GroupName = groupName;
                pointFeatureList.Add(groupablePointFeature);
            }
        }
    }
}