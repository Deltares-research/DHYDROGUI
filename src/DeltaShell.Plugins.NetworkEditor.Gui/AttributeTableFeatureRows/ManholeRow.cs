using System.ComponentModel;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows
{
    /// <summary>
    /// Representation object of a <see cref="IManhole"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public sealed class ManholeRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly IManhole manhole;
        private readonly NameValidator nameValidator;

        /// <summary>
        /// Initialize a new instance of the <see cref="ManholeRow"/> class.
        /// </summary>
        /// <param name="manhole"> The manhole to be presented. </param>
        /// <param name="nameValidator"> The name validator to use when the name is set. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="manhole"/> or <paramref name="nameValidator"/> is <c>null</c>.
        /// </exception>
        public ManholeRow(IManhole manhole, NameValidator nameValidator)
            : base((INotifyPropertyChanged)manhole)
        {
            Ensure.NotNull(manhole, nameof(manhole));
            Ensure.NotNull(nameValidator, nameof(nameValidator));
            
            this.manhole = manhole;
            this.nameValidator = nameValidator;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => manhole.Name;
            set
            {
                if (nameValidator.ValidateWithLogging(value))
                {
                    manhole.Name = value;
                }
            }
        }

        [DisplayName("Compartments")]
        public int CompartmentCount => manhole.Compartments.Count;

        [DisplayName("X coordinate")]
        public double XCoordinate => manhole.Geometry.Coordinate.X;

        [DisplayName("Y coordinate")]
        public double YCoordinate => manhole.Geometry.Coordinate.Y;

        [DisplayName("Is on single branch")]
        public bool IsOnSingleBranch => manhole.IsOnSingleBranch;

        /// <summary>
        /// Gets the underlying <see cref="IManhole"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return manhole;
        }
    }
}