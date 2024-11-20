using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Dimr;
using DeltaShell.Dimr.Export;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using NSubstitute;
using NUnit.Framework;

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
            var dataItemOutput = Substitute.For<IDataItem>();
            dataItemOutput.Role.Returns(DataItemRole.Output);

            var dataItemInput = Substitute.For<IDataItem>();
            dataItemInput.Role.Returns(DataItemRole.Input);
            dataItemInput.LinkedTo.Returns(dataItemOutput);

            IDimrModel source1 = Substitute.For<IDimrModel, ICoupledModel>();
            var sourceDataItems = new List<IDataItem> {dataItemOutput};
            ((ICoupledModel) source1).GetDataItemsUsedForCouplingModel(DataItemRole.Output).Returns(sourceDataItems);
            source1.Name.Returns(sourcemodelText);
            source1.GetItemString(dataItemOutput).Returns(sourceDataitemText);

            IDimrModel extraSource1 = Substitute.For<IDimrModel, ICoupledModel>();
            var extraSourceDataItems = new List<IDataItem> {dataItemOutput};
            ((ICoupledModel) extraSource1).GetDataItemsUsedForCouplingModel(Arg.Any<DataItemRole>()).Returns(extraSourceDataItems);
            extraSource1.Name.Returns(otherSourceModelText);
            extraSource1.GetItemString(dataItemOutput).Returns(sourceDataitemText);

            IDimrModel target1 = Substitute.For<IDimrModel, ICoupledModel>();
            var targetDataItems = new List<IDataItem> {dataItemInput};
            ((ICoupledModel) target1).GetDataItemsUsedForCouplingModel(Arg.Any<DataItemRole>()).Returns(targetDataItems);
            target1.Name.Returns(target_couplerText);
            target1.GetItemString(dataItemInput).Returns(targetDataitemText);

            target1.ShortName.Returns(target_couplerText);

            var subWFActivities = new EventedList<IActivity>();
            IDimrModel subWF = Substitute.For<IDimrModel, ICompositeActivity>();
            ((ICompositeActivity) subWF).Activities.Returns(subWFActivities);

            var workflowActivities = new EventedList<IActivity>();
            var workflow = Substitute.For<ICompositeActivity>(); // similar to rtc + {fm + f1d} workflow
            workflow.Activities.Returns(workflowActivities);

            IDimrModel sourceCoupler1 = Substitute.For<IDimrModel, ICompositeActivity>();
            ((ICompositeActivity) sourceCoupler1).CurrentWorkflow.Returns(workflow);
            sourceCoupler1.Name.Returns(source_couplerText);

            sourceCoupler1.ShortName.Returns(source_couplerText);

            source1.Owner.Returns(sourceCoupler1);
            extraSource1.Owner.Returns(sourceCoupler1);

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

            string expectedCouplerName = sourceCoupler1.ShortName + DimrConfigModelCouplerFactory.COUPLER_NAME_COMBINER + ((IDimrModel) target1).ShortName;

            IDimrConfigModelCoupler modelCoupler = DimrConfigModelCouplerFactory.GetCouplerForModels(source1, target1, (ICompositeActivity) sourceCoupler1, null);

            Assert.That(modelCoupler.Name, Is.EqualTo(expectedCouplerName));
            Assert.That(modelCoupler.CoupleInfos.Count(), Is.EqualTo(1));
            Assert.That(modelCoupler.CoupleInfos.First().Source, Is.EqualTo(sourcemodelText + "/" + sourceDataitemText));
            Assert.That(modelCoupler.CoupleInfos.First().Target, Is.EqualTo(targetDataitemText));

            modelCoupler.UpdateModel(extraSource1, target1, (ICompositeActivity) sourceCoupler1, null);

            Assert.That(modelCoupler.Name, Is.EqualTo(expectedCouplerName));
            Assert.That(modelCoupler.CoupleInfos.Count(), Is.EqualTo(2));
            Assert.That(modelCoupler.CoupleInfos.First().Source, Is.EqualTo(sourcemodelText + "/" + sourceDataitemText));
            Assert.That(modelCoupler.CoupleInfos.First().Target, Is.EqualTo(targetDataitemText));

            Assert.That(modelCoupler.CoupleInfos.ElementAt(1).Source, Is.EqualTo(otherSourceModelText + "/" + sourceDataitemText));
            Assert.That(modelCoupler.CoupleInfos.ElementAt(1).Target, Is.EqualTo(targetDataitemText));
        }
    }
}