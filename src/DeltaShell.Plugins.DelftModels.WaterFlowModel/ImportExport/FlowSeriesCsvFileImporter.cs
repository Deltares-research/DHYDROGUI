using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.Plugins.CommonTools.Functions;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using GeoAPI.Extensions.Feature;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    public class FlowSeriesCsvFileImporter : TimeSeriesCsvImporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (FlowSeriesCsvFileImporter));

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

        public override object ImportItem(string path, object target)
        {
            var functionList = ((IEnumerable<IFunction>) base.ImportItem(path, target)).ToList();

            var targetFeatureData = target as IFeatureData;
            if (targetFeatureData != null)
            {
                if (functionList.Count() == 1)
                    targetFeatureData.Data = functionList[0];
                else
                {
                    throw new InvalidOperationException("Only one function expected from Flow1D CSV importer.");
                }
            }

            var targetAsLateralList = target as DataItemsEventedListAdapter<WaterFlowModel1DLateralSourceData>;
            if (targetAsLateralList != null)
            {
                updateDataItems(functionList, targetAsLateralList);
            }

            var targetAsBoundariesList = target as DataItemsEventedListAdapter<WaterFlowModel1DBoundaryNodeData>;
            if (targetAsBoundariesList != null)
            {
                updateDataItems(functionList, targetAsBoundariesList);
            }

            if (targetFeatureData == null && targetAsLateralList == null && targetAsBoundariesList == null)
            {
                Log.ErrorFormat("An error occurred while setting {0} to target {1}", functionList, target);
            }

            return target;
        }

        private void updateDataItems<T> 
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
                        updateLateralSourceData(lateralSourceData, newFunction);
                        overwritten = true;
                        break;   // Stop searching. 
                    }

                    var boundaryData = existingDataItem.Value as WaterFlowModel1DBoundaryNodeData;
                    if (boundaryData != null && newFunction.Name == boundaryData.Feature.Name)
                    {
                        updateBoundaryConditionsSourceData(boundaryData, newFunction);
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

        private void updateLateralSourceData(WaterFlowModel1DLateralSourceData existingData, IFunction newData)
        {
            if (FileImporter.BoundaryRelationType == BoundaryRelationType.Q)
            {
                existingData.DataType = WaterFlowModel1DLateralDataType.FlowConstant;
                existingData.Flow = (double) newData.Components[0].Values[0];
            }
            else if (FileImporter.BoundaryRelationType == BoundaryRelationType.QH || FileImporter.BoundaryRelationType == BoundaryRelationType.QT)
            {
                existingData.Data = newData; 
                // The lateral source data will automatically recognise whether it is Q(h) or Q(t), depending on the datatype of the argument (double or DateTime)
                // It will change the existingData.DataType accordingly. 
            }
        }

        private void updateBoundaryConditionsSourceData(WaterFlowModel1DBoundaryNodeData existingData, IFunction newData)
        {
            if (FileImporter.BoundaryRelationType == BoundaryRelationType.Q)
            {
                existingData.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant;
                existingData.Flow = (double)newData.Components[0].Values[0];
            }
            else if (FileImporter.BoundaryRelationType == BoundaryRelationType.H)
            {
                existingData.DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant;
                existingData.Flow = (double)newData.Components[0].Values[0];
            }
            else if (FileImporter.BoundaryRelationType == BoundaryRelationType.QH ||
                     FileImporter.BoundaryRelationType == BoundaryRelationType.HT)
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