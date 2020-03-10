using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Drawing;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.DataObjects;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Geometries;
using log4net;
using SharpMap.Api;
using SharpMap.Api.Layers;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.Rendering.Thematics;
using SharpMap.Styles;

namespace DeltaShell.Plugins.FMSuite.Common.Gui
{
    public class FMSuiteCommonGuiMapLayerProvider : IMapLayerProvider
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FMSuiteCommonGuiMapLayerProvider));

        public ILayer CreateLayer(object data, object parent)
        {
            var boundaryNodeData = data as IEventedList<Model1DBoundaryNodeData>;
            var coordinateSystem = (parent as IHasCoordinateSystem)?.CoordinateSystem;
            if (coordinateSystem == null)
            {
                if (parent is ModelFolder)
                {
                    var networkModel = ((ModelFolder) parent) as IModelWithNetwork;
                    coordinateSystem = networkModel?.Network?.CoordinateSystem;
                } 
            }
            if (boundaryNodeData != null)
            {
                return CreateBoundaryNodeDataLayer(boundaryNodeData, coordinateSystem);
            }
            var lateralSourceData = data as IEventedList<Model1DLateralSourceData>;
            if (lateralSourceData != null)
            {
                return CreateLateralDataLayer(lateralSourceData, coordinateSystem);
            }
            return null;
        }
        private static VectorLayer CreateBoundaryNodeDataLayer(IEventedList<Model1DBoundaryNodeData> boundaryNodeDataList, ICoordinateSystem coordinateSystem)
        {
            return new VectorLayer("Boundary Data 1D")
            {
                Visible = false,
                Selectable = true,
                NameIsReadOnly = true,
                DataSource = new FeatureCollection
                {
                    FeatureType = typeof(Model1DBoundaryNodeData),
                    Features = (IList)boundaryNodeDataList,
                    CoordinateSystem = coordinateSystem
                },
                Theme = new CategorialTheme
                {
                    
                    AttributeName = nameof(Model1DBoundaryNodeData.DataType),
                    DefaultStyle = new VectorStyle
                    {
                        GeometryType = typeof(IPoint),
                        Fill = new SolidBrush(Color.Transparent),
                        EnableOutline = false
                    }
                    ,
                    NoDataValues = new List<string> { "" },
                    ThemeItems = new EventedList<IThemeItem>
                    {
                        CreateCategorialThemeItem(Model1DBoundaryNodeDataType.WaterLevelConstant, Properties.Resources.HConst),
                        CreateCategorialThemeItem(Model1DBoundaryNodeDataType.WaterLevelTimeSeries, Properties.Resources.HBoundary),
                        CreateCategorialThemeItem(Model1DBoundaryNodeDataType.FlowConstant, Properties.Resources.QConst),
                        CreateCategorialThemeItem(Model1DBoundaryNodeDataType.FlowTimeSeries, Properties.Resources.QBoundary),
                        CreateCategorialThemeItem(Model1DBoundaryNodeDataType.FlowWaterLevelTable, Properties.Resources.QHBoundary)
                    }
                }
            };
        }

        private static VectorLayer CreateLateralDataLayer(IEventedList<Model1DLateralSourceData> lateralSourceDataList, ICoordinateSystem coordinateSystem)
        {
            return new VectorLayer("Lateral Data 1D")
            {
                Visible = false,
                Selectable = true,
                NameIsReadOnly = true,
                DataSource = new FeatureCollection
                {
                    FeatureType = typeof(Model1DLateralSourceData),
                    Features = (IList)lateralSourceDataList,
                    CoordinateSystem = coordinateSystem
                },
                Theme = new CategorialTheme
                {
                    AttributeName = nameof(Model1DLateralSourceData.DataType),
                    DefaultStyle = new VectorStyle(),
                    NoDataValues = new List<string> { "" },
                    ThemeItems = new EventedList<IThemeItem>
                    {
                        CreateCategorialThemeItem(Model1DLateralDataType.FlowConstant, Properties.Resources.QConst),
                        CreateCategorialThemeItem(Model1DLateralDataType.FlowTimeSeries, Properties.Resources.QBoundary),
                        CreateCategorialThemeItem(Model1DLateralDataType.FlowWaterLevelTable, Properties.Resources.QHBoundary),
                        CreateCategorialThemeItem(Model1DLateralDataType.FlowRealTime, Properties.Resources.realtime)
                    }
                }
            };
        }
       
        private static CategorialThemeItem CreateCategorialThemeItem<T>(T enumValue, Image overlayImage) where T : struct, IConvertible
        {
            var value = (Enum)Enum.Parse(typeof(T), enumValue.ToString());

            return new CategorialThemeItem
            {
                Value = value,
                Label = value.GetDescription(),
                Style = new VectorStyle
                {
                    Symbol = new Bitmap(Properties.Resources.Boundary_1d.AddOverlayImage(overlayImage, 1, 1))
                },

            };
        }
        public bool CanCreateLayerFor(object data, object parentObject)
        {
            return data is IEventedList<Model1DBoundaryNodeData>
                   || data is IEventedList<Model1DLateralSourceData>;

        }

        public IEnumerable<object> ChildLayerObjects(object data)
        {
           yield break;
        }
    }
}