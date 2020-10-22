using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms
{
    /// <summary>
    /// Helper class to determine the connection rules between shapes.
    /// </summary>
    public static class ShapeConnectionsRulesController
    {
        private static readonly Dictionary<Type, IEnumerable<ConnectionRule>> connectionMapping = new Dictionary<Type, IEnumerable<ConnectionRule>>
        {
            {
                typeof(InputItemShape), new[]
                {
                    new ConnectionRule(typeof(ConditionShape), new HashSet<ConnectorType>(new[]
                    {
                        ConnectorType.Top
                    })),
                    new ConnectionRule(typeof(SignalShape), new HashSet<ConnectorType>(new[]
                    {
                        ConnectorType.Top
                    })),
                    new ConnectionRule(typeof(RuleShape), new HashSet<ConnectorType>(new[]
                    {
                        ConnectorType.Top
                    })),
                    new ConnectionRule(typeof(MathematicalExpressionShape), new HashSet<ConnectorType>(new[]
                    {
                        ConnectorType.Top
                    }))
                }
            },
            {
                typeof(ConditionShape), new[]
                {
                    new ConnectionRule(typeof(RuleShape), new HashSet<ConnectorType>(new[]
                    {
                        ConnectorType.Left
                    })),
                    new ConnectionRule(typeof(MathematicalExpressionShape), new HashSet<ConnectorType>(new[]
                    {
                        ConnectorType.Left
                    }))
                }
            },
            {
                typeof(RuleShape), new[]
                {
                    new ConnectionRule(typeof(OutputItemShape), new HashSet<ConnectorType>(new[]
                    {
                        ConnectorType.Left
                    }))
                }
            },
            {
                typeof(SignalShape), new[]
                {
                    new ConnectionRule(typeof(RuleShape), new HashSet<ConnectorType>(new[]
                    {
                        ConnectorType.Left,
                        ConnectorType.Top,
                        ConnectorType.Bottom
                    }))
                }
            },
            {
                typeof(MathematicalExpressionShape), new[]
                {
                    new ConnectionRule(typeof(ConditionShape), new HashSet<ConnectorType>(new[]
                    {
                        ConnectorType.Top,
                        ConnectorType.Left
                    })),
                    new ConnectionRule(typeof(RuleShape), new HashSet<ConnectorType>(new[]
                    {
                        ConnectorType.Top,
                        ConnectorType.Left,
                        ConnectorType.Bottom
                    })),
                    new ConnectionRule(typeof(MathematicalExpressionShape), new HashSet<ConnectorType>(new[]
                    {
                        ConnectorType.Top
                    }))
                }
            }
        };

        /// <summary>
        /// Check if a connector of a specific shape is compatible with a target connector
        /// according to a specific connectionMapping
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="target">The target object.</param>
        /// <param name="targetConnector">The connector type of the target.</param>
        /// <returns> </returns>
        public static bool IsConnectorSourceCompatibleWithConnectorDestination(ShapeBase source,
                                                                               ShapeBase target,
                                                                               ConnectorType targetConnector)
        {
            if (source == null || target == null)
            {
                throw new ArgumentNullException("Could not check if source shape is connectable with target shape.");
            }

            IEnumerable<ConnectionRule> connectionRules = connectionMapping[source.GetType()];
            ConnectionRule rule = connectionRules.SingleOrDefault(r => r.ShapeType == target.GetType());
            return rule != null && rule.AllowedConnectors.Contains(targetConnector);
        }

        private class ConnectionRule
        {
            public ConnectionRule(Type shapeType, HashSet<ConnectorType> allowedConnectors)
            {
                ShapeType = shapeType;
                AllowedConnectors = allowedConnectors;
            }

            /// <summary>
            /// Gets the allowable shapes.
            /// </summary>
            public Type ShapeType { get; }

            /// <summary>
            /// Gets the allowed connectors belonging to <see cref="ShapeType"/>.
            /// </summary>
            public HashSet<ConnectorType> AllowedConnectors { get; }
        }
    }
}