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
    /// Representation object of a <see cref="IHydroNode"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public sealed class HydroNodeRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly IHydroNode hydroNode;
        private readonly NameValidator nameValidator;

        /// <summary>
        /// Initialize a new instance of the <see cref="HydroNodeRow"/> class.
        /// </summary>
        /// <param name="hydroNode"> The hydro node to be presented. </param>
        /// <param name="nameValidator"> The name validator to use when the name is set. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="hydroNode"/> or <paramref name="nameValidator"/> is <c>null</c>.
        /// </exception>
        public HydroNodeRow(IHydroNode hydroNode, NameValidator nameValidator)
            : base((INotifyPropertyChanged)hydroNode)
        {
            Ensure.NotNull(hydroNode, nameof(hydroNode));
            Ensure.NotNull(nameValidator, nameof(nameValidator));
            
            this.hydroNode = hydroNode;
            this.nameValidator = nameValidator;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => hydroNode.Name;
            set
            {
                if (nameValidator.ValidateWithLogging(value))
                {
                    hydroNode.Name = value;
                }
            }
        }

        [DisplayName("Long name")]
        public string LongName
        {
            get => hydroNode.LongName;
            set => hydroNode.LongName = value;
        }

        [DisplayName("X coordinate")]
        public double XCoordinate => hydroNode.Geometry.Coordinate.X;

        [DisplayName("Y coordinate")]
        public double YCoordinate => hydroNode.Geometry.Coordinate.Y;

        [DisplayName("Incoming branches")]
        public int IncomingBranchesCount => hydroNode.IncomingBranches.Count;

        [DisplayName("Outgoing branches")]
        public int OutgoingBranchesCount => hydroNode.OutgoingBranches.Count;

        [DisplayName("Is on single branch")]
        public bool IsOnSingleBranch => hydroNode.IsOnSingleBranch;

        /// <summary>
        /// Gets the underlying <see cref="IHydroNode"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return hydroNode;
        }
    }
}