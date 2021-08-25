using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Units;
using DelftTools.Utils.NetCdf;
using DeltaShell.NGHS.IO.Grid;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores
{
    internal static class FouFileReader
    {
        private const string fourierAttributeToCheck = "frequency_degrees_per_hour";
        private const string noDataAttributeName = "_FillValue";
        private const string unitsAttributeName = "units";

        internal const string ncVariableName = "nc_name";
        
        private enum MeshType
        {
            Mesh1d,
            Mesh2d
        }

        public static FouFileMetaData ReadMetaData(string path)
        {
            var grid = UGridFileHelper.ReadUnstructuredGrid(path, recreateCells:false);
            
            var hydroNetwork = new HydroNetwork();
            var discretization = new Discretization{Network = hydroNetwork};
            UGridFileHelper.ReadNetworkAndDiscretisation(path,discretization,hydroNetwork,null,null);

            var variables1D = GetMeshDependentVariables(path, MeshType.Mesh1d);
            var variables2D = GetMeshDependentVariables(path, MeshType.Mesh2d);

            var networkLocations = discretization.Locations.Values.ToList();

            return new FouFileMetaData
            {
                Path = path,
                Grid = grid,
                Network = hydroNetwork,
                Mesh1dLocations = networkLocations,
                Variables1D = variables1D,
                Variables2D = variables2D,
            };
        }

        public static IEnumerable<INetworkCoverage> Create1dMeshCoverages(FouFileMetaData metaData)
        {
            if (!File.Exists(metaData.Path))
                yield break;

            var file = NetCdfFile.OpenExisting(metaData.Path);

            foreach (NetCdfVariable variable in metaData.Variables1D)
            {
                yield return GenerateCoverageForVariable(file, variable, () => 
                                                             new NetworkCoverage("", false) { Network = metaData.Network });
            }
        }

        public static IEnumerable<UnstructuredGridCoverage> Create2dMeshCoverages(FouFileMetaData metaData)
        {
            var file = NetCdfFile.OpenExisting(metaData.Path);
            foreach (NetCdfVariable variable in metaData.Variables2D)
            {
                yield return GenerateCoverageForVariable(file, variable, () => new UnstructuredGridCellCoverage(metaData.Grid, false));
            }
        }

        private static T GenerateCoverageForVariable<T>(NetCdfFile file, NetCdfVariable variable, Func<T> createCoverageFunc) where T : ICoverage
        {
            var name = file.GetVariableName(variable);
            var attributes = file.GetAttributes(variable);

            attributes.TryGetValue(unitsAttributeName, out object unit);
            attributes.TryGetValue(noDataAttributeName, out object noDataValue);

            var coverage = createCoverageFunc.Invoke();

            coverage.Name = name;
            ((IFunction)coverage).Attributes = attributes.ToDictionary(a => a.Key, a => a.Value?.ToString());

            var component = coverage.Components[0];
            component.NoDataValue = noDataValue;
            component.Unit = new Unit("", unit?.ToString());
            component.Attributes.Add(ncVariableName, name);

            return coverage;
        }

        private static IEnumerable<NetCdfVariable> GetMeshDependentVariables(string path, MeshType meshType)
        {
            var file = NetCdfFile.OpenExisting(path);

            var dimensions = file.GetAllDimensions()
                                 .Where(d => string.Equals(file.GetDimensionName(d), GetDimensionName(meshType), StringComparison.InvariantCultureIgnoreCase))
                                 .ToArray();

            return file.GetVariables().Where(v =>
            {
                var variableDimensions = file.GetDimensions(v).ToArray();
                var attributes = file.GetAttributes(v);

                return variableDimensions.Length == 1 
                       && dimensions.Contains(variableDimensions[0])
                       && attributes.ContainsKey(fourierAttributeToCheck);
            });
        }

        private static string GetDimensionName(MeshType meshType)
        {
            switch (meshType)
            {
                case MeshType.Mesh1d:
                    return "mesh1d_nNodes";
                case MeshType.Mesh2d:
                    return "mesh2d_nFaces";
                default:
                    throw new ArgumentOutOfRangeException(nameof(meshType), meshType, null);
            }
        }
    }
}