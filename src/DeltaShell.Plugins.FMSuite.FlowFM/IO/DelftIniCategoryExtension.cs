using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class DelftIniCategoryExtension
    {
        public static void AddSedimentProperty(this IDelftIniCategory category, string name, string value, string unit,
                                               string comment)
        {
            category.AddProperty(name, value,
                                 string.Format("{0,-10}{1}",
                                               string.IsNullOrEmpty(unit) ? string.Empty : "[" + unit + "]", comment));
        }
    }
}