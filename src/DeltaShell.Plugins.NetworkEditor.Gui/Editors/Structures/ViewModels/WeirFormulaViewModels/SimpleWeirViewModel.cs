using System.ComponentModel;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Guards;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels.WeirFormulaViewModels
{
    /// <summary>
    /// <see cref="SimpleWeirViewModel"/> defines the view model for the
    /// <see cref="Views.WeirFormulaViews.SimpleWeirView"/>.
    /// </summary>
    /// <seealso cref="WeirViewModel"/>
    [Description("Simple Weir")]
    public sealed class SimpleWeirViewModel : WeirViewModel
    {
        private readonly SimpleWeirFormula formula;

        /// <summary>
        /// Creates a new <see cref="SimpleWeirViewModel"/>.
        /// </summary>
        /// <param name="formula">The formula.</param>
        /// <param name="weirPropertiesViewModel">The weir properties view model.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public SimpleWeirViewModel(SimpleWeirFormula formula,
                                   WeirPropertiesViewModel weirPropertiesViewModel) :
            base(weirPropertiesViewModel)
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