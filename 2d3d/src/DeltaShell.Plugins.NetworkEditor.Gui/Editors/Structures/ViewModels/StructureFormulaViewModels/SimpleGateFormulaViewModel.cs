using System;
using System.ComponentModel;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels.StructureFormulaViewModels
{
    /// <summary>
    /// <see cref="SimpleGateFormulaViewModel"/> provides the view model for the
    /// <see cref="Views.WeirFormulaViews.GatedWeirView"/>.
    /// </summary>
    /// <seealso cref="StructureFormulaViewModel"/>
    [Description("Simple Gate")]
    public sealed class SimpleGateFormulaViewModel : StructureFormulaViewModel, IDisposable
    {
        /// <summary>
        /// Creates a new <see cref="SimpleGateFormulaViewModel"/>.
        /// </summary>
        /// <param name="formula">The formula.</param>
        /// <param name="structurePropertiesViewModel">The weir properties view model.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public SimpleGateFormulaViewModel(SimpleGateFormula formula,
                                  StructurePropertiesViewModel structurePropertiesViewModel) :
            base(structurePropertiesViewModel)
        {
            Ensure.NotNull(formula, nameof(formula));
            GatePropertiesViewModel = new GatePropertiesViewModel(formula, structurePropertiesViewModel, true);
        }

        /// <summary>
        /// Gets the gate properties view model.
        /// </summary>
        public GatePropertiesViewModel GatePropertiesViewModel { get; }

        // Note that we do not have any unmanaged resources and the 
        // class is sealed, as such this simple Dispose is sufficient.
        public void Dispose()
        {
            GatePropertiesViewModel?.Dispose();
        }
    }
}