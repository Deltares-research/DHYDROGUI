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
    /// Representation object of a <see cref="Gully"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public class GullyRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly Gully gully;
        private readonly NameValidator nameValidator;

        /// <summary>
        /// Initialize a new instance of the <see cref="GullyRow"/> class.
        /// </summary>
        /// <param name="gully"> The gully to be presented. </param>
        /// <param name="nameValidator"> The name validator to use when the name is set. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="gully"/> or <paramref name="nameValidator"/> is <c>null</c>.
        /// </exception>
        public GullyRow(Gully gully, NameValidator nameValidator)
            : base(gully)
        {
            Ensure.NotNull(gully, nameof(gully));
            Ensure.NotNull(nameValidator, nameof(nameValidator));
            
            this.gully = gully;
            this.nameValidator = nameValidator;
        }

        [DisplayName("Group name")]
        public string GroupName
        {
            get => gully.GroupName;
            set => gully.GroupName = value;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => gully.Name;
            set
            {
                if (nameValidator.ValidateWithLogging(value))
                {
                    gully.Name = value;
                }
            }
        }

        [DisplayName("X")]
        public double X => gully.X;

        [DisplayName("Y")]
        public double Y => gully.Y;

        /// <summary>
        /// Gets the underlying <see cref="Gully"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return gully;
        }
    }
}