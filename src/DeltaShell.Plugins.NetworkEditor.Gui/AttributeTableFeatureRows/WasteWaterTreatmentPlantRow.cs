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
    /// Representation object of a <see cref="WasteWaterTreatmentPlant"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public class WasteWaterTreatmentPlantRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly WasteWaterTreatmentPlant wasteWaterTreatmentPlant;
        private readonly NameValidator nameValidator;

        /// <summary>
        /// Initialize a new instance of the <see cref="WasteWaterTreatmentPlantRow"/> class.
        /// </summary>
        /// <param name="wasteWaterTreatmentPlant"> The waste water treatment plant to be presented. </param>
        /// <param name="nameValidator"> The name validator to use when the name is set. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="wasteWaterTreatmentPlant"/> or <paramref name="nameValidator"/> is <c>null</c>.
        /// </exception>
        public WasteWaterTreatmentPlantRow(WasteWaterTreatmentPlant wasteWaterTreatmentPlant, NameValidator nameValidator)
            : base((INotifyPropertyChanged)wasteWaterTreatmentPlant)
        {
            Ensure.NotNull(wasteWaterTreatmentPlant, nameof(wasteWaterTreatmentPlant));
            Ensure.NotNull(nameValidator, nameof(nameValidator));
            
            this.wasteWaterTreatmentPlant = wasteWaterTreatmentPlant;
            this.nameValidator = nameValidator;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => wasteWaterTreatmentPlant.Name;
            set
            {
                if (nameValidator.ValidateWithLogging(value))
                {
                    wasteWaterTreatmentPlant.Name = value;
                }
            }
        }

        [DisplayName("Description")]
        public string Description
        {
            get => wasteWaterTreatmentPlant.Description;
            set => wasteWaterTreatmentPlant.Description = value;
        }

        [DisplayName("LongName")]
        public string LongName
        {
            get => wasteWaterTreatmentPlant.LongName;
            set => wasteWaterTreatmentPlant.LongName = value;
        }

        /// <summary>
        /// Gets the underlying <see cref="WasteWaterTreatmentPlant"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return wasteWaterTreatmentPlant;
        }
    }
}