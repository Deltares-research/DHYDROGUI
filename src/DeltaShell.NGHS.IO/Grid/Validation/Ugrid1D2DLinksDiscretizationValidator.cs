using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.IO.Grid.DeltaresUGrid;
using DeltaShell.NGHS.IO.Properties;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.NGHS.IO.Grid.Validation
{
    /// <summary>
    /// Validates (U)grid meshes with network coverage <seealso cref="IDiscretization"/> and the links connected to them
    /// </summary>
    public class Ugrid1D2DLinksDiscretizationValidator : IValidator<IEnumerable<ILink1D2D>, IDiscretization>
    {
        private readonly IHydroModel modelToValidateIn;

        public Ugrid1D2DLinksDiscretizationValidator(IHydroModel model)
        {
            modelToValidateIn = model;
        }

        /// <summary>
        /// Validates (U)grid meshes with network coverage <seealso cref="IDiscretization"/> and the links connected to them
        /// </summary>
        /// <param name="link1D2Ds">The links between the 1d mesh and the 2d mesh.</param>
        /// <param name="discretization">The network coverage discretization used for calculation results of the 1 mesh / network.</param>
        /// <returns><seealso cref="ValidationReport"/> with the results of vadility of the 1d2d links with the provided discretization.</returns>
        public ValidationReport Validate(IEnumerable<ILink1D2D> link1D2Ds, IDiscretization discretization = null)
        {
            IEnumerable<ValidationIssue> issues;

            if (discretization == null)
            {
                issues = new List<ValidationIssue>() { new ValidationIssue(modelToValidateIn, ValidationSeverity.Error, Resources.Ugrid1D2DLinksDiscretizationValidator_Validate_Discretization_for_1D_network_is_not_set) };
            }
            else
            {
                var mesh1d = discretization.CreateDisposable1DMeshGeometry();
                issues = mesh1d.ValidateMesh1DSourceLocationsOnlyExistOnce(link1D2Ds)
                               .Select(errorMessage => new ValidationIssue(link1D2Ds, ValidationSeverity.Error, errorMessage));
            }

            return new ValidationReport(Resources.Ugrid1D2DLinksDiscretizationValidator_Validate__1D2D_link_mesh1D_source_discretization_locations_validation, issues);
        }
    }
}