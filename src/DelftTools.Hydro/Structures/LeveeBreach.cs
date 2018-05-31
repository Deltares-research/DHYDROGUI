using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Aop;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using DelftTools.Hydro.Structures.LeveeBreachFormula;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro.Structures
{
    [Entity]
    public class LeveeBreach : GroupableFeature2D, IStructure
    {
        private double breachLocationX;
        private double breachLocationY;
        private bool isLocationSet = false;
        private readonly IEnumerable<LeveeBreachSettings> leveeBreachSettings;

        public LeveeBreach()
        {
            leveeBreachSettings = new List<LeveeBreachSettings>
            {
                new UserDefinedBreach(),
                new VerheijVdKnaap2002Breach()
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

        public LeveeBreachGrowthFormula LeveeBreachFormula { get; set; } = LeveeBreachGrowthFormula.VerheijvdKnaap2002;

        public LeveeBreachSettings GetLeveeBreachSettings()
        {
            return GetLeveeBreachSettingsByFormula(LeveeBreachFormula);
        }

        public void SetBaseLeveeBreachSettings(DateTime startTime, bool breachGrowthActive)
        {
            foreach (var setting in leveeBreachSettings)
            {
                setting.StartTimeBreachGrowth = startTime;
                setting.BreachGrowthActive = breachGrowthActive;
            }
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

        #region Implementation of IStructure

        public int CompareTo(INetworkFeature other)
        {
            throw new System.NotImplementedException();
        }

        public INetwork Network { get; set; }
        public IHydroNetwork HydroNetwork { get; }

        string IHydroNetworkFeature.Description
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }

        public string LongName { get; set; }

        string INetworkFeature.Description
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }

        public IHydroRegion Region { get; }
        public IEventedList<HydroLink> Links { get; set; }
        public bool CanBeLinkSource { get; }
        public bool CanBeLinkTarget { get; }
        public HydroLink LinkTo(IHydroObject target)
        {
            throw new System.NotImplementedException();
        }

        public void UnlinkFrom(IHydroObject target)
        {
            throw new System.NotImplementedException();
        }

        public bool CanLinkTo(IHydroObject target)
        {
            throw new System.NotImplementedException();
        }

        public int CompareTo(IBranchFeature other)
        {
            throw new System.NotImplementedException();
        }

        public void CopyFrom(object source)
        {
            throw new System.NotImplementedException();
        }

        public IBranch Branch { get; set; }
        public double Chainage { get; set; }
        public double Length { get; set; }
        public ICompositeBranchStructure ParentStructure { get; set; }
        public double OffsetY { get; set; }
        public StructureType GetStructureType()
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
