using System.Collections.Generic;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekRRUnitHydrograph
    {
        public SobekRRUnitHydrograph()
        {
            Values = new List<double>(new double[36]);
        }

        public string Id { get; set; }

        public double Stepsize { get; set; }

        public IList<double> Values { get; private set; } 
    }
}
