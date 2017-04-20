# Open existing flow model within project
flow = CurrentProject.RootFolder["CF-E7-case2"]

# create single time series for plotting for East1 and East2 gridpoints for both modelresults and measurements
Overallts = List[IFunction]()

# Run the model
Application.RunActivity(flow)

# Import the measurement timeseries for East1 and East2
East1measured = ImportCSVTimeSeries("c:\\Users\\putten_hs\\Desktop\\CF-E7.lit\\East1.csv")
East1measured.Name = "Measured water level at East1 [m]"
East2measured = ImportCSVTimeSeries("c:\\Users\\putten_hs\\Desktop\\CF-E7.lit\\East2.csv")
East2measured.Name = "Measured water level at East2 [m]"

# Add model results at grid point "East1" to timeseries
tsEast1 = GetWaterLevelResultsAtCalcPoint(flow, "East1")
tsEast1.Name = "Calculated water level at East1 [m]"
# Add model results at grid point "East2" to timeseries
tsEast2 = GetWaterLevelResultsAtCalcPoint(flow, "East2")
tsEast2.Name = "Calculated water level at East2 [m]"

# Add all timeseries (model results and measurements) to overall timeseries for plotting
Overallts.Add(East1measured) 
Overallts.Add(tsEast1)
Overallts.Add(East2measured)
Overallts.Add(tsEast2)

# Plot the results
Gui.CommandHandler.OpenView(Overallts)

# Beautify Chart
from DelftTools.Controls.Swf.Charting import PointerStyles
from DelftTools.Utils.Drawing import ColorHelper
activeView = Gui.DocumentViews.ActiveView
activeView.ChartView.Chart.Series[0].PointerStyle = PointerStyles.Triangle
activeView.ChartView.Chart.Series[0].Color = ColorHelper.Transparent
activeView.ChartView.Chart.Series[0].PointerVisible = "true"
activeView.ChartView.Chart.Series[0].Title = East1measured.Name

activeView.ChartView.Chart.Series[1].Title = tsEast1.Name

activeView.ChartView.Chart.Series[2].PointerStyle = PointerStyles.Triangle
activeView.ChartView.Chart.Series[2].Color = ColorHelper.Transparent
activeView.ChartView.Chart.Series[2].PointerVisible = "true"
activeView.ChartView.Chart.Series[2].Title = East2measured.Name

activeView.ChartView.Chart.Series[3].Title = tsEast2.Name
