using System;
using System.Collections.Generic;
using Deltares.Infrastructure.API.Guards;

namespace DHYDRO.Common.IO.InitialField
{
    /// <summary>
    /// Represents contextual information for reading/writing initial field files (*.ini).
    /// </summary>
    public sealed class InitialFieldFileContext
    {
        private readonly Dictionary<string, string> originalDataFiles = new Dictionary<string, string>();

        /// <summary>
        /// Gets the collection of the stored initial field data filenames.
        /// </summary>
        public IEnumerable<string> DataFileNames => originalDataFiles.Values;

        /// <summary>
        /// Clears the stored initial field data filenames.
        /// </summary>
        public void ClearDataFileNames()
        {
            originalDataFiles.Clear();
        }

        /// <summary>
        /// Stores the initial field data filename.
        /// </summary>
        /// <param name="initialFieldData">The initial field data to store.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="initialFieldData"/> is <c>null</c>.</exception>
        public void StoreDataFileName(InitialFieldData initialFieldData)
        {
            Ensure.NotNull(initialFieldData, nameof(initialFieldData));

            if (!IsSpatialDataField(initialFieldData))
            {
                return;
            }

            string spatialOperationKey = GetSpatialOperationKey(initialFieldData);

            originalDataFiles[spatialOperationKey] = initialFieldData.DataFile;
        }

        /// <summary>
        /// Restores the initial field data filename.
        /// </summary>
        /// <param name="initialFieldData">The initial field data to restore.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="initialFieldData"/> is <c>null</c>.</exception>
        public void RestoreDataFileName(InitialFieldData initialFieldData)
        {
            Ensure.NotNull(initialFieldData, nameof(initialFieldData));

            if (!IsSpatialDataField(initialFieldData))
            {
                return;
            }

            string spatialOperationKey = GetSpatialOperationKey(initialFieldData);

            if (originalDataFiles.TryGetValue(spatialOperationKey, out string dataFile))
            {
                initialFieldData.DataFile = dataFile;
            }
        }

        private static bool IsSpatialDataField(InitialFieldData initialFieldData)
        {
            return !string.IsNullOrEmpty(initialFieldData.SpatialOperationName) &&
                   !string.IsNullOrEmpty(initialFieldData.SpatialOperationQuantity);
        }

        private static string GetSpatialOperationKey(InitialFieldData initialFieldData)
        {
            return $"{initialFieldData.SpatialOperationQuantity} - {initialFieldData.SpatialOperationName}";
        }
    }
}