using System.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.NetworkEditor;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class WaterFlowFMModelReaderData
    {
        public List<DelftIniCategory> PropertiesPerBranch { get; set; }
        public NetworkUGridDataModel NetworkDataModel { get; set; }

    }
}
