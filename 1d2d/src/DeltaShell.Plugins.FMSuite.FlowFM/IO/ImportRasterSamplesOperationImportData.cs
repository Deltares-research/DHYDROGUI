using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Extensions.Coverages;
using SharpMap;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class ImportRasterSamplesOperationImportData : ImportSamplesOperationImportData
    {
        public override IEnumerable<IPointValue> GetPoints()
        {
            if (!Dirty && Output.Provider != null)
            {
                return Output.Provider.Features.OfType<IPointValue>();
            }
            if (!File.Exists(FilePath))
            {
                throw new ArgumentException("Cannot find file " + FilePath);
            }
            string extension = Path.GetExtension(FilePath);
            if (extension != RasterFile.AscExtension && extension != RasterFile.TiffExtension)
            {
                log.WarnFormat("File {0} does not have the right extension (only .asc/.tif imports are supported for now)", FilePath);
            }

            var samples = RasterFile.ReadPointValues(FilePath,checkForUnsupportedSize:true);
            if (samples == null || !samples.Any()) return null;

            if (SourceCoordinateSystem != null && TargetCoordinateSystem != null &&
                SourceCoordinateSystem != TargetCoordinateSystem)
            {
                var coordinateTransformation =
                    Map.CoordinateSystemFactory.CreateTransformation(SourceCoordinateSystem,
                        TargetCoordinateSystem);

                foreach (var pointValue in samples)
                {
                    var transformedCoordinates =
                        coordinateTransformation.MathTransform.Transform(new[] { pointValue.X, pointValue.Y });
                    pointValue.X = transformedCoordinates[0];
                    pointValue.Y = transformedCoordinates[1];
                }
            }

            return samples;
        }

        protected override ImportSamplesOperation CreateImportSamplesOperation()
        {
            return new ImportRasterSamplesOperationImportData
            {
                Name = Name,
                CoordinateSystem = CoordinateSystem,
                Dirty = true,
                Enabled = Enabled,
                FilePath = FilePath,
            };
        }
    }
}