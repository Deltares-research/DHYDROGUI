using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests
{
    [TestFixture]
    public class RealTimeControlModelCopyPasteHelperShadowTest
    {
        [Test]
        public void Instance_Always_ReturnsSameInstance()
        {
            // Call
            RealTimeControlModelCopyPasteHelperShadow firstInstance = RealTimeControlModelCopyPasteHelperShadow.Instance;
            RealTimeControlModelCopyPasteHelperShadow secondInstance = RealTimeControlModelCopyPasteHelperShadow.Instance;

            // Assert
            Assert.That(firstInstance, Is.SameAs(secondInstance));
        }

        [Test]
        public void Instance_ExpectedProperties()
        {
            // Call
            RealTimeControlModelCopyPasteHelperShadow instance = RealTimeControlModelCopyPasteHelperShadow.Instance;

            // Assert
            Assert.That(instance.CopiedShapes, Is.Empty);
            Assert.That(instance.IsDataSet, Is.False);
        }

        [Test]
        public void SetCopiedData_ShapesNull_ThrowsArgumentNullException()
        {
            // Setup
            RealTimeControlModelCopyPasteHelperShadow helper = RealTimeControlModelCopyPasteHelperShadow.Instance;

            // Call
            TestDelegate call = () => helper.SetCopiedData(null);

            // Assert
            Assert.That(call, Throws.TypeOf<ArgumentNullException>()
                                    .With.Property(nameof(ArgumentNullException.ParamName))
                                    .EqualTo("shapes"));
        }

        [Test]
        public void SetCopiedData_CollectionEmpty_SetsCopiedShapesAndIsDataSetFalse()
        {
            // Setup
            RealTimeControlModelCopyPasteHelperShadow helper = RealTimeControlModelCopyPasteHelperShadow.Instance;

            // Call
            helper.SetCopiedData(Enumerable.Empty<ShapeBase>());

            // Assert
            Assert.That(helper.IsDataSet, Is.False);
            Assert.That(helper.CopiedShapes, Is.Empty);
        }

        [Test]
        public void SetCopiedData_CollectionNotEmpty_SetsCopiedShapesAndIsDataSetTrue()
        {
            // Setup
            var shapes = new[] {new TestShape(), new TestShape(), new TestShape()};
            RealTimeControlModelCopyPasteHelperShadow helper = RealTimeControlModelCopyPasteHelperShadow.Instance;
            
            // Call
            helper.SetCopiedData(shapes);

            // Assert
            CollectionAssert.AreEqual(shapes, helper.CopiedShapes);
            Assert.That(helper.IsDataSet, Is.True);
        }

        [Test]
        public void GivenHelperWithSetData_WhenClearingCopiedData_ThenDataIsClearedAndDataSetFalse()
        {
            // Given
            var shapes = new[] { new TestShape(), new TestShape(), new TestShape() };
            RealTimeControlModelCopyPasteHelperShadow helper = RealTimeControlModelCopyPasteHelperShadow.Instance;

            helper.SetCopiedData(shapes);

            // Precondition
            Assert.That(helper.CopiedShapes, Is.Not.Empty);

            // When
            helper.ClearData();

            // Then
            Assert.That(helper.IsDataSet, Is.False);
            Assert.That(helper.CopiedShapes, Is.Empty);
        }

        private class TestShape : ShapeBase
        {
            protected override void Initialize() {}
        }

        /// <summary>
        /// Helper class to assist with the copy paste actions of the Real Time Control Model.
        /// </summary>
        public class RealTimeControlModelCopyPasteHelperShadow
        {
            private static RealTimeControlModelCopyPasteHelperShadow instance;
            private readonly List<ShapeBase> copiedShapes;

            private RealTimeControlModelCopyPasteHelperShadow()
            {
                copiedShapes = new List<ShapeBase>();
                IsDataSet = false;
            }

            /// <summary>
            /// Gets the instance of <see cref="RealTimeControlModelCopyPasteHelperShadow"/>.
            /// </summary>
            public static RealTimeControlModelCopyPasteHelperShadow Instance
            {
                get
                {
                    return instance ?? (instance = new RealTimeControlModelCopyPasteHelperShadow());
                }
            }

            /// <summary>
            /// Gets the collection of copied shapes.
            /// </summary>
            public IEnumerable<ShapeBase> CopiedShapes => copiedShapes;

            /// <summary>
            /// Gets the indicator whether the data is set for copying.
            /// </summary>
            public bool IsDataSet { get; private set; }

            /// <summary>
            /// Sets the copied data to the helper.
            /// </summary>
            /// <param name="shapes">The collection of <see cref="ShapeBase"/> to set.</param>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="shapes"/>
            /// is <c>null</c>.</exception>
            public void SetCopiedData(IEnumerable<ShapeBase> shapes)
            {
                if (shapes == null)
                {
                    throw new ArgumentNullException(nameof(shapes));
                }

                IsDataSet = shapes.Any();
                copiedShapes.AddRange(shapes);
            }

            /// <summary>
            /// Clears the data that is set.
            /// </summary>
            public void ClearData()
            {
                IsDataSet = false;
                copiedShapes.Clear();
            }
        }
    }
}