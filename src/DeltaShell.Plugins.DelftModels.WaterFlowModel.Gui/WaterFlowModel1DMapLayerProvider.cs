using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Drawing;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Properties;
using GeoAPI.Extensions.CoordinateSystems;
using SharpMap.Api;
using SharpMap.Api.Layers;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.Rendering.Thematics;
using SharpMap.Styles;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui
{
    public class WaterFlowModel1DMapLayerProvider : IMapLayerProvider
    {
        public bool CanCreateLayerFor(object data, object parentObject)
        {
            return data is WaterFlowModel1D
                   || (data is ModelFolder && ((ModelFolder)data).Model is WaterFlowModel1D)
                   || data is IEventedList<WaterFlowModel1DLateralSourceData>
                   || data is IEventedList<WaterFlowModel1DBoundaryNodeData>
                   ;
        }

        public IEnumerable<object> ChildLayerObjects(object data)
        {
            var flowModel1D = data as WaterFlowModel1D;
            if (flowModel1D != null)
            {
                yield return new ModelFolder {Model = flowModel1D, Role = DataItemRole.Input};
                yield return new ModelFolder { Model = flowModel1D, Role = DataItemRole.Output };
            }

            var modelFolder = data as ModelFolder;
            if (modelFolder != null && modelFolder.Model is WaterFlowModel1D)
            {
                var flowModel = (WaterFlowModel1D) modelFolder.Model;
                if (modelFolder.Role == DataItemRole.Input)
                {
                    var rootModel = GetRootModel(flowModel);
                    if (rootModel == null || rootModel is WaterFlowModel1D || flowModel.GetDataItemByValue(flowModel.Network).LinkedTo == null)
                    {
                        yield return flowModel.Network;
                    }

                    yield return flowModel.NetworkDiscretization;
                    yield return flowModel.BoundaryConditions;
                    yield return flowModel.LateralSourceData;
                    yield return flowModel.InitialConditions;
                    yield return flowModel.InitialFlow;

                    if (flowModel.UseSalt)
                    {
                        yield return flowModel.InitialSaltConcentration;
                    }

                    yield return flowModel.WindShielding;

                    if (flowModel.UseSalt)
                    {
                        yield return flowModel.DispersionCoverage;
                        if (flowModel.DispersionFormulationType != DispersionFormulationType.Constant)
                        {
                            yield return flowModel.DispersionF3Coverage;
                            yield return flowModel.DispersionF4Coverage;
                        }
                    }
                }
                else
                {
                    foreach (var function in flowModel.OutputFunctions)
                    {
                        yield return function;
                    }
                }
            }
        }

        public ILayer CreateLayer(object data, object parentData)
        {
            var flowModel1D = data as WaterFlowModel1D;
            if (flowModel1D != null)
            {
                return CreateReadonlyGroupLayer(flowModel1D.Name);
            }

            var modelFolder = data as ModelFolder;
            if (modelFolder != null)
            {
                return CreateReadonlyGroupLayer(modelFolder.Role == DataItemRole.Input ? "Input" : "Output");
            }
            
            var lateralSourceData = data as IEventedList<WaterFlowModel1DLateralSourceData>;
            if (lateralSourceData != null)
            {
                ICoordinateSystem coordinateSystem = null;
                var folder = parentData as ModelFolder;
                if (folder != null && folder.Model is WaterFlowModel1D)
                {
                    coordinateSystem = ((WaterFlowModel1D) folder.Model).Network.CoordinateSystem;
                }

                return CreateLateralDataLayer(lateralSourceData, coordinateSystem);
            }

            var boundaryNodeData = data as IEventedList<WaterFlowModel1DBoundaryNodeData>;
            if (boundaryNodeData != null)
            {
                ICoordinateSystem coordinateSystem = null;
                var folder = parentData as ModelFolder;
                if (folder != null && folder.Model is WaterFlowModel1D)
                {
                    coordinateSystem = ((WaterFlowModel1D)folder.Model).Network.CoordinateSystem;
                }

                return CreateBoundaryNodeDataLayer(boundaryNodeData, coordinateSystem);
            }
            
            return null;
        }

        private static VectorLayer CreateLateralDataLayer(IEventedList<WaterFlowModel1DLateralSourceData> lateralSourceDataList, ICoordinateSystem coordinateSystem)
        {
            return new VectorLayer("Lateral Data")
                       {
                           Visible = false,
                           Selectable = true,
                           NameIsReadOnly = true,
                           DataSource = new FeatureCollection
                                            {
                                                FeatureType = typeof (WaterFlowModel1DLateralSourceData),
                                                Features = (IList) lateralSourceDataList,
                                                CoordinateSystem = coordinateSystem
                                            },
                           Theme = new CategorialTheme
                                       {
                                           AttributeName = "DataType",
                                           ThemeItems = new EventedList<IThemeItem>
                                                            {
                                                                CreateCategorialThemeItem(WaterFlowModel1DLateralDataType.FlowConstant, Resources.QConst),
                                                                CreateCategorialThemeItem(WaterFlowModel1DLateralDataType.FlowTimeSeries, Resources.QBoundary),
                                                                CreateCategorialThemeItem(WaterFlowModel1DLateralDataType.FlowWaterLevelTable, Resources.QHBoundary)
                                                            }
                                       }
                       };
        }

        private static VectorLayer CreateBoundaryNodeDataLayer(IEventedList<WaterFlowModel1DBoundaryNodeData> boundaryNodeDataList, ICoordinateSystem coordinateSystem)
        {
            return new VectorLayer("Boundary Data")
                       {
                           Visible = false,
                           Selectable = true,
                           NameIsReadOnly = true,
                           DataSource = new FeatureCollection
                                            {
                                                FeatureType = typeof(WaterFlowModel1DBoundaryNodeData),
                                                Features = (IList) boundaryNodeDataList,
                                                CoordinateSystem = coordinateSystem
                                            },
                           Theme = new CategorialTheme
                                       {
                                           AttributeName = "DataType",
                                           ThemeItems = new EventedList<IThemeItem>
                                                            {
                                                                CreateCategorialThemeItem(WaterFlowModel1DBoundaryNodeDataType.None, Resources.None),
                                                                CreateCategorialThemeItem(WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant, Resources.HConst),
                                                                CreateCategorialThemeItem(WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries, Resources.HBoundary),
                                                                CreateCategorialThemeItem(WaterFlowModel1DBoundaryNodeDataType.FlowConstant, Resources.QConst),
                                                                CreateCategorialThemeItem(WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries, Resources.QBoundary),
                                                                CreateCategorialThemeItem(WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable, Resources.QHBoundary)
                                                            }
                                       }
                       };
        }

        private static CategorialThemeItem CreateCategorialThemeItem<T>(T enumValue, Image overlayImage) where T : struct, IConvertible
        {
            var value = (Enum) Enum.Parse(typeof (T), enumValue.ToString());

            return new CategorialThemeItem
                       {
                           Value = value,
                           Label = value.GetDescription(),
                           Style = new VectorStyle
                                       {
                                           Symbol = new Bitmap(Resources.Boundary.AddOverlayImage(overlayImage, 1, 1))
                                       }
                       };
        }

        private static GroupLayer CreateReadonlyGroupLayer(string name)
        {
            return new GroupLayer(name)
                       {
                           LayersReadOnly = true,
                           Selectable = false, 
                           NameIsReadOnly = true
                       };
        }
        
        private IModel GetRootModel(IModel model)
        {
            var rootModelForModel = GetRootModelRecursive(model);
            return rootModelForModel == model ? null : rootModelForModel;
        }

        private static IModel GetRootModelRecursive(IModel model)
        {
            var ownerModel = model.Owner as IModel;
            return ownerModel != null
                ? GetRootModelRecursive(ownerModel)
                : model;
        }
    }
}