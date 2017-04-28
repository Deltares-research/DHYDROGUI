/*
 *
 *  Copyright (C) Stichting Deltares, 2015.
 *    
 *  This file is part of the D-Flow Flexible Mesh Plugin for the Delta 
 *  Shell Framework.
 * 
 *  The D-Flow Flexible Mesh Plugin is free software: you can 
 *  redistribute it and/or modify it under the terms of the GNU 
 *  General Public License as published by the Free Software 
 *  Foundation, either version 3 of the License, or (at your option) 
 *  any later version.
 * 
 *  The D-Flow Flexible Mesh Plugin is distributed in the hope that it 
 *  will be useful, but WITHOUT ANY WARRANTY; without even the implied 
 *  warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 *  See the GNU General Public License for more details.
 *  
 *  You should have received a copy of the GNU General Public 
 *  License along with the Delta Shell Framework. If not, see 
 *  <http://www.gnu.org/licenses/>.
 *
 *  Contact: software@deltares.nl                                         
 *  Stichting Deltares                                                           
 *  P.O. Box 177                                                                 
 *  2600 MH Delft, The Netherlands                                               
 *                                                                             
 *  All indications and logos of, and references to, "Deltares” and
 *  "Delft3D" are registered trademarks of Stichting Deltares, and 
 *  remain the property of Stichting Deltares. All rights reserved.                     
 *
 */

