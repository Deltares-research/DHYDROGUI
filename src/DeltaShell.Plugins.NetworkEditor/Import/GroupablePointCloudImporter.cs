using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.SharpMapGis.ImportExport;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    public class GroupablePointCloudImporter : PointCloudImporter<GroupablePointFeature>
    { 
        protected override object OnImportItem(string path, object target = null)
        {
            var onImportItem = base.OnImportItem(path, target);
            if (target == null) return null;

            var dataItem = target as IDataItem;
            var pointFeatureList = dataItem != null
                ? dataItem.Value as IList<GroupablePointFeature>
                : target as IList<GroupablePointFeature>;

            if (pointFeatureList == null) return null;
            pointFeatureList.ForEach(pf => pf.GroupName = path );

            return onImportItem;
        }
    }
}