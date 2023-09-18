using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;

namespace DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition
{
    public static class WaterFlowFMFileBasedItemFactory
    {
        public const string MduFileProperty = "Mdu file";
        public const string HisFileProperty = "Output his file";
        public const string MapFileProperty = "Output map file";

        public static FileBasedModelItem CreateParentNode(WaterFlowFMModel model)
        {
            var parentNode = new FileBasedModelItem(MduFileProperty, model.MduSavePath);

            var extForceFile = model.MduFile.ExternalForcingsFile ?? new ExtForceFile();

            var newFormatBoundaryConditions =
                model.BoundaryConditions.Except(extForceFile.ExistingBoundaryConditions).Any();
            
            foreach (var subFile in model.SubFiles)
            {
                var file = subFile.Value;

                var newNode = parentNode.AddChildItem(subFile.Key.PropertyDefinition.Caption, file);

                if (subFile.Key.Equals(model.ModelDefinition.GetModelProperty(KnownProperties.ExtForceFile)))
                {
                    var writeToDisk = extForceFile.WriteToDisk;

                    extForceFile.WriteToDisk = false;

                    var extForceFileItems =
                        extForceFile.WriteExtForceFileSubFiles(model.ExtFilePath, model.ModelDefinition,
                            !newFormatBoundaryConditions);

                    extForceFile.WriteToDisk = writeToDisk;

                    foreach (var extForceFileItem in extForceFileItems)
                    {
                        newNode.AddChildItem(extForceFileItem.Quantity, extForceFileItem.FileName);
                    }
                    if (!newFormatBoundaryConditions)
                    {
                        var boundaryDataItems = extForceFile.GetFeatureDataFiles(model.ModelDefinition);

                        foreach (var boundaryDataItem in boundaryDataItems)
                        {
                            newNode.AddChildItem(boundaryDataItem[0], boundaryDataItem[1]);
                        }
                    }
                }

                if (subFile.Key.Equals(model.ModelDefinition.GetModelProperty(KnownProperties.BndExtForceFile)))
                {
                    var bndExtForceFile = model.MduFile.BoundaryExternalForcingsFile ?? new BndExtForceFile();
                    
                    if (newFormatBoundaryConditions)
                    {
                        var writeToDisk = bndExtForceFile.WriteToDisk;

                        bndExtForceFile.WriteToDisk = false;

                        var bndExtForceFileItems = bndExtForceFile.WriteBndExtForceFileSubFiles(model.Name, model.BoundaryConditionSets, model.ReferenceTime);

                        var locationFiles =
                            bndExtForceFileItems.Select(
                                item => item.GetPropertyValueWithOptionalDefaultValue(BndExtForceFile.LocationFileKey)).ToList();

                        foreach (var locationFile in locationFiles.Distinct())
                        {
                            newNode.AddChildItem(BndExtForceFile.LocationFileKey, locationFile);
                        }

                        var forcingFiles =
                            bndExtForceFileItems.Select(item => item.GetPropertyValueWithOptionalDefaultValue(BndExtForceFile.ForcingFileKey));

                        foreach (var forcingFile in forcingFiles.Distinct())
                        {
                            newNode.AddChildItem(BndExtForceFile.ForcingFileKey, forcingFile);
                        }

                        bndExtForceFile.WriteToDisk = writeToDisk;
                    }
                }
            }

            parentNode.AddChildItem(HisFileProperty, model.HisSavePath);

            parentNode.AddChildItem(MapFileProperty, model.MapSavePath);

            return parentNode;
        }
    }
}
