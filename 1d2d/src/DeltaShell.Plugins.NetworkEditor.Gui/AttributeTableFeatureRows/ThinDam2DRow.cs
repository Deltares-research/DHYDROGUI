using System.ComponentModel;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Validation;
using DelftTools.Utils.Validation.NameValidation;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows
{
    /// <summary>
    /// Representation object of a <see cref="ThinDam2D"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public class ThinDam2DRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly ThinDam2D thinDam2D;
        private readonly NameValidator nameValidator;

        /// <summary>
        /// Initialize a new instance of the <see cref="ThinDam2DRow"/> class.
        /// </summary>
        /// <param name="thinDam2D"> The thin dam 2D to be presented. </param>
        /// <param name="nameValidator"> The name validator to use when the name is set. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="thinDam2D"/> or <paramref name="nameValidator"/> is <c>null</c>.
        /// </exception>
        public ThinDam2DRow(ThinDam2D thinDam2D, NameValidator nameValidator)
            : base(thinDam2D)
        {
            Ensure.NotNull(thinDam2D, nameof(thinDam2D));
            Ensure.NotNull(nameValidator, nameof(nameValidator));
            
            this.thinDam2D = thinDam2D;
            this.nameValidator = nameValidator;
        }

        [DisplayName("Group name")]
        public string GroupName
        {
            get => thinDam2D.GroupName;
            set => thinDam2D.GroupName = value;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => thinDam2D.Name;
            set
            {
                if (nameValidator.ValidateWithLogging(value))
                {
                    thinDam2D.Name = value;
                }
            }
        }

        /// <summary>
        /// Gets the underlying <see cref="ThinDam2D"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return thinDam2D;
        }
    }
}