using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class DimrConfigModelCouplerFactoryTest
    {
        private const string sourceDataitemText = "source_dataitem";
        private const string targetDataitemText = "target_dataitem";
        private const string sourcemodelText = "My SourceModel";
        private const string otherSourceModelText = "My Other SourceModel";
        private const string source_couplerText = "source_coupler";
        private const string target_couplerText = "target_coupler";

        private MockRepository mocks;
        private IModel source;
        private IModel extraSource;
        private IModel target;
        private ICompositeActivity sourceCoupler;

        [Test]
        public void CreateDHydroModelCouplerWithDimrCoupleInfoInDimrConfigModelCouplerAndCallUpdateModel()
        {
            mocks.ReplayAll();

            var expectedCouperName = ((IDimrModel)sourceCoupler).ShortName + DimrConfigModelCouplerFactory.COUPLER_NAME_COMBINER + ((IDimrModel)target).ShortName;

            var modelCoupler = DimrConfigModelCouplerFactory.GetCouplerForModels(source, target, sourceCoupler, null);

            Assert.That(modelCoupler.Name, Is.EqualTo(expectedCouperName));
            Assert.That(modelCoupler.CoupleInfos.Count(), Is.EqualTo(1));
            Assert.That(modelCoupler.CoupleInfos.First().Source, Is.EqualTo(sourcemodelText + "/" + sourceDataitemText));
            Assert.That(modelCoupler.CoupleInfos.First().Target, Is.EqualTo(targetDataitemText));

            modelCoupler.UpdateModel(extraSource, target, sourceCoupler, null);

            Assert.That(modelCoupler.Name, Is.EqualTo(expectedCouperName));
            Assert.That(modelCoupler.CoupleInfos.Count(), Is.EqualTo(2));
            Assert.That(modelCoupler.CoupleInfos.First().Source, Is.EqualTo(sourcemodelText + "/" + sourceDataitemText));
            Assert.That(modelCoupler.CoupleInfos.First().Target, Is.EqualTo(targetDataitemText));

            Assert.That(modelCoupler.CoupleInfos.ElementAt(1).Source, Is.EqualTo(otherSourceModelText + "/" + sourceDataitemText));
            Assert.That(modelCoupler.CoupleInfos.ElementAt(1).Target, Is.EqualTo(targetDataitemText));

            mocks.VerifyAll();
        }

        [SetUp]
        public void Setup()
        {
            /**
             * Testing rtc + {1d2d} (or rtc + {fm + f1d}) model with links but then generic way!
             * 
             * 
             **/

            mocks = new MockRepository();
            var dataItemInput = mocks.StrictMock<IDataItem>();
            dataItemInput.Expect(di => di.Role).Return(DataItemRole.Input).Repeat.Any();
            
            var dataItemOutput = mocks.StrictMock<IDataItem>();
            dataItemOutput.Expect(di => di.Role).Return(DataItemRole.Output).Repeat.Any();
            dataItemOutput.Expect(di => di.LinkedTo).Return(null).Repeat.Any();
            dataItemInput.Expect(di => di.LinkedTo).Return(dataItemOutput).Repeat.Any();

            source = mocks.StrictMultiMock<IModel>(typeof(IDimrModel), typeof(IActivity));
            var sourceDataItems = new List<IDataItem> {dataItemOutput};
            source.Expect(m => m.AllDataItems).Return(sourceDataItems).Repeat.Any();
            ((IDimrModel) source).Expect(m => m.Name).Return(sourcemodelText).Repeat.Any();
            ((IDimrModel) source).Expect(dm => dm.GetItemString(dataItemOutput)).Return(sourceDataitemText).Repeat.Any();

            extraSource = mocks.StrictMultiMock<IModel>(typeof(IDimrModel), typeof(IActivity));
            var extraSourceDataItems = new List<IDataItem> {dataItemOutput};
            extraSource.Expect(m => m.AllDataItems).Return(extraSourceDataItems).Repeat.Any();

            extraSource.Expect(m => m.Name).Return(otherSourceModelText).Repeat.Any();
            ((IDimrModel) extraSource).Expect(dm => dm.GetItemString(dataItemOutput)).Return(sourceDataitemText).Repeat.Any();

            target = mocks.StrictMultiMock<IModel>(typeof(IDimrModel));
            var targetDataItems = new List<IDataItem> {dataItemInput};
            target.Expect(m => m.AllDataItems).Return(targetDataItems).Repeat.Any();
            target.Expect(m => m.Owner).Return(null).Repeat.Any();
            target.Expect(dm => dm.Name).Return(target_couplerText).Repeat.Any();
            ((IDimrModel) target).Expect(dm => dm.ShortName).Return(target_couplerText).Repeat.Any();
            ((IDimrModel) target).Expect(dm => dm.GetItemString(dataItemInput)).Return(targetDataitemText).Repeat.Any();

            var subWF = mocks.StrictMultiMock<ICompositeActivity>(typeof(IDimrModel));// similar to iterative1d2dCoupler
            subWF.Expect(wf => wf.Activities).Return(
                new EventedList<IActivity>() {source, new ActivityWrapper(extraSource)}
            ).Repeat.Any();
            ((IDimrModel) subWF).Expect(dm => dm.IsMasterTimeStep).Return(true).Repeat.Any();
            var workflow = mocks.StrictMock<ICompositeActivity>(); // similar to rtc + {fm + f1d} workflow
            workflow.Expect(wf => wf.Activities).Return(
                new EventedList<IActivity>() {new ActivityWrapper(target), new ActivityWrapper(subWF)}
            ).Repeat.Any();

            sourceCoupler = mocks.StrictMultiMock<ICompositeActivity>(typeof(IDimrModel));
            sourceCoupler.Expect(c => c.CurrentWorkflow).Return(workflow).Repeat.Any();
            ((IDimrModel) sourceCoupler).Expect(dm => dm.IsMasterTimeStep).Return(false).Repeat.Any();
            ((IDimrModel) sourceCoupler).Expect(dm => dm.ShortName).Return(source_couplerText).Repeat.Any();
            ((IDimrModel) sourceCoupler).Expect(dm => dm.Name).Return(source_couplerText).Repeat.Any();

            source.Expect(m => m.Owner).Return(sourceCoupler).Repeat.Any();
            extraSource.Expect(m => m.Owner).Return(sourceCoupler).Repeat.Any();
        }
    }
}