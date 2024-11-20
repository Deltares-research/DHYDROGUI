using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Utils.Collections;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData
{
    public static class BridgePillarFmModelFeatureCoordinateDataSyncExtensions
    {
        public const string DiameterColumnName = "Pillar Diameter [m]";
        public const string DragcoefficientColumnName = "Drag coefficient [-]";

        public static void UpdateDataColumns(this ModelFeatureCoordinateData<BridgePillar> data)
        {
            List<IDataColumn> expectedColumns = DataColumns().ToList();

            IEnumerable<IDataColumn> missingDataColumns = expectedColumns.Except(data.DataColumns);

            foreach (IDataColumn missingDataColumn in missingDataColumns)
            {
                int index = expectedColumns.IndexOf(missingDataColumn);
                data.DataColumns.Insert(index, missingDataColumn);
            }

            data.DataColumns
                .Where(c => expectedColumns.Contains(c))
                .ForEach(c => c.IsActive = true);
        }

        private static IEnumerable<IDataColumn> DataColumns()
        {
            yield return new DataColumn<double>(DiameterColumnName) {DefaultValue = -999};
            yield return new DataColumn<double>(DragcoefficientColumnName) {DefaultValue = 1};
        }
    }
}