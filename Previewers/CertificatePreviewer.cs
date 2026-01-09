using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FilePreview.Previewers;

public class CertificatePreviewer : IPreviewer
{
    private static readonly string[] Extensions = { ".cer", ".crt", ".der", ".pem" };

    public bool CanPreview(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLower();
        return Extensions.Contains(ext);
    }

    public FrameworkElement CreateControl(string filePath)
    {
        var stackPanel = new StackPanel { Margin = new Thickness(20) };

        try
        {
            var cert = X509CertificateLoader.LoadCertificateFromFile(filePath);
            
            stackPanel.Children.Add(new TextBlock 
            { 
                Text = "Certificate Information", 
                FontSize = 20, 
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 15)
            });

            AddInfo(stackPanel, "Subject", cert.Subject);
            AddInfo(stackPanel, "Issuer", cert.Issuer);
            AddInfo(stackPanel, "Valid From", cert.NotBefore.ToString());
            AddInfo(stackPanel, "Valid To", cert.NotAfter.ToString());
            AddInfo(stackPanel, "Serial Number", cert.SerialNumber);
            AddInfo(stackPanel, "Thumbprint", cert.Thumbprint);
            AddInfo(stackPanel, "Algorithm", cert.SignatureAlgorithm.FriendlyName ?? "Unknown");
            AddInfo(stackPanel, "Version", cert.Version.ToString());

            var now = DateTime.Now;
            if (now < cert.NotBefore || now > cert.NotAfter)
            {
                stackPanel.Children.Add(new TextBlock 
                { 
                    Text = "⚠️ This certificate is expired or not yet valid.", 
                    Foreground = System.Windows.Media.Brushes.Orange,
                    Margin = new Thickness(0, 10, 0, 0),
                    FontWeight = FontWeights.SemiBold
                });
            }
        }
        catch (Exception ex)
        {
            stackPanel.Children.Add(new TextBlock { Text = $"Error loading certificate: {ex.Message}", Foreground = System.Windows.Media.Brushes.Red });
        }

        return new ScrollViewer { Content = stackPanel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
    }

    private void AddInfo(StackPanel panel, string label, string value)
    {
        var grid = new Grid { Margin = new Thickness(0, 0, 0, 5) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var labelBlock = new TextBlock { Text = label + ":", FontWeight = FontWeights.SemiBold, Foreground = System.Windows.Media.Brushes.Gray };
        var valueBlock = new TextBlock { Text = value ?? string.Empty, TextWrapping = TextWrapping.Wrap };

        Grid.SetColumn(labelBlock, 0);
        Grid.SetColumn(valueBlock, 1);

        grid.Children.Add(labelBlock);
        grid.Children.Add(valueBlock);

        panel.Children.Add(grid);
    }
}
