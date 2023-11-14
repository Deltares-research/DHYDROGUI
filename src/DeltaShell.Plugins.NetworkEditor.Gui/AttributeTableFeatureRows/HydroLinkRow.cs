using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows
{
    /// <summary>
    /// Representation object of a <see cref="HydroLink"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public class HydroLinkRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly HydroLink hydroLink;

        /// <summary>
        /// Initialize a new instance of the <see cref="HydroLinkRow"/> class.
        /// </summary>
        /// <param name="hydroLink"> The hydro link to be presented. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="hydroLink"/> is <c>null</c>.
        /// </exception>
        public HydroLinkRow(HydroLink hydroLink)
            : base((INotifyPropertyChanged)hydroLink)
        {
            Ensure.NotNull(hydroLink, nameof(hydroLink));
            this.hydroLink = hydroLink;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => hydroLink.Name;
            set => hydroLink.SetNameIfValid(value);
        }

        [DisplayName("Source")]
        public string Source => hydroLink.Source.Name;

        [DisplayName("Target")]
        public string Target => hydroLink.Target.Name;

        /// <summary>
        /// Gets the underlying <see cref="HydroLink"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return hydroLink;
        }
    }
}