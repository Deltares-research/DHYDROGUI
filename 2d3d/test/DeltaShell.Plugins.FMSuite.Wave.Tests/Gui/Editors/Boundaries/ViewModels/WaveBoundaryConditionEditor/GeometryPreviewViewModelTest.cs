using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.SupportPoints;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api;

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
            var configurator = Substitute.For<IGeometryPreviewMapConfigurator>();
            SupportPointDataComponentViewModel supportPointDataComponentViewModel = GetViewModel();

            // Call
            using (var viewModel = new GeometryPreviewViewModel(waveBoundary, supportPointDataComponentViewModel, configurator))
            {
                // Assert
                Assert.That(viewModel, Is.InstanceOf<IRefreshGeometryView>());
                Assert.That(viewModel.Map, Is.Not.Null);

                configurator.Received(1).ConfigureMap(Arg.Is<IMap>(x => x == viewModel.Map),
                                                      Arg.Is<IBoundaryProvider>(x => x.Boundaries.Contains(waveBoundary) && x.Boundaries.Count == 1),
                                                      Arg.Is<SupportPointDataComponentViewModel>(x => x == supportPointDataComponentViewModel),
                                                      Arg.Is<IRefreshGeometryView>(x => x == viewModel));
            }
        }

        [Test]
        public void Constructor_WaveBoundaryNull_ThrowsArgumentNullException()
        {
            var configurator = Substitute.For<IGeometryPreviewMapConfigurator>();
            SupportPointDataComponentViewModel supportPointDataComponentViewModel = GetViewModel();

            void Call() => new GeometryPreviewViewModel(null, supportPointDataComponentViewModel, configurator);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("waveBoundary"));
        }

        [Test]
        public void Constructor_SupportPointDataComponentViewModelNull_ThrowsArgumentNullException()
        {
            var waveBoundary = Substitute.For<IWaveBoundary>();
            var configurator = Substitute.For<IGeometryPreviewMapConfigurator>();

            void Call() => new GeometryPreviewViewModel(waveBoundary, null, configurator);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("supportPointDataComponentViewModel"));
        }

        [Test]
        public void Constructor_ConfiguratorNull_ThrowsArgumentNullException()
        {
            var waveBoundary = Substitute.For<IWaveBoundary>();
            SupportPointDataComponentViewModel supportPointDataComponentViewModel = GetViewModel();
            void Call() => new GeometryPreviewViewModel(waveBoundary, supportPointDataComponentViewModel, null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("configurator"));
        }

        private static SupportPointDataComponentViewModel GetViewModel()
        {
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
            var announceChanged = Substitute.For<IAnnounceSupportPointDataChanged>();

            return new SupportPointDataComponentViewModel(conditionDefinition,
                                                          parametersFactory,
                                                          announceChanged);
        }
    }
}