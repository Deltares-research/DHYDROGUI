using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.FileWriters.SpatialData;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Roughness;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Roughness
{
    [TestFixture]
    public class RegularRoughnessConverterTest
    {
        private const string MainSectionName = "Main";

        [Test]
        public void GivenSimpleCorrectRoughnessDataModel_WhenConvertingToRoughnessSection_ThenCorrectRoughnessSectionIsReturned()
        {
            var contentCategory = CreateRoughnessContentCategory();
            var categories = new List<DelftIniCategory> {contentCategory};
            
            var roughnessConverter = new RegularRoughnessConverter();
            var network = new HydroNetwork();
            var errorMessages = new List<string>();
            var roughnessSection = roughnessConverter.Convert(categories, network, new List<RoughnessSection>(), errorMessages);

            Assert.IsNotNull(roughnessSection);
            Assert.That(roughnessSection.Name, Is.EqualTo("Main"));
            Assert.That(roughnessSection.GetDefaultRoughnessType(), Is.EqualTo(RoughnessType.DeBosAndBijkerk));
            Assert.That(roughnessSection.GetDefaultRoughnessValue(), Is.EqualTo(25.08));

            var roughnessNetworkCoverageArgument = roughnessSection.RoughnessNetworkCoverage.Arguments.FirstOrDefault();
            Assert.IsNotNull(roughnessNetworkCoverageArgument);
            Assert.That(roughnessNetworkCoverageArgument.InterpolationType, Is.EqualTo(InterpolationType.Linear));
        }

        [Test]
        public void GivenSimpleCorrectReversedRoughnessDataModel_WhenConvertingToRoughnessSection_ThenCorrectReversedRoughnessSectionIsReturned()
        {
            var reversedRoughnessCategory = CreateRoughnessContentCategory();
            reversedRoughnessCategory.SetProperty(RoughnessDataRegion.FlowDirection.Key, "True");
            var categories = new List<DelftIniCategory> { reversedRoughnessCategory };

            var roughnessConverter = new RegularRoughnessConverter();
            var network = new HydroNetwork();
            var errorMessages = new List<string>();
            var section = new RoughnessSection(new CrossSectionSectionType {Name = MainSectionName}, network );
            var roughnessSections = new List<RoughnessSection>{ section };
            var reversedRoughnessSection = roughnessConverter.Convert(categories, network, roughnessSections, errorMessages) as ReverseRoughnessSection;
            
            Assert.IsNotNull(reversedRoughnessSection);
            Assert.That(reversedRoughnessSection.Name, Is.EqualTo(MainSectionName + " (Reversed)"));
            Assert.That(reversedRoughnessSection.GetDefaultRoughnessType(), Is.EqualTo(RoughnessType.DeBosAndBijkerk));
            Assert.That(reversedRoughnessSection.GetDefaultRoughnessValue(), Is.EqualTo(25.08));
            Assert.IsFalse(reversedRoughnessSection.UseNormalRoughness);

            var roughnessNetworkCoverageArgument = reversedRoughnessSection.RoughnessNetworkCoverage.Arguments.FirstOrDefault();
            Assert.IsNotNull(roughnessNetworkCoverageArgument);
            Assert.That(roughnessNetworkCoverageArgument.InterpolationType, Is.EqualTo(InterpolationType.Linear));
        }

        [Test]
        public void GivenReversedRoughnessDataModelWithoutGlobalType_WhenConvertingToRoughnessSection_ThenUseNormalRoughnessIsTrue()
        {
            var reversedRoughnessCategory = CreateRoughnessContentCategory();
            reversedRoughnessCategory.SetProperty(RoughnessDataRegion.FlowDirection.Key, "True");
            reversedRoughnessCategory.Properties.RemoveAllWhere(p => p.Name == RoughnessDataRegion.GlobalType.Key);
            var categories = new List<DelftIniCategory> { reversedRoughnessCategory };

            var roughnessConverter = new RegularRoughnessConverter();
            var network = new HydroNetwork();
            var errorMessages = new List<string>();

            var section = new RoughnessSection(new CrossSectionSectionType { Name = MainSectionName }, network);
            var roughnessSections = new List<RoughnessSection> { section };
            var reversedRoughnessSection = roughnessConverter.Convert(categories, network, roughnessSections, errorMessages) as ReverseRoughnessSection;

            Assert.IsNotNull(reversedRoughnessSection);
            Assert.IsTrue(reversedRoughnessSection.UseNormalRoughness);
        }

        [Test]
        public void GivenReversedRoughnessDataModelAndNoNormalRoughnessSectionAvailableForThisReversedRoughness_WhenConvertingToRoughnessSection_ThenErrorMessageIsReturned()
        {
            var reversedRoughnessCategory = CreateRoughnessContentCategory();
            reversedRoughnessCategory.SetProperty(RoughnessDataRegion.FlowDirection.Key, "True");
            reversedRoughnessCategory.Properties.RemoveAllWhere(p => p.Name == RoughnessDataRegion.GlobalType.Key);
            var categories = new List<DelftIniCategory> { reversedRoughnessCategory };

            var roughnessConverter = new RegularRoughnessConverter();
            var network = new HydroNetwork();
            var errorMessages = new List<string>();

            var section = new RoughnessSection(new CrossSectionSectionType { Name = "SomeSectionName" }, network);
            var roughnessSections = new List<RoughnessSection> {section}; // Only sections available with a different name than defined in the reversedRoughnessCategory

            var roughnessSection = roughnessConverter.Convert(categories, network, roughnessSections, errorMessages);
            Assert.IsNull(roughnessSection);

            Assert.That(errorMessages.Count, Is.EqualTo(1));
            var expectedErrorMessage = string.Format(Resources.RoughnessDataFileReader_ReadRoughnessSection_When_reading_reverse_roughness_section___0___the_referring__linked___normal__roughness_section___1___is_not_found__The_normal_section___1___should_be_imported_first_, MainSectionName + " (Reversed)", MainSectionName);
            Assert.That(errorMessages.Contains(expectedErrorMessage));
        }

        [Test]
        public void GivenTwoRoughnessDataModelsWithTheSameName_WhenConvertingToRoughnessSection_ThenErrorMessageIsReturned()
        {
            var categories = new List<DelftIniCategory>
            {
                new DelftIniCategory(RoughnessDataRegion.ContentIniHeader),
                new DelftIniCategory(RoughnessDataRegion.ContentIniHeader)
            };

            var roughnessConverter = new RegularRoughnessConverter();
            var network = new HydroNetwork();
            var errorMessages = new List<string>();
            var roughnessSection = roughnessConverter.Convert(categories, network, new List<RoughnessSection>(), errorMessages);

            Assert.IsNull(roughnessSection);
            Assert.That(errorMessages.Count, Is.EqualTo(1));
            var expectedErrorMessage = string.Format(Resources.RoughnessConverter_Convert_Two_sections_were_found_with_same_header, RoughnessDataRegion.ContentIniHeader);
            Assert.That(errorMessages.Contains(expectedErrorMessage));
        }

        [Test]
        public void GivenRoughnessDataModelWithRoughnessSpecifiedForSpecificBranch_WhenConvertingToRoughnessSection_ThenCorrectRoughnessSectionIsReturned()
        {
            var myBranchId = "myBranch";
            var categories = new List<DelftIniCategory>
            {
                CreateRoughnessContentCategory(),
                CreateBranchPropertiesCategory(myBranchId),
                CreateDefinitionCategory(myBranchId)
            };

            var roughnessConverter = new RegularRoughnessConverter();
            var errorMessages = new List<string>();
            var network = new HydroNetwork();
            var channel = new Channel {Name = myBranchId};
            network.Branches.Add(channel);
            var roughnessSection = roughnessConverter.Convert(categories, network, new List<RoughnessSection>(), errorMessages);

            Assert.IsNotNull(roughnessSection);
            Assert.That(roughnessSection.Name, Is.EqualTo(MainSectionName));
            Assert.That(errorMessages.Count, Is.EqualTo(0));

            var networkCoverage = roughnessSection.RoughnessNetworkCoverage;
            var expectedNetworkLocation = new NetworkLocation(channel, 100.0);
            Assert.That(roughnessSection.GetRoughnessFunctionType(channel), Is.EqualTo(RoughnessFunction.Constant));
            Assert.That(networkCoverage.EvaluateRoughnessType(expectedNetworkLocation), Is.EqualTo(RoughnessType.WhiteColebrook));
            Assert.That(networkCoverage.EvaluateRoughnessValue(expectedNetworkLocation), Is.EqualTo(2.3));
        }

        private static DelftIniCategory CreateRoughnessContentCategory()
        {
            var category = new DelftIniCategory(RoughnessDataRegion.ContentIniHeader);
            category.AddProperty(RoughnessDataRegion.SectionId.Key, MainSectionName);
            category.AddProperty(RoughnessDataRegion.FlowDirection.Key, "False");
            category.AddProperty(RoughnessDataRegion.Interpolate.Key, "1");
            category.AddProperty(RoughnessDataRegion.GlobalType.Key, "9");
            category.AddProperty(RoughnessDataRegion.GlobalValue.Key, "25.08");
            return category;
        }

        private static DelftIniCategory CreateBranchPropertiesCategory(string branchId)
        {
            var category = new DelftIniCategory(RoughnessDataRegion.BranchPropertiesIniHeader);
            category.AddProperty(SpatialDataRegion.BranchId.Key, branchId);
            category.AddProperty(RoughnessDataRegion.RoughnessType.Key, "7");
            category.AddProperty(RoughnessDataRegion.FunctionType.Key, "0");
            return category;
        }

        private static DelftIniCategory CreateDefinitionCategory(string branchId)
        {
            var category = new DelftIniCategory(RoughnessDataRegion.DefinitionIniHeader);
            category.AddProperty(SpatialDataRegion.BranchId.Key, branchId);
            category.AddProperty(SpatialDataRegion.Chainage.Key, "100.000");
            category.AddProperty(SpatialDataRegion.Value.Key, "2.30000");
            return category;
        }
    }
}