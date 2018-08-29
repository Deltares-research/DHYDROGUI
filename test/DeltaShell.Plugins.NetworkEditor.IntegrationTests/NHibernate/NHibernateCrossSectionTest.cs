using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.NetworkEditor.IntegrationTests.NHibernate
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    public class NHibernateCrossSectionTest : NHibernateHydroRegionTestBase
    {
        [Test]
        public void SaveLoadCrossSectionXYZ()
        {
            var crossSection = new CrossSectionDefinitionXYZ();

            var coordinates = new List<Coordinate>
                               {
                                   new Coordinate(1.0, 1.0, 0.0),
                                   new Coordinate(2.0, 1.0, 0.1),
                                   new Coordinate(3.0, 1.0, 0.1),
                                   new Coordinate(4.0, 1.0, 0.1)
                               };

            crossSection.Geometry = new LineString(coordinates.ToArray());
            //add some storage to the 1st point
            crossSection.XYZDataTable[0].DeltaZStorage = 5.0;

            var cs1 = new CrossSection(crossSection);

            var retrievedCrossSection = SaveLoadBranchFeature(cs1, TestHelper.GetCurrentMethodName() + ".dsproj");

            Assert.AreEqual(crossSection.Geometry, retrievedCrossSection.Geometry);
            Assert.IsTrue(crossSection.XYZDataTable.ContentEquals(retrievedCrossSection.Definition.RawData));
        }

        [Test]
        public void SaveLoadCrossSectionSummerdike()
        {
            var crossSection = new CrossSectionDefinitionZW
            {
                SummerDike = new SummerDike
                {
                    CrestLevel = 1,
                    FloodPlainLevel = 2,
                    FloodSurface = 3,
                    TotalSurface = 4
                }
            };
            var cs1 = new CrossSection(crossSection);

            var retrievedCrossSection = SaveLoadBranchFeature(cs1, TestHelper.GetCurrentMethodName() + ".dsproj");

            var retrievedDefinition = retrievedCrossSection.Definition as CrossSectionDefinitionZW;
            Assert.AreEqual(crossSection.SummerDike, retrievedDefinition.SummerDike);
        }

        [Test]
        public void SaveLoadCrossSectionYZ()
        {
            var crossSectionDefinition = new CrossSectionDefinitionYZ();

            crossSectionDefinition.YZDataTable.AddCrossSectionYZRow(10, 15, 7);
            crossSectionDefinition.YZDataTable.AddCrossSectionYZRow(20, 16, 8);

            var cs1 = new CrossSection(crossSectionDefinition) { LongName = "LongName" };

            var retrievedCrossSection = SaveLoadBranchFeature(cs1, TestHelper.GetCurrentMethodName() + ".dsproj");

            Assert.IsTrue(crossSectionDefinition.YZDataTable.ContentEquals(retrievedCrossSection.Definition.RawData));
            Assert.AreEqual(cs1.LongName, retrievedCrossSection.LongName);
            Assert.AreEqual(cs1.CrossSectionType, retrievedCrossSection.CrossSectionType);
            Assert.AreEqual(crossSectionDefinition.GeometryBased, retrievedCrossSection.Definition.GeometryBased);
            Assert.AreEqual(cs1.Geometry, retrievedCrossSection.Geometry);
            Assert.AreEqual(crossSectionDefinition.Profile, retrievedCrossSection.Definition.Profile);
        }

        [Test]
        public void SaveLoadCrossSectionStorage()
        {
            var crossSection = new CrossSectionDefinitionYZ();

            var yzCoordinates = new List<Coordinate>() { new Coordinate(0, 10), new Coordinate(10, 0), new Coordinate(20, 0), new Coordinate(30, 10) };

            crossSection.YZDataTable.SetWithCoordinates(yzCoordinates);

            var cs1 = new CrossSection(crossSection);

            var retrievedCrossSection = SaveLoadBranchFeature(cs1, TestHelper.GetCurrentMethodName() + ".dsproj");

            Assert.AreEqual(crossSection.FlowProfile.ToList(), retrievedCrossSection.Definition.FlowProfile.ToList());
        }

        [Test]
        public void SaveLoadCrossSectionZW()
        {
            var CrossSectionZW = new CrossSectionDefinitionZW();
            CrossSectionZW.IsClosed = true;
            CrossSectionZW.ZWDataTable.AddCrossSectionZWRow(0, 0, 0);
            CrossSectionZW.ZWDataTable.AddCrossSectionZWRow(5, 10, 2);
            CrossSectionZW.ZWDataTable.AddCrossSectionZWRow(10, 12, 0);

            //check a random value of summerdike..summerdike peristancy is checked elsewhere we just check the many-to-one here
            CrossSectionZW.SummerDike.FloodSurface = 8;

            var retrieved = SaveAndRetrieveObject(CrossSectionZW);
            Assert.IsTrue(retrieved.IsClosed);
            Assert.IsTrue(retrieved.ZWDataTable.ContentEquals(CrossSectionZW.ZWDataTable));
            Assert.AreEqual(8, retrieved.SummerDike.FloodSurface);
        }

        [Test]
        public void SaveLoadCrossSectionProxiedInNetwork()
        {
            var name = "zwName";
            var CrossSectionZW = new CrossSectionDefinitionZW(name);
            CrossSectionZW.IsClosed = true;
            CrossSectionZW.ZWDataTable.AddCrossSectionZWRow(0, 0, 0);
            CrossSectionZW.ZWDataTable.AddCrossSectionZWRow(5, 10, 2);
            CrossSectionZW.ZWDataTable.AddCrossSectionZWRow(10, 12, 0);

            var proxy = new CrossSectionDefinitionProxy(CrossSectionZW);
            var cs = new CrossSection(proxy);
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            var branch = network.Branches[0];
            NetworkHelper.AddBranchFeatureToBranch(cs, branch, 10.0);

            network.SharedCrossSectionDefinitions.Add(CrossSectionZW);

            //check a random value of summerdike..summerdike peristancy is checked elsewhere we just check the many-to-one here
            CrossSectionZW.SummerDike.FloodSurface = 8;

            var shift = 55.0;
            proxy.LevelShift = shift;

            var retrievedNetwork = SaveAndRetrieveObject(network);
            var retrieved = retrievedNetwork.CrossSections.First().Definition as CrossSectionDefinitionProxy;
            var retrievedZW = retrieved.InnerDefinition as CrossSectionDefinitionZW;

            Assert.IsTrue(retrieved.IsProxy);
            Assert.AreEqual(shift, retrieved.LevelShift);
            Assert.IsTrue(retrievedZW.ZWDataTable.ContentEquals(CrossSectionZW.ZWDataTable));
            Assert.AreEqual(8, retrievedZW.SummerDike.FloodSurface);
            Assert.AreEqual(name, retrievedZW.Name);
        }

        [Test]
        public void WriteAndReadProjectWithCrossSection()
        {
            var network = new HydroNetwork();

            INode fromNode = new HydroNode { Name = "From", Network = network, Geometry = new Point(1000, 1000) };
            INode toNode = new HydroNode { Name = "To", Network = network, Geometry = new Point(1000, 1500) };
            network.Nodes.Add(fromNode);
            network.Nodes.Add(toNode);

            var branch = CreateChannel(fromNode, toNode);
            network.Branches.Add(branch);

            var yzCoordinates = new List<Coordinate>
                                    {
                                        new Coordinate(0.0, 0.0),
                                        new Coordinate(1.0, 0.11)
                                    };

            var cs1 = CrossSectionHelper.AddXYZCrossSectionFromYZCoordinates(branch, 0.0, yzCoordinates);

            network.Name = "NHibernateProjectRepositoryTests.WriteAndReadProjectWithCrossSection";
            var project = new Project();

            project.RootFolder.Add(new DataItem(network));

            string path = TestHelper.GetCurrentMethodName() + ".dsproj";
            ProjectRepository.Create(path);
            ProjectRepository.SaveOrUpdate(project);
            ProjectRepository.Close();

            var retrievedProject = ProjectRepository.Open(path);
            var retrievedNetwork = (IHydroNetwork)retrievedProject.RootFolder.DataItems.FirstOrDefault().Value;
            var retrievedCrossSection = retrievedNetwork.CrossSections.First();

            Assert.AreEqual(cs1.Definition.GeometryBased, retrievedCrossSection.Definition.GeometryBased);
            //Assert.AreEqual(0.001, retrievedCrossSection.GetCrossSectionSection(1.0).Roughness, 1.0e-6);
            //Assert.AreEqual(RoughnessType.Manning, retrievedCrossSection.GetCrossSectionSection(1.0).RoughnessType);
            Assert.AreEqual(cs1.Geometry, retrievedCrossSection.Geometry);
        }

        [Test]
        public void ShareCrossSectionAndSave()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            var branch = network.Channels.First();


            var definitionZW = CrossSectionDefinitionZW.CreateDefault();
            var crossSection = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch, definitionZW, 0.0);

            network.Name = "NHibernateProjectRepositoryTests.WriteAndReadProjectWithCrossSection";
            var project = new Project();

            project.RootFolder.Add(new DataItem(network));

            string path = TestHelper.GetCurrentMethodName() + ".dsproj";
            ProjectRepository.Create(path);
            ProjectRepository.SaveOrUpdate(project);
            //change the definition of the CS...should result in deletion of the previous def
            var oldId = definitionZW.Id;

            //change it to a proxy and re-save
            crossSection.ShareDefinitionAndChangeToProxy();
            ProjectRepository.SaveOrUpdate(project);
            ProjectRepository.Close();

            var retrievedProject = ProjectRepository.Open(path);


        }

        [Test]
        public void SaveLoadCrossSectionStandard()
        {
            var crossSectionStandardShapeRound = new CrossSectionStandardShapeCircle
                                                     {
                                                         Diameter = 5
                                                     };
            var crossSectionDefinitionStandard = new CrossSectionDefinitionStandard(crossSectionStandardShapeRound)
                                                     {
                                                         LevelShift = 2
                                                     };

            var retrieved = SaveAndRetrieveObject(crossSectionDefinitionStandard);
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(2, retrieved.LevelShift);
            ICrossSectionStandardShape crossSectionStandardShape = retrieved.Shape;
            Assert.IsTrue(crossSectionStandardShape is CrossSectionStandardShapeCircle);
            var shape = (CrossSectionStandardShapeCircle)retrieved.Shape;
            Assert.AreEqual(5, shape.Diameter);
        }
        
        [Test]
        public void GivenArchStandardShape_WhenSaveLoadWithNHibernate_ThenPublicPropertiesArePreserved()
        {
            var shape = new CrossSectionStandardShapeArch();
            TestNHibernateSaveLoadFunctionality(shape);
        }

        [Test]
        public void GivenCunetteStandardShape_WhenSaveLoadWithNHibernate_ThenPublicPropertiesArePreserved()
        {
            var shape = new CrossSectionStandardShapeCunette();
            TestNHibernateSaveLoadFunctionality(shape);
        }

        [Test]
        public void GivenEggStandardShape_WhenSaveLoadWithNHibernate_ThenPublicPropertiesArePreserved()
        {
            var shape = new CrossSectionStandardShapeEgg();
            TestNHibernateSaveLoadFunctionality(shape);
        }

        [Test]
        public void GivenEllipticalStandardShape_WhenSaveLoadWithNHibernate_ThenPublicPropertiesArePreserved()
        {
            var shape = new CrossSectionStandardShapeElliptical();
            TestNHibernateSaveLoadFunctionality(shape);
        }

        [Test]
        public void GivenRectangleStandardShape_WhenSaveLoadWithNHibernate_ThenPublicPropertiesArePreserved()
        {
            var shape = new CrossSectionStandardShapeRectangle();
            TestNHibernateSaveLoadFunctionality(shape);
        }

        [Test]
        public void GivenRoundStandardShape_WhenSaveLoadWithNHibernate_ThenPublicPropertiesArePreserved()
        {
            var shape = new CrossSectionStandardShapeCircle();
            TestNHibernateSaveLoadFunctionality(shape);
        }

        [Test]
        public void GivenSteelCunetteStandardShape_WhenSaveLoadWithNHibernate_ThenPublicPropertiesArePreserved()
        {
            var shape = new CrossSectionStandardShapeSteelCunette();
            TestNHibernateSaveLoadFunctionality(shape);
        }

        [Test]
        public void GivenTrapeziumStandardShape_WhenSaveLoadWithNHibernate_ThenPublicPropertiesArePreserved()
        {
            var shape = new CrossSectionStandardShapeTrapezium
            {
                Slope = 35.0
            };
            TestNHibernateSaveLoadFunctionality(shape);
        }

        private void TestNHibernateSaveLoadFunctionality(CrossSectionStandardShapeBase shape)
        {
            ReflectionTestHelper.FillRandomValuesForValueTypeProperties(shape, "Type", "Slope");
            var retrievedShape = SaveAndRetrieveObject(shape);
            ReflectionTestHelper.AssertPublicPropertiesAreEqual(retrievedShape, shape);
        }
    }
}