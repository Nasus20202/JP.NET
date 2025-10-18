using System.Composition;
using System.Windows.Controls;
using Lab4.Contracts;

namespace Lab4.TextWidget;

[Export(typeof(IWidget))]
public class TextWidget : IWidget
{
    [ExportMetadata("Name", "Text Widget")]
    public string Name => "Text Widget";
    public UserControl View => _view;

    private readonly TextWidgetView _view = new();

    [ImportingConstructor]
    public TextWidget(IEventAggregator eventAggregator)
    {
        eventAggregator.GetEvent<DataSubmittedEvent>().Subscribe(OnDataReceived);
    }

    private void OnDataReceived(string data)
    {
        _view.UpdateView(data);
    }
}
