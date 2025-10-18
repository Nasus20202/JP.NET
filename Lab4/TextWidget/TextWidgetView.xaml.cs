using System.Windows.Controls;

namespace Lab4.TextWidget;

public partial class TextWidgetView : UserControl
{
    public TextWidgetView()
    {
        InitializeComponent();
    }

    public void UpdateView(string data)
    {
        Dispatcher.Invoke(() =>
        {
            ReceivedTextBlock.Text = data;

            int charCount = data.Length;
            CharCountText.Text = charCount.ToString();

            int wordCount = data.Split(
                new[] { ' ', '\t', '\n', '\r' },
                StringSplitOptions.RemoveEmptyEntries
            ).Length;
            WordCountText.Text = wordCount.ToString();
        });
    }
}
