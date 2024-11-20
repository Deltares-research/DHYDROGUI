using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;

namespace DeltaShell.Plugins.ImportExport.Sobek
{

    public static class GeometryHelper
    {
        /// <summary>
        /// Calculates geometry based on offset and angle of branch centres as stored in the network.cp file.
        /// Sobek supports two versions of the network.cp file.
        /// A Sobek 2.xx versions 
        /// TBLE ... tble  = Table with 'curving points: 
        /// column 1 = location on the branch in meters
        /// column 2 = angle (0 = north, 90= east)
        /// location must be read as the center of the segment -> first location > 0
        /// simplest possible example
        /// 
        /// StartNode-------------------------cp---------------------------EndNode
        ///    0,0                                                         100,0
        /// network.cp will have 1 cp at 50 with, angle 90
        /// In DeltaShell this cp is unnecessary
        /// 
        /// 
        /// Slightly more complex example:
        /// 
        ///     +-----------------------------cp------------------------------+
        ///     |                                                             |
        ///     |                                                             |
        ///     |                                                             |
        ///     |                                                             cp
        ///     |                                                             |
        ///     |                                                             |
        ///    cp                                                             |
        ///     |                                                          EndNode   
        ///     |    
        ///     |    
        ///     |    
        ///     |    
        ///     |    
        /// StartNode
        /// 
        /// In DeltaShell we will need coordinate at the startnode and endnode and at the actual curvepoints (+ in 
        /// diagram).
        /// The algorithm used in CalculateGeometry will duplcate the offset of the cp and use angle and x,y of
        /// its predecessor to calculate the x, y coordinates (of +). This will possible/always lead to a duplicat 
        /// coordinate at the end of the branch -> Ignore last point if it overlaps with end node.
        /// ( this will also eliminate the curvepoint for the simplest example above, as desired; QED )
        /// 
        /// 

        private static readonly ILog log = LogManager.GetLogger(typeof(GeometryHelper));

        /// B found in SobekRe; not yet supported?
        /// TBLE ... tble  = Table with 'curving points: 
        /// column 1 = location on the branch in meters
        /// column 2 = angle (0 = north, 90= east)
        /// location must be read as position of the curvepoint -> first location = 0
        /// 
        /// </summary>
        /// <param name="branchGeometry"></param>
        /// <param name="startNode"></param>
        /// <param name="endNode"></param>
        /// <returns></returns>
        public static IGeometry CalculateGeometry(bool sobek2Import, BranchGeometry branchGeometry, INode startNode, INode endNode)
        {
            var vertices = new List<Coordinate>();
            Coordinate firstCoordinate = PointToCoordinate((IPoint)startNode.Geometry);
            Coordinate lastCoordinate = PointToCoordinate((IPoint)endNode.Geometry);
            vertices.Add(firstCoordinate);
           
            if (branchGeometry != null)
            {
                ParseSobekCurvePoints(vertices, branchGeometry, firstCoordinate, lastCoordinate, sobek2Import);
            }

            vertices.Add(lastCoordinate);
            ILineString lineString = new LineString(vertices.ToArray());
            lineString.GeometryChangedAction();
            return lineString;
        }

