using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.PhysicalParameters;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    public static class WaterFlowModel1DBoundaryFileWriter
    {
        public static void WriteFile(string targetFile, WaterFlowModel1D waterFlowModel1D)
        {
            new Model1DBoundaryFileWriter().WriteFile(targetFile,waterFlowModel1D.StartTime, waterFlowModel1D.BoundaryConditions, waterFlowModel1D.LateralSourceData,waterFlowModel1D.UseSalt, waterFlowModel1D.UseTemperature, waterFlowModel1D.Wind, waterFlowModel1D.MeteoData);
        }
    }
}
