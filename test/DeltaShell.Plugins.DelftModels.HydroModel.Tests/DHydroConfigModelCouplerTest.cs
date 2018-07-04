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
    public class DHydroConfigModelCouplerTest
    {
        [Test]
		public void CreateDHydroModelCoupler()
		{
            var source = MockRepository.GenerateMock<IModel, IDimrModel>();
            source.Stub(m => m.AllDataItems).Return(new List<IDataItem>()).Repeat.Any();
            source.Stub(m => m.Name).Return("source").Repeat.Any();
            ((IDimrModel)source).Stub(dm => dm.ShortName).Return("sourceModel").Repeat.Any();

            var target = MockRepository.GenerateMock<IModel, IDimrModel>();
            target.Stub(m => m.AllDataItems).Return(new List<IDataItem>()).Repeat.Any();
            target.Stub(m => m.Name).Return("target").Repeat.Any();
            ((IDimrModel)target).Stub(dm => dm.ShortName).Return("targetModel").Repeat.Any();

            var modelCoupler = new DimrConfigModelCoupler(source, target, null, null);

            Assert.NotNull(modelCoupler);
            //Assert.AreEqual(source, modelCoupler.SourceModel);
            Assert.AreEqual("source", modelCoupler.Source);
            //Assert.AreEqual(target, modelCoupler.TargetModel);
            Assert.AreEqual("target", modelCoupler.Target);
            var expectedModelCouplerName = string.Join(DimrConfigModelCouplerFactory.COUPLER_NAME_COMBINER, ((IDimrModel)source).ShortName, ((IDimrModel)target).ShortName);

            Assert.That(modelCoupler.Name, Is.EqualTo(expectedModelCouplerName));
		}

        [Test]
        public void CreateDHydroModelCouplerWithLinks()
        {
            var mocks = new MockRepository();
            var dataItemInput = mocks.Stub<IDataItem>();
            dataItemInput.Role = DataItemRole.Input;

            var dataItemOutput = mocks.Stub<IDataItem>();
            dataItemOutput.Role = DataItemRole.Output;
            Expect.Call(dataItemInput.LinkedTo).Return(dataItemOutput).Repeat.Any();
            
            var source = MockRepository.GenerateMock<IModel, IDimrModel>();
            var sourceDataItems = new List<IDataItem> { dataItemOutput };
            source.Stub(m => m.AllDataItems).Return(sourceDataItems).Repeat.Any();
            ((IDimrModel)source).Stub(dm => dm.GetItemString(dataItemOutput)).Return("source_dataitem").Repeat.Any();


            var target = MockRepository.GenerateMock<IModel, IDimrModel>();
            var targetDataItems = new List<IDataItem> { dataItemInput };
            target.Stub(m => m.AllDataItems).Return(targetDataItems).Repeat.Any();
            ((IDimrModel)target).Stub(dm => dm.GetItemString(dataItemInput)).Return("target_dataitem").Repeat.Any();
            mocks.ReplayAll();

            var modelCoupler = new DimrConfigModelCoupler(source, target, null, null);

            Assert.That(modelCoupler.CoupleInfos.Count(), Is.EqualTo(1));
            Assert.That(modelCoupler.CoupleInfos.First().Source, Is.EqualTo("source_dataitem"));
            Assert.That(modelCoupler.CoupleInfos.First().Target, Is.EqualTo("target_dataitem"));

            mocks.VerifyAll();
        }

        [Test]
        public void CreateDHydroModelCouplerWithDimrCoupleInfoInDimrConfigModelCouplerAndCallUpdateModel()
        {
            const string sourceDataitemText = "source_dataitem";
            const string targetDataitemText = "target_dataitem";
            const string sourcemodelText = "My SourceModel";
            const string targetmodelText = "My TargetModel";
            const string otherSourceModelText = "My Other SourceModel";
            const string couplerText = "source_coupler";

            /**
             * Testing rtc + {1d2d} (or rtc + {fm + f1d}) model with links but then generic way!
             * 
             * 
             **/

            var mocks = new MockRepository();
            
            var dataItemOutput = mocks.StrictMock<IDataItem>();
            dataItemOutput.Expect(di => di.Role).Return(DataItemRole.Output).Repeat.Any();
            dataItemOutput.Expect(di => di.LinkedTo).Return(null).Repeat.Any();

            var dataItemInput = mocks.StrictMock<IDataItem>();
            dataItemInput.Expect(di => di.Role).Return(DataItemRole.Input).Repeat.Any();
            dataItemInput.Expect(di => di.LinkedTo).Return(dataItemOutput).Repeat.Any();

            var source = mocks.StrictMultiMock<IModel>(typeof(IDimrModel), typeof(IActivity));
            var sourceDataItems = new List<IDataItem> { dataItemOutput };
            source.Expect(m => m.AllDataItems).Return(sourceDataItems).Repeat.Any();
            source.Expect(m => m.Name).Return(sourcemodelText).Repeat.Any();
            source.Expect(m => m.StatusChanged += null).IgnoreArguments().Repeat.Any();
            source.Expect(m => m.ProgressChanged += null).IgnoreArguments().Repeat.Any();
            ((IDimrModel)source).Expect(dm => dm.GetItemString(dataItemOutput)).Return(sourceDataitemText).Repeat.Any();

            var extraSource = mocks.StrictMultiMock<IModel>(typeof(IDimrModel), typeof(IActivity));
            var extraSourceDataItems = new List<IDataItem> { dataItemOutput };
            extraSource.Expect(m => m.AllDataItems).Return(extraSourceDataItems).Repeat.Any();
            extraSource.Expect(m => m.StatusChanged += null).IgnoreArguments().Repeat.Any();
            extraSource.Expect(m => m.ProgressChanged += null).IgnoreArguments().Repeat.Any();

            extraSource.Expect(m => m.Name).Return(otherSourceModelText).Repeat.Any();
            ((IDimrModel)extraSource).Expect(dm => dm.GetItemString(dataItemOutput)).Return(sourceDataitemText).Repeat.Any();

            var target = mocks.StrictMultiMock<IModel>(typeof(IDimrModel));
            var targetDataItems = new List<IDataItem> { dataItemInput };
            target.Expect(m => m.AllDataItems).Return(targetDataItems).Repeat.Any();
            target.Expect(dm => dm.Name).Return(targetmodelText).Repeat.Any();
            target.Expect(dm => dm.Owner).Return(null).Repeat.Any();
            target.Expect(m => m.StatusChanged += null).IgnoreArguments().Repeat.Any();
            target.Expect(m => m.ProgressChanged += null).IgnoreArguments().Repeat.Any();
            ((IDimrModel)target).Expect(dm => dm.ShortName).Return(targetmodelText).Repeat.Any();
            ((IDimrModel)target).Expect(dm => dm.GetItemString(dataItemInput)).Return(targetDataitemText).Repeat.Any();

            var subWFActivities = new EventedList<IActivity>();
            var subWF = mocks.StrictMultiMock<ICompositeActivity>(typeof(IDimrModel)); // similar to iterative1d2dCoupler
            subWF.Expect(wf => wf.Activities).Return(subWFActivities).Repeat.Any();
            subWF.Expect(m => m.StatusChanged += null).IgnoreArguments().Repeat.Any();
            subWF.Expect(m => m.ProgressChanged += null).IgnoreArguments().Repeat.Any();
            ((IDimrModel)subWF).Expect(dm => dm.IsMasterTimeStep).Return(true).Repeat.Any();

            var workflowActivities = new EventedList<IActivity>();
            var workflow = mocks.StrictMock<ICompositeActivity>(); // similar to rtc + {fm + f1d} workflow
            workflow.Expect(wf => wf.Activities).Return(workflowActivities).Repeat.Any();

            var sourceCoupler = mocks.StrictMultiMock<ICompositeActivity>(typeof(IDimrModel));
            sourceCoupler.Expect(c => c.CurrentWorkflow).Return(workflow).Repeat.Any();
            sourceCoupler.Expect(c => c.Name).Return(couplerText).Repeat.Any();
            
            ((IDimrModel)sourceCoupler).Expect(dm => dm.ShortName).Return(couplerText).Repeat.Any();
            ((IDimrModel)sourceCoupler).Expect(dm => dm.IsMasterTimeStep).Return(false).Repeat.Any();

            source.Expect(m => m.Owner).Return(sourceCoupler).Repeat.Any();
            extraSource.Expect(m => m.Owner).Return(sourceCoupler).Repeat.Any();

            mocks.ReplayAll();

            subWFActivities.AddRange(new List<IActivity> { source, new ActivityWrapper(extraSource) });
            workflowActivities.AddRange(new List<IActivity> { new ActivityWrapper(target), new ActivityWrapper(subWF) });

            var expectedCouperName = ((IDimrModel)sourceCoupler).ShortName + DimrConfigModelCouplerFactory.COUPLER_NAME_COMBINER + ((IDimrModel)target).ShortName;

            var modelCoupler = new DimrConfigModelCoupler(source, target, sourceCoupler, null);

            Assert.That(modelCoupler.Name, Is.EqualTo(expectedCouperName));
            Assert.That(modelCoupler.CoupleInfos.Count(), Is.EqualTo(1));
            Assert.That(modelCoupler.CoupleInfos.First().Source, Is.EqualTo(sourcemodelText+"/"+sourceDataitemText));
            Assert.That(modelCoupler.CoupleInfos.First().Target, Is.EqualTo(targetDataitemText));

            modelCoupler.UpdateModel(extraSource, target, sourceCoupler, null);

            Assert.That(modelCoupler.Name, Is.EqualTo(expectedCouperName));
            Assert.That(modelCoupler.CoupleInfos.Count(), Is.EqualTo(2));
            Assert.That(modelCoupler.CoupleInfos.First().Source, Is.EqualTo(sourcemodelText+"/" + sourceDataitemText));
            Assert.That(modelCoupler.CoupleInfos.First().Target, Is.EqualTo(targetDataitemText));

            Assert.That(modelCoupler.CoupleInfos.ElementAt(1).Source, Is.EqualTo(otherSourceModelText+"/" + sourceDataitemText));
            Assert.That(modelCoupler.CoupleInfos.ElementAt(1).Target, Is.EqualTo(targetDataitemText));

            mocks.VerifyAll();
        }
    }
}