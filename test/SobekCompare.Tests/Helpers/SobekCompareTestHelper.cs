using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Collections;

namespace SobekCompare.Tests.Helpers
{
    public static class SobekCompareTestHelper
    {
        public static void RefreshCrossSectionDefinitionSectionWidths(IHydroNetwork network)
        {
            // fix for added validation (cross section definition sections total width should not be less than total cross section width
            network.CrossSections.Select(cs => cs.Definition)
                .OfType<CrossSectionDefinition>()
                .Union
                (
                    network.CrossSections.Select(cs => cs.Definition)
                        .OfType<CrossSectionDefinitionProxy>()
                        .Select(csdp => csdp.InnerDefinition)
                        .OfType<CrossSectionDefinition>()
                )
                .ForEach(csd => csd.RefreshSectionsWidths());
        }
    }
}
