using DelftTools.Shell.Gui;
using DeltaShell.NGHS.Common.Gui.PropertyGrid.PropertyInfoCreation;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Gui.Tests.PropertyGrid.PropertyInfoCreation
{
    [TestFixture]
    public class PropertyInfoCreatorTest
    {
        [Test]
        public void Constructor_ArgNull_ThrowsArgumentNullException()
        {
            // Call
            void Call()
            {
                _ = new PropertyInfoCreator(null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Create_ArgNull_ThrowsArgumentNullException()
        {
            // Setup
            var propertyInfoCreator = new PropertyInfoCreator(new GuiContainer());

            // Call
            void Call()
            {
                propertyInfoCreator.Create<TestData, TestProperties>(null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Create_ReturnsCorrectPropertyInfo()
        {
            // Setup
            var propertyInfoCreator = new PropertyInfoCreator(new GuiContainer());

            // Call
            var creationContext = Substitute.For<IPropertyInfoCreationContext<TestData, TestProperties>>();
            PropertyInfo propertyInfo = propertyInfoCreator.Create(creationContext);

            // Assert
            Assert.That(propertyInfo, Is.TypeOf<PropertyInfo>());
            Assert.That(propertyInfo.PropertyType, Is.EqualTo(typeof(TestProperties)));
            Assert.That(propertyInfo.ObjectType, Is.EqualTo(typeof(TestData)));
            Assert.That(propertyInfo.AfterCreate, Is.Not.Null);
        }

        [Test]
        public void InvokeAfterCreatePropertyInfo_AfterCreate_CustomizesTheProperties()
        {
            // Setup
            var guiContainer = new GuiContainer();
            var propertyInfoCreator = new PropertyInfoCreator(guiContainer);

            var creationContext = Substitute.For<IPropertyInfoCreationContext<TestData, TestProperties>>();
            PropertyInfo propertyInfo = propertyInfoCreator.Create(creationContext);

            var testData = new TestData();
            var testProperties = new TestProperties { Data = testData };

            // Call
            propertyInfo.AfterCreate.Invoke(testProperties);

            // Assert
            creationContext.Received(1).CustomizeProperties(testProperties, guiContainer);
        }

        public class TestData
        {
        }

        public class TestProperties : ObjectProperties<TestData>
        {
        }
    }
}