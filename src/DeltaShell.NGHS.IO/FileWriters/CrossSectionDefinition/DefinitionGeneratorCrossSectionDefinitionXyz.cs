using System.Linq;
using DelftTools.Hydro.CrossSections;
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

        public override DelftIniCategory CreateDefinitionRegion(ICrossSectionDefinition crossSectionDefinition)
        {
            AddCommonProperties(crossSectionDefinition);
            AddCoordinates(crossSectionDefinition);

            IniCategory.AddProperty(DefinitionPropertySettings.Conveyance, DefinitionPropertySettings.Conveyance.DefaultValue);

            AddFrictionData(crossSectionDefinition);
            // can't create a protected base function! (because CrossSectionDefinitionXYZ != CrossSectionDefinitionYZ)
            var xyzCrossSectionDefinition = crossSectionDefinition.IsProxy ? ((CrossSectionDefinitionProxy)crossSectionDefinition).InnerDefinition as CrossSectionDefinitionXYZ : crossSectionDefinition as CrossSectionDefinitionXYZ;
            if (xyzCrossSectionDefinition == null) return IniCategory;

            var deltaZStorage = xyzCrossSectionDefinition.XYZDataTable.Select(row => row.DeltaZStorage);
            IniCategory.AddProperty(DefinitionPropertySettings.DeltaZStorage, deltaZStorage);

            return IniCategory;
        }

        private void AddCoordinates(ICrossSectionDefinition crossSectionDefinition)
        {
            var crossSectionDefinitionXyz = crossSectionDefinition as CrossSectionDefinitionXYZ;
            if (crossSectionDefinitionXyz == null) return;

            var xyzCount = crossSectionDefinitionXyz.Geometry.Coordinates.ToList().Count;
            IniCategory.AddProperty(DefinitionPropertySettings.XYZCount, xyzCount);

            var xCoordinates = crossSectionDefinitionXyz.Geometry.Coordinates.Select(c => c.X);
            IniCategory.AddProperty(DefinitionPropertySettings.XCoors, xCoordinates);

            var yCoordinates = crossSectionDefinitionXyz.Geometry.Coordinates.Select(c => c.Y);
            IniCategory.AddProperty(DefinitionPropertySettings.YCoors, yCoordinates);

            var zCoordinates = crossSectionDefinitionXyz.Geometry.Coordinates.Select(c => c.Z);
            IniCategory.AddProperty(DefinitionPropertySettings.ZCoors, zCoordinates);
        }

    }
}