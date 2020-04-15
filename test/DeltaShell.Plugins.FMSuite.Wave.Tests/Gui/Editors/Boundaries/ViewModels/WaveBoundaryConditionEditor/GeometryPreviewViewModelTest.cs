using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.SupportPoints;
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
        private static SupportPointDataComponentViewModel GetViewModel()
        {
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
            var announceChanged = Substitute.For<IAnnounceSupportPointDataChanged>();

            return new SupportPointDataComponentViewModel(conditionDefinition,
                                                          parametersFactory,
                                                          announceChanged);
        }

        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            var waveBoundary = Substitute.For<IWaveBoundary>();
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();
            SupportPointDataComponentViewModel supportPointDataComponentViewModel = GetViewModel();

            var lineString = new LineString(new Coordinate[]
            {
                new Coordinate(0, 0),
                new Coordinate(1, 1),
            });

            geometryFactory.ConstructBoundaryLineGeometry(waveBoundary).Returns(lineString);

            // Call
            using (var viewModel = new GeometryPreviewViewModel(waveBoundary, supportPointDataComponentViewModel, geometryFactory))
            {
                // Assert
                Assert.That(viewModel.MapViewModel, Is.Not.Null);
            }
        }

        [Test]
        public void Constructor_WaveBoundaryNull_ThrowsArgumentNullException()
        {
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();
            SupportPointDataComponentViewModel supportPointDataComponentViewModel = GetViewModel();

            void Call() => new GeometryPreviewViewModel(null, supportPointDataComponentViewModel, geometryFactory);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("waveBoundary"));
        }

        [Test]
        public void Constructor_SupportPointDataComponentViewModelNull_ThrowsArgumentNullException()
        {
            var waveBoundary = Substitute.For<IWaveBoundary>();
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();

            void Call() => new GeometryPreviewViewModel(waveBoundary, null, geometryFactory);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("supportPointDataComponentViewModel"));
        }

        [Test]
        public void Constructor_GeometryFactoryNull_ThrowsArgumentNullException()
        {
            var waveBoundary = Substitute.For<IWaveBoundary>();
            SupportPointDataComponentViewModel supportPointDataComponentViewModel = GetViewModel();
            void Call() => new GeometryPreviewViewModel(waveBoundary, supportPointDataComponentViewModel, null);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("geometryFactory"));
        }
    }
}