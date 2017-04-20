using System;
using DelftTools.Functions.Filters;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    public sealed class CatchmentCoverageMaintainer : ICatchmentCoverageMaintainer
    {
        private readonly ICatchmentModelDataSynchronizer synchronizer;

        public CatchmentCoverageMaintainer(RainfallRunoffModel model, ICatchmentModelDataSynchronizer synchronizer = null)
        {
            this.synchronizer = synchronizer ?? new CatchmentModelDataSynchronizer<CatchmentModelData>(model);
            this.synchronizer.OnAreaAddedOrModified = OnAreaAddedOrModified;
            this.synchronizer.OnAreaRemoved = OnAreaRemoved;
        }

        private IFeatureCoverage FeatureCoverage { get; set; }
        
        public void Initialize(IFeatureCoverage featureCoverage)
        {
            if (featureCoverage == null)
            {
                throw new ArgumentException("FeatureCoverage null");
            }
            FeatureCoverage = featureCoverage;
        }
        
        public void Cleanup()
        {
            FeatureCoverage = null;
            synchronizer.Disconnect();
        }

        private bool IsIncluded(CatchmentModelData area)
        {
            return FeatureCoverage.Features.Contains(area.Catchment);
        }

        private void OnAreaAddedOrModified(CatchmentModelData area)
        {
            if (IsIncluded(area))
            {
                return;
            }

            Catchment catchment = area.Catchment;
            FeatureCoverage.Features.Add(catchment);
            FeatureCoverage.FeatureVariable.Values.Add(catchment);
        }

        private void OnAreaRemoved(CatchmentModelData area)
        {
            if (!IsIncluded(area))
            {
                return;
            }

            Catchment catchment = area.Catchment;
            FeatureCoverage.Features.Remove(catchment);
            FeatureCoverage.FeatureVariable.RemoveValues(
                new VariableValueFilter<IFeature>(FeatureCoverage.FeatureVariable, catchment));
        }
    }
}