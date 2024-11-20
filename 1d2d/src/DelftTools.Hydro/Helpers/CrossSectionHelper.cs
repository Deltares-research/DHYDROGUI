using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;

namespace DelftTools.Hydro.Helpers
{
    ///<summary>
    /// Utility class to work with cross sections
    ///</summary>
    public static class CrossSectionHelper
    {
        private const double DefaultCrossSectionWidth = 100.0; 
        private static readonly ILog Log = LogManager.GetLogger(typeof(CrossSectionHelper));

        /// <summary>
        /// Adds a default \/ to the geometry of cross-section. Default z is now NaN.
        /// </summary>
        /// <param name="crossSectionDefinition"></param>
        public static void AddDefaultZToGeometry(CrossSectionDefinitionXYZ crossSectionDefinition)
        {
            var chainages = crossSectionDefinition.GetProfile().Select(c => c.X).ToArray();
            var length = chainages.Last();
            for (int i = 0; i < crossSectionDefinition.Geometry.Coordinates.Length;i++ )
            {
                //for the first part go down 10 meters until we reach half the length
                if (chainages[i] < length /2)
                {
                    crossSectionDefinition.Geometry.Coordinates[i].Z = 10 - chainages[i]/length*20;
                }
                //come up a again to a max of 10 
                else
                {
                    crossSectionDefinition.Geometry.Coordinates[i].Z = -10 + chainages[i]/length*20;
                }
            }
        }
        ///<summary>
        /// Calculator to update the conveyance table for the cross section.
        ///</summary>
        public static IConveyanceCalculator CurrentConveyanceCalculator { get; set; }

        static CrossSectionHelper()
        {
            CurrentConveyanceCalculator = new DefaultConveyanceCalculator();
        }

        /// <summary>
        /// Updates the conveyance table
        /// </summary>
        /// <param name="crossSection"></param>
        public static IFunction GetConveyanceTable(ICrossSection crossSection)
        {
            return CurrentConveyanceCalculator.GetConveyance(crossSection);
        }

        /// <summary>
        /// Sets a default geometry for the cross section. The default geometry is a linestring geometry 
        /// perpendicular to the branch. 
        /// </summary>
        /// <param name="branchGeometry"></param>
        /// <param name="crossSectionGeometry"></param>
        /// <param name="mapChainage"></param>
        /// <param name="length"></param>
        /// <param name="thalWeg"></param>
        /// <param name="thalwegOffset"></param>
        /// The default length of the generated linestring geometry. Since this can be different for each network 
        /// it is not a setting of the CrossSectionService
        public static IGeometry ComputeDefaultCrossSectionGeometry(IGeometry branchGeometry, double mapChainage,
            double length, double thalWeg, double thalwegOffset)
        {
            Coordinate coordinate;
            double angle;

            if (null == branchGeometry)
            {
                throw new ArgumentNullException("branchGeometry");
            }

            /// use linear location to determine
            /// A. the point on the branch at which the cross section is located
            /// B. the coordinates of the branchlinesegment at the location of the 
            /// cross section
            LinearLocation location = LengthLocationMap.GetLocation(branchGeometry, mapChainage);
            coordinate = location.GetCoordinate(branchGeometry);

            var lineComp = (ILineString) branchGeometry.GetGeometryN(location.ComponentIndex);
            Coordinate p1;
            var p0 = lineComp.GetCoordinateN(location.SegmentIndex);

            if (location.SegmentIndex >= lineComp.NumPoints - 1)
            {
                p1 = p0;
                p0 = lineComp.GetCoordinateN(location.SegmentIndex - 1);
            }
            else
            {
                p1 = lineComp.GetCoordinateN(location.SegmentIndex + 1);
            }

            var line = new LineString(new[] {p0, p1});
            angle = line.AngleRad() + Math.PI/2.0; // rotation of 90 degrees

            var delta = thalWeg - thalwegOffset;
            var sin = Math.Sin(angle);
            var cos = Math.Cos(angle);

            var crossSectionGeometry = new LineString(new Coordinate[2]);

            crossSectionGeometry.Coordinates[0] = new Coordinate(coordinate.X - delta*sin, coordinate.Y - delta*cos);
            delta = length - delta;
            crossSectionGeometry.Coordinates[1] = new Coordinate(coordinate.X + delta*sin, coordinate.Y + delta*cos);

            UpdateEnvelopeInternal(crossSectionGeometry);

            return crossSectionGeometry;
        }

