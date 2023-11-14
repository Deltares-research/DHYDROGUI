using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Validation.NameValidation;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro.Validators
{
    /// <summary>
    /// Unique name validation services for <see cref="ICompartment"/>.
    /// </summary>
    public class CompartmentUniqueNameValidationService : ContainedItemUniqueNameValidationService<INode, ICompartment>
    {
        public CompartmentUniqueNameValidationService(IEventedList<INode> containers)
            : base(containers)
        {
        }

        protected override IEventedList<ICompartment> GetContainedItems(INode container)
        {
            if (container is IManhole manhole)
            {
                return manhole.Compartments;
            }

            return null;
        }
    }
}