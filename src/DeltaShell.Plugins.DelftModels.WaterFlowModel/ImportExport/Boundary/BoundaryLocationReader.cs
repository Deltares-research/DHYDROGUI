using System;
using System.Collections.Generic;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Helpers;


namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Boundary
{
    /// <summary>
    /// BoundaryLocationReader is responsible for parsing and extracting the
    /// BoundaryLocations from a boundary location file. Reporting any
    /// exceptions encountered through the createAndErrorReport.
    /// </summary>
    public class BoundaryLocationReader
    {
        /// <summary>
        /// Construct a new BoundaryLocationReader with a DelftIniParser.ReadFile,
        /// a BoundaryLocation.Convert, and an empty createAndAddErrorReport.
        /// </summary>
        public BoundaryLocationReader() : this(null)
        { }

        /// <summary>
        /// Construct a new BoundaryLocationReader with the createAndAddErrorReport.
        /// </summary>
        /// <param name="createAndAddErrorReport"> The action used to report any errors encountered. </param>
        public BoundaryLocationReader(Action<string, IList<string>> createAndAddErrorReport) : 
            this(DelftIniFileParser.ReadFile,
                 BoundaryLocationConverter.Convert,
                 createAndAddErrorReport)
        { }

        /// <summary>
        /// Construct a new BoundaryLocationReader with the given functions.
        /// </summary>
        /// <param name="parseFunc"> The function used for parsing the BoundaryLocation.ini file. </param>
        /// <param name="convertFunc"> The function used to extracting the relevant values from the dataAccessModel. </param>
        /// <param name="createAndAddErrorReport"> The action used to report any errors encountered. </param>
        /// <exception cref="ArgumentException"> parseFunc == null || convertFunc == null </exception>
        public BoundaryLocationReader(Func<string, IList<DelftIniCategory>> parseFunc,
                                      Func<IList<DelftIniCategory>, IList<string>, IList<BoundaryLocation>> convertFunc,
                                      Action<string, IList<string>> createAndAddErrorReport)
        {
            if (parseFunc != null) this.parseFunc = parseFunc;
            else throw new ArgumentException("Parser cannot be null.");

            if (convertFunc != null) this.convertFunc = convertFunc;
            else throw new ArgumentException("Converter cannot be null.");

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

        /// <summary> Function used to parse the BoundaryLocation.ini file to dataAccessModel. </summary>
        private readonly Func<string, IList<DelftIniCategory>> parseFunc;
        /// <summary> Function used to extract the BoundaryLocations from the dataAccessModel. </summary>
        private readonly Func<IList<DelftIniCategory>, IList<string>, IList<BoundaryLocation>> convertFunc;
        /// <summary> Action used to report any errors encountered during reading. </summary>
        private readonly Action<string, IList<string>> createAndAddErrorReport;
    }
}
