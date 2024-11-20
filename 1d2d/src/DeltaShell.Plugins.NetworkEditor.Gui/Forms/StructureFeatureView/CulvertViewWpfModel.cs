using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections.DataSets;
using DelftTools.Hydro.Structures;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView
{
    public class CulvertViewWpfViewModel : INotifyPropertyChanged, IDisposable
    {
        private ICulvert culvert = new Culvert();

        public ICulvert Culvert
        {
            get { return culvert; }
            set
            {
                if (culvert != null)
                {
                    ((INotifyPropertyChanged) culvert).PropertyChanged -= UpdateAllProperties;
                }

                culvert = value;

                if (culvert == null)
                {
                    return;
                }

                ((INotifyPropertyChanged) culvert).PropertyChanged += UpdateAllProperties;
                
                SelectedCulvertGeometryType = culvert.GeometryType;
                SelectedCulvertFrictionType = culvert.FrictionType;
                SelectedCulvertStructureType = culvert.CulvertType;
                GeometryTabulated = culvert.TabulatedCrossSectionDefinition.ZWDataTable;
                
            }
        }

        private void UpdateAllProperties(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(FlowIsNegative));
            OnPropertyChanged(nameof(FlowIsPositive));
            OnPropertyChanged(nameof(SelectedCulvertStructureType));
            OnPropertyChanged(nameof(SelectedCulvertGeometryType));
            OnPropertyChanged(nameof(SelectedCulvertFrictionType));
            OnPropertyChanged(nameof(IsGated));
            OnPropertyChanged(nameof(CulvertLength));
            OnPropertyChanged(nameof(CulvertOffsetY));
            OnPropertyChanged(nameof(InletLevel));
            OnPropertyChanged(nameof(OutletLevel));
            OnPropertyChanged(nameof(InletLossCoeff));
            OnPropertyChanged(nameof(OutletLossCoeff));
            OnPropertyChanged(nameof(GateInitialGateOpening));
            OnPropertyChanged(nameof(GateLowEdgeLevel));
            OnPropertyChanged(nameof(FrictionValue));
            OnPropertyChanged(nameof(GeometryDiameter));
            OnPropertyChanged(nameof(GeometryTabulated));
            OnPropertyChanged(nameof(GeometryWidth));
            OnPropertyChanged(nameof(GeometryHeight));
            OnPropertyChanged(nameof(GeometryArcHeight));
            OnPropertyChanged(nameof(GeometryRadiusR));
            OnPropertyChanged(nameof(GeometryRadiusR1));
            OnPropertyChanged(nameof(GeometryRadiusR2));
            OnPropertyChanged(nameof(GeometryRadiusR3));
            OnPropertyChanged(nameof(GeometryAngleA));
            OnPropertyChanged(nameof(GeometryAngleA1));
            OnPropertyChanged(nameof(IsInvertedSiphon));
            OnPropertyChanged(nameof(IsCulvert));
            OnPropertyChanged(nameof(IsTabulated));
            OnPropertyChanged(nameof(IsRound));
            OnPropertyChanged(nameof(IsSteelCunette));
            OnPropertyChanged(nameof(IsArch));
            OnPropertyChanged(nameof(IsCunette));
            OnPropertyChanged(nameof(IsArch));
            OnPropertyChanged(nameof(IsEllipse));
            OnPropertyChanged(nameof(IsRectangle));
            OnPropertyChanged(nameof(IsEgg));
            OnPropertyChanged(nameof(IsInvertedEgg));
            OnPropertyChanged(nameof(IsUShape));
            OnPropertyChanged(nameof(GeometryWidthVisibility));
            OnPropertyChanged(nameof(GeometryHeightVisibility));
            OnPropertyChanged(nameof(BendLossCoeffVisibility));
            OnPropertyChanged(nameof(HasArcHeight));
        }

        private void UpdateFlow(bool positive, bool negative)
        {
            if (positive && negative)
            {
                culvert.FlowDirection = FlowDirection.Both;
            }
            else if (positive && !negative)
            {
                culvert.FlowDirection = FlowDirection.Positive;
            }
            else if (negative)
            {
                culvert.FlowDirection = FlowDirection.Negative;
            }
            else
            {
                culvert.FlowDirection = FlowDirection.None;
            }
        }

        private static string GetFrictionUnit(CulvertFrictionType type)
        {
            switch (type)
            {
                case CulvertFrictionType.Chezy:
                    return "m^1/2*s^-1";
                case CulvertFrictionType.Manning:
                    return "s*m^-1/3";
                case CulvertFrictionType.StricklerKn:
                    return "m";
                case CulvertFrictionType.StricklerKs:
                    return "m^1/3*s^-1";
                case CulvertFrictionType.WhiteColebrook:
                    return "m";
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        #region Type selectors

        public CulvertType SelectedCulvertStructureType
        {
            get { return culvert.CulvertType; }
            set
            {
                //Update properties
                culvert.CulvertType = value;
                IsCulvert = value == CulvertType.Culvert;
                IsInvertedSiphon = value == CulvertType.InvertedSiphon;

                //Update visibilities
                BendLossCoeffVisibility = IsInvertedSiphon;
            }
        }

        public CulvertFrictionType SelectedCulvertFrictionType
        {
            get { return culvert.FrictionType; }
            set
            {
                culvert.FrictionType = value;
                //Update visibilities
                GroundLayerThicknessUnit = GetFrictionUnit(value);
                GroundLayerRoughnessUnit = GetFrictionUnit(value);
            }
        }

        public CulvertGeometryType SelectedCulvertGeometryType
        {
            get { return culvert.GeometryType; }
            set
            {
                culvert.GeometryType = value;
                // Update visibilities
                IsSteelCunette = value == CulvertGeometryType.SteelCunette;
                IsCunette = value == CulvertGeometryType.Cunette;
                IsArch = value == CulvertGeometryType.Arch;
                IsEllipse = value == CulvertGeometryType.Ellipse;
                IsRectangle = value == CulvertGeometryType.Rectangle;
                IsEgg = value == CulvertGeometryType.Egg;
                IsRound = value == CulvertGeometryType.Round;
                IsTabulated = value == CulvertGeometryType.Tabulated;
                IsInvertedEgg = value == CulvertGeometryType.InvertedEgg;
                IsUShape = value == CulvertGeometryType.UShape;

                GeometryHeightVisibility = IsSteelCunette || IsCunette || IsArch || IsEllipse || IsRectangle || IsEgg ||
                                           IsInvertedEgg || IsUShape;
                GeometryWidthVisibility = IsCunette || IsArch || IsEllipse || IsRectangle || IsEgg || IsInvertedEgg ||
                                          IsUShape;
                HasArcHeight = IsArch || IsUShape;
            }
        }

        #endregion

        #region Related model properties

        public bool IsGroundLayer
        {
            get { return culvert.GroundLayerEnabled; }
            set { culvert.GroundLayerEnabled = value; }
        }

        public bool IsGated
        {
            get { return culvert.IsGated; }
            set { culvert.IsGated = value; }
        }

        public bool FlowIsPositive
        {
            get
            {
                return (culvert.FlowDirection == FlowDirection.Both || culvert.FlowDirection == FlowDirection.Positive);
            }
            set { UpdateFlow(value, FlowIsNegative); }
        }

        public bool FlowIsNegative
        {
            get
            {
                return (culvert.FlowDirection == FlowDirection.Both || culvert.FlowDirection == FlowDirection.Negative);
            }
            set { UpdateFlow(FlowIsPositive, value); }
        }

        public double CulvertLength
        {
            get { return culvert.Length; }
            set { culvert.Length = value; }
        }

        public double CulvertOffsetY
        {
            get { return culvert.OffsetY; }
            set { culvert.OffsetY = value; }
        }

        public double InletLevel
        {
            get { return culvert.InletLevel; }
            set { culvert.InletLevel = value; }
        }

        public double OutletLevel
        {
            get { return culvert.OutletLevel; }
            set { culvert.OutletLevel = value; }
        }

        public double InletLossCoeff
        {
            get { return culvert.InletLossCoefficient; }
            set { culvert.InletLossCoefficient = value; }
        }

        public double OutletLossCoeff
        {
            get { return culvert.OutletLossCoefficient; }
            set { culvert.OutletLossCoefficient = value; }
        }

        public double BendLossCoefficient
        {
            get { return culvert.BendLossCoefficient; }
            set { culvert.BendLossCoefficient = value; }
        }
        
        public double GateInitialGateOpening
        {
            get => culvert.GateInitialOpening;
            set => culvert.GateInitialOpening = value;
        }

        public bool UseGateInitialOpeningTimeSeries
        {
            get => culvert.UseGateInitialOpeningTimeSeries;
            set => culvert.UseGateInitialOpeningTimeSeries = value;
        }

        public TimeSeries GateInitialOpeningTimeSeries => 
            culvert.GateInitialOpeningTimeSeries;

        public double GateLowEdgeLevel
        {
            get { return culvert.GateLowerEdgeLevel; }
        }

        public double FrictionValue
        {
            get { return culvert.Friction; }
            set { culvert.Friction = value; }
        }

        public double GroundLayerRoughness
        {
            get { return culvert.GroundLayerRoughness; }
            set { culvert.GroundLayerRoughness = value; }
        }

        public double GroundLayerThickness
        {
            get { return culvert.GroundLayerThickness; }
            set { culvert.GroundLayerThickness = value; }
        }

        public FastZWDataTable GeometryTabulated { get; set; }

        public double GeometryDiameter
        {
            get { return culvert.Diameter; }
            set { culvert.Diameter = value; }
        }

        public double GeometryWidth
        {
            get { return culvert.Width; }
            set { culvert.Width = value; }
        }

        public double GeometryHeight
        {
            get { return culvert.Height; }
            set { culvert.Height = value; }
        }

        public double GeometryArcHeight
        {
            get { return culvert.ArcHeight; }
            set { culvert.ArcHeight = value; }
        }

        public double GeometryRadiusR
        {
            get { return culvert.Radius; }
            set { culvert.Radius = value; }
        }

        public double GeometryRadiusR1
        {
            get { return culvert.Radius1; }
            set { culvert.Radius1 = value; }
        }

        public double GeometryRadiusR2
        {
            get { return culvert.Radius2; }
            set { culvert.Radius2 = value; }
        }

        public double GeometryRadiusR3
        {
            get { return culvert.Radius3; }
            set { culvert.Radius3 = value; }
        }

        public double GeometryAngleA
        {
            get { return culvert.Angle; }
            set { culvert.Angle = value; }
        }

        public double GeometryAngleA1
        {
            get { return culvert.Angle1; }
            set { culvert.Angle1 = value; }
        }

        #endregion

        #region View properties
        public bool IsInvertedSiphon { get; set; }
        public bool IsCulvert { get; set; }
        public bool IsTabulated { get; set; }
        public bool IsRound { get; set; }
        public bool IsSteelCunette { get; set; }
        public bool IsCunette { get; set; }
        public bool IsArch { get; set; }
        public bool IsEllipse { get; set; }
        public bool IsRectangle { get; set; }
        public bool IsEgg { get; set; }
        public bool IsInvertedEgg { get; set; }
        public bool IsUShape { get; set; }
        public bool GeometryWidthVisibility { get; set; }
        public bool GeometryHeightVisibility { get; set; }
        public bool BendLossCoeffVisibility { get; set; }
        public string GroundLayerRoughnessUnit { get; set; }
        public string GroundLayerThicknessUnit { get; set; }

        public bool HasArcHeight { get; set; }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (culvert == null)
            {
                return;
            }
            
            ((INotifyPropertyChanged) culvert).PropertyChanged -= UpdateAllProperties;
        }
    }
}
