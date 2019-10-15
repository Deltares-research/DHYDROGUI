using DeltaShell.NGHS.IO.Store1D;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class FmMapFile1DOutputFileReader : Output1DFileReader<LocationMetaData, TimeDependentVariableMetaDataBase>
    {
        public FmMapFile1DOutputFileReader()
        {
            branchidVariableNameInNetCDFFile = "network_branch_id";
            chainageVariableNameInNetCDFFile = "mesh1d_node_offset";
            cfRoleAttributeNameInNetCdfFile = "mesh1d_node_long_name";
            cfRoleAttributeValueInNetCdfFile = "Long name of mesh nodes";

            timeVariableNameInNetCDFFile = "time";
            timeDimensionNameInNetCdfFile = "time";

            xCoordinateVariableNameInNetCDFFile = "mesh1d_node_x";
            yCoordinateVariableNameInNetCDFFile = "mesh1d_node_y";
            unitsAttributeKeyNameInNetCdfFile = "units";
            timeVariableUnitValuePrefixInNetCdfFile = "seconds since";
            dateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            longNameAttributeKeyNameInNetCdfFile = "long_name";
        }
    }
}