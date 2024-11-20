using DelftTools.Utils.Aop;

namespace DelftTools.Hydro.CrossSections.StandardShapes
{
    [Entity(FireOnCollectionChange=false)]
    public abstract class CrossSectionStandardShapeWidthHeightBase : CrossSectionStandardShapeBase
    {
        public virtual double Width { get; set; }

        public virtual double Height { get; set; }
    }
}