using System;
using System.ComponentModel;
using System.Drawing.Design;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using GeoAPI.Extensions.CoordinateSystems;
using SharpMap.UI.Forms;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui
{
    [ResourcesDisplayName(typeof(Resources), "RealTimeControlModelProperties_DisplayName")]
    public class RealTimeControlModelProperties : ObjectProperties<RealTimeControlModel>
    {
        [PropertyOrder(1)]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Common_Name_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RealTimeControlModelProperties_Name_Description")]
        public string Name
        {
            get
            {
                return data.Name;
            }
            set
            {
                data.Name = value;
            }
        }

        [PropertyOrder(2)]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "RealTimeControlModelProperties_CoordinateSystem_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RealTimeControlModelProperties_CoordinateSystem_Description")]
        [TypeConverter(typeof(CoordinateSystemStringTypeConverter))]
        [Editor(typeof(CoordinateSystemTypeEditor), typeof(UITypeEditor))]
        public ICoordinateSystem CoordinateSystem
        {
            get
            {
                return data.CoordinateSystem;
            }
            set
            {
                data.CoordinateSystem = value;
            }
        }

        [PropertyOrder(0)]
        [ResourcesCategory(typeof(Resources), "RealTimeControlModelProperties_Category_RunParameters")]
        [ResourcesDisplayName(typeof(Resources), "RealTimeControlModelProperties_UseRestart_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RealTimeControlModelProperties_UseRestart_Description")]
        public bool UseRestart
        {
            get
            {
                return data.UseRestart;
            }
            set
            {
                data.UseRestart = value;
            }
        }

        [PropertyOrder(1)]
        [ResourcesCategory(typeof(Resources), "RealTimeControlModelProperties_Category_RunParameters")]
        [ResourcesDisplayName(typeof(Resources), "RealTimeControlModelProperties_WriteRestart_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RealTimeControlModelProperties_WriteRestart_Description")]
        public bool WriteRestart
        {
            get
            {
                return data.WriteRestart;
            }
            set
            {
                data.WriteRestart = value;
            }
        }

        [PropertyOrder(2)]
        [TypeConverter(typeof(DeltaShellDateTimeConverter))]
        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "RealTimeControlModelProperties_Category_RunParameters")]
        [ResourcesDisplayName(typeof(Resources), "RealTimeControlModelProperties_SaveStateStartTime_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RealTimeControlModelProperties_SaveStateStartTime_Description")]
        public DateTime SaveStateStartTime
        {
            get
            {
                return data.SaveStateStartTime;
            }
            set
            {
                data.SaveStateStartTime = value;
            }
        }

        [PropertyOrder(3)]
        [TypeConverter(typeof(DeltaShellTimeSpanConverter))]
        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "RealTimeControlModelProperties_Category_RunParameters")]
        [ResourcesDisplayName(typeof(Resources), "RealTimeControlModelProperties_SaveStateTimeStep_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RealTimeControlModelProperties_SaveStateTimeStep_Description")]
        public TimeSpan SaveStateTimeStep
        {
            get
            {
                return data.SaveStateTimeStep;
            }
            set
            {
                data.SaveStateTimeStep = value;
            }
        }

        [PropertyOrder(4)]
        [TypeConverter(typeof(DeltaShellDateTimeConverter))]
        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "RealTimeControlModelProperties_Category_RunParameters")]
        [ResourcesDisplayName(typeof(Resources), "RealTimeControlModelProperties_SaveStateStopTime_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RealTimeControlModelProperties_SaveStateStopTime_Description")]
        public DateTime SaveStateStopTime
        {
            get
            {
                return data.SaveStateStopTime;
            }
            set
            {
                data.SaveStateStopTime = value;
            }
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