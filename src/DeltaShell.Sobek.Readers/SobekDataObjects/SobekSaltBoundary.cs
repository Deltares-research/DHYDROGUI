using System.Data;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public enum SaltStorageType
    {
        Constant = 0,
        FunctionOfTime = 1
    }

    public enum SaltBoundaryType
    {
        DrySubstance = 0,
        Concentration = 1
    }

    public class SobekSaltBoundary
    {
        public string Id { get; set; }
        public double Length { get; set; }
        public string LateralId { get; set; }
        public SaltStorageType SaltStorageType { get; set; }
        public SaltBoundaryType SaltBoundaryType { get; set; }

        public double ConcentrationConst { get; set; }
        public DataTable ConcentrationTable { get; set; }

        public double DryLoadConst { get; set; }
        public DataTable DryLoadTable { get; set; }
    }
}