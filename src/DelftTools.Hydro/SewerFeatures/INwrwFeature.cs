using DelftTools.Utils;

namespace DelftTools.Hydro.SewerFeatures
{
    public interface INwrwFeature: INameable
    {
        void AddNwrwCatchmentModelDataToModel(IHydroModel model);
    }
}