using System;
using DelftTools.Hydro.CrossSections;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.ProfileMutators;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView
{
    [Obsolete("D3DFMIQ-1923 remove cross section")]
    public static class CrossSectionProfileMutatorProvider
    {
        public static ICrossSectionProfileMutator GetFlowProfileMutator(this ICrossSectionDefinition crossSectionDefinition)
        {
            if (crossSectionDefinition.IsProxy)
            {
                var crossSectionDefinitionProxy = (CrossSectionDefinitionProxy) crossSectionDefinition;
                ICrossSectionProfileMutator innerMutator = crossSectionDefinitionProxy.InnerDefinition.GetFlowProfileMutator();
                return new ProxyProfileMutator(crossSectionDefinitionProxy, innerMutator);
            }

            switch (crossSectionDefinition.CrossSectionType)
            {
                case CrossSectionType.GeometryBased:
                    return new XYZFlowProfileMutator((CrossSectionDefinitionXYZ) crossSectionDefinition);
                case CrossSectionType.YZ:
                    return new YZFlowProfileMutator((CrossSectionDefinitionYZ) crossSectionDefinition);
                case CrossSectionType.ZW:
                    return new ZWFlowProfileMutator((CrossSectionDefinitionZW) crossSectionDefinition);
                case CrossSectionType.Standard:
                    return new ImmutableProfileMutator();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static ICrossSectionProfileMutator GetProfileMutator(this ICrossSectionDefinition crossSectionDefinition)
        {
            if (crossSectionDefinition.IsProxy)
            {
                var crossSectionDefinitionProxy = (CrossSectionDefinitionProxy) crossSectionDefinition;
                ICrossSectionProfileMutator innerMutator = crossSectionDefinitionProxy.InnerDefinition.GetProfileMutator();
                return new ProxyProfileMutator(crossSectionDefinitionProxy, innerMutator);
            }

            switch (crossSectionDefinition.CrossSectionType)
            {
                case CrossSectionType.GeometryBased:
                    return new XYZProfileMutator((CrossSectionDefinitionXYZ) crossSectionDefinition);
                case CrossSectionType.YZ:
                    return new YZProfileMutator((CrossSectionDefinitionYZ) crossSectionDefinition);
                case CrossSectionType.ZW:
                    return new ZWProfileMutator((CrossSectionDefinitionZW) crossSectionDefinition);
                case CrossSectionType.Standard:
                    return new ImmutableProfileMutator();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}