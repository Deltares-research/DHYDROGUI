using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public interface IStructureConverter
    {
        IStructure1D ConvertToStructure1D(IDelftIniCategory category, IList<IChannel> channelsList);
    }
    public abstract class StructureConverter : IStructureConverter
    {
        public virtual IStructure1D ConvertToStructure1D(IDelftIniCategory category, IList<IChannel> channelsList)
        {
            return null;
        }
    }
}