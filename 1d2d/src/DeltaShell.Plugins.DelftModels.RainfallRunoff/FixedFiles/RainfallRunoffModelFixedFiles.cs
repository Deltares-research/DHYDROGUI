using System;
using System.IO;
using System.Reflection;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.FixedFiles
{
    public class RainfallRunoffModelFixedFiles
    {
        private const string UnpavedStorageCoeffFileTag = "UnpavedStorageCoeffFile";
        private const string UnpavedCropTypesFileTag = "UnpavedCropTypesFile";
        private const string GreenhouseClassesFileTag = "GreenhouseClassesFile";
        private const string GreenhouseStorageFileTag = "GreenhouseStorageFile";
        private const string GreenhouseUsageFileTag = "GreenhouseUsageFile";
        private const string OpenWaterCropFactorFileTag = "OpenWaterCropFactorFile";

        public const string UnpavedCropFactors = "Unpaved crop factors";
        public const string UnpavedStorageCoefficient = "Unpaved storage coefficient";
        public const string GreenhouseClasses = "Greenhouse classes";
        public const string GreenhouseStorage = "Greenhouse storage";
        public const string GreenhouseUsage = "Greenhouse usage";
        public const string OpenWaterCropFactor = "Open water 'crop' factor";

        private const string CropFactFileName = "CROPFACT";
        private const string BergCoefFileName = "BERGCOEF";
        private const string KasKlassFileName = "KASKLASS";
        private const string KasInitFileName = "KASINIT";
        private const string KasGebrFileName = "KASGEBR";
        private const string OpenWaterCropFactFileName = "CROP_OW.PRN";
        private readonly Action<object, string, DataItemRole, string> addDataItemDelegate;
        private readonly RainfallRunoffModel model;

        public RainfallRunoffModelFixedFiles(RainfallRunoffModel model,
                                             Action<object, string, DataItemRole, string> addDataItemDelegate)
        {
            this.model = model;
            this.addDataItemDelegate = addDataItemDelegate;
            Initialize();
        }

        public TextDocument UnpavedCropFactorsFile
        {
            get { return (TextDocument) model.GetDataItemByTag(UnpavedCropTypesFileTag).Value; }
            set
            {
                if (value == UnpavedCropFactorsFile)
                {
                    return;
                }
                model.GetDataItemByTag(UnpavedCropTypesFileTag).Value = value;
            }
        }

        public TextDocument UnpavedStorageCoeffFile
        {
            get { return (TextDocument) model.GetDataItemByTag(UnpavedStorageCoeffFileTag).Value; }
            set
            {
                if (value == UnpavedStorageCoeffFile)
                {
                    return;
                }
                model.GetDataItemByTag(UnpavedStorageCoeffFileTag).Value = value;
            }
        }

        public TextDocument GreenhouseClassesFile
        {
            get { return (TextDocument) model.GetDataItemByTag(GreenhouseClassesFileTag).Value; }
            set
            {
                if (value == GreenhouseClassesFile)
                {
                    return;
                }
                model.GetDataItemByTag(GreenhouseClassesFileTag).Value = value;
            }
        }

        public TextDocument GreenhouseStorageFile
        {
            get { return (TextDocument) model.GetDataItemByTag(GreenhouseStorageFileTag).Value; }
            set
            {
                if (value == GreenhouseStorageFile)
                {
                    return;
                }
                model.GetDataItemByTag(GreenhouseStorageFileTag).Value = value;
            }
        }

        public TextDocument GreenhouseUsageFile
        {
            get { return (TextDocument) model.GetDataItemByTag(GreenhouseUsageFileTag).Value; }
            set
            {
                if (value == GreenhouseUsageFile)
                {
                    return;
                }
                model.GetDataItemByTag(GreenhouseUsageFileTag).Value = value;
            }
        }

        public TextDocument OpenWaterCropFactorFile
        {
            get { return (TextDocument) model.GetDataItemByTag(OpenWaterCropFactorFileTag).Value; }
            set
            {
                if (value == OpenWaterCropFactorFile)
                {
                    return;
                }
                model.GetDataItemByTag(OpenWaterCropFactorFileTag).Value = value;
            }
        }

        private void Initialize()
        {
            var cropTypes = new TextDocument(false) {Name = CropFactFileName};
            cropTypes.Content = ReadFixedFileFromResource(CropFactFileName);
            addDataItemDelegate(cropTypes, cropTypes.Name, DataItemRole.None, UnpavedCropTypesFileTag);

            var storageCoeff = new TextDocument(false) {Name = BergCoefFileName};
            storageCoeff.Content = ReadFixedFileFromResource(BergCoefFileName);
            addDataItemDelegate(storageCoeff, storageCoeff.Name, DataItemRole.None, UnpavedStorageCoeffFileTag);

            var greenhouseClasses = new TextDocument(false) {Name = KasKlassFileName};
            greenhouseClasses.Content = ReadFixedFileFromResource(KasKlassFileName);
            addDataItemDelegate(greenhouseClasses, greenhouseClasses.Name, DataItemRole.None, GreenhouseClassesFileTag);

            var greenhouseStorage = new TextDocument(false) {Name = KasInitFileName};
            greenhouseStorage.Content = ReadFixedFileFromResource(KasInitFileName);
            addDataItemDelegate(greenhouseStorage, greenhouseStorage.Name, DataItemRole.None, GreenhouseStorageFileTag);

            var greenhouseUsage = new TextDocument(false) {Name = KasGebrFileName};
            greenhouseUsage.Content = ReadFixedFileFromResource(KasGebrFileName);
            addDataItemDelegate(greenhouseUsage, greenhouseUsage.Name, DataItemRole.None, GreenhouseUsageFileTag);

            var openWaterCropFactor = new TextDocument(false) {Name = OpenWaterCropFactFileName};
            openWaterCropFactor.Content = ReadFixedFileFromResource(OpenWaterCropFactFileName);
            addDataItemDelegate(openWaterCropFactor, openWaterCropFactor.Name, DataItemRole.None,
                                OpenWaterCropFactorFileTag);
        }

        public static string ReadFixedFileFromResource(string fileName)
        {
            Type type = typeof (RainfallRunoffModelFixedFiles);
            string currentNamespace = type.Namespace;
            Assembly assembly = type.Assembly;

            string resourceName = String.Format("{0}.{1}", currentNamespace, fileName);

            Stream resourceContents = assembly.GetManifestResourceStream(resourceName);
            if (resourceContents != null)
            {
                var streamReader = new StreamReader(resourceContents);
                return streamReader.ReadToEnd();
            }
            return null;
        }
    }
}