        public static void ParseSobekCurvePoints(List<Coordinate> vertices, BranchGeometry branchGeometry,
            Coordinate firstCoordinate, Coordinate lastCoordinate, bool sobek2Import)
        {

            if (branchGeometry.CurvingPoints.Count == 0) return;
           
            double x = firstCoordinate.X;
            double y = firstCoordinate.Y;
            double segmentStartPoint = 0;

            if (sobek2Import)
            {
                for (int i = 0; i < branchGeometry.CurvingPoints.Count; i++)
                {
                    CurvingPoint curvingPoint = branchGeometry.CurvingPoints[i];
                    double segmentLength = (curvingPoint.Location - segmentStartPoint) * 2.0;
                    x += segmentLength * Math.Sin(curvingPoint.Angle * Math.PI / 180);
                    y += segmentLength * Math.Cos(curvingPoint.Angle * Math.PI / 180);
                    // add new curvepoint if is not the last or if does not overlaps.
                    if ((i < (branchGeometry.CurvingPoints.Count - 1)) ||
                        (Math.Abs(x - lastCoordinate.X) > 1.0e-6) || (Math.Abs(y - lastCoordinate.Y) > 1.0e-6))
                    {
                        vertices.Add(new Coordinate(x, y));
                    }

                    segmentStartPoint += segmentLength;
                }

                var lastCurvingPoint = branchGeometry.CurvingPoints.Last();
                double segmentLength2 = (lastCurvingPoint.Location - segmentStartPoint) * 2.0;
                x += segmentLength2 * Math.Sin(lastCurvingPoint.Angle * Math.PI / 180);
                y += segmentLength2 * Math.Cos(lastCurvingPoint.Angle * Math.PI / 180);
            }
            else //Based on Import Maas model (JAMM2010) the best guess to get geometry from curving points (tried also a circle from curving point to curving point, but this result was better)
            {
                double residueLength = 0;
                double angle = 0;

                for (int i = 1; i < branchGeometry.CurvingPoints.Count; i++)
                {
                    CurvingPoint curvingPoint = branchGeometry.CurvingPoints[i - 1];
                    CurvingPoint nextPoint = branchGeometry.CurvingPoints[i];
                    double halfLength = ((nextPoint.Location - curvingPoint.Location) * 0.5);
                    double segmentLength = residueLength + halfLength;
                    residueLength = halfLength;
                    angle = curvingPoint.Angle;

                    x += segmentLength * Math.Sin(angle * Math.PI / 180);
                    y += segmentLength * Math.Cos(angle * Math.PI / 180);

                    if ((i < (branchGeometry.CurvingPoints.Count - 1)) ||
                        (Math.Abs(x - lastCoordinate.X) > 1.0e-6) || (Math.Abs(y - lastCoordinate.Y) > 1.0e-6))
                    {
                        vertices.Add(new Coordinate(x, y));
                    }
                }

                CheckIfLastPointIsCorrect(vertices.Last(), new Coordinate(x, y), lastCoordinate);
            }
        }

        private static void CheckIfLastPointIsCorrect(Coordinate penultimateCurvingPoint, Coordinate lastCurvingPoint, Coordinate lastCoordinate)
        {
                var widthCurvingPoints = lastCurvingPoint.X-penultimateCurvingPoint.X;
                var heightCurvingPoints = lastCurvingPoint.Y-penultimateCurvingPoint.Y;
                var lengthCurvingPoints = Math.Sqrt(Math.Pow(widthCurvingPoints, 2) + Math.Pow(heightCurvingPoints, 2));

                var widthCorrected = lastCoordinate.X-penultimateCurvingPoint.X;
                var heightCorrected = lastCoordinate.Y-penultimateCurvingPoint.Y;
                var lengthCorrected = Math.Sqrt(Math.Pow(widthCorrected, 2) + Math.Pow(heightCorrected, 2));

                var length = lengthCorrected - lengthCurvingPoints;

                if(Math.Abs(length) > 10) //greater than 10 meter
                {
                    log.ErrorFormat("Last curvingpoint is not meeting the target coordinate. The correction has been changed the geometry length (> 10) of the branch with {0}.", length.ToString("N1"));
                }
        }

        private static Coordinate PointToCoordinate(IPoint p)
        {
            double x = p.X;
            double y = p.Y;
            return new Coordinate(x, y);
        }

        public static IGeometry GetPointGeometry(IBranch channel, double calculatedChainage)
        {
            var offset = channel.IsLengthCustom
                 ? (channel.Geometry.Length / channel.Length) * calculatedChainage
                 : calculatedChainage;

            LinearLocation loc = LengthLocationMap.GetLocation(channel.Geometry, offset);
            return new Point(loc.GetCoordinate(channel.Geometry));
        }

    }
}