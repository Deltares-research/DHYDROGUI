using System;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using log4net;
using SharpMap;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas
{
    public class OverwriteLabelOperation : SpatialOperation
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (OverwriteValueOperation));
        private string labelToSet;
        private double x;
        private double y;

        public OverwriteLabelOperation()
        {
            Inputs.Add(new SpatialOperationData(this){Name = MainInputName, FeatureType = typeof(ICoverage)});
        }

        public virtual double X
        {
            get { return x; }
            set
            {
                x = value; 
                SetDirty();
            }
        }

        public virtual double Y
        {
            get { return y; }
            set
            {
                y = value;
                SetDirty();
            }
        }

        public virtual string Label
        {
            get { return labelToSet; }
            set
            {
                if (value != null)
                {
                    value = value.ToLowerInvariant();
                }

                labelToSet = value;
                SetDirty();
            }
        }

        public virtual ICoordinateSystem InputCoordinateSystem { get; set; }

        protected override void OnExecute()
        {
            var coverage = MainInput.Provider.GetFeature(0) as WaterQualityObservationAreaCoverage;
            if (coverage != null)
            {
                var clone = (WaterQualityObservationAreaCoverage)coverage.Clone();
                int indexOfLabel = clone.AddLabel(Label);

                var x = X;
                var y = Y;
                if (InputCoordinateSystem != null && CoordinateSystem != null &&
                    InputCoordinateSystem != CoordinateSystem)
                {
                    var transform = Map.CoordinateSystemFactory.CreateTransformation(InputCoordinateSystem,
                        CoordinateSystem);
                    if (transform != null)
                    {
                        try
                        {
                            var tranformedComponents = transform.MathTransform.Transform(new[] {X, Y});
                            x = tranformedComponents[0];
                            y = tranformedComponents[1];
                        }
                        catch (Exception e)
                        {
                            Log.ErrorFormat(
                                "Could not apply coordinate transformation from {0} to {1} to point ({2},{3}): {4}",
                                InputCoordinateSystem.Name, CoordinateSystem.Name, X, Y, e.Message);
                            Output.Provider = new CoverageFeatureProvider
                            {
                                Coverage = clone,
                                CoordinateSystem = CoordinateSystem
                            };
                            return;
                        }
                    }
                }
                FeatureProviderMask.SetNearestPointValue(clone, new Coordinate(x, y), indexOfLabel, CoordinateSystem);
                Output.Provider = new CoverageFeatureProvider {Coverage = clone, CoordinateSystem = CoordinateSystem};
            }
        }

        public override string ToString()
        {
            return Name + " : " + Label;
        }
    }
}