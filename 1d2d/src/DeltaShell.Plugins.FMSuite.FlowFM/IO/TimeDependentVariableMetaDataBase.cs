namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class TimeDependentVariableMetaDataBase
    {
        public TimeDependentVariableMetaDataBase(string name, string longName, string unit)
        {
            Name = name;
            LongName = longName;
            Unit = unit;
        }

        public string Name { get; set; }
        public string LongName { get; set; }
        public string Unit { get; set; }
    }
}