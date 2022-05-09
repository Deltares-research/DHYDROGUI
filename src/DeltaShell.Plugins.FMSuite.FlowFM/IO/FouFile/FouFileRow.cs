namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.FouFile
{
    internal class FouFileRow
    {
        public string Var { get; set; }
        public double Tsrts { get; set; }
        public double Sstop { get; set; }
        public int Numcyc { get; set; }
        public int Knfac { get; set; }
        public int V0plu { get; set; }
        public int? Layno { get; set; }
        public string Elp { get; set; }
    }
}