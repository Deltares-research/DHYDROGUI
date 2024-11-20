# import our dependencies
import clr
clr.AddReference("System.Windows.Forms")
from System import *
from System.Collections.Generic import *
from DelftTools.Controls.Swf import CustomInputDialog
from System.Windows.Forms import DialogResult
from System.Windows.Forms import MessageBox

# create a custom input dialog (initially empty except for OK and Cancel button)
dialog = CustomInputDialog()

# add an input of type string, with label 'Sediment name'
dialog.AddInput[String]('Sediment name')

# add an input of type boolean, and directly specify a tooltip
dialog.AddInput[Boolean]('Is mud').ToolTip = "The name of the sediment"

# if we want to specify multiple options, first assign to a variable (here: d50input)
d50input = dialog.AddInput[Double]('D50')
# add a tooltip:
d50input.ToolTip = "Specify the D50 grain size for this sediment"
# assign validation logic, horrible syntax unfortunately (empty string = no error):
d50input.ValidationMethod = Func[object, object, String] ( lambda o,value : 'Value must be positive' if value < 0 else '' )

# add several other variables
dialog.AddInput[Boolean]('Is sand')
dialog.AddInput[Boolean]('Is silt')
dialog.AddInput[Boolean]('Is rock')

dialog.AddChoice('Sediment type', List[String]({'Sand','Silt','Rock'}))

# show dialog and wait for the user to click OK
if dialog.ShowDialog() == DialogResult.OK:

	# retrieve values as filled in by user (using label name)
	grainSize = dialog['D50']
	sedimentName = dialog['Sediment name']
	sedimentType = dialog['Sediment type']
	
	# show in message box for confirmation
	MessageBox.Show('User supplied ' + str(grainSize) + ' as value for D50 grain size for sediment ' + sedimentName + ', with type ' + sedimentType)
