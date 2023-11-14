using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows
{
    /// <summary>
    /// Representation object of a <see cref="ObservationCrossSection2D"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public class ObservationCrossSection2DRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly ObservationCrossSection2D observationCrossSection2D;

        /// <summary>
        /// Initialize a new instance of the <see cref="ObservationCrossSection2DRow"/> class.
        /// </summary>
        /// <param name="observationCrossSection2D"> The observation cross-section 2D to be presented. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="observationCrossSection2D"/> is <c>null</c>.
        /// </exception>
        public ObservationCrossSection2DRow(ObservationCrossSection2D observationCrossSection2D)
            : base(observationCrossSection2D)
        {
            Ensure.NotNull(observationCrossSection2D, nameof(observationCrossSection2D));
            this.observationCrossSection2D = observationCrossSection2D;
        }

        [DisplayName("Group name")]
        public string GroupName
        {
            get => observationCrossSection2D.GroupName;
            set => observationCrossSection2D.GroupName = value;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => observationCrossSection2D.Name;
            set => observationCrossSection2D.SetNameIfValid(value);
        }

        /// <summary>
        /// Gets the underlying <see cref="ObservationCrossSection2D"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return observationCrossSection2D;
        }
    }
}