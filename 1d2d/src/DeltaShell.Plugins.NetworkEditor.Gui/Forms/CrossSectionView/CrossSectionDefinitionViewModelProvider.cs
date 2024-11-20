using System;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView
{
    public static class CrossSectionDefinitionViewModelProvider
    {
        public static CrossSectionDefinitionViewModel GetViewModel(ICrossSectionDefinition crossSectionDefinition,IHydroNetwork hydroNetwork = null)
        {
            var model = GetViewModelForType(crossSectionDefinition);

            model.CrossSectionSectionTypes = hydroNetwork != null ? 
                hydroNetwork.CrossSectionSectionTypes : 
                new EventedList<CrossSectionSectionType>();
            model.HydroNetwork = hydroNetwork;
            return model;
        }
        
        
        private static CrossSectionDefinitionViewModel GetViewModelForType(ICrossSectionDefinition crossSectionDefinition)
        {
            switch (crossSectionDefinition.CrossSectionType)
            {
                case CrossSectionType.GeometryBased:
                    return new CrossSectionDefinitionViewModel(false, "Y'Z Table (geometry based)", int.MaxValue, 2, "Y' (m)", "Z (m)", false);
                case CrossSectionType.YZ:
                    return new CrossSectionDefinitionViewModel(false, "Y'Z Table", int.MaxValue, 3, "Y' (m)", "Z (m)", false);
                case CrossSectionType.ZW:
                    return new CrossSectionDefinitionViewModel(true, "ZW Table", 3, 2, "Offset (m)", "Z (m)", false);
                case CrossSectionType.Standard:
                    return new CrossSectionDefinitionViewModel(false, "", 1, 0, "Offset (m)", "Z (m)", true);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }
}