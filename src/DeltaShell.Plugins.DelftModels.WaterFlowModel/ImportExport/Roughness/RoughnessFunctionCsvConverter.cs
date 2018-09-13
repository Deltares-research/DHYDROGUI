using System.Collections.Generic;
using DelftTools.Hydro.Roughness;
using DeltaShell.Plugins.NetworkEditor.ImportExportCsv;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness
{
    public class RoughnessFunctionCsvConverter : CustomEnumCsvConverter<RoughnessFunction>
    {
        private static readonly IDictionary<RoughnessFunction, string> Conversion = new Dictionary<RoughnessFunction, string>
                                                                                        {
                                                                                            {RoughnessFunction.Constant, "Constant"},
                                                                                            {RoughnessFunction.FunctionOfQ, "Discharge"},
                                                                                            {RoughnessFunction.FunctionOfH, "Waterlevel"},
                                                                                        };

        public static RoughnessFunction Fromstring(string source)
        {
            return Fromstring(Conversion, source);
        }

        public static string ToString(RoughnessFunction source)
        {
            return ToString(Conversion, source);
        }
    }
}