        ///<summary>
        /// Creates an xyz geometry for a cross section at a given chainage in a branch. The yzCoordinates are used to calculate
        /// the coordinates in the x, y plane. The resulting geometry is a straigh line perpendiculair to the branch.
        ///</summary>
        ///<param name="branchGeometry"></param>
        ///<param name="chainage"></param>
        ///<param name="yzCoordinates"></param>
        ///<returns></returns>
        ///<exception cref="ArgumentException"></exception>
        public static IGeometry CreateCrossSectionGeometryForXyzCrossSectionFromYZ(IGeometry branchGeometry, double chainage,
            IEnumerable<Coordinate> yzCoordinates)
        {
            var coordinates = yzCoordinates.ToList();

            return CreateCrossSectionGeometryForXyzCrossSectionFromYZ(branchGeometry, chainage, coordinates,
                (coordinates[coordinates.Count - 1].X - coordinates[0].X) / 2); // set default thalweg to center
        }

        ///<summary>
        /// Creates an xyz geometry for a cross section at a given chainage in a branch. The yzCoordinates are used to calculate
        /// the coordinates in the x, y plane. The resulting geometry is a straigh line perpendiculair to the branch.
        ///</summary>
        ///<param name="branchGeometry"></param>
        ///<param name="chainage"></param>
        ///<param name="yzCoordinates"></param>
        ///<param name="thalWegOffset"></param>
        ///<returns></returns>
        ///<exception cref="ArgumentException"></exception>
        public static IGeometry CreateCrossSectionGeometryForXyzCrossSectionFromYZ(IGeometry branchGeometry, double chainage,
            IList<Coordinate> yzCoordinates, double thalWegOffset)
        {
            if (null == branchGeometry)
            {
                throw new ArgumentException("The geometry can not be set for cross section that is not connected to a branch.");
            }

            /// use linear location to determine
            /// A. the point on the branch at which the cross section is located
            /// B. the coordinates of the branchlinesegment at the location of the cross section
            LinearLocation location = LengthLocationMap.GetLocation(branchGeometry, chainage);
            Coordinate thalWeg = location.GetCoordinate(branchGeometry);

            var lineComp = (ILineString)branchGeometry.GetGeometryN(location.ComponentIndex);
            Coordinate p1;
            Coordinate p0 = lineComp.GetCoordinateN(location.SegmentIndex);

            if (location.SegmentIndex >= lineComp.NumPoints - 1)
            {
                p1 = p0;
                p0 = lineComp.GetCoordinateN(location.SegmentIndex - 1);
            }
            else
            {
                p1 = lineComp.GetCoordinateN(location.SegmentIndex + 1);
            }

            /// construct a line segment perpendicular to that of the branchlinesegment
            /// that crosses the branch at the cross section location
            var line = new LineString(new[] { p0, p1 });
            double angle = line.Angle * Math.PI / 180 + Math.PI / 2.0;
            if (Double.IsNaN(angle))
            {
                Log.ErrorFormat("Error calculating channel angle; set to 0");
                angle = 0;
            }

            List<Coordinate> vertices = new List<Coordinate>();
            for (int i=0; i<yzCoordinates.Count; i++)
            {
                Coordinate yzCoordinate = yzCoordinates[i];
                double delta = yzCoordinate.X - yzCoordinates[0].X;
                vertices.Add(new Coordinate(thalWeg.X + (thalWegOffset - delta) * Math.Sin(angle),
                                            thalWeg.Y + (thalWegOffset - delta) * Math.Cos(angle), yzCoordinate.Y));
            }

            GeometryFactory geometryFactory = new GeometryFactory();
            IGeometry geometry = geometryFactory.CreateLineString(vertices.ToArray());
            UpdateEnvelopeInternal(geometry);
            return geometry;
        }

