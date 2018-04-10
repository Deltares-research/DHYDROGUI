using System;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
/*using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;*/

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    [Entity]
    public class PipeViewModel
    {
        private Pipe pipe;

        public Pipe Pipe
        {
            get { return pipe; }
            set
            {
                pipe = value;
                /*PlotModel = CreatePlotModel(pipe);*/
            }
        }
        
        /*public PlotModel PlotModel { get; set; }

        private static PlotModel CreatePlotModel(IPipe pipe)
        {
            if (pipe == null) return null;

            var plotModel = new PlotModel
            {
                Title = "This is a test",
            };

            var xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom
            };

            plotModel.Axes.Add(xAxis);

            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left
            };

            plotModel.Axes.Add(yAxis);


            var pipeSeries = new LineSeries();

            var dx = GetPipeDeltaX(pipe);
            pipeSeries.Points.Add(new DataPoint(0, pipe.LevelSource));
            pipeSeries.Points.Add(new DataPoint(dx, pipe.LevelTarget));
            plotModel.Series.Add(pipeSeries);

            return plotModel;
        }*/

        private static double GetPipeDeltaX(IPipe pipe)
        {
            var length = pipe.Length;
            var dy = pipe.LevelTarget - pipe.LevelSource;

            var dx = Math.Sqrt(Math.Pow(length, 2) - Math.Pow(dy, 2));
            return dx;

        }

    }
}