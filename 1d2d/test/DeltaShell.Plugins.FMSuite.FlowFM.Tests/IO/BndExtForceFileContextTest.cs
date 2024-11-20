using System.IO;
using DelftTools.Hydro;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class BndExtForceFileContextTest
    {
        private const string modelName = "some_model_name";
        private const string feature2DName = "some_feature_2d";

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void SetModelName_ValueIsNullOrWhiteSpace_ThrowsArgumentException(string arg)
        {
            BndExtForceFileContext context = CreateContext();
            Assert.That(() => context.ModelName = arg, Throws.ArgumentException);
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void SetRoofAreaFileName_ValueIsNullOrWhiteSpace_ThrowsArgumentException(string arg)
        {
            BndExtForceFileContext context = CreateContext();
            Assert.That(() => context.RoofAreaFileName = arg, Throws.ArgumentException);
        }

        [Test]
        public void GetRoofAreaFileName_ValueIsUnset_ReturnsDefaultName()
        {
            BndExtForceFileContext context = CreateContext();

            var fileName = $"{modelName}_roofs.pol";

            Assert.That(context.RoofAreaFileName, Is.EqualTo(fileName));
        }

        [Test]
        public void AddForcingFileNameForFmMeteoField_DataIsNull_ThrowsArgumentNullException()
        {
            BndExtForceFileContext context = CreateContext();
            string fileName = GetFileName();

            Assert.That(() => context.AddForcingFileName((IFmMeteoField)null, fileName), Throws.ArgumentNullException);
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void AddForcingFileNameForFmMeteoField_FileNameIsNullOrWhiteSpace_ThrowsArgumentException(string arg)
        {
            BndExtForceFileContext context = CreateContext();
            IFmMeteoField data = CreateMeteoField();

            Assert.That(() => context.AddForcingFileName(data, arg), Throws.ArgumentException);
        }

        [Test]
        public void AddForcingFileNameForFmMeteoField_FileNameIsAdded()
        {
            BndExtForceFileContext context = CreateContext();
            IFmMeteoField data = CreateMeteoField();
            string fileName = GetFileName();

            context.AddForcingFileName(data, fileName);

            Assert.That(context.ForcingFileNames, Does.Contain(fileName));
        }

        [Test]
        public void AddForcingFileNameForModel1DBoundaryNodeData_DataIsNull_ThrowsArgumentNullException()
        {
            BndExtForceFileContext context = CreateContext();
            string fileName = GetFileName();

            Assert.That(() => context.AddForcingFileName((Model1DBoundaryNodeData)null, fileName), Throws.ArgumentNullException);
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void AddForcingFileNameForModel1DBoundaryNodeData_FileNameIsNullOrWhiteSpace_ThrowsArgumentException(string arg)
        {
            BndExtForceFileContext context = CreateContext();
            Model1DBoundaryNodeData data = CreateModel1DBoundaryNodeData();

            Assert.That(() => context.AddForcingFileName(data, arg), Throws.ArgumentException);
        }

        [Test]
        public void AddForcingFileNameForModel1DBoundaryNodeData_FileNameIsAdded()
        {
            BndExtForceFileContext context = CreateContext();
            Model1DBoundaryNodeData data = CreateModel1DBoundaryNodeData();
            string fileName = GetFileName();

            context.AddForcingFileName(data, fileName);

            Assert.That(context.ForcingFileNames, Does.Contain(fileName));
        }

        [Test]
        public void AddForcingFileNameForModel1DLateralSourceData_DataIsNull_ThrowsArgumentNullException()
        {
            BndExtForceFileContext context = CreateContext();
            string fileName = GetFileName();

            Assert.That(() => context.AddForcingFileName((Model1DLateralSourceData)null, fileName), Throws.ArgumentNullException);
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void AddForcingFileNameForModel1DLateralSourceData_FileNameIsNullOrWhiteSpace_ThrowsArgumentException(string arg)
        {
            BndExtForceFileContext context = CreateContext();
            Model1DLateralSourceData data = CreateModel1DLateralSourceData();

            Assert.That(() => context.AddForcingFileName(data, arg), Throws.ArgumentException);
        }

        [Test]
        public void AddForcingFileNameForModel1DLateralSourceData_FileNameIsAdded()
        {
            BndExtForceFileContext context = CreateContext();
            Model1DLateralSourceData data = CreateModel1DLateralSourceData();
            string fileName = GetFileName();

            context.AddForcingFileName(data, fileName);

            Assert.That(context.ForcingFileNames, Does.Contain(fileName));
        }

        [Test]
        public void AddIniSectionForBoundaryCondition_DataIsNull_ThrowsArgumentNullException()
        {
            BndExtForceFileContext context = CreateContext();
            IniSection iniSection = CreateIniSection();

            Assert.That(() => context.AddIniSection(null, iniSection), Throws.ArgumentNullException);
        }

        [Test]
        public void AddIniSectionForBoundaryCondition_IniSectionIsNull_ThrowsArgumentNullException()
        {
            BndExtForceFileContext context = CreateContext();
            IBoundaryCondition data = CreateBoundaryCondition();

            Assert.That(() => context.AddIniSection(data, null), Throws.ArgumentNullException);
        }

        [Test]
        public void AddPolylineFileName_FeatureIsNull_ThrowsArgumentNullException()
        {
            BndExtForceFileContext context = CreateContext();
            string fileName = GetFileName();

            Assert.That(() => context.AddPolylineFileName(null, fileName), Throws.ArgumentNullException);
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void AddPolylineFileName_FileNameIsNullOrWhiteSpace_ThrowsArgumentException(string arg)
        {
            BndExtForceFileContext context = CreateContext();
            IFeature feature = CreateFeature();

            Assert.That(() => context.AddPolylineFileName(feature, arg), Throws.ArgumentException);
        }

        [Test]
        public void AddPolylineFileName_FileNameIsAdded()
        {
            BndExtForceFileContext context = CreateContext();
            IFeature feature = CreateFeature();
            string fileName = GetFileName();

            context.AddPolylineFileName(feature, fileName);

            Assert.That(context.PolylineFileNames, Does.Contain(fileName));
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void GetFeatureForPolylineFileName_FileNameIsNullOrEmpty_ThrowsArgumentException(string arg)
        {
            BndExtForceFileContext context = CreateContext();

            Assert.That(() => context.GetFeatureForPolylineFileName(arg), Throws.ArgumentException);
        }

        [Test]
        public void GetFeatureForPolylineFileName_WithCorrespondingFeature_ReturnsFirstMatchingCorrectFeature()
        {
            BndExtForceFileContext context = CreateContext();
            IFeature feature1 = CreateFeature();
            IFeature feature2 = CreateFeature();
            string fileName = GetFileName();

            context.AddPolylineFileName(feature1, fileName);
            context.AddPolylineFileName(feature2, fileName);
            IFeature retrievedFeature = context.GetFeatureForPolylineFileName(fileName);

            Assert.That(retrievedFeature, Is.SameAs(feature1));
        }

        [Test]
        public void GetFeatureForPolylineFileName_WithoutCorrespondingFeature_ReturnsNull()
        {
            BndExtForceFileContext context = CreateContext();
            string fileName = GetFileName();

            IFeature retrievedFeature = context.GetFeatureForPolylineFileName(fileName);

            Assert.That(retrievedFeature, Is.Null);
        }

        [Test]
        public void GetForcingFileNameForFmMeteoField_DataIsNull_ThrowsArgumentNullException()
        {
            BndExtForceFileContext context = CreateContext();

            Assert.That(() => context.GetForcingFileName((IFmMeteoField)null), Throws.ArgumentNullException);
        }

        [Test]
        public void GetForcingFileNameForFmMeteoField_WithCorrespondingData_ReturnsCorrectFileName()
        {
            BndExtForceFileContext context = CreateContext();
            IFmMeteoField data = CreateMeteoField();
            string fileName = GetFileName();

            context.AddForcingFileName(data, fileName);
            string retrievedFileName = context.GetForcingFileName(data);

            Assert.That(retrievedFileName, Is.EqualTo(fileName));
        }

        [Test]
        public void GetForcingFileNameForFmMeteoField_WithoutCorrespondingData_ReturnsDefaultFileName()
        {
            BndExtForceFileContext context = CreateContext();
            IFmMeteoField data = CreateMeteoField();

            string retrievedFileName = context.GetForcingFileName(data);

            Assert.That(retrievedFileName, Is.EqualTo($"{modelName}_meteo.bc"));
        }

        [Test]
        public void GetForcingFileNameForModel1DBoundaryNodeData_DataIsNull_ThrowsArgumentNullException()
        {
            BndExtForceFileContext context = CreateContext();

            Assert.That(() => context.GetForcingFileName((Model1DBoundaryNodeData)null), Throws.ArgumentNullException);
        }

        [Test]
        public void GetForcingFileNameForModel1DBoundaryNodeData_WithCorrespondingData_ReturnsCorrectFileName()
        {
            BndExtForceFileContext context = CreateContext();
            Model1DBoundaryNodeData data = CreateModel1DBoundaryNodeData();
            string fileName = GetFileName();

            context.AddForcingFileName(data, fileName);
            string retrievedFileName = context.GetForcingFileName(data);

            Assert.That(retrievedFileName, Is.EqualTo(fileName));
        }

        [Test]
        public void GetForcingFileNameForModel1DBoundaryNodeData_WithoutCorrespondingData_ReturnsDefaultFileName()
        {
            BndExtForceFileContext context = CreateContext();
            Model1DBoundaryNodeData data = CreateModel1DBoundaryNodeData();

            string retrievedFileName = context.GetForcingFileName(data);

            Assert.That(retrievedFileName, Is.EqualTo($"{modelName}_boundaryconditions1d.bc"));
        }

        [Test]
        public void GetForcingFileNameForModel1DLateralSourceData_DataIsNull_ThrowsArgumentNullException()
        {
            BndExtForceFileContext context = CreateContext();

            Assert.That(() => context.GetForcingFileName((Model1DLateralSourceData)null), Throws.ArgumentNullException);
        }

        [Test]
        public void GetForcingFileNameForModel1DLateralSourceData_WithCorrespondingData_ReturnsCorrectFileName()
        {
            BndExtForceFileContext context = CreateContext();
            Model1DLateralSourceData data = CreateModel1DLateralSourceData();
            string fileName = GetFileName();

            context.AddForcingFileName(data, fileName);
            string retrievedFileName = context.GetForcingFileName(data);

            Assert.That(retrievedFileName, Is.EqualTo(fileName));
        }

        [Test]
        public void GetForcingFileNameForModel1DLateralSourceData_WithoutCorrespondingData_ReturnsDefaultFileName()
        {
            BndExtForceFileContext context = CreateContext();
            Model1DLateralSourceData data = CreateModel1DLateralSourceData();

            string retrievedFileName = context.GetForcingFileName(data);

            Assert.That(retrievedFileName, Is.EqualTo($"{modelName}_lateral_sources.bc"));
        }

        [Test]
        public void GetIniSectionForBoundaryCondition_DataIsNull_ThrowsArgumentNullException()
        {
            BndExtForceFileContext context = CreateContext();

            Assert.That(() => context.GetIniSection(null), Throws.ArgumentNullException);
        }

        [Test]
        public void GetIniSectionForBoundaryCondition_WithCorrespondingData_ReturnsCorrectSection()
        {
            BndExtForceFileContext context = CreateContext();
            IBoundaryCondition data = CreateBoundaryCondition();
            IniSection iniSection = CreateIniSection();

            context.AddIniSection(data, iniSection);
            IniSection retrievedIniSection = context.GetIniSection(data);

            Assert.That(retrievedIniSection, Is.SameAs(iniSection));
        }

        [Test]
        public void GetIniSectionForBoundaryCondition_WithoutCorrespondingData_ReturnsNull()
        {
            BndExtForceFileContext context = CreateContext();
            IBoundaryCondition data = CreateBoundaryCondition();

            IniSection retrievedIniSection = context.GetIniSection(data);

            Assert.That(retrievedIniSection, Is.Null);
        }

        [Test]
        public void GetPolylineFileName_FeatureIsNull_ThrowsArgumentNullException()
        {
            BndExtForceFileContext context = CreateContext();

            Assert.That(() => context.GetPolylineFileName((IFeature)null), Throws.ArgumentNullException);
        }

        [Test]
        public void GetPolylineFileName_WithCorrespondingData_ReturnsCorrectFileName()
        {
            BndExtForceFileContext context = CreateContext();
            IFeature feature = CreateFeature();
            string fileName = GetFileName();

            context.AddPolylineFileName(feature, fileName);
            string retrievedFileName = context.GetPolylineFileName(feature);

            Assert.That(retrievedFileName, Is.EqualTo(fileName));
        }

        [Test]
        public void GetPolylineFileName_WithoutCorrespondingData_ReturnsNull()
        {
            BndExtForceFileContext context = CreateContext();
            IFeature feature = CreateFeature();

            string retrievedFileName = context.GetPolylineFileName(feature);

            Assert.That(retrievedFileName, Is.Null);
        }

        [Test]
        public void GetPolylineFileName_BoundaryConditionSetIsNull_ThrowsArgumentNullException()
        {
            BndExtForceFileContext context = CreateContext();

            Assert.That(() => context.GetPolylineFileName((BoundaryConditionSet)null), Throws.ArgumentNullException);
        }

        [Test]
        public void GetPolylineFileName_BoundaryConditionSetFeatureIsNull_ThrowsArgumentNullException()
        {
            BndExtForceFileContext context = CreateContext();

            Assert.That(() => context.GetPolylineFileName(new BoundaryConditionSet()), Throws.ArgumentNullException);
        }

        [Test]
        public void GetPolylineFileName_WithCorrespondingBoundaryConditionSetFeature_ReturnsCorrectFileName()
        {
            BndExtForceFileContext context = CreateContext();
            BoundaryConditionSet data = CreateBoundaryConditionSet();
            string fileName = GetFileName();

            context.AddPolylineFileName(data.Feature, fileName);
            string retrievedFileName = context.GetPolylineFileName(data);

            Assert.That(retrievedFileName, Is.EqualTo(fileName));
        }

        [Test]
        public void GetPolylineFileName_WithoutBoundaryConditionSetFeature_ReturnsDefaultFileName()
        {
            BndExtForceFileContext context = CreateContext();
            BoundaryConditionSet data = CreateBoundaryConditionSet();

            string retrievedFileName = context.GetPolylineFileName(data);

            Assert.That(retrievedFileName, Is.EqualTo($"{data.Feature.Name}.pli"));
        }

        [Test]
        public void GetPolylineFileName_EmbanktmentIsNull_ThrowsArgumentNullException()
        {
            BndExtForceFileContext context = CreateContext();

            Assert.That(() => context.GetPolylineFileName((Embankment)null), Throws.ArgumentNullException);
        }

        [Test]
        public void GetPolylineFileName_WithCorrespondingEmbanktment_ReturnsCorrectFileName()
        {
            BndExtForceFileContext context = CreateContext();
            Embankment data = CreateEmbankment();
            string fileName = GetFileName();

            context.AddPolylineFileName(data, fileName);
            string retrievedFileName = context.GetPolylineFileName(data);

            Assert.That(retrievedFileName, Is.EqualTo(fileName));
        }

        [Test]
        public void GetPolylineFileName_WithoutCorrespondingEmbanktment_ReturnsDefaultFileName()
        {
            BndExtForceFileContext context = CreateContext();
            Embankment data = CreateEmbankment();

            string retrievedFileName = context.GetPolylineFileName(data);

            Assert.That(retrievedFileName, Is.EqualTo($"{data.Name}_bnk.pliz"));
        }

        [Test]
        public void GetPolylineFileName_MeteoFieldIsNull_ThrowsArgumentNullException()
        {
            BndExtForceFileContext context = CreateContext();

            Assert.That(() => context.GetPolylineFileName((IFmMeteoField)null), Throws.ArgumentNullException);
        }

        [Test]
        public void GetPolylineFileName_MeteoFieldFeatureDataIsNull_ThrowsArgumentNullException()
        {
            BndExtForceFileContext context = CreateContext();
            var meteoField = Substitute.For<IFmMeteoField>();
            meteoField.FeatureData = null;

            Assert.That(() => context.GetPolylineFileName(meteoField), Throws.ArgumentNullException);
        }

        [Test]
        public void GetPolylineFileName_MeteoFieldFeatureIsNull_ThrowsArgumentNullException()
        {
            BndExtForceFileContext context = CreateContext();
            var meteoField = Substitute.For<IFmMeteoField>();
            meteoField.FeatureData.Feature = null;

            Assert.That(() => context.GetPolylineFileName(meteoField), Throws.ArgumentNullException);
        }

        [Test]
        public void GetPolylineFileName_WithCorrespondingMeteoField_ReturnsCorrectFileName()
        {
            BndExtForceFileContext context = CreateContext();
            IFmMeteoField data = CreateMeteoField();
            string fileName = GetFileName();

            context.AddPolylineFileName(data.FeatureData.Feature, fileName);
            string retrievedFileName = context.GetPolylineFileName(data);

            Assert.That(retrievedFileName, Is.EqualTo(fileName));
        }

        [Test]
        public void GetPolylineFileName_WithoutCorrespondingMeteoField_ReturnsDefaultFileName()
        {
            BndExtForceFileContext context = CreateContext();
            IFmMeteoField data = CreateMeteoField();

            string retrievedFileName = context.GetPolylineFileName(data);

            Assert.That(retrievedFileName, Is.EqualTo($"{feature2DName}.pli"));
        }

        [Test]
        public void Clear_ClearsAllData()
        {
            BndExtForceFileContext context = CreateContext();

            context.AddIniSection(CreateBoundaryCondition(), CreateIniSection());
            context.AddPolylineFileName(CreateFeature(), GetFileName());
            context.AddPolylineFileName(CreateModel1DBoundaryNodeData(), GetFileName());

            context.Clear();

            Assert.That(context.PolylineFileNames, Is.Empty);
            Assert.That(context.ForcingFileNames, Is.Empty);
        }

        private static BndExtForceFileContext CreateContext() => new BndExtForceFileContext { ModelName = modelName };

        private static IniSection CreateIniSection() => new IniSection("some_section");

        private static IBoundaryCondition CreateBoundaryCondition() => Substitute.For<IBoundaryCondition>();

        private static IFeature CreateFeature() => Substitute.For<IFeature>();

        private static Model1DBoundaryNodeData CreateModel1DBoundaryNodeData() => new Model1DBoundaryNodeData();

        private static Model1DLateralSourceData CreateModel1DLateralSourceData() => new Model1DLateralSourceData();

        private static IFmMeteoField CreateMeteoField()
        {
            var meteoField = Substitute.For<IFmMeteoField>();
            meteoField.FeatureData.Feature = CreateFeature2D();

            return meteoField;
        }

        private static BoundaryConditionSet CreateBoundaryConditionSet() => new BoundaryConditionSet { Feature = CreateFeature2D() };

        private static Embankment CreateEmbankment() => new Embankment { Name = "some_embankment" };

        private static Feature2D CreateFeature2D() => new Feature2D { Name = feature2DName };

        private static string GetFileName() => Path.GetRandomFileName();
    }
}