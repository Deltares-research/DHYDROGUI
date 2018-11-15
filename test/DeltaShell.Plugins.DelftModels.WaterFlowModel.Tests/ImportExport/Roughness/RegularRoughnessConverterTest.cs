using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Roughness;
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
        [ExpectedException(typeof(FileReadingException))]
        public void GivenReversedRoughnessDataModelAndNoNormalRoughnessSectionAvailable_WhenConvertingToRoughnessSection_ThenErrorMessage()
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

            roughnessConverter.Convert(categories, network, roughnessSections, errorMessages);
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
    }
}