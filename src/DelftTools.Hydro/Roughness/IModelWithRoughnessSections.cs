using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Coverages;

namespace DelftTools.Hydro.Roughness
{
    public interface IModelWithRoughnessSections: IModel
    {
        IEventedList<RoughnessSection> RoughnessSections { get; }
        bool UseReverseRoughness { get; set; }
    }

    public interface IModelWithNetwork : IModel
    {
        IHydroNetwork Network { get; set; }
        IDiscretization NetworkDiscretization { get; set; }
    }
}
