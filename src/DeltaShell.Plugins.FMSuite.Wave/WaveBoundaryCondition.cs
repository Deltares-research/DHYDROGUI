using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.SpectralData;
using DeltaShell.Plugins.FMSuite.Wave.IO;
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

        protected override void UpdateName()
        {
            // to avoid the name setting in FeatureData... 
        }

        public double PeakEnhancementFactor
        {
            get => spectralData.PeakEnhancementFactor;
            set => spectralData.PeakEnhancementFactor = value;
        }

        public WaveDirectionalSpreadingType DirectionalSpreadingType
        {
            get => spectralData.DirectionalSpreadingType;
            set
            {
                spectralData.DirectionalSpreadingType = value;

                switch (DataType)
                {
                    case BoundaryConditionDataType.ParameterizedSpectrumConstant:
                        double defaultSpreadingValue = GetDefaultSpreadingValue();
                        SpectrumParameters.Values.ForEach(parameters => parameters.Spreading = defaultSpreadingValue);
                        break;
                    case BoundaryConditionDataType.ParameterizedSpectrumTimeseries:
                        PointData.ForEach(function =>
                        {
                            UpdateSpreadingComponentDefaultValue(function);
                            UpdateDirectionComponentVariableUnit(function);
                        });
                        break;
                }
            }
        }

        private double GetDefaultSpreadingValue()
        {
            return DirectionalSpreadingType == WaveDirectionalSpreadingType.Power
                       ? 4.0
                       : 30.0;
        }

        public IDictionary<int, string> SpectrumFiles { get; }
        public IDictionary<int, WaveBoundaryParameters> SpectrumParameters { get; }

        private WaveBoundaryConditionSpatialDefinitionType spatialDefinitionType;

        public WaveBoundaryConditionSpatialDefinitionType SpatialDefinitionType
        {
            get => spatialDefinitionType;
            set
            {
                if (spatialDefinitionType == value)
                {
                    return;
                }

                spatialDefinitionType = value;
                ClearData();
            }
        }

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

        /// <summary>
        /// Removes the point at index
        /// <param name="i" />
        /// and removes all related data to it.
        /// </summary>
        /// <param name="i"> The index of the data point in <see cref="BoundaryCondition.DataPointIndices" />. </param>
        public override void RemovePoint(int i)
        {
            if (DataType == BoundaryConditionDataType.SpectrumFromFile)
            {
                SpectrumFiles.Remove(i);
            }

            if (DataType == BoundaryConditionDataType.ParameterizedSpectrumConstant)
            {
                SpectrumParameters.Remove(i);
            }

            base.RemovePoint(i);
        }

        public override int VariableDimension => 1;

        protected override IFunction CreateFunction()
        {
            switch (DataType)
            {
                case BoundaryConditionDataType.ParameterizedSpectrumTimeseries:
                    IFunction function = CreateEmptyWaveEnergyFunction();
                    function.Components.Where(c => c.Name == PeriodVariableName).ForEach(c => c.DefaultValue = 1.0);
                    UpdateSpreadingComponentDefaultValue(function);
                    UpdateDirectionComponentVariableUnit(function);
                    return function;
                case BoundaryConditionDataType.ParameterizedSpectrumConstant:
                case BoundaryConditionDataType.SpectrumFromFile:
                    return new Function("dummy");
                default:
                    return base.CreateFunction();
            }
        }

        private void UpdateSpreadingComponentDefaultValue(IFunction function)
        {
            function.Components.Where(c => c.Name == SpreadingVariableName)
                    .ForEach(c => c.DefaultValue = GetDefaultSpreadingValue());
        }

        private void UpdateDirectionComponentVariableUnit(IFunction function)
        {
            function.Components.Where(c => c.Name == DirectionVariableName)
                    .ForEach(c =>
                                 c.Unit = new Unit(
                                     DirectionalSpreadingType == WaveDirectionalSpreadingType.Degrees
                                         ? DegreesUnitName
                                         : PowerUnitName,
                                     DirectionalSpreadingType == WaveDirectionalSpreadingType.Degrees
                                         ? DegreesUnitSymbol
                                         : PowerUnitSymbol));
        }

        public const string TimeVariableName = "Time";
        public const string HeightVariableName = "Hs";
        public const string PeriodVariableName = "Tp";
        public const string DirectionVariableName = "Dir";
        public const string SpreadingVariableName = "Spreading";

        private const string DegreesUnitName = "degrees";
        public const string DegreesUnitSymbol = "deg";
        private const string PowerUnitName = "power";
        public const string PowerUnitSymbol = "-";

        public static IFunction CreateEmptyWaveEnergyFunction()
        {
            // todo: add as variable name definition
            var function = new Function(WaveQuantityName);
            function.Arguments.Add(new Variable<DateTime>(TimeVariableName));
            function.Components.Add(new Variable<double>(HeightVariableName, new Unit("meter", "m")));
            function.Components.Add(new Variable<double>(PeriodVariableName, new Unit("second", "s")));
            function.Components.Add(
                new Variable<double>(DirectionVariableName, new Unit(DegreesUnitName, DegreesUnitSymbol)));
            function.Components.Add(new Variable<double>(SpreadingVariableName, new Unit("", "-")));

            function.Attributes[BcwFile.TimeFunctionAttributeName] = "non-equidistant";
            function.Attributes[BcwFile.RefDateAttributeName] = new DateTime().ToString(BcwFile.DateFormatString);
            function.Attributes[BcwFile.TimeUnitAttributeName] = "minutes";

            return function;
        }

        public IGeometry Geometry
        {
            get => Feature.Geometry.Centroid;
            set => throw new Exception("Cannot move wave boundary condition");
        }

        public IFeatureAttributeCollection Attributes { get; set; }
    }
}