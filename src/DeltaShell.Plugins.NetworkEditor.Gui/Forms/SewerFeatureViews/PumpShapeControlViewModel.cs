using System;
using System.Windows;
using System.Windows.Media;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    [Entity]
    public class PumpShapeControlViewModel
    {
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

        public double BaseStrokeThickness { get; set; } = 0.01;
        
        public double CenterLeftOffset { get; set; }
        public double CenterTopOffset { get; set; }
        public double CenterHeight { get; set; }
        public double CenterWidth { get; set; }

        public PointCollection SuctionStartLevel { get; set; }
        public PointCollection SuctionStopLevel { get; set; }
        public PointCollection SuctionConnection { get; set; }

        public PointCollection DeliveryStartLevel { get; set; }
        public PointCollection DeliveryStopLevel { get; set; }
        public PointCollection DeliveryConnection { get; set; }


        private double levelAtTop;

        private void Update()
        {
            var maxSuction = Math.Max(pumpShape.StartSuctionLevel, pumpShape.StopSuctionLevel);
            var maxDelivery = Math.Max(pumpShape.StartDeliveryLevel, pumpShape.StopDeliveryLevel);
            
            levelAtTop = Math.Max(maxSuction, maxDelivery);

            var suctionLeft = 0;
            var suctionRight = pumpShape.Width / 6.0;
            var suctionStart = ConvertToOffsetFromTop(pumpShape.StartSuctionLevel);
            var suctionStop = ConvertToOffsetFromTop(pumpShape.StopSuctionLevel);
            var suctionMid = (suctionStart + suctionStop) / 2.0;

            SuctionStartLevel = new PointCollection
            {
                new Point(suctionLeft, suctionStart),
                new Point(suctionRight, suctionStart),
                new Point(suctionRight, suctionMid),
            };

            SuctionStopLevel = new PointCollection
            {
                new Point(suctionLeft, suctionStop),
                new Point(suctionRight, suctionStop),
                new Point(suctionRight, suctionMid),
            };

            var deliveryLeft = 5.0 * pumpShape.Width / 6.0;
            var deliveryRight = pumpShape.Width;
            var deliveryStart = ConvertToOffsetFromTop(pumpShape.StartDeliveryLevel);
            var deliveryStop = ConvertToOffsetFromTop(pumpShape.StopDeliveryLevel);
            var deliveryMid = (deliveryStart + deliveryStop) / 2.0;
            
            DeliveryStartLevel = new PointCollection
            {
                new Point(deliveryRight, deliveryStart),
                new Point(deliveryLeft, deliveryStart),
                new Point(deliveryLeft, deliveryMid),
            };

            DeliveryStopLevel = new PointCollection
            {
                new Point(deliveryRight, deliveryStop),
                new Point(deliveryLeft, deliveryStop),
                new Point(deliveryLeft, deliveryMid),
            };

            var centerX = pumpShape.Width / 2.0;
            var centerY = (suctionMid + deliveryMid) / 2.0;
            
            CenterHeight = pumpShape.Width / 5.0;
            CenterWidth = CenterHeight;

            CenterLeftOffset = centerX - 0.5 * CenterWidth;
            CenterTopOffset = centerY - 0.5 * CenterHeight;

            DeliveryConnection = new PointCollection
            {
                new Point(deliveryLeft, deliveryMid),
                new Point((deliveryLeft + centerX) * 0.5, deliveryMid),
                new Point((deliveryLeft + centerX) * 0.5, centerY),
                new Point(centerX, centerY),
            };

            SuctionConnection = new PointCollection
            {
                new Point(suctionRight, suctionMid),
                new Point((suctionRight + centerX) * 0.5, suctionMid),
                new Point((suctionRight + centerX) * 0.5, centerY),
                new Point(centerX, centerY),
            };
        }

        private double ConvertToOffsetFromTop(double y)
        {
            return levelAtTop - y;
        }
    }
}