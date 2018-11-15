using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileReaders.Network;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileReaders.Location
{
    public class LateralSourceFileReader
    {
        private readonly Action<string, IList<string>> createAndAddErrorReport;

        public LateralSourceFileReader(Action<string, IList<string>> createAndAddErrorReport)
        {
            this.createAndAddErrorReport = createAndAddErrorReport;
        }

        public IList<ILateralSource> ReadLateralSources(string filePath, IList<IChannel> channelsList)
        {
            var errorMessages = new List<string>();
            IList<DelftIniCategory> categories = new List<DelftIniCategory>();
            try 
            {
                categories = DelftIniFileParser.ReadFile(filePath);
            }
            catch (Exception e)
            {
                errorMessages.Add(e.Message);
            }

            var lateralSources = LateralSourceConverter.Convert(categories, channelsList, errorMessages);

            if (errorMessages.Count > 0)
                createAndAddErrorReport?.Invoke("While reading the lateral sources from file, an error occured", errorMessages);

            return lateralSources;
        }
    }
}