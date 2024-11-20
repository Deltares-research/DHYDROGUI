using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Hydro.Tests.Helpers;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.ImportExport.GWSW.SewerFeatures;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.GWSW.Tests
{
    [TestFixture]
    public class ISewerFeatureTest : SewerFeatureFactoryTestHelper
    {
        #region Compartment

        [TestCase(TestSewerNetworkProvider.SourceManholeId)]
        [TestCase(null)]
        public void GivenSimpleSewerNetwork_WhenAddingCompartmentWithDuplicateName_ThenReplaceExistingCompartmentWithNewCompartment(string parentManholeId)
        {
            var network = TestSewerNetworkProvider.CreateSewerNetwork_OneSewerConnectionTwoManholesWithOneCompartmentEach();

            var newSurfaceLevel = 2.2;
            var newCompartment = new Compartment(TestSewerNetworkProvider.SourceCompartmentName) {SurfaceLevel = newSurfaceLevel, ParentManholeName = parentManholeId};
            AddSewerFeatureToNetwork(newCompartment, network);

            Assert.That((object) network.Nodes.Count, Is.EqualTo(2));
            Assert.That(Enumerable.Sum<IManhole>(network.Manholes, m => m.Compartments.Count), Is.EqualTo(2));

            var sourceManholeInNetwork = network.Nodes.FirstOrDefault(n => n.Name == TestSewerNetworkProvider.SourceManholeId) as Manhole;
            Assert.IsNotNull(sourceManholeInNetwork);
            Assert.That(sourceManholeInNetwork.Compartments.Count, Is.EqualTo(1));

            var sourceCompartmentInNetwork = sourceManholeInNetwork.GetCompartmentByName(TestSewerNetworkProvider.SourceCompartmentName);
            Assert.IsNotNull(sourceCompartmentInNetwork);
            Assert.That(sourceCompartmentInNetwork.SurfaceLevel, Is.EqualTo(newSurfaceLevel));
        }

        [TestCase(TestSewerNetworkProvider.SourceManholeId)]
        [TestCase(null)]
        public void GivenSimpleSewerNetwork_WhenAddingCompartmentWithDuplicateName_ThenNewCompartmentReplacesOldCompartmentAsSourceCompartment(string parentManholeId)
        {
            var network = TestSewerNetworkProvider.CreateSewerNetwork_OneSewerConnectionTwoManholesWithOneCompartmentEach();

            var newSurfaceLevel = 2.2;
            var newCompartment = new Compartment(TestSewerNetworkProvider.SourceCompartmentName) {SurfaceLevel = newSurfaceLevel, ParentManholeName = parentManholeId};
            AddSewerFeatureToNetwork(newCompartment, network);

            var sewerConnectionInNetwork = network.Branches.FirstOrDefault(b => b.Name == TestSewerNetworkProvider.SewerConnectionName) as SewerConnection;
            Assert.IsNotNull(sewerConnectionInNetwork);

            var sourceCompartment = sewerConnectionInNetwork.SourceCompartment;
            Assert.IsNotNull(sourceCompartment);
            Assert.That(sourceCompartment.Name, Is.EqualTo(TestSewerNetworkProvider.SourceCompartmentName));
            Assert.That(sourceCompartment.SurfaceLevel, Is.EqualTo(newSurfaceLevel));
        }

        [TestCase(TestSewerNetworkProvider.TargetManholeId)]
        [TestCase(null)]
        public void GivenSimpleSewerNetwork_WhenAddingCompartmentWithDuplicateName_ThenNewCompartmentReplacesOldCompartmentAsTargetCompartment(string parentManholeId)
        {
            var network = TestSewerNetworkProvider.CreateSewerNetwork_OneSewerConnectionTwoManholesWithOneCompartmentEach();

            var newSurfaceLevel = 2.2;
            var newCompartment = new Compartment(TestSewerNetworkProvider.TargetCompartmentName) {SurfaceLevel = newSurfaceLevel, ParentManholeName = parentManholeId};
            AddSewerFeatureToNetwork(newCompartment, network);

            var sewerConnectionInNetwork = network.Branches.FirstOrDefault(b => b.Name == TestSewerNetworkProvider.SewerConnectionName) as SewerConnection;
            Assert.IsNotNull(sewerConnectionInNetwork);

            var targetCompartment = sewerConnectionInNetwork.TargetCompartment;
            Assert.IsNotNull(targetCompartment);
            Assert.That(targetCompartment.Name, Is.EqualTo(TestSewerNetworkProvider.TargetCompartmentName));
            Assert.That(targetCompartment.SurfaceLevel, Is.EqualTo(newSurfaceLevel));
        }

        #endregion

        #region OutletCompartment

        [Test]
        public void GivenEmptyNetwork_WhenAddingOutletCompartmentToNetwork_OutletCompartmentIsPresentInNetworkWithCorrectValues()
        {
            var network = new HydroNetwork();

            var surfaceWaterLevel = 3.3;
            var parentManholeName = "myManhole";
            var outletCompartmentName = "myOutlet";
            var outletGeometry = new Point(2.2, 4.4);
            var outlet = new OutletCompartment(outletCompartmentName)
            {
                ParentManholeName = parentManholeName,
                SurfaceWaterLevel = surfaceWaterLevel,
                Geometry = outletGeometry
            };

            outlet.AddToHydroNetwork(network, null);
            Assert.That(network.Manholes.Count(), Is.EqualTo(1));

            var manhole = network.Manholes.FirstOrDefault(m => m.Name == parentManholeName);
            Assert.IsNotNull(manhole);
            Assert.That(manhole.Compartments.Count, Is.EqualTo(1));
            Assert.That((object) manhole.Geometry, Is.EqualTo(outletGeometry));

            var outletCompartment = manhole.Compartments.FirstOrDefault(c => c.Name == outletCompartmentName) as OutletCompartment;
            Assert.IsNotNull(outletCompartment);
            Assert.That(outletCompartment.SurfaceWaterLevel, Is.EqualTo(surfaceWaterLevel));
            Assert.That(outletCompartment.Geometry, Is.EqualTo(outletGeometry));
        }

        [TestCase(TestSewerNetworkProvider.SourceManholeId)]
        [TestCase(null)]
        public void GivenSimpleSewerNetwork_WhenAddingOutletCompartmentWithDuplicateName_ThenReplaceExistingCompartmentWithNewCompartment(string parentManholeId)
        {
            var network = TestSewerNetworkProvider.CreateSewerNetwork_OneSewerConnectionTwoManholesWithOneCompartmentEach();

            var surfaceWaterLevel = 2.2;
            var newCompartment = new OutletCompartment(TestSewerNetworkProvider.SourceCompartmentName) { SurfaceWaterLevel = surfaceWaterLevel, ParentManholeName = parentManholeId };
            AddSewerFeatureToNetwork(newCompartment, network);

            Assert.That((object) network.Nodes.Count, Is.EqualTo(2));
            Assert.That(Enumerable.Sum<IManhole>(network.Manholes, m => m.Compartments.Count), Is.EqualTo(2));

            var sourceManholeInNetwork = network.Nodes.FirstOrDefault(n => n.Name == TestSewerNetworkProvider.SourceManholeId) as Manhole;
            Assert.IsNotNull(sourceManholeInNetwork);
            Assert.That(sourceManholeInNetwork.Compartments.Count, Is.EqualTo(1));

            var sourceCompartmentInNetwork = sourceManholeInNetwork.GetCompartmentByName(TestSewerNetworkProvider.SourceCompartmentName) as OutletCompartment;
            Assert.IsNotNull(sourceCompartmentInNetwork);
            Assert.That(sourceCompartmentInNetwork.SurfaceWaterLevel, Is.EqualTo(surfaceWaterLevel));
        }

        #endregion

        #region SewerConnection

        [Test]
        public void GivenNetwork_WhenAddingSewerConnectionToNetwork_ThenIsPresentInTheNetwork()
        {
            var network = new HydroNetwork();
            var sewerConnection = new SewerConnection("mySewerConnection");

            AddSewerFeatureToNetwork(sewerConnection, network);
            Assert.IsTrue(network.SewerConnections.Contains(sewerConnection));
            Assert.That((object) sewerConnection.Network, Is.EqualTo(network));
        }

        [Test]
        public void GivenFmModel_WhenAddingSewerConnectionWithZeroLength_ThenStillTwoDiscretisationPointsHaveBeenAddedToTheModelNetwork()
        {
            var fmModel = new WaterFlowFMModel();
            var network = fmModel.Network;
            var manhole1 = new Manhole { Compartments = { new Compartment() } };
            var manhole2 = new Manhole { Compartments = { new Compartment() } };

            var sewerConnection = new SewerConnection("mySewerConnection")
            {
                Source = manhole1,
                Target = manhole2,
                Length = 0
            };

            sewerConnection.AddToHydroNetwork(network, null);
            Assert.That(Enumerable.Count(network.SewerConnections), Is.EqualTo(1));

            var discretizationLocations = fmModel.NetworkDiscretization.Locations.Values;
            Assert.That((object)discretizationLocations.Count, Is.EqualTo(2));
        }

        [Test]
        public void GivenSewerNetworkWithTwoManholes_WhenAddingSewerConnectionToNetwork_ThenCoordinatesAreSet()
        {
            var network = TestSewerNetworkProvider.CreateSewerNetwork_TwoManholesWithOneCompartmentEach();
            var sourceCompartmentName = TestSewerNetworkProvider.SourceCompartmentName;
            var targetCompartmentName = TestSewerNetworkProvider.TargetCompartmentName;

            var sewerConnection = new SewerConnection("mySewerConnection") {SourceCompartmentName = sourceCompartmentName, TargetCompartmentName = targetCompartmentName};
            AddSewerFeatureToNetwork(sewerConnection, network);

            var sourceManholeInNetwork = Enumerable.FirstOrDefault<IManhole>(network.Manholes, m => m.ContainsCompartmentWithName(sourceCompartmentName));
            var targetManholeInNetwork = Enumerable.FirstOrDefault<IManhole>(network.Manholes, m => m.ContainsCompartmentWithName(targetCompartmentName));
            Assert.IsNotNull(sourceManholeInNetwork);
            Assert.IsNotNull(targetManholeInNetwork);

            var sourceCompartmentInNetwork = sourceManholeInNetwork.GetCompartmentByName(sourceCompartmentName);
            var targetCompartmentInNetwork = targetManholeInNetwork.GetCompartmentByName(targetCompartmentName);
            Assert.IsNotNull(sourceCompartmentInNetwork);
            Assert.IsNotNull(targetCompartmentInNetwork);

            var sewerConnectionGeometryCoordinates = sewerConnection.Geometry.Coordinates;
            Assert.That((object) sewerConnectionGeometryCoordinates[0].X, Is.EqualTo(sourceCompartmentInNetwork.Geometry.Coordinate.X));
            Assert.That((object) sewerConnectionGeometryCoordinates[0].Y, Is.EqualTo(sourceCompartmentInNetwork.Geometry.Coordinate.Y));
            Assert.That((object) sewerConnectionGeometryCoordinates[1].X, Is.EqualTo(targetCompartmentInNetwork.Geometry.Coordinate.X));
            Assert.That((object) sewerConnectionGeometryCoordinates[1].Y, Is.EqualTo(targetCompartmentInNetwork.Geometry.Coordinate.Y));
        }

        [Test]
        public void GivenSewerNetworkWithTwoManholes_WhenAddingSewerConnectionToNetwork_ThenSourceCompartmentAndManholeAreSet()
        {
            var network = TestSewerNetworkProvider.CreateSewerNetwork_TwoManholesWithOneCompartmentEach();
            var sourceCompartmentName = TestSewerNetworkProvider.SourceCompartmentName;

            var sewerConnection = new SewerConnection("mySewerConnection") {SourceCompartmentName = sourceCompartmentName};
            AddSewerFeatureToNetwork(sewerConnection, network);

            var sourceManholeInNetwork = Enumerable.FirstOrDefault<IManhole>(network.Manholes, m => m.ContainsCompartmentWithName(sourceCompartmentName));
            Assert.IsNotNull(sourceManholeInNetwork);

            var sourceCompartmentInNetwork = sourceManholeInNetwork.GetCompartmentByName(sourceCompartmentName);
            Assert.IsNotNull(sourceCompartmentInNetwork);
            Assert.That(sewerConnection.SourceCompartment, Is.EqualTo(sourceCompartmentInNetwork));
            Assert.That(sewerConnection.Source, Is.EqualTo(sourceManholeInNetwork));
        }

        [Test]
        public void GivenSewerNetworkWithTwoManholes_WhenAddingSewerConnectionToNetwork_ThenTargetCompartmentAndManholeAreSet()
        {
            var network = TestSewerNetworkProvider.CreateSewerNetwork_TwoManholesWithOneCompartmentEach();
            var targetCompartmentName = TestSewerNetworkProvider.TargetCompartmentName;

            var sewerConnection = new SewerConnection("mySewerConnection") {TargetCompartmentName = targetCompartmentName};
            AddSewerFeatureToNetwork(sewerConnection, network);

            var targetManholeInNetwork = Enumerable.FirstOrDefault<IManhole>(network.Manholes, m => m.ContainsCompartmentWithName(targetCompartmentName));
            Assert.IsNotNull(targetManholeInNetwork);

            var targetCompartmentInNetwork = targetManholeInNetwork.GetCompartmentByName(targetCompartmentName);
            Assert.IsNotNull(targetCompartmentInNetwork);
            Assert.That(sewerConnection.TargetCompartment, Is.EqualTo(targetCompartmentInNetwork));
            Assert.That(sewerConnection.Target, Is.EqualTo(targetManholeInNetwork));
        }

        [Test]
        public void AddingOrificeToNetworkSequence1()
        {
            var network = new HydroNetwork();
            var orificeName = TestSewerNetworkProvider.OrificeName;
            
            var sewerConnection = new SewerConnection(orificeName);
            AddSewerFeatureToNetwork(sewerConnection, network);
            Assert.That(network.SewerConnections.Count(), Is.EqualTo(1));
            Assert.That(network.Orifices.Count(), Is.EqualTo(0));

            var orifice = new Orifice(orificeName);
            AddSewerFeatureToNetwork(orifice, network);
            Assert.That(network.SewerConnections.Count(), Is.EqualTo(1));
            Assert.That(network.Orifices.Count(), Is.EqualTo(1));
        }

        [Test]
        public void AddingOrificeToNetworkSequence2()
        {
            var network = new HydroNetwork();
            var orificeName = TestSewerNetworkProvider.OrificeName;

            var sewerConnection = new SewerConnection("myConnection");
            AddSewerFeatureToNetwork(sewerConnection, network);
            Assert.That(network.SewerConnections.Count(), Is.EqualTo(1));
            Assert.That(network.Orifices.Count(), Is.EqualTo(0));

            var orifice = new Orifice(orificeName);
            AddSewerFeatureToNetwork(orifice, network);
            Assert.That(network.SewerConnections.Count(), Is.EqualTo(2));
            Assert.That(network.Orifices.Count(), Is.EqualTo(1));
        }

        [Test]
        public void AddingOrificeToNetworkSequence6()
        {
            var network = TestSewerNetworkProvider.CreateSewerNetwork_TwoManholesWithOneCompartmentEachAndOneOrifice();
            var orificeName = TestSewerNetworkProvider.OrificeName;
            
            var orifice = new Orifice(orificeName);
            AddSewerFeatureToNetwork(orifice, network);
            Assert.That(Enumerable.Count<ISewerConnection>(network.SewerConnections), Is.EqualTo(1));
            Assert.That(Enumerable.Count<IOrifice>(network.Orifices), Is.EqualTo(1));

            AddSewerFeatureToNetwork(orifice, network);
            Assert.That(Enumerable.Count<ISewerConnection>(network.SewerConnections), Is.EqualTo(1));
            Assert.That(Enumerable.Count<IOrifice>(network.Orifices), Is.EqualTo(1));
        }

        #endregion

        #region Pipe

        [Test]
        public void GivenNetworkWithSharedCrossSection_WhenAddingPipeToNetwork_ThenSharedCrossSectionIsAddedToSewerConnection()
        {
            var network = TestSewerNetworkProvider.CreateSewerNetwork_OneSharedCrossSection();
            var csDefinitionName = TestSewerNetworkProvider.crossSectionDefinitionName;

            var pipeName = "mySewerConnection";
            var pipe = new Pipe {Name = pipeName, CrossSectionDefinitionName = csDefinitionName};
            AddSewerFeatureToNetwork(pipe, network);
            Assert.That(Enumerable.Count<IPipe>(network.Pipes), Is.EqualTo(1));
            Assert.That((object) network.SharedCrossSectionDefinitions.Count, Is.EqualTo(1));

            var pipeInNetwork = Enumerable.FirstOrDefault<IPipe>(network.Pipes);
            var sharedDefinitionInNetwork = Enumerable.FirstOrDefault<ICrossSectionDefinition>(network.SharedCrossSectionDefinitions);
            Assert.IsNotNull(pipeInNetwork);
            Assert.That(((CrossSectionDefinitionProxy) pipeInNetwork.CrossSection.Definition).InnerDefinition, Is.EqualTo(sharedDefinitionInNetwork));
            Assert.That(pipeInNetwork.Material, Is.EqualTo(SewerProfileMapping.SewerProfileMaterial.Concrete));
        }

        [Test]
        public void GivenNetworkWithSharedCrossSection_WhenAddingPipeToNetworkWithoutCrossSectionDefinitionId_ThenDefaultSharedCrossSectionIsAddedToSewerConnection()
        {
            var network = TestSewerNetworkProvider.CreateSewerNetwork_OneSharedCrossSection();

            var pipeName = "mySewerConnection";
            var pipe = new Pipe {Name = pipeName};
            AddSewerFeatureToNetwork(pipe, network);
            Assert.That(Enumerable.Count<IPipe>(network.Pipes), Is.EqualTo(1));
            Assert.That((object) network.SharedCrossSectionDefinitions.Count, Is.EqualTo(2)); // 1 added in test nw + default pipe csd

            var pipeInNetwork = Enumerable.FirstOrDefault<IPipe>(network.Pipes);
            Assert.IsNotNull(pipeInNetwork);

            var pipeCrossSectionDefinition = pipeInNetwork.CrossSection?.Definition;
            var expectedCrossSectionDefinition = (CrossSectionDefinitionStandard) CrossSectionDefinitionStandard.CreateDefault();
            Assert.That(pipeCrossSectionDefinition.CrossSectionType, Is.EqualTo(expectedCrossSectionDefinition.CrossSectionType));
            Assert.That(pipeCrossSectionDefinition, Is.TypeOf<CrossSectionDefinitionProxy>());
            ICrossSectionDefinition crossSectionDefinition = ((CrossSectionDefinitionProxy)pipeCrossSectionDefinition).InnerDefinition;
            Assert.That(crossSectionDefinition, Is.TypeOf<CrossSectionDefinitionStandard>());
            var crossSectionDefinitionStandard = ((CrossSectionDefinitionStandard)crossSectionDefinition);
            Assert.That(crossSectionDefinitionStandard.ShapeType, Is.EqualTo(CrossSectionStandardShapeType.Circle));
            Assert.That(((CrossSectionStandardShapeCircle)crossSectionDefinitionStandard.Shape).Diameter, Is.EqualTo(0.4));
            Assert.That(pipe.LevelSource, Is.EqualTo(-10.0));
            Assert.That(pipe.LevelTarget, Is.EqualTo(-10.0));
        }

        #endregion

        #region Orifice

        [Test]
        public void GivenSimpleSewerNetwork_WhenAddingGwswConnectionOrificeToNetwork_ThenSewerConnectionIsAddedWithAnOrificeAsBranchFeature()
        {
            var orificeName = "myOrifice";
            var sourceCompartmentName = TestSewerNetworkProvider.SourceCompartmentName;
            var targetCompartmentName = TestSewerNetworkProvider.TargetCompartmentName;

            var network = TestSewerNetworkProvider.CreateSewerNetwork_TwoManholesWithOneCompartmentEach();
            var levelSource = 0.7;
            var levelTarget = 1.1;
            var length = 44.0;
            var sewerConnectionWaterType = SewerConnectionWaterType.StormWater;
            var orifice = new GwswConnectionOrifice(orificeName)
            {
                LevelSource = levelSource,
                LevelTarget = levelTarget,
                Length = length,
                WaterType = sewerConnectionWaterType,
                SourceCompartmentName = sourceCompartmentName,
                TargetCompartmentName = targetCompartmentName
            };
            AddSewerFeatureToNetwork(orifice, network);
            Assert.That<int>(network.SewerConnections.Count, Is.EqualTo(1));

            var sewerConnection = Enumerable.FirstOrDefault<ISewerConnection>(network.SewerConnections);
            Assert.IsNotNull(sewerConnection);
            Assert.That(sewerConnection.Name, Is.EqualTo(orificeName));
            Assert.That(sewerConnection.SourceCompartment.Name, Is.EqualTo(sourceCompartmentName));
            Assert.That(sewerConnection.TargetCompartment.Name, Is.EqualTo(targetCompartmentName));
            Assert.That(sewerConnection.SourceCompartmentName, Is.EqualTo(sourceCompartmentName));
            Assert.That(sewerConnection.TargetCompartmentName, Is.EqualTo(targetCompartmentName));
            Assert.That(sewerConnection.LevelSource, Is.EqualTo(levelSource));
            Assert.That(sewerConnection.LevelTarget, Is.EqualTo(levelTarget));
            Assert.That(sewerConnection.Length, Is.EqualTo(length));
            Assert.That(sewerConnection.WaterType, Is.EqualTo(sewerConnectionWaterType));

            var branchFeatures = sewerConnection.BranchFeatures;
            Assert.That(branchFeatures.Count, Is.EqualTo(2));

            var orificeInNetwork = branchFeatures.FirstOrDefault(bf => bf is Orifice) as Orifice;
            Assert.IsNotNull(orificeInNetwork);
            Assert.That((object) orificeInNetwork.Name, Is.EqualTo(orificeName));
        }

        [Test]
        public void GivenSimpleSewerNetwork_WhenAddingGwswStructureOrificeToNetwork_ThenSewerConnectionIsAddedWithAnOrificeAsBranchFeature()
        {
            var orificeName = "myOrifice";

            var network = TestSewerNetworkProvider.CreateSewerNetwork_TwoManholesWithOneCompartmentEach();
            var crestLevel = 0.7;
            var correctionCoefficient = 1.1;
            var maxDischarge = 44.0;
            var orifice = new Orifice(orificeName)
            {
                CrestLevel = crestLevel,
                MaxDischarge = maxDischarge,
                WeirFormula = new GatedWeirFormula
                {
                    ContractionCoefficient= correctionCoefficient
                }
            };
            AddSewerFeatureToNetwork(orifice, network);
            Assert.That<int>(network.SewerConnections.Count, Is.EqualTo(1));

            var sewerConnection = Enumerable.FirstOrDefault<ISewerConnection>(network.SewerConnections);
            Assert.IsNotNull(sewerConnection);
            Assert.That(sewerConnection.Name, Is.EqualTo(orificeName));
            Assert.IsNull(sewerConnection.SourceCompartmentName);
            Assert.IsNull(sewerConnection.TargetCompartmentName);

            var branchFeatures = sewerConnection.BranchFeatures;
            Assert.That(branchFeatures.Count, Is.EqualTo(2));

            var orificeInNetwork = branchFeatures.FirstOrDefault(bf => bf is Orifice) as Orifice;
            Assert.IsNotNull(orificeInNetwork);
            Assert.That((object) orificeInNetwork.Name, Is.EqualTo(orificeName));
            Assert.That(orificeInNetwork.CrestLevel, Is.EqualTo(crestLevel));
            Assert.That(((GatedWeirFormula)orificeInNetwork.WeirFormula).ContractionCoefficient, Is.EqualTo(correctionCoefficient));
            Assert.That(orificeInNetwork.MaxDischarge, Is.EqualTo(maxDischarge));
        }

        [Test]
        public void GivenSewerNetworkWithTwoManholesAndOneOrifice_WhenAddingOrificeWithSameIdToNetwork_ThenTheCorrectValuesHaveBeenSet()
        {
            var orificeName = "myOrifice";
            var sourceCompartmentName = TestSewerNetworkProvider.SourceCompartmentName;
            var targetCompartmentName = TestSewerNetworkProvider.TargetCompartmentName;

            var network = TestSewerNetworkProvider.CreateSewerNetwork_TwoManholesWithOneCompartmentEachAndOneOrifice();
            var maxDischarge = 5.5;
            var contractionCoefficent = 4.4;
            var crestLevel = 3.3;
            var orifice = new Orifice(orificeName)
            {
                CrestLevel = crestLevel,
                MaxDischarge = maxDischarge,
                WeirFormula = new GatedWeirFormula
                {
                    ContractionCoefficient = contractionCoefficent
                }
            };

            AddSewerFeatureToNetwork(orifice, network);
            Assert.That<int>(network.SewerConnections.Count, Is.EqualTo(1));

            var sewerConnectionInNetwork = Enumerable.FirstOrDefault<ISewerConnection>(network.SewerConnections);
            Assert.IsNotNull(sewerConnectionInNetwork);
            Assert.That(sewerConnectionInNetwork.Name, Is.EqualTo(orificeName));
            Assert.That(sewerConnectionInNetwork.SourceCompartment.Name, Is.EqualTo(sourceCompartmentName));
            Assert.That(sewerConnectionInNetwork.TargetCompartment.Name, Is.EqualTo(targetCompartmentName));
            Assert.That(sewerConnectionInNetwork.SourceCompartmentName, Is.EqualTo(sourceCompartmentName));
            Assert.That(sewerConnectionInNetwork.TargetCompartmentName, Is.EqualTo(targetCompartmentName));
            
            var sewerConnectionBranchFeatures = sewerConnectionInNetwork.BranchFeatures;
            Assert.That(sewerConnectionBranchFeatures.Count, Is.EqualTo(2));

            var orificeInNetwork = sewerConnectionBranchFeatures.FirstOrDefault(bf => bf is Orifice) as Orifice;
            Assert.IsNotNull(orificeInNetwork);
            Assert.That((object) orificeInNetwork.Name, Is.EqualTo(orificeName));
            Assert.That(orificeInNetwork.CrestLevel, Is.EqualTo(crestLevel));
            Assert.That(((GatedWeirFormula)orificeInNetwork.WeirFormula).ContractionCoefficient, Is.EqualTo(contractionCoefficent));
            Assert.That(orificeInNetwork.MaxDischarge, Is.EqualTo(maxDischarge));
        }

        [Test]
        public void GivenSewerNetworkWithTwoManholesAndOneOrifice_WhenAddingGwswConnectionOrificeWithSameIdToNetwork_ThenTheCorrectValuesHaveBeenSet()
        {
            var orificeName = "myOrifice";
            var sourceCompartmentName = TestSewerNetworkProvider.SourceCompartmentName;
            var targetCompartmentName = TestSewerNetworkProvider.TargetCompartmentName;

            var network = TestSewerNetworkProvider.CreateSewerNetwork_TwoManholesWithOneCompartmentEachAndOneOrifice();
            var levelSource = 0.7;
            var levelTarget = 1.1;
            var length = 44.0;
            var sewerConnectionWaterType = SewerConnectionWaterType.StormWater;
            var allowNegativeFlow = false;
            var allowPositiveFlow = true;
            var gwswConnectionOrifice = new GwswConnectionOrifice(orificeName)
            {
                LevelSource = levelSource,
                LevelTarget = levelTarget,
                Length = length,
                WaterType = sewerConnectionWaterType,
                SourceCompartmentName = sourceCompartmentName,
                TargetCompartmentName = targetCompartmentName,
                AllowNegativeFlow = allowNegativeFlow,
                AllowPositiveFlow = allowPositiveFlow
            };

            AddSewerFeatureToNetwork(gwswConnectionOrifice, network);
            Assert.That(network.SewerConnections.Count, Is.EqualTo(1));

            var sewerConnection = network.SewerConnections.FirstOrDefault();
            Assert.IsNotNull(sewerConnection);
            Assert.That(sewerConnection.Name, Is.EqualTo(orificeName));
            Assert.That(sewerConnection.SourceCompartment.Name, Is.EqualTo(sourceCompartmentName));
            Assert.That(sewerConnection.TargetCompartment.Name, Is.EqualTo(targetCompartmentName));
            Assert.That(sewerConnection.SourceCompartmentName, Is.EqualTo(sourceCompartmentName));
            Assert.That(sewerConnection.TargetCompartmentName, Is.EqualTo(targetCompartmentName));
            Assert.That(sewerConnection.LevelSource, Is.EqualTo(levelSource));
            Assert.That(sewerConnection.LevelTarget, Is.EqualTo(levelTarget));
            Assert.That(sewerConnection.Length, Is.EqualTo(length));
            Assert.That(sewerConnection.WaterType, Is.EqualTo(sewerConnectionWaterType));

            var branchFeatures = sewerConnection.BranchFeatures;
            Assert.That(branchFeatures.Count, Is.EqualTo(2));

            var orifice = branchFeatures.FirstOrDefault(bf => bf is Orifice) as Orifice;
            Assert.IsNotNull(orifice);
            Assert.That(orifice.Name, Is.EqualTo(orificeName));
            Assert.That(orifice.AllowPositiveFlow, Is.EqualTo(allowPositiveFlow));
            Assert.That(orifice.AllowNegativeFlow, Is.EqualTo(allowNegativeFlow));

            var formula = orifice.WeirFormula as GatedWeirFormula;
            Assert.IsNotNull(formula);
            Assert.That(formula.UseMaxFlowNeg, Is.False);
            Assert.That(formula.UseMaxFlowPos, Is.False);
        }

        [Test]
        public void GivenFmModel_WhenAddingConnectionOrificeAndThenStructureOrifice_ThenDiscretisationPointsHaveBeenAddedToTheModelNetwork()
        {
            var fmModel = new WaterFlowFMModel { Network = TestSewerNetworkProvider.CreateSewerNetwork_TwoManholesWithOneCompartmentEach() };

            var orificeName = "myOrifice";
            var structureOrifice = new Orifice(orificeName);
            var connectionOrifice = new GwswConnectionOrifice(orificeName)
            {
                Length = 22.3,
                SourceCompartmentName = TestSewerNetworkProvider.SourceCompartmentName,
                TargetCompartmentName = TestSewerNetworkProvider.TargetCompartmentName
            };

            connectionOrifice.AddToHydroNetwork(fmModel.Network, null);
            Assert.That(Enumerable.Count<ISewerConnection>(fmModel.Network.SewerConnections), Is.EqualTo(1));

            var discretizationLocations = fmModel.NetworkDiscretization.Locations.Values;
            Assert.That((object) discretizationLocations.Count, Is.EqualTo(2));

            structureOrifice.AddToHydroNetwork(fmModel.Network, null);
            Assert.That(Enumerable.Count<ISewerConnection>(fmModel.Network.SewerConnections), Is.EqualTo(1));
            Assert.That((object) discretizationLocations.Count, Is.EqualTo(2));
        }

        [Test]
        public void GivenFmModel_WhenAddingStructureOrificeAndThenConnectionOrifice_ThenDiscretisationPointsHaveBeenAddedToTheModelNetwork()
        {
            var fmModel = new WaterFlowFMModel { Network = TestSewerNetworkProvider.CreateSewerNetwork_TwoManholesWithOneCompartmentEach() };

            var orificeName = "myOrifice";
            var structureOrifice = new Orifice(orificeName);
            var connectionOrifice = new GwswConnectionOrifice(orificeName)
            {
                Length = 22.3,
                SourceCompartmentName = TestSewerNetworkProvider.SourceCompartmentName,
                TargetCompartmentName = TestSewerNetworkProvider.TargetCompartmentName
            };

            structureOrifice.AddToHydroNetwork(fmModel.Network, null);
            Assert.That(Enumerable.Count(fmModel.Network.SewerConnections), Is.EqualTo(1));

            var discretizationLocations = fmModel.NetworkDiscretization.Locations.Values;
            Assert.That((object) discretizationLocations.Count, Is.EqualTo(1));

            connectionOrifice.AddToHydroNetwork(fmModel.Network, null);
            Assert.That(Enumerable.Count(fmModel.Network.SewerConnections), Is.EqualTo(1));
            Assert.That((object) discretizationLocations.Count, Is.EqualTo(2));
        }

        [Test]
        public void AddingGwswConnectionOrificeToNetworkSequence1()
        {
            var network = new HydroNetwork();
            var orificeName = TestSewerNetworkProvider.OrificeName;

            var orifice = new GwswConnectionOrifice(orificeName);
            AddSewerFeatureToNetwork(orifice, network);
            Assert.That(network.SewerConnections.Count(), Is.EqualTo(1));
            Assert.That(network.Orifices.Count(), Is.EqualTo(1));

            AddSewerFeatureToNetwork(orifice, network);
            Assert.That(network.SewerConnections.Count(), Is.EqualTo(1));
            Assert.That(network.Orifices.Count(), Is.EqualTo(1));
        }

        [Test]
        public void AddingGwswConnectionOrificeToNetworkSequence2()
        {
            var network = new HydroNetwork();
            var orificeName = TestSewerNetworkProvider.OrificeName;

            var orifice = new Orifice(orificeName);
            AddSewerFeatureToNetwork(orifice, network);
            Assert.That(network.SewerConnections.Count(), Is.EqualTo(1));
            Assert.That(network.Orifices.Count(), Is.EqualTo(1));

            var createdSewerConnection = network.SewerConnections.FirstOrDefault();
            Assert.IsNotNull(createdSewerConnection);
            // weir / orifice level source is default -10
            Assert.That(createdSewerConnection.LevelSource, Is.EqualTo(-10.0).Within(0.01));

            var gwswOrifice = new GwswConnectionOrifice(orificeName)
            {
                LevelSource = 80.1
            };
            AddSewerFeatureToNetwork(gwswOrifice, network);
            Assert.That(network.SewerConnections.Count(), Is.EqualTo(1));
            Assert.That(network.Orifices.Count(), Is.EqualTo(1));
            Assert.That(createdSewerConnection.LevelSource, Is.EqualTo(80.1).Within(0.01));
        }

        [Test]
        public void AddingGwswConnectionOrificeToNetworkSequence4()
        {
            var network = new HydroNetwork();
            var orificeName = TestSewerNetworkProvider.OrificeName;

            var orifice = new Orifice(orificeName){CrestLevel = 12.3 };
            AddSewerFeatureToNetwork(orifice, network);
            Assert.That(network.SewerConnections.Count(), Is.EqualTo(1));
            Assert.That(network.Orifices.Count(), Is.EqualTo(1));

            var createdSewerConnection = network.SewerConnections.FirstOrDefault();
            Assert.IsNotNull(createdSewerConnection);
            // weir / orifice level source is default -10
            Assert.That(createdSewerConnection.LevelSource, Is.EqualTo(-10.0).Within(0.01));



            var gwswOrifice = new GwswConnectionOrifice(orificeName){ LevelSource = 80.1 };
            AddSewerFeatureToNetwork(gwswOrifice, network);
            Assert.That(network.SewerConnections.Count(), Is.EqualTo(1));
            Assert.That(network.Orifices.Count(), Is.EqualTo(1));

            var or2 = network.Orifices.FirstOrDefault();
            Assert.IsNotNull(or2);
            Assert.That(or2.CrestLevel, Is.EqualTo(12.3).Within(0.01));

            createdSewerConnection = network.SewerConnections.FirstOrDefault();
            Assert.IsNotNull(createdSewerConnection);
            Assert.That(createdSewerConnection.LevelSource, Is.EqualTo(80.1).Within(0.01));
        }

        [Test]
        public void AddingGwswConnectionOrificeToNetworkSequence5()
        {
            var network = new HydroNetwork();
            var orificeName = TestSewerNetworkProvider.OrificeName;
            var expectedCrestLevel = 12.3;
            var expectedLevelSource = 80.1;

            var gwswOrifice = new GwswConnectionOrifice(orificeName) { LevelSource = expectedLevelSource };
            AddSewerFeatureToNetwork(gwswOrifice, network);
            Assert.That(network.SewerConnections.Count(), Is.EqualTo(1));
            Assert.That(network.Orifices.Count(), Is.EqualTo(1));

            var sewerConnection = network.SewerConnections.FirstOrDefault();
            Assert.IsNotNull(sewerConnection);
            Assert.That(sewerConnection.LevelSource, Is.EqualTo(expectedLevelSource).Within(0.01));

            var orifice = new Orifice(orificeName) { CrestLevel = expectedCrestLevel };
            AddSewerFeatureToNetwork(orifice, network);
            Assert.That(network.SewerConnections.Count(), Is.EqualTo(1));
            Assert.That(network.Orifices.Count(), Is.EqualTo(1));

            var or2 = network.Orifices.FirstOrDefault();
            Assert.IsNotNull(or2);
            Assert.That(or2.CrestLevel, Is.EqualTo(expectedCrestLevel).Within(0.01));

            sewerConnection = network.SewerConnections.FirstOrDefault();
            Assert.IsNotNull(sewerConnection);
            Assert.That(sewerConnection.LevelSource, Is.EqualTo(expectedLevelSource).Within(0.01));
        }

        #endregion

        #region Pump

        [Test]
        public void GivenSimpleSewerNetwork_WhenAddingGwswConnectionPumpToNetwork_ThenSewerConnectionIsAddedWithThePumpAsBranchFeature()
        {
            var pumpName = "myPump";
            var sourceCompartmentName = TestSewerNetworkProvider.SourceCompartmentName;
            var targetCompartmentName = TestSewerNetworkProvider.TargetCompartmentName;

            var network = TestSewerNetworkProvider.CreateSewerNetwork_TwoManholesWithOneCompartmentEach();
            var pumpToAdd = new GwswConnectionPump(pumpName)
            {
                DirectionIsPositive = true,
                SourceCompartmentName = sourceCompartmentName,
                TargetCompartmentName = targetCompartmentName
            };
            AddSewerFeatureToNetwork(pumpToAdd, network);
            Assert.That<int>(network.SewerConnections.Count, Is.EqualTo(1));

            var sewerConnectionInNetwork = Enumerable.FirstOrDefault<ISewerConnection>(network.SewerConnections);
            Assert.IsNotNull(sewerConnectionInNetwork);
            Assert.That(sewerConnectionInNetwork.Name, Is.EqualTo(pumpName));
            Assert.That(sewerConnectionInNetwork.SourceCompartment.Name, Is.EqualTo(sourceCompartmentName));
            Assert.That(sewerConnectionInNetwork.TargetCompartment.Name, Is.EqualTo(targetCompartmentName));
            Assert.That(sewerConnectionInNetwork.SourceCompartmentName, Is.EqualTo(sourceCompartmentName));
            Assert.That(sewerConnectionInNetwork.TargetCompartmentName, Is.EqualTo(targetCompartmentName));

            var sewerConnectionBranchFeatures = sewerConnectionInNetwork.BranchFeatures;
            Assert.That(sewerConnectionBranchFeatures.Count, Is.EqualTo(2));

            var pump = sewerConnectionBranchFeatures.FirstOrDefault(bf => bf is Pump) as Pump;
            Assert.IsNotNull(pump);
            Assert.That((object) pump.Name, Is.EqualTo(pumpName));
            Assert.IsTrue(pump.DirectionIsPositive);
        }

        [Test]
        public void GivenSimpleSewerNetwork_WhenAddingGwswStructurePumpToNetwork_ThenSewerConnectionIsAddedWithThePumpAsBranchFeature()
        {
            var pumpName = "myPump";

            var network = TestSewerNetworkProvider.CreateSewerNetwork_TwoManholesWithOneCompartmentEach();
            var capacity = 30.0;
            var startDelivery = -1.0;
            var stopDelivery = -0.5;
            var startSuction = 0.5;
            var stopSuction = 1.0;
            var pumpToAdd = new GwswStructurePump(pumpName)
            {
                Capacity = capacity,
                StartDelivery = startDelivery,
                StopDelivery = stopDelivery,
                StartSuction = startSuction,
                StopSuction = stopSuction
            };

            AddSewerFeatureToNetwork(pumpToAdd, network);
            Assert.That<int>(network.SewerConnections.Count, Is.EqualTo(1));

            var sewerConnection = Enumerable.FirstOrDefault<ISewerConnection>(network.SewerConnections);
            Assert.IsNotNull(sewerConnection);
            Assert.That(sewerConnection.Name, Is.EqualTo(pumpName));
            Assert.IsNull(sewerConnection.SourceCompartmentName);
            Assert.IsNull(sewerConnection.TargetCompartmentName);

            var branchFeatures = sewerConnection.BranchFeatures;
            Assert.That(branchFeatures.Count, Is.EqualTo(2));

            var pump = branchFeatures.FirstOrDefault(bf => bf is Pump) as Pump;
            Assert.IsNotNull(pump);
            Assert.That((object) pump.Name, Is.EqualTo(pumpName));
            Assert.That(pump.Capacity, Is.EqualTo(capacity));
            Assert.That(pump.StartDelivery, Is.EqualTo(startDelivery));
            Assert.That(pump.StopDelivery, Is.EqualTo(stopDelivery));
            Assert.That(pump.StartSuction, Is.EqualTo(startSuction));
            Assert.That(pump.StopSuction, Is.EqualTo(stopSuction));
        }

        [Test]
        public void GivenSewerNetworkWithTwoManholesAndOnePump_WhenAddingGwswStructurePumpWithSameIdToNetwork_ThenTheCorrectValuesHaveBeenSet()
        {
            const string pumpName = TestSewerNetworkProvider.PumpName;
            const string sourceCompartmentName = TestSewerNetworkProvider.SourceCompartmentName;
            const string targetCompartmentName = TestSewerNetworkProvider.TargetCompartmentName;

            var network = TestSewerNetworkProvider.CreateSewerNetwork_TwoManholesWithOneCompartmentEachAndOnePump();

            var capacity = 30.0;
            var startDelivery = -1.0;
            var stopDelivery = -0.5;
            var startSuction = 0.5;
            var stopSuction = 1.0;
            var pumpToAdd = new GwswStructurePump(pumpName)
            {
                Capacity = capacity,
                StartDelivery = startDelivery,
                StopDelivery = stopDelivery,
                StartSuction = startSuction,
                StopSuction = stopSuction
            };

            AddSewerFeatureToNetwork(pumpToAdd, network);

            var sewerConnection = Enumerable.FirstOrDefault<ISewerConnection>(network.SewerConnections);
            Assert.IsNotNull(sewerConnection);
            Assert.That(sewerConnection.SourceCompartment.Name, Is.EqualTo(sourceCompartmentName));
            Assert.That(sewerConnection.TargetCompartment.Name, Is.EqualTo(targetCompartmentName));
            Assert.That(sewerConnection.SourceCompartmentName, Is.EqualTo(sourceCompartmentName));
            Assert.That(sewerConnection.TargetCompartmentName, Is.EqualTo(targetCompartmentName));

            var branchFeatures = sewerConnection.BranchFeatures;
            Assert.IsNotNull(branchFeatures);
            Assert.That(branchFeatures.Count, Is.EqualTo(2));

            var pump = branchFeatures.FirstOrDefault(bf => bf is Pump) as IPump;
            Assert.IsNotNull(pump);
            Assert.That(pump.Name, Is.EqualTo(pumpName));
            Assert.That(pump.Capacity, Is.EqualTo(capacity));
            Assert.That(pump.StartDelivery, Is.EqualTo(startDelivery));
            Assert.That(pump.StopDelivery, Is.EqualTo(stopDelivery));
            Assert.That(pump.StartSuction, Is.EqualTo(startSuction));
            Assert.That(pump.StopSuction, Is.EqualTo(stopSuction));
        }

        [Test]
        public void GivenSewerNetworkWithTwoManholesAndOnePump_WhenAddingGwswConnectionPumpWithSameIdToNetwork_ThenTheCorrectValuesHaveBeenSet()
        {
            const string pumpName = TestSewerNetworkProvider.PumpName;
            const string sourceCompartmentName = TestSewerNetworkProvider.SourceCompartmentName;
            const string targetCompartmentName = TestSewerNetworkProvider.TargetCompartmentName;

            var network = TestSewerNetworkProvider.CreateSewerNetwork_TwoManholesWithOneCompartmentEachAndOnePump();

            var pumpToAdd = new GwswConnectionPump(pumpName)
            {
                DirectionIsPositive = true,
                SourceCompartmentName = sourceCompartmentName,
                TargetCompartmentName = targetCompartmentName
            };

            AddSewerFeatureToNetwork(pumpToAdd, network);

            var sewerConnection = Enumerable.FirstOrDefault<ISewerConnection>(network.SewerConnections);
            Assert.IsNotNull(sewerConnection);
            Assert.That(sewerConnection.SourceCompartment.Name, Is.EqualTo(sourceCompartmentName));
            Assert.That(sewerConnection.TargetCompartment.Name, Is.EqualTo(targetCompartmentName));
            Assert.That(sewerConnection.SourceCompartmentName, Is.EqualTo(sourceCompartmentName));
            Assert.That(sewerConnection.TargetCompartmentName, Is.EqualTo(targetCompartmentName));

            var branchFeatures = sewerConnection.BranchFeatures;
            Assert.IsNotNull(branchFeatures);
            Assert.That(branchFeatures.Count, Is.EqualTo(2));

            var pump = branchFeatures.FirstOrDefault(bf => bf is Pump) as IPump;
            Assert.IsNotNull(pump);
            Assert.That(pump.Name, Is.EqualTo(pumpName));
            Assert.IsTrue(pump.DirectionIsPositive);
        }

        [Test]
        public void GivenFmModel_WhenAddingConnectionPumpAndThenStructurePump_ThenDiscretisationPointsHaveBeenAddedToTheModelNetwork()
        {
            var fmModel = new WaterFlowFMModel { Network = TestSewerNetworkProvider.CreateSewerNetwork_TwoManholesWithOneCompartmentEach() };

            var pumpName = "myPump";
            var structurePump = new GwswStructurePump(pumpName);
            var connectionPump = new GwswConnectionPump(pumpName)
            {
                Length = 22.3,
                SourceCompartmentName = TestSewerNetworkProvider.SourceCompartmentName,
                TargetCompartmentName = TestSewerNetworkProvider.TargetCompartmentName
            };

            connectionPump.AddToHydroNetwork(fmModel.Network, null);
            Assert.That(Enumerable.Count<ISewerConnection>(fmModel.Network.SewerConnections), Is.EqualTo(1));

            var discretizationLocations = fmModel.NetworkDiscretization.Locations.Values;
            Assert.That((object) discretizationLocations.Count, Is.EqualTo(2));

            structurePump.AddToHydroNetwork(fmModel.Network, null);
            Assert.That(Enumerable.Count<ISewerConnection>(fmModel.Network.SewerConnections), Is.EqualTo(1));
            Assert.That((object) discretizationLocations.Count, Is.EqualTo(2));
        }

        [Test]
        public void GivenFmModel_WhenAddingStructurePumpAndThenConnectionPump_ThenDiscretisationPointsHaveBeenAddedToTheModelNetwork()
        {
            var fmModel = new WaterFlowFMModel { Network = TestSewerNetworkProvider.CreateSewerNetwork_TwoManholesWithOneCompartmentEach() };

            var pumpName = "myPump";
            var structurePump = new GwswStructurePump(pumpName);
            var connectionPump = new GwswConnectionPump(pumpName)
            {
                Length = 22.3,
                SourceCompartmentName = TestSewerNetworkProvider.SourceCompartmentName,
                TargetCompartmentName = TestSewerNetworkProvider.TargetCompartmentName
            };

            structurePump.AddToHydroNetwork(fmModel.Network, null);
            Assert.That(Enumerable.Count<ISewerConnection>(fmModel.Network.SewerConnections), Is.EqualTo(1));

            var discretizationLocations = fmModel.NetworkDiscretization.Locations.Values;
            Assert.That((object) discretizationLocations.Count, Is.EqualTo(1));

            connectionPump.AddToHydroNetwork(fmModel.Network, null);
            Assert.That(Enumerable.Count<ISewerConnection>(fmModel.Network.SewerConnections), Is.EqualTo(1));
            Assert.That((object) discretizationLocations.Count, Is.EqualTo(2));
        }

        #endregion

        #region Weir

        [Test]
        public void GivenSimpleSewerNetwork_WhenAddingGwswConnectionWeirToNetwork_ThenSewerConnectionIsAddedWithTheWeirAsBranchFeature()
        {
            var weirName = TestSewerNetworkProvider.WeirName;
            var sourceCompartmentName = TestSewerNetworkProvider.SourceCompartmentName;
            var targetCompartmentName = TestSewerNetworkProvider.TargetCompartmentName;

            var network = TestSewerNetworkProvider.CreateSewerNetwork_TwoManholesWithOneCompartmentEach();
            var flowDirection = FlowDirection.Both;
            var weirToAdd = new GwswConnectionWeir(weirName)
            {
                FlowDirection = flowDirection,
                SourceCompartmentName = sourceCompartmentName,
                TargetCompartmentName = targetCompartmentName
            };
            AddSewerFeatureToNetwork(weirToAdd, network);
            Assert.That<int>(network.SewerConnections.Count, Is.EqualTo(1));

            var sewerConnection = Enumerable.FirstOrDefault<ISewerConnection>(network.SewerConnections);
            Assert.IsNotNull(sewerConnection);
            Assert.That(sewerConnection.Name, Is.EqualTo(weirName));
            Assert.That(sewerConnection.SourceCompartment.Name, Is.EqualTo(sourceCompartmentName));
            Assert.That(sewerConnection.TargetCompartment.Name, Is.EqualTo(targetCompartmentName));
            Assert.That(sewerConnection.SourceCompartmentName, Is.EqualTo(sourceCompartmentName));
            Assert.That(sewerConnection.TargetCompartmentName, Is.EqualTo(targetCompartmentName));

            var branchFeatures = sewerConnection.BranchFeatures;
            Assert.That(branchFeatures.Count, Is.EqualTo(2));

            var weir = branchFeatures.FirstOrDefault(bf => bf is Weir) as Weir;
            Assert.IsNotNull(weir);
            Assert.That((object) weir.Name, Is.EqualTo(weirName));
            Assert.That(weir.FlowDirection, Is.EqualTo(flowDirection));
        }

        [Test]
        public void GivenSimpleSewerNetwork_WhenAddingGwswStructureWeirToNetwork_ThenSewerConnectionIsAddedWithTheWeirAsBranchFeature()
        {
            var network = TestSewerNetworkProvider.CreateSewerNetwork_TwoManholesWithOneCompartmentEach();

            var weirName = TestSewerNetworkProvider.WeirName;
            var crestWidth = 3.3;
            var crestLevel = -2.1;
            var correctionCoefficient = 0.8;
            var weirToAdd = new GwswStructureWeir(weirName)
            {
                CrestWidth = crestWidth,
                CrestLevel = crestLevel,
                WeirFormula = new SimpleWeirFormula
                {
                    CorrectionCoefficient = correctionCoefficient
                }
            };

            AddSewerFeatureToNetwork(weirToAdd, network);
            Assert.That<int>(network.SewerConnections.Count, Is.EqualTo(1));

            var sewerConnection = Enumerable.FirstOrDefault<ISewerConnection>(network.SewerConnections);
            Assert.IsNotNull(sewerConnection);
            Assert.That(sewerConnection.Name, Is.EqualTo(weirName));

            var branchFeatures = sewerConnection.BranchFeatures;
            Assert.That(branchFeatures.Count, Is.EqualTo(2));

            var weir = branchFeatures.FirstOrDefault(bf => bf is Weir) as Weir;
            Assert.IsNotNull(weir);
            Assert.That((object) weir.Name, Is.EqualTo(weirName));
            Assert.That(weir.CrestWidth, Is.EqualTo(crestWidth));
            Assert.That(weir.CrestLevel, Is.EqualTo(crestLevel));

            var weirFormula = weir.WeirFormula as SimpleWeirFormula;
            Assert.IsNotNull(weirFormula);
            Assert.That(weirFormula.CorrectionCoefficient, Is.EqualTo(correctionCoefficient));
        }

        [Test]
        public void GivenSewerNetworkWithTwoManholesAndOneWeir_WhenAddingGwswConnectionWeirWithSameIdToNetwork_ThenTheCorrectValuesHaveBeenSet()
        {
            var weirName = TestSewerNetworkProvider.WeirName;
            var sourceCompartmentName = TestSewerNetworkProvider.SourceCompartmentName;
            var targetCompartmentName = TestSewerNetworkProvider.TargetCompartmentName;

            var network = TestSewerNetworkProvider.CreateSewerNetwork_TwoManholesWithOneCompartmentEachAndOneWeir();
            var flowDirection = FlowDirection.Both;
            var weirToAdd = new GwswConnectionWeir(weirName)
            {
                FlowDirection = flowDirection,
                SourceCompartmentName = sourceCompartmentName,
                TargetCompartmentName = targetCompartmentName
            };
            AddSewerFeatureToNetwork(weirToAdd, network);
            Assert.That<int>(network.SewerConnections.Count, Is.EqualTo(1));

            var sewerConnection = Enumerable.FirstOrDefault<ISewerConnection>(network.SewerConnections);
            Assert.IsNotNull(sewerConnection);
            Assert.That(sewerConnection.Name, Is.EqualTo(weirName));
            Assert.That(sewerConnection.SourceCompartment.Name, Is.EqualTo(sourceCompartmentName));
            Assert.That(sewerConnection.TargetCompartment.Name, Is.EqualTo(targetCompartmentName));
            Assert.That(sewerConnection.SourceCompartmentName, Is.EqualTo(sourceCompartmentName));
            Assert.That(sewerConnection.TargetCompartmentName, Is.EqualTo(targetCompartmentName));

            var branchFeatures = sewerConnection.BranchFeatures;
            Assert.That(branchFeatures.Count, Is.EqualTo(2));

            var weir = branchFeatures.FirstOrDefault(bf => bf is Weir) as Weir;
            Assert.IsNotNull(weir);
            Assert.That((object) weir.Name, Is.EqualTo(weirName));
            Assert.That(weir.FlowDirection, Is.EqualTo(flowDirection));
        }

        [Test]
        public void GivenSewerNetworkWithTwoManholesAndOneWeir_WhenAddingGwswStructureWeirToNetwork_ThenTheCorrectValuesHaveBeenSet()
        {
            var network = TestSewerNetworkProvider.CreateSewerNetwork_TwoManholesWithOneCompartmentEachAndOneWeir();

            var weirName = TestSewerNetworkProvider.WeirName;
            var crestWidth = 3.3;
            var crestLevel = -2.1;
            var correctionCoefficient = 0.8;
            var weirToAdd = new GwswStructureWeir(weirName)
            {
                CrestWidth = crestWidth,
                CrestLevel = crestLevel,
                WeirFormula = new SimpleWeirFormula
                {
                    CorrectionCoefficient = correctionCoefficient
                }
            };

            AddSewerFeatureToNetwork(weirToAdd, network);
            Assert.That<int>(network.SewerConnections.Count, Is.EqualTo(1));

            var sewerConnection = Enumerable.FirstOrDefault<ISewerConnection>(network.SewerConnections);
            Assert.IsNotNull(sewerConnection);
            Assert.That(sewerConnection.Name, Is.EqualTo(weirName));

            var branchFeatures = sewerConnection.BranchFeatures;
            Assert.That(branchFeatures.Count, Is.EqualTo(2));

            var weir = branchFeatures.FirstOrDefault(bf => bf is Weir) as Weir;
            Assert.IsNotNull(weir);
            Assert.That((object) weir.Name, Is.EqualTo(weirName));
            Assert.That(weir.CrestWidth, Is.EqualTo(crestWidth));
            Assert.That(weir.CrestLevel, Is.EqualTo(crestLevel));

            var weirFormula = weir.WeirFormula as SimpleWeirFormula;
            Assert.IsNotNull(weirFormula);
            Assert.That(weirFormula.CorrectionCoefficient, Is.EqualTo(correctionCoefficient));
        }

        [Test]
        public void GivenFmModel_WhenAddingConnectionWeirAndThenStructureWeir_ThenDiscretisationPointsHaveBeenAddedToTheModelNetwork()
        {
            var fmModel = new WaterFlowFMModel { Network = TestSewerNetworkProvider.CreateSewerNetwork_TwoManholesWithOneCompartmentEach() };

            var weirName = "myWeir";
            var structureWeir = new GwswStructureWeir(weirName);
            var connectionWeir = new GwswConnectionWeir(weirName)
            {
                Length = 22.3,
                SourceCompartmentName = TestSewerNetworkProvider.SourceCompartmentName,
                TargetCompartmentName = TestSewerNetworkProvider.TargetCompartmentName
            };

            connectionWeir.AddToHydroNetwork(fmModel.Network, null);
            Assert.That(Enumerable.Count<ISewerConnection>(fmModel.Network.SewerConnections), Is.EqualTo(1));

            var discretizationLocations = fmModel.NetworkDiscretization.Locations.Values;
            Assert.That((object) discretizationLocations.Count, Is.EqualTo(2));

            structureWeir.AddToHydroNetwork(fmModel.Network, null);
            Assert.That(Enumerable.Count<ISewerConnection>(fmModel.Network.SewerConnections), Is.EqualTo(1));
            Assert.That((object) discretizationLocations.Count, Is.EqualTo(2));
        }

        [Test]
        public void GivenFmModel_WhenAddingStructureWeirAndThenConnectionWeir_ThenDiscretisationPointsHaveBeenAddedToTheModelNetwork()
        {
            var fmModel = new WaterFlowFMModel();
            fmModel.Network = TestSewerNetworkProvider.CreateSewerNetwork_TwoManholesWithOneCompartmentEach();

            var weirName = "myWeir";
            var structureWeir = new GwswStructureWeir(weirName);
            var connectionWeir = new GwswConnectionWeir(weirName)
            {
                Length = 22.3,
                SourceCompartmentName = TestSewerNetworkProvider.SourceCompartmentName,
                TargetCompartmentName = TestSewerNetworkProvider.TargetCompartmentName
            };

            structureWeir.AddToHydroNetwork(fmModel.Network, null);
            Assert.That(Enumerable.Count<ISewerConnection>(fmModel.Network.SewerConnections), Is.EqualTo(1));

            var discretizationLocations = fmModel.NetworkDiscretization.Locations.Values;
            Assert.That((object) discretizationLocations.Count, Is.EqualTo(1));

            connectionWeir.AddToHydroNetwork(fmModel.Network, null);
            Assert.That(Enumerable.Count<ISewerConnection>(fmModel.Network.SewerConnections), Is.EqualTo(1));
            Assert.That((object) discretizationLocations.Count, Is.EqualTo(2));
        }
        #endregion

        #region CrossSectionStandardShapeBase

        [Test]
        public void GivenEmptyHydroNetwork_WhenAddingACrossSectionStandardShape_ThenItIsAddedToTheSharedCrossSectionDefinitions()
        {
            var network = new HydroNetwork();
            var roundShape = new CrossSectionStandardShapeCircle
            {
                Name = "myShape"
            };
            
            AddSewerFeatureToNetwork(roundShape, network);
            Assert.That(network.SharedCrossSectionDefinitions.Count, Is.EqualTo(1));

            var crossSectionDefinition = network.SharedCrossSectionDefinitions[0] as CrossSectionDefinitionStandard;
            Assert.IsNotNull(crossSectionDefinition);
            Assert.That(crossSectionDefinition.Name, Is.EqualTo(roundShape.Name));
        }

        [Test]
        public void GivenEmptyHydroNetwork_WhenAddingACrossSectionStandardShapeWithDuplicateName_ThenItReplacesTheExistingcrossSectionDefinition()
        {
            var network = new HydroNetwork();
            var csDefinitionName = "myShape";
            var roundShape1 = new CrossSectionStandardShapeCircle { Name = csDefinitionName };
            AddSewerFeatureToNetwork(roundShape1, network);
            Assert.That(network.SharedCrossSectionDefinitions.Count, Is.EqualTo(1));

            var roundShape2 = new CrossSectionStandardShapeCircle { Name = csDefinitionName };
            AddSewerFeatureToNetwork(roundShape2, network);
            Assert.That(network.SharedCrossSectionDefinitions.Count, Is.EqualTo(1));

            var crossSectionDefinition = network.SharedCrossSectionDefinitions[0] as CrossSectionDefinitionStandard;
            Assert.IsNotNull(crossSectionDefinition);
            Assert.That(crossSectionDefinition.Name, Is.EqualTo(csDefinitionName));
        }

        [Test]
        public void GivenPipes_WhenAddingShapeWithSameCsDefinitionId_ThenShapeIsAddedToAllCorrectPipes()
        {
            var crossSectionDefinitionName = "myCsDefinition";
            var materialType = SewerProfileMapping.SewerProfileMaterial.CastIron;

            var network = new HydroNetwork();
            var pipe1 = new Pipe { Name = "myPipe1", CrossSectionDefinitionName = crossSectionDefinitionName };
            var pipe2 = new Pipe { Name = "myPipe2", CrossSectionDefinitionName = crossSectionDefinitionName };
            var pipe3 = new Pipe { Name = "myPipe3", CrossSectionDefinitionName = "otherId" };
            AddSewerFeatureToNetwork(pipe1, network);
            AddSewerFeatureToNetwork(pipe2, network);
            AddSewerFeatureToNetwork(pipe3, network);
            Assert.That(network.SharedCrossSectionDefinitions.Count, Is.EqualTo(0));

            var roundShape = new CrossSectionStandardShapeCircle
            {
                Name = crossSectionDefinitionName,
                MaterialName = materialType.GetDescription()
            };
            AddSewerFeatureToNetwork(roundShape, network);
            Assert.That(network.SharedCrossSectionDefinitions.Count, Is.EqualTo(1));

            Assert.That(pipe1.Profile.Shape, Is.EqualTo(roundShape));
            Assert.That(pipe1.Profile.Name, Is.EqualTo(crossSectionDefinitionName));
            Assert.That(pipe1.Material, Is.EqualTo(materialType));

            Assert.That(pipe2.Profile.Shape, Is.EqualTo(roundShape));
            Assert.That((pipe2.CrossSection?.Definition).Name, Is.EqualTo(crossSectionDefinitionName));
            Assert.That(pipe2.Material, Is.EqualTo(materialType));

            Assert.IsNull(pipe3.CrossSection?.Definition);
        }

        [Test]
        public void GivenPipe_WhenAddingShapeWithSameCsDefinitionId_ThenShapeIsAddedPipeAndToSharedCrossSectionDefinitions()
        {
            var crossSectionDefinitionName = "myCsDefinition";
            var materialType = SewerProfileMapping.SewerProfileMaterial.SheetMetal;

            var network = new HydroNetwork();
            var pipe1 = new Pipe { Name = "myPipe1", CrossSectionDefinitionName = crossSectionDefinitionName };
            AddSewerFeatureToNetwork(pipe1, network);
            Assert.That(network.SharedCrossSectionDefinitions.Count, Is.EqualTo(0));

            var circleShape = new CrossSectionStandardShapeCircle
            {
                Name = crossSectionDefinitionName,
                Diameter = 0.4,
                MaterialName = materialType.GetDescription()
            };
            AddSewerFeatureToNetwork(circleShape, network);
            Assert.That(network.SharedCrossSectionDefinitions.Count, Is.EqualTo(1));

            var csDefinition = pipe1.Profile;
            Assert.That(csDefinition.Name, Is.EqualTo(crossSectionDefinitionName));
            Assert.That(pipe1.Material, Is.EqualTo(materialType));

            var csShape = csDefinition.Shape as CrossSectionStandardShapeCircle;
            Assert.IsNotNull(csShape);
            Assert.That(csShape.Name, Is.EqualTo(crossSectionDefinitionName));
            Assert.That(csShape.Diameter, Is.EqualTo(0.4));

            var networkCsDefinition = network.SharedCrossSectionDefinitions.FirstOrDefault() as CrossSectionDefinitionStandard;
            var csShapeInNetwork = networkCsDefinition.Shape as CrossSectionStandardShapeCircle;
            csShapeInNetwork.Diameter = 3.3;

            Assert.That(csShape.Diameter, Is.EqualTo(3.3));
        }
        #endregion
    }
}