using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.NetworkEditor.Gui.Commands;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    [Entity]
    public class ManholeVisualisationControlViewModel
    {
        private int shapeNumber = 0;
        private Random rand = new Random();
        public ManholeVisualisationControlViewModel()
        {
            AddShapeCommand = new RelayCommand(AddShape, CanAddShape);
            RemoveShapeCommand = new RelayCommand(RemoveShape, CanRemoveShape);
        }

        private void RemoveShape(object obj)
        {
            RemoveShapeAction?.Invoke();
            shapeNumber--;
        }

        private bool CanRemoveShape(object obj)
        {
            return true;
        }

        private bool CanAddShape(object obj)
        {
            return true;
        }

        private void AddShape(object obj)
        {
            var rect = new Rectangle
            {
                Width = rand.NextDouble() * 100,
                Height = rand.NextDouble() * 100,
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 2,
                Fill = new SolidColorBrush(Colors.Orange),

            };
            AddShapeAction?.Invoke(rect);

            shapeNumber++;
        }

        public Action<Shape> AddShapeAction { get; set; }

        public Action RemoveShapeAction { get; set; }

        public Manhole Manhole { get; set; }

        public ICommand AddShapeCommand { get; set; }

        public ICommand RemoveShapeCommand { get; set; }
    }

    public class CompartmentShape : Shape
    {
        private Compartment compartment;

        private Rect rect;

        private Point topLeft = new Point(0, 0);
        private Point bottomRight = new Point(0, 0);

        public CompartmentShape(Compartment compartment)
        {
            this.compartment = compartment;

            topLeft.X = 0;
            topLeft.Y = compartment.SurfaceLevel;

            bottomRight.X = compartment.ManholeWidth;
            bottomRight.Y = compartment.BottomLevel;
            rect = new Rect(topLeft, bottomRight);

            Width = compartment.ManholeWidth;
            Height = compartment.SurfaceLevel - compartment.BottomLevel;
        }


        private RectangleGeometry geometry = new RectangleGeometry();


        protected override Geometry DefiningGeometry
        {
            get
            {
                geometry.Rect = rect;
                return geometry;
            }
        }
    }
}