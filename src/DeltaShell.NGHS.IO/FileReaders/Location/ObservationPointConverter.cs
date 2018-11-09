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
    public static class ObservationPointConverter
    {
        public static IList<IObservationPoint> Convert(IList<DelftIniCategory> categories, IHydroNetwork network, IList<string> errorMessages)
        {
            IList<IObservationPoint> observationPoints = new List<IObservationPoint>();
            foreach (var observationPointCategory in categories.Where(category => category.Name == ObservationPointRegion.IniHeader))
            {
                try
                {
                    var generatedObservationPoint = ConvertToObservationPoint(observationPointCategory, network);
                    ValidateConvertedObservationPoint(generatedObservationPoint, observationPoints);
                    observationPoints.Add(generatedObservationPoint);
                }
                catch (Exception e)
                {
                    errorMessages.Add(e.Message);
                }
            }

            return observationPoints;
        }
   
        private static IObservationPoint ConvertToObservationPoint(IDelftIniCategory category, IHydroNetwork network)
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

            return new ObservationPoint()
            {
                Branch = branch,
                Name = name,
                LongName = longName,
                Chainage = chainage,
                Geometry = new Point(LengthLocationMap.GetLocation(branch.Geometry, chainage).GetCoordinate(branch.Geometry)),
            };
        }

        private static void ValidateConvertedObservationPoint(IObservationPoint readObservationPoint, IList<IObservationPoint> generatedObservationPoints)
        {
            if (readObservationPoint.IsDuplicateIn(generatedObservationPoints))
            {
                var errorMessage2 = string.Format("Observation Point with id {0} already exists, there cannot be any duplicate observation point ids.{1}", readObservationPoint.Name, Environment.NewLine);
                throw new Exception(errorMessage2);
            }
        }

        private static bool IsDuplicateIn(this IObservationPoint readObservationPoint, IList<IObservationPoint> observationPoints)
        {
            return observationPoints.Contains(readObservationPoint) || observationPoints.Any(n => n.Name == readObservationPoint.Name);
        }

    }
}

