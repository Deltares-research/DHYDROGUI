using System;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels
{
    public class WaveBoundaryConditionEditorViewModel
    {
        private readonly IWaveBoundary observedBoundary;

        /// <summary>
        /// Creates a new <see cref="WaveBoundaryConditionEditorViewModel"/>.
        /// </summary>
        /// <param name="observedBoundary"> The observed boundary. </param>
        /// <param name="geometryFactory"> The geometry factory required for the geometry preview. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="observedBoundary"/> or <paramref name="geometryFactory"/> is <c>null</c>.
        /// </exception>
        public WaveBoundaryConditionEditorViewModel(IWaveBoundary observedBoundary,
                                                    IWaveBoundaryGeometryFactory geometryFactory)
        {
            Ensure.NotNull(observedBoundary, nameof(observedBoundary));
            Ensure.NotNull(geometryFactory, nameof(geometryFactory));

            this.observedBoundary = observedBoundary;

            var modelDataComponentFactory =
                new BoundaryConditionDataComponentFactory(new BoundaryParametersFactory());
            var dataComponentFactory = new ViewDataComponentFactory(modelDataComponentFactory);

            BoundarySpecificParametersSettingsViewModel =
                new BoundarySpecificParametersSettingsViewModel(observedBoundary.ConditionDefinition,
                                                                dataComponentFactory);

            GeometryViewModel = new BoundaryGeometryViewModel(observedBoundary, 
                                                              geometryFactory);

            var mediator = new WaveBoundaryConditionEditorMediator(GeometryViewModel.SupportPointEditorViewModel,
                                                                   BoundarySpecificParametersSettingsViewModel);

            DescriptionViewModel = new BoundaryDescriptionViewModel(observedBoundary,
                                                                    dataComponentFactory, 
                                                                    mediator);

            var viewShapeFactory = new ViewShapeFactory(new BoundaryConditionShapeFactory());
            BoundaryWideParametersViewModel = new BoundaryWideParametersViewModel(observedBoundary.ConditionDefinition, 
                                                                                  viewShapeFactory);
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name => observedBoundary.Name;

        /// <summary>
        /// Gets the <see cref="BoundaryDescriptionViewModel"/>.
        /// </summary>
        public BoundaryDescriptionViewModel DescriptionViewModel { get; }

        /// <summary>
        /// Gets the <see cref="BoundaryWideParametersViewModel"/>.
        /// </summary>
        public BoundaryWideParametersViewModel BoundaryWideParametersViewModel { get; }

        /// <summary>
        /// Gets the <see cref="BoundaryGeometryViewModel"/>
        /// </summary>
        public BoundaryGeometryViewModel GeometryViewModel { get; }

        /// <summary>
        /// Gets the boundary specific parameter settings view model.
        /// </summary>
        public BoundarySpecificParametersSettingsViewModel BoundarySpecificParametersSettingsViewModel { get; }
    }
}