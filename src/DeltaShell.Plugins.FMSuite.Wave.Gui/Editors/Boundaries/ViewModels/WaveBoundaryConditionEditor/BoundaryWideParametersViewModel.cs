using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Enums;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.Shapes;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor
{
    /// <summary>
    /// <see cref="BoundaryWideParametersViewModel"/> defines the view model for the boundary wide parameters view.
    /// </summary>
    /// <seealso cref="INotifyPropertyChanged" />
    public class BoundaryWideParametersViewModel : INotifyPropertyChanged
    {
        private readonly IWaveBoundaryConditionDefinition observedBoundaryCondition;
        private readonly IViewShapeFactory shapeFactory;

        /// <summary>
        /// Creates a new <see cref="BoundaryWideParametersViewModel"/>.
        /// </summary>
        /// <param name="observedBoundaryCondition"> The observed boundary condition. </param>
        /// <param name="shapeFactory"> The shape view factory. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any argument is <c>null</c>.
        /// </exception>
        public BoundaryWideParametersViewModel(IWaveBoundaryConditionDefinition observedBoundaryCondition,
                                               IViewShapeFactory shapeFactory)
        {
            this.observedBoundaryCondition = observedBoundaryCondition ?? throw new ArgumentNullException(nameof(observedBoundaryCondition));
            this.shapeFactory = shapeFactory ?? throw new ArgumentNullException(nameof(shapeFactory));

            Shape = this.shapeFactory.ConstructFromShape(observedBoundaryCondition.Shape);
        }

        /// <summary>
        /// Gets the list of Shape types available.
        /// </summary>
        public IReadOnlyList<Type> ShapeTypeList { get; } = new List<Type>()
        {
            typeof(GaussViewShape),
            typeof(JonswapViewShape),
            typeof(PiersonMoskowitzViewShape),
        };

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

                Shape = shapeFactory.ConstructFromType(value);
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

        private IViewShape shape;

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
            get => observedBoundaryCondition.DirectionalSpreadingType.ConvertToDirectionalSpreadingViewType();
            set
            {
                BoundaryConditionDirectionalSpreadingType modelDirectionalSpreadingType =
                    value.ConvertToBoundaryConditionDirectionalSpreadingType();

                if (observedBoundaryCondition.DirectionalSpreadingType == modelDirectionalSpreadingType)
                {
                    return;
                }

                observedBoundaryCondition.DirectionalSpreadingType = modelDirectionalSpreadingType;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}