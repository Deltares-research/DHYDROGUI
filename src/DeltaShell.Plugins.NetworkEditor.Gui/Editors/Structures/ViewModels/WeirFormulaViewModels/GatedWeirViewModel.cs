using System.ComponentModel;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Guards;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels.WeirFormulaViewModels
{
    /// <summary>
    /// <see cref="GatedWeirViewModel"/> provides the view model for the
    /// <see cref="Views.WeirFormulaViews.GatedWeirView"/>.
    /// </summary>
    /// <seealso cref="WeirViewModel"/>
    [Description("Simple Gate")]
    public sealed class GatedWeirViewModel : WeirViewModel
    {
        /// <summary>
        /// Creates a new <see cref="GatedWeirViewModel"/>.
        /// </summary>
        /// <param name="formula">The formula.</param>
        /// <param name="weirPropertiesViewModel">The weir properties view model.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public GatedWeirViewModel(GatedWeirFormula formula,
                                  WeirPropertiesViewModel weirPropertiesViewModel) :
            base(weirPropertiesViewModel)
        {
            Ensure.NotNull(formula, nameof(formula));
            GatePropertiesViewModel = new GatePropertiesViewModel(formula, weirPropertiesViewModel, true);
        }

        /// <summary>
        /// Gets the gate properties view model.
        /// </summary>
        public GatePropertiesViewModel GatePropertiesViewModel { get; }
    }
}