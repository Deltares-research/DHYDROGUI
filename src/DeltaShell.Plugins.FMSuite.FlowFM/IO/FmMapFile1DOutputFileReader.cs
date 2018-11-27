using DeltaShell.NGHS.IO.Store1D;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class FmMapFile1DOutputFileReader : Output1DFileReader<LocationMetaData, TimeDependentVariableMetaDataBase>
    {
        public FmMapFile1DOutputFileReader()
        {
            branchidVariableNameInNetCDFFile = "mesh1d_nodes_branch_id";
            chainageVariableNameInNetCDFFile = "mesh1d_nodes_branch_offset";
            cfRoleAttributeNameInNetCdfFile = "long_name";
            cfRoleAttributeValueInNetCdfFile = "the node ids";

            timeVariableNameInNetCDFFile = "time";
            timeDimensionNameInNetCdfFile = "time";

            xCoordinateVariableNameInNetCDFFile = "x_coordinate";
            yCoordinateVariableNameInNetCDFFile = "y_coordinate";
            unitsAttributeKeyNameInNetCdfFile = "units";
            timeVariableUnitValuePrefixInNetCdfFile = "seconds since";
            dateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            longNameAttributeKeyNameInNetCdfFile = "long_name";
        }
    }
}