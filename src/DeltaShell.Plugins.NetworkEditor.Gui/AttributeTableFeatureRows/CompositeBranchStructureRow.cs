using System.ComponentModel;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows
{
    /// <summary>
    /// Representation object of a <see cref="ICompositeBranchStructure"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public class CompositeBranchStructureRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly ICompositeBranchStructure compositeBranchStructure;

        /// <summary>
        /// Initialize a new instance of the <see cref="CompositeBranchStructureRow"/> class.
        /// </summary>
        /// <param name="compositeBranchStructure"> The composite branch structure to be presented. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="compositeBranchStructure"/> is <c>null</c>.
        /// </exception>
        public CompositeBranchStructureRow(ICompositeBranchStructure compositeBranchStructure)
            : base((INotifyPropertyChanged)compositeBranchStructure)
        {
            Ensure.NotNull(compositeBranchStructure, nameof(compositeBranchStructure));
            this.compositeBranchStructure = compositeBranchStructure;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => compositeBranchStructure.Name;
            set => compositeBranchStructure.SetNameIfValid(value);
        }

        [DisplayName("Long name")]
        public string LongName
        {
            get => compositeBranchStructure.LongName;
            set => compositeBranchStructure.LongName = value;
        }

        [DisplayName("Branch")]
        public string Branch => compositeBranchStructure.Branch.Name;

        [DisplayName("Structures")]
        public int Structures => compositeBranchStructure.Structures.Count;

        /// <summary>
        /// Gets the underlying <see cref="ICompositeBranchStructure"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return compositeBranchStructure;
        }
    }
}