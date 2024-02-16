using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Geometries;
using SharpMap.Api;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Api
{
    public class UnstrucGridOperationApi : IGridOperationApi, IDisposable
    {
        public const string NoSnapping = "no_snap";
        public const string ThinDams = "thindam";
        public const string FixedWeir = "fixedweir";
        public const string ObsPoint = "obspoint";
        public const string ObsCrossSection = "crosssection";
        public const string Weir = "weir";
        public const string Gate = "gate";
        public const string Pump = "pump";
        public const string SourceSink = "sourcesink";
        public const string Boundary = "boundary";
        public const string WaterLevelBnd = "waterlevelbnd";
        public const string VelocityBnd = "velocitybnd";
        public const string DischargeBnd = "dischargebnd";
        private const double MissingValue = -999.0;

        private static readonly ILog Log = LogManager.GetLogger(typeof(UnstrucGridOperationApi));

        private static readonly string[] propertiesToClear =
        {
            KnownProperties.ExtForceFile,
            KnownProperties.ThinDamFile,
            KnownProperties.FixedWeirFile,
            KnownProperties.DryPointsFile,
            KnownProperties.BridgePillarFile,
            KnownProperties.ObsCrsFile,
            KnownProperties.LandBoundaryFile,
            KnownProperties.ObsFile,
            KnownProperties.StructuresFile,
            KnownProperties.PartitionFile,
            KnownProperties.ManholeFile,
            KnownProperties.ProfdefFile,
            KnownProperties.ProflocFile,
            KnownProperties.RestartFile,
            KnownProperties.WaterLevIniFile,
            KnownProperties.TrtRou,
            KnownProperties.TrtDef,
            KnownProperties.TrtL
        };

        private readonly string tempPath;
        private readonly string mduFilePath;
        private IFlexibleMeshModelApi api;

        private bool disposed;

        public UnstrucGridOperationApi(IFlexibleMeshModelApi api)
        {
            this.api = api;
        }

        /// <summary>
        /// Gets an iterator for iterating over feature coverage time series
        /// </summary>
        /// <param name="model"> </param>
        /// <param name="fullExport"> When false makes an export without extForces or features </param>
        /// <returns> </returns>
        public UnstrucGridOperationApi(WaterFlowFMModel model, bool fullExport = true)
        {
            tempPath = FileUtils.CreateTempDirectory();

            // gather paths            
            string mduName = model.Name + FileConstants.MduFileExtension;

            mduFilePath = Path.Combine(tempPath, model.Name, DirectoryNameConstants.InputDirectoryName, mduName);

            // make sure we initialize without: ext, thin dams, cross sections, etc..
            List<WaterFlowFMProperty> adjustedMduProperties = model.ModelDefinition.Properties.ToList();
            foreach (string propertyToClear in propertiesToClear)
            {
                WaterFlowFMProperty existingProperty = model.ModelDefinition.GetModelProperty(propertyToClear);
                var clonedProperty = (WaterFlowFMProperty)existingProperty.Clone();
                if (propertyToClear.ToLowerInvariant() == KnownProperties.TrtRou.ToLowerInvariant())
                {
                    clonedProperty.SetValueFromString("N");
                }
                else
                {
                    clonedProperty.SetValueFromString(string.Empty); //clear 
                }

                int adjustedIndex = adjustedMduProperties.FindIndex(
                    p => p.PropertyDefinition.MduPropertyName.Equals(propertyToClear,
                                                                     StringComparison.InvariantCultureIgnoreCase));
                adjustedMduProperties[adjustedIndex] = clonedProperty;
            }

            // do write grid file
            string gridFile = model.NetFilePath;
            if (!File.Exists(gridFile))
            {
                return;
            }

            /* When initializing this api for GridSnap features, we are not interested in doing a full export, only in having
             the api running.*/
            model.ExportTo(mduFilePath, false, fullExport, fullExport);

            // Overwrite existing mdu to ignore the properties with adjusted properties
            var mduFile = new MduFile();

            var mduFileWriteConfig = new MduFileWriteConfig
            {
                WriteExtForcings = fullExport,
                WriteFeatures = fullExport,
                WriteMorphologySediment = fullExport,
                DisableFlowNodeRenumbering = model.DisableFlowNodeRenumbering
            };

            mduFile.WriteProperties(mduFilePath,
                                    fullExport ? model.ModelDefinition.Properties.ToList() : adjustedMduProperties,
                                    mduFileWriteConfig);

            TryInitializeApi();
        }

        public string MduFilePath => mduFilePath;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool SnapsToGrid(IGeometry geometry)
        {
            if (geometry == null)
            {
                return false;
            }

            return !GetGridSnappedGeometry(ThinDams, geometry).IsEmpty;
        }

        public IGeometry GetGridSnappedGeometry(string featureType, IGeometry geometry)
        {
            return GetGridSnappedGeometry(featureType, new[]
            {
                geometry
            }).Single();
        }

        public IEnumerable<IGeometry> GetGridSnappedGeometry(string featureType, ICollection<IGeometry> geometries)
        {
            if (api == null) // no grid?
            {
                return geometries;
            }

            if (featureType == NoSnapping)
            {
                return geometries;
            }

            if (geometries.Count == 0)
            {
                return geometries;
            }

            try
            {
                IEnumerable<IGeometry> snappedGeometries = GetGridSnappedGeometryCore(featureType, geometries);

                if (featureType == WaterLevelBnd || featureType == VelocityBnd || featureType == DischargeBnd
                    || featureType == SourceSink)
                {
                    snappedGeometries = snappedGeometries.Select(ConvertToMultiPoint).ToList();
                }

                return snappedGeometries;
            }
            catch (Exception e) // for now don't bother the users with our bugs:
            {
                DisposeApiIfNotReachable(featureType);

                Console.WriteLine(e);
                return geometries;
            }
        }

        public int[] GetLinkedCells() =>
            throw new NotSupportedException();

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing && api != null)
            {
                api.Finish();
                api.Dispose();
                api = null;
                Thread.Sleep(100);
                try
                {
                    FileUtils.DeleteIfExists(tempPath);
                    disposed = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to clean up temp snap directory: " + e);
                }
            }
        }

        private static MultiPoint ConvertToMultiPoint(IGeometry geom)
        {
            return new MultiPoint(geom.Coordinates.Select(c => (IPoint)new Point(c.X, c.Y)).ToArray());
        }

        private IEnumerable<IGeometry> GetGridSnappedGeometryCore(string featureType, ICollection<IGeometry> geometries)
        {
            var xin = new List<double>();
            var yin = new List<double>();
            double[] xout = new double[0], yout = new double[0];
            var featureIds = new int[0];

            foreach (IGeometry geom in geometries)
            {
                foreach (Coordinate coord in geom.Coordinates)
                {
                    xin.Add(coord.X);
                    yin.Add(coord.Y);
                }

                // no separators for point geometries, like obs points. However for source and sinks they are needed.
                if (geom.Coordinates.Length != 1 || featureType == SourceSink)
                {
                    xin.Add(MissingValue);
                    yin.Add(MissingValue);
                }
            }

            if (!api.GetSnappedFeature(featureType, xin.ToArray(), yin.ToArray(), ref xout, ref yout, ref featureIds))
            {
                throw new InvalidOperationException("Error during snapping");
            }

            // rebuild geometries:
            var outGeometries = new IGeometry[geometries.Count];

            if (featureType == ObsPoint)
            {
                for (var i = 0; i < xout.Length; i++)
                {
                    double x = xout[i];
                    double y = yout[i];
                    if (IsMissingValue(x) && IsMissingValue(y))
                    {
                        outGeometries[i] = GeometryCollection.Empty;
                    }
                    else
                    {
                        outGeometries[i] = new Point(x, y);
                    }
                }
            }
            else
            {
                var lastCoordinates = new List<Coordinate>();
                var lastId = 0;
                for (var i = 0; i < xout.Length; i++)
                {
                    if (featureIds[i] == 0) // feature separator
                    {
                        if (lastCoordinates.Count > 0)
                        {
                            outGeometries[lastId] = CreateGeometry(lastCoordinates);
                            lastCoordinates.Clear();
                        }
                    }
                    else
                    {
                        lastCoordinates.Add(new Coordinate(xout[i], yout[i]));
                    }

                    lastId = featureIds[i] - 1; //make it 0 based
                }

                if (lastCoordinates.Count > 0)
                {
                    outGeometries[lastId] = CreateGeometry(lastCoordinates);
                }

                // replace null entries with empty geometry
                for (var i = 0; i < outGeometries.Length; i++)
                {
                    if (outGeometries[i] == null)
                    {
                        outGeometries[i] = GeometryCollection.Empty;
                    }
                }
            }

            return outGeometries;
        }

        private void DisposeApiIfNotReachable(string featureType)
        {
            if (api != null)
            {
                try
                {
                    //Check whether the api is still available, if so just go on normally.
                    api.GetVersionString();
                    return;
                }
                catch
                {
                    api.Dispose(); //cleanup previous instance
                    Log.WarnFormat(
                        Resources
                            .UnstrucGridOperationApi_DisposeApiIfNotReachable_API_failed_to_generate_snapped_feature__0___Try_reopening_the_project_if_the_problem_persists_,
                        featureType);
                }
            }

            TryInitializeApi();
        }

        private void TryInitializeApi()
        {
            api = FlexibleMeshModelApiFactory.CreateNew();
            if (api == null)
            {
                throw new InvalidOperationException("No FlexibleMesh api could be constructed.");
            }

            try
            {
                api.Initialize(mduFilePath);
            }
            catch (Exception)
            {
                api.Dispose(); // cleanup remote instance on crash!
                throw;
            }
        }

        private static bool IsMissingValue(double val)
        {
            return Math.Abs(val - MissingValue) < 1e-3;
        }

        private static IGeometry CreateGeometry(IList<Coordinate> coordinates)
        {
            if (coordinates.Count == 0)
            {
                return null;
            }

            if (coordinates.Count == 1)
            {
                return new Point(coordinates[0].X, coordinates[0].Y);
            }

            var geometries = new List<IGeometry>();
            var lastCoordinates = new List<Coordinate>();
            foreach (Coordinate coord in coordinates)
            {
                if (IsMissingValue(coord.X))
                {
                    if (lastCoordinates.Count > 0)
                    {
                        geometries.Add(CreateGeometryCore(lastCoordinates));
                        lastCoordinates.Clear();
                    }
                }
                else
                {
                    lastCoordinates.Add(coord);
                }
            }

            if (lastCoordinates.Count > 0)
            {
                geometries.Add(CreateGeometryCore(lastCoordinates));
            }

            if (geometries.Count == 1)
            {
                return geometries.First();
            }

            return new GeometryCollection(geometries.ToArray());
        }

        private static IGeometry CreateGeometryCore(IList<Coordinate> coordinates)
        {
            if (coordinates.Count == 1)
            {
                return new Point(coordinates[0].X, coordinates[0].Y);
            }

            return new LineString(coordinates.ToArray());
        }
    }
}