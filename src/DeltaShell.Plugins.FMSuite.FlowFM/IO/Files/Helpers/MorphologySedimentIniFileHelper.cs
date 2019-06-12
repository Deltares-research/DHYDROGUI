using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers
{
    internal static class MorphologySedimentIniFileHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MorphologySedimentIniFileHelper));

        private static readonly string FMSuiteFlowModelVersion =
            typeof(WaterFlowFMModel).Assembly.GetName().Version.ToString();

        private static string fmDllVersion;

        private static readonly string createdBy = "Deltares, FM-Suite DFlowFM Model Version " +
                                                   FMSuiteFlowModelVersion + ", DFlow FM Version " + FMDllVersion;

        private const string SEDFILEVERSION = "02.00";

        public static DelftIniCategory CreateMorpologyGeneralDelftIniCategory()
        {
            var generalCategory = new DelftIniCategory(MorphologyFile.GeneralHeader);
            AddGeneralProperties(generalCategory);
            return generalCategory;
        }

        public static DelftIniCategory CreateSedimentGeneralDelftIniCategory()
        {
            var generalCategory = new DelftIniCategory(SedimentFile.GeneralHeader);
            AddGeneralProperties(generalCategory);
            return generalCategory;
        }

        private static void AddGeneralProperties(IDelftIniCategory generalDelftIniCategory)
        {
            DateTime creationTime = DateTime.Now;
            generalDelftIniCategory.AddProperty(SedimentFile.FileCreatedBy, createdBy);
            generalDelftIniCategory.AddProperty(SedimentFile.FileCreationDate,
                                                creationTime.ToString("ddd MMM dd yyyy, HH:mm:ss"));
            generalDelftIniCategory.AddProperty(SedimentFile.FileVersion, SEDFILEVERSION);
        }

        public static DelftIniCategory CreateSedimentOverallDelftIniCategory(
            IEnumerable<ISedimentProperty> sedimentOverallProperties)
        {
            var overallDelftIniCategory = new DelftIniCategory(SedimentFile.OverallHeader);

            foreach (ISedimentProperty sedimentOverallProperty in sedimentOverallProperties)
            {
                sedimentOverallProperty.SedimentPropertyWrite(overallDelftIniCategory);
            }

            return overallDelftIniCategory;
        }

        public static IEnumerable<DelftIniCategory> CreateDelftIniCategoriesFromModelProperties(
            IEnumerable<WaterFlowFMProperty> customPropertiesOfCustomGroups)
        {
            IEnumerable<IGrouping<string, WaterFlowFMProperty>> categories =
                customPropertiesOfCustomGroups.GroupBy(p => p.PropertyDefinition.FileCategoryName.ToLower());

            foreach (IGrouping<string, WaterFlowFMProperty> category in categories)
            {
                string categoryName = category.Key;
                if (categoryName.Equals(KnownProperties.morphology))
                {
                    categoryName = MorphologyFile.Header;
                }

                var delftIniCategory = new DelftIniCategory(categoryName);

                foreach (WaterFlowFMProperty property in category)
                {
                    delftIniCategory.AddProperty(property.PropertyDefinition.FilePropertyName,
                                                 property.GetValueAsString());
                }

                yield return delftIniCategory;
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
    }
}