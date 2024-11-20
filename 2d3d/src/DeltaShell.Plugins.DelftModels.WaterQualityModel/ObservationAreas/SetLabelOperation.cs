using System;
using System.ComponentModel;
using System.Linq;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas
{
    public enum PointwiseOperationType
    {
        [Description("Overwrite")]
        Overwrite = 0,

        [Description("Overwrite where missing")]
        OverwriteWhereMissing = 1
    }

    public class SetLabelOperation : SpatialOperation
    {
        private PointwiseOperationType operationType;
        private string labelToSet;

        public SetLabelOperation()
        {
            Inputs.Add(new SpatialOperationData(this)
            {
                Name = MainInputName,
                FeatureType = typeof(ICoverage)
            });
            Inputs.Add(new SpatialOperationData(this)
            {
                Name = MaskInputName,
                FeatureType = typeof(IFeature)
            });
        }

        /// <summary>
        /// The label is used to set information on the observation area coverage.
        /// Which area does this part belong to?
        /// </summary>
        public virtual string Label
        {
            get => labelToSet;
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

        /// <summary>
        /// Marks the type of operation being performed when calling <see cref="SpatialOperation.Execute"/>.
        /// </summary>
        public virtual PointwiseOperationType OperationType
        {
            get => operationType;
            set
            {
                operationType = value;
                SetDirty();
            }
        }

        public override string ToString()
        {
            return Name + " : " + Label;
        }

        protected override void OnExecute()
        {
            var filter = new FeatureProviderMask
            {
                Polygons = Polygons.ToList(),
                Points = Points.ToList()
            };

            var coverage = (WaterQualityObservationAreaCoverage) MainInput.Provider.GetFeature(0);

            var clone = (WaterQualityObservationAreaCoverage) coverage.Clone();
            object noDataValue = clone.Components[0].NoDataValue;

            // first add the label, because ints are set in the FeatureProviderMask and SetValuesAsLabels isn't used.
            int index = clone.AddLabel(Label);

            filter.ApplyToCoverage<int>(clone, (c, v) => Evaluate(v, index, (int) noDataValue, OperationType));
            Output.Provider = new CoverageFeatureProvider
            {
                Coverage = clone,
                CoordinateSystem = CoordinateSystem
            };
        }

        /// <summary>
        /// Determines the new value for a given original and the value of the mask being
        /// applying a particular operation.
        /// </summary>
        /// <param name="originalValue">
        /// The original value for which a new value should be
        /// determined.
        /// </param>
        /// <param name="maskValue"> The value associated with the mask. </param>
        /// <param name="noDataValue"> The value representing the absence of data. </param>
        /// <param name="operationType"> Type of the operation. </param>
        /// <returns> The new value that should replace <paramref name="originalValue"/>. </returns>
        private static int Evaluate(int originalValue, int maskValue, int noDataValue,
                                    PointwiseOperationType operationType)
        {
            switch (operationType)
            {
                case PointwiseOperationType.Overwrite:
                    return maskValue;
                case PointwiseOperationType.OverwriteWhereMissing:
                    return originalValue == noDataValue ? maskValue : originalValue;
                default:
                    throw new NotImplementedException("Operation not supported");
            }
        }
    }
}