using System;
using System.Drawing;
using DelftTools.Controls;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.NodePresenters;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.NodePresenters
{
    [TestFixture]
    public class RtcObjectNodePresenterTest
    {
        [TestCase(typeof(FactorRule), "FactorRule")]
        [TestCase(typeof(HydraulicRule), "HydraulicRule")]
        [TestCase(typeof(IntervalRule), "IntervalRule")]
        [TestCase(typeof(PIDRule), "PIDRule")]
        [TestCase(typeof(RelativeTimeRule), "RelativeTimeRule")]
        [TestCase(typeof(TimeRule), "TimeRule")]
        [TestCase(typeof(DirectionalCondition), "DirectionalCondition")]
        [TestCase(typeof(StandardCondition), "StandardCondition")]
        [TestCase(typeof(TimeCondition), "TimeCondition")]
        [TestCase(typeof(LookupSignal), "LookupSignal")]
        [TestCase(typeof(Output), "Output")]
        [TestCase(typeof(Input), "Input")]
        [TestCase(typeof(MathematicalExpression), "MathExpr")]
        public void NodePresenter_AssignsNameTagImage_ToNode(Type rtcType, string name)
        {
            // arrange
            var parentNode = Substitute.For<ITreeNode>();
            var node = Substitute.For<ITreeNode>();

            var instance = (RtcBaseObject) Activator.CreateInstance(rtcType);
            instance.Name = name;

            var rtcObjectNodePresenter = new RtcObjectNodePresenter();

            // Act
            rtcObjectNodePresenter.UpdateNode(parentNode, node, instance);

            // Assert
            node.Received(1).Text = name;
            node.Received(1).Tag = instance;
            node.Received(1).Image = Arg.Any<Bitmap>();
        }
    }
}