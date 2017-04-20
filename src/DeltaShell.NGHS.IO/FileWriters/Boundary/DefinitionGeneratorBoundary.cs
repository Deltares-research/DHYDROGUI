using System;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Boundary
{
    public class DefinitionGeneratorBoundary : IDefinitionGeneratorBoundary
    {
        private IDelftBcCategory BcCategory { get; set; }

        public DefinitionGeneratorBoundary(string header)
        {
            BcCategory = new DelftBcCategory(header);
        }

        public IDelftBcCategory CreateRegion(string name, string function, string interpolation, string periodic = null)
        {
            AddCommonProperties(name, function, interpolation);
            if (!String.IsNullOrEmpty(periodic))
            {
                BcCategory.AddProperty(BoundaryRegion.Periodic.Key, periodic, BoundaryRegion.Periodic.Description);
            }
            return BcCategory;
        }
      
        private void AddCommonProperties(string name, string function, string interpolation)
        {
            BcCategory.AddProperty(BoundaryRegion.Name.Key, name, BoundaryRegion.Name.Description);
            BcCategory.AddProperty(BoundaryRegion.Function.Key, function, BoundaryRegion.Function.Description);
            BcCategory.AddProperty(BoundaryRegion.Interpolation.Key, interpolation, BoundaryRegion.Interpolation.Description);
        }
    }

    public interface IDefinitionGeneratorBoundary
    {
        IDelftBcCategory CreateRegion(string name, string function, string interpolation, string periodic = null);
    }
}
