using System.Composition;
using System.Windows;
using System.Windows.Controls;
using Lab4.Contracts;

namespace Lab4.DashboardApp;

[Export]
public partial class Dashboard : Window
{
    [Import]
    public IEventAggregator EventAggregator { get; set; } = null!;

    private WidgetManager _widgetManager = null!;

    public Dashboard()
    {
        InitializeComponent();
        InitializeEventHandlers();
    }

    [OnImportsSatisfied]
    public void OnImportsSatisfied()
    {
        _widgetManager = new WidgetManager(EventAggregator);
        _widgetManager.WidgetsChanged += OnWidgetsChanged;
        _widgetManager.LoadWidgets();
    }

    private void InitializeEventHandlers()
    {
        SendButton.Click += SendButton_Click;
    }

    private void SendButton_Click(object sender, RoutedEventArgs e)
    {
        string inputData = InputTextBox.Text;

        if (string.IsNullOrWhiteSpace(inputData))
        {
            return;
        }

        EventAggregator.GetEvent<DataSubmittedEvent>().Publish(inputData);
    }

    public void AddWidget(IWidget widget)
    {
        var tabItem = new TabItem { Header = widget.Name, Content = widget.View };

        WidgetsTabControl.Items.Add(tabItem);
    }

    public void RemoveWidget(int index)
    {
        if (index >= 0 && index < WidgetsTabControl.Items.Count)
        {
            WidgetsTabControl.Items.RemoveAt(index);
        }
    }

    public void ClearWidgets()
    {
        WidgetsTabControl.Items.Clear();
    }

    public void OnWidgetsChanged(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            ClearWidgets();

            if (_widgetManager.Widgets != null)
            {
                foreach (var widget in _widgetManager.Widgets)
                {
                    try
                    {
                        widget.View.DataContext = EventAggregator;
                        AddWidget(widget);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Error loading widget {widget.Name}: {ex.Message}",
                            "Widget Load Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );
                    }
                }
            }
        });
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _widgetManager.Dispose();
    }
}
