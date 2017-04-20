using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.ImportExportCsv;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness
{
    /// <summary>
    /// Converts RoughnessType to a more user friendly string format and vice versa.
    /// Used by RoughnessFromCsvFileImporter and RoughnessFromCsvFileExporter.
    /// </summary>
    public class RoughnessTypeCsvConverter : CustomEnumCsvConverter<RoughnessType>
    {
        private static readonly IDictionary<RoughnessType, string> Conversion = new Dictionary<RoughnessType, string>
                                                                           {
                                                                               {RoughnessType.Chezy, "Chezy"},
                                                                               {RoughnessType.Manning, "Manning"},
                                                                               {RoughnessType.StricklerKs, "Strickler ks"},
                                                                               {RoughnessType.StricklerKn, "Strickler kn"},
                                                                               {RoughnessType.WhiteColebrook, "White & Colebrook"},
                                                                               {RoughnessType.DeBosAndBijkerk, "Bos & Bijkerk"},
                                                                           };

        public static RoughnessType Fromstring(string source)
        {
            return Fromstring(Conversion, source);
        }

        public static string ToString(RoughnessType source)
        {
            return ToString(Conversion, source);
        }
    }

}