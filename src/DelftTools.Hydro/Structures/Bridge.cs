using System.ComponentModel;
using DelftTools.Hydro.CrossSections;
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
    [Entity(FireOnCollectionChange=false)]
    public class Bridge : BranchStructure, IBridge
    {
        public Bridge()
            : this("Bridge")
        {
        }

        public Bridge(string name)
        {
            base.Name = name;
            Width = 50;
            Height = 3;
            Length = 20;
            Shift = 0;
            PillarWidth = 3;
            ShapeFactor = 1.5;
            
            TabulatedCrossSectionDefinition = CrossSectionDefinitionZW.CreateDefault(name);
            YZCrossSectionDefinition = CrossSectionDefinitionYZ.CreateDefault(name);
            BridgeType = BridgeType.Tabulated;
        }

        #region IBridge Members

        private CrossSectionDefinitionZW tabulatedCrossSectionDefinition;
        private CrossSectionDefinitionYZ yzCrossSectionDefinition;
        private ICrossSectionDefinition crossSectionDefinition;
        private BridgeType bridgeType;

        /// <summary>
        /// CrossSection of the bridge.
        /// </summary>
        public virtual CrossSectionDefinitionZW TabulatedCrossSectionDefinition
        {
            get { return tabulatedCrossSectionDefinition; }
            set { tabulatedCrossSectionDefinition = value; }
        }
        
        /// <summary>
        /// CrossSection of the bridge.
        /// </summary>
        public virtual CrossSectionDefinitionYZ YZCrossSectionDefinition
        {
            get { return yzCrossSectionDefinition; }
            set { yzCrossSectionDefinition = value; }
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
            set
            {
                switch (value)
                {
                    case null:
                        UpdateCrossSectionDefinition(bridgeType);
                        break;
                    case CrossSectionDefinitionStandard definitionStandard:
                        if(definitionStandard.ShapeType != CrossSectionStandardShapeType.Rectangle)
                            UpdateCrossSectionDefinition(bridgeType);
                        else
                        {
                            Width = ((CrossSectionStandardShapeRectangle)definitionStandard.Shape).Width;
                            Height = ((CrossSectionStandardShapeRectangle)definitionStandard.Shape).Height;
                        }
                        BridgeType = BridgeType.Rectangle;
                        break;
                    case CrossSectionDefinitionYZ definitionYz:
                        YZCrossSectionDefinition = definitionYz;
                        BridgeType = BridgeType.YzProfile;
                        break;
                    case CrossSectionDefinitionZW definitionZw:
                        TabulatedCrossSectionDefinition = definitionZw;
                        BridgeType = BridgeType.Tabulated;
                        break;
                }
            }
        }

        /// <summary>
        /// Binding code
        /// </summary>
        [NoNotifyPropertyChange]
        public virtual bool IsTabulated
        {
            get { return BridgeType == BridgeType.Tabulated; }
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
        public virtual bool IsYz
        {
            get { return BridgeType == BridgeType.YzProfile; }
            set
            {
                if (value)
                {
                    BridgeType = BridgeType.YzProfile;
                }
            }
        }

        /// <summary>
        /// Binding code
        /// </summary>
        [NoNotifyPropertyChange]
        public virtual bool IsRectangle
        {
            get { return BridgeType == BridgeType.Rectangle; }
            set
            {
                if(value)
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
            get
            {
                return false; //return BridgeType == BridgeType.Pillar;//Not yet implemented in the kernel
            }
            set
            {
                if (value)
                {
                    //BridgeType = BridgeType.Pillar;//Not yet implemented in the kernel
                }
            }
        }

        [DynamicReadOnly]
        [DisplayName("Inlet loss coefficient")]
        [FeatureAttribute(Order = 8, ExportName = "InLossCoef")]
        public virtual double InletLossCoefficient { get; set; }

        [DynamicReadOnly]
        [DisplayName("Outlet loss coefficient")]
        [FeatureAttribute(Order = 9, ExportName = "OutLossCoef")]
        public virtual double OutletLossCoefficient { get; set; }

        [DisplayName("Flow direction")]
        [FeatureAttribute(Order = 7, ExportName = "FlowDir")]
        public virtual FlowDirection FlowDirection { get; set; }
        
        [DynamicReadOnly]
        [DisplayName("Roughness type")]
        [FeatureAttribute(Order = 13, ExportName = "RoughType")]
        public virtual BridgeFrictionType FrictionType
        {
            get { return FrictionTypeConverter.ConvertToBridgeFrictionType(FrictionDataType); }
            set { FrictionDataType = FrictionTypeConverter.ConvertFrictionType(value); }
        }

        [Browsable(false)]
        public virtual Friction FrictionDataType { get; set; }
        
        [DynamicReadOnly]
        [DisplayName("Roughness")]
        [FeatureAttribute(Order = 14, ExportName = "Roughness")]
        public virtual double Friction { get; set; }

        [DynamicReadOnly]
        [DisplayName("Pillar width")]
        //[FeatureAttribute(Order = 18)]
        [Browsable(false)]
        public virtual double PillarWidth { get; set; }

        [DynamicReadOnly]
        [DisplayName("Shape factor")]
        //[FeatureAttribute(Order = 19)]
        [Browsable(false)]
        public virtual double ShapeFactor { get; set; }

        [DynamicReadOnly]
        [DisplayName("Ground layer roughness")]
        //[FeatureAttribute(Order = 11, ExportName = "GLRoughness")]
        [Browsable(false)]
        public virtual double GroundLayerRoughness { get; set; }

        [DynamicReadOnly]
        [DisplayName("Ground layer thickness")]
        //[FeatureAttribute(Order = 12, ExportName = "GLThickness")]
        [Browsable(false)]
        public virtual double GroundLayerThickness { get; set; }

        [DynamicReadOnly]
        [DisplayName("Ground layer")]
        //[FeatureAttribute(Order = 10, ExportName = "GroundLayer")]
        [Browsable(false)]
        public virtual bool GroundLayerEnabled { get; set; }

        [DisplayName("Shape")]
        [FeatureAttribute(Order = 5, ExportName = "Shape")]
        public virtual BridgeType BridgeType
        {
            get { return bridgeType; }
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
            get { return Length; }
            set { Length = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Shift")]
        [FeatureAttribute(Order = 15)]
        public virtual double Shift { get; set; }

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
            get
            {
                return FlowDirection == FlowDirection.Both ||
                       FlowDirection == FlowDirection.Negative;
            }
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
            get
            {
                {
                    return FlowDirection == FlowDirection.Both ||
                           FlowDirection == FlowDirection.Positive;
                }
            }
            set
            {
                if (value != AllowPositiveFlow)
                {
                    SetFlowDirection(value, AllowNegativeFlow);
                }
            }
        }
        
        #endregion
        public override void CopyFrom(object source)
        {
            Bridge copyFrom = (Bridge) source;
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
                foreach (var zwwRow in copyFrom.TabulatedCrossSectionDefinition.ZWDataTable)
                {
                    TabulatedCrossSectionDefinition.ZWDataTable.AddCrossSectionZWRow(zwwRow.Z, zwwRow.Width, zwwRow.StorageWidth);
                }
            }
            if (copyFrom.BridgeType == BridgeType.YzProfile)
            {
                YZCrossSectionDefinition.YZDataTable.Clear();
                foreach (var yzRow in copyFrom.YZCrossSectionDefinition.YZDataTable)
                {
                    YZCrossSectionDefinition.YZDataTable.AddCrossSectionYZRow(yzRow.Yq, yzRow.Z);
                }
            }
            Shift = copyFrom.Shift;
            Width = copyFrom.Width;
            Height = copyFrom.Height;
            OffsetY = copyFrom.OffsetY;
            ShapeFactor = copyFrom.ShapeFactor;
            PillarWidth = copyFrom.PillarWidth;
        }

        protected static FlowDirection GetPossibleFlowDirection(bool allowPositiveFlow, bool allowNegativeFlow)
        {
            return allowPositiveFlow
                       ? (allowNegativeFlow ? FlowDirection.Both : FlowDirection.Positive)
                       : (allowNegativeFlow ? FlowDirection.Negative : FlowDirection.None);
        }

        /// <summary>
        /// Set the bridge geometry to a tabulated profile with one single rectangular segment.
        /// </summary>
        /// <param name="shift"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public virtual void SetRectangleCrossSection(double shift, double width, double height)
        {
            Shift = shift;
            Width = width;
            Height = height;
            //create a single section. Reference level is not used since the crossection is defined absolute. (Ref = 0)

            TabulatedCrossSectionDefinition.SetAsRectangle(shift, width, height);
            YZCrossSectionDefinition.SetAsRectangle(shift, width, height);
        }


        public static Bridge CreateDefault()
        {
            var bridge = new Bridge();
            bridge.SetRectangleCrossSection(bridge.Shift, bridge.Width, bridge.Height);
            bridge.FrictionType = BridgeFrictionType.Chezy;
            bridge.Friction = 45.0; 
            return bridge;
        }

        public static Bridge CreateDefault(IBranch branch)
        {
            var bridge = CreateDefault();
            AddStructureToNetwork(bridge, branch);
            return bridge;
        }

        [DynamicReadOnlyValidationMethod]
        public virtual bool DynamicReadOnlyValidationMethod(string propertyName)
        {
            if (propertyName == nameof(BridgeLength) || propertyName == nameof(FrictionType) || propertyName == nameof(Friction) ||
                propertyName == nameof(GroundLayerEnabled) || propertyName == nameof(InletLossCoefficient) || propertyName == nameof(OutletLossCoefficient))
            {
                return IsPillar;
            }

            if (propertyName == nameof(GroundLayerThickness) || propertyName == nameof(GroundLayerRoughness))
            {
                return IsPillar || !GroundLayerEnabled;
            }

            if (propertyName == nameof(Width) || propertyName == nameof(Height))
            {
                return !IsRectangle;
            }

            if (propertyName == nameof(PillarWidth) || propertyName == nameof(ShapeFactor))
            {
                return !IsPillar;
            }

            return false;
        }

        public override StructureType GetStructureType()
        {
            if(IsPillar) return StructureType.BridgePillar;
            else return StructureType.Bridge;
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
                        }) { Name = Name };
                    break;
                case BridgeType.Tabulated:
                    crossSectionDefinition = tabulatedCrossSectionDefinition;
                    if (crossSectionDefinition.Name != Name)
                        crossSectionDefinition.Name = Name;
                    break;
                case BridgeType.YzProfile:
                    crossSectionDefinition = yzCrossSectionDefinition;
                    if (crossSectionDefinition.Name != Name)
                        crossSectionDefinition.Name = Name;
                    break;
                default:
                    crossSectionDefinition = null;
                    break;
            }
        }
    }
}