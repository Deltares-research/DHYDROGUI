using System.ComponentModel;
using DelftTools.Utils.Validation;
using DelftTools.Utils.Validation.NameValidation;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows
{
    /// <summary>
    /// Representation object of a <see cref="Feature2D"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public class Feature2DRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly Feature2D feature2D;
        private readonly NameValidator nameValidator;

        /// <summary>
        /// Initialize a new instance of the <see cref="Feature2DRow"/> class.
        /// </summary>
        /// <param name="feature2D"> The feature 2D to be presented. </param>
        /// <param name="nameValidator"> The name validator to use when the name is set. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="feature2D"/> or <paramref name="nameValidator"/> is <c>null</c>.
        /// </exception>
        public Feature2DRow(Feature2D feature2D, NameValidator nameValidator)
            : base(feature2D)
        {
            Ensure.NotNull(feature2D, nameof(feature2D));
            Ensure.NotNull(nameValidator, nameof(nameValidator));
            
            this.feature2D = feature2D;
            this.nameValidator = nameValidator;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => feature2D.Name;
            set
            {
                if (nameValidator.ValidateWithLogging(value))
                {
                    feature2D.Name = value;
                }
            }
        }

        /// <summary>
        /// Gets the underlying <see cref="Feature2D"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return feature2D;
        }
    }
}