using System;
using System.Globalization;
using System.IO;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.Utils.Extensions;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    /// <summary>
    /// Represents a delft ini category specific for lateral source data from the external forcings file.
    /// </summary>
    public class LateralSourceExtCategory : ILateralSourceExtCategory
    {
        private const string branchIdKey = "branchId";
        private const string idKey = "id";
        private const string nameKey = "name";
        private const string chainageKey = "chainage";
        private const string nodeIdKey = "nodeId";
        private const string dischargeKey = "discharge";

        private readonly ILogHandler logHandler;
        private readonly int lineNumber;

        /// <summary>
        /// Initializes a new instance of the <see cref="LateralSourceExtCategory"/> class.
        /// </summary>
        /// <param name="category"> The delft ini category to parse from. </param>
        /// <param name="logHandler"> Optional parameter; the log handler to report errors. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="category"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the <paramref name="category"/> is not a lateral category.
        /// This means that the category should have name 'Lateral'.
        /// </exception>
        /// <remarks>
        /// The category is expected to have the following properties:
        /// - 'id'
        /// - 'name'
        /// - 'nodeId' OR 'branchId' with 'chainage'
        /// - 'discharge'
        /// If a property is missing, an error will be logged.
        /// </remarks>
        public LateralSourceExtCategory(IDelftIniCategory category, ILogHandler logHandler = null)
        {
            Ensure.NotNull(category, nameof(category));
            EnsureLateralCategory(category, nameof(category));

            this.logHandler = logHandler;
            lineNumber = category.LineNumber;

            Id = category.GetPropertyValue(idKey);
            Name = category.GetPropertyValue(nameKey);
            NodeName = category.GetPropertyValue(nodeIdKey);
            BranchName = category.GetPropertyValue(branchIdKey);
            if (!string.IsNullOrEmpty(BranchName))
            {
                Chainage = category.ReadProperty<double>(chainageKey);
            }

            SetDischargeData(category);
        }

        /// <summary>
        /// The id of the lateral source.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The name of the lateral source.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The name of the node the lateral source is on.
        /// </summary>
        public string NodeName { get; }

        /// <summary>
        /// The name of the branch the lateral source is on.
        /// </summary>
        public string BranchName { get; }

        /// <summary>
        /// The chainage of the lateral source on the branch it is on.
        /// </summary>
        public double Chainage { get; } = double.NaN;

        /// <summary>
        /// The name of the boundary conditions file with the discharge data.
        /// </summary>
        public string DischargeFile { get; private set; }

        /// <summary>
        /// The constant discharge.
        /// </summary>
        public double Discharge { get; private set; } = double.NaN;

        private static void EnsureLateralCategory(IDelftIniCategory category, string paramName)
        {
            if (!category.Name.EqualsCaseInsensitive(BndExtForceFile.LateralHeaderKey))
            {
                throw new ArgumentException("The category is not a lateral category.", paramName);
            }
        }

        private void SetDischargeData(IDelftIniCategory category)
        {
            string dischargeVal = category.GetPropertyValue(dischargeKey);
            if (TryParseDouble(dischargeVal, out double discharge))
            {
                Discharge = discharge;
            }
            else if (Path.GetExtension(dischargeVal).EqualsCaseInsensitive(BcFile.Extension))
            {
                DischargeFile = dischargeVal;
            }
        }

        private bool TryParseDouble(string doubleString, out double doubleVal)
        {
            const NumberStyles numberStyle = NumberStyles.AllowLeadingWhite |
                                             NumberStyles.AllowTrailingWhite |
                                             NumberStyles.AllowLeadingSign |
                                             NumberStyles.AllowDecimalPoint |
                                             NumberStyles.AllowThousands |
                                             NumberStyles.AllowExponent;

            if (double.TryParse(doubleString, numberStyle, CultureInfo.InvariantCulture, out double doubleVal2))
            {
                doubleVal = doubleVal2;
                return true;
            }

            logHandler?.ReportError($"Cannot parse '{doubleString}' to a double, see category on line {lineNumber}.");
            doubleVal = doubleVal2;
            return false;
        }
    }
}