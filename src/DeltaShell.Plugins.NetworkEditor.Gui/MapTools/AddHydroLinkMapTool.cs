using System;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Gui.Modals.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Geometries;
using SharpMap.Api.Editors;
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
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="layerFilter"/> is <c>null</c>.
        /// </exception>
        public AddHydroLinkMapTool(Func<ILayer, bool> layerFilter) : base(layerFilter, ToolName)
        {
            Ensure.NotNull(layerFilter, nameof(layerFilter));

            AddNewFeature = AddNewHydroLink;
            Cursor = cursor;
        }

        private static void AddNewHydroLink(IGeometry geometry, ICoordinateSystem coordinateSystem, SnapResult snappedSource, SnapResult snappedTarget, NewArrowLineTool tool)
        {
            if (snappedSource.SnappedFeature is Catchment && snappedTarget.SnappedFeature is LateralSource)
            {
                if (UserCancels())
                {
                    return;
                }
            }

            if (!TryGetLinksLayer(snappedSource, snappedTarget, tool, out ILayer layer))
            {
                return;
            }

            IGeometry transformedGeometry = GetTransformedGeometry(geometry, coordinateSystem, layer.CoordinateSystem);
            layer.DataSource.Add(transformedGeometry);
            layer.RenderRequired = true;
        }

        private static bool TryGetLinksLayer(SnapResult snappedSource, SnapResult snappedTarget, IMapTool tool, out ILayer layer)
        {
            IHydroRegion region = HydroRegion.GetCommonRegion((IHydroObject) snappedSource.SnappedFeature, (IHydroObject) snappedTarget.SnappedFeature);
            layer = tool.Layers.FirstOrDefault(l => Equals(l.DataSource.Features, region.Links));
            return layer != null;
        }

        private static bool UserCancels()
        {
            UserInput? userInput = userInputService.RequestUserInput(
                Resources.HydroRegionEditorMapTool_Overwriting_existing_later_source_flow_data,
                Resources.HydroRegionEditorMapTool_Connecting_hydro_link_removes_existing_data + Environment.NewLine +
                Resources.HydroRegionEditorMapTool_Do_you_want_to_continue);

            return userInput != UserInput.Continue;
        }

        private static IGeometry GetTransformedGeometry(IGeometry geometry, ICoordinateSystem sourceCoordinateSystem, ICoordinateSystem targetCoordinateSystem)
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