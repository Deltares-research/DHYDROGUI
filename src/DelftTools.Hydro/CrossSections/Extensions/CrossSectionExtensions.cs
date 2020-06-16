namespace DelftTools.Hydro.CrossSections.Extensions
{
    public static class CrossSectionExtensions
    {
        public static ICrossSectionDefinition GetCrossSectionDefinition(this ICrossSection crossSection)
        {
            var crossSectionDefinition = crossSection.Definition;

            return crossSectionDefinition.IsProxy
                ? ((CrossSectionDefinitionProxy) crossSectionDefinition).InnerDefinition
                : crossSectionDefinition;
        }
    }
}