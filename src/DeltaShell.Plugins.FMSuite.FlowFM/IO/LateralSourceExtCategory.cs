using System;
using System.Globalization;
using System.IO;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.Utils.Extensions;
using DHYDRO.Common.IO.Ini;
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
        /// <param name="iniSection"> The INI section to parse from. </param>
        /// <param name="logHandler"> Optional parameter; the log handler to report errors. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="iniSection"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the <paramref name="iniSection"/> is not a lateral section.
        /// This means that the section should have name 'Lateral'.
        /// </exception>
        /// <remarks>
        /// The section is expected to have the following properties:
        /// - 'id'
        /// - 'name'
        /// - 'nodeId' OR 'branchId' with 'chainage'
        /// - 'discharge'
        /// If a property is missing, an error will be logged.
        /// </remarks>
        public LateralSourceExtCategory(IniSection iniSection, ILogHandler logHandler = null)
        {
            Ensure.NotNull(iniSection, nameof(iniSection));
            EnsureLateralSection(iniSection, nameof(iniSection));

            this.logHandler = logHandler;
            lineNumber = iniSection.LineNumber;

            Id = iniSection.GetPropertyValueWithOptionalDefaultValue(idKey);
            Name = iniSection.GetPropertyValueWithOptionalDefaultValue(nameKey);
            NodeName = iniSection.GetPropertyValueWithOptionalDefaultValue(nodeIdKey);
            BranchName = iniSection.GetPropertyValueWithOptionalDefaultValue(branchIdKey);
            if (!string.IsNullOrEmpty(BranchName))
            {
                Chainage = iniSection.ReadProperty<double>(chainageKey);
            }

            SetDischargeData(iniSection);
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

        private static void EnsureLateralSection(IniSection iniSection, string paramName)
        {
            if (!iniSection.Name.EqualsCaseInsensitive(BndExtForceFile.LateralHeaderKey))
            {
                throw new ArgumentException($"{nameof(iniSection)} should have header {BndExtForceFile.LateralHeaderKey}" +
                                            $" for laterals.", paramName);
            }
        }

        private void SetDischargeData(IniSection iniSection)
        {
            string dischargeVal = iniSection.GetPropertyValueWithOptionalDefaultValue(dischargeKey);
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