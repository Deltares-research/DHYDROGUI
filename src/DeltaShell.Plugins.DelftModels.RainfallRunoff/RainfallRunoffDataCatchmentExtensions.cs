using System.Linq;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Polder;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    internal static class RainfallRunoffDataCatchmentExtensions
    {
        internal static CatchmentModelData CreateDefaultModelData(this Catchment catchment)
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
                case CatchmentType.NwrwTypeName:
                    return new NwrwData(catchment);
                default:
                    return null;
            }
        }

        internal static bool IsModelDataCompatible(this Catchment catchment, CatchmentModelData modelData)
        {
            //todo: implement this nicely, for now: 1-on-1 matches only
            var desiredModelData = CreateDefaultModelData(catchment);
            if (desiredModelData == null)
            {
                return modelData == null;
            }
            return modelData != null && modelData.GetType() == desiredModelData.GetType();
        }

        internal static void AddDefaultModelDataForCatchment(this Catchment catchment, RainfallRunoffModel rainfallRunoffModel, bool catchmentInBasin = false)
        {
            var catchmentModelData = catchment.CreateDefaultModelData();

            if (catchmentModelData == null)
                return;

            if (catchmentInBasin || rainfallRunoffModel.Basin.Catchments.Contains(catchment))
            {
                rainfallRunoffModel.ModelData.Add(catchmentModelData);
            }
            else
            {
                var parentCatchment = rainfallRunoffModel.Basin.AllCatchments.First(c => c.SubCatchments.Contains(catchment));
                var parentModelData = rainfallRunoffModel.GetCatchmentModelData(parentCatchment);
                parentModelData.SubCatchmentModelData.Add(catchmentModelData);
            }

            rainfallRunoffModel.FireModelDataAdded(catchmentModelData);
        }
    }
}