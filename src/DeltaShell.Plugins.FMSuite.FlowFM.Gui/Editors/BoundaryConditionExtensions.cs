using System;
using DelftTools.Functions;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.Gui.Forms;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    public static class BoundaryConditionExtensions
    {
        public static void ApplyForSupportPointMode<T>(this IBoundaryCondition boundaryCondition, SupportPointMode mode, T[] newComponentValues, Func<T[], IFunction, bool> applyToFunction, string actionName, int selectedIndex = -1)
        {
            boundaryCondition.BeginEdit(actionName);
            int count = boundaryCondition.Feature.Geometry.Coordinates.Length;
            switch (mode)
            {
                case SupportPointMode.NoPoints:
                    return;
                case SupportPointMode.SelectedPoint:
                    if (boundaryCondition.GetDataAtPoint(selectedIndex) == null)
                    {
                        boundaryCondition.AddPoint(selectedIndex);
                    }

                    applyToFunction(newComponentValues, boundaryCondition.GetDataAtPoint(selectedIndex));
                    break;
                case SupportPointMode.ActivePoints:
                    foreach (IFunction function in boundaryCondition.PointData)
                    {
                        applyToFunction(newComponentValues, function);
                    }

                    break;
                case SupportPointMode.InactivePoints:

                    for (var i = 0; i < count; ++i)
                    {
                        if (boundaryCondition.DataPointIndices.Contains(i))
                        {
                            continue;
                        }

                        boundaryCondition.AddPoint(i);
                        applyToFunction(newComponentValues, boundaryCondition.GetDataAtPoint(i));
                    }

                    break;
                case SupportPointMode.AllPoints:

                    for (var i = 0; i < count; ++i)
                    {
                        if (!boundaryCondition.DataPointIndices.Contains(i))
                        {
                            boundaryCondition.AddPoint(i);
                        }

                        applyToFunction(newComponentValues, boundaryCondition.GetDataAtPoint(i));
                    }

                    break;
                default:
                    throw new NotImplementedException("Support point selection method not recognized.");
            }

            boundaryCondition.EndEdit();
        }
    }
}