using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows
{
    /// <summary>
    /// Representation object of a <see cref="Embankment"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public sealed class EmbankmentRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly Embankment embankment;

        /// <summary>
        /// Initialize a new instance of the <see cref="EmbankmentRow"/> class.
        /// </summary>
        /// <param name="embankment"> The embankment to be presented. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="embankment"/> is <c>null</c>.
        /// </exception>
        public EmbankmentRow(Embankment embankment)
            : base(embankment)
        {
            Ensure.NotNull(embankment, nameof(embankment));
            this.embankment = embankment;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => embankment.Name;
            set => embankment.SetNameIfValid(value);
        }

        /// <summary>
        /// Gets the underlying <see cref="Embankment"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return embankment;
        }
    }
}