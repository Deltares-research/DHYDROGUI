using DeltaShell.NGHS.IO.Store1D;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class FmMapFile1DOutputFileReader : Output1DFileReader<TimeDependentVariableMetaDataBase>
    {
        public FmMapFile1DOutputFileReader()
        {
            timeVariableNameInNetCDFFile = "time";
            timeDimensionNameInNetCdfFile = "time";

            unitsAttributeKeyNameInNetCdfFile = "units";
            timeVariableUnitValuePrefixInNetCdfFile = "seconds since";
            dateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            longNameAttributeKeyNameInNetCdfFile = "long_name";
        }
    }
}