using DelftTools.Utils.Data;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public interface IUrbanRrDefinition : IUnique<long>
    {
        string Name { get; set; }
        string Remark { get; set; }
    }
}