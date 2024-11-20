using System;
using System.IO;
using DelftTools.Utils.NetCdf;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition
{
    public static class ICoordinateSystemExtensions
    {
        public static bool IsNetfileCoordinateSystemUpToDate(this ICoordinateSystem coordinateSystem, string targetFile)
        {
            if (File.Exists(targetFile))
            {
                var fileCoordinateSystem = NetFile.ReadCoordinateSystem(targetFile);
                var fileProjectedCSName = GetProjectedCoordinateSystemNameFromNetFile(targetFile);

                if (fileCoordinateSystem == null || (coordinateSystem == null && fileCoordinateSystem != null) ||
                    fileCoordinateSystem.AuthorityCode != coordinateSystem.AuthorityCode ||
                    (!String.IsNullOrEmpty(fileProjectedCSName) && fileProjectedCSName != coordinateSystem.Name))
                {
                    return false;
                }
            }

            return true;
        }
        private static string GetProjectedCoordinateSystemNameFromNetFile(string targetFile)
        {
            NetCdfFile netCdfFile = null;
            string nameProjectedCS = string.Empty;

            try
            {
                netCdfFile = NetCdfFile.OpenExisting(targetFile, true);
                var projectedCSVariable = netCdfFile.GetVariableByName("projected_coordinate_system");
                if (projectedCSVariable != null)
                    nameProjectedCS = netCdfFile.GetAttributeValue(projectedCSVariable, "name");
            }
            finally
            {
                if (netCdfFile != null)
                    netCdfFile.Close();
            }

            return nameProjectedCS;
        }
    }
}