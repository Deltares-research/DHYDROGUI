using System.Drawing;
using System.Windows.Forms;
using DeltaShell.Plugins.DeveloperTools.VisualStudioExtensions.DebuggerVisualizers;
using Microsoft.VisualStudio.DebuggerVisualizers;

[assembly: System.Diagnostics.DebuggerVisualizer(typeof(ImageVisualizer), typeof(VisualizerObjectSource), Target = typeof(Image), Description = "Image Visualizer")]

namespace DeltaShell.Plugins.DeveloperTools.VisualStudioExtensions.DebuggerVisualizers
{
    public class ImageVisualizer: DialogDebuggerVisualizer
    {
        override protected void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            var image = (Image)objectProvider.GetObject();
            
            if (null == image)
            {
                return;
            }

            var form = new Form { Width = image.Width, Height = image.Height };

            var pictureBox = new PictureBox { Dock = DockStyle.Fill, Image = image };

            form.Controls.Add(pictureBox);

            windowService.ShowDialog(form);
        }
    }
}
