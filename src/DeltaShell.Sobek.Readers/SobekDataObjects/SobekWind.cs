using DelftTools.Functions;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekWind
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string BranchId { get; set; }
        public bool IsGlobal { get { return BranchId == "-1"; } }
        public bool Used { get; set; }
        public bool IsConstantVelocity { get; set; }
        public bool IsConstantDirection { get; set; }
        public double ConstantDirection { get; set; }
        public double ConstantVelocity { get; set; }
        public IFunction Wind { get; set; }
    }
}
