using DeltaShell.NGHS.IO.FileWriters.Location;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public class DefinitionGeneratorCrossSectionDefinitionUShape : DefinitionGeneratorCrossSectionDefinitionArch
    {
        public DefinitionGeneratorCrossSectionDefinitionUShape() : base(CrossSectionRegion.CrossSectionDefinitionType.UShape)
        {
        }
    }
}