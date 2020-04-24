using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Coverages;

namespace DelftTools.Hydro.Roughness
{
    public interface IModelWithRoughnessSections: IModel
    {
        IEventedList<RoughnessSection> RoughnessSections { get; }
        bool UseReverseRoughness { get; set; }
        bool UseReverseRoughnessInCalculation { get; set; }
    }

    public interface IModelWithNetwork : IModel
    {
        IHydroNetwork Network { get; set; }
        IDiscretization NetworkDiscretization { get; set; }

        void SubscribeToNetwork(IHydroNetwork network);
        void UnSubscribeFromNetwork(IHydroNetwork network);
        void SubscribeBoundaryConditions1D();
        void UnSubscribeBoundaryConditions1D();
        void SubscribeLateralSourcesData();
        void UnSubscribeLateralSourcesData();

    }
}
