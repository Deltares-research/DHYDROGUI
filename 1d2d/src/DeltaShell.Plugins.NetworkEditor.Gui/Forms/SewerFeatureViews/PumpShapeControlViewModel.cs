using System;
using System.Windows;
using System.Windows.Media;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    [Entity]
    public class PumpShapeControlViewModel
    {
        private double minX;
        private double maxX;
        private double minY;
        private double maxY;

        private PumpShape pumpShape;

        public PumpShape PumpShape
        {
            get { return pumpShape; }
            set
            {
                pumpShape = value;
                if (pumpShape != null)
                {
                    Update();
                }
            }
        }

        public PointCollection SuctionStartLevel { get; set; }
        public PointCollection SuctionStopLevel { get; set; }
        public PointCollection SuctionConnection { get; set; }

        public PointCollection DeliveryStartLevel { get; set; }
        public PointCollection DeliveryStopLevel { get; set; }
        public PointCollection Connection { get; set; }

        public Func<double> GetActualWidth { get; set; }
        public Func<double> GetActualHeight { get; set; }

        public double IconLeftOffset { get; set; }
        public double IconTopOffset { get; set; }
        
        public void Update()
        {
            SetRanges();
            
            double suctionMid;
            var suctionRight = UpdateSuctionSide(out suctionMid);

            double deliveryMid;
            var deliveryLeft = UpdateDeliverySide(out deliveryMid);

            var centerX = pumpShape.Width / 2.0;
            var centerY = (suctionMid + deliveryMid) / 2.0;
            
            SetPumpIconProperties(centerX, centerY);

            Connection = new PointCollection
            {
                new Point(ScaleX(suctionRight), ScaleY(suctionMid)),
                new Point(ScaleX((suctionRight + centerX) * 0.5), ScaleY(suctionMid)),
                new Point(ScaleX((suctionRight + centerX) * 0.5), ScaleY(centerY)),

                new Point(ScaleX((deliveryLeft + centerX) * 0.5), ScaleY(centerY)),
                new Point(ScaleX((deliveryLeft + centerX) * 0.5), ScaleY(deliveryMid)),
                new Point(ScaleX(deliveryLeft), ScaleY(deliveryMid)),

            };
        }

        private void SetPumpIconProperties(double x, double y)
        {
            IconLeftOffset = ScaleX(x) - 8;
            IconTopOffset = ScaleY(y) - 8; 
        }

        private double UpdateDeliverySide(out double deliveryMid)
        {
            var deliveryLeft = 5.0 * pumpShape.Width / 6.0;
            var deliveryRight = pumpShape.Width;
            var deliveryStart = pumpShape.StartDeliveryLevel;
            var deliveryStop = pumpShape.StopDeliveryLevel;
            deliveryMid = (deliveryStart + deliveryStop) / 2.0;

            DeliveryStartLevel = new PointCollection
            {
                new Point(ScaleX(deliveryRight), ScaleY(deliveryStart)),
                new Point(ScaleX(deliveryLeft), ScaleY(deliveryStart)),
                new Point(ScaleX(deliveryLeft), ScaleY(deliveryMid)),
            };

            DeliveryStopLevel = new PointCollection
            {
                new Point(ScaleX(deliveryRight), ScaleY(deliveryStop)),
                new Point(ScaleX(deliveryLeft), ScaleY(deliveryStop)),
                new Point(ScaleX(deliveryLeft), ScaleY(deliveryMid)),
            };
            return deliveryLeft;
        }

        private double UpdateSuctionSide(out double suctionMid)
        {
            var suctionLeft = 0;
            var suctionRight = pumpShape.Width / 6.0;
            var suctionStart = pumpShape.StartSuctionLevel;
            var suctionStop = pumpShape.StopSuctionLevel;
            suctionMid = (suctionStart + suctionStop) / 2.0;

            SuctionStartLevel = new PointCollection
            {
                new Point(ScaleX(suctionLeft), ScaleY(suctionStart)),
                new Point(ScaleX(suctionRight), ScaleY(suctionStart)),
                new Point(ScaleX(suctionRight), ScaleY(suctionMid)),
            };

            SuctionStopLevel = new PointCollection
            {
                new Point(ScaleX(suctionLeft), ScaleY(suctionStop)),
                new Point(ScaleX(suctionRight), ScaleY(suctionStop)),
                new Point(ScaleX(suctionRight), ScaleY(suctionMid)),
            };
            
            return suctionRight;
        }

        private void SetRanges()
        {
            var minSuction = Math.Min(pumpShape.StartSuctionLevel, pumpShape.StopSuctionLevel);
            var minDelivery = Math.Min(pumpShape.StartDeliveryLevel, pumpShape.StopDeliveryLevel);
            minY = Math.Min(minSuction, minDelivery);

            var maxSuction = Math.Max(pumpShape.StartSuctionLevel, pumpShape.StopSuctionLevel);
            var maxDelivery = Math.Max(pumpShape.StartDeliveryLevel, pumpShape.StopDeliveryLevel);

            maxY = Math.Max(maxSuction, maxDelivery);
            minX = 0;
            maxX = pumpShape.Width;

        }

        private double ScaleX(double x)
        {
            return GetActualWidth == null ? 0 : CoordinateScalingHelper.ScaleX(x, minX, maxX, GetActualWidth.Invoke());
        }

        private double ScaleY(double y)
        {
            return GetActualHeight == null ? 0 : CoordinateScalingHelper.ScaleY(y, minY, maxY, GetActualHeight.Invoke());
        }
    }
}