using System;
using System.Collections.Generic;
using System.Linq;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers
{
    internal static class MorphologySedimentIniFileHelper
    {
        private const string SEDFILEVERSION = "02.00";
        private static readonly ILog Log = LogManager.GetLogger(typeof(MorphologySedimentIniFileHelper));

        private static readonly string FMSuiteFlowModelVersion =
            typeof(WaterFlowFMModel).Assembly.GetName().Version.ToString();

        private static string fmDllVersion;

        private static readonly string createdBy = "Deltares, FM-Suite DFlowFM Model Version " +
                                                   FMSuiteFlowModelVersion + ", DFlow FM Version " + FMDllVersion;

        public static IniSection CreateMorphologyGeneralSection()
        {
            var generalCategory = new IniSection(MorphologyFile.GeneralHeader);
            AddGeneralProperties(generalCategory);
            return generalCategory;
        }

        public static IniSection CreateSedimentGeneralSection()
        {
            var generalSection = new IniSection(SedimentFile.GeneralHeader);
            AddGeneralProperties(generalSection);
            return generalSection;
        }

        public static IniSection CreateSedimentOverallSection(IEnumerable<ISedimentProperty> sedimentOverallProperties)
        {
            var overallSection = new IniSection(SedimentFile.OverallHeader);

            foreach (ISedimentProperty sedimentOverallProperty in sedimentOverallProperties)
            {
                sedimentOverallProperty.SedimentPropertyWrite(overallSection);
            }

            return overallSection;
        }

        public static IEnumerable<IniSection> CreateSectionsFromModelProperties(
            IEnumerable<WaterFlowFMProperty> customPropertiesOfCustomGroups)
        {
            IEnumerable<IGrouping<string, WaterFlowFMProperty>> categories =
                customPropertiesOfCustomGroups.GroupBy(p => p.PropertyDefinition.FileSectionName);

            foreach (IGrouping<string, WaterFlowFMProperty> category in categories)
            {
                string categoryName = category.Key;
                if (categoryName.Equals(KnownProperties.morphology))
                {
                    categoryName = MorphologyFile.Header;
                }

                var section = new IniSection(categoryName);

                foreach (WaterFlowFMProperty property in category)
                {
                    section.AddProperty(property.PropertyDefinition.FilePropertyKey, property.GetValueAsString());
                }

                yield return section;
            }
        }

        private static string FMDllVersion
        {
            get
            {
                if (fmDllVersion != null)
                {
                    return fmDllVersion; // do it once
                }

                fmDllVersion = "Unknown";

                IFlexibleMeshModelApi api = FlexibleMeshModelApiFactory.CreateNew();
                if (api == null)
                {
                    return fmDllVersion;
                }

                using (api)
                {
                    try
                    {
                        fmDllVersion = api.GetVersionString();
                    }
                    catch (Exception ex)
                    {
                        string exception = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                        Log.ErrorFormat("Error retrieving FM Dll version: {0}", exception);

                        fmDllVersion = "Unknown";
                    }
                }

                return fmDllVersion;
            }
        }

        private static void AddGeneralProperties(IniSection generalSection)
        {
            DateTime creationTime = DateTime.Now;
            generalSection.AddProperty(SedimentFile.FileCreatedBy, createdBy);
            generalSection.AddProperty(SedimentFile.FileCreationDate,
                                                creationTime.ToString("ddd MMM dd yyyy, HH:mm:ss"));
            generalSection.AddProperty(SedimentFile.FileVersion, SEDFILEVERSION);
        }
    }
}