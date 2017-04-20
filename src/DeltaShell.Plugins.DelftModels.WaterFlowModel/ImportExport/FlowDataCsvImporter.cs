/*
 *
 *  Copyright (C) Stichting Deltares, 2015.
 *    
 *  This file is part of the D-Flow1D Plugin for the Delta Shell 
 *  Framework.
 * 
 *  The D-Flow1D Plugin is free software: you can redistribute it 
 *  and/or modify it under the terms of the GNU General Public 
 *  License as published by the Free Software Foundation, either version 
 *  3 of the License, or (at your option) any later version.
 * 
 *  The D-Flow1D Plugin is distributed in the hope that it will be 
 *  useful, but WITHOUT ANY WARRANTY; without even the implied warranty 
 *  of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU 
 *  General Public License for more details.
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

﻿using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
﻿using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.Plugins.CommonTools.Functions;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using GeoAPI.Extensions.Feature;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    public class FlowDataCsvImporter : TimeSeriesCsvImporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FlowDataCsvImporter));

        public FlowDataCsvImporter()
        {
            FileImporter = new FlowTimeSeriesCsvFileImporter();
        }

        public FlowTimeSeriesCsvFileImporter FlowFileImporter
        {
            get { return FileImporter as FlowTimeSeriesCsvFileImporter; }
        }

        public override string Name
        {
            get { return "Flow1D CSV Importer"; }
        }

        public override IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(WaterFlowModel1DBoundaryNodeData);
                yield return typeof(WaterFlowModel1DLateralSourceData);
                yield return typeof(DataItemsEventedListAdapter<WaterFlowModel1DBoundaryNodeData>);
                yield return typeof(DataItemsEventedListAdapter<WaterFlowModel1DLateralSourceData>);
            }
        }

        public BoundaryRelationType BoundaryRelationType
        {
            get { return FlowFileImporter.BoundaryRelationType; }
            set { FlowFileImporter.BoundaryRelationType = value; }
        }

        public override object ImportItem(string path, object target)
        {
            var functionList = ((IEnumerable<IFunction>) base.ImportItem(path, target)).ToList();

            var targetFeatureData = target as IFeatureData;
            if (targetFeatureData != null)
            {
                if (functionList.Count() == 1)
                {
                    var data = functionList[0];
                    var importSucceeded = false;

                    var lateralSourceData = targetFeatureData as WaterFlowModel1DLateralSourceData;
                    if (lateralSourceData != null)
                    {
                        UpdateLateralSourceData(lateralSourceData, data);
                        importSucceeded = true;
                    }
                    var boundaryNodeData = targetFeatureData as WaterFlowModel1DBoundaryNodeData;
                    if (boundaryNodeData != null)
                    {
                        UpdateBoundaryConditionsSourceData(boundaryNodeData, data);
                        importSucceeded = true;
                    }
                    if (!importSucceeded)
                    {
                        targetFeatureData.Data = data;
                    }
                }
                else
                {
                    throw new InvalidOperationException("Only one function expected from Flow1D CSV importer.");
                }
            }

            var targetAsLateralList = target as DataItemsEventedListAdapter<WaterFlowModel1DLateralSourceData>;
            if (targetAsLateralList != null)
            {
                UpdateDataItems(functionList, targetAsLateralList);
            }

            var targetAsBoundariesList = target as DataItemsEventedListAdapter<WaterFlowModel1DBoundaryNodeData>;
            if (targetAsBoundariesList != null)
            {
                UpdateDataItems(functionList, targetAsBoundariesList);
            }

            if (targetFeatureData == null && targetAsLateralList == null && targetAsBoundariesList == null)
            {
                Log.ErrorFormat("An error occurred while setting {0} to target {1}", functionList, target);
            }

            return target;
        }

        private void UpdateDataItems<T> 
            (IEnumerable<IFunction> newFunctionList, DataItemsEventedListAdapter<T> existingList) where T: class, IFeatureData
        {
            // This operation can be expensive: O(n^2). In case of a large import, this might be too expensive. 
            foreach (var newFunction in newFunctionList)
            {
                var overwritten = false;
                foreach (var existingDataItem in existingList.DataItems)
                {
                    var lateralSourceData = existingDataItem.Value as WaterFlowModel1DLateralSourceData;
                    if (lateralSourceData != null && newFunction.Name == lateralSourceData.Feature.Name)
                    {
                        UpdateLateralSourceData(lateralSourceData, newFunction);
                        overwritten = true;
                        break;   // Stop searching. 
                    }

                    var boundaryData = existingDataItem.Value as WaterFlowModel1DBoundaryNodeData;
                    if (boundaryData != null && newFunction.Name == boundaryData.Feature.Name)
                    {
                        UpdateBoundaryConditionsSourceData(boundaryData, newFunction);
                        overwritten = true;
                        break;   // Stop searching.
                    }
                }

                if (!overwritten)
                {
                    Log.WarnFormat("Could not find suitable target for: {0}", newFunction.Name);
                }
            }
        }

        private void UpdateLateralSourceData(WaterFlowModel1DLateralSourceData existingData, IFunction newData)
        {
            if (FlowFileImporter.BoundaryRelationType == BoundaryRelationType.Q)
            {
                existingData.DataType = WaterFlowModel1DLateralDataType.FlowConstant;
                existingData.Flow = (double) newData.Components[0].Values[0];
            }
            else if (FlowFileImporter.BoundaryRelationType == BoundaryRelationType.QH || FlowFileImporter.BoundaryRelationType == BoundaryRelationType.QT)
            {
                existingData.Data = newData; 
                // The lateral source data will automatically recognise whether it is Q(h) or Q(t), depending on the datatype of the argument (double or DateTime)
                // It will change the existingData.DataType accordingly. 
            }
        }

        private void UpdateBoundaryConditionsSourceData(WaterFlowModel1DBoundaryNodeData existingData, IFunction newData)
        {
            if (FlowFileImporter.BoundaryRelationType == BoundaryRelationType.Q)
            {
                existingData.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant;
                existingData.Flow = (double)newData.Components[0].Values[0];
            }
            else if (FlowFileImporter.BoundaryRelationType == BoundaryRelationType.H)
            {
                existingData.DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant;
                existingData.Flow = (double)newData.Components[0].Values[0];
            }
            else if (FlowFileImporter.BoundaryRelationType == BoundaryRelationType.QH ||
                     FlowFileImporter.BoundaryRelationType == BoundaryRelationType.HT)
            {
                existingData.Data = newData;
                // Datatype will be automatically derived. 
            }
            else
            {
                // Hack to make the discovery algorithm aware that this is Q(t) and not H(t). 
                newData.Components[0].Attributes[FunctionAttributes.StandardName] =
                    FunctionAttributes.StandardNames.WaterDischarge;
                existingData.Data = newData;
            }
        }
    }
}