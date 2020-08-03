using DelftTools.Utils.Guards;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels.WeirFormulaViewModels
{
    /// <summary>
    /// <see cref="WeirViewModel"/> acts as the base class for all weir
    /// formula view models.
    /// </summary>
    public abstract class WeirViewModel
    {
        /// <summary>
        /// Creates a new <see cref="WeirViewModel"/>.
        /// </summary>
        /// <param name="weirPropertiesViewModel">The weir properties view model.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="weirPropertiesViewModel"/> is <c>null</c>.
        /// </exception>
        protected WeirViewModel(WeirPropertiesViewModel weirPropertiesViewModel)
        {
            Ensure.NotNull(weirPropertiesViewModel, nameof(weirPropertiesViewModel));
            WeirPropertiesViewModel = weirPropertiesViewModel;
        }

        /// <summary>
        /// Gets the weir properties view model.
        /// </summary>
        public WeirPropertiesViewModel WeirPropertiesViewModel { get; }
    }
}