using DelftTools.Utils.Data;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public class NwrwSpecialArea : Unique<long>
    {
        public virtual int Area { get; set; }
        public virtual string SpecialInflowReference { get; set; }
    }
}
