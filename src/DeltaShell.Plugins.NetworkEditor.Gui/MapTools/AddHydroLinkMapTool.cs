using System;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro;
using DeltaShell.NGHS.Common.Gui.Modals.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Geometries;
using SharpMap.Api.Layers;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Extensions.CoordinateSystems;
using SharpMap.UI.Helpers;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.MapTools
{
    /// <summary>
    /// Map tool for adding a new <see cref="HydroLink"/>.
    /// </summary>
    public sealed class AddHydroLinkMapTool : NewArrowLineTool
    {
        /// <summary>
        /// Enum for requesting the user input when linking a <see cref="HydroLink"/> to a <see cref="LateralSource"/>.
        /// </summary>
        private enum UserInput
        {
            /// <summary>
            /// The user continues.
            /// </summary>
            Continue,

            /// <summary>
            /// The user cancels.
            /// </summary>
            Cancel
        }

        /// <summary>
        /// The name of the tool.
        /// </summary>
        public const string ToolName = "add hydro link";
        
        private static readonly IRequestUserInputService<UserInput> userInputService = new RequestUserInputService<UserInput>();
        private static readonly Cursor cursor = MapCursors.CreateArrowOverlayCuror(Resources.Link);

        /// <summary>
        /// Initializes a new instance of the <see cref="AddHydroLinkMapTool"/> class.
        /// </summary>
        /// <param name="layerFilter"> The layer filter. </param>
        public AddHydroLinkMapTool(Func<ILayer, bool> layerFilter) : base(layerFilter, ToolName)
        {
            AddNewFeature = (g, cs, sourecSr, targetSr, tool) =>
            {
                if (sourecSr.SnappedFeature is Catchment && targetSr.SnappedFeature is LateralSource)
                {
                    UserInput? result = userInputService.RequestUserInput(
                        Resources.HydroRegionEditorMapTool_Overwriting_existing_later_source_flow_data,
                        Resources.HydroRegionEditorMapTool_Connecting_hydro_link_removes_existing_data + Environment.NewLine +
                        Resources.HydroRegionEditorMapTool_Do_you_want_to_continue);

                    if (result != UserInput.Continue)
                    {
                        return;
                    }
                }

                // Find the correct link layer to add to
                IHydroRegion region = HydroRegion.GetCommonRegion((IHydroObject) sourecSr.SnappedFeature, (IHydroObject) targetSr.SnappedFeature);
                ILayer layer = tool.Layers.FirstOrDefault(l => Equals(l.DataSource.Features, region.Links));
                if (layer == null)
                {
                    return;
                }

                layer.DataSource.Add(GetLocalGeometry(g, cs, layer.CoordinateSystem));
                layer.RenderRequired = true;
            };
            Cursor = cursor;
        }

        private static IGeometry GetLocalGeometry(IGeometry geometry, ICoordinateSystem sourceCoordinateSystem, ICoordinateSystem targetCoordinateSystem)
        {
            if (sourceCoordinateSystem == null || targetCoordinateSystem == null ||
                sourceCoordinateSystem == targetCoordinateSystem)
            {
                return geometry;
            }

            var coordinateSystemFactory = new OgrCoordinateSystemFactory();
            ICoordinateTransformation transformation = coordinateSystemFactory.CreateTransformation(sourceCoordinateSystem, targetCoordinateSystem);

            return GeometryTransform.TransformGeometry(geometry, transformation.MathTransform);
        }
    }
}