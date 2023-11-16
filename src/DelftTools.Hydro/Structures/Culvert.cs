using System;
using System.Collections.Generic;
using System.ComponentModel;
using DelftTools.Functions;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures.SteerableProperties;
using DelftTools.Utils.Aop;
using DelftTools.Utils.ComponentModel;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro.Structures
{
    ///<summary>
    /// <see cref="Culvert"/> provides the <see cref="ICulvert"/> structure which
    /// can be placed on a branch.
    ///</summary>
    [Entity(FireOnCollectionChange=false)]
    public class Culvert : BranchStructure, ICulvert, IHasSteerableProperties
    {
        private CrossSectionDefinitionZW tabulatedCrossSectionDefinition;
        private ICrossSectionDefinition crossSectionDefinition;
        private CulvertGeometryType geometryType;
        private CulvertType culvertType;

        /// <summary>
        /// Creates a new <see cref="Culvert"/> with a default name.
        /// </summary>
        public Culvert() : this("Culvert") { }

        /// <summary>
        /// Creates a new <see cref="Culvert"/> with the specified <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the culvert.</param>
        public Culvert(string name)
        {
            base.Name = name;
            
            GateOpeningLossCoefficientFunction = 
                FunctionHelper.Get1DFunction<double, double>("gatereduction", 
                                                             "gate height opening factor", 
                                                             "loss efficient");
            
            Width = 1.0;
            Height = 1.0;
            
            TabulatedCrossSectionDefinition = new CrossSectionDefinitionZW {IsClosed = true};
            GeometryType = CulvertGeometryType.Round;
            CulvertType = CulvertType.Culvert;
            BendLossCoefficient = 1.0;

            gateInitialOpening = new SteerableProperty(1.0, 
                                                       "Valve Opening Height",
                                                       "Valve Opening Height",
                                                       "m");
        }

        [DisplayName("Flow direction")]
        [FeatureAttribute(Order = 16,ExportName = "FlowDir")]
        public virtual FlowDirection FlowDirection { get; set; }

        /// <summary>
        /// Calculates the cross-section as defined at the inlet
        /// </summary>
        public virtual CrossSectionDefinitionZW CrossSectionDefinitionAtInletAbsolute => 
            CrossSectionDefinitionForCalculation.AddLevel(InletLevel);

        public virtual CrossSectionDefinitionZW CrossSectionDefinitionAtOutletAbsolute => 
            CrossSectionDefinitionForCalculation.AddLevel(OutletLevel);

        /// <summary>
        /// Crosssection as used for model api. It does not include any level since these are passed separately
        /// </summary>
        public virtual CrossSectionDefinitionZW CrossSectionDefinitionForCalculation
        {
            get
            {
                switch (GeometryType)
                {
                    case CulvertGeometryType.Tabulated:
                        return TabulatedCrossSectionDefinition;
                    case CulvertGeometryType.Rectangle:
                        return StandardCrossSectionsFactory.GetTabulatedCrossSectionFromRectangle(Width, Height);
                    case CulvertGeometryType.Round:
                        return StandardCrossSectionsFactory.GetTabulatedCrossSectionFromEllipse(Diameter, Diameter);
                    case CulvertGeometryType.Ellipse:
                        return StandardCrossSectionsFactory.GetTabulatedCrossSectionFromEllipse(Width, Height);
                    case CulvertGeometryType.Egg:
                        return StandardCrossSectionsFactory.GetTabulatedCrossSectionFromEgg(Width);
                    case CulvertGeometryType.InvertedEgg:
                        return StandardCrossSectionsFactory.GetTabulatedCrossSectionFromInvertedEgg(Width);
                    case CulvertGeometryType.Arch:
                        return StandardCrossSectionsFactory.GetTabulatedCrossSectionFromArch(Width, Height, ArcHeight);
                    case CulvertGeometryType.Cunette:
                        return StandardCrossSectionsFactory.GetTabulatedCrossSectionFromCunette(Width, Height);
                    case CulvertGeometryType.UShape:
                        return StandardCrossSectionsFactory.GetTabulatedCrossSectionFromUShape(Width, Height,ArcHeight);
                    case CulvertGeometryType.SteelCunette:
                        return StandardCrossSectionsFactory.GetTabulatedCrossSectionFromSteelCunette(Height, Radius, Radius1,
                                            Radius2, Radius3, Angle, Angle1);
                    default:
                        throw new InvalidOperationException(
                            "A crossection is only defined if geometry type is tabulated or rectangle");
                }
            }
        }

        /// <summary>
        /// Crosssection as used for model api. It does not include any level since these are passed separately
        /// </summary>
        public virtual ICrossSectionDefinition CrossSectionDefinition
        {
            get
            {
                UpdateCrossSectionDefinition(geometryType);
                return crossSectionDefinition;
            }
            set
            {
                switch (value)
                {
                    case null:
                        UpdateCrossSectionDefinition(geometryType);
                        break;
                    case CrossSectionDefinitionStandard definitionStandard:
                        switch (definitionStandard.ShapeType)
                        {
                            case CrossSectionStandardShapeType.Rectangle:
                                Width = ((CrossSectionStandardShapeRectangle)definitionStandard.Shape).Width;
                                Height = ((CrossSectionStandardShapeRectangle)definitionStandard.Shape).Height;
                                GeometryType = CulvertGeometryType.Rectangle;
                                break;
                            case CrossSectionStandardShapeType.Arch:
                                Width = ((CrossSectionStandardShapeArch)definitionStandard.Shape).Width;
                                Height = ((CrossSectionStandardShapeArch)definitionStandard.Shape).Height;
                                ArcHeight = ((CrossSectionStandardShapeArch)definitionStandard.Shape).ArcHeight;
                                GeometryType = CulvertGeometryType.Arch;
                                break;
                            case CrossSectionStandardShapeType.Cunette:
                                Width = ((CrossSectionStandardShapeCunette)definitionStandard.Shape).Width;
                                GeometryType = CulvertGeometryType.Cunette;
                                break;
                            case CrossSectionStandardShapeType.Elliptical:
                                Width = ((CrossSectionStandardShapeElliptical)definitionStandard.Shape).Width;
                                Height = ((CrossSectionStandardShapeElliptical)definitionStandard.Shape).Height;
                                GeometryType = CulvertGeometryType.Ellipse;
                                break;
                            case CrossSectionStandardShapeType.SteelCunette:
                                Height = ((CrossSectionStandardShapeSteelCunette) definitionStandard.Shape).Height;
                                Radius = ((CrossSectionStandardShapeSteelCunette) definitionStandard.Shape).RadiusR;
                                Radius1 = ((CrossSectionStandardShapeSteelCunette) definitionStandard.Shape).RadiusR1;
                                Radius2 = ((CrossSectionStandardShapeSteelCunette) definitionStandard.Shape).RadiusR2;
                                Radius3 = ((CrossSectionStandardShapeSteelCunette) definitionStandard.Shape).RadiusR3;
                                Angle = ((CrossSectionStandardShapeSteelCunette) definitionStandard.Shape).AngleA;
                                Angle1 = ((CrossSectionStandardShapeSteelCunette) definitionStandard.Shape).AngleA1;
                                GeometryType = CulvertGeometryType.SteelCunette;
                                break;
                            case CrossSectionStandardShapeType.Egg:
                                Width = ((CrossSectionStandardShapeEgg) definitionStandard.Shape).Width;
                                GeometryType = CulvertGeometryType.Egg;
                                break;
                            case CrossSectionStandardShapeType.Circle:
                                Diameter = ((CrossSectionStandardShapeCircle) definitionStandard.Shape).Diameter;
                                GeometryType = CulvertGeometryType.Round;
                                break;
                            case CrossSectionStandardShapeType.InvertedEgg:
                                Width = ((CrossSectionStandardShapeInvertedEgg)definitionStandard.Shape).Width;
                                GeometryType = CulvertGeometryType.InvertedEgg;
                                break;
                            case CrossSectionStandardShapeType.UShape:
                                Width = ((CrossSectionStandardShapeUShape)definitionStandard.Shape).Width;
                                Height = ((CrossSectionStandardShapeUShape)definitionStandard.Shape).Height;
                                ArcHeight = ((CrossSectionStandardShapeUShape)definitionStandard.Shape).ArcHeight;
                                GeometryType = CulvertGeometryType.UShape;
                                break;
                            default:
                                UpdateCrossSectionDefinition(geometryType);
                                break;
                        }

                        break;
                    case CrossSectionDefinitionZW definitionZw:
                        TabulatedCrossSectionDefinition = definitionZw;
                        GeometryType = CulvertGeometryType.Tabulated;
                        break;
                }
            }
        }

        [DisplayName("Roughness")]
        [FeatureAttribute(Order = 7, ExportName = "Roughness")]
        public virtual double Friction { get; set; }

        [DynamicReadOnly]
        [DisplayName("Ground layer roughness")]
        //[FeatureAttribute(Order = 9, ExportName = "GLRoughness")]
        [Browsable(false)]
        public virtual double GroundLayerRoughness { get; set; }

        [DynamicReadOnly]
        [DisplayName("Ground layer thickness")]
        //[FeatureAttribute(Order = 10, ExportName = "GLThickness")]
        [Browsable(false)]
        public virtual double GroundLayerThickness { get; set; }

        [DisplayName("Ground layer")]
        //[FeatureAttribute(Order = 8, ExportName = "GroundLayer")]
        [Browsable(false)]
        public virtual bool GroundLayerEnabled { get; set; }

        [DisplayName("Roughness type")]
        [FeatureAttribute(Order = 6, ExportName = "RoughType")]
        public virtual CulvertFrictionType FrictionType
        {
            get { return FrictionTypeConverter.ConvertToCulvertFrictionType(FrictionDataType); }
            set { FrictionDataType = FrictionTypeConverter.ConvertFrictionType(value); }
        }

        [Browsable(false)]
        public virtual Friction FrictionDataType { get; set; }

        [DisplayName("Length")]
        [FeatureAttribute(Order = 5, ExportName = "Length")]
        public virtual double CulvertLength
        {
            get { return Length; }
            set { Length = value; }
        }

        [DynamicReadOnly]
        [FeatureAttribute(Order = 24)]
        public virtual double Width { get; set; }

        [DynamicReadOnly]
        [FeatureAttribute(Order = 25)]
        public virtual double Height { get; set; }

        [DynamicReadOnly]
        [DisplayName("Arc height")]
        [FeatureAttribute(Order = 26)]
        public virtual double ArcHeight { get; set; }

        [DynamicReadOnly]
        [DisplayName("Diameter")]
        [FeatureAttribute(Order = 27)]
        public virtual double Diameter { get; set; }

        [DynamicReadOnly]
        [DisplayName("Radius")]
        [FeatureAttribute(Order = 28)]
        public virtual double Radius { get; set; }

        [DynamicReadOnly]
        [DisplayName("Radius 1")]
        [FeatureAttribute(Order = 29)]
        public virtual double Radius1 { get; set; }

        [DynamicReadOnly]
        [DisplayName("Radius 2")]
        [FeatureAttribute(Order = 30)]
        public virtual double Radius2 { get; set; }

        [DynamicReadOnly]
        [DisplayName("Radius 3")]
        [FeatureAttribute(Order = 31)]
        public virtual double Radius3 { get; set; }

        [DynamicReadOnly]
        [DisplayName("Angle")]
        [FeatureAttribute(Order = 32)]
        public virtual double Angle { get; set; }

        [DynamicReadOnly]
        [DisplayName("Angle 1")]
        [FeatureAttribute(Order = 33)]
        public virtual double Angle1 { get; set; }
        
        [DisplayName("Sub type")]
        [FeatureAttribute(Order = 20)]
        public virtual CulvertType CulvertType {
            get { return culvertType; }
            set => culvertType = value;
        }

        public virtual bool Closed { get; set; }
        
        [DisplayName("Gated")]
        [FeatureAttribute(Order = 17, ExportName = "Gated")]
        public virtual bool IsGated { get; set; }

        private SteerableProperty gateInitialOpening;

        public virtual bool UseGateInitialOpeningTimeSeries
        {
            get => gateInitialOpening.CurrentDriver == SteerablePropertyDriver.TimeSeries;
            set => gateInitialOpening.CurrentDriver = value ? SteerablePropertyDriver.TimeSeries 
                                                            : SteerablePropertyDriver.Constant;
        }

        [DynamicReadOnly]
        [DisplayName("Gate opening")]
        [FeatureAttribute(Order = 18, ExportName = "GateOpening")]
        public virtual double GateInitialOpening
        {
            get => gateInitialOpening.Constant; 
            set => gateInitialOpening.Constant = value; 
        }

        public virtual TimeSeries GateInitialOpeningTimeSeries
        {
            get => gateInitialOpening.TimeSeries;
            set => gateInitialOpening.TimeSeries = value;
        }

        [DynamicReadOnly]
        [DisplayName("Gate lower edge")]
        [FeatureAttribute(Order = 19, ExportName = "GateLowEdge")]
        public virtual double GateLowerEdgeLevel => 
            BottomLevel + GateInitialOpening;

        [DisplayName("Inlet loss coefficient")]
        [FeatureAttribute(Order = 13, ExportName = "InLossCoef")]
        public virtual double InletLossCoefficient { get; set; }

        [DisplayName("Outlet loss coefficient")]
        [FeatureAttribute(Order = 14, ExportName = "OutLossCoef")]
        public virtual double OutletLossCoefficient { get; set; }

        [DisplayName("Inlet level")]
        [FeatureAttribute(Order = 11, ExportName = "InletLvl")]
        public virtual double InletLevel { get; set; }

        [DisplayName("Outlet level")]
        [FeatureAttribute(Order = 12, ExportName = "OutletLvl")]
        public virtual double OutletLevel { get; set; }

        public virtual double BottomLevel => (InletLevel + OutletLevel)/2;

        [DynamicReadOnly]
        [DisplayName("Bend loss coefficient")]
        [FeatureAttribute(Order = 15, ExportName = "BendLosCoef")]
        public virtual double BendLossCoefficient { get; set; }

        [DisplayName("Shape")]
        [FeatureAttribute(Order = 23, ExportName = "GeomType")]
        public virtual CulvertGeometryType GeometryType
        {
            get { return geometryType; }
            set
            {
                geometryType = value;
                UpdateCrossSectionDefinition(geometryType);
            }
        }

        private void UpdateCrossSectionDefinition(CulvertGeometryType type)
        {
            switch (type)
            {
                case CulvertGeometryType.Rectangle:
                    crossSectionDefinition =
                        new CrossSectionDefinitionStandard(new CrossSectionStandardShapeRectangle()
                        {
                            Width = Width,
                            Height = Height
                        }) { Name = Name };
                    break;
                case CulvertGeometryType.Round:
                    crossSectionDefinition =
                        new CrossSectionDefinitionStandard(new CrossSectionStandardShapeCircle()
                        {
                            Diameter = Diameter,
                        }) { Name = Name };
                    break;
                case CulvertGeometryType.Ellipse:
                    crossSectionDefinition =
                        new CrossSectionDefinitionStandard(new CrossSectionStandardShapeElliptical()
                        {
                            Width = Width,
                            Height = Height
                        }) { Name = Name };
                    break;
                case CulvertGeometryType.Egg:
                    crossSectionDefinition =
                        new CrossSectionDefinitionStandard(new CrossSectionStandardShapeEgg()
                        {
                            Width = Width
                        }) { Name = Name };
                    break;
                case CulvertGeometryType.InvertedEgg:
                    crossSectionDefinition =
                        new CrossSectionDefinitionStandard(new CrossSectionStandardShapeInvertedEgg()
                        {
                            Width = Width
                        }) { Name = Name };
                    break;
                case CulvertGeometryType.Arch:
                    crossSectionDefinition =
                        new CrossSectionDefinitionStandard(new CrossSectionStandardShapeArch()
                        {
                            Width = Width,
                            Height = Height,
                            ArcHeight = ArcHeight
                        }) { Name = Name };
                    break;
                case CulvertGeometryType.UShape:
                    crossSectionDefinition =
                        new CrossSectionDefinitionStandard(new CrossSectionStandardShapeUShape()
                        {
                            Width = Width,
                            Height = Height,
                            ArcHeight = ArcHeight
                        }) { Name = Name };
                    break;
                case CulvertGeometryType.Cunette:
                    crossSectionDefinition =
                        new CrossSectionDefinitionStandard(new CrossSectionStandardShapeCunette()
                        {
                            Width = Width
                        }) { Name = Name };
                    break;
                case CulvertGeometryType.SteelCunette:
                    crossSectionDefinition =
                        new CrossSectionDefinitionStandard(new CrossSectionStandardShapeSteelCunette()
                        {
                            Height = Height,
                            RadiusR = Radius,
                            RadiusR1 = Radius1,
                            RadiusR2 = Radius2,
                            RadiusR3 = Radius3,
                            AngleA = Angle,
                            AngleA1 = Angle1
                        }) { Name = Name };
                    break;
                default:
                    crossSectionDefinition = tabulatedCrossSectionDefinition;
                    if(crossSectionDefinition != null && crossSectionDefinition.Name != Name)
                        crossSectionDefinition.Name = Name;
                    break;
            }
        }

        public virtual bool AllowNegativeFlow
        {
            get => FlowDirection == FlowDirection.Both ||
                   FlowDirection == FlowDirection.Negative;
            set
            {
                if (value != AllowNegativeFlow)
                {
                    UpdateFlowDirection(AllowPositiveFlow, value);
                }
            }
        }

        public virtual bool AllowPositiveFlow
        {
            get => FlowDirection == FlowDirection.Both ||
                   FlowDirection == FlowDirection.Positive;
            set
            {
                if (value != AllowPositiveFlow)
                {
                    UpdateFlowDirection(value, AllowNegativeFlow);
                }
            }
        }

        public virtual CrossSectionDefinitionZW TabulatedCrossSectionDefinition
        {
            get => tabulatedCrossSectionDefinition;
            set => tabulatedCrossSectionDefinition = value;
        }

        public virtual IFunction GateOpeningLossCoefficientFunction { get; set; }

        public override void CopyFrom(object source)
        {
            base.CopyFrom(source);
            var sourceCulvert = (Culvert) source;

            CulvertType = sourceCulvert.CulvertType;
            BendLossCoefficient = sourceCulvert.BendLossCoefficient;
            Diameter = sourceCulvert.Diameter;
            FlowDirection = sourceCulvert.FlowDirection;
            Friction = sourceCulvert.Friction;
            FrictionType = sourceCulvert.FrictionType;
            
            gateInitialOpening = new SteerableProperty(sourceCulvert.gateInitialOpening);

            GateOpeningLossCoefficientFunction = (IFunction)sourceCulvert.GateOpeningLossCoefficientFunction.Clone();
            GeometryType = sourceCulvert.GeometryType;
            Height = sourceCulvert.Height;
            InletLevel = sourceCulvert.InletLevel;
            InletLossCoefficient = sourceCulvert.InletLossCoefficient;
            IsGated = sourceCulvert.IsGated;

            GroundLayerThickness = sourceCulvert.GroundLayerThickness;
            GroundLayerRoughness = sourceCulvert.GroundLayerRoughness;

            OutletLevel = sourceCulvert.OutletLevel;
            OutletLossCoefficient = sourceCulvert.OutletLossCoefficient;
            Radius = sourceCulvert.Radius;
            Radius1 = sourceCulvert.Radius1;
            Radius2 = sourceCulvert.Radius2;
            Radius3 = sourceCulvert.Radius3;
            TabulatedCrossSectionDefinition = (CrossSectionDefinitionZW)sourceCulvert.TabulatedCrossSectionDefinition.Clone();
            Width = sourceCulvert.Width;
        }

        private void UpdateFlowDirection(bool allowPositiveFlow, bool allowNegativeFlow)
        {
            FlowDirection = GetPossibleFlowDirection(allowPositiveFlow, allowNegativeFlow);
        }

        private static FlowDirection GetPossibleFlowDirection(bool allowPositiveFlow, bool allowNegativeFlow)
        {
            return allowPositiveFlow
                       ? (allowNegativeFlow ? FlowDirection.Both : FlowDirection.Positive)
                       : (allowNegativeFlow ? FlowDirection.Negative : FlowDirection.None);
        }
        
        /// <summary>
        /// Sets defaults as seen in Sobek 2.12
        /// </summary>
        /// <param name="gateFunction"></param>
        private static void SetDefaultGateOpeningFunction(IFunction gateFunction)
        {
            gateFunction.Clear();
            gateFunction[0.0] = 2.1;
            gateFunction[0.1] = 1.96;
            gateFunction[0.2] = 1.8;
            gateFunction[0.3] = 1.74;
            gateFunction[0.4] = 1.71;
            gateFunction[0.5] = 1.71;
            gateFunction[0.6] = 1.71;
            gateFunction[0.7] = 1.64;
            gateFunction[0.8] = 1.51;
            gateFunction[0.9] = 1.36; 
            gateFunction[1.0] = 1.19;
        }

        public static Culvert CreateDefault()
        {
            var culvert = new Culvert
            {
                InletLevel = -5.0,
                OutletLevel = -5.0,
                Width = 1.0,
                Height = 1.0,
                Length = 10.0,
                InletLossCoefficient = 0.1,
                OutletLossCoefficient = 0.1,
                BendLossCoefficient = 1.0,
                ArcHeight = 0.25,
                Diameter = 4.0, 
                Radius = 0.5,
                Radius1 = 0.8,
                Radius2 = 0.2,
                Radius3 = 0,
                Angle = 28,
                Angle1 = 0,
                Friction = 45.0,
                FrictionDataType = Hydro.Friction.Chezy
            };

            culvert.TabulatedCrossSectionDefinition.SetAsRectangle(0, 2, 2);
            SetDefaultGateOpeningFunction(culvert.GateOpeningLossCoefficientFunction);
            return culvert;
        }

        public static Culvert CreateDefault(IBranch branch)
        {
            Culvert culvert = CreateDefault();
            AddStructureToNetwork(culvert,branch);
            return culvert;
        }

        [DynamicReadOnlyValidationMethod]
        public virtual bool DynamicReadOnlyValidationMethod(string propertyName)
        {
            if (propertyName == nameof(GateInitialOpening))
            {
                return !IsGated;
            }

            if (propertyName == nameof(GroundLayerRoughness) || propertyName == nameof(GroundLayerThickness))
            {
                return !GroundLayerEnabled;
            }

            if (propertyName == nameof(Width))
            {
                return GeometryType == CulvertGeometryType.SteelCunette ||
                         GeometryType == CulvertGeometryType.Tabulated ||
                         GeometryType == CulvertGeometryType.Round;
            }

            if (propertyName == nameof(Height))
            {
                return GeometryType == CulvertGeometryType.Tabulated || GeometryType == CulvertGeometryType.Round;
            }

            if (propertyName == nameof(ArcHeight))
            {
                return GeometryType != CulvertGeometryType.Arch;
            }

            if (propertyName == nameof(Diameter))
            {
                return GeometryType != CulvertGeometryType.Round;
            }
            
            if (propertyName == nameof(Radius) || propertyName == nameof(Radius1) || propertyName == nameof(Radius2) || propertyName == nameof(Radius3) ||
                propertyName == nameof(Angle) || propertyName == nameof(Angle1))
            {
                return GeometryType != CulvertGeometryType.SteelCunette;
            }

            if (propertyName == nameof(BendLossCoefficient))
            {
                return CulvertType != CulvertType.InvertedSiphon;
            }

            return false;
        }

        public override StructureType GetStructureType()
        {
            switch (CulvertType)
            {
                case CulvertType.Culvert:
                    return StructureType.Culvert;
                case CulvertType.InvertedSiphon:
                    return StructureType.InvertedSiphon;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public virtual IEnumerable<SteerableProperty> RetrieveSteerableProperties()
        {
            yield return gateInitialOpening;
        }
    }
}