using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Data;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    /// <summary>
    /// Class storing all data read from a .hyd file.
    /// </summary>
    public sealed class HydFileData : Unique<long>, IHydroData, IEquatable<HydFileData>
    {
        private readonly IDictionary<string, Func<string>> delwaqDataToFilePathMapping;

        private readonly string SalinityName = "salinity";
        private readonly string TemperatureName = "temp";
        private readonly string TauName = "tau";
        private readonly string TauFlowName = "tauflow";
        private readonly string ChezyName = "chezy";
        private readonly string VelocityName = "velocity";
        private readonly string WidthName = "width";
        private readonly string SurfName = "surf";
        private LayerType layerType;
        private FileInfo path;
        private FileSystemWatcher fileWatcher;

        /// <summary>
        /// Occures when one of the hyd files changes (async event)
        /// </summary>
        public event EventHandler<EventArgs<string>> DataChanged;

        public HydFileData()
        {
            HydrodynamicLayerThicknesses = new double[0];
            NumberOfHydrodynamicLayersPerWaqSegmentLayer = new int[0];

            delwaqDataToFilePathMapping = new Dictionary<string, Func<string>>
            {
                {SalinityName, () => SalinityRelativePath},
                {TemperatureName, () => TemperatureRelativePath},
                {TauName, () => ShearStressesRelativePath},
                {TauFlowName, () => ShearStressesRelativePath},
                {ChezyName, () => ChezyCoefficientsRelativePath},
                {VelocityName, () => VelocitiesRelativePath},
                {WidthName, () => WidthsRelativePath},
                {SurfName, () => SurfacesRelativePath}
            };

            VolumesRelativePath = string.Empty;
            AreasRelativePath = string.Empty;
            FlowsRelativePath = string.Empty;
            PointersRelativePath = string.Empty;
            LengthsRelativePath = string.Empty;
            SalinityRelativePath = string.Empty;
            TemperatureRelativePath = string.Empty;
            VerticalDiffusionRelativePath = string.Empty;
            SurfacesRelativePath = string.Empty;
            ShearStressesRelativePath = string.Empty;
            GridRelativePath = string.Empty;
            AttributesRelativePath = string.Empty;
            VelocitiesRelativePath = string.Empty;
            WidthsRelativePath = string.Empty;
            ChezyCoefficientsRelativePath = string.Empty;

            fileWatcher = new FileSystemWatcher
            {
                IncludeSubdirectories = false,
                NotifyFilter = NotifyFilters.LastWrite
            };

            fileWatcher.Changed += (s, e) => FireDataChanged(e.FullPath);
        }

        /// <summary>
        /// Filepath of the read .hyd file.
        /// </summary>
        public FileInfo Path
        {
            get => path;
            set
            {
                path = value;

                if (!Directory.Exists(path.DirectoryName))
                {
                    return;
                }

                fileWatcher.Path = path.DirectoryName;
                fileWatcher.Filter = System.IO.Path.GetFileNameWithoutExtension(path.Name) + "*.*";
                fileWatcher.EnableRaisingEvents = true;
            }
        }

        /// <summary>
        /// Gets or sets the calculated checksum for the hyd-file.
        /// </summary>
        public string Checksum { get; set; }

        /// <summary>
        /// Relative filepath from <see cref="Path"/> to the boundary definitions.
        /// </summary>
        public string BoundariesRelativePath { get; set; }

        public IDictionary<WaterQualityBoundary, int[]> BoundaryNodeIds { get; set; }

        public IEventedList<WaterQualityBoundary> Boundaries { get; set; }

        /// <summary>
        /// The type of geometry used for the schematization.
        /// </summary>
        /// <seealso cref="BoundariesRelativePath"/>
        /// <seealso cref="GridRelativePath"/>
        public HydroDynamicModelType HydroDynamicModelType { get; set; }

        public LayerType LayerType
        {
            get => layerType;
            set
            {
                layerType = value;
                if (layerType == LayerType.Sigma)
                {
                    ZTop = 0.0;
                    ZBot = 1.0;
                }
            }
        }

        public double ZTop { get; set; }

        public double ZBot { get; set; }

        public DateTime ConversionReferenceTime { get; set; }

        public DateTime ConversionStartTime { get; set; }

        public DateTime ConversionStopTime { get; set; }

        public TimeSpan ConversionTimeStep { get; set; }

        /// <summary>
        /// The number of horizontal delwaq exchanges.
        /// </summary>
        public int NumberOfHorizontalExchanges { get; set; }

        /// <summary>
        /// The number of vertical delwaq exchanges.
        /// </summary>
        public int NumberOfVerticalExchanges { get; set; }

        /// <summary>
        /// The number of hydrodynamic layers available.
        /// </summary>
        public int NumberOfHydrodynamicLayers { get; set; }

        /// <summary>
        /// The fractional thickness of each hydrodynamic layer.
        /// </summary>
        public double[] HydrodynamicLayerThicknesses { get; set; }

        public int NumberOfDelwaqSegmentsPerHydrodynamicLayer { get; set; }

        /// <summary>
        /// The number of waq segment layers.
        /// </summary>
        public int NumberOfWaqSegmentLayers { get; set; }

        /// <summary>
        /// Returns the number of hydrodynamic layers is associated with each waq segment layer.
        /// </summary>
        public int[] NumberOfHydrodynamicLayersPerWaqSegmentLayer { get; set; }

        /// <summary>
        /// Relative filepath from <see cref="Path"/> to the grid file path.
        /// </summary>
        /// <example> *_flowgeom.nc OR *_waqgeom.nc </example>
        public string GridRelativePath { get; set; }

        public string FilePath => Path.FullName;

        public UnstructuredGrid Grid { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((HydFileData) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Path != null ? Path.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (Checksum != null ? Checksum.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return Path != null ? FilePath : "";
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other"> An object to compare with this object. </param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        public bool Equals(HydFileData other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (!Equals(Checksum, other.Checksum))
            {
                return false;
            }

            var pathsToCheck = new Func<HydFileData, string>[]
            {
                hfd => hfd.VolumesRelativePath,
                hfd => hfd.AreasRelativePath,
                hfd => hfd.FlowsRelativePath,
                hfd => hfd.PointersRelativePath,
                hfd => hfd.LengthsRelativePath,
                hfd => hfd.SalinityRelativePath,
                hfd => hfd.TemperatureRelativePath,
                hfd => hfd.VerticalDiffusionRelativePath,
                hfd => hfd.SurfacesRelativePath,
                hfd => hfd.ShearStressesRelativePath,
                hfd => hfd.GridRelativePath,
                hfd => hfd.AttributesRelativePath,
                hfd => hfd.VelocitiesRelativePath,
                hfd => hfd.WidthsRelativePath,
                hfd => hfd.ChezyCoefficientsRelativePath
            };
            return pathsToCheck.All(pathGetter => IsFileEqualWithOtherHydFile(other, pathGetter));
        }

        public IEventedList<WaterQualityBoundary> GetBoundaries()
        {
            return Boundaries;
        }

        public IDictionary<WaterQualityBoundary, int[]> GetBoundaryNodeIds()
        {
            return BoundaryNodeIds;
        }

        public bool HasDataFor(string functionName)
        {
            return !string.IsNullOrWhiteSpace(GetFilePathForFunctionName(functionName));
        }

        public string GetFilePathFor(string functionName)
        {
            string filePath = GetFilePathForFunctionName(functionName);
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Cannot give filepath for function '{0}' as it's not available in hydro dynamics file.",
                        functionName));
            }

            return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(FilePath), filePath);
        }

        public bool HasSameSchematization(IHydroData data)
        {
            var hydFileData = data as HydFileData;
            if (hydFileData != null)
            {
                return Equals(Checksum, hydFileData.Checksum);
            }

            return false;
        }

        public void Dispose()
        {
            fileWatcher.Dispose();
        }

        private void FireDataChanged(string fullPath)
        {
            if (DataChanged == null)
            {
                return;
            }

            DataChanged(this, new EventArgs<string>(fullPath));
        }

        private string GetFilePathForFunctionName(string functionName)
        {
            if (string.IsNullOrEmpty(functionName))
            {
                return null;
            }

            string name = functionName.ToLower();
            return delwaqDataToFilePathMapping.ContainsKey(name)
                       ? delwaqDataToFilePathMapping[name]()
                       : null;
        }

        private bool IsFileEqualWithOtherHydFile(HydFileData other, Func<HydFileData, string> getRelativePath)
        {
            string filePath = System.IO.Path.Combine(Path?.DirectoryName ?? string.Empty, getRelativePath(this));
            string otherFilePath =
                System.IO.Path.Combine(other.Path?.DirectoryName ?? string.Empty, getRelativePath(other));
            if (File.Exists(filePath) && File.Exists(otherFilePath))
            {
                return FileUtils.PathsAreEqual(filePath, otherFilePath)
                       && FileUtils.FilesAreEqual(filePath, otherFilePath);
            }

            return !File.Exists(filePath)
                   && !File.Exists(otherFilePath);
        }

        #region Bulk data filepaths

        /// <summary>
        /// Relative filepath from <see cref="Path"/> to the volumes raw data (binary file).
        /// </summary>
        public string VolumesRelativePath { get; set; }

        /// <summary>
        /// Relative filepath from <see cref="Path"/> to the area raw data (binary file).
        /// </summary>
        public string AreasRelativePath { get; set; }

        /// <summary>
        /// Relative filepath from <see cref="Path"/> to the flow raw data (binary file).
        /// </summary>
        public string FlowsRelativePath { get; set; }

        /// <summary>
        /// Relative filepath from <see cref="Path"/> to the segment exchanges raw data (binary file).
        /// </summary>
        public string PointersRelativePath { get; set; }

        /// <summary>
        /// Relative filepath from <see cref="Path"/> to the lengths raw data (binary file).
        /// </summary>
        public string LengthsRelativePath { get; set; }

        /// <summary>
        /// Relative filepath from <see cref="Path"/> to the salinity raw data (binary file).
        /// </summary>
        public string SalinityRelativePath { get; set; }

        /// <summary>
        /// Relative filepath from <see cref="Path"/> to the temperature raw data (binary file).
        /// </summary>
        public string TemperatureRelativePath { get; set; }

        /// <summary>
        /// Relative filepath from <see cref="Path"/> to the vertical diffusion raw data (binary file).
        /// </summary>
        public string VerticalDiffusionRelativePath { get; set; }

        /// <summary>
        /// Relative filepath from <see cref="Path"/> to the surfaces raw data (binary file).
        /// </summary>
        public string SurfacesRelativePath { get; set; }

        /// <summary>
        /// Relative filepath from <see cref="Path"/> to the shear stress raw data (binary file).
        /// </summary>
        public string ShearStressesRelativePath { get; set; }

        /// <summary>
        /// Relative filepath from <see cref="Path"/> to the attributes data that will be included in the input file.
        /// </summary>
        public string AttributesRelativePath { get; set; }

        /// <summary>
        /// Gets the velocities file path.Relative filepath from <see cref="Path"/>.
        /// </summary>
        /// <value>
        /// The velocities relative file path (with respect to hydraulic data location). Extension .dat.
        /// </value>
        public string VelocitiesRelativePath { get; set; }

        /// <summary>
        /// Gets the widths file path. Relative filepath from <see cref="Path"/>.
        /// </summary>
        /// <value>
        /// The widths relative file path (with respect to hydraulic data location). Extension .dat.
        /// </value>
        public string WidthsRelativePath { get; set; }

        /// <summary>
        /// Gets the chezy coefficients file path. Relative filepath from <see cref="Path"/>.
        /// </summary>
        /// <value>
        /// The chezy coefficients relative file path (with respect to hydraulic data location). Extension .dat.
        /// </value>
        public string ChezyCoefficientsRelativePath { get; set; }

        #endregion
    }
}