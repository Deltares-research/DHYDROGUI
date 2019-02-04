using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Properties;
using MessageBox = DelftTools.Controls.Swf.MessageBox;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_DisplayName")]
    public class WaterFlowModel1DProperties : ObjectProperties<WaterFlowModel1D>
    {
        [PropertyOrder(1)]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Common_Name_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_Name_Description")]
        public string Name
        {
            get { return data.Name; }
            set { data.Name = value; }
        }

        [PropertyOrder(2)]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_Status_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_Status_Description")]
        public ActivityStatus Status
        {
            get { return data.Status; }
        }

        [PropertyOrder(3)]
        [TypeConverter(typeof(DeltaShellDateTimeConverter))]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_CurrentTime_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_CurrentTime_Description")]
        public DateTime CurrentTime
        {
            get { return data.CurrentTime; }
        }

        [PropertyOrder(1)]
        [ResourcesCategory(typeof(Resources), "WaterFlowModel1DProperties_Categories_RunParameters")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_ModelParameters_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_ModelParameters_Description")]
        public ModelApiParameterProperties[] ModelParameters
        {
            get { return data.ParameterSettings.Select(p => new ModelApiParameterProperties(p)).ToArray(); }
        }

        [PropertyOrder(2)]
        [TypeConverter(typeof(DeltaShellDateTimeConverter))]
        [ResourcesCategory(typeof(Resources), "WaterFlowModel1DProperties_Categories_RunParameters")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_StartTime_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_StartTime_Description")]
        public DateTime StartTime
        {
            get { return data.StartTime; }
            set { data.StartTime = value; }
        }

        [PropertyOrder(3)]
        [TypeConverter(typeof(DeltaShellTimeSpanWithMilliSecondsConverter))]
        [ResourcesCategory(typeof(Resources), "WaterFlowModel1DProperties_Categories_RunParameters")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_TimeStep_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_TimeStep_Description")]
        public TimeSpan TimeStep
        {
            get { return data.TimeStep; }
            set { data.TimeStep = value; }
        }

        [PropertyOrder(4)]
        [TypeConverter(typeof(DeltaShellDateTimeConverter))]
        [ResourcesCategory(typeof(Resources), "WaterFlowModel1DProperties_Categories_RunParameters")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_StopTime_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_StopTime_Description")]
        public DateTime StopTime
        {
            get { return data.StopTime; }
            set { data.StopTime = value; }
        }

        [PropertyOrder(6)]
        [ResourcesCategory(typeof(Resources), "WaterFlowModel1DProperties_Categories_RunParameters")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_UseRestart_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_UseRestart_Description")]
        public bool UseRestart
        {
            get { return data.UseRestart; }
            set { data.UseRestart = value; }
        }

        [PropertyOrder(7)]
        [ResourcesCategory(typeof(Resources), "WaterFlowModel1DProperties_Categories_RunParameters")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_WriteRestart_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_WriteRestart_Description")]
        public bool WriteRestart
        {
            get { return data.WriteRestart; }
            set { data.WriteRestart = value; }
        }

        [PropertyOrder(8)]
        [TypeConverter(typeof(DeltaShellDateTimeConverter))]
        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "WaterFlowModel1DProperties_Categories_RunParameters")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_SaveStateStartTime_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_SaveStateStartTime_Description")]
        public DateTime SaveStateStartTime
        {
            get { return data.SaveStateStartTime; }
            set { data.SaveStateStartTime = value; }
        }

        [PropertyOrder(9)]
        [TypeConverter(typeof(DeltaShellTimeSpanConverter))]
        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "WaterFlowModel1DProperties_Categories_RunParameters")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_SaveStateTimeStep_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_SaveStateTimeStep_Description")]
        public TimeSpan SaveStateTimeStep
        {
            get { return data.SaveStateTimeStep; }
            set { data.SaveStateTimeStep = value; }
        }

        [PropertyOrder(10)]
        [TypeConverter(typeof(DeltaShellDateTimeConverter))]
        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "WaterFlowModel1DProperties_Categories_RunParameters")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_SaveStateStopTime_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_SaveStateStopTime_Description")]
        public DateTime SaveStateStopTime
        {
            get { return data.SaveStateStopTime; }
            set { data.SaveStateStopTime = value; }
        }

        [PropertyOrder(1)]
        [TypeConverter(typeof(DeltaShellTimeSpanWithMilliSecondsConverter))]
        [ResourcesCategory(typeof(Resources), "WaterFlowModel1DProperties_Categories_OutputParameters")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_OutputTimeStep_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_OutputTimeStep_Description")]
        public TimeSpan OutputTimeStep
        {
            get { return data.OutputTimeStep; }
        }

        [PropertyOrder(1)]
        [ResourcesCategory(typeof(Resources), "WaterFlowModel1DProperties_Categories_ModelSettings")]
        [DisplayName("Salinity properties")]
        [Description("Salinity properties")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public WaterFlowModel1DSalinityProperties Salinity
        {
            get { return new WaterFlowModel1DSalinityProperties(data); }
        }

        [PropertyOrder(2)]
        [ResourcesCategory(typeof(Resources), "WaterFlowModel1DProperties_Categories_ModelSettings")]
        [DisplayName("Temperature properties")]
        [Description("Temperature properties")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public WaterFlowModel1DTemperatureProperties Temperature
        {
            get { return new WaterFlowModel1DTemperatureProperties(data); }
        }

        [PropertyOrder(3)]
        [ResourcesCategory(typeof(Resources), "WaterFlowModel1DProperties_Categories_ModelSettings")]
        [DisplayName("Advanced options")]
        [Description("Advanced options")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public WaterFlowModel1DAdvancedOptions AdvancedOptions
        {
            get { return new WaterFlowModel1DAdvancedOptions(data); }
        }

        [PropertyOrder(4)]
        [ResourcesCategory(typeof(Resources), "WaterFlowModel1DProperties_Categories_ModelSettings")]
        [DisplayName("Roughness properties")]
        [Description("Roughness properties")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public WaterFlowModel1DRoughnessProperties Roughness
        {
            get { return new WaterFlowModel1DRoughnessProperties(data); }
        }

        [PropertyOrder(5)]
        [ResourcesCategory(typeof(Resources), "WaterFlowModel1DProperties_Categories_ModelSettings")]
        [DisplayName("Morphology properties")]
        [Description("Morphology properties")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public WaterFlowModel1DMorphologyProperties Morphology
        {
            get { return new WaterFlowModel1DMorphologyProperties(data); }
        }

        [PropertyOrder(6)]
        [ResourcesCategory(typeof(Resources), "WaterFlowModel1DProperties_Categories_ModelSettings")]
        [DisplayName("Sediment properties")]
        [Description("Sediment properties")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public WaterFlowModel1DSedimentProperties Sediment
        {
            get { return new WaterFlowModel1DSedimentProperties(data); }
        }

        [PropertyOrder(1)]
        [ResourcesCategory(typeof(Resources), "WaterFlowModel1DProperties_Categories_InitialConditions")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_InitialConditionsType_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_InitialConditionsType_Description")]
        public InitialConditionsType InitialConditionsType
        {
            get { return data.InitialConditionsType; }
            set
            {
                string message;

                if (!WaterFlowModel1DHelper.CanChangeInitialConditionsType(data, out message))
                {
                    MessageBox.Show(message);
                    return;
                }

                data.InitialConditionsType = value;
            }
        }

        [PropertyOrder(2)]
        [ResourcesCategory(typeof(Resources), "WaterFlowModel1DProperties_Categories_InitialConditions")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_DefaultInitialWaterLevel_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_DefaultInitialWaterLevel_Description")]
        public double DefaultInitialWaterLevel
        {
            get { return data.DefaultInitialWaterLevel; }
            set { data.DefaultInitialWaterLevel = value; }
        }

        [PropertyOrder(3)]
        [ResourcesCategory(typeof(Resources), "WaterFlowModel1DProperties_Categories_InitialConditions")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_DefaultInitialDepth_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_DefaultInitialDepth_Description")]
        public double DefaultInitialDepth
        {
            get { return data.DefaultInitialDepth; }
            set { data.DefaultInitialDepth = value; }
        }

        [PropertyOrder(1)]
        [ResourcesCategory(typeof(Resources), "WaterFlowModel1DProperties_Categories_ModelSettings")]
        [DisplayName("Specials properties")]
        [Description("Specials properties")]
        public WaterFlowModel1DSpecialsProperties Specials
        {
            get { return new WaterFlowModel1DSpecialsProperties(data); }
        }

        [DynamicReadOnlyValidationMethod]
        public bool ValidateDynamicAttributes(string propertyName)
        {
            if (propertyName.Equals(nameof(SaveStateStartTime)) ||
                propertyName.Equals(nameof(SaveStateStopTime)) ||
                propertyName.Equals(nameof(SaveStateTimeStep)))
            {
                return !data.WriteRestart;
            }

            return false;
        }
    }
}
