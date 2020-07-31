using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.Enums;
using DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels.WeirFormulaViewModels;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels
{
    /// <summary>
    /// <see cref="StructureViewModel"/> defines the view model for the
    /// <see cref="Views.StructureView"/>.
    /// </summary>
    public sealed class StructureViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly IWeir weir;
        private readonly WeirPropertiesViewModel weirPropertiesViewModel;

        /// <summary>
        /// Creates a new <see cref="StructureViewModel"/>.
        /// </summary>
        /// <param name="weir">The weir.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="weir"/> is <c>null</c>.
        /// </exception>
        public StructureViewModel(Weir2D weir)
        {
            Ensure.NotNull(weir, nameof(weir));
            this.weir = weir;

            weirPropertiesViewModel = new WeirPropertiesViewModel(weir);
            WeirViewModel = ConstructWeirViewModel(weir.WeirFormula, weirPropertiesViewModel);
        }

        private static WeirViewModel ConstructWeirViewModel(IWeirFormula weirFormula,
                                                            WeirPropertiesViewModel weirProperties)
        {
            switch (weirFormula)
            {
                case SimpleWeirFormula simpleWeirFormula:
                    return new SimpleWeirViewModel(simpleWeirFormula, 
                                                   weirProperties);
                case GatedWeirFormula gatedWeirFormula:
                    return new GatedWeirViewModel(gatedWeirFormula, 
                                                  weirProperties);
                case GeneralStructureWeirFormula generalStructureWeirFormula:
                    return new GeneralStructureViewModel(generalStructureWeirFormula, 
                                                         weirProperties);
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(weirFormula));
            }
        }

        /// <summary>
        /// Gets the weir formula view model.
        /// </summary>
        public WeirViewModel WeirViewModel
        {
            get => weirViewModel;
            private set
            {
                if (weirViewModel == value)
                {
                    return;
                }

                weirViewModel = value;
                OnPropertyChanged();
            }
        }

        private WeirViewModel weirViewModel;

        /// <summary>
        /// Gets or sets the type of the formula view.
        /// </summary>
        public FormulaViewType FormulaViewType
        {
            get => WeirViewModel.FormulaViewType;
            set
            {
                if (value == FormulaViewType)
                {
                    return;
                }

                weir.WeirFormula = GetFormulaOfType(value);
                WeirViewModel = ConstructWeirViewModel(weir.WeirFormula, WeirViewModel.WeirPropertiesViewModel);
                OnPropertyChanged();
            }
        }

        private IWeirFormula GetFormulaOfType(FormulaViewType value)
        {
            switch (value)
            {
                case FormulaViewType.SimpleWeir:
                    return new SimpleWeirFormula();
                case FormulaViewType.SimpleGate:
                    return new GatedWeirFormula(true);
                case FormulaViewType.GeneralStructure:
                    return new GeneralStructureWeirFormula()
                    {
                        BedLevelStructureCentre = weir.CrestLevel,
                        WidthStructureCentre = weir.CrestWidth,
                        WidthStructureLeftSide = double.NaN,
                        WidthStructureRightSide = double.NaN,
                        WidthLeftSideOfStructure = double.NaN,
                        WidthRightSideOfStructure = double.NaN
                    };
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(value));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>
        /// Note that this class is sealed, and no un-managed resources need to be
        /// released, as such we do not need to use an isDisposing approach.
        /// </remarks>
        public void Dispose()
        {
            if (hasDisposed)
            {
                return;
            }

            weirPropertiesViewModel.Dispose();
            hasDisposed = true;
        }

        private bool hasDisposed = false;
    }
}