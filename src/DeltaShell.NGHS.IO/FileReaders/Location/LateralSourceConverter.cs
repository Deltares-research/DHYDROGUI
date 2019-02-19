using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;

namespace DeltaShell.NGHS.IO.FileReaders.Location
{
    public static class LateralSourceConverter
    {
        public static IList<ILateralSource> Convert(IList<DelftIniCategory> categories, IList<IChannel> channelsList, IList<string> errorMessages)
        {
            IList<ILateralSource> lateralSources = new List<ILateralSource>();
            foreach (var observationPointCategory in categories.Where(category =>
                string.Equals(category.Name, BoundaryRegion.LateralDischargeHeader, StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var generatedLateralSource = ConvertToLateralSource(observationPointCategory, channelsList);
                    ValidateConvertedLateralSource(generatedLateralSource, lateralSources);
                    lateralSources.Add(generatedLateralSource);
                }
                catch (Exception e)
                {
                    errorMessages.Add(e.Message);
                }
            }
            
            return lateralSources;
        }
        
        private static ILateralSource ConvertToLateralSource(IDelftIniCategory category, IList<IChannel> channelsList)
        {
           // Essential Properties (an error will be generated if these fail)
            var name = category.ReadProperty<string>(LocationRegion.Id.Key);
            var chainage = category.ReadProperty<double>(LocationRegion.Chainage.Key);
            
            var branchName = category.ReadProperty<string>(LocationRegion.BranchId.Key);
            var branch = channelsList.FirstOrDefault(c => string.Equals(c.Name, branchName, StringComparison.OrdinalIgnoreCase));
            if (branch == null)
            {
                var errorMessage = string.Format("Unable to parse {0} property: {1}, Branch not found in Network.{2}", category.Name, LocationRegion.BranchId.Key, Environment.NewLine);
                throw new Exception(errorMessage);
            }

             // Optional Properties (an error will not be generated if these fail)
            var longName = category.ReadProperty<string>(LocationRegion.Name.Key, true) ?? string.Empty;
             var diffuseLength = category.ReadProperty<double?>(LateralSourceLocationRegion.Length.Key, true);

            var resultingChainage = chainage / branch.Length * branch.Geometry.Length;
            var geometry = new Point(
                LengthLocationMap.GetLocation(branch.Geometry, resultingChainage).GetCoordinate(branch.Geometry));

            var lateralSource = new LateralSource
            {
                Branch = branch,
                Name = name,
                LongName = longName,
                Chainage = chainage,
                Geometry = geometry,
            };
            if (diffuseLength.HasValue)
                lateralSource.Length = diffuseLength.Value;

            return lateralSource;
        }

        private static void ValidateConvertedLateralSource(ILateralSource readLateralSource, IList<ILateralSource> generatedLateralSources)
        {
            if (readLateralSource.IsDuplicateIn(generatedLateralSources))
            {
                var errorMessage2 = string.Format("Lateral source with id {0} already exists, there cannot be any duplicate lateral source ids.{1}", readLateralSource.Name, Environment.NewLine);
                throw new Exception(errorMessage2);
            }
        }

        private static bool IsDuplicateIn(this ILateralSource readLateralSource, IList<ILateralSource> lateralSources)
        {
            return lateralSources.Contains(readLateralSource) || lateralSources.Any(n => n.Name == readLateralSource.Name);
        }

    }
}

