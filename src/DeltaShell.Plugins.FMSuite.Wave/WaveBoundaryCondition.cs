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

    [Entity]
    public class WaveBoundaryCondition : BoundaryCondition, IFeature
    {
        public const string WaveProcessName = "wave";
        public const string WaveQuantityName = "wave_energy_density";

        public WaveBoundaryCondition(BoundaryConditionDataType bcDataType) : base(bcDataType)
        {
            SpectralData = new WaveBoundarySpectralData
            {
                PeakEnhancementFactor = 3.3
            };
            SpectrumFiles = new Dictionary<int, string>();
            SpectrumParameters = new Dictionary<int, WaveBoundaryParameters>();
        }

        protected override void UpdateName()
        {
            // to avoid the name setting in FeatureData... 
        }
        
        public WaveBoundarySpectralData SpectralData { get; set; }

        public WaveDirectionalSpreadingType DirectionalSpreadingType
        {
            get => SpectralData.DirectionalSpreadingType;
            set
            {
                SpectralData.DirectionalSpreadingType = value;

                var defaultSpreadingValue = GetDefaultSpreadingValue(value);

                switch (DataType)
                {
                    case BoundaryConditionDataType.ParameterizedSpectrumConstant:
                        SpectrumParameters.Values.ForEach(parameters => parameters.Spreading = defaultSpreadingValue);
                        break;
                    case BoundaryConditionDataType.ParameterizedSpectrumTimeseries:
                        PointData.ForEach(function =>
                        {
                            UpdateSpreadingDefaultValue(function);
                            UpdateDirectionVariableUnit(function);
                        });
                        break;
                }
            }
        }

        private static double GetDefaultSpreadingValue(WaveDirectionalSpreadingType directionalSpreadingType)
        {
            return directionalSpreadingType == WaveDirectionalSpreadingType.Power ? 2.0 : 30.0;
        }

        public IDictionary<int, string> SpectrumFiles { get; }
        public IDictionary<int, WaveBoundaryParameters> SpectrumParameters { get; }

        private WaveBoundaryConditionSpatialDefinitionType spatialDefinitionType;
        public WaveBoundaryConditionSpatialDefinitionType SpatialDefinitionType
        {
            get => spatialDefinitionType;
            set
            {
                if (spatialDefinitionType == value) return;

                spatialDefinitionType = value;

                ClearData();
            }
        }

        public Coordinate StartCoordinate => Feature.Geometry.Coordinates[0];

        public Coordinate EndCoordinate
        {
            get
            {
                var nrOfCoordinates = Feature.Geometry.Coordinates.Length;
                return Feature.Geometry.Coordinates[nrOfCoordinates - 1];
            }
        }

        public double GetCondSpecAtDist(int dataPointIndex)
        {
            var coordinates = Feature.Geometry.Coordinates;
            return Enumerable.Range(1, dataPointIndex)
                             .Aggregate(0.0, (sum, i) => sum + coordinates[i].Distance(coordinates[i - 1]));
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

        public override bool IsHorizontallyUniform => spatialDefinitionType == WaveBoundaryConditionSpatialDefinitionType.Uniform;

        public override bool IsVerticallyUniform => true;

        public override void AddPoint(int i)
        {
            if (DataType == BoundaryConditionDataType.SpectrumFromFile)
            {
                SpectrumFiles[i] = "";
            }
            if (DataType == BoundaryConditionDataType.ParameterizedSpectrumConstant)
            {
                SpectrumParameters[i] = new WaveBoundaryParameters
                {
                    Spreading = GetDefaultSpreadingValue(DirectionalSpreadingType)
                };
            }
            base.AddPoint(i);
        }

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
                    var function = CreateEmptyWaveEnergyFunction();
                    function.Components.Where(c => c.Name == PeriodVariableName).ForEach(c => c.DefaultValue = 1.0);
                    UpdateSpreadingDefaultValue(function);
                    UpdateDirectionVariableUnit(function);
                    return function;
                case BoundaryConditionDataType.ParameterizedSpectrumConstant:
                case BoundaryConditionDataType.SpectrumFromFile:
                    return new Function("dummy");
                default:
                    return base.CreateFunction();
            }
        }

        private void UpdateSpreadingDefaultValue(IFunction function)
        {
            function.Components.Where(c => c.Name == SpreadingVariableName)
                .ForEach(c => c.DefaultValue = GetDefaultSpreadingValue(DirectionalSpreadingType));
        }

        private void UpdateDirectionVariableUnit(IFunction function)
        {
            function.Components.Where(c => c.Name == DirectionVariableName)
                .ForEach(c =>
                    c.Unit = new Unit(
                        DirectionalSpreadingType == WaveDirectionalSpreadingType.Degrees ? "degree" : "power",
                        DirectionalSpreadingType == WaveDirectionalSpreadingType.Degrees ? "deg" : "-"));
        }

        public const string TimeVariableName = "Time";
        public const string HeightVariableName = "Hs";
        public const string PeriodVariableName = "Tp";
        public const string DirectionVariableName = "Dir";
        public const string SpreadingVariableName = "Spreading";

        public static IFunction CreateEmptyWaveEnergyFunction()
        {
            // todo: add as variable name definition
            var function = new Function(WaveQuantityName);
            function.Arguments.Add(new Variable<DateTime>(TimeVariableName));
            function.Components.Add(new Variable<double>(HeightVariableName, new Unit("meter", "m")));
            function.Components.Add(new Variable<double>(PeriodVariableName, new Unit("second", "s")));
            function.Components.Add(new Variable<double>(DirectionVariableName, new Unit("degree", "deg")));
            function.Components.Add(new Variable<double>(SpreadingVariableName, new Unit("", "-")));

            function.Attributes[BcwFile.TimeFunctionAttributeName] = "non-equidistant";
            function.Attributes[BcwFile.RefDateAttributeName] = new DateTime().ToString(BcwFile.DateFormatString);
            function.Attributes[BcwFile.TimeUnitAttributeName] = "minutes";

            return function;
        }

        // TODO: Somehow you cannot call this methods for a second time if you haven't cleared the index. Function and Variable will throw an exception. This should be resolved in the framework.
        public void SetTimeSeriesToSupportPoint(int dataPointIndex, IFunction f)
        {
            AddPoint(dataPointIndex);

            var func = GetDataAtPoint(dataPointIndex);
            func.Arguments[0].SetValues(f.Arguments[0].GetValues());
            for (var j = 0; j < func.Components.Count; ++j)
            {
                func.Components[j].SetValues(f.Components[j].GetValues());
                func.Components[j].Unit = (IUnit) f.Components[j].Unit.Clone();
            }
            foreach (var att in f.Attributes)
            {
                func.Attributes[att.Key] = att.Value;
            }
        }

        public IGeometry Geometry
        {
            get => Feature.Geometry.Centroid;
            set => throw new Exception("Cannot move wave boundary condition");
        }

        public IFeatureAttributeCollection Attributes { get; set; }
    }
}