        public static IGeometry CreatePerpendicularGeometry(IGeometry branchGeometry, double chainage, 
            double crossSectionWidth,double thalweg)
        {
            var minY = -crossSectionWidth/2;
            var maxY = crossSectionWidth / 2;
            return CreatePerpendicularGeometry(branchGeometry, chainage, minY, maxY, thalweg);
        }
        /// <summary>
        /// Creates an xyz geometry for a cross section at a given chainage in a branch. The maximum width is used to calculate
        /// the geometry. The resulting geometry is a straigh line perpendiculair to the branch.
        /// </summary>
        /// <param name="branchGeometry"></param>
        /// <param name="chainage"></param>
        /// <param name="tabulatedData"></param>
        /// <returns></returns>
        public static IGeometry CreatePerpendicularGeometry(IGeometry branchGeometry, double chainage, 
            double crossSectionWidth)
        {
            return CreatePerpendicularGeometry(branchGeometry, chainage, 0, crossSectionWidth, crossSectionWidth/2);
        }


        /// <summary>
        /// Calculates an y'z table for an xyz cross section
        /// </summary>
        /// <param name="yZValues"></param>
        /// <param name="geometry"></param>
        public static IEnumerable<Coordinate> CalculateYZProfileFromGeometry(IGeometry geometry)
        {
            var yzValues = new List<Coordinate>();
            if (geometry == null)
            {
                return yzValues;
            }
            double offset = 0.0;
            Coordinate[] coordinates = geometry.Coordinates;
            for (int i = 0; i < coordinates.Length; i++)
            {
                yzValues.Add(new Coordinate(offset, coordinates[i].Z));
                if (i < coordinates.Length - 1)
                {
                    offset += coordinates[i + 1].Distance(coordinates[i]);
                }
            }
            return yzValues;
        }

        public static void SetDefaultYZTableAndUpdateThalWeg(this CrossSectionDefinitionYZ crossSectionDefinition,double width = DefaultCrossSectionWidth)
        {
            crossSectionDefinition.YZDataTable.BeginLoadData();
            crossSectionDefinition.YZDataTable.AddCrossSectionYZRow(0, 0.0);
            crossSectionDefinition.YZDataTable.AddCrossSectionYZRow((4 * width / 18), 0.0);
            crossSectionDefinition.YZDataTable.AddCrossSectionYZRow((6 * width / 18), -10.0);
            crossSectionDefinition.YZDataTable.AddCrossSectionYZRow((12 * width / 18), -10.0);
            crossSectionDefinition.YZDataTable.AddCrossSectionYZRow((14 * width / 18), 0.0);
            crossSectionDefinition.YZDataTable.AddCrossSectionYZRow(width, 0.0);
            crossSectionDefinition.YZDataTable.EndLoadData();

            crossSectionDefinition.Thalweg = crossSectionDefinition.Width / 2;
        }

        public static void SetDefaultZWTable(this CrossSectionDefinitionZW crossSectionDefinition)
        {
            crossSectionDefinition.ZWDataTable.AddCrossSectionZWRow(0, DefaultCrossSectionWidth, 0);
            crossSectionDefinition.ZWDataTable.AddCrossSectionZWRow(-10.0, DefaultCrossSectionWidth / 3, 0);
        }

        public static CrossSectionDefinitionZW ConverStandardToZw(this CrossSectionDefinitionStandard standardDefinition)
        {
            var crossSectionDefinitionZw = standardDefinition.Shape.GetTabulatedDefinition();

            crossSectionDefinitionZw.ShiftLevel(standardDefinition.LevelShift);
            var section = standardDefinition.Sections.FirstOrDefault();
            crossSectionDefinitionZw.Sections.Add(new CrossSectionSection
            {
                SectionType = section == null ? new CrossSectionSectionType { Name = RoughnessDataSet.MainSectionTypeName } : section.SectionType,
                MinY = 0,
                MaxY = crossSectionDefinitionZw.Width / 2
            });
            return crossSectionDefinitionZw;
        }

        /// <summary>
        /// Update the envelope. This is not by default updates by NTS because modifying geometries is not done.
        /// </summary>
        /// <param name="geometry"></param>
        private static void UpdateEnvelopeInternal(IGeometry geometry)
        {
            Coordinate[] coordinates = geometry.Coordinates;

            if (coordinates.Length > 1)
            {
                for (int i = 0; i < coordinates.Length; i++)
                {
                    if (0 == i)
                    {
                        geometry.EnvelopeInternal.Init(coordinates[i]);
                    }
                    else
                    {
                        geometry.EnvelopeInternal.ExpandToInclude(coordinates[i]);
                    }
                }
            }
        }
        
