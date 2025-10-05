using System;
using DirectoryAnalyzer.Core;
using Terminal.Gui;

namespace DirectoryAnalyzer.UI;

public class MainMenuUI
{
    private readonly ExcelReportGenerator _reportGenerator;

    public MainMenuUI()
    {
        _reportGenerator = new ExcelReportGenerator();
    }

    public void Run()
    {
        Application.Init();

        try
        {
            ShowMainMenu();
        }
        catch (Exception ex)
        {
            MessageBox.ErrorQuery("Error", $"An unexpected error occurred: {ex.Message}", "OK");
        }
        finally
        {
            Application.Shutdown();
        }
    }

    private void ShowMainMenu()
    {
        var window = new Window("Directory Structure Analyzer")
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };

        var directoryPathLabel = new Label("Directory path:") { X = 1, Y = 1 };

        var directoryPathField = new TextField("")
        {
            X = Pos.Right(directoryPathLabel) + 2,
            Y = 1,
            Width = 50,
        };

        var depthLabel = new Label("Search depth:") { X = 1, Y = 3 };

        var depthField = new TextField("2")
        {
            X = Pos.Right(depthLabel) + 2,
            Y = 3,
            Width = 10,
        };

        var outputFileLabel = new Label("Output filename:") { X = 1, Y = 5 };

        var outputFileField = new TextField("directory_analysis.xlsx")
        {
            X = Pos.Right(outputFileLabel) + 2,
            Y = 5,
            Width = 30,
        };

        var generateButton = new Button("Generate Excel Report") { X = 1, Y = 7 };

        var exitButton = new Button("Exit") { X = Pos.Right(generateButton) + 5, Y = 7 };

        generateButton.Clicked += () =>
        {
            HandleGenerateReport(directoryPathField, depthField, outputFileField);
        };

        exitButton.Clicked += () =>
        {
            Application.RequestStop();
        };

        window.Add(
            directoryPathLabel,
            directoryPathField,
            depthLabel,
            depthField,
            outputFileLabel,
            outputFileField,
            generateButton,
            exitButton
        );

        Application.Top.Add(window);
        Application.Run();
    }

    private void HandleGenerateReport(
        TextField directoryPathField,
        TextField depthField,
        TextField outputFileField
    )
    {
        var directoryPath = directoryPathField.Text.ToString();
        var outputFile = outputFileField.Text.ToString();

        if (!int.TryParse(depthField.Text.ToString(), out int depth))
        {
            MessageBox.ErrorQuery("Error", "Invalid search depth. Please enter an integer.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            MessageBox.ErrorQuery("Error", "Please enter a directory path.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(outputFile))
        {
            MessageBox.ErrorQuery("Error", "Please enter an output filename.", "OK");
            return;
        }

        try
        {
            _reportGenerator.GenerateReport(directoryPath, depth, outputFile);
            MessageBox.Query("Success", $"Report has been generated: {outputFile}", "OK");
        }
        catch (Exception ex)
        {
            MessageBox.ErrorQuery("Error", $"Failed to generate report: {ex.Message}", "OK");
        }
    }
}
