using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.FileReaders.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.Roughness;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using NetTopologySuite.Extensions.Networks;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders
{
    [TestFixture]
    public class CrossSectionFileReaderTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFile_TwoLocationShareDefinition_DefinitionIsAddedToSharedCrossSectionDefinitions()
        {
            // Setup
            string fileContentLocations = string.Join(
                Environment.NewLine,
                "[General]",
                "    fileVersion           = 1.01",
                "    fileType              = crossLoc",
                "",
                "[CrossSection]",
                "    id                    = some_id_",
                "    branchId              = some_branch_id",
                "    chainage              = 1.23",
                "    shift                 = 2.34",
                "    definitionId          = some_definition_id",
                "",
                "[CrossSection]",
                "    id                    = some_id_2",
                "    branchId              = some_branch_id",
                "    chainage              = 5.67",
                "    shift                 = 6.78",
                "    definitionId          = some_definition_id"
            );

            string fileContentDefinitions = string.Join(
                Environment.NewLine,
                "[General]",
                "    fileVersion           = 3.00",
                "    fileType              = crossDef",
                "",
                "[Definition]",
                "    id                    = some_definition_id",
                "    type                  = yz",
                "    thalweg               = 50.0",
                "    singleValuedZ         = 1",
                "    yzCount               = 4",
                "    yCoordinates          = 0 33 66 100",
                "    zCoordinates          = 0 -10 -10 -0",
                "    sectionCount          = 1",
                "    frictionIds           = Channels"
            );

            using (var temp = new TemporaryDirectory())
            {
                string locationFile = temp.CreateFile("crsloc.ini", fileContentLocations);
                string definitionFile = temp.CreateFile("crsdef.ini", fileContentDefinitions);
                var network = new HydroNetwork();
                network.Branches.Add(new Branch { Name = "some_branch_id" });

                // Call
                CrossSectionFileReader.ReadFile(locationFile, definitionFile, network, null);

                // Assert
                Assert.That(network.SharedCrossSectionDefinitions, Has.Count.EqualTo(1));
                Assert.That(network.SharedCrossSectionDefinitions[0].Name, Is.EqualTo("some_definition_id"));
            }
        }

        [Test]
        public void CheckDefinitionNameLookup()
        {
            // arrange
            var hydroNetwork = Substitute.For<IHydroNetwork>();
            var csdCategory = Substitute.For<IDelftIniCategory>();
            var csdCategories = Enumerable.Repeat(csdCategory, 1);
            var contextToCheckFor = "MyContext";
            
            // act & assert
            TestHelper.AssertAtLeastOneLogMessagesContains(() =>
                {
                    var lookup = TypeUtils.CallPrivateStaticMethod(typeof(CrossSectionFileReader), "DefinitionNameLookup", hydroNetwork, csdCategories, contextToCheckFor);
                    Assert.That(lookup, Is.InstanceOf<IDictionary<string, ICrossSectionDefinition>>());
                }, string.Format(Resources.CrossSectionFileReader_CreateCrossSectionDefinitionFromCategory_No_definition_reader_available_for_this_cross_section_definition_type__0_, string.Empty)
            );
        }



        [Test]
        public void CheckDefinitionNameLookupWithData()
        {
            // arrange
            var hydroNetwork = Substitute.For<IHydroNetwork>();
            var csdCategory = (IDelftIniCategory)new DefinitionGeneratorCrossSectionDefinitionZw().CreateDefinitionRegion(CrossSectionDefinitionZW.CreateDefault("myCrossSection"), true, RoughnessDataSet.MainSectionTypeName);
            var csdCategories = Enumerable.Repeat(csdCategory, 10);
            var contextToCheckFor = "MyContext";

            // act & assert
            TestHelper.AssertAtLeastOneLogMessagesContains(() =>
            {
                var lookup = TypeUtils.CallPrivateStaticMethod(typeof(CrossSectionFileReader), "DefinitionNameLookup", hydroNetwork, csdCategories, contextToCheckFor);
                Assert.That(lookup, Is.InstanceOf<IDictionary<string, ICrossSectionDefinition>>());
            }, Resources.CrossSectionFileReader_DefinitionNameLookup_The_following_cross_section_entries_were_not_unique__);
        }
        
        [Test]
        public void CheckDefinitionNameLookupWithYZData()
        {
            // arrange
            var hydroNetwork = Substitute.For<IHydroNetwork>();
            var csdCategory = (IDelftIniCategory)new DefinitionGeneratorCrossSectionDefinitionYz().CreateDefinitionRegion(CrossSectionDefinitionYZ.CreateDefault("myCrossSection"), false, RoughnessDataRegion.SectionId.DefaultValue);
            var csdCategories = Enumerable.Repeat(csdCategory, 10);
            var contextToCheckFor = "MyContext";

            // act & assert
            TestHelper.AssertAtLeastOneLogMessagesContains(() =>
            {
                var lookup = TypeUtils.CallPrivateStaticMethod(typeof(CrossSectionFileReader), "DefinitionNameLookup", hydroNetwork, csdCategories, contextToCheckFor);
                Assert.That(lookup, Is.InstanceOf<IDictionary<string, ICrossSectionDefinition>>());
            }, Resources.CrossSectionFileReader_DefinitionNameLookup_The_following_cross_section_entries_were_not_unique__);
        }
        
        [Test]
        public void CheckDefinitionNameLookupWithYZDataCustomFrictions()
        {
            // arrange
            var hydroNetwork = Substitute.For<IHydroNetwork>();
            var crossSectionDefinitionYz = CrossSectionDefinitionYZ.CreateDefault("myCrossSection");
            var channelscssdType = Substitute.For<CrossSectionSectionType>();
            channelscssdType.Name = RoughnessDataRegion.SectionId.DefaultValue;

            var ft1cssdType = Substitute.For<CrossSectionSectionType>();
            ft1cssdType.Name = "frictionType1";

            var ft2cssdType = Substitute.For<CrossSectionSectionType>();
            ft2cssdType.Name = "frictionType2";

            crossSectionDefinitionYz.AddSection(channelscssdType, 50);
            crossSectionDefinitionYz.AddSection(ft1cssdType, 30);
            crossSectionDefinitionYz.AddSection(ft2cssdType, 20);

            var csdCategory = (IDelftIniCategory)new DefinitionGeneratorCrossSectionDefinitionYz().CreateDefinitionRegion(crossSectionDefinitionYz, true, RoughnessDataRegion.SectionId.DefaultValue);
            var csdCategories = Enumerable.Repeat(csdCategory, 10);
            var contextToCheckFor = "MyContext";


            // act & assert
            TestHelper.AssertAtLeastOneLogMessagesContains(() =>
            {
                var lookup = TypeUtils.CallPrivateStaticMethod(typeof(CrossSectionFileReader), "DefinitionNameLookup", hydroNetwork, csdCategories, contextToCheckFor);
                Assert.That(lookup, Is.InstanceOf<IDictionary<string, ICrossSectionDefinition>>());
            }, Resources.CrossSectionFileReader_DefinitionNameLookup_The_following_cross_section_entries_were_not_unique__);
        }
        
        [Test]
        public void CheckDefinitionNameLookupWithStdData()
        {
            // arrange
            var hydroNetwork = Substitute.For<IHydroNetwork>();
            var crossSectionDefinitionStd = CrossSectionDefinitionStandard.CreateDefault();
            crossSectionDefinitionStd.Name = "MyCrossSection";
            var csdCategory = (IDelftIniCategory)new DefinitionGeneratorCrossSectionDefinitionRectangle().CreateDefinitionRegion(crossSectionDefinitionStd, false, RoughnessDataRegion.SectionId.DefaultValue);
            var csdCategories = Enumerable.Repeat(csdCategory, 10);
            var contextToCheckFor = "MyContext";

            // act & assert
            TestHelper.AssertAtLeastOneLogMessagesContains(() =>
            {
                var lookup = TypeUtils.CallPrivateStaticMethod(typeof(CrossSectionFileReader), "DefinitionNameLookup", hydroNetwork, csdCategories, contextToCheckFor);
                Assert.That(lookup, Is.InstanceOf<IDictionary<string, ICrossSectionDefinition>>());
            }, Resources.CrossSectionFileReader_DefinitionNameLookup_The_following_cross_section_entries_were_not_unique__);
        }
        
        [Test]
        public void CheckDefinitionNameLookupWithStdDataCustomFrictions()
        {
            // arrange
            var hydroNetwork = Substitute.For<IHydroNetwork>();
            var crossSectionDefinitionStd = CrossSectionDefinitionStandard.CreateDefault();
            crossSectionDefinitionStd.Name = "MyCrossSection"; 
            
            var ft1cssdType = Substitute.For<CrossSectionSectionType>();
            ft1cssdType.Name = "frictionType1";

            var ft2cssdType = Substitute.For<CrossSectionSectionType>();
            ft2cssdType.Name = "frictionType2";

            crossSectionDefinitionStd.AddSection(ft1cssdType, 70);
            crossSectionDefinitionStd.AddSection(ft2cssdType, 30);

            var csdCategory = (IDelftIniCategory)new DefinitionGeneratorCrossSectionDefinitionRectangle().CreateDefinitionRegion(crossSectionDefinitionStd, true, RoughnessDataRegion.SectionId.DefaultValue);
            var csdCategories = Enumerable.Repeat(csdCategory, 10);
            var contextToCheckFor = "MyContext";
            
            // act & assert
            TestHelper.AssertAtLeastOneLogMessagesContains(() =>
            {
                var lookup = TypeUtils.CallPrivateStaticMethod(typeof(CrossSectionFileReader), "DefinitionNameLookup", hydroNetwork, csdCategories, contextToCheckFor);
                Assert.That(lookup, Is.InstanceOf<IDictionary<string, ICrossSectionDefinition>>());
            }, Resources.CrossSectionFileReader_DefinitionNameLookup_The_following_cross_section_entries_were_not_unique__);
        }
        
        [Test]
        public void CheckDefinitionNameLookupWithCrossSectionSectionData()
        {
            // arrange
            var hydroNetwork = Substitute.For<IHydroNetwork>();
            var crossSectionDefinitionZw = CrossSectionDefinitionZW.CreateDefault("myCrossSection");
            var maincssdType = Substitute.For<CrossSectionSectionType>();
            maincssdType.Name = RoughnessDataSet.MainSectionTypeName;

            var fp1cssdType = Substitute.For<CrossSectionSectionType>();
            fp1cssdType.Name = RoughnessDataSet.Floodplain1SectionTypeName;

            var fp2cssdType = Substitute.For<CrossSectionSectionType>();
            fp2cssdType.Name = RoughnessDataSet.Floodplain2SectionTypeName;

            crossSectionDefinitionZw.AddSection(maincssdType,50);
            crossSectionDefinitionZw.AddSection(fp1cssdType,30);
            crossSectionDefinitionZw.AddSection(fp2cssdType,20);
            var csdCategory = (IDelftIniCategory)new DefinitionGeneratorCrossSectionDefinitionZw().CreateDefinitionRegion(crossSectionDefinitionZw, true, RoughnessDataSet.MainSectionTypeName);
            var csdCategories = Enumerable.Repeat(csdCategory, 10000);
            var contextToCheckFor = "MyContext";

            // act & assert
            TestHelper.AssertAtLeastOneLogMessagesContains(() =>
            {
                var lookup = TypeUtils.CallPrivateStaticMethod(typeof(CrossSectionFileReader), "DefinitionNameLookup", hydroNetwork, csdCategories, contextToCheckFor);
                Assert.That(lookup, Is.InstanceOf<IDictionary<string, ICrossSectionDefinition>>());
            }, Resources.CrossSectionFileReader_DefinitionNameLookup_The_following_cross_section_entries_were_not_unique__);
        }

        [Test]
        public void CheckDefinitionNameLookupWithDataCheckContextStringValue()
        {
            // arrange
            var hydroNetwork = Substitute.For<IHydroNetwork>();
            var csdCategory = (IDelftIniCategory)new DefinitionGeneratorCrossSectionDefinitionZw().CreateDefinitionRegion(CrossSectionDefinitionZW.CreateDefault("myCrossSection"), true, RoughnessDataSet.MainSectionTypeName);
            var csdCategories = Enumerable.Repeat(csdCategory, 10);
            var contextToCheckFor = "MyContext";

            // act & assert
            TestHelper.AssertAtLeastOneLogMessagesContains(() =>
            {
                var lookup = TypeUtils.CallPrivateStaticMethod(typeof(CrossSectionFileReader), "DefinitionNameLookup", hydroNetwork, csdCategories, contextToCheckFor);
                Assert.That(lookup, Is.InstanceOf<IDictionary<string, ICrossSectionDefinition>>());
            }, contextToCheckFor);

        }

    }
}