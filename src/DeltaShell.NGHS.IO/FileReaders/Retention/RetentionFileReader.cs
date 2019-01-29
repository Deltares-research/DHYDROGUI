using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;

namespace DeltaShell.NGHS.IO.FileReaders.Retention
{
    public class RetentionFileReader
    {
        private readonly Action<string, IList<string>> createAndAddErrorReport;

        public RetentionFileReader(Action<string, IList<string>> createAndAddErrorReport)
        {
            this.createAndAddErrorReport = createAndAddErrorReport;
        }

        public IList<IRetention> ReadRetention(string filePath, IList<IChannel> channelsList)
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

            var retention = RetentionConverter.Convert(categories, channelsList, errorMessages);

            if (errorMessages.Count > 0)
                createAndAddErrorReport?.Invoke(Resources.RetentionFileReader_ReadRetention_While_reading_the_retention_from_file__an_error_occured, errorMessages);

            return retention;
        }
    }
}

