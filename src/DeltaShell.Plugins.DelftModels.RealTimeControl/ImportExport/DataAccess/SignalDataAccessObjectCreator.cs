using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xsd;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport.DataAccess
{
    /// <summary>
    /// Creates a <see cref="SignalDataAccessObject"/> based of a <see cref="RuleXML"/>.
    /// </summary>
    public static class SignalDataAccessObjectCreator
    {
        /// <summary>
        /// Creates a <see cref="SignalDataAccessObject"/> from the specified <paramref name="signalElement"/>.
        /// </summary>
        /// <param name="signalElement"> The rule XML. </param>
        /// <returns>
        /// A <see cref="SignalDataAccessObject"/> created from the specified <paramref name="signalElement"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="signalElement"/> is <c>null</c>.
        /// </exception>
        public static SignalDataAccessObject Create(RuleXML signalElement)
        {
            Ensure.NotNull(signalElement, nameof(signalElement));

            if (!(signalElement.Item is LookupTableXML lookupTableElement))
            {
                return null;
            }

            string id = lookupTableElement.id;
            LookupSignal signal = CreateLookupSignal(lookupTableElement);
            var signalDataAccessObject = new SignalDataAccessObject(id, signal);

            signalDataAccessObject.InputReferences.Add(lookupTableElement.input.x.Value);

            return signalDataAccessObject;
        }

        private static LookupSignal CreateLookupSignal(LookupTableXML lookupTableElement)
        {
            string signalName = RealTimeControlXmlReaderHelper.GetComponentNameFromElementId(lookupTableElement.id);
            List<DateRecord2DataXML> records = (lookupTableElement.Item as TableLookupTableXML)?.record;

            var signal = new LookupSignal
            {
                Name = signalName
            };

            if (records != null)
            {
                DefineFunction(signal.Function, records);
            }

            signal.Interpolation = GetInterpolationType(lookupTableElement.interpolationOption);
            signal.Extrapolation = GetExtrapolationType(lookupTableElement.extrapolationOption);

            return signal;
        }

        private static void DefineFunction(IFunction function, List<DateRecord2DataXML> records)
        {
            if (records == null || function == null)
            {
                return;
            }

            function.Arguments[0].SetValues(records.Select(e => e.x));
            function.Components[0].SetValues(records.Select(e => e.y));
        }

        private static ExtrapolationType GetExtrapolationType(interpolationOptionEnumStringType extrapolationOption)
        {
            return extrapolationOption == interpolationOptionEnumStringType.BLOCK
                       ? ExtrapolationType.Constant
                       : ExtrapolationType.Linear;
        }

        private static InterpolationType GetInterpolationType(interpolationOptionEnumStringType interpolationOption)
        {
            return interpolationOption == interpolationOptionEnumStringType.BLOCK
                       ? InterpolationType.Constant
                       : InterpolationType.Linear;
        }
    }
}