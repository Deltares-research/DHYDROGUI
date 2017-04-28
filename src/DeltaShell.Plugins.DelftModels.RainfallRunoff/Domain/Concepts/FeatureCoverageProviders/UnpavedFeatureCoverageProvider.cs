using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FeatureCoverageProviders;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.FeatureCoverageProviders
{
    public class UnpavedFeatureCoverageProvider : ConceptFeatureCoverageProvider<UnpavedData>
    {
        protected override IList<Expression<Func<double>>> Attributes { get; set; }

        public UnpavedFeatureCoverageProvider(RainfallRunoffModel model)
        {
            Model = model;

            var source = new UnpavedData(new Catchment());
            Attributes = new List<Expression<Func<double>>>
                {
                    () => source.GroundWaterLayerThickness,
                    () => source.InfiltrationCapacity, //todo: use unit!!
                    () => source.InitialGroundWaterLevelConstant,
                    () => source.InitialLandStorage,
                    () => source.MaximumAllowedGroundWaterLevel,
                    () => source.MaximumLandStorage,
                    () => source.SeepageConstant,
                    () => source.SurfaceLevel,
                    () => source.TotalAreaForGroundWaterCalculations, //todo: check boolean
                };
        }

        protected override string ConceptName
        {
            get { return "Unpaved"; }
        }

    }
}