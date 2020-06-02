using System.Collections.Generic;
using System.Linq;
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

        [Test]
        public void CreateDHydroModelCouplerWithDimrCoupleInfoInDimrConfigModelCouplerAndCallUpdateModel()
        {
            var mocks1 = new MockRepository();
            var dataItemInput = mocks1.StrictMock<IDataItem>();
            dataItemInput.Expect(di => di.Role).Return(DataItemRole.Input).Repeat.Any();

            var dataItemOutput = mocks1.StrictMock<IDataItem>();
            dataItemOutput.Expect(di => di.Role).Return(DataItemRole.Output).Repeat.Any();
            dataItemOutput.Expect(di => di.LinkedTo).Return(null).Repeat.Any();
            dataItemInput.Expect(di => di.LinkedTo).Return(dataItemOutput).Repeat.Any();

            var source1 = mocks1.StrictMultiMock<IModel>(typeof(IDimrModel), typeof(IActivity));
            var sourceDataItems = new List<IDataItem> {dataItemOutput};
            source1.Expect(m => m.AllDataItems).Return(sourceDataItems).Repeat.Any();
            source1.Expect(m => m.StatusChanged += null).IgnoreArguments().Repeat.Any();
            source1.Expect(m => m.ProgressChanged += null).IgnoreArguments().Repeat.Any();
            ((IDimrModel) source1).Expect(m => m.Name).Return(sourcemodelText).Repeat.Any();
            ((IDimrModel) source1).Expect(dm => dm.GetItemString(dataItemOutput)).Return(sourceDataitemText).Repeat.Any();

            var extraSource1 = mocks1.StrictMultiMock<IModel>(typeof(IDimrModel), typeof(IActivity));
            var extraSourceDataItems = new List<IDataItem> {dataItemOutput};
            extraSource1.Expect(m => m.AllDataItems).Return(extraSourceDataItems).Repeat.Any();
            extraSource1.Expect(m => m.StatusChanged += null).IgnoreArguments().Repeat.Any();
            extraSource1.Expect(m => m.ProgressChanged += null).IgnoreArguments().Repeat.Any();

            extraSource1.Expect(m => m.Name).Return(otherSourceModelText).Repeat.Any();
            ((IDimrModel) extraSource1).Expect(dm => dm.GetItemString(dataItemOutput)).Return(sourceDataitemText).Repeat.Any();

            var target1 = mocks1.StrictMultiMock<IModel>(typeof(IDimrModel));
            var targetDataItems = new List<IDataItem> {dataItemInput};
            target1.Expect(m => m.AllDataItems).Return(targetDataItems).Repeat.Any();
            target1.Expect(m => m.Owner).Return(null).Repeat.Any();
            target1.Expect(dm => dm.Name).Return(target_couplerText).Repeat.Any();
            target1.Expect(m => m.StatusChanged += null).IgnoreArguments().Repeat.Any();
            target1.Expect(m => m.ProgressChanged += null).IgnoreArguments().Repeat.Any();
            ((IDimrModel) target1).Expect(dm => dm.ShortName).Return(target_couplerText).Repeat.Any();
            ((IDimrModel) target1).Expect(dm => dm.GetItemString(dataItemInput)).Return(targetDataitemText).Repeat.Any();

            var subWFActivities = new EventedList<IActivity>();
            var subWF = mocks1.StrictMultiMock<ICompositeActivity>(typeof(IDimrModel)); // similar to iterative1d2dCoupler
            subWF.Expect(wf => wf.Activities).Return(subWFActivities).Repeat.Any();
            subWF.Expect(m => m.StatusChanged += null).IgnoreArguments().Repeat.Any();
            subWF.Expect(m => m.ProgressChanged += null).IgnoreArguments().Repeat.Any();
            ((IDimrModel) subWF).Expect(dm => dm.IsMasterTimeStep).Return(true).Repeat.Any();

            var workflowActivities = new EventedList<IActivity>();
            var workflow = mocks1.StrictMock<ICompositeActivity>(); // similar to rtc + {fm + f1d} workflow
            workflow.Expect(wf => wf.Activities).Return(workflowActivities).Repeat.Any();

            var sourceCoupler1 = mocks1.StrictMultiMock<ICompositeActivity>(typeof(IDimrModel));
            sourceCoupler1.Expect(c => c.CurrentWorkflow).Return(workflow).Repeat.Any();
            ((IDimrModel) sourceCoupler1).Expect(dm => dm.IsMasterTimeStep).Return(false).Repeat.Any();
            ((IDimrModel) sourceCoupler1).Expect(dm => dm.ShortName).Return(source_couplerText).Repeat.Any();
            ((IDimrModel) sourceCoupler1).Expect(dm => dm.Name).Return(source_couplerText).Repeat.Any();

            source1.Expect(m => m.Owner).Return(sourceCoupler1).Repeat.Any();
            extraSource1.Expect(m => m.Owner).Return(sourceCoupler1).Repeat.Any();

            mocks1.ReplayAll();

            subWFActivities.AddRange(new List<IActivity>
            {
                source1,
                new ActivityWrapper(extraSource1)
            });
            workflowActivities.AddRange(new List<IActivity>
            {
                new ActivityWrapper(target1),
                new ActivityWrapper(subWF)
            });

            string expectedCouperName = ((IDimrModel) sourceCoupler1).ShortName + DimrConfigModelCouplerFactory.COUPLER_NAME_COMBINER + ((IDimrModel) target1).ShortName;

            IDimrConfigModelCoupler modelCoupler = DimrConfigModelCouplerFactory.GetCouplerForModels(source1, target1, sourceCoupler1, null);

            Assert.That(modelCoupler.Name, Is.EqualTo(expectedCouperName));
            Assert.That(modelCoupler.CoupleInfos.Count(), Is.EqualTo(1));
            Assert.That(modelCoupler.CoupleInfos.First().Source, Is.EqualTo(sourcemodelText + "/" + sourceDataitemText));
            Assert.That(modelCoupler.CoupleInfos.First().Target, Is.EqualTo(targetDataitemText));

            modelCoupler.UpdateModel(extraSource1, target1, sourceCoupler1, null);

            Assert.That(modelCoupler.Name, Is.EqualTo(expectedCouperName));
            Assert.That(modelCoupler.CoupleInfos.Count(), Is.EqualTo(2));
            Assert.That(modelCoupler.CoupleInfos.First().Source, Is.EqualTo(sourcemodelText + "/" + sourceDataitemText));
            Assert.That(modelCoupler.CoupleInfos.First().Target, Is.EqualTo(targetDataitemText));

            Assert.That(modelCoupler.CoupleInfos.ElementAt(1).Source, Is.EqualTo(otherSourceModelText + "/" + sourceDataitemText));
            Assert.That(modelCoupler.CoupleInfos.ElementAt(1).Target, Is.EqualTo(targetDataitemText));

            mocks1.VerifyAll();
        }
    }
}