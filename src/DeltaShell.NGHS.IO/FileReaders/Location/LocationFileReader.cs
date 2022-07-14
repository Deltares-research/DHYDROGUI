using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;

namespace DeltaShell.NGHS.IO.FileReaders.Location
{
    public static class LocationFileReader
    {
        private class LocationPropertyValues
        {
            public string name, longName;
            public double chainage;
            public IChannel branch;
            public double? diffuseLength;
        }
        
        public static void ReadFileObservationPointLocations(string filename, IHydroNetwork network)
        {
            if (!File.Exists(filename)) throw new FileReadingException(string.Format(Resources.Could_not_read_file_0_properly_it_doesnt_exist, filename));
            var categories = new DelftIniReader().ReadDelftIniFile(filename);
            if (categories.Count == 0) return;
            
            IList<FileReadingException> fileReadingExceptions = new List<FileReadingException>();  

            IList<LocationPropertyValues> locationPropertyValuesList = new List<LocationPropertyValues>();
            foreach (var observationPointCategory in categories.Where(category => category.Name == ObservationPointRegion.IniHeader))
            {
                try
                {
                    var locationPropertyValues = GetCommonLocationPropertyValues(observationPointCategory, network);
                    if (locationPropertyValues == null) continue;
                    locationPropertyValuesList.Add(locationPropertyValues);
                }
                catch (FileReadingException fileReadingException)
                {
                    fileReadingExceptions.Add(new FileReadingException("Could not read observation point", fileReadingException));
                }
            }
            if (fileReadingExceptions.Count > 0)
            {
                var innerExceptionMessages = fileReadingExceptions.Select(fileReadingException => fileReadingException.InnerException.Message + Environment.NewLine);
                throw new FileReadingException(string.Format("While reading observation points an error occured :{0} {1}", Environment.NewLine, string.Join(Environment.NewLine, innerExceptionMessages)));
            }

            //add to model
            foreach (var locationPropertyValue in locationPropertyValuesList)
            {
                var observationPoint = new ObservationPoint()
                {
                    Name = locationPropertyValue.name,
                    LongName = locationPropertyValue.longName,
                    Chainage = locationPropertyValue.branch.GetBranchSnappedChainage(locationPropertyValue.chainage),
                };
                observationPoint.Geometry = HydroNetworkHelper.GetStructureGeometry(locationPropertyValue.branch, observationPoint.Chainage);
                locationPropertyValue.branch.BranchFeatures.Add(observationPoint);                
            }
        }

        public static void ReadFileLateralDischargeLocations(string filename, IHydroNetwork network)
        {
            if (!File.Exists(filename)) throw new FileReadingException(string.Format(Resources.Could_not_read_file_0_properly_it_doesnt_exist, filename));
            var categories = new DelftIniReader().ReadDelftIniFile(filename);
            if (categories.Count == 0) throw new FileReadingException(string.Format(Resources.Could_not_read_file_0_properly_it_seems_empty, filename));

            IList<FileReadingException> fileReadingExceptions = new List<FileReadingException>();

            IList<LocationPropertyValues> locationPropertyValuesList = new List<LocationPropertyValues>();
            foreach (var lateralDischargeCategory in categories.Where(category => category.Name == BoundaryRegion.LateralDischargeHeader))
            {
                try
                {
                    var locationPropertyValues = GetCommonLocationPropertyValues(lateralDischargeCategory, network);
                    if (locationPropertyValues == null) continue;
                    locationPropertyValuesList.Add(locationPropertyValues);
                }
                catch (FileReadingException fileReadingException)
                {
                    fileReadingExceptions.Add(new FileReadingException("Could not read lateral discharge location", fileReadingException));
                }
            }
            if (fileReadingExceptions.Count > 0)
            {
                var innerExceptionMessages = fileReadingExceptions.Select(fileReadingException => fileReadingException.InnerException.Message + Environment.NewLine);
                throw new FileReadingException(string.Format("While reading lateral discharge locations an error occured :{0} {1}", Environment.NewLine, string.Join(Environment.NewLine, innerExceptionMessages)));
            }

            // Restore LateralSources to model
            foreach (var locationPropertyValue in locationPropertyValuesList)
            {
                var lateralSource = new LateralSource()
                {
                    Branch = locationPropertyValue.branch,
                    Name = locationPropertyValue.name,
                    LongName = locationPropertyValue.longName,
                    Chainage = locationPropertyValue.branch.GetBranchSnappedChainage(locationPropertyValue.chainage)
                };
                if (locationPropertyValue.diffuseLength.HasValue)
                    lateralSource.Length = locationPropertyValue.diffuseLength.Value;
                locationPropertyValue.branch.BranchFeatures.Add(lateralSource);
            }
        }
   
        private static LocationPropertyValues GetCommonLocationPropertyValues(IDelftIniCategory category, IHydroNetwork network)
        {
            var locationPropertyValues = new LocationPropertyValues();
            if (category.GetPropertyValue(LocationRegion.Chainage.Key) == null ||
                category.GetPropertyValue(LocationRegion.BranchId.Key) == null) return null; //2d obs point do not add as a 1d obs point
            
            // Essential Properties (an error will be generated if these fail)
            locationPropertyValues.name =
                string.IsNullOrEmpty(category.ReadProperty<string>(LocationRegion.Id.Key, true))
                    ? category.ReadProperty<string>(LocationRegion.ObsId.Key)
                    : category.ReadProperty<string>(LocationRegion.Id.Key);

            
            var branchName = category.ReadProperty<string>(LocationRegion.BranchId.Key);
            locationPropertyValues.branch = network.Channels.FirstOrDefault(c => c.Name == branchName);
            if (locationPropertyValues.branch == null)
            {
                var errorMessage = string.Format("Unable to parse {0} property: {1}, Branch not found in Network.{2}", category.Name, LocationRegion.BranchId.Key, Environment.NewLine);
                throw new FileReadingException(errorMessage);
            }

            locationPropertyValues.chainage = locationPropertyValues.branch.GetBranchSnappedChainage(category.ReadProperty<double>(LocationRegion.Chainage.Key));
            
            // Optional Properties (an error will not be generated if these fail) 
            locationPropertyValues.longName = category.ReadProperty<string>(LocationRegion.Name.Key, true) ?? string.Empty;
            locationPropertyValues.diffuseLength = category.ReadProperty<double?>(LateralSourceLocationRegion.Length.Key, true); 

            return locationPropertyValues;
        }
        
    }
}

