using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;

namespace DelftTools.Hydro.SewerFeatures
{
    /// <summary>
    /// An instance creator for sewer connection cross section definitions.
    /// </summary>
    public class SewerCrossSectionDefinitionInstanceCreator
    {
        private const string defaultProfileDefinitionName = "Default Sewer Profile";
        private const string defaultPressurizedPipeSewerConnectionProfileName = "Default Pressurized Pipe Sewer Connection Profile";
        private const string defaultWeirSewerConnectionProfileName = "Default Weir/Orifice Sewer Connection Profile";

        /// <summary>
        /// Creates the default sewer cross section definition.
        /// </summary>
        /// <returns>
        /// A default <see cref="CrossSectionDefinition"/> for sewers.
        /// </returns>
        public CrossSectionDefinition CreateDefaultSewerProfile()
        {
            var circleShape = new CrossSectionStandardShapeCircle { Diameter = 0.4 };
            return new CrossSectionDefinitionStandard(circleShape) { Name = defaultProfileDefinitionName };
        }

        /// <summary>
        /// Creates the default pressurized pipe sewer connection cross section definition.
        /// </summary>
        /// <returns>
        /// A default <see cref="CrossSectionDefinition"/> for pressurized pipe sewer connections.
        /// </returns>
        public CrossSectionDefinition CreateDefaultPressurizedPipeSewerConnectionProfile()
        {
            var circleShape = new CrossSectionStandardShapeCircle { Diameter = 0.1 };
            return new CrossSectionDefinitionStandard(circleShape) { Name = defaultPressurizedPipeSewerConnectionProfileName };
        }

        /// <summary>
        /// Creates the default weir/orifice sewer connection cross section definition.
        /// </summary>
        /// <returns>
        /// A default <see cref="CrossSectionDefinition"/> for weir/orifice sewer connections.
        /// </returns>
        public CrossSectionDefinition CreateDefaultWeirSewerConnectionProfile()
        {
            var rectangleShape = new CrossSectionStandardShapeRectangle
            {
                Height = 10,
                Width = 10
            };
            return new CrossSectionDefinitionStandard(rectangleShape) { Name = defaultWeirSewerConnectionProfileName };
        }
    }
}