using System;
using System.ComponentModel;
using DelftTools.Utils.Aop;
using NUnit.Framework;

namespace DeltaShell.NGHS.Utils.Test
{
    [TestFixture]
    public class EventingHelperTest
    {
        [Test]
        public void GivenEventingHelper_DoingActionWithoutEvents_ShouldNotGenerateEvents()
        {
            //Arrange
            var testEntity = new CustomEntity();
            var propertyChangedCount = 0;

            ((INotifyPropertyChanged)testEntity).PropertyChanged += (sender, args) => propertyChangedCount++;

            // Act & Assert
            testEntity.TestString = "Test";
            Assert.AreEqual(1, propertyChangedCount);
            
            EventingHelper.DoWithoutEvents(() =>
            {
                testEntity.TestString = "Test2";
            });

            Assert.AreEqual(1, propertyChangedCount);
        }

        [Test]
        public void GivenEventingHelper_DoingActionWithoutEvents_ShouldAlsoRestoreStateOnError()
        {
            //Arrange
            var testEntity = new CustomEntity();
            var propertyChangedCount = 0;

            ((INotifyPropertyChanged)testEntity).PropertyChanged += (sender, args) => propertyChangedCount++;

            // Act & Assert
            try
            {
                EventingHelper.DoWithoutEvents(() => throw new Exception("Forced exception"));
            }
            catch (Exception)
            {
                // catch exception to test eventing
            }

            testEntity.TestString = "Test";
            Assert.AreEqual(1, propertyChangedCount);
        }

        [Test]
        public void GivenEventingHelper_DoingActionWithEvents_ShouldGenerateEvents()
        {
            //Arrange
            var testEntity = new CustomEntity();
            var propertyChangedCount = 0;

            ((INotifyPropertyChanged)testEntity).PropertyChanged += (sender, args) => propertyChangedCount++;

            // Act & Assert
            EventingHelper.DoWithoutEvents(() =>
            {
                testEntity.TestString = "Test";
                Assert.AreEqual(0, propertyChangedCount);

                EventingHelper.DoWithEvents(() => testEntity.TestString = "Test2");
                Assert.AreEqual(1, propertyChangedCount);
            });
        }

        [Test]
        public void GivenEventingHelper_DoingActionWithEvents_ShouldAlsoRestoreStateOnError()
        {
            //Arrange
            var testEntity = new CustomEntity();
            var propertyChangedCount = 0;

            ((INotifyPropertyChanged)testEntity).PropertyChanged += (sender, args) => propertyChangedCount++;

            // Act & Assert
            EventingHelper.DoWithoutEvents(() =>
            {
                testEntity.TestString = "Test";
                Assert.AreEqual(0, propertyChangedCount);

                try
                {
                    EventingHelper.DoWithEvents(() => throw new Exception("Forced exception"));
                }
                catch (Exception)
                {
                    // catch exception to test eventing
                }

                // test if eventing has been restored to previous (disabled) state
                testEntity.TestString = "Test2";
                Assert.AreEqual(0, propertyChangedCount);
            });

            testEntity.TestString = "Test3";
            Assert.AreEqual(1, propertyChangedCount);
        }

        [Entity]
        internal class CustomEntity
        {
            public string TestString { get; set; }
        }
    }
}