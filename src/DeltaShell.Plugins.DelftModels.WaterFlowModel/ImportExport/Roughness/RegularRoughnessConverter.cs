using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Roughness;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness
{
    public class RegularRoughnessConverter : RoughnessConverter
    {
        protected override RoughnessSection ReadRoughnessSection(IDelftIniCategory roughnessSectionCategory, IEnumerable<RoughnessSection> roughnessSections, IHydroNetwork network, IList<string> errorMessages)
        {
            RoughnessSection roughnessSection;
            var sectionId = roughnessSectionCategory.ReadProperty<string>(RoughnessDataRegion.SectionId.Key);
            var isReversed = roughnessSectionCategory.ReadProperty<bool>(RoughnessDataRegion.FlowDirection.Key);
            var hasGlobalType = roughnessSectionCategory.Properties.Any(p => p.Name == RoughnessDataRegion.GlobalType.Key);
            var interpolationType = (InterpolationType)roughnessSectionCategory.ReadProperty<int>(RoughnessDataRegion.Interpolate.Key);

            if (isReversed)
            {
                roughnessSection = GetReverseRoughnessSection(roughnessSections, sectionId, hasGlobalType, errorMessages);
                if (roughnessSection == null) return null;
            }
            else
            {
                var existingType = network.CrossSectionSectionTypes.FirstOrDefault(t => t.Name == sectionId);
                if (existingType == null)
                {
                    existingType = new CrossSectionSectionType { Name = sectionId };
                    network.CrossSectionSectionTypes.Add(existingType);
                }
                roughnessSection = new RoughnessSection(existingType, network);
            }

            if (!isReversed || hasGlobalType)
            {
                SetRoughnessDefaults(roughnessSection, roughnessSectionCategory);
            }
            roughnessSection.Name = sectionId;

            SetRoughnessInterpolationType(roughnessSection, interpolationType);

            return roughnessSection;
        }

        private static RoughnessSection GetReverseRoughnessSection(IEnumerable<RoughnessSection> roughnessSections, string sectionId, bool hasGlobalType, IList<string> errorMessages)
        {
            RoughnessSection roughnessSection;
            var normalSectionExists = roughnessSections.FirstOrDefault(rs => rs.Name == sectionId);
            if (normalSectionExists == null)
            {
                var message = Resources.RoughnessDataFileReader_ReadRoughnessSection_When_reading_reverse_roughness_section___0___the_referring__linked___normal__roughness_section___1___is_not_found__The_normal_section___1___should_be_imported_first_;
                errorMessages.Add(string.Format(message, sectionId + " (Reversed)", sectionId));
                return null;
            }

            roughnessSection = new ReverseRoughnessSection(normalSectionExists) { UseNormalRoughness = !hasGlobalType };
            return roughnessSection;
        }

        private static void SetRoughnessDefaults(RoughnessSection roughnessSection, IDelftIniCategory roughnessSectionCategory)
        {
            var globalType = FrictionTypeConverter.ConvertToRoughnessFrictionType(roughnessSectionCategory.ReadProperty<Friction>(RoughnessDataRegion.GlobalType.Key));
            var globalValue = roughnessSectionCategory.ReadProperty<double>(RoughnessDataRegion.GlobalValue.Key);
            roughnessSection.SetDefaults(globalType, globalValue);
        }

        private static void SetRoughnessInterpolationType(RoughnessSection roughnessSection, InterpolationType interpolationType)
        {
            var firstRoughnessNetworkCoverage = roughnessSection.RoughnessNetworkCoverage.Arguments.FirstOrDefault();
            if (firstRoughnessNetworkCoverage == null)
            {
                throw new FileReadingException(Resources.RoughnessDataFileReader_ReadRoughnessSection_While_creating_the_roughnes_section_from_the_roughness_file_the_fisrt_argument_of_the_roughness_network_coverage_is_not_created__used_to_set_the_interpolation_type);
            }

            firstRoughnessNetworkCoverage.InterpolationType = interpolationType;
        }
    }
}
