using DelftTools.Controls;

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
        /// <summary>
        /// Generate a ReplaceVerifier.
        /// <param name="crossSectionDefinition">the object to generate a Replace verifier for</param>
        /// <param name="minimumAmountOfRows">minimumAmountOfRows used for verification</param>
        /// <returns>IReplaceVerifier</returns>
        /// </summary>
        public static IReplaceVerifier GenerateReplaceVerifier(this ICrossSectionDefinition crossSectionDefinition, int minimumAmountOfRows)
        {
            return crossSectionDefinition.CrossSectionType == CrossSectionType.YZ ? new YzCrossSectionAndMinimalNumberOfTableRowsReplaceVerifier(minimumAmountOfRows) : null;
        }

        /// <summary>
        /// Last handling to be taken to finish the paste.
        /// </summary>
        public static void FinishPasteHandling(this ICrossSectionDefinition crossSectionDefinition)
        {
            ICrossSectionPasteHandler pasteHandler = null;
            switch (crossSectionDefinition)
            {
                case CrossSectionDefinitionYZ crossSectionDefinitionYz:
                    pasteHandler = new YzCrossSectionPasteHandler(crossSectionDefinitionYz);
                    break;
            }
            pasteHandler?.FinishPasteActions();
        }
    }
}