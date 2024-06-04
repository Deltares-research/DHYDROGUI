using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows
{
    /// <summary>
    /// Representation object of a <see cref="LandBoundary2D"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public class LandBoundary2DRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly LandBoundary2D landBoundary2D;
        private readonly NameValidator nameValidator;

        /// <summary>
        /// Initialize a new instance of the <see cref="LandBoundary2DRow"/> class.
        /// </summary>
        /// <param name="landBoundary2D"> The land boundary 2D to be presented. </param>
        /// <param name="nameValidator"> The name validator to use when the name is set. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="landBoundary2D"/> or <paramref name="nameValidator"/> is <c>null</c>.
        /// </exception>
        public LandBoundary2DRow(LandBoundary2D landBoundary2D, NameValidator nameValidator)
            : base(landBoundary2D)
        {
            Ensure.NotNull(landBoundary2D, nameof(landBoundary2D));
            Ensure.NotNull(nameValidator, nameof(nameValidator));
            
            this.landBoundary2D = landBoundary2D;
            this.nameValidator = nameValidator;
        }

        [DisplayName("Group name")]
        public string GroupName
        {
            get => landBoundary2D.GroupName;
            set => landBoundary2D.GroupName = value;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => landBoundary2D.Name;
            set
            {
                if (nameValidator.ValidateWithLogging(value))
                {
                    landBoundary2D.Name = value;
                }
            }
        }

        /// <summary>
        /// Gets the underlying <see cref="LandBoundary2D"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return landBoundary2D;
        }
    }
}