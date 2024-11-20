using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Utils.Validation;
using DelftTools.Utils.Validation.NameValidation;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows
{
    /// <summary>
    /// Representation object of a <see cref="IObservationPoint"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public sealed class ObservationPointRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly IObservationPoint observationPoint;
        private readonly NameValidator nameValidator;

        /// <summary>
        /// Initialize a new instance of the <see cref="ObservationPointRow"/> class.
        /// </summary>
        /// <param name="observationPoint"> The observation point to be presented. </param>
        /// <param name="nameValidator"> The name validator to use when the name is set. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="observationPoint"/> or <paramref name="nameValidator"/> is <c>null</c>.
        /// </exception>
        public ObservationPointRow(IObservationPoint observationPoint, NameValidator nameValidator)
            : base((INotifyPropertyChanged)observationPoint)
        {
            Ensure.NotNull(observationPoint, nameof(observationPoint));
            Ensure.NotNull(nameValidator, nameof(nameValidator));
            
            this.observationPoint = observationPoint;
            this.nameValidator = nameValidator;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => observationPoint.Name;
            set
            {
                if (nameValidator.ValidateWithLogging(value))
                {
                    observationPoint.Name = value;
                }
            }
        }

        [DisplayName("Long name")]
        public string LongName
        {
            get => observationPoint.LongName;
            set => observationPoint.LongName = value;
        }

        [DisplayName("Branch")]
        public string Branch => observationPoint.Branch.Name;

        [DisplayName("Chainage")]
        public double Chainage => observationPoint.Chainage;

        /// <summary>
        /// Gets the underlying <see cref="IObservationPoint"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return observationPoint;
        }
    }
}