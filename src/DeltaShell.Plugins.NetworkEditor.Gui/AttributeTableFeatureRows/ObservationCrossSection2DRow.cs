using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation.Common;
using DelftTools.Utils.Validation.NameValidation;
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
        private readonly NameValidator nameValidator;

        /// <summary>
        /// Initialize a new instance of the <see cref="ObservationCrossSection2DRow"/> class.
        /// </summary>
        /// <param name="observationCrossSection2D"> The observation cross-section 2D to be presented. </param>
        /// <param name="nameValidator"> The name validator to use when the name is set. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="observationCrossSection2D"/> or <paramref name="nameValidator"/> is <c>null</c>.
        /// </exception>
        public ObservationCrossSection2DRow(ObservationCrossSection2D observationCrossSection2D, NameValidator nameValidator)
            : base(observationCrossSection2D)
        {
            Ensure.NotNull(observationCrossSection2D, nameof(observationCrossSection2D));
            Ensure.NotNull(nameValidator, nameof(nameValidator));
            
            this.observationCrossSection2D = observationCrossSection2D;
            this.nameValidator = nameValidator;
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
            set
            {
                if (nameValidator.ValidateWithLogging(value))
                {
                    observationCrossSection2D.Name = value;
                }
            }
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