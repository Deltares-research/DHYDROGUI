using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils;
using DeltaShell.NGHS.IO;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    class QhFile : FMSuiteFileBase
    {
        public void Write(string filePath, IFunction data)
        {
            using (CultureUtils.SwitchToInvariantCulture())
            {
                try
                {
                    OpenOutputFile(filePath);
                    foreach (var argumentValue in data.Arguments[0].Values)
                    {
                        var value = (double) data[argumentValue];
                        WriteLine(String.Format("{0:0.0000000e+00}  {1:0.0000000e+00}", argumentValue, value));
                    }
                }
                finally
                {
                    CloseOutputFile();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is an empty string ("").</exception>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is null.</exception>
        /// <exception cref="System.IO.FileNotFoundException">The file cannot be found.</exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="System.IO.IOException"><paramref name="filePath"/> includes an incorrect or invalid syntax for file name, directory name, or volume label.</exception>
        /// <exception cref="OutOfMemoryException">There is insufficient memory to allocate a buffer for the returned string.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occured.</exception>
        /// <exception cref="FormatException">When a line with invalid format was encountered.</exception>
        public IFunction Read(string filePath)
        {
            OpenInputFile(filePath);
            var qvalues = new List<double>();
            var hvalues = new List<double>();
            var line = GetNextLine();
            while (line != null)
            {
                var lineFields = SplitLine(line).ToList();
                if (lineFields.Count < 2)
                {
                    throw new FormatException(String.Format("Invalid q-value/h-value row on line {0} in file {1}",
                                                            LineNumber, filePath));
                }

                qvalues.Add(GetDouble(lineFields[0], "q-value"));
                hvalues.Add(GetDouble(lineFields[1], "h-value"));

                line = GetNextLine();
            }

            var function = new Function();
            function.Arguments.Add(new Variable<double>());
            function.Arguments[0].AddValues(qvalues);
            function.Components.Add(new Variable<double>());
            function.Components[0].SetValues(hvalues);
            return function;
        }
    }
}
