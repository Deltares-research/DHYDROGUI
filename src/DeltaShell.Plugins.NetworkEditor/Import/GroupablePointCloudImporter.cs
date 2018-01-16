using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.NetworkEditor.Properties;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    public class GroupablePointCloudImporter : PointCloudImporter<GroupablePointFeature>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GroupablePointCloudImporter));

        protected override object OnImportItem(string path, object target = null)
        {
            try
            {
                var onImportItem = base.OnImportItem(path, target);
                if (target == null) return null;

                var dataItem = target as IDataItem;
                var pointFeatureList = dataItem != null
                    ? dataItem.Value as IList<GroupablePointFeature>
                    : target as IList<GroupablePointFeature>;

                if (pointFeatureList == null) return null;
                pointFeatureList.ForEach(pf => pf.GroupName = path);

                return onImportItem;
            }
            catch (FormatException e)
            {
                Log.Error(e.Message);
                Log.Error(Resources.GroupablePointCloudImporter_OnImportItem_Wrong_content_format__Only_allowed__three__columns_space_separated_);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }

            return null;
        }
    }
}