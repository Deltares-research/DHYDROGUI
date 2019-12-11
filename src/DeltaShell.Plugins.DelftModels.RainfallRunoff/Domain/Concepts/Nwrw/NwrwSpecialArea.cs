namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public class NwrwSpecialArea
    {
        public virtual int SpecialAreaId { get; set; }
        public virtual int Area { get; set; }
        public virtual string SpecialInflowReference { get; set; }
    }
}
