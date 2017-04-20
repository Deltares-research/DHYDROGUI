using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public class ObsFile : FMSuiteFileBase, IFeature2DFileBase<Feature2DPoint>
    {
        public void Write(string obsFilePath, IEnumerable<Feature2DPoint> observationPoints, bool includeName = true)
        {
            using (CultureUtils.SwitchToInvariantCulture())
            {
                OpenOutputFile(obsFilePath);
                try
                {
                    foreach (Feature2DPoint observationPoint in observationPoints)
                    {
                        if (includeName)
                        {
                            WriteLine(String.Format("{0,24} {1,24} '{2}'",
                                                    observationPoint.X, observationPoint.Y,
                                                    observationPoint.Name));
                        }
                        else
                        {
                            WriteLine(String.Format("{0,24} {1,24}", 
                                observationPoint.X, observationPoint.Y));
                        }
                    }
                }
                finally
                {
                    CloseOutputFile();
                }
            }
        }

        public IEventedList<Feature2DPoint> Read(string obsFilePath, bool includeName = true)
        {
            IEventedList<Feature2DPoint> observationPoints = new EventedList<Feature2DPoint>();

            OpenInputFile(obsFilePath);
            try
            {
                string line = GetNextLine();

                int nameSuffix = 0;
                int expectedLineCount = includeName ? 3 : 2;
                
                while (line != null)
                {
                    var lineFields = SplitLine(line).Take(3).ToList();
                    if (lineFields.Count != expectedLineCount)
                    {
                        throw new Exception(String.Format("Invalid point row on line {0} in file {1}", LineNumber, obsFilePath));
                    }

                    var observationPoint = new Feature2DPoint();

                    var x = GetDouble(lineFields[0], "x-coord");
                    var y = GetDouble(lineFields[1], "y-coord");
                    observationPoint.Geometry = new Point(x, y);

                    observationPoint.Name = includeName
                                                ? lineFields[2]
                                                : string.Format("point{0}", nameSuffix++);

                    observationPoints.Add(observationPoint);

                    line = GetNextLine();
                }
            }
            finally
            {
                CloseInputFile();
            }
            return observationPoints;
        }

        public void Write(string path, IEnumerable<Feature2DPoint> features)
        {
            Write(path, features, true);
        }

        public IList<Feature2DPoint> Read(string path)
        {
            return Read(path, true);
        }
    }
}