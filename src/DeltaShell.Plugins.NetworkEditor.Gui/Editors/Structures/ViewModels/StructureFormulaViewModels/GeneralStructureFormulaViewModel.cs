using System;
using System.ComponentModel;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels.StructureFormulaViewModels
{
    /// <summary>
    /// <see cref="GeneralStructureFormulaViewModel"/> provides the view model for the
    /// <see cref="Views.WeirFormulaViews.GeneralStructureView"/>.
    /// </summary>
    /// <seealso cref="StructureFormulaViewModel"/>
    [Description("General Structure")]
    public sealed class GeneralStructureFormulaViewModel : StructureFormulaViewModel, IDisposable
    {
        private readonly GeneralStructureFormula formula;

        /// <summary>
        /// Creates a new <see cref="GeneralStructureFormulaViewModel"/>
        /// </summary>
        /// <param name="formula">The formula.</param>
        /// <param name="structurePropertiesViewModel">The weir properties view model.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public GeneralStructureFormulaViewModel(GeneralStructureFormula formula,
                                                StructurePropertiesViewModel structurePropertiesViewModel) :
            base(structurePropertiesViewModel)
        {
            Ensure.NotNull(formula, nameof(formula));

            this.formula = formula;
            GatePropertiesViewModel = new GatePropertiesViewModel(formula, structurePropertiesViewModel, false);
        }

        /// <summary>
        /// Gets or sets the Upstream1 width.
        /// </summary>
        public double? Upstream1Width
        {
            get => ToNullableValue(formula.Upstream1Width);
            set
            {
                // The floating point values are provided by the user in an entry
                // As such no error can be introduced, and either values or the same
                // or they should be updated.
                if (value == Upstream1Width)
                {
                    return;
                }

                formula.Upstream1Width = value ?? double.NaN;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the Upstream1 level.
        /// </summary>
        public double Upstream1Level
        {
            get => formula.Upstream1Level;
            set
            {
                if (value == Upstream1Level)
                {
                    return;
                }

                formula.Upstream1Level = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the Upstream2 width.
        /// </summary>
        public double? Upstream2Width
        {
            get => ToNullableValue(formula.Upstream2Width);
            set
            {
                if (value == Upstream2Width)
                {
                    return;
                }

                formula.Upstream2Width = value ?? double.NaN;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the Upstream2 level.
        /// </summary>
        public double Upstream2Level
        {
            get => formula.Upstream2Level;
            set
            {
                if (value == Upstream2Level)
                {
                    return;
                }

                formula.Upstream2Level = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the Downstream1 width.
        /// </summary>
        public double? Downstream1Width
        {
            get => ToNullableValue(formula.Downstream1Width);
            set
            {
                if (value == Downstream1Width)
                {
                    return;
                }

                formula.Downstream1Width = value ?? double.NaN;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the Downstream1 level.
        /// </summary>
        public double Downstream1Level
        {
            get => formula.Downstream1Level;
            set
            {
                if (value == Downstream1Level)
                {
                    return;
                }

                formula.Downstream1Level = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the Downstream2 width.
        /// </summary>
        public double? Downstream2Width
        {
            get => ToNullableValue(formula.Downstream2Width);
            set
            {
                if (value == Downstream2Width)
                {
                    return;
                }

                formula.Downstream2Width = value ?? double.NaN;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the Downstream2 level.
        /// </summary>
        public double Downstream2Level
        {
            get => formula.Downstream2Level;
            set
            {
                if (value == Downstream2Level)
                {
                    return;
                }

                formula.Downstream2Level = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the gate properties view model.
        /// </summary>
        public GatePropertiesViewModel GatePropertiesViewModel { get; }

        /// <summary>
        /// Gets or sets the positive free gate flow.
        /// </summary>
        public double FreeGateFlowPositive
        {
            get => formula.PositiveFreeGateFlow;
            set
            {
                if (value == FreeGateFlowPositive)
                {
                    return;
                }

                formula.PositiveFreeGateFlow = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the negative free gate flow.
        /// </summary>
        public double FreeGateFlowNegative
        {
            get => formula.NegativeFreeGateFlow;
            set
            {
                if (value == FreeGateFlowNegative)
                {
                    return;
                }

                formula.NegativeFreeGateFlow = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the positive drowned gate flow.
        /// </summary>
        public double DrownedGateFlowPositive
        {
            get => formula.PositiveDrownedGateFlow;
            set
            {
                if (value == DrownedGateFlowPositive)
                {
                    return;
                }

                formula.PositiveDrownedGateFlow = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the negative drowned gate flow.
        /// </summary>
        public double DrownedGateFlowNegative
        {
            get => formula.NegativeDrownedGateFlow;
            set
            {
                if (value == DrownedGateFlowNegative)
                {
                    return;
                }

                formula.NegativeDrownedGateFlow = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the positive free weir flow.
        /// </summary>
        public double FreeWeirFlowPositive
        {
            get => formula.PositiveFreeWeirFlow;
            set
            {
                if (value == FreeWeirFlowPositive)
                {
                    return;
                }

                formula.PositiveFreeWeirFlow = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the negative free weir flow.
        /// </summary>
        public double FreeWeirFlowNegative
        {
            get => formula.NegativeFreeWeirFlow;
            set
            {
                if (value == FreeWeirFlowNegative)
                {
                    return;
                }

                formula.NegativeFreeWeirFlow = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the positive drowned weir flow.
        /// </summary>
        public double DrownedWeirFlowPositive
        {
            get => formula.PositiveDrownedWeirFlow;
            set
            {
                if (value == DrownedWeirFlowPositive)
                {
                    return;
                }

                formula.PositiveDrownedWeirFlow = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the negative drowned weir flow.
        /// </summary>
        public double DrownedWeirFlowNegative
        {
            get => formula.NegativeDrownedWeirFlow;
            set
            {
                if (value == DrownedWeirFlowNegative)
                {
                    return;
                }

                formula.NegativeDrownedWeirFlow = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the positive contraction coefficient.
        /// </summary>
        public double ContractionCoefficientPositive
        {
            get => formula.PositiveContractionCoefficient;
            set
            {
                if (value == ContractionCoefficientPositive)
                {
                    return;
                }

                formula.PositiveContractionCoefficient = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the negative contraction coefficient.
        /// </summary>
        public double ContractionCoefficientNegative
        {
            get => formula.NegativeContractionCoefficient;
            set
            {
                if (value == ContractionCoefficientNegative)
                {
                    return;
                }

                formula.NegativeContractionCoefficient = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the extra resistance.
        /// </summary>
        public double ExtraResistance
        {
            get => formula.ExtraResistance;
            set
            {
                if (value == ExtraResistance)
                {
                    return;
                }

                formula.ExtraResistance = value;
                OnPropertyChanged();
            }
        }

        private static double? ToNullableValue(double value) =>
            double.IsNaN(value) ? null : (double?) value;

        // Note that we do not have any unmanaged resources and the 
        // class is sealed, as such this simple Dispose is sufficient.
        public void Dispose()
        {
            GatePropertiesViewModel?.Dispose();
        }
    }
}