        ///<summary>
        /// Validate thalweg to fall between min and max y; better to add limits to the ICursorLineTool
        ///</summary>
        ///<param name="crossSectionDefinition"></param>
        ///<param name="thalWeg"></param>
        public static double ValidateThalWay(ICrossSectionDefinition crossSectionDefinition, double thalWeg)
        {
            double validated = thalWeg;
            var yzValues = crossSectionDefinition.GetProfile();
            validated = Math.Max(validated, yzValues.Min(yz => yz.X));
            validated = Math.Min(validated, yzValues.Max(yz => yz.X));
            return validated;

        }


        public static void AddCrossSection(IChannel branch1, double chainage, double bedLevel)
        {
            var crossSection1 = new CrossSection(new CrossSectionDefinitionXYZ("crs1"));

            NetworkHelper.AddBranchFeatureToBranch(crossSection1, branch1, chainage);
            double surfaceLevel = bedLevel+10.0;
            var yzCoordinates = new List<Coordinate>
                                    {
                                        new Coordinate(0.0, surfaceLevel),
                                        new Coordinate(100.0, surfaceLevel),
                                        new Coordinate(150.0, bedLevel),
                                        new Coordinate(300.0, bedLevel),
                                        new Coordinate(350.0, surfaceLevel),
                                        new Coordinate(500.0, surfaceLevel)
                                    };
            crossSection1.Geometry = CreateCrossSectionGeometryForXyzCrossSectionFromYZ(branch1.Geometry, chainage, yzCoordinates);
        }


        public static void SetDefaultThalweg(ICrossSectionDefinition crossSectionDefinition)
        {
            var profileValues = crossSectionDefinition.GetProfile().ToList();

            if (profileValues.Count > 0)
            {
                var min = profileValues[0].X;
                var max = profileValues[profileValues.Count - 1].X;

                crossSectionDefinition.Thalweg = (min + max) / 2.0;
            }
            else
            {
                crossSectionDefinition.Thalweg = 0.0;
            }
        }

        public static IGeometry CreatePerpendicularGeometry(IGeometry branchGeometry, double chainage,
            double minY, double maxY, double thalWegOffset)
        {
            if (null == branchGeometry)
            {
                throw new ArgumentException(
                    "The geometry can not be set for cross section that is not connected to a branch.");
            }

            // use linear location to determine
            // A. the point on the branch at which the cross section is located
            // B. the coordinates of the branchlinesegment at the location of the cross section
            var intersectionLocation = LengthLocationMap.GetLocation(branchGeometry, chainage);
            var line = GetBranchLineAtLocation(branchGeometry, intersectionLocation);

            var thalWeg = intersectionLocation.GetCoordinate(branchGeometry);

            return GetLine(maxY - minY, thalWegOffset - minY, thalWeg, line.AngleRad() + Math.PI / 2.0);
        }

        /// <summary>
        /// Creates a line at a certain center with offset, angle and lenght
        /// </summary>
        /// <param name="length"></param>
        /// <param name="centerOffsetAlongLine"></param>
        /// <param name="center"></param>
        /// <param name="angleRad">And in radians</param>
        /// <returns></returns>
        private static ILineString GetLine(double length, double centerOffsetAlongLine, Coordinate center, double angleRad)
        {
            var yzCoordinates = new List<Coordinate> {new Coordinate(0, 0), new Coordinate(length, 0)};

            var vertices = new List<Coordinate>();
            for (int i = 0; i < yzCoordinates.Count; i++)
            {
                Coordinate yzCoordinate = yzCoordinates[i];
                double delta = yzCoordinate.X - centerOffsetAlongLine;
                vertices.Add(new Coordinate(center.X + (delta)*Math.Sin(angleRad),
                                            center.Y + (delta)*Math.Cos(angleRad)));
            }

            var geometryFactory = new GeometryFactory();
            var geometry = geometryFactory.CreateLineString(vertices.ToArray());
            UpdateEnvelopeInternal(geometry);
            return geometry;
        }

