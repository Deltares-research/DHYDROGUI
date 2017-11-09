using System.Collections.Generic;

namespace DelftTools.Hydro
{
    public class Manhole : HydroNode
    {
        public long ManholeId { get; set; }

        public ICollection<ManholeCompartment> Compartments { get; set; }

        public ICollection<IStructure> Connections { get; set; }
    }
}
