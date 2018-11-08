using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileReaders.Network;

namespace DeltaShell.NGHS.IO.FileReaders.Location
{
    public class LateralSourceFileReader
    {
        public IList<ILateralSource> ReadLateralSources(string filePath, IHydroNetwork network)
        {
            IList<FileReadingException> fileReadingExceptions = new List<FileReadingException>();
            var categories = DelftIniFileParser.ReadFile(filePath);
            var lateralSources = LateralSourceConverter.Convert(categories, network, fileReadingExceptions);

            if (fileReadingExceptions.Count > 0)
            {
                var innerExceptionMessages = fileReadingExceptions.Select(fileReadingException => fileReadingException.InnerException?.Message + Environment.NewLine);
                throw new FileReadingException($"While reading the lateral sources from file, an error occured :{Environment.NewLine} {string.Join(Environment.NewLine, innerExceptionMessages)}");
            }

            return lateralSources;
        }
    }
}