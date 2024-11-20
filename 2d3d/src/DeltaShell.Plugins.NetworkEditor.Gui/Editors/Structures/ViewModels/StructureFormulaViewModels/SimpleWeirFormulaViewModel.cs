using System.ComponentModel;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels.StructureFormulaViewModels
{
    /// <summary>
    /// <see cref="SimpleWeirFormulaViewModel"/> defines the view model for the
    /// <see cref="Views.StructureFormulaViews.SimpleWeirFormulaView"/>.
    /// </summary>
    /// <seealso cref="StructureFormulaViewModel"/>
    [Description("Simple Weir")]
    public sealed class SimpleWeirFormulaViewModel : StructureFormulaViewModel
    {
        private readonly SimpleWeirFormula formula;

        /// <summary>
        /// Creates a new <see cref="SimpleWeirFormulaViewModel"/>.
        /// </summary>
        /// <param name="formula">The formula.</param>
        /// <param name="structurePropertiesViewModel">The weir properties view model.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public SimpleWeirFormulaViewModel(SimpleWeirFormula formula,
                                          StructurePropertiesViewModel structurePropertiesViewModel) :
            base(structurePropertiesViewModel)
        {
            Ensure.NotNull(formula, nameof(formula));
            this.formula = formula;
        }

        /// <summary>
        /// Gets or sets the contraction coefficient.
        /// </summary>
        public double ContractionCoefficient
        {
            get => formula.LateralContraction;
            set
            {
                // The floating point values are provided by the user in an entry
                // As such no error can be introduced, and either values or the same
                // or they should be updated.
                if (value == ContractionCoefficient)
                {
                    return;
                }

                formula.LateralContraction = value;
                OnPropertyChanged();
            }
        }
    }
}