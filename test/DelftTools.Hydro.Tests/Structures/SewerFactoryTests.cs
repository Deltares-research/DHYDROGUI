using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.Structures
{
    [TestFixture]
    public class SewerFactoryTests
    {
        [Test]
        public void GeneratingAPipe_CheckCrossSectionDefaultSettings()
        {
            var network = new HydroNetwork();
            var retrievedPipe = AddPipeToNetworkAndReturn(network);

            Assert.IsNotNull(retrievedPipe.CrossSection?.Definition);
            var profile = retrievedPipe.Profile;

            Assert.AreEqual(CrossSectionType.Standard ,profile.CrossSectionType);

            var csRoundShape = profile.Shape as CrossSectionStandardShapeCircle;
            Assert.IsNotNull(csRoundShape);
            Assert.That(csRoundShape.Diameter, Is.EqualTo(0.4));
            Assert.That(csRoundShape.Type, Is.EqualTo(CrossSectionStandardShapeType.Circle));

            Assert.That(network.SharedCrossSectionDefinitions.Count, Is.EqualTo(1));
            Assert.AreSame(((CrossSectionDefinitionProxy)retrievedPipe.CrossSection?.Definition), network.SharedCrossSectionDefinitions.First());
        }

        [Test]
        public void GeneratingAPipe_CheckPipeDefaultSettings()
        {
            var network = new HydroNetwork();
            var retrievedPipe = AddPipeToNetworkAndReturn(network);

            Assert.AreEqual(-2.0, retrievedPipe.LevelSource);
            Assert.AreEqual(-2.0, retrievedPipe.LevelTarget);
            Assert.AreEqual(retrievedPipe.Geometry.Length, retrievedPipe.Length);
            Assert.That(retrievedPipe.WaterType, Is.EqualTo(SewerConnectionWaterType.Combined));
            Assert.That(retrievedPipe.Material, Is.EqualTo(SewerProfileMapping.SewerProfileMaterial.Concrete));
        }

        [Test]
        public void GeneratingPipe_CheckConnections()
        {
            var network = new HydroNetwork();
            var retrievedPipe = AddPipeToNetworkAndReturn(network);

            var sourceCompartment = retrievedPipe.SourceCompartment;
            var targetCompartment = retrievedPipe.TargetCompartment;
            Assert.AreSame(sourceCompartment, network.Manholes.First().Compartments.First());
            Assert.AreSame(targetCompartment, network.Manholes.Last().Compartments.First());
        }

        [Test]
        public void GeneratingAManhole_CheckCompartmentDefaultSettings()
        {
            var network = new HydroNetwork();
            SewerFactory.CreateDefaultManholeAndAddToNetwork(network, new Coordinate(100.0, 100.0));
            
            Assert.AreEqual(1, network.Manholes.Count());
            var manhole = network.Manholes.First();
            Assert.That(manhole.Compartments.Count, Is.EqualTo(1));

            var compartment = manhole.Compartments.First();
            Assert.That(compartment.SurfaceLevel, Is.EqualTo(0.0));
            Assert.That(compartment.BottomLevel, Is.EqualTo(-2.0));
            Assert.That(compartment.FloodableArea, Is.EqualTo(100.0));
            Assert.That(compartment.ManholeLength, Is.EqualTo(0.64));
            Assert.That(compartment.ManholeWidth, Is.EqualTo(0.64));
        }

        private static Pipe AddPipeToNetworkAndReturn(IHydroNetwork network)
        {
            var pipe = new Pipe
            {
                Geometry = new LineString(new[] {new Coordinate(0.0, 0.0), new Coordinate(100.0, 100.0)})
            };
            SewerFactory.AddDefaultPipeToNetwork(pipe, network);

            Assert.That(network.Pipes.Count(), Is.EqualTo(1));
            
            var retrievedPipe = network.Pipes.First() as Pipe;
            Assert.IsNotNull(retrievedPipe);
            return retrievedPipe;
        }
    }
}
