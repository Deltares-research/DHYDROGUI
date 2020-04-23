using DelftTools.Hydro.SewerFeatures;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;

namespace DeltaShell.Plugins.ImportExport.Gwsw
{
    public interface IGwswFeatureGenerator<T>
    {
        T Generate(GwswElement gwswElement);
    }
}