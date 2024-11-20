using System;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Boundary
{
    public class DefinitionGeneratorBoundary : IDefinitionGeneratorBoundary
    {
        private BcIniSection BcIniSection { get; set; }

        public DefinitionGeneratorBoundary(string header)
        {
            BcIniSection = new BcIniSection(header);
        }

        public BcIniSection CreateRegion(string name, string function, string interpolation, string periodic = null)
        {
            AddCommonProperties(name, function, interpolation);
            if (!String.IsNullOrEmpty(periodic))
            {
                BcIniSection.Section.AddPropertyWithOptionalComment(BoundaryRegion.Periodic.Key, periodic, BoundaryRegion.Periodic.Description);
            }
            return BcIniSection;
        }
      
        private void AddCommonProperties(string name, string function, string interpolation)
        {
            BcIniSection.Section.AddPropertyWithOptionalComment(BoundaryRegion.Name.Key, name, BoundaryRegion.Name.Description);
            BcIniSection.Section.AddPropertyWithOptionalComment(BoundaryRegion.Function.Key, function, BoundaryRegion.Function.Description);
            BcIniSection.Section.AddPropertyWithOptionalComment(BoundaryRegion.Interpolation.Key, interpolation, BoundaryRegion.Interpolation.Description);
        }
    }

    public interface IDefinitionGeneratorBoundary
    {
        BcIniSection CreateRegion(string name, string function, string interpolation, string periodic = null);
    }
}
