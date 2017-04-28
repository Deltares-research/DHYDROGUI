using System.Data;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public enum SaltBoundaryNodeType
    {
        Concentration = 0,
        ZeroFlux = 1
    }

    public class SobekSaltNodeBoundary
    {
        public string Id { get; set; }
        public SaltStorageType SaltStorageType { get; set; }
        public SaltBoundaryNodeType SaltBoundaryNodeType { get; set; }
        public double ConcentrationConst { get; set; }
        public double TimeLag { get; set; }
        public DataTable ConcentrationTable { get; set; }
    }
}