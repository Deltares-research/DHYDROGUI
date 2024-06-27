using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Enums;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.Shapes;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor
{
    /// <summary>
    /// <see cref="BoundaryWideParametersViewModel"/> defines the view model for the boundary wide parameters view.
    /// </summary>
    /// <seealso cref="INotifyPropertyChanged"/>
    /// <seealso cref="IRefreshViewModel"/>
    public class BoundaryWideParametersViewModel : INotifyPropertyChanged, IRefreshViewModel
    {
        private readonly IWaveBoundaryConditionDefinition observedBoundaryCondition;
        private readonly IViewShapeFactory shapeFactory;
        private readonly IViewDataComponentFactory dataComponentFactory;
        private readonly IViewEnumFromDataComponentQuerier viewEnumFromDataComponentQuerier;
        private IAnnounceDataComponentChanged announceDataComponentChanged;

        private IViewShape shape;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Creates a new <see cref="BoundaryWideParametersViewModel"/>.
        /// </summary>
        /// <param name="observedBoundaryCondition"> The observed boundary condition. </param>
        /// <param name="shapeFactory"> The shape view factory. </param>
        /// <param name="dataComponentFactory">
        /// <see cref="IViewDataComponentFactory"/> to construct data components with.
        /// </param>
        /// <param name="viewEnumFromDataComponentQuerier">
        /// The <see cref="IViewEnumFromDataComponentQuerier"/> used to obtain the view enums.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any argument is <c>null</c>.
        /// </exception>
        public BoundaryWideParametersViewModel(IWaveBoundaryConditionDefinition observedBoundaryCondition,
                                               IViewShapeFactory shapeFactory,
                                               IViewDataComponentFactory dataComponentFactory,
                                               IViewEnumFromDataComponentQuerier viewEnumFromDataComponentQuerier)
        {
            Ensure.NotNull(observedBoundaryCondition, nameof(observedBoundaryCondition));
            Ensure.NotNull(shapeFactory, nameof(shapeFactory));
            Ensure.NotNull(dataComponentFactory, nameof(dataComponentFactory));
            Ensure.NotNull(viewEnumFromDataComponentQuerier, nameof(viewEnumFromDataComponentQuerier));

            this.observedBoundaryCondition = observedBoundaryCondition;
            this.shapeFactory = shapeFactory;
            this.dataComponentFactory = dataComponentFactory;
            this.viewEnumFromDataComponentQuerier = viewEnumFromDataComponentQuerier;

            Shape = this.shapeFactory.ConstructFromShape(observedBoundaryCondition.Shape);
            ShapeTypeList = this.shapeFactory.GetViewShapeTypesList();
        }

        /// <summary>
        /// Gets the list of Shape types available.
        /// </summary>
        public IReadOnlyList<Type> ShapeTypeList { get; }

        /// <summary>
        /// Gets or sets the type of the shape.
        /// </summary>
        /// <remarks>
        /// This value is expected to be a child class of <see cref="IViewShape"/>.
        /// </remarks>
        public Type ShapeType
        {
            get => Shape.GetType();
            set
            {
                if (value == ShapeType)
                {
                    return;
                }

                Shape = shapeFactory.ConstructFromType(ToViewShapeType(value));
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the shape.
        /// </summary>
        public IViewShape Shape
        {
            get => shape;
            private set
            {
                shape = value;
                observedBoundaryCondition.Shape = shape.ObservedShape;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="PeriodViewType"/>.
        /// </summary>
        public PeriodViewType PeriodType
        {
            get => observedBoundaryCondition.PeriodType.ConvertToPeriodViewType();
            set
            {
                BoundaryConditionPeriodType modelPeriodType = value.ConvertToBoundaryConditionPeriodType();

                if (observedBoundaryCondition.PeriodType == modelPeriodType)
                {
                    return;
                }

                observedBoundaryCondition.PeriodType = modelPeriodType;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DirectionalSpreadingViewType"/>.
        /// </summary>
        public DirectionalSpreadingViewType DirectionalSpreadingType
        {
            get => viewEnumFromDataComponentQuerier.GetDirectionalSpreadingViewType(observedBoundaryCondition.DataComponent);
            set
            {
                if (DirectionalSpreadingType == value)
                {
                    return;
                }

                observedBoundaryCondition.DataComponent =
                    dataComponentFactory.ConvertBoundaryConditionDataComponentSpreadingType(observedBoundaryCondition.DataComponent,
                                                                                            value);
                OnPropertyChanged();
                AnnounceDataComponentChanged();
            }
        }

        /// <summary>
        /// Gets whether the parameters should be visible.
        /// </summary>
        public bool IsVisible =>
            viewEnumFromDataComponentQuerier.GetForcingType(observedBoundaryCondition.DataComponent) != ForcingViewType.FileBased;

        /// <summary>
        /// Sets the mediator on this class that should announce changes.
        /// </summary>
        /// <param name="mediator">
        /// The <see cref="IAnnounceDataComponentChanged"/> used to signal the data component has changed.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="mediator"/> is <c>null</c>.
        /// </exception>
        public void SetMediator(IAnnounceDataComponentChanged mediator)
        {
            Ensure.NotNull(mediator, nameof(mediator));
            announceDataComponentChanged = mediator;
        }

        public void RefreshViewModel()
        {
            OnPropertyChanged(string.Empty);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static ViewShapeType ToViewShapeType(Type t)
        {
            if (t == typeof(GaussViewShape))
            {
                return ViewShapeType.Gauss;
            }

            if (t == typeof(JonswapViewShape))
            {
                return ViewShapeType.Jonswap;
            }

            if (t == typeof(PiersonMoskowitzViewShape))
            {
                return ViewShapeType.PiersonMoskowitz;
            }

            throw new NotSupportedException($"The conversion of Type {t.FullName} is not supported.");
        }

        private void AnnounceDataComponentChanged() =>
            announceDataComponentChanged.AnnounceDataComponentChanged();
    }
}