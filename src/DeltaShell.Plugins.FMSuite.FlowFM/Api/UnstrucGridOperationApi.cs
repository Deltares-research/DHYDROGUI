using System;
using System.IO;
using System.Linq;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Geometries;
using System.Collections.Generic;
using System.Threading;
using NetTopologySuite.Geometries;
using SharpMap.Api;
using Point = NetTopologySuite.Geometries.Point;

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
        public const string Boundary = "boundary";
        public const string WaterLevelBnd = "waterlevelbnd";
        public const string VelocityBnd = "velocitybnd";
        public const string DischargeBnd = "dischargebnd";

        private readonly string tempPath;
        private IFlexibleMeshModelApi api;
        private const double MissingValue = -999.0;

        private static readonly string[] propertiesToClear =
        {
            KnownProperties.ExtForceFile, KnownProperties.ThinDamFile,
            KnownProperties.FixedWeirFile, KnownProperties.DryPointsFile,
            KnownProperties.ObsCrsFile, KnownProperties.LandBoundaryFile, KnownProperties.ObsFile,
            KnownProperties.StructuresFile, KnownProperties.PartitionFile, KnownProperties.ManholeFile,
            KnownProperties.ProfdefFile, KnownProperties.ProflocFile, KnownProperties.RestartFile,
            KnownProperties.WaterLevIniFile
        };

        private bool disposed;

        public UnstrucGridOperationApi(IFlexibleMeshModelApi api)
        {
            this.api = api;
        }

        public UnstrucGridOperationApi(WaterFlowFMModel model)
        {
            tempPath = FileUtils.CreateTempDirectory();

            // gather paths            
            var mduName = model.Name + MduFile.MduExtension;
            var mduFilePath = Path.Combine(tempPath, mduName);

            // make sure we initialize without: ext, thin dams, cross sections, etc..
            var adjustedMduProperties = model.ModelDefinition.Properties.ToList();
            foreach (var propertyToClear in propertiesToClear)
            {
                var existingProperty = model.ModelDefinition.GetModelProperty(propertyToClear);
                var clonedProperty = (WaterFlowFMProperty)existingProperty.Clone();
                clonedProperty.Value = ""; //clear

                int adjustedIndex = adjustedMduProperties.FindIndex(p => p.PropertyDefinition.MduPropertyName.Equals(propertyToClear, StringComparison.InvariantCultureIgnoreCase));
                adjustedMduProperties[adjustedIndex] = clonedProperty;
            }

            // write mdu with adjusted properties
            var mduFile = new MduFile();
            mduFile.WriteProperties(mduFilePath, adjustedMduProperties, false, false, useNetCDFMapFormat: model.UseNetCDFMapFormat, disableFlowNodeRenumbering:model.DisableFlowNodeRenumbering);

            // do write grid file
            var gridFile = model.NetFilePath;
            if (!File.Exists(gridFile)) 
                return;

            model.ExportTo(mduFilePath, false);
            model.SetModelStateHandlerModelWorkingDirectory(model.ExplicitWorkingDirectory??model.WorkingDirectory??Environment.CurrentDirectory);
            
            api = new RemoteFlexibleMeshModelApi();
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
        
        public bool SnapsToGrid(IGeometry geometry)
        {
            if (geometry == null)
                return false;
            return !GetGridSnappedGeometry(ThinDams, geometry).IsEmpty;
        }

        public IGeometry GetGridSnappedGeometry(string featureType, IGeometry geometry)
        {
            return GetGridSnappedGeometry(featureType, new[] {geometry}).Single();
        }

        public IEnumerable<IGeometry> GetGridSnappedGeometry(string featureType, ICollection<IGeometry> geometries)
        {
            if (api == null) // no grid?
                return geometries;

            if (featureType == NoSnapping)
                return geometries;

            if (geometries.Count == 0)
                return geometries;

            try
            {
                var snappedGeometries = GetGridSnappedGeometryCore(featureType, geometries);

                if (featureType == WaterLevelBnd || featureType == VelocityBnd || featureType == DischargeBnd)
                    snappedGeometries = snappedGeometries.Select(ConvertToMultiPoint).ToList();

                return snappedGeometries;
            }
            catch (Exception e) // for now don't bother the users with our bugs:
            {
                Console.WriteLine(e);
                return geometries;
            }
        }

        public int[] GetLinkedCells()
        {
            int[] linkedCells = new int[0];

            if (api == null) return linkedCells; // no grid, return empty list

            try
            {
                linkedCells = GetLinkedCellsCore();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return linkedCells;
        }

        private int[] GetLinkedCellsCore()
        {
            int[] edgesAlongEmbankments = new int[0];

            var edgeNumbers = api.GetValues("edgenumbers1d2d");
            if (edgeNumbers == null || edgeNumbers.Length == 0) return edgesAlongEmbankments;
            edgesAlongEmbankments = new int[edgeNumbers.Length];
            for (int i = 0; i < edgeNumbers.Length; i++)
            {
                edgesAlongEmbankments[i] = (int)edgeNumbers.GetValue(i);
            }

            return edgesAlongEmbankments;
        }

        private static MultiPoint ConvertToMultiPoint(IGeometry geom)
        {
            return new MultiPoint(geom.Coordinates.Select(c => (IPoint) new Point(c.X, c.Y)).ToArray());
        }

        private IEnumerable<IGeometry> GetGridSnappedGeometryCore(string featureType, ICollection<IGeometry> geometries)
        {
            var xin = new List<double>();
            var yin = new List<double>();
            double[] xout = new double[0], yout = new double[0];
            int[] featureIds = new int[0];

            foreach (var geom in geometries)
            {
                foreach (var coord in geom.Coordinates)
                {
                    xin.Add(coord.X);
                    yin.Add(coord.Y);
                }

                // no separators for point geometries (obs points):
                if (geom.Coordinates.Length != 1)
                {
                    xin.Add(MissingValue);
                    yin.Add(MissingValue);
                }
            }

            if (!api.GetSnappedFeature(featureType, xin.ToArray(), yin.ToArray(), ref xout, ref yout, ref featureIds))
                throw new InvalidOperationException("Error during snapping");

            // rebuild geometries:
            var outGeometries = new IGeometry[geometries.Count];

            if (featureType == ObsPoint)
            {
                for (var i = 0; i < xout.Length; i++)
                {
                    var x = xout[i];
                    var y = yout[i];
                    if (IsMissingValue(x) && IsMissingValue(y))
                        outGeometries[i] = GeometryCollection.Empty;
                    else
                        outGeometries[i] = new Point(x, y);
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
                        lastCoordinates.Add(new Coordinate(xout[i], yout[i]));

                    lastId = featureIds[i] - 1; //make it 0 based
                }

                if (lastCoordinates.Count > 0)
                    outGeometries[lastId] = CreateGeometry(lastCoordinates);

                // replace null entries with empty geometry
                for (var i = 0; i < outGeometries.Length; i++)
                    if (outGeometries[i] == null)
                        outGeometries[i] = GeometryCollection.Empty;
            }
            return outGeometries;
        }

        private static bool IsMissingValue(double val)
        {
            return Math.Abs(val - MissingValue) < 1e-3;
        }

        private static IGeometry CreateGeometry(IList<Coordinate> coordinates)
        {
            if (coordinates.Count == 0)
                return null;

            if (coordinates.Count == 1)
                return new Point(coordinates[0].X, coordinates[0].Y);
            
            var geometries = new List<IGeometry>();
            var lastCoordinates = new List<Coordinate>();
            foreach (var coord in coordinates)
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
                    lastCoordinates.Add(coord);
            }
            if (lastCoordinates.Count > 0)
                geometries.Add(CreateGeometryCore(lastCoordinates));

            if (geometries.Count == 1)
                return geometries.First();
            return new GeometryCollection(geometries.ToArray());
        }

        private static IGeometry CreateGeometryCore(IList<Coordinate> coordinates)
        {
            if (coordinates.Count == 1)
                return new Point(coordinates[0].X, coordinates[0].Y);
            return new LineString(coordinates.ToArray());
        }

        public void Dispose()
        {
            if(disposed) return;
            if (api != null)
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
                finally
                {
                    // Must always ensure this happens to prevent GC deadlock on project close!
                    GC.SuppressFinalize(this);
                }
            }
        }
    }
}