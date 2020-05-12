using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Files
{
    public class ObsFile<T> : NGHSFileBase, IFeature2DFileBase<T> where T : Feature2DPoint, new()
    {
        public void Write(string obsFilePath, IEnumerable<T> observationPoints, bool includeName = true)
        {
            using (CultureUtils.SwitchToInvariantCulture())
            {
                OpenOutputFile(obsFilePath);
                try
                {
                    foreach (T observationPoint in observationPoints)
                    {
                        if (includeName)
                        {
                            WriteLine(string.Format("{0,24} {1,24} '{2}'",
                                                    observationPoint.X, observationPoint.Y,
                                                    observationPoint.Name));
                        }
                        else
                        {
                            WriteLine(string.Format("{0,24} {1,24}",
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

        public IEventedList<T> Read(string obsFilePath, bool includeName = true)
        {
            IEventedList<T> observationPoints = new EventedList<T>();

            OpenInputFile(obsFilePath);
            try
            {
                string line = GetNextLine();

                var nameSuffix = 0;
                int expectedLineCount = includeName ? 3 : 2;

                while (line != null)
                {
                    List<string> lineFields = SplitLine(line).Take(3).ToList();
                    if (lineFields.Count != expectedLineCount)
                    {
                        throw new Exception(string.Format("Invalid point row on line {0} in file {1}", LineNumber,
                                                          obsFilePath));
                    }

                    var observationPoint = new T
                    {
                        Geometry = new Point(GetDouble(lineFields[0], "x-coord"), GetDouble(lineFields[1], "y-coord")),
                        Name = includeName
                                   ? lineFields[2]
                                   : string.Format("point{0}", nameSuffix++)
                    };

                    observationPoint.TrySetGroupName(obsFilePath);
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

        public void Write(string filePath, IEnumerable<T> features)
        {
            Write(filePath, features, true);
        }

        public IList<T> Read(string filePath)
        {
            return Read(filePath, true);
        }
    }
}