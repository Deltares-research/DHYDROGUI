using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;
using SharpMap;
using SharpMap.Api.SpatialOperations;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Serialization
{
    /// <summary>
    /// Writer for the spatial data files, such as sample and polygon files.
    /// </summary>
    public sealed class SpatialDataFileWriter : ISpatialDataFileWriter
    {
        /// <summary>
        /// Write the spatial data to file in the specified directory.
        /// </summary>
        /// <param name="targetDirectory"> The target write directory. </param>
        /// <param name="initialFieldFileData"> The initial field file data. </param>
        /// <param name="modelDefinition"> The model definition containing the spatial data. </param>
        public void Write(string targetDirectory, InitialFieldFileData initialFieldFileData, WaterFlowFMModelDefinition modelDefinition)
        {
            foreach (InitialField initialField in initialFieldFileData.InitialConditions)
            {
                WriteSpatialData(targetDirectory, modelDefinition, initialField);
            }

            foreach (InitialField initialField in initialFieldFileData.Parameters)
            {
                WriteSpatialData(targetDirectory, modelDefinition, initialField);
            }
        }

        private void WriteSpatialData(string targetDirectory, WaterFlowFMModelDefinition modelDefinition, InitialField initialField)
        {
            IList<ISpatialOperation> spatialOperations = modelDefinition.GetSpatialOperations(initialField.SpatialOperationQuantity);
            ISpatialOperation spatialOperation = spatialOperations.Single(o => o.Name == initialField.SpatialOperationName);
            string spatialDataFilePath = Path.Combine(targetDirectory, initialField.DataFile);

            WriteSpatialData(spatialDataFilePath, spatialOperation);
        }

        private void WriteSpatialData(string filePath, ISpatialOperation spatialOperation)
        {
            var importSamplesOperation = spatialOperation as ImportSamplesSpatialOperation;
            if (importSamplesOperation != null)
            {
                string targetDirectory = Path.GetDirectoryName(filePath);
                importSamplesOperation.CopyTo(targetDirectory);
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