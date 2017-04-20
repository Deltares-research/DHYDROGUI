using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Polder;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    public class RainfallRunoffCatchmentModelDataFactory
    {
        public CatchmentModelData CreateDefaultModelData(Catchment catchment)
        {
            switch(catchment.CatchmentType.Name)
            {
                case CatchmentType.PolderTypeName:
                    return new PolderConcept(catchment);
                case CatchmentType.PavedTypeName:
                    return new PavedData(catchment);
                case CatchmentType.UnpavedTypeName:
                    return new UnpavedData(catchment);
                case CatchmentType.GreenhouseTypeName:
                    return new GreenhouseData(catchment);
                case CatchmentType.OpenwaterTypeName:
                    return new OpenWaterData(catchment);
                case CatchmentType.SacramentoTypeName:
                    return new SacramentoData(catchment);
                case CatchmentType.HbvTypeName:
                    return new HbvData(catchment);
                default:
                    return null;
            }
        }

        public bool IsModelDataCompatible(Catchment catchment, CatchmentModelData modelData)
        {
            //todo: implement this nicely, for now: 1-on-1 matches only
            var desiredModelData = CreateDefaultModelData(catchment);
            if (desiredModelData == null)
            {
                return modelData == null;
            }
            return modelData != null && modelData.GetType() == desiredModelData.GetType();
        }
    }
}