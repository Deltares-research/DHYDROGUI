using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RTCShapes;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms
{
    /// <summary>
    /// Provides shape-related operations control groups.
    /// </summary>
    public sealed class ControlGroupShapeController : IShapeAccessor, IShapeSetter
    {
        private readonly IGuiContextManager guiContextManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlGroupShapeController"/> class.
        /// </summary>
        /// <param name="guiContextManager">Manages the view contexts.</param>
        /// <exception cref="System.ArgumentNullException">When <paramref name="guiContextManager"/> is <c>null</c>.</exception>
        public ControlGroupShapeController(IGuiContextManager guiContextManager)
        {
            Ensure.NotNull(guiContextManager, nameof(guiContextManager));

            this.guiContextManager = guiContextManager;
        }

        /// <inheritdoc/>
        public IEnumerable<ShapeBase> GetShapes(ControlGroup controlGroup)
        {
            Ensure.NotNull(controlGroup, nameof(controlGroup));

            ControlGroupEditorViewContext viewContext = GetViewContext(controlGroup);
            return viewContext?.ShapeList ?? Enumerable.Empty<ShapeBase>();
        }

        /// <inheritdoc/>
        public void SetShapes(ControlGroup controlGroup, IEnumerable<ShapeBase> shapes)
        {
            Ensure.NotNull(controlGroup, nameof(controlGroup));
            Ensure.NotNull(shapes, nameof(shapes));

            ControlGroupEditorViewContext viewContext = GetOrAddViewContext(controlGroup);
            viewContext.ShapeList = shapes.ToList();
        }

        private ControlGroupEditorViewContext GetOrAddViewContext(ControlGroup controlGroup)
        {
            ControlGroupEditorViewContext viewContext = GetViewContext(controlGroup);

            if (viewContext != null)
            {
                return viewContext;
            }

            viewContext = new ControlGroupEditorViewContext { ControlGroup = controlGroup };
            guiContextManager.AddViewContext(viewContext);

            return viewContext;
        }

        private ControlGroupEditorViewContext GetViewContext(ControlGroup controlGroup)
        {
            return guiContextManager.ProjectViewContexts
                                    .OfType<ControlGroupEditorViewContext>()
                                    .FirstOrDefault(x => x.ControlGroup == controlGroup);
        }
    }
}