using System;
using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RTCShapes.IO;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using DeltaShell.Plugins.DelftModels.RTCShapes.Xsd;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RTCShapes.Tests.IO
{
    [TestFixture]
    public class ShapesXmlConverterTest
    {
        [Test]
        public void ConvertToXmlData_ShapesGroupsIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => ShapesXmlConverter.ConvertToXmlData(null), Throws.ArgumentNullException);
        }

        [Test]
        public void ConvertToXmlData_UnknownShapeType_ThrowsNotSupportedException()
        {
            Assert.That(() => ShapesXmlConverter.ConvertToXmlData(CreateShapesGroup(new UnknownShape())), Throws.InstanceOf<NotSupportedException>());
        }

        [Test]
        public void ConvertToXmlData_EmptyShapesInGroup_ReturnsXmlDataObjectWithoutGroups()
        {
            var shapesGroup = new ShapesGroup("id", Array.Empty<ShapeBase>());

            ShapesConfigComplexType xmlData = ShapesXmlConverter.ConvertToXmlData(shapesGroup);

            Assert.That(xmlData, Is.Not.Null);
            Assert.That(xmlData.groups, Is.Not.Null);
            Assert.That(xmlData.groups, Is.Empty);
        }

        [Test]
        [TestCaseSource(nameof(GetKnownShapeTestCases))]
        public void ConvertToXmlData_KnownShapeType_ReturnsXmlDataObject(ShapeBase shape, ShapeEnumStringType expectedType)
        {
            shape.X = 10;
            shape.Y = 20;
            shape.Width = 100;
            shape.Height = 200;
            shape.Title = "TestShape";

            ShapesGroup shapesGroup = CreateShapesGroup(shape);
            ShapesConfigComplexType xmlData = ShapesXmlConverter.ConvertToXmlData(shapesGroup);

            Assert.That(xmlData, Is.Not.Null);
            Assert.That(xmlData.groups, Is.Not.Null);
            Assert.That(xmlData.groups, Has.Length.EqualTo(1));
            Assert.That(xmlData.groups[0].groupId, Is.EqualTo(shapesGroup.GroupId));
            Assert.That(xmlData.groups[0].shapes, Is.Not.Null);
            Assert.That(xmlData.groups[0].shapes, Has.Length.EqualTo(1));
            Assert.That(xmlData.groups[0].shapes[0].x, Is.EqualTo(10));
            Assert.That(xmlData.groups[0].shapes[0].y, Is.EqualTo(20));
            Assert.That(xmlData.groups[0].shapes[0].width, Is.EqualTo(100));
            Assert.That(xmlData.groups[0].shapes[0].height, Is.EqualTo(200));
            Assert.That(xmlData.groups[0].shapes[0].title, Is.EqualTo("TestShape"));
        }

        private static IEnumerable<TestCaseData> GetKnownShapeTestCases()
        {
            yield return new TestCaseData(new InputItemShape(), ShapeEnumStringType.input).SetName("Input");
            yield return new TestCaseData(new OutputItemShape(), ShapeEnumStringType.output).SetName("Output");
            yield return new TestCaseData(new RuleShape(), ShapeEnumStringType.rule).SetName("Rule");
            yield return new TestCaseData(new SignalShape(), ShapeEnumStringType.signal).SetName("Signal");
            yield return new TestCaseData(new ConditionShape(), ShapeEnumStringType.condition).SetName("Condition");
            yield return new TestCaseData(new MathematicalExpressionShape(), ShapeEnumStringType.expression).SetName("Expression");
        }

        [Test]
        public void ConvertFromXmlData_XmlDataIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => ShapesXmlConverter.ConvertFromXmlData(null), Throws.ArgumentNullException);
        }

        [Test]
        public void ConvertFromXmlData_UnknownShapeType_ThrowsNotSupportedException()
        {
            var shapeData = new ShapeComplexType { type = (ShapeEnumStringType)(-1) };

            Assert.That(() => ShapesXmlConverter.ConvertFromXmlData(CreateShapesConfig(shapeData)), Throws.InstanceOf<NotSupportedException>());
        }

        [Test]
        [TestCaseSource(nameof(GetKnownShapeDataTypeTestCases))]
        public void ConvertFromXmlData_KnownShapeType_ReturnsShapesGroup(ShapeEnumStringType shapeEnumType, Type expectedShapeType)
        {
            var shapeData = new ShapeComplexType
            {
                type = shapeEnumType,
                x = 50,
                y = 120,
                width = 30,
                height = 40,
                title = "SomeShape"
            };

            ShapesConfigComplexType shapesConfig = CreateShapesConfig(shapeData);
            IReadOnlyList<ShapesGroup> shapesGroups = ShapesXmlConverter.ConvertFromXmlData(shapesConfig);

            Assert.That(shapesGroups, Is.Not.Null);
            Assert.That(shapesGroups, Has.Length.EqualTo(1));
            Assert.That(shapesGroups[0].GroupId, Is.EqualTo(shapesConfig.groups[0].groupId));
            Assert.That(shapesGroups[0].Shapes, Is.Not.Null);
            Assert.That(shapesGroups[0].Shapes, Has.Length.EqualTo(1));
            Assert.That(shapesGroups[0].Shapes[0], Is.TypeOf(expectedShapeType));
            Assert.That(shapesGroups[0].Shapes[0].X, Is.EqualTo(50));
            Assert.That(shapesGroups[0].Shapes[0].Y, Is.EqualTo(120));
            Assert.That(shapesGroups[0].Shapes[0].Width, Is.EqualTo(30));
            Assert.That(shapesGroups[0].Shapes[0].Height, Is.EqualTo(40));
            Assert.That(shapesGroups[0].Shapes[0].Title, Is.EqualTo("SomeShape"));
        }

        private static IEnumerable<TestCaseData> GetKnownShapeDataTypeTestCases()
        {
            yield return new TestCaseData(ShapeEnumStringType.input, typeof(InputItemShape)).SetName("Input");
            yield return new TestCaseData(ShapeEnumStringType.output, typeof(OutputItemShape)).SetName("Output");
            yield return new TestCaseData(ShapeEnumStringType.rule, typeof(RuleShape)).SetName("Rule");
            yield return new TestCaseData(ShapeEnumStringType.signal, typeof(SignalShape)).SetName("Signal");
            yield return new TestCaseData(ShapeEnumStringType.condition, typeof(ConditionShape)).SetName("Condition");
            yield return new TestCaseData(ShapeEnumStringType.expression, typeof(MathematicalExpressionShape)).SetName("Expression");
        }

        private static ShapesGroup CreateShapesGroup(ShapeBase shape)
            => new ShapesGroup(shape.GetType().Name, new[] { shape });

        private static ShapesConfigComplexType CreateShapesConfig(ShapeComplexType shape)
        {
            var group = new ShapeGroupComplexType
            {
                groupId = "SomeGroup",
                shapes = new[] { shape }
            };

            return new ShapesConfigComplexType { groups = new[] { group } };
        }

        private class UnknownShape : ShapeBase
        {
            protected override void Initialize()
            {
            }
        }
    }
}