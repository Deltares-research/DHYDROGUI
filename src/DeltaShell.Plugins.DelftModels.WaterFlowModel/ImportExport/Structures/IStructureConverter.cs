using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public interface IStructureConverter
    {
        IStructure1D ConvertToStructure1D(IDelftIniCategory category, IBranch branch, IList<string> warningMessages);
    }
}