using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView
{
    /// <summary>
    /// Contains meta-information about the crossection that is only used in views. Possibly move more functionality from crossection to the viewmodel to
    /// keep the CS clean.
    /// </summary>
    public class CrossSectionDefinitionViewModel
    {
        public CrossSectionDefinitionViewModel(bool isSymmetrical, string tableDescription, int maxSections, int minimalNumberOfTableRows, string xUnit, string yUnit, bool fixOnScreenRatio)
        {
            this.isSymmetrical = isSymmetrical;
            this.tableDescription = tableDescription;
            this.maxSections = maxSections;
            this.minimalNumberOfTableRows = minimalNumberOfTableRows;
            this.fixOnScreenRatio = fixOnScreenRatio;
            this.yUnit = yUnit;
            this.xUnit = xUnit;
        }
        
        private readonly bool isSymmetrical;
        /// <summary>
        /// Is the cross section reflection-symmetrical (along a vertical axis). Typically true for height-width defined cross sections
        /// </summary>
        public bool IsSymmetrical
        {
            get { return isSymmetrical; }
        }


        private readonly string tableDescription;
        public string TableDescription
        {
            get { return tableDescription; }
        }

        private readonly int maxSections;

        /// <summary>
        /// Maximum number of roughness sections this cross section supports
        /// </summary>
        public virtual int MaxSections
        {
            get { return maxSections; }
        }

        private readonly int minimalNumberOfTableRows;
        
        public virtual int MinimalNumberOfTableRows
        {
            get { return minimalNumberOfTableRows; }
        }

        private readonly bool fixOnScreenRatio;
        public bool FixOnScreenRatio
        {
            get { return fixOnScreenRatio; }

        }

        private string yUnit;
        public string YUnit
        {
            get { return yUnit; }
        }

        private string xUnit;
        public string XUnit
        {
            get { return xUnit; }
        }

        public IEventedList<CrossSectionSectionType> CrossSectionSectionTypes
        {
            get; set;
        }

        public IHydroNetwork HydroNetwork { get; set; }
        public bool IsCurrentlyOnChannel { get; set; } = true;
    }
}
