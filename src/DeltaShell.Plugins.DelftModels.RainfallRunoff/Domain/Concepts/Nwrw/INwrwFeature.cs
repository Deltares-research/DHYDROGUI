using DelftTools.Hydro;
using DelftTools.Utils;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public interface INwrwFeature : INameable
    {
        IGeometry Geometry { get; set; }
        void AddNwrwCatchmentModelDataToModel(IHydroModel model);
    }
}