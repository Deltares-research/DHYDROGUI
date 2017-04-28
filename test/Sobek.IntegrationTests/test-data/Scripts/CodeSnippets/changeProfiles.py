from DelftTools.Hydro.CrossSections import *

def filter_yz(c): return c.CrossSectionType == CrossSectionType.YZ

def remove_center_coord(csDef):
	rowToRemove = 0
	for row in csDef.RawData:
		if row[0] == 0.0:
			rowToRemove = row
			break
	if rowToRemove != 0:
		csDef.RawData.Rows.Remove(rowToRemove)
	return

im = CurrentProject.RootFolder["integrated model"]
network = im.Region.SubRegions[0]

crossSectionList = filter(filter_yz, list(network.CrossSections))
definitions = [cs.Definition for cs in crossSectionList]

for csDef in definitions:
	remove_center_coord(csDef)