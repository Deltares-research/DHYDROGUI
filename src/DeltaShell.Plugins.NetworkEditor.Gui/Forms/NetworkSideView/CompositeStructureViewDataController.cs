using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CompositeStructureView;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView
{
    /// <summary>
    /// CompositeStructureViewData is the same as 'normal' side view data but includes
    /// extra members for calculating min and max Y and selected structure
    /// 
    /// </summary>
    public class CompositeStructureViewDataController : NetworkSideViewDataController, IStructureViewData
    {
        public CompositeStructureViewDataController(ICompositeBranchStructure structure, Route route, NetworkSideViewCoverageManager coverageManager)
            : base(route, coverageManager)
        {
            CompositeBranchStructure = structure;
        }

        #region IStructureViewData Members

        public ICompositeBranchStructure CompositeBranchStructure { get; private set; }
        public ICrossSection CrossSectionBefore { get; set; }
        public ICrossSection CrossSectionAfter { get; set; }

        public HydroNetwork HydroNetwork
        {
            get
            {
                return (HydroNetwork) CompositeBranchStructure?.Network;
            }
            
        }

        #endregion

      
    }
}