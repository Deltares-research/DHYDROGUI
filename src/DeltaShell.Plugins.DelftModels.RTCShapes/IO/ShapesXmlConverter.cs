using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using DeltaShell.Plugins.DelftModels.RTCShapes.Xsd;

namespace DeltaShell.Plugins.DelftModels.RTCShapes.IO
{
    /// <summary>
    /// Converts RTC shapes to data access objects and vice-versa.
    /// </summary>
    public static class ShapesXmlConverter
    {
        /// <summary>
        /// Converts a collection of shapes groups to a data access object.
        /// </summary>
        /// <param name="shapesGroups">The collection of shapes grouped by group ID to convert.</param>
        /// <returns>The shapes configuration data access object.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="shapesGroups"/> is <c>null</c>.</exception>
        /// <remarks>Empty shape groups are excluded from the resulting data access object.</remarks>
        public static ShapesConfigComplexType ConvertToXmlData(params ShapesGroup[] shapesGroups)
        {
            Ensure.NotNull(shapesGroups, nameof(shapesGroups));

            IEnumerable<ShapeGroupComplexType> groups = shapesGroups.Select(ConvertToXmlData);
            IEnumerable<ShapeGroupComplexType> groupsWithShapes = groups.Where(g => g.shapes.Any());

            return new ShapesConfigComplexType { groups = groupsWithShapes.ToArray() };
        }

        private static ShapeGroupComplexType ConvertToXmlData(ShapesGroup shapesGroup)
        {
            IEnumerable<ShapeComplexType> shapeData = shapesGroup.Shapes.Select(ConvertToXmlData);

            return new ShapeGroupComplexType
            {
                groupId = shapesGroup.GroupId,
                shapes = shapeData.ToArray()
            };
        }

        private static ShapeComplexType ConvertToXmlData(ShapeBase shape)
        {
            return new ShapeComplexType
            {
                type = GetShapeDataType(shape),
                x = shape.X,
                y = shape.Y,
                width = shape.Width,
                height = shape.Height,
                title = shape.Title
            };
        }

        private static ShapeEnumStringType GetShapeDataType(ShapeBase shape)
        {
            switch (shape)
            {
                case InputItemShape _:
                    return ShapeEnumStringType.input;
                case OutputItemShape _:
                    return ShapeEnumStringType.output;
                case RuleShape _:
                    return ShapeEnumStringType.rule;
                case SignalShape _:
                    return ShapeEnumStringType.signal;
                case ConditionShape _:
                    return ShapeEnumStringType.condition;
                case MathematicalExpressionShape _:
                    return ShapeEnumStringType.expression;
                default:
                    throw new NotSupportedException($"Unknown shape type: '{shape.GetType()}'.");
            }
        }

        /// <summary>
        /// Converts a shapes configuration data access object to a collection of shapes groups.
        /// </summary>
        /// <param name="xmlData">The shapes configuration data access object to convert.</param>
        /// <returns>The collection of shapes groups.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="xmlData"/> is <c>null</c>.</exception>
        public static IReadOnlyList<ShapesGroup> ConvertFromXmlData(ShapesConfigComplexType xmlData)
        {
            Ensure.NotNull(xmlData, nameof(xmlData));

            return xmlData.groups.Select(ConvertFromXmlData).ToArray();
        }

        private static ShapesGroup ConvertFromXmlData(ShapeGroupComplexType xmlData)
        {
            IEnumerable<ShapeBase> shapes = xmlData.shapes.Select(ConvertFromXmlData);

            return new ShapesGroup(xmlData.groupId, shapes.ToArray());
        }

        private static ShapeBase ConvertFromXmlData(ShapeComplexType xmlData)
        {
            ShapeBase shape = CreateShape(xmlData.type);

            shape.X = xmlData.x;
            shape.Y = xmlData.y;
            shape.Width = xmlData.width;
            shape.Height = xmlData.height;
            shape.Title = xmlData.title;

            return shape;
        }

        private static ShapeBase CreateShape(ShapeEnumStringType type)
        {
            switch (type)
            {
                case ShapeEnumStringType.input:
                    return new InputItemShape();
                case ShapeEnumStringType.output:
                    return new OutputItemShape();
                case ShapeEnumStringType.rule:
                    return new RuleShape();
                case ShapeEnumStringType.signal:
                    return new SignalShape();
                case ShapeEnumStringType.condition:
                    return new ConditionShape();
                case ShapeEnumStringType.expression:
                    return new MathematicalExpressionShape();
                default:
                    throw new NotSupportedException($"Unknown shape data type: '{type}'.");
            }
        }
    }
}