using System.Collections.Generic;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Converters;
using log4net;
using ValidationAspects;
using ValidationAspects.Exceptions;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    /// <summary>
    /// The trigger reads
    /// if y(k) > 0 then 1  |
    /// <-- 0 seems invalid
    ///     else 0
    ///     The following operators are supported:>
    /// >= = <= <
    /// </summary>
    [Entity(FireOnCollectionChange = false)]
    public class StandardCondition : ConditionBase
    {
        private bool inputRequired;

        public StandardCondition() : this(true) {}

        public StandardCondition(bool inputRequired)
        {
            Operation = Operation.Equal;
            Reference = ReferenceType.Explicit; // = default EXPLICIT
            this.inputRequired = inputRequired;
        }

        /// <summary>
        /// valid values are "EXPLICIT" "IMPLICIT"; default is EXPLICIT
        /// </summary>
        public virtual string Reference { get; set; }

        public Operation Operation { get; set; }

        [ValidationMethod]
        public static void Validate(StandardCondition standardCondition)
        {
            var exceptions = new List<ValidationException>();

            if (standardCondition.Input == null && standardCondition.inputRequired)
            {
                exceptions.Add(new ValidationException(string.Format("Condition '{0}' has no input; this is required for standard conditions.", standardCondition.Name)));
            }

            if (exceptions.Count > 0)
            {
                throw new ValidationContextException(exceptions);
            }
        }

        public override string GetDescription()
        {
            return new OperationConverter().OperationToString(Operation) + Value;
        }

        public override object Clone()
        {
            var standardCondition = new StandardCondition();
            standardCondition.CopyFrom(this);
            return standardCondition;
        }

        public override void CopyFrom(object source)
        {
            var standardCondition = source as StandardCondition;
            if (standardCondition != null)
            {
                base.CopyFrom(source);
                Reference = standardCondition.Reference;
                Operation = standardCondition.Operation;
            }
        }

        public static class ReferenceType
        {
            public const string Explicit = "EXPLICIT";
            public const string Implicit = "IMPLICIT";
        }
    }
}