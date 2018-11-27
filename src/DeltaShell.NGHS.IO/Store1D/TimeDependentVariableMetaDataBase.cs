namespace DeltaShell.NGHS.IO.Store1D
{
    public interface ITimeDependentVariableMetaDataBase
    {
        string Name { get; }
        string LongName { get; }
        string Unit { get; }
    }

    public class TimeDependentVariableMetaDataBase : ITimeDependentVariableMetaDataBase
    {
        public TimeDependentVariableMetaDataBase()
        {
            
        }
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