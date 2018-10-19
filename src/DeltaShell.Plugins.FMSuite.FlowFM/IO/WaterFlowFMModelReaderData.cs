using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.IO;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class WaterFlowFMModelReaderData
    {
        public IList<BranchFile.BranchProperties> PropertiesPerBranch { get; set; }
        public IList<NodeFile.CompartmentProperties> PropertiesPerCompartment { get; set; }
        public NetworkUGridDataModel NetworkDataModel { get; set; }
        public NetworkDiscretisationUGridDataModel NetworkDiscretisationDataModel { get; set; }
        public WaterFlowFMModelDefinition ModelDefinition { get; set; }
        public HydroArea Area { get; set; }
    }
}
