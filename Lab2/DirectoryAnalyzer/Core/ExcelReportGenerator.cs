using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using DirectoryAnalyzer.Models;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;

namespace DirectoryAnalyzer.Core;

public class ExcelReportGenerator
{
    private readonly List<string> _warnings;

    public ExcelReportGenerator()
    {
        ExcelPackage.License.SetNonCommercialPersonal("Politechnika");
        _warnings = new List<string>();
    }

    public IReadOnlyList<string> Warnings => _warnings.AsReadOnly();

    private void AddWarning(string message)
    {
        _warnings.Add(message);
    }

    public void ClearWarnings()
    {
        _warnings.Clear();
    }

    public void GenerateReport(string directoryPath, int maxDepth, string outputFileName)
    {
        // Validate directory path exists
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new ArgumentException("Directory path cannot be empty or whitespace.", nameof(directoryPath));
        }

        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"The specified directory does not exist: {directoryPath}");
        }

        // Check if we have access to the directory
        try
        {
            var testAccess = Directory.GetFiles(directoryPath);
        }
        catch (UnauthorizedAccessException)
        {
            throw new UnauthorizedAccessException($"Access denied to directory: {directoryPath}");
        }
        catch (SecurityException)
        {
            throw new SecurityException($"Security error while accessing directory: {directoryPath}");
        }

        // Validate output filename
        if (string.IsNullOrWhiteSpace(outputFileName))
        {
            throw new ArgumentException("Output filename cannot be empty or whitespace.", nameof(outputFileName));
        }

        try
        {
            using var package = new ExcelPackage();

            var structureWorksheet = package.Workbook.Worksheets.Add("Directory Structure");
            CreateDirectoryStructureSheet(structureWorksheet, directoryPath, maxDepth);

            var statisticsWorksheet = package.Workbook.Worksheets.Add("Statistics");
            CreateStatisticsSheet(statisticsWorksheet, directoryPath, maxDepth);

            var fileInfo = new FileInfo(outputFileName);
            
            // Check if output directory exists and is writable
            var outputDirectory = Path.GetDirectoryName(outputFileName);
            if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
            {
                throw new DirectoryNotFoundException($"Output directory does not exist: {outputDirectory}");
            }

            package.SaveAs(fileInfo);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new UnauthorizedAccessException($"Access denied when creating output file: {outputFileName}", ex);
        }
        catch (IOException ex)
        {
            throw new IOException($"IO error while creating report file '{outputFileName}': {ex.Message}", ex);
        }
        catch (SecurityException ex)
        {
            throw new SecurityException($"Security error while creating report file: {outputFileName}", ex);
        }
    }

    private void CreateDirectoryStructureSheet(
        ExcelWorksheet worksheet,
        string rootPath,
        int maxDepth
    )
    {
        worksheet.Cells[1, 1].Value = "Name";
        worksheet.Cells[1, 2].Value = "Path";
        worksheet.Cells[1, 3].Value = "Type";
        worksheet.Cells[1, 4].Value = "Extension";
        worksheet.Cells[1, 5].Value = "Size (bytes)";
        worksheet.Cells[1, 6].Value = "Attributes";
        worksheet.Cells[1, 7].Value = "Modified Date";

        using (var range = worksheet.Cells[1, 1, 1, 7])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        var allItems = new List<FileSystemItem>();
        var row = 2;

        ProcessDirectory(rootPath, 0, maxDepth, allItems, worksheet, ref row);

        if (worksheet.Dimension != null)
        {
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }
    }

    private void ProcessDirectory(
        string directoryPath,
        int currentDepth,
        int maxDepth,
        List<FileSystemItem> allItems,
        ExcelWorksheet worksheet,
        ref int row
    )
    {
        if (currentDepth > maxDepth)
            return;

        // Check if directory exists before processing
        if (!Directory.Exists(directoryPath))
        {
            AddWarning($"Warning: Directory not found: {directoryPath}");
            return;
        }

        DirectoryInfo directoryInfo;
        try
        {
            directoryInfo = new DirectoryInfo(directoryPath);
        }
        catch (ArgumentException ex)
        {
            AddWarning($"Warning: Invalid directory path '{directoryPath}': {ex.Message}");
            return;
        }
        catch (SecurityException ex)
        {
            AddWarning($"Warning: Security error accessing directory '{directoryPath}': {ex.Message}");
            return;
        }

        var groupStartRow = row;

        var indent = new string(' ', currentDepth * 2);
        worksheet.Cells[row, 1].Value = indent + directoryInfo.Name;
        worksheet.Cells[row, 2].Value = directoryInfo.FullName;
        worksheet.Cells[row, 3].Value = "Directory";
        worksheet.Cells[row, 4].Value = "";
        worksheet.Cells[row, 5].Value = "";
        worksheet.Cells[row, 6].Value = directoryInfo.Attributes.ToString();
        worksheet.Cells[row, 7].Value = directoryInfo.LastWriteTime;
        row++;

        var itemsInDirectory = new List<FileSystemItem>();

        // Try to get files from directory
        FileInfo[] files;
        try
        {
            files = directoryInfo.GetFiles();
        }
        catch (UnauthorizedAccessException)
        {
            AddWarning($"Warning: Access denied to directory: {directoryInfo.FullName}");
            var fileIndent = new string(' ', (currentDepth + 1) * 2);
            worksheet.Cells[row, 1].Value = fileIndent + "[Access Denied]";
            worksheet.Cells[row, 2].Value = directoryInfo.FullName;
            worksheet.Cells[row, 3].Value = "Error";
            row++;
            return;
        }
        catch (DirectoryNotFoundException)
        {
            AddWarning($"Warning: Directory not found: {directoryInfo.FullName}");
            return;
        }
        catch (IOException ex)
        {
            AddWarning($"Warning: IO error accessing directory '{directoryInfo.FullName}': {ex.Message}");
            return;
        }

        foreach (var file in files)
        {
            try
            {
                // Additional check for file accessibility
                if (!File.Exists(file.FullName))
                {
                    AddWarning($"Warning: File not found during processing: {file.FullName}");
                    continue;
                }

                var fileItem = new FileSystemItem
                {
                    Name = file.Name,
                    FullPath = file.FullName,
                    IsDirectory = false,
                    Extension = file.Extension,
                    Size = file.Length,
                    Attributes = file.Attributes.ToString(),
                    LastModified = file.LastWriteTime,
                    Depth = currentDepth + 1,
                };

                itemsInDirectory.Add(fileItem);
                allItems.Add(fileItem);

                var fileIndent = new string(' ', (currentDepth + 1) * 2);
                worksheet.Cells[row, 1].Value = fileIndent + file.Name;
                worksheet.Cells[row, 2].Value = file.FullName;
                worksheet.Cells[row, 3].Value = "File";
                worksheet.Cells[row, 4].Value = file.Extension;
                worksheet.Cells[row, 5].Value = file.Length;
                worksheet.Cells[row, 6].Value = file.Attributes.ToString();
                worksheet.Cells[row, 7].Value = file.LastWriteTime;
                row++;
            }
            catch (UnauthorizedAccessException)
            {
                AddWarning($"Warning: Access denied to file: {file.FullName}");
                continue;
            }
            catch (FileNotFoundException)
            {
                AddWarning($"Warning: File not found: {file.FullName}");
                continue;
            }
            catch (IOException ex)
            {
                AddWarning($"Warning: IO error accessing file '{file.FullName}': {ex.Message}");
                continue;
            }
            catch (SecurityException ex)
            {
                AddWarning($"Warning: Security error accessing file '{file.FullName}': {ex.Message}");
                continue;
            }
        }

        // Try to get subdirectories
        DirectoryInfo[] subdirectories;
        try
        {
            subdirectories = directoryInfo.GetDirectories();
        }
        catch (UnauthorizedAccessException)
        {
            AddWarning($"Warning: Access denied to subdirectories of: {directoryInfo.FullName}");
            return;
        }
        catch (DirectoryNotFoundException)
        {
            AddWarning($"Warning: Directory not found: {directoryInfo.FullName}");
            return;
        }
        catch (IOException ex)
        {
            AddWarning($"Warning: IO error accessing subdirectories of '{directoryInfo.FullName}': {ex.Message}");
            return;
        }

        foreach (var subdirectory in subdirectories)
        {
            try
            {
                ProcessDirectory(
                    subdirectory.FullName,
                    currentDepth + 1,
                    maxDepth,
                    allItems,
                    worksheet,
                    ref row
                );
            }
            catch (UnauthorizedAccessException)
            {
                AddWarning($"Warning: Access denied to subdirectory: {subdirectory.FullName}");
                continue;
            }
            catch (DirectoryNotFoundException)
            {
                AddWarning($"Warning: Subdirectory not found: {subdirectory.FullName}");
                continue;
            }
            catch (IOException ex)
            {
                AddWarning($"Warning: IO error processing subdirectory '{subdirectory.FullName}': {ex.Message}");
                continue;
            }
            catch (SecurityException ex)
            {
                AddWarning($"Warning: Security error processing subdirectory '{subdirectory.FullName}': {ex.Message}");
                continue;
            }
        }

        if (row > groupStartRow + 1)
        {
            worksheet.Row(groupStartRow).OutlineLevel = currentDepth;
            for (int i = groupStartRow + 1; i < row; i++)
            {
                worksheet.Row(i).OutlineLevel = currentDepth + 1;
            }
        }
    }

    private void CreateStatisticsSheet(ExcelWorksheet worksheet, string rootPath, int maxDepth)
    {
        var allFiles = new List<FileSystemItem>();
        CollectAllFiles(rootPath, 0, maxDepth, allFiles);

        worksheet.Cells[1, 1].Value = "Top 10 Largest Files";
        worksheet.Cells[1, 1].Style.Font.Bold = true;
        worksheet.Cells[1, 1].Style.Font.Size = 14;

        worksheet.Cells[3, 1].Value = "No.";
        worksheet.Cells[3, 2].Value = "Filename";
        worksheet.Cells[3, 3].Value = "Size (bytes)";
        worksheet.Cells[3, 4].Value = "Path";

        using (var range = worksheet.Cells[3, 1, 3, 4])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
        }

        var topFiles = allFiles
            .Where(f => !f.IsDirectory)
            .OrderByDescending(f => f.Size)
            .Take(10)
            .ToList();

        for (int i = 0; i < topFiles.Count; i++)
        {
            var file = topFiles[i];
            worksheet.Cells[4 + i, 1].Value = i + 1;
            worksheet.Cells[4 + i, 2].Value = file.Name;
            worksheet.Cells[4 + i, 3].Value = file.Size;
            worksheet.Cells[4 + i, 4].Value = file.FullPath;
        }

        var extensionStats = allFiles
            .Where(f => !f.IsDirectory)
            .GroupBy(f => string.IsNullOrEmpty(f.Extension) ? "No extension" : f.Extension)
            .Select(g => new
            {
                Extension = g.Key,
                Count = g.Count(),
                TotalSize = g.Sum(f => f.Size),
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        int statsStartRow = Math.Max(15, topFiles.Count + 6);
        worksheet.Cells[statsStartRow, 1].Value = "Extension Statistics";
        worksheet.Cells[statsStartRow, 1].Style.Font.Bold = true;
        worksheet.Cells[statsStartRow, 1].Style.Font.Size = 14;

        worksheet.Cells[statsStartRow + 2, 1].Value = "Extension";
        worksheet.Cells[statsStartRow + 2, 2].Value = "File Count";
        worksheet.Cells[statsStartRow + 2, 3].Value = "Total Size (bytes)";

        using (var range = worksheet.Cells[statsStartRow + 2, 1, statsStartRow + 2, 3])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
        }

        for (int i = 0; i < extensionStats.Count; i++)
        {
            var stat = extensionStats[i];
            worksheet.Cells[statsStartRow + 3 + i, 1].Value = stat.Extension;
            worksheet.Cells[statsStartRow + 3 + i, 2].Value = stat.Count;
            worksheet.Cells[statsStartRow + 3 + i, 3].Value = stat.TotalSize;
        }

        if (extensionStats.Any())
        {
            var chartCount =
                worksheet.Drawings.AddChart("ChartCount", eChartType.Pie) as ExcelPieChart;
            if (chartCount != null)
            {
                chartCount.Title.Text = "File Distribution by Extension (Count)";
                chartCount.SetPosition(1, 0, 6, 0);
                chartCount.SetSize(400, 300);

                var countDataRange = worksheet.Cells[
                    statsStartRow + 3,
                    1,
                    statsStartRow + 2 + extensionStats.Count,
                    2
                ];
                chartCount.Series.Add(countDataRange.Offset(0, 1), countDataRange.Offset(0, 0));
                chartCount.Legend.Position = eLegendPosition.Right;
            }

            var chartSize =
                worksheet.Drawings.AddChart("ChartSize", eChartType.Pie) as ExcelPieChart;
            if (chartSize != null)
            {
                chartSize.Title.Text = "File Distribution by Extension (Size)";
                chartSize.SetPosition(20, 0, 6, 0);
                chartSize.SetSize(400, 300);

                var sizeDataRange = worksheet.Cells[
                    statsStartRow + 3,
                    1,
                    statsStartRow + 2 + extensionStats.Count,
                    3
                ];
                chartSize.Series.Add(sizeDataRange.Offset(0, 2), sizeDataRange.Offset(0, 0));
                chartSize.Legend.Position = eLegendPosition.Right;
            }
        }

        if (worksheet.Dimension != null)
        {
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }
    }

    private void CollectAllFiles(
        string directoryPath,
        int currentDepth,
        int maxDepth,
        List<FileSystemItem> allFiles
    )
    {
        if (currentDepth > maxDepth)
            return;

        // Check if directory exists
        if (!Directory.Exists(directoryPath))
        {
            AddWarning($"Warning: Directory not found: {directoryPath}");
            return;
        }

        DirectoryInfo directoryInfo;
        try
        {
            directoryInfo = new DirectoryInfo(directoryPath);
        }
        catch (ArgumentException ex)
        {
            AddWarning($"Warning: Invalid directory path '{directoryPath}': {ex.Message}");
            return;
        }
        catch (SecurityException ex)
        {
            AddWarning($"Warning: Security error accessing directory '{directoryPath}': {ex.Message}");
            return;
        }

        // Try to get files
        FileInfo[] files;
        try
        {
            files = directoryInfo.GetFiles();
        }
        catch (UnauthorizedAccessException)
        {
            AddWarning($"Warning: Access denied to directory: {directoryInfo.FullName}");
            return;
        }
        catch (DirectoryNotFoundException)
        {
            AddWarning($"Warning: Directory not found: {directoryInfo.FullName}");
            return;
        }
        catch (IOException ex)
        {
            AddWarning($"Warning: IO error accessing directory '{directoryInfo.FullName}': {ex.Message}");
            return;
        }

        foreach (var file in files)
        {
            try
            {
                // Additional check for file accessibility
                if (!File.Exists(file.FullName))
                {
                    AddWarning($"Warning: File not found during processing: {file.FullName}");
                    continue;
                }

                allFiles.Add(
                    new FileSystemItem
                    {
                        Name = file.Name,
                        FullPath = file.FullName,
                        IsDirectory = false,
                        Extension = file.Extension,
                        Size = file.Length,
                        Attributes = file.Attributes.ToString(),
                        LastModified = file.LastWriteTime,
                        Depth = currentDepth,
                    }
                );
            }
            catch (UnauthorizedAccessException)
            {
                AddWarning($"Warning: Access denied to file: {file.FullName}");
                continue;
            }
            catch (FileNotFoundException)
            {
                AddWarning($"Warning: File not found: {file.FullName}");
                continue;
            }
            catch (IOException ex)
            {
                AddWarning($"Warning: IO error accessing file '{file.FullName}': {ex.Message}");
                continue;
            }
            catch (SecurityException ex)
            {
                AddWarning($"Warning: Security error accessing file '{file.FullName}': {ex.Message}");
                continue;
            }
        }

        // Try to get subdirectories
        DirectoryInfo[] subdirectories;
        try
        {
            subdirectories = directoryInfo.GetDirectories();
        }
        catch (UnauthorizedAccessException)
        {
            AddWarning($"Warning: Access denied to subdirectories of: {directoryInfo.FullName}");
            return;
        }
        catch (DirectoryNotFoundException)
        {
            AddWarning($"Warning: Directory not found: {directoryInfo.FullName}");
            return;
        }
        catch (IOException ex)
        {
            AddWarning($"Warning: IO error accessing subdirectories of '{directoryInfo.FullName}': {ex.Message}");
            return;
        }

        foreach (var subdirectory in subdirectories)
        {
            try
            {
                CollectAllFiles(subdirectory.FullName, currentDepth + 1, maxDepth, allFiles);
            }
            catch (UnauthorizedAccessException)
            {
                AddWarning($"Warning: Access denied to subdirectory: {subdirectory.FullName}");
                continue;
            }
            catch (DirectoryNotFoundException)
            {
                AddWarning($"Warning: Subdirectory not found: {subdirectory.FullName}");
                continue;
            }
            catch (IOException ex)
            {
                AddWarning($"Warning: IO error processing subdirectory '{subdirectory.FullName}': {ex.Message}");
                continue;
            }
            catch (SecurityException ex)
            {
                AddWarning($"Warning: Security error processing subdirectory '{subdirectory.FullName}': {ex.Message}");
                continue;
            }
        }
    }
}