using System;
using System.IO;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Adaptors;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Api.TempImpl
{
    public static class GridInterpolateApi
    {
        /// <summary>
        /// Given a net-file, create an UnstructuredGrid object. It achieves this by creating a temporary model, initialise it, and 
        /// read the grid from the resulting map file. This is done because the initialise step can do a 'renumbering' of the grid 
        /// cells. Now, the user sees the grid cell indices that are actually used by the computational core. 
        /// If the initialise fails (throws an exception), the exception will be caught at a higher level. This can be the case when 
        /// the grid is non-orthogonal, for instance. In that case, the non-renumbered grid will be used. 
        /// </summary>
        /// <param name="netFilePath"></param>
        /// <returns></returns>
        public static UnstructuredGrid CreateGrid(string netFilePath)
        {
            if (!File.Exists(netFilePath) || Path.GetFileName(netFilePath) == null) return null;

            var tempPath = FileUtils.CreateTempDirectory();
            var tempModel = new WaterFlowFMModel { Name = "flowinit" };

            tempModel.ModelDefinition.GetModelProperty(KnownProperties.NetFile)
                .SetValueAsString(Path.GetFileName(netFilePath));

            tempModel.ModelDefinition.GetModelProperty(GuiProperties.MapOutputDeltaT).Value = new TimeSpan(1, 0, 0);
            tempModel.ModelDefinition.GetModelProperty(GuiProperties.WriteMapFile).Value = true;
            tempModel.ModelDefinition.GetModelProperty(KnownProperties.ExtForceFile).SetValueAsString("");
            tempModel.ModelDefinition.GetModelProperty(KnownProperties.BndExtForceFile).SetValueAsString("");

            File.Copy(netFilePath, Path.Combine(tempPath, Path.GetFileName(netFilePath)));

            var mduName = tempModel.Name + MduFile.MduExtension;
            var mduFilePath = Path.Combine(tempPath, mduName);

            tempModel.ExportTo(mduFilePath, false, false, false); //TODO: add features
            var mapFilePath = Path.Combine(Path.Combine(tempPath, "DFM_OUTPUT_" + tempModel.Name), tempModel.MapSavePath);

            // call model initialize
            using (var api = new RemoteFlexibleMeshModelApi())
            {
                try
                {
                    api.Initialize(mduFilePath);
                    if (!File.Exists(mapFilePath))
                    {
                        var logFilePath = Path.Combine(tempPath, "flowinit.dia");
                        throw new InvalidOperationException(
                            "Kernel failed to initialize model, could not perform grid interpolation. See log file for information: " +
                            Environment.NewLine + Environment.NewLine +
                            logFilePath);
                    }
                    
                }
                finally
                {
                    api.Finish();
                }
            }

            GridApiDataSet.DataSetConventions convention;
            using (var gridApi = GridApiFactory.CreateNew())
            {
                convention = gridApi.GetConvention(mapFilePath);
            }

            UnstructuredGrid importedGrid = null;
            if (convention == GridApiDataSet.DataSetConventions.IONC_CONV_OTHER)
            {
                importedGrid = NetFileImporter.ImportModelGrid(mapFilePath);
            }
            else if (convention == GridApiDataSet.DataSetConventions.IONC_CONV_UGRID)
            {
                using (var fmUGridAdaptor = new UGridToUnstructuredGridAdaptor(mapFilePath))
                {
                    importedGrid = fmUGridAdaptor.GetUnstructuredGridFromUGridMeshId(1);
                }
            }
            
            FileUtils.DeleteIfExists(tempPath);
            return importedGrid;
        }

        /// <summary>
        /// Given a net-file, create an UnstructuredGrid object. It achieves this by creating a temporary model, initialise it, and 
        /// read the grid from the resulting map file. This is done because the initialise step can do a 'renumbering' of the grid 
        /// cells. Now, the user sees the grid cell indices that are actually used by the computational core. 
        /// If the initialise fails (throws an exception), the exception will be caught at a higher level. This can be the case when 
        /// the grid is non-orthogonal, for instance. In that case, the non-renumbered grid will be used. 
        /// </summary>
        /// <param name="netFilePath"></param>
        /// <returns></returns>
        public static UnstructuredGrid CreateNetCdfGrid(string netFilePath)
        {
            if (!File.Exists(netFilePath) || Path.GetFileName(netFilePath) == null) return null;

            var tempPath = FileUtils.CreateTempDirectory();
            var tempModel = new WaterFlowFMModel { Name = "flowinit" };

            tempModel.ModelDefinition.GetModelProperty(KnownProperties.NetFile)
                .SetValueAsString(Path.GetFileName(netFilePath));

            tempModel.ModelDefinition.GetModelProperty(GuiProperties.MapOutputDeltaT).Value = new TimeSpan(1, 0, 0);
            tempModel.ModelDefinition.GetModelProperty(GuiProperties.WriteMapFile).Value = true;
            tempModel.ModelDefinition.GetModelProperty(KnownProperties.ExtForceFile).SetValueAsString("");
            tempModel.ModelDefinition.GetModelProperty(KnownProperties.BndExtForceFile).SetValueAsString("");
            tempModel.ModelDefinition.GetModelProperty("MapFormat").SetValueAsString("1");
            tempModel.ModelDefinition.GetModelProperty("RenumberFlowNodes").SetValueAsString("0");

            File.Copy(netFilePath, Path.Combine(tempPath, Path.GetFileName(netFilePath)));

            var mduName = tempModel.Name + MduFile.MduExtension;
            var mduFilePath = Path.Combine(tempPath, mduName);

            tempModel.ExportTo(mduFilePath, false, false, false); //TODO: add features
            var flowFmOutputPath = Path.Combine(tempPath, "DFM_OUTPUT_" + tempModel.Name);
            
            if (!Directory.Exists(flowFmOutputPath))
            {
                Directory.CreateDirectory(flowFmOutputPath);
            }
            var mapFilePath = Path.Combine(flowFmOutputPath, tempModel.MapSavePath);

            // call model initialize
            using (var api = new RemoteFlexibleMeshModelApi())
            {
                try
                {
                    api.Initialize(mduFilePath);

                    if (!File.Exists(mapFilePath))
                    {
                        var logFilePath = Path.Combine(tempPath, "flowinit.dia");
                        throw new InvalidOperationException(
                            "Kernel failed to initialize model, could not change grid from ugrid format to old netcdf format needed for 1d2d. See log file for information: " +
                            Environment.NewLine + Environment.NewLine +
                            logFilePath);
                    }
                }
                finally
                {
                    api.Finish();
                }
            }

            GridApiDataSet.DataSetConventions convention;
            using (var gridApi = GridApiFactory.CreateNew())
            {
                convention = gridApi.GetConvention(mapFilePath);
            }

            UnstructuredGrid importedGrid = null;
            if (convention == GridApiDataSet.DataSetConventions.IONC_CONV_OTHER)
            {
                importedGrid = NetFileImporter.ImportModelGrid(mapFilePath);
            }
            else if (convention == GridApiDataSet.DataSetConventions.IONC_CONV_UGRID)
            {
                using (var fmUGridAdaptor = new UGridToUnstructuredGridAdaptor(mapFilePath))
                {
                    importedGrid = fmUGridAdaptor.GetUnstructuredGridFromUGridMeshId(1);
                }
            }
            
            //FileUtils.DeleteIfExists(tempPath);
            return importedGrid;
        }
    }
}