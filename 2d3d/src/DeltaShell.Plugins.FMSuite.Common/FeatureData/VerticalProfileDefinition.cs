using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Data;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Common.FeatureData
{
    [TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
    public enum VerticalProfileType
    {
        [Description("vertically uniform")]
        Uniform,

        [Description("at bed/surface")]
        TopBottom,

        [Description("z from bed")]
        ZFromBed,

        [Description("z from surface")]
        ZFromSurface,

        [Description("z from datum")]
        ZFromDatum,

        [Description("percentage from bed")]
        PercentageFromBed,

        [Description("percentage from surface")]
        PercentageFromSurface
    }

    [Entity]
    public class VerticalProfileDefinition : EditableObjectUnique<long>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(VerticalProfileDefinition));

        public readonly IEventedList<double> PointDepths;

        public readonly VerticalProfileType Type;

        public VerticalProfileDefinition()
        {
            Type = VerticalProfileType.Uniform;
            PointDepths = new EventedList<double>(new[]
            {
                1.0
            });
            PointDepths.CollectionChanging += PointDepthsCollectionChanging;
        }

        public VerticalProfileDefinition(VerticalProfileType type, params double[] values)
            : this(type, values.AsEnumerable()) {}

        private VerticalProfileDefinition(VerticalProfileType type, IEnumerable<double> values)
        {
            Type = type;
            switch (Type)
            {
                case VerticalProfileType.Uniform:
                    PointDepths = new EventedList<double>(new[]
                    {
                        1.0
                    });
                    break;
                case VerticalProfileType.TopBottom:
                    PointDepths = new EventedList<double>(new[]
                    {
                        0.0,
                        1.0
                    });
                    break;
                case VerticalProfileType.ZFromBed:
                case VerticalProfileType.ZFromSurface:
                case VerticalProfileType.ZFromDatum:
                    PointDepths = new EventedList<double>(values);
                    break;
                case VerticalProfileType.PercentageFromBed:
                case VerticalProfileType.PercentageFromSurface:
                    PointDepths = new EventedList<double>(values);
                    break;
                default:
                    throw new NotImplementedException(string.Format("Vertical profile type {0} unknown", type));
            }

            PointDepths.CollectionChanging += PointDepthsCollectionChanging;
        }

        public int ProfilePoints => PointDepths.Count;

        public IEnumerable<double> SortedPointDepths => SortDepths(PointDepths);

        public IEnumerable<string> LayerNames
        {
            get
            {
                switch (Type)
                {
                    case VerticalProfileType.Uniform:
                        return new[]
                        {
                            "single"
                        };
                    case VerticalProfileType.TopBottom:
                        return new[]
                        {
                            "bed",
                            "surface"
                        };
                    case VerticalProfileType.PercentageFromBed:
                        return SortedPointDepths.Select(d => d.ToString() + "% above bed");
                    case VerticalProfileType.PercentageFromSurface:
                        return SortedPointDepths.Select(d => d.ToString() + "% below surface");
                    case VerticalProfileType.ZFromBed:
                        return SortedPointDepths.Select(d => d.ToString() + "m above bed");
                    case VerticalProfileType.ZFromSurface:
                        return SortedPointDepths.Select(d => d.ToString() + "m below surface");
                    case VerticalProfileType.ZFromDatum:
                        return SortedPointDepths.Select(d => d.ToString() + "m above datum");
                    default:
                        return Enumerable.Range(1, ProfilePoints + 1).Select(i => "Point " + i);
                }
            }
        }

        public static VerticalProfileDefinition Create(VerticalProfileType type, IEnumerable<double> values)
        {
            switch (type)
            {
                case VerticalProfileType.Uniform:
                    if (values.Any())
                    {
                        log.WarnFormat("Ignoring vertical profile depths for uniform vertical profile definition...");
                    }

                    return new VerticalProfileDefinition(type, values);
                case VerticalProfileType.TopBottom:
                    if (values.Any())
                    {
                        log.WarnFormat("Ignoring vertical profile depths for surface-bedlevel vertical definition...");
                    }

                    return new VerticalProfileDefinition(type, values);
                case VerticalProfileType.ZFromBed:
                case VerticalProfileType.ZFromSurface:
                case VerticalProfileType.ZFromDatum:
                case VerticalProfileType.PercentageFromBed:
                    List<double> ascendingDepths = SortDepths(values, type).ToList();
                    if (ascendingDepths.Count != ascendingDepths.Distinct().Count())
                    {
                        log.ErrorFormat("Duplicate profile depths enountered...");
                        return null;
                    }

                    return new VerticalProfileDefinition(type, ascendingDepths);
                case VerticalProfileType.PercentageFromSurface:
                    List<double> descendingDepths = SortDepths(values, type).ToList();
                    if (descendingDepths.Count != descendingDepths.Distinct().Count())
                    {
                        log.ErrorFormat("Duplicate profile depths enountered...");
                        return null;
                    }

                    return new VerticalProfileDefinition(type, descendingDepths);
                default:
                    throw new NotImplementedException(string.Format("Vertical profile type {0} unknown", type));
            }
        }

        public IEnumerable<double> SortDepths(IEnumerable<double> depths)
        {
            return SortDepths(depths, Type);
        }

        public static IEnumerable<double> SortDepths(IEnumerable<double> depths, VerticalProfileType type)
        {
            switch (type)
            {
                case VerticalProfileType.Uniform:
                    return new[]
                    {
                        1.0
                    };
                case VerticalProfileType.TopBottom:
                    return new[]
                    {
                        0.0,
                        1.0
                    };
                case VerticalProfileType.PercentageFromSurface:
                case VerticalProfileType.ZFromSurface:
                    return depths.OrderByDescending(d => d);
                default:
                    return depths.OrderBy(d => d);
            }
        }

        private void PointDepthsCollectionChanging(object sender, NotifyCollectionChangingEventArgs e)
        {
            if (Type == VerticalProfileType.Uniform || Type == VerticalProfileType.TopBottom)
            {
                e.Cancel = true;
            }

            if (e.Action == NotifyCollectionChangeAction.Reset)
            {
                e.Cancel = true;
            }

            if (e.Action == NotifyCollectionChangeAction.Remove && PointDepths.Count == 1)
            {
                e.Cancel = true;
            }
        }
    }
}