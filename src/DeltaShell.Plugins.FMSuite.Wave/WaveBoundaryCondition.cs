using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils.Aop;
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
        public IDictionary<int, string> SpectrumFiles { get; set; }
        public IDictionary<int, WaveBoundaryParameters> SpectrumParameters { get; set; }

        private WaveBoundaryConditionSpatialDefinitionType spatialDefinitionType;
        public WaveBoundaryConditionSpatialDefinitionType SpatialDefinitionType
        {
            get { return spatialDefinitionType; }
            set
            {
                if (spatialDefinitionType == value) return;

                spatialDefinitionType = value;

                ClearData();
            }
        }

        public Coordinate StartCoordinate
        {
            get
            {
                return Feature.Geometry.Coordinates[0];
            }
        }

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
                SpectrumParameters[0] = new WaveBoundaryParameters();
            }

            EndEdit();
        }

        public override string ProcessName
        {
            get { return WaveProcessName; }
        }

        public override string VariableName
        {
            get { return WaveQuantityName; }
        }

        public override string VariableDescription
        {
            get { return "Wave Energy Density"; }
        }

        public override IUnit VariableUnit
        {
            get { return new Unit("mHummel"); }
        }

        public override bool IsHorizontallyUniform
        {
            get { return spatialDefinitionType == WaveBoundaryConditionSpatialDefinitionType.Uniform; }
        }

        public override bool IsVerticallyUniform
        {
            get { return true; }
        }
        
        public override void AddPoint(int i)
        {
            if (DataType == BoundaryConditionDataType.SpectrumFromFile)
            {
                SpectrumFiles[i] = "";
            }
            if (DataType == BoundaryConditionDataType.ParametrizedSpectrumConstant)
            {
                SpectrumParameters[i] = new WaveBoundaryParameters();
            }
            base.AddPoint(i);
        }

        public override void RemovePoint(int i)
        {
            if (DataType == BoundaryConditionDataType.SpectrumFromFile)
            {
                SpectrumFiles.Remove(i);
            }
            if (DataType == BoundaryConditionDataType.ParametrizedSpectrumConstant)
            {
                SpectrumParameters.Remove(i);
            }
            base.RemovePoint(i);
        }

        public override int VariableDimension
        {
            get { return 1; }
        }
        
        protected override IFunction CreateFunction()
        {
            switch (DataType)
            {
                case BoundaryConditionDataType.ParametrizedSpectrumTimeseries:
                    return CreateEmptyWaveEnergyFunction();
                case BoundaryConditionDataType.ParametrizedSpectrumConstant:
                case BoundaryConditionDataType.SpectrumFromFile:
                    return new Function("dummy");
                default:
                    return base.CreateFunction();
            }
        }

        public static IFunction CreateEmptyWaveEnergyFunction()
        {
            // todo: add as variable name definition
            var function = new Function(WaveQuantityName);
            function.Arguments.Add(new Variable<DateTime>("Time"));
            function.Components.Add(new Variable<double>("Hs", new Unit("meter", "m")));
            function.Components.Add(new Variable<double>("Tp", new Unit("second", "s")));
            function.Components.Add(new Variable<double>("Dir", new Unit("degree", "deg")));
            function.Components.Add(new Variable<double>("Spreading", new Unit("", "-")));

            function.Attributes[BcwFile.TimeFunctionAttributeName] = "non-equidistant";
            function.Attributes[BcwFile.RefDateAttributeName] = new DateTime().ToString(BcwFile.DateFormatString);
            function.Attributes[BcwFile.TimeUnitAttributeName] = "minutes";

            return function;
        }

        // TODO: Somehow you cannot call this methods for a second time if you haven't cleared the index. Function and Variable will throw an exception. This should be resolved in the framework.
        public void SetTimeseriesToSupportPoint(int dataPointIndex, IFunction f)
        {
            AddPoint(dataPointIndex);

            var func = GetDataAtPoint(dataPointIndex);
            func.Arguments[0].SetValues(f.Arguments[0].GetValues());
            for (int j = 0; j < func.Components.Count; ++j)
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
            get { return Feature.Geometry.Centroid; }
            set { throw new Exception("Cannot move wave boundary condition"); }
        }

        public IFeatureAttributeCollection Attributes { get; set; }
    }
}