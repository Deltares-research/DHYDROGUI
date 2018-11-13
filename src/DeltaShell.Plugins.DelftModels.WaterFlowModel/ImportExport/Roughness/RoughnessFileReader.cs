using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Roughness;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness
{
    public class RoughnessFileReader : RoughnessReader
    {
        private readonly Action<string, IList<string>> createAndAddErrorReport;

        public RoughnessFileReader(Action<string, IList<string>> createAndAddErrorReport)
        {
            this.createAndAddErrorReport = createAndAddErrorReport;
        }

        protected override RoughnessSection ReadRoughnessSection(INetwork network, IList<RoughnessSection> roughnessSections, IDelftIniCategory contentSection)
        {
            var sectionId = contentSection.ReadProperty<string>(RoughnessDataRegion.SectionId.Key);
            var isReversed = contentSection.ReadProperty<bool>(RoughnessDataRegion.FlowDirection.Key);
            var interpolationType = (InterpolationType)contentSection.ReadProperty<int>(RoughnessDataRegion.Interpolate.Key);

            var globalType = RoughnessType.Chezy;
            double? globalValue = null;

            RoughnessSection roughnessSection;
            
            var hasGlobalType = contentSection.Properties.Any(p => p.Name == RoughnessDataRegion.GlobalType.Key);

            if (isReversed)
            {
                var normalSectionExists = roughnessSections.FirstOrDefault(rs => rs.Name == sectionId);
                if (normalSectionExists != null)
                {
                    roughnessSection = new ReverseRoughnessSection(normalSectionExists) { UseNormalRoughness = !hasGlobalType };
                }
                else
                {
                    var message = Resources.RoughnessDataFileReader_ReadRoughnessSection_When_reading_reverse_roughness_section___0___the_referring__linked___normal__roughness_section___1___is_not_found__The_normal_section___1___should_be_imported_first_;
                    throw new FileReadingException(string.Format(message, sectionId + " (Reversed)", sectionId));
                }
            }
            else
            {
                var crossSectionSectionType = new CrossSectionSectionType { Name = sectionId };
                roughnessSection = new RoughnessSection(crossSectionSectionType, network);
            }

            if (!isReversed || hasGlobalType)
            {
                globalType = FrictionTypeConverter.ConvertToRoughnessFrictionType(contentSection.ReadProperty<Friction>(RoughnessDataRegion.GlobalType.Key));
                globalValue = contentSection.ReadProperty<double>(RoughnessDataRegion.GlobalValue.Key);
            }

            roughnessSection.Name = sectionId;
            

            if (roughnessSection.RoughnessNetworkCoverage == null)
                throw new FileReadingException(Resources.RoughnessDataFileReader_ReadRoughnessSection_While_creating_the_roughnes_section_from_the_roughness_file_the_roughness_network_coverage_is_not_created);

            var roughnessNetworkCoverage = roughnessSection.RoughnessNetworkCoverage;
            var firstNwcArgument = roughnessNetworkCoverage.Arguments.FirstOrDefault();

            if (firstNwcArgument == null)
                throw new FileReadingException(Resources.RoughnessDataFileReader_ReadRoughnessSection_While_creating_the_roughnes_section_from_the_roughness_file_the_fisrt_argument_of_the_roughness_network_coverage_is_not_created__used_to_set_the_interpolation_type);

            firstNwcArgument.InterpolationType = interpolationType;
            if (globalValue.HasValue)
                roughnessSection.SetDefaults(globalType, globalValue.Value);

            return roughnessSection;
        }
    }
}