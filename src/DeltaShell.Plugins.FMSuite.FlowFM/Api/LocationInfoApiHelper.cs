using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Api
{
    public static class LocationInfoApiHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LocationInfoApiHelper));

        public static Dictionary<string, QuantityInfo> ReadQuantities(WaterFlowFMModel.WaterFlowFMModel model, string[] variableNames)
        {
            UnstructuredGrid grid = model.Grid;
            if (grid.IsEmpty || grid.Cells.Count == 1)
            {
                return null;
            }

            var skipDelete = false;
            string tempPath = FileUtils.CreateTempDirectory();
            try
            {
                var tempModel = new WaterFlowFMModel.WaterFlowFMModel {Name = "temp"};

                tempModel.ModelDefinition.GetModelProperty(KnownProperties.NetFile)
                         .SetValueAsString(Path.GetFileName(model.NetFilePath));

                tempModel.ModelDefinition.GetModelProperty(KnownProperties.ExtForceFile).SetValueAsString("");
                tempModel.ModelDefinition.GetModelProperty(KnownProperties.BndExtForceFile).SetValueAsString("");
                tempModel.ModelDefinition.GetModelProperty(KnownProperties.DryPointsFile).SetValueAsString("");
                tempModel.ModelDefinition.GetModelProperty(KnownProperties.FixedWeirFile).SetValueAsString("");
                tempModel.ModelDefinition.GetModelProperty(KnownProperties.BridgePillarFile).SetValueAsString("");
                tempModel.ModelDefinition.GetModelProperty(KnownProperties.LandBoundaryFile).SetValueAsString("");
                tempModel.ModelDefinition.GetModelProperty(KnownProperties.ThinDamFile).SetValueAsString("");
                tempModel.ModelDefinition.GetModelProperty(KnownProperties.StructuresFile).SetValueAsString("");
                tempModel.ModelDefinition.GetModelProperty(KnownProperties.SolverType).SetValueAsString("2");

                File.Copy(model.NetFilePath, Path.Combine(tempPath, Path.GetFileName(model.NetFilePath)));

                string mduName = tempModel.Name + MduFile.MduExtension;
                string mduFilePath = Path.Combine(tempPath, mduName);

                tempModel.ExportTo(mduFilePath, false, false, false);

                var supportedQuantities = new Dictionary<string, QuantityInfo>();

                IFlexibleMeshModelApi api = FlexibleMeshModelApiFactory.CreateNew();
                if (api == null)
                {
                    Log.ErrorFormat("Failed to initialise FlexibleMeshModelApi");
                    return null;
                }

                // call model initialize
                using (api)
                {
                    api.Initialize(mduFilePath);
                    foreach (string variable in variableNames)
                    {
                        string location = api.GetVariableLocation(variable);
                        switch (location)
                            // TODO: use more readable names as keys once available from the fm kernel (DELFT3DFM-431)
                        {
                            case "face":
                                supportedQuantities.Add(variable, new QuantityInfo(variable, ElementType.Cell));
                                break;
                            case "edge":
                                supportedQuantities.Add(variable, new QuantityInfo(variable, ElementType.FlowLink));
                                break;
                            case "node":
                                supportedQuantities.Add(variable, new QuantityInfo(variable, ElementType.Vertex));
                                break;
                        }
                    }

                    api.Finish();
                }

                return supportedQuantities;
            }
            catch (Exception e)
            {
                skipDelete = true;
                Log.ErrorFormat("Reading locations info failed: " + e.Message);
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

            return null;
        }
    }

    public class QuantityInfo
    {
        public QuantityInfo(string bmiName, ElementType elementType)
        {
            BmiName = bmiName;
            ElementType = elementType;
        }

        public readonly string BmiName;
        public readonly ElementType ElementType;
    }

    public enum ElementType
    {
        FlowLink,
        Vertex,
        Cell
    }
}