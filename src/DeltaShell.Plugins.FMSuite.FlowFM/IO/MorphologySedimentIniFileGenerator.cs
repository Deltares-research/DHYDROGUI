using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
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
        public static DelftIniCategory GenerateSedimentGeneralRegion()
        {
            var generalCategory = new DelftIniCategory(SedimentFile.GeneralHeader);
            AddGeneralProperties(generalCategory);
            return generalCategory;
        }

        private static void AddGeneralProperties(DelftIniCategory generalCategory)
        {
            DateTime creationTime = DateTime.Now;
            generalCategory.AddProperty(SedimentFile.FileCreatedBy, createdBy);
            generalCategory.AddProperty(SedimentFile.FileCreationDate, creationTime.ToString("ddd MMM dd yyyy, HH:mm:ss"));
            generalCategory.AddProperty(SedimentFile.FileVersion, SEDFILEVERSION);
        }

        public static DelftIniCategory GenerateMorpologyGeneralRegion()
        {
            var generalCategory = new DelftIniCategory(MorphologyFile.GeneralHeader);
            AddGeneralProperties(generalCategory);
            return generalCategory;
        }
            
        public static DelftIniCategory GenerateOverallRegion(IEnumerable<ISedimentProperty> sedimentOverallProperties)
        {
            var overallRegion = new DelftIniCategory(SedimentFile.OverallHeader);
            foreach (var sedimentOverallProperty in sedimentOverallProperties)
            {
                sedimentOverallProperty.SedimentPropertyWrite(overallRegion);
            }
            
            return overallRegion;
        }

        public static IEnumerable<DelftIniCategory> CreateDelftIniCategoriesFromProperties(IEnumerable<WaterFlowFMProperty> customPropertiesOfCustomGroups)
        {
            var categories = customPropertiesOfCustomGroups.GroupBy(p => p.PropertyDefinition.FileCategoryName);

            foreach (var category in categories)
            {
                var categoryName = category.Key;
                if (categoryName.Equals(KnownProperties.morphology))
                {
                    categoryName = MorphologyFile.Header;
                }

                var delftIniCategory = new DelftIniCategory(categoryName);

                foreach (WaterFlowFMProperty property in category)
                {
                    delftIniCategory.AddProperty(property.PropertyDefinition.FilePropertyName, property.GetValueAsString());
                }

                yield return delftIniCategory;
            }
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