using System.Drawing;
using DelftTools.Shell.Core.Workflow;
using Netron.GraphLib.Interfaces;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.ActivityShapes
{
    public static class ShapeFactory
    {
        /// <summary>
        /// Creates a shape for <see cref="Netron.GraphLib.UI.GraphControl"/> for
        /// the given activity.
        /// </summary>
        /// <param name="activity">Activity to create a shape for.</param>
        /// <param name="graphControl">
        /// The <see cref="Netron.GraphLib.UI.GraphControl"/> for which
        /// the shape is going to be used.
        /// </param>
        /// <returns>An activity shape.</returns>
        public static ActivityShapeBase CreateShapeFromActivity(IActivity activity, IGraphSite graphControl)
        {
            var activityWrapper = activity as ActivityWrapper;
            activity = activityWrapper == null ? activity : activityWrapper.Activity;

            // Check from most specific to less specific:
            var parallelActivity = activity as ParallelActivity;
            if (parallelActivity != null)
            {
                var parallelActivityShape = new ParallelActivityShape(graphControl) {Activity = activity};
                parallelActivityShape.ShapeColor = CreateDefaultColorFromActivityShape(parallelActivityShape);
                parallelActivityShape.Inflate();
                return parallelActivityShape;
            }

            var sequentialActivity = activity as SequentialActivity;
            if (sequentialActivity != null)
            {
                var sequentialActivityShape = new SequentialActivityShape(graphControl) {Activity = activity};
                sequentialActivityShape.ShapeColor = CreateDefaultColorFromActivityShape(sequentialActivityShape);
                sequentialActivityShape.Inflate();
                return sequentialActivityShape;
            }

            var compositeActivity = activity as ICompositeActivity;
            if (compositeActivity != null)
            {
                var compositeActivityShape = new CompositeActivityShape(graphControl) {Activity = activity};
                compositeActivityShape.ShapeColor = CreateDefaultColorFromActivityShape(compositeActivityShape);
                compositeActivityShape.Inflate();
                return compositeActivityShape;
            }

            var simpleActivityShape = new SimpleActivityShape(graphControl) {Activity = activity};
            simpleActivityShape.ShapeColor = CreateDefaultColorFromActivityShape(simpleActivityShape);
            simpleActivityShape.Inflate();
            return simpleActivityShape;
        }

        /// <summary>
        /// Creates the default color for a shape.
        /// </summary>
        /// <param name="shape">The shape for which the default color is to be generated.</param>
        /// <returns>The default color for an activity shape.</returns>
        public static Color CreateDefaultColorFromActivityShape(ActivityShapeBase shape)
        {
            // Check from most specific to less specific:
            var parallelActivityShape = shape as ParallelActivityShape;
            if (parallelActivityShape != null)
            {
                return Color.SandyBrown;
            }

            var sequentialActivityShape = shape as SequentialActivityShape;
            if (sequentialActivityShape != null)
            {
                return Color.LightSkyBlue;
            }

            var compositeActivityShape = shape as CompositeActivityShape;
            if (compositeActivityShape != null)
            {
                return Color.GreenYellow;
            }

            return Color.LightGray;
        }
    }
}