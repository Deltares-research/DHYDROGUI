using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas
{
    /// <summary>
    /// A coverage used to define observation area's. The user will specify these area's
    /// with spatial operations and setting a name for these regions. Internally, a storage
    /// of int is used for rendering purposed why keeping a int-strign association between
    /// the two.
    /// </summary>
    public class WaterQualityObservationAreaCoverage : UnstructuredGridCellCoverage
    {
        public const string NoDataLabel = null;

        public WaterQualityObservationAreaCoverage(UnstructuredGrid grid) : base(grid, false)
        {
            // Data is stored as int as this makes visualization easier, while exposing this as a coverage of 'strings' in the UI.
            Components.Clear();
            Components.Add(new Variable<int>("value")
            {
                DefaultValue = -999,
                NoDataValue = -999
            });
        }

        protected WaterQualityObservationAreaCoverage() {} // NHibernate

        /// <summary>
        /// Gives all specified observation areas by name and their corresponding grid cells.
        /// </summary>
        public virtual Dictionary<string, IList<int>> GetOutputLocations()
        {
            var obsInformation = new Dictionary<string, IList<int>>();
            var cellIndex = 0;
            foreach (int intValue in GetValues<int>()) // Not using ToArray and for loop to save RAM
            {
                if (!Equals(Components[0].NoDataValue, intValue))
                {
                    string observationAreaName = GetLabel(intValue);
                    if (!obsInformation.ContainsKey(observationAreaName))
                    {
                        obsInformation[observationAreaName] = new List<int>();
                    }

                    obsInformation[observationAreaName].Add(cellIndex + 1); // + 1 because waq is one based
                }

                cellIndex++;
            }

            return obsInformation;
        }

        /// <summary>
        /// Calls <see cref="Function.SetValues{T}"/> with ints. The labels that were not yet present
        /// are added to the list of labels. Then SetValues is called to actually
        /// set the ints on the coverage.
        /// Use null as NoDataValue.
        /// </summary>
        public virtual void SetValuesAsLabels(IEnumerable<string> labels)
        {
            var result = new List<int>();

            // generate a list of ints to set on the coverage
            foreach (string label in labels)
            {
                result.Add(AddLabel(label));
            }

            SetValues(result);
        }

        /// <summary>
        /// Calls <see cref="IFunction.GetValues{T}"/> and transforms all
        /// ints into the corresponding labels.
        /// </summary>
        public virtual IList<string> GetValuesAsLabels()
        {
            IMultiDimensionalArray<int> values = GetValues<int>();
            var result = new List<string>(values.Count);

            foreach (int value in values)
            {
                if (value == (int) Components[0].NoDataValue)
                {
                    result.Add(NoDataLabel);
                }
                else
                {
                    result.Add(GetLabel(value));
                }
            }

            return result;
        }

        /// <summary>
        /// Add a label to the list of labels.
        /// This can be used if a method calls <see cref="Function.SetValues{T}"/> instead of
        /// <see cref="SetValuesAsLabels"/> afterwards.
        /// </summary>
        /// <returns> The index that can be used in SetValues{int}. Returns NoDataValue when label is null. </returns>
        public virtual int AddLabel(string label)
        {
            if (label == NoDataLabel)
            {
                return (int) Components[0].NoDataValue;
            }

            label = label.ToLowerInvariant();

            string intValue;
            if (Components[0].Attributes.TryGetValue(label, out intValue))
            {
                return Convert.ToInt32(intValue);
            }
            else
            {
                int newIndex = Components[0].Attributes.Count;
                Components[0].Attributes.Add(label, newIndex.ToString());

                return newIndex;
            }
        }

        public virtual IEnumerable<string> GetLabelList()
        {
            return Components[0].Attributes != null
                       ? Components[0].Attributes.Keys
                       : Enumerable.Empty<string>().ToList();
        }

        /// <summary>
        /// Find the label corresponding to the integer value that is used in the coverage.
        /// </summary>
        /// <returns> Null if the int was no data value </returns>
        private string GetLabel(int intValue)
        {
            if (intValue == (int) Components[0].NoDataValue)
            {
                return NoDataLabel;
            }

            var intString = intValue.ToString();
            KeyValuePair<string, string>
                result = Components[0].Attributes.FirstOrDefault(kvp => kvp.Value == intString);

            if (!Equals(result, default(KeyValuePair<string, string>)))
            {
                return result.Key;
            }
            else
            {
                return NoDataLabel;
            }
        }
    }
}