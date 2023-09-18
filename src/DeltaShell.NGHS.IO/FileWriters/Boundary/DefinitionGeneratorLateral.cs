using System;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Boundary
{
    public class DefinitionGeneratorBoundary : IDefinitionGeneratorBoundary
    {
        private DelftBcCategory BcCategory { get; set; }

        public DefinitionGeneratorBoundary(string header)
        {
            BcCategory = new DelftBcCategory(header);
        }

        public DelftBcCategory CreateRegion(string name, string function, string interpolation, string periodic = null)
        {
            AddCommonProperties(name, function, interpolation);
            if (!String.IsNullOrEmpty(periodic))
            {
                BcCategory.Section.AddPropertyWithOptionalComment(BoundaryRegion.Periodic.Key, periodic, BoundaryRegion.Periodic.Description);
            }
            return BcCategory;
        }
      
        private void AddCommonProperties(string name, string function, string interpolation)
        {
            BcCategory.Section.AddPropertyWithOptionalComment(BoundaryRegion.Name.Key, name, BoundaryRegion.Name.Description);
            BcCategory.Section.AddPropertyWithOptionalComment(BoundaryRegion.Function.Key, function, BoundaryRegion.Function.Description);
            BcCategory.Section.AddPropertyWithOptionalComment(BoundaryRegion.Interpolation.Key, interpolation, BoundaryRegion.Interpolation.Description);
        }
    }

    public interface IDefinitionGeneratorBoundary
    {
        DelftBcCategory CreateRegion(string name, string function, string interpolation, string periodic = null);
    }
}
