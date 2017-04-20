from API import *
from NetTopologySuite.Extensions.Coverages import NetworkLocation
import itertools

## Read the PEST result file containing the roughness at locations
roughness_locations = []
roughness_values = []
with open(r'FRICTION_FROM_PEST.TXT', 'r') as file:
   file_content = file.readlines()
   for line in file_content:
      a,b = line.split()
      roughness_locations.append(a)
      roughness_values.append(b)

## Read the PEST result file containing the spatial dispersion at locations
dispersion_locations = []
dispersion_values = []
with open(r'DISPERSION_FROM_PEST.TXT', 'r') as file:
   file_content = file.readlines()
   for line in file_content:
      a,b = line.split()
      dispersion_locations.append(a)
      dispersion_values.append(b)

# Open project (when running outside Sobek 3 GUI)
#Application.OpenProject("cottica_testrun.dsproj")
im = CurrentProject.RootFolder["integrated model"]
flow = im.Activities[0]

## Change the existing roughness values within the model
rsArray = flow.RoughnessSections
rs = GetRoughnessSection(flow, "Main")
for l,v in itertools.izip(roughness_locations, roughness_values):
   ChangeRoughnessValuesForSectionAtLocation(rs, v, l)

## Change the existing dispersion values within the model
for dl,dv in izip(dispersion_locations, dispersion_values):
   branch_name, chainage_name = dl.split('_')
   for branch in flow.Network.Branches:
      if branch.Name == branch_name:
         flow.DispersionCoverage[branch, chainage_name] = dv

## Run the adapted model
Application.RunActivity(flow)

## Export results on observation points to CSV
observation_points = [x.Name for x in list(flow.Network.ObservationPoints)]
water_level_sets = []
discharge_sets = []
times = []
for point in observation_points:
    wl = GetWaterLevelResultsAtObservationPoint(flow, point)
    times.append(wl.Arguments[0].Values)
    water_level_sets.append(wl.GetValues())
    dl = GetDischargeResultsAtObservationPoint(flow, point)
    discharge_sets.append(dl.GetValues())

# time, q1, h1, q2, h2, etc
output_file = r"obs_values_to_pest.csv"
with open(output_file, 'w') as file:
    header_string = 'time'
    for i in xrange(len(observation_points)):
        header_string = header_string+','+ observation_points[i]+'.Q,'+observation_points[i]+'.h'
    file.write(header_string+'\n')
    string_list = [time.ToString() for time in times[0]]
    for i in xrange(len(times[0])):
        for j in xrange(len(observation_points)):
            string_list[i] = string_list[i]+','+str(discharge_sets[j][i])+','+str(water_level_sets[j][i])
    for line in string_list:
        file.write(line+'\n')

## Export salt concentration results on observation points to CSV
observation_points = [x.Name for x in list(flow.Network.ObservationPoints)]
salt_concentration_sets = []
times = []
for point in observation_points:
    sc = GetSaltConcentrationResultsAtObservationPoint(flow, point)
    times.append(sc.Arguments[0].Values)
    salt_concentration_sets.append(sc.GetValues())

# time, sc1, sc2, etc
output_file = r"salt_concentration_to_pest.csv"
with open(output_file, 'w') as file:
    header_string = 'time'
    for i in xrange(len(observation_points)):
        header_string = header_string+','+ observation_points[i]+'.sc'
    file.write(header_string+'\n')
    string_list = [time.ToString() for time in times[0]]
    for i in xrange(len(times[0])):
        for j in xrange(len(observation_points)):
            string_list[i] = string_list[i]+','+str(salt_concentration_sets[j][i])
    for line in string_list:
        file.write(line+'\n')

## Save the project
#Application.SaveProjectAs("D:\\temp\\deltaShell\\pest\\model\\cotticaOut.dsproj")
