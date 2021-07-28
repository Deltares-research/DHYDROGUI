namespace DelftTools.Hydro.CrossSections.Extensions
{
    public static class CrossSectionExtensions
    {
        public static ICrossSectionDefinition GetCrossSectionDefinition(this ICrossSection crossSection)
        {
            return crossSection.Definition.GetBaseDefinition();
        }

        public static ICrossSectionDefinition GetBaseDefinition(this ICrossSectionDefinition crossSectionDefinition)
        {
            return crossSectionDefinition.IsProxy
                       ? ((CrossSectionDefinitionProxy) crossSectionDefinition).InnerDefinition
                       : crossSectionDefinition;
        }
    }
}