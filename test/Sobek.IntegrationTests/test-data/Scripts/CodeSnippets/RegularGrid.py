from NetTopologySuite.Extensions.Coverages import *

# create 2d regular grid 2x3 with deltax = 100, deltay = 50
grid = RegularGridCoverage(2.0, 3.0, 100.0, 50.0)

grid.SetValues((1.0, 2.0, 3.0, 4.0, 5.0, 6.0))

CurrentProject.RootFolder.Add(grid)

Gui.CommandHandler.OpenView(grid)


