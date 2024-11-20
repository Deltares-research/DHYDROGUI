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
    public class RealTimeControlModelProperties : ObjectProperties<IRealTimeControlModel>
    {
        [PropertyOrder(1)]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Common_Name_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RealTimeControlModelProperties_Name_Description")]
        public string Name
        {
            get => data.Name;
            set => data.Name = value;
        }

        [PropertyOrder(2)]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "RealTimeControlModelProperties_CoordinateSystem_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RealTimeControlModelProperties_CoordinateSystem_Description")]
        [TypeConverter(typeof(CoordinateSystemStringTypeConverter))]
        [Editor(typeof(CoordinateSystemTypeEditor), typeof(UITypeEditor))]
        public ICoordinateSystem CoordinateSystem
        {
            get => data.CoordinateSystem;
            set => data.CoordinateSystem = value;
        }

        [PropertyOrder(0)]
        [ReadOnly(true)]
        [ResourcesCategory(typeof(Resources), "RealTimeControlModelProperties_Category_RunParameters")]
        [ResourcesDisplayName(typeof(Resources), "RealTimeControlModelProperties_UseRestart_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RealTimeControlModelProperties_UseRestart_Description")]
        public bool UseRestart => data.UseRestart;

        [PropertyOrder(1)]
        [ResourcesCategory(typeof(Resources), "RealTimeControlModelProperties_Category_RunParameters")]
        [ResourcesDisplayName(typeof(Resources), "RealTimeControlModelProperties_WriteRestart_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RealTimeControlModelProperties_WriteRestart_Description")]
        public bool WriteRestart
        {
            get => data.WriteRestart;
            set
            {
                if (data.WriteRestart == value)
                {
                    return;
                }
                
                data.WriteRestart = value;
                data.MarkOutputOutOfSync();
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
            get => data.SaveStateStartTime;
            set
            {
                if (data.SaveStateStartTime == value)
                {
                    return;
                }
                
                data.SaveStateStartTime = value;
                data.MarkOutputOutOfSync();
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
            get => data.SaveStateTimeStep;
            set
            {
                if (data.SaveStateTimeStep == value)
                {
                    return;
                }
                
                data.SaveStateTimeStep = value;
                data.MarkOutputOutOfSync();
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
            get => data.SaveStateStopTime;
            set
            {
                if (data.SaveStateStopTime == value)
                {
                    return;
                }
                
                data.SaveStateStopTime = value;
                data.MarkOutputOutOfSync();
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