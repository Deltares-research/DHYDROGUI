using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Dimr;
using DeltaShell.Dimr.Export;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters;
using NSubstitute;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Exporters
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class RRFlowDimrConfigModelCouplerTest
    {
        private const string sourceDataitemText = "source_dataitem";
        private const string targetDataitemText = "target_dataitem";
        private const string sourcemodelText = "My SourceModel";
        private const string targetmodelText = "My TargetModel";
        private const string couplerText = "source_coupler";

       
        [Test]
        [TestCase("Greenhouse", true)]
        [TestCase("OpenWater", true)]
        [TestCase("Paved", true)]
        [TestCase("Unpaved", true)]
        [TestCase("Sacramento", true)]
        [TestCase("HBV", true)]
        [TestCase("None", true)]
        public void GenerateRRFlowWithCouplerDimrConfigTest(string catchment, bool parallel)
        {
            var mocks = new MockRepository();
            var dataItemInput = mocks.StrictMock<IDataItem>();
            var dataItemInputChild = mocks.StrictMock<IDataItem>();
            var dataItemOutput = mocks.StrictMock<IDataItem>();
            var dataItemOutputChild = mocks.StrictMock<IDataItem>();

            dataItemInputChild.Expect(di => di.Role).Return(DataItemRole.Input).Repeat.Any();
            dataItemInputChild.Expect(di => di.Children).Return(new EventedList<IDataItem>()).Repeat.Any();
            dataItemInputChild.Expect(di => di.Name).Return("Water level").Repeat.Any();
            
            dataItemInput.Expect(di => di.Role).Return(DataItemRole.Input).Repeat.Any();
            dataItemInput.Expect(di => di.Children).Return(new EventedList<IDataItem>() { dataItemInputChild }).Repeat.Any();
            dataItemInput.Expect(di => di.Name).Return("Water level").Repeat.Any();
            
            dataItemOutputChild.Expect(di => di.Role).Return(DataItemRole.Output).Repeat.Any();
            dataItemOutputChild.Expect(di => di.Children).Return(new EventedList<IDataItem>()).Repeat.Any();
            dataItemOutputChild.Expect(di => di.Name).Return(RainfallRunoffModelParameterNames.BoundaryDischarge).Repeat.Any();
            
            dataItemOutput.Expect(di => di.Role).Return(DataItemRole.Output).Repeat.Any();
            dataItemOutput.Expect(di => di.Children).Return(new EventedList<IDataItem>() { dataItemOutputChild }).Repeat.Any();
            dataItemOutput.Expect(di => di.Name).Return(RainfallRunoffModelParameterNames.BoundaryDischarge).Repeat.Any();

            dataItemInputChild.Expect(di => di.LinkedTo).Return(dataItemOutput).Repeat.Any();
            dataItemInput.Expect(di => di.LinkedTo).Return(null).Repeat.Any();
            dataItemOutputChild.Expect(di => di.LinkedTo).Return(dataItemInput).Repeat.Any();
            dataItemOutput.Expect(di => di.LinkedTo).Return(null).Repeat.Any();

            // our rainfall runoff model
            var source = mocks.StrictMultiMock<IRainfallRunoffModel>(typeof(IHydroModel), typeof(IDimrModel));
            var sourceDataItems = new List<IDataItem> { dataItemOutput, dataItemInput };
            source.Expect(m => m.AllDataItems).Return(sourceDataItems).Repeat.Any();
            source.Expect(m => m.Name).Return(sourcemodelText).Repeat.Any();
            source.Expect(m => m.StatusChanged += null).IgnoreArguments().Repeat.Any();
            source.Expect(m => m.ProgressChanged += null).IgnoreArguments().Repeat.Any();
            ((IDimrModel)source).Expect(m => m.ShortName).Return("rr").Repeat.Any();
            

            // our 'f1d' model
            var target = mocks.StrictMultiMock<IModel>(typeof(IDimrModel), typeof(IHydroModel));
            var targetDataItems = new List<IDataItem> { dataItemInput, dataItemOutput };
            target.Expect(m => m.AllDataItems).Return(targetDataItems).Repeat.Any();
            target.Expect(m => m.Name).Return(targetmodelText).Repeat.Any();
            target.Expect(m => m.StatusChanged += null).IgnoreArguments().Repeat.Any();
            target.Expect(m => m.ProgressChanged += null).IgnoreArguments().Repeat.Any();
            ((IDimrModel)target).Expect(dm => dm.GetItemString(dataItemInput)).Return(targetDataitemText).Repeat.Any();
            ((IDimrModel)target).Expect(m => m.ShortName).Return("flowd1d").Repeat.Any();
            ((IDimrModel)target).Expect(dm => dm.IsMasterTimeStep).Return(true).Repeat.Any();

            CatchmentType catchmentType = CatchmentType.LoadFromString(catchment);
            var targetObj = Substitute.For<IHydroObject, IHydroNode>();
            targetObj.Name = "Node001";

            var catchment1 = Substitute.For<Catchment>();
            catchment1.Name = "Catchment1";
            catchment1.CatchmentType = catchmentType;
            var links = new EventedList<HydroLink>() { new HydroLink(catchment1, targetObj) };
            catchment1.Links = links;

            
            var catchmentsList = new List<Catchment>{catchment1};
            var catchments = mocks.Stub<IEventedList<Catchment>>();

            catchments.Stub(x => x.GetEnumerator())
                // create new enumerator instance for each call
                .WhenCalled(call => call.ReturnValue = catchmentsList.GetEnumerator())
                .Return(null) // is ignored, but needed for Rhinos validation
                .Repeat.Any();

            var basin = mocks.StrictMock<IDrainageBasin>();
            source.Expect(m => m.Basin).Return(basin).Repeat.Any();
            basin.Expect(b => b.Catchments).Return(catchments).Repeat.Any();
            ((IHydroModel)source).Expect(m => m.Region).Return(basin).Repeat.Any();
            ((IDimrModel)source).Expect(dm => dm.GetItemString(dataItemOutput)).Return(sourceDataitemText).Repeat.Any();
            ((IDimrModel)source).Expect(dm => dm.IsMasterTimeStep).Return(false).Repeat.Any();

            ICompositeActivity sourceCoupler;

            var activities = new EventedList<IActivity>();

            if (parallel)
            {
                var workflow = mocks.StrictMock<ParallelActivity>();// similar to rr + f1d workflow
                workflow.Expect(wf => wf.Activities).Return(activities).Repeat.Any();

                sourceCoupler = mocks.StrictMultiMock<ICompositeActivity>(typeof(IDimrModel));
                sourceCoupler.Expect(c => c.CurrentWorkflow).Return(workflow).Repeat.Any();

                ((IDimrModel)sourceCoupler).Expect(dm => dm.ShortName).Return(couplerText).Repeat.Any();
                ((IDimrModel)sourceCoupler).Expect(dm => dm.IsMasterTimeStep).Return(false).Repeat.Any();

                source.Expect(m => m.Owner).Return(sourceCoupler).Repeat.Any();
            }
            else
            {
                var workflow = mocks.StrictMock<SequentialActivity>(); // similar to rr + f1d workflow
                workflow.Expect(wf => wf.Activities).Return(activities).Repeat.Any();

                sourceCoupler = mocks.StrictMultiMock<ICompositeActivity>(typeof(IDimrModel));
                sourceCoupler.Expect(c => c.CurrentWorkflow).Return(workflow).Repeat.Any();

                ((IDimrModel)sourceCoupler).Expect(dm => dm.ShortName).Return(couplerText).Repeat.Any();
                ((IDimrModel)sourceCoupler).Expect(dm => dm.IsMasterTimeStep).Return(false).Repeat.Any();

                source.Expect(m => m.Owner).Return(sourceCoupler).Repeat.Any();
            }
            mocks.ReplayAll();

            activities.AddRange(new [] { new ActivityWrapper(source), new ActivityWrapper(target) });
            DimrConfigModelCouplerFactory.CouplerProviders.Add(new RRDimrConfigModelCouplerProvider());
            var expectedCouperName = ((IDimrModel)source).ShortName + DimrConfigModelCouplerFactory.COUPLER_NAME_COMBINER + ((IDimrModel)target).ShortName;

            var modelCoupler = DimrConfigModelCouplerFactory.GetCouplerForModels(source, target, null, null);

            Assert.That(modelCoupler.Name, Is.EqualTo(expectedCouperName));
            Assert.That(modelCoupler.CoupleInfos.Count(), Is.EqualTo(1));
            Assert.That(modelCoupler.CoupleInfos.First().Source, Is.EqualTo("catchments/Node001/water_discharge"));
            Assert.That(modelCoupler.CoupleInfos.First().Target, Is.EqualTo("boundaries/Node001/water_discharge")); //seems new functionality, NO MORE WATERLEVELS!
            if (catchmentType == CatchmentType.Unpaved || Equals(catchmentType, CatchmentType.Paved))
            {
                links.Clear();
                links.Add(new HydroLink(targetObj, catchment1));;
            }
            modelCoupler = DimrConfigModelCouplerFactory.GetCouplerForModels(target, source, null, null);
            if (Equals(catchmentType, CatchmentType.Unpaved) || Equals(catchmentType, CatchmentType.Paved))
            {
                expectedCouperName = ((IDimrModel) target).ShortName +
                                     DimrConfigModelCouplerFactory.COUPLER_NAME_COMBINER +
                                     ((IDimrModel) source).ShortName;
                Assert.That(modelCoupler.Name, Is.EqualTo(expectedCouperName));
                Assert.That(modelCoupler.CoupleInfos.Count(), Is.EqualTo(1));
                Assert.That(modelCoupler.CoupleInfos.First().Target, Is.EqualTo("boundaries/Catchment1/water_level"));//seems new functionality, NO MORE WATER Discharges!
                Assert.That(modelCoupler.CoupleInfos.First().Source, Is.EqualTo("catchments/Catchment1/water_level"));
            }
            else
            {
                Assert.That(modelCoupler.Name, Is.Null);
                Assert.That(modelCoupler.CoupleInfos.Count(), Is.EqualTo(0));
            }
            mocks.VerifyAll();
        }
    }
}