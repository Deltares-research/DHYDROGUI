using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO.SubFileImporterComponents
{
    /// <summary>
    /// Helper class which contains helper methods to import data from a sub file.
    /// </summary>
    public static class SubFileHelper
    {
        /// <summary>
        /// Gets a regex pattern based on its input arguments.
        /// </summary>
        /// <param name="propertyRegexInfos">
        /// The collection of <see cref="SubFilePropertyRegexInfo"/>
        /// to create the regex for.
        /// </param>
        /// <param name="propertySeparator"> The separator between the different properties. </param>
        /// <returns> A regex pattern. </returns>
        /// <exception cref="ArgumentNullException"> Thrown when any parameter is <c> null </c>. </exception>
        public static string GetRegexPattern(IEnumerable<SubFilePropertyRegexInfo> propertyRegexInfos, string propertySeparator)
        {
            if (propertyRegexInfos == null)
            {
                throw new ArgumentNullException(nameof(propertyRegexInfos));
            }

            if (propertySeparator == null)
            {
                throw new ArgumentNullException(nameof(propertySeparator));
            }

            var regexBuilder = new StringBuilder();
            foreach (SubFilePropertyRegexInfo regexInfo in propertyRegexInfos)
            {
                regexBuilder.Append($@"\s*{regexInfo.PropertyName}\s*'(?<{regexInfo.CaptureGroupName}>{regexInfo.CaptureGroupPattern})'{propertySeparator}");
            }

            return regexBuilder.ToString();
        }
    }
}