        private static LineString GetBranchLineAtLocation(IGeometry branchGeometry, LinearLocation location)
        {
            var lineComp = (ILineString)branchGeometry.GetGeometryN(location.ComponentIndex);
            Coordinate p1;
            Coordinate p0 = lineComp.GetCoordinateN(location.SegmentIndex);

            if (location.SegmentIndex >= lineComp.NumPoints - 1)
            {
                p1 = p0;
                p0 = lineComp.GetCoordinateN(location.SegmentIndex - 1);
            }
            else
            {
                p1 = lineComp.GetCoordinateN(location.SegmentIndex + 1);
            }

            /// construct a line segment perpendicular to that of the branchlinesegment
            /// that crosses the branch at the cross section location
            return new LineString(new[] { p0, p1 });
        }

        public static double CalculateStorageArea(IEnumerable<Coordinate> bottomProfile, IEnumerable<Coordinate> flowProfile)
        {
            if (bottomProfile.Count() < 2)
            {
                return 0.0;
            }

            var upperProfile = flowProfile.Reverse();

            var ring = bottomProfile.Concat(upperProfile).ToList();
            if (ring.Count > 0)
            {
                ring.Add(ring[0]); //connect to beginning
            }

            var area = new Polygon(new LinearRing(ring.ToArray()));
            return area.Area;
        }

        public static ICrossSection AddXYZCrossSectionFromYZCoordinates(IChannel branch, double chainage, IEnumerable<Coordinate> yzCoordinates, string name = "")
        {
            var csDef = new CrossSectionDefinitionXYZ();

            var cs = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch, csDef, chainage);

            //if no name is defined generate one.
            if (string.IsNullOrEmpty(name))
            {
                name = HydroNetworkHelper.GetUniqueFeatureName(branch.HydroNetwork, cs);
            }
            cs.Name = name;


            csDef.Geometry = CreateCrossSectionGeometryForXyzCrossSectionFromYZ(branch.Geometry, chainage, yzCoordinates);

            return cs;
        }

        public static void AddYZCrossSectionFromYZCoordinates(IChannel branch, double chainage, IEnumerable<Coordinate> yzCoordinates, string name = "")
        {
            var csDef = new CrossSectionDefinitionYZ();

            var cs = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch, csDef, chainage);

            //if no name is defined generate one.
            if (string.IsNullOrEmpty(name))
            {
                name = HydroNetworkHelper.GetUniqueFeatureName(branch.HydroNetwork, cs);
            }
            cs.Name = name;

            foreach (var yzCoordinate in yzCoordinates)
            {
                csDef.YZDataTable.AddCrossSectionYZRow(yzCoordinate.X, yzCoordinate.Y);
            }
            csDef.Thalweg = 0.0;
        }

        public static void AddZWCrossSectionFromHeightWidthTable(IChannel branch, double chainage, List<HeightFlowStorageWidth> heightWidthFlowStorage, string name = "" )
        {
            var csDef = new CrossSectionDefinitionZW();
            csDef.ZWDataTable.Set(heightWidthFlowStorage);

            var mainSection = branch.HydroNetwork.CrossSectionSectionTypes
                .FirstOrDefault(cst => string.Equals(cst.Name, RoughnessDataSet.MainSectionTypeName, StringComparison.InvariantCultureIgnoreCase)) 
                ?? 
                new CrossSectionSectionType { Name = RoughnessDataSet.MainSectionTypeName };

            csDef.AddSection(mainSection, csDef.FlowWidth());

            var cs = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch, csDef, chainage);

            //if no name is defined generate one.
            if (string.IsNullOrEmpty(name))
            {
                name = HydroNetworkHelper.GetUniqueFeatureName(branch.HydroNetwork, cs);
            }
            cs.Name = name;
            csDef.Thalweg = 0.0;
        }

        public static ICrossSection CreateNewCrossSectionXYZ(List<Coordinate> vertices)
        {
            var crossSection = new CrossSectionDefinitionXYZ();

            IGeometry geometry = new LineString(vertices.ToArray());

            crossSection.Geometry = geometry;
            return new CrossSection(crossSection);
        }
    }
}