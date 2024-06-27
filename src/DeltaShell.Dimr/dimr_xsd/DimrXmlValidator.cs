using System;
using System.Text;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.Dimr.DimrXsd;
using DeltaShell.Dimr.Properties;

namespace DeltaShell.Dimr.dimr_xsd
{
    /// <summary>
    /// Validator to validate .xml files for DIMR.
    /// </summary>
    public class DimrXmlValidator
    {
        private readonly ILogHandler logHandler;
        private readonly StringBuilder invalidElements;
        private bool isValidFile;
        private dimrXML dimrXml;
        private string locationMessage;

        /// <summary>
        /// Creates a new instance of the <see cref="DimrXmlValidator"/>.
        /// </summary>
        /// <param name="logHandler">The log handler to log messages with.</param>
        public DimrXmlValidator(ILogHandler logHandler)
        {
            Ensure.NotNull(logHandler, nameof(logHandler));
            
            this.logHandler = logHandler;
            invalidElements = new StringBuilder();
        }

        /// <summary>
        /// Validates the Xml file given and logs messages when the file is invalid.
        /// </summary>
        /// <param name="dimrXML">Xml file which is to be validated.</param>
        /// <param name="path">Path which will be referred to in logging.</param>
        /// <returns>True if <paramref name="dimrXML"/> is valid, else false.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="dimrXML"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="path"/> is null or whitespace.
        /// </exception>
        public bool IsValid(dimrXML dimrXML, string path)
        {
            Ensure.NotNull(dimrXML, nameof(dimrXML));
            Ensure.NotNullOrWhiteSpace(path, nameof(path));

            dimrXml = dimrXML;
            locationMessage = path;
            isValidFile = true;
            invalidElements.Clear();

            ValidateDocumentation();
            ValidateControl();
            ValidateComponent();
            ValidateCoupler();
            
            ReportLog();

            return isValidFile;
        }

        private void ValidateDocumentation()
        {
            if (dimrXml.documentation == null)
            {
                isValidFile = false;
                AddInvalidElement("Documentation");
            }
        }

        private void ValidateControl()
        {
            if (dimrXml.control == null || dimrXml.control.Length == 0)
            {
                isValidFile = false;
                AddInvalidElement("Control");
            }
        }

        private void ValidateComponent()
        {
            if (dimrXml.component == null || dimrXml.component.Length == 0)
            {
                isValidFile = false;
                AddInvalidElement("Component");
            }
        }

        private void ValidateCoupler()
        {
            if (dimrXml.component == null || dimrXml.component.Length <= 1)
            {
                return;
            }

            if (dimrXml.coupler == null || dimrXml.coupler.Length == 0)
            {
                isValidFile = false;
                AddInvalidElement("Coupler");
            }
        }
        
        private void AddInvalidElement(string element)
        {
            if (invalidElements.Length > 0)
            {
                invalidElements.Append(", ");
            }

            invalidElements.Append(element);
        }
        
        private void ReportLog()
        {
            if (!isValidFile)
            {
                logHandler.ReportError(string.Format(Resources.DimrXmlValidator_ReportLog_DIMR_xml__0__is_not_valid__It_is_missing_the_following_element_s____1_, locationMessage, invalidElements));
            }
        }
    }
}