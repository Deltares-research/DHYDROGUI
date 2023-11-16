using System;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Tests.TestObjects;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class CrossSectionTest
    {
        [Test]
        public void MakeDefinitionLocalCreatesAShiftedCopyOfInnerDefinition()
        {
            var innerDefinition = CrossSectionDefinitionYZ.CreateDefault();

            //create a shifted proxy
            const double levelShift = 1.0;
            var proxy = new CrossSectionDefinitionProxy(innerDefinition) {LevelShift = levelShift};

            var hydroNetwork = HydroNetworkHelper.GetSnakeHydroNetwork(1);

            var crossSection = new CrossSection(proxy) { Branch = hydroNetwork.Channels.First() };

            crossSection.MakeDefinitionLocal();

            Assert.IsFalse(crossSection.Definition.IsProxy);
            Assert.IsTrue(crossSection.Definition is CrossSectionDefinitionYZ);
            Assert.AreEqual(crossSection.Definition.GetProfile(),innerDefinition.GetProfile().Select(c=>new Coordinate(c.X,c.Y+levelShift)).ToList());
        }
        
        [Test]
        public void MakeDefinitionSharedCopiesDefinitionToNetwork()
        {
            var crossSectionDefinitionYZ = CrossSectionDefinitionYZ.CreateDefault();

            var hydroNetwork = HydroNetworkHelper.GetSnakeHydroNetwork(1);

            var crossSection = new CrossSection(crossSectionDefinitionYZ) { Branch = hydroNetwork.Channels.First() };

            Assert.AreEqual(0,hydroNetwork.SharedCrossSectionDefinitions.Count);

            crossSection.ShareDefinitionAndChangeToProxy();

            Assert.AreEqual(1, hydroNetwork.SharedCrossSectionDefinitions.Count);
            Assert.AreEqual(crossSectionDefinitionYZ,hydroNetwork.SharedCrossSectionDefinitions.First());
        }

        [Test]
        public void DefaultDefinitionIsClonedWithHydroNetworkCopy()
        {
            // Create network with shared and default cross section defs.
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            var crossSectionDefinition = CrossSectionDefinitionYZ.CreateDefault();
            network.SharedCrossSectionDefinitions.Add(crossSectionDefinition);
            network.DefaultCrossSectionDefinition = crossSectionDefinition;

            // Create network copy.
            var networkCopy = (HydroNetwork)network.Clone();
            var defaultDefinition = networkCopy.DefaultCrossSectionDefinition;

            // Compare dafault cross section defs.
            Assert.AreEqual(networkCopy.SharedCrossSectionDefinitions.First(), defaultDefinition);
            Assert.AreEqual(crossSectionDefinition.GetType(), defaultDefinition.GetType());
        }

        [Test]
        public void CopyFrom()
        {
            var type = new CrossSectionSectionType {Name = "Main"};
            var sourceDefinition = new CrossSectionDefinitionYZ();
            sourceDefinition.AddSection(type, 20);

            var sourceCrossSection = new CrossSection(sourceDefinition);

            var targetDefinition = new CrossSectionDefinitionYZ();
            var targetCrossSection = new CrossSection(targetDefinition);

            targetCrossSection.CopyFrom(sourceCrossSection);
            
            Assert.AreEqual("Main",targetCrossSection.Definition.Sections[0].SectionType.Name);
        }

        [Test]
        public void Clone()
        {
            var crossSection = new TestCrossSectionDefinition("Test")
            {
                Thalweg = 3.0,
            };
            var type = new CrossSectionSectionType();
            crossSection.AddSection(type, 20);

            var clone = (TestCrossSectionDefinition)crossSection.Clone();
            
            Assert.AreEqual(crossSection.Thalweg,clone.Thalweg);
            Assert.AreEqual(crossSection.Sections.Count, clone.Sections.Count);
            Assert.AreNotSame(crossSection.Sections[0], clone.Sections[0]);
            Assert.AreEqual(crossSection.Sections[0].MinY, clone.Sections[0].MinY);
            Assert.AreEqual(crossSection.Sections[0].MaxY, clone.Sections[0].MaxY);
            Assert.AreSame(crossSection.Sections[0].SectionType, clone.Sections[0].SectionType);
        }

        [Test]
        public void ChangeInStorageDoesNotChangeProfile()
        {
            var crossSection = new CrossSectionDefinitionXYZ();

            var coordinates = new[]
                         {
                             new Coordinate(0, 0),
                             new Coordinate(2, 0),
                             new Coordinate(4, -10),
                             new Coordinate(6, -10),
                             new Coordinate(8, 0),
                             new Coordinate(10, 0)
                         };
            
            //make geometry on the y/z plane
            crossSection.Geometry = new LineString(coordinates.Select(c=>new Coordinate(0,c.X,c.Y)).ToArray());


            //since the profile is defined on the y/z plane we can ignore the x values
            Assert.AreEqual(coordinates, crossSection.GetProfile());
        }

        [Test]
        public void ModifyingOneSectionUpdatesTheOther()
        {
            var crossSectionDef = new CrossSectionDefinitionYZ {ForceSectionsSpanFullWidth = true};

            crossSectionDef.YZDataTable.AddCrossSectionYZRow(0, 5);
            crossSectionDef.YZDataTable.AddCrossSectionYZRow(5, 1);
            crossSectionDef.YZDataTable.AddCrossSectionYZRow(25, 1);
            crossSectionDef.YZDataTable.AddCrossSectionYZRow(35, 5);

            var sectionType = new CrossSectionSectionType();
            var section1 = new CrossSectionSection {MinY = 0, MaxY = 10, SectionType = sectionType};
            var section2 = new CrossSectionSection {MinY = 10, MaxY = 30, SectionType = sectionType};
            crossSectionDef.Sections.Add(section1);
            crossSectionDef.Sections.Add(section2);
            
            // action
            section1.MaxY = 15;

            Assert.AreEqual(15, section2.MinY);
            Assert.AreEqual(35, section2.MaxY);
        }

        [Test]
        public void ModifyingProfileUpdatesSections()
        {
            var crossSectionDef = new CrossSectionDefinitionYZ { ForceSectionsSpanFullWidth = true };

            crossSectionDef.YZDataTable.AddCrossSectionYZRow(0, 5);
            crossSectionDef.YZDataTable.AddCrossSectionYZRow(5, 1);
            crossSectionDef.YZDataTable.AddCrossSectionYZRow(25, 1);
            crossSectionDef.YZDataTable.AddCrossSectionYZRow(30, 5);

            var sectionType = new CrossSectionSectionType();
            var section1 = new CrossSectionSection { MinY = 0, MaxY = 10, SectionType = sectionType };
            var section2 = new CrossSectionSection { MinY = 10, MaxY = 30, SectionType = sectionType };
            crossSectionDef.Sections.Add(section1);
            crossSectionDef.Sections.Add(section2);

            // action
            crossSectionDef.YZDataTable.AddCrossSectionYZRow(35, 6);

            Assert.AreEqual(35, section2.MaxY);

            var crossSectionDefZw = new CrossSectionDefinitionZW {ForceSectionsSpanFullWidth = true};

            crossSectionDefZw.ZWDataTable.AddCrossSectionZWRow(8, 8, 0);
            crossSectionDefZw.ZWDataTable.AddCrossSectionZWRow(0, 10, 0);

            var sectionTypeZw = new CrossSectionSectionType();
            var section1Zw = new CrossSectionSection { MinY = 0, MaxY = 7, SectionType = sectionTypeZw };
            var section2Zw = new CrossSectionSection { MinY = 7, MaxY = 10, SectionType = sectionTypeZw };
            crossSectionDefZw.Sections.Add(section1Zw);
            crossSectionDefZw.Sections.Add(section2Zw);

            // action
            crossSectionDefZw.ZWDataTable.AddCrossSectionZWRow(15, 20, 0);

            Assert.AreEqual(10, section2Zw.MaxY);
        }

        [Test]
        public void CrossSectionWithXYZDefinitionCanNotShareDefiniton()
        {
            var crossSectionDefinitionXYZ = new CrossSectionDefinitionXYZ();
            var crossSection = new CrossSection(crossSectionDefinitionXYZ);

            var error = Assert.Throws<InvalidOperationException>(() => crossSection.ShareDefinitionAndChangeToProxy());
            Assert.AreEqual("XYZ definitions can not be shared", error.Message);
        }

        [Test]
        public void ChangeInDefinitionUpdatesGeometry()
        {
            //v-shaped cs 100 wide
            var crossSectionDefinitionYZ = new CrossSectionDefinitionYZ("");
            crossSectionDefinitionYZ.YZDataTable.AddCrossSectionYZRow(0, 100);
            crossSectionDefinitionYZ.YZDataTable.AddCrossSectionYZRow(50, 0);
            crossSectionDefinitionYZ.YZDataTable.AddCrossSectionYZRow(100, 100);
            crossSectionDefinitionYZ.Thalweg = 50;

            //horizontal line
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0,0),new Point(100,0));
            var branch = network.Channels.First();
            ICrossSection crossSection = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch, crossSectionDefinitionYZ, 50);
            

            var expectedGeometry = new LineString(new[] {new Coordinate(50, 50), new Coordinate(50, -50)});
            //use equals exact because rounding errors occur
            Assert.IsTrue(expectedGeometry.EqualsExact(crossSection.Geometry, 0.0001));
            
            //action : change the profile
            crossSectionDefinitionYZ.YZDataTable[0].Yq = -20;
            
            expectedGeometry = new LineString(new[] { new Coordinate(50, 70), new Coordinate(50, -50) });
            Assert.IsTrue(expectedGeometry.EqualsExact(crossSection.Geometry, 0.0001));

            //action: change the thalweg
            crossSectionDefinitionYZ.Thalweg = 40;

            expectedGeometry = new LineString(new[] { new Coordinate(50, 60), new Coordinate(50, -60) });
            Assert.IsTrue(expectedGeometry.EqualsExact(crossSection.Geometry, 0.0001));
        }
    }
}