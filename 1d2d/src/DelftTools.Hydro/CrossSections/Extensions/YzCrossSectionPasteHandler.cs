using DelftTools.Hydro.CrossSections.DataSets;

namespace DelftTools.Hydro.CrossSections.Extensions
{
    /// <summary>
    /// Handler for pasting into a Yz-cross-section.
    /// </summary>
    internal class YzCrossSectionPasteHandler : ICrossSectionPasteHandler
    {
        private readonly CrossSectionDefinitionYZ crossSectionDefinitionYz;

        public YzCrossSectionPasteHandler(CrossSectionDefinitionYZ crossSectionDefinition)
        {
            crossSectionDefinitionYz = crossSectionDefinition;
        }

        public void FinishPasteActions()
        {
            FindNewThalweg();
        }

        /// <summary>
        /// Find new Thalweg, new Thalweg will be lowest point or lowest point in sequence.
        /// </summary>
        private void FindNewThalweg()
        {
            if (crossSectionDefinitionYz.YZDataTable.Rows.Count <= 0)
            {
                return;
            }

            crossSectionDefinitionYz.Thalweg = FindLowestPointOrLowestPointInSequenceForNewThalweg(crossSectionDefinitionYz.Thalweg, crossSectionDefinitionYz.YZDataTable);
        }

        private double FindLowestPointOrLowestPointInSequenceForNewThalweg(double d, FastYZDataTable fastYzDataTable)
        {
            double lowestPoint = fastYzDataTable.Rows[0].Z;
            double startOfSequence = fastYzDataTable.Rows[0].Yq;
            double newThalweg = d;

            foreach (CrossSectionDataSet.CrossSectionYZRow row in fastYzDataTable)
            {
                if (row.Z < lowestPoint)
                {
                    lowestPoint = row.Z;
                    startOfSequence = row.Yq;
                    newThalweg = row.Yq;
                }
                else if (LowestPointsAreInSequence(row.Z, lowestPoint))
                {
                    newThalweg = CalculateThalwegForSequence(startOfSequence, row.Yq);
                }
            }

            return newThalweg;
        }

        private bool LowestPointsAreInSequence(double currentPoint, double lowestPoint)
        {
            return currentPoint.Equals(lowestPoint);
        }

        private double CalculateThalwegForSequence(double startOfSequence, double endOfSequence)
        {
            return (startOfSequence + endOfSequence) / 2;
        }
    }
}