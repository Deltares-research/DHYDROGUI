using System;
using DelftTools.Hydro.CrossSections;

namespace DelftTools.Hydro
{
    [Serializable]
    public class Pipe : SewerConnection, IPipe
    {
        public CrossSection CrossSectionShape { get; set; }
        public string PipeId { get; set; }
    }
}