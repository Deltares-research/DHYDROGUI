using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor
{
    [TestFixture]
    public class GeometryPreviewViewModelTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            var waveBoundary = Substitute.For<IWaveBoundary>();
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();

            var lineString = new LineString(new Coordinate[]
            {
                new Coordinate(0, 0),
                new Coordinate(1, 1),
            });

            geometryFactory.ConstructBoundaryLineGeometry(waveBoundary).Returns(lineString);

            // Call
            using (var viewModel = new GeometryPreviewViewModel(waveBoundary, geometryFactory))
            {
                // Assert
                Assert.That(viewModel.MapViewModel, Is.Not.Null);
            }
        }

        [Test]
        public void Constructor_WaveBoundaryNull_ThrowsArgumentNullException()
        {
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();
            void Call() => new GeometryPreviewViewModel(null, geometryFactory);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("waveBoundary"));
        }

        [Test]
        public void Constructor_GeometryFactoryNull_ThrowsArgumentNullException()
        {
            var waveBoundary = Substitute.For<IWaveBoundary>();
            void Call() => new GeometryPreviewViewModel(waveBoundary, null);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("geometryFactory"));
        }
    }
}