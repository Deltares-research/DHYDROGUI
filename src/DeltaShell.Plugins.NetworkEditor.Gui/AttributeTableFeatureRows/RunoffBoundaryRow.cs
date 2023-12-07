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
    /// Representation object of a <see cref="RunoffBoundaryRow"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public sealed class RunoffBoundaryRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly RunoffBoundary runoffBoundary;
        private readonly NameValidator nameValidator;

        /// <summary>
        /// Initialize a new instance of the <see cref="RunoffBoundaryRow"/> class.
        /// </summary>
        /// <param name="runoffBoundary"> The runoff boundary to be presented. </param>
        /// <param name="nameValidator"> The name validator to use when the name is set. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="runoffBoundary"/> or <paramref name="nameValidator"/> is <c>null</c>.
        /// </exception>
        public RunoffBoundaryRow(RunoffBoundary runoffBoundary, NameValidator nameValidator)
            : base((INotifyPropertyChanged)runoffBoundary)
        {
            Ensure.NotNull(runoffBoundary, nameof(runoffBoundary));
            Ensure.NotNull(nameValidator, nameof(nameValidator));
            
            this.runoffBoundary = runoffBoundary;
            this.nameValidator = nameValidator;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => runoffBoundary.Name;
            set
            {
                if (nameValidator.ValidateWithLogging(value))
                {
                    runoffBoundary.Name = value;
                }
            }
        }

        [DisplayName("Description")]
        public string Description
        {
            get => runoffBoundary.Description;
            set => runoffBoundary.Description = value;
        }

        [DisplayName("Long name")]
        public string LongName
        {
            get => runoffBoundary.LongName;
            set => runoffBoundary.LongName = value;
        }

        /// <summary>
        /// Gets the underlying <see cref="RunoffBoundary"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return runoffBoundary;
        }
    }
}