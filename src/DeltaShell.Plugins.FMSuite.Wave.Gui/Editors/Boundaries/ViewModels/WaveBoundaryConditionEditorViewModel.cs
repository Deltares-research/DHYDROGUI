using System;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.SupportPoints;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels
{
    /// <summary>
    /// <see cref="WaveBoundaryConditionEditorViewModel"/> defines the view
    /// model for the wave boundary condition editor view.
    /// </summary>
    public class WaveBoundaryConditionEditorViewModel
    {
        private readonly IWaveBoundary observedBoundary;

        /// <summary>
        /// Creates a new <see cref="WaveBoundaryConditionEditorViewModel"/>.
        /// </summary>
        /// <param name="observedBoundary"> The observed boundary. </param>
        /// <param name="geometryFactory"> The geometry factory required for the geometry preview. </param>
        /// <param name="referenceDateTimeProvider">Reference date time provider.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="observedBoundary"/> or <paramref name="geometryFactory"/> is <c>null</c>.
        /// </exception>
        public WaveBoundaryConditionEditorViewModel(IWaveBoundary observedBoundary,
                                                    IWaveBoundaryGeometryFactory geometryFactory,
                                                    IReferenceDateTimeProvider referenceDateTimeProvider)
        {
            Ensure.NotNull(observedBoundary, nameof(observedBoundary));
            Ensure.NotNull(geometryFactory, nameof(geometryFactory));
            Ensure.NotNull(referenceDateTimeProvider, nameof(referenceDateTimeProvider));

            this.observedBoundary = observedBoundary;

            var parametersFactory = new ForcingTypeDefinedParametersFactory();
            var modelDataComponentFactory = new SpatiallyDefinedDataComponentFactory(parametersFactory);
            var dataComponentFactory = new ViewDataComponentFactory(modelDataComponentFactory,
                                                                    referenceDateTimeProvider);

            BoundarySpecificParametersSettingsViewModel = new BoundarySpecificParametersSettingsViewModel(observedBoundary.ConditionDefinition,
                                                                                                          dataComponentFactory);

            var dataComponentModelMediator = new DataComponentChangeMediator(BoundarySpecificParametersSettingsViewModel);

            var dataComponentModel = new SupportPointDataComponentViewModel(observedBoundary.ConditionDefinition,
                                                                            parametersFactory,
                                                                            dataComponentModelMediator);

            GeometryViewModel = new BoundaryGeometryViewModel(observedBoundary,
                                                              geometryFactory,
                                                              dataComponentModel);

            DescriptionViewModel = new BoundaryDescriptionViewModel(observedBoundary,
                                                                    dataComponentFactory);

            var viewShapeFactory = new ViewShapeFactory(new BoundaryConditionShapeFactory());
            BoundaryWideParametersViewModel = new BoundaryWideParametersViewModel(observedBoundary.ConditionDefinition,
                                                                                  viewShapeFactory,
                                                                                  dataComponentFactory);

            var mediator = new WaveBoundaryConditionEditorMediator(GeometryViewModel.SupportPointEditorViewModel,
                                                                   BoundarySpecificParametersSettingsViewModel,
                                                                   BoundaryWideParametersViewModel);

            DescriptionViewModel.SetMediator(mediator);
            BoundaryWideParametersViewModel.SetMediator(mediator);
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