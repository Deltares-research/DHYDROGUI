using System;
using System.ComponentModel;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl
{
    [ResourcesDisplayName(typeof(Resources), "RealTimeControlModelProperties_DisplayName")]
    public class RealTimeControlModelProperties : ObjectProperties<RealTimeControlModel>
    {
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Common_Name_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RealTimeControlModelProperties_Name_Description")]
        public string Name
        {
            get { return data.Name; }
            set { data.Name = value; }
        }

        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "RealTimeControlModelProperties_LimitMemory_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RealTimeControlModelProperties_LimitMemory_Description")]
        public bool LimitMemory
        {
            get { return data.LimitMemory; }
            set { data.LimitMemory = value; }
        }
        
        [PropertyOrder(0)]
        [ResourcesCategory(typeof(Resources), "RealTimeControlModelProperties_Category_RunParameters")]
        [ResourcesDisplayName(typeof(Resources), "RealTimeControlModelProperties_UseSaveStateTimeRange_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RealTimeControlModelProperties_UseSaveStateTimeRange_Description")]
        public bool UseSaveStateTimeRange
        {
            get { return data.UseSaveStateTimeRange; }
            set { data.UseSaveStateTimeRange = value; }
        }

        [PropertyOrder(1)]
        [TypeConverter(typeof(DeltaShellDateTimeConverter))]
        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "RealTimeControlModelProperties_Category_RunParameters")]
        [ResourcesDisplayName(typeof(Resources), "RealTimeControlModelProperties_SaveStateStartTime_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RealTimeControlModelProperties_SaveStateStartTime_Description")]
        public DateTime SaveStateStartTime
        {
            get { return data.SaveStateStartTime; }
            set { data.SaveStateStartTime = value; }
        }

        [PropertyOrder(2)]
        [TypeConverter(typeof(DeltaShellDateTimeConverter))]
        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "RealTimeControlModelProperties_Category_RunParameters")]
        [ResourcesDisplayName(typeof(Resources), "RealTimeControlModelProperties_SaveStateStopTime_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RealTimeControlModelProperties_SaveStateStopTime_Description")]
        public DateTime SaveStateStopTime
        {
            get { return data.SaveStateStopTime; }
            set { data.SaveStateStopTime = value; }
        }

        [PropertyOrder(3)]
        [TypeConverter(typeof(DeltaShellTimeSpanConverter))]
        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "RealTimeControlModelProperties_Category_RunParameters")]
        [ResourcesDisplayName(typeof(Resources), "RealTimeControlModelProperties_SaveStateTimeStep_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RealTimeControlModelProperties_SaveStateTimeStep_Description")]
        public TimeSpan SaveStateTimeStep
        {
            get { return data.SaveStateTimeStep; }
            set { data.SaveStateTimeStep = value; }
        }

        [ResourcesCategory(typeof(Resources), "RealTimeControlModelProperties_Category_RunParameters")]
        [ResourcesDisplayName(typeof(Resources), "RealTimeControlModelProperties_UseRestart_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RealTimeControlModelProperties_UseRestart_Description")]
        public bool UseRestart
        {
            get { return data.UseRestart; }
            set { data.UseRestart = value; }
        }

        [ResourcesCategory(typeof(Resources), "RealTimeControlModelProperties_Category_RunParameters")]
        [ResourcesDisplayName(typeof(Resources), "RealTimeControlModelProperties_WriteRestart_DisplayName")]
        [ResourcesDescription(typeof(Resources), "RealTimeControlModelProperties_WriteRestart_Description")]
        public bool WriteRestart
        {
            get { return data.WriteRestart; }
            set { data.WriteRestart = value; }
        }

        [DynamicReadOnlyValidationMethod]
        public bool ValidateDynamicAttributes(string propertyName)
        {
            if (propertyName.Equals("SaveStateStartTime") ||
                propertyName.Equals("SaveStateStopTime") ||
                propertyName.Equals("SaveStateTimeStep"))
            {
                return !data.UseSaveStateTimeRange;
            }

            return false;
        }
    }
}