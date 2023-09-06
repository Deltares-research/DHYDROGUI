using System.Windows;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    /// <summary>
    /// View model for <see cref="OrificeShapeEditView"/>.
    /// </summary>
    [Entity]
    public class OrificeShapeEditViewModel
    {
        private readonly IOrifice orifice;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrificeShapeEditViewModel"/> class.
        /// </summary>
        /// <param name="orifice">The orifice for this view model.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="orifice"/> is <c>null</c>.</exception>
        public OrificeShapeEditViewModel(IOrifice orifice)
        {
            Ensure.NotNull(orifice, nameof(orifice));

            this.orifice = orifice;
        }

        /// <summary>
        /// Gets or sets the crest level of the orifice.
        /// </summary>
        public double CrestLevel
        {
            get => orifice.CrestLevel;
            set => orifice.CrestLevel = value;
        }

        /// <summary>
        /// Gets the description for the crest level.
        /// </summary>
        public string CrestLevelDescription => Resources.OrificeShapeEditViewModel_CrestLevelDescription;

        /// <summary>
        /// Gets or sets the crest width of the orifice.
        /// </summary>
        public double CrestWidth
        {
            get => orifice.CrestWidth;
            set => orifice.CrestWidth = value;
        }

        /// <summary>
        /// Gets the description for the crest width.
        /// </summary>
        public string CrestWidthDescription => Resources.OrificeShapeEditViewModel_CrestWidthDescription;

        /// <summary>
        /// Gets or sets the gate lower edge level of the orifice.
        /// </summary>
        public double GateLowerEdgeLevel
        {
            get
            {
                if (orifice.WeirFormula is GatedWeirFormula formula)
                {
                    return formula.LowerEdgeLevel;
                }

                return double.NaN;
            }
            set
            {
                if (orifice.WeirFormula is GatedWeirFormula formula)
                {
                    formula.LowerEdgeLevel = value;
                }
            }
        }

        /// <summary>
        /// Gets the description for the gate lower edge level.
        /// </summary>
        public string GateLowerEdgeLevelDescription => Resources.OrificeShapeEditViewModel_GateLowerEdgeLevelDescription;

        /// <summary>
        /// Gets or sets the contraction coefficient of the orifice.
        /// </summary>
        public double ContractionCoefficient
        {
            get
            {
                if (orifice.WeirFormula is GatedWeirFormula formula)
                {
                    return formula.ContractionCoefficient;
                }

                return double.NaN;
            }
            set
            {
                if (orifice.WeirFormula is GatedWeirFormula formula)
                {
                    formula.ContractionCoefficient = value;
                }
            }
        }

        /// <summary>
        /// Gets the description for the contraction coefficient.
        /// </summary>
        public string ContractionCoefficientDescription => Resources.OrificeShapeEditViewModel_ContractionCoefficientDescription;

        /// <summary>
        /// Gets whether or not an input should be enabled.
        /// </summary>
        public bool IsEnabled => orifice.WeirFormula is IGatedWeirFormula;

        /// <summary>
        /// Gets whether or not an input should be visible.
        /// </summary>
        public Visibility IsVisible => IsEnabled
                                           ? Visibility.Visible
                                           : Visibility.Hidden;
    }
}