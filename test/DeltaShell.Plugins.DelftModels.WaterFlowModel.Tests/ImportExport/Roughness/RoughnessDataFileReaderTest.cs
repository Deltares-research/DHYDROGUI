using System;
using System.Collections.Generic;
using System.ComponentModel;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Roughness;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Roughness
{
    [TestFixture]
    public class RoughnessDataFileReaderTest : RoughnessFileReaderTestHelper
    {
        [TestCase("NonExistingFilePath")]
        [TestCase(null)]
        public void WhenReadingRoughnessFromANonExistingFile_ThenErrorMessagesAreReturnedAndNoRoughnessSectionHasBeenRead(string filePath)
        {
            var messages = string.Empty;
            Action<string, IList<string>> getReport = (header, errorMessages) => { messages += errorMessages; };

            var roughnessReader = new RegularRoughnessFileReader(getReport);
            var roughnessSection = roughnessReader.ReadFile("NonExistingFilePath", new HydroNetwork(), new List<RoughnessSection>());

            Assert.IsFalse(string.IsNullOrEmpty(messages));
            Assert.IsNull(roughnessSection);
        }

        [Test]
        public void TestRoughnessDataFileReader_With_Calibrated_RoughnessSectionFile()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(3);
            var crossSectionSectionType = new CrossSectionSectionType { Name = "Main" };
            var roughnessSection = new RoughnessSection(crossSectionSectionType, network);

            var roughnessFile = TestHelper.GetTestFilePath(@"FileReaders/roughness-Main.ini");

            //check original defaults:
            roughnessSection.SetDefaults(RoughnessType.DeBosAndBijkerk, 801.0d);
            Assert.That(roughnessSection.RoughnessNetworkCoverage.DefaultRoughnessType, Is.EqualTo(RoughnessType.DeBosAndBijkerk));
            Assert.That(roughnessSection.RoughnessNetworkCoverage.DefaultValue, Is.EqualTo(801.0).Within(0.0001));

            //check constant values:
            Assert.That(roughnessSection.RoughnessNetworkCoverage.Locations.Values.Count, Is.EqualTo(0));

            //no functions are set for the roughness section
            foreach (var branch in network.Branches)
            {
                Assert.That(() => roughnessSection.FunctionOfH(branch), Throws.Exception.TypeOf<KeyNotFoundException>());
                Assert.That(() => roughnessSection.FunctionOfQ(branch), Throws.Exception.TypeOf<KeyNotFoundException>());
            }

            new CalibratedRoughnessFileReader().ReadFile(roughnessFile, network, new[] { roughnessSection });
            CheckResults(roughnessSection, network);

            //re-read file & check to see if no duplicates are created
            new CalibratedRoughnessFileReader().ReadFile(roughnessFile, network, new[] { roughnessSection });
            CheckResults(roughnessSection, network);
        }

        [Test]
        public void ReadReverseRoughnessFile()
        {
            var network = (IHydroNetwork)MockRepository.GenerateStrictMock(typeof(IHydroNetwork), new[] { typeof(INotifyPropertyChanged), typeof(INotifyCollectionChanged) });

            var branches = new EventedList<IBranch> { new Branch { Name = "branch1" } };
            network.Expect(n => n.Branches).Return(branches).Repeat.Any();
            network.Expect(n => n.CoordinateSystem).Return(null).Repeat.Any();
            ((INotifyCollectionChanged)network).Expect(n => n.CollectionChanged += null).IgnoreArguments().Repeat.Twice();

            network.Replay();

            var path = TestHelper.GetTestFilePath(@"FileReaders\ReverseRoughness.ini");

            var orginalSection = new RoughnessSection(new CrossSectionSectionType { Name = "Test" }, network);

            var roughnessSections = new List<RoughnessSection> { orginalSection };

            var roughnessSection = new RegularRoughnessFileReader((header, errorMessages) => { }).ReadFile(path, network, roughnessSections);
            roughnessSections.Add(roughnessSection);

            Assert.AreEqual(2, roughnessSections.Count);
            var reversedSection = roughnessSections[1] as ReverseRoughnessSection;
            Assert.NotNull(reversedSection);

            Assert.AreEqual("Test (Reversed)", reversedSection.Name);
            Assert.AreEqual(true, reversedSection.Reversed);
            Assert.AreEqual(false, reversedSection.UseNormalRoughness);
            Assert.AreEqual(RoughnessType.Manning, reversedSection.GetDefaultRoughnessType());
            Assert.AreEqual(41, reversedSection.GetDefaultRoughnessValue());

            var coverage = reversedSection.RoughnessNetworkCoverage;
            Assert.AreEqual(InterpolationType.Linear, coverage.Arguments[0].InterpolationType);
            Assert.AreEqual(1, coverage.Locations.Values.Count);
        }

        [Test]
        public void ReadReverseRoughnessFileWithUseNormalRoughness()
        {
            var network = (IHydroNetwork)MockRepository.GenerateStrictMock(typeof(IHydroNetwork), new[] { typeof(INotifyPropertyChanged), typeof(INotifyCollectionChanged) });

            network.Expect(n => n.Branches).Return(new EventedList<IBranch>()).Repeat.Any();
            network.Expect(n => n.CoordinateSystem).Return(null).Repeat.Any();
            ((INotifyCollectionChanged)network).Expect(n => n.CollectionChanged += null).IgnoreArguments().Repeat.Twice();

            network.Replay();

            var path = TestHelper.GetTestFilePath(@"FileReaders\ReverseRoughnessUseNormalRoughness.ini");

            var orginalSection = new RoughnessSection(new CrossSectionSectionType { Name = "Test" }, network);
            orginalSection.SetDefaults(RoughnessType.WhiteColebrook, 2.2);
            var roughnessSections = new List<RoughnessSection> { orginalSection };

            var roughnessSection = new RegularRoughnessFileReader((header, errorMessages) => { }).ReadFile(path, network, roughnessSections);
            roughnessSections.Add(roughnessSection);

            Assert.AreEqual(2, roughnessSections.Count);
            var reversedSection = roughnessSections[1] as ReverseRoughnessSection;
            Assert.NotNull(reversedSection);

            Assert.AreEqual("Test (Reversed)", reversedSection.Name);
            Assert.AreEqual(true, reversedSection.Reversed);
            Assert.AreEqual(true, reversedSection.UseNormalRoughness);
            Assert.AreEqual(RoughnessType.WhiteColebrook, reversedSection.GetDefaultRoughnessType());
            Assert.AreEqual(2.2, reversedSection.GetDefaultRoughnessValue());
        }
    }
}