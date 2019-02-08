using System;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Extensions;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using NUnit.Framework;
using Rhino.Mocks;


namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Import
{
    [TestFixture]
    public class WaterFlowModel1DFileImporterHydroTest
    {
        /// <summary>
        /// GIVEN a WaterFlowModel1D
        ///   AND some path
        ///   AND some target WaterModel1D with a hydromodel owner
        /// WHEN ImportItem is called with these parameters
        ///  AND this new model is read
        /// THEN this hydromodel is returned
        ///  AND the target model has been replaced in the hydromodel
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void GivenAWaterFlowModel1DAndSomePathAndSomeTargetWaterModel1DWithAHydromodelOwner_WhenImportItemIsCalledWithTheseParametersAndThisNewModelIsRead_ThenThisHydromodelIsReturnedAndTheTargetModelHasBeenReplacedInTheHydromodel()
        {
            // Given
            var hydroModel = new HydroModel();
            var prevModel = new WaterFlowModel1D("definitely-not-a-potato");
            prevModel.MoveModelIntoIntegratedModel(null, hydroModel);

            var model = new WaterFlowModel1D("potato");

            const string path = "somePath";

            var readFunc =
                MockRepository.GenerateStrictMock<Func<string, Action<string, int, int>, WaterFlowModel1D>>();
            readFunc.Expect(f => f.Invoke(null, null))
                    .IgnoreArguments()
                    .Return(model)
                    .Repeat.AtLeastOnce();
            var importer = new WaterFlowModel1DFileImporter(readFunc);

            // When
            var result = (HydroModel) importer.ImportItem(path, prevModel);

            // Then
            readFunc.VerifyAllExpectations();

            Assert.That(result, Is.EqualTo(hydroModel), "Expected returned model to be equal to the hydro model:");
            Assert.That(model.Owner(), Is.EqualTo(hydroModel), "Expected the owner of the read model to be equal to the hydro model:");
        }

        /// <summary>
        /// GIVEN a WaterFlowModel1D
        ///   AND some path
        ///   AND some target hydromodel
        /// WHEN ImportItem is called with these parameters
        ///  AND this new model is read
        /// THEN this hydromodel is returned
        ///  AND the waterflow model has been replaced in the hydromodel
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void GivenAWaterFlowModel1DAndSomePathAndSomeTargetHydromodel_WhenImportItemIsCalledWithTheseParametersAndThisNewModelIsRead_ThenThisHydromodelIsReturnedAndTheWaterflowModelHasBeenReplacedInTheHydromodel()
        {
            // Given
            var hydroModel = new HydroModel();
            var prevModel = new WaterFlowModel1D("definitely-not-a-potato");
            prevModel.MoveModelIntoIntegratedModel(null, hydroModel);

            var model = new WaterFlowModel1D("potato");

            const string path = "somePath";

            var readFunc =
                MockRepository.GenerateStrictMock<Func<string, Action<string, int, int>, WaterFlowModel1D>>();
            readFunc.Expect(f => f.Invoke(null, null))
                    .IgnoreArguments()
                    .Return(model)
                    .Repeat.AtLeastOnce();
            var importer = new WaterFlowModel1DFileImporter(readFunc);

            // When
            var result = (HydroModel)importer.ImportItem(path, hydroModel);

            // Then
            readFunc.VerifyAllExpectations();

            Assert.That(result, Is.EqualTo(hydroModel), "Expected returned model to be equal to the hydro model:");
            Assert.That(model.Owner(), Is.EqualTo(hydroModel), "Expected the owner of the read model to be equal to the hydro model:");
        }

    }
}
