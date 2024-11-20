using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers.Concepts
{
    /// <summary>
    /// Use this class to fill nwrw output coverages with (nwrw) catchment features
    /// </summary>
    public class NwrwModelController : ConceptModelController<NwrwData>
    {
        public override bool CanHandle(ElementSet elementSet)
        {
            return elementSet == ElementSet.NWRWElmSet;
        }

        protected override void OnAddArea(IRainfallRunoffModel model, NwrwData data, IList<ModelLink> links)
        {
            //Nwrw is written differently
        }
    }
}