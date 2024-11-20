using System.Collections.Generic;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Validation;
using ValidationAspects;

namespace DelftTools.Hydro.Validators
{
    public static class StructuresValidator
    {
        public static ValidationReport Validate(IHydroNetwork network)
        {
            var issues = new List<ValidationIssue>();

            foreach (var composite in network.CompositeBranchStructures)
            {
                if (composite.Structures.Count == 0)
                {
                    issues.Add(new ValidationIssue(composite, ValidationSeverity.Error, "Does not contain any structures",
                        new ValidatedFeatures(network, composite)));
                }

                foreach (var structure in composite.Structures)
                {
                    //generic validation (using Validation Aspects)
                    var result = structure.Validate();
                    if (!result.IsValid)
                    {
                        issues.Add(new ValidationIssue(structure, ValidationSeverity.Error,
                            result.ValidationException.Message));
                    }

                    switch (structure)
                    {
                        case IWeir weir:
                            ValidateWeir(weir, issues);
                            break;
                        case IPump pump:
                            ValidatePump(pump, issues);
                            break;
                    }
                }
            }
            return new ValidationReport("Structures", issues);
        }
        private static void ValidatePump(IPump structure, List<ValidationIssue> issues)
        {
            // Capacity must be >= 0
            if (structure.Capacity < 0)
            {
                issues.Add(new ValidationIssue(structure, ValidationSeverity.Error, "pump '" + structure.Name + "\': Capacity must be greater than or equal to 0."));
            }

            switch (structure.ControlDirection)
            {
                case PumpControlDirection.DeliverySideControl:
                    ValidatePumpDeliverySide(structure, issues);
                    break;
                case PumpControlDirection.SuctionAndDeliverySideControl:
                    ValidatePumpDeliverySide(structure, issues);
                    ValidatePumpSuctionSide(structure, issues);
                    break;
                case PumpControlDirection.SuctionSideControl:
                    ValidatePumpSuctionSide(structure, issues);
                    break;
            }
        }

        private static void ValidatePumpSuctionSide(IPump sobekPump, ICollection<ValidationIssue> issues)
        {
            if (sobekPump.StartSuction < sobekPump.StopSuction)
            {
                issues.Add(new ValidationIssue(sobekPump, ValidationSeverity.Error,
                    "pump '" + sobekPump.Name + "\': Suction start level must be greater than or equal to suction stop level."));
            }
        }

        private static void ValidatePumpDeliverySide(IPump sobekPump, ICollection<ValidationIssue> issues)
        {
            if (sobekPump.StartDelivery > sobekPump.StopDelivery)
            {
                issues.Add(new ValidationIssue(sobekPump, ValidationSeverity.Error,
                    "pump '" + sobekPump.Name + "\': Delivery start level must be less than or equal to delivery stop level."));
            }
        }

        private static void ValidateWeir(IWeir structure, ICollection<ValidationIssue> issues)
        {
            if (structure.CrestWidth < 0)
            {
                issues.Add(new ValidationIssue(structure, ValidationSeverity.Error, "weir '" + structure.Name + "\': Crest width must be greater than or equal to 0."));
            }

            if (structure.WeirFormula is GatedWeirFormula)
            {
                ValidateGatedWeirFormula(structure, issues);
            }
        }

        private static void ValidateGatedWeirFormula(IWeir weir, ICollection<ValidationIssue> issues)
        {
            var gatedWeirFormula = (GatedWeirFormula)weir.WeirFormula;

            if (gatedWeirFormula.GateOpening < 0)
            {
                issues.Add(new ValidationIssue(weir, ValidationSeverity.Error, "weir '" + weir.Name + "\': Gate opening must be greater than or equal to 0."));
            }

            if (gatedWeirFormula.UseMaxFlowPos && gatedWeirFormula.MaxFlowPos < 0)
            {
                issues.Add(new ValidationIssue(weir, ValidationSeverity.Error, "weir '" + weir.Name + "\': Maximum positive flow restrictions must be greater than or equal to 0."));
            }
            if (gatedWeirFormula.UseMaxFlowNeg && gatedWeirFormula.MaxFlowNeg < 0)
            {
                issues.Add(new ValidationIssue(weir, ValidationSeverity.Error, "weir '" + weir.Name + "\': Maximum negative flow restrictions must be greater than or equal to 0."));
            }
        }

    }
}