using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Drawing;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using SharpMap.Api.Layers;
using SharpMap.Layers;
using SharpMap.Rendering.Thematics;
using SharpMap.Styles;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui
{
    public class RealTimeControlMapLayerProvider : IMapLayerProvider
    {
        public ILayer CreateLayer(object data, object parentData)
        {
            var realTimeControlModel = data as RealTimeControlModel;
            if (realTimeControlModel != null)
            {
                return new GroupLayer(realTimeControlModel.Name)
                {
                    LayersReadOnly = true,
                    NameIsReadOnly = true
                };
            }

            if (data is ModelFolder modelFolder && modelFolder.Model is RealTimeControlModel)
            {
                return new GroupLayer(modelFolder.Role == DataItemRole.Input ? "Input" : "Output")
                {
                    LayersReadOnly = true,
                    NameIsReadOnly = true
                };
            }

            if (data is IEventedList<ControlGroup> controlGroups)
            {
                var model = parentData as RealTimeControlModel;

                var layers = new[]
                {
                    new VectorLayer
                    {
                        Name = "Inputs/Outputs",
                        NameIsReadOnly = true,
                        Visible = false,
                        ReadOnly = true,
                        DataSource = new ControlGroupFeatureCollection(controlGroups) {EditableObjectRefresh = model},
                        Theme = new CategorialTheme
                        {
                            AttributeName = "ConnectionType",
                            ThemeItems =
                            {
                                CreateEnumCategorialThemeItem(ConnectionType.Input, Resources.input),
                                CreateEnumCategorialThemeItem(ConnectionType.Output, Resources.output)
                            }
                        }
                    },
                    new VectorLayer
                    {
                        Name = "Connections",
                        NameIsReadOnly = true,
                        Visible = false,
                        ReadOnly = true,
                        DataSource = new ControlGroupFeatureCollection(controlGroups)
                        {
                            UseConnections = true,
                            EditableObjectRefresh = model
                        },
                        Style = new VectorStyle
                        {
                            GeometryType = typeof(ILineString),
                            Line = new Pen(new SolidBrush(ColorHelper.GetIndexedColor(100, 0)), 8) {EndCap = LineCap.ArrowAnchor}
                        }
                    }
                };

                var controlGroupLayer = new GroupLayer("Control groups");
                controlGroupLayer.Layers.AddRange(layers);
                controlGroupLayer.LayersReadOnly = true;
                controlGroupLayer.NameIsReadOnly = true;

                return controlGroupLayer;
            }

            return null;
        }

        public bool CanCreateLayerFor(object data, object parentData)
        {
            return data is RealTimeControlModel
                   || (data as ModelFolder)?.Model is RealTimeControlModel
                   || data is IEventedList<ControlGroup>;
        }

        public IEnumerable<object> ChildLayerObjects(object data)
        {
            if (data is RealTimeControlModel realTimeControlModel)
            {
                yield return realTimeControlModel.ControlGroups;
                yield return new ModelFolder
                {
                    Model = realTimeControlModel,
                    Role = DataItemRole.Output
                };
            }

            if (!(data is ModelFolder modelFolder) 
                || !(modelFolder.Model is RealTimeControlModel rtcModel) 
                || modelFolder.Role != DataItemRole.Output)
            {
                yield break;
            }

            foreach (IFeatureCoverage outputFeatureCoverage in rtcModel.OutputFeatureCoverages)
            {
                yield return outputFeatureCoverage;
            }
        }

        public void AfterCreate(ILayer layer, object layerObject, object parentObject, IDictionary<ILayer, object> objectsLookup)
        {
            // Nothing needs to be done after creation
        }

        private static CategorialThemeItem CreateEnumCategorialThemeItem<T>(T enumValue, Image image) where T : struct, IConvertible
        {
            var value = (Enum) Enum.Parse(typeof(T), enumValue.ToString());

            return new CategorialThemeItem
            {
                Value = value,
                Label = value.GetDescription(),
                Style = new VectorStyle {Symbol = new Bitmap(image)}
            };
        }
    }
}