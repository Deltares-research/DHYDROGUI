using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Aop;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;

namespace DelftTools.Hydro.Structures
{
    [Entity]
    public class LeveeBreach : GroupableFeature2D
    {
        private double breachLocationX;
        private double breachLocationY;
        private bool isLocationSet = false;
        private readonly IEnumerable<LeveeBreachSettings> leveeBreachSettings;

        public LeveeBreach()
        {
            leveeBreachSettings = new List<LeveeBreachSettings>
            {
                new LeveeBreachSettingsVdKnaap2000(),
                new LeveeBreachSettingsVerheijVdKnaap2002()
            };
        }

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

        public LeveeBreachGrowthFormula LeveeBreachFormula { get; set; } = LeveeBreachGrowthFormula.VdKnaap2000;

        public LeveeBreachSettings GetLeveeBreachSettings()
        {
            return GetLeveeBreachSettingsByFormula(LeveeBreachFormula);
        }

        private LeveeBreachSettings GetLeveeBreachSettingsByFormula(LeveeBreachGrowthFormula growthFormula)
        {
            return leveeBreachSettings.FirstOrDefault(s => s.GrowthFormula == growthFormula);
        }

        private void SetDefaultBreachLocation()
        {
            var line = Geometry as ILineString;
            if (line == null) return;

            var lengthIndexedLine = new LengthIndexedLine(line);

            var offset = line.Length / 2.0;
            var point = new Point(lengthIndexedLine.ExtractPoint(offset));
            breachLocationX = point.X;
            breachLocationY = point.Y;
        }
    }
}
