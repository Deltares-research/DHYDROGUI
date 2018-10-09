using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Collections.Generic;

namespace DelftTools.Hydro.Roughness
{
    public interface IModelWithRoughnessSections: IModel
    {
        IEventedList<RoughnessSection> RoughnessSections { get; }
        bool UseReverseRoughness { get; set; }
        bool UseReverseRoughnessInCalculation { get; set; }
       
    }
}
