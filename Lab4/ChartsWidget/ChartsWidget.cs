using Lab4.Contracts;
using System.Composition;
using System.Windows.Controls;

namespace Lab4.ChartsWidget;

[Export(typeof(IWidget))]
public class ChartsWidget : IWidget
{
    [ExportMetadata("Name", "Charts Widget")]
    public string Name => "Charts Widget";
    public UserControl View => _view;

    private readonly ChartsWidgetView _view = new();

    [ImportingConstructor]
    public ChartsWidget(IEventAggregator eventAggregator)
    {
        eventAggregator.GetEvent<DataSubmittedEvent>().Subscribe(OnDataReceived);
    }

    private void OnDataReceived(string data)
    {
        _view.UpdateView(data);
    }
}