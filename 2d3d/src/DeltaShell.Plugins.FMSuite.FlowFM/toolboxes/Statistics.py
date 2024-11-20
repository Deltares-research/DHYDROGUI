import clr
clr.AddReference("System.Windows.Forms")
from System.Windows.Forms import *

s  = 'Model: ' + Model.Name + '\r\n'
s += '\r\n'
s += 'Directory: ' + Model.WorkingDirectory + '\r\n'
s += '\r\n'

s += 'Selected feature:\r\n'

if MapControl.SelectedFeatures.Count > 0:
	s += MapControl.SelectedFeatures[0].Name
	s += '\r\n'
	s += MapControl.SelectedFeatures[0].Geometry.ToString()
else:
	s += '<none>'
s += '\r\n\r\n'

if Model.CoordinateSystem:
	s += 'Coordinate System: ' + Model.CoordinateSystem.Name + '\r\n'

s += '\r\n'
s += 'Vertices:\t\t'       + str(Model.Grid.Vertices.Count) + '\r\n'
s += 'Edges:\t\t'          + str(Model.Grid.Edges.Count) + '\r\n'
s += 'Cells:\t\t'          + str(Model.Grid.Cells.Count) + '\r\n'
s += '\r\n'
s += 'Obs points:\t'       + str(Model.Area.ObservationPoints.Count) + '\r\n'
s += 'Obs crossections:\t' + str(Model.Area.ObservationCrossSections.Count) + '\r\n'
s += 'Thindams:\t'         + str(Model.Area.ThinDams.Count) + '\r\n'
s += 'Pumps:\t\t'          + str(Model.Area.Pumps.Count) + '\r\n'
s += 'Weirs:\t\t'          + str(Model.Area.Weirs.Count) + '\r\n'
s += 'Boundaries:\t'       + str(Model.Boundaries.Count) + '\r\n'

MessageBox.Show(s, 'Statistics', MessageBoxButtons.OK, MessageBoxIcon.Information)
