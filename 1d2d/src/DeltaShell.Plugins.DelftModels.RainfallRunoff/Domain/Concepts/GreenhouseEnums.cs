using System.ComponentModel;
using DelftTools.Utils;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts
{
    public static class GreenhouseEnums
    {
        #region AreaPerGreenhouseType enum

        [TypeConverter(typeof (EnumDescriptionAttributeTypeConverter))]
        public enum AreaPerGreenhouseType
        {
            [Description("< 500 m³/ha")] lessThan500 = 0,
            [Description("500-1000 m³/ha")] from500to1000,
            [Description("1000-1500 m³/ha")] from1000to1500,
            [Description("1500-2000 m³/ha")] from1500to2000,
            [Description("2000-2500 m³/ha")] from2000to2500,
            [Description("2500-3000 m³/ha")] from2500to3000,
            [Description("3000-4000 m³/ha")] from3000to4000,
            [Description("4000-5000 m³/ha")] from4000to5000,
            [Description("5000-6000 m³/ha")] from5000to6000,
            [Description("> 6000 m³/ha")] moreThan6000
        }

        #endregion
    }
}