using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Roughness;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness
{
    public class CalibratedRoughnessFileReader : RoughnessReader
    {
        protected override RoughnessSection ReadRoughnessSection(INetwork network, IList<RoughnessSection> roughnessSections, IDelftIniCategory contentSection)
        {
            var sectionId = contentSection.ReadProperty<string>(RoughnessDataRegion.SectionId.Key);
            var isReversed = contentSection.ReadProperty<bool>(RoughnessDataRegion.FlowDirection.Key);
            var interpolationType = (InterpolationType)contentSection.ReadProperty<int>(RoughnessDataRegion.Interpolate.Key);

            var roughnessSection = !isReversed
                ? roughnessSections.FirstOrDefault(rs => rs.Name == sectionId)
                : roughnessSections.OfType<ReverseRoughnessSection>().FirstOrDefault(rs => rs.NormalSection.Name == sectionId);

            if (roughnessSection == null)
                throw new FileReadingException(string.Format(Resources.RoughnessDataFileReader_ReadRoughnessSection_Could_not_import_calibrated_roughness_section__0__because_the_calibrated_roughness_section_you_want_to_import_doesn_t_exist_in_the_model, sectionId));

            //cleanup old roughnessdata
            foreach (var branch in roughnessSection.Network.Branches)
            {
                roughnessSection.RemoveRoughnessFunctionsForBranch(branch);
            }
            roughnessSection.RoughnessNetworkCoverage.Clear();

            var globalType = FrictionTypeConverter.ConvertToRoughnessFrictionType(contentSection.ReadProperty<Friction>(RoughnessDataRegion.GlobalType.Key));
            double? globalValue = contentSection.ReadProperty<double>(RoughnessDataRegion.GlobalValue.Key);
            

            if (roughnessSection.RoughnessNetworkCoverage == null)
                throw new FileReadingException(Resources.RoughnessDataFileReader_ReadRoughnessSection_While_creating_the_roughnes_section_from_the_roughness_file_the_roughness_network_coverage_is_not_created);

            var roughnessNetworkCoverage = roughnessSection.RoughnessNetworkCoverage;
            var firstNwcArgument = roughnessNetworkCoverage.Arguments.FirstOrDefault();

            if (firstNwcArgument == null)
                throw new FileReadingException(Resources.RoughnessDataFileReader_ReadRoughnessSection_While_creating_the_roughnes_section_from_the_roughness_file_the_fisrt_argument_of_the_roughness_network_coverage_is_not_created__used_to_set_the_interpolation_type);

            firstNwcArgument.InterpolationType = interpolationType;
            roughnessSection.SetDefaults(globalType, globalValue.Value);

            return roughnessSection;
        }
    }
}
