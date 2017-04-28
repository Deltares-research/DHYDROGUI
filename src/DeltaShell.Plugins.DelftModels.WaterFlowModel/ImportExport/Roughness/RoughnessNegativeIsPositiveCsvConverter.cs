using System.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.ImportExportCsv;

//using DeltaShell.Plugins.NetworkEditor.ImportExportCsv;

//public enum RoughnessCsvNegativeIsPositive
//{
//    Same,
//    Different
//}

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness
{
    //public class RoughnessNegativeIsPositiveCsvConverter2 : CustomEnumCsvConverter<RoughnessCsvNegativeIsPositive>
    //{
    //    private static readonly IDictionary<RoughnessCsvNegativeIsPositive, string> Conversion = new Dictionary<RoughnessCsvNegativeIsPositive, string>
    //                                                                                                 {
    //                                                                                                     {RoughnessCsvNegativeIsPositive.Same, "Same"},
    //                                                                                                     {RoughnessCsvNegativeIsPositive.Different, "Different"},
    //                                                                                                 };

    //    public static bool Fromstring(string source)
    //    {
    //        return Fromstring(Conversion, source) == RoughnessCsvNegativeIsPositive.Same;
    //    }

    //    public static string ToString(bool source)
    //    {
    //        return ToString(Conversion, source ? RoughnessCsvNegativeIsPositive.Same : RoughnessCsvNegativeIsPositive.Different);
    //    }
    //}

    public class RoughnessNegativeIsPositiveCsvConverter : CustomEnumCsvConverter<bool>
    {
        protected static readonly IDictionary<bool, string> Conversion = new Dictionary<bool, string>
                                                                             {
                                                                                 {false, "Different"},
                                                                                 {true, "Same"},
                                                                             };

        public static bool Fromstring(string source)
        {
            return Fromstring(Conversion, source);
        }

        public static string ToString(bool source)
        {
            return ToString(Conversion, source);
        }
    }

}