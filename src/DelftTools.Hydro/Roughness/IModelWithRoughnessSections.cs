using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
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
