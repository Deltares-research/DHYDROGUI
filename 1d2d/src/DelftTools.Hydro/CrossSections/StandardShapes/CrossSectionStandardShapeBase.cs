using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Data;
using DelftTools.Utils.Reflection;
using GeoAPI.Geometries;

namespace DelftTools.Hydro.CrossSections.StandardShapes
{
    [Entity(FireOnCollectionChange=false)]
    public abstract class CrossSectionStandardShapeBase : Unique<long>, ICrossSectionStandardShape
    {
        public virtual string Name { get; set; }

        public virtual string MaterialName { get; set; }

        public abstract CrossSectionStandardShapeType Type { get; }
        
        public virtual IEnumerable<Coordinate> Profile
        {
            get
            {
                return GetTabulatedDefinition().GetProfile();
            }
        }

        public abstract CrossSectionDefinitionZW GetTabulatedDefinition();


        public virtual object Clone()
        {
            return TypeUtils.MemberwiseClone(this);
        }

        public virtual void AddToHydroNetwork(IHydroNetwork hydroNetwork, SewerImporterHelper helper)
        {
            CrossSectionSectionType sewerSectionType = null;
            lock (hydroNetwork.CrossSectionSectionTypes)
            {
                sewerSectionType = hydroNetwork.CrossSectionSectionTypes.FirstOrDefault(css =>
                    string.Equals(css.Name, RoughnessDataSet.SewerSectionTypeName,
                        StringComparison.InvariantCultureIgnoreCase));
            }

            if (sewerSectionType == null)
            {
                //sight.... this should be done somewhere on a higher level (where roughness sections can be synchronized with cross section sections)... but i am too tired to do this correctly because GWSW importer is a mess.
                sewerSectionType = new CrossSectionSectionType {Name = RoughnessDataSet.SewerSectionTypeName};
                lock (hydroNetwork.CrossSectionSectionTypes)
                {
                    hydroNetwork.CrossSectionSectionTypes.Add(sewerSectionType);
                }
            }
            
            var crossSectionDefinitionToAdd = new CrossSectionDefinitionStandard(this)
            {
                Name = Name,
                Sections = {new CrossSectionSection {SectionType = sewerSectionType}},
            };
            
            lock (hydroNetwork.SharedCrossSectionDefinitions)
            {
                hydroNetwork.SharedCrossSectionDefinitions.RemoveAllWhere(d =>
                        string.Equals(d.Name, Name, StringComparison.InvariantCultureIgnoreCase));
                hydroNetwork.SharedCrossSectionDefinitions.Add(crossSectionDefinitionToAdd);
            }

            var sharedCrossSectionDefinitionToAdd = new CrossSectionDefinitionProxy(crossSectionDefinitionToAdd);
            if (helper != null)
            {
                helper?.CrossSectionDefinitionsByPipe?.AddOrUpdate(Name, sharedCrossSectionDefinitionToAdd,
                    (existingName, crossSectionDefinition) => { return crossSectionDefinition; });
                
                var materialEnumValue = string.IsNullOrWhiteSpace(MaterialName)
                                            ? null
                                            : (SewerProfileMapping.SewerProfileMaterial?) typeof(SewerProfileMapping.SewerProfileMaterial)
                                                .GetEnumValueFromDescription(MaterialName);

                var material = materialEnumValue ?? SewerProfileMapping.SewerProfileMaterial.Unknown;

                helper?.SewerProfileMaterialsByPipe?.AddOrUpdate(Name, material,
                                                                 (existingName, oldMaterial) => { return oldMaterial; });
            }
            else
            {
                var pipesWithSameCrossSectionDefinitionId = hydroNetwork.Pipes.Where(p => string.Equals(p.CrossSectionDefinitionName, Name, StringComparison.InvariantCultureIgnoreCase));
                pipesWithSameCrossSectionDefinitionId.ForEach(p =>
                {
                    p.CrossSection = new CrossSection(crossSectionDefinitionToAdd);
                    p.Material =
                        (SewerProfileMapping.SewerProfileMaterial) typeof(SewerProfileMapping.SewerProfileMaterial)
                            .GetEnumValueFromDescription(MaterialName);
                });
            }

        }
    }
}