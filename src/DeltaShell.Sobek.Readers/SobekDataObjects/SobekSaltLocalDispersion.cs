using System.Data;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    /// <summary>
    /// SobekSaltLocalDispersion holds dispersion on branch. This is only relevant if in GLDS record type is given as function of place.
    /// In this case the GLDS also contains a DSPN record with branchId -1.
    /// 
    /// </summary>
    public class SobekSaltLocalDispersion
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string BranchId { get; set; }
        public DispersionType DispersionType { get; set; }

        // the meaning of F1 .. F4 depends on the DispersionOptionType given in the global dispersion record
        public double F1 { get; set; }
        public double F2 { get; set; }
        public double F3 { get; set; }
        public double F4 { get; set; }

        public DataTable Data { get; set; }
    }
}