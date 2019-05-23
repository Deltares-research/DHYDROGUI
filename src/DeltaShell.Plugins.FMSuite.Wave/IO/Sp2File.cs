using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using GeoAPI.Geometries;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Wave.IO
{
    public class Sp2File : FMSuiteFileBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Sp2File));

        public IDictionary<Coordinate,IFunction> Read(string sp2FilePath)
        {
            IList<Coordinate> coordinates = null;

            try
            {
                OpenInputFile(sp2FilePath);

                while (GetNextLine() != null)
                {
                    if (CurrentLine.Trim().StartsWith("$")) continue;
                    if (CurrentLine.Trim().StartsWith("SWAN")) continue;

                    // read coordinates
                    if (CurrentLine.Trim().StartsWith("LOCATIONS"))
                    {
                        var fields = GetNextLine().Trim().Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                        int nrOfCoordinates = int.Parse(fields[0], CultureInfo.InvariantCulture);
                        coordinates = ReadCoordinates(nrOfCoordinates).ToList();
                    }

                    break; // currently, all we need are coordinates
                }

            }
            catch (Exception e)
            {
                Log.ErrorFormat("Error parsing sp2 file \'{0}\' at line {1}: {2}", sp2FilePath, LineNumber, e.Message);
            }
            finally
            {
                CloseInputFile();
            }

            // todo: read wave enery density functions
            var data = new Dictionary<Coordinate, IFunction>();
            if (coordinates != null) coordinates.ForEach(c => data.Add(c, null));
            return data;
        }

        

        private IEnumerable<Coordinate> ReadCoordinates(int nrOfCoordinates)
        {
            for (int i = 0; i < nrOfCoordinates; ++i)
            {
                var fields = GetNextLine().Trim().Split(new[]{' '}, StringSplitOptions.RemoveEmptyEntries);
                var x = double.Parse(fields[0], CultureInfo.InvariantCulture);
                var y = double.Parse(fields[1], CultureInfo.InvariantCulture);
                yield return new Coordinate(x,y);
            }
        } 
        
        public void Write(IDictionary<Coordinate, IFunction> data, string sp2FilePath)
        {

        }
    }
}
