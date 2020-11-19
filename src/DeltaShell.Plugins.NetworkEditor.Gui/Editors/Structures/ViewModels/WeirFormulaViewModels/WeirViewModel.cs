using System.ComponentModel;
using System.Runtime.CompilerServices;
using DelftTools.Utils.Guards;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels.WeirFormulaViewModels
{
    /// <summary>
    /// <see cref="WeirViewModel"/> acts as the base class for all weir
    /// formula view models.
    /// </summary>
    public abstract class WeirViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

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
        /// <remarks>
        /// Note that the <see cref="WeirViewModel"/> is not the "owner " of the
        /// <see cref="WeirPropertiesViewModel"/> it merely holds a reference to it.
        /// The actual life-time management is done by the <see cref="StructureViewModel"/>
        /// As such we do not need to dispose of this object.
        /// </remarks>
        public WeirPropertiesViewModel WeirPropertiesViewModel { get; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}