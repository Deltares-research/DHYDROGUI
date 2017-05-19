using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Editing;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData
{
    [TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
    public enum FlowBoundaryQuantityType
    {
        [Category("Flow")]
        [Description("Water level")]
        WaterLevel,
        [Category("Flow")]
        [Description("Velocity")]
        Velocity,
        [Category("Flow")]
        [Description("Discharge")]
        Discharge,
        [Category("Flow")]
        [Description("Riemann invariant")]
        Riemann,
        [Category("Flow")]
        [Description("Riemann velocity")]
        RiemannVelocity,
        [Category("Flow")]
        [Description("Neumann gradient")]
        Neumann,
        [Category("Flow")]
        [Description("Outflow")]
        Outflow,
        [Category("Flow")]
        [Description("Normal velocity")]
        NormalVelocity,
        [Category("Flow")]
        [Description("Tangential velocity")]
        TangentVelocity,
        [Category("Flow")]
        [Description("Velocity vector")]
        VelocityVector,
        [Category("Salinity")]
        [Description("Salinity")]
        Salinity,
        [Category("Temperature")]
        [Description("Temperature")]
        Temperature,
        [Category("Sediment concentration")]
        [Description("Sediment concentration")]
        SedimentConcentration,
        [Category("Morphology")]
        [Description("Bed level prescribed")]
        MorphologyBedLevelPrescribed,
        [Category("Morphology")]
        [Description("Bed level changed prescribed")]
        MorphologyBedLevelChangedPrescribed,
        [Category("Morphology")]
        [Description("Bed load transport")]
        MorphologyBedLoadTransport,
        [Category("Tracer")]
        [Description("Tracer")]
        Tracer,
    }

    [Entity]
    public class FlowBoundaryCondition : BoundaryCondition
    {
        /// <summary>
        /// Constrained BC combinations within a set. Quantities not appearing anywhere in these lists are considered to be unconstrained, 
        /// i.e. the user can combine these with any quantity. (e.g. SedimentConcentration)
        /// </summary>
        public static readonly IList<IList<FlowBoundaryQuantityType>> ValidBoundaryConditionCombinations = new[]
        {
        /*
            Note: When adding combinations here, also check the Morphology related combinations (below)
        */

            // WaterLevel & Combinations
            new[] {FlowBoundaryQuantityType.WaterLevel},
            new[] {FlowBoundaryQuantityType.WaterLevel, FlowBoundaryQuantityType.NormalVelocity},
            new[] {FlowBoundaryQuantityType.WaterLevel, FlowBoundaryQuantityType.TangentVelocity},
            new[] {FlowBoundaryQuantityType.WaterLevel, FlowBoundaryQuantityType.NormalVelocity, FlowBoundaryQuantityType.TangentVelocity},
            new[] {FlowBoundaryQuantityType.WaterLevel, FlowBoundaryQuantityType.VelocityVector},
            
            // Velocity & combinations
            new[] {FlowBoundaryQuantityType.Velocity},

            // Riemann & Combinations
            new[] {FlowBoundaryQuantityType.Riemann},
            new[] {FlowBoundaryQuantityType.Riemann, FlowBoundaryQuantityType.TangentVelocity},

            // RiemannVelocity & Combinations
            new[] {FlowBoundaryQuantityType.RiemannVelocity},

            // Neumann & Combinations
            new[] {FlowBoundaryQuantityType.Neumann},

            // Discharge & Combinations
            new[] {FlowBoundaryQuantityType.Discharge},

            // Outflow & Combinations
            new[] {FlowBoundaryQuantityType.Outflow},

            #region Morphology related combinations

        /*
            Note: Morphology related quantities are mutually exclusive (i.e. cannot be combined with each other)
                    The exception being SedimentConcentration which can be combined with Everything (hense it does not appear in these lists)
        */

            new[] {FlowBoundaryQuantityType.MorphologyBedLevelPrescribed},
            new[] {FlowBoundaryQuantityType.MorphologyBedLevelChangedPrescribed},
            new[] {FlowBoundaryQuantityType.MorphologyBedLoadTransport},

            // WaterLevel & Combinations          
            new[] {FlowBoundaryQuantityType.WaterLevel, FlowBoundaryQuantityType.MorphologyBedLevelPrescribed},
            new[] {FlowBoundaryQuantityType.WaterLevel, FlowBoundaryQuantityType.NormalVelocity, FlowBoundaryQuantityType.MorphologyBedLevelPrescribed},
            new[] {FlowBoundaryQuantityType.WaterLevel, FlowBoundaryQuantityType.TangentVelocity, FlowBoundaryQuantityType.MorphologyBedLevelPrescribed},
            new[] {FlowBoundaryQuantityType.WaterLevel, FlowBoundaryQuantityType.NormalVelocity, FlowBoundaryQuantityType.TangentVelocity, FlowBoundaryQuantityType.MorphologyBedLevelPrescribed},
            new[] {FlowBoundaryQuantityType.WaterLevel, FlowBoundaryQuantityType.VelocityVector, FlowBoundaryQuantityType.MorphologyBedLevelPrescribed},

            new[] {FlowBoundaryQuantityType.WaterLevel, FlowBoundaryQuantityType.MorphologyBedLevelChangedPrescribed},
            new[] {FlowBoundaryQuantityType.WaterLevel, FlowBoundaryQuantityType.NormalVelocity, FlowBoundaryQuantityType.MorphologyBedLevelChangedPrescribed},
            new[] {FlowBoundaryQuantityType.WaterLevel, FlowBoundaryQuantityType.TangentVelocity, FlowBoundaryQuantityType.MorphologyBedLevelChangedPrescribed},
            new[] {FlowBoundaryQuantityType.WaterLevel, FlowBoundaryQuantityType.NormalVelocity, FlowBoundaryQuantityType.TangentVelocity, FlowBoundaryQuantityType.MorphologyBedLevelChangedPrescribed},
            new[] {FlowBoundaryQuantityType.WaterLevel, FlowBoundaryQuantityType.VelocityVector, FlowBoundaryQuantityType.MorphologyBedLevelChangedPrescribed},

            new[] {FlowBoundaryQuantityType.WaterLevel, FlowBoundaryQuantityType.MorphologyBedLoadTransport},
            new[] {FlowBoundaryQuantityType.WaterLevel, FlowBoundaryQuantityType.NormalVelocity, FlowBoundaryQuantityType.MorphologyBedLoadTransport},
            new[] {FlowBoundaryQuantityType.WaterLevel, FlowBoundaryQuantityType.TangentVelocity, FlowBoundaryQuantityType.MorphologyBedLoadTransport},
            new[] {FlowBoundaryQuantityType.WaterLevel, FlowBoundaryQuantityType.NormalVelocity, FlowBoundaryQuantityType.TangentVelocity, FlowBoundaryQuantityType.MorphologyBedLoadTransport},
            new[] {FlowBoundaryQuantityType.WaterLevel, FlowBoundaryQuantityType.VelocityVector, FlowBoundaryQuantityType.MorphologyBedLoadTransport},

            // Velocity & combinations
            new[] {FlowBoundaryQuantityType.Velocity, FlowBoundaryQuantityType.MorphologyBedLevelPrescribed},
            new[] {FlowBoundaryQuantityType.Velocity, FlowBoundaryQuantityType.MorphologyBedLevelChangedPrescribed},
            new[] {FlowBoundaryQuantityType.Velocity, FlowBoundaryQuantityType.MorphologyBedLoadTransport},

            // Riemann & Combinations
            new[] {FlowBoundaryQuantityType.Riemann, FlowBoundaryQuantityType.MorphologyBedLevelPrescribed},
            new[] {FlowBoundaryQuantityType.Riemann, FlowBoundaryQuantityType.MorphologyBedLevelChangedPrescribed},
            new[] {FlowBoundaryQuantityType.Riemann, FlowBoundaryQuantityType.MorphologyBedLoadTransport},
            new[] {FlowBoundaryQuantityType.Riemann, FlowBoundaryQuantityType.TangentVelocity, FlowBoundaryQuantityType.MorphologyBedLevelPrescribed},
            new[] {FlowBoundaryQuantityType.Riemann, FlowBoundaryQuantityType.TangentVelocity, FlowBoundaryQuantityType.MorphologyBedLevelChangedPrescribed},
            new[] {FlowBoundaryQuantityType.Riemann, FlowBoundaryQuantityType.TangentVelocity, FlowBoundaryQuantityType.MorphologyBedLoadTransport},

            // RiemannVelocity & Combinations
            new[] {FlowBoundaryQuantityType.RiemannVelocity, FlowBoundaryQuantityType.MorphologyBedLevelPrescribed},
            new[] {FlowBoundaryQuantityType.RiemannVelocity, FlowBoundaryQuantityType.MorphologyBedLevelChangedPrescribed},
            new[] {FlowBoundaryQuantityType.RiemannVelocity, FlowBoundaryQuantityType.MorphologyBedLoadTransport},

            // Neumann & Combinations
            new[] {FlowBoundaryQuantityType.Neumann, FlowBoundaryQuantityType.MorphologyBedLevelPrescribed},
            new[] {FlowBoundaryQuantityType.Neumann, FlowBoundaryQuantityType.MorphologyBedLevelChangedPrescribed},
            new[] {FlowBoundaryQuantityType.Neumann, FlowBoundaryQuantityType.MorphologyBedLoadTransport},

            // Discharge & Combinations
            new[] {FlowBoundaryQuantityType.Discharge, FlowBoundaryQuantityType.MorphologyBedLevelPrescribed},
            new[] {FlowBoundaryQuantityType.Discharge, FlowBoundaryQuantityType.MorphologyBedLevelChangedPrescribed},
            new[] {FlowBoundaryQuantityType.Discharge, FlowBoundaryQuantityType.MorphologyBedLoadTransport},

            // Outflow & Combinations
            new[] {FlowBoundaryQuantityType.Outflow, FlowBoundaryQuantityType.MorphologyBedLevelPrescribed},
            new[] {FlowBoundaryQuantityType.Outflow, FlowBoundaryQuantityType.MorphologyBedLevelChangedPrescribed},
            new[] {FlowBoundaryQuantityType.Outflow, FlowBoundaryQuantityType.MorphologyBedLoadTransport},

            #endregion
        };

        public static readonly IList<FlowBoundaryQuantityType> AlwaysAllowedQuantities =
            Enum.GetValues(typeof (FlowBoundaryQuantityType))
                .Cast<FlowBoundaryQuantityType>()
                .Except(ValidBoundaryConditionCombinations.SelectMany(l => l).Distinct())
                .ToList();

        public static string GetVariableNameForQuantity(FlowBoundaryQuantityType flowQuantity)
        {
            return flowQuantity.ToString();
        }

        public static string GetDescription(FlowBoundaryQuantityType flowQuantity)
        {
            return EnumDescriptionAttributeTypeConverter.GetEnumDescription(flowQuantity);
        }

        public static string GetDescription(BoundaryConditionDataType boundaryConditionDataType)
        {
            return EnumDescriptionAttributeTypeConverter.GetEnumDescription(boundaryConditionDataType);
        }

        public static string GetDescription(FlowBoundaryQuantityType flowBoundaryQuantityType,
            BoundaryConditionDataType boundaryConditionDataType)
        {
            return GetDescription(flowBoundaryQuantityType) + " (" +
                   GetDescription(boundaryConditionDataType) + ")";
        }

        public static string GetProcessNameForQuantity(FlowBoundaryQuantityType flowQuantity)
        {
            var attributes =
                typeof (FlowBoundaryQuantityType).GetField(flowQuantity.ToString())
                                                 .GetCustomAttributes(typeof (CategoryAttribute), false)
                                                 .OfType<CategoryAttribute>()
                                                 .ToList();

            return attributes.Any() ? attributes.First().Category : "<none>";
        }

        public static double GetPeriodInMinutes(double frequencyInDegPerHour)
        {
            return frequencyInDegPerHour == 0 ? 0 : 60*360/frequencyInDegPerHour;
        }

        public static double GetFrequencyInDegPerHour(double periodInMinutes)
        {
            return periodInMinutes == 0 ? 0 : 60*360/periodInMinutes;
        }

        private VerticalInterpolationType verticalInterpolationType;
        private string tracerName;

        public FlowBoundaryCondition(FlowBoundaryQuantityType flowQuantity, BoundaryConditionDataType dataType) : base(dataType)
        {
            FlowQuantity = flowQuantity;
            Offset = 0;
            Factor = 1;
            ThatcherHarlemanTimeLag = TimeSpan.Zero;
            verticalInterpolationType = SupportedVerticalInterpolationTypes.First();
        }

        public FlowBoundaryQuantityType FlowQuantity { get; private set; }

        public double Offset { get; set; }

        public double Factor { get; set; }

        public TimeSpan ThatcherHarlemanTimeLag { get; set; }

        public bool SupportsReflection
        {
            get
            {
                return false;
            }
        }

        public IEnumerable<VerticalInterpolationType> SupportedVerticalInterpolationTypes
        {
            get
            {
                if (IsVerticallyUniform)
                {
                    yield return VerticalInterpolationType.Uniform;
                }
                else
                {
                    yield return VerticalInterpolationType.Linear;
                    yield return VerticalInterpolationType.Step;
                    yield return VerticalInterpolationType.Logarithmic;
                }
            }
        }

        [DisplayName("Reflection parameter")]
        public double ReflectionAlpha { get; set; }

        public IUnit ReflectionUnit
        {
            get
            {
                switch (FlowQuantity)
                {
                    case FlowBoundaryQuantityType.WaterLevel:
                        return new Unit("", "s²");
                    case FlowBoundaryQuantityType.Velocity:
                    case FlowBoundaryQuantityType.NormalVelocity:
                    case FlowBoundaryQuantityType.TangentVelocity:
                    case FlowBoundaryQuantityType.VelocityVector:
                    case FlowBoundaryQuantityType.Discharge:
                        return new Unit("time", "s");
                    default:
                        throw new ArgumentOutOfRangeException(String.Format("VariableName type {0} not supported",
                                                                            FlowQuantity));
                }
            }
        }

        //TODO: move the vertical profile, depends upon support point.
        public VerticalInterpolationType VerticalInterpolationType
        {
            get { return verticalInterpolationType; }
            set
            {
                if (SupportedVerticalInterpolationTypes.Contains(value))
                {
                    verticalInterpolationType = value;
                }
            }
        }

        public void KeepBottomLayer()
        {
            for (int i = 0; i < PointDepthLayerDefinitions.Count; ++i)
            {
                var numLayers = PointDepthLayerDefinitions[i].ProfilePoints;
                var data = PointData[i];
                var componentsToRemove = (numLayers - 1)*(data.Components.Count/numLayers);
                for (int j = 0; j < componentsToRemove; ++j)
                {
                    data.Components.RemoveAt(j);
                }
                PointDepthLayerDefinitions[i] = new VerticalProfileDefinition();
            }
        }

        public void KeepTopLayer()
        {
            for (int i = 0; i < PointDepthLayerDefinitions.Count; ++i)
            {
                PointDepthLayerDefinitions[i] = new VerticalProfileDefinition();
            }
        }

        /// <summary>
        /// The tracer name is only set when the <see cref="FlowQuantity"/> is set to <see cref="FlowBoundaryQuantityType.Tracer"/>.
        /// It is the postfix of tracer_{TracerName}.
        /// </summary>
        public string TracerName
        {
            get { return tracerName; }
            set 
            {
                tracerName = value;

                if(FlowQuantity == FlowBoundaryQuantityType.Tracer)
                {
                    foreach (var function in PointData)
                    {
                        function.BeginEdit(new DefaultEditAction("Tracer name"));

                        function.Components[0].Name = VariableName;
                        function.Name = VariableName;

                        function.EndEdit();
                    }
                }

                UpdateName();
            }
        }

        public string SedimentFractionName
        {
            get { return sedimentFractionName; }
            set
            {
                sedimentFractionName = value;
                if (FlowQuantity == FlowBoundaryQuantityType.SedimentConcentration)
                {
                    foreach (var function in PointData)
                    {
                        function.BeginEdit(new DefaultEditAction("Sediment concentration name"));

                        function.Components[0].Name = VariableName;
                        function.Name = VariableName;

                        function.EndEdit();
                    }
                }

                UpdateName();

            }
        }

        private string sedimentFractionName;

        public List<string> SedimentFractionNames { get; set; } /* Sediment fraction names are unique */

        public override string VariableName
        {
            get
            {
                if (FlowQuantity == FlowBoundaryQuantityType.Tracer)
                {
                    return TracerName;
                }
                else if (FlowQuantity == FlowBoundaryQuantityType.SedimentConcentration)
                {
                    return SedimentFractionName;
                }
                return GetVariableNameForQuantity(FlowQuantity);
            }
        }

        [FeatureAttribute(Order=3)]
        [ReadOnly(true)]
        [DisplayName("Quantity")]
        public override string VariableDescription
        {
            get
            {
                return FlowQuantity == FlowBoundaryQuantityType.Tracer ? VariableName : GetDescription(FlowQuantity);
            }
        }

        public override string Description
        {
            get { return GetDescription(FlowQuantity, DataType); }
        }

        public override IUnit VariableUnit
        {
            get
            {
                switch (FlowQuantity)
                {
                    case FlowBoundaryQuantityType.WaterLevel:
                        return new Unit("meters", "m");
                    case FlowBoundaryQuantityType.Velocity:
                    case FlowBoundaryQuantityType.Riemann:
                    case FlowBoundaryQuantityType.RiemannVelocity:
                    case FlowBoundaryQuantityType.NormalVelocity:
                    case FlowBoundaryQuantityType.TangentVelocity:
                    case FlowBoundaryQuantityType.VelocityVector:
                        return new Unit("meters per second", "m/s");
                    case FlowBoundaryQuantityType.Discharge:
                        return new Unit("cubic meters per second", "m³/s");
                    case FlowBoundaryQuantityType.Neumann:
                    case FlowBoundaryQuantityType.Outflow:
                    case FlowBoundaryQuantityType.Tracer:
                    case FlowBoundaryQuantityType.MorphologyBedLevelPrescribed:
                    case FlowBoundaryQuantityType.MorphologyBedLevelChangedPrescribed:
                    case FlowBoundaryQuantityType.MorphologyBedLoadTransport:
                        return new Unit("", "-");
                    case FlowBoundaryQuantityType.Salinity:
                        return new Unit("parts per trillion", "ppt");
                    case FlowBoundaryQuantityType.Temperature:
                        return new Unit("degree celsius", "°C");
                    case FlowBoundaryQuantityType.SedimentConcentration:
                        return new Unit("", "kg/m³");
                    default:
                        throw new ArgumentOutOfRangeException(String.Format("VariableName type {0} not supported",
                                                                            FlowQuantity));
                }
            }
        }

        public override int VariableDimension
        {
            get
            {
                return FlowQuantity == FlowBoundaryQuantityType.VelocityVector? 2: 1;
            }
        }

        [FeatureAttribute(Order=2)]
        [ReadOnly(true)]
        [DisplayName("Process")]
        public override string ProcessName
        {
            get { return GetProcessNameForQuantity(FlowQuantity); }
        }

        public override bool IsHorizontallyUniform
        {
            get
            {
                return FlowQuantity == FlowBoundaryQuantityType.Discharge ||
                       DataType == BoundaryConditionDataType.Qh;
            }
        }

        public override bool IsVerticallyUniform
        {
            get
            {
                if ((FlowQuantity == FlowBoundaryQuantityType.Salinity ||
                     FlowQuantity == FlowBoundaryQuantityType.Temperature ||
                     FlowQuantity == FlowBoundaryQuantityType.SedimentConcentration ||
                     FlowQuantity == FlowBoundaryQuantityType.VelocityVector) &&
                    DataType == BoundaryConditionDataType.TimeSeries) return false;
                return true;
            }
        }

        protected override IFunction CreateFunction()
        {
            if (DataType == BoundaryConditionDataType.Qh)
            {
                var function = new Function(VariableName);
                function.Arguments.Add(new Variable<double>("Q", new Unit("cubic meters", "m³/s")));
                function.Components.Add(new Variable<double>("h", new Unit("meters", "m"))
                {
                    NoDataValue = Double.NaN
                });

                return function;
            }

            if (FlowQuantity == FlowBoundaryQuantityType.MorphologyBedLoadTransport &&
                DataType == BoundaryConditionDataType.TimeSeries)
            {
                if (SedimentFractionNames.Count == 0)
                    return null;
                IFunction loadTransport = new Function(VariableName);

                loadTransport.Arguments.Add(new Variable<DateTime>("Time"));
                foreach (var fraction in SedimentFractionNames)
                {
                    AddSedimentFractionToFunction(loadTransport, fraction);
                }
                return loadTransport;

            }

            return base.CreateFunction();
        }

        public void AddSedimentFractionToFunction(IFunction loadTransport, string fraction)
        {
            if( ! SedimentFractionNames.Contains(fraction) )
                SedimentFractionNames.Add(fraction);
            loadTransport.Components.Add(new Variable<double>(fraction, VariableUnit)
            {
                NoDataValue = double.NaN
            });
        }

        public void RemoveSedimentFractionFromFunction(IFunction loadTransport, string fraction)
        {
            if (SedimentFractionNames.Contains(fraction))
                SedimentFractionNames.Remove(fraction);
            loadTransport.Components.RemoveAllWhere(fc => fc.Name.Equals(fraction));
        }

        protected override void FeaturePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(TypeUtils.GetMemberName<Feature2D>(f => f.Name)))
            {
                UpdateName();
            }
            else
            {
                base.FeaturePropertyChanged(sender, e);                
            }
        }

        protected override void UpdateName()
        {
            Name = Feature == null ? VariableDescription : Feature.Name + "-" + VariableDescription;
        }

        public static IEnumerable<BoundaryConditionDataType> GetSupportedDataTypesForQuantity(
            FlowBoundaryQuantityType flowBoundaryQuantityType)
        {
            yield return BoundaryConditionDataType.TimeSeries;
            if (flowBoundaryQuantityType == FlowBoundaryQuantityType.Tracer) yield break;
            if (flowBoundaryQuantityType == FlowBoundaryQuantityType.SedimentConcentration) yield break;
            if (flowBoundaryQuantityType == FlowBoundaryQuantityType.MorphologyBedLoadTransport
                || flowBoundaryQuantityType == FlowBoundaryQuantityType.MorphologyBedLevelPrescribed
                || flowBoundaryQuantityType == FlowBoundaryQuantityType.MorphologyBedLevelChangedPrescribed) yield break;
            yield return BoundaryConditionDataType.AstroComponents;
            yield return BoundaryConditionDataType.AstroCorrection;
            yield return BoundaryConditionDataType.Harmonics;
            yield return BoundaryConditionDataType.HarmonicCorrection;

            if (flowBoundaryQuantityType == FlowBoundaryQuantityType.WaterLevel)
            {
                yield return BoundaryConditionDataType.Qh;
            }
        }

        public bool StrictlyPositive
        {
            get
            {
                return FlowQuantity == FlowBoundaryQuantityType.Salinity ||
                       FlowQuantity == FlowBoundaryQuantityType.Temperature ||
                       FlowQuantity == FlowBoundaryQuantityType.SedimentConcentration ||
                       FlowQuantity == FlowBoundaryQuantityType.Tracer;
            }
        }

        public bool SupportsThatcherHarleman
        {
            get
            {
                return FlowQuantity == FlowBoundaryQuantityType.Salinity ||
                       FlowQuantity == FlowBoundaryQuantityType.Temperature ||
                       FlowQuantity == FlowBoundaryQuantityType.SedimentConcentration ||
                       FlowQuantity == FlowBoundaryQuantityType.Tracer;
            }
        }
    }
}
