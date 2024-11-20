using System;
using System.Collections;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    /// <summary>
    /// Represents the data read from attribute files.
    /// </summary>
    public class AttributesFileData
    {
        private readonly BitArray segmentEnabledData;

        public AttributesFileData(int nrOfSegmentsPerLayer, int nrOfLayers)
        {
            if (nrOfSegmentsPerLayer <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(nrOfSegmentsPerLayer),
                                                      "Cannot create attribute file data without segments.");
            }

            if (nrOfLayers <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(nrOfLayers),
                                                      "Cannot create attribute file data without layers.");
            }

            IndexCount = nrOfSegmentsPerLayer * nrOfLayers;
            segmentEnabledData = new BitArray(IndexCount);
        }

        /// <summary>
        /// The number of segment indexes read.
        /// </summary>
        public int IndexCount { get; private set; }

        /// <summary>
        /// Determines whether a segment of a specific index is active or not.
        /// </summary>
        /// <param name="segmentIndex"> Index of the segment. </param>
        public bool IsSegmentActive(int segmentIndex)
        {
            int arrayIndex = segmentIndex - 1;
            if (arrayIndex >= segmentEnabledData.Count)
            {
                string message = string.Format("Segment index is out of range (count = {0}).",
                                               segmentEnabledData.Count);
                throw new ArgumentOutOfRangeException(nameof(segmentIndex), segmentIndex, message);
            }

            return segmentEnabledData[arrayIndex];
        }

        /// <summary>
        /// Sets the enabledness state for a particular segment with the given index.
        /// </summary>
        /// <param name="segmentIndex"> Index of the segment. </param>
        /// <param name="boolean"> True if the segment is enabled, false if not. </param>
        public void SetSegmentActive(int segmentIndex, bool boolean)
        {
            int arrayIndex = segmentIndex - 1;
            if (arrayIndex >= segmentEnabledData.Count)
            {
                string message = string.Format("Segment index is out of range (count = {0}).",
                                               segmentEnabledData.Count);
                throw new ArgumentOutOfRangeException(nameof(segmentIndex), segmentIndex, message);
            }

            segmentEnabledData[arrayIndex] = boolean;
        }
    }
}