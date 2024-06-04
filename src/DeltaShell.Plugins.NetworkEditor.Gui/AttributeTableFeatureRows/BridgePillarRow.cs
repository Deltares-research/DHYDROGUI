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
    /// Representation object of a <see cref="BridgePillar"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public sealed class BridgePillarRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly BridgePillar bridgePillar;
        private readonly NameValidator nameValidator;

        /// <summary>
        /// Initialize a new instance of the <see cref="BridgePillarRow"/> class.
        /// </summary>
        /// <param name="bridgePillar"> The bridge pillar to be presented. </param>
        /// <param name="nameValidator"> The name validator to use when the name is set. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="bridgePillar"/> or <paramref name="nameValidator"/> is <c>null</c>.
        /// </exception>
        public BridgePillarRow(BridgePillar bridgePillar, NameValidator nameValidator)
            : base(bridgePillar)
        {
            Ensure.NotNull(bridgePillar, nameof(bridgePillar));
            Ensure.NotNull(nameValidator, nameof(nameValidator));
            
            this.bridgePillar = bridgePillar;
            this.nameValidator = nameValidator;
        }

        [DisplayName("Group name")]
        public string GroupName
        {
            get => bridgePillar.GroupName;
            set => bridgePillar.GroupName = value;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => bridgePillar.Name;
            set
            {
                if (nameValidator.ValidateWithLogging(value))
                {
                    bridgePillar.Name = value;
                }
            }
        }

        /// <summary>
        /// Gets the underlying <see cref="BridgePillar"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return bridgePillar;
        }
    }
}