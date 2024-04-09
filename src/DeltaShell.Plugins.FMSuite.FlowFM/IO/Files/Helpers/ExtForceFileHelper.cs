using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers
{
    // YAGNI (GvdO) merge into extforce file, or strip extforce file from all path/file logic, but now it is not clear who is doing what
    public static class ExtForceFileHelper
    {
        private static readonly List<string> previousPaths = new List<string>();

        public static string GetPliFileName(IFeatureData featureData)
        {
            var featurePart =
                new string(
                    ((Feature2D)featureData.Feature).Name?.Where(c => !Path.GetInvalidFileNameChars().Contains(c))
                                                     .ToArray());
            if (string.IsNullOrEmpty(featurePart))
            {
                return null;
            }

            string quantityPart = ExtForceQuantNames.GetPliQuantitySuffix(featureData);
            string filename = featurePart + quantityPart;
            while (File.Exists(filename))
            {
                filename += "_corr";
            }

            return filename + "." + PliFile<Feature2D>.Extension;
        }

        /// <summary>
        /// Get the data files that are references in the extForceFile.
        /// </summary>
        /// <param name="modelDefinition">The model definition.</param>
        /// <param name="polyLineForceFileItems">The existing items.</param>
        /// <returns>A collection containing collections of names.</returns>
        public static IEnumerable<string[]> GetFeatureDataFiles(WaterFlowFMModelDefinition modelDefinition,
                                                                IDictionary<IFeatureData, ExtForceFileItem> polyLineForceFileItems)
        {
            StartWritingSubFiles();

            foreach (BoundaryConditionSet boundaryConditionSet in
                modelDefinition.BoundaryConditionSets.Where(bc => bc.Feature.Name != null))
            {
                foreach (FlowBoundaryCondition bc in boundaryConditionSet
                                                     .BoundaryConditions.OfType<FlowBoundaryCondition>())
                {
                    polyLineForceFileItems.TryGetValue(bc, out ExtForceFileItem matchingItem);
                    string[][] dataFiles = GetBoundaryDataFiles(bc, boundaryConditionSet, matchingItem).ToArray();

                    foreach (string[] dataFile in dataFiles)
                    {
                        yield return dataFile;
                    }
                }
            }

            foreach (SourceAndSink sourceAndSink in modelDefinition.SourcesAndSinks)
            {
                polyLineForceFileItems.TryGetValue(sourceAndSink, out ExtForceFileItem matchingItem);
                string[][] dataFiles = GetSourceAndSinkDataFiles(sourceAndSink, matchingItem).ToArray();

                foreach (string[] dataFile in dataFiles)
                {
                    yield return dataFile;
                }
            }
        }

        public static void StartWritingSubFiles()
        {
            previousPaths.Clear();
        }

        // works (only) in conjuction with StartWritingSubFiles
        public static void AddSuffixInCaseOfDuplicateFile(ExtForceFileItem item)
        {
            if (previousPaths.Contains(item.FileName))
            {
                string fileName = item.FileName;
                string extension = Path.GetExtension(item.FileName);
                string fileNameWithoutExtension = fileName.Replace(extension, string.Empty);

                // search for unique filename using recognizable suffix
                var i = 2;
                while (true)
                {
                    var newFileName = $"{fileNameWithoutExtension}__{i:00000}{extension}";
                    if (!previousPaths.Contains(newFileName))
                    {
                        item.FileName = newFileName;
                        break;
                    }

                    i++;
                }
            }

            previousPaths.Add(item.FileName);
        }

        public static IEnumerable<HarmonicComponent> ToHarmonicComponents(IFunction function)
        {
            var list = new EventedList<HarmonicComponent>();

            bool isAstro = function.Arguments[0].ValueType == typeof(string);

            foreach (object arg in function.Arguments[0].Values)
            {
                var amplitude = (double)function.Components[0][arg];

                int phaseIndex = function.Components.Count == 4 ? 2 : 1;

                var phase = (double)function.Components[phaseIndex][arg];

                list.Add(isAstro
                             ? new HarmonicComponent((string)arg, amplitude, phase)
                             : new HarmonicComponent((double)arg, amplitude, phase));
            }

            return list;
        }

        public static IWindField CreateWindField(ExtForceFileItem extForceFileItem, string filePath)
        {
            if (!ExtForceQuantNames.WindQuantityNames.Values.Contains(extForceFileItem.Quantity))
            {
                throw new NotSupportedException($"Wind quantity {extForceFileItem.Quantity} is not supported");
            }

            WindQuantity quantity = ExtForceQuantNames
                                    .WindQuantityNames.First(kvp => kvp.Value == extForceFileItem.Quantity).Key;

            switch (extForceFileItem.FileType)
            {
                case ExtForceQuantNames.FileTypes.Uniform:
                    if (quantity == WindQuantity.VelocityX)
                    {
                        return UniformWindField.CreateWindXSeries();
                    }

                    if (quantity == WindQuantity.VelocityY)
                    {
                        return UniformWindField.CreateWindYSeries();
                    }

                    if (quantity == WindQuantity.VelocityVector)
                    {
                        return UniformWindField.CreateWindXYSeries();
                    }

                    if (quantity == WindQuantity.AirPressure)
                    {
                        return UniformWindField.CreatePressureSeries();
                    }

                    break;
                case ExtForceQuantNames.FileTypes.UniMagDir:
                    if (quantity == WindQuantity.VelocityVector)
                    {
                        return UniformWindField.CreateWindPolarSeries();
                    }

                    break;
                case ExtForceQuantNames.FileTypes.ArcInfo:
                    if (quantity == WindQuantity.VelocityX)
                    {
                        return GriddedWindField.CreateXField(filePath);
                    }

                    if (quantity == WindQuantity.VelocityY)
                    {
                        return GriddedWindField.CreateYField(filePath);
                    }

                    if (quantity == WindQuantity.AirPressure)
                    {
                        return GriddedWindField.CreatePressureField(filePath);
                    }

                    break;
                case ExtForceQuantNames.FileTypes.SpiderWeb:
                    if (quantity == WindQuantity.VelocityVectorAirPressure)
                    {
                        return SpiderWebWindField.Create(filePath);
                    }

                    break;
                case ExtForceQuantNames.FileTypes.Curvi:
                    if (quantity == WindQuantity.VelocityVectorAirPressure)
                    {
                        return GriddedWindField.CreateCurviField(filePath,
                                                                 GriddedWindField
                                                                     .GetCorrespondingGridFilePath(filePath));
                    }

                    break;
                case ExtForceQuantNames.FileTypes.NCgrid:
                    if (quantity == WindQuantity.VelocityX)
                    {
                        return GriddedWindField.CreateXField(filePath);
                    }

                    if (quantity == WindQuantity.VelocityY)
                    {
                        return GriddedWindField.CreateYField(filePath);
                    }

                    if (quantity == WindQuantity.AirPressure)
                    {
                        return GriddedWindField.CreatePressureField(filePath);
                    }

                    break;
            }

            throw new NotSupportedException(
                string.Format("External forcing for wind quantity {0}, method {1} and file type {2} is not supported",
                              extForceFileItem.Quantity, extForceFileItem.Method, extForceFileItem.FileType));
        }

        public static string GetNumberedFilePath(string pliFilePath, string fileExtension, int i)
        {
            string directoryName = Path.GetDirectoryName(pliFilePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pliFilePath);
            if (fileNameWithoutExtension == null)
            {
                throw new FormatException("Invalid file path " + pliFilePath);
            }

            string filePathWithoutExtension = directoryName != null
                                                  ? Path.Combine(directoryName, fileNameWithoutExtension)
                                                  : fileNameWithoutExtension;
            return i == 0
                       ? string.Join(".", filePathWithoutExtension, fileExtension)
                       : $"{filePathWithoutExtension}_{i:0000}.{fileExtension}";
        }

        private static IEnumerable<string[]> GetBoundaryDataFiles(FlowBoundaryCondition boundaryCondition,
                                                                  BoundaryConditionSet boundaryConditionSet,
                                                                  ExtForceFileItem existingExtForceFileItem)
        {
            string quantityName =
                ExtForceQuantNames.GetQuantityString(boundaryCondition);

            ExtForceFileItem extForceFileItem = existingExtForceFileItem ?? new ExtForceFileItem(quantityName)
            {
                FileName = GetPliFileName(boundaryCondition),
                FileType = ExtForceQuantNames.FileTypes.PolyTim
            };

            AddSuffixInCaseOfDuplicateFile(extForceFileItem);

            foreach (int i in boundaryCondition.DataPointIndices)
            {
                var quantity =
                    $"{boundaryCondition.VariableDescription.ToLower()} {boundaryCondition.DataType.GetDescription().ToLower()} at {boundaryConditionSet.SupportPointNames[i]}";

                string filePath;

                if (boundaryCondition.DataType == BoundaryConditionDataType.TimeSeries &&
                    !boundaryCondition.IsVerticallyUniform)
                {
                    filePath = GetNumberedFilePath(extForceFileItem.FileName, ExtForceQuantNames.T3DFileExtension,
                                                   i + 1);
                }
                else
                {
                    filePath = GetNumberedFilePath(extForceFileItem.FileName,
                                                   ExtForceQuantNames.ForcingToFileExtensionMapping[
                                                       boundaryCondition.DataType], i + 1);
                }

                if (filePath == null)
                {
                    yield break;
                }

                yield return new[]
                {
                    quantity,
                    filePath
                };
            }
        }

        private static IEnumerable<string[]> GetSourceAndSinkDataFiles(SourceAndSink sourceAndSink,
                                                                       ExtForceFileItem existingExtForceFileItem)
        {
            const string quantityName = ExtForceQuantNames.SourceAndSink;

            ExtForceFileItem extForceFileItem = existingExtForceFileItem ?? new ExtForceFileItem(quantityName)
            {
                FileName = GetPliFileName(sourceAndSink),
                FileType = ExtForceQuantNames.FileTypes.PolyTim
            };

            AddSuffixInCaseOfDuplicateFile(extForceFileItem);

            string filePath = Path.ChangeExtension(extForceFileItem.FileName, ExtForceQuantNames.TimFileExtension);

            yield return new[]
            {
                "Source/Sink",
                filePath
            };
        }
    }
}