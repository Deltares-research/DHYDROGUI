using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.XmlValidation
{
    public class Validator
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Validator));
        private XmlSchemaSet xmlSchemaSet;

        public Validator(IEnumerable<string> xmlSchemaPaths)
        {
            xmlSchemaSet = new XmlSchemaSet();
            foreach (string xmlSchemaPath in xmlSchemaPaths)
            {
                xmlSchemaSet.Add(FileToObject.ConvertToXmlSchema(xmlSchemaPath));
            }
        }

        /// <summary>
        /// Validates the XmlDocument
        /// </summary>
        /// <param name="xmlDocumentPath">ComplexType Document Path</param>
        /// <exception cref="ValidatorException">Throws ValidatorException on failure</exception>
        public void Validate(string xmlDocumentPath)
        {
            Validate(FileToObject.ConvertToXDocument(xmlDocumentPath));
        }

        /// <summary>
        /// Validates the XmlDocument
        /// </summary>
        /// <param name="xDocument">ComplexType Document</param>
        /// <exception cref="ValidatorException">Throws ValidatorException on failure</exception>
        public void Validate(XDocument xDocument)
        {
            var messages = "";
            xDocument.Validate(xmlSchemaSet, (o, e) => { messages += e.Message; }, true);

            if (messages != "")
            {
                throw new XmlException(messages);
            }
        }

        public bool IsValid(string xmlDocumentPath)
        {
            return IsValid(FileToObject.ConvertToXDocument(xmlDocumentPath));
        }

        public bool IsValid(XDocument xDocument)
        {
            try
            {
                Validate(xDocument);
            }
            catch (Exception ex)
            {
                Log.Warn(ex.Message);
                return false;
            }

            return true;
        }
    }
}