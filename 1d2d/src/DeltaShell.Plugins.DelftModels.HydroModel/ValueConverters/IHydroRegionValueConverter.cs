using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;

namespace DeltaShell.Plugins.DelftModels.HydroModel.ValueConverters
{
    public interface IHydroRegionValueConverter : IValueConverter
    {
        IHydroRegion HydroRegion { get; set; }
    }
}