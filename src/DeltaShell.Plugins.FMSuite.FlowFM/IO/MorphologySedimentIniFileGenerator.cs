using System;
using System.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DHYDRO.Common.IO.Ini;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    internal static class MorphologySedimentIniFileGenerator
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MorphologySedimentIniFileGenerator));
        private static readonly string FMSuiteFlowModelVersion = typeof(WaterFlowFMModel).Assembly.GetName().Version.ToString();
        private static string fmDllVersion;

        private static readonly string createdBy = "Deltares, FM-Suite DFlowFM Model Version " + FMSuiteFlowModelVersion + ", DFlow FM Version " + FMDllVersion;

        private const string SEDFILEVERSION = "02.00";
        public static IniSection GenerateSedimentGeneralRegion()
        {
            var generalIniSection = new IniSection(SedimentFile.GeneralHeader);
            AddGeneralProperties(generalIniSection);
            return generalIniSection;
        }

        private static void AddGeneralProperties(IniSection generalIniSection)
        {
            DateTime creationTime = DateTime.Now;
            generalIniSection.AddPropertyWithOptionalComment(SedimentFile.FileCreatedBy, createdBy);
            generalIniSection.AddPropertyWithOptionalComment(SedimentFile.FileCreationDate, creationTime.ToString("ddd MMM dd yyyy, HH:mm:ss"));
            generalIniSection.AddPropertyWithOptionalComment(SedimentFile.FileVersion, SEDFILEVERSION);
        }

        public static IniSection GenerateMorpologyGeneralRegion()
        {
            var generalIniSection = new IniSection(MorphologyFile.GeneralHeader);
            AddGeneralProperties(generalIniSection);
            return generalIniSection;
        }
            
        public static IniSection GenerateOverallRegion(IEnumerable<ISedimentProperty> sedimentOverallProperties)
        {
            var overallRegion = new IniSection(SedimentFile.OverallHeader);
            foreach (var sedimentOverallProperty in sedimentOverallProperties)
            {
                sedimentOverallProperty.SedimentPropertyWrite(overallRegion);
            }
            
            return overallRegion;
        }

        private static string FMDllVersion
        {
            get
            {
                if (fmDllVersion != null)
                    return fmDllVersion; // do it once

                fmDllVersion = "Unknown";

                var api = FlexibleMeshModelApiFactory.CreateNew();
                if (api == null) return fmDllVersion;

                using (api)
                {
                    try
                    {
                        fmDllVersion = api.GetVersionString();
                    }
                    catch (Exception ex)
                    {
                        var exception = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                        Log.ErrorFormat("Error retrieving FM Dll version: {0}", exception);

                        fmDllVersion = "Unknown";
                    }
                }

                return fmDllVersion;
            }
        }
    }
}