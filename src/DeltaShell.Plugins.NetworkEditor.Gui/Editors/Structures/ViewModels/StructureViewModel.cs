using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels.StructureFormulaViewModels;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels
{
    /// <summary>
    /// <see cref="StructureViewModel"/> defines the view model for the
    /// <see cref="Views.StructureView"/>.
    /// </summary>
    public sealed class StructureViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly IStructure structure;
        private readonly StructurePropertiesViewModel structurePropertiesViewModel;

        private StructureFormulaViewModel structureFormulaViewModel;

        private bool hasDisposed = false;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Creates a new <see cref="StructureViewModel"/>.
        /// </summary>
        /// <param name="structure">The structure.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="structure"/> is <c>null</c>.
        /// </exception>
        public StructureViewModel(IStructure structure)
        {
            Ensure.NotNull(structure, nameof(structure));
            this.structure = structure;

            structurePropertiesViewModel = new StructurePropertiesViewModel(structure);
            StructureFormulaViewModel = 
                ConstructStructureFormulaViewModel(structure.Formula, 
                                                   structurePropertiesViewModel);
        }

        /// <summary>
        /// Gets the structure formula view model.
        /// </summary>
        public StructureFormulaViewModel StructureFormulaViewModel
        {
            get => structureFormulaViewModel;
            private set
            {
                if (structureFormulaViewModel == value)
                {
                    return;
                }

                structureFormulaViewModel = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the list of WeirFormula types available.
        /// </summary>
        public IReadOnlyList<Type> FormulaTypeList { get; } =
            new[]
            {
                typeof(SimpleWeirFormulaViewModel),
                typeof(SimpleGateFormulaViewModel),
                typeof(GeneralStructureFormulaViewModel)
            };

        /// <summary>
        /// Gets or sets the type of the FormulaType.
        /// </summary>
        /// <remarks>
        /// This value is expected to be a child class of <see cref="StructureFormulaViewModel"/>.
        /// </remarks>
        public Type FormulaType
        {
            get => StructureFormulaViewModel.GetType();
            set
            {
                if (value == FormulaType)
                {
                    return;
                }

                structure.Formula = GetFormulaOfType(value);
                StructureFormulaViewModel = 
                    ConstructStructureFormulaViewModel(structure.Formula, 
                                                       StructureFormulaViewModel.StructurePropertiesViewModel);
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

            structurePropertiesViewModel.Dispose();
            (structureFormulaViewModel as IDisposable)?.Dispose();
            hasDisposed = true;
        }

        private static StructureFormulaViewModel ConstructStructureFormulaViewModel(
            IStructureFormula structureFormula,
            StructurePropertiesViewModel structureProperties)
        {
            switch (structureFormula)
            {
                case SimpleWeirFormula simpleWeirFormula:
                    return new SimpleWeirFormulaViewModel(simpleWeirFormula,
                                                   structureProperties);
                case SimpleGateFormula gatedWeirFormula:
                    return new SimpleGateFormulaViewModel(gatedWeirFormula,
                                                  structureProperties);
                case GeneralStructureFormula generalStructureWeirFormula:
                    return new GeneralStructureFormulaViewModel(generalStructureWeirFormula,
                                                         structureProperties);
                default:
                    throw new ArgumentOutOfRangeException(nameof(structureFormula));
            }
        }

        private IStructureFormula GetFormulaOfType(Type value)
        {
            if (value == typeof(SimpleWeirFormulaViewModel))
            {
                return new SimpleWeirFormula();
            }

            if (value == typeof(SimpleGateFormulaViewModel))
            {
                return new SimpleGateFormula(true);
            }

            if (value == typeof(GeneralStructureFormulaViewModel))
            {
                return new GeneralStructureFormula()
                {
                    CrestLevel = structure.CrestLevel,
                    CrestWidth = structure.CrestWidth,
                    Upstream2Width = double.NaN,
                    Downstream1Width = double.NaN,
                    Upstream1Width = double.NaN,
                    Downstream2Width = double.NaN
                };
            }

            throw new ArgumentException($"Type {value.FullName} is not a supported {nameof(StructureFormulaViewModel)}");
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}