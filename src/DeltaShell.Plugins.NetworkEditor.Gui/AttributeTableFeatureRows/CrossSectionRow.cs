using System;
using System.ComponentModel;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows
{
    /// <summary>
    /// Representation object of a <see cref="ICrossSection"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public class CrossSectionRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly ICrossSection crossSection;
        private readonly NameValidator nameValidator;

        /// <summary>
        /// Initialize a new instance of the <see cref="CrossSectionRow"/> class.
        /// </summary>
        /// <param name="crossSection"> The cross-section to be presented. </param>
        /// <param name="nameValidator"> The name validator to use when the name is set. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="crossSection"/> or <paramref name="nameValidator"/> is <c>null</c>.
        /// </exception>
        public CrossSectionRow(ICrossSection crossSection, NameValidator nameValidator)
            : base((INotifyPropertyChanged)crossSection)
        {
            Ensure.NotNull(crossSection, nameof(crossSection));
            Ensure.NotNull(nameValidator, nameof(nameValidator));
            
            this.crossSection = crossSection;
            this.nameValidator = nameValidator;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => crossSection.Name;
            set
            {
                if (nameValidator.ValidateWithLogging(value))
                {
                    crossSection.Name = value;
                }
            }
        }

        [DisplayName("Long name")]
        public string LongName
        {
            get => crossSection.LongName;
            set => crossSection.LongName = value;
        }

        [DisplayName("Branch")]
        public string Branch => crossSection.Branch.Name;

        [DisplayName("Chainage")]
        public double Chainage
        {
            get => crossSection.Chainage;
            set => crossSection.Chainage = value;
        }

        [DisplayName("Lowest point")]
        public double LowestPoint => crossSection.Definition.LowestPoint;

        [DisplayName("Highest point")]
        public double HighestPoint => crossSection.Definition.HighestPoint;

        [DisplayName("Type")]
        public CrossSectionType CrossSectionType => crossSection.Definition.CrossSectionType;

        [DisplayName("Width")]
        public double Width => crossSection.Definition.Width;

        [DisplayName("Thalweg")]
        public double Thalweg => Math.Round(crossSection.Definition.Thalweg, 2);

        [DisplayName("Definition")]
        public string DefinitionName => crossSection.Definition.Name;

        /// <summary>
        /// Gets the underlying <see cref="ICrossSection"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return crossSection;
        }
    }
}