using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{

    public enum SobekCrossSectionDefinitionType
    {
        Tabulated = 0, 
        Trapezoidal = 1,
        OpenCircle = 2,
        Sedredge = 3, // 2d morfology
        ClosedCircle = 4,
        EggShapedWidth = 6,
        //EggShapedRadius = 7, according to Sobek Help not implemented
        //ClosedRectangular = 8, according to Sobek Help not implemented
        Yztable = 10,
        AsymmetricalTrapezoidal = 11
    }

    public class SobekTabulatedProfileRow
    {
        public double Height { get; set; }
        public double TotalWidth { get; set; }
        public double FlowWidth { get; set; }
    }

    //public class SobekYZProfileRow
    //{
    //    public double Y { get; set; }
    //    public double Z { get; set; }
    //}

    public class SobekCrossSectionDefinition
    {
        public IList<Coordinate> YZ { get; private set; }
        public IList<SobekTabulatedProfileRow> TabulatedProfile { get; private set; }

        public SobekCrossSectionDefinition()
        {
            YZ = new List<Coordinate>();
            TabulatedProfile = new List<SobekTabulatedProfileRow>();
            Width = -1;  Height = -1;  ArcHeight = -1;
            Slope = -1; MaxFlowWidth = -1; RadiusR = -1;
            RadiusR1 = -1; RadiusR2 = -1; RadiusR3 = -1;
            AngleA = -1; AngleA1 = -1;
            BedWidth = -1;
        }

        public string Name { get; set; }
        public SobekCrossSectionDefinitionType Type { get; set; }
        public double FloodPlain1Width { get; set; }
        public double FloodPlain2Width { get; set; }

        /// <summary>
        /// Indicates whether a tabulated profile (0) should be interpreted as a river profile.
        /// A river profile can have different roughness for main channel, floodplain1 and floodplain2
        /// Also Sobek River support Summerdikes. Summerdikes are not yet supported by DelftModel1d
        /// </summary>
        public bool IsRiverProfile { get; set; }

        /// <summary>
        /// Only for river profile (special case Tabulated (0)
        /// </summary>
        public bool SummerDikeActive { get; set; }

        /// <summary>
        /// Only for river profile (special case Tabulated (0)
        /// </summary>
        public double CrestLevel { get; set; }

        /// <summary>
        /// Only for river profile (special case Tabulated (0)
        /// </summary>
        public double FloodPlainLevel { get; set; }

        /// <summary>
        /// Only for river profile (special case Tabulated (0)
        /// </summary>
        public double FlowArea { get; set; }

        /// <summary>
        /// Only for river profile (special case Tabulated (0)
        /// </summary>
        public double TotalArea { get; set; }

        public double GroundLayerDepth { get; set; }
        public bool UseGroundLayer { get; set; }
        public double MainChannelWidth { get; set; }
        public double SedimentTransportWidth { get; set; }
        public string ID { get; set; }

        // specific for: ClosedCircle, EggShapeWidth
        public double BedLevel { get; set; }

        // specific for: ClosedCircle
        public double Radius { get; set; }

        // specific for: EggShapeWidth & Trapezium
        public double BedWidth { get; set; }

        // addition for standard cross section types
        // shared among several standard types
        public double Width { get; set; }// = width (also max. flow width for trapezium)
        public double Height { get; set; }// = height
        // arch-specific
        public double ArcHeight { get; set; }// = archeight
        // trapezium-specific
        public double Slope { get; set; }// = slope
        public double MaxFlowWidth { get; set; }// = maximum flow width
        // stell cunette-specific
        public double RadiusR { get; set; }// = radius r
        public double RadiusR1 { get; set; }// = radius r1
        public double RadiusR2 { get; set; }// = radius r2
        public double RadiusR3 { get; set; }// = radius r2
        public double AngleA { get; set; }// = angle a
        public double AngleA1 { get; set; }// = angle a1

        /// <summary>
        /// 0 = reservoir
        /// 1 = loss of water that is above surface level
        /// </summary>
        public int StorageType { get; set; }

        public bool InferStandardType { get; set; }

        public bool IsTabulatedProfileClosedRectangularShape
        {
            get
            {
                var sobekTabulatedProfileRows = TabulatedProfile.ToArray();
                if (sobekTabulatedProfileRows.Length == 3)
                {
                    var b2 = sobekTabulatedProfileRows[1];
                    var b3 = sobekTabulatedProfileRows[2];
                    if (b3.Height - b2.Height <= 0.002
                        && b3.TotalWidth < b2.TotalWidth - 0.02)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Adds row to the ty=0 ZW cross section table
        /// </summary>
        /// <param name="height"></param>
        /// <param name="totalWidth"></param>
        /// <param name="flowWidth"></param>
        public void AddTableRow(double height, double totalWidth, double flowWidth)
        {
            TabulatedProfile.Add(new SobekTabulatedProfileRow
                                     {Height = height, TotalWidth = totalWidth, FlowWidth = flowWidth});
        }
    }
}