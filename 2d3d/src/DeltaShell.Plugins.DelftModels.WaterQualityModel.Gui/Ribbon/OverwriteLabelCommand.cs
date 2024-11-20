using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;
using DeltaShell.Plugins.SharpMapGis.Gui.Commands.SpatialOperations;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms.SpatialOperations.CommandForms;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using SharpMap.Api.Layers;
using SharpMap.Api.SpatialOperations;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Ribbon
{
    public class OverwriteLabelCommand : SpatialOperationCommandBase
    {
        private Coordinate clickedCoordinate;

        public override bool Enabled
        {
            get
            {
                var pointTool = MapControl?.GetToolByType<SamplePointTool>();

                if (pointTool != null)
                {
                    return base.Enabled && !pointTool.IsActive;
                }

                return base.Enabled;
            }
        }

        protected override IMapTool MapTool => MapControl?.GetToolByType<QueryTool>();

        protected override void OnExecute(params object[] arguments)
        {
            if (MapTool != null)
            {
                MapControl.ActivateTool(MapTool);
                var queryTool = (QueryTool) MapTool;
                queryTool.OnMouseClick = OnMouseClick;
            }
        }

        protected override string GetOperationPrefix()
        {
            return "Overwrite label";
        }

        protected override ISpatialOperation CreateSpatialOperation(ILayer targetLayer)
        {
            // create a user input form
            var form = new SpatialOperationInputForm {Text = Resources.OverwriteLabelOperationProperties_DisplayName};

            var labelControl =
                new SpatialOperationPropertyTextBox {LabelText = Resources.OverwriteLabelOperation_Label_DisplayName};
            labelControl.Validating += LabelControlValidating;
            form.AddPropertyPanel(labelControl);

            // show the form to the user.
            if (form.ShowDialog() == DialogResult.Cancel)
            {
                return null;
            }
            else
            {
                return new OverwriteLabelOperation
                {
                    Label = labelControl.ValueString,
                    X = clickedCoordinate.X,
                    Y = clickedCoordinate.Y,
                    InputCoordinateSystem = Map.CoordinateSystem
                };
            }
        }

        protected override bool FeatureTypeIsSupported(Type type)
        {
            return base.FeatureTypeIsSupported(type) && type == typeof(WaterQualityObservationAreaCoverage);
        }

        protected override bool EnabledSelectedFeatures(IEnumerable<IFeature> polygons, IEnumerable<IFeature> polyLines,
                                                        IEnumerable<IFeature> points)
        {
            return base.EnabledSelectedFeatures(polygons, polyLines, points) && !polygons.Any() && !polyLines.Any() &&
                   !points.Any();
        }

        private void OnMouseClick(Coordinate coordinate, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                clickedCoordinate = coordinate;
                base.OnExecute();
                var queryTool = (QueryTool) MapTool;
                queryTool.OnMouseClick = null;
            }
        }

        /// <summary>
        /// Validate the value control that sets the value on the points/coverage.
        /// </summary>
        /// <param name="sender"> The value control. </param>
        /// <param name="cancelEventArgs"> The event to cancel when validation fails. </param>
        private static void LabelControlValidating(object sender, CancelEventArgs cancelEventArgs)
        {
            var control = (SpatialOperationPropertyTextBox) sender;
            string labelString = control.ValueString;
            if (string.IsNullOrWhiteSpace(labelString))
            {
                cancelEventArgs.Cancel = true;
                control.ErrorMessage = "The specified label was empty or whitespaces only.";
            }
            else
            {
                control.ErrorMessage = "";
            }
        }
    }
}