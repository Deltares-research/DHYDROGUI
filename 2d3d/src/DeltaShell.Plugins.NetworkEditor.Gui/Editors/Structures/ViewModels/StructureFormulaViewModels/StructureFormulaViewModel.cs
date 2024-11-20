using System.ComponentModel;
using System.Runtime.CompilerServices;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels.StructureFormulaViewModels
{
    /// <summary>
    /// <see cref="StructureFormulaViewModel"/> acts as the base class for all structure
    /// formula view models.
    /// </summary>
    public abstract class StructureFormulaViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Creates a new <see cref="StructureFormulaViewModel"/>.
        /// </summary>
        /// <param name="structurePropertiesViewModel">The weir properties view model.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="structurePropertiesViewModel"/> is <c>null</c>.
        /// </exception>
        protected StructureFormulaViewModel(StructurePropertiesViewModel structurePropertiesViewModel)
        {
            Ensure.NotNull(structurePropertiesViewModel, nameof(structurePropertiesViewModel));
            StructurePropertiesViewModel = structurePropertiesViewModel;
        }

        /// <summary>
        /// Gets the weir properties view model.
        /// </summary>
        /// <remarks>
        /// Note that the <see cref="StructureFormulaViewModel"/> is not the "owner " of the
        /// <see cref="StructurePropertiesViewModel"/> it merely holds a reference to it.
        /// The actual life-time management is done by the <see cref="StructureViewModel"/>
        /// As such we do not need to dispose of this object.
        /// </remarks>
        public StructurePropertiesViewModel StructurePropertiesViewModel { get; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}