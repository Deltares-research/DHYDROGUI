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
        public static IList<ILateralSource> Convert(IList<DelftIniCategory> categories, IHydroNetwork network, IList<string> errorMessages)
        {
            IList<ILateralSource> lateralSources = new List<ILateralSource>();
            foreach (var observationPointCategory in categories.Where(category => category.Name == BoundaryRegion.LateralDischargeHeader))
            {
                try
                {
                    var generatedLateralSource = ConvertToLateralSource(observationPointCategory, network);
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
                throw new Exception(errorMessage);
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

