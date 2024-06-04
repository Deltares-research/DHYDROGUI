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
    /// Representation object of a <see cref="Embankment"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public sealed class EmbankmentRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly Embankment embankment;
        private readonly NameValidator nameValidator;

        /// <summary>
        /// Initialize a new instance of the <see cref="EmbankmentRow"/> class.
        /// </summary>
        /// <param name="embankment"> The embankment to be presented. </param>
        /// <param name="nameValidator"> The name validator to use when the name is set. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="embankment"/> or <paramref name="nameValidator"/> is <c>null</c>.
        /// </exception>
        public EmbankmentRow(Embankment embankment, NameValidator nameValidator)
            : base(embankment)
        {
            Ensure.NotNull(embankment, nameof(embankment));
            Ensure.NotNull(nameValidator, nameof(nameValidator));
            
            this.embankment = embankment;
            this.nameValidator = nameValidator;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => embankment.Name;
            set
            {
                if (nameValidator.ValidateWithLogging(value))
                {
                    embankment.Name = value;
                }
            }
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