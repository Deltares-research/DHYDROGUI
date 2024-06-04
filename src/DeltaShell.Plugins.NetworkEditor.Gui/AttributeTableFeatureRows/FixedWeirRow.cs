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
    /// Representation object of a <see cref="FixedWeir"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public class FixedWeirRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly FixedWeir fixedWeir;
        private readonly NameValidator nameValidator;

        /// <summary>
        /// Initialize a new instance of the <see cref="FixedWeirRow"/> class.
        /// </summary>
        /// <param name="fixedWeir"> The fixed weir to be presented. </param>
        /// <param name="nameValidator"> The name validator to use when the name is set. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="fixedWeir"/> or <paramref name="nameValidator"/> is <c>null</c>.
        /// </exception>
        public FixedWeirRow(FixedWeir fixedWeir, NameValidator nameValidator)
            : base(fixedWeir)
        {
            Ensure.NotNull(fixedWeir, nameof(fixedWeir));
            Ensure.NotNull(nameValidator, nameof(nameValidator));
            
            this.fixedWeir = fixedWeir;
            this.nameValidator = nameValidator;
        }

        [DisplayName("Group name")]
        public string GroupName
        {
            get => fixedWeir.GroupName;
            set => fixedWeir.GroupName = value;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => fixedWeir.Name;
            set
            {
                if (nameValidator.ValidateWithLogging(value))
                {
                    fixedWeir.Name = value;
                }
            }
        }

        /// <summary>
        /// Gets the underlying <see cref="FixedWeir"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return fixedWeir;
        }
    }
}