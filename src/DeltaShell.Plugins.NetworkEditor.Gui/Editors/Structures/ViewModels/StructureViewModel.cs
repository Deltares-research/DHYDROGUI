using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels.WeirFormulaViewModels;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels
{
    /// <summary>
    /// <see cref="StructureViewModel"/> defines the view model for the
    /// <see cref="Views.StructureView"/>.
    /// </summary>
    public sealed class StructureViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly IStructure weir;
        private readonly WeirPropertiesViewModel weirPropertiesViewModel;

        private WeirViewModel weirViewModel;

        private bool hasDisposed = false;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Creates a new <see cref="StructureViewModel"/>.
        /// </summary>
        /// <param name="weir">The weir.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="weir"/> is <c>null</c>.
        /// </exception>
        public StructureViewModel(IStructure weir)
        {
            Ensure.NotNull(weir, nameof(weir));
            this.weir = weir;

            weirPropertiesViewModel = new WeirPropertiesViewModel(weir);
            WeirViewModel = ConstructWeirViewModel(weir.Formula, weirPropertiesViewModel);
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

        /// <summary>
        /// Gets the list of WeirFormula types available.
        /// </summary>
        public IReadOnlyList<Type> FormulaTypeList { get; } =
            new[]
            {
                typeof(SimpleWeirViewModel),
                typeof(GatedWeirViewModel),
                typeof(GeneralStructureViewModel)
            };

        /// <summary>
        /// Gets or sets the type of the FormulaType.
        /// </summary>
        /// <remarks>
        /// This value is expected to be a child class of <see cref="WeirViewModel"/>.
        /// </remarks>
        public Type FormulaType
        {
            get => WeirViewModel.GetType();
            set
            {
                if (value == FormulaType)
                {
                    return;
                }

                weir.Formula = GetFormulaOfType(value);
                WeirViewModel = ConstructWeirViewModel(weir.Formula, WeirViewModel.WeirPropertiesViewModel);
                OnPropertyChanged();
            }
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
            (weirViewModel as IDisposable)?.Dispose();
            hasDisposed = true;
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
                    throw new ArgumentOutOfRangeException(nameof(weirFormula));
            }
        }

        private IWeirFormula GetFormulaOfType(Type value)
        {
            if (value == typeof(SimpleWeirViewModel))
            {
                return new SimpleWeirFormula();
            }

            if (value == typeof(GatedWeirViewModel))
            {
                return new GatedWeirFormula(true);
            }

            if (value == typeof(GeneralStructureViewModel))
            {
                return new GeneralStructureWeirFormula()
                {
                    BedLevelStructureCentre = weir.CrestLevel,
                    WidthStructureCentre = weir.CrestWidth,
                    WidthStructureLeftSide = double.NaN,
                    WidthStructureRightSide = double.NaN,
                    WidthLeftSideOfStructure = double.NaN,
                    WidthRightSideOfStructure = double.NaN
                };
            }

            throw new ArgumentException($"Type {value.FullName} is not a supported {nameof(WeirViewModel)}");
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}