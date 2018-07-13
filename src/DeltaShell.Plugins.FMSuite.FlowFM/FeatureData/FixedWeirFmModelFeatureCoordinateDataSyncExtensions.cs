using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData
{
    public static class FixedWeirFmModelFeatureCoordinateDataSyncExtensions
    {
        public static void UpdateDataColumns(this ModelFeatureCoordinateData<FixedWeir> data, string fixedWeirScheme)
        {
            FixedWeirSchemes scheme;
            if (!Enum.TryParse(fixedWeirScheme, true, out scheme)) return; // Todo : error ??

            var expectedColumns = GetExpectedColumns(scheme).ToList();

            var nameComparer = new DataColumnsNameComparer();

            var missingDataColumns = expectedColumns.Except(data.DataColumns, nameComparer);
            var unexpectedColumns = data.DataColumns.Except(expectedColumns, nameComparer);

            foreach (var missingDataColumn in missingDataColumns)
            {
                var index = expectedColumns.IndexOf(missingDataColumn);
                data.DataColumns.Insert(index, missingDataColumn);
            }

            unexpectedColumns.ForEach(c => c.IsActive = false);
        }

        private static IEnumerable<IDataColumn> GetExpectedColumns(FixedWeirSchemes scheme)
        {
            switch (scheme)
            {
                case FixedWeirSchemes.None:
                case FixedWeirSchemes.Scheme6:
                case FixedWeirSchemes.Scheme8:
                    return DataColumnsForScheme6And8And0();
                case FixedWeirSchemes.Scheme9:
                    return DataColumnsForScheme6And8And0().Concat(DataColumnsScheme9());
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static IEnumerable<IDataColumn> DataColumnsForScheme6And8And0()
        {
            yield return new DataColumn<double>("Crest Levels");
            yield return new DataColumn<double>("Ground Levels Left");
            yield return new DataColumn<double>("Ground Levels Right");
        }

        private static IEnumerable<IDataColumn> DataColumnsScheme9()
        {
            yield return new DataColumn<double>("Crest Length") {DefaultValue = 3.0};
            yield return new DataColumn<double>("Talud Up") {DefaultValue = 4.0};
            yield return new DataColumn<double>("Talud Down") {DefaultValue = 4.0};
            yield return new DataColumn<double>("Vegetation Coefficient");
        }
    }
}