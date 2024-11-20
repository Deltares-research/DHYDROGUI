using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers
{
    public abstract class ConceptModelController<TConceptData> : IConceptModelController where TConceptData : CatchmentModelData
    {
        public IRRModelHybridFileWriter Writer { get; set; }

        public IRainfallRunoffModelController RootController { get; set; }

        public bool CanHandle(CatchmentModelData area)
        {
            return area is TConceptData;
        }

        private readonly IList<Catchment> catchments = new List<Catchment>();
        
        public void OnInitializeFeatureCoverage(EngineParameter modelParameter, IFeatureCoverage featureCoverage)
        {
            RainfallRunoffModelController.AddFeaturesToFeatureCoverage(featureCoverage, catchments.ToList());
        }

        public void Reset()
        {
            catchments.Clear();
        }

        public void AddArea(IRainfallRunoffModel model, CatchmentModelData area, IList<ModelLink> links, IList<IFeature> allRRNodes)
        {
            allRRNodes.Add(area.Catchment);
            catchments.Add(area.Catchment);
            OnAddArea(model, (TConceptData)area, links);
        }

        protected string GetMeteoId(IRainfallRunoffModel model, CatchmentModelData catchmentModelData)
        {
            var rrModel = model as RainfallRunoffModel; 
            if (rrModel != null && rrModel.Precipitation.DataDistributionType == MeteoDataDistributionType.PerStation)
            {
                if (rrModel.MeteoStations.Contains(catchmentModelData.MeteoStationName))
                {
                    return catchmentModelData.MeteoStationName;
                }
                return rrModel.MeteoStations.FirstOrDefault() ?? "";
            }
            return catchmentModelData.Name;
        }

        protected double GetAreaAdjustmentFactor(IRainfallRunoffModel model, CatchmentModelData unpavedData)
        {
            var rrModel = model as RainfallRunoffModel;
            if (rrModel == null) 
            {
                return unpavedData.AreaAdjustmentFactor;
            }
            return rrModel.Precipitation.DataDistributionType == MeteoDataDistributionType.PerStation
                       ? unpavedData.AreaAdjustmentFactor
                       : 1.0;
        }

        public abstract bool CanHandle(ElementSet elementSet);
        protected abstract void OnAddArea(IRainfallRunoffModel model, TConceptData data, IList<ModelLink> links);
    }
}