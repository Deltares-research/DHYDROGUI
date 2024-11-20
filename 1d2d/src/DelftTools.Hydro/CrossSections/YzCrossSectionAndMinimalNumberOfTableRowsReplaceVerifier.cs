using DelftTools.Controls;

namespace DelftTools.Hydro.CrossSections {
    internal class YzCrossSectionAndMinimalNumberOfTableRowsReplaceVerifier : IReplaceVerifier
    {
        private readonly int minimalNumberOfTableRows;

        /// <summary>
        /// Verifies if replacing is allowed and runs finished actions.
        /// </summary>
        /// <param name="minimalNumberOfTableRows">Minimal number of rows allowed in table.</param>
        public YzCrossSectionAndMinimalNumberOfTableRowsReplaceVerifier(int minimalNumberOfTableRows)
        {
            this.minimalNumberOfTableRows = minimalNumberOfTableRows;
        }

        /// <summary>
        /// Verifies if data should be replaced.
        /// </summary>
        /// <param name="lineCount">Amount of rows in table.</param>
        /// <returns></returns>
        public bool ShouldReplace(int lineCount)
        {
            return lineCount >= minimalNumberOfTableRows;
        }
    }
}