using DelftTools.Utils;
using GeoAPI.Geometries;

namespace DelftTools.Hydro.SewerFeatures
{
    public interface INwrwFeature: INameable
    {
        void SetGeometry(IGeometry geometry);
        void AddNwrwCatchmentModelDataToModel(IHydroModel model);
    }
}