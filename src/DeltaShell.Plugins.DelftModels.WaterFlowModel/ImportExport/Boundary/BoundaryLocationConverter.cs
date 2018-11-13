using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Boundary
{
    /// <summary>
    /// BoundaryLocationConverter is responsible for extracting a set of BoundaryLocations
    /// from a set of DelftIniCategories describing a BoundaryLocation file.
    /// </summary>
    public static class BoundaryLocationConverter
    {
        /// <summary>
        /// Convert the <paramref name="dataAccessModel"/> of a set of BoundaryLocations
        /// into an actual set of BoundaryLocations.
        /// </summary>
        /// <param name="dataAccessModel"> A set of DelftIniCategories describing a set of BoundaryLocations</param>
        /// <returns>A set of BoundaryLocations</returns>
        public static IList<BoundaryLocation> Convert(IList<DelftIniCategory> dataAccessModel, IList<string> errorMessages)
        {
            var boundaryLocations = new List<BoundaryLocation>();

            if (!ValidateCategoryList(dataAccessModel, errorMessages))
                return boundaryLocations;

            foreach (var category in dataAccessModel)
            {
                if (Validate(category, errorMessages))
                    boundaryLocations.Add(ConvertToBoundaryLocation(category));
            }

            return boundaryLocations;
        }

        private static bool ValidateCategoryList(IList<DelftIniCategory> dataCategories, IList<string> errorMessages)
        {
            if (dataCategories == null || !dataCategories.Any())
            {
                errorMessages.Add("Unable to parse empty set of boundary locations.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validate the current category and return whether <paramref name="category"/> is convertible.
        ///
        /// Any Problems encountered will be added to error messages.
        /// </summary>
        /// <returns>True if <paramref name="category"/> can be converted. </returns>
        private static bool Validate(DelftIniCategory category, IList<string> errorMessages)
        {
            if (category == null)
            {
                errorMessages.Add("Unable to parse null category.");
                return false;
            }

            if (category.Name != BoundaryRegion.BoundaryHeader)
            {
                errorMessages.Add($"Could not parse boundary location category: {category.Name} at line {category.LineNumber}: Invalid header");
                return false;
            }

            // Validate not enough properties
            var nProperties = category.Properties.Count();
            if (nProperties < 2)
            {
                errorMessages.Add($"Could not parse boundary location category: {category.Name} at line {category.LineNumber}: Missing data");
                return false;
            }

            // Validate if at least NodeID and type are properly defined
            // Validate NodeID
            var nNodeId = category.Properties.Count(e => e.Name == BoundaryRegion.NodeId.Key);
            if (nNodeId < 1)
            {
                errorMessages.Add($"Could not parse boundary location category: {category.Name} at line {category.LineNumber}: Missing data");
                return false;
            }

            if (nNodeId > 1)
            {
                errorMessages.Add($"Could not parse boundary location category: {category.Name} at line {category.LineNumber}: Multiple defined data");
                return false;
            } 

            // Validate Type
            var nType = category.Properties.Count(e => e.Name == BoundaryRegion.Type.Key);
            if (nType < 1)
            {
                errorMessages.Add(
                    $"Could not parse boundary location category: {category.Name} at line {category.LineNumber}: Missing data");
                return false;
            }

            if (nType > 1)
            {
                errorMessages.Add($"Could not parse boundary location category: {category.Name} at line {category.LineNumber}: Multiple defined data");
                return false;
            }

            int? type;
            try
            {
                type = category.ReadProperty<int?>(BoundaryRegion.Type.Key);
            }
            catch (System.Exception e)
            {
                // This function should not fail when parsing unknown data
                type = null;
            }

            if (type == null || type < 1 || type > 2)
            {
                errorMessages.Add($"Could not parse boundary location category: {category.Name} at line {category.LineNumber}: Invalid data");
                return false;
            }

            
            // If a third variable exists, it should be Thatcher-Harlemann
            var expectedNProperties = 2;

            // Validate Thatcher-Harlemann coefficient
            var nTHC = category.Properties.Count(e => e.Name == BoundaryRegion.ThatcherHarlemanCoeff.Key);
            if (nTHC > 1)
            {
                errorMessages.Add($"Could not parse boundary location category: {category.Name} at line {category.LineNumber}: Multiple defined data");
                return false;
            }

            double? coefTH = null;
            if (nTHC == 1)
            {
                try
                {
                    coefTH = category.ReadProperty<double?>(BoundaryRegion.ThatcherHarlemanCoeff.Key);
                }
                catch (System.Exception e)
                {
                    // coefTH should stay null, if the value cannot be read.
                }


                if (coefTH == null)
                {
                    errorMessages.Add(
                        $"Could not parse boundary location category: {category.Name} at line {category.LineNumber}: Invalid data");
                    return false;
                }
                expectedNProperties += 1;
            }

            // These values will still be parseable, but the user needs to be informed
            if (nProperties > expectedNProperties)
            {
                errorMessages.Add($"Location category contains additional data: {category.Name} at line {category.LineNumber}: Unknown data");
            }

            // Thatcher-Harlemann exists, however it uses an invalid value, thus warn the user only.
            if (coefTH != null && coefTH < 0.0)
            {
                errorMessages.Add(
                    $"Could not parse Thatcher Harlemann Coefficient: {category.Name} at line {category.LineNumber}: Defaulting to zero");
            }

            return true;
        }


        /// <summary>
        /// Convert the specified category to a BoundaryLocation.
        /// </summary>
        /// <param name="category">The Category to be converted.</param>
        /// <pre-condition>this.Validate(category, [])</pre-condition>
        /// <returns>A boundary location corresponding with the specified category.</returns>
        private static BoundaryLocation ConvertToBoundaryLocation(DelftIniCategory category)
        {
            var id = category.ReadProperty<string>(BoundaryRegion.NodeId.Key);
            BoundaryType type = category.ReadProperty<int>(BoundaryRegion.Type.Key) == 1
                ? BoundaryType.Level
                : BoundaryType.Discharge;

            var thatcherHarlemannCoefficient = category.ReadProperty<double>(BoundaryRegion.ThatcherHarlemanCoeff.Key, isOptional:true);
            // Thatcher Harlemann coefficient cannot be negative.
            if (thatcherHarlemannCoefficient < 0.0)
                thatcherHarlemannCoefficient = 0.0;

            return new BoundaryLocation(id, type, thatcherHarlemannCoefficient);
        }
    }
}
