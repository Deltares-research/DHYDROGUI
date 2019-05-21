# create 2d velocity field function f = (vx, vy)(x, y)

x = Variable[float](Name="x")
y = Variable[float](Name="y")
vx = Variable[float](Name="vx")
vy = Variable[float](Name="vy")

f = Function(Name = "velocity field")

f.Arguments.AddRange([x, y])
f.Components.AddRange([vx, vy])

x.Values.AddRange((0, 1, 2))
y.Values.AddRange((0, 1))

vx.SetValues((1.0, 2.0, 3.0, 4.0, 5.0, 6.0))

vx.Values[0, 0] = 1
vy.Values[0, 0] = 2

print "vx:", vx.Values
print "vy:", vy.Values

CurrentProject.RootFolder.Add(f)

Gui.Selection = f
Gui.CommandHandler.OpenView(f)
