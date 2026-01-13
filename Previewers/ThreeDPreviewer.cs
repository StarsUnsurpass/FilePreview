using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace FilePreview.Previewers;

public class ThreeDPreviewer : IPreviewer
{
    private static readonly string[] Extensions = { ".stl", ".obj", ".3ds", ".ply" };

    public bool CanPreview(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLower();
        return System.Linq.Enumerable.Contains(Extensions, ext);
    }

    public FrameworkElement CreateControl(string filePath)
    {
        var viewport = new HelixViewport3D
        {
            ZoomExtentsWhenLoaded = true,
            ShowCoordinateSystem = true,
            ShowViewCube = true,
            Background = System.Windows.Media.Brushes.Transparent
        };

        var lights = new DefaultLights();
        viewport.Children.Add(lights);

        System.Threading.Tasks.Task.Run(() =>
        {
            try
            {
                var importer = new ModelImporter();
                // Load model on background thread
                var modelGroup = importer.Load(filePath);
                
                if (modelGroup != null)
                {
                    // Create visual on UI thread or freeze? 
                    // 3D objects in WPF usually need to be created on UI thread or frozen.
                    // HelixToolkit's Load returns a Model3DGroup which is a DependencyObject.
                    // We can try freezing it.
                    if (modelGroup.CanFreeze)
                    {
                        modelGroup.Freeze();
                    }

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        var modelVisual = new ModelVisual3D { Content = modelGroup };
                        viewport.Children.Add(modelVisual);
                        viewport.ZoomExtents();
                    });
                }
            }
            catch (Exception ex)
            {
                // Handle error
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    // Maybe show an error text overlay?
                    // For now, we rely on the empty viewport or maybe logs
                    Serilog.Log.Error(ex, "Failed to load 3D model.");
                });
            }
        });

        return viewport;
    }
}
