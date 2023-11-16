using System;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Coverages
{
    [Entity]
    public class CoverageDepthLayersList
    {
        private readonly Func<string, ICoverage> createCoverageFunc;
        private VerticalProfileDefinition verticalProfileDefinition;

        public CoverageDepthLayersList(Func<string, ICoverage> createCoverageFunc, bool isDepthIndependent = false)
        {
            this.createCoverageFunc = createCoverageFunc;
            IsDepthIndependent = isDepthIndependent;
            Coverages = new EventedList<ICoverage>();
            verticalProfileDefinition = new VerticalProfileDefinition();
        }

        public string Name { get; set; }

        public IEventedList<ICoverage> Coverages { get; private set; }

        public VerticalProfileDefinition VerticalProfile
        {
            get => verticalProfileDefinition;
            set
            {
                verticalProfileDefinition = value;
                AfterVerticalProfileSet();
            }
        }

        private bool IsDepthIndependent { get; set; }

        private void AfterVerticalProfileSet()
        {
            int layerCount = verticalProfileDefinition.ProfilePoints;
            if (layerCount < Coverages.Count)
            {
                if (layerCount == 1)
                {
                    Coverages[0].Name = Name;
                }

                for (int i = Coverages.Count - 1; i >= layerCount; i--)
                {
                    Coverages.RemoveAt(i);
                }
            }
            else if (layerCount > Coverages.Count)
            {
                if (Coverages.Count == 1)
                {
                    Coverages[0].Name = Name + " (layer 1)";
                }

                for (int i = Coverages.Count; i < layerCount; i++)
                {
                    AddDepthLayer(i);
                }
            }
        }

        private void AddDepthLayer(int layer)
        {
            if (IsDepthIndependent && Coverages.Count >= 1)
            {
                throw new InvalidOperationException(
                    "Not allowed to add a depth layer to this definition; not depth dependent");
            }

            if (createCoverageFunc == null)
            {
                throw new InvalidOperationException(
                    "Cannot create new spatial data because the creation method has not been specified");
            }

            string name = verticalProfileDefinition.Type == VerticalProfileType.Uniform
                              ? Name
                              : Name + " (layer " + (layer + 1) + ")";

            Coverages.Add(createCoverageFunc(name));
        }
    }
}