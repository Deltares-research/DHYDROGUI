using DelftTools.Hydro.Structures;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Plugins.ImportExport.Sobek.Builders.HydroTypeInitializers
{
    public interface IBridgeInitializer
    {
        void Initialize(SobekBridge sobekBridge, IBridge bridge);
    }
}