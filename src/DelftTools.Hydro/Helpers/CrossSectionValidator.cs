using System;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils;

namespace DelftTools.Hydro.Helpers
{
    public static class CrossSectionValidator
    {
        public static bool IsFlowProfileValid(ICrossSectionDefinition crossSectionDefinition)
        {
            if (crossSectionDefinition.CrossSectionType == CrossSectionType.Standard)
            {
                return true;
            }

            if (crossSectionDefinition.CrossSectionType == CrossSectionType.ZW)
            {
                if (crossSectionDefinition.IsProxy)
                {
                    crossSectionDefinition = GetUnProxiedCrossSectionDefinition(crossSectionDefinition);
                }
                var crossSectionZw = crossSectionDefinition as CrossSectionDefinitionZW;
                if (crossSectionZw != null)
                {
                    var zeroWidthEntries =
                        crossSectionZw.ZWDataTable.Rows.Where(c => Comparer.AlmostEqual2sComplement(c.Width, 0.0)).ToList();
                    switch (zeroWidthEntries.Count)
                    {
                        case 0:
                            return true;
                        case 1:
                            return Comparer.AlmostEqual2sComplement(crossSectionZw.ZWDataTable.Rows.First().Width, 0.0) ||
                                   Comparer.AlmostEqual2sComplement(crossSectionZw.ZWDataTable.Rows.Last().Width, 0.0);
                        case 2:
                            return Comparer.AlmostEqual2sComplement(crossSectionZw.ZWDataTable.Rows.First().Width, 0.0) &&
                                   Comparer.AlmostEqual2sComplement(crossSectionZw.ZWDataTable.Rows.Last().Width, 0.0) &&
                                   crossSectionZw.ZWDataTable.Rows.Count() > 2;
                        default:
                            return false;

                    }
                }
                return true;
            }

            var flowProfile = crossSectionDefinition.FlowProfile;

            // Is it a profile
            if (flowProfile.Count() < 3)
            {
                return false;
            }

            // Are x values increment
            var lastX = double.MinValue;
            foreach (var x in flowProfile.Select(c => c.X))
            {
                if (x < lastX)
                {
                    return false;
                }

                lastX = x;
            }

            return true;
        }

        public static bool IsCrossSectionAllowedOnBranch(CrossSection crossSection, out string errorMessage)
        {
            errorMessage = "";

            // Allowed on all branches
            if (crossSection.CrossSectionType == CrossSectionType.Standard)
            {
                var crossSectionDefinition = crossSection.Definition.IsProxy
                                                 ? (CrossSectionDefinitionStandard) ((CrossSectionDefinitionProxy) crossSection.Definition).InnerDefinition
                                                 : (CrossSectionDefinitionStandard) crossSection.Definition;
                
                switch (crossSectionDefinition.ShapeType)
                {
                    case CrossSectionStandardShapeType.Rectangle:
                        return true;
                    case CrossSectionStandardShapeType.Elliptical:
                        return true;
                    case CrossSectionStandardShapeType.Cunette:
                        return true;
                    case  CrossSectionStandardShapeType.SteelCunette:
                        return true;
                    case CrossSectionStandardShapeType.Arch:
                        return true;
                }
            }

            // IChannel branches are defined as open branches
            if (crossSection.Branch is IChannel)
            {
                // Allowed only on open branches
                switch (crossSection.CrossSectionType)
                {
                    case CrossSectionType.ZW:
                        return true;
                    case CrossSectionType.YZ:
                        return true;
                    case CrossSectionType.GeometryBased:
                        return true;
                    case CrossSectionType.Standard:
                        var crossSectionDefinition = crossSection.Definition.IsProxy
                                                 ? (CrossSectionDefinitionStandard) ((CrossSectionDefinitionProxy)crossSection.Definition).InnerDefinition
                                                 : (CrossSectionDefinitionStandard) crossSection.Definition;
                        CrossSectionStandardShapeType crossSectionDefinitionShapeType = crossSectionDefinition.ShapeType;
                        return crossSectionDefinitionShapeType == CrossSectionStandardShapeType.Trapezium
                               || crossSectionDefinitionShapeType == CrossSectionStandardShapeType.Circle
                               || crossSectionDefinitionShapeType == CrossSectionStandardShapeType.Egg
                               || crossSectionDefinitionShapeType == CrossSectionStandardShapeType.InvertedEgg
                               || crossSectionDefinitionShapeType == CrossSectionStandardShapeType.UShape;
                }
            }

            // IPipe branches are defined as closed branches
            if (crossSection.Branch is IPipe)
            {
                errorMessage = "Cross-sections on enclosed branches are not supported.";
                
                return false;
            }

            return false;
        }

        public static bool AreCrossSectionsEqualToTheFlowWidth(ICrossSectionDefinition crossSectionDefinition)
        {
            var csDefToCheck = crossSectionDefinition.IsProxy ? GetUnProxiedCrossSectionDefinition(crossSectionDefinition) : crossSectionDefinition;
            var crossSection = csDefToCheck as CrossSectionDefinition;
            return crossSection == null || IsTotalSectionsWidthEqualToFlowWidth(crossSection);
        }

        public static bool AreFloodPlain1AndFloodPlain2WidthsValid(ICrossSectionDefinition crossSectionDefinition)
        {
            if (crossSectionDefinition.Sections.Count != 3) return true;

            var csDefToCheck = crossSectionDefinition.IsProxy ? GetUnProxiedCrossSectionDefinition(crossSectionDefinition) : crossSectionDefinition;
            var crossSectionZw = csDefToCheck as CrossSectionDefinitionZW;
            if (crossSectionZw == null) return true;

            var floodPlain1Width = crossSectionZw.GetSectionWidth(RoughnessDataSet.Floodplain1SectionTypeName);
            if (!floodPlain1Width.Equals(0.0)) return true;

            var floodPlain2Width = crossSectionZw.GetSectionWidth(RoughnessDataSet.Floodplain2SectionTypeName);
            return !(floodPlain2Width > 0.0);
        }

        private static bool IsTotalSectionsWidthEqualToFlowWidth(CrossSectionDefinition crossSectionDefinition)
        {
            // Skip trapezium
            if (crossSectionDefinition is CrossSectionDefinitionStandard stdCrossSection &&
                stdCrossSection.ShapeType == CrossSectionStandardShapeType.Trapezium)
            {
                return true;
            }

            // Also Skip cases in which no sections are defined
            if (!crossSectionDefinition.Sections.Any())
            {
                return true;
            }

            return Math.Abs(crossSectionDefinition.SectionsTotalWidth() - crossSectionDefinition.FlowWidth()) <= 1e-4;
        }

        private static ICrossSectionDefinition GetUnProxiedCrossSectionDefinition(ICrossSectionDefinition crossSectionDefinition)
        {
            return ((CrossSectionDefinitionProxy) crossSectionDefinition).GetUnProxiedDefinition();
        }
    }
}
