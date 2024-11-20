using System;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO.SubFileImporterComponents
{
    /// <summary>
    /// Class which holds the regex information to extract a parameter from a string.
    /// </summary>
    public class SubFilePropertyRegexInfo
    {
        /// <summary>
        /// Creates a new instance of <see cref="SubFilePropertyRegexInfo"/>.
        /// </summary>
        /// <param name="propertyName"> The name of the property. </param>
        /// <param name="captureGroupName"> The name of the Regex capture group. </param>
        /// <param name="captureGroupPattern"> The legit characters in the capture group. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="captureGroupPattern"/>
        /// is <c> null </c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="propertyName"/> or
        /// <paramref name="captureGroupName"/> is <c> null </c>, empty or consists of only whitespace.
        /// </exception>
        public SubFilePropertyRegexInfo(string propertyName, string captureGroupName, string captureGroupPattern)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentException("'propertyName' cannot be null, empty or consist of only whitespace.");
            }

            if (string.IsNullOrWhiteSpace(captureGroupName))
            {
                throw new ArgumentException("'captureGroupName' cannot be null, empty or consist of only whitespace.");
            }

            if (captureGroupPattern == null)
            {
                throw new ArgumentNullException(nameof(captureGroupPattern));
            }

            PropertyName = propertyName;
            CaptureGroupName = captureGroupName;
            CaptureGroupPattern = captureGroupPattern;
        }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Gets the name of the capture group.
        /// </summary>
        public string CaptureGroupName { get; }

        /// <summary>
        /// Gets the pattern that is part of the capture group.
        /// </summary>
        public string CaptureGroupPattern { get; }
    }
}