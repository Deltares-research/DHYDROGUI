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
            AddCommonRegionElements(crossSectionDefinition);
            
            AddCoordinates(crossSectionDefinition);

            AddValuesYz(crossSectionDefinition);

            // can't create a protected base function! (because CrossSectionDefinitionXYZ != CrossSectionDefinitionYZ)
            var xyzCrossSectionDefinition = crossSectionDefinition.IsProxy ? ((CrossSectionDefinitionProxy)crossSectionDefinition).InnerDefinition as CrossSectionDefinitionXYZ : crossSectionDefinition as CrossSectionDefinitionXYZ;
            if (xyzCrossSectionDefinition == null) return IniCategory;

            var deltaZStorage = xyzCrossSectionDefinition.XYZDataTable.Select(row => row.DeltaZStorage);
            IniCategory.AddProperty(DefinitionRegion.DeltaZStorage.Key, deltaZStorage, DefinitionRegion.DeltaZStorage.Description, DefinitionRegion.DeltaZStorage.Format);

            return IniCategory;
        }

        private void AddCoordinates(ICrossSectionDefinition crossSectionDefinition)
        {
            var crossSectionDefinitionXyz = crossSectionDefinition as CrossSectionDefinitionXYZ;
            if (crossSectionDefinitionXyz == null) return;

            var xyzCount = crossSectionDefinitionXyz.Geometry.Coordinates.ToList().Count;
            IniCategory.AddProperty(DefinitionRegion.XYZCount.Key, xyzCount, DefinitionRegion.XYZCount.Description);

            var xCoordinates = crossSectionDefinitionXyz.Geometry.Coordinates.Select(c => c.X);
            IniCategory.AddProperty(DefinitionRegion.XCoors.Key, xCoordinates, DefinitionRegion.XCoors.Description, DefinitionRegion.XCoors.Format);

            var yCoordinates = crossSectionDefinitionXyz.Geometry.Coordinates.Select(c => c.Y);
            IniCategory.AddProperty(DefinitionRegion.YCoors.Key, yCoordinates, DefinitionRegion.YCoors.Description, DefinitionRegion.YCoors.Format);

            var zCoordinates = crossSectionDefinitionXyz.Geometry.Coordinates.Select(c => c.Z);
            IniCategory.AddProperty(DefinitionRegion.ZCoors.Key, zCoordinates, DefinitionRegion.ZCoors.Description, DefinitionRegion.ZCoors.Format);
        }

    }
}