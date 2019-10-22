using DeltaShell.NGHS.IO.Store1D;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class FmMapFile1DOutputFileReader : Output1DFileReader<LocationMetaData, TimeDependentVariableMetaDataBase>
    {
        public FmMapFile1DOutputFileReader()
        {
            branchidVariableNameInNetCDFFile = "mesh1d_node_branch";
            chainageVariableNameInNetCDFFile = "mesh1d_node_offset";
            cfRoleAttributeNameInNetCdfFile = "long_name";
            cfRoleAttributeValueInNetCdfFile = "ID of mesh nodes";

            timeVariableNameInNetCDFFile = "time";
            timeDimensionNameInNetCdfFile = "time";

            xNodeCoordinateVariableNameInNetCDFFile = "mesh1d_node_x";
            yNodeCoordinateVariableNameInNetCDFFile = "mesh1d_node_y";

            edgeIdVariableNameInNetCDFFile = "mesh1d_edge_nodes";
            edgeBranchidVariableNameInNetCDFFile = "mesh1d_edge_branch";
            edgeChainageVariableNameInNetCDFFile = "mesh1d_edge_offset";

            xEdgeCoordinateVariableNameInNetCDFFile = "mesh1d_edge_x";
            yEdgeCoordinateVariableNameInNetCDFFile = "mesh1d_edge_y";
            
            unitsAttributeKeyNameInNetCdfFile = "units";
            timeVariableUnitValuePrefixInNetCdfFile = "seconds since";
            dateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            longNameAttributeKeyNameInNetCdfFile = "long_name";
        }
    }
}