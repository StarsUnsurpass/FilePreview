using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace FilePreview.Previewers;

public class CsvPreviewer : IPreviewer
{
    public bool CanPreview(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLower();
        return ext == ".csv" || ext == ".tsv";
    }

    public FrameworkElement CreateControl(string filePath)
    {
        var grid = new Grid();
        var statusText = new TextBlock { Text = "Loading data...", HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = System.Windows.VerticalAlignment.Center };
        grid.Children.Add(statusText);

        System.Threading.Tasks.Task.Run(() =>
        {
            try
            {
                var dataTable = new DataTable();
                var lines = new System.Collections.Generic.List<string>();
                
                // Read only first 1000 lines for preview
                using (var reader = new StreamReader(filePath))
                {
                    int count = 0;
                    string? line;
                    while ((line = reader.ReadLine()) != null && count < 1000)
                    {
                        lines.Add(line);
                        count++;
                    }
                }

                if (lines.Count > 0)
                {
                    char delimiter = Path.GetExtension(filePath).ToLower() == ".tsv" ? '\t' : ',';
                    
                    var headers = lines[0].Split(delimiter);
                    foreach (var header in headers)
                    {
                        var columnName = header.Trim();
                        // Handle empty or duplicate column names
                        if (string.IsNullOrEmpty(columnName)) columnName = "Column";
                        int count = 1;
                        string originalName = columnName;
                        while(dataTable.Columns.Contains(columnName))
                        {
                            columnName = $"{originalName}{count++}";
                        }
                        dataTable.Columns.Add(columnName);
                    }

                    for (int i = 1; i < lines.Count; i++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[i])) continue;
                        
                        var row = dataTable.NewRow();
                        var fields = lines[i].Split(delimiter);
                        
                        for (int j = 0; j < headers.Length && j < fields.Length; j++)
                        {
                            row[j] = fields[j].Trim();
                        }
                        
                        dataTable.Rows.Add(row);
                    }
                }

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    grid.Children.Clear();
                    var dataGrid = new System.Windows.Controls.DataGrid
                    {
                        AutoGenerateColumns = true,
                        IsReadOnly = true,
                        GridLinesVisibility = DataGridGridLinesVisibility.All,
                        AlternatingRowBackground = System.Windows.Media.Brushes.LightGray,
                        ItemsSource = dataTable.DefaultView
                    };
                    grid.Children.Add(dataGrid);
                });
            }
            catch (Exception ex)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    statusText.Text = $"Error parsing file: {ex.Message}";
                    statusText.Foreground = System.Windows.Media.Brushes.Red;
                });
            }
        });

        return grid;
    }
}
