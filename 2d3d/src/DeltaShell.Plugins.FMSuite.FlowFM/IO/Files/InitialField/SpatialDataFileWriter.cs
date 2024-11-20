using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.IO;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DHYDRO.Common.IO.InitialField;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;
using SharpMap;
using SharpMap.Api.SpatialOperations;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialField
{
    /// <summary>
    /// Writer for the spatial data files, such as sample and polygon files.
    /// </summary>
    public sealed class SpatialDataFileWriter : ISpatialDataFileWriter
    {
        private WaterFlowFMModelDefinition modelDefinition;
        private string targetDirectory;
        private bool switchToNewPath;
        
        /// <summary>
        /// Write the spatial data to file in the specified directory.
        /// </summary>
        /// <param name="directory"> The target write directory. </param>
        /// <param name="switchTo">Whether the spatial operation file path be switched to the new file location.</param>
        /// <param name="data"> The initial field file data. </param>
        /// <param name="definition"> The model definition containing the spatial data. </param>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="directory"/> is <c>null</c> or empty.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="data"/> or <paramref name="definition"/> is <c>null</c>.
        /// </exception>
        public void Write(string directory, bool switchTo, InitialFieldFileData data, WaterFlowFMModelDefinition definition)
        {
            Ensure.NotNullOrWhiteSpace(directory, nameof(directory));
            Ensure.NotNull(data, nameof(data));
            Ensure.NotNull(definition, nameof(definition));

            modelDefinition = definition;
            targetDirectory = directory;
            switchToNewPath = switchTo;

            foreach (InitialFieldData initialField in data.InitialConditions)
            {
                WriteSpatialData(initialField);
            }

            foreach (InitialFieldData initialField in data.Parameters)
            {
                WriteSpatialData(initialField);
            }
        }

        private void WriteSpatialData(InitialFieldData initialFieldData)
        {
            IList<ISpatialOperation> spatialOperations = modelDefinition.GetSpatialOperations(initialFieldData.SpatialOperationQuantity);
            ISpatialOperation spatialOperation = spatialOperations.Single(o => o.Name == initialFieldData.SpatialOperationName);
            
            string spatialDataFilePath = Path.Combine(targetDirectory, initialFieldData.DataFile);
            FileUtils.CreateDirectoryIfNotExists(Path.GetDirectoryName(spatialDataFilePath));

            WriteSpatialData(spatialDataFilePath, spatialOperation);
        }

        private void WriteSpatialData(string filePath, ISpatialOperation spatialOperation)
        {
            var importSamplesOperation = spatialOperation as ImportSamplesSpatialOperation;
            if (importSamplesOperation != null)
            {
                importSamplesOperation.CopyTo(filePath, switchToNewPath);
            }

            var polygonOperation = spatialOperation as SetValueOperation;
            if (polygonOperation != null)
            {
                new PolFile<Feature2DPolygon>().Write(filePath, polygonOperation.Mask.Provider.Features.OfType<IFeature>());
            }

            var addSamplesOperation = spatialOperation as AddSamplesOperation;
            if (addSamplesOperation != null)
            {
                XyzFile.Write(filePath, addSamplesOperation.GetPoints());
            }
        }
    }
}