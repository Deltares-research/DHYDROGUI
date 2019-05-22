using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccess;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;

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

            ExtForceFile extForceFile = model.MduFile.ExternalForcingsFile ?? new ExtForceFile();

            bool newFormatBoundaryConditions =
                model.BoundaryConditions.Except(extForceFile.ExistingBoundaryConditions).Any();

            foreach (KeyValuePair<WaterFlowFMProperty, string> subFile in model.SubFiles)
            {
                string file = subFile.Value;

                FileBasedModelItem newNode = parentNode.AddChildItem(subFile.Key.PropertyDefinition.Caption, file);

                if (subFile.Key.Equals(model.ModelDefinition.GetModelProperty(KnownProperties.ExtForceFile)))
                {
                    bool writeToDisk = extForceFile.WriteToDisk;

                    extForceFile.WriteToDisk = false;

                    IEnumerable<ExtForceFileItem> extForceFileItems =
                        extForceFile.WriteExtForceFileSubFiles(model.ExtFilePath, model.ModelDefinition, false,
                                                               !newFormatBoundaryConditions);

                    extForceFile.WriteToDisk = writeToDisk;

                    foreach (ExtForceFileItem extForceFileItem in extForceFileItems)
                    {
                        newNode.AddChildItem(extForceFileItem.Quantity, extForceFileItem.FileName);
                    }

                    if (!newFormatBoundaryConditions)
                    {
                        IEnumerable<string[]> boundaryDataItems =
                            extForceFile.GetFeatureDataFiles(model.ModelDefinition);

                        foreach (string[] boundaryDataItem in boundaryDataItems)
                        {
                            newNode.AddChildItem(boundaryDataItem[0], boundaryDataItem[1]);
                        }
                    }
                }

                if (subFile.Key.Equals(model.ModelDefinition.GetModelProperty(KnownProperties.BndExtForceFile)))
                {
                    BndExtForceFile bndExtForceFile =
                        model.MduFile.BoundaryExternalForcingsFile ?? new BndExtForceFile();

                    if (newFormatBoundaryConditions)
                    {
                        bool writeToDisk = bndExtForceFile.WriteToDisk;

                        bndExtForceFile.WriteToDisk = false;

                        IList<DelftIniCategory> bndExtForceFileItems =
                            bndExtForceFile.WriteBndExtForceFileSubFiles(
                                model.Name, model.BoundaryConditionSets, model.ReferenceTime);

                        List<string> locationFiles =
                            bndExtForceFileItems.Select(
                                                    item => item.GetPropertyValue(BndExtForceFile.LocationFileKey))
                                                .ToList();

                        foreach (string locationFile in locationFiles.Distinct())
                        {
                            newNode.AddChildItem(BndExtForceFile.LocationFileKey, locationFile);
                        }

                        IEnumerable<string> forcingFiles =
                            bndExtForceFileItems.Select(item => item.GetPropertyValue(BndExtForceFile.ForcingFileKey));

                        foreach (string forcingFile in forcingFiles.Distinct())
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