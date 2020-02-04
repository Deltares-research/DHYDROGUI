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
                return GetTabulatedDefinition().Profile;
            }
        }

        public abstract CrossSectionDefinitionZW GetTabulatedDefinition();


        public virtual object Clone()
        {
            return TypeUtils.MemberwiseClone(this);
        }
        public virtual void AddToHydroNetwork(IHydroNetwork network, SewerImporterHelper helper)
        {
            var sewerSectionType = network.CrossSectionSectionTypes.FirstOrDefault(css => string.Equals(css.Name, RoughnessDataSet.SewerSectionTypeName, StringComparison.InvariantCultureIgnoreCase));
            if (sewerSectionType == null)
            {
                //sight.... this should be done somewhere on a higher level (where roughness sections can be synchronized with cross section sections)... but i am too tired to do this correctly because GWSW importer is a mess.
                sewerSectionType = new CrossSectionSectionType { Name = RoughnessDataSet.SewerSectionTypeName };
                network.CrossSectionSectionTypes.Add(sewerSectionType);
            }
            var crossSectionDefinitionToAdd = new CrossSectionDefinitionStandard(this)
            {
                Name = Name,
                Sections = { new CrossSectionSection{ SectionType = sewerSectionType } }
            };

            var pipesWithSameCrossSectionDefinitionId = network.Pipes.Where(p => string.Equals(p.CrossSectionDefinitionName, Name, StringComparison.InvariantCultureIgnoreCase));
            pipesWithSameCrossSectionDefinitionId.ForEach(p =>
            {
                p.CrossSectionDefinition = crossSectionDefinitionToAdd;
                p.Material = (SewerProfileMapping.SewerProfileMaterial)typeof(SewerProfileMapping.SewerProfileMaterial).GetEnumValueFromDescription(MaterialName);


            });
            
            network.SharedCrossSectionDefinitions.RemoveAllWhere(d =>string.Equals(d.Name, Name, StringComparison.InvariantCultureIgnoreCase));
            network.SharedCrossSectionDefinitions.Add(crossSectionDefinitionToAdd);
        }
    }
}