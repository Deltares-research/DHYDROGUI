using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using NetTopologySuite.Extensions.Coverages;

namespace DelftTools.Hydro {
    public partial class HydroNetwork
    {
        [EditAction]
        private void RoutesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var route = e.GetRemovedOrAddedItem() as Route;

            if (route == null)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    route.Network = this;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    route.Network = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [EditAction]
        private void SharedCrossSectionDefinitionsCollectionChanging(object sender, NotifyCollectionChangingEventArgs e)
        {
            //check if the section is in use somewhere..the deletion is not allowed
            if ((e.Action == NotifyCollectionChangeAction.Replace) || (e.Action == NotifyCollectionChangeAction.Remove))
            {
                var definitionBeingRemoved = e.Item as ICrossSectionDefinition;

                if (definitionBeingRemoved == null)
                {
                    return;
                }

                if (definitionBeingRemoved == DefaultCrossSectionDefinition)
                {
                    DefaultCrossSectionDefinition = null;
                }

                var crossSectionsUsingDefinitionBeingRemoved =
                    CrossSections.Where(
                        cs => cs.Definition.IsProxy &&
                              ((CrossSectionDefinitionProxy) cs.Definition).InnerDefinition == definitionBeingRemoved);

                if (crossSectionsUsingDefinitionBeingRemoved.Any())
                {
                    log.ErrorFormat(
                        "Cannot remove definition '{0}', it is in use by {1} cross section(s). (For example cross section: '{2}').",
                        definitionBeingRemoved.Name, crossSectionsUsingDefinitionBeingRemoved.Count(),
                        crossSectionsUsingDefinitionBeingRemoved.First().Name);
                    e.Cancel = true;
                }
            }
            else if (e.Action == NotifyCollectionChangeAction.Add)
            {
                if (e.Item is CrossSectionDefinitionXYZ)
                {
                    throw new NotSupportedException("XYZ cross sections cannot be added as definitions.");
                }
            }
        }

        [EditAction]
        private void SectionTypesPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name")
            {
                var sectionName = ((CrossSectionSectionType) sender).Name;
                if (crossSectionSectionTypes.Count(sec => sec.Name == sectionName) > 1)
                {
                    ((CrossSectionSectionType) sender).Name = sectionName + "_1";
                    return;
                }

                CrossSections.Select(cs => cs.Definition)
                             .OfType<CrossSectionDefinitionZW>().ForEach(csd => csd.RemoveInvalidSections());
            }
        }

        [EditAction]
        private void SectionTypesCollectionChanging(object sender, NotifyCollectionChangingEventArgs e)
        {
            //check if the section is in use somewhere..the deletion is not allowed
            if ((e.Action == NotifyCollectionChangeAction.Replace) || (e.Action == NotifyCollectionChangeAction.Remove))
            {
                var crossSectionSectionType = (CrossSectionSectionType) e.Item;

                var crossSection =
                    CrossSections.FirstOrDefault(
                        c => c.Definition.Sections.Any(sec => sec.SectionType == crossSectionSectionType));
                if (crossSection != null)
                {
                    log.ErrorFormat("Unable to remove section type. It is in use in cross section {0}.",
                                    crossSection.Name);
                    e.Cancel = true;
                }
            }

            if (e.Action == NotifyCollectionChangeAction.Add)
            {
                var sectionName = ((CrossSectionSectionType) e.Item).Name;
                if (crossSectionSectionTypes.Select(sec => sec.Name).Contains(sectionName))
                {
                    log.ErrorFormat("Unable to add cross section section type with non-identical name {0}.", sectionName);
                    e.Cancel = true;
                }
            }
        }
    }
}