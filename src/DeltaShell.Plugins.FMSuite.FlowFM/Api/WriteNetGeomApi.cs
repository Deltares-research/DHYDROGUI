using System;
using System.IO;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Api
{
    public static class WriteNetGeomApi
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WriteNetGeomApi));

        public static void WriteNetGeometryFile(WaterFlowFMModel model, string geomFile)
        {
            UnstructuredGrid grid = model.Grid;
            if (grid.IsEmpty || grid.Cells.Count == 1)
            {
                return;
            }

            var skipDelete = false;
            string tempPath = FileUtils.CreateTempDirectory();
            try
            {
                var tempModel = new WaterFlowFMModel {Name = "temp"};

                tempModel.ModelDefinition.GetModelProperty(KnownProperties.NetFile)
                         .SetValueFromString(Path.GetFileName(model.NetFilePath));

                tempModel.ModelDefinition.GetModelProperty(KnownProperties.ExtForceFile).SetValueFromString("");
                tempModel.ModelDefinition.GetModelProperty(KnownProperties.BndExtForceFile).SetValueFromString("");
                tempModel.ModelDefinition.GetModelProperty(KnownProperties.DryPointsFile).SetValueFromString("");
                tempModel.ModelDefinition.GetModelProperty(KnownProperties.FixedWeirFile).SetValueFromString("");
                tempModel.ModelDefinition.GetModelProperty(KnownProperties.BridgePillarFile).SetValueFromString("");
                tempModel.ModelDefinition.GetModelProperty(KnownProperties.LandBoundaryFile).SetValueFromString("");
                tempModel.ModelDefinition.GetModelProperty(KnownProperties.ThinDamFile).SetValueFromString("");
                tempModel.ModelDefinition.GetModelProperty(KnownProperties.StructuresFile).SetValueFromString("");
                tempModel.ModelDefinition.GetModelProperty(KnownProperties.SolverType).SetValueFromString("2");

                File.Copy(model.NetFilePath, Path.Combine(tempPath, Path.GetFileName(model.NetFilePath)));

                string mduName = tempModel.Name + FileConstants.MduFileExtension;
                string mduFilePath = Path.Combine(tempPath, mduName);

                tempModel.ExportTo(mduFilePath, false, false, false);

                IFlexibleMeshModelApi api = FlexibleMeshModelApiFactory.CreateNew();
                if (api == null)
                {
                    throw new InvalidOperationException("No FlexibleMesh api could be constructed.");
                }

                // call model initialize
                using (api)
                {
                    api.Initialize(mduFilePath);
                    api.WriteNetGeometry(geomFile);
                    api.Finish();
                }
            }
            catch (Exception e)
            {
                skipDelete = true;
                Log.ErrorFormat("Write net geometry failed: " + e.Message);
            }
            finally
            {
                try
                {
                    if (!skipDelete)
                    {
                        FileUtils.DeleteIfExists(tempPath);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}