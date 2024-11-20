using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData
{
    public static class BridgePillarFmModelFeatureCoordinateDataSyncExtensions
    {
        public const string DiameterColumnName = "Pillar Diameter [m]";
        public const string DragcoefficientColumnName = "Drag coefficient [-]";

        public static void UpdateDataColumns(this ModelFeatureCoordinateData<BridgePillar> data)
        {
            var expectedColumns = DataColumns().ToList();

            var missingDataColumns = expectedColumns.Except(data.DataColumns);

            foreach (var missingDataColumn in missingDataColumns)
            {
                var index = expectedColumns.IndexOf(missingDataColumn);
                data.DataColumns.Insert(index, missingDataColumn);
            }

            data.DataColumns
                .Where(c => expectedColumns.Contains(c))
                .ForEach(c => c.IsActive = true);
        }

        private static IEnumerable<IDataColumn> DataColumns()
        {
            yield return new DataColumn<double>(DiameterColumnName){DefaultValue = -999};
            yield return new DataColumn<double>(DragcoefficientColumnName){DefaultValue = 1};
        }
    }
}


