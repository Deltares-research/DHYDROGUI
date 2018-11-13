using System;
using System.Collections.Generic;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Boundary
{
    public class BoundaryLocationReader
    {
        public BoundaryLocationReader() : this(null)
        {
        }

        public BoundaryLocationReader(Action<string, IList<string>> createAndAddErrorReport) : 
            this(DelftIniFileParser.ReadFile,
                 BoundaryLocationConverter.Convert,
                 createAndAddErrorReport)
        {
        }

        public BoundaryLocationReader(Func<string, IList<DelftIniCategory>> parseFunc,
                                      Func<IList<DelftIniCategory>, IList<string>, IList<BoundaryLocation>> convertFunc,
                                      Action<string, IList<string>> createAndAddErrorReport)
        {
            this.parseFunc = parseFunc ?? throw new ArgumentException("Parser cannot be null.");
            this.convertFunc = convertFunc ?? throw new ArgumentException("Converter cannot be null.");
            this.createAndAddErrorReport = createAndAddErrorReport;
        }

        /// <summary>
        /// Read the BoundaryLocation.ini file at the specified <paramref name="filePath"/> and
        /// return any valid boundary locations within this file.
        /// </summary>
        /// <param name="filePath">The path on the file system to the boundary locations file.</param>
        /// <returns>If the file is readable, a set of valid boundary locations, else null</returns>
        public IList<BoundaryLocation> Read(string filePath)
        {
            var errorMessages = new List<string>();

            IList<DelftIniCategory> categories;
            try
            {
                categories = parseFunc.Invoke(filePath);
            }
            catch (Exception e)
            {
                errorMessages.Add(e.Message);
                createAndAddErrorReport?.Invoke("While reading the boundary locations from file, the following errors occured:", errorMessages);
                return null;
            }

            var result =  convertFunc.Invoke(categories, errorMessages);

            if (errorMessages.Count > 0)
                createAndAddErrorReport?.Invoke("While reading the boundary locations from file, the following errors occured:", errorMessages);

            return result;
        }

        private readonly Func<string, IList<DelftIniCategory>> parseFunc;
        private readonly Func<IList<DelftIniCategory>, IList<string>, IList<BoundaryLocation>> convertFunc;
        private readonly Action<string, IList<string>> createAndAddErrorReport;
    }
}
