using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers.Concepts
{
    public class OpenWaterModelController : ConceptModelController<OpenWaterData>
    {
        private void AddOpenWater(IRainfallRunoffModel model, IRRModelHybridFileWriter writer, OpenWaterData openWaterData, IList<ModelLink> links)
        {
            string openWaterId = openWaterData.Catchment.Name;

            writer.AddOpenWater(openWaterId, openWaterData.CalculationArea,
                GetMeteoId(model, openWaterData),
                GetAreaAdjustmentFactor(model, openWaterData),openWaterData.Catchment?.InteriorPoint?.X ?? 0d, openWaterData.Catchment?.InteriorPoint?.Y ?? 0d );

            links.Add(RainfallRunoffModelController.CreateModelLink(openWaterData.Catchment));
        }

        public override bool CanHandle(ElementSet elementSet)
        {
            return elementSet == ElementSet.OpenWaterElmSet;
        }

        protected override void OnAddArea(IRainfallRunoffModel model, OpenWaterData data, IList<ModelLink> links)
        {
            AddOpenWater(model, Writer, data, links);
        }
    }
}