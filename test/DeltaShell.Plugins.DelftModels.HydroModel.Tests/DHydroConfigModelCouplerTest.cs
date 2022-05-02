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
    public class DHydroConfigModelCouplerTest
    {
        [Test]
        public void CreateDHydroModelCoupler()
        {
            var source = Substitute.For<IDimrModel>();
            source.Name.Returns("source");
            source.ShortName.Returns("sourceModel");

            var target = Substitute.For<IDimrModel>();
            target.Name.Returns("target");
            target.ShortName.Returns("targetModel");

            var modelCoupler = new DimrConfigModelCoupler(source, target, null, null);

            Assert.NotNull(modelCoupler);
            Assert.AreEqual("source", modelCoupler.Source);
            Assert.AreEqual("target", modelCoupler.Target);
            string expectedModelCouplerName = string.Join(DimrConfigModelCouplerFactory.COUPLER_NAME_COMBINER, ((IDimrModel) source).ShortName, ((IDimrModel) target).ShortName);

            Assert.That(modelCoupler.Name, Is.EqualTo(expectedModelCouplerName));
        }

        [Test]
        public void CreateDHydroModelCouplerWithLinks()
        {
            var dataItemInput = Substitute.For<IDataItem>();
            dataItemInput.Role = DataItemRole.Input;

            var dataItemOutput = Substitute.For<IDataItem>();
            dataItemOutput.Role = DataItemRole.Output;
            dataItemInput.LinkedTo.Returns(dataItemOutput);

            IDimrModel source = Substitute.For<IDimrModel, ICoupledModel>();
            var sourceDataItems = new List<IDataItem> {dataItemOutput};
            ((ICoupledModel) source).GetDataItemsUsedForCouplingModel(DataItemRole.Output).Returns(sourceDataItems);
            source.GetItemString(dataItemOutput).Returns("source_dataitem");

            IDimrModel target = Substitute.For<IDimrModel, ICoupledModel>();
            var targetDataItems = new List<IDataItem> {dataItemInput};
            ((ICoupledModel) target).GetDataItemsUsedForCouplingModel(DataItemRole.Input).Returns(targetDataItems);
            target.GetItemString(dataItemInput).Returns("target_dataitem");

            var modelCoupler = new DimrConfigModelCoupler(source, target, null, null);

            Assert.That(modelCoupler.CoupleInfos.Count(), Is.EqualTo(1));
            Assert.That(modelCoupler.CoupleInfos.First().Source, Is.EqualTo("source_dataitem"));
            Assert.That(modelCoupler.CoupleInfos.First().Target, Is.EqualTo("target_dataitem"));
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
            var dataItemOutput = Substitute.For<IDataItem>();
            dataItemOutput.Role.Returns(DataItemRole.Output);

            var dataItemInput = Substitute.For<IDataItem>();
            dataItemInput.Role.Returns(DataItemRole.Input);
            dataItemInput.LinkedTo.Returns(dataItemOutput);

            IDimrModel source = Substitute.For<IDimrModel, ICoupledModel>();
            var sourceDataItems = new List<IDataItem> {dataItemOutput};
            ((ICoupledModel) source).GetDataItemsUsedForCouplingModel(DataItemRole.Output).Returns(sourceDataItems);
            source.Name.Returns(sourcemodelText);
            source.GetItemString(dataItemOutput).Returns(sourceDataitemText);

            IDimrModel extraSource = Substitute.For<IDimrModel, ICoupledModel>();
            var extraSourceDataItems = new List<IDataItem> {dataItemOutput};
            ((ICoupledModel) extraSource).GetDataItemsUsedForCouplingModel(Arg.Any<DataItemRole>()).Returns(extraSourceDataItems);
            extraSource.Name.Returns(otherSourceModelText);
            extraSource.GetItemString(dataItemOutput).Returns(sourceDataitemText);

            IDimrModel target = Substitute.For<IDimrModel, ICoupledModel>();
            var targetDataItems = new List<IDataItem> {dataItemInput};
            ((ICoupledModel) target).GetDataItemsUsedForCouplingModel(Arg.Any<DataItemRole>()).Returns(targetDataItems);
            target.Name.Returns(targetmodelText);
            target.GetItemString(dataItemInput).Returns(targetDataitemText);

            target.ShortName.Returns(targetmodelText);

            var subWFActivities = new EventedList<IActivity>();
            IDimrModel subWF = Substitute.For<IDimrModel, ICompositeActivity>();
            ((ICompositeActivity) subWF).Activities.Returns(subWFActivities);

            var workflowActivities = new EventedList<IActivity>();
            var workflow = Substitute.For<ICompositeActivity>(); // similar to rtc + {fm + f1d} workflow
            workflow.Activities.Returns(workflowActivities);

            IDimrModel sourceCoupler = Substitute.For<IDimrModel, ICompositeActivity>();
            ((ICompositeActivity) sourceCoupler).CurrentWorkflow.Returns(workflow);
            sourceCoupler.Name.Returns(couplerText);

            sourceCoupler.ShortName.Returns(couplerText);

            source.Owner.Returns(sourceCoupler);
            extraSource.Owner.Returns(sourceCoupler);

            subWFActivities.AddRange(new List<IActivity>
            {
                source,
                new ActivityWrapper(extraSource)
            });
            workflowActivities.AddRange(new List<IActivity>
            {
                new ActivityWrapper(target),
                new ActivityWrapper(subWF)
            });

            string expectedCouplerName = sourceCoupler.ShortName + DimrConfigModelCouplerFactory.COUPLER_NAME_COMBINER + ((IDimrModel) target).ShortName;

            var modelCoupler = new DimrConfigModelCoupler(source, target, (ICompositeActivity) sourceCoupler, null);

            Assert.That(modelCoupler.Name, Is.EqualTo(expectedCouplerName));
            Assert.That(modelCoupler.CoupleInfos.Count(), Is.EqualTo(1));
            Assert.That(modelCoupler.CoupleInfos.First().Source, Is.EqualTo(sourcemodelText + "/" + sourceDataitemText));
            Assert.That(modelCoupler.CoupleInfos.First().Target, Is.EqualTo(targetDataitemText));

            modelCoupler.UpdateModel(extraSource, target, (ICompositeActivity) sourceCoupler, null);

            Assert.That(modelCoupler.Name, Is.EqualTo(expectedCouplerName));
            Assert.That(modelCoupler.CoupleInfos.Count(), Is.EqualTo(2));
            Assert.That(modelCoupler.CoupleInfos.First().Source, Is.EqualTo(sourcemodelText + "/" + sourceDataitemText));
            Assert.That(modelCoupler.CoupleInfos.First().Target, Is.EqualTo(targetDataitemText));

            Assert.That(modelCoupler.CoupleInfos.ElementAt(1).Source, Is.EqualTo(otherSourceModelText + "/" + sourceDataitemText));
            Assert.That(modelCoupler.CoupleInfos.ElementAt(1).Target, Is.EqualTo(targetDataitemText));
        }
    }
}