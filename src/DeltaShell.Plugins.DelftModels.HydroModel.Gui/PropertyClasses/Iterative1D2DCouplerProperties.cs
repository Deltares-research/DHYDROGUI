using DelftTools.Utils;
using DeltaShell.Plugins.CommonTools.Gui.Property;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.PropertyClasses
{
    [ResourcesDisplayName(typeof (Resources), "Iterative1D2DCouplerProperties_DisplayName")]
    public class Iterative1D2DCouplerProperties : CompositeActivityProperties
    {
        private Iterative1D2DCouplerData CouplerData { get { return (Iterative1D2DCouplerData)((Iterative1D2DCoupler)data).Data; } }

        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Iterative1D2DCoupler_MaxIteration")]
        [ResourcesDescription(typeof(Resources), "Iterative1D2DCoupler_MaxIteration_Description")]
        public int MaxIteration
        {
            get { return CouplerData.MaxIteration; }
            set { CouplerData.MaxIteration = value; }
        }
        
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Iterative1D2DCoupler_MaxError")]
        [ResourcesDescription(typeof(Resources), "Iterative1D2DCoupler_MaxError_Description")]
        public double MaxError
        {
            get { return CouplerData.MaxError; }
            set { CouplerData.MaxError = value; }
        }

        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Iterative1D2DCoupler_CreateDebugMessages")]
        [ResourcesDescription(typeof(Resources), "Iterative1D2DCoupler_CreateDebugMessages_Description")]
        public bool CreateDebugMessages
        {
            get { return CouplerData.Debug; }
            set { CouplerData.Debug = value; }
        }

        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDisplayName(typeof(Resources), "Iterative1D2DCoupler_Refresh1d2dLinks")]
        [ResourcesDescription(typeof(Resources), "Iterative1D2DCoupler_Refresh1d2dLinks_Description")]
        public bool Refresh1d2dLinks
        {
            get { return CouplerData.Refresh1D2DLinks; }
            set { CouplerData.Refresh1D2DLinks = value; }
        }
    }
}