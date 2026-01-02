using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace FilePreview.Previewers;

public class CsvPreviewer : IPreviewer
{
    public bool CanPreview(string filePath)
    {
        return Path.GetExtension(filePath).ToLower() == ".csv";
    }

    public FrameworkElement CreateControl(string filePath)
    {
        var dataGrid = new System.Windows.Controls.DataGrid
        {
            AutoGenerateColumns = true,
            IsReadOnly = true,
            GridLinesVisibility = DataGridGridLinesVisibility.All,
            AlternatingRowBackground = System.Windows.Media.Brushes.LightGray
        };

        try 
        {
            var dataTable = new DataTable();
            var lines = File.ReadAllLines(filePath);
            
            if (lines.Length > 0)
            {
                // Simple parser - assumes no commas in values
                var headers = lines[0].Split(',');
                foreach (var header in headers)
                {
                    // Ensure unique column names
                    var columnName = header.Trim();
                    int count = 1;
                    while(dataTable.Columns.Contains(columnName))
                    {
                        columnName = $"{header.Trim()}{count++}";
                    }
                    dataTable.Columns.Add(columnName);
                }

                for (int i = 1; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;
                    
                    var row = dataTable.NewRow();
                    var fields = lines[i].Split(',');
                    
                    for (int j = 0; j < headers.Length && j < fields.Length; j++)
                    {
                        row[j] = fields[j].Trim();
                    }
                    
                    dataTable.Rows.Add(row);
                }
            }
            
            dataGrid.ItemsSource = dataTable.DefaultView;
        }
        catch
        {
            return new TextBlock { Text = "Error parsing CSV", VerticalAlignment = System.Windows.VerticalAlignment.Center, HorizontalAlignment = System.Windows.HorizontalAlignment.Center };
        }

        return dataGrid;
    }
}
