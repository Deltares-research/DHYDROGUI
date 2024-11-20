using DelftTools.Functions;
using DelftTools.Hydro.CrossSections;

namespace DelftTools.Hydro.Structures
{
    public interface ICulvert : IStructureWithCrossSectionDefinition, 
                                IFrictionData, 
                                IGroundLayer
    {
        string LongName { get; set; }

        /// <summary>
        /// Geometry used if geometry type of culvert is tabulated
        /// </summary>
        CrossSectionDefinitionZW TabulatedCrossSectionDefinition { get; }

        /// <summary>
        /// Function Losscoefficient of gateopening 
        /// </summary>
        IFunction GateOpeningLossCoefficientFunction { get; set; }

        /// <summary>
        /// Crosssection of the culvert. The inlet level is added to the cross-section
        /// </summary>
        /// <returns>Crosssection as in StructureView</returns>
        CrossSectionDefinitionZW CrossSectionDefinitionAtInletAbsolute { get; }
        
        /// <summary>
        /// Crosssection of the culvert. The outlet level is added to the cross-section
        /// </summary>
        /// <returns>Crosssection as in StructureView</returns>
        CrossSectionDefinitionZW CrossSectionDefinitionAtOutletAbsolute { get; }

        /// <summary>
        /// Width of the culvert (if shape ellipse, rectangle, egg, arch, cunette)
        /// </summary>
        double Width { get; set; }

        /// <summary>
        /// Height of the culvert (if shape ellipse, rectangle, egg, arch, cunette, steelcunette)
        /// </summary>
        double Height { get; set; }

        /// <summary>
        /// ArcHeight of the culvert (if shape arch)
        /// </summary>
        double ArcHeight { get; set; }

        /// <summary>
        /// Diameter of the culvert (if shape round)
        /// </summary>
        double Diameter { get; set; }

        /// <summary>
        /// Radius of the culvert (if shape steel cunette)
        /// </summary>
        double Radius { get; set; }
    
        /// <summary>
        /// Radius1 of the culvert (if shape steel cunette)
        /// </summary>
        double Radius1 { get; set; }

        /// <summary>
        /// Radius2 of the culvert (if shape steel cunette)
        /// </summary>
        double Radius2 { get; set; }

        /// <summary>
        /// Radius3 of the culvert (if shape steel cunette)
        /// </summary>
        double Radius3 { get; set; }

        /// <summary>
        /// Angle of the culvert (if shape steel cunette)
        /// </summary>
        double Angle { get; set; }

        /// <summary>
        /// Angle1 of the culvert (if shape steel cunette)
        /// </summary>
        double Angle1 { get; set; }

        CulvertFrictionType FrictionType { get; set; }
        
        bool IsGated { get; set; }

        /// <summary>
        /// Gets or sets whether to use <see cref="GateInitialOpeningTimeSeries"/>, <c>true</c>, or
        /// <see cref="GateInitialOpening"/>, <c>false</c>.
        /// </summary>
        bool UseGateInitialOpeningTimeSeries { get; set; }

        /// <summary>
        /// Gets or sets the constant gate initial opening in [m].
        /// </summary>
        double GateInitialOpening { get; set; }

        /// <summary>
        /// Gets the gate initial opening <see cref="TimeSeries"/> in [m].
        /// </summary>
        TimeSeries GateInitialOpeningTimeSeries { get; }

        /// <summary>
        /// Bedlevel + GateOpening
        /// </summary>
        double GateLowerEdgeLevel { get; }

        double InletLossCoefficient { get; set; }

        double OutletLossCoefficient { get; set; }

        double InletLevel { get; set; }

        double OutletLevel { get; set; }

        /// <summary>
        /// Average bedlevel of the culvert. Used for crossectional view.
        /// </summary>
        double BottomLevel { get;}

        double BendLossCoefficient { get; set; }

        CulvertGeometryType GeometryType { get; set; }

        /// <summary>
        /// Direction in which water is allowed to flow
        /// </summary>
        FlowDirection FlowDirection { get; set; }

        /// <summary>
        /// Is negative flow possible
        /// </summary>
        bool AllowNegativeFlow { get; set; }

        /// <summary>
        /// Is positive flow possible
        /// </summary>
        bool AllowPositiveFlow { get; set; }

        /// <summary>
        /// Offset across the branch
        /// </summary>
        double OffsetY { get; set; }

        /// <summary>
        /// Crosssection as used for model api. It does not include any level since these are passed separately
        /// If rectangle a single section tabulated is returned. 
        /// </summary>
        CrossSectionDefinitionZW CrossSectionDefinitionForCalculation { get; }
        
        CulvertType CulvertType { get; set; }
        bool Closed { get; set; }
    }
}