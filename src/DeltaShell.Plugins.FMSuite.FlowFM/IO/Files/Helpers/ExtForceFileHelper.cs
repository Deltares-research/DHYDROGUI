using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers
{
    public static class ExtForceFileHelper
    {
        public static string GetPliFileName(IFeatureData featureData)
        {
            var featurePart =
                new string(
                    ((Feature2D) featureData.Feature).Name?.Where(c => !Path.GetInvalidFileNameChars().Contains(c))
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

        public static IEnumerable<HarmonicComponent> ToHarmonicComponents(IFunction function)
        {
            var list = new EventedList<HarmonicComponent>();

            bool isAstro = function.Arguments[0].ValueType == typeof(string);

            foreach (object arg in function.Arguments[0].Values)
            {
                var amplitude = (double) function.Components[0][arg];

                int phaseIndex = function.Components.Count == 4 ? 2 : 1;

                var phase = (double) function.Components[phaseIndex][arg];

                list.Add(isAstro
                             ? new HarmonicComponent((string) arg, amplitude, phase)
                             : new HarmonicComponent((double) arg, amplitude, phase));
            }

            return list;
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
    }
}