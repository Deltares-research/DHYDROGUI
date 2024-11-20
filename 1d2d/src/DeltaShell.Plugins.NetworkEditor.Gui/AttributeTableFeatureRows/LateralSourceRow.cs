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
    /// Representation object of a <see cref="ILateralSource"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public sealed class LateralSourceRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly ILateralSource lateralSource;
        private readonly NameValidator nameValidator;

        /// <summary>
        /// Initialize a new instance of the <see cref="LateralSourceRow"/> class.
        /// </summary>
        /// <param name="lateralSource"> The lateral source to be presented. </param>
        /// <param name="nameValidator"> The name validator to use when the name is set. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="lateralSource"/> or <paramref name="nameValidator"/> is <c>null</c>.
        /// </exception>
        public LateralSourceRow(ILateralSource lateralSource, NameValidator nameValidator)
            : base((INotifyPropertyChanged)lateralSource)
        {
            Ensure.NotNull(lateralSource, nameof(lateralSource));
            Ensure.NotNull(nameValidator, nameof(nameValidator));
            
            this.lateralSource = lateralSource;
            this.nameValidator = nameValidator;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => lateralSource.Name;
            set
            {
                if (nameValidator.ValidateWithLogging(value))
                {
                    lateralSource.Name = value;
                }
            }
        }

        [DisplayName("Long name")]
        public string LongName
        {
            get => lateralSource.LongName;
            set => lateralSource.LongName = value;
        }

        [DisplayName("Branch")]
        public string Branch => lateralSource.Branch.Name;

        [DisplayName("Chainage")]
        public double Chainage
        {
            get => lateralSource.Chainage;
            set => lateralSource.Chainage = value;
        }

        [DisplayName("Diffuse lateral")]
        public bool DiffuseLateral => lateralSource.IsDiffuse;

        [DisplayName("Length")]
        public double Length => lateralSource.Length;

        /// <summary>
        /// Gets the underlying <see cref="ILateralSource"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return lateralSource;
        }
    }
}