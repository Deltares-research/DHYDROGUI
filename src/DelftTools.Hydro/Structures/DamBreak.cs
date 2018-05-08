using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;

namespace DelftTools.Hydro.Structures
{
    public class DamBreak: GroupableFeature2D
    {
        private double breachLocationX;
        private double breachLocationY;
        private bool isLocationSet = false;

        public override IGeometry Geometry
        {
            get { return base.Geometry; }
            set
            {
                base.Geometry = value;

                if (!isLocationSet)
                {
                    SetDefaultBreachLocation();
                }
            }
        }

        public double BreachLocationX
        {
            get { return breachLocationX; }
            set
            {
                breachLocationX = value;
                isLocationSet = true;
            }
        }

        public double BreachLocationY
        {
            get { return breachLocationY; }
            set
            {
                breachLocationY = value;
                isLocationSet = true;
            }
        }

        public IPoint BreachLocation
        {
            get
            {
                return new Point(BreachLocationX, BreachLocationY);
            }
        }


        private void SetDefaultBreachLocation()
        {
            if (Geometry is ILineString)
            {
                var line = Geometry as ILineString;
                var lengthIndexedLine = new LengthIndexedLine(line);

                var offset = line.Length / 2.0;
                var point = new Point(lengthIndexedLine.ExtractPoint(offset));
                breachLocationX = point.X;
                breachLocationY = point.Y;
            }
        }
    }
}
