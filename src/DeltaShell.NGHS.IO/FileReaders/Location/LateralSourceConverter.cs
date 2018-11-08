using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Extensions;
using DeltaShell.NGHS.IO.FileReaders.Network;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;

namespace DeltaShell.NGHS.IO.FileReaders.Location
{
    public static class LateralSourceConverter
    {
        public static IList<ILateralSource> Convert(IList<DelftIniCategory> categories, IHydroNetwork network, IList<FileReadingException> fileReadingExceptions)
        {
            IList<ILateralSource> lateralSources = new List<ILateralSource>();
            IList<string> errorMessages = new List<string>();
            foreach (var observationPointCategory in categories.Where(category => category.Name == BoundaryRegion.LateralDischargeHeader))
            {
                try
                {
                    var generatedLateralSource = ConvertToLateralSource(observationPointCategory, network);
                    errorMessages.AddRange(ValidateConvertedLateralSource(generatedLateralSource, lateralSources));
                    lateralSources.Add(generatedLateralSource);
                }
                catch (Exception e)
                {
                    errorMessages.Add(e.Message);
                }
            }

            if (errorMessages.Count > 0)
            {
                var fileReadingException = FileReadingException.GetReportAsException("lateral sources", errorMessages);
                fileReadingExceptions.Add(fileReadingException);
            }

            return lateralSources;
        }
        
        private static ILateralSource ConvertToLateralSource(IDelftIniCategory category, IHydroNetwork network)
        {
           // Essential Properties (an error will be generated if these fail)
            var name = category.ReadProperty<string>(LocationRegion.Id.Key);
            var chainage = category.ReadProperty<double>(LocationRegion.Chainage.Key);
            
            var branchName = category.ReadProperty<string>(LocationRegion.BranchId.Key);
            var branch = network.Channels.FirstOrDefault(c => c.Name == branchName);
            if (branch == null)
            {
                var errorMessage = string.Format("Unable to parse {0} property: {1}, Branch not found in Network.{2}", category.Name, LocationRegion.BranchId.Key, Environment.NewLine);
                throw new FileReadingException(errorMessage);
            }

             // Optional Properties (an error will not be generated if these fail)
            var longName = category.ReadProperty<string>(LocationRegion.Name.Key, true) ?? string.Empty;
             var diffuseLength = category.ReadProperty<double?>(LateralSourceLocationRegion.Length.Key, true);

            var lateralSource = new LateralSource()
            {
                Branch = branch,
                Name = name,
                LongName = longName,
                Chainage = chainage,
                Geometry = new Point(LengthLocationMap.GetLocation(branch.Geometry, chainage).GetCoordinate(branch.Geometry)),
            };
            if (diffuseLength.HasValue)
                lateralSource.Length = diffuseLength.Value;

            return lateralSource;
        }

        private static IEnumerable<string> ValidateConvertedLateralSource(ILateralSource readLateralSource, IList<ILateralSource> generatedLateralSources)
        {
            if (readLateralSource.IsDuplicateIn(generatedLateralSources))
                yield return $"Observation Point with id {readLateralSource.Name} already exists, there cannot be any duplicate Node ids";
        }

        private static bool IsDuplicateIn(this ILateralSource readLateralSource, IList<ILateralSource> nodes)
        {
            return nodes.Contains(readLateralSource) || nodes.Any(n => n.Name == readLateralSource.Name);
        }

    }
}

