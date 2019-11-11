using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    public class PipeShapeDragAndDropStrategy : IDragAndDropStrategy
    {
        private PipeShape pipeShape;

        private CompartmentShape newCompartmentShape;

        public bool FindNewPosition(Canvas canvas, ContentPresenter contentPresenter, double leftOffset, double originalLeft)
        {
            pipeShape = contentPresenter.Content as PipeShape;
            var compartmentShapes = GetContentPresentersByContentType<CompartmentShape>(canvas);
            var newLeftOffset = leftOffset + originalLeft + 0.5 * contentPresenter.ActualWidth;
            var tuple = compartmentShapes.Select(cp => new KeyValuePair<ContentPresenter, double>(cp, GetElementMiddle(cp))).ToList();
            var closestCompartment = tuple.OrderBy(t => Math.Abs(t.Value - newLeftOffset)).FirstOrDefault().Key;

            newCompartmentShape = (CompartmentShape) closestCompartment.Content;
            return true;
        }

        public bool Validate()
        {
            return newCompartmentShape != null;
        }

        public void Reposition()
        {
            if (pipeShape?.Pipe == null) return;

            var newCompartment = GetCompartmentFromCompartmentShape(newCompartmentShape);
            pipeShape.ConnectedCompartmentShape = newCompartmentShape;
            var manholeIsPipeSource = (Manhole)pipeShape.Pipe.Source == newCompartment.ParentManhole;
            if (manholeIsPipeSource)
            {
                pipeShape.Pipe.SourceCompartment = newCompartment;
            }
            else
            {
                pipeShape.Pipe.TargetCompartment = newCompartment;
            }

            pipeShape = null;
            newCompartmentShape = null;
        }

        private ICompartment GetCompartmentFromCompartmentShape(CompartmentShape compartmentShape)
        {
            return compartmentShape?.Compartment;
        }

        protected virtual IEnumerable<ContentPresenter> GetContentPresentersByContentType<T>(Canvas canvas) where T : IDrawingShape
        {
            return canvas.Children.OfType<ContentPresenter>().Where(cp => cp.Content is T);
        }

        protected virtual double GetElementMiddle(FrameworkElement element)
        {
            var x = Canvas.GetLeft(element) + 0.5 * element.ActualWidth;
            return x;
        }
    }
}