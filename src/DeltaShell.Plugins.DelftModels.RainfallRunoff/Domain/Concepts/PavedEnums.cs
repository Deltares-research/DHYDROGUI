using System.ComponentModel;
using DelftTools.Utils;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts
{
    public static class PavedEnums
    {
        #region DryWeatherFlowOptions enum

        [TypeConverter(typeof (EnumDescriptionAttributeTypeConverter))]
        public enum DryWeatherFlowOptions
        {
            [Description("# inhabitants * constant DWF")] NumberOfInhabitantsTimesConstantDWF = 0,
            [Description("# inhabitants * variable DWF")] NumberOfInhabitantsTimesVariableDWF,
            [Description("1 * constant DWF")] ConstantDWF,
            [Description("1 * variable DWF")] VariableDWF
        }

        #endregion

        #region SewerPumpCapacityUnit enum

        [TypeConverter(typeof (EnumDescriptionAttributeTypeConverter))]
        public enum SewerPumpCapacityUnit
        {
            [Description("mm/hr")] mm_hr,
            [Description("m³/min")] m3_min,
            [Description("m³/hr")] m3_hr,
            [Description("m³/s")] m3_s,
        }

        #endregion

        #region SewerPumpDischargeTarget enum

        [TypeConverter(typeof (EnumDescriptionAttributeTypeConverter))]
        public enum SewerPumpDischargeTarget
        {
            [Description("Lateral source or boundary node")]
            BoundaryNode=0,
            [Description("Open water")]
            OpenWater = 1,
            [Description("Wastewater treatment plant")] 
            WWTP=2
        }

        #endregion

        #region SewerType enum

        [TypeConverter(typeof (EnumDescriptionAttributeTypeConverter))]
        public enum SewerType
        {
            [Description("Mixed system")] MixedSystem,
            [Description("Separate system")] SeparateSystem,
            [Description("Improved separate system")] ImprovedSeparateSystem
        }

        #endregion

        #region SpillingDefinition enum

        [TypeConverter(typeof (EnumDescriptionAttributeTypeConverter))]
        public enum SpillingDefinition
        {
            [Description("No Delay")] NoDelay,
            [Description("Use Runoff Coefficient")] UseRunoffCoefficient,
            //[Description("Use QH-relation")]
            //UseQHRelation
        }

        #endregion

        #region WaterUseUnit enum

        [TypeConverter(typeof (EnumDescriptionAttributeTypeConverter))]
        public enum WaterUseUnit
        {
            [Description("m³/s")] m3_s = 0,
            [Description("l/hr")] l_hr,
            [Description("l/day")] l_day
        }

        #endregion
    }
}