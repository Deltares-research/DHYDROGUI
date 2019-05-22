using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.DataSets;
using DelftTools.Utils;
using GeoAPI.Geometries;

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
                    List<CrossSectionDataSet.CrossSectionZWRow> zeroWidthEntries =
                        crossSectionZw.ZWDataTable.Rows.Where(c => Comparer.AlmostEqual2sComplement(c.Width, 0.0))
                                      .ToList();
                    switch (zeroWidthEntries.Count)
                    {
                        case 0:
                            return true;
                        case 1:
                            return Comparer.AlmostEqual2sComplement(crossSectionZw.ZWDataTable.Rows.First().Width,
                                                                    0.0) ||
                                   Comparer.AlmostEqual2sComplement(crossSectionZw.ZWDataTable.Rows.Last().Width, 0.0);
                        case 2:
                            return Comparer.AlmostEqual2sComplement(crossSectionZw.ZWDataTable.Rows.First().Width,
                                                                    0.0) &&
                                   Comparer.AlmostEqual2sComplement(crossSectionZw.ZWDataTable.Rows.Last().Width,
                                                                    0.0) &&
                                   crossSectionZw.ZWDataTable.Rows.Count() > 2;
                        default:
                            return false;
                    }
                }

                return true;
            }

            IEnumerable<Coordinate> flowProfile = crossSectionDefinition.FlowProfile;

            // Is it a profile
            if (flowProfile.Count() < 3)
            {
                return false;
            }

            // Are x values increment
            double lastX = double.MinValue;
            foreach (double x in flowProfile.Select(c => c.X))
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
                CrossSectionDefinitionStandard crossSectionDefinition = crossSection.Definition.IsProxy
                                                                            ? (CrossSectionDefinitionStandard)
                                                                            ((CrossSectionDefinitionProxy) crossSection
                                                                                    .Definition).InnerDefinition
                                                                            : (CrossSectionDefinitionStandard)
                                                                            crossSection.Definition;

                switch (crossSectionDefinition.ShapeType)
                {
                    case CrossSectionStandardShapeType.Rectangle:
                        return true;
                    case CrossSectionStandardShapeType.Elliptical:
                        return true;
                    case CrossSectionStandardShapeType.Cunette:
                        return true;
                    case CrossSectionStandardShapeType.SteelCunette:
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
                        CrossSectionDefinitionStandard crossSectionDefinition = crossSection.Definition.IsProxy
                                                                                    ? (CrossSectionDefinitionStandard)
                                                                                    ((CrossSectionDefinitionProxy)
                                                                                        crossSection.Definition)
                                                                                    .InnerDefinition
                                                                                    : (CrossSectionDefinitionStandard)
                                                                                    crossSection.Definition;
                        return crossSectionDefinition.ShapeType == CrossSectionStandardShapeType.Trapezium;
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

        public static bool AreCrossSectionsLengthsLargerThanTheFlowWidth(ICrossSectionDefinition crossSectionDefinition)
        {
            ICrossSectionDefinition csDefToCheck = crossSectionDefinition.IsProxy
                                                       ? GetUnProxiedCrossSectionDefinition(crossSectionDefinition)
                                                       : crossSectionDefinition;
            var crossSection = csDefToCheck as CrossSectionDefinition;
            return crossSection == null || IsTotalSectionsWidthAtLeastAsWideAsFlowWidth(crossSection);
        }

        public static bool AreFloodPlain1AndFloodPlain2WidthsValid(ICrossSectionDefinition crossSectionDefinition)
        {
            if (crossSectionDefinition.Sections.Count != 3)
            {
                return true;
            }

            ICrossSectionDefinition csDefToCheck = crossSectionDefinition.IsProxy
                                                       ? GetUnProxiedCrossSectionDefinition(crossSectionDefinition)
                                                       : crossSectionDefinition;
            var crossSectionZw = csDefToCheck as CrossSectionDefinitionZW;
            if (crossSectionZw == null)
            {
                return true;
            }

            double floodPlain1Width =
                crossSectionZw.GetSectionWidth(CrossSectionDefinitionZW.Floodplain1SectionTypeName);
            if (!floodPlain1Width.Equals(0.0))
            {
                return true;
            }

            double floodPlain2Width =
                crossSectionZw.GetSectionWidth(CrossSectionDefinitionZW.Floodplain2SectionTypeName);
            return !(floodPlain2Width > 0.0);
        }

        /// <summary>
        /// Checks if the first and last roughness positions are equal to the first and last y' value.
        /// If they are not equal a validation warning will be given.
        /// </summary>
        /// <remarks>
        /// Returning true when crossSectionDefinition.Sections.Count == 0 is a a fix for models that do not have roughness
        /// positions defined.
        /// The tolerance is set to 0.0001 to match the tolerance of the kernel.
        /// </remarks>
        /// <param name="crossSectionDefinition"> The cross section. </param>
        /// <returns> equals(true) or not equals(false) </returns>
        public static bool AreRoughnessPositionsEqualToFirstAndLastYValue(
            ICrossSectionDefinition crossSectionDefinition)
        {
            if (crossSectionDefinition.Sections.Count == 0)
            {
                return true;
            }

            double startRoughnessPosition = crossSectionDefinition.Sections.First().MinY;
            double endRoughnessPosition = crossSectionDefinition.Sections.Last().MaxY;

            double firstYValue = crossSectionDefinition.Left;
            double lastYValue = crossSectionDefinition.Profile.Last().X;

            return Math.Abs(startRoughnessPosition - firstYValue) < 0.0001 &&
                   Math.Abs(endRoughnessPosition - lastYValue) < 0.0001;
        }

        private static bool IsTotalSectionsWidthAtLeastAsWideAsFlowWidth(CrossSectionDefinition crossSection)
        {
            double sectionsTotalWidth = crossSection.SectionsTotalWidth();
            return sectionsTotalWidth - crossSection.FlowWidth() >= -1e-10;
        }

        private static ICrossSectionDefinition GetUnProxiedCrossSectionDefinition(
            ICrossSectionDefinition crossSectionDefinition)
        {
            return ((CrossSectionDefinitionProxy) crossSectionDefinition).GetUnProxiedDefinition();
        }
    }
}