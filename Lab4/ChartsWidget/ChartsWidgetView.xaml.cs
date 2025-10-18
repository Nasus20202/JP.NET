using System.Globalization;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Lab4.ChartsWidget;

public partial class ChartsWidgetView : UserControl
{
    public ChartsWidgetView()
    {
        InitializeComponent();
    }

    public void UpdateView(string data)
    {
        Dispatcher.Invoke(() =>
        {
            ChartCanvas.Children.Clear();

            var numbers = ParseNumbers(data);

            if (numbers.Count == 0)
            {
                ErrorTextBlock.Visibility = System.Windows.Visibility.Visible;
                ErrorTextBlock.Text =
                    "Cannot parse the numbers. Input space separated numbers (e.g. 10 50 30 80)";
                return;
            }

            ErrorTextBlock.Visibility = System.Windows.Visibility.Collapsed;

            DrawChart(numbers);
        });
    }

    private List<double> ParseNumbers(string text)
    {
        var numbers = new List<double>();
        var tokens = text.Split(
            new[] { ' ', '\t', '\n', '\r', ',', ';' },
            StringSplitOptions.RemoveEmptyEntries
        );

        foreach (var token in tokens)
        {
            if (
                double.TryParse(
                    token,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out double number
                )
            )
            {
                numbers.Add(number);
            }
            else if (
                double.TryParse(token, NumberStyles.Any, CultureInfo.CurrentCulture, out number)
            )
            {
                numbers.Add(number);
            }
        }

        return numbers;
    }

    private void DrawChart(List<double> numbers)
    {
        if (numbers.Count == 0)
            return;

        const double margin = 40;
        const double barWidth = 50;
        const double spacing = 20;

        double maxValue = numbers.Max();
        if (maxValue == 0)
            maxValue = 1;

        double availableHeight =
            ChartCanvas.ActualHeight > 0 ? ChartCanvas.ActualHeight - 2 * margin : 300 - 2 * margin;
        double totalWidth = numbers.Count * (barWidth + spacing) + 2 * margin;

        ChartCanvas.Width = totalWidth;
        ChartCanvas.Height = ChartCanvas.ActualHeight > 0 ? ChartCanvas.ActualHeight : 300;

        var xAxis = new Line
        {
            X1 = margin,
            Y1 = ChartCanvas.Height - margin,
            X2 = totalWidth - margin,
            Y2 = ChartCanvas.Height - margin,
            Stroke = Brushes.Black,
            StrokeThickness = 2,
        };
        ChartCanvas.Children.Add(xAxis);

        var yAxis = new Line
        {
            X1 = margin,
            Y1 = margin,
            X2 = margin,
            Y2 = ChartCanvas.Height - margin,
            Stroke = Brushes.Black,
            StrokeThickness = 2,
        };
        ChartCanvas.Children.Add(yAxis);

        var colors = new[]
        {
            Brushes.CornflowerBlue,
            Brushes.Coral,
            Brushes.MediumSeaGreen,
            Brushes.Orchid,
            Brushes.Gold,
        };

        for (int i = 0; i < numbers.Count; i++)
        {
            double barHeight = (numbers[i] / maxValue) * availableHeight;
            double x = margin + i * (barWidth + spacing) + spacing;
            double y = ChartCanvas.Height - margin - barHeight;

            var rectangle = new Rectangle
            {
                Width = barWidth,
                Height = barHeight,
                Fill = colors[i % colors.Length],
                Stroke = Brushes.Black,
                StrokeThickness = 1,
            };
            Canvas.SetLeft(rectangle, x);
            Canvas.SetTop(rectangle, y);
            ChartCanvas.Children.Add(rectangle);

            var valueText = new TextBlock
            {
                Text = numbers[i].ToString("F1"),
                FontSize = 12,
                FontWeight = System.Windows.FontWeights.Bold,
                Foreground = Brushes.Black,
            };
            Canvas.SetLeft(valueText, x + barWidth / 2 - 15);
            Canvas.SetTop(valueText, y - 20);
            ChartCanvas.Children.Add(valueText);
            var label = new TextBlock
            {
                Text = (i + 1).ToString(),
                FontSize = 12,
                Foreground = Brushes.Black,
            };
            Canvas.SetLeft(label, x + barWidth / 2 - 5);
            Canvas.SetTop(label, ChartCanvas.Height - margin + 5);
            ChartCanvas.Children.Add(label);
        }
    }
}
