using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;

namespace DelftTools.Hydro.SewerFeatures
{
    /// <summary>
    /// An instance creator for sewer cross section definitions.
    /// </summary>
    public static class SewerCrossSectionDefinitionFactory
    {
        public const string DefaultPipeProfileName = "Default Pipe Profile";

        /// <summary>
        /// The name of the default pump sewer structure profile.
        /// </summary>
        public const string DefaultPumpSewerStructureProfileName = "Default Pump sewer structure profile";

        /// <summary>
        /// The name of the default weir/orifice sewer structure profile.
        /// </summary>
        public const string DefaultWeirSewerStructureProfileName = "Default Weir/Orifice sewer structure profile";

        /// <summary>
        /// Creates the default pipe cross section definition.
        /// </summary>
        /// <returns>
        /// A default <see cref="CrossSectionDefinition"/> for pipes.
        /// </returns>
        internal static CrossSectionDefinition CreateDefaultPipeProfile()
        {
            var circleShape = new CrossSectionStandardShapeCircle { Diameter = 0.4 };
            return new CrossSectionDefinitionStandard(circleShape) { Name = DefaultPipeProfileName };
        }

        /// <summary>
        /// Creates the default pump sewer structure cross section definition.
        /// </summary>
        /// <returns>
        /// A default <see cref="CrossSectionDefinition"/> for pump sewer structures.
        /// </returns>
        internal static CrossSectionDefinition CreateDefaultPumpSewerStructureProfile()
        {
            var circleShape = new CrossSectionStandardShapeCircle { Diameter = 0.1 };
            return new CrossSectionDefinitionStandard(circleShape) { Name = DefaultPumpSewerStructureProfileName };
        }

        /// <summary>
        /// Creates the default weir/orifice sewer structure cross section definition.
        /// </summary>
        /// <returns>
        /// A default <see cref="CrossSectionDefinition"/> for weir/orifice sewer structures.
        /// </returns>
        internal static CrossSectionDefinition CreateDefaultWeirSewerStructureProfile()
        {
            var circleShape = new CrossSectionStandardShapeCircle { Diameter = 0.1 };
            return new CrossSectionDefinitionStandard(circleShape) { Name = DefaultWeirSewerStructureProfileName };
        }
    }
}