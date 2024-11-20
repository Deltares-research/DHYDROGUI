using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Utils.Binding;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;
using DeltaShell.Plugins.SharpMapGis.Gui.Commands.SpatialOperations;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms.SpatialOperations.CommandForms;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api.Layers;
using SharpMap.Api.SpatialOperations;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;
using GisResources = DeltaShell.Plugins.SharpMapGis.Gui.Properties.Resources;
using PointwiseOperationType = DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas.PointwiseOperationType;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Ribbon
{
    public class SetLabelCommand : SpatialOperationCommandBase
    {
        protected override string GetOperationPrefix()
        {
            return "Set Label";
        }

        protected override bool FeatureTypeIsSupported(Type type)
        {
            return base.FeatureTypeIsSupported(type) &&
                   typeof(WaterQualityObservationAreaCoverage).IsAssignableFrom(type);
        }

        protected override bool EnabledSelectedFeatures(
            IEnumerable<IFeature> polygons,
            IEnumerable<IFeature> polyLines,
            IEnumerable<IFeature> points)
        {
            return polygons.Any() && !points.Any() && !polyLines.Any();
        }

        protected override ISpatialOperation CreateSpatialOperation(ILayer targetLayer)
        {
            // create a user input form
            var form = new SpatialOperationInputForm {Text = Resources.SetLabelOperationProperties_DisplayName};

            var labelControl =
                new SpatialOperationPropertyTextBox {LabelText = Resources.SetLabelOperation_Label_DisplayName};
            labelControl.Validating += LabelControlValidating;
            form.AddPropertyPanel(labelControl);

            var operationTypeControl =
                new SpatialOperationPropertyCombobox {LabelText = GisResources.ValueOperation_Operation_DisplayName};
            operationTypeControl.SetDataSource(EnumBindingHelper.ToList<PointwiseOperationType>(), "Key", "Value");
            operationTypeControl.Validating += OperationTypeControlValidating;
            form.AddPropertyPanel(operationTypeControl);

            // show the form to the user.
            if (form.ShowDialog() == DialogResult.Cancel)
            {
                return null;
            }
            else
            {
                // parse the values
                var type = (PointwiseOperationType) operationTypeControl.SelectedItem;

                // create set value operation
                return CreateSetLabelOperation(labelControl.ValueString, type);
            }
        }

        /// <summary>
        /// Create the set value operation with the right parameters and inputs
        /// </summary>
        /// <param name="label"> </param>
        /// <param name="type"> The pointwise operation on the set value operation. </param>
        /// <returns> </returns>
        private ISpatialOperation CreateSetLabelOperation(string label, PointwiseOperationType type)
        {
            // create a spatial operation
            var setValueOperation = new SetLabelOperation
            {
                Label = label,
                OperationType = type
            };
            setValueOperation.SetInputData(SpatialOperation.MaskInputName,
                                           new FeatureCollection(new EventedList<IFeature>(Polygons.Concat(Points)),
                                                                 typeof(Feature2D)) {CoordinateSystem = SourceCoordinateSystem});
            return setValueOperation;
        }

        /// <summary>
        /// Validate the value control that sets the value on the points/coverage.
        /// </summary>
        /// <param name="sender"> The value control. </param>
        /// <param name="cancelEventArgs"> The event to cancel when validation fails. </param>
        private static void LabelControlValidating(object sender, CancelEventArgs cancelEventArgs)
        {
            var control = (SpatialOperationPropertyTextBox) sender;
            string valueString = control.ValueString;
            if (string.IsNullOrWhiteSpace(valueString))
            {
                cancelEventArgs.Cancel = true;
                control.ErrorMessage = "The specified label is empty or white spaces only.";
            }
            else
            {
                control.ErrorMessage = "";
            }
        }

        private static void OperationTypeControlValidating(object sender, CancelEventArgs e)
        {
            var control = (SpatialOperationPropertyCombobox) sender;
            if (control.SelectedItem == null)
            {
                e.Cancel = true;
                control.ErrorMessage = "Select an operation type";
            }
            else
            {
                control.ErrorMessage = "";
            }
        }
    }
}