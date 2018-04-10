using System;
using System.Windows;
using System.Windows.Controls;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    /// <summary>
    /// Interaction logic for PumpShapeControl.xaml
    /// </summary>
    public partial class PumpShapeControl : UserControl
    {
        public static readonly DependencyProperty PumpShapeProperty = DependencyProperty.Register(nameof(PumpShape), typeof(PumpShape), typeof(PumpShapeControl), new PropertyMetadata(default(PumpShape), PropertyChangedCallback));
        public static readonly DependencyProperty BaseStrokeThicknessProperty = DependencyProperty.Register(nameof(BaseStrokeThickness), typeof(double), typeof(PumpShapeControl), new PropertyMetadata(default(double), PropertyChangedCallback));

        public double BaseStrokeThickness
        {
            get { return (double) GetValue(BaseStrokeThicknessProperty); }
            set { SetValue(BaseStrokeThicknessProperty, value); }
        }
        public PumpShape PumpShape
        {
            get { return (PumpShape) GetValue(PumpShapeProperty); }
            set { SetValue(PumpShapeProperty, value); }
        }

        public PumpShapeControl()
        {
            InitializeComponent();
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var view = dependencyObject as PumpShapeControl;
            if (view == null) return;

            var pumpShape = dependencyPropertyChangedEventArgs.NewValue as PumpShape;
            if (pumpShape != null)
            {
                view.ViewModel.PumpShape = pumpShape;
            }
            
            if (dependencyPropertyChangedEventArgs.NewValue is double)
            {
                view.ViewModel.BaseStrokeThickness = (double)dependencyPropertyChangedEventArgs.NewValue;
            }
        }
    }
}
