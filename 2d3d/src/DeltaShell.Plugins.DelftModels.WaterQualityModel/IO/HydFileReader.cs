using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO.HydFileElement;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    /// <summary>
    /// Reader for hydrodynamic (extension: .hyd) files.
    /// </summary>
    public static class HydFileReader
    {
        /// <summary>
        /// Reads the hydrodynamics file.
        /// </summary>
        /// <returns> All parsed data in the file. </returns>
        /// <exception cref="System.InvalidOperationException">
        /// When the hydro dynamics file
        /// to be read is missing.
        /// </exception>
        /// <exception cref="FormatException"> When a parsing error occurs. </exception>
        public static HydFileData ReadAll(FileInfo hydFile)
        {
            if (hydFile == null || !hydFile.Exists)
            {
                string fullName = hydFile == null ? "" : hydFile.FullName;
                throw new InvalidOperationException(string.Format("Cannot find hydrodynamics file ({0}).", fullName));
            }

            var hydFileData = new HydFileData
            {
                Path = hydFile,
                Checksum = FileUtils.GetChecksum(hydFile.FullName)
            };

            Dictionary<string, IHydFileElement> elementsOfInterest = GetAllHydFileElementsOfInterest();
            using (StreamReader streamReader = hydFile.OpenText())
            {
                IHydFileElement fileElement;
                while ((fileElement = ReadNextElement(streamReader, elementsOfInterest)) != null)
                {
                    fileElement.SetDataTo(hydFileData);
                }
            }

            ReadBoundaries(hydFileData);

            ReadGrid(hydFileData);

            return hydFileData;
        }

        private static void ReadGrid(HydFileData hydFileData)
        {
            if (string.IsNullOrEmpty(hydFileData.GridRelativePath))
            {
                throw new FormatException(
                    "Error parsing value for Hyd file element: waqgeom-file, value not specified");
            }

            string schematizationFilePath =
                Path.Combine(hydFileData.Path.DirectoryName, hydFileData.GridRelativePath);

            hydFileData.Grid = UnstructuredGridFileHelper.LoadFromFile(schematizationFilePath, true);
        }

        private static void ReadBoundaries(HydFileData hydFileData)
        {
            if (string.IsNullOrEmpty(hydFileData.BoundariesRelativePath))
            {
                throw new FormatException(
                    "Error parsing value for Hyd file element: boundaries-file, value not specified");
            }

            var boundariesFile =
                new FileInfo(Path.Combine(hydFileData.Path.DirectoryName, hydFileData.BoundariesRelativePath));

            hydFileData.BoundaryNodeIds = BoundaryFileReader.ReadAll(boundariesFile);
            hydFileData.Boundaries = new EventedList<WaterQualityBoundary>(hydFileData.BoundaryNodeIds.Keys);
        }

        private static Dictionary<string, IHydFileElement> GetAllHydFileElementsOfInterest()
        {
            return new Dictionary<string, IHydFileElement>
            {
                {"geometry", new SchematizationElement()},
                {"z-layers-ztop", new KeyValueElement<double>((hydFileData, value) => hydFileData.ZTop = value)},
                {"z-layers-zbot", new KeyValueElement<double>((hydFileData, value) => hydFileData.ZBot = value)},
                {"conversion-ref-time", new KeyValueElement<DateTime>((hydFileData, value) => hydFileData.ConversionReferenceTime = value)},
                {"conversion-start-time", new KeyValueElement<DateTime>((hydFileData, value) => hydFileData.ConversionStartTime = value)},
                {"conversion-stop-time", new KeyValueElement<DateTime>((hydFileData, value) => hydFileData.ConversionStopTime = value)},
                {"conversion-timestep", new KeyValueElement<TimeSpan>((hydFileData, value) => hydFileData.ConversionTimeStep = value)},
                {
                    "number-water-quality-segments-per-layer", new KeyValueElement<int>((hydFileData, value) =>
                                                                                            hydFileData.NumberOfDelwaqSegmentsPerHydrodynamicLayer = value)
                },
                {"number-horizontal-exchanges", new KeyValueElement<int>((hydFileData, value) => hydFileData.NumberOfHorizontalExchanges = value)},
                {"number-vertical-exchanges", new KeyValueElement<int>((hydFileData, value) => hydFileData.NumberOfVerticalExchanges = value)},
                {"number-hydrodynamic-layers", new KeyValueElement<int>((hydFileData, value) => hydFileData.NumberOfHydrodynamicLayers = value)},
                {
                    "hydrodynamic-layers", new KeyValueElement<double[]>((hydFileData, value) =>
                                                                             hydFileData.HydrodynamicLayerThicknesses = value)
                },
                {"number-water-quality-layers", new KeyValueElement<int>((hydFileData, value) => hydFileData.NumberOfWaqSegmentLayers = value)},
                {
                    "water-quality-layers", new KeyValueElement<int[]>((hydFileData, value) =>
                                                                           hydFileData.NumberOfHydrodynamicLayersPerWaqSegmentLayer = value)
                },
                {"boundaries-file", new KeyValueElement<string>((hydFileData, value) => hydFileData.BoundariesRelativePath = value)},
                {"waqgeom-file", new KeyValueElement<string>((hydFileData, value) => hydFileData.GridRelativePath = value)},
                {"volumes-file", new KeyValueElement<string>((hydFileData, value) => hydFileData.VolumesRelativePath = value)},
                {"areas-file", new KeyValueElement<string>((hydFileData, value) => hydFileData.AreasRelativePath = value)},
                {"flows-file", new KeyValueElement<string>((hydFileData, value) => hydFileData.FlowsRelativePath = value)},
                {"pointers-file", new KeyValueElement<string>((hydFileData, value) => hydFileData.PointersRelativePath = value)},
                {"lengths-file", new KeyValueElement<string>((hydFileData, value) => hydFileData.LengthsRelativePath = value)},
                {"salinity-file", new KeyValueElement<string>((hydFileData, value) => hydFileData.SalinityRelativePath = value)},
                {"temperature-file", new KeyValueElement<string>((hydFileData, value) => hydFileData.TemperatureRelativePath = value)},
                {
                    "vert-diffusion-file", new KeyValueElement<string>((hydFileData, value) =>
                                                                           hydFileData.VerticalDiffusionRelativePath = value)
                },
                {"horizontal-surfaces-file", new KeyValueElement<string>((hydFileData, value) => hydFileData.SurfacesRelativePath = value)},
                {"shear-stresses-file", new KeyValueElement<string>((hydFileData, value) => hydFileData.ShearStressesRelativePath = value)},
                {"attributes-file", new KeyValueElement<string>((hydFileData, value) => hydFileData.AttributesRelativePath = value)},
                {"velocities-file", new KeyValueElement<string>((hydFileData, value) => hydFileData.VelocitiesRelativePath = value)},
                {"widths-file", new KeyValueElement<string>((hydFileData, value) => hydFileData.WidthsRelativePath = value)},
                {
                    "chezy-coefficients-file", new KeyValueElement<string>((hydFileData, value) =>
                                                                               hydFileData.ChezyCoefficientsRelativePath = value)
                }
            };
        }

        /// <summary>
        /// Reads the next key-value element in the file.
        /// </summary>
        /// <param name="streamReader"> The stream reader. </param>
        /// <param name="elementsOfInterest"> </param>
        /// <returns> The next key-value element, or null when at the end of the file. </returns>
        /// <exception cref="FormatException"> When a parsing error occurs. </exception>
        private static IHydFileElement ReadNextElement(TextReader streamReader,
                                                       Dictionary<string, IHydFileElement> elementsOfInterest)
        {
            KeyValuePair<string, string>? keyValuePair;
            while ((keyValuePair = ReadKeyValuePair(streamReader)) != null)
            {
                if (elementsOfInterest.ContainsKey(keyValuePair.Value.Key))
                {
                    try
                    {
                        return elementsOfInterest[keyValuePair.Value.Key].ParseValue(keyValuePair.Value.Value);
                    }
                    catch (FormatException ex)
                    {
                        throw new FormatException(
                            string.Format("Error parsing value for Hyd file element: {0}", keyValuePair.Value.Key), ex);
                    }
                }
            }

            return null;
        }

        private static KeyValuePair<string, string>? ReadKeyValuePair(TextReader streamReader)
        {
            string line;
            while ((line = streamReader.ReadLine()) != null)
            {
                string[] lineElements = GetWhiteSpaceSeparatedStrings(line);
                if (lineElements.Length == 0)
                {
                    // Empty line, read next
                    continue;
                }

                if (lineElements.Length == 1)
                {
                    return ReadCollectionKeyValuePair(streamReader, lineElements[0]);
                }

                // Normal key-value pair:
                return ReadKeyValuePair(lineElements);
            }

            return null;
        }

        private static string[] GetWhiteSpaceSeparatedStrings(string line)
        {
            return Regex.Split(line.Trim(), "\\s+").Where(element => element.Length > 0).ToArray();
        }

        private static KeyValuePair<string, string> ReadCollectionKeyValuePair(TextReader streamReader, string keyName)
        {
            string line;
            string collectionEndKey = "end-" + keyName;
            var endOfCollectionReached = false;

            // Read all lines between start and end keywords:
            var list = new List<string>();
            while ((line = streamReader.ReadLine()) != null)
            {
                string lineElement = line.Trim();
                if (Equals(lineElement, collectionEndKey))
                {
                    endOfCollectionReached = true;
                    break;
                }

                list.Add(lineElement);
            }

            if (!endOfCollectionReached)
            {
                throw new FormatException(string.Format(
                                              "Error parsing value for Hyd file element: {0}, no 'end-' key found for this collection",
                                              keyName));
            }

            return new KeyValuePair<string, string>(keyName, string.Join(" ", list));
        }

        private static KeyValuePair<string, string> ReadKeyValuePair(string[] lineElements)
        {
            return new KeyValuePair<string, string>(lineElements[0], string.Join(" ", lineElements.Skip(1)));
        }
    }
}