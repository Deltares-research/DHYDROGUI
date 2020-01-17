using DelftTools.Hydro;
using DelftTools.Utils;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public interface INwrwFeature : INameable
    {
        void SetGeometry(NwrwData nwrwData, IGeometry geometry);
        void AddNwrwCatchmentModelDataToModel(IHydroModel model);
    }
}