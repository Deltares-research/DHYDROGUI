using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileReaders;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public class Feature2DPointFile<T> : FMSuiteFileBase, IFeature2DFileBase<T> where T : Feature2DPoint, new()
    {
        public void Write(string path, IEnumerable<T> features)
        {
            using (CultureUtils.SwitchToInvariantCulture())
            {
                OpenOutputFile(path);
                try
                {
                    foreach (var feature in features)
                    {
                        WriteLine($"{feature.X,24} {feature.Y,24} '{feature.Name}'");
                    }
                }
                finally
                {
                    CloseOutputFile();
                }
            }
        }

        public IList<T> Read(string path)
        {
            IEventedList<T> features = new EventedList<T>();

            OpenInputFile(path);
            try
            {
                string line = GetNextLine();

                int expectedLineCount = true ? 3 : 2;
                
                while (line != null)
                {
                    if (line == "[General]") break;
                    var lineFields = SplitLine(line).Take(3).ToList();
                    if (lineFields.Count != expectedLineCount)
                    {
                        throw new FileReadingException($"Invalid point row on line {LineNumber} in file {path}");
                    }

                    var feature = new T
                    {
                        Geometry = new Point(GetDouble(lineFields[0], "x-coord"), GetDouble(lineFields[1], "y-coord")),
                        Name = lineFields[2]
                    };

                    feature.TrySetGroupName(path);
                    features.Add(feature);

                    line = GetNextLine();
                }
            }
            finally
            {
                CloseInputFile();
            }
            return features;
        }
    }
}