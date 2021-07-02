using DelftTools.Hydro.CrossSections;

namespace DelftTools.Hydro.Structures
{
    public interface IBridge : IStructureWithCrossSectionDefinition, IFrictionData, IGroundLayer
    {
        string Name { get; set; }
        string LongName { get; set; }

        /// <summary>
        /// Crosssection of the bridge. If rectangle a single section tabulated is returned.
        /// </summary>
        /// <returns>Crosssection as used for ModelAPI</returns>
        CrossSectionDefinitionZW EffectiveCrossSectionDefinition { get; }

        /// <summary>
        /// Inlet loss
        /// </summary>
        double InletLossCoefficient { get; set; }
        
        /// <summary>
        /// Outlet loss
        /// </summary>
        double OutletLossCoefficient { get; set; }

        /// <summary>
        /// Allowed flow direction
        /// </summary>
        FlowDirection FlowDirection { get; set; }

        /// <summary>
        /// Frictiontype of the bridge bed
        /// </summary>
        BridgeFrictionType FrictionType { get; set; }
        
        /// <summary>
        /// Rectangle or tabulated geometry
        /// </summary>
        BridgeType BridgeType { get; set; }

        /// <summary>
        /// Bed level for rectangle geometry (TODO: rename to Shift, see FM1D2D-1671) 
        /// </summary>
        double BottomLevel { get; set;}

        /// <summary>
        /// Width of rectangle geometry
        /// </summary>
        double Width { get; set; }

        /// <summary>
        /// Height of rectangle geometry
        /// </summary>
        double Height { get; set; }

        /// <summary>
        /// Offset across the branch
        /// </summary>
        double OffsetY { get; set; }

        /// <summary>
        /// Duplicated for databinding to see length. Implemented on BranchFeature
        /// </summary>
        double Length { get; set; }

        /// <summary>
        /// Is negative flow possible
        /// </summary>
        bool AllowNegativeFlow { get; set; }

        /// <summary>
        /// Is positive flow possible
        /// </summary>
        bool AllowPositiveFlow { get; set; }

        /// <summary>
        /// Geometry used if type is tabulated
        /// </summary>
        CrossSectionDefinitionZW TabulatedCrossSectionDefinition { get; }

        /// <summary>
        /// Geometry used if type is Yz
        /// </summary>
        CrossSectionDefinitionYZ YZCrossSectionDefinition { get; }

        /// <summary>
        /// Total width of pillars
        /// </summary>
        double PillarWidth { get; set; }

        /// <summary>
        /// Shapefactor of the pillars
        /// </summary>
        double ShapeFactor { get; set; }
        
        /// <summary>
        /// ZW (used for databinding). Use bridgegeometrytype to find out about geometry etc
        /// </summary>
        bool IsTabulated { get; set; }
        
        /// <summary>
        /// YZ (used for databinding). Use bridgegeometrytype to find out about geometry etc
        /// </summary>
        bool IsYz { get; set; }

        /// <summary>
        /// Rectangle (used for databinding). Use bridgegeometrytype to find out about geometry etc
        /// </summary>
        bool IsRectangle { get; set; }

        /// <summary>
        /// Pillar (used for databinding). Use bridgegeometrytype to find out about geometry etc
        /// </summary>
        bool IsPillar { get; set; }
    }

    public enum BridgeType
    {
        Rectangle,
        Tabulated,
        YzProfile
    }
}