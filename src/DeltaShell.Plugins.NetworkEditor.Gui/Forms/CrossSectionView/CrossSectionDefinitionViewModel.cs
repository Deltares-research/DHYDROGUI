using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView
{
    /// <summary>
    /// Contains meta-information about the crossection that is only used in views. Possibly move more functionality from
    /// crossection to the viewmodel to
    /// keep the CS clean.
    /// </summary>
    public class CrossSectionDefinitionViewModel
    {
        private readonly bool isSymmetrical;

        private readonly string tableDescription;

        private readonly int maxSections;

        private readonly int minimalNumberOfTableRows;

        private readonly bool fixOnScreenRatio;

        private string yUnit;

        private string xUnit;

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

        /// <summary>
        /// Is the cross section reflection-symmetrical (along a vertical axis). Typically true for height-width defined cross
        /// sections
        /// </summary>
        public bool IsSymmetrical
        {
            get
            {
                return isSymmetrical;
            }
        }

        public string TableDescription
        {
            get
            {
                return tableDescription;
            }
        }

        /// <summary>
        /// Maximum number of roughness sections this cross section supports
        /// </summary>
        public virtual int MaxSections
        {
            get
            {
                return maxSections;
            }
        }

        public virtual int MinimalNumberOfTableRows
        {
            get
            {
                return minimalNumberOfTableRows;
            }
        }

        public bool FixOnScreenRatio
        {
            get
            {
                return fixOnScreenRatio;
            }
        }

        public string YUnit
        {
            get
            {
                return yUnit;
            }
        }

        public string XUnit
        {
            get
            {
                return xUnit;
            }
        }

        public IEventedList<CrossSectionSectionType> CrossSectionSectionTypes { get; set; }

        public IHydroNetwork HydroNetwork { get; set; }
    }
}