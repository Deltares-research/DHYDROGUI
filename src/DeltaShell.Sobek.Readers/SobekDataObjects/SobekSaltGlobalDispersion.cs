namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public enum DispersionOptionType
    {
        Option1, // 0 
        Option2, // 1
        ThatcherHarlemann, // 2
        Empirical // 3
    }

    public enum DispersionType
    {
        Constant, // 0 
        FunctionOfTime, // 1
        FunctionOfPlace // 2
    }

    public class SobekSaltGlobalDispersion
    {
        public DispersionOptionType DispersionOptionType { get; set; }
        public DispersionType DispersionType { get; set; }

        // the meaning of F1 .. F4 depends on the DispersionOptionType
        public double F1 { get; set; }
        public double F2 { get; set; }
        public double F3 { get; set; }
        public double F4 { get; set; }

        public SobekSaltLocalDispersion SobekSaltLocalDispersion { get; set; }
    }
}