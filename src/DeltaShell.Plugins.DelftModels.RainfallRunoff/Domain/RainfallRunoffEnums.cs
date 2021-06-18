using System.ComponentModel;
using DelftTools.Utils;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain
{
    [TypeConverter(typeof (EnumDescriptionAttributeTypeConverter))]
    public enum RainfallRunoffConceptsEnum
    {
        [Description("Not schematized")] NotSchematized = 0,
        [Description("Polder concept")] PolderConcept = 1,
    }

    public static class RainfallRunoffEnums
    {
        #region AreaUnit enum

        [TypeConverter(typeof (EnumDescriptionAttributeTypeConverter))]
        public enum AreaUnit
        {
            [Description("m²")] m2,
            [Description("ha")] ha,
            [Description("km²")] km2
        }

        #endregion

        #region RainfallCapacityUnit enum

        [TypeConverter(typeof (EnumDescriptionAttributeTypeConverter))]
        public enum RainfallCapacityUnit
        {
            [Description("mm/hr")] mm_hr,
            [Description("mm/day")] mm_day
        }

        #endregion

        #region StorageUnit enum

        [TypeConverter(typeof (EnumDescriptionAttributeTypeConverter))]
        public enum StorageUnit
        {
            [Description("mm (x Area)")] mm,
            [Description("m³")] m3,
        }

        #endregion

        #region CapSim

        [TypeConverter(typeof (EnumDescriptionAttributeTypeConverter))]
        public enum CapsimInitOptions
        {
            [Description("No Capsim")] NoCapsim = 0,
            [Description("At equilibrium moisture")] AtEquilibriumMoisture = 1,
            [Description("At moisture content pF2")] AtMoistureContentpF2 = 2,
            [Description("At moisture content pF3")] AtMoistureContentpF3 = 3
        }

        [TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
        public enum CapsimCropAreaOptions
        {
            [Description("CapSim per crop area")] PerCropArea = -1,
            [Description("CapSim average crops per unpaved area")] AveragedDataPerUnpavedArea = 0
        }

        #endregion CapSim
    }
}