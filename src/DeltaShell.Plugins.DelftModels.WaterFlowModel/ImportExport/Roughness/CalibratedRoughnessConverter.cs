using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Roughness;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness
{
    public class CalibratedRoughnessConverter : RoughnessConverter
    {
        protected override RoughnessSection ReadRoughnessSection(IDelftIniCategory roughnessSectionCategory, IEnumerable<RoughnessSection> roughnessSections, IHydroNetwork network)
        {
            var sectionId = roughnessSectionCategory.ReadProperty<string>(RoughnessDataRegion.SectionId.Key);
            var isReversed = roughnessSectionCategory.ReadProperty<bool>(RoughnessDataRegion.FlowDirection.Key);
            var interpolationType = (InterpolationType)roughnessSectionCategory.ReadProperty<int>(RoughnessDataRegion.Interpolate.Key);

            var roughnessSection = isReversed
                ? roughnessSections.OfType<ReverseRoughnessSection>().FirstOrDefault(rs => rs.NormalSection.Name == sectionId)
                : roughnessSections.FirstOrDefault(rs => rs.Name == sectionId);

            if (roughnessSection == null)
                throw new FileReadingException(string.Format(Resources.RoughnessDataFileReader_ReadRoughnessSection_Could_not_import_calibrated_roughness_section__0__because_the_calibrated_roughness_section_you_want_to_import_doesn_t_exist_in_the_model, sectionId));
            
            ClearRoughnessSectionData(roughnessSection);
            SetRoughnessDefaults(roughnessSection, roughnessSectionCategory);
            SetRoughnessInterpolationType(roughnessSection, interpolationType);

            return roughnessSection;
        }

        private static void ClearRoughnessSectionData(RoughnessSection roughnessSection)
        {
            foreach (var branch in roughnessSection.Network.Branches)
            {
                roughnessSection.RemoveRoughnessFunctionsForBranch(branch);
            }
            roughnessSection.RoughnessNetworkCoverage.Clear();
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
