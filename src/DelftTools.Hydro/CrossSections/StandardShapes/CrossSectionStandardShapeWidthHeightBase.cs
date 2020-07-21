using System;
using DelftTools.Utils.Aop;

namespace DelftTools.Hydro.CrossSections.StandardShapes
{
    [Obsolete("D3DFMIQ-1923 remove cross section")]
    [Entity(FireOnCollectionChange = false)]
    public abstract class CrossSectionStandardShapeWidthHeightBase : CrossSectionStandardShapeBase
    {
        public virtual double Width { get; set; }

        public virtual double Height { get; set; }
    }
}