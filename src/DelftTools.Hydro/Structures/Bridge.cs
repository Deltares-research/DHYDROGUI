using System.ComponentModel;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.DataSets;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Aop;
using DelftTools.Utils.ComponentModel;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro.Structures
{
    ///<summary>
    ///</summary>
    [Entity(FireOnCollectionChange = false)]
    public class Bridge : BranchStructure, IBridge
    {
        public Bridge()
            : this("Bridge") {}

        public Bridge(string name)
        {
            base.Name = name;
            Width = 50;
            Height = 3;
            Length = 20;
            BottomLevel = 0;
            PillarWidth = 3;
            ShapeFactor = 1.5;

            TabulatedCrossSectionDefinition = new CrossSectionDefinitionZW();
            BridgeType = BridgeType.Tabulated;
        }

        /// <summary>
        /// Set the bridge geometry to a tabulated profile with one single rectangular segment.
        /// </summary>
        /// <param name="bedLevel"> </param>
        /// <param name="width"> </param>
        /// <param name="height"> </param>
        public virtual void SetRectangleCrossSection(double bedLevel, double width, double height)
        {
            BottomLevel = bedLevel;
            Width = width;
            Height = height;
            //create a single section. Reference level is not used since the crossection is defined absolute. (Ref = 0)

            TabulatedCrossSectionDefinition.SetAsRectangle(bedLevel, width, height);
        }

        public static Bridge CreateDefault()
        {
            var bridge = new Bridge();
            bridge.TabulatedCrossSectionDefinition.SetWithHfswData(new[]
            {
                new HeightFlowStorageWidth(-10, 50, 50),
                new HeightFlowStorageWidth(0, 100, 100)
            });
            bridge.FrictionType = BridgeFrictionType.Chezy;
            bridge.Friction = 45.0;
            return bridge;
        }

        public static Bridge CreateDefault(IBranch branch)
        {
            Bridge bridge = CreateDefault();
            AddStructureToNetwork(bridge, branch);
            return bridge;
        }

        [DynamicReadOnlyValidationMethod]
        public virtual bool DynamicReadOnlyValidationMethod(string propertyName)
        {
            if (propertyName == "BridgeLength" || propertyName == "FrictionType" || propertyName == "Friction" ||
                propertyName == "GroundLayerEnabled" || propertyName == "InletLossCoefficient" ||
                propertyName == "OutletLossCoefficient")
            {
                return IsPillar;
            }

            if (propertyName == "GroundLayerThickness" || propertyName == "GroundLayerRoughness")
            {
                return IsPillar || !GroundLayerEnabled;
            }

            if (propertyName == "BottomLevel" || propertyName == "Width" || propertyName == "Height")
            {
                return !IsRectangle;
            }

            if (propertyName == "PillarWidth" || propertyName == "ShapeFactor")
            {
                return !IsPillar;
            }

            return false;
        }

        public override void CopyFrom(object source)
        {
            var copyFrom = (Bridge) source;
            base.CopyFrom(source);
            InletLossCoefficient = copyFrom.InletLossCoefficient;
            OutletLossCoefficient = copyFrom.OutletLossCoefficient;
            FlowDirection = copyFrom.FlowDirection;
            FrictionType = copyFrom.FrictionType;
            Friction = copyFrom.Friction;

            GroundLayerThickness = copyFrom.GroundLayerThickness;
            GroundLayerRoughness = copyFrom.GroundLayerRoughness;

            BridgeType = copyFrom.BridgeType;
            if (copyFrom.BridgeType == BridgeType.Tabulated)
            {
                TabulatedCrossSectionDefinition.ZWDataTable.Clear();
                foreach (CrossSectionDataSet.CrossSectionZWRow zwwRow in copyFrom
                                                                         .TabulatedCrossSectionDefinition.ZWDataTable)
                {
                    TabulatedCrossSectionDefinition.ZWDataTable.AddCrossSectionZWRow(
                        zwwRow.Z, zwwRow.Width, zwwRow.StorageWidth);
                }
            }

            BottomLevel = copyFrom.BottomLevel;
            Width = copyFrom.Width;
            Height = copyFrom.Height;
            OffsetY = copyFrom.OffsetY;
            ShapeFactor = copyFrom.ShapeFactor;
            PillarWidth = copyFrom.PillarWidth;
        }

        public override StructureType GetStructureType()
        {
            return IsPillar
                       ? StructureType.BridgePillar
                       : StructureType.Bridge;
        }

        protected static FlowDirection GetPossibleFlowDirection(bool allowPositiveFlow, bool allowNegativeFlow)
        {
            return allowPositiveFlow
                       ? allowNegativeFlow ? FlowDirection.Both : FlowDirection.Positive
                       : allowNegativeFlow
                           ? FlowDirection.Negative
                           : FlowDirection.None;
        }

        private void UpdateCrossSectionDefinition(BridgeType type)
        {
            switch (type)
            {
                case BridgeType.Rectangle:
                    crossSectionDefinition =
                        new CrossSectionDefinitionStandard(new CrossSectionStandardShapeRectangle()
                        {
                            Width = Width,
                            Height = Height
                        }) {Name = Name};
                    break;
                case BridgeType.Tabulated:
                    crossSectionDefinition = tabulatedCrossSectionDefinition;
                    if (crossSectionDefinition.Name != Name)
                    {
                        crossSectionDefinition.Name = Name;
                    }

                    break;
                default:
                    crossSectionDefinition = null;
                    break;
            }
        }

        #region IBridge Members

        private CrossSectionDefinitionZW tabulatedCrossSectionDefinition;
        private ICrossSectionDefinition crossSectionDefinition;
        private BridgeType bridgeType;

        /// <summary>
        /// CrossSection of the bridge.
        /// </summary>
        public virtual CrossSectionDefinitionZW TabulatedCrossSectionDefinition
        {
            get => tabulatedCrossSectionDefinition;
            set => tabulatedCrossSectionDefinition = value;
        }

        /// <summary>
        /// Crosssection as used for model api. It does not include any level since these are passed separately
        /// </summary>
        public virtual ICrossSectionDefinition CrossSectionDefinition
        {
            get
            {
                UpdateCrossSectionDefinition(bridgeType);
                return crossSectionDefinition;
            }
        }

        /// <summary>
        /// Binding code
        /// </summary>
        [NoNotifyPropertyChange]
        public virtual bool IsTabulated
        {
            get => BridgeType == BridgeType.Tabulated;
            set
            {
                if (value)
                {
                    BridgeType = BridgeType.Tabulated;
                }
            }
        }

        /// <summary>
        /// Binding code
        /// </summary>
        [NoNotifyPropertyChange]
        public virtual bool IsRectangle
        {
            get => BridgeType == BridgeType.Rectangle;
            set
            {
                if (value)
                {
                    BridgeType = BridgeType.Rectangle;
                }
            }
        }

        /// <summary>
        /// Binding code
        /// </summary>
        [NoNotifyPropertyChange]
        public virtual bool IsPillar
        {
            get => BridgeType == BridgeType.Pillar;
            set
            {
                if (value)
                {
                    BridgeType = BridgeType.Pillar;
                }
            }
        }

        /// <summary>
        /// Effective crosssection of the bridge. If rectangle a single section tabulated is returned.
        /// </summary>
        /// <returns> Crosssection as used for ModelAPI and in views </returns>
        public virtual CrossSectionDefinitionZW EffectiveCrossSectionDefinition
        {
            get
            {
                if (BridgeType == BridgeType.Tabulated)
                {
                    return TabulatedCrossSectionDefinition;
                }

                return StandardCrossSectionsFactory
                       .GetTabulatedCrossSectionFromRectangle(Width, Height).AddLevel(BottomLevel);
            }
        }

        [DynamicReadOnly]
        [DisplayName("Inlet loss coefficient")]
        [FeatureAttribute(Order = 13, ExportName = "InLossCoef")]
        public virtual double InletLossCoefficient { get; set; }

        [DynamicReadOnly]
        [DisplayName("Outlet loss coefficient")]
        [FeatureAttribute(Order = 14, ExportName = "OutLossCoef")]
        public virtual double OutletLossCoefficient { get; set; }

        [DisplayName("Flow direction")]
        [FeatureAttribute(Order = 12, ExportName = "FlowDir")]
        public virtual FlowDirection FlowDirection { get; set; }

        [DynamicReadOnly]
        [DisplayName("Roughness type")]
        [FeatureAttribute(Order = 7, ExportName = "RoughType")]
        public virtual BridgeFrictionType FrictionType
        {
            get => FrictionTypeConverter.ConvertToBridgeFrictionType(FrictionDataType);
            set => FrictionDataType = FrictionTypeConverter.ConvertFrictionType(value);
        }

        [Browsable(false)]
        public virtual Friction FrictionDataType { get; set; }

        [DynamicReadOnly]
        [DisplayName("Roughness")]
        [FeatureAttribute(Order = 8, ExportName = "Roughness")]
        public virtual double Friction { get; set; }

        [DynamicReadOnly]
        [DisplayName("Pillar width")]
        [FeatureAttribute(Order = 18)]
        public virtual double PillarWidth { get; set; }

        [DynamicReadOnly]
        [DisplayName("Shape factor")]
        [FeatureAttribute(Order = 19)]
        public virtual double ShapeFactor { get; set; }

        [DynamicReadOnly]
        [DisplayName("Ground layer roughness")]
        [FeatureAttribute(Order = 10, ExportName = "GLRoughness")]
        public virtual double GroundLayerRoughness { get; set; }

        [DynamicReadOnly]
        [DisplayName("Ground layer thickness")]
        [FeatureAttribute(Order = 11, ExportName = "GLThickness")]
        public virtual double GroundLayerThickness { get; set; }

        [DynamicReadOnly]
        [DisplayName("Ground layer")]
        [FeatureAttribute(Order = 9, ExportName = "GroundLayer")]
        public virtual bool GroundLayerEnabled { get; set; }

        [DisplayName("Shape")]
        [FeatureAttribute(Order = 5, ExportName = "Shape")]
        public virtual BridgeType BridgeType
        {
            get => bridgeType;
            set
            {
                bridgeType = value;
                UpdateCrossSectionDefinition(bridgeType);
            }
        }

        [DynamicReadOnly]
        [DisplayName("Length")]
        [FeatureAttribute(Order = 6, ExportName = "Length")]
        public virtual double BridgeLength
        {
            get => Length;
            set => Length = value;
        }

        [DynamicReadOnly]
        [DisplayName("Bed level")]
        [FeatureAttribute(Order = 15)]
        public virtual double BottomLevel { get; set; }

        [DynamicReadOnly]
        [DisplayName("Width")]
        [FeatureAttribute(Order = 16)]
        public virtual double Width { get; set; }

        [DynamicReadOnly]
        [DisplayName("Height")]
        [FeatureAttribute(Order = 17)]
        public virtual double Height { get; set; }

        public virtual bool AllowNegativeFlow
        {
            get =>
                FlowDirection == FlowDirection.Both ||
                FlowDirection == FlowDirection.Negative;
            set
            {
                if (value != AllowNegativeFlow)
                {
                    SetFlowDirection(AllowPositiveFlow, value);
                }
            }
        }

        [EditAction]
        private void SetFlowDirection(bool allowPositiveFlow, bool allowNegativeFlow)
        {
            FlowDirection = GetPossibleFlowDirection(allowPositiveFlow, allowNegativeFlow);
        }

        public virtual bool AllowPositiveFlow
        {
            get =>
                FlowDirection == FlowDirection.Both ||
                FlowDirection == FlowDirection.Positive;
            set
            {
                if (value != AllowPositiveFlow)
                {
                    SetFlowDirection(value, AllowNegativeFlow);
                }
            }
        }

        #endregion
    }
}