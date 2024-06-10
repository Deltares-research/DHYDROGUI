using System.ComponentModel;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms.Properties
{
    [ResourcesDisplayName(typeof(Resources), "OutputProperties_DisplayName")]
    public class OutputProperties : ObjectProperties<Output>
    {
        /// <summary>
        /// Display Name as Id to user, identical behaviour to rule with Name and LongName
        /// </summary>
        [ReadOnly(true)]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Common_Name_DisplayName")]
        [ResourcesDescription(typeof(Resources), "OutputProperties_Name_Description")]
        public string Name
        {
            get
            {
                return data.Name;
            }
        }

        [ReadOnly(true)]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "OutputProperties_Location_DisplayName")]
        [ResourcesDescription(typeof(Resources), "OutputProperties_Location_Description")]
        public string Location
        {
            get
            {
                return data.LocationName;
            }
        }

        [ReadOnly(true)]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "OutputProperties_Parameter_DisplayName")]
        [ResourcesDescription(typeof(Resources), "OutputProperties_Parameter_Description")]
        public string Parameter
        {
            get
            {
                return data.ParameterName;
            }
        }

        [ReadOnly(true)]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "OutputProperties_Unit_DisplayName")]
        [ResourcesDescription(typeof(Resources), "OutputProperties_Unit_Description")]
        public string Unit
        {
            get
            {
                return data.UnitName;
            }
        }
    }
}