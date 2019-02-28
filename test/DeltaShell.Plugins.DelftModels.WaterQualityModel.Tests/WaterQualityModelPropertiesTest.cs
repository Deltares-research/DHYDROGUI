using System;
using System.Globalization;
using System.Linq;

using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.PropertyBag.Dynamic;
using DelftTools.Utils.Reflection;

using DeltaShell.Gui.Forms.PropertyGrid;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Extentions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.PropertyGrid;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils;

using NUnit.Framework;

using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests
{
    [TestFixture]
    public class WaterQualityModelPropertiesTest
    {
        static readonly string[] iterationRelatedProperties =
        {
            TypeUtils.GetMemberName<WaterQualityModelProperties>(s => s.IterationMaximum),
            TypeUtils.GetMemberName<WaterQualityModelProperties>(s => s.Tolerance),
            TypeUtils.GetMemberName<WaterQualityModelProperties>(s => s.WriteIterationReport)
        };

        static readonly NumericalScheme[] iterationRelatedSchemes =
        {
            NumericalScheme.Scheme15,
            NumericalScheme.Scheme16,
            NumericalScheme.Scheme21,
            NumericalScheme.Scheme22
        };  

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowPropertyGridWithDefaultModelProperties()
        {
            var mockrepos = new MockRepository();
            var guiMock = mockrepos.Stub<IGui>();
            var grid = new PropertyGrid(guiMock)
            {
                Data = new DynamicPropertyBag(new WaterQualityModelProperties
                {
                    Data = new WaterQualityModel()
                })
            };

            WindowsFormsTestHelper.ShowModal(grid);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowPropertyGridWithModifiedModelProperties2()
        {
            var mockrepos = new MockRepository();
            var guiMock = mockrepos.Stub<IGui>();
            var model1D = new WaterQualityModel();
            var settings = model1D.ModelSettings;

            // General
            model1D.Name = "test model";
            settings.WorkDirectory = @"D:\Temp";

            // Simulation timers
            model1D.StartTime = new DateTime(2011, 1, 1);
            model1D.StopTime = new DateTime(2011, 2, 1);
            model1D.TimeStep = new TimeSpan(1, 1, 1, 1);

            // His output timers
            settings.HisStartTime = new DateTime(2012, 1, 1);
            settings.HisStopTime = new DateTime(2012, 2, 1);
            settings.HisTimeStep = new TimeSpan(2, 2, 2, 2);

            // Map output timers
            settings.MapStartTime = new DateTime(2013, 1, 1);
            settings.MapStopTime = new DateTime(2013, 2, 1);
            settings.MapTimeStep = new TimeSpan(3, 3, 3, 3);

            // Balance output timers
            settings.BalanceStartTime = new DateTime(2014, 1, 1);
            settings.BalanceStopTime = new DateTime(2014, 2, 1);
            settings.BalanceTimeStep = new TimeSpan(4, 4, 4, 4);

            var grid = new PropertyGrid(guiMock)
            {
                Data = new WaterQualityModelProperties
                {
                    Data = model1D
                }
            };

            WindowsFormsTestHelper.ShowModal(grid);
        }

        [Test]
        public void ModelWaqLayerPropertiesShouldBeCorrectForDefaultWaqModelTest()
        {
            // setup
            var model = new WaterQualityModel();
            var properties = new WaterQualityModelProperties
            {
                Data = model
            };

            Assert.IsNull(model.HydroData,
                "Precondition check failed: no hydro data importer should be set on waq-model.");

            // call
            var layers = properties.WaterQualityLayerThicknesses;

            // assert
            CollectionAssert.IsEmpty(layers);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ModelWaqLayerPropertiesShouldBeCorrectForRealWaqModelTest()
        {
            // setup
            string hydPath = TestHelper.GetTestFilePath(@"IO\real\uni3d.hyd");

            using (var model = new WaterQualityModel())
            {
                new HydFileImporter().ImportItem(hydPath, model);

                var properties = new WaterQualityModelProperties
                {
                    Data = model
                };

                // call
                var layers = properties.WaterQualityLayerThicknesses;

                // assert
                var expectedLayerNumbers = new[] { 0.143, 0.143, 0.143, 0.143, 0.143, 0.143, 0.143 };
                var expectedTexts = expectedLayerNumbers.Select(d => d.ToString("F3", CultureInfo.InvariantCulture)).ToArray();
                CollectionAssert.AreEqual(expectedTexts, layers);
            }
        }

        // TODO: Check WaqLayer for vertically aggregated waq-grid.

        [Test]
        public void HorizontalDispersionIsReadOnlyWhenDispersionIsAnUnstructuredGridCellCoverage()
        {
            // setup
            var model = new WaterQualityModel();
            var horizontalDispersionFunction = model.Dispersion[0];

            var properties = new WaterQualityModelProperties { Data = model };
            
            var creator = FunctionTypeCreatorFactory.CreateUnstructuredGridCoverageCreator();
            FunctionTypeCreator.ReplaceFunctionUsingCreator(model.Dispersion, horizontalDispersionFunction, creator, model);

            horizontalDispersionFunction = model.Dispersion[0];
            Assert.IsTrue(horizontalDispersionFunction.IsUnstructuredGridCellCoverage(),
                "Test precondition: Dispersion should be a coverage");

            // call
            var propertyName = TypeUtils.GetMemberName<WaterQualityModel>(m => m.HorizontalDispersion);
            var isReadOnly = properties.ValidateDynamicAttributes(propertyName);

            // assert
            Assert.IsTrue(isReadOnly);
        }

        [Test]
        public void HorizontalDispersionIsEditableWhenDispersionIsConstantValue()
        {
            // setup
            var model = new WaterQualityModel();
            var horizontalDispersionFunction = model.Dispersion[0];

            var properties = new WaterQualityModelProperties { Data = model };

            Assert.IsTrue(horizontalDispersionFunction.IsConst(),
                "Test precondition: Dispersion should be a constant value.");

            // call
            var propertyName = TypeUtils.GetMemberName<WaterQualityModel>(m => m.HorizontalDispersion);
            var isReadOnly = properties.ValidateDynamicAttributes(propertyName);

            // assert
            Assert.IsFalse(isReadOnly);
        }

        [Test]
        public void PropertiesAreEditableByDefault()
        {
            // setup
            var properties = new WaterQualityModelProperties();

            // call
            var isReadOnly = properties.ValidateDynamicAttributes(null);

            // assert
            Assert.IsFalse(isReadOnly);
        }
        
        [Test]
        [TestCaseSource("iterationRelatedSchemes")]
        public void IterationRelatedPropertiesShouldBeEditableForIterativeCalculationSchemes(NumericalScheme scheme)
        {
            // setup
            var model = new WaterQualityModel();
            model.ModelSettings.NumericalScheme = scheme;

            var properties = new WaterQualityModelProperties { Data = model };

            foreach (var propertyName in iterationRelatedProperties)
            {
                // call
                var isReadOnly = properties.ValidateDynamicAttributes(propertyName);

                // assert
                Assert.IsFalse(isReadOnly, string.Format("Expected property {0} to be editable", propertyName));
            }
        }

        [Test]
        public void IterationRelatedPropertiesShouldBeReadOnlyForNoniterativeCalculationSchemes()
        {
            // setup
            var model = new WaterQualityModel();
            
            var properties = new WaterQualityModelProperties { Data = model };

            foreach (var nonIterativeScheme in Enum.GetValues(typeof(NumericalScheme)).OfType<NumericalScheme>().Except(iterationRelatedSchemes))
            {
                model.ModelSettings.NumericalScheme = nonIterativeScheme;
                foreach (var propertyName in iterationRelatedProperties)
                {
                    // call
                    var isReadonly = properties.ValidateDynamicAttributes(propertyName);

                    // assert
                    Assert.IsTrue(isReadonly, string.Format("Expected property {0} to be read-only for scheme {1}", propertyName, nonIterativeScheme));
                }
            }
        }
    }
}
