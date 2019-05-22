import sys, os, time, glob
import clr
clr.AddReference("System.Drawing")
clr.AddReference("System.Windows.Forms")
clr.AddReference("System")
from System.Drawing import Point, Color, Size, Image
from System.Windows.Forms import Application, Form, Label, TextBox,CheckBox,   RadioButton, Panel, Button, GroupBox, PictureBox, PictureBoxSizeMode
from System.Windows.Forms import OpenFileDialog, ToolBar, ToolBarButton, DialogResult, MessageBox, MessageBoxButtons, MessageBoxIcon, ScrollBars

class MainForm(Form):
    def __init__(self):
    #Define the layout of the form
        self.Text = "Test Python GUI" # Title of window
        self.Name = "Test Python GUI"
        self.Height = 600
        self.Width = 600

Application.EnableVisualStyles() 
GUI = MainForm()
Application.Run(GUI)