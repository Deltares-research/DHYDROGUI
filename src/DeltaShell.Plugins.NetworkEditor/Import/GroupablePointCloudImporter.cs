using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Extensions.Feature;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    public class GroupablePointCloudImporter : PointCloudImporter<GroupablePointFeature>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GroupablePointCloudImporter));

        public override bool CanImportOnRootLevel
        {
            get { return false; }
        }

        public Func<IList<GroupablePointFeature>, string> GetBaseFolder { get; set; }

        public Action<IRegion, IList<GroupablePointFeature>, IList<GroupablePointFeature>> SetItems { get; set; }

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
                if (GetBaseFolder != null && GetRegion != null && SetItems != null)
                {
                    var newList = new List<GroupablePointFeature>();
                    onImportItem = base.OnImportItem(path, newList);
                    var baseFolder = GetBaseFolder(pointFeatureList);
                    var groupName = FileUtils.GetRelativePath(baseFolder, path);
                    newList.ForEach(f => f.GroupName = groupName);

                    var region = GetRegion?.Invoke(pointFeatureList);
                    SetItems(region, newList, pointFeatureList);
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
    }
}