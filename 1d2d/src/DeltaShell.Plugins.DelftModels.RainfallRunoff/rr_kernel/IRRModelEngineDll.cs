using System.Text;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.rr_kernel
{
    public interface IRRModelEngineDll
    {
        int ModelInitialize(string componentId, string schemId);

        int ModelFinalize(string componentId, string schemId);

        int ModelPerformTimeStep(string componentId, string schemId);

        int GetValuesByIntId(string componentId, string schemId, ref int valueId, ref int elementsetId, ref int ivalues, double[] values);
        
        int GetSize(string componentId, string schemId, string elementSetName);
        
        int SetValues(string componentID, string schemID, string quantityID, string elementsetID, int elementCount, double[] values);

        int GetError(ref int errorId, StringBuilder errorDescription);
    }
}