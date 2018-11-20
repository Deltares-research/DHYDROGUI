using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Dimr;
using DeltaShell.Dimr.xsd;
using DeltaShell.NGHS.IO.FileConverters;
using log4net;

namespace DeltaShell.NGHS.IO.FileReaders.ConfigXml
{
    public static class DelftConfigXmlFileReader
    {
        public static readonly ILog Log = LogManager.GetLogger(typeof(DelftConfigXmlFileReader));
        private static readonly Action<string, IList<string>> createAndAddErrorReport;
        public static object dataAccessModel;

        public static IDimrModel dimrModel;
        public static ICompositeActivity hydroModel;
        public static dimrXML dimrDataModel;
        //public static IDimrConfigModelCoupler modelCoupler;

        public static object Read(string xmlFileSource)
        {
            if (string.IsNullOrEmpty(xmlFileSource)) { throw new FileReadingException("Configuration file cannot be found"); }

            XDocument xmlConfigFile;
            string rootName;
            var errorMessages = new List<string>();
            try
            {
                xmlConfigFile = XDocument.Load(xmlFileSource);
                rootName = xmlConfigFile?.Root?.Name.LocalName;
            }
            catch
            {
                throw new XmlException("Unable to parse file");
            }

            var reader = xmlConfigFile?.Root?.CreateReader();

            dataAccessModel = DelftXmlFileConverter.Convert(reader, rootName, errorMessages );

            //CreateErrorReport(errorMessages);
            //LogErrorReport(errorMessages, report => Log.Warn(report));
            Log.Warn(errorMessages);
            dimrDataModel = (dimrXML)dataAccessModel;
       
            CreateControls();
            CreateCouplers();
            CreateComponents();
            
            return dataAccessModel;
        }

        //private static void CreateErrorReport(List<string> errorMessages)
        //{
        //    if (errorMessages.Count > 0)
        //        createAndAddErrorReport?.Invoke($"While reading the from file , the following errors occured", errorMessages);
        //    Log.Warn(createAndAddErrorReport);
        //}

        private static void LogErrorReport(List<string> errorReport, Action<string> logAction)
        {
            errorReport.ForEach(logAction);
        }

        private static void CreateComponents()
        {
            var components = dimrDataModel.component.ToList();
            //dimrModel.LibraryName = components.ElementAt(0).library;
            //dimrModel.Name = components.ElementAt(0).name;
            //dimrModel.DirectoryName = components.ElementAt(0).workingDir;
            //dimrModel.InputFile = components.ElementAt(0).inputFile;
        }

        private static void CreateCouplers()
        {
            var couplers = dimrDataModel.coupler.ToList();
          

        }

        private static void CreateControls()
        {
            var controls = dimrDataModel.control.ToList();
            //hydroModel.CurrentWorkflow = dimrDataModel.control.ElementAt(0);

        }
    }
}
