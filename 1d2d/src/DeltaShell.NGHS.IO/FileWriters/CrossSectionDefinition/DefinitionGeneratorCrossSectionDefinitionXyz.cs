using System.Linq;
using DelftTools.Hydro.CrossSections;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public class DefinitionGeneratorCrossSectionDefinitionXyz : DefinitionGeneratorCrossSectionDefinitionYz
    {       
        public  DefinitionGeneratorCrossSectionDefinitionXyz()
            : base(CrossSectionRegion.CrossSectionDefinitionType.Xyz)
        {
        }

        public override IniSection CreateDefinitionRegion(
            ICrossSectionDefinition crossSectionDefinition,
            bool writeFrictionFromDefinition,
            string defaultFrictionId)
        {
            AddCommonProperties(crossSectionDefinition);

            AddCoordinates(crossSectionDefinition);

            IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.Conveyance, DefinitionPropertySettings.Conveyance.DefaultValue);

            AddFrictionData(crossSectionDefinition, writeFrictionFromDefinition, defaultFrictionId);
            // can't create a protected base function! (because CrossSectionDefinitionXYZ != CrossSectionDefinitionYZ)
            
            return IniSection;
        }

        private void AddCoordinates(ICrossSectionDefinition crossSectionDefinition)
        {
            var crossSectionDefinitionXyz = crossSectionDefinition as CrossSectionDefinitionXYZ;
            if (crossSectionDefinitionXyz == null) return;

            var xyzCount = crossSectionDefinitionXyz.Geometry.Coordinates.ToList().Count;
            IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.XYZCount, xyzCount);

            var xCoordinates = crossSectionDefinitionXyz.Geometry.Coordinates.Select(c => c.X);
            IniSection.AddPropertyFromConfigurationWithMultipleValues(DefinitionPropertySettings.XCoors, xCoordinates);

            var yCoordinates = crossSectionDefinitionXyz.Geometry.Coordinates.Select(c => c.Y);
            IniSection.AddPropertyFromConfigurationWithMultipleValues(DefinitionPropertySettings.YCoors, yCoordinates);

            var zCoordinates = crossSectionDefinitionXyz.Geometry.Coordinates.Select(c => c.Z);
            IniSection.AddPropertyFromConfigurationWithMultipleValues(DefinitionPropertySettings.ZCoors, zCoordinates);
        }
    }
}