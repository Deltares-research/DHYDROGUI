using System;
using DelftTools.Hydro.CrossSections;
using NetTopologySuite.Extensions.Networks;

namespace DelftTools.Hydro
{
    [Serializable]
    public class Pipe : Branch, IPipe
    {
        public string PipeId { get; set; }
        public CrossSection CrossSectionShape { get; set; }
        public PipeType PipeType { get; set; }
        public double LevelSource { get; set; }
        public double LevelTarget { get; set; }
        /*
         * The following properties are inherated from the branch
         * NODE_ID_START
         * NODE_ID_END
         * LENGTH
        */

        public override bool IsLengthCustom
        {
            get { return true; }
        }
    }
}