using System;
using System.Collections.Generic;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.Gui.MapLayers;
using NSubstitute;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap.Api.Layers;
using SharpMap.Layers;

namespace DeltaShell.NGHS.Common.Gui.Tests.MapLayers
{
    [TestFixture]
    public class LayerExtensionsTest
    {
        [Test]
        public void SetName_ArgumentNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => ((Layer) null).SetName("new_name");

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("layer"));
        }

        [Test]
        public void SetName_SetsNameCorrectly_AndRestoresProperty([Values] bool nameIsReadOnly)
        {
            // Setup
            var layer = Substitute.For<Layer>();
            layer.NameIsReadOnly = nameIsReadOnly;

            // Call
            layer.SetName("new_name");

            // Assert
            Assert.That(layer.NameIsReadOnly, Is.EqualTo(nameIsReadOnly));
            Assert.That(layer.Name, Is.EqualTo("new_name"));
        }

        [Test]
        public void GivenLayerExtensions_DoingSetRenderOrderByObjectOrder_ShouldResetTheRenderingOrderCorrectly()
        {
            //Arrange
            var mocks = new MockRepository();
            var groupLayer1 = mocks.Stub<IGroupLayer>();
            var groupLayer2 = mocks.Stub<IGroupLayer>();

            var layer1 = mocks.Stub<ILayer>();
            var layer2 = mocks.Stub<ILayer>();
            var layer3 = mocks.Stub<ILayer>();
            var layer4 = mocks.Stub<ILayer>();

            var groupObject1 = "groupObject1";
            var groupObject2 = "groupObject2";

            var object1 = "object1";
            var object2 = "object2";
            var object3 = "object3";
            var object4 = "object4";

            layer3.RenderOrder = 2;
            layer4.RenderOrder = 1;

            groupLayer1.Layers = new EventedList<ILayer> { layer1, groupLayer2, layer2 };
            groupLayer2.Layers = new EventedList<ILayer> { layer3, layer4 };

            var lookup = new Dictionary<ILayer, object>
            {
                { groupLayer1, groupObject1 },
                { groupLayer2, groupObject2 },
                { layer1, object1 },
                { layer2, object2 },
                { layer3, object3 },
                { layer4, object4 }
            };

            mocks.ReplayAll();

            // Act

            var objectsInRenderOrder = new object[]
            {
                groupObject2,
                object2,
                object1
            };

            groupLayer1.SetRenderOrderByObjectOrder(objectsInRenderOrder, lookup);

            // Assert layer render order is as specified by objectsInRenderOrder

            // grouplayer1 sub order is preserved
            Assert.AreEqual(1, layer4.RenderOrder);
            Assert.AreEqual(2, layer3.RenderOrder);

            // object 2
            Assert.AreEqual(3, layer2.RenderOrder);

            // object1
            Assert.AreEqual(4, layer1.RenderOrder);
        }
    }
}