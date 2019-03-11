using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Roughness;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections.Writer
{
    public static class RoughnessDataProcessor
    {
        public static DelftIniCategory AddRoughnessDataToFileContent(DelftIniCategory iniCategory, ICrossSection crossSection, IList<RoughnessSection> listOfRoughnessSections, bool useReverseRoughness)
        {
            var sectionSections = crossSection.Definition.Sections as IList<CrossSectionSection>;
            if (sectionSections.Count == 0)
            {
                IList<double> y = crossSection.Definition.Profile.Select(yz => yz.X).ToArray();

                IList<CrossSectionSection> crossSectionSections = new List<CrossSectionSection>
                {
                    new CrossSectionSection
                    {
                        MinY = y[0],
                        MaxY = y[y.Count - 1],
                        // always use "main"?; first is temporary fix
                        SectionType = listOfRoughnessSections[0].CrossSectionSectionType
                    }
                };
                sectionSections = crossSectionSections;

            }

            var roughnessPositions = sectionSections.Select(s => s.MinY).Union(sectionSections.Select(s => s.MaxY));
            var frictionNames = new List<string>();
            var frictionTypePositive = new List<int>();
            var frictionValuePositive = new List<double>();
            var frictionTypeNegative = new List<int>();
            var frictionValueNegative = new List<double>();

            var roughnessSections = sectionSections.Select(section => GetRoughnessSection(listOfRoughnessSections, section));
            foreach (var roughnessSection in roughnessSections)
            {
                frictionNames.Add(roughnessSection.Name);
                //The roughness values for YZ cannot be Q or H dependent (specifically: not Q dependent without major performance issues and changes to rekenhart). 
                //In the user interface this is not clear, so we need to add a validation warning. It does make life easier here, just use the coverage:
                frictionTypePositive.Add(
                    (int)
                    FrictionTypeConverter.ConvertFrictionType(
                        roughnessSection.EvaluateRoughnessType(crossSection.ToNetworkLocation())));
                //For YZ this is not constrained to be the same, but for tabulated it is. To keep things simple, in the UI it must be the same for all. 
                frictionTypeNegative.Add(frictionTypePositive.Last());

                frictionValuePositive.Add(roughnessSection.EvaluateRoughnessValue(crossSection.ToNetworkLocation()));
                frictionValueNegative.Add(useReverseRoughness
                    ? GetNegativeFrictionValue(listOfRoughnessSections, roughnessSection, crossSection)
                    : frictionValuePositive.Last());
            }

            iniCategory.AddProperty(DefinitionRegion.SectionCount.Key, sectionSections.Count.ToString(), DefinitionRegion.SectionCount.Description);
            iniCategory.AddProperty(DefinitionRegion.RoughnessNames.Key, string.Join(";", frictionNames), DefinitionRegion.RoughnessNames.Description);
            iniCategory.AddProperty(DefinitionRegion.RoughnessPositions.Key, roughnessPositions, DefinitionRegion.RoughnessPositions.Description, DefinitionRegion.RoughnessPositions.Format);
            iniCategory.AddProperty(DefinitionRegion.RoughnessTypesPos.Key, frictionTypePositive, DefinitionRegion.RoughnessTypesPos.Description);
            iniCategory.AddProperty(DefinitionRegion.RoughnessValuesPos.Key, frictionValuePositive, DefinitionRegion.RoughnessValuesPos.Description, DefinitionRegion.RoughnessValuesPos.Format);
            iniCategory.AddProperty(DefinitionRegion.RoughnessTypesNeg.Key, frictionTypeNegative, DefinitionRegion.RoughnessTypesNeg.Description);
            iniCategory.AddProperty(DefinitionRegion.RoughnessValuesNeg.Key, frictionValueNegative, DefinitionRegion.RoughnessValuesNeg.Description, DefinitionRegion.RoughnessValuesNeg.Format);
            return iniCategory;
        }

        private static double GetNegativeFrictionValue(IEnumerable<RoughnessSection> roughnessSections, RoughnessSection roughnessSection, IBranchFeature crossSection)
        {
            var reverseRoughnessSection = roughnessSections.GetApplicableReverseRoughnessSection(roughnessSection);
            return reverseRoughnessSection.RoughnessNetworkCoverage.EvaluateRoughnessValue(crossSection.ToNetworkLocation());
        }

        private static RoughnessSection GetRoughnessSection(IEnumerable<RoughnessSection> roughnessSections, CrossSectionSection crossSectionSection)
        {
            var roughnessSection = roughnessSections.FirstOrDefault(rs => rs.Name == crossSectionSection.SectionType.Name);
            if (roughnessSection == null)
            {
                throw new InvalidOperationException("No roughnessSection found with name " + crossSectionSection.SectionType.Name);
            }
            return roughnessSection;
        }
    }
}