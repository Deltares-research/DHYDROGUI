using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Properties;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.PhysicalParameters;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.PropertyGrid
{
    [DisplayName("Temperature properties")]
    [Description("Temperature properties")]
    public class WaterFlowModel1DTemperatureProperties
    {
        private WaterFlowModel1D data;

        public WaterFlowModel1DTemperatureProperties(WaterFlowModel1D data)
        {
            this.data = data;
        }

        [PropertyOrder(1)]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_UseTemperature_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_UseTemperature_Description")]
        public bool UseTemperature
        {
            get { return data.UseTemperature; }
            set
            {
                // Pop a dialog with confirmation for turning off temperature
                if (!value)
                {
                    var result = MessageBox.Show("This will remove all temperature related data from your model. Continue?", "Remove temperature", MessageBoxButtons.YesNo);
                    if (result == DialogResult.No) return;
                }

                data.UseTemperature = value;
            }
        }

        [PropertyOrder(2)]
        [DynamicReadOnly]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_TemperatureModel_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_TemperatureModel_Description")]
        public TemperatureModelType TemperatureModelType
        {
            get { return data.TemperatureModelType; }
            set { if (value != null) data.TemperatureModelType = value; }
        }

        [PropertyOrder(3)]
        [DynamicReadOnly]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_BackgroundTemperature_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_BackgroundTemperature_Description")]
        public double BackgroundTemperature
        {
            get { return data.BackgroundTemperature; }
            set
            {
                if (value != null) data.BackgroundTemperature = value;
            }
        }

        [PropertyOrder(4)]
        [DynamicReadOnly]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_SurfaceArea_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_SurfaceArea_Description")]
        public double SurfaceArea
        {
            get { return data.SurfaceArea; }
            set
            {
                if (value != null) data.SurfaceArea = value;
            }
        }

        [PropertyOrder(5)]
        [DynamicReadOnly]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_AtmosphericPressure_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_AtmosphericPressure_Description")]
        public double AtmosphericPressure
        {
            get { return data.AtmosphericPressure; }
            set
            {
                if (value != null) data.AtmosphericPressure = value;
            }
        }

        [PropertyOrder(6)]
        [DynamicReadOnly]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_StantonNumber_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_StantonNumber_Description")]
        public double StantonNumber
        {
            get { return data.StantonNumber; }
            set
            {
                if (value != null) data.StantonNumber = value;
            }
        }

        [PropertyOrder(7)]
        [DynamicReadOnly]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_DaltonNumber_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_DaltonNumber_Description")]
        public double DaltonNumber
        {
            get { return data.DaltonNumber; }
            set
            {
                if (value != null) data.DaltonNumber = value;
            }
        }

        [PropertyOrder(8)]
        [DynamicReadOnly]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_HeatCapacity_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_HeatCapacity_Description")]
        public double HeatCapacityWater
        {
            get { return data.HeatCapacityWater; }
            set
            {
                if (value != null) data.HeatCapacityWater = value;
            }
        }

        /* Override needed to avoid displaying the whole class name in the property grid */
        public override string ToString()
        {
            return "";
        }

        readonly List<string> _enabledWhenUseTemperature = new List<string>
        {
            "SurfaceArea",
            "AtmosphericPressure",
            "StantonNumber",
            "DaltonNumber",
            "HeatCapacityWater"
        };

        [DynamicReadOnlyValidationMethod]
        public bool ValidateDynamicAttributes(string propertyName)
        {
            if (propertyName.Equals("TemperatureModelType"))
            {
                return !data.UseTemperature;
            }

            if (propertyName.Equals("BackgroundTemperature"))
            {
                if (data.UseTemperature)
                {
                    return !(data.TemperatureModelType == TemperatureModelType.Composite || data.TemperatureModelType == TemperatureModelType.Excess);
                }
                return true;
            }

            if (_enabledWhenUseTemperature.Contains(propertyName))
            {
                if (data.UseTemperature)
                {
                    return data.TemperatureModelType != TemperatureModelType.Composite;
                }
                return true;
            }
            return false;
        }
    }
}