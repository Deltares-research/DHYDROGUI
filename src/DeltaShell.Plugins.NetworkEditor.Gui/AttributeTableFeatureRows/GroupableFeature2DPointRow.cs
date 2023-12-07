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
    /// Representation object of a <see cref="GroupableFeature2DPoint"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public class GroupableFeature2DPointRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly GroupableFeature2DPoint feature;
        private readonly NameValidator nameValidator;

        /// <summary>
        /// Initialize a new instance of the <see cref="GroupableFeature2DPolygonRow"/> class.
        /// </summary>
        /// <param name="feature"> The feature to be presented. </param>
        /// <param name="nameValidator"> The name validator to use when the name is set. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="feature"/> or <paramref name="nameValidator"/> is <c>null</c>.
        /// </exception>
        public GroupableFeature2DPointRow(GroupableFeature2DPoint feature, NameValidator nameValidator)
            : base(feature)
        {
            Ensure.NotNull(feature, nameof(feature));
            Ensure.NotNull(nameValidator, nameof(nameValidator));
            
            this.feature = feature;
            this.nameValidator = nameValidator;
        }

        [DisplayName("Group name")]
        public string GroupName
        {
            get => feature.GroupName;
            set => feature.GroupName = value;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => feature.Name;
            set
            {
                if (nameValidator.ValidateWithLogging(value))
                {
                    feature.Name = value;
                }
            }
        }

        [DisplayName("X")]
        public double X => feature.X;

        [DisplayName("Y")]
        public double Y => feature.Y;

        /// <summary>
        /// Gets the underlying <see cref="GroupableFeature2DPoint"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return feature;
        }
    }
}