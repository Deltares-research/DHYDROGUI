using System;
using System.Collections.Generic;
using System.ComponentModel;
using DelftTools.Units;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.SpectralData;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave
{
    public enum WaveBoundaryConditionSpatialDefinitionType
    {
        [Description("Uniform")]
        Uniform,

        [Description("Spatially Varying")]
        SpatiallyVarying
    }

    /// <summary>
    /// A class that represents boundary conditions that are used in wave models.
    /// </summary>
    /// <seealso cref="DeltaShell.Plugins.FMSuite.Common.FeatureData.BoundaryCondition" />
    /// <seealso cref="GeoAPI.Extensions.Feature.IFeature" />
    [Entity]
    public class WaveBoundaryCondition : BoundaryCondition, IFeature
    {
        public const string WaveProcessName = "wave";
        public const string WaveQuantityName = "wave_energy_density";

        private readonly WaveBoundarySpectralData spectralData;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaveBoundaryCondition" /> class.
        /// </summary>
        /// <param name="bcDataType"> The data type of the wave boundary condition. </param>
        public WaveBoundaryCondition(BoundaryConditionDataType bcDataType) : base(bcDataType)
        {
            spectralData = new WaveBoundarySpectralData {PeakEnhancementFactor = 3.3};
            SpectrumFiles = new Dictionary<int, string>();
            SpectrumParameters = new Dictionary<int, WaveBoundaryParameters>();
        }

        public WaveDirectionalSpreadingType DirectionalSpreadingType => spectralData.DirectionalSpreadingType;

        private double GetDefaultSpreadingValue()
        {
            return DirectionalSpreadingType == WaveDirectionalSpreadingType.Power
                       ? 4.0
                       : 30.0;
        }

        public IDictionary<int, string> SpectrumFiles { get; }
        public IDictionary<int, WaveBoundaryParameters> SpectrumParameters { get; }

        private WaveBoundaryConditionSpatialDefinitionType spatialDefinitionType;

        protected override void AfterDataTypeChanged(BoundaryConditionDataType previousDataType)
        {
            ClearData();
        }

        private void ClearData()
        {
            BeginEdit(new DefaultEditAction("Clearing data"));
            SpectrumFiles.Clear();
            SpectrumParameters.Clear();
            PointData.Clear();
            DataPointIndices.Clear();
            PointDepthLayerDefinitions.Clear();

            // Uniform boundaries require to have exactly one point. This point is used to store the uniform data in.
            if (spatialDefinitionType == WaveBoundaryConditionSpatialDefinitionType.Uniform)
            {
                AddPoint(0);
                SpectrumFiles[0] = "";
            }

            EndEdit();
        }

        public override string ProcessName => WaveProcessName;

        public override string VariableName => WaveQuantityName;

        public override string VariableDescription => "Wave Energy Density";

        public override IUnit VariableUnit => new Unit("mHummel");

        public override bool IsHorizontallyUniform =>
            spatialDefinitionType == WaveBoundaryConditionSpatialDefinitionType.Uniform;

        public override bool IsVerticallyUniform => true;

        /// <summary>
        /// Adds a data point to the wave boundary condition and additionally adds related default values to it.
        /// </summary>
        /// <param name="i"> The index of the data point in <see cref="BoundaryCondition.DataPointIndices" />. </param>
        public override void AddPoint(int i)
        {
            if (DataType == BoundaryConditionDataType.SpectrumFromFile)
            {
                SpectrumFiles[i] = "";
            }

            if (DataType == BoundaryConditionDataType.ParameterizedSpectrumConstant)
            {
                SpectrumParameters[i] = new WaveBoundaryParameters {Spreading = GetDefaultSpreadingValue()};
            }

            base.AddPoint(i);
        }

        public override int VariableDimension => 1;

        public IGeometry Geometry
        {
            get => Feature.Geometry.Centroid;
            set => throw new Exception("Cannot move wave boundary condition");
        }

        public IFeatureAttributeCollection Attributes { get; set; }
    }
}