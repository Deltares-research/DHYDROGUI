from System import *
from NetTopologySuite.Extensions.Coverages import *

# create 2d regular grid 2x3 with deltax = 100, deltay = 50
grid = RegularGridCoverage(2.0, 3.0, 100.0, 50.0)

grid.IsTimeDependent = True

grid[DateTime(2000, 1, 1)] = (1.0, 2.0, 3.0, 4.0, 5.0, 6.0)
grid[DateTime(2000, 2, 1)] = (1.0, 2.0, 3.0, 4.0, 5.0, 60.0)
grid[DateTime(2000, 3, 1)] = (1.0, 2.0, 3.0, 4.0, 50.0, 6.0)

CurrentProject.RootFolder.Add(grid)

Gui.CommandHandler.OpenView(grid)


