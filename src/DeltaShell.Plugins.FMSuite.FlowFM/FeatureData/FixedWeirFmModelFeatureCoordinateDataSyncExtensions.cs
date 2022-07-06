using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData
{
    public static class FixedWeirFmModelFeatureCoordinateDataSyncExtensions
    {
        public const string CrestLevelColumnName = "Crest Level [mAD]";
        public const string SillUpColumnName = "Ground Height Left [m]";
        public const string SillDownColumnName = "Ground Height Right [m]";
        public const string CrestLengthColumnName = "Crest Width [m]";
        public const string TaludUpColumnName = "Slope Left [-]";
        public const string TaludDownColumnName = "Slope Right  [-]";
        public const string VegetationCoefficientColumnName = "Roughness Code [-]";

        public static void UpdateDataColumns(this ModelFeatureCoordinateData<FixedWeir> data, string fixedWeirScheme)
        {
            FixedWeirSchemes scheme;
            if (!Enum.TryParse(fixedWeirScheme, true, out scheme)) return;

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

           data.DataColumns
               .Where(c => expectedColumns.Contains(c, nameComparer))
               .ForEach(c => c.IsActive = true);
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
            yield return new DataColumn<double>(CrestLevelColumnName);
            yield return new DataColumn<double>(SillUpColumnName);
            yield return new DataColumn<double>(SillDownColumnName);
        }

        private static IEnumerable<IDataColumn> DataColumnsScheme9()
        {
            yield return new DataColumn<double>(CrestLengthColumnName) {DefaultValue = 3.0};
            yield return new DataColumn<double>(TaludUpColumnName) {DefaultValue = 4.0};
            yield return new DataColumn<double>(TaludDownColumnName) {DefaultValue = 4.0};
            yield return new DataColumn<double>(VegetationCoefficientColumnName);
        }
    }
}