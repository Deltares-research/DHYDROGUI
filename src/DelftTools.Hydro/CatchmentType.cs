using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Utils;

namespace DelftTools.Hydro
{
    public class CatchmentType : INameable, ICloneable
    {


        public IEnumerable<CatchmentType> SubCatchmentTypes => throw new NotImplementedException();

        public string Name { get; set; }
        public object Clone() => throw new NotImplementedException();
    }
}