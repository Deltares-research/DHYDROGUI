using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows
{
    /// <summary>
    /// Representation object of a <see cref="Weir2D"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    public class Weir2DRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly Weir2D weir2D;

        /// <summary>
        /// Initialize a new instance of the <see cref="Weir2DRow"/> class.
        /// </summary>
        /// <param name="weir2D"> The weir 2D to be presented. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="weir2D"/> is <c>null</c>.
        /// </exception>
        public Weir2DRow(Weir2D weir2D)
            : base(weir2D)
        {
            Ensure.NotNull(weir2D, nameof(weir2D));
            this.weir2D = weir2D;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => weir2D.Name;
            set => weir2D.SetNameIfValid(value);
        }

        [DisplayName("Long name")]
        public string LongName
        {
            get => weir2D.LongName;
            set => weir2D.LongName = value;
        }

        [DisplayName("Branch")]
        public string Branch => string.Empty;

        [DisplayName("Formula")]
        public string WeirFormulaName => weir2D.WeirFormula.Name;

        [DisplayName("Crest width")]
        public double CrestWidth
        {
            get => weir2D.CrestWidth;
            set => weir2D.CrestWidth = value;
        }

        [DisplayName("Crest level")]
        public double CrestLevel
        {
            get => weir2D.CrestLevel;
            set => weir2D.CrestLevel = value;
        }

        [DisplayName("Flow direction")]
        public FlowDirection FlowDirection
        {
            get => weir2D.FlowDirection;
            set => weir2D.FlowDirection = value;
        }

        [DisplayName("Group name")]
        public string GroupName
        {
            get => weir2D.GroupName;
            set => weir2D.GroupName = value;
        }

        /// <summary>
        /// Gets the underlying <see cref="Weir2D"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return weir2D;
        }
    }
}