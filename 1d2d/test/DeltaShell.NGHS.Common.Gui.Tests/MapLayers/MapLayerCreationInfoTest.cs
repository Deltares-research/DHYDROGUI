using System;
using DeltaShell.NGHS.Common.Gui.MapLayers;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.NGHS.Common.Gui.Tests.MapLayers
{
    [TestFixture]
    public class MapLayerCreationInfoTest
    {
        [Test]
        public void GivenMapLayerCreationInfo_CallingMethods_ShouldCastToCorrectType()
        {
            //Arrange
            var info = new MapLayerCreationInfo<string,int>
            {
                CreateLayerFunc = (s,i) => Substitute.For<ILayer>(),
                ChildLayerObjectsFunc = s => new []{"test"},
                AfterCreateFunc = (l, s, i, lookup) => {}
            };

            // Act & Assert
            Assert.AreEqual(typeof(string), info.SupportedType);
            Assert.False(info.CanBuildWithParent(1.9), "MapLayerCreationInfo should check parent type");
            Assert.True(info.CanBuildWithParent(1));

            Assert.Throws<InvalidCastException>(() => info.CreateLayer("string", 1.9), "Calling with incorrect type should lead to exception");
            Assert.Throws<InvalidCastException>(() => info.ChildLayerObjects(1.9), "Calling with incorrect type should lead to exception");
            Assert.Throws<InvalidCastException>(() => info.AfterCreate(null, 1.9,"string", null), "Calling with incorrect type should lead to exception");
        }

        [Test]
        public void GivenMapLayerCreationInfo_CallingMethods_ShouldCallCastedVersionOfAddedFunctions()
        {
            //Arrange
            var expectedLayer = Substitute.For<ILayer>();
            var expectedChildObjects = new[] { "test" };
            var afterCreateCalled = false;
            var info = new MapLayerCreationInfo<string, int>
            {
                CreateLayerFunc = (s, i) => expectedLayer,
                ChildLayerObjectsFunc = s => expectedChildObjects,
                AfterCreateFunc = (l, s, i, lookup) => { afterCreateCalled = true; },
                CanBuildWithParentFunc = i => i > 10
            };

            // Act & Assert
            Assert.AreEqual(expectedLayer, info.CreateLayer("test", 1));
            Assert.AreEqual(expectedChildObjects, info.ChildLayerObjects("test"));
            Assert.True(info.CanBuildWithParent(12));
            Assert.False(info.CanBuildWithParent(2));
            
            info.AfterCreateFunc(null, "test", 13, null);
            Assert.IsTrue(afterCreateCalled);
        }
    }
}