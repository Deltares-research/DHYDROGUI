using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
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

        public virtual void AddToHydroNetwork(IHydroNetwork network)
        {
            var crossSectionDefinitionToAdd = new CrossSectionDefinitionStandard(this)
            {
                Name = Name
            };
            var pipesWithSameCrossSectionDefinitionId = network.Pipes.Where(p => p.CrossSectionDefinitionId == Name);
            pipesWithSameCrossSectionDefinitionId.ForEach(p =>
            {
                p.CrossSectionDefinition = crossSectionDefinitionToAdd;
                p.Material = (SewerProfileMapping.SewerProfileMaterial)EnumDescriptionAttributeTypeConverter.GetEnumValue<SewerProfileMapping.SewerProfileMaterial>(MaterialName);
            });
            
            network.SharedCrossSectionDefinitions.RemoveAllWhere(d => d.Name == Name);
            network.SharedCrossSectionDefinitions.Add(crossSectionDefinitionToAdd);
        }
    }
}