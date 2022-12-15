using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers.Concepts
{
    public class GreenhouseModelController : ConceptModelController<GreenhouseData>
    {
        private void AddGreenhouse(IRainfallRunoffModel model, IRRModelHybridFileWriter writer, GreenhouseData greenhouseData, IList<ModelLink> links)
        {
            string greenhouseId = greenhouseData.Name;
            Writer = writer;
            Writer.AddGreenhouse(greenhouseId, greenhouseData.AreaPerGreenhouse.Values.ToArray(),
                greenhouseData.SurfaceLevel, greenhouseData.InitialRoofStorage,
                greenhouseData.MaximumRoofStorage, greenhouseData.SiloCapacity,
                greenhouseData.PumpCapacity, greenhouseData.UseSubsoilStorage,
                greenhouseData.SubSoilStorageArea, GetMeteoId(model, greenhouseData), GetAreaAdjustmentFactor(model, greenhouseData), greenhouseData.Catchment?.InteriorPoint?.X ?? 0d, greenhouseData.Catchment?.InteriorPoint?.Y ?? 0d);

            links.Add(RainfallRunoffModelController.CreateModelLink(greenhouseData.Catchment));
        }
        
        public override bool CanHandle(ElementSet elementSet)
        {
            return elementSet == ElementSet.GreenhouseElmSet;
        }

        protected override void OnAddArea(IRainfallRunoffModel model, GreenhouseData data, IList<ModelLink> links)
        {
            AddGreenhouse(model, Writer, data, links);
        }
    }
}