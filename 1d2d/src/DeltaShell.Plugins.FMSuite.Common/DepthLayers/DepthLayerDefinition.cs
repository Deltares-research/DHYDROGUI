using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Extensions;

namespace DeltaShell.Plugins.FMSuite.Common.DepthLayers
{
    public class DepthLayerDefinition: ICloneable
    {
        private readonly IList<double> layerDepths;

        public DepthLayerDefinition(int numLayers)
        {
            Type = numLayers == 0 ? DepthLayerType.Single : DepthLayerType.Sigma;
            layerDepths = new List<double>();
            if (Type == DepthLayerType.Sigma)
            {
                layerDepths.AddRange(Enumerable.Repeat((double) 1/numLayers, numLayers));
            }
        }
        
        public DepthLayerDefinition(DepthLayerType type, IEnumerable<double> values)
        {
            Type = type;
            switch (Type)
            {
                case DepthLayerType.Single:
                    layerDepths = new List<double>();
                    break;
                case DepthLayerType.Sigma:
                    if (values.Any())
                    {
                        var sum = values.Sum();
                        layerDepths = values.Select(v => v/sum).ToList();
                    }
                    else
                    {
                        layerDepths = new double[] {1};
                    }
                    break;
                case DepthLayerType.Z:
                    layerDepths = !values.Any() ? (IList<double>) new double[] {1} : values.ToList();
                    break;
                default:
                    throw new NotImplementedException(string.Format("Depth layer type {0} not implemented", Type));
            }
        }

        public DepthLayerDefinition(DepthLayerType type, params double[] values)
            : this(type, values.AsEnumerable())
        {
        }

        public DepthLayerType Type { get; private set; }

        public int NumLayers
        {
            get
            {
                switch (Type)
                {
                    case DepthLayerType.Single:
                        return 1;
                    case DepthLayerType.Sigma:
                    case DepthLayerType.Z:
                        return layerDepths.Count;
                    default:
                        throw new NotImplementedException(string.Format("Depth layer type {0} not implemented", Type));
                }
            }
        }

        public bool UseLayers
        {
            get { return Type != DepthLayerType.Single || layerDepths.Count != 0; }
        }

        public string Description
        {
            get
            {
                switch (Type)
                {
                    case DepthLayerType.Single:
                        return "Single";
                    case DepthLayerType.Sigma:
                        return string.Format("{0} sigma-layers", layerDepths.Count);
                    case DepthLayerType.Z:
                        return string.Format("{0} z-layers", layerDepths.Count);
                    default:
                        throw new NotImplementedException(string.Format("Depth layer type {0} not implemented", Type));
                }
            }
        }

        public IEnumerable<double> LayerThicknesses
        {
            get { return layerDepths; }
        }

        public object Clone()
        {
            return new DepthLayerDefinition(Type, layerDepths);
        }
    }
}
