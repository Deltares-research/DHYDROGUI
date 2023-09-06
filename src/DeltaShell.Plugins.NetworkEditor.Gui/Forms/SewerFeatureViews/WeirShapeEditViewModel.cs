using System.Windows;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using FlowDirection = DelftTools.Hydro.FlowDirection;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    /// <summary>
    /// View model for <see cref="WeirShapeEditViewModel"/>.
    /// </summary>
    [Entity]
    public class WeirShapeEditViewModel
    {
        private readonly IWeir weir;

        /// <summary>
        /// Initializes a new instance of the <see cref="WeirShapeEditViewModel"/> class.
        /// </summary>
        /// <param name="weir">The orifice for this view model.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="weir"/> is <c>null</c>.</exception>
        public WeirShapeEditViewModel(IWeir weir)
        {
            Ensure.NotNull(weir, nameof(weir));

            this.weir = weir;
        }

        /// <summary>
        /// Gets or sets the crest level of the weir.
        /// </summary>
        public double CrestLevel
        {
            get => weir.CrestLevel;
            set => weir.CrestLevel = value;
        }

        /// <summary>
        /// Gets the description for the crest level.
        /// </summary>
        public string CrestLevelDescription => Resources.WeirShapeEditViewModel_CrestLevelDescription;

        /// <summary>
        /// Gets or sets the crest width of the weir.
        /// </summary>
        public double CrestWidth
        {
            get => weir.CrestWidth;
            set => weir.CrestWidth = value;
        }

        /// <summary>
        /// Gets the description for the crest width.
        /// </summary>
        public string CrestWidthDescription => Resources.WeirShapeEditViewModel_CrestWidthDescription;

        /// <summary>
        /// Gets or sets the flow direction of the weir.
        /// </summary>
        public FlowDirection FlowDirection
        {
            get => weir.FlowDirection;
            set => weir.FlowDirection = value;
        }

        /// <summary>
        /// Gets the description for the flow direction.
        /// </summary>
        public string FlowDirectionDescription => Resources.WeirShapeEditViewModel_FlowDirectionDescription;

        /// <summary>
        /// Gets or sets the correction coefficient of the weir.
        /// </summary>
        public double CorrectionCoefficient
        {
            get
            {
                if (weir.WeirFormula is SimpleWeirFormula formula)
                {
                    return formula.CorrectionCoefficient;
                }

                return double.NaN;
            }

            set
            {
                if (weir.WeirFormula is SimpleWeirFormula formula)
                {
                    formula.CorrectionCoefficient = value;
                }
            }
        }

        /// <summary>
        /// Gets the description for the correction coefficient.
        /// </summary>
        public string CorrectionCoefficientDescription => Resources.WeirShapeEditViewModel_CorrectionCoefficientDescription;

        /// <summary>
        /// Gets whether or not an input should be enabled.
        /// </summary>
        public bool IsEnabled => weir.WeirFormula is SimpleWeirFormula;

        /// <summary>
        /// Gets whether or not an input should be visible.
        /// </summary>
        public Visibility IsVisible => IsEnabled
                                           ? Visibility.Visible
                                           : Visibility.Hidden;
    }
}